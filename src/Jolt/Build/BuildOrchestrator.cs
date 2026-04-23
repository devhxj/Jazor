using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.Vue;
using Jolt.DevServer;
using Jolt.Frontend.Deno.Hosting;
using Jazor.SourceMaps;

namespace Jolt.Build;

/// <summary>
/// Orchestrates the production build pipeline.
/// Coordinates compilation, Deno bundling, and post-processing.
/// </summary>
internal sealed partial class BuildOrchestrator
{
    private const string ManifestFileName = "jazor-build-manifest.json";
    private const string IncrementalStateFileName = "jazor-build-state.json";
    private const string IncrementalCacheHitMessage = "Incremental build cache hit.";
    private const string IncrementalHtmlRefreshMessage = "Incremental build html refresh.";
    private const string IncrementalScanBypassMessage = "Incremental cache bypassed because one or more input files could not be read reliably.";
    private static readonly Regex CssSourceMapCommentPattern = new(
        @"/\*#\s*sourceMappingURL=(?<value>[^*]+?)\s*\*/\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly StringComparer FilePathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private static readonly StringComparison FilePathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
    private static readonly HashSet<string> BuildGraphCompilableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jazor",
        ".vue",
        ".ts",
        ".js",
        ".css"
    };
    private static readonly HashSet<string> IncrementalFingerprintExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jazor",
        ".vue",
        ".ts",
        ".js",
        ".css",
        ".html",
        ".json"
    };
    private readonly record struct CssFragment(
        string Content,
        string SourcePublicPath,
        string SourcePath,
        int? SourceLineStart,
        int? SourceLineCount,
        IReadOnlyList<string> OwnerChunkFilePaths);
    private readonly record struct EmittedCssFragment(
        string Content,
        string SourcePath,
        int? SourceLineStart,
        int? SourceLineCount,
        IReadOnlyList<string> OwnerChunkFilePaths);
    private sealed record SourceMapOwnershipContext(
        IReadOnlyDictionary<string, IReadOnlySet<string>> ChunkFilePathsByModulePath,
        IReadOnlyDictionary<string, IReadOnlySet<string>> ImporterModulePathsByCssPath);
    private sealed class BuildIncrementalState
    {
        public required string Fingerprint { get; init; }

        public required string ManifestPath { get; init; }

        public string EntryRequestPath { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, string> Inputs { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<ChunkInfo> Chunks { get; init; } = [];

        public IReadOnlyList<AssetInfo> CssAssets { get; init; } = [];

        public IReadOnlyList<AssetInfo> StaticAssets { get; init; } = [];

        public long TotalSize { get; init; }
    }

    /// <summary>
    /// Executes a production build.
    /// </summary>
    public async Task<BuildResult> BuildAsync(
        BuildOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var context = new BuildContext(options);
            // 生产构建先固定一份输入文件快照，避免诊断和增量指纹各自重复遍历工作区。
            var incrementalInputFiles = CollectIncrementalInputFiles(context);
            await AppendProjectLegacyImportDiagnosticsAsync(context, incrementalInputFiles, cancellationToken);
            if (HasErrorDiagnostics(context.Diagnostics))
            {
                stopwatch.Stop();
                return new BuildResult
                {
                    Success = false,
                    Diagnostics = [.. context.Diagnostics],
                    Duration = stopwatch.Elapsed
                };
            }

            IReadOnlyDictionary<string, string> incrementalInputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? incrementalFingerprint = null;
            var canPersistIncrementalState = false;
            if (options.Incremental)
            {
                var incrementalInputSnapshot = CollectIncrementalInputSnapshot(context, incrementalInputFiles);
                incrementalInputs = incrementalInputSnapshot.Inputs;
                canPersistIncrementalState = !incrementalInputSnapshot.HasReadFailure;
                if (incrementalInputSnapshot.HasReadFailure)
                {
                    // 输入集不完整时宁可放弃增量命中，也不能在生产构建里错误复用旧产物。
                    context.Diagnostics.Add(new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Warning,
                        Message = IncrementalScanBypassMessage
                    });
                }

                incrementalFingerprint = ComputeIncrementalFingerprint(options, incrementalInputs);
                if (!incrementalInputSnapshot.HasReadFailure
                    && TryReadIncrementalState(context, out var incrementalState)
                    && AreIncrementalOutputsAvailable(context, incrementalState))
                {
                    if (string.Equals(incrementalState.Fingerprint, incrementalFingerprint, StringComparison.Ordinal))
                    {
                        stopwatch.Stop();
                        return new BuildResult
                        {
                            Success = true,
                            OutDirectory = context.OutDirectory,
                            ManifestPath = ResolveAbsolutePath(context.RootDirectory, incrementalState.ManifestPath),
                            Chunks = incrementalState.Chunks,
                            CssAssets = incrementalState.CssAssets,
                            StaticAssets = incrementalState.StaticAssets,
                            Diagnostics =
                            [
                                .. context.Diagnostics,
                                new BuildDiagnostic
                                {
                                    Severity = DiagnosticSeverity.Info,
                                    Message = IncrementalCacheHitMessage
                                }
                            ],
                            Duration = stopwatch.Elapsed,
                            TotalSize = incrementalState.TotalSize
                        };
                    }

                    var htmlRefreshResult = await TryBuildHtmlRefreshIncrementalResultAsync(
                        context,
                        options,
                        incrementalState,
                        incrementalInputs,
                        incrementalFingerprint,
                        cancellationToken);
                    if (htmlRefreshResult is not null)
                    {
                        stopwatch.Stop();
                        return new BuildResult
                        {
                            Success = htmlRefreshResult.Success,
                            OutDirectory = htmlRefreshResult.OutDirectory,
                            ManifestPath = htmlRefreshResult.ManifestPath,
                            Chunks = htmlRefreshResult.Chunks,
                            CssAssets = htmlRefreshResult.CssAssets,
                            StaticAssets = htmlRefreshResult.StaticAssets,
                            Diagnostics = htmlRefreshResult.Diagnostics,
                            Duration = stopwatch.Elapsed,
                            TotalSize = htmlRefreshResult.TotalSize
                        };
                    }
                }
            }

            PrepareOutputDirectory(context);

            await using var denoHost = CreateDenoHost();
            await denoHost.StartAsync(cancellationToken);

            // 生产构建阶段的模块解析不跟随 reparse point，避免 bundler/dev-server
            // 把工作区外源码伪装成本地依赖读入产物。
            var moduleResolver = new ModuleResolver(
                options.RootDirectory,
                options.ResolveAliases,
                enforceTrustedProjectPaths: true);
            var frontendCompiler = new DenoFrontendModuleCompiler(denoHost, isProduction: true);
            var compiler = new OnDemandCompiler(
                new JazorVueParser(),
                new JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver,
                buildMode: true);

            var devOptions = new DevServerOptions
            {
                RootDirectory = options.RootDirectory,
                Host = IPAddress.Loopback.ToString(),
                // Let Kestrel bind an ephemeral port directly to avoid
                // cross-test port races between "pick free port" and "bind".
                Port = 0,
                HmrEnabled = false,
                FrontendCompiler = "deno"
            };

            await using var moduleServer = new DevHttpServer(
                devOptions,
                compiler,
                moduleResolver,
                new HtmlTransformer(devOptions));
            await moduleServer.StartAsync(cancellationToken);

            var entryPointPath = BuildEntryPointResolver.ResolveEntryPoint(options.RootDirectory);
            var entryRequestPath = ResolveEntryRequestPath(options.RootDirectory, entryPointPath);
            var serverUri = moduleServer.ListeningUri ?? new Uri($"http://{devOptions.Host}:{devOptions.Port}/");
            var entryUri = new Uri(serverUri, entryRequestPath);

            var bundleRunner = new DenoBundleRunner(context);
            var bundleResult = await bundleRunner.RunAsync(entryUri, cancellationToken);

            if (!bundleResult.Success)
            {
                stopwatch.Stop();
                return new BuildResult
                {
                    Success = false,
                    Diagnostics = [.. bundleResult.Diagnostics, .. context.Diagnostics],
                    Duration = stopwatch.Elapsed
                };
            }

            var staticAssetHandler = new StaticAssetHandler(context);
            var staticAssets = await staticAssetHandler.CopyPublicAssetsAsync(cancellationToken);
            await EnsureBuildGraphCompiledAsync(
                context,
                compiler,
                moduleResolver,
                entryPointPath,
                cancellationToken);
            if (HasErrorDiagnostics(context.Diagnostics))
            {
                stopwatch.Stop();
                return new BuildResult
                {
                    Success = false,
                    Diagnostics = [.. bundleResult.Diagnostics, .. context.Diagnostics],
                    Duration = stopwatch.Elapsed
                };
            }

            var cachedResults = compiler.GetCachedResults()
                .ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.OrdinalIgnoreCase);
            var sourceMapOwnershipContext = CreateSourceMapOwnershipContext(
                context.RootDirectory,
                bundleResult.Chunks,
                cachedResults,
                moduleResolver);
            var cssFragments = await CollectExtractedCssFragmentsAsync(
                context.RootDirectory,
                cachedResults,
                moduleResolver,
                entryPointPath,
                bundleResult.Chunks,
                sourceMapOwnershipContext,
                cancellationToken);
            var sourceCssAssets = await CopyReferencedSourceAssetsAsync(
                context,
                staticAssetHandler,
                cssFragments,
                staticAssets,
                cancellationToken);
            staticAssets = [.. staticAssets, .. sourceCssAssets];
            var extractedCssAssets = await EmitExtractedCssAssetsAsync(
                context,
                cssFragments,
                staticAssets,
                bundleResult.Chunks.FirstOrDefault(static chunk => chunk.IsEntry)?.FilePath,
                cancellationToken);
            var bundlerCssAssets = ResolveBundledCssAssetOwners(
                context.RootDirectory,
                bundleResult.Chunks,
                bundleResult.CssAssets,
                sourceMapOwnershipContext,
                moduleResolver);
            await RewriteCssAssetReferencesAsync(
                context,
                [.. bundlerCssAssets, .. staticAssets.Where(static asset => asset.FilePath.EndsWith(".css", StringComparison.OrdinalIgnoreCase))],
                staticAssets,
                cancellationToken);
            var cssAssets = RefreshAssetSizes(context, [.. bundlerCssAssets, .. extractedCssAssets]);
            staticAssets = RefreshAssetSizes(context, staticAssets);
            var chunksWithCss = await AttachCssAssetsToChunksAsync(
                context,
                bundleResult.Chunks,
                cssAssets,
                cancellationToken);
            await RewriteDynamicChunkCssImportsAsync(
                context,
                chunksWithCss,
                cancellationToken);
            var chunks = RefreshChunks(
                context,
                chunksWithCss);
            var totalSize = chunks.Sum(static chunk => chunk.Size)
                + chunks.Sum(chunk => GetOptionalFileSize(context, chunk.SourceMapPath))
                + cssAssets.Sum(static asset => asset.Size)
                + cssAssets.Sum(asset => GetOptionalFileSize(context, asset.SourceMapPath))
                + staticAssets.Sum(static asset => asset.Size)
                + staticAssets.Sum(asset => GetOptionalFileSize(context, asset.SourceMapPath));
            await GenerateHtmlAsync(
                context,
                chunks,
                cssAssets,
                staticAssets,
                entryRequestPath,
                cancellationToken);
            var manifestPath = await WriteManifestAsync(
                context,
                chunks,
                cssAssets,
                staticAssets,
                totalSize,
                cancellationToken);

            stopwatch.Stop();
            var buildResult = new BuildResult
            {
                Success = true,
                OutDirectory = context.OutDirectory,
                ManifestPath = manifestPath,
                Chunks = chunks,
                CssAssets = cssAssets,
                StaticAssets = staticAssets,
                Diagnostics = [.. bundleResult.Diagnostics, .. context.Diagnostics],
                Duration = stopwatch.Elapsed,
                TotalSize = totalSize
            };

            if (options.Incremental
                && canPersistIncrementalState
                && !string.IsNullOrWhiteSpace(incrementalFingerprint))
            {
                // 只有在输入快照完整可信时才落盘增量状态，避免把“部分输入视图”写进缓存。
                await PersistIncrementalStateAsync(
                    context,
                    buildResult,
                    incrementalFingerprint,
                    incrementalInputs,
                    entryRequestPath,
                    cancellationToken);
            }

            return buildResult;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new BuildResult
            {
                Success = false,
                Diagnostics = [new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Build was cancelled."
                }],
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new BuildResult
            {
                Success = false,
                Diagnostics = [new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = ex.Message
                }],
                Duration = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Generates the production index.html in the output directory.
    /// </summary>
    private static async Task GenerateHtmlAsync(
        BuildContext context,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyList<AssetInfo> cssAssets,
        IReadOnlyList<AssetInfo> staticAssets,
        string entryRequestPath,
        CancellationToken cancellationToken)
    {
        var htmlPath = Path.Combine(context.RootDirectory, "index.html");
        // 生产构建读取入口 HTML 也要走项目输入信任边界，避免通过链接/越界路径
        // 把工作区外模板写进最终产物。
        if (!TryResolveTrustedProjectInputFilePath(context.RootDirectory, htmlPath, out var trustedHtmlPath))
        {
            context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "index.html is missing or could not be trusted from project root. Skipping HTML generation."
            });
            return;
        }

        var html = await File.ReadAllTextAsync(trustedHtmlPath, cancellationToken);

        // Remove dev-mode script references
        html = HtmlTransformer.RemoveDevScriptRefs(html);
        html = HtmlTransformer.RemoveScriptReference(html, entryRequestPath);
        html = HtmlTransformer.RewriteAssetReferences(
            html,
            staticAssets.Select(asset => CreateHtmlAssetInfo(context, asset)).ToArray());

        // Inject production script (first entry chunk)
        var entryChunk = chunks.FirstOrDefault(c => c.IsEntry)
            ?? chunks.FirstOrDefault();

        if (entryChunk is not null)
        {
            foreach (var cssPath in entryChunk.Css)
            {
                html = HtmlTransformer.InjectCss(html, ToHtmlPath(context, cssPath));
            }

            html = HtmlTransformer.InjectScript(html, ToHtmlPath(context, entryChunk.FilePath));
        }
        else
        {
            context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "No entry chunk found in the bundled output."
            });
        }

        // Write to dist/index.html
        var outPath = EnsureTrustedBuildOutputPath(
            context.RootDirectory,
            context.OutDirectory,
            Path.Combine(context.OutDirectory, "index.html"),
            allowMissingLeaf: true);
        await File.WriteAllTextAsync(outPath, html, cancellationToken);
    }

    private static AssetInfo CreateHtmlAssetInfo(BuildContext context, AssetInfo asset)
        => new()
        {
            FileName = asset.FileName,
            FilePath = ToHtmlPath(context, asset.FilePath),
            Size = asset.Size,
            SourceMapPath = asset.SourceMapPath is null
                ? null
                : ToHtmlPath(context, asset.SourceMapPath),
            OriginalPath = asset.OriginalPath,
            SourceModulePaths = asset.SourceModulePaths,
            OwnerChunkFilePaths = asset.OwnerChunkFilePaths,
            OwnerChunkFilePath = asset.OwnerChunkFilePath
        };

    private static async Task RewriteCssAssetReferencesAsync(
        BuildContext context,
        IReadOnlyList<AssetInfo> cssAssets,
        IReadOnlyList<AssetInfo> staticAssets,
        CancellationToken cancellationToken)
    {
        if (cssAssets.Count == 0 || staticAssets.Count == 0)
        {
            return;
        }

        var htmlAssets = staticAssets
            .Select(asset => CreateHtmlAssetInfo(context, asset))
            .ToArray();

        foreach (var cssAsset in cssAssets)
        {
            var cssPath = ResolveTrustedBuildOutputPath(context, cssAsset.FilePath);
            if (!File.Exists(cssPath))
            {
                continue;
            }

            var cssPublicPath = ToHtmlPath(context, cssAsset.FilePath);
            var originalCss = await File.ReadAllTextAsync(cssPath, cancellationToken);
            var rewrittenCss = CssUrlRewriter.RewriteAssetReferences(originalCss, cssPublicPath, htmlAssets);
            if (string.Equals(originalCss, rewrittenCss, StringComparison.Ordinal))
            {
                continue;
            }

            await File.WriteAllTextAsync(cssPath, rewrittenCss, cancellationToken);
        }
    }

    private static IReadOnlyList<AssetInfo> RefreshAssetSizes(
        BuildContext context,
        IReadOnlyList<AssetInfo> assets)
        => assets.Select(asset => new AssetInfo
            {
                FileName = asset.FileName,
                FilePath = asset.FilePath,
                Size = GetAssetSize(context, asset.FilePath),
                SourceMapPath = asset.SourceMapPath,
                OriginalPath = asset.OriginalPath,
                SourceModulePaths = asset.SourceModulePaths,
                OwnerChunkFilePaths = asset.OwnerChunkFilePaths,
                OwnerChunkFilePath = asset.OwnerChunkFilePath
            })
            .ToArray();

    private static IReadOnlyList<AssetInfo> ResolveBundledCssAssetOwners(
        string rootDirectory,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyList<AssetInfo> cssAssets,
        SourceMapOwnershipContext? sourceMapOwnershipContext,
        ModuleResolver moduleResolver)
    {
        if (cssAssets.Count == 0)
        {
            return cssAssets;
        }

        var entryChunk = chunks.FirstOrDefault(static chunk => chunk.IsEntry)
            ?? chunks.FirstOrDefault();
        return cssAssets.Select(asset =>
        {
            var sourceModulePaths = ReadNormalizedSourceMapSources(rootDirectory, asset.SourceMapPath, moduleResolver);
            if (sourceMapOwnershipContext is null || entryChunk is null)
            {
                return new AssetInfo
                {
                    FileName = asset.FileName,
                    FilePath = asset.FilePath,
                    Size = asset.Size,
                    SourceMapPath = asset.SourceMapPath,
                    OriginalPath = asset.OriginalPath,
                    SourceModulePaths = sourceModulePaths,
                    OwnerChunkFilePaths = asset.OwnerChunkFilePaths,
                    OwnerChunkFilePath = asset.OwnerChunkFilePath
                };
            }

            var ownerChunkFilePaths = new HashSet<string>(FilePathComparer);
            foreach (var sourceModulePath in sourceModulePaths)
            {
                if (sourceMapOwnershipContext.ImporterModulePathsByCssPath.TryGetValue(sourceModulePath, out var importerModulePaths))
                {
                    foreach (var importerModulePath in importerModulePaths)
                    {
                        if (sourceMapOwnershipContext.ChunkFilePathsByModulePath.TryGetValue(importerModulePath, out var importerChunkFilePaths))
                        {
                            ownerChunkFilePaths.UnionWith(importerChunkFilePaths);
                        }
                    }
                }
            }

            var normalizedOwnerChunkFilePaths = NormalizeOwnerChunkFilePaths(ownerChunkFilePaths, entryChunk.FilePath);
            return new AssetInfo
            {
                FileName = asset.FileName,
                FilePath = asset.FilePath,
                Size = asset.Size,
                SourceMapPath = asset.SourceMapPath,
                OriginalPath = asset.OriginalPath,
                SourceModulePaths = sourceModulePaths,
                OwnerChunkFilePaths = normalizedOwnerChunkFilePaths,
                OwnerChunkFilePath = normalizedOwnerChunkFilePaths.Count == 1
                    ? normalizedOwnerChunkFilePaths[0]
                    : null
            };
        }).ToArray();
    }

    private static IReadOnlyList<ChunkInfo> RefreshChunks(
        BuildContext context,
        IReadOnlyList<ChunkInfo> chunks)
        => chunks.Select(chunk => new ChunkInfo
            {
                FileName = chunk.FileName,
                FilePath = chunk.FilePath,
                Size = GetAssetSize(context, chunk.FilePath),
                IsEntry = chunk.IsEntry,
                IsDynamic = chunk.IsDynamic,
                Imports = chunk.Imports,
                Css = chunk.Css,
                SourceMapPath = chunk.SourceMapPath
            })
            .ToArray();

    private static async Task<IReadOnlyList<ChunkInfo>> AttachCssAssetsToChunksAsync(
        BuildContext context,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyList<AssetInfo> cssAssets,
        CancellationToken cancellationToken)
    {
        if (chunks.Count == 0)
        {
            return chunks;
        }

        var entryChunk = chunks.FirstOrDefault(static chunk => chunk.IsEntry)
            ?? chunks.First();
        var directCssByChunk = chunks.ToDictionary(
            static chunk => chunk.FilePath,
            static _ => new HashSet<string>(StringComparer.Ordinal),
            FilePathComparer);

        foreach (var cssAsset in cssAssets)
        {
            foreach (var ownerChunkFilePath in GetAssetOwnerChunkFilePaths(cssAsset, entryChunk.FilePath))
            {
                if (directCssByChunk.TryGetValue(ownerChunkFilePath, out var cssFilePaths))
                {
                    cssFilePaths.Add(cssAsset.FilePath);
                }
            }
        }

        var dynamicImportsByChunk = await ReadDynamicImportsByChunkAsync(context, chunks, cancellationToken);
        var cssClosureByChunk = BuildCssClosureByChunk(
            chunks,
            directCssByChunk.ToDictionary(
                static entry => entry.Key,
                static entry => (IReadOnlySet<string>)entry.Value,
                FilePathComparer),
            dynamicImportsByChunk);

        return chunks.Select(chunk => new ChunkInfo
            {
                FileName = chunk.FileName,
                FilePath = chunk.FilePath,
                Size = chunk.Size,
                IsEntry = chunk.IsEntry,
                IsDynamic = chunk.IsDynamic,
                Imports = chunk.Imports,
                Css = cssClosureByChunk.TryGetValue(chunk.FilePath, out var chunkCss)
                    ? chunkCss
                    : [],
                SourceMapPath = chunk.SourceMapPath
            })
            .ToArray();
    }

    private static async Task RewriteDynamicChunkCssImportsAsync(
        BuildContext context,
        IReadOnlyList<ChunkInfo> chunks,
        CancellationToken cancellationToken)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        var chunkCssByFilePath = chunks.ToDictionary(
            static chunk => chunk.FilePath,
            chunk => chunk.Css
                .Select(cssFilePath => ToHtmlPath(context, cssFilePath))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            FilePathComparer);

        foreach (var chunk in chunks)
        {
            var chunkAbsolutePath = ResolveTrustedBuildOutputPath(context, chunk.FilePath);
            if (!File.Exists(chunkAbsolutePath))
            {
                continue;
            }

            var originalContent = await File.ReadAllTextAsync(chunkAbsolutePath, cancellationToken);
            var currentChunkDirectory = GetContainingDirectoryPath(chunkAbsolutePath);
            var rewrittenContent = JavaScriptModuleSpecifierScanner.RewriteDynamicImportExpressions(
                originalContent,
                specifier =>
                {
                    if (!TryResolveBuiltChunkDynamicImportFilePath(
                            context,
                            currentChunkDirectory,
                            specifier.Value,
                            out var targetChunkFilePath))
                    {
                        return null;
                    }

                    if (!chunkCssByFilePath.TryGetValue(targetChunkFilePath, out var targetCssPaths) || targetCssPaths.Length == 0)
                    {
                        return null;
                    }

                    var originalImportExpression = originalContent.Substring(specifier.ExpressionStart, specifier.ExpressionLength);
                    return CreateDynamicChunkCssImportExpression(originalImportExpression, targetCssPaths);
                });

            if (string.Equals(originalContent, rewrittenContent, StringComparison.Ordinal))
            {
                continue;
            }

            await File.WriteAllTextAsync(chunkAbsolutePath, rewrittenContent, cancellationToken);
        }
    }

    private static IReadOnlyList<string> GetAssetOwnerChunkFilePaths(AssetInfo asset, string entryChunkFilePath)
        => asset.OwnerChunkFilePaths.Count > 0
            ? NormalizeOwnerChunkFilePaths(asset.OwnerChunkFilePaths, entryChunkFilePath)
            : NormalizeOwnerChunkFilePaths(CreateOwnerChunkFilePaths(asset.OwnerChunkFilePath), entryChunkFilePath);

    private static async Task<IReadOnlyDictionary<string, IReadOnlySet<string>>> ReadDynamicImportsByChunkAsync(
        BuildContext context,
        IReadOnlyList<ChunkInfo> chunks,
        CancellationToken cancellationToken)
    {
        var dynamicImportsByChunk = new Dictionary<string, IReadOnlySet<string>>(FilePathComparer);

        foreach (var chunk in chunks)
        {
            var chunkAbsolutePath = ResolveTrustedBuildOutputPath(context, chunk.FilePath);
            if (!File.Exists(chunkAbsolutePath))
            {
                continue;
            }

            var chunkContent = await File.ReadAllTextAsync(chunkAbsolutePath, cancellationToken);
            var currentChunkDirectory = GetContainingDirectoryPath(chunkAbsolutePath);
            var dynamicImports = new HashSet<string>(FilePathComparer);

            foreach (var specifier in JavaScriptModuleSpecifierScanner.EnumerateSpecifiers(chunkContent)
                         .Where(static specifier => specifier.Kind == JavaScriptModuleSpecifierKind.DynamicImport))
            {
                if (!TryResolveBuiltChunkDynamicImportFilePath(
                        context,
                        currentChunkDirectory,
                        specifier.Value,
                        out var targetChunkFilePath))
                {
                    continue;
                }

                dynamicImports.Add(targetChunkFilePath);
            }

            dynamicImportsByChunk[chunk.FilePath] = dynamicImports;
        }

        return dynamicImportsByChunk;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildCssClosureByChunk(
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyDictionary<string, IReadOnlySet<string>> directCssByChunk,
        IReadOnlyDictionary<string, IReadOnlySet<string>> dynamicImportsByChunk)
    {
        var chunkByFilePath = chunks.ToDictionary(static chunk => chunk.FilePath, static chunk => chunk, FilePathComparer);
        var cssClosureByChunk = new Dictionary<string, IReadOnlyList<string>>(FilePathComparer);

        IReadOnlyList<string> ResolveCssClosure(string chunkFilePath, HashSet<string> visitingChunkFilePaths)
        {
            if (cssClosureByChunk.TryGetValue(chunkFilePath, out var cachedCssClosure))
            {
                return cachedCssClosure;
            }

            if (!chunkByFilePath.TryGetValue(chunkFilePath, out var chunk))
            {
                return [];
            }

            if (!visitingChunkFilePaths.Add(chunkFilePath))
            {
                return directCssByChunk.TryGetValue(chunkFilePath, out var directCss)
                    ? directCss.OrderBy(static cssPath => cssPath, StringComparer.Ordinal).ToArray()
                    : [];
            }

            var cssClosure = new HashSet<string>(StringComparer.Ordinal);
            if (directCssByChunk.TryGetValue(chunkFilePath, out var chunkDirectCss))
            {
                cssClosure.UnionWith(chunkDirectCss);
            }

            var dynamicImports = dynamicImportsByChunk.TryGetValue(chunkFilePath, out var chunkDynamicImports)
                ? chunkDynamicImports
                : new HashSet<string>(FilePathComparer);
            foreach (var importedChunkFilePath in chunk.Imports)
            {
                if (dynamicImports.Contains(importedChunkFilePath))
                {
                    continue;
                }

                cssClosure.UnionWith(ResolveCssClosure(importedChunkFilePath, visitingChunkFilePaths));
            }

            visitingChunkFilePaths.Remove(chunkFilePath);
            var resolvedCssClosure = cssClosure.OrderBy(static cssPath => cssPath, StringComparer.Ordinal).ToArray();
            cssClosureByChunk[chunkFilePath] = resolvedCssClosure;
            return resolvedCssClosure;
        }

        foreach (var chunk in chunks)
        {
            ResolveCssClosure(chunk.FilePath, []);
        }

        return cssClosureByChunk;
    }

    private static bool TryGetBuiltChunkDynamicImportPath(string specifier, out string path)
    {
        (path, _) = JavaScriptModuleSpecifierScanner.SplitPathAndSuffix(specifier);
        return (path.StartsWith("./", StringComparison.Ordinal)
                || path.StartsWith("../", StringComparison.Ordinal))
            && path.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool TryResolveBuiltChunkDynamicImportFilePath(
        BuildContext context,
        string currentChunkDirectory,
        string specifier,
        out string targetChunkFilePath)
    {
        targetChunkFilePath = string.Empty;
        if (!TryGetBuiltChunkDynamicImportPath(specifier, out var specifierPath))
        {
            return false;
        }

        try
        {
            var targetAbsolutePath = Path.GetFullPath(Path.Combine(
                currentChunkDirectory,
                specifierPath.Replace('/', Path.DirectorySeparatorChar)));

            // 已生成 chunk 的 import 文本也不当作可信输入；目标必须仍落在当前输出目录内，
            // 防止异常产物里的 ../ 路径污染 CSS 闭包或二次重写。
            var trustedTargetPath = EnsureTrustedBuildOutputPath(
                context.RootDirectory,
                context.OutDirectory,
                targetAbsolutePath,
                allowMissingLeaf: true);
            targetChunkFilePath = Path.GetRelativePath(context.RootDirectory, trustedTargetPath).Replace('\\', '/');
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static string CreateDynamicChunkCssImportExpression(
        string originalImportExpression,
        IReadOnlyList<string> cssPaths)
    {
        var cssArrayLiteral = JsonSerializer.Serialize(cssPaths);
        return string.Concat(
            "((globalThis.__jazorImportCss ??= async function(hrefs){if(typeof document===\"undefined\"||!Array.isArray(hrefs)||hrefs.length===0){return;}const registry=globalThis.__jazorLoadedCss ??= new Set();await Promise.all(hrefs.map(function(href){if(!href||registry.has(href)){return Promise.resolve();}const existing=document.querySelector('link[rel=\"stylesheet\"][href=\"'+href+'\"]');if(existing){registry.add(href);return Promise.resolve();}return new Promise(function(resolve,reject){const link=document.createElement(\"link\");link.rel=\"stylesheet\";link.href=href;link.onload=function(){registry.add(href);resolve();};link.onerror=function(){reject(new Error(\"Failed to load stylesheet \"+href));};document.head.appendChild(link);});}));}),globalThis.__jazorImportCss(",
            cssArrayLiteral,
            ").then(function(){return ",
            originalImportExpression,
            ";}))");
    }

    private static string GetContainingDirectoryPath(string path)
        => Path.GetDirectoryName(path)
            ?? Path.GetPathRoot(path)
            ?? string.Empty;

    private static async Task<string> WriteManifestAsync(
        BuildContext context,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyList<AssetInfo> cssAssets,
        IReadOnlyList<AssetInfo> staticAssets,
        long totalSize,
        CancellationToken cancellationToken)
    {
        var entryChunk = chunks.FirstOrDefault(static chunk => chunk.IsEntry)
            ?? chunks.FirstOrDefault()
            ?? throw new InvalidOperationException("Cannot write build manifest without an entry chunk.");
        var manifest = new BuildManifest
        {
            Entry = ToHtmlPath(context, entryChunk.FilePath),
            Chunks = chunks
                .Select(chunk => new BuildManifestChunk
                {
                    File = ToHtmlPath(context, chunk.FilePath),
                    IsEntry = chunk.IsEntry,
                    Imports = chunk.Imports.Select(importPath => ToHtmlPath(context, importPath)).ToArray(),
                    Css = chunk.Css.Select(cssPath => ToHtmlPath(context, cssPath)).ToArray(),
                    SourceMap = chunk.SourceMapPath is null
                        ? null
                        : ToHtmlPath(context, chunk.SourceMapPath)
                })
                .ToArray(),
            Css = entryChunk.Css.Select(cssPath => ToHtmlPath(context, cssPath)).ToArray(),
            StaticAssets = staticAssets
                .Where(static asset => !string.IsNullOrWhiteSpace(asset.OriginalPath))
                .Select(asset => new BuildManifestStaticAsset
                {
                    File = ToHtmlPath(context, asset.FilePath),
                    OriginalPath = asset.OriginalPath!
                })
                .ToArray(),
            TotalSize = totalSize
        };

        var manifestPath = EnsureTrustedBuildOutputPath(
            context.RootDirectory,
            context.OutDirectory,
            Path.Combine(context.OutDirectory, ManifestFileName),
            allowMissingLeaf: true);
        var json = JsonSerializer.Serialize(
            manifest,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        await File.WriteAllTextAsync(manifestPath, json, cancellationToken);
        return manifestPath;
    }

}

