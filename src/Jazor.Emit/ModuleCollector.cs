using System.Reflection;

namespace Jazor.Emit;

internal sealed class ModuleCollector(EmitLoadContext loadContext)
{
    private const string RazorVueRuntimeRelativePathPrefix = "@jazor/vue-runtime/";

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

        var orderedModules = RetainReferencedRazorVueRuntimeModules(byKey.Values)
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToArray();
        var orderedAssets = assetsByArtifactPath.Values
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();

        return CollectResult.Success(assemblies.Count, catalogCount, orderedModules, orderedAssets);
    }

    internal static IReadOnlyList<EmitModuleRecord> RetainReferencedRazorVueRuntimeModules(
        IEnumerable<EmitModuleRecord> modules)
    {
        var allModules = modules.ToArray();
        var runtimeModules = allModules
            .Where(IsRazorVueRuntimeModule)
            .ToArray();
        if (runtimeModules.Length == 0)
            return allModules;

        // RazorVue ships these files through its analyzer assembly, not as normal runtime
        // references. Materialize only the static ESM dependency closure so direct render
        // remains free of its unused legacy bridge. RazorVue runtime 随 analyzer 提供，必须按
        // 静态 ESM 依赖闭包物化，避免 direct render 输出未使用的 render-context bridge。
        var selectedPaths = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<EmitModuleRecord>();
        foreach (var runtime in runtimeModules)
        {
            if (allModules.Any(module =>
                    !IsRazorVueRuntimeModule(module) &&
                    HasQuotedImport(module.Content, runtime.RelativePath)))
            {
                selectedPaths.Add(runtime.RelativePath);
                pending.Enqueue(runtime);
            }
        }

        while (pending.TryDequeue(out var importer))
        {
            foreach (var runtime in runtimeModules)
            {
                if (selectedPaths.Contains(runtime.RelativePath) ||
                    !HasQuotedImport(
                        importer.Content,
                        GetRelativeImportSpecifier(importer.RelativePath, runtime.RelativePath)))
                {
                    continue;
                }

                selectedPaths.Add(runtime.RelativePath);
                pending.Enqueue(runtime);
            }
        }

        return allModules
            .Where(module => !IsRazorVueRuntimeModule(module) || selectedPaths.Contains(module.RelativePath))
            .ToArray();
    }

    private static bool IsRazorVueRuntimeModule(EmitModuleRecord module)
        => module.RelativePath.StartsWith(RazorVueRuntimeRelativePathPrefix, StringComparison.Ordinal);

    private static bool HasQuotedImport(string content, string specifier)
        => content.Contains("\"" + specifier + "\"", StringComparison.Ordinal) ||
           content.Contains("'" + specifier + "'", StringComparison.Ordinal);

    private static string GetRelativeImportSpecifier(string importerPath, string importedPath)
    {
        var importerDirectory = Path.GetDirectoryName(importerPath.Replace('/', Path.DirectorySeparatorChar))
            ?? string.Empty;
        var relativePath = Path.GetRelativePath(
                importerDirectory,
                importedPath.Replace('/', Path.DirectorySeparatorChar))
            .Replace(Path.DirectorySeparatorChar, '/');
        return relativePath.StartsWith(".", StringComparison.Ordinal)
            ? relativePath
            : "./" + relativePath;
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
