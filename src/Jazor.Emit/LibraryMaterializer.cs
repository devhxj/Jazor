using System.Security.Cryptography;
using System.Text.Json;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Materializes browser-ready assets carried by binding packages. The consumer
/// contract is intentionally package-only: this code never probes node_modules or a cache.
/// </summary>
internal sealed class LibraryMaterializer
{
    public LibraryAssets Materialize(
        IEnumerable<string> manifestPaths,
        string destinationRoot,
        BuildMode mode,
        IEnumerable<string>? requiredImports = null,
        IEnumerable<string>? providedModulePaths = null)
    {
        var manifests = manifestPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(LibraryManifest.Load)
            .OrderBy(static manifest => manifest.LibraryId, StringComparer.Ordinal)
            .ThenBy(static manifest => manifest.Version, StringComparer.Ordinal)
            .ToArray();

        foreach (var providers in manifests.GroupBy(static manifest => manifest.LibraryId, StringComparer.Ordinal))
        {
            var distinctVersions = providers
                .Select(static manifest => manifest.Version)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (distinctVersions.Length > 1)
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_VERSION_CONFLICT",
                    $"Library '{providers.Key}' has conflicting versions: {string.Join(", ", distinctVersions)}.");
            }

            if (providers.Skip(1).Any())
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_PROVIDER_DUPLICATE",
                    $"Library '{providers.Key}' is provided by more than one manifest.");
            }
        }

        var providersById = manifests.ToDictionary(static manifest => manifest.LibraryId, StringComparer.Ordinal);
        foreach (var manifest in manifests)
        {
            foreach (var requirement in manifest.Requires)
            {
                if (!providersById.TryGetValue(requirement.Key, out var provider))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_PROVIDER_MISSING",
                        $"Library '{manifest.LibraryId}' requires provider '{requirement.Key}', but no matching manifest was supplied.");
                }
                if (!Satisfies(provider.Version, requirement.Value))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_VERSION_MISMATCH",
                        $"Library '{manifest.LibraryId}' requires '{requirement.Key}' version '{requirement.Value}', but '{provider.Version}' was supplied.");
                }
            }
        }

        // Providers must be copied before their consumers so stylesheet order follows the
        // dependency graph. Sibling libraries retain deterministic ordinal ordering.
        manifests = OrderByDependencies(manifests, providersById);

        var importPaths = new Dictionary<string, string>(StringComparer.Ordinal);
        var importHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        var stylePaths = new List<string>();
        var copiedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var importsBySpecifier = CreateImportIndex(manifests);
        var selectedImports = SelectImports(
            importsBySpecifier,
            mode,
            requiredImports,
            providedModulePaths);

        foreach (var manifest in manifests)
        {
            var selectedForLibrary = selectedImports
                .Where(selection => ReferenceEquals(selection.Manifest, manifest))
                .OrderBy(static selection => selection.Specifier, StringComparer.Ordinal)
                .ToArray();
            if (selectedForLibrary.Length == 0)
                continue;

            // A library's root files are shared metadata/license assets. Entry-owned files carry
            // relative ESM closure such as devtools modules, so they follow the selected entry.
            // root files 随激活库复制；entry files 只跟随实际 import，避免 browser 带入 SSR/devtools 闭包。
            foreach (var file in manifest.Files)
            {
                var sourcePath = GetSafeSourcePath(manifest.Directory, file);
                var targetRelativePath = $"vendor/{manifest.LibraryId}/{manifest.Version}/{file}";
                var targetPath = GetSafeTargetPath(destinationRoot, targetRelativePath);
                CopyFile(sourcePath, targetPath, copiedPaths);
            }

            foreach (var selection in selectedForLibrary)
            {
                var specifier = selection.Specifier;
                var entry = selection.Entry;
                var selectedPath = mode == BuildMode.Development
                    ? entry.Development
                    : entry.Production;
                var sourcePath = GetSafeSourcePath(manifest.Directory, selectedPath);
                var targetRelativePath = $"vendor/{manifest.LibraryId}/{manifest.Version}/{selectedPath}";
                var hash = ComputeHash(sourcePath);

                if (importHashes.TryGetValue(specifier, out var existingHash))
                {
                    if (!string.Equals(existingHash, hash, StringComparison.Ordinal))
                    {
                        throw new LibraryException(
                            "JAZOR_LIBRARY_IMPORT_CONFLICT",
                            $"Library import '{specifier}' is provided by incompatible package assets. " +
                            $"Keep exactly one version/provider in the restore graph.");
                    }

                    continue;
                }

                var targetPath = GetSafeTargetPath(destinationRoot, targetRelativePath);
                CopyFile(sourcePath, targetPath, copiedPaths);
                importHashes.Add(specifier, hash);
                importPaths.Add(specifier, targetRelativePath);

                foreach (var file in entry.Files)
                {
                    var fileSourcePath = GetSafeSourcePath(manifest.Directory, file);
                    var fileTargetRelativePath = $"vendor/{manifest.LibraryId}/{manifest.Version}/{file}";
                    var fileTargetPath = GetSafeTargetPath(destinationRoot, fileTargetRelativePath);
                    CopyFile(fileSourcePath, fileTargetPath, copiedPaths);
                }
            }

            foreach (var style in manifest.Styles)
            {
                var sourcePath = GetSafeSourcePath(manifest.Directory, style);
                var targetRelativePath = $"vendor/{manifest.LibraryId}/{manifest.Version}/{style}";
                var targetPath = GetSafeTargetPath(destinationRoot, targetRelativePath);
                CopyFile(sourcePath, targetPath, copiedPaths);
                stylePaths.Add(targetRelativePath);
            }
        }

        var missingImports = selectedImports
            .Select(static selection => selection.Specifier)
            .Where(specifier => !importPaths.ContainsKey(specifier))
            .OrderBy(static specifier => specifier, StringComparer.Ordinal)
            .ToArray();
        if (missingImports.Length > 0)
        {
            throw new LibraryException(
                "JAZOR_LIBRARY_IMPORT_MISSING",
                $"No library manifest provides: {string.Join(", ", missingImports)}.");
        }

        return new LibraryAssets(importPaths, stylePaths, manifests.Select(static item => item.SourcePath).ToArray());
    }

    private static Dictionary<string, ImportSelection> CreateImportIndex(IReadOnlyList<LibraryManifest> manifests)
    {
        var imports = new Dictionary<string, ImportSelection>(StringComparer.Ordinal);
        foreach (var manifest in manifests)
        {
            foreach (var (specifier, entry) in manifest.Imports)
            {
                if (imports.TryGetValue(specifier, out var existing))
                {
                    var existingPath = existing.Entry.Production;
                    if (!string.Equals(existingPath, entry.Production, StringComparison.Ordinal) ||
                        !string.Equals(existing.Manifest.LibraryId, manifest.LibraryId, StringComparison.Ordinal) ||
                        !string.Equals(existing.Manifest.Version, manifest.Version, StringComparison.Ordinal))
                    {
                        throw new LibraryException(
                            "JAZOR_LIBRARY_IMPORT_CONFLICT",
                            $"Library import '{specifier}' is provided by incompatible package assets. " +
                            "Keep exactly one version/provider in the restore graph.");
                    }

                    continue;
                }

                imports.Add(specifier, new ImportSelection(manifest, specifier, entry));
            }
        }

        return imports;
    }

    private static IReadOnlyList<ImportSelection> SelectImports(
        IReadOnlyDictionary<string, ImportSelection> importsBySpecifier,
        BuildMode mode,
        IEnumerable<string>? requiredImports,
        IEnumerable<string>? providedModulePaths)
    {
        var requested = requiredImports is null
            ? importsBySpecifier.Keys
            : requiredImports
                .Where(ECMAScriptModulePath.IsPackageSpecifier)
                // Generated application modules may use bare catalog paths (for example host/app.mjs).
                // Only paths absent from the application manifest must be supplied by a library package.
                .Where(specifier => !IsProvidedModule(specifier, providedModulePaths));
        var selected = new Dictionary<string, ImportSelection>(StringComparer.Ordinal);

        void Add(string specifier)
        {
            if (!importsBySpecifier.TryGetValue(specifier, out var selection))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_IMPORT_MISSING",
                    $"No library manifest provides: {specifier}.");
            }

            if (!selected.TryAdd(specifier, selection))
                return;

            foreach (var dependency in selection.Entry.GetDependencies(mode))
                Add(dependency);
        }

        foreach (var specifier in requested
                     .Where(static specifier => !string.IsNullOrWhiteSpace(specifier))
                     .Select(static specifier => specifier.Trim())
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(static specifier => specifier, StringComparer.Ordinal))
        {
            Add(specifier);
        }

        return selected.Values
            .OrderBy(static selection => selection.Manifest.LibraryId, StringComparer.Ordinal)
            .ThenBy(static selection => selection.Manifest.Version, StringComparer.Ordinal)
            .ThenBy(static selection => selection.Specifier, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsProvidedModule(string specifier, IEnumerable<string>? modulePaths)
    {
        if (modulePaths is null)
            return false;

        return modulePaths.Any(path =>
            string.Equals(
                path.Replace('\\', '/').TrimStart('.').TrimStart('/'),
                specifier.Replace('\\', '/').TrimStart('.').TrimStart('/'),
                StringComparison.OrdinalIgnoreCase));
    }

    private static LibraryManifest[] OrderByDependencies(
        IReadOnlyList<LibraryManifest> manifests,
        IReadOnlyDictionary<string, LibraryManifest> providers)
    {
        var ordered = new List<LibraryManifest>(manifests.Count);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        void Visit(LibraryManifest manifest)
        {
            if (visited.Contains(manifest.LibraryId))
                return;
            if (!visiting.Add(manifest.LibraryId))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_DEPENDENCY_CYCLE",
                    $"Library dependency cycle contains '{manifest.LibraryId}'.");
            }

            foreach (var requirement in manifest.Requires.Keys.OrderBy(static id => id, StringComparer.Ordinal))
                Visit(providers[requirement]);

            visiting.Remove(manifest.LibraryId);
            visited.Add(manifest.LibraryId);
            ordered.Add(manifest);
        }

        foreach (var manifest in manifests.OrderBy(static item => item.LibraryId, StringComparer.Ordinal))
            Visit(manifest);

        return ordered.ToArray();
    }

    private static bool Satisfies(string versionText, string rangeText)
    {
        if (!Version.TryParse(versionText, out var version))
            throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version '{versionText}' is invalid.");

        var range = rangeText.Trim();
        if (!range.StartsWith("^", StringComparison.Ordinal))
        {
            return Version.TryParse(range, out var exact) && version == exact;
        }

        if (!Version.TryParse(range.Substring(1), out var minimum))
            throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version range '{rangeText}' is invalid.");
        if (version < minimum)
            return false;

        var maximum = minimum.Major > 0
            ? new Version(minimum.Major + 1, 0, 0)
            : minimum.Minor > 0
                ? new Version(0, minimum.Minor + 1, 0)
                : new Version(0, 0, minimum.Build + 1);
        return version < maximum;
    }

    private static void CopyFile(string sourcePath, string targetPath, ISet<string> copiedPaths)
    {
        if (!copiedPaths.Add(targetPath))
            return;

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.Copy(sourcePath, targetPath, overwrite: true);
    }

    private static string GetSafeSourcePath(string root, string relativePath)
    {
        var candidate = GetSafeTargetPath(root, relativePath);
        if (!File.Exists(candidate))
            throw new FileNotFoundException($"Library asset was not found: '{relativePath}'.", candidate);

        return candidate;
    }

    private static string GetSafeTargetPath(string root, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            throw new InvalidOperationException($"Library asset path must be relative: '{relativePath}'.");

        var normalizedRoot = Path.EndsInDirectorySeparator(root) ? Path.GetFullPath(root) : Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Library asset path escapes its package: '{relativePath}'.");
        return candidate;
    }

    private static string ComputeHash(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}

