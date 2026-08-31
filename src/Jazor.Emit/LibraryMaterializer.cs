using System.Text.Json;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Resolves and materializes JS resource libraries from their manifest and dist files.
/// The resolver consumes only explicit manifest edges; it never parses JavaScript or scans a
/// package directory to infer a dependency. All validation happens before the owned vendor tree
/// is replaced.
/// </summary>
internal sealed class LibraryMaterializer
{
    private const int ManifestSchemaVersion = 2;
    internal const string ModuleType = "module";
    internal const string SourceMapType = "source-map";
    private const string StyleType = "style";
    private const string LicenseType = "license";
    private const string StaticType = "static";

    public LibraryAssets Materialize(
        IEnumerable<string> manifestPaths,
        string destinationRoot,
        BuildMode mode,
        IEnumerable<string>? requiredImports = null,
        IEnumerable<string>? providedModulePaths = null)
    {
        ArgumentNullException.ThrowIfNull(manifestPaths);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationRoot);

        var destination = Path.GetFullPath(destinationRoot);
        // Read manifest metadata first. File bytes and provider constraints are validated only
        // after roots have selected a dependency closure; an unrelated transitive package must
        // not make an otherwise valid build fail because it is not part of this output.
        var manifests = manifestPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(LibraryManifest.LoadMetadata)
            .ToArray();

        var importIndex = CreateImportIndex(manifests);
        var selectedImports = SelectImports(importIndex, mode, requiredImports, providedModulePaths);
        var selectedManifests = ResolveManifestClosure(selectedImports, manifests);
        var providersById = selectedManifests.ToDictionary(static manifest => manifest.LibraryId, StringComparer.Ordinal);
        selectedManifests = OrderByDependencies(selectedManifests, providersById);
        // A selected manifest is an integrity boundary: validate every declaration in that
        // package (including profile variants and associated maps) before touching the output.
        // Manifests outside this closure remain metadata-only and cannot block the build.
        foreach (var manifest in selectedManifests)
            manifest.ValidateAllFiles();

        var plan = new MaterializationPlan(destination);

        foreach (var manifest in selectedManifests)
        {
            var selected = selectedImports
                .Where(selection => ReferenceEquals(selection.Manifest, manifest))
                .OrderBy(static selection => selection.Specifier, StringComparer.Ordinal)
                .ToArray();

            foreach (var file in manifest.Files)
                plan.Add(manifest, file, owner: "root");

            foreach (var style in manifest.Styles)
            {
                plan.Add(manifest, style, owner: "style");
                plan.AddStyle(manifest, style.Path);
            }

            foreach (var selection in selected)
                AddEntryClosure(plan, selection, mode);
        }

        // The selected set is already closed over package dependencies. This check catches a
        // malformed index even when a provider was duplicated with identical bytes.
        var missing = selectedImports
            .Select(static selection => selection.Specifier)
            .Where(specifier => !plan.ImportPaths.ContainsKey(specifier))
            .OrderBy(static specifier => specifier, StringComparer.Ordinal)
            .ToArray();
        if (missing.Length > 0)
        {
            throw new LibraryException(
                "JAZOR_LIBRARY_IMPORT_MISSING",
                $"No library manifest provides: {string.Join(", ", missing)}.");
        }

