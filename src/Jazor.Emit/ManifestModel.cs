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
        return JsonSerializer.Deserialize<ManifestModel>(json, JsonOptions);
    }

    public void Save(string manifestPath)
    {
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(this, JsonOptions));
    }
}

internal sealed record ManifestModuleEntry(
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Hash);
