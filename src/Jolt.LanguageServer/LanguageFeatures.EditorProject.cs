using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal sealed partial class LanguageFeatures
{
    private static readonly Lazy<AdhocWorkspace> EditorWorkspace = new(() => new(Host.Value));
    private static readonly object EditorProjectLock = new();
    private static readonly ConditionalWeakTable<Compilation, EditorProject> EditorProjects = new();
    // Keep one project available when the server has already replaced its compiler
    // snapshot. The weak table ties all other solutions to their compiler snapshot.
    private static EditorProject? _latestEditorProject;

    private sealed record EditorProject(string Directory, long SnapshotId, Microsoft.CodeAnalysis.Project Project,
        Dictionary<(string Path, int Index), (DocumentId Id, SyntaxTree Tree)> Trees,
        Dictionary<SyntaxTree, DocumentId> Ids)
    {
        internal Document GetDocument(SyntaxTree tree) => Project.GetDocument(Ids[tree])!;
    }

    private EditorProject GetEditorProject(CancellationToken cancellationToken)
    {
        lock (EditorProjectLock)
        {
            var model = projection.SemanticModel;
            if (EditorProjects.TryGetValue(model.Compilation, out var cached))
                return cached;
            var latest = _latestEditorProject;
            var previous = latest?.Directory == projectDirectory ? latest : null;
            var project = UpdateEditorProject(model, previous, cancellationToken);
            EditorProjects.Add(model.Compilation, project);
            // In-flight requests retain their own immutable solution. An older request
            // must not replace the Roslyn state used by the latest document version.
            if (latest is null || snapshotId > latest.SnapshotId)
                _latestEditorProject = project;
            return project;
        }
    }

    private EditorProject UpdateEditorProject(SemanticModel model, EditorProject? previous,
        CancellationToken cancellationToken)
    {
        var compilation = model.Compilation;
        var project = previous?.Project;
        if (project is null || project.AssemblyName != compilation.AssemblyName ||
            !project.ParseOptions!.Equals(model.SyntaxTree.Options) ||
            !project.CompilationOptions!.Equals(compilation.Options) ||
            !project.MetadataReferences.SequenceEqual(compilation.References))
        {
            previous = null;
            var info = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Jolt",
                compilation.AssemblyName!, LanguageNames.CSharp, parseOptions: model.SyntaxTree.Options,
                compilationOptions: compilation.Options, metadataReferences: compilation.References);
            project = EditorWorkspace.Value.CurrentSolution.AddProject(info).GetProject(info.Id)!;
        }

        var solution = project.Solution;
        var trees = new Dictionary<(string Path, int Index), (DocumentId Id, SyntaxTree Tree)>();
        var ids = new Dictionary<SyntaxTree, DocumentId>();
        var indexes = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var tree in compilation.SyntaxTrees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var index = indexes.GetValueOrDefault(tree.FilePath);
            indexes[tree.FilePath] = index + 1;
            var key = (tree.FilePath, index);
            DocumentId id;
            if (previous is not null && previous.Trees.TryGetValue(key, out var prior))
            {
                id = prior.Id;
                // The compiler may produce a new tree for an unchanged projected source.
                // Preserve Roslyn's document and compilation state when the text is identical.
                if (tree != prior.Tree && !tree.GetText(cancellationToken).ContentEquals(prior.Tree.GetText(cancellationToken)))
                {
                    var original = solution.GetDocument(id)!;
                    var current = tree.GetText(cancellationToken);
                    // A related SourceText lets Workspace preserve the incremental parse and
                    // the declaration version when only a method body or expression changed.
                    solution = original.TryGetText(out var text)
                        ? solution.WithDocumentText(id, WithTextChange(text, current), PreservationMode.PreserveIdentity)
                        : solution.WithDocumentSyntaxRoot(id, tree.GetRoot(cancellationToken), PreservationMode.PreserveIdentity);
                }
            }
            else
            {
                id = DocumentId.CreateNewId(project.Id);
                solution = solution.AddDocument(id, Path.GetFileName(tree.FilePath), tree.GetRoot(cancellationToken),
                    filePath: tree.FilePath, preservationMode: PreservationMode.PreserveIdentity);
            }
            trees.Add(key, (id, tree));
            ids.Add(tree, id);
        }
        if (previous is not null)
        {
            var removed = previous.Trees.Where(entry => !trees.ContainsKey(entry.Key)).Select(entry => entry.Value.Id).ToImmutableArray();
            if (removed.Length > 0)
                solution = solution.RemoveDocuments(removed);
        }
        return new(projectDirectory, snapshotId, solution.GetProject(project.Id)!, trees, ids);
    }

    private static SourceText WithTextChange(SourceText previous, SourceText current)
    {
        var start = 0;
        var suffix = 0;
        var sharedLength = Math.Min(previous.Length, current.Length);
        while (start < sharedLength && previous[start] == current[start])
            start++;
        while (suffix < sharedLength - start && previous[previous.Length - suffix - 1] == current[current.Length - suffix - 1])
            suffix++;
        return previous.WithChanges(new TextChange(TextSpan.FromBounds(start, previous.Length - suffix),
            current.ToString(TextSpan.FromBounds(start, current.Length - suffix))));
    }

    private async Task<Document> CompletionDocumentAsync(SemanticModel model, EditorProject project, int? trailingDot,
        CancellationToken cancellationToken)
    {
        if (!project.Ids.ContainsKey(model.SyntaxTree))
            project = UpdateEditorProject(model, project, cancellationToken);
        var document = project.GetDocument(model.SyntaxTree);
        if (trailingDot is not { } dot)
            return document;
        var text = await document.GetTextAsync(cancellationToken);
        return document.WithText(text.WithChanges(new TextChange(new TextSpan(dot, 0), ".")));
    }
}
