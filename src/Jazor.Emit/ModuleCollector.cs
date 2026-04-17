using System.Reflection;

namespace Jazor.Emit;

internal sealed class ModuleCollector(EmitLoadContext loadContext)
{
    private readonly EmitLoadContext _loadContext = loadContext;
    private readonly HashSet<string> _assemblyPaths = new(StringComparer.OrdinalIgnoreCase);

    public void AddAssembly(string assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
            return;

        var fullPath = Path.GetFullPath(assemblyPath);
        if (File.Exists(fullPath))
            _assemblyPaths.Add(fullPath);
    }

    public CollectResult Collect(bool failOnPathConflict)
    {
        var assemblies = new List<Assembly>();
        foreach (var assemblyPath in _assemblyPaths.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                assemblies.Add(_loadContext.LoadFromAssemblyPath(assemblyPath));
            }
            catch (Exception ex)
            {
                return CollectResult.Fail(2, $"Failed to load '{assemblyPath}': {ex.Message}");
            }
        }

        var byKey = new Dictionary<string, EmitModuleRecord>(StringComparer.Ordinal);
        var razorVueByKey = new Dictionary<string, RazorVueEmitArtifactRecord>(StringComparer.Ordinal);
        var byRelativePath = new Dictionary<string, EmitModuleRecord>(StringComparer.OrdinalIgnoreCase);
        var razorVueByRelativePath = new Dictionary<string, RazorVueEmitArtifactRecord>(StringComparer.OrdinalIgnoreCase);
        var catalogCount = 0;
        var razorVueCatalogCount = 0;
        var razorVueCatalogs = new List<RazorVueCatalogRecord>();

