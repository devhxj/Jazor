using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Emit;

internal sealed record ManifestModel
{
    public const int CurrentSchemaVersion = 1;
    public const int CurrentRuntimeProtocolVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ManifestModel(string RootAssemblyPath, DateTime GeneratedAtUtc, List<ManifestModuleEntry> Modules)
        : this(
            CurrentSchemaVersion,
            CurrentRuntimeProtocolVersion,
            DeriveRootAssemblyName(RootAssemblyPath, Modules),
            RootAssemblyPath,
            GeneratedAtUtc,
            Modules,
            entries: null)
    {
    }

    public ManifestModel(string RootAssemblyPath, List<ManifestModuleEntry> Modules)
        : this(
            CurrentSchemaVersion,
            CurrentRuntimeProtocolVersion,
            DeriveRootAssemblyName(RootAssemblyPath, Modules),
            RootAssemblyPath,
            generatedAtUtc: null,
            Modules,
            entries: null)
    {
    }

    private ManifestModel(
        int schemaVersion,
        int runtimeProtocolVersion,
        string rootAssemblyName,
        string rootAssemblyPath,
        DateTime? generatedAtUtc,
        List<ManifestModuleEntry> modules,
        List<string>? entries)
    {
        SchemaVersion = schemaVersion;
        RuntimeProtocolVersion = runtimeProtocolVersion;
        RootAssemblyName = string.IsNullOrWhiteSpace(rootAssemblyName)
            ? DeriveRootAssemblyName(rootAssemblyPath, modules)
            : rootAssemblyName;
        RootAssemblyPath = rootAssemblyPath ?? string.Empty;
        GeneratedAtUtc = generatedAtUtc;
        Modules = NormalizeModules(modules ?? []);
        Entries = NormalizeEntries(entries, Modules, RootAssemblyName);
    }

    public int SchemaVersion { get; init; }

    public int RuntimeProtocolVersion { get; init; }

    public string RootAssemblyName { get; init; }

    public string RootAssemblyPath { get; init; }

    public DateTime? GeneratedAtUtc { get; init; }

    public List<string> Entries { get; init; }

    public List<ManifestModuleEntry> Modules { get; init; }

    public static ManifestModel? TryLoad(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;

        var schemaVersion = ReadOptionalInt(root, "schemaVersion", "SchemaVersion") ?? CurrentSchemaVersion;
        if (schemaVersion != CurrentSchemaVersion)
            throw new InvalidOperationException($"Unsupported Jazor manifest schema version '{schemaVersion}'.");

        var runtimeProtocolVersion = ReadOptionalInt(root, "runtimeProtocolVersion", "RuntimeProtocolVersion")
            ?? CurrentRuntimeProtocolVersion;
        if (runtimeProtocolVersion != CurrentRuntimeProtocolVersion)
            throw new InvalidOperationException(
                $"Unsupported Jazor manifest runtime protocol version '{runtimeProtocolVersion}'.");

        var modules = ReadModules(root);
        var rootAssemblyPath = ReadOptionalString(root, "rootAssemblyPath", "RootAssemblyPath") ?? string.Empty;
        var rootAssemblyName =
            ReadOptionalString(root, "rootAssemblyName", "RootAssemblyName") ??
            DeriveRootAssemblyName(rootAssemblyPath, modules);
        var generatedAtUtc = ReadOptionalDateTime(root, "generatedAtUtc", "GeneratedAtUtc");
        var entries = ReadEntries(root);

        return new ManifestModel(
            schemaVersion,
            runtimeProtocolVersion,
            rootAssemblyName,
            rootAssemblyPath,
            generatedAtUtc,
            modules,
            entries);
    }

