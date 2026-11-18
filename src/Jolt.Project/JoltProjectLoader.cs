using System.Text.Json;
using Jolt.Compiler;
using Jolt.Npm;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Project;

public sealed record JoltWorkspace(
    IReadOnlyList<JoltSource> Sources,
    JoltCompileOptions Options,
    IReadOnlyList<Diagnostic> Diagnostics,
    IReadOnlyList<string> Packages,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Exports);

public static class JoltProjectLoader
{
    private static readonly NpmVueMetadataProvider Metadata = new();
    private static readonly DiagnosticDescriptor ImportError = new("JOLT1001", "Component import", "{0}", "Jolt", DiagnosticSeverity.Error, true);
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
        { "node_modules", "bin", "obj", "dist" };

    public static async Task<JoltWorkspace> LoadAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays = null, CancellationToken cancellationToken = default)
    {
        var workspace = await LoadSourcesAsync(projectDirectory, overlays, cancellationToken);
        return await LoadNpmAsync(projectDirectory, workspace, cancellationToken);
    }

    public static async Task<JoltWorkspace> LoadSourcesAsync(string projectDirectory,
        IReadOnlyDictionary<string, string>? overlays, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(projectDirectory);
        var sources = new SortedDictionary<string, JoltSource>(StringComparer.Ordinal);
        foreach (var path in Discover(root))
        {
            var name = Relative(path);
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
                var name = Relative(Path.GetFullPath(path, root));
                if (name.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("../", StringComparison.Ordinal) && !Path.IsPathRooted(name))
                    sources[name] = new(name, content);
            }

        var manifest = Path.Combine(root, "package.json");
        var packages = new SortedSet<string>(StringComparer.Ordinal);
        var rootNamespace = "App";
        if (File.Exists(manifest))
        {
            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(manifest, cancellationToken));
            var project = json.RootElement;
            if (project.TryGetProperty("jolt", out var options) && options.TryGetProperty("rootNamespace", out var ns))
                rootNamespace = ns.GetString() ?? rootNamespace;
            foreach (var group in new[] { "dependencies", "devDependencies", "peerDependencies", "optionalDependencies" })
                if (project.TryGetProperty(group, out var deps))
                    foreach (var dep in deps.EnumerateObject())
                        packages.Add(dep.Name);
        }

        return new(sources.Values.ToArray(), new(rootNamespace), [], packages.ToArray(),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal));

        string Relative(string path) => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    public static async Task<JoltWorkspace> LoadNpmAsync(string projectDirectory, JoltWorkspace workspace, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(projectDirectory);
        var exports = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var metadata = new Dictionary<(string Module, string Export), NpmVueComponent>();
        var diagnostics = new List<Diagnostic>();
        foreach (var file in workspace.Sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parsed = JoltParser.Parse(file.Content, file.Path);
            var tags = Elements(parsed.Nodes).Where(e => e.Name.Length != 0 && char.IsUpper(e.Name[0])).ToArray();
            var documentPath = Path.Combine(root, file.Path);
            foreach (var directive in parsed.Nodes.OfType<JoltUsing>().Where(d => d.Code.StartsWith("Npm.", StringComparison.Ordinal)))
            {
                try
                {
                    var matches = new List<string>();
                    foreach (var package in workspace.Packages)
                    {
                        var ns = NpmNames.NamespaceFor(package);
                        if (ns == directive.Code)
                            matches.Add(package);
                        else if (directive.Code.StartsWith(ns + ".", StringComparison.Ordinal))
                            matches.AddRange((await Metadata.ListSpecifiersAsync(documentPath, package, cancellationToken))
                                .Where(spec => NpmNames.NamespaceFor(spec) == directive.Code));
                    }
                    if (matches.Count != 1)
                    {
                        Report(directive.CodeSpan, matches.Count == 0
                            ? $"No declared npm dependency or public export matches {directive.Code}."
                            : $"{directive.Code} matches multiple npm specifiers: {string.Join(", ", matches)}.");
                        continue;
                    }
                    var module = matches[0];
                    if (!exports.TryGetValue(module, out var names))
                        exports[module] = names = await Metadata.ListExportsAsync(documentPath, module, cancellationToken);
                    foreach (var export in names)
                    {
                        var site = Array.Find(tags, e => e.Name == NpmNames.TagFor(module, export));
                        if (site == null || metadata.ContainsKey((module, export)))
                            continue;
                        try
                        {
                            metadata[(module, export)] = await Metadata.ReadAsync(documentPath, module, export, cancellationToken);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            Report(site.NameSpan, $"{module} ({export}): {ex.Message}");
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Report(directive.CodeSpan, $"{directive.Code}: {ex.Message}");
                }
            }

            void Report(TextSpan span, string message)
            {
                var lines = SourceText.From(file.Content).Lines;
                var location = Location.Create(file.Path, span, lines.GetLinePositionSpan(span));
                diagnostics.Add(Diagnostic.Create(ImportError, location, message));
            }
        }
        return workspace with
        {
            Options = workspace.Options with { NpmComponents = metadata.Values.ToArray() },
            Diagnostics = diagnostics,
            Exports = exports,
        };
    }

    private static IEnumerable<string> Discover(string root)
    {
        var directories = new Stack<string>();
        directories.Push(root);
        while (directories.TryPop(out var directory))
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.jazor"))
                yield return file;
            foreach (var child in new DirectoryInfo(directory).EnumerateDirectories())
                if (!child.Name.StartsWith('.') && !Excluded.Contains(child.Name) && !child.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    directories.Push(child.FullName);
        }
    }

    private static IEnumerable<JoltElement> Elements(IEnumerable<JoltNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is JoltElement element)
                yield return element;
            IEnumerable<JoltNode> children = node switch
            {
                JoltElement el => el.Children,
                JoltIf condition => condition.Then.Concat(condition.Else),
                JoltForEach loop => loop.Children,
                _ => [],
            };
            foreach (var child in Elements(children))
                yield return child;
        }
    }
}