        plan.Commit();
        return new LibraryAssets(
            new Dictionary<string, string>(plan.ImportPaths, StringComparer.Ordinal),
            plan.StylePaths.ToArray(),
            selectedManifests.Select(static manifest => manifest.SourcePath).ToArray());
    }

    private static void ValidateUniqueLibraries(IReadOnlyList<LibraryManifest> manifests)
    {
        foreach (var group in manifests.GroupBy(static manifest => manifest.LibraryId, StringComparer.Ordinal))
        {
            var versions = group.Select(static manifest => manifest.Version)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static version => version, StringComparer.Ordinal)
                .ToArray();
            if (versions.Length > 1)
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_VERSION_CONFLICT",
                    $"Library '{group.Key}' has conflicting versions: {string.Join(", ", versions)}.");
            }

            if (group.Skip(1).Any())
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_PROVIDER_DUPLICATE",
                    $"Library '{group.Key}' is provided by more than one manifest.");
            }
        }
    }

    private static LibraryManifest[] ResolveManifestClosure(
        IReadOnlyList<ImportSelection> selectedImports,
        IReadOnlyList<LibraryManifest> manifests)
    {
        var candidatesById = manifests
            .GroupBy(static manifest => manifest.LibraryId, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .OrderBy(static manifest => manifest.Version, StringComparer.Ordinal)
                    .ThenBy(static manifest => manifest.SourcePath, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                StringComparer.Ordinal);
        var selectedByPath = new Dictionary<string, LibraryManifest>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<LibraryManifest>();

        foreach (var selection in selectedImports)
        {
            if (selectedByPath.TryAdd(selection.Manifest.SourcePath, selection.Manifest))
                queue.Enqueue(selection.Manifest);
        }

        while (queue.Count > 0)
        {
            var manifest = queue.Dequeue();
            foreach (var requirement in manifest.Requires.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                if (!candidatesById.TryGetValue(requirement.Key, out var candidates) || candidates.Length == 0)
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_PROVIDER_MISSING",
                        $"Library '{manifest.LibraryId}' requires provider '{requirement.Key}', but no matching manifest was supplied.");
                }

                var versions = candidates
                    .Select(static candidate => candidate.Version)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(static version => version, StringComparer.Ordinal)
                    .ToArray();
                if (versions.Length > 1)
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_VERSION_CONFLICT",
                        $"Library '{requirement.Key}' has conflicting versions: {string.Join(", ", versions)}.");
                }

                if (candidates.Length > 1)
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_PROVIDER_DUPLICATE",
                        $"Library '{requirement.Key}' is provided by more than one manifest.");
                }

                var provider = candidates[0];
                if (!Satisfies(provider.Version, requirement.Value))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_VERSION_MISMATCH",
                        $"Library '{manifest.LibraryId}' requires '{requirement.Key}' version '{requirement.Value}', but '{provider.Version}' was supplied.");
                }

                if (selectedByPath.TryAdd(provider.SourcePath, provider))
                    queue.Enqueue(provider);
            }
        }

        var selected = selectedByPath.Values.ToArray();
        ValidateUniqueLibraries(selected);
        return selected;
    }

    private static Dictionary<string, IReadOnlyList<ImportSelection>> CreateImportIndex(
        IReadOnlyList<LibraryManifest> manifests)
    {
        var imports = new Dictionary<string, List<ImportSelection>>(StringComparer.Ordinal);
        foreach (var manifest in manifests)
        {
            foreach (var pair in manifest.Imports.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                var specifier = ValidatePackageSpecifier(pair.Key, "import");
                var selection = new ImportSelection(manifest, specifier, pair.Value);
                if (!imports.TryGetValue(specifier, out var candidates))
                {
                    candidates = [];
                    imports.Add(specifier, candidates);
                }

                candidates.Add(selection);
            }
        }

        return imports.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyList<ImportSelection>)pair.Value
                .OrderBy(selection => selection.Manifest.LibraryId, StringComparer.Ordinal)
                .ThenBy(selection => selection.Manifest.Version, StringComparer.Ordinal)
                .ThenBy(selection => selection.Manifest.SourcePath, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            StringComparer.Ordinal);
    }

    private static bool EquivalentEntry(ImportSelection left, ImportSelection right)
    {
        if (!string.Equals(left.Manifest.LibraryId, right.Manifest.LibraryId, StringComparison.Ordinal) ||
            !string.Equals(left.Manifest.Version, right.Manifest.Version, StringComparison.Ordinal))
        {
            return false;
        }

        var a = left.Entry;
        var b = right.Entry;
        return string.Equals(a.Type, b.Type, StringComparison.Ordinal) &&
               string.Equals(a.Development, b.Development, StringComparison.Ordinal) &&
               string.Equals(a.Production, b.Production, StringComparison.Ordinal) &&
               string.Equals(a.DevelopmentHash, b.DevelopmentHash, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(a.ProductionHash, b.ProductionHash, StringComparison.OrdinalIgnoreCase) &&
               a.DevelopmentDependencies.SequenceEqual(b.DevelopmentDependencies, StringComparer.Ordinal) &&
               a.ProductionDependencies.SequenceEqual(b.ProductionDependencies, StringComparer.Ordinal) &&
               a.DevelopmentModuleDependencies.SequenceEqual(b.DevelopmentModuleDependencies, StringComparer.Ordinal) &&
               a.ProductionModuleDependencies.SequenceEqual(b.ProductionModuleDependencies, StringComparer.Ordinal) &&
               a.Files.SequenceEqual(b.Files);
    }

    private static IReadOnlyList<ImportSelection> SelectImports(
        IReadOnlyDictionary<string, IReadOnlyList<ImportSelection>> importsBySpecifier,
        BuildMode mode,
        IEnumerable<string>? requiredImports,
        IEnumerable<string>? providedModulePaths)
    {
        var requested = requiredImports is null
            ? importsBySpecifier.Keys
            : requiredImports
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(static value => value.Trim())
                .Where(value => !IsProvidedModule(value, providedModulePaths))
                .Where(ECMAScriptModulePath.IsPackageSpecifier);

        var selected = new Dictionary<string, ImportSelection>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        ImportSelection Resolve(string specifier)
        {
            if (!importsBySpecifier.TryGetValue(specifier, out var candidates) || candidates.Count == 0)
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_IMPORT_MISSING",
                    $"No library manifest provides: {specifier}.");
            }

            var first = candidates[0];
            var distinctProviders = candidates
                .Select(static candidate => candidate.Manifest.SourcePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (distinctProviders > 1 &&
                candidates.All(candidate =>
                    string.Equals(candidate.Manifest.LibraryId, first.Manifest.LibraryId, StringComparison.Ordinal) &&
                    string.Equals(candidate.Manifest.Version, first.Manifest.Version, StringComparison.Ordinal)))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_PROVIDER_DUPLICATE",
                    $"Library '{first.Manifest.LibraryId}' is provided by more than one manifest.");
            }

            if (candidates.Skip(1).Any(candidate => !EquivalentEntry(first, candidate)))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_IMPORT_CONFLICT",
                    $"Library import '{specifier}' is provided by incompatible package assets. " +
                    "Keep exactly one version/provider in the restore graph.");
            }

            return first;
        }

        void Add(string specifier, bool packageEdge)
        {
            specifier = ValidatePackageSpecifier(specifier, "dependency");
            if (selected.ContainsKey(specifier))
                return;
            var selection = Resolve(specifier);
            if (!visiting.Add(specifier))
            {
                // A module dependency can describe an ordinary ESM cycle. Package dependency
                // cycles are a manifest/provider error and remain rejected deterministically.
                if (packageEdge)
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_IMPORT_CYCLE",
                        $"Library import dependency cycle contains '{specifier}'.");
                }

                return;
            }

            foreach (var dependency in selection.Entry.GetPackageDependencies(mode))
                Add(dependency, packageEdge: true);

            visiting.Remove(specifier);
            selected.Add(specifier, selection);
        }

        foreach (var specifier in requested
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(static value => value, StringComparer.Ordinal))
        {
            Add(specifier, packageEdge: true);
        }

        return selected.Values
            .OrderBy(static selection => selection.Manifest.LibraryId, StringComparer.Ordinal)
            .ThenBy(static selection => selection.Manifest.Version, StringComparer.Ordinal)
            .ThenBy(static selection => selection.Specifier, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddEntryClosure(
        MaterializationPlan plan,
        ImportSelection selection,
        BuildMode mode)
    {
        var identity = selection.Manifest.LibraryId + "\n" + selection.Manifest.Version + "\n" + selection.Specifier;
        // ESM graphs are allowed to contain cycles. Mark an entry before following its edges so a
        // back-edge is treated as an already selected module; package `requires` cycles are still
        // rejected separately by SelectImports because those are version/provider cycles.
        if (!plan.SelectedEntries.Add(identity))
            return;

        try
        {
            var entry = selection.Entry;
            var selectedPath = mode == BuildMode.Development ? entry.Development : entry.Production;
            var selectedHash = mode == BuildMode.Development ? entry.DevelopmentHash : entry.ProductionHash;
            var mainFile = new ManifestFile(ModuleType, selectedPath, selectedHash, selection.Specifier);
            plan.Add(selection.Manifest, mainFile, owner: selection.Specifier);
            plan.AddImport(selection.Specifier, plan.GetTargetRelativePath(selection.Manifest, selectedPath), selectedHash);

            foreach (var file in entry.Files)
                plan.Add(selection.Manifest, file, owner: selection.Specifier);

            var moduleDependencies = entry.GetModuleDependencies(mode);
            foreach (var dependency in moduleDependencies)
            {
                // A module edge is always owned by the manifest that declares it. Cross-library
                // edges use the package-dependency channel above; consulting the global import
                // index here would let an unrelated provider silently capture a local identity.
                // An imported module may itself declare module edges. Resolve it through the
                // owning manifest's import table and recurse so the complete ESM closure is
                // materialized (for example IndexModule -> RuntimeModule -> StringModule).
                if (selection.Manifest.Imports.TryGetValue(dependency, out var dependencyEntry))
                {
                    AddEntryClosure(
                        plan,
                        new ImportSelection(selection.Manifest, dependency, dependencyEntry),
                        mode);
                    continue;
                }

                var file = selection.Manifest.FindModule(dependency, mode)
                    ?? throw new LibraryException(
                        "JAZOR_LIBRARY_MODULE_DEPENDENCY_MISSING",
                        $"Library '{selection.Manifest.LibraryId}' entry '{selection.Specifier}' declares missing module dependency '{dependency}'.");
                plan.Add(selection.Manifest, file, owner: selection.Specifier);
            }
        }
        catch
        {
            // Materialize constructs a new plan per request; remove the mark so the in-memory
            // state cannot make a subsequent caller observe a failed partial traversal.
            plan.SelectedEntries.Remove(identity);
            throw;
        }
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

            foreach (var dependency in manifest.Requires.Keys.OrderBy(static id => id, StringComparer.Ordinal))
                Visit(providers[dependency]);

            visiting.Remove(manifest.LibraryId);
            visited.Add(manifest.LibraryId);
            ordered.Add(manifest);
        }

        foreach (var manifest in manifests.OrderBy(static item => item.LibraryId, StringComparer.Ordinal))
            Visit(manifest);
        return ordered.ToArray();
    }

    private static bool IsProvidedModule(string specifier, IEnumerable<string>? modulePaths)
    {
        if (modulePaths is null)
            return false;

        var normalized = NormalizeComparisonPath(specifier);
        return modulePaths.Any(path => string.Equals(normalized, NormalizeComparisonPath(path), StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeComparisonPath(string path)
        => path.Replace('\\', '/').Trim().TrimStart('.', '/');

    private static string ValidatePackageSpecifier(string value, string kind)
    {
        var specifier = ECMAScriptModulePath.ValidateExternalImportSpecifier(value);
        if (!ECMAScriptModulePath.IsPackageSpecifier(specifier))
        {
            throw new LibraryException(
                "JAZOR_LIBRARY_IMPORT_INVALID",
                $"Library {kind} '{specifier}' must be a logical package specifier.");
        }

        return specifier;
    }

    private static bool Satisfies(string versionText, string rangeText)
    {
        if (!Version.TryParse(versionText, out var version))
            throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version '{versionText}' is invalid.");

        var range = rangeText.Trim();
        if (!range.StartsWith("^", StringComparison.Ordinal))
        {
            if (!Version.TryParse(range, out var exact))
                throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version range '{rangeText}' is invalid.");
            return version == exact;
        }

        if (!Version.TryParse(range[1..], out var minimum))
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

    private static string GetSafePath(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new InvalidOperationException($"Library asset path must be relative: '{relativePath}'.");

        var normalizedRoot = EnsureDirectorySeparator(Path.GetFullPath(root));
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Library asset path escapes its package: '{relativePath}'.");
        return candidate;
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static string NormalizePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.StartsWith("/", StringComparison.Ordinal) ||
            Path.IsPathRooted(normalized))
        {
            throw new InvalidOperationException($"Manifest path must be relative: '{path}'.");
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Manifest path cannot escape its package: '{path}'.");
        return string.Join('/', segments);
    }

    private static string NormalizeHash(string hash)
        => ArtifactHash.RequireSha256(hash, "Manifest SHA-256 hash");

    private static string ComputeHash(string path)
        => ArtifactHash.ComputeSha256(File.ReadAllBytes(path));

    private sealed class MaterializationPlan(string destinationRoot)
    {
        private readonly Dictionary<string, PlannedFile> _files = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _importHashes = new(StringComparer.Ordinal);
        private readonly List<string> _stylePaths = [];

        public string DestinationRoot { get; } = destinationRoot;

        public Dictionary<string, string> ImportPaths { get; } = new(StringComparer.Ordinal);

        public IReadOnlyList<string> StylePaths => _stylePaths;

        public HashSet<string> SelectedEntries { get; } = new(StringComparer.Ordinal);

        public void Add(LibraryManifest manifest, ManifestFile file, string owner)
        {
            var sourceRelativePath = NormalizePath(file.Path);
            var sourcePath = GetSafePath(manifest.Directory, sourceRelativePath);
            if (!File.Exists(sourcePath))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_FILE_MISSING",
                    $"Library '{manifest.LibraryId}' file '{sourceRelativePath}' was not found.");
            }

            var expectedHash = NormalizeHash(file.Hash);
            var actualHash = ComputeHash(sourcePath);
            if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_FILE_HASH_MISMATCH",
                    $"Library '{manifest.LibraryId}' file '{sourceRelativePath}' hash does not match its manifest.");
            }

            var targetRelativePath = GetTargetRelativePath(manifest, sourceRelativePath);
            if (_files.TryGetValue(targetRelativePath, out var existing))
            {
                if (!string.Equals(existing.Hash, expectedHash, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existing.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_ASSET_CONFLICT",
                        $"Different package assets claim output path '{targetRelativePath}'.");
                }
                return;
            }

            _files.Add(targetRelativePath, new PlannedFile(sourcePath, targetRelativePath, expectedHash, owner));
        }

        public string GetTargetRelativePath(LibraryManifest manifest, string packageRelativePath)
        {
            var normalized = NormalizePath(packageRelativePath);
            return $"vendor/{manifest.LibraryId}/{manifest.Version}/{normalized}";
        }

        public void AddImport(string specifier, string targetRelativePath, string hash)
        {
            if (_importHashes.TryGetValue(specifier, out var existingHash))
            {
                if (!string.Equals(existingHash, hash, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(ImportPaths[specifier], targetRelativePath, StringComparison.Ordinal))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_IMPORT_CONFLICT",
                        $"Logical import '{specifier}' resolves to incompatible assets.");
                }
                return;
            }

            _importHashes.Add(specifier, hash);
            ImportPaths.Add(specifier, targetRelativePath);
        }

        public void AddStyle(LibraryManifest manifest, string packageRelativePath)
        {
            var target = GetTargetRelativePath(manifest, packageRelativePath);
            if (!_stylePaths.Contains(target, StringComparer.Ordinal))
                _stylePaths.Add(target);
        }

        public void Commit()
        {
            var parent = Directory.GetParent(DestinationRoot)?.FullName
                ?? throw new InvalidOperationException($"Could not determine parent directory for '{DestinationRoot}'.");
            Directory.CreateDirectory(parent);
            var staging = Path.Combine(parent, ".jazor-library-" + Guid.NewGuid().ToString("N"));
            var stagedVendor = Path.Combine(staging, "vendor");
            var destinationVendor = Path.Combine(DestinationRoot, "vendor");
            var backupVendor = Path.Combine(parent, ".jazor-library-backup-" + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(stagedVendor);
                foreach (var file in _files.Values.OrderBy(static item => item.TargetRelativePath, StringComparer.OrdinalIgnoreCase))
                {
                    var stagedPath = GetSafePath(staging, file.TargetRelativePath);
                    var directory = Path.GetDirectoryName(stagedPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);
                    File.Copy(file.SourcePath, stagedPath, overwrite: false);
                    if (!string.Equals(ComputeHash(stagedPath), file.Hash, StringComparison.OrdinalIgnoreCase))
                        throw new LibraryException("JAZOR_LIBRARY_FILE_HASH_MISMATCH", $"Staged asset '{file.TargetRelativePath}' failed hash verification.");
                }

                if (!Directory.Exists(DestinationRoot))
                    Directory.CreateDirectory(DestinationRoot);
                if (Directory.Exists(destinationVendor))
                    Directory.Move(destinationVendor, backupVendor);
                Directory.Move(stagedVendor, destinationVendor);
                if (Directory.Exists(backupVendor))
                    Directory.Delete(backupVendor, recursive: true);
            }
            catch
            {
                if (Directory.Exists(destinationVendor) && Directory.Exists(backupVendor))
                    Directory.Delete(destinationVendor, recursive: true);
                if (!Directory.Exists(destinationVendor) && Directory.Exists(backupVendor))
                    Directory.Move(backupVendor, destinationVendor);
                throw;
            }
            finally
            {
                if (Directory.Exists(staging))
                    Directory.Delete(staging, recursive: true);
                if (Directory.Exists(backupVendor))
                    Directory.Delete(backupVendor, recursive: true);
            }
        }

        private sealed record PlannedFile(string SourcePath, string TargetRelativePath, string Hash, string Owner);
    }

    private sealed record ImportSelection(LibraryManifest Manifest, string Specifier, ImportEntry Entry);
}

