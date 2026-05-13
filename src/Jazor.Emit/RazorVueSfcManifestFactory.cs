namespace Jazor.Emit;

internal static class RazorVueSfcManifestFactory
{
    public static RazorVueManifestModel Create(RazorVueSfcCatalogRecord catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        return Create(catalog.AssemblyName, [catalog]);
    }

    public static RazorVueManifestModel Create(string rootAssemblyPath, IReadOnlyList<RazorVueSfcCatalogRecord> catalogs)
    {
        if (string.IsNullOrWhiteSpace(rootAssemblyPath))
            throw new ArgumentException("Root assembly path is required.", nameof(rootAssemblyPath));

        ArgumentNullException.ThrowIfNull(catalogs);

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

    private static RazorVueManifestEntry CreateEntry(string assemblyName, RazorVueEmitSfcArtifactRecord artifact)
    {
        return new RazorVueManifestEntry(
            assemblyName,
            artifact.Identity.ComponentId,
            artifact.Identity.ModuleId,
            artifact.ComponentName,
            artifact.RelativeSfcPath,
            artifact.RelativeSfcPath + ".map",
            artifact.RelativeSfcPath + ".origins.json",
            artifact.Imports.ToList(),
            RazorVueManifestFactory.NormalizeHostRequirementList(artifact.Styles),
            RazorVueManifestFactory.NormalizeHostRequirementList(artifact.PluginRequirements),
            artifact.Identity.DescriptorHash,
            artifact.Identity.TemplateHash,
            artifact.Identity.LogicHash,
            ComputeSha256Hex(artifact.SfcText),
            artifact.Identity.HmrBoundaryKind,
            artifact.Hints.RequiresHydration,
            artifact.Hints.SupportsSsr,
            artifact.Identity.StyleHash,
            ManifestComponentModel.Sfc);
    }

    private static string ResolveManifestAssemblyName(string rootAssemblyPath, IReadOnlyList<RazorVueSfcCatalogRecord> catalogs)
    {
        if (catalogs.Count == 1)
            return catalogs[0].AssemblyName;

        var fileName = Path.GetFileNameWithoutExtension(rootAssemblyPath);
        return string.IsNullOrWhiteSpace(fileName) ? "Jazor.Emit" : fileName;
    }

    private static List<string> AggregateHostRequirementList(
        IReadOnlyList<RazorVueSfcCatalogRecord> catalogs,
        Func<RazorVueEmitSfcArtifactRecord, IReadOnlyList<string>> selector)
        => RazorVueManifestFactory.NormalizeHostRequirementList(catalogs.SelectMany(static catalog => catalog.Artifacts).SelectMany(selector).ToArray());

    private static string ComputeSha256Hex(string content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty));
        return string.Concat(bytes.Select(static item => item.ToString("X2")));
    }
}