/// <summary>Resolved local imports, styles, and source manifests for one build.</summary>
internal sealed record LibraryAssets(
    IReadOnlyDictionary<string, string> ImportPaths,
    IReadOnlyList<string> StylePaths,
    IReadOnlyList<string> ManifestPaths);

/// <summary>Development and production entry points for one logical import.</summary>
internal sealed record ImportEntry(
    string Development,
    string Production,
    IReadOnlyList<string> DevelopmentDependencies,
    IReadOnlyList<string> ProductionDependencies,
    IReadOnlyList<string> Files)
{
    public IReadOnlyList<string> GetDependencies(BuildMode mode)
        => mode == BuildMode.Development
            ? DevelopmentDependencies
            : ProductionDependencies;
}

/// <summary>Validated package manifest for one browser-ready binding library.</summary>
internal sealed record LibraryManifest(
    string SourcePath,
    string LibraryId,
    string Version,
    IReadOnlyDictionary<string, ImportEntry> Imports,
    IReadOnlyDictionary<string, string> Requires,
    IReadOnlyList<string> Styles,
    IReadOnlyList<string> Files)
{
    public string Directory => Path.GetDirectoryName(SourcePath)!;

    public static LibraryManifest Load(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Library manifest was not found.", manifestPath);

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;
        var schemaVersion = GetRequiredInt(root, "schemaVersion");
        if (schemaVersion != 1)
            throw new InvalidOperationException($"Unsupported library manifest schema version '{schemaVersion}' in '{manifestPath}'.");

        var libraryId = GetRequiredString(root, "libraryId");
        var version = GetRequiredString(root, "version");
        var imports = ReadImports(root, manifestPath);
        var requires = ReadStringMap(root, "requires");
        var styles = ReadPathValues(root, "styles");
        var files = ReadPathValues(root, "files");

        if (imports.Count == 0)
            throw new InvalidOperationException($"Library manifest '{manifestPath}' does not declare an import.");

        return new LibraryManifest(Path.GetFullPath(manifestPath), libraryId, version, imports, requires, styles, files);
    }

    private static Dictionary<string, ImportEntry> ReadImports(JsonElement root, string manifestPath)
    {
        if (!root.TryGetProperty("imports", out var importsElement) || importsElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Library manifest '{manifestPath}' must contain an imports object.");

        var imports = new Dictionary<string, ImportEntry>(StringComparer.Ordinal);
        foreach (var property in importsElement.EnumerateObject())
        {
            if (property.Name.StartsWith("npm:", StringComparison.Ordinal) ||
                property.Name.Contains("://", StringComparison.Ordinal))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_IMPORT_INVALID",
                    $"Library import '{property.Name}' must be a logical package specifier.");
            }
            if (property.Value.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Library import '{property.Name}' must define development and production paths.");

            imports.Add(
                property.Name,
                new ImportEntry(
                    GetRequiredString(property.Value, "development"),
                    GetRequiredString(property.Value, "production"),
                    ReadDependencies(property.Value, "developmentDependencies"),
                    ReadDependencies(property.Value, "productionDependencies"),
                    ReadPathValues(property.Value, "files")));
        }

        return imports;
    }

    private static IReadOnlyList<string> ReadDependencies(JsonElement element, string name)
    {
        var values = ReadPathValues(element, name);
        foreach (var value in values)
        {
            if (!ECMAScriptModulePath.IsPackageSpecifier(value))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_IMPORT_INVALID",
                    $"Library import dependency '{value}' must be a logical package specifier.");
            }
        }

        return values;
    }

    private static Dictionary<string, string> ReadStringMap(JsonElement root, string name)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!root.TryGetProperty(name, out var element))
            return values;
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Library manifest property '{name}' must be an object.");

        foreach (var property in element.EnumerateObject())
            values.Add(property.Name, property.Value.GetString() ?? string.Empty);
        return values;
    }

    private static List<string> ReadPathValues(JsonElement root, string name)
    {
        var values = new List<string>();
        if (!root.TryGetProperty(name, out var element))
            return values;

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                values.Add(item.GetString() ?? throw new InvalidOperationException($"Library manifest '{name}' must contain strings."));
            return values;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in element.EnumerateObject())
            {
                if (item.Value.ValueKind == JsonValueKind.String)
                    values.Add(item.Value.GetString()!);
                else if (item.Value.ValueKind == JsonValueKind.Array)
                    values.AddRange(item.Value.EnumerateArray().Select(static value => value.GetString() ?? throw new InvalidOperationException("Style paths must be strings.")));
                else
                    throw new InvalidOperationException($"Library manifest '{name}' values must be paths.");
            }
            return values;
        }

        throw new InvalidOperationException($"Library manifest property '{name}' must be an array or object.");
    }

    private static string GetRequiredString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidOperationException($"Library manifest is missing required string '{name}'.");
        return value.GetString()!;
    }

    private static int GetRequiredInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || !value.TryGetInt32(out var result))
            throw new InvalidOperationException($"Library manifest is missing required integer '{name}'.");
        return result;
    }
}

/// <summary>Stable library-manifest failure surfaced by debug and release lanes.</summary>
internal sealed class LibraryException(string code, string message) : InvalidOperationException(message)
{
    public string Code { get; } = code;
}

/// <summary>One selected logical import and the package manifest that owns its ESM entry.</summary>
internal sealed record ImportSelection(
    LibraryManifest Manifest,
    string Specifier,
    ImportEntry Entry);
