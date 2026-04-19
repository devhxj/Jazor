using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.Emit.SourceMaps;
using Jazor.VueHost.DevServer;

namespace Jazor.VueHost.Build;

internal sealed partial class BuildOrchestrator
{
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

            var optimizedContent = context.Options.Minify
                ? MinifyExtractedCss(
                    content,
                    preserveLineMapping: context.Options.GenerateSourceMap)
                : content;
            if (string.IsNullOrWhiteSpace(optimizedContent))
            {
                continue;
            }

            var fileName = CreateHashedAssetFileName(baseName, ".css", optimizedContent, context.Options.AssetHashLength);
            var outputPath = Path.Combine(context.AssetsDirectory, fileName);
            string? sourceMapPath = null;
            var finalContent = optimizedContent;
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
                            finalContent = AppendCssSourceMapComment(optimizedContent, Path.GetFileName(sourceMapOutputPath));
                            break;
                        case SourceMapOption.Inline:
                            finalContent = AppendInlineCssSourceMapComment(optimizedContent, sourceMap);
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

    private static string MinifyExtractedCss(string css, bool preserveLineMapping)
    {
        var withoutComments = RemoveBlockComments(css, preserveLineMapping);
        return preserveLineMapping
            ? MinifyCssPreservingLines(withoutComments)
            : MinifyCssCompact(withoutComments);
    }

    private static string RemoveBlockComments(string css, bool preserveLineMapping)
    {
        if (string.IsNullOrEmpty(css))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(css.Length);
        for (var index = 0; index < css.Length; index++)
        {
            var current = css[index];
            var hasNext = index + 1 < css.Length;
            if (current == '/' && hasNext && css[index + 1] == '*')
            {
                index += 2;
                while (index < css.Length)
                {
                    if (css[index] == '\n' && preserveLineMapping)
                    {
                        builder.Append('\n');
                    }

                    if (css[index] == '*' && index + 1 < css.Length && css[index + 1] == '/')
                    {
                        index++;
                        break;
                    }

                    index++;
                }

                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    private static string MinifyCssPreservingLines(string css)
    {
        var normalized = NormalizeLineEndings(css);
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        var lines = normalized.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (line.Length == 0)
            {
                lines[index] = string.Empty;
                continue;
            }

            line = Regex.Replace(line, @"\s+", " ");
            line = Regex.Replace(line, @"\s*([{}:;,>+~])\s*", "$1");
            line = line.Replace(";}", "}", StringComparison.Ordinal);
            lines[index] = line;
        }

        return string.Join('\n', lines);
    }

    private static string MinifyCssCompact(string css)
    {
        if (string.IsNullOrWhiteSpace(css))
        {
            return string.Empty;
        }

        var linePreserved = MinifyCssPreservingLines(css);
        var compact = Regex.Replace(linePreserved, @"\s+", " ");
        compact = Regex.Replace(compact, @"\s*([{}:;,>+~])\s*", "$1");
        compact = compact.Replace(";}", "}", StringComparison.Ordinal);
        return compact.Trim();
    }

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


}

