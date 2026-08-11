using System.Reflection;

namespace Jazor.Emit;

/// <summary>Loads requested assemblies and gathers their module catalogs into one graph.</summary>
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

        var byKey = new Dictionary<string, ModuleRecord>(StringComparer.Ordinal);
        var byRelativePath = new Dictionary<string, ModuleRecord>(StringComparer.OrdinalIgnoreCase);
        var assetsByArtifactPath = new Dictionary<string, AssetEntry>(StringComparer.OrdinalIgnoreCase);
        var importMapEntries = new List<ImportMapEntry>();
        var catalogCount = 0;

        foreach (var assembly in assemblies)
        {
            CatalogReadResult catalog;
            try
            {
                catalog = CatalogReader.TryReadCatalogs(assembly);
            }
            catch (Exception ex)
            {
                return CollectResult.Fail(3, $"Failed to read catalog from '{assembly.Location}': {ex.Message}");
            }

            if (catalog.Modules.Count == 0 && catalog.ImportMapEntries.Count == 0)
                continue;

            catalogCount++;
            importMapEntries.AddRange(catalog.ImportMapEntries);
            foreach (var module in catalog.Modules)
            {
                var key = $"{module.AssemblyName}::{module.Id}";
                if (byKey.TryGetValue(key, out var existing))
                {
                    if (!HasSameContent(existing, module))
                        return CollectResult.Fail(4, $"Conflicting module content for '{key}'.");

                    continue;
                }

                foreach (var asset in module.Assets ?? [])
                {
                    if (assetsByArtifactPath.TryGetValue(asset.ArtifactPath, out var existingAsset))
                    {
                        if (!HasSameAsset(existingAsset, asset))
                            return CollectResult.Fail(4, $"Conflicting asset for '{asset.ArtifactPath}'.");

                        continue;
                    }

                    assetsByArtifactPath[asset.ArtifactPath] = asset;
                }

                if (byRelativePath.TryGetValue(module.RelativePath, out var existingPath))
                {
                    if (!HasSameContent(existingPath, module) && failOnPathConflict)
                    {
                        return CollectResult.Fail(
                            4,
                            $"Path conflict for '{module.RelativePath}' between '{existingPath.TypeName}' and '{module.TypeName}'.");
                    }

                    continue;
                }

                byKey[key] = module;
                byRelativePath[module.RelativePath] = module;
            }
        }

        IReadOnlyList<ModuleRecord> retainedModules;
        try
        {
            retainedModules = RetainReferencedRuntimeProviderModules(byKey.Values);
        }
        catch (Exception ex)
        {
            return CollectResult.Fail(4, ex.Message);
        }

        var orderedModules = retainedModules
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToArray();
        var orderedAssets = assetsByArtifactPath.Values
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();

        var activeProviderIds = orderedModules
            .Select(static module => module.RuntimeProviderId)
            .Where(static providerId => !string.IsNullOrWhiteSpace(providerId))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
        if (!TrySelectImportMapEntries(importMapEntries, activeProviderIds, out var orderedImportMaps, out var importMapError))
            return CollectResult.Fail(4, importMapError!);

        return CollectResult.Success(
            assemblies.Count,
            catalogCount,
            orderedModules,
            orderedAssets,
            orderedImportMaps);
    }

    /// <summary>
    /// Keeps runtime provider modules only when an application module references an entry module,
    /// then follows the provider-declared dependency paths. Emit deliberately does not infer a
    /// framework runtime graph from resource names or framework-specific import prefixes.
    /// </summary>
    internal static IReadOnlyList<ModuleRecord> RetainReferencedRuntimeProviderModules(
        IEnumerable<ModuleRecord> modules)
    {
        var allModules = modules.ToArray();
        var runtimeModules = allModules
            .Where(static module => !string.IsNullOrWhiteSpace(module.RuntimeProviderId))
            .ToArray();
        if (runtimeModules.Length == 0)
            return allModules;

        var runtimeByPath = runtimeModules.ToDictionary(
            static module => module.RelativePath,
            StringComparer.OrdinalIgnoreCase);
        var selectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<ModuleRecord>();
        foreach (var runtime in runtimeModules)
        {
            if (!allModules.Any(module =>
                    string.IsNullOrWhiteSpace(module.RuntimeProviderId) &&
                    HasQuotedImport(module.Content, runtime.RelativePath)))
            {
                continue;
            }

            selectedPaths.Add(runtime.RelativePath);
            pending.Enqueue(runtime);
        }

        while (pending.TryDequeue(out var importer))
        {
            foreach (var dependencyPath in importer.RuntimeDependencies ?? [])
            {
                if (!runtimeByPath.TryGetValue(dependencyPath, out var dependency))
                {
                    throw new InvalidOperationException(
                        $"Runtime provider '{importer.RuntimeProviderId}' declares missing module dependency '{dependencyPath}'.");
                }

                if (!selectedPaths.Add(dependency.RelativePath))
                    continue;

                pending.Enqueue(dependency);
            }
        }

        return allModules
            .Where(module => string.IsNullOrWhiteSpace(module.RuntimeProviderId) || selectedPaths.Contains(module.RelativePath))
            .ToArray();
    }

    private static bool TrySelectImportMapEntries(
        IEnumerable<ImportMapEntry> entries,
        IReadOnlySet<string> activeProviderIds,
        out IReadOnlyList<ImportMapEntry> selected,
        out string? error)
    {
        var bySpecifier = new Dictionary<string, ImportMapEntry>(StringComparer.Ordinal);
        foreach (var entry in entries
                     .Where(entry => activeProviderIds.Contains(entry.ProviderId))
                     .OrderBy(static entry => entry.Specifier, StringComparer.Ordinal)
                     .ThenBy(static entry => entry.ProviderId, StringComparer.Ordinal)
                     .ThenBy(static entry => entry.ArtifactPath, StringComparer.Ordinal))
        {
            if (bySpecifier.TryGetValue(entry.Specifier, out var existing))
            {
                if (!StringComparer.Ordinal.Equals(existing.ArtifactPath, entry.ArtifactPath))
                {
                    selected = [];
                    error = $"Conflicting import-map contribution for '{entry.Specifier}' from providers '{existing.ProviderId}' and '{entry.ProviderId}'.";
                    return false;
                }

                continue;
            }

            bySpecifier.Add(entry.Specifier, entry);
        }

        selected = bySpecifier.Values
            .OrderBy(static entry => entry.Specifier, StringComparer.Ordinal)
            .ThenBy(static entry => entry.ProviderId, StringComparer.Ordinal)
            .ToArray();
        error = null;
        return true;
    }

    private static bool HasSameContent(ModuleRecord left, ModuleRecord right)
        => StringComparer.Ordinal.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.MapHash, right.MapHash) &&
           StringComparer.Ordinal.Equals(left.SourceMapRelativePath, right.SourceMapRelativePath) &&
           Equals(left.Hmr, right.Hmr) &&
           StringComparer.Ordinal.Equals(left.RuntimeProviderId, right.RuntimeProviderId) &&
           (left.RuntimeDependencies ?? []).SequenceEqual(right.RuntimeDependencies ?? [], StringComparer.Ordinal);

    private static bool HasSameAsset(AssetEntry left, AssetEntry right)
        => StringComparer.Ordinal.Equals(left.SourcePath, right.SourcePath) &&
           StringComparer.Ordinal.Equals(left.Kind, right.Kind) &&
           StringComparer.Ordinal.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.ImportPath, right.ImportPath);

    private static bool HasQuotedImport(string content, string specifier)
        => content.Contains("\"" + specifier + "\"", StringComparison.Ordinal) ||
           content.Contains("'" + specifier + "'", StringComparison.Ordinal);
}

/// <summary>In-memory module content and its manifest metadata before it is written.</summary>
internal sealed record ModuleRecord(
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
    IReadOnlyList<AssetEntry>? Assets = null,
    IReadOnlyList<string>? PackageImports = null,
    HmrMetadata? Hmr = null,
    string? RuntimeProviderId = null,
    IReadOnlyList<string>? RuntimeDependencies = null);

/// <summary>Outcome of catalog collection before files are materialized.</summary>
internal sealed record CollectResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int AssemblyCount,
    int CatalogCount,
    IReadOnlyList<ModuleRecord> Modules,
    IReadOnlyList<AssetEntry> Assets,
    IReadOnlyList<ImportMapEntry> ImportMapEntries)
{
    public static CollectResult Success(
        int assemblyCount,
        int catalogCount,
        IReadOnlyList<ModuleRecord> modules,
        IReadOnlyList<AssetEntry> assets,
        IReadOnlyList<ImportMapEntry> importMapEntries)
        => new(true, 0, null, assemblyCount, catalogCount, modules, assets, importMapEntries);

    public static CollectResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, [], [], []);
}
