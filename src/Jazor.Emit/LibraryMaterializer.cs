using System.Text.Json;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Resolves binding metadata and materializes the selected local package resources.
/// External npm/JSR packages stay as bare package imports; only an explicit embedded-mjs
/// manifest contributes local files. The resolver consumes explicit manifest edges and never
/// scans a third-party package directory to infer a dependency.
/// </summary>
internal sealed class LibraryMaterializer
{
    private const int ManifestSchemaVersion = 2;
    internal const string ModuleType = "module";
    internal const string SourceMapType = "source-map";
    private const string StyleType = "style";
    private const string LicenseType = "license";
    private const string StaticType = "static";
    private const string WorkerType = "worker";

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
        var unresolvedExternalPackages = new Dictionary<string, LibraryPackageReference>(StringComparer.Ordinal);
        var selectedImports = SelectImports(
            importIndex,
            mode,
            requiredImports,
            providedModulePaths,
            unresolvedExternalPackages);
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

            // External npm/JSR manifests are package metadata. Their module bytes and package
            // styles are supplied by the package manager; a binding must never copy a historical
            // dist snapshot. Explicit non-module resources such as license/static/worker files
            // remain producer-owned and are copied through the same integrity-checked carrier.
            foreach (var file in manifest.Files.Where(file =>
                         manifest.IsEmbedded || file.Type is LicenseType or StaticType or WorkerType))
            {
                plan.Add(manifest, file, owner: "root");
            }

            if (manifest.IsEmbedded)
            {
                foreach (var style in manifest.Styles)
                {
                    plan.Add(manifest, style, owner: "style");
                    plan.AddStyle(manifest, style.Path);
                }
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
        var packageReferences = plan.ImportPaths.Keys
            .OrderBy(static specifier => specifier, StringComparer.Ordinal)
            .ToDictionary(
                static specifier => specifier,
                specifier => importIndex[specifier][0].Manifest.GetPackageReference(specifier),
                StringComparer.Ordinal);
        foreach (var reference in unresolvedExternalPackages.Values)
        {
            if (packageReferences.TryGetValue(reference.Name, out var existing) && !Equals(existing, reference))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_PACKAGE_CONFLICT",
                    $"Package '{reference.Name}' resolves to conflicting external identities.");
            }

