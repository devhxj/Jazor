using System.Text;
using System.Text.RegularExpressions;
using NetPack;
using NetPack.Graph;

namespace Jazor.Emit;

internal sealed class NetpackModuleBundler
{
    private static readonly Regex ImportFromPattern = new(
        "(?<prefix>\\bfrom\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportOnlyPattern = new(
        "(?<prefix>\\bimport\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

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
        catch (Exception ex) when (ex is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException)
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

        var bundleWorkspaceRoot = string.IsNullOrWhiteSpace(options.SourceRoot)
            ? options.InputDirectory
            : options.SourceRoot;
        var bundleWorkspace = Path.Combine(bundleWorkspaceRoot, "__jazor_netpack_bundle__");
        if (Directory.Exists(bundleWorkspace))
            Directory.Delete(bundleWorkspace, recursive: true);

        Directory.CreateDirectory(bundleWorkspace);

        try
        {
            var frontendAssets = CopyFrontendAssets(manifest, options, bundleWorkspace);
            var importRewrites = CreateImportRewrites(relativePaths, frontendAssets.ImportRewrites);
            foreach (var relativePath in relativePaths)
            {
                var sourcePath = GetSafePath(options.InputDirectory, relativePath);
                var targetPath = GetSafePath(bundleWorkspace, relativePath);
                var targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                var content = await File.ReadAllTextAsync(sourcePath);
                var rewritten = RewriteModuleImports(content, relativePath, importRewrites);
                await File.WriteAllTextAsync(targetPath, rewritten, Utf8WithoutBom);
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

            var entryPath = Path.Combine(bundleWorkspace, "__jazor_netpack_entry__.mjs");
            await File.WriteAllTextAsync(
                entryPath,
                string.Join("\n", entryRelativePaths.Select(static relativePath => $"export * from \"./{relativePath}\";")),
                Utf8WithoutBom);

            var result = await Bundler.BundleAsync(
                entryPath,
                new global::NetPack.BundleOptions
                {
                    Format = ModuleFormat.Esm,
                    Platform = Platform.Web,
                    SourceMaps = true,
                    EntryNames = Path.GetFileNameWithoutExtension(options.OutputPath),
                    ExternalPackages = false
                });

            var wroteBundle = WriteOutputs(result.Outputs, options.OutputPath);
            if (!File.Exists(options.OutputPath))
            {
                return BundleResult.Fail(
                    8,
                    wroteBundle
                        ? $"Netpack did not materialize expected bundle '{options.OutputPath}'."
                        : $"Netpack did not emit a JavaScript entry bundle. Outputs: {string.Join(", ", result.Outputs.Keys.OrderBy(static key => key, StringComparer.Ordinal))}.");
            }

            CopyStaticAssetsToOutput(options.OutputPath, frontendAssets.StaticAssets);
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

    private static FrontendAssetPreparation CopyFrontendAssets(
        ManifestModel manifest,
        BundleOptions options,
        string bundleWorkspace)
    {
        var rewrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var staticAssets = new List<PreparedStaticAsset>();
        if (manifest.Assets.Count == 0)
            return new FrontendAssetPreparation(rewrites, staticAssets);

        if (string.IsNullOrWhiteSpace(options.SourceRoot))
            throw new InvalidOperationException("Manifest frontend assets require an explicit source root.");

        foreach (var asset in manifest.Assets)
        {
            var sourcePath = GetSafePath(options.SourceRoot, asset.SourcePath);
            var artifactPath = GetSafePath(bundleWorkspace, asset.ArtifactPath);
            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            if (!string.IsNullOrWhiteSpace(artifactDirectory))
                Directory.CreateDirectory(artifactDirectory);

            File.Copy(sourcePath, artifactPath, overwrite: true);
            if (string.Equals(asset.Kind, ManifestAssetEntry.KindVueSfc, StringComparison.OrdinalIgnoreCase))
            {
                var artifactRelativePath = asset.ArtifactPath.Replace('\\', '/');
                rewrites[artifactRelativePath + ".mjs"] = artifactRelativePath;
                continue;
            }

            staticAssets.Add(new PreparedStaticAsset(sourcePath, asset.ArtifactPath.Replace('\\', '/')));
        }

        return new FrontendAssetPreparation(rewrites, staticAssets);
    }

    private static IReadOnlyDictionary<string, string> CreateImportRewrites(
        IReadOnlyList<string> moduleRelativePaths,
        IReadOnlyDictionary<string, string> assetImportRewrites)
    {
        var rewrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relativePath in moduleRelativePaths)
            rewrites[relativePath] = relativePath;

        foreach (var (source, target) in assetImportRewrites)
            rewrites[source] = target;

        return rewrites;
    }

    private static void CopyStaticAssetsToOutput(string outputPath, IReadOnlyList<PreparedStaticAsset> staticAssets)
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

    private static string RewriteModuleImports(
        string content,
        string importerRelativePath,
        IReadOnlyDictionary<string, string> importRewrites)
    {
        if (importRewrites.Count == 0)
            return content;

        string Rewrite(Match match)
        {
            var importPath = match.Groups["path"].Value;
            var rewrittenPath = RewriteImportPath(importPath, importerRelativePath, importRewrites);
            return ReferenceEquals(rewrittenPath, importPath)
                ? match.Value
                : match.Groups["prefix"].Value + rewrittenPath + match.Groups["suffix"].Value;
        }

        return ImportOnlyPattern.Replace(ImportFromPattern.Replace(content, Rewrite), Rewrite);
    }

    private static string RewriteImportPath(
        string importPath,
        string importerRelativePath,
        IReadOnlyDictionary<string, string> importRewrites)
    {
        if (!importPath.StartsWith("./", StringComparison.Ordinal) &&
            !importPath.StartsWith("../", StringComparison.Ordinal))
        {
            return importRewrites.TryGetValue(importPath, out var rewrittenBarePath)
                ? RebaseImportPath(rewrittenBarePath, importerRelativePath)
                : importPath;
        }

        var resolvedPath = ResolveImportPath(importPath, importerRelativePath);
        return importRewrites.TryGetValue(resolvedPath, out var rewrittenPath)
            ? RebaseImportPath(rewrittenPath, importerRelativePath)
            : importPath;
    }

    private static string ResolveImportPath(string importPath, string importerRelativePath)
    {
        var importerDirectory = Path.GetDirectoryName(importerRelativePath.Replace('\\', '/'))?.Replace('\\', '/') ?? string.Empty;
        var segments = new List<string>();
        foreach (var segment in SplitPathSegments(importerDirectory))
            segments.Add(segment);

        foreach (var segment in SplitPathSegments(importPath))
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (segments.Count == 0)
                    throw new InvalidOperationException("Frontend asset import path cannot escape the output directory.");

                segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(segment);
        }

        return string.Join("/", segments);
    }

    private static string RebaseImportPath(string targetRelativePath, string importerRelativePath)
    {
        var importerDirectory = Path.GetDirectoryName(importerRelativePath.Replace('\\', '/'))?.Replace('\\', '/') ?? string.Empty;
        var relativePath = string.IsNullOrEmpty(importerDirectory)
            ? targetRelativePath
            : Path.GetRelativePath(
                    importerDirectory.Replace('/', Path.DirectorySeparatorChar),
                    targetRelativePath.Replace('/', Path.DirectorySeparatorChar))
                .Replace('\\', '/');

        return relativePath.StartsWith("../", StringComparison.Ordinal) ||
               relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath
            : "./" + relativePath;
    }

    private static string[] SplitPathSegments(string path)
        => path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static bool WriteOutputs(IReadOnlyDictionary<string, byte[]> outputs, string outputPath)
    {
        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? string.Empty;
        var bundleOutput = outputs
            .Where(static item => IsJavaScriptOutput(item.Key))
            .OrderBy(static item => item.Key, StringComparer.Ordinal)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bundleOutput.Key))
            return false;

