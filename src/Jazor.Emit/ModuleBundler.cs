using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DenoHost.Core;
using ECMAScript.Contract.SourceMaps;

namespace Jazor.Emit;

internal sealed class ModuleBundler
{
    private static readonly Regex ImportFromPattern = new(
        "(?<prefix>\\bfrom\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportOnlyPattern = new(
        "(?<prefix>\\bimport\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly SourceMapChainBuilder ChainBuilder = new();
    private static readonly SourceMapWriter SourceMapWriter = new();
    private static readonly RazorVueHostAssetWriter RazorVueHostAssetWriter = new();
    private static readonly RazorVueUpdatePlanWriter RazorVueUpdatePlanWriter = new();

    public async Task<BundleResult> BundleAsync(BundleOptions options)
    {
        var manifest = ManifestModel.TryLoad(options.ManifestPath);
        if (manifest is null)
            return BundleResult.Fail(6, $"Manifest was not found: '{options.ManifestPath}'.");

        var razorVueManifestPath = RazorVueModuleWriter.GetManifestPath(options.ManifestPath);
        var razorVueManifest = RazorVueManifestSerializer.TryLoad(razorVueManifestPath);
        var previousRazorVueManifest = TryLoadPreviousRazorVueManifest(options);

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
            var rewritten = RewriteModuleImports(content, relativePath, knownPaths);
            await File.WriteAllTextAsync(targetPath, rewritten, Utf8WithoutBom);

            var sourceMapPath = sourcePath + ".map";
            if (!File.Exists(sourceMapPath))
                continue;

            await File.WriteAllTextAsync(targetPath + ".map", await File.ReadAllTextAsync(sourceMapPath), Utf8WithoutBom);
        }

        var razorVueHostRequirementsRelativePath = await TryCopyRazorVueHostRequirementsAsync(
            options.InputDirectory,
            options.ManifestPath,
            bundleWorkspace);

        var tempEntryPath = Path.Combine(bundleWorkspace, "__jazor_bundle_entry__.mjs");
        await WriteBundleEntryAsync(
            tempEntryPath,
            bundleWorkspace,
            entryRelativePaths,
            razorVueHostRequirementsRelativePath);

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
            await EnsureBundleSourceMappingUrlAsync(options.OutputPath);
            RazorVueHostAssetWriter.Sync(options.OutputPath, razorVueManifest);
            WriteRazorVueUpdatePlanIfRequested(options, previousRazorVueManifest, razorVueManifest);
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
        IReadOnlyList<string> entryRelativePaths,
        string? razorVueHostRequirementsRelativePath)
    {
        var entryLines = entryRelativePaths
            .Select(static relativePath => $"export * from \"./{relativePath}\";")
            .ToList();
        if (!string.IsNullOrWhiteSpace(razorVueHostRequirementsRelativePath))
        {
            // Keep bundled hosts on the same compiler-owned metadata contract as unbundled output.
            entryLines.Add(
                $"export {{ razorVueHostAssemblyName, razorVueHostGeneratedAtUtc, razorVueHostModules, razorVueHostRequirements, razorVuePluginRequirements, razorVueStyles }} from \"./{razorVueHostRequirementsRelativePath}\";");
        }

        var entrySource = string.Join("\n", entryLines);
        var entryMapPath = tempEntryPath + ".map";
        var entrySourcePaths = entryRelativePaths.ToList();
        if (!string.IsNullOrWhiteSpace(razorVueHostRequirementsRelativePath))
            entrySourcePaths.Add(razorVueHostRequirementsRelativePath);

        var entryMap = await BuildEntrySourceMapAsync(bundleWorkspace, Path.GetFileName(tempEntryPath), entrySourcePaths);
        var entryCode = SourceMapWriter.AppendSourceMappingUrl(entrySource, Path.GetFileName(entryMapPath));

        await File.WriteAllTextAsync(tempEntryPath, entryCode, Utf8WithoutBom);
        await File.WriteAllTextAsync(entryMapPath, SourceMapWriter.Write(entryMap), Utf8WithoutBom);
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

    private static RazorVueManifestModel? TryLoadPreviousRazorVueManifest(BundleOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PreviousRazorVueManifestPath))
            return null;

        return RazorVueManifestSerializer.TryLoad(options.PreviousRazorVueManifestPath);
    }

    private static void WriteRazorVueUpdatePlanIfRequested(
        BundleOptions options,
        RazorVueManifestModel? previousManifest,
        RazorVueManifestModel? currentManifest)
    {
        if (string.IsNullOrWhiteSpace(options.RazorVueUpdatePlanPath))
            return;

        if (currentManifest is null)
        {
            DeleteIfExists(options.RazorVueUpdatePlanPath);
            return;
        }

        // The previous manifest is snapshotted before emit because emit may rewrite
        // or delete the live manifest before bundling starts.
        var diff = RazorVueManifestDiffer.Diff(previousManifest, currentManifest);
        RazorVueUpdatePlanWriter.Write(options.RazorVueUpdatePlanPath, previousManifest, currentManifest, diff);
    }

    private static async Task<string?> TryCopyRazorVueHostRequirementsAsync(
        string inputDirectory,
        string manifestPath,
        string bundleWorkspace)
    {
        var razorVueManifestPath = RazorVueModuleWriter.GetManifestPath(manifestPath);
        var razorVueManifest = RazorVueManifestSerializer.TryLoad(razorVueManifestPath);
        if (razorVueManifest is null || razorVueManifest.Modules.Count == 0)
            return null;

        var sourcePath = RazorVueModuleWriter.GetHostRequirementsModulePath(inputDirectory);
        if (!File.Exists(sourcePath))
            return null;

        var normalizedInputDirectory = EnsureDirectorySeparator(Path.GetFullPath(inputDirectory));
        var relativePath = Path.GetRelativePath(normalizedInputDirectory, Path.GetFullPath(sourcePath)).Replace('\\', '/');
        var targetPath = Path.Combine(bundleWorkspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var targetDirectory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        await File.WriteAllTextAsync(targetPath, await File.ReadAllTextAsync(sourcePath), Utf8WithoutBom);
        return relativePath;
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

    private static async Task EnsureBundleSourceMappingUrlAsync(string outputPath)
    {
        if (!File.Exists(outputPath))
            return;

        var code = await File.ReadAllTextAsync(outputPath);
        if (code.Contains("sourceMappingURL=", StringComparison.Ordinal))
            return;

        var updated = SourceMapWriter.AppendSourceMappingUrl(code, Path.GetFileName(GetBundleMapPath(outputPath)));
        await File.WriteAllTextAsync(outputPath, updated, Utf8WithoutBom);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static string GetBundleMapPath(string outputPath)
        => outputPath + ".map";

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static string GetRootAssemblyName(ManifestModel manifest)
    {
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

    private static string RewriteModuleImports(string content, string currentModulePath, IReadOnlyDictionary<string, string> knownPaths)
    {
        var rewritten = ImportFromPattern.Replace(content, match => RewriteMatch(match, currentModulePath, knownPaths));
        rewritten = ImportOnlyPattern.Replace(rewritten, match => RewriteMatch(match, currentModulePath, knownPaths));
        return rewritten;
    }

    private static string RewriteMatch(Match match, string currentModulePath, IReadOnlyDictionary<string, string> knownPaths)
    {
        var importPath = match.Groups["path"].Value.Replace('\\', '/');
        if (importPath.StartsWith("./", StringComparison.Ordinal) ||
            importPath.StartsWith("../", StringComparison.Ordinal) ||
            importPath.StartsWith("/", StringComparison.Ordinal) ||
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
}

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
