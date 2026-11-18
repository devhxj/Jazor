using System.Collections.Immutable;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Syntax;

public static class JoltParser
{
    private static readonly CSharpParseOptions CSharpOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

    public static JoltDocument Parse(string source, string path = "Component.razor")
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
                            or "ComponentMarkupBlockPass" or "ComponentMarkupEncodingPass")
                        builder.Features.RemoveAt(index);
                }
                var optimizer = builder.Phases.Single(phase => phase is IRazorOptimizationPhase);
                while (builder.Phases[^1] != optimizer)
                    builder.Phases.RemoveAt(builder.Phases.Count - 1);
            });

        engine.Process(RazorSourceDocument.Create(source, path), RazorFileKind.Component, [], TagHelperCollection.Empty);
        return new Projection(source).Convert(capture.Document!, path);
    }

    private sealed class CaptureDocumentPass : IntermediateNodePassBase, IRazorDocumentClassifierPass
    {
        public DocumentIntermediateNode? Document { get; private set; }

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode, CancellationToken cancellationToken)
            => Document = documentNode;
    }

    private sealed class Projection(string source)
    {
        private readonly ImmutableArray<JoltDiagnostic>.Builder _diagnostics = ImmutableArray.CreateBuilder<JoltDiagnostic>();

        public JoltDocument Convert(DocumentIntermediateNode document, string path)
        {
            foreach (var diagnostic in document.GetAllDiagnostics())
            {
                var span = Span(diagnostic.Span);
                // Jolt resolves component tags and event attributes in its own binding stages.
                if (diagnostic.Id == "RZ10012"
                    || diagnostic.Id == "RZ10028" && source.AsSpan(span.Start).StartsWith("@on", StringComparison.Ordinal))
                    continue;
                _diagnostics.Add(new JoltDiagnostic(diagnostic.Id, diagnostic.GetMessage(),
                    diagnostic.Severity == RazorDiagnosticSeverity.Error ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, span));
            }

            var nodes = ImmutableArray.CreateBuilder<JoltNode>();
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
                    var span = TextSpan.FromBounds(start >= 0 ? start : name.Start, name.End);
                    if (parameter.Constraints is not null || !SyntaxFacts.IsValidIdentifier(parameter.Name.Content))
                        _diagnostics.Add(new("JOLT1020", "Use @typeparam followed by one type parameter name.", DiagnosticSeverity.Error, span));
                    else
                        nodes.Add(new JoltTypeParameter(parameter.Name.Content, name, span));
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
                    var end = codeSpan.End;
                    if (end < source.Length && source[end] == '}')
                        end++;
                    nodes.Add(new JoltCode(Slice(codeSpan), codeSpan, TextSpan.FromBounds(directive.Index, end)));
                }
            }

            if (document.FindPrimaryMethod() is { } method)
                nodes.AddRange(ConvertBody(method.Children));
            return new JoltDocument(source, path, nodes.Select(ScopedStyle).OrderBy(node => node.Span.Start).ToImmutableArray(), _diagnostics.ToImmutable());
        }

        private JoltNode ScopedStyle(JoltNode node)
        {
            if (node is not JoltElement { Name: "style", ClosingNameSpan: { } closing } element ||
                !element.Attributes.Any(attribute => attribute.Name == "scoped"))
                return node;
            if (element.Attributes is not [{ Name: "scoped", Value.Length: 0 }])
            {
                _diagnostics.Add(new("JOLT1019", "Use a plain stylesheet inside <style scoped>.", DiagnosticSeverity.Error, element.NameSpan));
                return node;
            }
            var cssStart = source.IndexOf('>', element.Attributes[0].Span.End) + 1;
            var css = TextSpan.FromBounds(cssStart, closing.Start - 2);
            // Slice the raw author text so CSS retains comments, escapes and entities.
            return new JoltScopedStyle(Slice(css), css, element.Span);
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
            var nameOffset = attribute.Prefix.AsSpan().IndexOf(attribute.AttributeName, StringComparison.Ordinal);
            var nameSpan = new TextSpan(span.Start + nameOffset, attribute.AttributeName.Length);
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

            if (attribute.AttributeName.StartsWith("@on", StringComparison.Ordinal) && values.Count == 0 && cursor < valueEnd)
            {
                var codeSpan = TextSpan.FromBounds(cursor, valueEnd);
                values.Add(new JoltExpression(Slice(codeSpan), codeSpan, codeSpan));
            }
            else if (cursor < valueEnd || values.Count == 0 && cursor == valueEnd && attribute.Prefix.Contains('='))
            {
                values.Add(AttributeText(TextSpan.FromBounds(cursor, valueEnd)));
            }
            return new JoltAttribute(attribute.AttributeName, nameSpan, values.ToImmutable(), span);
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

            public ImmutableArray<JoltNode> ReadBody(bool inBlock = false)
            {
                var nodes = ImmutableArray.CreateBuilder<JoltNode>();
                while (_index < parts.Count)
                {
                    if (inBlock && Kind == SyntaxKind.CloseBraceToken)
                        break;
                    if (parts[_index].Node is { } node)
                    {
                        nodes.Add(node);
                        _index++;
                    }
                    else if (Kind is SyntaxKind.IfKeyword or SyntaxKind.ForEachKeyword)
                    {
                        nodes.Add(ReadStatement());
                    }
                    else
                    {
                        var span = parts[_index++].Span;
                        owner._diagnostics.Add(new JoltDiagnostic("JOLT1001",
                            "Only if and foreach statements are supported in markup.", DiagnosticSeverity.Error, owner.TransitionSpan(span)));
                    }
                }
                return nodes.ToImmutable();
            }

            private JoltNode ReadStatement()
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
                var children = ReadBlock();
                ImmutableArray<JoltNode> otherwise = [];
                if (kind == SyntaxKind.IfKeyword && Eat(SyntaxKind.ElseKeyword))
                    otherwise = Kind == SyntaxKind.IfKeyword ? [ReadStatement()] : ReadBlock();

                var span = owner.TransitionSpan(TextSpan.FromBounds(start, PreviousEnd));
                return kind == SyntaxKind.IfKeyword
                    ? new JoltIf(owner.Slice(header), header, children, otherwise, span)
                    : new JoltForEach(owner.Slice(header), header, children, span);
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
