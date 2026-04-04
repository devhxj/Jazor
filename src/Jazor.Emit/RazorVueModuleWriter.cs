using System.Text;

namespace Jazor.Emit;

internal sealed class RazorVueModuleWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public WriteResult Write(
        string rootAssemblyPath,
        string outputDirectory,
        string manifestPath,
        IReadOnlyList<RazorVueCatalogRecord> catalogs,
        bool clean)
    {
        Directory.CreateDirectory(outputDirectory);

        var normalizedOutputDirectory = EnsureDirectorySeparator(Path.GetFullPath(outputDirectory));
        var existingManifest = RazorVueManifestModel.TryLoad(manifestPath);
        var existingByPath = existingManifest?.Modules.ToDictionary(static module => module.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, RazorVueManifestEntry>(StringComparer.OrdinalIgnoreCase);
        var artifacts = catalogs
            .SelectMany(static catalog => catalog.Artifacts)
            .OrderBy(static artifact => artifact.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
            .ToArray();

        var written = 0;
        var skipped = 0;
        var deleted = 0;

        // The aggregate manifest may merge multiple RazorVue catalogs, so each
        // entry keeps its originating assembly name instead of inferring from component ids.
        var nextManifest = RazorVueManifestModel.Create(rootAssemblyPath, catalogs);

        foreach (var artifact in artifacts)
        {
            var targetPath = GetTargetPath(normalizedOutputDirectory, artifact.RelativeModulePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var contentHash = ComputeSha256Hex(artifact.ModuleCode);
            if (existingByPath.TryGetValue(artifact.RelativeModulePath, out var existingEntry) &&
                StringComparer.Ordinal.Equals(existingEntry.ContentHash, contentHash) &&
                File.Exists(targetPath))
            {
                skipped++;
            }
            else
            {
                File.WriteAllText(targetPath, artifact.ModuleCode, Utf8WithoutBom);
                written++;
            }
        }

        if (clean && existingManifest is not null)
        {
            var currentPaths = nextManifest.Modules
                .Select(static module => module.RelativeModulePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var oldModule in existingManifest.Modules)
            {
                if (currentPaths.Contains(oldModule.RelativeModulePath))
                    continue;

                var oldTargetPath = GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath);
                if (File.Exists(oldTargetPath))
                {
                    File.Delete(oldTargetPath);
                    deleted++;
                }
            }
        }

        nextManifest.Save(manifestPath);
        return WriteResult.Success(written, skipped, deleted);
    }

    public static string GetManifestPath(string baseManifestPath)
    {
        var directory = Path.GetDirectoryName(baseManifestPath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(baseManifestPath);
        var extension = Path.GetExtension(baseManifestPath);
        return Path.Combine(directory, fileName + "-razorvue" + extension);
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
        return string.Concat(bytes.Select(static item => item.ToString("X2")));
    }

    private static string GetTargetPath(string normalizedOutputDirectory, string relativePath)
    {
        var relativePlatformPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(normalizedOutputDirectory, relativePlatformPath));
        if (!fullPath.StartsWith(normalizedOutputDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to write outside output directory: '{relativePath}'.");

        return fullPath;
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
}
