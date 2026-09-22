using System.Reflection;

namespace Jazor.Emit;

/// <summary>
/// Loads the requested managed assembly closure and gathers only ModuleCatalog data.
/// Package resources are collected separately from manifest locators by LibraryMaterializer.
/// </summary>
internal sealed class ModuleCollector(EmitLoadContext loadContext)
{
    private readonly EmitLoadContext _loadContext = loadContext;
    private readonly HashSet<string> _assemblyPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _rootAssemblyPaths = new(StringComparer.OrdinalIgnoreCase);

    public void AddAssembly(string assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
            return;

        var fullPath = Path.GetFullPath(assemblyPath);
        if (File.Exists(fullPath))
            _assemblyPaths.Add(fullPath);
    }

    public void AddRootAssembly(string assemblyPath)
    {
        AddAssembly(assemblyPath);
        if (!string.IsNullOrWhiteSpace(assemblyPath))
            _rootAssemblyPaths.Add(Path.GetFullPath(assemblyPath));
    }

    public CollectResult Collect(string rootAssemblyPath)
    {
        var normalizedRootAssemblyPath = Path.GetFullPath(rootAssemblyPath);
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

        var discoveredModules = new List<ModuleRecord>();
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
                return CollectResult.Fail(3, $"Failed to read ModuleCatalog from '{assembly.Location}': {ex.Message}");
            }

            if (catalog.Modules.Count == 0)
                continue;

