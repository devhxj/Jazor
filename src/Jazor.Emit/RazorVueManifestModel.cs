using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Emit;

internal sealed record RazorVueManifestModel(
    string AssemblyName,
    DateTime GeneratedAtUtc,
    List<RazorVueManifestEntry> Modules,
    List<string>? Styles = null,
    List<string>? PluginRequirements = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static RazorVueManifestModel Create(RazorVueCatalogRecord catalog)
    {
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));

        return Create(catalog.AssemblyName, [catalog]);
    }

    public static RazorVueManifestModel Create(string rootAssemblyPath, IReadOnlyList<RazorVueCatalogRecord> catalogs)
    {
        if (string.IsNullOrWhiteSpace(rootAssemblyPath))
            throw new ArgumentException("Root assembly path is required.", nameof(rootAssemblyPath));

        if (catalogs is null)
            throw new ArgumentNullException(nameof(catalogs));

        return new RazorVueManifestModel(
            ResolveManifestAssemblyName(rootAssemblyPath, catalogs),
            DateTime.UtcNow,
            catalogs
                .SelectMany(static catalog => catalog.Artifacts.Select(artifact => CreateEntry(catalog.AssemblyName, artifact)))
                .OrderBy(static artifact => artifact.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
                .ToList(),
            AggregateHostRequirementList(catalogs, static artifact => artifact.Styles),
            AggregateHostRequirementList(catalogs, static artifact => artifact.PluginRequirements));
    }

    public static RazorVueManifestModel? TryLoad(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<RazorVueManifestModel>(json, JsonOptions);
        if (manifest is null)
            return null;

        return manifest with
        {
            Styles = NormalizeHostRequirementList(
                manifest.Styles is not null
                    ? manifest.Styles
                    : manifest.Modules.SelectMany(static module => module.Styles).ToList()),
            PluginRequirements = NormalizeHostRequirementList(
                manifest.PluginRequirements is not null
                    ? manifest.PluginRequirements
                    : manifest.Modules.SelectMany(static module => module.PluginRequirements).ToList())
        };
    }

    public void Save(string manifestPath)
    {
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(this, JsonOptions));
    }

    private static RazorVueManifestEntry CreateEntry(string assemblyName, RazorVueEmitArtifactRecord artifact)
    {
        return new RazorVueManifestEntry(
            assemblyName,
            artifact.ComponentName,
            artifact.RelativeModulePath,
            artifact.Imports.ToList(),
            NormalizeHostRequirementList(artifact.Styles),
            NormalizeHostRequirementList(artifact.PluginRequirements),
            artifact.Identity.DescriptorHash,
            artifact.Identity.TemplateHash,
            artifact.Identity.LogicHash,
            ComputeSha256Hex(artifact.ModuleCode),
            artifact.Identity.HmrBoundaryKind,
            artifact.Hints.RequiresHydration,
            artifact.Hints.SupportsSsr);
    }

    private static string ResolveManifestAssemblyName(string rootAssemblyPath, IReadOnlyList<RazorVueCatalogRecord> catalogs)
    {
        if (catalogs.Count == 1)
            return catalogs[0].AssemblyName;

        var fileName = Path.GetFileNameWithoutExtension(rootAssemblyPath);
        return string.IsNullOrWhiteSpace(fileName) ? "Jazor.Emit" : fileName;
    }

    private static List<string> NormalizeHostRequirementList(IReadOnlyList<string> values)
    {
        // Keep host-facing dependency metadata deterministic so manifest diffs only
        // reflect real contract changes rather than descriptor discovery order.
        return values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> AggregateHostRequirementList(
        IReadOnlyList<RazorVueCatalogRecord> catalogs,
        Func<RazorVueEmitArtifactRecord, IReadOnlyList<string>> selector)
        => NormalizeHostRequirementList(catalogs.SelectMany(static catalog => catalog.Artifacts).SelectMany(selector).ToArray());

    private static string ComputeSha256Hex(string content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty));
        return string.Concat(bytes.Select(static item => item.ToString("X2")));
    }
}

internal sealed record RazorVueManifestEntry(
    string AssemblyName,
    string ComponentName,
    string RelativeModulePath,
    List<string> Imports,
    List<string> Styles,
    List<string> PluginRequirements,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string ContentHash,
    RazorVueHmrBoundaryKind HmrBoundaryKind,
    bool RequiresHydration,
    bool SupportsSsr);
