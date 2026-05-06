using System.Reflection;

namespace Jazor.Emit;

internal static class CatalogReader
{
    public static IReadOnlyList<EmitModuleRecord>? TryRead(Assembly assembly)
    {
        var catalogType = ResolveCatalogType(assembly);
        if (catalogType is null)
            return null;
        var sourceMapsById = TryReadSourceMapsById(assembly);

        var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) 
            ?? throw new InvalidOperationException($"GetModules was not found in '{assembly.Location}'.");
		if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in '{assembly.Location}'.");

        var modules = new List<EmitModuleRecord>();
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            var moduleId = ReadString(itemType, item, "Id");
            sourceMapsById.TryGetValue(moduleId, out var sourceMap);
            modules.Add(new EmitModuleRecord(
                assembly.Location,
                ReadString(itemType, item, "AssemblyName"),
                ReadString(itemType, item, "TypeName"),
                moduleId,
                NormalizeRelativePath(ReadString(itemType, item, "RelativePath")),
                ReadString(itemType, item, "Content"),
                ReadString(itemType, item, "Hash"),
                sourceMap?.SourceMapRelativePath,
                sourceMap?.SourceMapContent,
                sourceMap?.MapHash));
        }

        return modules;
    }

    private static Type? ResolveCatalogType(Assembly assembly)
    {
        var catalogType = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false);
        if (catalogType is not null)
            return catalogType;

        // ECMAScript ships a repository-owned CLR runtime catalog rather than a user-generated module catalog.
        return assembly.GetType("ECMAScript.Catalog", throwOnError: false, ignoreCase: false);
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

    private sealed record EmitModuleSourceMapRecord(
        string SourceMapRelativePath,
        string SourceMapContent,
        string MapHash);
}
