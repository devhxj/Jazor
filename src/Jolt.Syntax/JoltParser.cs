using System.Collections.Immutable;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Syntax;

public static class JoltParser
{
    private static readonly CSharpParseOptions CSharpOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
    // Razor's template IR node is internal even though its base exposes the
    // children and source span through the public intermediate tree.
    private static bool IsTemplate(IntermediateNode node) => node.GetType().Name == "TemplateIntermediateNode";

    public static JoltDocument Parse(string source, string path = "Component.razor")
        => Parse(source, source, path);

    private static JoltDocument Parse(string source, string parserSource, string path)
    {
        var capture = new CaptureDocumentPass();
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(Path.GetDirectoryName(Path.GetFullPath(path))!),
            builder =>
            {
                builder.ConfigureParserOptions(options =>
                {
                    options.UseRoslynTokenizer = true;
                    options.CSharpParseOptions = CSharpOptions;
                });
                builder.Features.Add(capture);

                // Component lowering produces public markup nodes. Stop before render-time
                // passes discard author whitespace, coalesce static HTML, or encode text.
                for (var index = builder.Features.Count - 1; index >= 0; index--)
                {
                    if (builder.Features[index] is IRazorOptimizationPass pass
                        && pass.GetType().Name is "ComponentWhitespacePass"
                            or "ComponentMarkupBlockPass" or "ComponentMarkupEncodingPass" or "ComponentTemplateDiagnosticPass")
                        builder.Features.RemoveAt(index);
                }
                var optimizer = builder.Phases.Single(phase => phase is IRazorOptimizationPhase);
                while (builder.Phases[^1] != optimizer)
                    builder.Phases.RemoveAt(builder.Phases.Count - 1);
            });