            catalogCount++;
            discoveredModules.AddRange(catalog.Modules);
        }

        var candidatesByRelativePath = discoveredModules
            .GroupBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .OrderBy(static module => module.SourceAssemblyPath, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
                    .ThenBy(static module => module.Id, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);
        var selectedByKey = new Dictionary<string, ModuleRecord>(StringComparer.Ordinal);
        var selectedByRelativePath = new Dictionary<string, ModuleRecord>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<ModuleRecord>();

        bool TrySelect(ModuleRecord module, out string? error)
        {
            error = null;
            var key = module.AssemblyName + "::" + module.Id;
            if (selectedByKey.TryGetValue(key, out var existingByKey))
            {
                if (!HasSameContent(existingByKey, module))
                {
                    error = $"Conflicting ModuleCatalog module identity '{key}'.";
                    return false;
                }

                return true;
            }

            if (selectedByRelativePath.TryGetValue(module.RelativePath, out var existingByPath))
            {
                // The host assembly is the authored application root. A package may carry a
                // generated catalog with the same path/type; keep the host copy so its route
                // and component declarations remain authoritative.
                var existingIsRoot = SamePath(existingByPath.SourceAssemblyPath, normalizedRootAssemblyPath);
                var moduleIsRoot = SamePath(module.SourceAssemblyPath, normalizedRootAssemblyPath);
                if (string.Equals(existingByPath.TypeName, module.TypeName, StringComparison.Ordinal) &&
                    (existingIsRoot || moduleIsRoot))
                {
                    if (moduleIsRoot && !existingIsRoot)
                        selectedByRelativePath[module.RelativePath] = module;
                    selectedByKey[key] = moduleIsRoot ? module : existingByPath;
                    return true;
                }

                if (!HasSameContent(existingByPath, module))
                {
                    error = $"Path conflict for '{module.RelativePath}' between '{existingByPath.TypeName}' and '{module.TypeName}'.";
                    return false;
                }

                selectedByKey.Add(key, existingByPath);
                return true;
            }

            selectedByKey.Add(key, module);
            selectedByRelativePath.Add(module.RelativePath, module);
            queue.Enqueue(module);
            return true;
        }

        var rootAssemblyPaths = _rootAssemblyPaths;
        var rootModules = discoveredModules
            .Where(module => SamePath(module.SourceAssemblyPath, normalizedRootAssemblyPath) ||
                             rootAssemblyPaths.Contains(Path.GetFullPath(module.SourceAssemblyPath)))
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ThenBy(static module => module.Id, StringComparer.Ordinal)
            .ToArray();

        // Every assembly supplied through --assembly-list is an explicit project input. Its
        // catalog modules must remain available to the same standard JS project graph even
        // when the host assembly also has an entry module; otherwise referenced component
        // modules are mistaken for missing library imports.
        var roots = rootModules.Length > 0
            ? rootModules
            : discoveredModules.ToArray();

        var orderedRoots = roots
            .OrderByDescending(module => SamePath(module.SourceAssemblyPath, normalizedRootAssemblyPath))
            .ThenBy(static module => module.SourceAssemblyPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ThenBy(static module => module.Id, StringComparer.Ordinal)
            .ToArray();

        foreach (var rootModule in orderedRoots)
        {
            if (!TrySelect(rootModule, out var error))
                return CollectResult.Fail(4, error!);
        }

        while (queue.Count > 0)
        {
            var module = queue.Dequeue();
            // A declared source import can be supplied by a referenced ModuleCatalog as well
            // as a resource carrier. Resolve known catalog paths before resource materialization;
            // the generated import text and the declared project path stay unchanged.
            var dependencies = (module.Dependencies ?? [])
                .Concat((module.PackageImports ?? []).Where(candidatesByRelativePath.ContainsKey))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in dependencies)
            {
                if (!candidatesByRelativePath.TryGetValue(dependency, out var candidates) || candidates.Length == 0)
                {
                    return CollectResult.Fail(
                        4,
                        $"Module '{module.Id}' declares missing generated-module dependency '{dependency}'.");
                }

                var candidate = candidates[0];
                if (candidates.Skip(1).Any(other => !HasSameContent(candidate, other)))
                {
                    return CollectResult.Fail(
                        4,
                        $"Generated-module dependency '{dependency}' has conflicting owners in the selected assembly closure.");
                }

                if (!TrySelect(candidate, out var error))
                    return CollectResult.Fail(4, error!);
            }
        }

        var assetsByPath = new Dictionary<string, AssetEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in selectedByRelativePath.Values)
        {
            foreach (var asset in module.Assets ?? [])
            {
                if (assetsByPath.TryGetValue(asset.ArtifactPath, out var existingAsset))
                {
                    if (!HasSameAsset(existingAsset, asset))
                    {
                        return CollectResult.Fail(
                            4,
                            $"Conflicting ModuleCatalog asset for '{asset.ArtifactPath}'.");
                    }

                    continue;
                }

                assetsByPath.Add(asset.ArtifactPath, asset);
            }
        }

        var orderedModules = selectedByRelativePath.Values
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ThenBy(static module => module.Id, StringComparer.Ordinal)
            .ToArray();
        var orderedAssets = assetsByPath.Values
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();

        return CollectResult.Success(
            assemblies.Count,
            catalogCount,
            orderedModules,
            orderedAssets);
    }

    private static bool SamePath(string left, string right)
        => string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);

    private static bool HasSameContent(ModuleRecord left, ModuleRecord right)
        => StringComparer.Ordinal.Equals(left.Content, right.Content) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.MapHash, right.MapHash) &&
           StringComparer.Ordinal.Equals(left.SourceMapRelativePath, right.SourceMapRelativePath) &&
           StringComparer.Ordinal.Equals(left.SourceMapContent, right.SourceMapContent) &&
           StringComparer.Ordinal.Equals(left.AssemblyName, right.AssemblyName) &&
           StringComparer.Ordinal.Equals(left.TypeName, right.TypeName) &&
           StringComparer.Ordinal.Equals(left.Id, right.Id) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.RelativePath, right.RelativePath) &&
           Equals(left.Hmr, right.Hmr) &&
           (left.Dependencies ?? []).SequenceEqual(right.Dependencies ?? [], StringComparer.Ordinal) &&
           (left.PackageImports ?? []).SequenceEqual(right.PackageImports ?? [], StringComparer.Ordinal) &&
           HaveSameAssets(left.Assets, right.Assets);

    private static bool HaveSameAssets(IReadOnlyList<AssetEntry>? left, IReadOnlyList<AssetEntry>? right)
    {
        var leftAssets = (left ?? [])
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();
        var rightAssets = (right ?? [])
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();
        return leftAssets.Length == rightAssets.Length &&
               leftAssets.Zip(rightAssets, HasSameAsset).All(static value => value);
    }

    private static bool HasSameAsset(AssetEntry left, AssetEntry right)
        => StringComparer.Ordinal.Equals(left.SourcePath, right.SourcePath) &&
           StringComparer.Ordinal.Equals(left.ArtifactPath, right.ArtifactPath) &&
           StringComparer.Ordinal.Equals(left.Kind, right.Kind) &&
           StringComparer.Ordinal.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.ImportPath, right.ImportPath);
}

/// <summary>In-memory module content and its ModuleCatalog metadata before materialization.</summary>
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
    IReadOnlyList<string>? Dependencies = null);

/// <summary>Outcome of catalog collection before files are materialized.</summary>
internal sealed record CollectResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int AssemblyCount,
    int CatalogCount,
    IReadOnlyList<ModuleRecord> Modules,
    IReadOnlyList<AssetEntry> Assets)
{
    public static CollectResult Success(
        int assemblyCount,
        int catalogCount,
        IReadOnlyList<ModuleRecord> modules,
        IReadOnlyList<AssetEntry> assets)
        => new(true, 0, null, assemblyCount, catalogCount, modules, assets);

    public static CollectResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, [], []);
}
