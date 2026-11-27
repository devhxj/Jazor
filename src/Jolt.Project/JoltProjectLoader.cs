using System.Collections.Immutable;
using Jazor.Npm;
using System.Text.Json;
using Jolt.Compiler;
using Jolt.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Jolt.Project;

public sealed record JoltWorkspace(
    IReadOnlyList<JoltSource> Sources,
    JoltCompileOptions Options,
    IReadOnlyList<Diagnostic> Diagnostics,
    IReadOnlyList<string> Packages,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Exports)
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ValueExports { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
    public IReadOnlyList<string> VuePaths { get; init; } = [];
    public IReadOnlyDictionary<string, string> VueSources { get; init; } = new Dictionary<string, string>();
    public bool VueUsesJolt { get; init; }
    internal CSharpSourceSelection? CSharpSelection { get; init; }
}

internal sealed record CSharpSourceSelection(string Root, HashSet<string> ExplicitPaths, Matcher? Matcher)
{
    internal CSharpSourceSelection? Additional { get; init; }

    internal bool Includes(string path)
    {
        var relative = Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/');
        return Additional?.Includes(path) == true || ExplicitPaths.Contains(relative) || JoltProjectLoader.IsProjectRelative(relative) && (Matcher is not null &&
            Matcher.Match(Root, [relative]).HasMatches &&
            JoltProjectLoader.IsCSharpDiscoveryPath(Root, relative));
    }
}

