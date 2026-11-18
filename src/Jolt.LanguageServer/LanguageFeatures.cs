using System.Text.Json;
using System.Text.Json.Nodes;
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
using Microsoft.CodeAnalysis.Tags;

namespace Jolt.LanguageServer;

internal sealed partial class LanguageFeatures(
    JoltProjectedDocument projection,
    JoltAnalysis analysis,
    JoltWorkspace workspace,
    string projectDirectory)
{
    private readonly SourceText _text = SourceText.From(projection.Document.Source);
    private static readonly Lazy<MefHostServices> Host = new(() => MefHostServices.Create(
        MefHostServices.DefaultAssemblies.Append(System.Reflection.Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")).Distinct()));
    private static readonly string[] HtmlTags =
        ["a", "article", "button", "div", "form", "h1", "h2", "input", "label", "li", "main", "nav", "ol", "p", "section", "span", "textarea", "ul"];

    public async Task<object?> GetAsync(string method, int offset, CancellationToken cancellationToken, int? version = null)
    {
        if (method == "textDocument/definition" && MarkupAt(offset) is { } binding)
        {
            var target = analysis.Documents.Single(document => document.Document.Path == binding.Symbol.ComponentPath);
            var declarations = MarkupDeclarations(binding.Symbol, target);
            if (declarations.FirstOrDefault() is { } declaration)
                return Location(target.Document.Path, SourceText.From(target.Document.Source), declaration);
        }
        var mapping = projection.Spans.FirstOrDefault(span => offset >= span.Source.Start && offset <= span.Source.End);
        if (mapping is null && method == "textDocument/completion" && offset > 0 && _text[offset - 1] == '.' &&
            projection.Spans.FirstOrDefault(span => span.Source.Length > 0 && span.Source.End == offset - 1) is { } receiver)
        {
            // Razor leaves a trailing dot outside an implicit expression until the member has an identifier.
            return await CSharpCompletionAsync(receiver.Generated.End + 1, cancellationToken, version, offset);
        }
        if (mapping is not null)
        {
            var generated = mapping.Generated.Start + Math.Min(offset - mapping.Source.Start, mapping.Generated.Length);
            return method switch
            {
                "textDocument/completion" => await CSharpCompletionAsync(generated, cancellationToken, version),
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
        var parent = elements.Where(node => node != element).OrderBy(node => node.Span.Length).FirstOrDefault();
        var parentComponent = Component(parent?.Name);
        if (method == "textDocument/completion")
            return MarkupCompletion(offset, element, attribute, component, parentComponent, local, LocalComponent(parent?.Name));
        if (element is null)
            return null;
        if (method == "textDocument/hover" && attribute is not null && local is not null &&
            MarkupAt(offset)?.Symbol is { Kind: JoltMarkupKind.Property } member &&
            local.Properties.FirstOrDefault(property => property.Name == member.Name) is { } property)
            return Hover($"{property.Name}: {property.Type}");

        if (attribute is not null && component is not null)
        {
            var prop = component.Props.FirstOrDefault(item => MatchesProp(attribute.Name, item.Key));
            if (prop.Key is not null)
                return method == "textDocument/hover" ? Hover($"{attribute.Name}: {prop.Value.Type}") : FromNpm(prop.Value.Definition);
            var evt = component.Events.FirstOrDefault(item => MatchesEvent(attribute.Name, item.Key));
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
        return local is not null && method == "textDocument/hover" ? Hover(element.Name) : null;
    }

    private async Task<object[]> CSharpCompletionAsync(int position, CancellationToken cancellationToken, int? version, int? sourceAfterTrailingDot = null)
    {
        using var workspace = new AdhocWorkspace(Host.Value);
        var document = await CreateRoslynDocumentAsync(workspace, cancellationToken, sourceAfterTrailingDot.HasValue ? position - 1 : null);
        var generated = await document.GetTextAsync(cancellationToken);
        var service = CompletionService.GetService(document);
        if (service is null)
            return [];
        var result = await service.GetCompletionsAsync(document, position, cancellationToken: cancellationToken);
        if (result is null)
            return [];
        var items = new List<object>();
        var typed = result.Span.Start <= position ? generated.ToString(TextSpan.FromBounds(result.Span.Start, position)) : "";
        var candidates = result.ItemsList
            .Where(item => !item.DisplayText.StartsWith("__Jolt", StringComparison.Ordinal))
            .OrderByDescending(item => item.FilterText.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(item => item.Rules.MatchPriority)
            .ThenBy(item => item.SortText, StringComparer.Ordinal)
            .Take(100);
        foreach (var item in candidates)
        {
            var change = (await service.GetChangeAsync(document, item, cancellationToken: cancellationToken)).TextChange;
            TextSpan? span;
            if (sourceAfterTrailingDot is { } sourcePosition)
            {
                if (change.Span != new TextSpan(position, 0))
                    continue;
                span = new TextSpan(sourcePosition, 0);
            }
            else
                span = ToSource(change.Span);
            if (span is { } sourceSpan)
                items.Add(new
                {
                    label = item.DisplayText,
                    detail = item.InlineDescription,
                    filterText = item.FilterText,
                    sortText = $"{int.MaxValue - (long)item.Rules.MatchPriority:D10}:{item.SortText}",
                    kind = CompletionKind(item),
                    textEdit = new { range = Range(sourceSpan), newText = change.NewText },
                    // The editor asks for documentation only for the selected item.
                    data = new
                    {
                        uri = new Uri(Path.GetFullPath(projection.Document.Path, projectDirectory)).AbsoluteUri,
                        position, version, sourceAfterTrailingDot, item.SortText, item.FilterText, tag = item.Tags.FirstOrDefault(),
                    },
                });
        }
        return items.ToArray();
    }

    internal async Task<object> ResolveCompletionAsync(JsonElement item, JsonElement data, CancellationToken cancellationToken)
    {
        var position = data.GetProperty("position").GetInt32();
        using var workspace = new AdhocWorkspace(Host.Value);
        var document = await CreateRoslynDocumentAsync(workspace, cancellationToken,
            data.GetProperty("sourceAfterTrailingDot").ValueKind == JsonValueKind.Number ? position - 1 : null);
        var service = CompletionService.GetService(document);
        if (position > (await document.GetTextAsync(cancellationToken)).Length || service is null ||
            await service.GetCompletionsAsync(document, position, cancellationToken: cancellationToken) is not { } completions)
            return item;

        var label = item.GetProperty("label").GetString();
        var sort = data.GetProperty("sortText").GetString();
        var filter = data.GetProperty("filterText").GetString();
        var tag = data.GetProperty("tag").GetString();
        var candidate = completions.ItemsList.FirstOrDefault(entry => entry.DisplayText == label && entry.SortText == sort &&
            entry.FilterText == filter && entry.Tags.FirstOrDefault() == tag);
        if (candidate is null || await service.GetDescriptionAsync(document, candidate, cancellationToken) is not { Text.Length: > 0 } description)
            return item;

        var resolved = JsonNode.Parse(item.GetRawText())!.AsObject();
        resolved["documentation"] = description.Text;
        return resolved;
    }

    private async Task<Document> CreateRoslynDocumentAsync(AdhocWorkspace workspace, CancellationToken cancellationToken, int? trailingDot = null)
    {
        var documents = await CreateRoslynProjectAsync(workspace, cancellationToken, trailingDot);
        return documents[projection.Document.Path];
    }

    private async Task<Dictionary<string, Document>> CreateRoslynProjectAsync(
        AdhocWorkspace workspace, CancellationToken cancellationToken, int? trailingDot = null)
    {
        var model = projection.SemanticModel;
        var projectId = ProjectId.CreateNewId();
        var info = ProjectInfo.Create(projectId, VersionStamp.Create(), "Jolt", model.Compilation.AssemblyName!, LanguageNames.CSharp,
            parseOptions: model.SyntaxTree.Options, compilationOptions: model.Compilation.Options, metadataReferences: model.Compilation.References);
        var solution = workspace.AddProject(info).Solution;
        var ids = new Dictionary<string, DocumentId>(StringComparer.Ordinal);
        var paths = analysis.Documents.ToDictionary(author => author.SemanticModel.SyntaxTree, author => author.Document.Path);
        foreach (var tree in model.Compilation.SyntaxTrees)
        {
            var id = DocumentId.CreateNewId(projectId);
            var generated = await tree.GetTextAsync(cancellationToken);
            if (tree == model.SyntaxTree && trailingDot is { } dot)
                generated = generated.WithChanges(new TextChange(new TextSpan(dot, 0), "."));
            solution = solution.AddDocument(id, Path.GetFileName(tree.FilePath), generated, filePath: tree.FilePath);
            if (paths.TryGetValue(tree, out var author))
                ids.Add(author, id);
        }
        return ids.ToDictionary(item => item.Key, item => solution.GetDocument(item.Value)!, StringComparer.Ordinal);
    }

    private static int CompletionKind(CompletionItem item)
    {
        foreach (var tag in item.Tags)
        {
            var kind = tag switch
            {
                WellKnownTags.Method or WellKnownTags.ExtensionMethod => 2,
                WellKnownTags.Property => 10,
                WellKnownTags.Field => 5,
                WellKnownTags.Event => 23,
                WellKnownTags.Parameter or WellKnownTags.Local => 6,
                WellKnownTags.Class or WellKnownTags.Delegate => 7,
                WellKnownTags.Interface => 8,
                WellKnownTags.Namespace => 9,
                WellKnownTags.Enum => 13,
                WellKnownTags.Keyword => 14,
                WellKnownTags.EnumMember => 20,
                WellKnownTags.Constant => 21,
                WellKnownTags.Structure => 22,
                WellKnownTags.TypeParameter => 25,
                _ => 0,
            };
            if (kind != 0)
                return kind;
        }
        return 1;
    }

    private ISymbol? SymbolAt(int position, CancellationToken cancellationToken) =>
        SymbolAt(projection.SemanticModel, position, cancellationToken);

    private static ISymbol? SymbolAt(SemanticModel model, int position, CancellationToken cancellationToken, bool allowCandidate = true)
    {
        var token = model.SyntaxTree.GetRoot(cancellationToken).FindToken(position);
        var node = token.Parent;
        for (; node is not null; node = node.Parent)
        {
            if (node is SimpleNameSyntax or MemberAccessExpressionSyntax)
            {
                var info = model.GetSymbolInfo(node, cancellationToken);
                return info.Symbol ?? (allowCandidate ? info.CandidateSymbols.FirstOrDefault() : null);
            }
            if (model.GetDeclaredSymbol(node, cancellationToken) is { } symbol &&
                symbol.Locations.Any(location => location.SourceTree == model.SyntaxTree && location.SourceSpan.Contains(token.Span)))
                return symbol;
        }
        return null;
    }

    private object? CSharpDefinition(int position, CancellationToken cancellationToken)
    {
        if (SymbolAt(position, cancellationToken) is not { } symbol)
            return null;
        foreach (var location in symbol.Locations.Where(location => location.IsInSource))
            if (analysis.Documents.FirstOrDefault(document => document.SemanticModel.SyntaxTree == location.SourceTree) is { } author &&
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
        NpmVueComponent? parentComponent, JoltLocalComponent? local, JoltLocalComponent? localParent)
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
            var names = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var name in HtmlTags)
                names[name] = null;
            foreach (var candidate in projection.LocalComponents)
                names[candidate.Name] = candidate.Path;
            foreach (var (module, exports) in workspace.Exports.Where(item => UsesPackage(item.Key)))
                foreach (var export in exports)
                    names.TryAdd(NpmNames.TagFor(module, export), module);
            return names.OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => Completion(item.Key, prefix, item.Value, item.Value is null ? 10 : 7))
                .Concat(AutoImportCompletions(prefix, names.Keys, offset)).ToArray();
        }
        if (element?.Name == "template" && prefixStart > 0 && _text[prefixStart - 1] == '#')
        {
            if (parentComponent is not null)
                return parentComponent.Slots.Select(item => Completion(item.Key, prefix, Signatures(item.Key, item.Value.Signatures), 10)).ToArray();
            return localParent?.Slots.Select(name => Completion(name, prefix, $"Slot of {localParent.Name}", 10)).ToArray() ?? [];
        }
        if (element is null || offset <= element.NameSpan.End)
            return [];
        // Markup completion belongs to the opening tag. Known attribute spans skip quoted expressions containing '>'.
        var afterLastAttribute = element.Attributes.Where(item => item.Span.End <= offset)
            .Select(item => item.Span.End).DefaultIfEmpty(element.NameSpan.End).Max();
        if (attribute is null && _text.ToString(TextSpan.FromBounds(afterLastAttribute, offset)).Contains('>'))
            return [];
        if (attribute is not null && attribute.NameSpan.End < offset && attribute.Value.Any(value => value.Span.Contains(offset) || value.Span.End == offset))
        {
            if (component is not null)
            {
                var prop = component.Props.FirstOrDefault(item => MatchesProp(attribute.Name, item.Key));
                if (prop.Value is { LiteralValues.Count: > 0 } literal)
                    return literal.LiteralValues.Select(value => Completion(value, prefix, literal.Type, 12)).ToArray();
                if (prop.Value?.Type == "boolean")
                    return BooleanValues();
            }
            if (local?.Properties.Any(property => property.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase) &&
                property.Type is "bool" or "bool?" or "global::System.Boolean") == true)
                return BooleanValues();
            return [];
        }
        if (local is not null)
            return local.Properties
                .Where(property => Available(name => name.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(property => property.Required)
                .Select(property => Completion(property.Name, prefix, PropertyDetail(property.Type, property.Required), 10, property.Required))
                .ToArray();
        if (component is null)
            return [];

        var props = component.Props.Where(item => Available(name => MatchesProp(name, item.Key)))
            .OrderByDescending(item => item.Value.Required)
            .Select(item => Completion(Pascal(item.Key), prefix, PropertyDetail(item.Value.Type, item.Value.Required), 10, item.Value.Required));
        var eventPrefix = prefixStart > 0 && _text[prefixStart - 1] == '@';
        var events = component.Events.Where(item => Available(name => MatchesEvent(name, item.Key)))
            .Select(item => Completion(eventPrefix ? "on" + item.Key : EventName(item.Key), prefix, Signatures(item.Key, item.Value.Signatures), 23));
        if (eventPrefix)
            return events.ToArray();
        return props.Concat(events).ToArray();

        bool Available(Func<string, bool> matches) =>
            !element.Attributes.Any(other => !ReferenceEquals(other, attribute) && matches(other.Name));
        object[] BooleanValues() => [Completion("true", prefix, "bool", 12), Completion("false", prefix, "bool", 12)];
    }

    private IEnumerable<object> AutoImportCompletions(TextSpan tag, IEnumerable<string> visible, int offset)
    {
        var typed = _text.ToString(TextSpan.FromBounds(tag.Start, offset));
        var tagRange = Range(tag);
        foreach (var group in ImportCandidates(typed, visible).GroupBy(component => component.Name, StringComparer.Ordinal))
        {
            var disambiguate = group.Skip(1).Any();
            foreach (var component in group)
                yield return new
                {
                    label = disambiguate ? $"{component.Name} ({component.Namespace})" : component.Name,
                    filterText = component.Name,
                    detail = $"Add @using {component.Namespace}",
                    kind = 7,
                    sortText = "2" + component.Name + "." + component.Namespace,
                    textEdit = new { range = tagRange, newText = component.Name },
                    additionalTextEdits = new[] { ProtocolEdit(ImportEdit(tag, component.Namespace)) },
                };
        }
    }

    private IEnumerable<JoltLocalComponent> ImportCandidates(string typed, IEnumerable<string> visible)
    {
        var names = visible.ToHashSet(StringComparer.Ordinal);
        var usedTags = Elements(projection.Document.Nodes).Select(element => element.Name).Where(names.Contains).ToHashSet(StringComparer.Ordinal);
        return analysis.LocalComponents
            .Where(component => !component.Namespace.StartsWith("Npm.", StringComparison.Ordinal))
            .GroupBy(component => component.Namespace, StringComparer.Ordinal)
            .Where(group => !group.Any(component => usedTags.Contains(component.Name) &&
                !projection.LocalComponents.Contains(component)))
            .SelectMany(group => group)
            .Where(component => !names.Contains(component.Name) && component.Name.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(component => component.Name, StringComparer.Ordinal)
            .ThenBy(component => component.Namespace, StringComparer.Ordinal);
    }

    private TextChange ImportEdit(TextSpan tag, string componentNamespace)
    {
        var firstUsing = projection.Document.Nodes.OfType<JoltUsing>().FirstOrDefault(directive => directive.Span.Start < tag.Start);
        var insertion = firstUsing is null
            ? _text.Length > 0 && _text[0] == '\uFEFF' ? 1 : 0
            : _text.Lines.GetLineFromPosition(firstUsing.Span.Start).Start;
        var newline = NewLine();
        return new(new TextSpan(insertion, 0), "@using " + componentNamespace + newline + (firstUsing is null ? newline : ""));
    }

    private string NewLine()
    {
        var line = _text.Lines.FirstOrDefault(line => line.End < line.EndIncludingLineBreak);
        return line.EndIncludingLineBreak > line.End ? _text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak)) : "\n";
    }

    private object ProtocolEdit(TextChange edit) => new { range = Range(edit.Span), newText = edit.NewText };

    private object Completion(string name, TextSpan span, string? detail, int kind, bool required = false) => new
    {
        label = name, detail, kind, sortText = (required ? "0" : "1") + name, textEdit = new { range = Range(span), newText = name },
    };

    private NpmVueComponent? Component(string? name) =>
        workspace.Options.NpmComponents?.FirstOrDefault(component => NpmNames.TagFor(component.ModuleSpecifier, component.ExportName) == name && UsesPackage(component.ModuleSpecifier));

    private JoltLocalComponent? LocalComponent(string? name)
    {
        var matches = projection.LocalComponents.Where(component => component.Name == name).Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private bool UsesPackage(string package)
    {
        var @namespace = NpmNames.NamespaceFor(package);
        return projection.Document.Nodes.OfType<JoltUsing>().Any(use => use.Code.Trim().TrimEnd(';') == @namespace);
    }

    private static string EventName(string name) => "on" + Pascal(name);
    private static bool MatchesProp(string authored, string name) => authored.Equals(name, StringComparison.OrdinalIgnoreCase);
    private static bool MatchesEvent(string authored, string name) =>
        authored.Equals(EventName(name), StringComparison.OrdinalIgnoreCase) ||
        authored.Equals("@on" + name, StringComparison.OrdinalIgnoreCase);
    private static string? PropertyDetail(string? type, bool required) => required ? $"{type} (required)" : type;
    private static bool IsMarkupNameCharacter(char character) => char.IsLetterOrDigit(character) || character is '-' or '_' or ':';

    private static string Pascal(string name) => name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..];

    private static string Signatures(string name, IReadOnlyList<IReadOnlyList<NpmVueParameter>> signatures) =>
        string.Join("\n", signatures.Select(signature => $"{name}({string.Join(", ", signature.Select(p => $"{p.Name}: {p.Type}"))})"));

    internal static IEnumerable<JoltElement> Elements(IEnumerable<JoltNode> nodes)
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