        try
        {
            engine.Process(RazorSourceDocument.Create(parserSource, path), RazorFileKind.Component, [], TagHelperCollection.Empty);
        }
        catch (NullReferenceException exception) when (parserSource.EndsWith('@')
            && exception.TargetSite?.DeclaringType?.FullName == "Microsoft.AspNetCore.Razor.Language.Legacy.CSharpCodeParser")
        {
            // Razor's Roslyn tokenizer can throw on a terminal transition inside
            // C# while the author is typing @<. Keep offsets stable for recovery.
            var recovered = Parse(source, parserSource[..^1] + " ", path);
            return recovered with { Diagnostics = recovered.Diagnostics.Add(new JoltDiagnostic("JOLT1001",
                "Complete the Razor transition.", DiagnosticSeverity.Error, new TextSpan(source.Length - 1, 1))) };
        }
        return new Projection(source).Convert(capture, path);
    }

    private sealed class CaptureDocumentPass : IntermediateNodePassBase, IRazorDocumentClassifierPass
    {
        public DocumentIntermediateNode? Document { get; private set; }
        public List<(string Name, SourceSpan Span)> UnsupportedDirectives { get; } = [];
        public List<SourceSpan> AuthorCode { get; } = [];
        public List<IntermediateNode> Templates { get; } = [];
        public override int Order => int.MinValue;

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode, CancellationToken cancellationToken)
        {
            Document = documentNode;
            // Razor removes file directives during optimization. Read its structured
            // directive nodes before that happens so markup and C# text cannot match.
            CollectDirectives(documentNode);
        }

        private void CollectDirectives(IntermediateNode node)
        {
            if (node is DirectiveIntermediateNode { Source: { } span } directive)
            {
                if (directive.DirectiveName is "code" or "functions")
                    AuthorCode.Add(span);
                else if (directive.DirectiveName != "typeparam")
                    UnsupportedDirectives.Add((directive.DirectiveName, span));
            }
            if (IsTemplate(node))
                Templates.Add(node);
            foreach (var child in node.Children)
                CollectDirectives(child);
        }
    }

    private sealed class Projection(string source)
    {
        private readonly ImmutableArray<JoltDiagnostic>.Builder _diagnostics = ImmutableArray.CreateBuilder<JoltDiagnostic>();
        private ImmutableArray<JoltInlineTemplate> _templates = [];

        public JoltDocument Convert(CaptureDocumentPass capture, string path)
        {
            var document = capture.Document!;
            foreach (var diagnostic in document.GetAllDiagnostics())
            {
                var span = Span(diagnostic.Span);
                // Jolt resolves component tags and directive attributes in its own stages.
                if (diagnostic.Id is "RZ10012" or "RZ10028")
                    continue;
                // Razor treats every attribute as a unique markup key. Jolt
                // preserves repeated splats as separate, ordered bag expressions.
                if (diagnostic.Id == "RZ10007" &&
                    Slice(span).TrimStart() is var attribute && attribute.StartsWith("@attributes", StringComparison.Ordinal) &&
                    (attribute.Length == "@attributes".Length ||
                        char.IsWhiteSpace(attribute["@attributes".Length]) || attribute["@attributes".Length] == '='))
                    continue;
                _diagnostics.Add(new JoltDiagnostic(diagnostic.Id, diagnostic.GetMessage(),
                    diagnostic.Severity == RazorDiagnosticSeverity.Error ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, span));
            }

            var nodes = ImmutableArray.CreateBuilder<JoltNode>();
            foreach (var (name, raw) in capture.UnsupportedDirectives)
            {
                var span = TransitionSpan(Span(raw));
                var nameSpan = new TextSpan(span.Start, Math.Min(name.Length + 1, span.Length));
                UnsupportedDirective("@" + name, nameSpan);
                if (name is "page" or "layout")
                {
                    var argument = Trim(TextSpan.FromBounds(nameSpan.End, span.End));
                    nodes.Add(new JoltRouteDirective(name, nameSpan, Slice(argument), argument, span));
                }
            }

            if (document.FindPrimaryNamespace() is { } namespaceNode)
            {
                foreach (var directive in namespaceNode.Children.OfType<UsingDirectiveIntermediateNode>())
                {
                    if (directive.Source is not { } raw)
                        continue;
                    var span = Span(raw);
                    var tokens = SyntaxFactory.ParseTokens(Slice(span), initialTokenPosition: span.Start, options: CSharpOptions).ToArray();
                    if (tokens.Length < 2 || !tokens[0].IsKind(SyntaxKind.UsingKeyword))
                        continue;
                    var end = tokens[^1].SpanStart;
                    if (tokens[^2].IsKind(SyntaxKind.SemicolonToken))
                        end = tokens[^2].SpanStart;
                    var codeSpan = Trim(TextSpan.FromBounds(tokens[0].Span.End, end));
                    nodes.Add(new JoltUsing(Slice(codeSpan), codeSpan, TransitionSpan(span)));
                }
            }

            if (document.FindPrimaryClass() is { } classNode)
            {
                foreach (var parameter in classNode.TypeParameters)
                {
                    if (parameter.Name.Source is not { } raw)
                        continue;
                    var name = Span(raw);
                    var lineStart = source.LastIndexOf('\n', Math.Max(0, name.Start - 1)) + 1;
                    var start = source.IndexOf('@', lineStart, name.Start - lineStart);
                    var constraint = parameter.Constraints?.Source is { } constraintSource ? Trim(Span(constraintSource)) : (TextSpan?)null;
                    var span = TextSpan.FromBounds(start >= 0 ? start : name.Start, constraint?.End ?? name.End);
                    if (!SyntaxFacts.IsValidIdentifier(parameter.Name.Content))
                        _diagnostics.Add(new("JOLT1020", "Use @typeparam followed by a type parameter name and an optional where clause.", DiagnosticSeverity.Error, span));
                    else
                        nodes.Add(new JoltTypeParameter(parameter.Name.Content, name, span) { ConstraintSpan = constraint });
                }
                foreach (var code in classNode.Children.OfType<CSharpCodeIntermediateNode>())
                {
                    if (code.Source is not { } raw)
                        continue;
                    var codeSpan = Span(raw);
                    var opener = codeSpan.Start - 1;
                    if (opener < 0 || source[opener] != '{')
                        continue;
                    // The IR exposes the class body span. Recover its directive prefix
                    // from the author text so projections can also map the transition.
                    var directive = Regex.Match(source[..opener], @"@(?:code|functions)\b(?:\s|/\*[\s\S]*?\*/|//[^\r\n]*)*\z", RegexOptions.RightToLeft);
                    if (!directive.Success)
                        continue;
                    var authored = capture.AuthorCode.Select(Span).FirstOrDefault(span => span.Start == directive.Index);
                    // Razor splits @code at each inline template. Use the original
                    // directive extent to recover the single authored C# block.
                    if (authored.End > codeSpan.End && source[authored.End - 1] == '}')
                        codeSpan = TextSpan.FromBounds(codeSpan.Start, authored.End - 1);
                    var end = codeSpan.End;
                    if (end < source.Length && source[end] == '}')
                        end++;
                    nodes.Add(new JoltCode(Slice(codeSpan), codeSpan, TextSpan.FromBounds(directive.Index, end)));
                }
            }

            // Project inside-out so a C# statement in a template body can mask a
            // nested template before Roslyn reads the surrounding C# tokens.
            foreach (var template in capture.Templates.Where(template => template.Source.HasValue).AsEnumerable().Reverse())
                _templates = _templates.Add(new JoltInlineTemplate(ConvertBody(template.Children), TransitionSpan(Span(template.Source!.Value))));
            _templates = _templates.OrderBy(template => template.Span.Start).ToImmutableArray();
            if (document.FindPrimaryMethod() is { } method)
                nodes.AddRange(ConvertBody(method.Children));
            return new JoltDocument(source, path, nodes.Select(ScopedStyle).OrderBy(node => node.Span.Start).ToImmutableArray(), _diagnostics.ToImmutable())
            {
                InlineTemplates = _templates,
            };
        }

        private JoltNode ScopedStyle(JoltNode node)
        {
            if (node is not JoltElement { Name: "style", ClosingNameSpan: { } closing } element)
                return node;
            var scoped = element.Attributes.Any(attribute => attribute.Name == "scoped");
            if (!scoped && element.Attributes.Length != 0)
                return node;
            if (scoped && element.Attributes is not [{ Name: "scoped", Value.Length: 0 }])
            {
                _diagnostics.Add(new("JOLT1019", "Use a plain stylesheet inside <style scoped>.", DiagnosticSeverity.Error, element.NameSpan));
                return node;
            }
            var cssStart = source.IndexOf('>', element.NameSpan.End) + 1;
            var css = TextSpan.FromBounds(cssStart, closing.Start - 2);
            // Slice the raw author text so CSS retains comments, escapes and entities.
            return new JoltScopedStyle(Slice(css), css, element.Span) { Scoped = scoped };
        }

        private ImmutableArray<JoltNode> ConvertBody(IEnumerable<IntermediateNode> body, bool decodeEntities = true)
        {
            var parts = new List<BodyPart>();
            foreach (var item in body)
            {
                if (item.Source is not { } raw)
                    continue;
                var span = Span(raw);
                switch (item)
                {
                    case MarkupElementIntermediateNode element:
                        var name = new TextSpan(span.Start + 1, element.TagName.Length);
                        parts.Add(new BodyPart(new JoltElement(element.TagName, name,
                            element.Attributes.Select(ConvertAttribute).ToImmutableArray(),
                            ConvertBody(element.Body, !IsRawTextElement(element.TagName)), span)
                        {
                            ClosingNameSpan = ClosingName(span, element.TagName),
                        }));
                        break;
                    case HtmlContentIntermediateNode html:
                        var value = string.Concat(html.Children.OfType<IntermediateToken>().Select(token => token.Content));
                        if (value.Length != 0)
                            parts.Add(new BodyPart(new JoltText(decodeEntities ? WebUtility.HtmlDecode(value) : value,
                                IncludeEscapedTransition(span))));
                        break;
                    case CSharpExpressionIntermediateNode:
                        parts.Add(new BodyPart(Expression(span)));
                        break;
                    case CSharpCodeIntermediateNode:
                        foreach (var token in SyntaxFactory.ParseTokens(Slice(span), initialTokenPosition: span.Start, options: CSharpOptions))
                            if (!token.IsKind(SyntaxKind.EndOfFileToken))
                                parts.Add(new BodyPart(token));
                        break;
                    case var template when IsTemplate(template):
                        // Supply a length-preserving C# placeholder for statement parsing.
                        var templateSpan = TransitionSpan(span);
                        var placeholder = templateSpan.Length >= 4 ? "null" : "0";
                        parts.Add(new BodyPart(SyntaxFactory.ParseTokens(placeholder, initialTokenPosition: templateSpan.Start).First()));
                        break;
                }
            }
            return new BodyProjection(this, parts).ReadBody();
        }

        private static bool IsRawTextElement(string tagName)
            => tagName.ToLowerInvariant() is "script" or "style" or "iframe" or "xmp" or "noembed" or "noframes";

        private TextSpan? ClosingName(TextSpan element, string tag)
        {
            // The IR carries the complete element span. Match its trailing end tag,
            // including optional whitespace before '>', without inspecting child text.
            var end = element.End - 1;
            if (tag.Length == 0 || end <= element.Start || source[end] != '>')
                return null;
            while (end > element.Start && char.IsWhiteSpace(source[end - 1]))
                end--;
            var start = end - tag.Length;
            return start > element.Start + 2 && source.AsSpan(start - 2, 2).SequenceEqual("</") &&
                source.AsSpan(start, tag.Length).Equals(tag, StringComparison.OrdinalIgnoreCase)
                ? new TextSpan(start, tag.Length)
                : null;
        }

        private JoltAttribute ConvertAttribute(HtmlAttributeIntermediateNode attribute)
        {
            var span = Span(attribute.Source!.Value);
            var name = attribute.AttributeName;
            var nameOffset = attribute.Prefix.AsSpan().IndexOf(name, StringComparison.Ordinal);
            var nameSpan = new TextSpan(span.Start + nameOffset, name.Length);
            var (kind, target, modifier) = AttributeKind(name);
            if (kind is JoltAttributeKind.UnsupportedDirective)
                UnsupportedDirective(name, nameSpan);

            var values = ImmutableArray.CreateBuilder<JoltNode>();
            var valueStart = span.Start + attribute.Prefix.Length;
            var valueEnd = span.End - (attribute.Suffix?.Length ?? 0);
            var cursor = valueStart;

            foreach (var expression in attribute.Children.OfType<CSharpExpressionAttributeValueIntermediateNode>())
            {
                if (expression.Source is not { } raw)
                    continue;
                var expressionSpan = Span(raw);
                var transition = expressionSpan.Start + expression.Prefix.Length;
                if (cursor < transition)
                    values.Add(AttributeText(TextSpan.FromBounds(cursor, transition)));
                var tokens = expression.Children.OfType<CSharpIntermediateToken>().Where(token => token.Source.HasValue).ToArray();
                var codeSpan = tokens.Length != 0
                    ? TextSpan.FromBounds(Span(tokens[0].Source!.Value).Start, Span(tokens[^1].Source!.Value).End)
                    : new TextSpan(expressionSpan.End, 0);
                values.Add(new JoltExpression(Slice(codeSpan), codeSpan, TextSpan.FromBounds(transition, expressionSpan.End)));
                cursor = expressionSpan.End;
            }

            if (kind is JoltAttributeKind.Event or JoltAttributeKind.EventModifier or JoltAttributeKind.Bind or JoltAttributeKind.Key
                or JoltAttributeKind.Ref or JoltAttributeKind.Attributes
                && values.Count == 0 && cursor < valueEnd)
            {
                var codeSpan = Trim(TextSpan.FromBounds(cursor, valueEnd));
                values.Add(new JoltExpression(Slice(codeSpan), codeSpan, codeSpan));
            }
            else if (cursor < valueEnd || values.Count == 0 && cursor == valueEnd && attribute.Prefix.Contains('='))
            {
                values.Add(AttributeText(TextSpan.FromBounds(cursor, valueEnd)));
            }
            return new JoltAttribute(name, nameSpan, values.ToImmutable(), span)
            {
                Kind = kind,
                Target = target,
                Modifier = modifier,
            };
        }

        private static (JoltAttributeKind Kind, string? Target, string? Modifier) AttributeKind(string name)
        {
            switch (name)
            {
                case "@key": return (JoltAttributeKind.Key, null, null);
                case "@attributes": return (JoltAttributeKind.Attributes, null, null);
                case "@ref": return (JoltAttributeKind.Ref, null, null);
                case "@bind": return (JoltAttributeKind.Bind, null, null);
                case "@bind:event": return (JoltAttributeKind.BindEvent, null, null);
            }

            var separator = name.IndexOf(':');
            var directive = separator < 0 ? name : name[..separator];
            if (directive == "@bind" || directive.StartsWith("@bind-", StringComparison.Ordinal) && directive.Length > 6)
            {
                var modifier = separator < 0 ? null : name[(separator + 1)..];
                if (modifier is null or "get" or "set" or "after")
                    return (JoltAttributeKind.Bind, directive.Length == 5 ? null : directive[6..], modifier);
            }

            if (name.StartsWith("@on", StringComparison.Ordinal) && name.Length > 3 && name[3] != ':')
            {
                // npm event names can contain colons, for example update:open.
                separator = name.LastIndexOf(':');
                if (separator > 3 && name[(separator + 1)..] is "preventDefault" or "stopPropagation")
                    return (JoltAttributeKind.EventModifier, name[3..separator], name[(separator + 1)..]);
                return (JoltAttributeKind.Event, name[3..], null);
            }

            return (name.StartsWith('@') ? JoltAttributeKind.UnsupportedDirective : JoltAttributeKind.Markup, null, null);
        }

        private void UnsupportedDirective(string name, TextSpan span)
        {
            if (name == "@on")
            {
                _diagnostics.Add(new("JOLT1016", "The event attribute requires an event name, such as @onclick.", DiagnosticSeverity.Error, span));
                return;
            }
            var guidance = name switch
            {
                "@attributes" => "Use explicit attributes.",
                var bind when bind.StartsWith("@bind:", StringComparison.Ordinal)
                    || bind.StartsWith("@bind-", StringComparison.Ordinal) && bind.Contains(':') =>
                    "Use an explicit property and event handler.",
                "@page" or "@layout" => "Enable jolt.router in package.json or configure routing and layout in the Vue application.",
                _ => null,
            };
            _diagnostics.Add(new("JOLT1022", $"Jolt does not support {name}.{(guidance is null ? "" : " " + guidance)}",
                DiagnosticSeverity.Error, span));
        }

        private JoltText AttributeText(TextSpan span)
            => new(WebUtility.HtmlDecode(Slice(span).Replace("@@", "@", StringComparison.Ordinal)), span);

        private JoltExpression Expression(TextSpan codeSpan)
        {
            var span = TransitionSpan(codeSpan);
            if (span == codeSpan && span.Start >= 2 && source.AsSpan(span.Start - 2, 2).SequenceEqual("@("))
            {
                var end = span.End < source.Length && source[span.End] == ')' ? span.End + 1 : span.End;
                span = TextSpan.FromBounds(span.Start - 2, end);
            }
            return new JoltExpression(Slice(codeSpan), codeSpan, span);
        }

        private TextSpan TransitionSpan(TextSpan span)
            => span.Start > 0 && source[span.Start - 1] == '@' ? TextSpan.FromBounds(span.Start - 1, span.End) : span;

        private TextSpan IncludeEscapedTransition(TextSpan span)
            => span.Start > 0 && source[span.Start - 1] == '@' && source[span.Start] == '@'
                ? TextSpan.FromBounds(span.Start - 1, span.End) : span;

        private TextSpan Span(SourceSpan span)
        {
            var start = Math.Clamp(span.AbsoluteIndex, 0, source.Length);
            return new TextSpan(start, Math.Clamp(span.Length, 0, source.Length - start));
        }

        private TextSpan Trim(TextSpan span)
        {
            var start = span.Start;
            var end = span.End;
            while (start < end && char.IsWhiteSpace(source[start]))
                start++;
            while (end > start && char.IsWhiteSpace(source[end - 1]))
                end--;
            return TextSpan.FromBounds(start, end);
        }

        private string Slice(TextSpan span) => source.Substring(span.Start, span.Length);

        private string CodeSlice(TextSpan span)
        {
            if (!_templates.Any(template => span.Contains(template.Span)))
                return Slice(span);
            var text = Slice(span).ToCharArray();
            foreach (var template in _templates.Where(template => span.Contains(template.Span)))
            {
                var offset = template.Span.Start - span.Start;
                Array.Fill(text, ' ', offset, template.Span.Length);
                "null".CopyTo(0, text, offset, Math.Min(4, template.Span.Length));
            }
            return new string(text);
        }

        private readonly record struct BodyPart(JoltNode? Node, SyntaxToken Token)
        {
            public BodyPart(JoltNode node) : this(node, default) { }
            public BodyPart(SyntaxToken token) : this(null, token) { }
            public TextSpan Span => Node?.Span ?? Token.Span;
        }

        // Razor emits statement fragments around markup. The token cursor recombines
        // those fragments without ever parsing markup as C# or counting braces in strings.
        private sealed class BodyProjection(Projection owner, List<BodyPart> parts)
        {
            private int _index;
            private SyntaxKind Kind => _index < parts.Count ? parts[_index].Token.Kind() : SyntaxKind.None;
            private int PreviousEnd => _index > 0 ? parts[_index - 1].Span.End : 0;

            public ImmutableArray<JoltNode> ReadBody(bool inBlock = false, bool inSwitchSection = false)
            {
                var nodes = ImmutableArray.CreateBuilder<JoltNode>();
                while (_index < parts.Count)
                {
                    if (inBlock && Kind == SyntaxKind.CloseBraceToken || inSwitchSection && IsSwitchLabel)
                        break;
                    if (parts[_index].Node is { } node)
                    {
                        nodes.Add(node);
                        _index++;
                    }
                    else if (Kind is SyntaxKind.IfKeyword or SyntaxKind.ForEachKeyword or SyntaxKind.ForKeyword or SyntaxKind.SwitchKeyword)
                    {
                        nodes.Add(ReadControlFlow());
                    }
                    else if (Kind == SyntaxKind.OpenBraceToken)
                    {
                        var start = parts[_index].Span.Start;
                        var children = ReadBlock();
                        nodes.Add(new JoltBlock(children, TextSpan.FromBounds(start, PreviousEnd)));
                    }
                    else if (ReadSimpleStatement() is { } statement)
                        nodes.Add(statement);
                    else
                    {
                        var span = parts[_index++].Span;
                        owner._diagnostics.Add(new JoltDiagnostic("JOLT1001",
                            "Use a local declaration, expression statement, break, continue, if, foreach, for, or switch in markup.",
                            DiagnosticSeverity.Error, owner.TransitionSpan(span)));
                    }
                }
                return nodes.ToImmutable();
            }

            private JoltNode ReadControlFlow()
            {
                var kind = Kind;
                var start = parts[_index++].Span.Start;
                var headerStart = PreviousEnd;
                var hasOpeningParen = Eat(SyntaxKind.OpenParenToken);
                if (hasOpeningParen)
                    headerStart = PreviousEnd;

                var depth = 0;
                while (Kind != SyntaxKind.None)
                {
                    if (depth == 0 && (Kind == SyntaxKind.CloseParenToken
                        || !hasOpeningParen && Kind is SyntaxKind.OpenBraceToken or SyntaxKind.CloseBraceToken))
                        break;
                    if (Kind == SyntaxKind.OpenParenToken)
                        depth++;
                    else if (Kind == SyntaxKind.CloseParenToken)
                        depth--;
                    _index++;
                }

                var headerEnd = Kind == SyntaxKind.CloseParenToken ? parts[_index].Span.Start : PreviousEnd;
                var header = TextSpan.FromBounds(headerStart, headerEnd);
                Eat(SyntaxKind.CloseParenToken);
                if (kind == SyntaxKind.SwitchKeyword)
                {
                    var sections = ReadSwitchSections();
                    return new JoltSwitch(owner.Slice(header), header, sections, owner.TransitionSpan(TextSpan.FromBounds(start, PreviousEnd)));
                }

                var children = ReadBlock();
                ImmutableArray<JoltNode> otherwise = [];
                if (kind == SyntaxKind.IfKeyword && Eat(SyntaxKind.ElseKeyword))
                    otherwise = Kind == SyntaxKind.IfKeyword ? [ReadControlFlow()] : ReadBlock();

                var span = owner.TransitionSpan(TextSpan.FromBounds(start, PreviousEnd));
                return kind switch
                {
                    SyntaxKind.IfKeyword => new JoltIf(owner.Slice(header), header, children, otherwise, span),
                    SyntaxKind.ForKeyword => new JoltFor(owner.Slice(header), header, children, span),
                    _ => new JoltForEach(owner.Slice(header), header, children, span),
                };
            }

            private bool IsSwitchLabel => Kind == SyntaxKind.CaseKeyword
                || Kind == SyntaxKind.DefaultKeyword && _index + 1 < parts.Count
                    && parts[_index + 1].Token.IsKind(SyntaxKind.ColonToken);

            private ImmutableArray<JoltSwitchSection> ReadSwitchSections()
            {
                if (!Eat(SyntaxKind.OpenBraceToken))
                    return [];
                var sections = ImmutableArray.CreateBuilder<JoltSwitchSection>();
                while (_index < parts.Count && Kind != SyntaxKind.CloseBraceToken)
                {
                    var start = parts[_index].Span.Start;
                    var labels = ImmutableArray.CreateBuilder<JoltSwitchLabel>();
                    while (IsSwitchLabel && ReadSwitchLabel() is { } label)
                        labels.Add(label);
                    if (labels.Count == 0)
                    {
                        var span = parts[_index++].Span;
                        owner._diagnostics.Add(new("JOLT1001", "Expected a case or default label.", DiagnosticSeverity.Error, span));
                        continue;
                    }
                    var children = ReadBody(inBlock: true, inSwitchSection: true);
                    sections.Add(new JoltSwitchSection(labels.ToImmutable(), children, TextSpan.FromBounds(start, PreviousEnd)));
                }
                Eat(SyntaxKind.CloseBraceToken);
                return sections.ToImmutable();
            }

            private JoltSwitchLabel? ReadSwitchLabel()
            {
                const string prefix = "switch (value) { ";
                var code = ReadCodeFragment();
                // Let Roslyn locate the label colon after patterns and guards, which can
                // themselves contain colons (property patterns and conditionals).
                var syntax = (SwitchStatementSyntax)SyntaxFactory.ParseStatement(
                    prefix + owner.Slice(code) + "}", options: CSharpOptions);
                if (syntax.Sections.FirstOrDefault()?.Labels.FirstOrDefault() is not { ColonToken.IsMissing: false } label)
                    return null;
                var span = new TextSpan(code.Start, label.Span.End - prefix.Length);
                AdvanceThrough(span.End);
                return new JoltSwitchLabel(owner.Slice(span), span);
            }

            private JoltStatement? ReadSimpleStatement()
            {
                if (Kind == SyntaxKind.None)
                    return null;
                var code = ReadCodeFragment();
                var syntax = SyntaxFactory.ParseStatement(owner.CodeSlice(code), options: CSharpOptions, consumeFullText: false);
                if (syntax.Kind() is not (SyntaxKind.LocalDeclarationStatement or SyntaxKind.ExpressionStatement
                    or SyntaxKind.BreakStatement or SyntaxKind.ContinueStatement) || syntax.Span.IsEmpty)
                    return null;
                var span = new TextSpan(code.Start, syntax.Span.End);
                AdvanceThrough(span.End);
                return new JoltStatement(owner.Slice(span), span, span);
            }

            private TextSpan ReadCodeFragment()
            {
                var start = parts[_index].Span.Start;
                var end = _index + 1;
                while (end < parts.Count && parts[end].Node is null)
                    end++;
                return TextSpan.FromBounds(start, parts[end - 1].Span.End);
            }

            private void AdvanceThrough(int end)
            {
                while (_index < parts.Count && parts[_index].Node is null && parts[_index].Span.End <= end)
                    _index++;
            }

            private ImmutableArray<JoltNode> ReadBlock()
            {
                if (!Eat(SyntaxKind.OpenBraceToken))
                    return [];
                var result = ReadBody(inBlock: true);
                Eat(SyntaxKind.CloseBraceToken);
                return result;
            }

            private bool Eat(SyntaxKind kind)
            {
                if (Kind != kind)
                    return false;
                _index++;
                return true;
            }
        }
    }
}
