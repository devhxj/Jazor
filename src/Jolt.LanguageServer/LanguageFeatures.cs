using Jazor.Npm;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
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
    string projectDirectory,
    long snapshotId)
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
                    ? CSharpHover(symbol, cancellationToken)
                    : null,
                "textDocument/definition" => CSharpDefinition(generated, cancellationToken),
                "textDocument/signatureHelp" => SignatureHelp(generated, cancellationToken),
                _ => null,
            };
        }

        var elements = JoltTree.Elements(projection.Document).Where(node => node.Span.Start <= offset && offset <= node.Span.End);
        var element = elements.OrderBy(node => node.Span.Length).FirstOrDefault();
        var attribute = element?.Attributes.FirstOrDefault(item => item.Span.Start <= offset && offset <= item.Span.End);
        var component = Component(element?.Name);
        var local = LocalComponent(element?.Name);
        var parent = elements.Where(node => node != element).OrderBy(node => node.Span.Length).FirstOrDefault();
        var parentComponent = Component(parent?.Name);
        var localParent = LocalComponent(parent?.Name);
        var builtInParent = parentComponent is null && localParent is null ? VueBuiltIns.Find(parent?.Name) : null;
        if (method == "textDocument/completion")
            return MarkupCompletion(offset, element, attribute, component, parentComponent, local, localParent, builtInParent);
        if (element is null)
            return null;
        if (method == "textDocument/hover" && component is null && local is null && VueBuiltIns.Find(element.Name) is { } builtIn)
        {
            if (attribute is not null && BuiltInProperty(builtIn, attribute.Name) is { Type: { } type } prop)
                return Hover($"{Pascal(prop.Name)}: {type}");
            if (element.NameSpan.Contains(offset))
                return Hover($"Vue built-in component {builtIn.Name}");
        }
        if (method == "textDocument/hover" && attribute is not null && local is not null &&
            MarkupAt(offset)?.Symbol is { Kind: JoltMarkupKind.Property } member &&
            local.Properties.FirstOrDefault(property => property.Name == member.Name) is { } property)
            return Hover($"{property.Name}: {property.Type}");

        if (attribute is not null && component is not null)
        {
            if (attribute is { Kind: JoltAttributeKind.Bind, Target: { } modelName } &&
                NpmVueModels.Get(component).FirstOrDefault(candidate => MatchesProp(modelName, candidate.PropName)) is { } model)
                return method == "textDocument/hover" ? MarkupHover($"{attribute.Name}: {model.Type}", model.Definition) : FromNpm(model.Definition);
            var prop = component.Props.FirstOrDefault(item => MatchesProp(attribute.Name, item.Key));
            if (prop.Key is not null)
                return method == "textDocument/hover" ? MarkupHover($"{attribute.Name}: {prop.Value.Type}", prop.Value.Definition) : FromNpm(prop.Value.Definition);
            var evt = component.Events.FirstOrDefault(item => MatchesEvent(attribute.Name, item.Key));
            if (evt.Key is not null)
                return method == "textDocument/hover" ? MarkupHover(Signatures(evt.Key, evt.Value.Signatures), evt.Value.Definition) : FromNpm(evt.Value.Definition);
        }
        if (element.Name == "template" && attribute is { Name.Length: > 1 } && attribute.Name[0] == '#' &&
            parentComponent is not null && parentComponent.Slots.TryGetValue(attribute.Name[1..], out var slot))
            return method == "textDocument/hover" ? MarkupHover(Signatures(attribute.Name, slot.Signatures), slot.Definition) : FromNpm(slot.Definition);
        if (method == "textDocument/hover" && element.Name == "template" && attribute is not null &&
            builtInParent?.Slots.Any(slot => attribute.Name == "#" + slot.Name) == true)
            return Hover($"Slot of Vue {builtInParent.Name}");
        if (!element.NameSpan.Contains(offset))
            return null;
        if (component is not null)
            return method == "textDocument/hover"
                ? MarkupHover(component.LocalPath ?? $"{component.ExportName} from {component.ModuleSpecifier}@{component.Version}", component.Definition)
                : FromNpm(component.Definition);
        return local is not null && method == "textDocument/hover" ? Hover(element.Name) : null;
    }

    private static object MarkupHover(string label, NpmDefinition? definition)
    {
        var docs = definition?.Documentation;
        if (docs is null) return Hover(label);
        var deprecated = docs.Deprecated switch { null => null, "" => "Deprecated", var text => $"Deprecated — {text}" };
        return Hover(string.Join("\n\n", new[] { label, docs.Summary, docs.Remarks, deprecated }.Where(part => !string.IsNullOrWhiteSpace(part))));
    }

    private async Task<object[]> CSharpCompletionAsync(int position, CancellationToken cancellationToken, int? version, int? sourceAfterTrailingDot = null)
    {
        using var workspace = new AdhocWorkspace(Host.Value);
        var document = await CreateCompletionDocumentAsync(workspace, position, sourceAfterTrailingDot.HasValue, cancellationToken);
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
                        position, version, snapshotId, sourceAfterTrailingDot, item.SortText, item.FilterText, tag = item.Tags.FirstOrDefault(),
                    },
                });
        }
        return items.ToArray();
    }

    internal async Task<object> ResolveCompletionAsync(JsonElement item, JsonElement data, CancellationToken cancellationToken)
    {
        var position = data.GetProperty("position").GetInt32();
        using var workspace = new AdhocWorkspace(Host.Value);
        var document = await CreateCompletionDocumentAsync(workspace, position,
            data.GetProperty("sourceAfterTrailingDot").ValueKind == JsonValueKind.Number, cancellationToken);
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

    private async Task<Document> CreateCompletionDocumentAsync(AdhocWorkspace roslynWorkspace, int position, bool trailingDot,
        CancellationToken cancellationToken)
    {
        var dot = trailingDot ? position - 1 : (int?)null;
        if (workspace.ValueExports.Count > 0 && NpmValueReceiver(position - 1, trailingDot, cancellationToken) is { } receiver)
        {
            var imports = JoltProjectImports.Parse(workspace.Sources, cancellationToken);
            var import = imports.ImportsFor(projection.Document).FirstOrDefault(use => use.IsNpmValue &&
                use.Alias == receiver.Alias && use.NpmNamespace == receiver.Namespace);
            if (import is not null)
            {
                var candidates = await JoltProjectLoader.LoadNpmValueCompletionsAsync(projectDirectory, workspace, import, receiver.Prefix, cancellationToken);
                var values = workspace.Options.NpmValues.Concat(candidates).DistinctBy(value => (value.ModuleSpecifier, value.ExportName)).ToArray();
                if (values.Length > workspace.Options.NpmValues.Count)
                {
                    // The additional contracts exist only for Roslyn completion. Keep the validated source snapshot intact.
                    var expanded = JoltProjectCompiler.Analyze(workspace.Sources, workspace.Options with { NpmValues = values }, cancellationToken);
                    var author = expanded.Documents.Single(document => document.Document.Path == projection.Document.Path);
                    return await new LanguageFeatures(author, expanded, workspace, projectDirectory, snapshotId)
                        .CreateRoslynDocumentAsync(roslynWorkspace, cancellationToken, dot);
                }
            }
        }
        return await CreateRoslynDocumentAsync(roslynWorkspace, cancellationToken, dot);
    }

    private (string Alias, string Namespace, string Prefix)? NpmValueReceiver(int position, bool trailingDot, CancellationToken cancellationToken)
    {
        if (position < 0)
            return null;
        var model = projection.SemanticModel;
        var token = model.SyntaxTree.GetRoot(cancellationToken).FindToken(trailingDot ? Math.Max(0, position - 1) : position);
        var access = token.Parent?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault(candidate => candidate.OperatorToken == token || candidate.Name.Span.Contains(token.Span));
        var receiver = trailingDot ? token.Parent as IdentifierNameSyntax : access?.Expression as IdentifierNameSyntax;
        if (receiver is null || model.GetAliasInfo(receiver, cancellationToken)?.Target is not INamedTypeSymbol { Name: "Exports" } type)
            return null;
        var prefix = trailingDot || access is null || position < access.Name.SpanStart ? "" :
            model.SyntaxTree.GetText(cancellationToken).ToString(TextSpan.FromBounds(access.Name.SpanStart, position + 1));
        return (receiver.Identifier.ValueText, type.ContainingNamespace.ToDisplayString(), prefix);
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
        if (NpmValueDefinition(symbol) is { } npm)
            return FromNpm(npm);
        if (symbol is IPropertySymbol property)
        {
            var member = (property.ContainingType.Name, property.Name);
            if (projection.SlotMemberOrigins.TryGetValue(member, out var origin) &&
                analysis.Documents.FirstOrDefault(document => document.Document.Path == origin.Path) is { } outlet)
                return Location(origin.Path, SourceText.From(outlet.Document.Source), origin.Span);
            if (projection.ExternalSlotMemberOrigins.TryGetValue(member, out var definition))
                return FromNpm(definition);
        }
        foreach (var location in symbol.Locations.Where(location => location.IsInSource))
            if (analysis.Documents.FirstOrDefault(document => document.SemanticModel.SyntaxTree == location.SourceTree) is { } author &&
                ToSource(author, location.SourceSpan) is { } span)
                return Location(author.Document.Path, SourceText.From(author.Document.Source), span);
        return null;
    }

    private NpmDefinition? NpmValueDefinition(ISymbol symbol) =>
        analysis.NpmValueDefinitions.GetValueOrDefault(symbol.OriginalDefinition);

    private object CSharpHover(ISymbol symbol, CancellationToken cancellationToken)
    {
        if (NpmValueDefinition(symbol) is null)
            return Hover(symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

        var label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var documentation = FormatNpmDocumentation(ProjectedDocumentation(symbol, cancellationToken), includeParameters: true);
        return Hover(documentation is null ? label : label + "\n\n" + documentation);
    }

    private static XElement? ProjectedDocumentation(ISymbol symbol, CancellationToken cancellationToken)
    {
        var xml = symbol.OriginalDefinition.GetDocumentationCommentXml(cancellationToken: cancellationToken);
        return string.IsNullOrWhiteSpace(xml) ? null : XElement.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static string? FormatNpmDocumentation(XElement? documentation, bool includeParameters)
    {
        if (documentation is null)
            return null;
        var sections = new List<string>();
        foreach (var element in documentation.Elements())
        {
            var text = element.Value.Trim();
            if (text.Length == 0)
                continue;
            switch (element.Name.LocalName)
            {
                case "summary":
                case "remarks":
                    sections.Add(text);
                    break;
                case "param" when includeParameters:
                case "typeparam":
                    sections.Add($"{element.Attribute("name")?.Value} — {text}");
                    break;
                case "returns":
                    sections.Add("Returns\n" + text);
                    break;
                case "example":
                    sections.Add("Example\n" + text);
                    break;
            }
        }
        return sections.Count == 0 ? null : string.Join("\n\n", sections);
    }

    private object? SignatureHelp(int position, CancellationToken cancellationToken)
    {
        var model = projection.SemanticModel;
        var token = model.SyntaxTree.GetRoot(cancellationToken).FindToken(Math.Max(0, position - 1), findInsideTrivia: true);
        var call = token.Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (call is null)
            return null;
        var methods = model.GetMemberGroup(call.Expression, cancellationToken).OfType<IMethodSymbol>().ToArray();
        var selected = model.GetSymbolInfo(call, cancellationToken).Symbol as IMethodSymbol;
        var arguments = call.ArgumentList.Arguments;
        var activeParameter = arguments.GetSeparators().Count(separator => separator.SpanStart < position);
        var activeSignature = 0;
        if (selected is not null && NpmValueDefinition(selected) is not null)
        {
            activeSignature = Math.Max(0, Array.FindIndex(methods, method =>
                SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, selected.OriginalDefinition)));
            if (activeParameter < arguments.Count && arguments[activeParameter].NameColon is { } name &&
                selected.Parameters.FirstOrDefault(parameter => parameter.Name == name.Name.Identifier.ValueText) is { } boundParameter)
                activeParameter = boundParameter.Ordinal;
        }
        return methods.Length == 0 ? null : new
        {
            signatures = methods.Select(method => DescribeSignature(method, cancellationToken)).ToArray(),
            activeSignature,
            activeParameter,
        };
    }

    private object DescribeSignature(IMethodSymbol method, CancellationToken cancellationToken)
    {
        var label = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        // Read the generated XML so overloads describe only their own projected parameters.
        var xml = NpmValueDefinition(method) is null ? null : ProjectedDocumentation(method, cancellationToken);
        var parameters = method.Parameters.Select(parameter =>
        {
            var label = parameter.ToDisplayString();
            var description = xml?.Elements("param").FirstOrDefault(element => element.Attribute("name")?.Value == parameter.Name)?.Value.Trim();
            return description is null ? (object)new { label } : new { label, documentation = description };
        }).ToArray();
        var documentation = FormatNpmDocumentation(xml, includeParameters: false);
        return documentation is null ? new { label, parameters } : new { label, parameters, documentation };
    }

    private object[] MarkupCompletion(int offset, JoltElement? element, JoltAttribute? attribute, NpmVueComponent? component,
        NpmVueComponent? parentComponent, JoltLocalComponent? local, JoltLocalComponent? localParent, ComponentContract? builtInParent)
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
            foreach (var candidate in VueBuiltIns.Components)
                names[candidate.Name] = "Vue built-in component";
            foreach (var candidate in projection.LocalComponents)
                names[candidate.Name] = candidate.Path;
            foreach (var candidate in projection.VueComponents)
                names.TryAdd(Path.GetFileNameWithoutExtension(candidate.LocalPath!), candidate.LocalPath);
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
            if (localParent is not null)
                return localParent.Slots.Select(name => Completion(name, prefix, $"Slot of {localParent.Name}", 10)).ToArray();
            return builtInParent?.Slots
                .Select(slot => Completion(slot.Name, prefix, "Vue built-in slot", 10)).ToArray() ?? [];
        }
        if (element is null || offset <= element.NameSpan.End)
            return [];
        var builtIn = component is null && local is null ? VueBuiltIns.Find(element.Name) : null;
        // Markup completion belongs to the opening tag. Known attribute spans skip quoted expressions containing '>'.
        var afterLastAttribute = element.Attributes.Where(item => item.Span.End <= offset)
            .Select(item => item.Span.End).DefaultIfEmpty(element.NameSpan.End).Max();
        if (attribute is null && _text.ToString(TextSpan.FromBounds(afterLastAttribute, offset)).Contains('>'))
            return [];
        if (attribute is not null && attribute.NameSpan.End < offset && attribute.Value.Any(value => value.Span.Contains(offset) || value.Span.End == offset))
        {
            if (attribute.Kind == JoltAttributeKind.BindEvent && JoltFormBindings.Get(element) is JoltFormBinding.Text or JoltFormBinding.Number)
                return [Completion("oninput", prefix, "Write back on input", 12), Completion("onchange", prefix, "Write back on change", 12)];
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
            if (BuiltInProperty(builtIn, attribute.Name) is { } builtInProp)
            {
                if (builtInProp.LiteralExpressions is { } values)
                    return values.Keys.Select(value => Completion(value, prefix, builtInProp.Type, 12)).ToArray();
                if (builtInProp.Type == "bool")
                    return BooleanValues();
            }
            return [];
        }
        var directivePrefix = prefixStart > 0 && _text[prefixStart - 1] == '@';
        var directiveSpan = directivePrefix ? TextSpan.FromBounds(prefixStart - 1, replacementEnd) : prefix;
        var directives = DirectiveCompletions(element, attribute, component, local, directiveSpan, offset);

        if (local is not null)
            return directivePrefix ? directives.ToArray() : local.Properties
                .Where(property => Available(name => name.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(property => property.Required)
                .Select(property => Completion(property.Name, prefix, PropertyDetail(property.Type, property.Required), 10, property.Required))
                .Concat(directives).ToArray();
        if (component is null)
            return builtIn is null || directivePrefix ? directives.ToArray() : builtIn.Properties
                .Where(property => Available(name => MatchesProp(name, property.Name)))
                .OrderByDescending(property => property.Required)
                .Select(property => Completion(Pascal(property.Name), prefix, PropertyDetail(property.Type, property.Required), 10, property.Required))
                .Concat(directives).ToArray();

        var props = component.Props.Where(item => Available(name => MatchesProp(name, item.Key)))
            .OrderByDescending(item => item.Value.Required)
            .Select(item => Completion(Pascal(item.Key), prefix, PropertyDetail(item.Value.Type, item.Value.Required), 10, item.Value.Required));
        var events = component.Events.Where(item => Available(name => MatchesEvent(name, item.Key)))
            .Select(item => Completion(directivePrefix ? "@on" + item.Key : EventName(item.Key), directiveSpan, Signatures(item.Key, item.Value.Signatures), 23));
        if (directivePrefix)
            return _text.ToString(directiveSpan).StartsWith("@on", StringComparison.OrdinalIgnoreCase) ||
                directiveSpan.Length == 1 ? directives.Concat(events).ToArray() : directives.ToArray();
        return props.Concat(events).Concat(directives).ToArray();

        bool Available(Func<string, bool> matches) =>
            !element.Attributes.Any(other => !ReferenceEquals(other, attribute) && matches(other.Name));
        object[] BooleanValues() => [Completion("true", prefix, "bool", 12), Completion("false", prefix, "bool", 12)];
    }

    private static ComponentProperty? BuiltInProperty(ComponentContract? component, string name) =>
        component?.Properties.FirstOrDefault(property => MatchesProp(name, property.Name));

    private IEnumerable<object> DirectiveCompletions(JoltElement element, JoltAttribute? attribute,
        NpmVueComponent? component, JoltLocalComponent? local, TextSpan replacement, int offset)
    {
        if (element.Name == "slot")
            yield break;
        var authored = _text.ToString(TextSpan.FromBounds(replacement.Start, offset));
        var colon = authored.IndexOf(':');
        if (authored.StartsWith("@on", StringComparison.OrdinalIgnoreCase) && colon > 3 &&
            component is null && element.Name.Length > 0 && !char.IsUpper(element.Name[0]))
        {
            var eventName = authored[..colon];
            foreach (var modifier in new[] { "preventDefault", "stopPropagation" })
                if (Available(eventName + ":" + modifier) && MatchesPrefix(eventName + ":" + modifier))
                    yield return Completion(eventName + ":" + modifier, replacement, "DOM event modifier", 14);
            yield break;
        }

        if (Available("@key") && MatchesPrefix("@key"))
            yield return Completion("@key", replacement, "VNode key", 14);
        if (Available("@attributes") && MatchesPrefix("@attributes"))
            yield return Completion("@attributes", replacement, "Merge attributes from a dictionary", 14);
        if ((local?.HasReferenceContract == true || component is null && local is null && char.IsLower(element.Name[0])) &&
            Available("@ref") && MatchesPrefix("@ref"))
            yield return Completion("@ref", replacement, "Capture a template reference", 14);
        if (JoltFormBindings.Get(element) is { } form)
        {
            foreach (var item in BindCompletions(null, "Two-way form binding"))
                yield return item;
            if (form is JoltFormBinding.Text or JoltFormBinding.Number && Available("@bind:event") && MatchesPrefix("@bind:event"))
                yield return Completion("@bind:event", replacement, "DOM event that writes the value back", 14);
        }
        if (component is not null)
            foreach (var model in NpmVueModels.Get(component))
                foreach (var item in BindCompletions(model.PropName, model.Type))
                    yield return item;
        if (local is not null)
            foreach (var model in local.Models)
                foreach (var item in BindCompletions(model.Property, model.Type))
                    yield return item;

        IEnumerable<object> BindCompletions(string? target, string detail)
        {
            var name = target is null ? "@bind" : "@bind-" + Pascal(target);
            var hasDirect = false;
            var hasSplit = false;
            var hasAfter = false;
            foreach (var other in element.Attributes)
            {
                if (ReferenceEquals(other, attribute) || other.Kind != JoltAttributeKind.Bind ||
                    !string.Equals(other.Target, target, StringComparison.OrdinalIgnoreCase))
                    continue;
                hasDirect |= other.Modifier is null;
                hasSplit |= other.Modifier is "get" or "set";
                hasAfter |= other.Modifier == "after";
            }
            if (!hasSplit && Available(name) && MatchesPrefix(name))
                yield return Completion(name, replacement, detail, 14);
            if (!hasSplit && Available(name + ":after") && MatchesPrefix(name + ":after"))
                yield return Completion(name + ":after", replacement, "Run after writing the value back", 14);
            if (!hasDirect && !hasAfter)
            {
                if (Available(name + ":get") && MatchesPrefix(name + ":get"))
                    yield return Completion(name + ":get", replacement, "Read the bound value", 14);
                if (Available(name + ":set") && MatchesPrefix(name + ":set"))
                    yield return Completion(name + ":set", replacement, "Receive the updated value", 14);
            }
        }

        bool Available(string name) => !element.Attributes.Any(other => !ReferenceEquals(other, attribute) &&
            other.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        bool MatchesPrefix(string name) => !authored.StartsWith('@') || name.StartsWith(authored, StringComparison.OrdinalIgnoreCase);
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
        var usedTags = JoltTree.Elements(projection.Document).Select(element => element.Name).Where(names.Contains).ToHashSet(StringComparer.Ordinal);
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
        projection.VueComponents.FirstOrDefault(component => Path.GetFileNameWithoutExtension(component.LocalPath) == name)
        ?? workspace.Options.NpmComponents?.FirstOrDefault(component => NpmNames.TagFor(component.ModuleSpecifier, component.ExportName) == name && UsesPackage(component.ModuleSpecifier));

    private JoltLocalComponent? LocalComponent(string? name)
    {
        var matches = projection.LocalComponents.Where(component => component.Name == name).Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private bool UsesPackage(string package)
    {
        var @namespace = NpmNames.NamespaceFor(package);
        return projection.Imports.Contains(@namespace, StringComparer.Ordinal);
    }

    private static string EventName(string name) => ComponentContract.VueEventProperty(name);
    private static bool MatchesProp(string authored, string name) => authored.Equals(name, StringComparison.OrdinalIgnoreCase);
    private static bool MatchesEvent(string authored, string name) =>
        authored.Equals(EventName(name), StringComparison.OrdinalIgnoreCase) ||
        authored.Equals("@on" + name, StringComparison.OrdinalIgnoreCase);
    private static string? PropertyDetail(string? type, bool required) => required ? $"{type} (required)" : type;
    private static bool IsMarkupNameCharacter(char character) => char.IsLetterOrDigit(character) || character is '-' or '_' or ':';

    private static string Pascal(string name) => name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..];

    private static string Signatures(string name, IReadOnlyList<IReadOnlyList<NpmVueParameter>> signatures) =>
        string.Join("\n", signatures.Select(signature => $"{name}({string.Join(", ", signature.Select(p => $"{p.Name}: {p.Type}"))})"));

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

    private object? FromNpm(NpmDefinition? definition)
    {
        if (definition is null)
            return null;
        var path = Path.GetFullPath(definition.Path, projectDirectory);
        var content = workspace.VueSources.TryGetValue(path, out var overlay) ? overlay :
            File.Exists(path) ? File.ReadAllText(path) : null;
        return content is null || definition.Start + definition.Length > content.Length ? null
            : Location(path, SourceText.From(content), new TextSpan(definition.Start, definition.Length));
    }

    private object Location(string path, SourceText source, TextSpan span) =>
        new { uri = new Uri(Path.GetFullPath(path, projectDirectory)).AbsoluteUri, range = Server.Range(source.Lines.GetLinePositionSpan(span)) };
}
