using System.Text;
using System.Security.Cryptography;

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
        var currentModulePaths = modules
            .Select(static module => module.RelativePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var nextManifest = new ManifestModel(
            rootAssemblyPath,
            DateTime.UtcNow,
            existingManifest?.Modules
                .Where(static module => module.Component is not null)
                .Where(module => !currentModulePaths.Contains(module.RelativePath))
                .ToList() ?? []);

        foreach (var module in modules)
        {
            var targetPath = GetTargetPath(normalizedOutputDirectory, module.RelativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var hasSourceMap = !string.IsNullOrWhiteSpace(module.SourceMapRelativePath) &&
                               !string.IsNullOrWhiteSpace(module.SourceMapContent);
            var sourceMapRelativePath = hasSourceMap ? module.SourceMapRelativePath : null;
            var sourceMapPath = hasSourceMap
                ? GetTargetPath(normalizedOutputDirectory, sourceMapRelativePath!)
                : null;
            var mapHash = hasSourceMap
                ? module.MapHash ?? ComputeSha256Hex(module.SourceMapContent!)
                : null;
            var moduleContent = hasSourceMap
                ? AppendSourceMappingUrl(module.Content, Path.GetFileName(sourceMapPath!))
                : module.Content;
            if (hasSourceMap)
            {
                var sourceMapDirectory = Path.GetDirectoryName(sourceMapPath);
                if (!string.IsNullOrEmpty(sourceMapDirectory))
                    Directory.CreateDirectory(sourceMapDirectory);
            }

            if (existingByPath.TryGetValue(module.RelativePath, out var existingEntry) &&
                StringComparer.Ordinal.Equals(existingEntry.Hash, module.Hash) &&
                StringComparer.Ordinal.Equals(existingEntry.SourceMapPath, sourceMapRelativePath) &&
                StringComparer.Ordinal.Equals(existingEntry.MapHash, mapHash) &&
                File.Exists(targetPath) &&
                (!hasSourceMap || File.Exists(sourceMapPath!)))
            {
                skipped++;
            }
            else
            {
                File.WriteAllText(targetPath, moduleContent, Utf8WithoutBom);
                if (hasSourceMap)
                    File.WriteAllText(sourceMapPath!, module.SourceMapContent!, Utf8WithoutBom);

                if (existingEntry?.SourceMapPath is { Length: > 0 } oldSourceMapPath &&
                    !StringComparer.Ordinal.Equals(oldSourceMapPath, sourceMapRelativePath))
                {
                    DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldSourceMapPath), ref deleted);
                }

                written++;
            }

            if (existingEntry?.Component is not null)
            {
                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, existingEntry.Component.OriginMapPath), ref deleted);
            }

            nextManifest.Modules.Add(new ManifestModuleEntry(
                module.AssemblyName,
                module.TypeName,
                module.Id,
                module.RelativePath,
                module.Hash,
                sourceMapRelativePath,
                mapHash));
        }

        if (clean && existingManifest is not null)
        {
            var currentPaths = nextManifest.Modules
                .Select(static module => module.RelativePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var oldModule in existingManifest.Modules)
            {
                if (oldModule.Component is not null)
                    continue;

                if (currentPaths.Contains(oldModule.RelativePath))
                    continue;

                var oldTargetPath = GetTargetPath(normalizedOutputDirectory, oldModule.RelativePath);
                if (File.Exists(oldTargetPath))
                {
                    File.Delete(oldTargetPath);
                    deleted++;
                }

                if (oldModule.SourceMapPath is { Length: > 0 })
                    DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.SourceMapPath), ref deleted);
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

    private static void DeleteIfExists(string path, ref int deleted)
    {
        if (!File.Exists(path))
            return;

        File.Delete(path);
        deleted++;
    }

    private static string AppendSourceMappingUrl(string content, string mapFileName)
    {
        var normalized = (content ?? string.Empty).TrimEnd('\r', '\n');
        if (normalized.Length == 0)
            return $"//# sourceMappingURL={mapFileName}\n";

        return normalized + "\n" + $"//# sourceMappingURL={mapFileName}\n";
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var item in bytes)
            builder.Append(item.ToString("X2"));

        return builder.ToString();
    }
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
