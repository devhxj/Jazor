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
                PackageImports: TryReadStrings(itemType, item, "PackageImports")));
        }

        return modules;
    }

    private static Type? ResolveModuleCatalogType(Assembly assembly)
        => assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false);

    private static IReadOnlyList<ModuleRecord> ReadArtifactCatalog(Assembly assembly, Type catalogType)
    {
        ValidateSchema(catalogType, "artifact catalog", assembly);
        var producerId = ReadStaticString(catalogType, "ProducerId");
        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetModules was not found in artifact catalog '{assembly.Location}'.");
        if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in artifact catalog '{assembly.Location}'.");

        var assets = ReadCatalogAssets(assembly, catalogType, "artifact catalog");
        var assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assembly.Location);
        var modules = new List<ModuleRecord>();
        var moduleIds = new HashSet<string>(StringComparer.Ordinal);
        var modulePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var id = ReadString(itemType, item, "Id");
            var relativePathValue = ReadString(itemType, item, "RelativePath");
            EnsureModuleIdentityIsUnique(moduleIds, modulePaths, id, relativePathValue, producerId);
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
                NormalizeRelativePath(relativePathValue),
                ReadString(itemType, item, "Content"),
                ReadString(itemType, item, "Hash"),
                hasSourceMap ? NormalizeRelativePath(sourceMapRelativePath!) : null,
                hasSourceMap ? sourceMapContent : null,
                hasSourceMap ? mapHash : null,
                Assets: assets,
                PackageImports: TryReadStrings(itemType, item, "PackageImports"),
                Hmr: TryReadHmrMetadata(itemType, item, id, producerId)));
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

    private static IReadOnlyList<AssetEntry> ReadCatalogAssets(
        Assembly assembly,
        Type catalogType,
        string catalogKind)
    {
        var getAssets = catalogType.GetMethod("GetAssets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getAssets is null)
            return [];

        if (getAssets.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetAssets returned null in {catalogKind} '{assembly.Location}'.");

        var assets = new List<AssetEntry>();
        var assetsByPath = new Dictionary<string, AssetEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var sourcePath = NormalizeRelativePath(ReadString(itemType, item, "SourcePath"));
            var artifactPath = NormalizeRelativePath(ReadString(itemType, item, "ArtifactPath"));
            var kind = TryReadString(itemType, item, "Kind") ?? AssetEntry.KindStatic;
            var rawImportPath = TryReadString(itemType, item, "ImportPath");
            var importPath = string.IsNullOrWhiteSpace(rawImportPath)
                ? null
                : NormalizeRelativePath(rawImportPath!);
            if (kind is not (AssetEntry.KindStatic or AssetEntry.KindModuleSource))
            {
                throw new InvalidOperationException(
                    $"Unsupported artifact asset kind '{kind}' for '{artifactPath}' in {catalogKind} '{assembly.Location}'.");
            }

            if (kind == AssetEntry.KindModuleSource && importPath is null)
            {
                throw new InvalidOperationException(
                    $"Module-source asset '{artifactPath}' in {catalogKind} '{assembly.Location}' must declare ImportPath.");
            }

            var asset = new AssetEntry(
                sourcePath,
                artifactPath,
                kind,
                TryReadString(itemType, item, "ContentHash") ?? TryReadString(itemType, item, "Hash") ?? string.Empty,
                importPath);
            if (assetsByPath.TryGetValue(artifactPath, out var existing))
            {
                if (!HasSameAsset(existing, asset))
                {
                    throw new InvalidOperationException(
                        $"{catalogKind} '{assembly.Location}' declares conflicting assets for '{artifactPath}'.");
                }

                continue;
            }

            assetsByPath.Add(artifactPath, asset);
            assets.Add(asset);
        }

        return assets
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
        var assets = ReadCatalogAssets(assembly, catalogType, "runtime provider catalog");
        var modules = new List<ModuleRecord>();
        var moduleIds = new HashSet<string>(StringComparer.Ordinal);
        var modulePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var id = ReadString(itemType, item, "Id");
            var relativePathValue = ReadString(itemType, item, "RelativePath");
            var relativePath = NormalizeRelativePath(relativePathValue);
            EnsureModuleIdentityIsUnique(moduleIds, modulePaths, id, relativePathValue, providerId);
            var content = ReadRuntimeProviderModuleContent(assembly, itemType, item, id);
            var declaredHash = TryReadString(itemType, item, "Hash");
            if (declaredHash is not null && !HashMatchesContent(declaredHash, content))
            {
                throw new InvalidOperationException(
                    $"Runtime provider module '{id}' from provider '{providerId}' declares hash '{declaredHash}' that does not match its content.");
            }

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
                    $"Runtime provider module '{id}' from provider '{providerId}' must provide SourceMapRelativePath, SourceMapContent, and MapHash together.");
            }

            modules.Add(new ModuleRecord(
                assembly.Location,
                assemblyName,
                TryReadString(itemType, item, "TypeName") ?? id,
                id,
                relativePath,
                content,
                declaredHash ?? ComputeSha256Hash(content),
                hasSourceMap ? NormalizeRelativePath(sourceMapRelativePath!) : null,
                hasSourceMap ? sourceMapContent : null,
                hasSourceMap ? mapHash : null,
                Assets: assets,
                PackageImports: TryReadStrings(itemType, item, "PackageImports"),
                Hmr: TryReadHmrMetadata(itemType, item, id, providerId),
                RuntimeProviderId: providerId,
                Dependencies: ReadRuntimeProviderDependencies(itemType, item, id, providerId)));
        }

        return modules;
    }

    private static string ReadRuntimeProviderModuleContent(
        Assembly assembly,
        Type itemType,
        object item,
        string moduleId)
    {
        var inlineContent = TryReadString(itemType, item, "Content");
        var resourceName = TryReadString(itemType, item, "ResourceName");
        var hasInlineContent = inlineContent is not null;
        var hasResource = !string.IsNullOrWhiteSpace(resourceName);
        if (hasInlineContent == hasResource)
        {
            throw new InvalidOperationException(
                $"Runtime provider module '{moduleId}' must provide exactly one of Content or ResourceName.");
        }

        if (hasInlineContent)
            return inlineContent!.ReplaceLineEndings("\n");

        using var stream = assembly.GetManifestResourceStream(resourceName!)
            ?? throw new InvalidOperationException(
                $"Runtime provider resource '{resourceName}' for module '{moduleId}' was listed but could not be opened from '{assembly.Location}'.");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
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

        var entriesBySpecifier = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var specifier = ReadString(itemType, item, "Specifier");
            if (string.IsNullOrWhiteSpace(specifier))
            {
                throw new InvalidOperationException(
                    $"Runtime provider '{providerId}' declares an empty import-map specifier in '{assembly.Location}'.");
            }

            var artifactPath = NormalizeImportMapArtifactPath(ReadString(itemType, item, "ArtifactPath"));
            if (entriesBySpecifier.TryGetValue(specifier, out var existingArtifactPath))
            {
                if (!StringComparer.Ordinal.Equals(existingArtifactPath, artifactPath))
                {
                    throw new InvalidOperationException(
                        $"Runtime provider '{providerId}' declares conflicting import-map paths for '{specifier}'.");
                }

                continue;
            }

            entriesBySpecifier.Add(specifier, artifactPath);
            importMapEntries.Add(new ImportMapEntry(providerId, specifier, artifactPath));
        }
    }

    private static IReadOnlyList<string> ReadRuntimeProviderDependencies(
        Type itemType,
        object item,
        string moduleId,
        string providerId)
    {
        var property = itemType.GetProperty(
            "Dependencies",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var propertyValue = property?.GetValue(item);
        if (property is null || propertyValue is null)
            return [];

        if (propertyValue is string || propertyValue is not System.Collections.IEnumerable values)
        {
            throw new InvalidOperationException(
                $"Runtime provider module '{moduleId}' from provider '{providerId}' must expose Dependencies as a string collection.");
        }

        var dependencies = new List<string>();
        foreach (var value in values)
        {
            if (value is not string dependency || string.IsNullOrWhiteSpace(dependency))
            {
                throw new InvalidOperationException(
                    $"Runtime provider module '{moduleId}' from provider '{providerId}' declares an empty or non-string dependency path.");
            }

            dependencies.Add(NormalizeRelativePath(dependency));
        }

        return dependencies
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static dependency => dependency, StringComparer.Ordinal)
            .ToArray();
    }

    private static void EnsureModuleIdentityIsUnique(
        HashSet<string> moduleIds,
        HashSet<string> modulePaths,
        string id,
        string relativePath,
        string providerId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                $"Provider '{providerId}' declares a module with an empty id.");
        }

        var normalizedPath = NormalizeRelativePath(relativePath);
        if (!moduleIds.Add(id))
        {
            throw new InvalidOperationException(
                $"Provider '{providerId}' declares duplicate module id '{id}'.");
        }

        if (!modulePaths.Add(normalizedPath))
        {
            throw new InvalidOperationException(
                $"Provider '{providerId}' declares duplicate module path '{normalizedPath}'.");
        }
    }

    private static bool HasSameAsset(AssetEntry left, AssetEntry right)
        => StringComparer.Ordinal.Equals(left.SourcePath, right.SourcePath) &&
           StringComparer.Ordinal.Equals(left.Kind, right.Kind) &&
           StringComparer.Ordinal.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.ImportPath, right.ImportPath);

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
        var normalized = relativePath.Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Module relative path cannot be empty.");

        if (Path.IsPathRooted(normalized) ||
            normalized.StartsWith("/", StringComparison.Ordinal) ||
            (normalized.Length >= 2 && char.IsLetter(normalized[0]) && normalized[1] == ':'))
            throw new InvalidOperationException($"Module relative path must be relative: '{relativePath}'.");

        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0)
            throw new InvalidOperationException("Module relative path cannot be empty.");
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

    private static bool HashMatchesContent(string declaredHash, string content)
    {
        var normalizedDeclared = declaredHash.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? declaredHash.Substring("sha256:".Length)
            : declaredHash;
        var computed = ComputeSha256Hash(content).Substring("sha256:".Length);
        return string.Equals(normalizedDeclared, computed, StringComparison.OrdinalIgnoreCase);
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