            packageReferences[reference.Name] = reference;
        }
        var packageProjections = new Dictionary<string, LibraryPackageProjection>(StringComparer.Ordinal);
        foreach (var (specifier, owner) in plan.ModuleOwners
                     .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var reference = owner.Manifest.GetPackageReference(specifier);
            // External packages remain package specifiers. Their package.json, exports and
            // sideEffects metadata are restored by Deno and must be resolved by the consumer;
            // creating a synthetic projection here would turn `pkg/subpath` into a second,
            // binding-owned path and could bypass the upstream package contract.
            if (reference.Source is "npm" or "jsr")
                continue;

            var packageRoot = plan.GetPackageRootRelativePath(owner.Manifest, reference.Name);
            var packageTarget = plan.GetPackageExportPath(owner.Manifest, reference.Name, owner.PackageRelativePath);
            var exportName = GetPackageExportName(specifier, reference.Name, packageTarget);
            if (!packageProjections.TryGetValue(reference.Name, out var projection))
            {
                projection = new LibraryPackageProjection(reference, packageRoot);
                packageProjections.Add(reference.Name, projection);
            }
            else if (!Equals(projection.Reference, reference) ||
                     !string.Equals(projection.RootRelativePath, packageRoot, StringComparison.Ordinal))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_PACKAGE_CONFLICT",
                    $"Package '{reference.Name}' resolves to conflicting materialized providers.");
            }

            if (projection.Exports.TryGetValue(exportName, out var existingTarget) &&
                !string.Equals(existingTarget, "./" + packageTarget, StringComparison.Ordinal))
            {
                throw new LibraryException(
                    "JAZOR_LIBRARY_EXPORT_CONFLICT",
                    $"Package '{reference.Name}' export '{exportName}' resolves to conflicting targets.");
            }

            projection.Exports[exportName] = "./" + packageTarget;
        }
        return new LibraryAssets(
            new Dictionary<string, string>(plan.ImportPaths, StringComparer.Ordinal),
            new Dictionary<string, string>(plan.BrowserImportPaths, StringComparer.Ordinal),
            plan.StylePaths.ToArray(),
            plan.ExternalStyleModuleImports.ToArray(),
            plan.ExternalStylesheetPaths.ToArray(),
            plan.PublishAssets.ToArray(),
            plan.MaterializedPaths.ToArray(),
            selectedManifests.Select(static manifest => manifest.SourcePath).ToArray(),
            packageReferences,
            packageProjections);
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
                    if (manifest.Source is "npm" or "jsr")
                        continue;

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
               a.DevelopmentStyleImports.SequenceEqual(b.DevelopmentStyleImports, StringComparer.Ordinal) &&
               a.ProductionStyleImports.SequenceEqual(b.ProductionStyleImports, StringComparer.Ordinal) &&
               a.DevelopmentStylesheetImports.SequenceEqual(b.DevelopmentStylesheetImports, StringComparer.Ordinal) &&
               a.ProductionStylesheetImports.SequenceEqual(b.ProductionStylesheetImports, StringComparer.Ordinal) &&
               a.DevelopmentStyles.SequenceEqual(b.DevelopmentStyles) &&
               a.ProductionStyles.SequenceEqual(b.ProductionStyles) &&
               a.Files.SequenceEqual(b.Files);
    }

    private static IReadOnlyList<ImportSelection> SelectImports(
        IReadOnlyDictionary<string, IReadOnlyList<ImportSelection>> importsBySpecifier,
        BuildMode mode,
        IEnumerable<string>? requiredImports,
        IEnumerable<string>? providedModulePaths,
        IDictionary<string, LibraryPackageReference>? unresolvedExternalPackages)
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

        void Add(string specifier, bool packageEdge, ImportSelection? ownerSelection = null)
        {
            specifier = ValidatePackageSpecifier(specifier, "dependency");
            if (selected.ContainsKey(specifier))
                return;
            if (!importsBySpecifier.TryGetValue(specifier, out var candidates) || candidates.Count == 0)
            {
                // An npm/JSR dependency can be intentionally left to Deno's package
                // resolver. The owning manifest must still provide its package identity so the
                // generated project remains exact and auditable.
                var reference = ownerSelection?.Manifest.GetPackageReference(specifier);
                if (reference is not null && reference.Source is "npm" or "jsr")
                {
                    unresolvedExternalPackages?.TryAdd(reference.Name, reference);
                    return;
                }

                throw new LibraryException(
                    "JAZOR_LIBRARY_IMPORT_MISSING",
                    $"No library manifest provides: {specifier}.");
            }

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
                Add(dependency, packageEdge: true, ownerSelection: selection);
            foreach (var style in selection.Entry.GetStyleImports(mode))
                Add(style, packageEdge: true, ownerSelection: selection);
            foreach (var stylesheet in selection.Entry.GetStylesheetImports(mode))
                Add(stylesheet, packageEdge: true, ownerSelection: selection);

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
            if (!selection.Manifest.IsEmbedded)
            {
                // Keep the authored bare specifier intact. Deno/NetPack resolve it through the
                // restored package.json/exports graph; no local projection or import-map alias is
                // created for an external package.
                // Keep the C# logical import as the graph key, while allowing metadata to map it
                // to the exact upstream package export. This preserves authored binding paths and
                // gives NetPack/Deno the real package entry (`pkg/es/button/index.mjs`, etc.).
                plan.AddImport(
                    selection.Specifier,
                    selectedPath,
                    selection.Entry.GetBrowserPath(mode) ?? selectedPath,
                    hash: null);
                foreach (var styleSpecifier in selection.Entry.GetStyleImports(mode))
                    plan.AddExternalStyle(styleSpecifier);
                foreach (var stylesheetSpecifier in selection.Entry.GetStylesheetImports(mode))
                    plan.AddExternalStylesheet(stylesheetSpecifier);
                return;
            }

            var selectedHash = mode == BuildMode.Development
                ? selection.Entry.DevelopmentHash
                : selection.Entry.ProductionHash;
            if (string.IsNullOrWhiteSpace(selectedHash))
                throw new LibraryException(
                    "JAZOR_LIBRARY_ENTRY_HASH_MISSING",
                    $"Embedded library '{selection.Manifest.LibraryId}' entry '{selection.Specifier}' must declare a SHA-256 hash.");
            var mainFile = new ManifestFile(ModuleType, selectedPath, selectedHash, selection.Specifier);
            plan.Add(selection.Manifest, mainFile, owner: selection.Specifier);
            var targetRelativePath = plan.GetTargetRelativePath(selection.Manifest, selectedPath);
            plan.AddImport(
                selection.Specifier,
                targetRelativePath,
                targetRelativePath,
                selectedHash,
                selection.Manifest,
                selectedPath);

            foreach (var style in entry.GetStyles(mode))
            {
                plan.Add(selection.Manifest, style, owner: selection.Specifier);
                plan.AddStyle(selection.Manifest, style.Path);
            }

            foreach (var stylesheetSpecifier in entry.GetStylesheetImports(mode))
                plan.AddExternalStylesheet(stylesheetSpecifier);

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
            {
                if (providers.TryGetValue(dependency, out var provider))
                    Visit(provider);
            }

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

    private static string GetPackageExportName(string specifier, string packageName, string fallbackTarget)
    {
        if (string.Equals(specifier, packageName, StringComparison.Ordinal))
            return ".";
        if (!specifier.StartsWith(packageName + "/", StringComparison.Ordinal))
            // A typed module file can have a local moduleId (for example `shared`) that is not
            // itself a package subpath. Expose its physical package-relative path while keeping
            // the authored module id in the import map.
            return "./" + fallbackTarget;
        return "./" + specifier[(packageName.Length + 1)..];
    }

    private static bool Satisfies(string versionText, string rangeText)
    {
        if (!TryParsePackageVersion(versionText, out var version))
            throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version '{versionText}' is invalid.");

        var range = rangeText.Trim();
        if (range.Length == 0)
            throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version range '{rangeText}' is invalid.");

        var alternatives = range.Split("||", StringSplitOptions.TrimEntries);
        foreach (var alternative in alternatives)
        {
            if (!TrySatisfiesRangeClause(version, alternative, out var satisfies))
                throw new LibraryException("JAZOR_LIBRARY_VERSION_INVALID", $"Library version range '{rangeText}' is invalid.");

            if (satisfies)
                return true;
        }

        return false;
    }

    private static bool TrySatisfiesRangeClause(
        PackageVersion version,
        string clause,
        out bool satisfies)
    {
        satisfies = false;
        var tokens = clause.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
            return false;

        foreach (var token in tokens)
        {
            if (!TryEvaluateRangeToken(version, token, out var tokenSatisfies))
                return false;
            if (!tokenSatisfies)
                return true;
        }

        satisfies = true;
        return true;
    }

    private static bool TryEvaluateRangeToken(
        PackageVersion version,
        string token,
        out bool satisfies)
    {
        satisfies = false;
        if (token is "*" or "x" or "X")
        {
            satisfies = true;
            return true;
        }

        if (token.StartsWith('^') || token.StartsWith('~'))
        {
            if (!TryParsePackageVersion(token[1..], out var minimum))
                return false;

            var maximum = token[0] == '^'
                ? GetCaretUpperBound(minimum)
                : GetTildeUpperBound(minimum);
            satisfies = version.CompareTo(minimum) >= 0 && version.CompareTo(maximum) < 0;
            return true;
        }

        var operation = "=";
        var value = token;
        foreach (var candidate in new[] { ">=", "<=", ">", "<", "=" })
        {
            if (!token.StartsWith(candidate, StringComparison.Ordinal))
                continue;
            operation = candidate;
            value = token[candidate.Length..];
            break;
        }

        if (!TryParseRangeVersion(value, out var parsed, out var componentCount))
            return false;

        // A partial bare version is the npm-compatible major/minor range. Comparators use the
        // missing components as zero, which is the useful interpretation for peer constraints.
        if (operation == "=" && componentCount < 3)
        {
            var upper = componentCount == 1
                ? new PackageVersion(parsed.Major + 1, 0, 0, null)
                : new PackageVersion(parsed.Major, parsed.Minor + 1, 0, null);
            satisfies = version.CompareTo(parsed) >= 0 && version.CompareTo(upper) < 0;
            return true;
        }

        var comparison = version.CompareTo(parsed);
        satisfies = operation switch
        {
            ">=" => comparison >= 0,
            ">" => comparison > 0,
            "<=" => comparison <= 0,
            "<" => comparison < 0,
            _ => comparison == 0
        };
        return true;
    }

    private static bool TryParseRangeVersion(
        string value,
        out PackageVersion version,
        out int componentCount)
    {
        componentCount = 0;
        version = default;
        var text = value.Trim();
        if (text.Length == 0)
            return false;

        var parts = text.Split('.', StringSplitOptions.None);
        if (parts.Length is < 1 or > 3)
            return false;

        if (!parts.Any(static part => part is "*" or "x" or "X"))
        {
            if (!TryParsePackageVersion(text, out version))
                return false;
            componentCount = parts[0].Split('-', '+')[0].Length == 0
                ? 0
                : parts.Length;
            return true;
        }

        foreach (var part in parts)
        {
            if (part is "*" or "x" or "X")
                break;
            if (part.Length == 0 || !part.All(char.IsDigit))
                return false;
            componentCount++;
        }

        if (componentCount == 0)
        {
            version = new PackageVersion(0, 0, 0, null);
            return true;
        }

        var numeric = parts.Take(componentCount).Select(int.Parse).ToArray();
        version = new PackageVersion(
            numeric[0],
            numeric.Length > 1 ? numeric[1] : 0,
            numeric.Length > 2 ? numeric[2] : 0,
            null);
        return true;
    }

    private static PackageVersion GetCaretUpperBound(PackageVersion minimum)
        => minimum.Major > 0
            ? new PackageVersion(minimum.Major + 1, 0, 0, null)
            : minimum.Minor > 0
                ? new PackageVersion(0, minimum.Minor + 1, 0, null)
                : new PackageVersion(0, 0, minimum.Patch + 1, null);

    private static PackageVersion GetTildeUpperBound(PackageVersion minimum)
        => new(minimum.Major, minimum.Minor + 1, 0, null);

    private static bool TryParsePackageVersion(string value, out PackageVersion version)
    {
        version = default;
        var text = value.Trim();
        if (text.Length == 0)
            return false;

        var metadataIndex = text.IndexOf('+', StringComparison.Ordinal);
        var metadata = metadataIndex >= 0 ? text[(metadataIndex + 1)..] : null;
        if (metadataIndex >= 0)
            text = text[..metadataIndex];

        var separatorIndex = text.IndexOf('-', StringComparison.Ordinal);
        var numeric = separatorIndex >= 0 ? text[..separatorIndex] : text;
        var prerelease = separatorIndex >= 0 ? text[(separatorIndex + 1)..] : null;
        if (prerelease is not null &&
            (prerelease.Length == 0 || !prerelease.All(static character =>
                char.IsLetterOrDigit(character) || character is '.' or '-')))
            return false;
        if (metadata is not null &&
            (metadata.Length == 0 || !metadata.All(static character =>
                char.IsLetterOrDigit(character) || character is '.' or '-')))
            return false;

        var parts = numeric.Split('.');
        if (parts.Length is < 1 or > 3 || parts.Any(static part =>
                part.Length == 0 || part.Any(static character => !char.IsDigit(character)) || !int.TryParse(part, out _)))
            return false;

        var numbers = parts.Select(static part => int.Parse(part)).ToArray();
        version = new PackageVersion(
            numbers[0],
            numbers.Length > 1 ? numbers[1] : 0,
            numbers.Length > 2 ? numbers[2] : 0,
            prerelease);
        return true;
    }

    private readonly record struct PackageVersion(int Major, int Minor, int Patch, string? Prerelease)
        : IComparable<PackageVersion>
    {
        public int CompareTo(PackageVersion other)
        {
            var result = Major.CompareTo(other.Major);
            if (result != 0)
                return result;
            result = Minor.CompareTo(other.Minor);
            if (result != 0)
                return result;
            result = Patch.CompareTo(other.Patch);
            if (result != 0)
                return result;
            if (Prerelease is null)
                return other.Prerelease is null ? 0 : 1;
            if (other.Prerelease is null)
                return -1;
            return StringComparer.Ordinal.Compare(Prerelease, other.Prerelease);
        }
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
        private readonly Dictionary<string, string?> _importHashes = new(StringComparer.Ordinal);
        private readonly List<string> _stylePaths = [];
        private readonly List<string> _externalStyleModuleImports = [];
        private readonly List<string> _externalStylesheetPaths = [];
        private readonly List<LibraryPublishAsset> _publishAssets = [];

        public string DestinationRoot { get; } = destinationRoot;

        public Dictionary<string, string> ImportPaths { get; } = new(StringComparer.Ordinal);

        public Dictionary<string, string> BrowserImportPaths { get; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Owners for embedded manifest imports reached through the selected module closure.
        /// Package projection generation uses this set instead of only the package roots so
        /// cross-namespace imports such as System/* receive exact local exports.
        /// </summary>
        public Dictionary<string, MaterializedModuleOwner> ModuleOwners { get; } = new(StringComparer.Ordinal);

        public IReadOnlyList<string> StylePaths => _stylePaths;

        public IReadOnlyList<string> ExternalStyleModuleImports => _externalStyleModuleImports;

        public IReadOnlyList<string> ExternalStylesheetPaths => _externalStylesheetPaths;

        public IReadOnlyList<LibraryPublishAsset> PublishAssets => _publishAssets;

        /// <summary>
        /// The exact output closure selected by the manifest traversal. Consumers that reuse a
        /// materialization must copy this list rather than scanning the vendor directory, which
        /// would re-introduce unrelated entry points and assets into the bundle graph.
        /// </summary>
        public IReadOnlyList<string> MaterializedPaths
            => _files.Keys
                .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

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
            if (file.Type is StaticType or WorkerType or LicenseType)
                _publishAssets.Add(new LibraryPublishAsset(file.Type, targetRelativePath));
        }

        public string GetTargetRelativePath(LibraryManifest manifest, string packageRelativePath)
        {
            var packageName = manifest.GetPackageNameForPath(packageRelativePath);
            return GetPackageFilePath(manifest, packageName, packageRelativePath);
        }

        public string GetPackageRootRelativePath(LibraryManifest manifest, string? packageName = null)
        {
            return manifest.Source == "embedded-mjs" &&
                   string.Equals(manifest.LibraryId, "ecmascript", StringComparison.Ordinal)
                ? "packages/" + RequireCorePackageName(packageName)
                : "packages/" + (packageName ?? manifest.LibraryId);
        }

        public string GetPackageExportPath(
            LibraryManifest manifest,
            string packageName,
            string packageRelativePath)
        {
            var target = GetPackageFilePath(manifest, packageName, packageRelativePath);
            var root = GetPackageRootRelativePath(manifest, packageName);
            var relative = Path.GetRelativePath(
                    root.Replace('/', Path.DirectorySeparatorChar),
                    target.Replace('/', Path.DirectorySeparatorChar))
                .Replace(Path.DirectorySeparatorChar, '/');
            return NormalizePath(relative);
        }

        private string GetPackageFilePath(
            LibraryManifest manifest,
            string packageName,
            string packageRelativePath)
        {
            var normalized = NormalizePath(packageRelativePath);
            if (manifest.IsCoreSource)
            {
                if (normalized.StartsWith("clr/", StringComparison.OrdinalIgnoreCase))
                    normalized = normalized[4..];

                var packagePrefix = packageName + "/";
                if (normalized.StartsWith(packagePrefix, StringComparison.Ordinal))
                    normalized = normalized[packagePrefix.Length..];
            }

            return GetPackageRootRelativePath(manifest, packageName) + "/" + normalized;
        }

        private static string RequireCorePackageName(string? packageName)
            => string.IsNullOrWhiteSpace(packageName)
                ? throw new InvalidOperationException("The ECMAScript source package requires a namespace package identity.")
                : packageName;

        public void AddImport(
            string specifier,
            string targetRelativePath,
            string browserTarget,
            string? hash,
            LibraryManifest? ownerManifest = null,
            string? ownerPackageRelativePath = null)
        {
            if (_importHashes.TryGetValue(specifier, out var existingHash))
            {
                if (!string.Equals(existingHash, hash, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(ImportPaths[specifier], targetRelativePath, StringComparison.Ordinal) ||
                    !string.Equals(BrowserImportPaths[specifier], browserTarget, StringComparison.Ordinal))
                {
                    throw new LibraryException(
                        "JAZOR_LIBRARY_IMPORT_CONFLICT",
                        $"Logical import '{specifier}' resolves to incompatible assets.");
                }

                if (ownerManifest is not null && ownerPackageRelativePath is not null)
                {
                    if (ModuleOwners.TryGetValue(specifier, out var existingOwner) &&
                        (!ReferenceEquals(existingOwner.Manifest, ownerManifest) ||
                         !string.Equals(existingOwner.PackageRelativePath, ownerPackageRelativePath, StringComparison.Ordinal)))
                    {
                        throw new LibraryException(
                            "JAZOR_LIBRARY_IMPORT_CONFLICT",
                            $"Logical import '{specifier}' resolves to incompatible package owners.");
                    }

                    ModuleOwners[specifier] = new MaterializedModuleOwner(ownerManifest, ownerPackageRelativePath);
                }
                return;
            }

            _importHashes.Add(specifier, hash);
            ImportPaths.Add(specifier, targetRelativePath);
            BrowserImportPaths.Add(specifier, browserTarget);
            if (ownerManifest is not null && ownerPackageRelativePath is not null)
                ModuleOwners.Add(specifier, new MaterializedModuleOwner(ownerManifest, ownerPackageRelativePath));
        }

        public void AddStyle(LibraryManifest manifest, string packageRelativePath)
        {
            var target = GetTargetRelativePath(manifest, packageRelativePath);
            if (!_stylePaths.Contains(target, StringComparer.Ordinal))
                _stylePaths.Add(target);
        }

        public void AddExternalStyle(string specifier)
        {
            var normalized = ECMAScriptModulePath.ValidateExternalImportSpecifier(specifier);
            if (!ECMAScriptModulePath.IsPackageSpecifier(normalized))
                throw new LibraryException(
                    "JAZOR_LIBRARY_STYLE_INVALID",
                    $"External style '{specifier}' must be a package specifier.");
            var target = IsStylesheetSpecifier(normalized)
                ? _externalStylesheetPaths
                : _externalStyleModuleImports;
            if (!target.Contains(normalized, StringComparer.Ordinal))
                target.Add(normalized);
        }

        public void AddExternalStylesheet(string specifier)
        {
            var normalized = ECMAScriptModulePath.ValidateExternalImportSpecifier(specifier);
            if (!ECMAScriptModulePath.IsPackageSpecifier(normalized))
                throw new LibraryException(
                    "JAZOR_LIBRARY_STYLE_INVALID",
                    $"External stylesheet '{specifier}' must be a package specifier.");
            if (!_externalStylesheetPaths.Contains(normalized, StringComparer.Ordinal))
                _externalStylesheetPaths.Add(normalized);
        }

        private static bool IsStylesheetSpecifier(string specifier)
        {
            var queryStart = specifier.IndexOfAny(['?', '#']);
            var path = queryStart < 0 ? specifier : specifier[..queryStart];
            return path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".sass", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".less", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".styl", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".stylus", StringComparison.OrdinalIgnoreCase);
        }

        public void Commit()
        {
            var parent = Directory.GetParent(DestinationRoot)?.FullName
                ?? throw new InvalidOperationException($"Could not determine parent directory for '{DestinationRoot}'.");
            Directory.CreateDirectory(parent);
            var staging = Path.Combine(parent, ".jazor-library-" + Guid.NewGuid().ToString("N"));
            var stagedPayload = Path.Combine(staging, "payload");
            // `packages/` is the only binding-owned carrier in the standard project layout.
            // Legacy carriers are included in the same transaction solely so an incremental
            // Emit cannot leave stale resources visible beside the package graph. They are
            // backed up and removed; they are never recreated.
            var materializedRoots = new[] { "packages" };
            var legacyRoots = new[] { "vendor", "ecmascript", "embedded" };
            var rootsToReplace = materializedRoots.Concat(legacyRoots).ToArray();
            var backups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var movedRoots = new List<string>();

            try
            {
                Directory.CreateDirectory(stagedPayload);
                foreach (var file in _files.Values.OrderBy(static item => item.TargetRelativePath, StringComparer.OrdinalIgnoreCase))
                {
                    var stagedPath = GetSafePath(stagedPayload, file.TargetRelativePath);
                    var directory = Path.GetDirectoryName(stagedPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);
                    File.Copy(file.SourcePath, stagedPath, overwrite: false);
                    if (!string.Equals(ComputeHash(stagedPath), file.Hash, StringComparison.OrdinalIgnoreCase))
                        throw new LibraryException("JAZOR_LIBRARY_FILE_HASH_MISMATCH", $"Staged asset '{file.TargetRelativePath}' failed hash verification.");
                }

                if (!Directory.Exists(DestinationRoot))
                    Directory.CreateDirectory(DestinationRoot);
                foreach (var root in rootsToReplace)
                {
                    var stagedRoot = Path.Combine(stagedPayload, root);
                    var destinationRoot = Path.Combine(DestinationRoot, root);
                    var backupRoot = Path.Combine(parent, ".jazor-library-backup-" + Guid.NewGuid().ToString("N"));
                    if (Directory.Exists(destinationRoot))
                    {
                        DirectoryTransaction.Move(destinationRoot, backupRoot);
                        backups[destinationRoot] = backupRoot;
                    }

                    if (materializedRoots.Contains(root, StringComparer.Ordinal))
                    {
                        // Do not create an empty packages directory when this closure has no
                        // embedded files. A standard project can consist solely of restored
                        // external packages under node_modules.
                        if (Directory.Exists(stagedRoot))
                        {
                            DirectoryTransaction.Move(stagedRoot, destinationRoot);
                            movedRoots.Add(destinationRoot);
                        }
                    }
                }

                foreach (var backup in backups.Values)
                    DeleteDirectory(backup);
            }
            catch
            {
                foreach (var movedRoot in movedRoots)
                    DeleteDirectory(movedRoot);
                foreach (var backup in backups)
                {
                    if (Directory.Exists(backup.Value) && !Directory.Exists(backup.Key))
                        DirectoryTransaction.Move(backup.Value, backup.Key);
                }
                throw;
            }
            finally
            {
                DeleteDirectory(staging);
                foreach (var backup in backups.Values)
                    DeleteDirectory(backup);
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }

        private sealed record PlannedFile(string SourcePath, string TargetRelativePath, string Hash, string Owner);
    }

    private sealed record ImportSelection(LibraryManifest Manifest, string Specifier, ImportEntry Entry);
}

/// <summary>Resolved local imports, styles, and source manifests for one build.</summary>
internal sealed record LibraryAssets(
    IReadOnlyDictionary<string, string> ImportPaths,
    IReadOnlyDictionary<string, string> BrowserImportPaths,
    IReadOnlyList<string> StylePaths,
    IReadOnlyList<string> ExternalStyleModuleImports,
    IReadOnlyList<string> ExternalStylesheetPaths,
    IReadOnlyList<LibraryPublishAsset> PublishAssets,
    IReadOnlyList<string> MaterializedPaths,
    IReadOnlyList<string> ManifestPaths,
    IReadOnlyDictionary<string, LibraryPackageReference> PackageReferences,
    IReadOnlyDictionary<string, LibraryPackageProjection> PackageProjections);

/// <summary>
/// The package identity that owns a selected import. This is kept alongside the materialized
/// path so package-project generation never has to infer a version from a vendor directory.
/// </summary>
internal sealed record LibraryPackageReference(
    string Name,
    string Version,
    string Source,
    string? Integrity = null);

/// <summary>
/// The selected package exports and their materialized package root. This is the package graph
/// carrier used by NetPack and DenoHost; no consumer needs to infer it from vendor path names.
/// </summary>
internal sealed record LibraryPackageProjection(
    LibraryPackageReference Reference,
    string RootRelativePath)
{
    public SortedDictionary<string, string> Exports { get; } = new(StringComparer.Ordinal);
}

/// <summary>Manifest owner and source path for one embedded module import.</summary>
internal sealed record MaterializedModuleOwner(
    LibraryManifest Manifest,
    string PackageRelativePath);

/// <summary>A selected non-bundled library asset that must retain its package-relative URL.</summary>
internal sealed record LibraryPublishAsset(
    string Type,
    string RelativePath);

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
    string? BrowserDevelopment,
    string? BrowserProduction,
    string? DevelopmentHash,
    string? ProductionHash,
    IReadOnlyList<string> DevelopmentDependencies,
    IReadOnlyList<string> ProductionDependencies,
    IReadOnlyList<string> DevelopmentModuleDependencies,
    IReadOnlyList<string> ProductionModuleDependencies,
    IReadOnlyList<string> DevelopmentStyleImports,
    IReadOnlyList<string> ProductionStyleImports,
    IReadOnlyList<string> DevelopmentStylesheetImports,
    IReadOnlyList<string> ProductionStylesheetImports,
    IReadOnlyList<ManifestFile> DevelopmentStyles,
    IReadOnlyList<ManifestFile> ProductionStyles,
    IReadOnlyList<ManifestFile> Files)
{
    public string? GetBrowserPath(BuildMode mode)
        => mode == BuildMode.Development ? BrowserDevelopment : BrowserProduction;

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

    public IReadOnlyList<ManifestFile> GetStyles(BuildMode mode)
        => mode == BuildMode.Development ? DevelopmentStyles : ProductionStyles;

    public IReadOnlyList<string> GetStyleImports(BuildMode mode)
        => mode == BuildMode.Development ? DevelopmentStyleImports : ProductionStyleImports;

    public IReadOnlyList<string> GetStylesheetImports(BuildMode mode)
        => mode == BuildMode.Development ? DevelopmentStylesheetImports : ProductionStylesheetImports;
}

