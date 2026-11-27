using Jolt.Compiler;
using Jolt.Npm;
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
    private bool _projectSources;

    private ProjectState ProjectFor(string path)
    {
        var directory = ProjectDirectory(path);
        lock (_projectsLock)
        {
            if (!_projects.TryGetValue(directory, out var project))
            {
                _projects[directory] = project = new(directory);
                var observedProject = project;
                project.Declarations.Changed = () => OnExternalDeclarationsChanged(observedProject);
            }
            return project;
        }
    }

    private void OnExternalDeclarationsChanged(ProjectState project)
    {
        lock (_projectsLock)
        {
            if (_shutdown || _lifetime.IsCancellationRequested)
                return;
            project.Version?.Cancel();
            project.Version = null;
            project.SourceVersion++;
            project.DependencyVersion++;
        }
        _ = ValidateChangesAsync([project], _lifetime);
    }

    private ProjectState[] Invalidate(string path, bool fileSystem = false)
    {
        var csharp = Path.GetExtension(path).Equals(".cs", StringComparison.Ordinal);
        var dependencyChanged = fileSystem && !IsJazor(path) && !csharp;
        lock (_projectsLock)
        {
            // Actual source selection is retained from the last manifest load. Ordinary
            // C# edits, including generated files, don't schedule a project reload.
            var contextChanged = fileSystem && (csharp ||
                Path.GetExtension(path) is ".csproj" or ".props" or ".targets" ||
                Path.GetFileName(path) is "project.inputs" or "project.assets.json" or "global.json");
            var ancestorConfiguration = contextChanged && Path.GetFileName(path) is
                "Directory.Build.props" or "Directory.Build.targets" or "Directory.Packages.props" or "global.json";
            var affected = _projects.Values.Where(project => (IsWithin(project.Directory, path) ||
                    ancestorConfiguration && IsWithin(Path.GetDirectoryName(path)!, project.Directory) ||
                    csharp && project.CSharpSelection?.Includes(path) == true) &&
                (!csharp || !project.HasLoadedCSharpSelection || project.CSharpSelection?.Includes(path) == true ||
                    contextChanged && Directory.EnumerateFiles(project.Directory, "*.csproj").Any() &&
                    IsWithin(project.Directory, path) && JoltProjectLoader.IsCSharpDiscoveryPath(project.Directory,
                        Path.GetRelativePath(project.Directory, path).Replace('\\', '/')))).ToArray();
            foreach (var project in affected)
            {
                if (PathComparer.Equals(Path.Combine(project.Directory, "package.json"), path))
                    project.HasLoadedCSharpSelection = false;
                project.Version?.Cancel();
                project.Version = null;
                project.SourceVersion++;
                project.RefreshDotNetInputs |= contextChanged;
                if (dependencyChanged)
                    project.DependencyVersion++;
            }
            return affected;
        }
    }

    private VersionedProject GetProject(ProjectState project, Task? activeMemberRequest = null, JoltSource? openedSource = null)
    {
        lock (_projectsLock)
        {
            if (project.Version is { } current)
            {
                current.FirstMemberRequest ??= activeMemberRequest;
                return current;
            }
            var overlays = _buffers.ToArray()
                .Where(entry => (IsWithin(project.Directory, entry.Key) ||
                        project.CSharpSelection?.Includes(entry.Key) == true) &&
                    (!IsJazor(entry.Key) || PathComparer.Equals(ProjectDirectory(entry.Key), project.Directory)))
                .ToDictionary(entry => entry.Key, entry => entry.Value.Text.ToString(), PathComparer);
            var dependencyVersion = project.DependencyVersion;
            var sourceVersion = project.SourceVersion;
            var refreshDotNetInputs = project.RefreshDotNetInputs;
            var previousSources = project.Sources;
            var previousAnalysis = project.AnalysisDependencyVersion == dependencyVersion ? project.Analysis : null;
            var previousProvisional = project.ProvisionalDependencyVersion == dependencyVersion ? project.ProvisionalAnalysis : null;
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime);
            var token = cancellation.Token;
            // A cold @code member request can bind as soon as the component declarations exist.
            var declarations = previousAnalysis is null && previousProvisional is null
                ? new TaskCompletionSource<ProjectSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously) : null;
            // The loader retains matching source objects. Parse the actual open buffer while
            // enumerating disk inputs so both the first request and validation reuse its Razor tree.
            var openParse = openedSource is null ? Task.CompletedTask :
                Task.Run(() => JoltProjectImports.ParseDocument(openedSource), token);
            var needsFreshInputs = refreshDotNetInputs && previousSources is not null;
            var sources = Task.Run(() => JoltProjectLoader.LoadSourcesAsync(project.Directory, overlays, token,
                openedSource is null ? previousSources : [openedSource], evaluateDotNetProject: needsFreshInputs), token);
            var version = new VersionedProject(cancellation, declarations?.Task, sources) { FirstMemberRequest = activeMemberRequest };
            project.Version = version;
            // Prepare declarations for the first opened file. An immediate member request
            // reuses this work even when source discovery finishes before it arrives.
            var openedDocument = openedSource is not null && declarations is not null
                ? FocusedDocumentAsync(project, version, openedSource.Path) : null;
            version.Input = Task.Run(async () =>
            {
                var workspace = await sources;
                await openParse;
                if (openedDocument is not null)
                    await openedDocument;
                Task? priority;
                lock (_projectsLock)
                    priority = version.FirstMemberRequest;
                // If a member request arrived during discovery, let its Roslyn bind
                // finish before cold-parsing every template on the same thread pool.
                if (declarations is not null && priority is not null)
                    await priority.WaitAsync(token);
                if (declarations is not null)
                    LanguageFeatures.PrepareWorkspaceEditor();
                if (refreshDotNetInputs && !needsFreshInputs)
                {
                    workspace = await JoltProjectLoader.LoadSourcesAsync(project.Directory, overlays, token, workspace.Sources,
                        evaluateDotNetProject: true);
                }
                if (refreshDotNetInputs)
                {
                    lock (_projectsLock)
                        if (project.Version == version)
                            project.RefreshDotNetInputs = false;
                }
                return await BuildSnapshotAsync(project, workspace, overlays, sourceVersion, dependencyVersion,
                    previousSources, previousAnalysis, previousProvisional, declarations, token);
            }, token);
            return version;
        }
    }

    private bool IsCurrent(ProjectState project, VersionedProject version)
    {
        lock (_projectsLock)
            return project.Version == version;
    }

    private async Task<ProjectSnapshot> BuildSnapshotAsync(ProjectState project, JoltWorkspace workspace, IReadOnlyDictionary<string, string> overlays,
        int sourceVersion, int dependencyVersion, IReadOnlyList<JoltSource>? previousSources,
        JoltProjectCompiler.AnalysisState? previousAnalysis, JoltProjectCompiler.AnalysisState? previousProvisional,
        TaskCompletionSource<ProjectSnapshot>? declarations, CancellationToken cancellationToken)
    {
        var diagnosticPaths = overlays.Keys.Where(IsJazor)
            .Select(path => Path.GetRelativePath(project.Directory, path).Replace('\\', '/')).ToHashSet(PathComparer);
        var projectImports = previousSources is null
            ? JoltProjectImports.ParseCold(workspace.Sources, cancellationToken)
            : JoltProjectImports.Parse(workspace.Sources, cancellationToken);
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
        var csharpSources = workspace.Options.CSharpSources.Concat(workspace.Options.ProjectSources).ToArray();
        var key = System.Text.Json.JsonSerializer.Serialize(new
        {
            dependencyVersion, workspace.Packages,
            csharpSources, workspace.Options.ProjectDefines,
            npmInputs, workspace.VuePaths,
            VueOverlays = workspace.VueSources.OrderBy(entry => entry.Key, PathComparer),
        });
        NpmWork? npm = null;
        lock (_projectsLock)
        {
            if (project.SourceVersion != sourceVersion)
                throw new OperationCanceledException("The project changed while loading its sources.");
            project.Sources = workspace.Sources;
            project.HasLoadedCSharpSelection = true;
            project.CSharpSelection = workspace.CSharpSelection;
            if (_projectSources)
                PublishLinkedSources(project);

            // Symbol requests share native work. Replacing a buffer only cancels the caller's wait.
            if (!hasMetadata)
            {
                project.NpmKey = null;
                project.Declarations.Reset();
            }
            else if (project.NpmKey != key || project.Npm is null || VueNeedsAuthorSources(project.Npm) ||
                project.Npm.Metadata is { IsCompletedSuccessfully: true } failed && failed.Result.Diagnostics.Count > 0 &&
                !ReferenceEquals(workspace.Sources, failed.Result.Sources))
            {
                project.NpmKey = key;
                var preceding = project.Npm;
                var work = new NpmWork(workspace.Sources, csharpSources);
                work.Metadata = Task.Run(async () =>
                {
                    if (preceding is not null)
                        await ((Task)preceding.Metadata).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                    lock (_projectsLock)
                        if (project.Npm != work || project.NpmKey != key)
                            throw new OperationCanceledException("A newer npm input superseded this request.");
                    return await LoadObservedNpmAsync(work);
                }, _lifetime);
                project.Npm = work;
            }
            if (hasMetadata)
                npm = project.Npm;
        }
        if (npm is null)
            return Analyze(workspace);
        // Local Vue depends on the bound project. For a follow-up edit during cold
        // binding, the existing provisional state can serve metadata-only requests.
        if (workspace.VuePaths.Count > 0 ||
            npm.Metadata.IsCompletedSuccessfully && (previousProvisional is null || previousAnalysis is not null))
            return await FinishAsync(npm);

        // Metadata is already loading independently. Give the editor a usable
        // projection before starting a second full Roslyn analysis.
        var provisional = Analyze(workspace, provisional: true);
        return provisional with
        {
            Npm = Task.Run(() => FinishAsync(npm), cancellationToken),
            NpmCatalog = npm.ReadPhaseAsync(npm.Progress.Catalog.Task),
            NpmComponents = npm.ReadPhaseAsync(npm.Progress.Components.Task),
            NpmIdentity = npm,
        };

        bool VueNeedsAuthorSources(NpmWork previous)
        {
            if (workspace.VuePaths.Count == 0)
                return false;
            // The TS resolver reports whether the SFC graph consumes projected Jolt
            // declarations. Until the first query finishes, that dependency is unknown.
            if (previous.Metadata.IsCompletedSuccessfully && !previous.Metadata.Result.VueUsesJolt)
                return false;
            // The local metadata provider compares the actual declarations before invoking TS again.
            return !workspace.Sources.SequenceEqual(previous.AuthorSources) ||
                !csharpSources.SequenceEqual(previous.CSharpSources);
        }

        ProjectSnapshot Analyze(JoltWorkspace input, bool provisional = false)
        {
            var baseline = provisional ? previousProvisional : previousAnalysis;
            var (analysis, state) = JoltProjectCompiler.AnalyzeEditor(input.Sources, input.Options, diagnosticPaths,
                baseline, cancellationToken, declarations is null || declarations.Task.IsCompleted ? null : authors =>
                    declarations.TrySetResult(new(Interlocked.Increment(ref _nextSnapshotId), project.Directory, input, authors, imports)));
            lock (_projectsLock)
            {
                if (project.SourceVersion != sourceVersion)
                    throw new OperationCanceledException(cancellationToken);
                // Edits can arrive after catalog navigation but before the full npm
                // bind. Reuse like-for-like snapshots without replacing settled state.
                if (provisional)
                {
                    if (project.AnalysisSourceVersion != sourceVersion)
                    {
                        project.ProvisionalAnalysis = state;
                        project.ProvisionalDependencyVersion = dependencyVersion;
                    }
                }
                else
                {
                    project.Analysis = state;
                    project.AnalysisDependencyVersion = dependencyVersion;
                    project.AnalysisSourceVersion = sourceVersion;
                    project.ProvisionalAnalysis = null;
                }
            }
            return new(Interlocked.Increment(ref _nextSnapshotId), project.Directory, input, analysis, imports);
        }

        async Task<ProjectSnapshot> FinishAsync(NpmWork work)
        {
            var result = await work.Metadata.WaitAsync(cancellationToken);
            // The import graph can survive an edit. Rebind new source using the same npm contracts.
            if (!ReferenceEquals(workspace.Sources, result.Sources) && result.Diagnostics.Count != 0)
            {
                lock (_projectsLock)
                    if (project.SourceVersion != sourceVersion)
                        throw new OperationCanceledException("The project changed while loading npm metadata.");
                result = await LoadObservedNpmAsync(work);
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

        async Task<JoltWorkspace> LoadObservedNpmAsync(NpmWork work)
        {
            await project.MetadataGate.WaitAsync(_lifetime);
            try
            {
                lock (_projectsLock)
                    if (project.Npm != work || project.NpmKey != key)
                        throw new OperationCanceledException("A newer npm input superseded this request.");
                project.Declarations.Begin();
                var result = await JoltProjectLoader.LoadNpmAsync(project.Directory, workspace, project.MetadataProvider, _lifetime,
                    work.Progress);
                // Commit only observations from the still-current metadata load.
                lock (_projectsLock)
                    if (!_lifetime.IsCancellationRequested && project.Npm == work && project.NpmKey == key &&
                        project.DependencyVersion == dependencyVersion)
                        project.Declarations.Complete(result.Diagnostics.Count == 0);
                return result;
            }
            finally
            {
                project.MetadataGate.Release();
            }
        }
    }

    private static bool IsJazor(string path) => path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase);

    private static bool IsWithin(string directory, string path)
    {
        var relative = Path.GetRelativePath(directory, path);
        return relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !Path.IsPathRooted(relative);
    }

    private void PublishLinkedSources(ProjectState project)
    {
        var files = new HashSet<string>(PathComparer);
        var owners = new Dictionary<string, string>(PathComparer);
        for (var selection = project.CSharpSelection; selection is not null; selection = selection.Additional)
            foreach (var relative in selection.ExplicitPaths)
            {
                var path = Path.GetFullPath(relative, selection.Root);
                if (!IsWithin(project.Directory, path) || HasDifferentOwner(path))
                    files.Add(path);
            }
        if (project.LinkedSources.SetEquals(files))
            return;
        project.LinkedSources = files;
        _ = _transport.WriteAsync(new
        {
            jsonrpc = "2.0",
            method = "jolt/projectSources",
            @params = new { project = new Uri(project.Directory).AbsoluteUri, sources = files.Select(path => new Uri(path).AbsoluteUri).ToArray() },
        }, _lifetime);

        bool HasDifferentOwner(string path)
        {
            var directory = Path.GetDirectoryName(path)!;
            if (!owners.TryGetValue(directory, out var owner))
                owners[directory] = owner = ProjectDirectory(path);
            return !PathComparer.Equals(owner, project.Directory);
        }
    }

    private sealed class ProjectState
    {
        internal ProjectState(string directory)
        {
            Directory = directory;
            Declarations = new(directory);
            MetadataProvider = JoltProjectLoader.CreateDevMetadataProvider(Declarations.Observe);
        }

        internal string Directory { get; }
        internal JoltExternalDeclarations Declarations { get; }
        internal Jolt.Npm.NpmVueMetadataProvider MetadataProvider { get; }
        internal SemaphoreSlim MetadataGate { get; } = new(1);
        internal int SourceVersion { get; set; }
        internal IReadOnlyList<JoltSource>? Sources { get; set; }
        internal bool HasLoadedCSharpSelection { get; set; }
        internal HashSet<string> LinkedSources { get; set; } = new(PathComparer);
        internal bool RefreshDotNetInputs { get; set; } = true;
        internal CSharpSourceSelection? CSharpSelection { get; set; }
        internal JoltProjectCompiler.AnalysisState? Analysis { get; set; }
        internal int AnalysisDependencyVersion { get; set; } = -1;
        internal int AnalysisSourceVersion { get; set; } = -1;
        internal JoltProjectCompiler.AnalysisState? ProvisionalAnalysis { get; set; }
        internal int ProvisionalDependencyVersion { get; set; } = -1;
        internal int DependencyVersion { get; set; }
        internal VersionedProject? Version { get; set; }
        internal string? NpmKey { get; set; }
        internal NpmWork? Npm { get; set; }
        internal NpmWork? ClassifiedNpm { get; set; }
        internal CancellationTokenSource? Validation { get; set; }
    }

    private sealed class NpmWork(IReadOnlyList<JoltSource> authorSources, IReadOnlyList<JoltSource> csharpSources)
    {
        internal IReadOnlyList<JoltSource> AuthorSources { get; } = authorSources;
        internal IReadOnlyList<JoltSource> CSharpSources { get; } = csharpSources;
        internal JoltNpmLoadProgress Progress { get; } = new();
        internal Task<JoltWorkspace> Metadata { get; set; } = null!;

        internal async Task<T?> ReadPhaseAsync<T>(Task<T> phase) where T : class
        {
            // A canceled or superseded load may finish before reaching publication.
            await Task.WhenAny(phase, Metadata);
            return phase.IsCompletedSuccessfully ? phase.Result : null;
        }
    }

    private Task<ProjectSnapshot?> FocusedDocumentAsync(ProjectState project, VersionedProject version, string authorPath)
    {
        lock (_projectsLock)
        {
            if (project.Version != version)
                throw new OperationCanceledException("The project changed while loading the document.");
            if (version.FocusedDocuments.TryGetValue(authorPath, out var existing))
                return existing;
            var token = version.Cancellation.Token;
            return version.FocusedDocuments[authorPath] = Task.Run(async () =>
            {
                var workspace = await version.Sources;
                var source = workspace.Sources.FirstOrDefault(source => PathEquals(source.Path, authorPath));
                if (source is null || JoltProjectCompiler.ProjectEditorDocument(source, workspace.Sources, workspace.Options, token) is not { } document)
                    return (ProjectSnapshot?)null;
                return new ProjectSnapshot(Interlocked.Increment(ref _nextSnapshotId), project.Directory, workspace,
                    new JoltAnalysis([document], []), []);
            }, token);
        }
    }

    private sealed record VersionedProject(CancellationTokenSource Cancellation, Task<ProjectSnapshot>? Declarations, Task<JoltWorkspace> Sources)
    {
        internal Task<ProjectSnapshot> Input { get; set; } = null!;
        internal Task? FirstMemberRequest { get; set; }
        // A completion item keeps the exact same semantic model for resolve, even
        // after the project-wide render and npm snapshots become available.
        internal Dictionary<string, Task<ProjectSnapshot?>> FocusedDocuments { get; } = new(PathComparer);

        internal void Cancel()
        {
            Cancellation.Cancel();
            Cancellation.Dispose();
        }
    }

    private sealed record ProjectSnapshot(long Id, string Directory, JoltWorkspace Workspace, JoltAnalysis Analysis, HashSet<string> NpmImports)
    {
        internal Task<ProjectSnapshot>? Npm { get; init; }
        internal Task<IReadOnlyDictionary<string, IReadOnlyList<string>>?>? NpmCatalog { get; init; }
        internal Task<IReadOnlyList<NpmVueComponent>?>? NpmComponents { get; init; }
        internal NpmWork? NpmIdentity { get; init; }
    }
}
