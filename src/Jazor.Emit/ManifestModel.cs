using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Emit;

internal sealed record ManifestModel(
    string RootAssemblyPath,
    DateTime GeneratedAtUtc,
    List<ManifestModuleEntry> Modules)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static ManifestModel? TryLoad(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<ManifestModel>(json, JsonOptions);
        if (manifest is null)
            return null;

        var normalizedModules = manifest.Modules
            .Select(static module => new ManifestModuleEntry(
                module.AssemblyName,
                module.TypeName,
                module.Id,
                NormalizeRelativePath(module.RelativePath),
                module.Hash,
                module.SourceMapPath is null ? null : NormalizeRelativePath(module.SourceMapPath),
                module.MapHash))
            .ToList();

        return manifest with { Modules = normalizedModules };
    }

    public void Save(string manifestPath)
    {
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(this, JsonOptions));
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Manifest relative path cannot be empty.");

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"Manifest relative path must be relative: '{relativePath}'.");

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Manifest relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }
}

internal sealed record ManifestModuleEntry(
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Hash,
    string? SourceMapPath = null,
    string? MapHash = null);
