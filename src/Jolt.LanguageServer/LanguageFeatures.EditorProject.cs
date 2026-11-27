using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal sealed partial class LanguageFeatures
{
    private static readonly Lazy<AdhocWorkspace> EditorWorkspace = new(() => new(Host.Value));
    private static readonly object EditorProjectLock = new();
    private static readonly ConditionalWeakTable<Compilation, EditorProject> EditorProjects = new();
    private static readonly ConditionalWeakTable<PortableExecutableReference, PortableExecutableReference> EditorReferences = new();
    private static readonly Lazy<Task<Microsoft.CodeAnalysis.Project>> BaseEditorProject = new(CreateBaseProjectAsync);
    // Keep one project available when the server has already replaced its compiler
    // snapshot. The weak table ties all other solutions to their compiler snapshot.
    private static EditorProject? _latestEditorProject;

    internal static void PrepareEditor()
    {
        // The immutable platform reference set is independent of
        // the open project. Warm it while the client initializes.
        _ = Task.Run(() => Jolt.Compiler.JoltProjectCompiler.PlatformReferences.Value);
    }

    internal static void PrepareWorkspaceEditor() => _ = Task.Run(() => BaseEditorProject.Value);

    private static async Task<Microsoft.CodeAnalysis.Project> CreateBaseProjectAsync()
    {
        var workspace = EditorWorkspace.Value;
        _ = workspace.Services.GetLanguageServices(LanguageNames.CSharp).GetService<CompletionService>();
        var info = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Jolt", "JoltComponents", LanguageNames.CSharp,
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable),
            metadataReferences: Jolt.Compiler.JoltProjectCompiler.PlatformReferences.Value.Select(EditorReference));
        var project = workspace.CurrentSolution.AddProject(info).GetProject(info.Id)!;
        // Subsequent documents fork this project. Bind its stable metadata graph once
        // while the client opens the file; Roslyn can carry it across syntax-only edits.
        var compilation = (await project.GetCompilationAsync().ConfigureAwait(false))!;
        _ = compilation.GetSpecialType(SpecialType.System_Object);
        return project;
    }

    private static MetadataReference EditorReference(MetadataReference reference) => reference is PortableExecutableReference file
        ? EditorReferences.GetValue(file, static reference =>
        {
            var xml = Path.ChangeExtension(reference.FilePath, ".xml");
            return File.Exists(xml) && reference.GetMetadata() is AssemblyMetadata metadata
                ? metadata.GetReference(XmlDocumentationProvider.CreateFromFile(xml!), reference.Properties.Aliases,
                    reference.Properties.EmbedInteropTypes, reference.FilePath, reference.Display) : reference;
        })
        : reference;

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
        var references = compilation.References.Select(EditorReference).ToArray();
        var project = previous?.Project ?? BaseEditorProject.Value.GetAwaiter().GetResult();
        if (project.AssemblyName != compilation.AssemblyName ||
            !project.ParseOptions!.Equals(model.SyntaxTree.Options) ||
            !project.CompilationOptions!.Equals(compilation.Options) ||
            !project.MetadataReferences.SequenceEqual(references))
        {
            previous = null;
            var info = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Jolt",
                compilation.AssemblyName!, LanguageNames.CSharp, parseOptions: model.SyntaxTree.Options,
                compilationOptions: compilation.Options, metadataReferences: references);
            project = EditorWorkspace.Value.CurrentSolution.AddProject(info).GetProject(info.Id)!;
        }

        var solution = project.Solution;
        var trees = new Dictionary<(string Path, int Index), (DocumentId Id, SyntaxTree Tree)>();
        var ids = new Dictionary<SyntaxTree, DocumentId>();
        var indexes = new Dictionary<string, int>(StringComparer.Ordinal);
        var projectedPaths = analysis.Documents.Select(document => document.SemanticModel.SyntaxTree.FilePath)
            .ToHashSet(StringComparer.Ordinal);
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
                var root = tree.GetRoot(cancellationToken);
                // Lifted static extension methods live on the generated module. Let Roslyn
                // index those trees for import completion along with the shared .cs files.
                var generated = projectedPaths.Contains(tree.FilePath) &&
                    !root.DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax)
                        .Any(node => node is ExtensionBlockDeclarationSyntax || node is MethodDeclarationSyntax method &&
                            method.ParameterList.Parameters.FirstOrDefault()?.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ThisKeyword) == true);
                solution = solution.AddDocument(id, Path.GetFileName(tree.FilePath), root,
                    filePath: tree.FilePath, isGenerated: generated, preservationMode: PreservationMode.PreserveIdentity);
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
