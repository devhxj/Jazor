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

        var project = context.Document.Project;
        var options = project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!options.TryGetValue("build_property.MSBuildProjectFullPath", out var projectPath) || string.IsNullOrEmpty(projectPath))
            projectPath = project.FilePath;
        if (string.IsNullOrEmpty(projectPath) || Path.GetDirectoryName(projectPath) is not { Length: > 0 } directory)
            return;

        foreach (var parent in Parents(directory))
        {
            var path = Path.Combine(parent.FullName, "package.json");
            var manifest = project.AdditionalDocuments.FirstOrDefault(file => file.FilePath is { } filePath &&
                NpmEditorManifest.SamePath(filePath, path));
            if (manifest is null)
                continue;

            // A Balanced completion request can retain the old AdditionalDocument after
            // a package manager writes the manifest. Only an open buffer takes priority over disk.
            var workspace = project.Solution.Workspace;
            string manifestText;
            if (workspace.IsDocumentOpen(manifest.Id) && workspace.CurrentSolution.GetAdditionalDocument(manifest.Id) is { } open)
                manifestText = (await open.GetTextAsync(token).ConfigureAwait(false)).ToString();
            else
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    using var reader = new StreamReader(stream);
                    manifestText = await reader.ReadToEndAsync().ConfigureAwait(false);
                }
                catch (FileNotFoundException) { continue; }
                catch (DirectoryNotFoundException) { continue; }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    return; // The manifest may be replaced or removed during an install.
                }
            }
            var packages = NpmEditorManifest.ReadPackageNames(manifestText);
            var namespaces = NpmEditorManifest.RootNamespaces(packages, (CSharpCompilation)model.Compilation,
                new HashSet<string>(packages.Where(name => NpmEditorManifest.HasPackageRoot(parent.FullName, name)),
                    StringComparer.Ordinal));
            var children = namespaces.Where(name => name.StartsWith(prefix + ".", StringComparison.Ordinal))
                .Select(name => name.Substring(prefix.Length + 1).Split('.')[0]).ToList();
            if (namespaces.Contains(prefix, StringComparer.Ordinal) && !children.Contains("Exports", StringComparer.Ordinal))
                children.Add("Exports");

            foreach (var child in children.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal))
                if (existing is null || !existing.GetMembers(child).Any())
                    context.AddItem(CompletionItem.Create(Escape(child)));
            return;
        }
    }

    private static IEnumerable<DirectoryInfo> Parents(string directory)
    {
        for (var parent = new DirectoryInfo(directory); parent is not null; parent = parent.Parent)
            yield return parent;
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