internal sealed class JoltNpmLoadProgress
{
    internal TaskCompletionSource<IReadOnlyDictionary<string, IReadOnlyList<string>>> Catalog { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal TaskCompletionSource<IReadOnlyList<NpmVueComponent>> Components { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public static partial class JoltProjectLoader
{
    private static readonly NpmVueMetadataProvider Metadata = new();
    private static readonly DiagnosticDescriptor ImportError = new("JOLT1001", "npm import", "{0}", "Jolt", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor VueError = new("JOLT1040", "Vue component", "{0}", "Jolt", DiagnosticSeverity.Error, true);
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
        { "node_modules", "bin", "obj", "dist" };

    public static Task<JoltWorkspace> LoadAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays = null, CancellationToken cancellationToken = default)
        => LoadAsync(projectDirectory, Metadata, overlays, cancellationToken);

    internal static NpmVueMetadataProvider CreateDevMetadataProvider(Action<IReadOnlyDictionary<string, string?>> observe)
        => new() { ObserveQueryInputs = observe };

    internal static async Task<JoltWorkspace> LoadAsync(string projectDirectory, NpmVueMetadataProvider provider,
        IReadOnlyDictionary<string, string>? overlays = null, CancellationToken cancellationToken = default,
        IReadOnlyList<JoltSource>? previousSources = null)
    {
        var workspace = await LoadSourcesAsync(projectDirectory, overlays, cancellationToken, previousSources,
            evaluateDotNetProject: !File.Exists(Path.Combine(projectDirectory, ".jazor", "project.inputs")));
        return await LoadNpmAsync(projectDirectory, workspace, provider, cancellationToken);
    }

    public static Task<JoltWorkspace> LoadSourcesAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays, CancellationToken cancellationToken)
        => LoadSourcesAsync(projectDirectory, overlays, cancellationToken, null);

    internal static async Task<JoltWorkspace> LoadSourcesAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays, CancellationToken cancellationToken, IReadOnlyList<JoltSource>? previousSources,
        bool evaluateDotNetProject = false)
    {
        var root = Path.GetFullPath(projectDirectory);
        var dotnet = await ReadDotNetProjectAsync(root, overlays, evaluateDotNetProject, cancellationToken);
        var sources = new SortedDictionary<string, JoltSource>(StringComparer.Ordinal);
        var previous = previousSources?.ToDictionary(source => source.Path, StringComparer.Ordinal);
        var vuePaths = new SortedSet<string>(StringComparer.Ordinal);
        var vueSources = new Dictionary<string, string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var path in Discover(root, excludedDirectories: dotnet?.ExcludedDirectories))
        {
            var name = Relative(path);
            if (name.EndsWith(".vue", StringComparison.Ordinal))
            {
                vuePaths.Add(name);
                continue;
            }
            if (overlays?.ContainsKey(path) == true)
                continue;
            try
            {
                sources[name] = Current(name, await File.ReadAllTextAsync(path, cancellationToken));
            }
            catch (FileNotFoundException)
            {
                // A rename can remove the enumerated path before the editor reports the new one.
            }
        }
        if (overlays != null)
            foreach (var (path, content) in overlays)
            {
                var absolute = Path.GetFullPath(path, root);
                if (IsExcluded(absolute, dotnet?.ExcludedDirectories))
                    continue;
                var name = Relative(absolute);
                if (name.StartsWith("../", StringComparison.Ordinal) || Path.IsPathRooted(name))
                    continue;
                if (Path.GetExtension(name) is ".jazor")
                    sources[name] = Current(name, content);
                else if (Path.GetExtension(name) is ".vue" or ".ts" or ".tsx" or ".mts" or ".cts" or ".js" or ".jsx" or ".mjs")
                {
                    vueSources[absolute] = content;
                    if (name.EndsWith(".vue", StringComparison.Ordinal))
                        vuePaths.Add(name);
                }
            }

        var manifest = Path.Combine(root, "package.json");
        var packages = new SortedSet<string>(StringComparer.Ordinal);
        var rootNamespace = "App";
        bool hasRootNamespace = false;
        IReadOnlyList<JoltSource> csharpSources = [];
        CSharpSourceSelection? csharpSelection = null;
        bool router = false;
        if (File.Exists(manifest))
        {
            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(manifest, cancellationToken));
            var project = json.RootElement;
            if (project.TryGetProperty("jolt", out var options))
            {
                if (options.TryGetProperty("rootNamespace", out var ns))
                {
                    rootNamespace = ns.GetString() ?? rootNamespace;
                    hasRootNamespace = ns.ValueKind != JsonValueKind.Null;
                }
                router = options.TryGetProperty("router", out var routing) && routing.ValueKind == JsonValueKind.True;
                if (options.TryGetProperty("csharpSources", out var configuredSources))
                {
                    csharpSelection = ParseCSharpSelection(root, configuredSources);
                    csharpSources = await LoadCSharpSourcesAsync(root, csharpSelection, overlays, cancellationToken, dotnet?.ExcludedDirectories);
                }
            }
            foreach (var group in new[] { "dependencies", "devDependencies", "peerDependencies", "optionalDependencies" })
                if (project.TryGetProperty(group, out var deps))
                    foreach (var dep in deps.EnumerateObject())
                        packages.Add(dep.Name);
        }

