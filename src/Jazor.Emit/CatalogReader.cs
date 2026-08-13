using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Jazor.Emit;

/// <summary>
/// Reads framework-neutral generated artifact catalogs and runtime providers from emitted assemblies.
/// The contract is intentionally structural because the producer can be an analyzer assembly loaded in
/// an isolated context. Providers own their runtime resources and import-map contributions.
/// </summary>
internal static class CatalogReader
{
    private const string ArtifactCatalogTypeName = "Jazor.Generated.ArtifactCatalog";
    private const string RuntimeProviderCatalogTypeName = "Jazor.Artifacts.RuntimeProviderCatalog";
    private const string ClrRuntimeCatalogTypeName = "ECMAScript.Catalog";
    private const string ClrRuntimeProviderId = "jazor.clr";
    private const int ContractSchemaVersion = 1;

    /// <summary>Compatibility helper for callers that only need materializable modules.</summary>
    public static IReadOnlyList<ModuleRecord>? TryRead(Assembly assembly)
    {
        var result = TryReadCatalogs(assembly);
        return result.Modules.Count == 0
            ? null
            : result.Modules;
    }

    public static CatalogReadResult TryReadCatalogs(Assembly assembly)
    {
        var modules = new List<ModuleRecord>();
        var importMapEntries = new List<ImportMapEntry>();

        var catalogType = ResolveModuleCatalogType(assembly);
        if (catalogType is not null)
            modules.AddRange(ReadModuleCatalog(assembly, catalogType));

        var artifactCatalogType = assembly.GetType(ArtifactCatalogTypeName, throwOnError: false, ignoreCase: false);
        if (artifactCatalogType is not null)
            modules.AddRange(ReadArtifactCatalog(assembly, artifactCatalogType));

        var runtimeProviderCatalogType = assembly.GetType(RuntimeProviderCatalogTypeName, throwOnError: false, ignoreCase: false);
        if (runtimeProviderCatalogType is not null)
            modules.AddRange(ReadRuntimeProviderCatalog(assembly, runtimeProviderCatalogType, importMapEntries));

        return new CatalogReadResult(modules, importMapEntries);
    }

    private static IReadOnlyList<ModuleRecord> ReadModuleCatalog(Assembly assembly, Type catalogType)
    {
        var sourceMapsById = TryReadSourceMapsById(assembly);
        var isClrRuntimeCatalog = string.Equals(catalogType.FullName, ClrRuntimeCatalogTypeName, StringComparison.Ordinal);

        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetModules was not found in '{assembly.Location}'.");
        if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in '{assembly.Location}'.");

        var modules = new List<ModuleRecord>();
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var moduleId = ReadString(itemType, item, "Id");
            sourceMapsById.TryGetValue(moduleId, out var sourceMap);
            modules.Add(new ModuleRecord(
                assembly.Location,
                ReadString(itemType, item, "AssemblyName"),
                ReadString(itemType, item, "TypeName"),
                moduleId,
                NormalizeRelativePath(ReadString(itemType, item, "RelativePath")),
                ReadString(itemType, item, "Content"),
                ReadString(itemType, item, "Hash"),
                sourceMap?.SourceMapRelativePath,
                sourceMap?.SourceMapContent,
                sourceMap?.MapHash,
                PackageImports: TryReadStrings(itemType, item, "PackageImports"),
                RuntimeProviderId: isClrRuntimeCatalog ? ClrRuntimeProviderId : null,
                RuntimeDependencies: isClrRuntimeCatalog
                    ? TryReadStrings(itemType, item, "RuntimeDependencies")
                        .Select(NormalizeRelativePath)
                        .ToArray()
                    : null));
        }

