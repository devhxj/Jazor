using System.Text.Json;
using Jolt.Compiler;
using Jolt.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
}

public static class JoltProjectLoader
{
    private static readonly NpmVueMetadataProvider Metadata = new();
    private static readonly DiagnosticDescriptor ImportError = new("JOLT1001", "npm import", "{0}", "Jolt", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor VueError = new("JOLT1040", "Vue component", "{0}", "Jolt", DiagnosticSeverity.Error, true);
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
        { "node_modules", "bin", "obj", "dist" };

    public static Task<JoltWorkspace> LoadAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays = null, CancellationToken cancellationToken = default)
        => LoadAsync(projectDirectory, Metadata, overlays, cancellationToken);

    internal static async Task<JoltWorkspace> LoadAsync(string projectDirectory, NpmVueMetadataProvider provider,
        IReadOnlyDictionary<string, string>? overlays = null, CancellationToken cancellationToken = default)
    {
        var workspace = await LoadSourcesAsync(projectDirectory, overlays, cancellationToken);
        return await LoadNpmAsync(projectDirectory, workspace, provider, cancellationToken);
    }

    public static async Task<JoltWorkspace> LoadSourcesAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(projectDirectory);
        var sources = new SortedDictionary<string, JoltSource>(StringComparer.Ordinal);
        var vuePaths = new SortedSet<string>(StringComparer.Ordinal);
        var vueSources = new Dictionary<string, string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var path in Discover(root))
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
                sources[name] = new(name, await File.ReadAllTextAsync(path, cancellationToken));
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
                var name = Relative(absolute);
                if (name.StartsWith("../", StringComparison.Ordinal) || Path.IsPathRooted(name))
                    continue;
                if (name.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
                    sources[name] = new(name, content);
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
        bool router = false;
        if (File.Exists(manifest))
        {
            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(manifest, cancellationToken));
            var project = json.RootElement;
            if (project.TryGetProperty("jolt", out var options))
            {
                if (options.TryGetProperty("rootNamespace", out var ns))
                    rootNamespace = ns.GetString() ?? rootNamespace;
                router = options.TryGetProperty("router", out var routing) && routing.ValueKind == JsonValueKind.True;
            }
            foreach (var group in new[] { "dependencies", "devDependencies", "peerDependencies", "optionalDependencies" })
                if (project.TryGetProperty(group, out var deps))
                    foreach (var dep in deps.EnumerateObject())
                        packages.Add(dep.Name);
        }

        return new(sources.Values.ToArray(), new(rootNamespace) { Router = router }, [], packages.ToArray(),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal))
        {
            VuePaths = vuePaths.ToArray(),
            VueSources = vueSources,
        };

        string Relative(string path) => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    public static Task<JoltWorkspace> LoadNpmAsync(string projectDirectory, JoltWorkspace workspace, CancellationToken cancellationToken)
        => LoadNpmAsync(projectDirectory, workspace, Metadata, cancellationToken);

    private static async Task<JoltWorkspace> LoadNpmAsync(string projectDirectory, JoltWorkspace workspace,
        NpmVueMetadataProvider provider, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(projectDirectory);
        var exports = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var valueNames = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var metadata = new Dictionary<(string Module, string Export), NpmVueComponent>();
        var values = new Dictionary<(string Module, string Export), NpmValueExport>();
        var valueErrors = new Dictionary<(string Module, string Export), string>();
        var diagnostics = new List<Diagnostic>();
        var projectImports = JoltProjectImports.Parse(workspace.Sources, cancellationToken);
        var modules = new Dictionary<JoltImport, string?>();
        var valueAliases = new Dictionary<string, Dictionary<(string Alias, string Namespace), (string Module, string ImportPath)>>(StringComparer.Ordinal);
        foreach (var document in projectImports.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tags = JoltProjectImports.IsImportsFile(document.Path) ? [] :
                JoltTree.Elements(document).Where(e => e.Name.Length != 0 && char.IsUpper(e.Name[0])).ToArray();
            foreach (var import in projectImports.ImportsFor(document).Where(import => import.IsNpm).DistinctBy(import => import.Code))
            {
                var importPath = Path.Combine(root, import.Document.Path);
                if (!modules.TryGetValue(import, out var module))
                    modules[import] = module = await ResolveImportAsync(import, importPath);
                if (module == null)
                    continue;
                if (import.IsNpmValue)
                {
                    if (!JoltProjectImports.IsImportsFile(document.Path))
                    {
                        if (!valueAliases.TryGetValue(document.Path, out var aliases))
                            valueAliases[document.Path] = aliases = new();
                        aliases[(import.Alias!, import.NpmNamespace!)] = (module, importPath);
                    }
                    continue;
                }
                foreach (var export in exports[module])
                {
                    var site = Array.Find(tags, e => e.Name == NpmNames.TagFor(module, export));
                    if (site == null || metadata.ContainsKey((module, export)))
                        continue;
                    try
                    {
                        metadata[(module, export)] = await provider.ReadAsync(importPath, module, export, cancellationToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        var lines = SourceText.From(document.Source).Lines;
                        var location = Location.Create(document.Path, site.NameSpan, lines.GetLinePositionSpan(site.NameSpan));
                        diagnostics.Add(Diagnostic.Create(ImportError, location, $"{module} ({export}): {ex.Message}"));
                    }
                }
            }
        }
        var componentOptions = workspace.Options with { NpmComponents = metadata.Values.ToArray() };
        var (vueComponents, vueUsesJolt) = await LoadVueAsync(workspace with { Options = componentOptions }, projectImports, provider, root,
            diagnostics, cancellationToken);
        componentOptions = componentOptions with { VueComponents = vueComponents };
        if (valueAliases.Count != 0)
        {
            // Bind through the actual C# projection so a local/parameter can shadow
            // an alias and aliases in scoped slots see the real component contracts.
            var preliminary = JoltProjectCompiler.Analyze(workspace.Sources,
                componentOptions, cancellationToken);
            foreach (var projected in preliminary.Documents)
            {
                if (!valueAliases.TryGetValue(projected.Document.Path, out var aliases))
                    continue;
                var model = projected.SemanticModel;
                var seen = new HashSet<TextSpan>();
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
                    if (seen.Add(sourceSpan))
                        await LoadValueAsync(projected.Document, name.ValueText, sourceSpan, import.ImportPath, import.Module);
                }
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

        async Task LoadValueAsync(JoltDocument document, string member, TextSpan sourceSpan, string importPath, string module)
        {
            var matches = valueNames[module].Where(name => NpmNames.ValueNameFor(name) == member).ToArray();
            // A miss is diagnosed by Roslyn on the authored member access. Report
            // metadata errors here for real exports that cannot be projected into C#.
            if (matches.Length == 0)
                return;

            string message;
            if (matches.Length > 1)
            {
                message = $"'{member}' matches multiple exports of '{module}': {string.Join(", ", matches)}.";
            }
            else
            {
                var key = (module, matches[0]);
                if (values.ContainsKey(key))
                    return;
                if (!valueErrors.TryGetValue(key, out message!))
                {
                    try
                    {
                        values[key] = await provider.ReadValueAsync(importPath, module, key.Item2, cancellationToken);
                        return;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        valueErrors[key] = message = $"{module} ({key.Item2}): {ex.Message}";
                    }
                }
            }
            var lines = SourceText.From(document.Source).Lines;
            var location = Location.Create(document.Path, sourceSpan, lines.GetLinePositionSpan(sourceSpan));
            diagnostics.Add(Diagnostic.Create(ImportError, location, message));
        }

        async Task<string?> ResolveImportAsync(JoltImport import, string importPath)
        {
            var target = import.NpmNamespace!;
            string message;
            try
            {
                var matches = new List<string>();
                foreach (var package in workspace.Packages)
                {
                    var ns = NpmNames.NamespaceFor(package);
                    if (ns == target)
                        matches.Add(package);
                    else if (target.StartsWith(ns + ".", StringComparison.Ordinal))
                        matches.AddRange((await provider.ListSpecifiersAsync(importPath, package, cancellationToken))
                            .Where(spec => NpmNames.NamespaceFor(spec) == target));
                }
                if (matches.Count == 1)
                {
                    var module = matches[0];
                    if (import.IsNpmValue && !valueNames.ContainsKey(module))
                        valueNames[module] = await provider.ListValueExportsAsync(importPath, module, cancellationToken);
                    else if (!import.IsNpmValue && !exports.ContainsKey(module))
                        exports[module] = await provider.ListExportsAsync(importPath, module, cancellationToken);
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

    private static async Task<(IReadOnlyList<NpmVueComponent> Components, bool UsesJolt)> LoadVueAsync(JoltWorkspace workspace, JoltProjectImports imports,
        NpmVueMetadataProvider provider, string root, List<Diagnostic> diagnostics, CancellationToken cancellationToken)
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
                () => JoltProjectCompiler.ProjectAuthorDeclarations(workspace.Sources, workspace.Options, cancellationToken));
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
            .Where(group => group.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && group.Count() == 1)
            .Select(group => group.Single()).ToArray();
        if (names.Length == 0)
            return [];

        var importPath = Path.Combine(Path.GetFullPath(projectDirectory), import.Document.Path);
        return await Metadata.ReadValuesAsync(importPath, match.Key, names, cancellationToken);
    }

    private static IEnumerable<string> Discover(string root)
    {
        var directories = new Stack<string>();
        directories.Push(root);
        while (directories.TryPop(out var directory))
        {
            foreach (var file in Directory.EnumerateFiles(directory))
                if (Path.GetExtension(file) is ".jazor" or ".vue")
                    yield return file;
            foreach (var child in new DirectoryInfo(directory).EnumerateDirectories())
                if (!child.Name.StartsWith('.') && !Excluded.Contains(child.Name) && !child.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    directories.Push(child.FullName);
        }
    }

}
