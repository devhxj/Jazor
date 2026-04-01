using System.Reflection;

namespace Jazor.Emit;

internal static class CatalogReader
{
    public static IReadOnlyList<EmitModuleRecord>? TryRead(Assembly assembly)
    {
        var catalogType = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: false, ignoreCase: false);
        if (catalogType is null)
            return null;

		var getModules = catalogType.GetMethod("GetModules", BindingFlags.Public | BindingFlags.Static) 
            ?? throw new InvalidOperationException($"GetModules was not found in '{assembly.Location}'.");
		if (getModules.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetModules returned null in '{assembly.Location}'.");

        var modules = new List<EmitModuleRecord>();
        foreach (var item in items)
        {
            if (item is null)
                continue;

            var itemType = item.GetType();
            modules.Add(new EmitModuleRecord(
                assembly.Location,
                ReadString(itemType, item, "AssemblyName"),
                ReadString(itemType, item, "TypeName"),
                ReadString(itemType, item, "Id"),
                NormalizeRelativePath(ReadString(itemType, item, "RelativePath")),
                ReadString(itemType, item, "Content"),
                ReadString(itemType, item, "Hash")));
        }

        return modules;
    }

    private static string ReadString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
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

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Module relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }
}
