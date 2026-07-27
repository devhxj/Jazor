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
        var byRelativePath = new Dictionary<string, EmitModuleRecord>(StringComparer.OrdinalIgnoreCase);
        var assetsByArtifactPath = new Dictionary<string, ManifestAssetEntry>(StringComparer.OrdinalIgnoreCase);
        var catalogCount = 0;

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
                continue;

            catalogCount++;
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

                foreach (var asset in module.FrontendAssets ?? [])
                {
                    if (assetsByArtifactPath.TryGetValue(asset.ArtifactPath, out var existingAsset))
                    {
                        if (!StringComparer.Ordinal.Equals(existingAsset.SourcePath, asset.SourcePath) ||
                            !StringComparer.Ordinal.Equals(existingAsset.Kind, asset.Kind) ||
                            !StringComparer.Ordinal.Equals(existingAsset.Hash, asset.Hash))
                        {
                            return CollectResult.Fail(4, $"Conflicting frontend asset for '{asset.ArtifactPath}'.");
                        }

                        continue;
                    }

                    assetsByArtifactPath[asset.ArtifactPath] = asset;
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
                    }

                    continue;
                }

                byKey[key] = module;
                byRelativePath[module.RelativePath] = module;
            }
        }

        var orderedModules = byKey.Values
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToArray();
        var orderedAssets = assetsByArtifactPath.Values
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();

        return CollectResult.Success(assemblies.Count, catalogCount, orderedModules, orderedAssets);
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
    string? MapHash = null,
    IReadOnlyList<ManifestAssetEntry>? FrontendAssets = null);

internal sealed record CollectResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int AssemblyCount,
    int CatalogCount,
    IReadOnlyList<EmitModuleRecord> Modules,
    IReadOnlyList<ManifestAssetEntry> Assets)
{
    public static CollectResult Success(
        int assemblyCount,
        int catalogCount,
        IReadOnlyList<EmitModuleRecord> modules,
        IReadOnlyList<ManifestAssetEntry> assets)
        => new(true, 0, null, assemblyCount, catalogCount, modules, assets);

    public static CollectResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, [], []);
}