        foreach (var assembly in assemblies)
        {
            IReadOnlyList<EmitModuleRecord>? modules;
            try
            {
                modules = CatalogReader.TryRead(assembly);
            }
            catch (Exception ex)
            {
                return CollectResult.Fail(3, $"Failed to read catalog from '{assembly.Location}': {ex.Message}");
            }

            if (modules is null)
            {
                modules = [];
            }
            else
            {
                catalogCount++;
            }

            foreach (var module in modules)
            {
                var key = $"{module.AssemblyName}::{module.Id}";
                if (byKey.TryGetValue(key, out var existing))
                {
                    if (!StringComparer.Ordinal.Equals(existing.Hash, module.Hash) ||
                        !StringComparer.Ordinal.Equals(existing.MapHash, module.MapHash) ||
                        !StringComparer.Ordinal.Equals(existing.SourceMapRelativePath, module.SourceMapRelativePath))
                    {
                        return CollectResult.Fail(4, $"Conflicting module content for '{key}'.");
                    }

                    continue;
                }

                if (byRelativePath.TryGetValue(module.RelativePath, out var existingPath))
                {
                    if (!StringComparer.Ordinal.Equals(existingPath.Hash, module.Hash) ||
                        !StringComparer.Ordinal.Equals(existingPath.MapHash, module.MapHash) ||
                        !StringComparer.Ordinal.Equals(existingPath.SourceMapRelativePath, module.SourceMapRelativePath))
                    {
                        if (failOnPathConflict)
                        {
                            return CollectResult.Fail(
                                4,
                                $"Path conflict for '{module.RelativePath}' between '{existingPath.TypeName}' and '{module.TypeName}'.");
                        }

                        continue;
                    }

                    continue;
                }

                byKey[key] = module;
                byRelativePath[module.RelativePath] = module;
            }

            RazorVueCatalogRecord? razorVueCatalog;
            try
            {
                razorVueCatalog = RazorVueCatalogReader.TryRead(assembly);
            }
            catch (Exception ex)
            {
                return CollectResult.Fail(3, $"Failed to read RazorVue catalog from '{assembly.Location}': {ex.Message}");
            }

            if (razorVueCatalog is null)
                continue;

            razorVueCatalogCount++;
            var acceptedArtifacts = new List<RazorVueEmitArtifactRecord>();

            foreach (var artifact in razorVueCatalog.Artifacts)
            {
                var key = $"{razorVueCatalog.AssemblyName}::{artifact.Identity.ComponentId}";
                if (razorVueByKey.TryGetValue(key, out var existing))
                {
                    if (!StringComparer.Ordinal.Equals(existing.Identity.DescriptorHash, artifact.Identity.DescriptorHash) ||
                        !StringComparer.Ordinal.Equals(existing.Identity.TemplateHash, artifact.Identity.TemplateHash) ||
                        !StringComparer.Ordinal.Equals(existing.Identity.LogicHash, artifact.Identity.LogicHash) ||
                        !StringComparer.Ordinal.Equals(existing.ModuleCode, artifact.ModuleCode))
                    {
                        return CollectResult.Fail(4, $"Conflicting RazorVue artifact content for '{key}'.");
                    }

                    continue;
                }

                if (byRelativePath.ContainsKey(artifact.RelativeModulePath))
                {
                    if (failOnPathConflict)
                    {
                        return CollectResult.Fail(
                            4,
                            $"Path conflict for '{artifact.RelativeModulePath}' between a static module and RazorVue artifact '{artifact.ComponentName}'.");
                    }

                    continue;
                }

                if (razorVueByRelativePath.TryGetValue(artifact.RelativeModulePath, out var existingArtifact))
                {
                    if (!StringComparer.Ordinal.Equals(existingArtifact.Identity.DescriptorHash, artifact.Identity.DescriptorHash) ||
                        !StringComparer.Ordinal.Equals(existingArtifact.Identity.TemplateHash, artifact.Identity.TemplateHash) ||
                        !StringComparer.Ordinal.Equals(existingArtifact.Identity.LogicHash, artifact.Identity.LogicHash) ||
                        !StringComparer.Ordinal.Equals(existingArtifact.ModuleCode, artifact.ModuleCode))
                    {
                        if (failOnPathConflict)
                        {
                            return CollectResult.Fail(
                                4,
                                $"Path conflict for '{artifact.RelativeModulePath}' between RazorVue artifacts '{existingArtifact.ComponentName}' and '{artifact.ComponentName}'.");
                        }

                        continue;
                    }

                    continue;
                }

                razorVueByKey[key] = artifact;
                razorVueByRelativePath[artifact.RelativeModulePath] = artifact;
                acceptedArtifacts.Add(artifact);
            }

            razorVueCatalogs.Add(new RazorVueCatalogRecord(
                razorVueCatalog.AssemblyName,
                acceptedArtifacts
                    .OrderBy(static artifact => artifact.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
                    .ToArray()));
        }

        var orderedModules = byKey.Values
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToArray();
        var orderedCatalogs = razorVueCatalogs
            .OrderBy(static catalog => catalog.AssemblyName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return CollectResult.Success(assemblies.Count, catalogCount, razorVueCatalogCount, orderedModules, orderedCatalogs);
    }
}

internal sealed record EmitModuleRecord(
    string SourceAssemblyPath,
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Content,
    string Hash,
    string? SourceMapRelativePath = null,
    string? SourceMapContent = null,
    string? MapHash = null);

internal sealed record CollectResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int AssemblyCount,
    int CatalogCount,
    int RazorVueCatalogCount,
    IReadOnlyList<EmitModuleRecord> Modules,
    IReadOnlyList<RazorVueCatalogRecord> RazorVueCatalogs)
{
    public int RazorVueArtifactCount
        => RazorVueCatalogs.Sum(static catalog => catalog.Artifacts.Count);

    public IReadOnlyList<RazorVueEmitArtifactRecord> RazorVueArtifacts
        => RazorVueCatalogs.SelectMany(static catalog => catalog.Artifacts).ToArray();

    public static CollectResult Success(
        int assemblyCount,
        int catalogCount,
        int razorVueCatalogCount,
        IReadOnlyList<EmitModuleRecord> modules,
        IReadOnlyList<RazorVueCatalogRecord> razorVueCatalogs)
        => new(true, 0, null, assemblyCount, catalogCount, razorVueCatalogCount, modules, razorVueCatalogs);

    public static CollectResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, 0, [], []);
}
