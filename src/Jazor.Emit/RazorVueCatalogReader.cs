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
                continue;

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
        if (ReadEnumerable(itemType, item, "SourceOrigins") is not { } items)
            return [];

        var origins = new List<RazorVueEmitSourceOriginRecord>();
        foreach (var entry in items)
        {
            if (entry is null)
                continue;

            var entryType = entry.GetType();
            origins.Add(new RazorVueEmitSourceOriginRecord(
                ReadString(entryType, entry, "SourceFilePath"),
                ReadInt32(entryType, entry, "SourceSpanStart"),
                ReadInt32(entryType, entry, "SourceSpanLength"),
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
        if (ReadEnumerable(itemType, item, propertyName) is not { } items)
            return [];

        return items.OfType<object>().Select(static entry => entry?.ToString() ?? string.Empty).ToArray();
    }

    private static System.Collections.IEnumerable? ReadEnumerable(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(item) as System.Collections.IEnumerable;
    }

    private static string ReadString(Type itemType, object item, string propertyName)
    {
        var property = itemType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (property?.GetValue(property.GetMethod?.IsStatic == true ? null : item) is string value)
            return value;

        throw new InvalidOperationException($"Property '{propertyName}' was not found on '{itemType.FullName}'.");
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

internal sealed record RazorVueCatalogRecord(
    string AssemblyName,
    IReadOnlyList<RazorVueEmitArtifactRecord> Artifacts);

internal sealed record RazorVueEmitArtifactRecord(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    IReadOnlyList<string> Imports,
    IReadOnlyList<string> Styles,
    RazorVueEmitArtifactIdentity Identity,
    RazorVueEmitRuntimeHints Hints,
    IReadOnlyList<RazorVueEmitSourceOriginRecord> SourceOrigins);

internal sealed record RazorVueEmitArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    RazorVueHmrBoundaryKind HmrBoundaryKind);

internal sealed record RazorVueEmitRuntimeHints(
    bool RequiresVueRuntime,
    bool RequiresHydration,
    bool SupportsSsr,
    bool UsesTeleport,
    bool UsesSuspense,
    bool UsesKeepAlive);

internal sealed record RazorVueEmitSourceOriginRecord(
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    RazorVueMappingQualityRecord MappingQuality,
    RazorVueOriginProvenanceRecord Provenance);

internal enum RazorVueHmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}

internal enum RazorVueMappingQualityRecord
{
    ExactSource,
    MappedFromGenerated,
    GeneratedOnly
}

internal enum RazorVueOriginProvenanceRecord
{
    RazorSourceMap,
    GeneratedSyntaxLocation,
    GeneratedFallback
}
