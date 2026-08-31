using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>Persisted application manifest for generated modules, source maps, and assets.</summary>
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

    public ManifestModel(string RootAssemblyPath, DateTime GeneratedAtUtc, List<ModuleEntry> Modules)
        : this(
            CurrentSchemaVersion,
            CurrentRuntimeProtocolVersion,
            DeriveRootAssemblyName(RootAssemblyPath, Modules),
            RootAssemblyPath,
            GeneratedAtUtc,
            Modules,
            entries: null,
            assets: null)
    {
    }

    public ManifestModel(string RootAssemblyPath, List<ModuleEntry> Modules)
        : this(
            CurrentSchemaVersion,
            CurrentRuntimeProtocolVersion,
            DeriveRootAssemblyName(RootAssemblyPath, Modules),
            RootAssemblyPath,
            generatedAtUtc: null,
            Modules,
            entries: null,
            assets: null)
    {
    }

    private ManifestModel(
        int schemaVersion,
        int runtimeProtocolVersion,
        string rootAssemblyName,
        string rootAssemblyPath,
        DateTime? generatedAtUtc,
        List<ModuleEntry> modules,
        List<string>? entries,
        List<AssetEntry>? assets)
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
        Assets = NormalizeAssets(assets ?? []);
    }

    public int SchemaVersion { get; init; }

    public int RuntimeProtocolVersion { get; init; }

    public string RootAssemblyName { get; init; }

    public string RootAssemblyPath { get; init; }

    public DateTime? GeneratedAtUtc { get; init; }

    public List<string> Entries { get; init; }

    public List<ModuleEntry> Modules { get; init; }

    public List<AssetEntry> Assets { get; init; }

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
        var assets = ReadAssets(root);
        if (TryGetProperty(root, out _, "importMapEntries", "ImportMapEntries"))
        {
            throw new InvalidOperationException(
                "Jazor manifest must not contain provider import-map entries; package manifests own resource imports.");
        }

        return new ManifestModel(
            schemaVersion,
            runtimeProtocolVersion,
            rootAssemblyName,
            rootAssemblyPath,
            generatedAtUtc,
            modules,
            entries,
            assets);
    }

    public void Save(string manifestPath)
    {
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var normalizedModules = NormalizeModules(Modules);
        var normalizedEntries = NormalizeEntries(Entries, normalizedModules, RootAssemblyName);
        var normalizedAssets = NormalizeAssets(Assets);
        var fileAssets = normalizedAssets.Count == 0
            ? null
            : normalizedAssets
                .Select(static asset => new ManifestAssetFileEntry(
                    asset.SourcePath,
                    asset.ArtifactPath,
                    asset.Kind,
                    asset.Hash,
                    asset.ImportPath))
                .ToList();
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
                    module.MapHash,
                    module.PackageImports,
                    module.Dependencies,
                    ToFileHmrMetadata(module.Hmr)))
                .ToList(),
            fileAssets);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(fileModel, JsonOptions));
    }

    private static List<ModuleEntry> ReadModules(JsonElement root)
    {
        if (!TryGetProperty(root, out var modulesElement, "modules", "Modules") ||
            modulesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var modules = new List<ModuleEntry>();
        foreach (var moduleElement in modulesElement.EnumerateArray())
        {
            if (moduleElement.ValueKind != JsonValueKind.Object)
                continue;

            modules.Add(new ModuleEntry(
                ReadRequiredString(moduleElement, "assemblyName", "AssemblyName"),
                ReadRequiredString(moduleElement, "typeName", "TypeName"),
                ReadRequiredString(moduleElement, "id", "Id"),
                ReadRequiredString(moduleElement, "path", "relativePath", "RelativePath"),
                ReadRequiredString(moduleElement, "contentHash", "hash", "Hash"),
                ReadOptionalString(moduleElement, "sourceMap", "sourceMapPath", "SourceMapPath"),
                ReadOptionalString(moduleElement, "sourceMapHash", "mapHash", "MapHash"),
                ReadStringArray(moduleElement, "imports", "packageImports", "PackageImports"),
                ReadHmrMetadata(moduleElement),
                ReadStringArray(moduleElement, "dependencies", "Dependencies")));
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

    private static List<AssetEntry>? ReadAssets(JsonElement root)
    {
        if (!TryGetProperty(root, out var assetsElement, "assets", "Assets") ||
            assetsElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var assets = new List<AssetEntry>();
        foreach (var assetElement in assetsElement.EnumerateArray())
        {
            if (assetElement.ValueKind != JsonValueKind.Object)
                continue;

            assets.Add(new AssetEntry(
                ReadRequiredString(assetElement, "source", "sourcePath", "SourcePath"),
                ReadRequiredString(assetElement, "path", "artifactPath", "ArtifactPath"),
                ReadOptionalString(assetElement, "kind", "Kind") ?? AssetEntry.KindStatic,
                ReadOptionalString(assetElement, "contentHash", "hash", "Hash"),
                ReadOptionalString(assetElement, "importPath", "ImportPath")));
        }

        return assets;
    }

    private static List<ModuleEntry> NormalizeModules(IEnumerable<ModuleEntry> modules)
    {
        var normalizedModules = new List<ModuleEntry>();
        var indexByRelativePath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in modules)
        {
            var sourceMapPath = module.SourceMapPath is null ? null : NormalizeRelativePath(module.SourceMapPath);
            var mapHash = string.IsNullOrWhiteSpace(module.MapHash) ? null : module.MapHash;
            if ((sourceMapPath is null) != (mapHash is null))
            {
                throw new InvalidOperationException(
                    $"Manifest module '{module.RelativePath}' must provide sourceMap and sourceMapHash together.");
            }

            var normalizedModule = module with
            {
                RelativePath = NormalizeRelativePath(module.RelativePath),
                Hash = ArtifactHash.RequireSha256(module.Hash, $"Manifest module '{module.RelativePath}' hash"),
                SourceMapPath = sourceMapPath,
                MapHash = mapHash is null
                    ? null
                    : ArtifactHash.RequireSha256(mapHash, $"Manifest module '{module.RelativePath}' source-map hash"),
                PackageImports = NormalizePackageImports(module.PackageImports),
                Dependencies = NormalizeDependencies(module.Dependencies),
                Hmr = NormalizeHmrMetadata(module.Hmr)
            };

            if (indexByRelativePath.TryGetValue(normalizedModule.RelativePath, out var existingIndex))
            {
                var existingModule = normalizedModules[existingIndex];
                if (!Equivalent(existingModule, normalizedModule))
                {
                    throw new InvalidOperationException(
                        $"Manifest declares conflicting modules for '{normalizedModule.RelativePath}'.");
                }

                // Identical duplicate declarations are harmless and are retained only once so
                // input ordering cannot change the persisted application manifest.
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
        IReadOnlyList<ModuleEntry> modules,
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

    private static List<AssetEntry> NormalizeAssets(IEnumerable<AssetEntry> assets)
    {
        var normalizedAssets = new List<AssetEntry>();
        var indexByArtifactPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var asset in assets)
        {
            var normalizedAsset = asset with
            {
                SourcePath = NormalizeRelativePath(asset.SourcePath),
                ArtifactPath = NormalizeRelativePath(asset.ArtifactPath),
                Kind = string.IsNullOrWhiteSpace(asset.Kind) ? AssetEntry.KindStatic : asset.Kind,
                ImportPath = string.IsNullOrWhiteSpace(asset.ImportPath)
                    ? null
                    : NormalizeRelativePath(asset.ImportPath),
                Hash = string.IsNullOrWhiteSpace(asset.Hash)
                    ? null
                    : ArtifactHash.RequireSha256(asset.Hash, $"Manifest asset '{asset.ArtifactPath}' hash")
            };

            if (normalizedAsset.Kind is not (AssetEntry.KindStatic or AssetEntry.KindModuleSource))
            {
                throw new InvalidOperationException(
                    $"Unsupported artifact asset kind '{normalizedAsset.Kind}' for '{normalizedAsset.ArtifactPath}'.");
            }

            if (normalizedAsset.Kind == AssetEntry.KindModuleSource && normalizedAsset.ImportPath is null)
            {
                throw new InvalidOperationException(
                    $"Module-source asset '{normalizedAsset.ArtifactPath}' must declare ImportPath.");
            }

            if (indexByArtifactPath.TryGetValue(normalizedAsset.ArtifactPath, out var existingIndex))
            {
                var existingAsset = normalizedAssets[existingIndex];
                if (!Equivalent(existingAsset, normalizedAsset))
                {
                    throw new InvalidOperationException(
                        $"Manifest declares conflicting assets for '{normalizedAsset.ArtifactPath}'.");
                }

                // Identical duplicate declarations are harmless and are retained only once so
                // input ordering cannot change the persisted application manifest.
                continue;
            }

            indexByArtifactPath.Add(normalizedAsset.ArtifactPath, normalizedAssets.Count);
            normalizedAssets.Add(normalizedAsset);
        }

        return normalizedAssets
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string>? NormalizePackageImports(IEnumerable<string>? imports)
    {
        var normalized = imports?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray() ?? [];
        return normalized.Length == 0 ? null : normalized;
    }

    private static IReadOnlyList<string>? NormalizeDependencies(IEnumerable<string>? dependencies)
    {
        var normalized = dependencies?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizeRelativePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
        return normalized.Length == 0 ? null : normalized;
    }

    private static HmrMetadata? NormalizeHmrMetadata(HmrMetadata? hmr)
        => hmr is null
            ? null
            : HmrMetadata.Create(
                hmr.ProviderId,
                hmr.ModuleId,
                hmr.Payload);

    private static bool Equivalent(ModuleEntry left, ModuleEntry right)
        => StringComparer.Ordinal.Equals(left.AssemblyName, right.AssemblyName) &&
           StringComparer.Ordinal.Equals(left.TypeName, right.TypeName) &&
           StringComparer.Ordinal.Equals(left.Id, right.Id) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.RelativePath, right.RelativePath) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.Hash, right.Hash) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.SourceMapPath, right.SourceMapPath) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.MapHash, right.MapHash) &&
           (left.PackageImports ?? []).SequenceEqual(right.PackageImports ?? [], StringComparer.Ordinal) &&
           (left.Dependencies ?? []).SequenceEqual(right.Dependencies ?? [], StringComparer.OrdinalIgnoreCase) &&
           Equals(left.Hmr, right.Hmr);

    private static bool Equivalent(AssetEntry left, AssetEntry right)
        => StringComparer.Ordinal.Equals(left.SourcePath, right.SourcePath) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.ArtifactPath, right.ArtifactPath) &&
           StringComparer.Ordinal.Equals(left.Kind, right.Kind) &&
           StringComparer.OrdinalIgnoreCase.Equals(left.Hash, right.Hash) &&
           StringComparer.Ordinal.Equals(left.ImportPath, right.ImportPath);

    private static string NormalizeRelativePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new InvalidOperationException("Manifest relative path cannot be empty.");

        var normalized = relativePath.Replace('\\', '/').Trim();

        if (normalized.StartsWith('/', StringComparison.Ordinal) ||
            Path.IsPathRooted(normalized) ||
            (normalized.Length > 1 && char.IsLetter(normalized[0]) && normalized[1] == ':'))
            throw new InvalidOperationException($"Manifest relative path must be relative: '{relativePath}'.");

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Manifest relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }

    private static string DeriveRootAssemblyName(string? rootAssemblyPath, IReadOnlyList<ModuleEntry>? modules)
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

    private static IReadOnlyList<string>? ReadStringArray(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var array, names) || array.ValueKind != JsonValueKind.Array)
            return null;

        var values = array.EnumerateArray()
            .Where(static item => item.ValueKind == JsonValueKind.String)
            .Select(static item => item.GetString())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .ToArray();
        return values.Length == 0 ? null : values;
    }

    private static HmrMetadata? ReadHmrMetadata(JsonElement moduleElement)
    {
        if (!TryGetProperty(moduleElement, out var hmrElement, "hmr", "Hmr") ||
            hmrElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (hmrElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Manifest HMR metadata must be an object.");

        if (!TryGetProperty(hmrElement, out var payload, "data", "Data") ||
            payload.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Manifest HMR metadata data must be an object.");
        }

        return HmrMetadata.Create(
            ReadRequiredString(hmrElement, "providerId", "ProviderId"),
            ReadRequiredString(hmrElement, "moduleId", "ModuleId"),
            payload.GetRawText());
    }

    private static ManifestHmrFileEntry? ToFileHmrMetadata(HmrMetadata? hmr)
    {
        if (hmr is null)
            return null;

        using var document = JsonDocument.Parse(hmr.Payload);
        return new ManifestHmrFileEntry(
            hmr.ProviderId,
            hmr.ModuleId,
            document.RootElement.Clone());
    }

    private sealed record ManifestFileModel(
        int SchemaVersion,
        int RuntimeProtocolVersion,
        string RootAssemblyName,
        List<string> Entries,
        List<ManifestModuleFileEntry> Modules,
        List<ManifestAssetFileEntry>? Assets);

    private sealed record ManifestModuleFileEntry(
        string AssemblyName,
        string TypeName,
        string Id,
        string Path,
        string ContentHash,
        string? SourceMap = null,
        string? SourceMapHash = null,
        IReadOnlyList<string>? Imports = null,
        IReadOnlyList<string>? Dependencies = null,
        ManifestHmrFileEntry? Hmr = null);

    private sealed record ManifestHmrFileEntry(
        string ProviderId,
        string ModuleId,
        JsonElement Data);

    private sealed record ManifestAssetFileEntry(
        string Source,
        string Path,
        string Kind,
        string? ContentHash = null,
        string? ImportPath = null);

}

/// <summary>One generated module persisted in the application manifest.</summary>
internal sealed record ModuleEntry(
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Hash,
    string? SourceMapPath = null,
    string? MapHash = null,
    IReadOnlyList<string>? PackageImports = null,
    HmrMetadata? Hmr = null,
    IReadOnlyList<string>? Dependencies = null);

/// <summary>One source-controlled asset copied to the application artifact root.</summary>
internal sealed record AssetEntry(
    string SourcePath,
    string ArtifactPath,
    string Kind,
    string? Hash = null,
    string? ImportPath = null)
{
    public const string KindStatic = "static";
    public const string KindModuleSource = "module-source";
}
