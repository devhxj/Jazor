using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DenoHost.Core;
using Jazor.Common.SourceMaps;

namespace Jazor.Emit;

/// <summary>Bundles generated modules with the packaged Deno runtime and local library manifests.</summary>
internal sealed class DenoBundler
{
    private static readonly Regex ImportFromPattern = new(
        "(?<prefix>\\bfrom\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportOnlyPattern = new(
        "(?<prefix>\\bimport\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions DenoConfigSerializerOptions = new()
    {
        WriteIndented = true
    };
    private static readonly SourceMapChainBuilder ChainBuilder = new();
    private static readonly SourceMapWriter SourceMapWriter = new();

    public async Task<BundleResult> BundleAsync(BundleOptions options)
    {
        ManifestModel manifest;
        try
        {
            manifest = ManifestModel.TryLoad(options.ManifestPath)
                ?? throw new FileNotFoundException("Manifest was not found.", options.ManifestPath);
        }
        catch (FileNotFoundException)
        {
            return BundleResult.Fail(6, $"Manifest was not found: '{options.ManifestPath}'.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or InvalidOperationException)
        {
            return BundleResult.Fail(6, $"Jazor manifest could not be read: '{options.ManifestPath}'. {ex.Message}");
        }

        var relativePaths = manifest.Modules
            .Select(static module => module.RelativePath.Replace('\\', '/'))
            .Where(static relativePath => !string.IsNullOrWhiteSpace(relativePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static relativePath => relativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (relativePaths.Length == 0)
            return BundleResult.Fail(7, $"No modules were found in '{options.ManifestPath}'.");

        Directory.CreateDirectory(options.InputDirectory);

        var outputDirectory = Path.GetDirectoryName(options.OutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var bundleWorkspace = Path.Combine(options.InputDirectory, "__jazor_bundle__");
        if (Directory.Exists(bundleWorkspace))
            Directory.Delete(bundleWorkspace, recursive: true);

        Directory.CreateDirectory(bundleWorkspace);

        var knownPaths = relativePaths.ToDictionary(
            static relativePath => relativePath,
            static relativePath => relativePath,
            StringComparer.OrdinalIgnoreCase);
        LibraryAssets libraries;
        PreparedAssets assetPreparation;
        try
        {
            libraries = new LibraryMaterializer().Materialize(
                options.LibraryManifests ?? [],
                bundleWorkspace,
                BuildMode.Production,
                manifest.Modules.SelectMany(static module => module.PackageImports ?? []),
                relativePaths);
            foreach (var (specifier, path) in libraries.ImportPaths)
                knownPaths[specifier] = path;
            assetPreparation = await PrepareAssetsAsync(manifest, options, bundleWorkspace);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return BundleResult.Fail(6, $"Assets could not be prepared: {ex.Message}");
        }

        var rootAssemblyName = GetRootAssemblyName(manifest);
        var entryRelativePaths = manifest.Modules
            .Where(module => StringComparer.OrdinalIgnoreCase.Equals(module.AssemblyName, rootAssemblyName))
            .Select(static module => module.RelativePath.Replace('\\', '/'))
            .Where(static relativePath => !string.IsNullOrWhiteSpace(relativePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static relativePath => relativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (entryRelativePaths.Length == 0)
            entryRelativePaths = relativePaths;

        foreach (var relativePath in relativePaths)
        {
            var sourcePath = Path.Combine(options.InputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var targetPath = Path.Combine(bundleWorkspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var content = await File.ReadAllTextAsync(sourcePath);
            var rewritten = RewriteModuleImports(content, relativePath, knownPaths, assetPreparation.ImportRewrites);
            await File.WriteAllTextAsync(targetPath, rewritten, Utf8WithoutBom);

            var sourceMapPath = sourcePath + ".map";
            if (!File.Exists(sourceMapPath))
                continue;

            await File.WriteAllTextAsync(targetPath + ".map", await File.ReadAllTextAsync(sourceMapPath), Utf8WithoutBom);
        }

        var tempEntryPath = Path.Combine(bundleWorkspace, "__jazor_bundle_entry__.mjs");
        await WriteBundleEntryAsync(
            tempEntryPath,
            bundleWorkspace,
            entryRelativePaths);
        await EnsureBundleWorkspaceDenoConfigAsync(bundleWorkspace, libraries.ImportPaths);

        try
        {
            var commandArgs = new[]
            {
                "bundle",
                "--platform",
                "browser",
                "--sourcemap=external",
                "-o",
                options.OutputPath,
                Path.GetFileName(tempEntryPath)
            };

            await Deno.Execute(
                new DenoExecuteBaseOptions
                {
                    WorkingDirectory = bundleWorkspace
                },
                commandArgs);

            await TryRewriteBundleSourceMapAsync(options.OutputPath, bundleWorkspace, relativePaths, tempEntryPath);
            await EnsureBundleRazorSourceMapUrlAsync(options.OutputPath);
            await WriteBundleCssAsync(
                options.OutputPath,
                assetPreparation.StylePaths.Concat(libraries.StylePaths.Select(path => Path.Combine(bundleWorkspace, path))).ToArray());
            CopyStaticAssetsToOutput(options.OutputPath, assetPreparation.StaticAssets);
            return BundleResult.Success(options.OutputPath, relativePaths.Length);
        }
        catch (Exception ex)
        {
            return BundleResult.Fail(8, ex.ToString());
        }
        finally
        {
            try
            {
                if (Directory.Exists(bundleWorkspace))
                    Directory.Delete(bundleWorkspace, recursive: true);
            }
            catch
            {
            }
        }
    }

    private static async Task WriteBundleEntryAsync(
        string tempEntryPath,
        string bundleWorkspace,
        IReadOnlyList<string> entryRelativePaths)
    {
        var entryLines = entryRelativePaths
            .Select(static relativePath => $"export * from \"./{relativePath}\";")
            .ToList();

        var entrySource = string.Join("\n", entryLines);
        var entryMapPath = tempEntryPath + ".map";
        var entrySourcePaths = entryRelativePaths.ToList();

        var entryMap = await BuildEntrySourceMapAsync(bundleWorkspace, Path.GetFileName(tempEntryPath), entrySourcePaths);
        var entryCode = SourceMapWriter.AppendSourceMappingUrl(entrySource, Path.GetFileName(entryMapPath));

        await File.WriteAllTextAsync(tempEntryPath, entryCode, Utf8WithoutBom);
        await File.WriteAllTextAsync(entryMapPath, SourceMapWriter.Write(entryMap), Utf8WithoutBom);
    }

    private static async Task EnsureBundleWorkspaceDenoConfigAsync(
        string bundleWorkspace,
        IReadOnlyDictionary<string, string> importPaths)
    {
        var denoConfigPath = Path.Combine(bundleWorkspace, "deno.json");
        var denoConfig = JsonSerializer.Serialize(
            new
            {
                imports = importPaths.ToDictionary(
                    static item => item.Key,
                    static item => "./" + item.Value.Replace('\\', '/'),
                    StringComparer.Ordinal)
            },
            DenoConfigSerializerOptions);

        await File.WriteAllTextAsync(denoConfigPath, denoConfig, Utf8WithoutBom);
    }

    private static async Task<PreparedAssets> PrepareAssetsAsync(
        ManifestModel manifest,
        BundleOptions options,
        string bundleWorkspace)
    {
        if (manifest.Assets.Count == 0)
        {
            return new PreparedAssets(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                [],
                []);
        }

        if (string.IsNullOrWhiteSpace(options.SourceRoot))
            throw new InvalidOperationException("Manifest assets require an explicit source root.");

        var rewriteByArtifactPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var stylePaths = new List<string>();
        var staticAssets = new List<StaticAsset>();
        foreach (var asset in manifest.Assets)
        {
            var sourcePath = GetSafePath(options.SourceRoot, asset.SourcePath);
            var artifactPath = GetSafePath(bundleWorkspace, asset.ArtifactPath);
            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            if (!string.IsNullOrWhiteSpace(artifactDirectory))
                Directory.CreateDirectory(artifactDirectory);

            File.Copy(sourcePath, artifactPath, overwrite: true);

            if (!string.Equals(asset.Kind, AssetEntry.KindVueSfc, StringComparison.OrdinalIgnoreCase))
            {
                staticAssets.Add(new StaticAsset(sourcePath, asset.ArtifactPath.Replace('\\', '/')));
                continue;
            }

            var compiledArtifactPath = artifactPath + ".mjs";
            var compiledStylePath = artifactPath + ".css";
            await CompileVueSfcAsync(artifactPath, compiledArtifactPath, compiledStylePath);
            if (File.Exists(compiledStylePath))
                stylePaths.Add(compiledStylePath);

            rewriteByArtifactPath[asset.ArtifactPath.Replace('\\', '/')] = asset.ArtifactPath.Replace('\\', '/') + ".mjs";
        }

        return new PreparedAssets(rewriteByArtifactPath, stylePaths, staticAssets);
    }

    private static async Task CompileVueSfcAsync(string inputPath, string outputPath, string styleOutputPath)
    {
        var scriptPath = Path.Combine(Path.GetDirectoryName(inputPath)!, "__jazor_compile_sfc__.mjs");
        var compilerPath = Path.Combine(
            AppContext.BaseDirectory,
            "tooling",
            "vue",
            "compiler-sfc.esm-browser.js");
        if (!File.Exists(compilerPath))
            throw new FileNotFoundException("The bundled Vue SFC compiler was not found.", compilerPath);

        // compiler-sfc is shipped with Jazor.Emit. Keep SFC compilation on file: URLs so a
        // consumer build cannot consult npm, Deno's package cache, or the network.
        // compiler-sfc 随 Emit 工具交付；SFC 编译只允许 file: 导入，不访问 npm/cache/network。
        var compilerUrl = JsonSerializer.Serialize(new Uri(compilerPath).AbsoluteUri);
        var compilerScript = VueSfcCompilerScript.Replace(
            "__JAZOR_COMPILER_SFC_URL__",
            compilerUrl,
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(scriptPath, compilerScript, Utf8WithoutBom);

        try
        {
            await Deno.Execute(
                new DenoExecuteBaseOptions
                {
                    WorkingDirectory = Path.GetDirectoryName(inputPath)
                },
                [
                    "run",
                    "--allow-read",
                    "--allow-write",
                    Path.GetFileName(scriptPath),
                    inputPath,
                    outputPath,
                    styleOutputPath
                ]);
        }
        finally
        {
            try
            {
                if (File.Exists(scriptPath))
                    File.Delete(scriptPath);
            }
            catch
            {
            }
        }
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = EnsureDirectorySeparator(Path.GetFullPath(root));
        var fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Path escapes the toolchain root: '{relativePath}'.");

        return fullPath;
    }

    private static async Task WriteBundleCssAsync(string outputPath, IReadOnlyList<string> stylePaths)
    {
        if (stylePaths.Count == 0)
            return;

        var cssBuilder = new StringBuilder();
        foreach (var stylePath in stylePaths)
        {
            var css = await File.ReadAllTextAsync(stylePath);
            if (string.IsNullOrWhiteSpace(css))
                continue;

            if (cssBuilder.Length > 0)
                cssBuilder.Append('\n');

            cssBuilder.Append(css.TrimEnd('\r', '\n'));
            cssBuilder.Append('\n');
        }

        if (cssBuilder.Length == 0)
            return;

        var cssOutputPath = Path.ChangeExtension(outputPath, ".css");
        await File.WriteAllTextAsync(cssOutputPath, cssBuilder.ToString(), Utf8WithoutBom);
    }

    private static void CopyStaticAssetsToOutput(string outputPath, IReadOnlyList<StaticAsset> staticAssets)
    {
        if (staticAssets.Count == 0)
            return;

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? string.Empty;
        foreach (var asset in staticAssets)
        {
            var targetPath = GetSafePath(outputDirectory, asset.OutputRelativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            File.Copy(asset.SourcePath, targetPath, overwrite: true);
        }
    }

    private static async Task<SourceMapDocument> BuildEntrySourceMapAsync(string bundleWorkspace, string entryFileName, IReadOnlyList<string> entryRelativePaths)
    {
        var sources = new List<SourceMapSource>(entryRelativePaths.Count);
        var segments = new List<SourceMapSegment>(entryRelativePaths.Count);

        for (var index = 0; index < entryRelativePaths.Count; index++)
        {
            var relativePath = entryRelativePaths[index];
            var modulePath = Path.Combine(bundleWorkspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var moduleContent = File.Exists(modulePath)
                ? await File.ReadAllTextAsync(modulePath)
                : null;

            sources.Add(new SourceMapSource(relativePath, moduleContent));
            segments.Add(new SourceMapSegment(index, 0, index, 0, 0));
        }

        return new SourceMapDocument(entryFileName, sources, segments);
    }

    private static async Task TryRewriteBundleSourceMapAsync(string outputPath, string bundleWorkspace, IReadOnlyList<string> relativePaths, string tempEntryPath)
    {
        try
        {
            await RewriteBundleSourceMapAsync(outputPath, bundleWorkspace, relativePaths, tempEntryPath);
        }
        catch
        {
        }
    }

    private static async Task RewriteBundleSourceMapAsync(string outputPath, string bundleWorkspace, IReadOnlyList<string> relativePaths, string tempEntryPath)
    {
        var bundleMapPath = GetBundleMapPath(outputPath);
        if (!File.Exists(bundleMapPath))
            return;

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? string.Empty;
        var moduleMapJsonByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var sourceAliasByPath = new Dictionary<string, SourceMapSource>(StringComparer.OrdinalIgnoreCase);
        foreach (var relativePath in relativePaths)
        {
            var workspaceModulePath = Path.Combine(bundleWorkspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var workspaceModuleMapPath = workspaceModulePath + ".map";
            if (!File.Exists(workspaceModuleMapPath))
                continue;

            var mapJson = await File.ReadAllTextAsync(workspaceModuleMapPath);
            var mapDocument = ChainBuilder.Chain(mapJson, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            AddModuleMapLookup(moduleMapJsonByPath, relativePath, mapJson);
            AddModuleMapLookup(moduleMapJsonByPath, "./" + relativePath, mapJson);
            AddModuleMapLookup(moduleMapJsonByPath, Path.GetFileName(relativePath), mapJson);
            AddModuleMapLookup(moduleMapJsonByPath, workspaceModulePath, mapJson);
            AddModuleMapLookup(moduleMapJsonByPath, NormalizeSourceLookupKey(workspaceModulePath), mapJson);
            AddModuleMapLookup(moduleMapJsonByPath, new Uri(Path.GetFullPath(workspaceModulePath)).AbsoluteUri, mapJson);
            AddOutputRelativeLookup(moduleMapJsonByPath, outputDirectory, workspaceModulePath, mapJson);
            AddSourceAliases(sourceAliasByPath, outputDirectory, workspaceModulePath, mapDocument.Sources);
        }

        var entryMapPath = tempEntryPath + ".map";
        if (File.Exists(entryMapPath))
        {
            var entryMapJson = await File.ReadAllTextAsync(entryMapPath);
            AddModuleMapLookup(moduleMapJsonByPath, Path.GetFileName(tempEntryPath), entryMapJson);
            AddModuleMapLookup(moduleMapJsonByPath, "./" + Path.GetFileName(tempEntryPath), entryMapJson);
            AddModuleMapLookup(moduleMapJsonByPath, tempEntryPath, entryMapJson);
            AddModuleMapLookup(moduleMapJsonByPath, NormalizeSourceLookupKey(tempEntryPath), entryMapJson);
            AddModuleMapLookup(moduleMapJsonByPath, new Uri(Path.GetFullPath(tempEntryPath)).AbsoluteUri, entryMapJson);
            AddOutputRelativeLookup(moduleMapJsonByPath, outputDirectory, tempEntryPath, entryMapJson);
        }

        var bundleMapJson = await File.ReadAllTextAsync(bundleMapPath);
        var chainedDocument = ChainBuilder.Chain(bundleMapJson, moduleMapJsonByPath);
        chainedDocument = RewriteAliasedSources(chainedDocument, sourceAliasByPath);
        var finalDocument = chainedDocument with { File = Path.GetFileName(outputPath) };
        await File.WriteAllTextAsync(bundleMapPath, SourceMapWriter.Write(finalDocument), Utf8WithoutBom);
    }

    private static void AddSourceAliases(
        Dictionary<string, SourceMapSource> sourceAliasByPath,
        string outputDirectory,
        string workspaceModulePath,
        IReadOnlyList<SourceMapSource> sources)
    {
        var workspaceModuleDirectory = Path.GetDirectoryName(workspaceModulePath) ?? Path.GetDirectoryName(Path.GetFullPath(workspaceModulePath)) ?? string.Empty;
        foreach (var source in sources)
        {
            var fullSourcePath = Path.GetFullPath(Path.Combine(workspaceModuleDirectory, source.Path.Replace('/', Path.DirectorySeparatorChar)));
            AddSourceAlias(sourceAliasByPath, Path.GetRelativePath(outputDirectory, fullSourcePath), source);
            AddSourceAlias(sourceAliasByPath, fullSourcePath, source);
            AddSourceAlias(sourceAliasByPath, new Uri(fullSourcePath).AbsoluteUri, source);
        }
    }

    private static void AddSourceAlias(Dictionary<string, SourceMapSource> sourceAliasByPath, string key, SourceMapSource source)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        sourceAliasByPath[key] = source;
        sourceAliasByPath[NormalizeSourceLookupKey(key)] = source;
    }

    private static SourceMapDocument RewriteAliasedSources(SourceMapDocument document, IReadOnlyDictionary<string, SourceMapSource> sourceAliasByPath)
    {
        if (sourceAliasByPath.Count == 0)
            return document;

        var sources = new List<SourceMapSource>();
        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var remappedSegments = new List<SourceMapSegment>(document.Segments.Count);

        foreach (var segment in document.Segments)
        {
            if (segment.SourceIndex < 0 || segment.SourceIndex >= document.Sources.Count)
                continue;

            var source = document.Sources[segment.SourceIndex];
            source = ResolveAliasedSource(source, sourceAliasByPath);

            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, source);
            remappedSegments.Add(segment with { SourceIndex = sourceIndex });
        }

        return document with { Sources = sources, Segments = remappedSegments };
    }

    private static SourceMapSource ResolveAliasedSource(SourceMapSource source, IReadOnlyDictionary<string, SourceMapSource> sourceAliasByPath)
    {
        if (sourceAliasByPath.TryGetValue(source.Path, out var aliasedSource))
            return aliasedSource;

        var normalizedPath = NormalizeSourceLookupKey(source.Path);
        if (sourceAliasByPath.TryGetValue(normalizedPath, out aliasedSource))
            return aliasedSource;

        foreach (var candidate in sourceAliasByPath.Values)
        {
            var candidatePath = NormalizeSourceLookupKey(candidate.Path);
            if (string.IsNullOrWhiteSpace(candidatePath))
                continue;

            if (normalizedPath.EndsWith("/" + candidatePath, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedPath, candidatePath, StringComparison.OrdinalIgnoreCase))
                return candidate;
        }

        return source;
    }

    private static int GetOrAddSourceIndex(List<SourceMapSource> sources, Dictionary<string, int> sourceIndexByPath, SourceMapSource source)
    {
        var normalizedPath = NormalizeSourceLookupKey(source.Path);
        if (sourceIndexByPath.TryGetValue(normalizedPath, out var index))
            return index;

        index = sources.Count;
        sources.Add(source);
        sourceIndexByPath[normalizedPath] = index;
        return index;
    }

    private static string NormalizeSourceLookupKey(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) && uri.IsFile)
            normalized = uri.LocalPath;

        normalized = normalized.Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];

        return normalized;
    }

    private static void AddOutputRelativeLookup(Dictionary<string, string> lookup, string outputDirectory, string targetPath, string mapJson)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
            return;

        var relativeToOutput = Path.GetRelativePath(outputDirectory, Path.GetFullPath(targetPath)).Replace('\\', '/');
        AddModuleMapLookup(lookup, relativeToOutput, mapJson);
        if (!relativeToOutput.StartsWith("./", StringComparison.Ordinal) &&
            !relativeToOutput.StartsWith("../", StringComparison.Ordinal))
        {
            AddModuleMapLookup(lookup, "./" + relativeToOutput, mapJson);
        }
    }

    private static void AddModuleMapLookup(Dictionary<string, string> lookup, string key, string mapJson)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        lookup[key] = mapJson;
    }

    private static async Task EnsureBundleRazorSourceMapUrlAsync(string outputPath)
    {
        if (!File.Exists(outputPath))
            return;

        var code = await File.ReadAllTextAsync(outputPath);
        if (code.Contains("sourceMappingURL=", StringComparison.Ordinal))
            return;

        var updated = SourceMapWriter.AppendSourceMappingUrl(code, Path.GetFileName(GetBundleMapPath(outputPath)));
        await File.WriteAllTextAsync(outputPath, updated, Utf8WithoutBom);
    }

    private static string GetBundleMapPath(string outputPath)
        => outputPath + ".map";

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static string GetRootAssemblyName(ManifestModel manifest)
    {
        if (!string.IsNullOrWhiteSpace(manifest.RootAssemblyName))
            return manifest.RootAssemblyName;

        try
        {
            return AssemblyName.GetAssemblyName(manifest.RootAssemblyPath).Name ??
                Path.GetFileNameWithoutExtension(manifest.RootAssemblyPath);
        }
        catch
        {
            return Path.GetFileNameWithoutExtension(manifest.RootAssemblyPath);
        }
    }

    private static string RewriteModuleImports(
        string content,
        string currentModulePath,
        IReadOnlyDictionary<string, string> knownPaths,
        IReadOnlyDictionary<string, string> assetRewrites)
    {
        var rewritten = ImportFromPattern.Replace(content, match => RewriteMatch(match, currentModulePath, knownPaths, assetRewrites));
        rewritten = ImportOnlyPattern.Replace(rewritten, match => RewriteMatch(match, currentModulePath, knownPaths, assetRewrites));
        return rewritten;
    }

    private static string RewriteMatch(
        Match match,
        string currentModulePath,
        IReadOnlyDictionary<string, string> knownPaths,
        IReadOnlyDictionary<string, string> assetRewrites)
    {
        var importPath = match.Groups["path"].Value.Replace('\\', '/');
        if (importPath.StartsWith("./", StringComparison.Ordinal) ||
            importPath.StartsWith("../", StringComparison.Ordinal))
        {
            return RewriteRelativeAssetMatch(match, currentModulePath, importPath, assetRewrites);
        }

        if (importPath.StartsWith("/", StringComparison.Ordinal) ||
            importPath.Contains(':', StringComparison.Ordinal))
        {
            return match.Value;
        }

        if (!knownPaths.TryGetValue(importPath, out var targetPath))
            return match.Value;

        var currentDirectory = Path.GetDirectoryName(currentModulePath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty;
        var relativePath = Path.GetRelativePath(
            string.IsNullOrEmpty(currentDirectory) ? "." : currentDirectory,
            targetPath.Replace('/', Path.DirectorySeparatorChar))
            .Replace('\\', '/');

        if (!relativePath.StartsWith("./", StringComparison.Ordinal) &&
            !relativePath.StartsWith("../", StringComparison.Ordinal))
        {
            relativePath = "./" + relativePath;
        }

        return $"{match.Groups["prefix"].Value}{relativePath}{match.Groups["suffix"].Value}";
    }

    private static string RewriteRelativeAssetMatch(
        Match match,
        string currentModulePath,
        string importPath,
        IReadOnlyDictionary<string, string> assetRewrites)
    {
        if (assetRewrites.Count == 0)
            return match.Value;

        var currentDirectory = Path.GetDirectoryName(currentModulePath.Replace('/', Path.DirectorySeparatorChar))?.Replace('\\', '/') ?? string.Empty;
        var normalizedImport = NormalizeArtifactImportPath(currentDirectory, importPath);
        if (!assetRewrites.TryGetValue(normalizedImport, out var rewrittenTarget))
            return match.Value;

        var relativePath = Path.GetRelativePath(
            string.IsNullOrEmpty(currentDirectory) ? "." : currentDirectory,
            rewrittenTarget.Replace('/', Path.DirectorySeparatorChar))
            .Replace('\\', '/');

        if (!relativePath.StartsWith("./", StringComparison.Ordinal) &&
            !relativePath.StartsWith("../", StringComparison.Ordinal))
        {
            relativePath = "./" + relativePath;
        }

        return $"{match.Groups["prefix"].Value}{relativePath}{match.Groups["suffix"].Value}";
    }

    private static string NormalizeArtifactImportPath(string currentDirectory, string importPath)
    {
        var parts = new List<string>();
        foreach (var segment in (currentDirectory + "/" + importPath).Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (parts.Count > 0)
                    parts.RemoveAt(parts.Count - 1);

                continue;
            }

            parts.Add(segment);
        }

        return string.Join("/", parts);
    }

    private const string VueSfcCompilerScript =
        """
        import { parse, compileScript, compileTemplate, compileStyle } from __JAZOR_COMPILER_SFC_URL__;

        const [inputPath, outputPath, styleOutputPath] = Deno.args;
        const source = await Deno.readTextFile(inputPath);
        const parsed = parse(source, { filename: inputPath });
        if (parsed.errors.length > 0) {
          throw new Error(parsed.errors.map(error => String(error)).join("\n"));
        }

        const descriptor = parsed.descriptor;
        const id = "jazor-" + await digest(inputPath);
        const script = compileScript(descriptor, { id });
        const template = compileTemplate({
          id,
          filename: inputPath,
          source: descriptor.template ? descriptor.template.content : "",
          compilerOptions: {
            bindingMetadata: script.bindings
          }
        });
        if (template.errors.length > 0) {
          throw new Error(template.errors.map(error => String(error)).join("\n"));
        }

        const scriptCode = script.content.replace(/\bexport\s+default\b/, "const __sfc__ =");
        const templateCode = template.code.replaceAll('from "vue"', 'from "vue"').replaceAll("from 'vue'", "from 'vue'");
        await Deno.writeTextFile(outputPath, `${scriptCode}\n${templateCode}\n__sfc__.render = render;\nexport default __sfc__;\n`);

        const styles = [];
        for (const style of descriptor.styles) {
          const compiledStyle = compileStyle({
            id,
            filename: inputPath,
            source: style.content,
            scoped: style.scoped
          });
          if (compiledStyle.errors.length > 0) {
            throw new Error(compiledStyle.errors.map(error => String(error)).join("\n"));
          }
          if (compiledStyle.code.trim().length > 0) {
            styles.push(compiledStyle.code.trim());
          }
        }
        if (styles.length > 0) {
          await Deno.writeTextFile(styleOutputPath, styles.join("\n"));
        }

        async function digest(value) {
          const bytes = new TextEncoder().encode(value);
          const hash = await crypto.subtle.digest("SHA-256", bytes);
          return Array.from(new Uint8Array(hash)).map(item => item.toString(16).padStart(2, "0")).join("").slice(0, 8);
        }
        """;

    /// <summary>Temporary SFC rewrites, styles, and static files prepared for Deno.</summary>
    private sealed record PreparedAssets(
        IReadOnlyDictionary<string, string> ImportRewrites,
        IReadOnlyList<string> StylePaths,
        IReadOnlyList<StaticAsset> StaticAssets);

    /// <summary>Maps one source file to its publish-relative path.</summary>
    private sealed record StaticAsset(
        string SourcePath,
        string OutputRelativePath);
}

/// <summary>Common success or failure result returned by either bundler.</summary>
internal sealed record BundleResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    string? OutputPath,
    int ModuleCount)
{
    public static BundleResult Success(string outputPath, int moduleCount)
        => new(true, 0, null, outputPath, moduleCount);

    public static BundleResult Fail(int exitCode, string error)
        => new(false, exitCode, error, null, 0);
}