        if (dotnet is not null && !hasRootNamespace)
            rootNamespace = dotnet.Namespace;
        return new(sources.Values.ToArray(), new(rootNamespace, References: dotnet?.References) {
            CSharpSources = csharpSources,
            ProjectSources = dotnet?.Sources.ExceptBy(csharpSources.Select(source => source.Path), source => source.Path, DotNetPaths).ToArray() ?? [],
            ProjectDefines = dotnet?.Defines ?? [],
            Router = router,
        }, [], packages.ToArray(),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal))
        {
            CSharpSelection = csharpSelection is null ? dotnet?.Selection : csharpSelection with { Additional = dotnet?.Selection },
            VuePaths = vuePaths.ToArray(),
            VueSources = vueSources,
        };

        string Relative(string path) => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

        // A retained source owns its Razor parse. Compare the actual text so a
        // save with preserved timestamps or an unsaved editor overlay is observed.
        JoltSource Current(string path, string content) =>
            previous?.GetValueOrDefault(path) is { } last && last.Content == content ? last : new(path, content);
    }

    private static CSharpSourceSelection ParseCSharpSelection(string root, JsonElement entries)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var comparer = StringComparer.FromComparison(comparison);
        var explicitPaths = new HashSet<string>(comparer);
        var matcher = new Matcher(comparison);
        var hasPatterns = false;
        foreach (var item in entries.EnumerateArray())
        {
            var configured = item.GetString()?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(configured) || Path.IsPathRooted(configured) || configured.Contains(':') ||
                !configured.EndsWith(".cs", StringComparison.Ordinal))
                throw InvalidSource(configured);
            if (configured.Contains('*'))
            {
                if (configured.Split('/').Contains("..", StringComparer.Ordinal))
                    throw InvalidSource(configured);
                matcher.AddInclude(configured);
                hasPatterns = true;
                continue;
            }

            var relative = Path.GetRelativePath(root, Path.GetFullPath(configured, root)).Replace(Path.DirectorySeparatorChar, '/');
            if (!IsProjectRelative(relative))
                throw InvalidSource(configured);
            explicitPaths.Add(relative);
        }

        return new(root, explicitPaths, hasPatterns ? matcher : null);
    }

    private static async Task<JoltSource[]> LoadCSharpSourcesAsync(string root, CSharpSourceSelection selection,
        IReadOnlyDictionary<string, string>? overlays, CancellationToken cancellationToken,
        IReadOnlySet<string>? excludedDirectories)
    {
        var (explicitPaths, matcher) = (selection.ExplicitPaths, selection.Matcher);
        var comparer = explicitPaths.Comparer;
        var currentSources = new Dictionary<string, string>(comparer);
        if (overlays is not null)
            foreach (var (path, text) in overlays)
            {
                var relative = Relative(Path.GetFullPath(path, root));
                if (IsProjectRelative(relative) && relative.EndsWith(".cs", StringComparison.Ordinal))
                    currentSources[relative] = text;
            }

        var candidates = new HashSet<string>(comparer);
        if (matcher is not null)
        {
            foreach (var file in Discover(root, csharp: true, excludedDirectories: excludedDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                candidates.Add(Relative(file));
            }
            foreach (var relative in currentSources.Keys)
                if (IsCSharpDiscoveryPath(root, relative) && !IsExcluded(Path.GetFullPath(relative, root), excludedDirectories))
                    candidates.Add(relative);
        }

        // A literal keeps its missing-file error even when it also matches an include pattern.
        var paths = new HashSet<string>(explicitPaths, comparer);
        if (matcher is not null)
            paths.UnionWith(matcher.Match(root, candidates).Files.Select(file => file.Path));
        var sources = new List<JoltSource>(paths.Count);
        foreach (var relative in paths.Order(StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(relative, root);
            try
            {
                var content = currentSources.TryGetValue(relative, out var current) ? current :
                    await File.ReadAllTextAsync(path, cancellationToken);
                sources.Add(new(path, content));
            }
            catch (IOException error) when (!explicitPaths.Contains(relative) && error is FileNotFoundException or DirectoryNotFoundException)
            {
                // Glob results can disappear while the editor is reporting a rename.
            }
        }
        return sources.ToArray();

        string Relative(string path) => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    internal static bool IsProjectRelative(string path) =>
        path != ".." && !path.StartsWith("../", StringComparison.Ordinal) && !Path.IsPathRooted(path);

    private static InvalidDataException InvalidSource(string? configured) =>
        new($"Invalid jolt.csharpSources entry '{configured}'. Expected a project-relative .cs file or glob.");

    internal static bool IsCSharpDiscoveryPath(string root, string relative)
    {
        var parts = relative.Split('/');
        var path = root;
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            if (part.StartsWith('.') || (index < parts.Length - 1 && Excluded.Contains(part)))
                return false;
            path = Path.Combine(path, part);
            var attributes = new FileInfo(path).Attributes;
            if (attributes != (FileAttributes)(-1) &&
                (attributes & (FileAttributes.Hidden | FileAttributes.ReparsePoint)) != 0)
                return false;
        }
        return true;
    }

    public static Task<JoltWorkspace> LoadNpmAsync(string projectDirectory, JoltWorkspace workspace, CancellationToken cancellationToken)
        => LoadNpmAsync(projectDirectory, workspace, Metadata, cancellationToken);

    internal static async Task<JoltWorkspace> LoadNpmAsync(string projectDirectory, JoltWorkspace workspace,
        NpmVueMetadataProvider provider, CancellationToken cancellationToken,
        JoltNpmLoadProgress? progress = null)
    {
        var root = Path.GetFullPath(projectDirectory);
        var validation = new NpmValueMetadataProvider.Validation();
        var exports = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var valueNames = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var metadata = new Dictionary<(string Module, string Export), NpmVueComponent>();
        var values = new Dictionary<(string Module, string Export), NpmValueExport>();
        var diagnostics = new List<Diagnostic>();
        var projectImports = JoltProjectImports.Parse(workspace.Sources, cancellationToken);
        var modules = new Dictionary<JoltImport, string?>();
        var npmProjects = new Dictionary<string, string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var componentRequests = new List<NpmComponentRequest>();
        var valueImports = new List<(JoltDocument Document, JoltImport Import, string ImportPath)>();
        var valueAliases = new Dictionary<string, Dictionary<(string Alias, string Namespace), (string Module, string ImportPath)>>(StringComparer.Ordinal);
        foreach (var document in projectImports.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, JoltElement>? firstTags = null;
            foreach (var import in projectImports.ImportsFor(document).Where(import => import.IsNpm).DistinctBy(import => import.Code))
            {
                var importPath = Path.Combine(root, import.Document.Path);
                if (import.IsNpmValue)
                {
                    valueImports.Add((document, import, importPath));
                    continue;
                }
                if (!modules.TryGetValue(import, out var module))
                    modules[import] = module = await ResolveImportAsync(import, importPath);
                if (module == null)
                    continue;
                if (JoltProjectImports.IsImportsFile(document.Path))
                    continue;
                // Keep the first authored location for each tag and preserve package export order.
                firstTags ??= JoltTree.Elements(document).Where(e => e.Name.Length != 0 && char.IsUpper(e.Name[0]))
                    .DistinctBy(e => e.Name, StringComparer.Ordinal).ToDictionary(e => e.Name, StringComparer.Ordinal);
                var importDirectory = Path.GetDirectoryName(importPath)!;
                foreach (var export in exports[module])
                {
                    if (!firstTags.TryGetValue(NpmNames.TagFor(module, export), out var site))
                        continue;
                    if (!npmProjects.TryGetValue(importDirectory, out var npmProject))
                        try
                        {
                            npmProjects[importDirectory] = npmProject = NpmVueMetadataProvider.FindProject(importPath);
                        }
                        catch (FileNotFoundException ex)
                        {
                            ReportComponentError(document, site.NameSpan, module, export, ex.Message);
                            continue;
                        }
                    componentRequests.Add(new(npmProject, module, export, importPath, document, site.NameSpan));
                }
            }
        }

        // The names are complete at this point. Editor tag completion can use the
        // same catalog while individual component contracts are still loading.
        progress?.Catalog.TrySetResult(exports);

        // A batch shares the installed declaration graph. Apply its results in source
        // order so another workspace can supply a contract after an earlier failure.
        var requestsByProjectModule = componentRequests.ToLookup(request => (request.Project, request.Module));
        var componentBatches = new Dictionary<(string Project, string Module), IReadOnlyDictionary<string, NpmVueComponentResult>>();
        foreach (var request in componentRequests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (project, module, export, importPath, document, span) = request;
            if (metadata.ContainsKey((module, export)))
                continue;
            var key = (project, module);
            if (!componentBatches.TryGetValue(key, out var batch))
            {
                var names = requestsByProjectModule[key].Select(site => site.Export).Distinct(StringComparer.Ordinal)
                    .Where(name => !metadata.ContainsKey((module, name))).ToArray();
                try
                {
                    batch = await provider.ReadComponentsAsync(importPath, module, names, validation, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    batch = names.ToDictionary(name => name, _ => new NpmVueComponentResult(null, ex.Message), StringComparer.Ordinal);
                }
                componentBatches.Add(key, batch);
            }
            var result = batch[export];
            if (result.Component is { } component)
                metadata.Add((module, export), component);
            else
                ReportComponentError(document, span, module, export, result.Error!);
        }
        IReadOnlyList<NpmVueComponent> components = metadata.Values.ToImmutableArray();
        progress?.Components.TrySetResult(components);
        var componentOptions = workspace.Options with { NpmComponents = components };
        var (vueComponents, vueUsesJolt) = await LoadVueAsync(workspace with { Options = componentOptions }, projectImports, provider, root,
            diagnostics, validation, cancellationToken);
        componentOptions = componentOptions with { VueComponents = vueComponents };
        foreach (var (document, import, importPath) in valueImports)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!modules.TryGetValue(import, out var module))
                modules[import] = module = await ResolveImportAsync(import, importPath);
            if (module is null || JoltProjectImports.IsImportsFile(document.Path))
                continue;
            if (!valueAliases.TryGetValue(document.Path, out var aliases))
                valueAliases[document.Path] = aliases = new();
            aliases[(import.Alias!, import.NpmNamespace!)] = (module, importPath);
        }
        if (valueAliases.Count != 0)
        {
            var namesByModule = valueNames.ToDictionary(pair => pair.Key,
                pair => pair.Value.ToLookup(NpmNames.ValueNameFor, StringComparer.Ordinal), StringComparer.Ordinal);
            // Bind through the actual C# projection so a local/parameter can shadow
            // an alias and aliases in scoped slots see the real component contracts.
            var preliminary = JoltProjectCompiler.ProjectDocumentsForNpm(workspace.Sources,
                componentOptions, valueAliases.Keys.ToHashSet(StringComparer.Ordinal), cancellationToken);
            var requests = new List<(string Module, string Export, string ImportPath, Location Site, string? Variant)>();
            foreach (var projected in preliminary)
            {
                if (!valueAliases.TryGetValue(projected.Document.Path, out var aliases))
                    continue;
                var model = projected.SemanticModel;
                var seen = new HashSet<TextSpan>();
                var lines = SourceText.From(projected.Document.Source).Lines;
                foreach (var access in model.SyntaxTree.GetRoot(cancellationToken).DescendantNodes().OfType<MemberAccessExpressionSyntax>())
                {
                    if (access.Expression is not IdentifierNameSyntax receiver || access.Name.IsMissing ||
                        model.GetAliasInfo(receiver, cancellationToken)?.Target is not INamedTypeSymbol { Name: "Exports" } type ||
                        !aliases.TryGetValue((receiver.Identifier.ValueText, type.ContainingNamespace.ToDisplayString()), out var import))
                        continue;
                    var name = access.Name.Identifier;
                    var selection = projected.Spans.FirstOrDefault(span => span.Generated.Contains(name.Span));
                    if (selection is null)
                        continue;
                    var sourceSpan = new TextSpan(selection.Source.Start + name.SpanStart - selection.Generated.Start, name.Span.Length);
                    if (!seen.Add(sourceSpan))
                        continue;
                    var moduleNames = namesByModule[import.Module];
                    var matches = NpmExportSelection.FindMembers(moduleNames, name.ValueText);
                    var site = Location.Create(projected.Document.Path, sourceSpan, lines.GetLinePositionSpan(sourceSpan));
                    if (matches.Count > 1 && matches[0].Suffix is null)
                        diagnostics.Add(Diagnostic.Create(ImportError, site,
                            $"'{name.ValueText}' matches multiple exports of '{import.Module}': {string.Join(", ", matches.Select(match => match.Export))}."));
                    else
                        requests.AddRange(matches.Select(match => (import.Module, match.Export, import.ImportPath, site, match.Suffix)));
                }
            }
            // Resolve all selected exports against one snapshot per module. Import
            // locations come from @using (including inherited _Imports.jazor files).
            var sitesByExport = requests.ToLookup(request => (request.Module, request.Export));
            var byDirectory = sitesByExport.Select(sites => sites.First()).GroupBy(request =>
                (request.Module, Directory: Path.GetDirectoryName(request.ImportPath)));
            foreach (var group in byDirectory)
            {
                var module = group.Key.Module;
                var names = group.Select(request => request.Export).ToArray();
                NpmValueProjections projections;
                try
                {
                    projections = await provider.ReadValueProjectionsAsync(group.First().ImportPath, module, names, validation, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    projections = new([], names.ToDictionary(name => name, _ => ex.Message, StringComparer.Ordinal));
                }
                foreach (var value in projections.Values)
                    if (sitesByExport[(module, value.ExportName)].Any(site => NpmExportSelection.Matches(value, site.Variant)))
                        values.Add((module, value.ExportName), value);
                foreach (var (export, error) in projections.Errors)
                    foreach (var request in sitesByExport[(module, export)].Where(site => site.Variant is null))
                        diagnostics.Add(Diagnostic.Create(ImportError, request.Site, $"{module} ({export}): {error}"));
            }
        }
        return workspace with
        {
            Options = componentOptions with { NpmValues = values.Values.ToArray() },
            Diagnostics = diagnostics,
            Exports = exports,
            ValueExports = valueNames,
            VueUsesJolt = vueUsesJolt,
        };

        void ReportComponentError(JoltDocument document, TextSpan span, string module, string export, string error)
        {
            var lines = SourceText.From(document.Source).Lines;
            var location = Location.Create(document.Path, span, lines.GetLinePositionSpan(span));
            diagnostics.Add(Diagnostic.Create(ImportError, location, $"{module} ({export}): {error}"));
        }

        async Task<string?> ResolveImportAsync(JoltImport import, string importPath)
        {
            var target = import.NpmNamespace!;
            string message;
            try
            {
                var matches = await NpmExportSelection.FindModulesAsync(target, workspace.Packages,
                    package => provider.ListSpecifiersAsync(importPath, package, target, validation, cancellationToken));
                if (matches.Count == 1)
                {
                    var module = matches[0];
                    if (import.IsNpmValue && !valueNames.ContainsKey(module))
                        valueNames[module] = await provider.ListValueExportsAsync(importPath, module, validation, cancellationToken);
                    else if (!import.IsNpmValue && !exports.ContainsKey(module))
                        exports[module] = await provider.ListExportsAsync(importPath, module, validation, cancellationToken);
                    return module;
                }
                message = matches.Count == 0
                    ? $"No declared npm dependency or public export matches {target}."
                    : $"{target} matches multiple npm specifiers: {string.Join(", ", matches)}.";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message = $"{target}: {ex.Message}";
            }
            diagnostics.Add(Diagnostic.Create(ImportError, import.Location(import.Directive.CodeSpan), message));
            return null;
        }
    }

    private sealed record NpmComponentRequest(string Project, string Module, string Export, string ImportPath,
        JoltDocument Document, TextSpan Span);

    private static async Task<(IReadOnlyList<NpmVueComponent> Components, bool UsesJolt)> LoadVueAsync(JoltWorkspace workspace, JoltProjectImports imports,
        NpmVueMetadataProvider provider, string root, List<Diagnostic> diagnostics, NpmValueMetadataProvider.Validation validation,
        CancellationToken cancellationToken)
    {
        if (workspace.Sources.Count == 0 || workspace.VuePaths.Count == 0)
            return ([], false);

        var candidates = workspace.VuePaths.Select(path => (Path: path, Tag: Path.GetFileNameWithoutExtension(path),
            Namespace: NamespaceFor(path))).Where(candidate => SyntaxFacts.IsValidIdentifier(candidate.Tag) &&
                !SyntaxFactory.ParseName(candidate.Namespace).ContainsDiagnostics).ToArray();
        LocalVueSnapshot snapshot;
        try
        {
            snapshot = await provider.ReadLocalComponentSnapshotAsync(root, candidates.Select(candidate => candidate.Path).ToArray(),
                workspace.VueSources, cancellationToken,
                () => JoltProjectCompiler.ProjectAuthorDeclarations(workspace.Sources, workspace.Options, cancellationToken), validation);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            snapshot = new(candidates.Select(candidate => new LocalVueContract(candidate.Path, null, exception.Message)).ToArray(), true);
        }

        var result = new List<NpmVueComponent>();
        foreach (var (candidate, contract) in candidates.Zip(snapshot.Contracts))
        {
            if (contract.Component is { } component)
            {
                result.Add(component with { LocalPath = candidate.Path, LocalNamespace = candidate.Namespace });
                continue;
            }
            // Project-wide discovery enables completion, but only consumers of a
            // failing SFC need a build diagnostic from Jolt's contract reader.
            foreach (var document in imports.Documents.Where(document => !JoltProjectImports.IsImportsFile(document.Path)))
            {
                var ns = NamespaceFor(document.Path);
                if (ns != candidate.Namespace && !ns.StartsWith(candidate.Namespace + ".", StringComparison.Ordinal) &&
                    !imports.ImportsFor(document).Any(import => import.Code == candidate.Namespace))
                    continue;
                foreach (var element in JoltTree.Elements(document).Where(element => element.Name == candidate.Tag))
                {
                    var span = element.NameSpan;
                    var location = Location.Create(document.Path, span, SourceText.From(document.Source).Lines.GetLinePositionSpan(span));
                    diagnostics.Add(Diagnostic.Create(VueError, location, $"{contract.Path}: {contract.Error}"));
                }
            }
        }
        return (result, snapshot.UsesJolt);

        string NamespaceFor(string path) => string.Join('.', path.Split(['/', '\\']).SkipLast(1).Prepend(workspace.Options.RootNamespace));
    }

    public static async Task<IReadOnlyList<NpmValueExport>> LoadNpmValueCompletionsAsync(string projectDirectory,
        JoltWorkspace workspace, JoltImport import, string prefix, CancellationToken cancellationToken)
    {
        var match = workspace.ValueExports.SingleOrDefault(entry => NpmNames.NamespaceFor(entry.Key) == import.NpmNamespace);
        if (match.Key is null)
            return [];

        var names = match.Value.GroupBy(NpmNames.ValueNameFor, StringComparer.Ordinal)
            .Where(group => group.Count() == 1 && (group.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith(group.Key, StringComparison.OrdinalIgnoreCase)))
            .Select(group => group.Single()).ToArray();
        if (names.Length == 0)
            return [];

        var importPath = Path.Combine(Path.GetFullPath(projectDirectory), import.Document.Path);
        return await Metadata.ReadValuesAsync(importPath, match.Key, names, cancellationToken);
    }

    private static bool IsExcluded(string path, IReadOnlySet<string>? excludedDirectories) =>
        excludedDirectories is { Count: > 0 } &&
        excludedDirectories.Any(directory => IsProjectRelative(Path.GetRelativePath(directory, path).Replace('\\', '/')));

    private static IEnumerable<string> Discover(string root, bool csharp = false, bool stopAtNestedProjects = false,
        IReadOnlySet<string>? excludedDirectories = null)
    {
        var directories = new Stack<string>();
        directories.Push(root);
        while (directories.TryPop(out var directory))
        {
            if (stopAtNestedProjects && directory != root &&
                Directory.EnumerateFiles(directory, "*.csproj").Any())
                continue;
            foreach (var file in new DirectoryInfo(directory).EnumerateFiles())
                if (csharp
                    ? file.Extension == ".cs" && !file.Name.StartsWith('.') &&
                        (file.Attributes & (FileAttributes.Hidden | FileAttributes.ReparsePoint)) == 0
                    : file.Extension is ".jazor" or ".vue")
                    yield return file.FullName;
            foreach (var child in new DirectoryInfo(directory).EnumerateDirectories())
                if (!child.Name.StartsWith('.') && !Excluded.Contains(child.Name) && excludedDirectories?.Contains(child.FullName) != true &&
                    (child.Attributes & (csharp ? FileAttributes.Hidden | FileAttributes.ReparsePoint : FileAttributes.ReparsePoint)) == 0)
                    directories.Push(child.FullName);
        }
    }

}
