using System.Collections;
using System.Reflection;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Reads the one generated carrier used by pure Jazor libraries.
/// JS resource libraries are discovered through their package manifest and never through an
/// assembly type. Keeping this reader narrow prevents producer-specific catalogs from becoming
/// accidental public contracts.
/// </summary>
internal static class CatalogReader
{
    private const string ModuleCatalogTypeName = "Jazor.Generated.ModuleCatalog";
    private const int ContractSchemaVersion = 2;

    public static IReadOnlyList<ModuleRecord>? TryRead(Assembly assembly)
    {
        var result = TryReadCatalogs(assembly);
        return result.Modules.Count == 0 ? null : result.Modules;
    }

    public static CatalogReadResult TryReadCatalogs(Assembly assembly)
    {
        var catalogType = assembly.GetType(ModuleCatalogTypeName, throwOnError: false, ignoreCase: false);
        if (catalogType is null)
            return new CatalogReadResult([], []);

        ValidateSchema(catalogType, assembly);
        var assemblyName = TryReadStaticString(catalogType, "AssemblyName") ??
                           assembly.GetName().Name ??
                           Path.GetFileNameWithoutExtension(assembly.Location);
        var assets = ReadCatalogAssets(assembly, catalogType);
        var modules = ReadModules(assembly, catalogType, assemblyName);
        var boundModules = BindAssets(assembly, modules, assets);
        return new CatalogReadResult(
            boundModules,
            assets
                .Select(static asset => asset.Asset)
                .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
                .ToArray());
    }

    private static IReadOnlyList<ModuleRecord> ReadModules(
        Assembly assembly,
        Type catalogType,
        string catalogAssemblyName)
    {
        var getModules = catalogType.GetMethod(
            "GetModules",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"GetModules was not found on '{ModuleCatalogTypeName}' in '{assembly.Location}'.");

        if (getModules.Invoke(null, null) is not IEnumerable items)
            throw new InvalidOperationException(
                $"GetModules returned null on '{ModuleCatalogTypeName}' in '{assembly.Location}'.");

        var modules = new List<ModuleRecord>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var id = ReadRequiredString(itemType, item, "Id");
            var relativePath = NormalizeRelativePath(ReadRequiredString(itemType, item, "RelativePath"));
            if (!ids.Add(id))
                throw new InvalidOperationException(
                    $"ModuleCatalog '{assembly.Location}' declares duplicate module id '{id}'.");
            if (!paths.Add(relativePath))
                throw new InvalidOperationException(
                    $"ModuleCatalog '{assembly.Location}' declares duplicate module path '{relativePath}'.");

            var content = ReadRequiredString(itemType, item, "Content").ReplaceLineEndings("\n");
            var hash = ReadRequiredString(itemType, item, "Hash");
            if (!HashMatchesContent(hash, content))
                throw new InvalidOperationException(
                    $"ModuleCatalog module '{id}' in '{assembly.Location}' declares a hash that does not match its content.");

            var sourceMapPath = TryReadString(itemType, item, "SourceMapRelativePath");
            var sourceMapContent = TryReadString(itemType, item, "SourceMapContent");
            var mapHash = TryReadString(itemType, item, "MapHash");
            var hasSourceMap = !string.IsNullOrWhiteSpace(sourceMapPath) ||
                               !string.IsNullOrWhiteSpace(sourceMapContent) ||
                               !string.IsNullOrWhiteSpace(mapHash);
            if (hasSourceMap &&
                (string.IsNullOrWhiteSpace(sourceMapPath) ||
                 string.IsNullOrWhiteSpace(sourceMapContent) ||
                 string.IsNullOrWhiteSpace(mapHash)))
            {
                throw new InvalidOperationException(
                    $"ModuleCatalog module '{id}' must provide SourceMapRelativePath, SourceMapContent and MapHash together.");
            }

            if (hasSourceMap && !HashMatchesContent(mapHash!, sourceMapContent!))
                throw new InvalidOperationException(
                    $"ModuleCatalog source map for '{id}' declares a hash that does not match its content.");

            modules.Add(new ModuleRecord(
                assembly.Location,
                TryReadString(itemType, item, "AssemblyName") ?? catalogAssemblyName,
                TryReadString(itemType, item, "TypeName") ?? id,
                id,
                relativePath,
                content,
                ArtifactHash.RequireSha256(hash, $"ModuleCatalog module '{id}' hash"),
                hasSourceMap ? NormalizeRelativePath(sourceMapPath!) : null,
                hasSourceMap ? sourceMapContent!.ReplaceLineEndings("\n") : null,
                hasSourceMap ? ArtifactHash.RequireSha256(mapHash!, $"ModuleCatalog source map '{id}' hash") : null,
                PackageImports: ReadStrings(itemType, item, "PackageImports"),
                Hmr: ReadHmrMetadata(itemType, item, id),
                Dependencies: ReadStrings(itemType, item, "Dependencies", normalizePaths: true)));
        }

