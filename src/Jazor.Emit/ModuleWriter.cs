using System.Text;

namespace Jazor.Emit;

internal sealed class ModuleWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public WriteResult Write(
        string rootAssemblyPath,
        string outputDirectory,
        string manifestPath,
        IReadOnlyList<EmitModuleRecord> modules,
        bool clean)
    {
        Directory.CreateDirectory(outputDirectory);

        var normalizedOutputDirectory = EnsureDirectorySeparator(Path.GetFullPath(outputDirectory));
        var existingManifest = ManifestModel.TryLoad(manifestPath);
        var existingByPath = existingManifest?.Modules.ToDictionary(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, ManifestModuleEntry>(StringComparer.OrdinalIgnoreCase);

        var written = 0;
        var skipped = 0;
        var deleted = 0;

        var nextManifest = new ManifestModel(
            rootAssemblyPath,
            DateTime.UtcNow,
            []);

        foreach (var module in modules)
        {
            var targetPath = GetTargetPath(normalizedOutputDirectory, module.RelativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            if (existingByPath.TryGetValue(module.RelativePath, out var existingEntry) &&
                StringComparer.Ordinal.Equals(existingEntry.Hash, module.Hash) &&
                File.Exists(targetPath))
            {
                skipped++;
            }
            else
            {
                File.WriteAllText(targetPath, module.Content, Utf8WithoutBom);
                written++;
            }

            nextManifest.Modules.Add(new ManifestModuleEntry(
                module.AssemblyName,
                module.TypeName,
                module.Id,
                module.RelativePath,
                module.Hash));
        }

        if (clean && existingManifest is not null)
        {
            var currentPaths = nextManifest.Modules
                .Select(static module => module.RelativePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var oldModule in existingManifest.Modules)
            {
                if (currentPaths.Contains(oldModule.RelativePath))
                    continue;

                var oldTargetPath = GetTargetPath(normalizedOutputDirectory, oldModule.RelativePath);
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

internal sealed record WriteResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int Written,
    int Skipped,
    int Deleted)
{
    public static WriteResult Success(int written, int skipped, int deleted)
        => new(true, 0, null, written, skipped, deleted);

    public static WriteResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, 0);
}