        foreach (var (name, bytes) in outputs)
        {
            var targetPath = GetSafePath(outputDirectory, name);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            File.WriteAllBytes(targetPath, bytes);
        }

        var bundleBytes = RewriteSourceMappingUrl(
            bundleOutput.Value,
            Path.GetFileName(bundleOutput.Key) + ".map",
            Path.GetFileName(outputPath) + ".map");
        File.WriteAllBytes(outputPath, bundleBytes);

        if (outputs.TryGetValue(bundleOutput.Key + ".map", out var mapBytes))
            File.WriteAllBytes(outputPath + ".map", mapBytes);

        return true;
    }

    private static bool IsJavaScriptOutput(string name)
        => (name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)) &&
           !name.EndsWith(".map", StringComparison.OrdinalIgnoreCase);

    private static byte[] RewriteSourceMappingUrl(byte[] bytes, string originalMapName, string materializedMapName)
    {
        var text = Encoding.UTF8.GetString(bytes);
        if (!text.Contains(originalMapName, StringComparison.Ordinal))
            return bytes;

        return Utf8WithoutBom.GetBytes(text.Replace(originalMapName, materializedMapName, StringComparison.Ordinal));
    }

    private static string GetRootAssemblyName(ManifestModel manifest)
        => string.IsNullOrWhiteSpace(manifest.RootAssemblyName)
            ? manifest.Modules.FirstOrDefault()?.AssemblyName ?? string.Empty
            : manifest.RootAssemblyName;

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = EnsureDirectorySeparator(Path.GetFullPath(root));
        var fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Path escapes frontend toolchain root: '{relativePath}'.");

        return fullPath;
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private sealed record FrontendAssetPreparation(
        IReadOnlyDictionary<string, string> ImportRewrites,
        IReadOnlyList<PreparedStaticAsset> StaticAssets);

    private sealed record PreparedStaticAsset(
        string SourcePath,
        string OutputRelativePath);
}
