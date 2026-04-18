using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.Emit.SourceMaps;
using Jazor.Vue;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.Frontend.Deno.Hosting;

namespace Jazor.VueHost.Build;

/// <summary>
/// Orchestrates the production build pipeline.
/// Coordinates compilation, Deno bundling, and post-processing.
/// </summary>
internal sealed class BuildOrchestrator
{
    private const string ManifestFileName = "jazor-build-manifest.json";
    private const string IncrementalStateFileName = "jazor-build-state.json";
    private const string IncrementalCacheHitMessage = "Incremental build cache hit.";
    private const string IncrementalHtmlRefreshMessage = "Incremental build html refresh.";
    private static readonly Regex CssSourceMapCommentPattern = new(
        @"/\*#\s*sourceMappingURL=(?<value>[^*]+?)\s*\*/\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex DynamicImportPattern = new(
        @"\bimport\s*\(\s*[""'](?<specifier>[^""']+)[""']\s*\)",
        RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex BuiltChunkDynamicImportPattern = new(
        @"\bimport\s*\(\s*(?<quote>[""'])(?<specifier>\.{1,2}/[^""']+?\.js)(?<query>\?[^""']*)?\k<quote>\s*\)",
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
            IReadOnlyDictionary<string, string> incrementalInputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? incrementalFingerprint = null;
            if (options.Incremental)
            {
                incrementalInputs = CollectIncrementalInputSignatures(context);
                incrementalFingerprint = ComputeIncrementalFingerprint(options, incrementalInputs);
                if (TryReadIncrementalState(context, out var incrementalState)
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

            var moduleResolver = new ModuleResolver(options.RootDirectory, options.ResolveAliases);
            var frontendCompiler = new DenoFrontendModuleCompiler(denoHost);
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
                compiler,
                moduleResolver,
                entryPointPath,
                cancellationToken);
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

            if (options.Incremental && !string.IsNullOrWhiteSpace(incrementalFingerprint))
            {
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
        if (!File.Exists(htmlPath))
        {
            context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "index.html not found in project root. Skipping HTML generation."
            });
            return;
        }

        var html = await File.ReadAllTextAsync(htmlPath, cancellationToken);

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
        var outPath = Path.Combine(context.OutDirectory, "index.html");
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
            var cssPath = Path.Combine(
                context.RootDirectory,
                cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
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
            var chunkAbsolutePath = Path.Combine(
                context.RootDirectory,
                chunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(chunkAbsolutePath))
            {
                continue;
            }

            var originalContent = await File.ReadAllTextAsync(chunkAbsolutePath, cancellationToken);
            var currentChunkDirectory = Path.GetDirectoryName(chunkAbsolutePath)!;
            var rewrittenContent = BuiltChunkDynamicImportPattern.Replace(
                originalContent,
                match =>
                {
                    var specifier = match.Groups["specifier"].Value;
                    var targetAbsolutePath = Path.GetFullPath(Path.Combine(
                        currentChunkDirectory,
                        specifier.Replace('/', Path.DirectorySeparatorChar)));
                    var targetChunkFilePath = Path.GetRelativePath(context.RootDirectory, targetAbsolutePath).Replace('\\', '/');
                    if (!chunkCssByFilePath.TryGetValue(targetChunkFilePath, out var targetCssPaths) || targetCssPaths.Length == 0)
                    {
                        return match.Value;
                    }

                    return CreateDynamicChunkCssImportExpression(match.Value, targetCssPaths);
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
            var chunkAbsolutePath = Path.Combine(
                context.RootDirectory,
                chunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(chunkAbsolutePath))
            {
                continue;
            }

            var chunkContent = await File.ReadAllTextAsync(chunkAbsolutePath, cancellationToken);
            var currentChunkDirectory = Path.GetDirectoryName(chunkAbsolutePath)!;
            var dynamicImports = new HashSet<string>(FilePathComparer);

            foreach (Match match in BuiltChunkDynamicImportPattern.Matches(chunkContent))
            {
                var specifier = match.Groups["specifier"].Value;
                if (string.IsNullOrWhiteSpace(specifier))
                {
                    continue;
                }

                var targetAbsolutePath = Path.GetFullPath(Path.Combine(
                    currentChunkDirectory,
                    specifier.Replace('/', Path.DirectorySeparatorChar)));
                dynamicImports.Add(Path.GetRelativePath(context.RootDirectory, targetAbsolutePath).Replace('\\', '/'));
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

    private static async Task<IReadOnlyList<AssetInfo>> EmitExtractedCssAssetsAsync(
        BuildContext context,
        IReadOnlyList<CssFragment> cssFragments,
        IReadOnlyList<AssetInfo> staticAssets,
        string? entryChunkFilePath,
        CancellationToken cancellationToken)
    {
        if (cssFragments.Count == 0)
        {
            return [];
        }

        var htmlAssets = staticAssets
            .Select(asset => CreateHtmlAssetInfo(context, asset))
            .ToArray();
        Directory.CreateDirectory(context.AssetsDirectory);
        var assets = new List<AssetInfo>();
        var groupedFragments = cssFragments
            .GroupBy(
                fragment => CreateOwnerChunkSetKey(fragment.OwnerChunkFilePaths, entryChunkFilePath),
                StringComparer.Ordinal)
            .OrderBy(group => IsEntryOnlyOwnerSet(group.First().OwnerChunkFilePaths, entryChunkFilePath) ? 0 : 1)
            .ThenBy(static group => group.Key, StringComparer.Ordinal);

        foreach (var group in groupedFragments)
        {
            var ownerChunkFilePaths = NormalizeOwnerChunkFilePaths(group.First().OwnerChunkFilePaths, entryChunkFilePath);
            var baseName = CreateCssAssetBaseName(ownerChunkFilePaths, entryChunkFilePath);
            var extractedCssPublicPath = Path.GetRelativePath(
                context.OutDirectory,
                Path.Combine(context.AssetsDirectory, baseName + ".css")).Replace('\\', '/');
            var emittedFragments = group
                .Select(fragment => new EmittedCssFragment(
                    CssUrlRewriter.RewriteAssetReferences(
                        fragment.Content,
                        fragment.SourcePublicPath,
                        extractedCssPublicPath,
                        htmlAssets),
                    fragment.SourcePath,
                    fragment.SourceLineStart,
                    fragment.SourceLineCount,
                    ownerChunkFilePaths))
                .Where(static fragment => !string.IsNullOrWhiteSpace(fragment.Content))
                .ToArray();
            if (emittedFragments.Length == 0)
            {
                continue;
            }

            var content = string.Join(
                Environment.NewLine + Environment.NewLine,
                emittedFragments.Select(static fragment => fragment.Content));
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var fileName = CreateHashedAssetFileName(baseName, ".css", content, context.Options.AssetHashLength);
            var outputPath = Path.Combine(context.AssetsDirectory, fileName);
            string? sourceMapPath = null;
            var finalContent = content;
            if (context.Options.GenerateSourceMap)
            {
                var sourceMap = CreateExtractedCssSourceMap(context, emittedFragments, fileName);
                if (!string.IsNullOrWhiteSpace(sourceMap))
                {
                    switch (context.Options.SourceMap)
                    {
                        case SourceMapOption.External:
                            var sourceMapOutputPath = outputPath + ".map";
                            await File.WriteAllTextAsync(sourceMapOutputPath, sourceMap, cancellationToken);
                            sourceMapPath = Path.GetRelativePath(context.RootDirectory, sourceMapOutputPath).Replace('\\', '/');
                            finalContent = AppendCssSourceMapComment(content, Path.GetFileName(sourceMapOutputPath));
                            break;
                        case SourceMapOption.Inline:
                            finalContent = AppendInlineCssSourceMapComment(content, sourceMap);
                            break;
                    }
                }
            }

            await File.WriteAllTextAsync(outputPath, finalContent, cancellationToken);

            assets.Add(new AssetInfo
            {
                FileName = fileName,
                FilePath = Path.GetRelativePath(context.RootDirectory, outputPath).Replace('\\', '/'),
                Size = new FileInfo(outputPath).Length,
                SourceMapPath = sourceMapPath,
                SourceModulePaths = emittedFragments
                    .Select(static fragment => fragment.SourcePath)
                    .Where(static path => !string.IsNullOrWhiteSpace(path))
                    .Distinct(FilePathComparer)
                    .ToArray(),
                OwnerChunkFilePaths = ownerChunkFilePaths,
                OwnerChunkFilePath = ownerChunkFilePaths.Count == 1
                    ? ownerChunkFilePaths[0]
                    : null
            });
        }

        return assets;
    }

    private static string? CreateExtractedCssSourceMap(
        BuildContext context,
        IReadOnlyList<EmittedCssFragment> cssFragments,
        string outputFileName)
    {
        var sources = new List<SourceMapSource>();
        var segments = new List<SourceMapSegment>();
        var sourceContentCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sourceIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var generatedLine = 0;

        for (var fragmentIndex = 0; fragmentIndex < cssFragments.Count; fragmentIndex++)
        {
            var fragment = cssFragments[fragmentIndex];
            var generatedLineCount = CountSourceMapLines(fragment.Content);
            if (!string.IsNullOrWhiteSpace(fragment.SourcePath)
                && fragment.SourceLineStart.HasValue
                && fragment.SourceLineCount.HasValue
                && TryReadSourceMapContent(fragment.SourcePath, sourceContentCache, out var sourceContent))
            {
                if (!sourceIndices.TryGetValue(fragment.SourcePath, out var sourceIndex))
                {
                    sourceIndex = sources.Count;
                    sourceIndices[fragment.SourcePath] = sourceIndex;
                    sources.Add(new SourceMapSource(
                        CreateSourceMapRelativePath(context.AssetsDirectory, fragment.SourcePath),
                        sourceContent));
                }

                var sourceStartLine = Math.Max(fragment.SourceLineStart.Value - 1, 0);
                var maxSourceLineOffset = Math.Max(fragment.SourceLineCount.Value - 1, 0);
                for (var lineIndex = 0; lineIndex < generatedLineCount; lineIndex++)
                {
                    segments.Add(new SourceMapSegment(
                        generatedLine + lineIndex,
                        0,
                        sourceIndex,
                        sourceStartLine + Math.Min(lineIndex, maxSourceLineOffset),
                        0));
                }
            }

            generatedLine += generatedLineCount;
            if (fragmentIndex < cssFragments.Count - 1)
            {
                generatedLine++;
            }
        }

        if (segments.Count == 0 || sources.Count == 0)
        {
            return null;
        }

        return new SourceMapWriter().Write(new SourceMapDocument(outputFileName, sources, segments));
    }

    private static bool TryReadSourceMapContent(
        string sourcePath,
        IDictionary<string, string> sourceContentCache,
        out string sourceContent)
    {
        if (sourceContentCache.TryGetValue(sourcePath, out var cachedSourceContent))
        {
            sourceContent = cachedSourceContent;
            return true;
        }

        if (!File.Exists(sourcePath))
        {
            sourceContent = string.Empty;
            return false;
        }

        sourceContent = File.ReadAllText(sourcePath);
        sourceContentCache[sourcePath] = sourceContent;
        return true;
    }

    private static string CreateSourceMapRelativePath(string sourceMapDirectory, string sourcePath)
    {
        var relativePath = Path.GetRelativePath(sourceMapDirectory, sourcePath).Replace('\\', '/');
        return relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath[2..]
            : relativePath;
    }

    private static string AppendInlineCssSourceMapComment(string content, string sourceMap)
        => AppendCssSourceMapComment(
            content,
            "data:application/json;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceMap)));

    private static string AppendCssSourceMapComment(string content, string sourceMapReference)
    {
        var normalizedContent = CssSourceMapCommentPattern.Replace(content, string.Empty).TrimEnd('\r', '\n');
        return string.Concat(
            normalizedContent,
            Environment.NewLine,
            $"/*# sourceMappingURL={sourceMapReference} */",
            Environment.NewLine);
    }

    private static int CountSourceMapLines(string text)
        => NormalizeLineEndings(text).Split('\n').Length;

    private static string NormalizeLineEndings(string text)
        => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static async Task<IReadOnlyList<CssFragment>> CollectExtractedCssFragmentsAsync(
        string rootDirectory,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver,
        string entryPointPath,
        IReadOnlyList<ChunkInfo> chunks,
        SourceMapOwnershipContext? sourceMapOwnershipContext,
        CancellationToken cancellationToken)
    {
        if (cachedResults.Count == 0 || !cachedResults.ContainsKey(entryPointPath))
        {
            return [];
        }

        if (sourceMapOwnershipContext is not null)
        {
            return await CollectExtractedCssFragmentsFromSourceMapsAsync(
                cachedResults,
                moduleResolver,
                entryPointPath,
                sourceMapOwnershipContext,
                cancellationToken);
        }

        return await CollectExtractedCssFragmentsWithFallbackOwnershipAsync(
            rootDirectory,
            cachedResults,
            moduleResolver,
            entryPointPath,
            chunks,
            cancellationToken);
    }

    private static async Task<IReadOnlyList<CssFragment>> CollectExtractedCssFragmentsFromSourceMapsAsync(
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver,
        string entryPointPath,
        SourceMapOwnershipContext sourceMapOwnershipContext,
        CancellationToken cancellationToken)
    {
        var reachableModulePaths = CollectReachableModulePaths(entryPointPath, cachedResults, moduleResolver);
        if (reachableModulePaths.Count == 0)
        {
            return [];
        }

        var cssFragments = new List<CssFragment>();
        var cssOwnerChunkPathsByPath = new Dictionary<string, HashSet<string>>(FilePathComparer);

        foreach (var modulePath in reachableModulePaths)
        {
            if (!cachedResults.TryGetValue(modulePath, out var result))
            {
                continue;
            }

            var isCssModule = string.Equals(
                Path.GetExtension(modulePath),
                ".css",
                StringComparison.OrdinalIgnoreCase);
            var ownerChunkFilePaths = GetOwnerChunkFilePaths(
                modulePath,
                sourceMapOwnershipContext.ChunkFilePathsByModulePath);
            var embeddedStyleDependencyPaths = result.EmbeddedStyleDependencies
                .Select(dependency => moduleResolver.Resolve(dependency, modulePath))
                .Where(static resolved => resolved.Found && !resolved.IsVirtual)
                .Select(static resolved => resolved.AbsolutePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var dependency in result.Dependencies)
            {
                var resolved = moduleResolver.Resolve(dependency, modulePath);
                if (!resolved.Found || resolved.IsVirtual
                    || !string.Equals(Path.GetExtension(resolved.AbsolutePath), ".css", StringComparison.OrdinalIgnoreCase)
                    || embeddedStyleDependencyPaths.Contains(resolved.AbsolutePath))
                {
                    continue;
                }

                if (!cssOwnerChunkPathsByPath.TryGetValue(resolved.AbsolutePath, out var cssOwnerChunkPaths))
                {
                    cssOwnerChunkPaths = new HashSet<string>(FilePathComparer);
                    cssOwnerChunkPathsByPath[resolved.AbsolutePath] = cssOwnerChunkPaths;
                }

                cssOwnerChunkPaths.UnionWith(ownerChunkFilePaths);
            }

            if (isCssModule)
            {
                continue;
            }

            if (result.StyleFragments.Count > 0)
            {
                foreach (var styleFragment in result.StyleFragments)
                {
                    if (string.IsNullOrWhiteSpace(styleFragment.Content))
                    {
                        continue;
                    }

                    cssFragments.Add(new CssFragment(
                        styleFragment.Content,
                        GetStyleFragmentSourcePublicPath(moduleResolver, modulePath, styleFragment),
                        GetStyleFragmentSourcePath(modulePath, styleFragment),
                        styleFragment.SourceLineStart,
                        styleFragment.SourceLineCount,
                        ownerChunkFilePaths));
                }
            }
            else if (!string.IsNullOrWhiteSpace(result.StyleContent))
            {
                cssFragments.Add(new CssFragment(
                    result.StyleContent!,
                    moduleResolver.GetResolvedUrlForAbsolutePath(modulePath).TrimStart('/'),
                    modulePath,
                    null,
                    null,
                    ownerChunkFilePaths));
            }
        }

        foreach (var (cssPath, ownerChunkPaths) in cssOwnerChunkPathsByPath.OrderBy(static entry => entry.Key, FilePathComparer))
        {
            var cssText = await File.ReadAllTextAsync(cssPath, cancellationToken);
            cssFragments.Add(new CssFragment(
                cssText,
                moduleResolver.GetResolvedUrlForAbsolutePath(cssPath).TrimStart('/'),
                cssPath,
                1,
                CountSourceMapLines(cssText),
                NormalizeOwnerChunkFilePaths(ownerChunkPaths, entryChunkFilePath: null)));
        }

        return cssFragments;
    }

    private static async Task<IReadOnlyList<CssFragment>> CollectExtractedCssFragmentsWithFallbackOwnershipAsync(
        string rootDirectory,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver,
        string entryPointPath,
        IReadOnlyList<ChunkInfo> chunks,
        CancellationToken cancellationToken)
    {
        var cssFragments = new List<CssFragment>();
        var cssOwnerChunkPathsByPath = new Dictionary<string, HashSet<string>>(FilePathComparer);
        var visitedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ownerChunkByModulePath = CreateModuleChunkOwnershipMap(
            rootDirectory,
            entryPointPath,
            chunks,
            cachedResults,
            moduleResolver);

        await CollectCssFragmentsWithFallbackOwnershipAsync(
            entryPointPath,
            currentOwnerChunkFilePath: null,
            cachedResults,
            ownerChunkByModulePath,
            moduleResolver,
            visitedModules,
            cssOwnerChunkPathsByPath,
            cssFragments,
            cancellationToken);

        foreach (var (cssPath, ownerChunkPaths) in cssOwnerChunkPathsByPath.OrderBy(static entry => entry.Key, FilePathComparer))
        {
            var cssText = await File.ReadAllTextAsync(cssPath, cancellationToken);
            cssFragments.Add(new CssFragment(
                cssText,
                moduleResolver.GetResolvedUrlForAbsolutePath(cssPath).TrimStart('/'),
                cssPath,
                1,
                CountSourceMapLines(cssText),
                NormalizeOwnerChunkFilePaths(ownerChunkPaths, entryChunkFilePath: null)));
        }

        return cssFragments;
    }

    private static async Task CollectCssFragmentsWithFallbackOwnershipAsync(
        string modulePath,
        string? currentOwnerChunkFilePath,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        IReadOnlyDictionary<string, string> ownerChunkByModulePath,
        ModuleResolver moduleResolver,
        ISet<string> visitedModules,
        IDictionary<string, HashSet<string>> cssOwnerChunkPathsByPath,
        ICollection<CssFragment> cssFragments,
        CancellationToken cancellationToken)
    {
        var ownerChunkFilePath = ownerChunkByModulePath.TryGetValue(modulePath, out var explicitOwnerChunkFilePath)
            ? explicitOwnerChunkFilePath
            : currentOwnerChunkFilePath;
        var isCssModule = string.Equals(
            Path.GetExtension(modulePath),
            ".css",
            StringComparison.OrdinalIgnoreCase);
        var visitKey = CreateOwnedKey(ownerChunkFilePath, modulePath);
        if (!visitedModules.Add(visitKey) || !cachedResults.TryGetValue(modulePath, out var result))
        {
            return;
        }

        var embeddedStyleDependencyPaths = result.EmbeddedStyleDependencies
            .Select(dependency => moduleResolver.Resolve(dependency, modulePath))
            .Where(static resolved => resolved.Found && !resolved.IsVirtual)
            .Select(static resolved => resolved.AbsolutePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var dependency in result.Dependencies)
        {
            var resolved = moduleResolver.Resolve(dependency, modulePath);
            if (!resolved.Found || resolved.IsVirtual)
            {
                continue;
            }

            var extension = Path.GetExtension(resolved.AbsolutePath);
            if (string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase))
            {
                if (embeddedStyleDependencyPaths.Contains(resolved.AbsolutePath))
                {
                    continue;
                }

                if (!cssOwnerChunkPathsByPath.TryGetValue(resolved.AbsolutePath, out var cssOwnerChunkPaths))
                {
                    cssOwnerChunkPaths = new HashSet<string>(FilePathComparer);
                    cssOwnerChunkPathsByPath[resolved.AbsolutePath] = cssOwnerChunkPaths;
                }

                cssOwnerChunkPaths.UnionWith(CreateOwnerChunkFilePaths(ownerChunkFilePath));
                continue;
            }

            if (cachedResults.ContainsKey(resolved.AbsolutePath))
            {
                await CollectCssFragmentsWithFallbackOwnershipAsync(
                    resolved.AbsolutePath,
                    ownerChunkFilePath,
                    cachedResults,
                    ownerChunkByModulePath,
                    moduleResolver,
                    visitedModules,
                    cssOwnerChunkPathsByPath,
                    cssFragments,
                    cancellationToken);
            }
        }

        if (isCssModule)
        {
            return;
        }

        if (result.StyleFragments.Count > 0)
        {
            foreach (var styleFragment in result.StyleFragments)
            {
                if (string.IsNullOrWhiteSpace(styleFragment.Content))
                {
                    continue;
                }

                cssFragments.Add(new CssFragment(
                    styleFragment.Content,
                    GetStyleFragmentSourcePublicPath(moduleResolver, modulePath, styleFragment),
                    GetStyleFragmentSourcePath(modulePath, styleFragment),
                    styleFragment.SourceLineStart,
                    styleFragment.SourceLineCount,
                    CreateOwnerChunkFilePaths(ownerChunkFilePath)));
            }
        }
        else if (!string.IsNullOrWhiteSpace(result.StyleContent))
        {
            cssFragments.Add(new CssFragment(
                result.StyleContent!,
                moduleResolver.GetResolvedUrlForAbsolutePath(modulePath).TrimStart('/'),
                modulePath,
                null,
                null,
                CreateOwnerChunkFilePaths(ownerChunkFilePath)));
        }
    }

    private static SourceMapOwnershipContext? CreateSourceMapOwnershipContext(
        string rootDirectory,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        if (chunks.Count == 0 || cachedResults.Count == 0)
        {
            return null;
        }

        var chunkFilePathsByModulePath = new Dictionary<string, HashSet<string>>(FilePathComparer);
        foreach (var chunk in chunks)
        {
            foreach (var sourcePath in ReadNormalizedSourceMapSources(rootDirectory, chunk.SourceMapPath, moduleResolver))
            {
                if (!cachedResults.ContainsKey(sourcePath))
                {
                    continue;
                }

                if (!chunkFilePathsByModulePath.TryGetValue(sourcePath, out var chunkFilePaths))
                {
                    chunkFilePaths = new HashSet<string>(FilePathComparer);
                    chunkFilePathsByModulePath[sourcePath] = chunkFilePaths;
                }

                chunkFilePaths.Add(chunk.FilePath);
            }
        }

        if (chunkFilePathsByModulePath.Count == 0)
        {
            return null;
        }

        var importerModulePathsByCssPath = new Dictionary<string, HashSet<string>>(FilePathComparer);
        foreach (var (modulePath, result) in cachedResults)
        {
            var embeddedStyleDependencyPaths = result.EmbeddedStyleDependencies
                .Select(dependency => moduleResolver.Resolve(dependency, modulePath))
                .Where(static resolved => resolved.Found && !resolved.IsVirtual)
                .Select(static resolved => resolved.AbsolutePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var dependency in result.Dependencies)
            {
                var resolved = moduleResolver.Resolve(dependency, modulePath);
                if (!resolved.Found || resolved.IsVirtual
                    || !string.Equals(Path.GetExtension(resolved.AbsolutePath), ".css", StringComparison.OrdinalIgnoreCase)
                    || embeddedStyleDependencyPaths.Contains(resolved.AbsolutePath))
                {
                    continue;
                }

                if (!importerModulePathsByCssPath.TryGetValue(resolved.AbsolutePath, out var importerModulePaths))
                {
                    importerModulePaths = new HashSet<string>(FilePathComparer);
                    importerModulePathsByCssPath[resolved.AbsolutePath] = importerModulePaths;
                }

                importerModulePaths.Add(modulePath);
            }
        }

        return new SourceMapOwnershipContext(
            chunkFilePathsByModulePath.ToDictionary(
                static entry => entry.Key,
                static entry => (IReadOnlySet<string>)entry.Value,
                FilePathComparer),
            importerModulePathsByCssPath.ToDictionary(
                static entry => entry.Key,
                static entry => (IReadOnlySet<string>)entry.Value,
                FilePathComparer));
    }

    private static IReadOnlyList<string> CollectReachableModulePaths(
        string entryPointPath,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        var reachableModulePaths = new List<string>();
        var stack = new Stack<string>();
        var visitedModulePaths = new HashSet<string>(FilePathComparer);
        stack.Push(entryPointPath);

        while (stack.Count > 0)
        {
            var modulePath = stack.Pop();
            if (!visitedModulePaths.Add(modulePath) || !cachedResults.TryGetValue(modulePath, out var result))
            {
                continue;
            }

            reachableModulePaths.Add(modulePath);

            foreach (var dependency in result.Dependencies)
            {
                var resolved = moduleResolver.Resolve(dependency, modulePath);
                if (resolved.Found && !resolved.IsVirtual && cachedResults.ContainsKey(resolved.AbsolutePath))
                {
                    stack.Push(resolved.AbsolutePath);
                }
            }
        }

        return reachableModulePaths;
    }

    private static IReadOnlyList<string> GetOwnerChunkFilePaths(
        string sourceModulePath,
        IReadOnlyDictionary<string, IReadOnlySet<string>> chunkFilePathsByModulePath)
        => chunkFilePathsByModulePath.TryGetValue(sourceModulePath, out var ownerChunkFilePaths)
            ? NormalizeOwnerChunkFilePaths(ownerChunkFilePaths, entryChunkFilePath: null)
            : [];

    private static IReadOnlyList<string> ReadNormalizedSourceMapSources(
        string rootDirectory,
        string? sourceMapPath,
        ModuleResolver moduleResolver)
    {
        if (string.IsNullOrWhiteSpace(sourceMapPath))
        {
            return [];
        }

        var sourceMapAbsolutePath = Path.Combine(
            rootDirectory,
            sourceMapPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(sourceMapAbsolutePath))
        {
            return [];
        }

        try
        {
            using var sourceMapDocument = JsonDocument.Parse(File.ReadAllText(sourceMapAbsolutePath));
            if (!sourceMapDocument.RootElement.TryGetProperty("sources", out var sourcesElement)
                || sourcesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var normalizedSources = new HashSet<string>(FilePathComparer);
            foreach (var sourceElement in sourcesElement.EnumerateArray())
            {
                if (sourceElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var normalizedSourcePath = NormalizeSourceMapSourcePath(
                    rootDirectory,
                    Path.GetDirectoryName(sourceMapAbsolutePath)!,
                    sourceElement.GetString(),
                    moduleResolver);
                if (!string.IsNullOrWhiteSpace(normalizedSourcePath))
                {
                    normalizedSources.Add(normalizedSourcePath);
                }
            }

            return normalizedSources.OrderBy(static path => path, FilePathComparer).ToArray();
        }
        catch (IOException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? NormalizeSourceMapSourcePath(
        string rootDirectory,
        string sourceMapDirectory,
        string? source,
        ModuleResolver moduleResolver)
    {
        if (string.IsNullOrWhiteSpace(source) || source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (source.StartsWith("deno:", StringComparison.OrdinalIgnoreCase))
        {
            return NormalizeSourceMapSourcePath(
                rootDirectory,
                sourceMapDirectory,
                source["deno:".Length..],
                moduleResolver);
        }

        if (Uri.TryCreate(source, UriKind.Absolute, out var absoluteUri))
        {
            if (absoluteUri.IsFile)
            {
                return NormalizeAbsoluteSourceMapPath(rootDirectory, absoluteUri.LocalPath);
            }

            if (string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                var requestPath = NormalizeBundlerProxyRequestPath(Uri.UnescapeDataString(absoluteUri.AbsolutePath));
                var resolved = moduleResolver.Resolve(requestPath);
                return resolved.Found && !resolved.IsVirtual
                    ? NormalizeAbsoluteSourceMapPath(rootDirectory, resolved.AbsolutePath)
                    : null;
            }

            return null;
        }

        if (source.StartsWith("/", StringComparison.Ordinal))
        {
            var resolved = moduleResolver.Resolve(NormalizeBundlerProxyRequestPath(source));
            return resolved.Found && !resolved.IsVirtual
                ? NormalizeAbsoluteSourceMapPath(rootDirectory, resolved.AbsolutePath)
                : null;
        }

        return NormalizeAbsoluteSourceMapPath(
            rootDirectory,
            Path.Combine(sourceMapDirectory, source.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string? NormalizeAbsoluteSourceMapPath(string rootDirectory, string path)
    {
        var absolutePath = Path.GetFullPath(Uri.UnescapeDataString(path));
        return IsPathInsideRoot(rootDirectory, absolutePath) && File.Exists(absolutePath)
            ? absolutePath
            : null;
    }

    private static bool IsPathInsideRoot(string rootDirectory, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, candidatePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }

    private static IReadOnlyDictionary<string, string> CreateModuleChunkOwnershipMap(
        string rootDirectory,
        string entryPointPath,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        var ownerChunkByModulePath = new Dictionary<string, string>(FilePathComparer);
        var entryChunk = chunks.FirstOrDefault(static chunk => chunk.IsEntry)
            ?? chunks.FirstOrDefault();
        if (entryChunk is not null)
        {
            ownerChunkByModulePath[entryPointPath] = entryChunk.FilePath;
        }

        var dynamicImportRootModules = ExtractDynamicImportRootModules(cachedResults, moduleResolver);
        var chunkModulesByFilePath = ReadChunkModulesFromSourceMaps(
            rootDirectory,
            chunks,
            cachedResults,
            moduleResolver);

        foreach (var dynamicImportRootModule in dynamicImportRootModules)
        {
            var matchingChunkFilePaths = chunkModulesByFilePath
                .Where(entry => entry.Value.Contains(dynamicImportRootModule))
                .Select(entry => entry.Key)
                .Distinct(FilePathComparer)
                .ToArray();
            if (matchingChunkFilePaths.Length == 0)
            {
                continue;
            }

            var preferredChunkFilePaths = entryChunk is null
                ? matchingChunkFilePaths
                : matchingChunkFilePaths
                    .Where(chunkFilePath => !string.Equals(chunkFilePath, entryChunk.FilePath, FilePathComparison))
                    .ToArray();
            var resolvedChunkFilePaths = preferredChunkFilePaths.Length > 0
                ? preferredChunkFilePaths
                : matchingChunkFilePaths;
            if (resolvedChunkFilePaths.Length == 1)
            {
                ownerChunkByModulePath.TryAdd(dynamicImportRootModule, resolvedChunkFilePaths[0]);
            }
        }

        var modulePathsByStem = dynamicImportRootModules
            .Where(modulePath => !ownerChunkByModulePath.ContainsKey(modulePath))
            .GroupBy(static modulePath => Path.GetFileNameWithoutExtension(modulePath), FilePathComparer)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .OrderBy(static path => path.Length)
                    .ThenBy(static path => path, FilePathComparer)
                    .ToArray(),
                FilePathComparer);

        foreach (var chunk in chunks.Where(static chunk => !chunk.IsEntry))
        {
            var chunkBaseName = GetChunkBaseName(chunk.FileName);
            if (string.IsNullOrWhiteSpace(chunkBaseName)
                || !TryGetChunkCandidateModulePaths(chunkBaseName, modulePathsByStem, out var candidateModulePaths)
                || candidateModulePaths.Length != 1)
            {
                continue;
            }

            ownerChunkByModulePath.TryAdd(candidateModulePaths[0], chunk.FilePath);
        }

        var unresolvedChunks = chunks
            .Where(chunk => !chunk.IsEntry
                && !ownerChunkByModulePath.Values.Any(ownerChunkFilePath => string.Equals(ownerChunkFilePath, chunk.FilePath, FilePathComparison)))
            .ToArray();
        if (unresolvedChunks.Length == 1)
        {
            var unresolvedDynamicRoots = dynamicImportRootModules
                .Where(modulePath => !ownerChunkByModulePath.ContainsKey(modulePath))
                .ToArray();
            if (unresolvedDynamicRoots.Length == 1)
            {
                ownerChunkByModulePath[unresolvedDynamicRoots[0]] = unresolvedChunks[0].FilePath;
            }
        }

        return ownerChunkByModulePath;
    }

    private static bool TryGetChunkCandidateModulePaths(
        string chunkBaseName,
        IReadOnlyDictionary<string, string[]> modulePathsByStem,
        out string[] candidateModulePaths)
    {
        if (modulePathsByStem.TryGetValue(chunkBaseName, out var exactCandidateModulePaths))
        {
            candidateModulePaths = exactCandidateModulePaths;
            return true;
        }

        var prefixMatches = modulePathsByStem
            .Where(entry => chunkBaseName.StartsWith(entry.Key + "-", FilePathComparison))
            .OrderByDescending(static entry => entry.Key.Length)
            .ToArray();
        if (prefixMatches.Length == 0)
        {
            candidateModulePaths = [];
            return false;
        }

        var bestMatchLength = prefixMatches[0].Key.Length;
        var bestMatches = prefixMatches
            .Where(entry => entry.Key.Length == bestMatchLength)
            .ToArray();
        if (bestMatches.Length != 1)
        {
            candidateModulePaths = [];
            return false;
        }

        candidateModulePaths = bestMatches[0].Value;
        return true;
    }

    private static IReadOnlyDictionary<string, HashSet<string>> ReadChunkModulesFromSourceMaps(
        string rootDirectory,
        IReadOnlyList<ChunkInfo> chunks,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        var chunkModulesByFilePath = new Dictionary<string, HashSet<string>>(FilePathComparer);

        foreach (var chunk in chunks)
        {
            var chunkModules = ReadChunkSourceModules(rootDirectory, chunk, cachedResults, moduleResolver);
            if (chunkModules.Count > 0)
            {
                chunkModulesByFilePath[chunk.FilePath] = chunkModules;
            }
        }

        return chunkModulesByFilePath;
    }

    private static HashSet<string> ReadChunkSourceModules(
        string rootDirectory,
        ChunkInfo chunk,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        if (string.IsNullOrWhiteSpace(chunk.SourceMapPath))
        {
            return [];
        }

        var sourceMapAbsolutePath = Path.Combine(
            rootDirectory,
            chunk.SourceMapPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(sourceMapAbsolutePath))
        {
            return [];
        }

        try
        {
            using var sourceMapDocument = JsonDocument.Parse(File.ReadAllText(sourceMapAbsolutePath));
            if (!sourceMapDocument.RootElement.TryGetProperty("sources", out var sourcesElement)
                || sourcesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var sourceModules = new HashSet<string>(FilePathComparer);
            foreach (var sourceElement in sourcesElement.EnumerateArray())
            {
                if (sourceElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var sourceModulePath = NormalizeSourceMapSourceToModulePath(
                    sourceElement.GetString(),
                    cachedResults,
                    moduleResolver);
                if (!string.IsNullOrWhiteSpace(sourceModulePath))
                {
                    sourceModules.Add(sourceModulePath);
                }
            }

            return sourceModules;
        }
        catch (IOException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? NormalizeSourceMapSourceToModulePath(
        string? source,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        if (source.StartsWith("deno:", StringComparison.OrdinalIgnoreCase))
        {
            return NormalizeSourceMapSourceToModulePath(
                source["deno:".Length..],
                cachedResults,
                moduleResolver);
        }

        if (source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (Uri.TryCreate(source, UriKind.Absolute, out var absoluteUri))
        {
            if (absoluteUri.IsFile)
            {
                var absolutePath = Path.GetFullPath(Uri.UnescapeDataString(absoluteUri.LocalPath));
                return cachedResults.ContainsKey(absolutePath)
                    ? absolutePath
                    : null;
            }

            if (string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return ResolveSourceMapRequestPathToModulePath(
                    Uri.UnescapeDataString(absoluteUri.AbsolutePath),
                    cachedResults,
                    moduleResolver);
            }

            return null;
        }

        if (source.StartsWith("/", StringComparison.Ordinal))
        {
            return ResolveSourceMapRequestPathToModulePath(source, cachedResults, moduleResolver);
        }

        var resolved = moduleResolver.Resolve(source);
        return resolved.Found && !resolved.IsVirtual && cachedResults.ContainsKey(resolved.AbsolutePath)
            ? resolved.AbsolutePath
            : null;
    }

    private static string? ResolveSourceMapRequestPathToModulePath(
        string requestPath,
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        requestPath = NormalizeBundlerProxyRequestPath(requestPath);
        var resolved = moduleResolver.Resolve(requestPath);
        return resolved.Found && !resolved.IsVirtual && cachedResults.ContainsKey(resolved.AbsolutePath)
            ? resolved.AbsolutePath
            : null;
    }

    private static string NormalizeBundlerProxyRequestPath(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
        {
            return requestPath;
        }

        const string bundlerPrefixMarker = "/__jazor_bundle/";
        var markerIndex = requestPath.IndexOf(bundlerPrefixMarker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return requestPath;
        }

        var suffixIndex = requestPath.IndexOfAny(['?', '#']);
        var path = suffixIndex >= 0
            ? requestPath[..suffixIndex]
            : requestPath;
        var markerPathIndex = path.IndexOf(bundlerPrefixMarker, StringComparison.OrdinalIgnoreCase);
        if (markerPathIndex < 0)
        {
            return requestPath;
        }

        var tokenStart = markerPathIndex + bundlerPrefixMarker.Length;
        var afterTokenSeparatorIndex = path.IndexOf('/', tokenStart);
        if (afterTokenSeparatorIndex < 0)
        {
            return "/";
        }

        var normalizedPath = path[afterTokenSeparatorIndex..];
        if (suffixIndex < 0)
        {
            return normalizedPath;
        }

        return normalizedPath + requestPath[suffixIndex..];
    }

    private static IReadOnlyList<string> ExtractDynamicImportRootModules(
        IReadOnlyDictionary<string, CompilationResult> cachedResults,
        ModuleResolver moduleResolver)
    {
        var dynamicImportRootModules = new HashSet<string>(FilePathComparer);

        foreach (var (modulePath, result) in cachedResults)
        {
            foreach (Match match in DynamicImportPattern.Matches(result.Content))
            {
                var specifier = match.Groups["specifier"].Value;
                if (string.IsNullOrWhiteSpace(specifier))
                {
                    continue;
                }

                var resolved = moduleResolver.Resolve(specifier, modulePath);
                if (!resolved.Found || resolved.IsVirtual || !cachedResults.ContainsKey(resolved.AbsolutePath))
                {
                    continue;
                }

                dynamicImportRootModules.Add(resolved.AbsolutePath);
            }
        }

        return dynamicImportRootModules.ToArray();
    }

    private static string CreateOwnedKey(string? ownerChunkFilePath, string path)
        => $"{ownerChunkFilePath ?? "<entry>"}|{path}";

    private static IReadOnlyList<string> CreateOwnerChunkFilePaths(string? ownerChunkFilePath)
        => string.IsNullOrWhiteSpace(ownerChunkFilePath)
            ? []
            : [ownerChunkFilePath];

    private static IReadOnlyList<string> NormalizeOwnerChunkFilePaths(
        IEnumerable<string> ownerChunkFilePaths,
        string? entryChunkFilePath)
    {
        var normalizedOwnerChunkFilePaths = ownerChunkFilePaths
            .Where(static ownerChunkFilePath => !string.IsNullOrWhiteSpace(ownerChunkFilePath))
            .Distinct(FilePathComparer)
            .OrderBy(static ownerChunkFilePath => ownerChunkFilePath, FilePathComparer)
            .ToArray();
        if (normalizedOwnerChunkFilePaths.Length > 0)
        {
            return normalizedOwnerChunkFilePaths;
        }

        return string.IsNullOrWhiteSpace(entryChunkFilePath)
            ? []
            : [entryChunkFilePath];
    }

    private static string CreateOwnerChunkSetKey(
        IEnumerable<string> ownerChunkFilePaths,
        string? entryChunkFilePath)
        => string.Join(
            "|",
            NormalizeOwnerChunkFilePaths(ownerChunkFilePaths, entryChunkFilePath));

    private static bool IsEntryOnlyOwnerSet(
        IEnumerable<string> ownerChunkFilePaths,
        string? entryChunkFilePath)
    {
        var normalizedOwnerChunkFilePaths = NormalizeOwnerChunkFilePaths(ownerChunkFilePaths, entryChunkFilePath);
        return normalizedOwnerChunkFilePaths.Count == 1
            && string.Equals(normalizedOwnerChunkFilePaths[0], entryChunkFilePath, FilePathComparison);
    }

    private static string CreateCssAssetBaseName(
        IReadOnlyList<string> ownerChunkFilePaths,
        string? entryChunkFilePath)
    {
        if (ownerChunkFilePaths.Count == 0
            || IsEntryOnlyOwnerSet(ownerChunkFilePaths, entryChunkFilePath))
        {
            return "styles";
        }

        if (ownerChunkFilePaths.Count == 1)
        {
            return $"{GetChunkBaseName(Path.GetFileName(ownerChunkFilePaths[0]))}-styles";
        }

        var ownerKey = string.Join("|", ownerChunkFilePaths);
        var ownerHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ownerKey)))[..8].ToLowerInvariant();
        return $"shared-{ownerHash}-styles";
    }

    private static string GetChunkBaseName(string chunkFileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(chunkFileName);
        var separatorIndex = fileNameWithoutExtension.LastIndexOf('-');
        if (separatorIndex <= 0 || separatorIndex == fileNameWithoutExtension.Length - 1)
        {
            return fileNameWithoutExtension;
        }

        var suffix = fileNameWithoutExtension[(separatorIndex + 1)..];
        return IsHexString(suffix)
            ? fileNameWithoutExtension[..separatorIndex]
            : fileNameWithoutExtension;
    }

    private static bool IsHexString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (!Uri.IsHexDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<IReadOnlyList<AssetInfo>> CopyReferencedSourceAssetsAsync(
        BuildContext context,
        StaticAssetHandler staticAssetHandler,
        IReadOnlyList<CssFragment> cssFragments,
        IReadOnlyList<AssetInfo> existingStaticAssets,
        CancellationToken cancellationToken)
    {
        if (cssFragments.Count == 0)
        {
            return [];
        }

        var knownOriginalPaths = existingStaticAssets
            .Where(static asset => !string.IsNullOrWhiteSpace(asset.OriginalPath))
            .Select(static asset => asset.OriginalPath!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sourceAssetRequests = new List<SourceAssetRequest>();

        foreach (var cssFragment in cssFragments)
        {
            foreach (var referencedAssetPath in CssUrlRewriter.ExtractAssetReferences(
                         cssFragment.Content,
                         cssFragment.SourcePublicPath))
            {
                if (!knownOriginalPaths.Add(referencedAssetPath))
                {
                    continue;
                }

                var absolutePath = Path.Combine(
                    context.RootDirectory,
                    referencedAssetPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                sourceAssetRequests.Add(new SourceAssetRequest
                {
                    AbsolutePath = absolutePath,
                    OriginalPath = referencedAssetPath
                });
            }
        }

        return await staticAssetHandler.CopySourceAssetsAsync(sourceAssetRequests, cancellationToken);
    }

    private static string CreateHashedAssetFileName(
        string baseName,
        string extension,
        string content,
        int hashLength)
    {
        var normalizedHashLength = Math.Max(1, Math.Min(hashLength, 64));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..normalizedHashLength].ToLowerInvariant();
        return $"{baseName}-{hash}{extension}";
    }

    private static string GetStyleFragmentSourcePublicPath(
        ModuleResolver moduleResolver,
        string modulePath,
        CompiledStyleFragment styleFragment)
    {
        var sourcePath = GetStyleFragmentSourcePath(modulePath, styleFragment);

        try
        {
            return moduleResolver.GetResolvedUrlForAbsolutePath(sourcePath).TrimStart('/');
        }
        catch (InvalidOperationException)
        {
            return moduleResolver.GetResolvedUrlForAbsolutePath(modulePath).TrimStart('/');
        }
    }

    private static string GetStyleFragmentSourcePath(
        string modulePath,
        CompiledStyleFragment styleFragment)
        => string.IsNullOrWhiteSpace(styleFragment.SourcePath)
            ? modulePath
            : styleFragment.SourcePath!;

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

        var manifestPath = Path.Combine(context.OutDirectory, ManifestFileName);
        var json = JsonSerializer.Serialize(
            manifest,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        await File.WriteAllTextAsync(manifestPath, json, cancellationToken);
        return manifestPath;
    }

    /// <summary>
    /// Creates a DenoVolarHost for the build pipeline.
    /// </summary>
    private static DenoVolarHost CreateDenoHost()
    {
        var baseDirectory = ResolveDenoHostBaseDirectory();
        var parsedOptions = DenoVolarHostOptionsParser.Parse(["--deno-worker"], baseDirectory);
        var options = new DenoVolarHostOptions
        {
            Enabled = parsedOptions.Enabled,
            ExecutablePath = parsedOptions.ExecutablePath,
            HasExplicitExecutableOverride = parsedOptions.HasExplicitExecutableOverride,
            WorkerScriptPath = parsedOptions.WorkerScriptPath,
            CacheDirectory = parsedOptions.CacheDirectory,
            Arguments = parsedOptions.Arguments,
            WorkingDirectory = parsedOptions.WorkingDirectory,
            IgnoreStartupFailure = false
        };

        return new DenoVolarHost(options);
    }

    private static string ResolveDenoHostBaseDirectory()
    {
        var assemblyBaseDirectory = Path.GetDirectoryName(typeof(BuildOrchestrator).Assembly.Location)
            ?? AppContext.BaseDirectory;
        if (IsUsableDenoHostBaseDirectory(assemblyBaseDirectory))
        {
            return assemblyBaseDirectory;
        }

        var projectOutputBaseDirectory = TryResolveProjectOutputBaseDirectory(assemblyBaseDirectory);
        if (projectOutputBaseDirectory is not null && IsUsableDenoHostBaseDirectory(projectOutputBaseDirectory))
        {
            return projectOutputBaseDirectory;
        }

        var workspaceProjectOutputBaseDirectory = TryResolveWorkspaceProjectOutputBaseDirectory(assemblyBaseDirectory);
        if (workspaceProjectOutputBaseDirectory is not null)
        {
            return workspaceProjectOutputBaseDirectory;
        }

        var fallbackProjectOutputBaseDirectory = TryResolveFallbackProjectOutputBaseDirectory(projectOutputBaseDirectory)
            ?? TryResolveFallbackProjectOutputBaseDirectory(workspaceProjectOutputBaseDirectory);
        return fallbackProjectOutputBaseDirectory is not null
            ? fallbackProjectOutputBaseDirectory
            : assemblyBaseDirectory;
    }

    private static string? TryResolveFallbackProjectOutputBaseDirectory(string? projectOutputBaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(projectOutputBaseDirectory))
        {
            return null;
        }

        var targetFramework = Path.GetFileName(projectOutputBaseDirectory);
        var configurationDirectory = Path.GetDirectoryName(projectOutputBaseDirectory);
        var binDirectory = string.IsNullOrWhiteSpace(configurationDirectory)
            ? null
            : Path.GetDirectoryName(configurationDirectory);
        if (string.IsNullOrWhiteSpace(targetFramework)
            || string.IsNullOrWhiteSpace(binDirectory)
            || !Directory.Exists(binDirectory))
        {
            return null;
        }

        try
        {
            return Directory.EnumerateDirectories(binDirectory)
                .Select(configurationPath => Path.Combine(configurationPath, targetFramework))
                .Where(candidate => !string.Equals(
                    Path.GetFullPath(candidate),
                    Path.GetFullPath(projectOutputBaseDirectory),
                    FilePathComparison))
                .FirstOrDefault(IsUsableDenoHostBaseDirectory);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string? TryResolveProjectOutputBaseDirectory(string assemblyBaseDirectory)
    {
        var targetFramework = Path.GetFileName(assemblyBaseDirectory);
        var configurationDirectory = Path.GetDirectoryName(assemblyBaseDirectory);
        if (string.IsNullOrWhiteSpace(targetFramework) || string.IsNullOrWhiteSpace(configurationDirectory))
        {
            return null;
        }

        var configuration = Path.GetFileName(configurationDirectory);
        if (string.IsNullOrWhiteSpace(configuration))
        {
            return null;
        }

        var candidate = Path.GetFullPath(Path.Combine(
            assemblyBaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Jazor.VueHost",
            "bin",
            configuration,
            targetFramework));
        return Directory.Exists(candidate)
            ? candidate
            : null;
    }

    private static string? TryResolveWorkspaceProjectOutputBaseDirectory(string assemblyBaseDirectory)
    {
        var repositoryRoot = TryResolveRepositoryRoot(assemblyBaseDirectory);
        if (string.IsNullOrWhiteSpace(repositoryRoot))
        {
            return null;
        }

        var sourceProjectBinDirectory = Path.Combine(repositoryRoot, "src", "Jazor.VueHost", "bin");
        if (!Directory.Exists(sourceProjectBinDirectory))
        {
            return null;
        }

        var preferredConfiguration = TryResolveBuildConfiguration(assemblyBaseDirectory);
        try
        {
            foreach (var configurationDirectory in Directory.EnumerateDirectories(sourceProjectBinDirectory)
                .OrderByDescending(path => string.Equals(
                    Path.GetFileName(path),
                    preferredConfiguration,
                    StringComparison.OrdinalIgnoreCase))
                .ThenBy(static path => path, FilePathComparer))
            {
                foreach (var candidate in Directory.EnumerateDirectories(configurationDirectory)
                    .OrderBy(static path => path, FilePathComparer))
                {
                    if (IsUsableDenoHostBaseDirectory(candidate))
                    {
                        return candidate;
                    }
                }
            }
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return null;
    }

    private static string? TryResolveRepositoryRoot(string assemblyBaseDirectory)
    {
        var currentDirectory = new DirectoryInfo(assemblyBaseDirectory);
        while (currentDirectory is not null)
        {
            var sourceProjectPath = Path.Combine(
                currentDirectory.FullName,
                "src",
                "Jazor.VueHost",
                "Jazor.VueHost.csproj");
            if (File.Exists(sourceProjectPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static string? TryResolveBuildConfiguration(string assemblyBaseDirectory)
    {
        var directoryName = Path.GetFileName(assemblyBaseDirectory);
        if (IsBuildConfigurationName(directoryName))
        {
            return directoryName;
        }

        var parentDirectoryName = Path.GetFileName(Path.GetDirectoryName(assemblyBaseDirectory));
        return IsBuildConfigurationName(parentDirectoryName)
            ? parentDirectoryName
            : null;
    }

    private static bool IsBuildConfigurationName(string? value)
        => string.Equals(value, "Debug", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "Release", StringComparison.OrdinalIgnoreCase);

    private static bool IsUsableDenoHostBaseDirectory(string baseDirectory)
    {
        var workerPath = Path.Combine(baseDirectory, "Frontend", "Deno", "Worker", "frontend-worker.ts");
        var workerDirectory = Path.GetDirectoryName(workerPath);
        var workerConfigPath = string.IsNullOrWhiteSpace(workerDirectory)
            ? null
            : Path.Combine(workerDirectory, "deno.json");
        var workerNodeModulesDirectory = string.IsNullOrWhiteSpace(workerDirectory)
            ? null
            : Path.Combine(workerDirectory, "node_modules");
        var cacheDirectory = Path.Combine(baseDirectory, "Frontend", "Deno", "Cache");
        var npmCacheDirectory = Path.Combine(cacheDirectory, "npm");
        var registryCacheDirectory = Path.Combine(npmCacheDirectory, "registry.npmjs.org");
        return File.Exists(workerPath)
            && !string.IsNullOrWhiteSpace(workerConfigPath)
            && File.Exists(workerConfigPath)
            && HasReadyDenoWorkerDependencies(workerNodeModulesDirectory, registryCacheDirectory)
            && DenoRuntimeAssetResolver.TryResolveBundledExecutablePath(baseDirectory, out _);
    }

    private static bool HasReadyDenoWorkerDependencies(
        string? workerNodeModulesDirectory,
        string registryCacheDirectory)
    {
        if (!string.IsNullOrWhiteSpace(workerNodeModulesDirectory)
            && Directory.Exists(Path.Combine(workerNodeModulesDirectory, "@volar"))
            && Directory.Exists(Path.Combine(workerNodeModulesDirectory, "@vue")))
        {
            return true;
        }

        return Directory.Exists(Path.Combine(registryCacheDirectory, "@volar"))
            && Directory.Exists(Path.Combine(registryCacheDirectory, "@vue"));
    }

    private static async Task EnsureBuildGraphCompiledAsync(
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        string entryPointPath,
        CancellationToken cancellationToken)
    {
        var pendingModulePaths = new Stack<string>();
        var visitedModulePaths = new HashSet<string>(FilePathComparer);
        pendingModulePaths.Push(entryPointPath);

        while (pendingModulePaths.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var modulePath = pendingModulePaths.Pop();
            if (!visitedModulePaths.Add(modulePath)
                || !File.Exists(modulePath)
                || !IsBuildGraphCompilablePath(modulePath))
            {
                continue;
            }

            var result = await compiler.CompileAsync(modulePath, cancellationToken);
            foreach (var dependency in result.Dependencies)
            {
                var resolved = moduleResolver.Resolve(dependency, modulePath);
                if (!resolved.Found
                    || resolved.IsVirtual
                    || !IsBuildGraphCompilablePath(resolved.AbsolutePath))
                {
                    continue;
                }

                pendingModulePaths.Push(resolved.AbsolutePath);
            }
        }
    }

    private static bool IsBuildGraphCompilablePath(string path)
        => BuildGraphCompilableExtensions.Contains(Path.GetExtension(path));

    private static IReadOnlyDictionary<string, string> CollectIncrementalInputSignatures(BuildContext context)
    {
        var inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in EnumerateIncrementalInputFiles(context))
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(context.RootDirectory, filePath).Replace('\\', '/');
                var signature = fileInfo.Length.ToString(CultureInfo.InvariantCulture)
                    + "|"
                    + fileInfo.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture);
                inputs[relativePath] = signature;
            }
            catch (IOException)
            {
                // Skip transiently inaccessible files. A subsequent build run will re-evaluate.
            }
            catch (UnauthorizedAccessException)
            {
                // Skip inaccessible files. Fingerprint remains stable for accessible inputs.
            }
        }

        return inputs;
    }

    private static string ComputeIncrementalFingerprint(
        BuildOptions options,
        IReadOnlyDictionary<string, string> incrementalInputs)
    {
        var fingerprintBuilder = new StringBuilder();
        fingerprintBuilder.Append(BuildIncrementalOptionsFingerprint(options));
        foreach (var (path, signature) in incrementalInputs
                     .OrderBy(static item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            fingerprintBuilder
                .Append(path)
                .Append('|')
                .Append(signature)
                .AppendLine();
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintBuilder.ToString())));
    }

    private static IReadOnlyList<string> GetIncrementalChangedPaths(
        IReadOnlyDictionary<string, string> previousInputs,
        IReadOnlyDictionary<string, string> currentInputs)
    {
        var changedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, previousSignature) in previousInputs)
        {
            if (!currentInputs.TryGetValue(path, out var currentSignature)
                || !string.Equals(previousSignature, currentSignature, StringComparison.Ordinal))
            {
                changedPaths.Add(path);
            }
        }

        foreach (var path in currentInputs.Keys)
        {
            if (!previousInputs.ContainsKey(path))
            {
                changedPaths.Add(path);
            }
        }

        return changedPaths
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string BuildIncrementalOptionsFingerprint(BuildOptions options)
    {
        var builder = new StringBuilder();
        builder
            .Append("outDir=").Append(options.OutDir).AppendLine()
            .Append("sourceMap=").Append(options.SourceMap).AppendLine()
            .Append("minify=").Append(options.Minify.ToString(CultureInfo.InvariantCulture)).AppendLine()
            .Append("target=").Append(options.Target).AppendLine()
            .Append("codeSplitting=").Append(options.CodeSplitting.ToString(CultureInfo.InvariantCulture)).AppendLine()
            .Append("assetsDir=").Append(options.AssetsDir).AppendLine()
            .Append("assetHashLength=").Append(options.AssetHashLength.ToString(CultureInfo.InvariantCulture)).AppendLine()
            .Append("chunkSizeWarningLimit=").Append(options.ChunkSizeWarningLimit.ToString(CultureInfo.InvariantCulture)).AppendLine();
        foreach (var (alias, target) in options.ResolveAliases
                     .OrderBy(static item => item.Key, StringComparer.Ordinal)
                     .ThenBy(static item => item.Value, StringComparer.Ordinal))
        {
            builder
                .Append("alias:")
                .Append(alias)
                .Append('=')
                .Append(target)
                .AppendLine();
        }

        return builder.ToString();
    }

    private static IEnumerable<string> EnumerateIncrementalInputFiles(BuildContext context)
    {
        var rootDirectory = Path.GetFullPath(context.RootDirectory);
        if (!Directory.Exists(rootDirectory))
        {
            yield break;
        }

        var outDirectory = Path.GetFullPath(context.OutDirectory);
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(rootDirectory);

        while (pendingDirectories.Count > 0)
        {
            var directory = pendingDirectories.Pop();
            if (string.Equals(directory, outDirectory, FilePathComparison))
            {
                continue;
            }

            var directoryName = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.Equals(directory, rootDirectory, FilePathComparison)
                && IsIgnoredIncrementalDirectory(directoryName))
            {
                continue;
            }

            IEnumerable<string> childDirectories;
            try
            {
                childDirectories = Directory.EnumerateDirectories(directory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in childDirectories)
            {
                pendingDirectories.Push(Path.GetFullPath(childDirectory));
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var filePath in files)
            {
                if (ShouldIncludeIncrementalInputFile(rootDirectory, outDirectory, filePath))
                {
                    yield return filePath;
                }
            }
        }
    }

    private static bool IsIgnoredIncrementalDirectory(string? directoryName)
        => string.Equals(directoryName, ".git", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".jazor", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "node_modules", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".vs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".idea", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "bin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "obj", StringComparison.OrdinalIgnoreCase);

    private static bool ShouldIncludeIncrementalInputFile(
        string rootDirectory,
        string outDirectory,
        string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        if (string.Equals(Path.GetDirectoryName(fullPath), outDirectory, FilePathComparison))
        {
            return false;
        }

        var relativePath = Path.GetRelativePath(rootDirectory, fullPath).Replace('\\', '/');
        if (relativePath.StartsWith("public/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var fileName = Path.GetFileName(fullPath);
        if (string.Equals(fileName, "index.html", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "package.json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "jazor.config.json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IncrementalFingerprintExtensions.Contains(Path.GetExtension(fullPath));
    }

    private static bool TryReadIncrementalState(
        BuildContext context,
        out BuildIncrementalState state)
    {
        state = null!;
        var statePath = Path.Combine(context.OutDirectory, IncrementalStateFileName);
        if (!File.Exists(statePath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(statePath);
            var deserialized = JsonSerializer.Deserialize<BuildIncrementalState>(json);
            if (deserialized is null
                || string.IsNullOrWhiteSpace(deserialized.Fingerprint)
                || string.IsNullOrWhiteSpace(deserialized.ManifestPath))
            {
                return false;
            }

            state = deserialized;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool AreIncrementalOutputsAvailable(
        BuildContext context,
        BuildIncrementalState state)
    {
        if (!File.Exists(ResolveAbsolutePath(context.RootDirectory, state.ManifestPath)))
        {
            return false;
        }

        foreach (var chunk in state.Chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.FilePath)
                || !File.Exists(ResolveAbsolutePath(context.RootDirectory, chunk.FilePath)))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(chunk.SourceMapPath)
                && !File.Exists(ResolveAbsolutePath(context.RootDirectory, chunk.SourceMapPath!)))
            {
                return false;
            }
        }

        foreach (var asset in state.CssAssets.Concat(state.StaticAssets))
        {
            if (string.IsNullOrWhiteSpace(asset.FilePath)
                || !File.Exists(ResolveAbsolutePath(context.RootDirectory, asset.FilePath)))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(asset.SourceMapPath)
                && !File.Exists(ResolveAbsolutePath(context.RootDirectory, asset.SourceMapPath!)))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<BuildResult?> TryBuildHtmlRefreshIncrementalResultAsync(
        BuildContext context,
        BuildOptions options,
        BuildIncrementalState state,
        IReadOnlyDictionary<string, string> incrementalInputs,
        string incrementalFingerprint,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state.EntryRequestPath))
        {
            return null;
        }

        var changedPaths = GetIncrementalChangedPaths(state.Inputs, incrementalInputs);
        if (changedPaths.Count != 1
            || !string.Equals(changedPaths[0], "index.html", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string entryPointPath;
        try
        {
            entryPointPath = BuildEntryPointResolver.ResolveEntryPoint(options.RootDirectory);
        }
        catch
        {
            return null;
        }

        var currentEntryRequestPath = ResolveEntryRequestPath(options.RootDirectory, entryPointPath);
        if (!string.Equals(currentEntryRequestPath, state.EntryRequestPath, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await GenerateHtmlAsync(
            context,
            state.Chunks,
            state.CssAssets,
            state.StaticAssets,
            currentEntryRequestPath,
            cancellationToken);
        var manifestPath = await WriteManifestAsync(
            context,
            state.Chunks,
            state.CssAssets,
            state.StaticAssets,
            state.TotalSize,
            cancellationToken);

        var result = new BuildResult
        {
            Success = true,
            OutDirectory = context.OutDirectory,
            ManifestPath = manifestPath,
            Chunks = state.Chunks,
            CssAssets = state.CssAssets,
            StaticAssets = state.StaticAssets,
            Diagnostics =
            [
                .. context.Diagnostics,
                new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Info,
                    Message = IncrementalHtmlRefreshMessage
                }
            ],
            TotalSize = state.TotalSize
        };

        await PersistIncrementalStateAsync(
            context,
            result,
            incrementalFingerprint,
            incrementalInputs,
            currentEntryRequestPath,
            cancellationToken);
        return result;
    }

    private static async Task PersistIncrementalStateAsync(
        BuildContext context,
        BuildResult buildResult,
        string fingerprint,
        IReadOnlyDictionary<string, string> incrementalInputs,
        string entryRequestPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(buildResult.ManifestPath))
        {
            return;
        }

        var state = new BuildIncrementalState
        {
            Fingerprint = fingerprint,
            ManifestPath = ResolveRootRelativePath(context.RootDirectory, buildResult.ManifestPath),
            EntryRequestPath = entryRequestPath,
            Inputs = incrementalInputs,
            Chunks = buildResult.Chunks,
            CssAssets = buildResult.CssAssets,
            StaticAssets = buildResult.StaticAssets,
            TotalSize = buildResult.TotalSize
        };
        var statePath = Path.Combine(context.OutDirectory, IncrementalStateFileName);
        var stateJson = JsonSerializer.Serialize(
            state,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        await File.WriteAllTextAsync(statePath, stateJson, cancellationToken);
    }

    private static string ResolveRootRelativePath(string rootDirectory, string absoluteOrRelativePath)
    {
        if (string.IsNullOrWhiteSpace(absoluteOrRelativePath))
        {
            return string.Empty;
        }

        if (!Path.IsPathRooted(absoluteOrRelativePath))
        {
            return absoluteOrRelativePath.Replace('\\', '/');
        }

        var fullRootPath = Path.GetFullPath(rootDirectory);
        var fullPath = Path.GetFullPath(absoluteOrRelativePath);
        return IsInsideRoot(fullRootPath, fullPath)
            ? Path.GetRelativePath(fullRootPath, fullPath).Replace('\\', '/')
            : fullPath.Replace('\\', '/');
    }

    private static string ResolveAbsolutePath(string rootDirectory, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(
                rootDirectory,
                path.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void PrepareOutputDirectory(BuildContext context)
    {
        var rootDirectory = Path.GetFullPath(context.RootDirectory);
        var outDirectory = Path.GetFullPath(context.OutDirectory);
        if (!IsInsideRoot(rootDirectory, outDirectory)
            || string.Equals(rootDirectory, outDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Resolved build output directory '{outDirectory}' must stay inside project root '{rootDirectory}' and cannot point at the project root itself.");
        }

        if (Directory.Exists(outDirectory))
        {
            Directory.Delete(outDirectory, recursive: true);
        }

        Directory.CreateDirectory(outDirectory);
    }

    private static bool IsInsideRoot(string rootDirectory, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, candidatePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }

    private static string ResolveEntryRequestPath(string rootDirectory, string entryPointPath)
        => "/" + Path.GetRelativePath(rootDirectory, entryPointPath).Replace('\\', '/');

    private static string ToHtmlPath(BuildContext context, string rootRelativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootRelativePath);

        var absolutePath = Path.GetFullPath(Path.Combine(
            context.RootDirectory,
            rootRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var relativePath = Path.GetRelativePath(context.OutDirectory, absolutePath).Replace('\\', '/');
        return relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath[2..]
            : relativePath;
    }

    private static long GetAssetSize(BuildContext context, string rootRelativePath)
    {
        var absolutePath = Path.Combine(
            context.RootDirectory,
            rootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(absolutePath)
            ? new FileInfo(absolutePath).Length
            : 0;
    }

    private static long GetOptionalFileSize(BuildContext context, string? rootRelativePath)
        => string.IsNullOrWhiteSpace(rootRelativePath)
            ? 0
            : GetAssetSize(context, rootRelativePath);
}
