using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Jazor.Emit;

/// <summary>Reads compiler and RazorVue module catalogs from an emitted assembly.</summary>
internal static class CatalogReader
{
    private const string RazorVueRuntimeResourcePrefix = "Jazor.RazorVue.Runtime.";
    private const string RazorVueRuntimeRelativePathPrefix = "@jazor/vue-runtime/";

    public static IReadOnlyList<ModuleRecord>? TryRead(Assembly assembly)
    {
        var modules = new List<ModuleRecord>();
        var catalogType = ResolveModuleCatalogType(assembly);
        if (catalogType is not null)
            modules.AddRange(ReadModuleCatalog(assembly, catalogType));

        var vueRenderCatalogType = assembly.GetType("Jazor.Generated.VueRenderCatalog", throwOnError: false, ignoreCase: false);
        if (vueRenderCatalogType is not null)
            modules.AddRange(ReadVueRenderCatalog(assembly, vueRenderCatalogType));

        modules.AddRange(ReadRazorVueRuntimeResources(assembly));

        return modules.Count == 0
            ? null
            : modules;
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
    {
        var catalogType = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false);
        if (catalogType is not null)
            return catalogType;

        // ECMAScript ships a repository-owned CLR runtime catalog rather than a user-generated module catalog.
        return assembly.GetType("ECMAScript.Catalog", throwOnError: false, ignoreCase: false);
    }

    private static IReadOnlyList<ModuleRecord> ReadVueRenderCatalog(Assembly assembly, Type catalogType)
    {
        var schemaVersion = ReadStaticInt(catalogType, "SchemaVersion");
        if (schemaVersion != 1)
        {
            throw new InvalidOperationException(
                $"Unsupported VueRenderCatalog schema version '{schemaVersion}' in '{assembly.Location}'.");
        }

        var runtimeProtocolVersion = ReadStaticInt(catalogType, "RuntimeProtocolVersion");
        if (runtimeProtocolVersion != 1)
        {
            throw new InvalidOperationException(
                $"Unsupported VueRenderCatalog runtime protocol version '{runtimeProtocolVersion}' in '{assembly.Location}'.");
        }

        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetModules was not found in VueRenderCatalog '{assembly.Location}'.");
        if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in VueRenderCatalog '{assembly.Location}'.");

        var assets = ReadVueRenderCatalogAssets(assembly, catalogType);
        var assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assembly.Location);
        var modules = new List<ModuleRecord>();
        var assetCarrierWritten = false;
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var componentId = ReadString(itemType, item, "ComponentId");
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
                    $"VueRenderCatalog module '{componentId}' must provide SourceMapRelativePath, SourceMapContent, and MapHash together.");
            }

            modules.Add(new ModuleRecord(
                assembly.Location,
                assemblyName,
                componentId,
                componentId,
                NormalizeRelativePath(ReadString(itemType, item, "RelativePath")),
                ReadString(itemType, item, "ModuleText"),
                ReadString(itemType, item, "ContentHash"),
                hasSourceMap ? NormalizeRelativePath(sourceMapRelativePath!) : null,
                hasSourceMap ? sourceMapContent : null,
                hasSourceMap ? mapHash : null,
                Assets: assetCarrierWritten ? null : assets,
                PackageImports: TryReadStrings(itemType, item, "PackageImports")));
            assetCarrierWritten = true;
        }

        return modules;
    }

    private static IReadOnlyList<AssetEntry> ReadVueRenderCatalogAssets(Assembly assembly, Type catalogType)
    {
        var getAssets = catalogType.GetMethod("GetAssets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getAssets is null)
            return [];

        if (getAssets.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetAssets returned null in VueRenderCatalog '{assembly.Location}'.");

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
                TryReadString(itemType, item, "ContentHash") ?? TryReadString(itemType, item, "Hash") ?? string.Empty));
        }

        return assets
            .GroupBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ModuleRecord> ReadRazorVueRuntimeResources(Assembly assembly)
    {
        var resourceNames = assembly
            .GetManifestResourceNames()
            .Where(static name => name.StartsWith(RazorVueRuntimeResourcePrefix, StringComparison.Ordinal) &&
                                  name.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
        if (resourceNames.Length == 0)
            return [];

        var assemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assembly.Location);
        var modules = new List<ModuleRecord>(resourceNames.Length);
        foreach (var resourceName in resourceNames)
        {
            var fileName = resourceName.Substring(RazorVueRuntimeResourcePrefix.Length);
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"RazorVue runtime resource '{resourceName}' was listed but could not be opened.");
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = reader.ReadToEnd().ReplaceLineEndings("\n");
            var moduleId = RazorVueRuntimeResourcePrefix + Path.GetFileNameWithoutExtension(fileName);

            modules.Add(new ModuleRecord(
                assembly.Location,
                assemblyName,
                moduleId,
                moduleId,
                NormalizeRelativePath(RazorVueRuntimeRelativePathPrefix + fileName),
                content,
                ComputeSha256Hash(content)));
        }

        return modules;
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
