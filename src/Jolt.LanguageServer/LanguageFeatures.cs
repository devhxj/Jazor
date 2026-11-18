using Jolt.Compiler;
using Jolt.Npm;
using Jolt.Project;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal sealed class LanguageFeatures(
    JoltProjectedDocument projection,
    IReadOnlyList<JoltProjectedDocument> documents,
    JoltWorkspace workspace,
    string projectDirectory)
{
    private readonly SourceText _text = SourceText.From(projection.Document.Source);
    private static readonly Lazy<MefHostServices> Host = new(() => MefHostServices.Create(
        MefHostServices.DefaultAssemblies.Append(System.Reflection.Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")).Distinct()));
    private static readonly string[] HtmlTags =
        ["a", "article", "button", "div", "form", "h1", "h2", "input", "label", "li", "main", "nav", "ol", "p", "section", "span", "textarea", "ul"];

    public async Task<object?> GetAsync(string method, int offset, CancellationToken cancellationToken)
    {
        var mapping = projection.Spans.FirstOrDefault(span => offset >= span.Source.Start && offset <= span.Source.End);
        if (mapping is not null)
        {
            var generated = mapping.Generated.Start + Math.Min(offset - mapping.Source.Start, mapping.Generated.Length);
            return method switch
            {
                "textDocument/completion" => await CSharpCompletionAsync(generated, cancellationToken),
                "textDocument/hover" => SymbolAt(generated, cancellationToken) is { } symbol
                    ? Hover(symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                    : null,
                "textDocument/definition" => CSharpDefinition(generated, cancellationToken),
                "textDocument/signatureHelp" => SignatureHelp(generated, cancellationToken),
                _ => null,
            };
        }

        var elements = Elements(projection.Document.Nodes).Where(node => node.Span.Start <= offset && offset <= node.Span.End);
        var element = elements.OrderBy(node => node.Span.Length).FirstOrDefault();
        var attribute = element?.Attributes.FirstOrDefault(item => item.Span.Start <= offset && offset <= item.Span.End);
        var component = Component(element?.Name);
        var local = LocalComponent(element?.Name);
        var parentComponent = elements.Where(node => node != element).OrderBy(node => node.Span.Length)
            .Select(node => Component(node.Name)).FirstOrDefault(candidate => candidate is not null);
        if (method == "textDocument/completion")
            return MarkupCompletion(offset, element, attribute, component, parentComponent, local);
        if (element is null)
            return null;
        if (attribute is not null && local is not null &&
            local.Properties.FirstOrDefault(property => property.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase)) is { } property)
            return method == "textDocument/hover"
                ? Hover($"{property.Name}: {property.Type}")
                : LocalLocation(local, property.Source);

        if (attribute is not null && component is not null)
        {
            var prop = component.Props.FirstOrDefault(item => Pascal(item.Key) == attribute.Name || item.Key == attribute.Name);
            if (prop.Key is not null)
                return method == "textDocument/hover" ? Hover($"{attribute.Name}: {prop.Value.Type}") : FromNpm(prop.Value.Definition);
            var evt = component.Events.FirstOrDefault(item => EventName(item.Key).Equals(attribute.Name, StringComparison.OrdinalIgnoreCase));
            if (evt.Key is not null)
                return method == "textDocument/hover" ? Hover(Signatures(evt.Key, evt.Value.Signatures)) : FromNpm(evt.Value.Definition);
        }
        if (element.Name == "template" && attribute is { Name.Length: > 1 } && attribute.Name[0] == '#' &&
            parentComponent is not null && parentComponent.Slots.TryGetValue(attribute.Name[1..], out var slot))
            return method == "textDocument/hover" ? Hover(Signatures(attribute.Name, slot.Signatures)) : FromNpm(slot.Definition);
        if (!element.NameSpan.Contains(offset))
            return null;
        if (component is not null)
            return method == "textDocument/hover"
                ? Hover($"{component.ExportName} from {component.ModuleSpecifier}@{component.Version}")
                : FromNpm(component.Definition);
        return local is null ? null : method == "textDocument/hover"
            ? Hover(element.Name)
            : LocalLocation(local, default);
    }

    private async Task<object[]> CSharpCompletionAsync(int position, CancellationToken cancellationToken)
    {
        var model = projection.SemanticModel;
        using var workspace = new AdhocWorkspace(Host.Value);
        var identity = ProjectId.CreateNewId();
        var info = ProjectInfo.Create(identity, VersionStamp.Create(), "Jolt", model.Compilation.AssemblyName!, LanguageNames.CSharp,
            parseOptions: model.SyntaxTree.Options, compilationOptions: model.Compilation.Options, metadataReferences: model.Compilation.References);
        var project = workspace.AddProject(info);
        var documentId = DocumentId.CreateNewId(identity);
        var solution = project.Solution.AddDocument(documentId, "Projection.cs", await model.SyntaxTree.GetTextAsync(cancellationToken));
        foreach (var tree in model.Compilation.SyntaxTrees.Where(tree => tree != model.SyntaxTree))
            solution = solution.AddDocument(DocumentId.CreateNewId(identity), Path.GetFileName(tree.FilePath), await tree.GetTextAsync(cancellationToken));
        var document = solution.GetDocument(documentId)!;
        var service = CompletionService.GetService(document);
        if (service is null)
            return [];
        var result = await service.GetCompletionsAsync(document, position, cancellationToken: cancellationToken);
        if (result is null)
            return [];
        var items = new List<object>();
        foreach (var item in result.ItemsList.Where(item => !item.DisplayText.StartsWith("__Jolt", StringComparison.Ordinal)).Take(100))
        {
            var change = (await service.GetChangeAsync(document, item, cancellationToken: cancellationToken)).TextChange;
            if (ToSource(change.Span) is { } span)
                items.Add(new { label = item.DisplayText, textEdit = new { range = Range(span), newText = change.NewText } });
        }
        return items.ToArray();
    }

    private ISymbol? SymbolAt(int position, CancellationToken cancellationToken)
    {
        var model = projection.SemanticModel;
        var node = model.SyntaxTree.GetRoot(cancellationToken).FindToken(position).Parent;
        for (; node is not null; node = node.Parent)
        {
            if (node is SimpleNameSyntax or MemberAccessExpressionSyntax)
            {
                var info = model.GetSymbolInfo(node, cancellationToken);
                return info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
            }
            if (model.GetDeclaredSymbol(node, cancellationToken) is { } symbol)
                return symbol;
        }
        return null;
    }

    private object? CSharpDefinition(int position, CancellationToken cancellationToken)
    {
        if (SymbolAt(position, cancellationToken) is not { } symbol)
            return null;
        foreach (var location in symbol.Locations.Where(location => location.IsInSource))
            if (documents.FirstOrDefault(document => document.SemanticModel.SyntaxTree == location.SourceTree) is { } author &&
                ToSource(author, location.SourceSpan) is { } span)
                return Location(author.Document.Path, SourceText.From(author.Document.Source), span);
        return null;
    }

    private object? SignatureHelp(int position, CancellationToken cancellationToken)
    {
        var model = projection.SemanticModel;
        var token = model.SyntaxTree.GetRoot(cancellationToken).FindToken(Math.Max(0, position - 1), findInsideTrivia: true);
        var call = token.Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (call is null)
            return null;
        var methods = model.GetMemberGroup(call.Expression, cancellationToken).OfType<IMethodSymbol>().ToArray();
        return methods.Length == 0 ? null : new
        {
            signatures = methods.Select(method => new
            {
                label = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                parameters = method.Parameters.Select(parameter => new { label = parameter.ToDisplayString() }),
            }),
            activeSignature = 0,
            activeParameter = call.ArgumentList.Arguments.GetSeparators().Count(separator => separator.SpanStart < position),
        };
    }

    private object[] MarkupCompletion(int offset, JoltElement? element, JoltAttribute? attribute, NpmVueComponent? component,
        NpmVueComponent? parentComponent, JoltLocalComponent? local)
    {
        var prefixStart = offset;
        while (prefixStart > 0 && IsMarkupNameCharacter(_text[prefixStart - 1]))
            prefixStart--;
        var replacementEnd = offset;
        while (replacementEnd < _text.Length && IsMarkupNameCharacter(_text[replacementEnd]))
            replacementEnd++;
        var prefix = TextSpan.FromBounds(prefixStart, replacementEnd);
        if (prefixStart > 0 && _text[prefixStart - 1] == '<')
        {
            var names = HtmlTags.Concat(projection.LocalComponents.Select(component => component.Name)).ToList();
            foreach (var (module, exports) in workspace.Exports.Where(item => UsesPackage(item.Key)))
                names.AddRange(exports.Select(export => NpmNames.TagFor(module, export)));
            return names.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                .Select(name => Completion(name, prefix, detail: null, kind: 7)).ToArray();
        }
        if (element?.Name == "template" && prefixStart > 0 && _text[prefixStart - 1] == '#' && parentComponent is not null)
            return parentComponent.Slots.Select(item => Completion(item.Key, prefix, Signatures(item.Key, item.Value.Signatures), 10)).ToArray();
        if (local is not null)
            return local.Properties.Select(property => Completion(property.Name, prefix, property.Type, 10)).ToArray();
        if (element is null || component is null)
            return [];
        if (attribute is not null && attribute.NameSpan.End < offset)
        {
            var prop = component.Props.FirstOrDefault(item => Pascal(item.Key) == attribute.Name || item.Key == attribute.Name);
            return prop.Value?.LiteralValues.Select(value => Completion(value, prefix, prop.Value.Type, 12)).ToArray() ?? [];
        }

        var props = component.Props.Select(item => Completion(Pascal(item.Key), prefix, item.Value.Type, 10));
        var events = component.Events.Select(item => Completion(EventName(item.Key), prefix, Signatures(item.Key, item.Value.Signatures), 23));
        return props.Concat(events).ToArray();
    }

    private object Completion(string name, TextSpan span, string? detail, int kind) => new
    {
        label = name, detail, kind, textEdit = new { range = Range(span), newText = name },
    };

    private NpmVueComponent? Component(string? name) =>
        workspace.Options.NpmComponents?.FirstOrDefault(component => NpmNames.TagFor(component.ModuleSpecifier, component.ExportName) == name && UsesPackage(component.ModuleSpecifier));

    private JoltLocalComponent? LocalComponent(string? name)
    {
        var matches = projection.LocalComponents.Where(component => component.Name == name).Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private object? LocalLocation(JoltLocalComponent component, TextSpan? span) =>
        workspace.Sources.FirstOrDefault(source => source.Path == component.Path) is { } author
            ? Location(author.Path, SourceText.From(author.Content), span ?? default)
            : null;

    private bool UsesPackage(string package)
    {
        var @namespace = NpmNames.NamespaceFor(package);
        return projection.Document.Nodes.OfType<JoltUsing>().Any(use => use.Code.Trim().TrimEnd(';') == @namespace);
    }

    private static string EventName(string name) => "on" + Pascal(name);
    private static bool IsMarkupNameCharacter(char character) => char.IsLetterOrDigit(character) || character is '-' or '_' or ':';

    private static string Pascal(string name) => name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..];

    private static string Signatures(string name, IReadOnlyList<IReadOnlyList<NpmVueParameter>> signatures) =>
        string.Join("\n", signatures.Select(signature => $"{name}({string.Join(", ", signature.Select(p => $"{p.Name}: {p.Type}"))})"));

    private static IEnumerable<JoltElement> Elements(IEnumerable<JoltNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is JoltElement element)
                yield return element;
            IEnumerable<JoltNode> children = node switch
            {
                JoltElement container => container.Children,
                JoltIf conditional => conditional.Then.Concat(conditional.Else),
                JoltForEach loop => loop.Children,
                _ => [],
            };
            foreach (var descendant in Elements(children))
                yield return descendant;
        }
    }

    private TextSpan? ToSource(TextSpan generated) => ToSource(projection, generated);

    private static TextSpan? ToSource(JoltProjectedDocument document, TextSpan generated)
    {
        foreach (var span in document.Spans)
            if (span.Generated.Contains(generated) && generated.End - span.Generated.Start <= span.Source.Length)
                return new TextSpan(span.Source.Start + generated.Start - span.Generated.Start, generated.Length);
        return null;
    }

    private object Range(TextSpan span) => Server.Range(_text.Lines.GetLinePositionSpan(span));
    private static object Hover(string text) => new { contents = new { kind = "plaintext", value = text } };

    private object? FromNpm(NpmDefinition? definition) =>
        definition is not null && File.Exists(definition.Path)
            ? Location(definition.Path, SourceText.From(File.ReadAllText(definition.Path)), new TextSpan(definition.Start, definition.Length))
            : null;

    private object Location(string path, SourceText source, TextSpan span) =>
        new { uri = new Uri(Path.GetFullPath(path, projectDirectory)).AbsoluteUri, range = Server.Range(source.Lines.GetLinePositionSpan(span)) };
}
