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
    private long _nextSnapshotId;

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
        var dependencyChanged = fileSystem && !IsJazor(path);
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
                .Where(entry => IsWithin(project.Directory, entry.Key) &&
                    (!IsJazor(entry.Key) || PathComparer.Equals(ProjectDirectory(entry.Key), project.Directory)))
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
        var projectImports = JoltProjectImports.Parse(workspace.Sources, _lifetime);
        var npmInputs = projectImports.Documents
            .Select(document =>
            {
                var uses = projectImports.ImportsFor(document).Where(use => use.IsNpm).ToArray();
                return new
                {
                    document.Path,
                    Imports = uses.Select(use => use.Code).ToArray(),
                    Tags = JoltTree.Elements(document).Select(element => element.Name)
                        .Where(name => name.Length > 0 && char.IsUpper(name[0])).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                    // Roslyn discovers value uses from the source, including local symbols that shadow an alias.
                    ValueSource = uses.Any(use => use.IsNpmValue) ? document.Source : null,
                };
            })
            .Where(document => document.Imports.Length > 0).ToArray();
        var imports = npmInputs.Select(item => item.Path).ToHashSet(StringComparer.Ordinal);
        var hasMetadata = imports.Count > 0 || workspace.VuePaths.Count > 0;
        var key = System.Text.Json.JsonSerializer.Serialize(new
        {
            dependencyVersion, workspace.Packages,
            npmInputs, workspace.VuePaths,
            VueOverlays = workspace.VueSources.OrderBy(entry => entry.Key, PathComparer),
        });
        NpmWork? npm = null;
        lock (_projectsLock)
        {
            if (project.SourceVersion != sourceVersion)
                throw new OperationCanceledException("The project changed while loading its sources.");

            // Symbol requests share native work. Replacing a buffer only cancels the caller's wait.
            if (!hasMetadata)
            {
                project.NpmKey = null;
            }
            else if (project.NpmKey != key || project.Npm is null || VueNeedsAuthorSources(project.Npm) ||
                project.Npm.Metadata is { IsCompletedSuccessfully: true } failed && failed.Result.Diagnostics.Count > 0 &&
                !ReferenceEquals(workspace.Sources, failed.Result.Sources))
            {
                project.NpmKey = key;
                var preceding = project.Npm;
                var work = new NpmWork(workspace.Sources);
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
            if (hasMetadata)
                npm = project.Npm;
        }
        if (npm is null)
            return Analyze(workspace);
        // Local component diagnostics and definitions rely on the same Vue snapshot.
        if (workspace.VuePaths.Count > 0 || npm.Metadata.IsCompletedSuccessfully)
            return await FinishAsync(npm);
        var complete = FinishAsync(npm);
        return Analyze(workspace) with { Npm = complete, NpmIdentity = npm };

        bool VueNeedsAuthorSources(NpmWork previous)
        {
            if (workspace.VuePaths.Count == 0)
                return false;
            // The TS resolver reports whether the SFC graph consumes projected Jolt
            // declarations. Until the first query finishes, that dependency is unknown.
            if (previous.Metadata.IsCompletedSuccessfully && !previous.Metadata.Result.VueUsesJolt)
                return false;
            // The local metadata provider compares the actual declarations before invoking TS again.
            return !workspace.Sources.SequenceEqual(previous.AuthorSources);
        }

        ProjectSnapshot Analyze(JoltWorkspace input) =>
            new(Interlocked.Increment(ref _nextSnapshotId), project.Directory, input,
                JoltProjectCompiler.Analyze(input.Sources, input.Options, _lifetime), imports);

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
                Options = workspace.Options with
                {
                    NpmComponents = result.Options.NpmComponents,
                    NpmValues = result.Options.NpmValues,
                    VueComponents = result.Options.VueComponents,
                },
                Diagnostics = result.Diagnostics,
                Exports = result.Exports,
                ValueExports = result.ValueExports,
                VueSources = result.VueSources,
                VueUsesJolt = result.VueUsesJolt,
            }) with { NpmIdentity = work };
        }
    }

    private static bool IsJazor(string path) => path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase);

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

    private sealed class NpmWork(IReadOnlyList<JoltSource> authorSources)
    {
        internal IReadOnlyList<JoltSource> AuthorSources { get; } = authorSources;
        internal Task<JoltWorkspace> Metadata { get; set; } = null!;
    }

    private sealed record VersionedProject(Task<ProjectSnapshot> Input);

    private sealed record ProjectSnapshot(long Id, string Directory, JoltWorkspace Workspace, JoltAnalysis Analysis, HashSet<string> NpmImports)
    {
        internal Task<ProjectSnapshot>? Npm { get; init; }
        internal NpmWork? NpmIdentity { get; init; }
    }
}
