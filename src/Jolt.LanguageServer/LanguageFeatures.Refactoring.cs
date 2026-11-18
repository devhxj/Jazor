using Jolt.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal sealed partial class LanguageFeatures
{
    private sealed record AuthorSpan(string Path, TextSpan Span);
    private sealed record RenameResult(IReadOnlyDictionary<string, TextChange[]> Changes, string? NewPath, bool ChangesExport);

    private JoltMarkupBinding? MarkupAt(int offset) =>
        projection.MarkupBindings.FirstOrDefault(binding => binding.Span.Contains(offset));

    private IEnumerable<AuthorSpan> MarkupReferences(JoltMarkupSymbol symbol, bool declarations) =>
        analysis.Documents.SelectMany(author => author.MarkupBindings
            .Where(binding => binding.Symbol == symbol && (declarations || !binding.IsDeclaration))
            .Select(binding => new AuthorSpan(author.Document.Path, binding.Span)));

    private IEnumerable<TextSpan?> MarkupDeclarations(JoltMarkupSymbol symbol, JoltProjectedDocument target) => symbol.Kind switch
    {
        JoltMarkupKind.Component => [new TextSpan(0, 0)],
        JoltMarkupKind.Property => [analysis.LocalComponents.Single(component => component.Path == symbol.ComponentPath)
            .Properties.Single(property => property.Name == symbol.Name).Source],
        _ => target.MarkupBindings.Where(binding => binding.Symbol == symbol && binding.IsDeclaration).Select(binding => (TextSpan?)binding.Span),
    };

    internal async Task<object?> RefactorAsync(string method, int offset, bool includeDeclaration, string? newName,
        bool fileRename, bool annotations, Func<string, int?> version, CancellationToken cancellationToken)
    {
        var binding = MarkupAt(offset);
        var site = GeneratedSite(offset, binding);
        using var roslyn = new AdhocWorkspace(Host.Value);
        Dictionary<string, Document>? documents = null;
        ISymbol? symbol = null;
        if (site is { } selected)
        {
            documents = await CreateRoslynProjectAsync(roslyn, cancellationToken);
            var model = (await documents[selected.Path].GetSemanticModelAsync(cancellationToken))!;
            symbol = SymbolAt(model, selected.Span.Start, cancellationToken, allowCandidate: false)?.OriginalDefinition;
            if (symbol?.Locations.Any(location => location.IsInSource && ToAuthor(location) is not null) != true)
                symbol = null;
        }
        var markupSymbol = binding?.Symbol ?? (symbol is IPropertySymbol ? ParameterSymbol(symbol) : null);
        if (markupSymbol is null && symbol is null)
            return null;

        if (method == "textDocument/references")
        {
            var references = new HashSet<AuthorSpan>();
            if (markupSymbol is not null)
                references.UnionWith(MarkupReferences(markupSymbol, includeDeclaration));
            if (includeDeclaration && markupSymbol is { Kind: JoltMarkupKind.Component })
                references.Add(new(markupSymbol.ComponentPath, default));
            if (symbol is not null)
            {
                var solution = documents![site!.Path].Project.Solution;
                foreach (var group in await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken))
                {
                    if (includeDeclaration && group.Definition is not IMethodSymbol { AssociatedSymbol: IPropertySymbol })
                        foreach (var declaration in group.Definition.Locations)
                            if (ToAuthor(declaration) is { } author)
                                references.Add(author);
                    foreach (var reference in group.Locations)
                        if (ToAuthor(reference.Location) is { } author)
                            references.Add(author);
                }
            }
            return references.OrderBy(reference => reference.Path, StringComparer.Ordinal).ThenBy(reference => reference.Span.Start)
                .Select(reference => Location(reference.Path, Text(reference.Path), reference.Span)).ToArray();
        }

        if (markupSymbol is { Kind: JoltMarkupKind.Component } && !fileRename)
            throw new ArgumentException("The editor must support RenameFile workspace edits to rename a .jazor component.");
        if (markupSymbol is { Kind: JoltMarkupKind.Slot, Name: "default" })
            throw new ArgumentException("The default slot is also used by implicit component children.");
        if (method == "textDocument/prepareRename")
        {
            var span = binding?.Span ?? ToSource(projection, site!.Span);
            return span is { } target ? new { range = Range(target), placeholder = _text.ToString(target) } : null;
        }

        var renamed = await RenameAsync(symbol, markupSymbol, documents, newName!, cancellationToken);
        var documentChanges = new List<object>();
        foreach (var (path, edits) in renamed.Changes)
        {
            var text = Text(path);
            var uri = new Uri(Path.GetFullPath(path, projectDirectory)).AbsoluteUri;
            documentChanges.Add(new
            {
                textDocument = new { uri, version = version(Path.GetFullPath(path, projectDirectory)) },
                edits = edits.Select(edit =>
                {
                    var range = Server.Range(text.Lines.GetLinePositionSpan(edit.Span));
                    return renamed.ChangesExport && annotations
                        ? (object)new { range, newText = edit.NewText, annotationId = "export" }
                        : new { range, newText = edit.NewText };
                }).ToArray(),
            });
        }
        if (renamed.NewPath is { } newPath)
            documentChanges.Add(new
            {
                kind = "rename",
                oldUri = new Uri(Path.GetFullPath(markupSymbol!.ComponentPath, projectDirectory)).AbsoluteUri,
                newUri = new Uri(Path.GetFullPath(newPath, projectDirectory)).AbsoluteUri,
            });

        return renamed.ChangesExport && annotations
            ? new { documentChanges, changeAnnotations = new Dictionary<string, object>
                {
                    ["export"] = new { label = "Rename a Jazor ESM named export", needsConfirmation = true,
                        description = "Update JavaScript imports that use this export." },
                } }
            : new { documentChanges };
    }

    private AuthorSpan? GeneratedSite(int offset, JoltMarkupBinding? binding)
    {
        if (binding?.Symbol is { Kind: JoltMarkupKind.Property } property)
        {
            var author = analysis.Documents.Single(document => document.Document.Path == property.ComponentPath);
            var declaration = MarkupDeclarations(property, author).Single();
            if (declaration is { } source && author.Spans.FirstOrDefault(span => span.Source.Contains(source)) is { } mapping)
                return new(property.ComponentPath, new TextSpan(mapping.Generated.Start + source.Start - mapping.Source.Start, source.Length));
            return null;
        }
        if (binding is not null || projection.Spans.FirstOrDefault(span => span.Source.Contains(offset)) is not { } current)
            return null;
        var generated = current.Generated.Start + offset - current.Source.Start;
        var token = projection.SemanticModel.SyntaxTree.GetRoot().FindToken(generated);
        return token.Span.Contains(generated) && token.IsKind(SyntaxKind.IdentifierToken)
            ? new(projection.Document.Path, token.Span)
            : null;
    }

    private JoltMarkupSymbol? ParameterSymbol(ISymbol property)
    {
        foreach (var location in property.Locations)
            if (ToAuthor(location) is { } author && analysis.LocalComponents.Any(component => component.Path == author.Path &&
                component.Properties.Any(parameter => parameter.Source == author.Span && parameter.Name == property.Name)))
                return new(author.Path, JoltMarkupKind.Property, property.Name);
        return null;
    }

    private AuthorSpan? ToAuthor(Location location)
    {
        if (location.SourceTree is not { } tree)
            return null;
        var author = analysis.Documents.FirstOrDefault(document => document.SemanticModel.SyntaxTree.FilePath == tree.FilePath);
        return author is not null && ToSource(author, location.SourceSpan) is { } span ? new(author.Document.Path, span) : null;
    }

    private SourceText Text(string path) => SourceText.From(workspace.Sources.Single(source => source.Path == path).Content);

    private async Task<RenameResult> RenameAsync(ISymbol? symbol, JoltMarkupSymbol? markup, Dictionary<string, Document>? documents,
        string newName, CancellationToken cancellationToken)
    {
        if (markup?.Kind == JoltMarkupKind.Slot ? newName == "default" || newName.Length == 0 || !newName.All(IsMarkupNameCharacter)
            : !SyntaxFacts.IsValidIdentifier(newName) || newName.StartsWith('@'))
            throw new ArgumentException($"'{newName}' is invalid for this rename.");

        var edits = new HashSet<(string Path, TextChange Change)>();
        if (symbol is not null)
        {
            var solution = documents!.Values.First().Project.Solution;
            var renamed = await Renamer.RenameSymbolAsync(solution, symbol, new SymbolRenameOptions(), newName, cancellationToken);
            foreach (var author in analysis.Documents)
            {
                var oldDocument = documents[author.Document.Path];
                var changed = renamed.GetDocument(oldDocument.Id)!;
                foreach (var change in await changed.GetTextChangesAsync(oldDocument, cancellationToken))
                {
                    // The projected file contains generated CLR members. Every Roslyn
                    // edit for an authored rename must fit fully inside copied source.
                    if (ToSource(author, change.Span) is not { } source)
                        throw new ArgumentException("The rename requires an edit outside the authored C#.");
                    edits.Add((author.Document.Path, new TextChange(source, change.NewText ?? "")));
                }
            }
        }
        if (markup is not null)
        {
            foreach (var site in MarkupReferences(markup, true))
            {
                var authored = Text(site.Path).ToString(site.Span);
                var replacement = markup.Kind == JoltMarkupKind.Property && authored.StartsWith("@on", StringComparison.Ordinal) &&
                    newName.StartsWith("on", StringComparison.OrdinalIgnoreCase) ? "@on" + newName[2..] : newName;
                edits.Add((site.Path, new TextChange(site.Span, replacement)));
            }
        }
        string? newPath = null;
        if (markup is { Kind: JoltMarkupKind.Component })
        {
            newPath = Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(markup.ComponentPath)!, newName), ".jazor").Replace('\\', '/');
            if (newPath == markup.ComponentPath)
                return new(new Dictionary<string, TextChange[]>(), null, false);
            if (workspace.Sources.Any(source => source.Path != markup.ComponentPath && Server.PathEquals(source.Path, newPath)) ||
                !Server.PathEquals(markup.ComponentPath, newPath) && File.Exists(Path.GetFullPath(newPath, projectDirectory)))
                throw new ArgumentException($"A component already exists at '{newPath}'.");
        }
        var changes = edits.GroupBy(edit => edit.Path, StringComparer.Ordinal).ToDictionary(group => group.Key,
            group => group.Select(edit => edit.Change).OrderBy(edit => edit.Span.Start).ToArray(), StringComparer.Ordinal);
        var updated = workspace.Sources.Select(source =>
        {
            if (changes.TryGetValue(source.Path, out var change))
                source = source with { Content = SourceText.From(source.Content).WithChanges(change).ToString() };
            return newPath is not null && source.Path == markup!.ComponentPath ? source with { Path = newPath } : source;
        });
        var rebound = JoltProjectCompiler.Analyze(updated, workspace.Options, cancellationToken);
        var existing = analysis.Diagnostics.Select(diagnostic => (diagnostic.Id, diagnostic.GetMessage())).ToHashSet();
        var error = rebound.Diagnostics.FirstOrDefault(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
            !existing.Contains((diagnostic.Id, diagnostic.GetMessage())));
        if (error is not null)
            throw new ArgumentException(error.GetMessage());
        // A valid compilation can still rebind a tag or attribute after an insensitive
        // property match or a namespace collision. Validate all existing markup bindings.
        foreach (var author in analysis.Documents)
        {
            var updatedPath = newPath is not null && author.Document.Path == markup!.ComponentPath ? newPath : author.Document.Path;
            var current = rebound.Documents.Single(document => document.Document.Path == updatedPath);
            var bound = current.MarkupBindings.ToLookup(binding => binding.Span.Start);
            foreach (var original in author.MarkupBindings)
            {
                int start = original.Span.Start;
                if (changes.TryGetValue(author.Document.Path, out var authorChanges))
                    start += authorChanges.TakeWhile(change => change.Span.End <= start)
                        .Sum(change => change.NewText!.Length - change.Span.Length);
                var expected = original.Symbol;
                if (newPath is not null && expected.ComponentPath == markup!.ComponentPath)
                    expected = expected with { ComponentPath = newPath };
                if (original.Symbol == markup && markup.Kind != JoltMarkupKind.Component)
                    expected = expected with { Name = newName };
                if (!bound[start].Any(candidate => candidate.Symbol == expected && candidate.IsDeclaration == original.IsDeclaration))
                    throw new ArgumentException($"The rename would change a component binding in '{author.Document.Path}'.");
            }
        }
        return new(changes, newPath, symbol is { IsStatic: true, DeclaredAccessibility: Accessibility.Public, ContainingType.IsStatic: true }
            and (IMethodSymbol { MethodKind: MethodKind.Ordinary } or IFieldSymbol));
    }
}