        return modules;
    }

    private static IReadOnlyList<CatalogAssetRecord> ReadCatalogAssets(Assembly assembly, Type catalogType)
    {
        var getAssets = catalogType.GetMethod(
            "GetAssets",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getAssets is null)
            return [];

        if (getAssets.Invoke(null, null) is not IEnumerable items)
            throw new InvalidOperationException(
                $"GetAssets returned null on '{ModuleCatalogTypeName}' in '{assembly.Location}'.");

        var assets = new Dictionary<string, CatalogAssetRecord>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var ownerModulePath = NormalizeRelativePath(ReadRequiredString(itemType, item, "OwnerModulePath"));
            var sourcePath = NormalizeRelativePath(ReadRequiredString(itemType, item, "SourcePath"));
            var artifactPath = NormalizeRelativePath(ReadRequiredString(itemType, item, "ArtifactPath"));
            var kind = TryReadString(itemType, item, "Kind") ?? AssetEntry.KindStatic;
            if (kind is not (AssetEntry.KindStatic or AssetEntry.KindModuleSource))
                throw new InvalidOperationException(
                    $"ModuleCatalog asset '{artifactPath}' has unsupported type '{kind}'.");

            var importPath = TryReadString(itemType, item, "ImportPath");
            if (kind == AssetEntry.KindModuleSource && string.IsNullOrWhiteSpace(importPath))
                throw new InvalidOperationException(
                    $"ModuleCatalog module-source asset '{artifactPath}' must declare ImportPath.");

            var contentHash = TryReadString(itemType, item, "ContentHash") ?? TryReadString(itemType, item, "Hash");
            if (!string.IsNullOrWhiteSpace(contentHash))
            {
                // Keep the canonical (lowercase) value in the in-memory record so every
                // downstream comparison uses the same persisted hash contract.
                contentHash = ArtifactHash.RequireSha256(
                    contentHash,
                    $"ModuleCatalog asset '{artifactPath}' hash");
            }

            var asset = new AssetEntry(
                sourcePath,
                artifactPath,
                kind,
                contentHash,
                string.IsNullOrWhiteSpace(importPath) ? null : NormalizeRelativePath(importPath!));
            var catalogAsset = new CatalogAssetRecord(ownerModulePath, asset);
            var identity = ownerModulePath + "\n" + artifactPath;
            if (assets.TryGetValue(identity, out var existing) && !HasSameCatalogAsset(existing, catalogAsset))
                throw new InvalidOperationException(
                    $"ModuleCatalog '{assembly.Location}' declares conflicting assets for '{ownerModulePath}:{artifactPath}'.");
            assets[identity] = catalogAsset;
        }

        return assets.Values
            .OrderBy(static asset => asset.OwnerModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.Asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.Asset.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ModuleRecord> BindAssets(
        Assembly assembly,
        IReadOnlyList<ModuleRecord> modules,
        IReadOnlyList<CatalogAssetRecord> assets)
    {
        var modulesByPath = modules.ToDictionary(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase);
        var assetsByOwner = new Dictionary<string, List<AssetEntry>>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in assets)
        {
            if (!modulesByPath.ContainsKey(asset.OwnerModulePath))
            {
                throw new InvalidOperationException(
                    $"ModuleCatalog asset '{asset.Asset.ArtifactPath}' in '{assembly.Location}' references missing owner module '{asset.OwnerModulePath}'.");
            }

            if (!assetsByOwner.TryGetValue(asset.OwnerModulePath, out var ownerAssets))
            {
                ownerAssets = [];
                assetsByOwner.Add(asset.OwnerModulePath, ownerAssets);
            }

            ownerAssets.Add(asset.Asset);
        }

        return modules
            .Select(module => module with
            {
                Assets = assetsByOwner.TryGetValue(module.RelativePath, out var ownerAssets)
                    ? ownerAssets
                        .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
                        .ToArray()
                    : []
            })
            .ToArray();
    }

    private static HmrMetadata? ReadHmrMetadata(Type itemType, object item, string moduleId)
    {
        var providerId = TryReadString(itemType, item, "HmrProviderId");
        var hmrModuleId = TryReadString(itemType, item, "HmrModuleId");
        var payload = TryReadString(itemType, item, "HmrPayload");
        if (providerId is null && hmrModuleId is null && payload is null)
            return null;

        if (string.IsNullOrWhiteSpace(providerId) ||
            string.IsNullOrWhiteSpace(hmrModuleId) ||
            string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException(
                $"ModuleCatalog module '{moduleId}' must provide complete HMR metadata.");
        }

        return HmrMetadata.Create(providerId, hmrModuleId, payload);
    }

    private static IReadOnlyList<string> ReadStrings(
        Type itemType,
        object item,
        string propertyName,
        bool normalizePaths = false)
    {
        var property = itemType.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property is null || property.GetValue(item) is null)
            return [];
        var value = property.GetValue(item);
        if (value is string || value is not IEnumerable values)
            throw new InvalidOperationException(
                $"ModuleCatalog property '{propertyName}' must be a string collection.");

        var result = new List<string>();
        foreach (var itemValue in values)
        {
            if (itemValue is not string text || string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException(
                    $"ModuleCatalog property '{propertyName}' contains an empty or non-string value.");
            result.Add(normalizePaths ? NormalizeRelativePath(text) : text.Trim());
        }

        return result
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateSchema(Type catalogType, Assembly assembly)
    {
        var schemaVersion = ReadStaticInt(catalogType, "SchemaVersion");
        if (schemaVersion != ContractSchemaVersion)
            throw new InvalidOperationException(
                $"Unsupported ModuleCatalog schema version '{schemaVersion}' in '{assembly.Location}'.");
    }

    private static int ReadStaticInt(Type type, string name)
    {
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field?.GetValue(null) is int fieldValue)
            return fieldValue;
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (property?.GetValue(null) is int propertyValue)
            return propertyValue;
        throw new InvalidOperationException($"Static integer '{name}' was not found on '{type.FullName}'.");
    }

    private static string? TryReadStaticString(Type type, string name)
    {
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field?.GetValue(null) is string fieldValue && !string.IsNullOrWhiteSpace(fieldValue))
            return fieldValue;
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        return property?.GetValue(null) as string;
    }

    private static string ReadRequiredString(Type type, object item, string name)
        => TryReadString(type, item, name)
           ?? throw new InvalidOperationException(
               $"ModuleCatalog item property '{name}' was not found on '{type.FullName}'.");

    private static string? TryReadString(Type type, object item, string name)
    {
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return property?.GetValue(item) as string;
    }

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized) ||
            Path.IsPathRooted(normalized) ||
            normalized.StartsWith("/", StringComparison.Ordinal) ||
            (normalized.Length > 1 && char.IsLetter(normalized[0]) && normalized[1] == ':'))
        {
            throw new InvalidOperationException($"ModuleCatalog path must be a relative path: '{path}'.");
        }

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"ModuleCatalog path cannot escape its owner: '{path}'.");
        return string.Join("/", segments);
    }

    private static bool HasSameCatalogAsset(CatalogAssetRecord left, CatalogAssetRecord right)
        => StringComparer.OrdinalIgnoreCase.Equals(left.OwnerModulePath, right.OwnerModulePath) &&
           HasSameAsset(left.Asset, right.Asset);

    private static bool HasSameAsset(AssetEntry left, AssetEntry right)
        => StringComparer.Ordinal.Equals(left.SourcePath, right.SourcePath) &&
           StringComparer.Ordinal.Equals(left.ArtifactPath, right.ArtifactPath) &&
           StringComparer.Ordinal.Equals(left.Kind, right.Kind) &&
           StringComparer.Ordinal.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.ImportPath, right.ImportPath);

    private static bool HashMatchesContent(string declaredHash, string content)
        => StringComparer.Ordinal.Equals(
            ArtifactHash.RequireSha256(declaredHash, "ModuleCatalog hash"),
            ArtifactHash.ComputeSha256(content));
}

/// <summary>Asset ownership inside one ModuleCatalog before it is attached to its module.</summary>
internal sealed record CatalogAssetRecord(string OwnerModulePath, AssetEntry Asset);

/// <summary>All ModuleCatalog data discovered in one assembly.</summary>
internal sealed record CatalogReadResult(
    IReadOnlyList<ModuleRecord> Modules,
    IReadOnlyList<AssetEntry> Assets);