/// <summary>Validated package manifest for one browser-ready binding library.</summary>
internal sealed record LibraryManifest(
    string SourcePath,
    string LibraryId,
    string Version,
    IReadOnlyDictionary<string, ImportEntry> Imports,
    IReadOnlyDictionary<string, string> Requires,
    IReadOnlyList<ManifestFile> Styles,
    IReadOnlyList<ManifestFile> Files,
    IReadOnlyDictionary<string, LibraryPackageReference> Packages,
    string Source)
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

            var source = ReadSource(root, manifestPath);
            var imports = ReadImports(root, manifestPath, source);
            var requires = ReadRequires(root);
            var styles = ReadTypedFiles(root, "styles", "style");
            var files = ReadTypedFiles(root, "files", "license", "static", "worker");
            var packages = ReadPackages(root, version);
            if (imports.Count == 0)
                throw new InvalidOperationException($"Library manifest '{manifestPath}' does not declare an import.");

            var manifest = new LibraryManifest(
                Path.GetFullPath(manifestPath),
                libraryId,
                version,
                imports,
                requires,
                styles,
                files,
                packages,
                source);
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

    public LibraryPackageReference GetPackageReference(string specifier)
    {
        var packageName = GetPackageName(specifier);
        if (Packages.TryGetValue(packageName, out var package))
            return package;

        if (IsCoreSource)
        {
            var namespacePackage = GetCorePackageName(specifier);
            return new LibraryPackageReference(namespacePackage, Version, "embedded-mjs");
        }

        // A package without an explicit package table uses its import's package name as the
        // identity. The source still comes from the manifest; there is no legacy dist fallback.
        return new LibraryPackageReference(packageName, Version, Source);
    }

    public bool IsCoreSource
        => string.Equals(LibraryId, "ecmascript", StringComparison.Ordinal) && IsEmbedded;

    public bool IsEmbedded
        => string.Equals(Source, "embedded-mjs", StringComparison.Ordinal);

    public string GetPackageNameForPath(string path)
    {
        if (!IsCoreSource)
        {
            // An embedded package may use a scoped npm-compatible identity (for example
            // @jazor/vue-runtime) that is intentionally different from the binding library id.
            // Every materialized file must use the same identity as its package projection;
            // infer it only from an explicit embedded package declaration.
            var embeddedPackages = Packages.Values
                .Where(static package => string.Equals(package.Source, "embedded-mjs", StringComparison.Ordinal))
                .Select(static package => package.Name)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (embeddedPackages.Length == 1)
                return embeddedPackages[0];

            var importPackages = Imports.Keys
                .Select(GetPackageName)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (importPackages.Length == 1)
                return importPackages[0];

            return LibraryId;
        }

        var normalized = NormalizeManifestPath(path);
        if (normalized.StartsWith("clr/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[4..];

        var slash = normalized.IndexOf('/', StringComparison.Ordinal);
        var packageName = slash < 0 ? normalized : normalized[..slash];
        return packageName switch
        {
            "System" or "Microsoft" => packageName,
            _ => throw new LibraryException(
                "JAZOR_LIBRARY_PACKAGE_IDENTITY_INVALID",
                $"Core ECMAScript module '{path}' must begin with the System or Microsoft package namespace.")
        };
    }

    public ManifestFile? FindModule(string moduleId, BuildMode mode)
    {
        if (Imports.TryGetValue(moduleId, out var entry))
        {
            if (!IsEmbedded)
                return null;

            return mode == BuildMode.Development
                ? new ManifestFile(LibraryMaterializer.ModuleType, entry.Development, entry.DevelopmentHash!, moduleId)
                : new ManifestFile(LibraryMaterializer.ModuleType, entry.Production, entry.ProductionHash!, moduleId);
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
                new ManifestFile(LibraryMaterializer.ModuleType, entry.Development, entry.DevelopmentHash!, pair.Key),
                new ManifestFile(LibraryMaterializer.ModuleType, entry.Production, entry.ProductionHash!, pair.Key)
            }.Concat(entry.DevelopmentStyles)
             .Concat(entry.ProductionStyles)
             .Concat(entry.Files);
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
        // External npm/JSR bindings describe upstream package identities and entry specifiers;
        // their package files are supplied by Deno. Only an explicit embedded-mjs package
        // validates local files.
        if (!IsEmbedded)
            return;

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
                entry.Value.DevelopmentHash!,
                entry.Key));
            AddModuleVariant(moduleFiles, entry.Key, new ManifestFile(
                LibraryMaterializer.ModuleType,
                entry.Value.Production,
                entry.Value.ProductionHash!,
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

    private static Dictionary<string, ImportEntry> ReadImports(
        JsonElement root,
        string manifestPath,
        string? source)
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
            var development = ReadEntryPath(property.Value, "development", specifier, source);
            var production = ReadEntryPath(property.Value, "production", specifier, source);
            var browserDevelopment = ReadOptionalExternalEntryPath(property.Value, "browserDevelopment", source);
            var browserProduction = ReadOptionalExternalEntryPath(property.Value, "browserProduction", source);
            var developmentHash = ReadEntryHash(property.Value, "developmentHash", source);
            var productionHash = ReadEntryHash(property.Value, "productionHash", source);
            var entry = new ImportEntry(
                type,
                development,
                production,
                browserDevelopment,
                browserProduction,
                developmentHash,
                productionHash,
                ReadPackageDependencies(property.Value, "developmentDependencies"),
                ReadPackageDependencies(property.Value, "productionDependencies"),
                ReadModuleDependencies(property.Value, "developmentModuleDependencies"),
                ReadModuleDependencies(property.Value, "productionModuleDependencies"),
                ReadExternalStyleImports(property.Value, "developmentStyleImports"),
                ReadExternalStyleImports(property.Value, "productionStyleImports"),
                ReadExternalStyleImports(property.Value, "developmentStylesheetImports"),
                ReadExternalStyleImports(property.Value, "productionStylesheetImports"),
                ReadTypedFiles(property.Value, "developmentStyles", "style"),
                ReadTypedFiles(property.Value, "productionStyles", "style"),
                ReadTypedFiles(property.Value, "files", "module", "source-map", "static", "worker", "license"));
            imports.Add(specifier, entry);
        }
        return imports;
    }

    private static string ReadEntryPath(
        JsonElement element,
        string name,
        string fallbackSpecifier,
        string? source)
    {
        if (!TryGetProperty(element, out var property, name) || property.ValueKind == JsonValueKind.Null)
        {
            if (source is "npm" or "jsr")
                return fallbackSpecifier;
            throw new InvalidOperationException($"Library import entry must contain '{name}'.");
        }

        if (property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
            throw new InvalidOperationException($"Library import entry field '{name}' must be a non-empty string.");

        var value = property.GetString()!.Trim();
        return source is "npm" or "jsr"
            ? ECMAScriptModulePath.ValidateExternalImportSpecifier(value)
            : NormalizeManifestPath(value);
    }

    private static string? ReadOptionalExternalEntryPath(
        JsonElement element,
        string name,
        string? source)
    {
        if (!TryGetProperty(element, out var property, name) || property.ValueKind == JsonValueKind.Null)
            return null;

        if (source is not ("npm" or "jsr"))
        {
            throw new InvalidOperationException(
                $"Library import entry field '{name}' is available only for npm or JSR package entries.");
        }
        if (property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
            throw new InvalidOperationException($"Library import entry field '{name}' must be a non-empty string.");

        return ECMAScriptModulePath.ValidateExternalImportSpecifier(property.GetString()!.Trim());
    }

    private static string ReadSource(JsonElement root, string manifestPath)
    {
        if (!TryGetProperty(root, out var property, "source", "packageSource", "Source") ||
            property.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException(
                $"Library manifest '{manifestPath}' must declare source 'npm', 'jsr', or 'embedded-mjs'.");
        }

        if (property.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Library manifest '{manifestPath}' source must be a string.");

        var source = property.GetString()?.Trim();
        if (source is not ("npm" or "jsr" or "embedded-mjs"))
        {
            throw new InvalidOperationException(
                $"Library manifest '{manifestPath}' has unsupported source '{source}'.");
        }

        return source;
    }

    private static string? ReadEntryHash(JsonElement element, string name, string? source)
    {
        if (!TryGetProperty(element, out var property, name) || property.ValueKind == JsonValueKind.Null)
        {
            if (string.Equals(source, "embedded-mjs", StringComparison.Ordinal))
                throw new InvalidOperationException($"Library import entry must contain '{name}'.");
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Library import entry field '{name}' must be a string.");

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            if (string.Equals(source, "embedded-mjs", StringComparison.Ordinal))
                throw new InvalidOperationException($"Library import entry field '{name}' cannot be empty.");
            return null;
        }

        return NormalizeManifestHash(value);
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

    private static Dictionary<string, LibraryPackageReference> ReadPackages(JsonElement root, string defaultVersion)
    {
        var packages = new Dictionary<string, LibraryPackageReference>(StringComparer.Ordinal);
        if (!root.TryGetProperty("packages", out var element) || element.ValueKind == JsonValueKind.Null)
            return packages;
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Library manifest property 'packages' must be an object.");

        foreach (var property in element.EnumerateObject())
        {
            var name = GetPackageName(property.Name);
            if (!string.Equals(name, property.Name, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Library package metadata key '{property.Name}' must be a package name, not a subpath.");
            }
            if (property.Value.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Library package '{name}' must be an object.");

            var source = GetRequiredString(property.Value, "source");
            if (source is not ("npm" or "jsr" or "embedded-mjs"))
            {
                throw new InvalidOperationException(
                    $"Library package '{name}' has unsupported source '{source}'.");
            }

            var version = TryGetString(property.Value, "version") ?? defaultVersion;
            if (string.IsNullOrWhiteSpace(version))
                throw new InvalidOperationException($"Library package '{name}' must declare a version.");
            var integrity = TryGetString(property.Value, "integrity");
            if (!string.IsNullOrWhiteSpace(integrity) && !integrity.StartsWith("sha", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Library package '{name}' integrity must use a standard sha digest.");
            }

            packages.Add(name, new LibraryPackageReference(name, version, source, integrity));
        }

        return packages;
    }

    private static string GetPackageName(string specifier)
    {
        var normalized = ECMAScriptModulePath.ValidateExternalImportSpecifier(specifier);
        if (!ECMAScriptModulePath.IsPackageSpecifier(normalized))
            throw new LibraryException("JAZOR_LIBRARY_IMPORT_INVALID", $"Library package '{specifier}' must be a package specifier.");

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = normalized.StartsWith('@') ? 2 : 1;
        if (segments.Length < count || segments.Take(count).Any(static segment => string.IsNullOrWhiteSpace(segment)))
            throw new LibraryException("JAZOR_LIBRARY_IMPORT_INVALID", $"Library package '{specifier}' is invalid.");
        return string.Join('/', segments.Take(count));
    }

    private static string GetCorePackageName(string specifier)
    {
        var normalized = NormalizeManifestPath(specifier);
        var slash = normalized.IndexOf('/', StringComparison.Ordinal);
        var packageName = slash < 0 ? normalized : normalized[..slash];
        return packageName switch
        {
            "System" or "Microsoft" => packageName,
            _ => throw new LibraryException(
                "JAZOR_LIBRARY_PACKAGE_IDENTITY_INVALID",
                $"Core ECMAScript import '{specifier}' must begin with the System or Microsoft package namespace.")
        };
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

    private static IReadOnlyList<string> ReadExternalStyleImports(
        JsonElement element,
        string name)
    {
        if (!element.TryGetProperty(name, out var valuesElement) || valuesElement.ValueKind == JsonValueKind.Null)
            return [];
        if (valuesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException($"Library import entry property '{name}' must be an array.");

        var values = new List<string>();
        foreach (var item in valuesElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(item.GetString()))
                throw new InvalidOperationException($"Library import entry property '{name}' must contain non-empty strings.");
            var value = ECMAScriptModulePath.ValidateExternalImportSpecifier(item.GetString()!);
            if (!ECMAScriptModulePath.IsPackageSpecifier(value))
                throw new LibraryException("JAZOR_LIBRARY_STYLE_INVALID", $"External style '{value}' must be a package specifier.");
            values.Add(value);
        }

        return values.Distinct(StringComparer.Ordinal).OrderBy(static value => value, StringComparer.Ordinal).ToArray();
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

    private static bool TryGetProperty(
        JsonElement element,
        out JsonElement value,
        params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out value))
                return true;
        }

        value = default;
        return false;
    }

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