/// <summary>Resolved local imports, styles, and source manifests for one build.</summary>
internal sealed record LibraryAssets(
    IReadOnlyDictionary<string, string> ImportPaths,
    IReadOnlyList<string> StylePaths,
    IReadOnlyList<string> ManifestPaths);

/// <summary>One typed resource entry in a JS resource manifest.</summary>
internal sealed record ManifestFile(
    string Type,
    string Path,
    string Hash,
    string? ModuleId = null);

/// <summary>Development and production entry points for one logical module import.</summary>
internal sealed record ImportEntry(
    string Type,
    string Development,
    string Production,
    string DevelopmentHash,
    string ProductionHash,
    IReadOnlyList<string> DevelopmentDependencies,
    IReadOnlyList<string> ProductionDependencies,
    IReadOnlyList<string> DevelopmentModuleDependencies,
    IReadOnlyList<string> ProductionModuleDependencies,
    IReadOnlyList<ManifestFile> Files)
{
    public IReadOnlyList<string> GetPackageDependencies(BuildMode mode)
        => (mode == BuildMode.Development ? DevelopmentDependencies : ProductionDependencies)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<string> GetModuleDependencies(BuildMode mode)
        => (mode == BuildMode.Development ? DevelopmentModuleDependencies : ProductionModuleDependencies)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
}

