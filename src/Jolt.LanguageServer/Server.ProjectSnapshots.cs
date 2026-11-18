using Jolt.Compiler;
using Jolt.Project;
using Jolt.Syntax;

namespace Jolt.LanguageServer;

public sealed partial class Server
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private readonly Dictionary<string, ProjectState> _projects = new(PathComparer);
    private readonly Lock _projectsLock = new();
    private CancellationToken _lifetime;
    private int _serverRequestId;

    private ProjectState ProjectFor(string path)
    {
        var directory = ProjectDirectory(path);
        lock (_projectsLock)
        {
            if (!_projects.TryGetValue(directory, out var project))
                _projects[directory] = project = new(directory);
            return project;
        }
    }

    private ProjectState[] Invalidate(string path, bool fileSystem = false)
    {
        var dependencyChanged = fileSystem && !path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase);
        lock (_projectsLock)
        {
            var affected = _projects.Values.Where(project => IsWithin(project.Directory, path)).ToArray();
            foreach (var project in affected)
            {
                project.Version = null;
                project.SourceVersion++;
                if (dependencyChanged)
                    project.DependencyVersion++;
            }
            return affected;
        }
    }

    private VersionedProject GetProject(ProjectState project)
    {
        lock (_projectsLock)
        {
            if (project.Version is { } current)
                return current;
            var overlays = _buffers.ToArray()
                .Where(entry => PathComparer.Equals(ProjectDirectory(entry.Key), project.Directory))
                .ToDictionary(entry => entry.Key, entry => entry.Value.Text.ToString(), PathComparer);
            var dependencyVersion = project.DependencyVersion;
            var sourceVersion = project.SourceVersion;
            return project.Version = new(Task.Run(() => BuildSnapshotAsync(project, overlays, sourceVersion, dependencyVersion), _lifetime));
        }
    }

    private bool IsCurrent(ProjectState project, VersionedProject version)
    {
        lock (_projectsLock)
            return project.Version == version;
    }

    private async Task<ProjectSnapshot> BuildSnapshotAsync(ProjectState project, IReadOnlyDictionary<string, string> overlays,
        int sourceVersion, int dependencyVersion)
    {
        var workspace = await JoltProjectLoader.LoadSourcesAsync(project.Directory, overlays, _lifetime);
        var npmInputs = workspace.Sources.Where(source => source.Content.Contains("Npm.", StringComparison.Ordinal))
            .Select(source => JoltParser.Parse(source.Content, source.Path))
            .Select(document => (document.Path, Imports: document.Nodes.OfType<JoltUsing>()
                .Where(use => use.Code.StartsWith("Npm.", StringComparison.Ordinal)).Select(use => use.Code).ToArray(),
                Tags: LanguageFeatures.Elements(document.Nodes).Select(element => element.Name)
                    .Where(name => name.Length > 0 && char.IsUpper(name[0])).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()))
            .Where(document => document.Imports.Length > 0).ToArray();
        var imports = npmInputs.Select(item => item.Path).ToHashSet(StringComparer.Ordinal);
        var key = System.Text.Json.JsonSerializer.Serialize(new
        {
            dependencyVersion, workspace.Packages,
            npmInputs = npmInputs.Select(item => new { item.Path, item.Imports, item.Tags }),
        });
        NpmWork? npm = null;
        lock (_projectsLock)
        {
            if (project.SourceVersion != sourceVersion)
                throw new OperationCanceledException("The project changed while loading its sources.");

            // Symbol requests share native work. Replacing a buffer only cancels the caller's wait.
            if (imports.Count == 0)
            {
                project.NpmKey = null;
            }
            else if (project.NpmKey != key || project.Npm is null ||
                project.Npm.Metadata is { IsCompletedSuccessfully: true } failed && failed.Result.Diagnostics.Count > 0 &&
                !ReferenceEquals(workspace.Sources, failed.Result.Sources))
            {
                project.NpmKey = key;
                var preceding = project.Npm;
                var work = new NpmWork();
                work.Metadata = Task.Run(async () =>
                {
                    if (preceding is not null)
                        await ((Task)preceding.Metadata).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                    lock (_projectsLock)
                        if (project.Npm != work || project.NpmKey != key)
                            throw new OperationCanceledException("A newer npm input superseded this request.");
                    return await JoltProjectLoader.LoadNpmAsync(project.Directory, workspace, _lifetime);
                }, _lifetime);
                project.Npm = work;
            }
            if (imports.Count != 0)
                npm = project.Npm;
        }
        if (npm is null)
            return Analyze(workspace);
        if (npm.Metadata.IsCompletedSuccessfully)
            return await FinishAsync(npm);
        var complete = FinishAsync(npm);
        return Analyze(workspace) with { Npm = complete, NpmIdentity = npm };

        ProjectSnapshot Analyze(JoltWorkspace input) =>
            new(project.Directory, input, JoltProjectCompiler.Analyze(input.Sources, input.Options, _lifetime), imports);

        async Task<ProjectSnapshot> FinishAsync(NpmWork work)
        {
            var result = await work.Metadata;
            // The import graph can survive an edit. Rebind new source using the same npm contracts.
            if (!ReferenceEquals(workspace.Sources, result.Sources) && result.Diagnostics.Count != 0)
            {
                lock (_projectsLock)
                    if (project.SourceVersion != sourceVersion)
                        throw new OperationCanceledException("The project changed while loading npm metadata.");
                result = await JoltProjectLoader.LoadNpmAsync(project.Directory, workspace, _lifetime);
            }
            return Analyze(workspace with
            {
                Options = workspace.Options with { NpmComponents = result.Options.NpmComponents },
                Diagnostics = result.Diagnostics,
                Exports = result.Exports,
            }) with { NpmIdentity = work };
        }
    }

    private static bool IsWithin(string directory, string path)
    {
        var relative = Path.GetRelativePath(directory, path);
        return relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !Path.IsPathRooted(relative);
    }

    private sealed class ProjectState(string directory)
    {
        internal string Directory { get; } = directory;
        internal int SourceVersion { get; set; }
        internal int DependencyVersion { get; set; }
        internal VersionedProject? Version { get; set; }
        internal string? NpmKey { get; set; }
        internal NpmWork? Npm { get; set; }
        internal NpmWork? ClassifiedNpm { get; set; }
        internal CancellationTokenSource? Validation { get; set; }
    }

    private sealed class NpmWork
    {
        internal Task<JoltWorkspace> Metadata { get; set; } = null!;
    }

    private sealed record VersionedProject(Task<ProjectSnapshot> Input);

    private sealed record ProjectSnapshot(string Directory, JoltWorkspace Workspace, JoltAnalysis Analysis, HashSet<string> NpmImports)
    {
        internal Task<ProjectSnapshot>? Npm { get; init; }
        internal NpmWork? NpmIdentity { get; init; }
    }
}
