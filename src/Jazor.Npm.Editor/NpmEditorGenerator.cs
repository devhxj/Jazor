using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.Npm.Editor;

[Generator(LanguageNames.CSharp)]
public sealed class NpmEditorGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor ProjectionFailure = new("JAZOR", "npm projection failed", "{0}", "Jazor", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var settings = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
            provider.GlobalOptions.TryGetValue("build_property._JazorNpmEditorTool", out var tool);
            provider.GlobalOptions.TryGetValue("build_property.MSBuildProjectFullPath", out var project);
            provider.GlobalOptions.TryGetValue("build_property.JazorPnpmStoreDirectory", out var store);
            provider.GlobalOptions.TryGetValue("build_property.NetCoreRoot", out var netCoreRoot);
            return (Tool: tool, Project: project, Store: store, NetCoreRoot: netCoreRoot);
        });
        var package = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileName(file.Path) == "package.json")
            .Select(static (file, token) =>
            {
                var content = file.GetText(token)?.ToString();
                // An absent AdditionalFile may have an empty Roslyn snapshot. An actual
                // empty manifest still owns its directory and hides the parent's packages.
                return (file.Path, Content: string.IsNullOrEmpty(content) && !File.Exists(file.Path) ? null : content);
            })
            .Where(static file => file.Content is not null)
            .Collect();
        var packageRoots = package.Select(static (manifests, token) => manifests.Select(file =>
        {
            token.ThrowIfCancellationRequested();
            var directory = Path.GetDirectoryName(file.Path)!;
            var names = NpmEditorManifest.ReadPackageNames(file.Content ?? "");
            return (file.Path, Names: names, WithRoot: new HashSet<string>(
                names.Where(name => NpmEditorManifest.HasPackageRoot(directory, name)), StringComparer.Ordinal));
        }).ToArray());
        var locks = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileName(file.Path) is "pnpm-lock.yaml" or "pnpm-workspace.yaml")
            .Select(static (file, token) => (file.Path, Content: file.GetText(token)?.ToString()))
            .Collect();
        var declarations = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".d.ts", StringComparison.Ordinal) ||
                file.Path.EndsWith(".d.mts", StringComparison.Ordinal) || file.Path.EndsWith(".d.cts", StringComparison.Ordinal))
            .Select(static (file, token) => (file.Path, Checksum: Convert.ToBase64String(file.GetText(token)?.GetChecksum().ToArray() ?? [])))
            .Collect();
        // Roslyn reuses this input when editing C#. Compute the declaration identity only
        // when project files change, independently of the compilation and cursor position.
        var dependencies = declarations.Combine(package).Select(static (input, _) =>
        {
            var identity = new StringBuilder();
            foreach (var file in input.Left.OrderBy(file => file.Path, StringComparer.Ordinal))
                identity.AppendLine(file.Path).AppendLine(file.Checksum);
            foreach (var file in input.Right.OrderBy(file => file.Path, StringComparer.Ordinal))
                identity.AppendLine(file.Path).AppendLine(file.Content);
            return identity.ToString();
        });
        var inputs = context.CompilationProvider.Combine(settings).Combine(packageRoots).Combine(locks)
            .Select(static (input, token) =>
            {
                var (((compilation, options), manifests), lockFiles) = input;
                if (compilation is not CSharpCompilation csharp || string.IsNullOrEmpty(options.Tool) ||
                    string.IsNullOrEmpty(options.Project))
                    return (Projection: default(ProjectionInputs), Sites: Array.Empty<NpmSourceRequest>());

                var directory = Path.GetDirectoryName(options.Project!)!;
                string[]? packages = null;
                HashSet<string>? withRoot = null;
                string? lockText = null;
                string? workspaceConfig = null;
                for (var parent = new DirectoryInfo(directory); parent is not null; parent = parent.Parent)
                {
                    var path = Path.Combine(parent.FullName, "package.json");
                    var manifest = manifests.FirstOrDefault(file => NpmEditorManifest.SamePath(file.Path, path));
                    if (manifest.Names is null)
                        continue;
                    packages = manifest.Names;
                    withRoot = manifest.WithRoot;
                    var lockDirectory = parent.FullName;
                    // Restore resolves the lock from the nearest pnpm workspace above the importer.
                    for (var candidate = parent; candidate is not null; candidate = candidate.Parent)
                    {
                        var configPath = Path.Combine(candidate.FullName, "pnpm-workspace.yaml");
                        var config = lockFiles.FirstOrDefault(file => NpmEditorManifest.SamePath(file.Path, configPath));
                        if (config.Content is null)
                            continue;
                        workspaceConfig = config.Content;
                        lockDirectory = candidate.FullName;
                        break;
                    }
                    var lockPath = Path.Combine(lockDirectory, "pnpm-lock.yaml");
                    lockText = lockFiles.FirstOrDefault(file => NpmEditorManifest.SamePath(file.Path, lockPath)).Content;
                    break;
                }
                if (packages is null)
                    return (Projection: default(ProjectionInputs), Sites: Array.Empty<NpmSourceRequest>());

                var roots = NpmEditorManifest.RootNamespaces(packages, csharp, withRoot!);
                var packageRoots = new HashSet<string>(packages.Select(NpmNames.NamespaceFor), StringComparer.Ordinal);
                var eligibleRoots = new HashSet<string>(roots, StringComparer.Ordinal);
                var sites = NpmSourceRequests.Discover(csharp, packages, token)
                    .Where(request => !packageRoots.Contains(request.NpmNamespace) ||
                        eligibleRoots.Contains(request.NpmNamespace) && request.MemberName is not null)
                    .ToArray();
                var requests = sites.Select(request => (request.NpmNamespace, request.MemberName))
                    .Distinct()
                    .OrderBy(request => request.NpmNamespace, StringComparer.Ordinal)
                    .ThenBy(request => request.MemberName, StringComparer.Ordinal)
                    .ToArray();
                var selectedRoots = new HashSet<string>(requests.Select(request => request.NpmNamespace), StringComparer.Ordinal);
                var markers = MarkerSource(roots);
                var unselectedMarkers = MarkerSource(roots.Where(root => !selectedRoots.Contains(root)));
                if (requests.Length == 0)
                    return (Projection: new ProjectionInputs(null, null, null, null, null, null, markers, markers), Sites: sites);

                // Only semantic selections and npm inputs affect the host result. Source
                // positions and unrelated edits never invalidate this incremental step.
                var requestText = new StringBuilder().AppendLine(options.Project).AppendLine(options.Store);
                foreach (var request in requests)
                    requestText.AppendLine(request.NpmNamespace).AppendLine(request.MemberName);
                return (Projection: new ProjectionInputs(options.Tool!, options.NetCoreRoot, options.Project, requestText.ToString(), lockText, workspaceConfig, markers, unselectedMarkers), Sites: sites);
            });

        var source = inputs.Select(static (input, _) => input.Projection).Combine(dependencies).Select(static (input, token) =>
        {
            var projection = input.Left;
            if (projection.Tool is null)
                return new NpmEditorProjectionResult(projection.Markers, []);

            var resolved = NpmHostProcess.RunAsync(projection.Tool, projection.NetCoreRoot, projection.Project!, "--sdk-npm-generate",
                projection.Request!, token).GetAwaiter().GetResult();
            // Emit a single declaration per namespace. Balanced can reuse this
            // generated file for alias binding without evaluating new calls.
            if (resolved is null or "")
                return new NpmEditorProjectionResult(projection.Markers, []);
            var result = JsonSerializer.Deserialize<NpmEditorProjectionResult>(resolved)!;
            return result with { Source = result.Source + projection.UnselectedMarkers };
        });
        context.RegisterSourceOutput(source, static (output, result) =>
        {
            if (!string.IsNullOrEmpty(result.Source))
                output.AddSource(NpmSourceRequests.EditorSourceHint, SourceText.From(result.Source!, Encoding.UTF8));
        });
        // Map failures to the latest compilation independently of the cached host result.
        context.RegisterSourceOutput(source.Combine(inputs.Select(static (input, _) => input.Sites)),
            static (output, input) =>
            {
                foreach (var error in input.Left.Errors)
                    foreach (var request in input.Right)
                        if (request.NpmNamespace == error.Namespace && request.MemberName == error.Member)
                            output.ReportDiagnostic(Diagnostic.Create(ProjectionFailure, request.Location, error.Message));
            });
    }

    private static string MarkerSource(IEnumerable<string> roots) =>
        string.Concat(roots.Select(root => "\nnamespace " + string.Join(".", root.Split('.').Select(part => "@" + part)) +
            " { public static class Exports { } }\n"));

    private readonly record struct ProjectionInputs(string? Tool, string? NetCoreRoot, string? Project, string? Request, string? Lock, string? WorkspaceConfig,
        string? Markers, string? UnselectedMarkers);
}