/// <summary>Validated package manifest for one browser-ready binding library.</summary>
internal sealed record LibraryManifest(
    string SourcePath,
    string LibraryId,
    string Version,
    IReadOnlyDictionary<string, ImportEntry> Imports,
    IReadOnlyDictionary<string, string> Requires,
    IReadOnlyList<ManifestFile> Styles,
    IReadOnlyList<ManifestFile> Files)
{
    public string Directory => Path.GetDirectoryName(SourcePath)!;

    public static LibraryManifest Load(string manifestPath)
        => LoadCore(manifestPath, validateFiles: true);

    internal static LibraryManifest LoadMetadata(string manifestPath)
        => LoadCore(manifestPath, validateFiles: false);

    private static LibraryManifest LoadCore(string manifestPath, bool validateFiles)
    {
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Library manifest was not found.", manifestPath);

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var root = document.RootElement;
            var schemaVersion = GetRequiredInt(root, "schemaVersion");
            if (schemaVersion != 2)
                throw new InvalidOperationException($"Unsupported library manifest schema version '{schemaVersion}' in '{manifestPath}'.");

            var libraryId = GetRequiredString(root, "libraryId");
            var version = GetRequiredString(root, "version");
            if (libraryId.Contains('/') || libraryId.Contains('\\') || libraryId.Contains(':'))
                throw new InvalidOperationException($"Library id '{libraryId}' is not a stable package id.");

            var imports = ReadImports(root, manifestPath);
            var requires = ReadRequires(root);
            var styles = ReadTypedFiles(root, "styles", "style");
            var files = ReadTypedFiles(root, "files", "license", "static");
            if (imports.Count == 0)
                throw new InvalidOperationException($"Library manifest '{manifestPath}' does not declare an import.");

            var manifest = new LibraryManifest(
                Path.GetFullPath(manifestPath),
                libraryId,
                version,
                imports,
                requires,
                styles,
                files);
            if (validateFiles)
                manifest.ValidateAllFiles();
            return manifest;
        }
        catch (LibraryException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Library manifest '{manifestPath}' is not valid JSON: {ex.Message}", ex);
        }
    }

    public ManifestFile? FindModule(string moduleId, BuildMode mode)
    {
        if (Imports.TryGetValue(moduleId, out var entry))
        {
            return mode == BuildMode.Development
                ? new ManifestFile(LibraryMaterializer.ModuleType, entry.Development, entry.DevelopmentHash, moduleId)
                : new ManifestFile(LibraryMaterializer.ModuleType, entry.Production, entry.ProductionHash, moduleId);
        }

        return AllFiles().FirstOrDefault(file =>
            string.Equals(file.Type, LibraryMaterializer.ModuleType, StringComparison.Ordinal) &&
            string.Equals(file.ModuleId, moduleId, StringComparison.Ordinal));
    }

    private IEnumerable<ManifestFile> AllFiles()
    {
        // Main development and production files are part of the same typed file set as
        // associated modules/maps. Including both profiles here makes load-time validation
        // complete instead of deferring a missing production file until a release build.
        var entryFiles = Imports.SelectMany(pair =>
        {
            var entry = pair.Value;
            return new[]
            {
                new ManifestFile(LibraryMaterializer.ModuleType, entry.Development, entry.DevelopmentHash, pair.Key),
                new ManifestFile(LibraryMaterializer.ModuleType, entry.Production, entry.ProductionHash, pair.Key)
            }.Concat(entry.Files);
        });

        return Files.Concat(Styles).Concat(entryFiles)
            .GroupBy(static file => file.Path, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();
                if (group.Any(file => file != first))
                {
                    throw new InvalidOperationException(
                        $"Library '{LibraryId}' declares conflicting metadata for '{first.Path}'.");
                }

                return first;
            });
    }

    internal void ValidateAllFiles()
    {
        var distRoot = Path.Combine(Directory, "dist");
        if (!System.IO.Directory.Exists(distRoot))
        {
            throw new LibraryException(
                "JAZOR_LIBRARY_DIST_MISSING",
                $"Library '{LibraryId}' does not contain the required dist directory.");
        }

        var files = AllFiles().ToArray();
        foreach (var file in files)
        {
            var normalizedPath = NormalizeManifestPath(file.Path);
            var sourcePath = GetSafePath(Directory, normalizedPath);
            if (!File.Exists(sourcePath))
                throw new LibraryException("JAZOR_LIBRARY_FILE_MISSING", $"Library '{LibraryId}' file '{normalizedPath}' was not found.");
            var expected = NormalizeManifestHash(file.Hash);
            var actual = ArtifactHash.ComputeSha256(File.ReadAllBytes(sourcePath));
            if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                throw new LibraryException("JAZOR_LIBRARY_FILE_HASH_MISMATCH", $"Library '{LibraryId}' file '{normalizedPath}' hash does not match its manifest.");
        }

        ValidateModuleReferences(files);
    }

    private void ValidateModuleReferences(IReadOnlyList<ManifestFile> files)
    {
        var moduleFiles = new Dictionary<string, List<ManifestFile>>(StringComparer.Ordinal);
        foreach (var entry in Imports)
        {
            AddModuleVariant(moduleFiles, entry.Key, new ManifestFile(
                LibraryMaterializer.ModuleType,
                entry.Value.Development,
                entry.Value.DevelopmentHash,
                entry.Key));
            AddModuleVariant(moduleFiles, entry.Key, new ManifestFile(
                LibraryMaterializer.ModuleType,
                entry.Value.Production,
                entry.Value.ProductionHash,
                entry.Key));
        }

        foreach (var file in files.Where(file =>
                     string.Equals(file.Type, LibraryMaterializer.ModuleType, StringComparison.Ordinal)))
        {
            var moduleId = file.ModuleId!;
            if (moduleFiles.TryGetValue(moduleId, out var existing))
            {
                if (existing.Any(candidate => SameModuleFile(candidate, file)))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Library '{LibraryId}' declares conflicting module id '{moduleId}'.");
            }

            moduleFiles.Add(moduleId, [file]);
        }

        foreach (var sourceMap in files.Where(file =>
                     string.Equals(file.Type, LibraryMaterializer.SourceMapType, StringComparison.Ordinal)))
        {
            if (!moduleFiles.ContainsKey(sourceMap.ModuleId!))
            {
                throw new InvalidOperationException(
                    $"Library '{LibraryId}' source map '{sourceMap.Path}' references missing module id '{sourceMap.ModuleId}'.");
            }
        }

        foreach (var entry in Imports)
        {
            var dependencies = entry.Value.DevelopmentModuleDependencies
                .Concat(entry.Value.ProductionModuleDependencies)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static value => value, StringComparer.Ordinal);
            foreach (var dependency in dependencies)
            {
                if (!moduleFiles.ContainsKey(dependency))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_MODULE_DEPENDENCY_MISSING",
                        $"Library '{LibraryId}' entry '{entry.Key}' declares missing module dependency '{dependency}'.");
                }
            }
        }
    }

    private static void AddModuleVariant(
        IDictionary<string, List<ManifestFile>> moduleFiles,
        string moduleId,
        ManifestFile file)
    {
        if (!moduleFiles.TryGetValue(moduleId, out var variants))
        {
            moduleFiles.Add(moduleId, [file]);
            return;
        }

        if (!variants.Any(candidate => SameModuleFile(candidate, file)))
            variants.Add(file);
    }

    private static bool SameModuleFile(ManifestFile left, ManifestFile right)
        => string.Equals(left.Type, right.Type, StringComparison.Ordinal) &&
           string.Equals(left.Path, right.Path, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.Hash, right.Hash, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.ModuleId, right.ModuleId, StringComparison.Ordinal);

    private static Dictionary<string, ImportEntry> ReadImports(JsonElement root, string manifestPath)
    {
        if (!root.TryGetProperty("imports", out var importsElement) || importsElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Library manifest '{manifestPath}' must contain an imports object.");

        var imports = new Dictionary<string, ImportEntry>(StringComparer.Ordinal);
        foreach (var property in importsElement.EnumerateObject())
        {
            var specifier = property.Name.Trim();
            if (string.IsNullOrWhiteSpace(specifier) || !ECMAScriptModulePath.IsPackageSpecifier(specifier))
                throw new LibraryException("JAZOR_LIBRARY_IMPORT_INVALID", $"Library import '{property.Name}' must be a logical package specifier.");
            if (property.Value.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Library import '{specifier}' must define a typed module entry.");

            var type = GetRequiredString(property.Value, "type");
            if (!string.Equals(type, "module", StringComparison.Ordinal))
                throw new InvalidOperationException($"Library import '{specifier}' has unsupported type '{type}'.");
            var development = NormalizeManifestPath(GetRequiredString(property.Value, "development"));
            var production = NormalizeManifestPath(GetRequiredString(property.Value, "production"));
            var developmentHash = NormalizeManifestHash(GetRequiredString(property.Value, "developmentHash"));
            var productionHash = NormalizeManifestHash(GetRequiredString(property.Value, "productionHash"));
            var entry = new ImportEntry(
                type,
                development,
                production,
                developmentHash,
                productionHash,
                ReadPackageDependencies(property.Value, "developmentDependencies"),
                ReadPackageDependencies(property.Value, "productionDependencies"),
                ReadModuleDependencies(property.Value, "developmentModuleDependencies"),
                ReadModuleDependencies(property.Value, "productionModuleDependencies"),
                ReadTypedFiles(property.Value, "files", "module", "source-map", "static", "license"));
            imports.Add(specifier, entry);
        }
        return imports;
    }

    private static Dictionary<string, string> ReadRequires(JsonElement root)
    {
        var requires = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!root.TryGetProperty("requires", out var element) || element.ValueKind == JsonValueKind.Null)
            return requires;
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Library manifest property 'requires' must be an object.");
        foreach (var property in element.EnumerateObject())
            requires.Add(property.Name, GetJsonString(property.Value, "requires"));
        return requires;
    }

    private static IReadOnlyList<string> ReadPackageDependencies(JsonElement element, string name)
    {
        var values = ReadStringArray(element, name);
        foreach (var value in values)
        {
            if (!ECMAScriptModulePath.IsPackageSpecifier(value))
                throw new LibraryException("JAZOR_LIBRARY_IMPORT_INVALID", $"Library dependency '{value}' must be a package specifier.");
        }
        return values;
    }

    private static IReadOnlyList<string> ReadModuleDependencies(JsonElement element, string name)
    {
        var values = ReadStringArray(element, name);
        // This field is explicitly the module-edge channel. Values are package-relative paths;
        // a value may also equal another logical import key and will be resolved through the
        // manifest import index before the local-file fallback. Do not classify these strings via
        // IsPackageSpecifier: names such as `System/Foo.js` and `dist/chunk.mjs` are valid local
        // module identities even though they look like bare package names to JavaScript.
        return values.Select(NormalizeManifestPath)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ManifestFile> ReadTypedFiles(
        JsonElement element,
        string name,
        params string[] allowedTypes)
    {
        if (!element.TryGetProperty(name, out var filesElement) || filesElement.ValueKind == JsonValueKind.Null)
            return [];
        if (filesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException($"Library manifest property '{name}' must be an array of typed files.");

        var files = new List<ManifestFile>();
        foreach (var item in filesElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Library manifest '{name}' entries must be objects.");
            var type = GetRequiredString(item, "type");
            if (!allowedTypes.Contains(type, StringComparer.Ordinal))
                throw new InvalidOperationException($"Library manifest '{name}' entry type '{type}' is not allowed.");
            var path = NormalizeManifestPath(GetRequiredString(item, "path"));
            var hash = NormalizeManifestHash(GetRequiredString(item, "hash"));
            var moduleId = TryGetString(item, "moduleId");
            if (string.Equals(type, LibraryMaterializer.ModuleType, StringComparison.Ordinal) ||
                string.Equals(type, LibraryMaterializer.SourceMapType, StringComparison.Ordinal))
            {
                moduleId = NormalizeModuleId(moduleId, type, path);
            }
            else if (!string.IsNullOrWhiteSpace(moduleId))
            {
                throw new InvalidOperationException(
                    $"Library manifest '{name}' entry '{path}' type '{type}' cannot declare moduleId.");
            }
            files.Add(new ManifestFile(type, path, hash, moduleId));
        }

        return files
            .GroupBy(static file => file.Path, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();
                if (group.Any(file => file != first))
                    throw new InvalidOperationException($"Library manifest '{name}' declares conflicting file '{first.Path}'.");
                return first;
            })
            .OrderBy(static file => file.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var valuesElement) || valuesElement.ValueKind == JsonValueKind.Null)
            return [];
        if (valuesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException($"Library manifest property '{name}' must be an array.");

        return valuesElement.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String
                ? item.GetString()
                : throw new InvalidOperationException($"Library manifest '{name}' must contain strings."))
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value!.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetRequiredString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidOperationException($"Library manifest is missing required string '{name}'.");
        return value.GetString()!;
    }

    private static string? TryGetString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string GetJsonString(JsonElement value, string field)
        => value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()!
            : throw new InvalidOperationException($"Library manifest field '{field}' must be a non-empty string.");

    private static int GetRequiredInt(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
            ? result
            : throw new InvalidOperationException($"Library manifest is missing required integer '{name}'.");

    private static string NormalizeManifestPath(string value)
    {
        var normalized = value.Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.StartsWith("/", StringComparison.Ordinal) || Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"Library manifest path must be relative: '{value}'.");
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Library manifest path cannot escape package root: '{value}'.");
        return string.Join('/', segments);
    }

    private static string NormalizeManifestHash(string value)
        => ArtifactHash.RequireSha256(value, "Library manifest SHA-256 hash");

    private static string NormalizeModuleId(string? value, string type, string path)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Library manifest {type} entry '{path}' must declare moduleId.");
        }

        var normalized = value.Trim().Replace('\\', '/');
        if (normalized.StartsWith("/", StringComparison.Ordinal) ||
            Path.IsPathRooted(normalized) ||
            normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(static segment => segment == ".."))
        {
            throw new InvalidOperationException(
                $"Library manifest module id '{value}' must be a logical relative identity.");
        }

        return normalized;
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = Path.GetFullPath(root);
        var rootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Library manifest path escapes package root: '{relativePath}'.");
        return candidate;
    }
}

/// <summary>Stable library-manifest failure surfaced by debug and release lanes.</summary>
internal sealed class LibraryException(string code, string message) : InvalidOperationException(message)
{
    public string Code { get; } = code;
}