        return modules;
    }

    private static Type? ResolveModuleCatalogType(Assembly assembly)
    {
        var catalogType = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false);
        if (catalogType is not null)
            return catalogType;

        // ECMAScript ships a repository-owned CLR runtime catalog rather than a user-generated module catalog.
        return assembly.GetType("ECMAScript.Catalog", throwOnError: false, ignoreCase: false);
    }

    private static IReadOnlyList<ModuleRecord> ReadArtifactCatalog(Assembly assembly, Type catalogType)
    {
        ValidateSchema(catalogType, "artifact catalog", assembly);
        var producerId = ReadStaticString(catalogType, "ProducerId");
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetModules was not found in artifact catalog '{assembly.Location}'.");
        if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in artifact catalog '{assembly.Location}'.");

        var assets = ReadArtifactCatalogAssets(assembly, catalogType);
        var assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assembly.Location);
        var modules = new List<ModuleRecord>();
        var assetCarrierWritten = false;
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var id = ReadString(itemType, item, "Id");
            var sourceMapRelativePath = TryReadString(itemType, item, "SourceMapRelativePath");
            var sourceMapContent = TryReadString(itemType, item, "SourceMapContent");
            var mapHash = TryReadString(itemType, item, "MapHash");
            var hasSourceMap = !string.IsNullOrWhiteSpace(sourceMapRelativePath) ||
                               !string.IsNullOrWhiteSpace(sourceMapContent) ||
                               !string.IsNullOrWhiteSpace(mapHash);
            if (hasSourceMap &&
                (string.IsNullOrWhiteSpace(sourceMapRelativePath) ||
                 string.IsNullOrWhiteSpace(sourceMapContent) ||
                 string.IsNullOrWhiteSpace(mapHash)))
            {
                throw new InvalidOperationException(
                    $"Artifact catalog module '{id}' from provider '{producerId}' must provide SourceMapRelativePath, SourceMapContent, and MapHash together.");
            }

            modules.Add(new ModuleRecord(
                assembly.Location,
                assemblyName,
                ReadString(itemType, item, "TypeName"),
                id,
                NormalizeRelativePath(ReadString(itemType, item, "RelativePath")),
                ReadString(itemType, item, "Content"),
                ReadString(itemType, item, "Hash"),
                hasSourceMap ? NormalizeRelativePath(sourceMapRelativePath!) : null,
                hasSourceMap ? sourceMapContent : null,
                hasSourceMap ? mapHash : null,
                Assets: assetCarrierWritten ? null : assets,
                PackageImports: TryReadStrings(itemType, item, "PackageImports"),
                Hmr: TryReadHmrMetadata(itemType, item, id, producerId)));
            assetCarrierWritten = true;
        }

        return modules;
    }

    private static HmrMetadata? TryReadHmrMetadata(
        Type itemType,
        object item,
        string moduleId,
        string producerId)
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
                $"Artifact catalog module '{moduleId}' from provider '{producerId}' must provide complete HMR metadata when any HMR field is present.");
        }

        return HmrMetadata.Create(providerId, hmrModuleId, payload);
    }

    private static IReadOnlyList<AssetEntry> ReadArtifactCatalogAssets(Assembly assembly, Type catalogType)
    {
        var getAssets = catalogType.GetMethod("GetAssets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getAssets is null)
            return [];

        if (getAssets.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetAssets returned null in artifact catalog '{assembly.Location}'.");

        var assets = new List<AssetEntry>();
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            assets.Add(new AssetEntry(
                NormalizeRelativePath(ReadString(itemType, item, "SourcePath")),
                NormalizeRelativePath(ReadString(itemType, item, "ArtifactPath")),
                TryReadString(itemType, item, "Kind") ?? AssetEntry.KindStatic,
                TryReadString(itemType, item, "ContentHash") ?? TryReadString(itemType, item, "Hash") ?? string.Empty,
                TryReadString(itemType, item, "ImportPath") is { } importPath
                    ? NormalizeRelativePath(importPath)
                    : null));
        }

        return assets
            .GroupBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ModuleRecord> ReadRuntimeProviderCatalog(
        Assembly assembly,
        Type catalogType,
        List<ImportMapEntry> importMapEntries)
    {
        ValidateSchema(catalogType, "runtime provider catalog", assembly);
        var providerId = ReadStaticString(catalogType, "ProviderId");
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetModules was not found in runtime provider catalog '{assembly.Location}'.");
        if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in runtime provider catalog '{assembly.Location}'.");

        ReadRuntimeProviderImportMapEntries(assembly, catalogType, providerId, importMapEntries);

        var assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assembly.Location);
        var modules = new List<ModuleRecord>();
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var resourceName = ReadString(itemType, item, "ResourceName");
            var id = ReadString(itemType, item, "Id");
            var relativePath = NormalizeRelativePath(ReadString(itemType, item, "RelativePath"));
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Runtime provider resource '{resourceName}' was listed but could not be opened from '{assembly.Location}'.");
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = reader.ReadToEnd().ReplaceLineEndings("\n");

            modules.Add(new ModuleRecord(
                assembly.Location,
                assemblyName,
                id,
                id,
                relativePath,
                content,
                ComputeSha256Hash(content),
                RuntimeProviderId: providerId,
                RuntimeDependencies: TryReadStrings(itemType, item, "Dependencies")
                    .Select(NormalizeRelativePath)
                    .ToArray()));
        }

        return modules;
    }

    private static void ReadRuntimeProviderImportMapEntries(
        Assembly assembly,
        Type catalogType,
        string providerId,
        List<ImportMapEntry> importMapEntries)
    {
        var getImportMapEntries = catalogType.GetMethod(
            "GetImportMapEntries",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getImportMapEntries is null)
            return;

        if (getImportMapEntries.Invoke(null, null) is not System.Collections.IEnumerable items)
        {
            throw new InvalidOperationException(
                $"GetImportMapEntries returned null in runtime provider catalog '{assembly.Location}'.");
        }

        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            importMapEntries.Add(new ImportMapEntry(
                providerId,
                ReadString(itemType, item, "Specifier"),
                NormalizeImportMapArtifactPath(ReadString(itemType, item, "ArtifactPath"))));
        }
    }

    private static void ValidateSchema(Type catalogType, string catalogKind, Assembly assembly)
    {
        var schemaVersion = ReadStaticInt(catalogType, "SchemaVersion");
        if (schemaVersion != ContractSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported {catalogKind} schema version '{schemaVersion}' in '{assembly.Location}'.");
        }
    }

    private static int ReadStaticInt(Type catalogType, string fieldName)
    {
        var field = catalogType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field?.GetValue(null) is int value)
            return value;

        var property = catalogType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (property?.GetValue(null) is int propertyValue)
            return propertyValue;

        throw new InvalidOperationException($"Static integer '{fieldName}' was not found on '{catalogType.FullName}'.");
    }

    private static string ReadStaticString(Type catalogType, string fieldName)
    {
        var field = catalogType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field?.GetValue(null) is string value && !string.IsNullOrWhiteSpace(value))
            return value;

        var property = catalogType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (property?.GetValue(null) is string propertyValue && !string.IsNullOrWhiteSpace(propertyValue))
            return propertyValue;

        throw new InvalidOperationException($"Static string '{fieldName}' was not found on '{catalogType.FullName}'.");
    }

    private static IReadOnlyDictionary<string, EmitModuleSourceMapRecord> TryReadSourceMapsById(Assembly assembly)
    {
        var sourceMapCatalogType = assembly.GetType("Jazor.Generated.ModuleSourceMapCatalog", throwOnError: false, ignoreCase: false);
        if (sourceMapCatalogType is null)
            return new Dictionary<string, EmitModuleSourceMapRecord>(StringComparer.Ordinal);

        var getModules = sourceMapCatalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetModules was not found in source-map catalog '{assembly.Location}'.");
        if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in source-map catalog '{assembly.Location}'.");

        var sourceMapsById = new Dictionary<string, EmitModuleSourceMapRecord>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var id = ReadString(itemType, item, "Id");
            if (sourceMapsById.ContainsKey(id))
                throw new InvalidOperationException($"Duplicate source-map entry for module id '{id}'.");

            sourceMapsById.Add(id, new EmitModuleSourceMapRecord(
                NormalizeRelativePath(ReadString(itemType, item, "SourceMapRelativePath")),
                ReadString(itemType, item, "SourceMapContent"),
                ReadString(itemType, item, "MapHash")));
        }

        return sourceMapsById;
    }

    private static string ReadString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property?.GetValue(item) is string value)
            return value;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
    }

    private static string? TryReadString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return property?.GetValue(item) as string;
    }

    private static IReadOnlyList<string> TryReadStrings(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property?.GetValue(item) is not System.Collections.IEnumerable values)
            return [];

        return values.Cast<object?>()
            .OfType<string>()
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Module relative path cannot be empty.");

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"Module relative path must be relative: '{relativePath}'.");

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Module relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }

    private static string NormalizeImportMapArtifactPath(string artifactPath)
    {
        var hasTrailingSeparator = artifactPath.Replace('\\', '/').EndsWith("/", StringComparison.Ordinal);
        var normalized = NormalizeRelativePath(artifactPath);
        return hasTrailingSeparator ? normalized + "/" : normalized;
    }

    private static string ComputeSha256Hash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var item in bytes)
            builder.Append(item.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));

        return "sha256:" + builder;
    }

    private sealed record EmitModuleSourceMapRecord(
        string SourceMapRelativePath,
        string SourceMapContent,
        string MapHash);
}

/// <summary>All neutral catalog data discovered in one assembly.</summary>
internal sealed record CatalogReadResult(
    IReadOnlyList<ModuleRecord> Modules,
    IReadOnlyList<ImportMapEntry> ImportMapEntries);
