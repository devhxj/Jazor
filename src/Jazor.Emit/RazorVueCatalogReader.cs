using System.Reflection;

namespace Jazor.Emit;

internal static class RazorVueCatalogReader
{
    public static RazorVueCatalogRecord? TryRead(Assembly assembly)
    {
        var catalogType = assembly.GetType("Jazor.Generated.RazorVueCatalog", throwOnError: false, ignoreCase: false);
        if (catalogType is null)
            return null;

        var assemblyName = ReadCatalogAssemblyName(catalogType);
        var getArtifacts = catalogType.GetMethod("GetArtifacts", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"GetArtifacts was not found in '{assembly.Location}'.");

        if (getArtifacts.Invoke(null, null) is not System.Collections.IEnumerable items)
            throw new InvalidOperationException($"GetArtifacts returned null in '{assembly.Location}'.");

        var artifacts = new List<RazorVueEmitArtifactRecord>();
        foreach (var item in items)
        {
            if (item is null)
                throw new InvalidOperationException($"GetArtifacts in '{assembly.Location}' contains a null artifact entry.");

            artifacts.Add(ReadArtifact(item.GetType(), item));
        }

        return new RazorVueCatalogRecord(assemblyName, artifacts);
    }

    private static string ReadCatalogAssemblyName(Type catalogType)
    {
        var property = catalogType.GetProperty("AssemblyName", BindingFlags.Public | BindingFlags.Static);
        if (property?.GetValue(null) is string value)
            return value;

        throw new InvalidOperationException($"AssemblyName was not found on '{catalogType.FullName}'.");
    }

    private static RazorVueEmitArtifactRecord ReadArtifact(Type itemType, object item)
    {
        return new RazorVueEmitArtifactRecord(
            ReadString(itemType, item, "ComponentName"),
            NormalizeRelativePath(ReadString(itemType, item, "RelativeModulePath")),
            ReadString(itemType, item, "ModuleCode"),
            ReadStringArray(itemType, item, "Imports"),
            ReadStringArray(itemType, item, "Styles"),
            ReadStringArray(itemType, item, "PluginRequirements"),
            ReadIdentity(itemType, item),
            ReadHints(itemType, item),
            ReadOrigins(itemType, item));
    }

    private static RazorVueEmitArtifactIdentity ReadIdentity(Type itemType, object item)
    {
        var value = ReadObject(itemType, item, "Identity");
        var valueType = value.GetType();
        return new RazorVueEmitArtifactIdentity(
            ReadString(valueType, value, "ComponentId"),
            ReadString(valueType, value, "ModuleId"),
            ReadString(valueType, value, "DescriptorHash"),
            ReadString(valueType, value, "TemplateHash"),
            ReadString(valueType, value, "LogicHash"),
            ReadEnum<RazorVueHmrBoundaryKind>(valueType, value, "HmrBoundaryKind"));
    }

    private static RazorVueEmitRuntimeHints ReadHints(Type itemType, object item)
    {
        var value = ReadObject(itemType, item, "Hints");
        var valueType = value.GetType();
        return new RazorVueEmitRuntimeHints(
            ReadBool(valueType, value, "RequiresVueRuntime"),
            ReadBool(valueType, value, "RequiresHydration"),
            ReadBool(valueType, value, "SupportsSsr"),
            ReadBool(valueType, value, "UsesTeleport"),
            ReadBool(valueType, value, "UsesSuspense"),
            ReadBool(valueType, value, "UsesKeepAlive"));
    }

    private static IReadOnlyList<RazorVueEmitSourceOriginRecord> ReadOrigins(Type itemType, object item)
    {
        var items = ReadRequiredEnumerable(itemType, item, "SourceOrigins");

        var origins = new List<RazorVueEmitSourceOriginRecord>();
        foreach (var entry in items)
        {
            if (entry is null)
                throw new InvalidOperationException($"Property 'SourceOrigins' on '{itemType.FullName}' contains a null entry.");

            var entryType = entry.GetType();
            origins.Add(new RazorVueEmitSourceOriginRecord(
                ReadString(entryType, entry, "SourceFilePath"),
                ReadInt32(entryType, entry, "SourceSpanStart"),
                ReadInt32(entryType, entry, "SourceSpanLength"),
                TryReadNullableString(entryType, entry, "GeneratedFilePath"),
                TryReadNullableInt32(entryType, entry, "GeneratedSpanStart"),
                TryReadNullableInt32(entryType, entry, "GeneratedSpanLength"),
                ReadInt32(entryType, entry, "StartLine"),
                ReadInt32(entryType, entry, "StartColumn"),
                ReadEnum<RazorVueMappingQualityRecord>(entryType, entry, "MappingQuality"),
                ReadEnum<RazorVueOriginProvenanceRecord>(entryType, entry, "Provenance")));
        }

        return origins;
    }

    private static object ReadObject(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(item) is { } value)
            return value;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
    }

    private static bool ReadBool(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(item) is bool value)
            return value;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
    }

    private static int ReadInt32(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(item) is int value)
            return value;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
    }

    private static int? TryReadNullableInt32(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
            return null;

        return property.GetValue(item) switch
        {
            null => null,
            int value => value,
            _ => throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' is not an Int32.")
        };
    }

    private static int? ReadNullableInt32(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
            throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");

        return property.GetValue(item) switch
        {
            null => null,
            int value => value,
            _ => throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' is not an Int32.")
        };
    }

    private static TEnum ReadEnum<TEnum>(Type itemType, object item, string propertyName)
        where TEnum : struct
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(item) is { } value && Enum.TryParse<TEnum>(value.ToString(), ignoreCase: false, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
    }

    private static string[] ReadStringArray(Type itemType, object item, string propertyName)
    {
        var items = ReadRequiredEnumerable(itemType, item, propertyName);

        var values = new List<string>();
        foreach (var entry in items)
        {
            if (entry is not string value)
                throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' contains a non-string entry.");

            values.Add(value);
        }

        return values.ToArray();
    }

    private static System.Collections.IEnumerable ReadRequiredEnumerable(Type itemType, object item, string propertyName)
    {
        return ReadEnumerable(itemType, item, propertyName)
            ?? throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' returned null.");
    }

    private static System.Collections.IEnumerable? ReadEnumerable(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
            throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");

        return property.GetValue(item) switch
        {
            null => null,
            System.Collections.IEnumerable value => value,
            _ => throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' is not enumerable.")
        };
    }

    private static string ReadString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (property?.GetValue(property.GetMethod?.IsStatic == true ? null : item) is string value)
            return value;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
    }

    private static string? TryReadNullableString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (property is null)
            return null;

        return property.GetValue(property.GetMethod?.IsStatic == true ? null : item) switch
        {
            null => null,
            string value => value,
            _ => throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' is not a string.")
        };
    }

    private static string? ReadNullableString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (property is null)
            throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");

        return property.GetValue(property.GetMethod?.IsStatic == true ? null : item) switch
        {
            null => null,
            string value => value,
            _ => throw new InvalidOperationException($"Property '{propertyName}' on '{itemType.FullName}' is not a string.")
        };
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"RazorVue artifact relative path must be relative: '{relativePath}'.");

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"RazorVue artifact relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }
}
