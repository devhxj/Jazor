using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.Npm.Editor;

internal sealed partial class NpmCompletionProvider
{
    private static async Task AddPackageCompletionsAsync(CompletionContext context, SemanticModel model, NameSyntax receiver)
    {
        var prefix = NamespacePath(receiver);
        if (prefix is null || prefix != "Npm" && !prefix.StartsWith("Npm.", StringComparison.Ordinal))
            return;

        var token = context.CancellationToken;
        var info = model.GetSymbolInfo(receiver, token);
        var existing = info.Symbol as INamespaceSymbol;
        if (existing is not null ? existing.ToDisplayString() != prefix :
            info.Symbol is not null and not IErrorTypeSymbol || info.CandidateSymbols.Length != 0)
            return;

        var document = context.Document;
        var project = document.Project;
        if (FindCompletionWorkspace(document, (CSharpCompilation)model.Compilation, receiver.GetLocation(), token) is not { } workspaceRoot)
            return;

        var path = workspaceRoot.Manifest;
        var manifest = project.AdditionalDocuments.FirstOrDefault(file => file.FilePath is { } filePath &&
            NpmEditorManifest.SamePath(filePath, path));
        // A Balanced completion request can retain the old AdditionalDocument after
        // a package manager writes the manifest. Only an open buffer takes priority over disk.
        var workspace = project.Solution.Workspace;
        string manifestText;
        if (manifest is not null && workspace.IsDocumentOpen(manifest.Id) &&
            workspace.CurrentSolution.GetAdditionalDocument(manifest.Id) is { } open)
            manifestText = (await open.GetTextAsync(token).ConfigureAwait(false)).ToString();
        else
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                manifestText = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return; // The manifest may be replaced or removed during an install.
            }
        }
        var directory = Path.GetDirectoryName(path)!;
        var packages = NpmEditorManifest.ReadPackageNames(manifestText);
        var namespaces = NpmEditorManifest.RootNamespaces(packages, (CSharpCompilation)model.Compilation,
            new HashSet<string>(packages.Where(name => NpmEditorManifest.HasPackageRoot(directory, name)),
                StringComparer.Ordinal));
        var children = namespaces.Where(name => name.StartsWith(prefix + ".", StringComparison.Ordinal))
            .Select(name => name.Substring(prefix.Length + 1).Split('.')[0]).ToList();
        if (namespaces.Contains(prefix, StringComparer.Ordinal) && !children.Contains("Exports", StringComparer.Ordinal))
            children.Add("Exports");

        foreach (var child in children.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal))
            if (existing is null || !existing.GetMembers(child).Any())
                context.AddItem(CompletionItem.Create(Escape(child)));
    }

    private static (string Project, string Manifest)? FindCompletionWorkspace(Document document, CSharpCompilation compilation,
        Location completion, CancellationToken cancellationToken)
    {
        var options = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!options.TryGetValue("build_property._JazorNpmEditorTool", out var host) || string.IsNullOrEmpty(host))
            return null;
        if (!options.TryGetValue("build_property.MSBuildProjectFullPath", out var project) || string.IsNullOrEmpty(project))
            project = document.Project.FilePath;
        if (string.IsNullOrEmpty(project))
            return null;

        if (NpmWorkspace.FindProjectManifest(project!, File.Exists) is { } projectManifest)
            return (project!, projectManifest);

        // Previously generated facades describe the last workspace. Remove them when checking
        // today's source imports so they cannot conceal a change of workspace ownership.
        var source = compilation.RemoveSyntaxTrees(compilation.SyntaxTrees.Where(tree =>
            Path.GetFileName(tree.FilePath) == NpmSourceRequests.EditorSourceHint));
        var requests = NpmSourceRequests.Discover(source, packages: null, cancellationToken);
        var selection = NpmWorkspace.Select(project!, requests, File.Exists);
        if (selection.Conflicts.Count != 0)
            return null;

        // During `using Npm.` the incomplete qualified name cannot yet select a package.
        // Resolve that one source location with the same bounds as a complete import.
        var manifest = selection.Manifest ?? NpmWorkspace.Select(project!, [new("Npm", null, completion)], File.Exists).Manifest;
        return manifest is null ? null : (project!, manifest);
    }

    private static string Escape(string name) => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
        SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

    private static string? NamespacePath(NameSyntax name) => name switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        AliasQualifiedNameSyntax alias when alias.Alias.Identifier.Text == "global" => NamespacePath(alias.Name),
        QualifiedNameSyntax qualified when NamespacePath(qualified.Left) is { } left &&
            NamespacePath(qualified.Right) is { } right => left + "." + right,
        _ => null,
    };
}