    public void Save(string manifestPath)
    {
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var normalizedModules = NormalizeModules(Modules);
        var normalizedEntries = NormalizeEntries(Entries, normalizedModules, RootAssemblyName);
        var fileModel = new ManifestFileModel(
            CurrentSchemaVersion,
            CurrentRuntimeProtocolVersion,
            RootAssemblyName,
            normalizedEntries,
            normalizedModules
                .Select(static module => new ManifestModuleFileEntry(
                    module.AssemblyName,
                    module.TypeName,
                    module.Id,
                    module.RelativePath,
                    module.Hash,
                    module.SourceMapPath,
                    module.MapHash))
                .ToList());

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(fileModel, JsonOptions));
    }

    private static List<ManifestModuleEntry> ReadModules(JsonElement root)
    {
        if (!TryGetProperty(root, out var modulesElement, "modules", "Modules") ||
            modulesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var modules = new List<ManifestModuleEntry>();
        foreach (var moduleElement in modulesElement.EnumerateArray())
        {
            if (moduleElement.ValueKind != JsonValueKind.Object)
                continue;

            modules.Add(new ManifestModuleEntry(
                ReadRequiredString(moduleElement, "assemblyName", "AssemblyName"),
                ReadRequiredString(moduleElement, "typeName", "TypeName"),
                ReadRequiredString(moduleElement, "id", "Id"),
                ReadRequiredString(moduleElement, "path", "relativePath", "RelativePath"),
                ReadRequiredString(moduleElement, "contentHash", "hash", "Hash"),
                ReadOptionalString(moduleElement, "sourceMap", "sourceMapPath", "SourceMapPath"),
                ReadOptionalString(moduleElement, "sourceMapHash", "mapHash", "MapHash")));
        }

        return modules;
    }

    private static List<string>? ReadEntries(JsonElement root)
    {
        if (!TryGetProperty(root, out var entriesElement, "entries", "Entries") ||
            entriesElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var entries = new List<string>();
        foreach (var entryElement in entriesElement.EnumerateArray())
        {
            if (entryElement.ValueKind == JsonValueKind.String &&
                entryElement.GetString() is { Length: > 0 } entry)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static List<ManifestModuleEntry> NormalizeModules(IEnumerable<ManifestModuleEntry> modules)
    {
        var normalizedModules = new List<ManifestModuleEntry>();
        var indexByRelativePath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in modules)
        {
            var normalizedModule = module with
            {
                RelativePath = NormalizeRelativePath(module.RelativePath),
                SourceMapPath = module.SourceMapPath is null ? null : NormalizeRelativePath(module.SourceMapPath)
            };

            if (indexByRelativePath.TryGetValue(normalizedModule.RelativePath, out var existingIndex))
            {
                normalizedModules[existingIndex] = normalizedModule;
                continue;
            }

            indexByRelativePath.Add(normalizedModule.RelativePath, normalizedModules.Count);
            normalizedModules.Add(normalizedModule);
        }

        return normalizedModules
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> NormalizeEntries(
        IEnumerable<string>? entries,
        IReadOnlyList<ManifestModuleEntry> modules,
        string rootAssemblyName)
    {
        var selectedEntries = entries?.ToArray();
        if (selectedEntries is null || selectedEntries.Length == 0)
        {
            selectedEntries = modules
                .Where(module => string.Equals(module.AssemblyName, rootAssemblyName, StringComparison.OrdinalIgnoreCase))
                .Select(static module => module.RelativePath)
                .ToArray();
        }

        if (selectedEntries.Length == 0)
            selectedEntries = modules.Select(static module => module.RelativePath).ToArray();

        return selectedEntries
            .Where(static entry => !string.IsNullOrWhiteSpace(entry))
            .Select(NormalizeRelativePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static entry => entry, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Manifest relative path cannot be empty.");

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"Manifest relative path must be relative: '{relativePath}'.");

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Manifest relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }

    private static string DeriveRootAssemblyName(string? rootAssemblyPath, IReadOnlyList<ManifestModuleEntry>? modules)
    {
        if (!string.IsNullOrWhiteSpace(rootAssemblyPath))
        {
            try
            {
                if (File.Exists(rootAssemblyPath))
                {
                    var assemblyName = AssemblyName.GetAssemblyName(rootAssemblyPath).Name;
                    if (!string.IsNullOrWhiteSpace(assemblyName))
                        return assemblyName!;
                }
            }
            catch
            {
            }

            var fileName = Path.GetFileNameWithoutExtension(rootAssemblyPath);
            if (!string.IsNullOrWhiteSpace(fileName))
                return fileName;
        }

        var moduleAssemblyName = modules?
            .Select(static module => module.AssemblyName)
            .FirstOrDefault(static assemblyName => !string.IsNullOrWhiteSpace(assemblyName));
        return string.IsNullOrWhiteSpace(moduleAssemblyName)
            ? "Jazor"
            : moduleAssemblyName!;
    }

    private static string ReadRequiredString(JsonElement element, params string[] names)
        => ReadOptionalString(element, names)
           ?? throw new InvalidOperationException(
               $"Manifest field '{string.Join("' or '", names)}' is required.");

    private static string? ReadOptionalString(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }

    private static int? ReadOptionalInt(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
            return value;

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), out var stringValue))
        {
            return stringValue;
        }

        throw new InvalidOperationException($"Manifest integer field '{string.Join("' or '", names)}' is invalid.");
    }

    private static DateTime? ReadOptionalDateTime(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(property.GetString(), out var value))
        {
            return value;
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, out JsonElement property, params string[] names)
    {
        foreach (var item in element.EnumerateObject())
        {
            if (names.Any(name => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                property = item.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private sealed record ManifestFileModel(
        int SchemaVersion,
        int RuntimeProtocolVersion,
        string RootAssemblyName,
        List<string> Entries,
        List<ManifestModuleFileEntry> Modules);

    private sealed record ManifestModuleFileEntry(
        string AssemblyName,
        string TypeName,
        string Id,
        string Path,
        string ContentHash,
        string? SourceMap = null,
        string? SourceMapHash = null);
}

internal sealed record ManifestModuleEntry(
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Hash,
    string? SourceMapPath = null,
    string? MapHash = null);
