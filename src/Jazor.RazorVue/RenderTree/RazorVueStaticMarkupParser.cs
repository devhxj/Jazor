using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueStaticMarkupParser
{
    public sealed record Dependencies(
        Func<string, IOperation> CreateLiteralStringOperation,
        Func<string, Exception> CreateParseError);

    public static ImmutableArray<RazorVueRenderNode> Parse(
        string markup,
        ImmutableArray<RazorVueSourceOrigin> origins,
        Dependencies dependencies)
    {
        if (dependencies is null)
            throw new ArgumentNullException(nameof(dependencies));

        try
        {
            var roots = ImmutableArray.CreateBuilder<RazorVueRenderNode>();
            var openElements = new Stack<StaticElementBuilder>();
            var index = 0;

            while (index < markup.Length)
            {
                if (markup[index] != '<')
                {
                    var textStart = index;
                    while (index < markup.Length && markup[index] != '<')
                        index++;

                    AppendStaticText(markup.Substring(textStart, index - textStart));
                    continue;
                }

                if (StartsWith(markup, index, "<!--"))
                {
                    var commentEnd = markup.IndexOf("-->", index, StringComparison.Ordinal);
                    if (commentEnd < 0)
                        throw dependencies.CreateParseError($"could not parse static markup block '{markup}'.");

                    index = commentEnd + 3;
                    continue;
                }

                if (index + 1 < markup.Length && markup[index + 1] == '/')
                {
                    index += 2;
                    SkipWhitespace(markup, ref index);
                    var tagName = ReadName(markup, ref index);
                    SkipWhitespace(markup, ref index);
                    Expect(markup, ref index, '>');

                    if (openElements.Count == 0)
                        throw dependencies.CreateParseError($"found an unmatched closing tag '</{tagName}>'.");

                    var element = openElements.Pop();
                    if (!string.Equals(element.TagName, tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw dependencies.CreateParseError(
                            $"found a mismatched closing tag '</{tagName}>' for '<{element.TagName}>'.");
                    }

                    AddNode(element.Build());
                    continue;
                }

                index++;
                SkipWhitespace(markup, ref index);
                var startTagName = ReadName(markup, ref index);
                var attributes = ImmutableArray.CreateBuilder<RazorVueAttributeEntry>();
                var selfClosing = false;

                while (index < markup.Length)
                {
                    SkipWhitespace(markup, ref index);
                    if (index >= markup.Length)
                        break;

                    if (markup[index] == '>')
                    {
                        index++;
                        break;
                    }

                    if (markup[index] == '/' && index + 1 < markup.Length && markup[index + 1] == '>')
                    {
                        selfClosing = true;
                        index += 2;
                        break;
                    }

                    var attributeName = ReadName(markup, ref index);
                    SkipWhitespace(markup, ref index);

                    IOperation? attributeValue = null;
                    if (index < markup.Length && markup[index] == '=')
                    {
                        index++;
                        SkipWhitespace(markup, ref index);
                        attributeValue = dependencies.CreateLiteralStringOperation(ReadAttributeValue(markup, ref index));
                    }

                    attributes.Add(new RazorVueAttributeNode(attributeName, attributeValue, origins));
                }

                var builder = new StaticElementBuilder(startTagName, attributes.ToImmutable(), origins);
                if (selfClosing || VoidElementNames.Contains(startTagName))
                {
                    AddNode(builder.Build());
                    continue;
                }

                openElements.Push(builder);
            }

            if (openElements.Count > 0)
            {
                var unclosed = openElements.Peek();
                throw dependencies.CreateParseError($"found an unclosed static tag '<{unclosed.TagName}>'.");
            }

            return roots.ToImmutable();

            void AppendStaticText(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return;

                AddNode(new RazorVueTextNode(text, origins));
            }

            void AddNode(RazorVueRenderNode node)
            {
                if (openElements.Count == 0)
                {
                    roots.Add(node);
                    return;
                }

                openElements.Peek().Children.Add(node);
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException)
        {
            throw dependencies.CreateParseError($"could not parse static markup block '{markup}': {exception.Message}");
        }
    }

    private static readonly HashSet<string> VoidElementNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "area",
        "base",
        "br",
        "col",
        "embed",
        "hr",
        "img",
        "input",
        "link",
        "meta",
        "param",
        "source",
        "track",
        "wbr"
    };

    private static void SkipWhitespace(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
    }

    private static string ReadName(string text, ref int index)
    {
        var start = index;
        while (index < text.Length)
        {
            var current = text[index];
            if (char.IsWhiteSpace(current) || current is '=' or '>' or '/')
                break;
            index++;
        }

        if (index == start)
            throw new InvalidOperationException("Expected name in static markup.");

        return text.Substring(start, index - start);
    }

    private static string ReadAttributeValue(string text, ref int index)
    {
        if (index >= text.Length)
            return string.Empty;

        var quote = text[index];
        if (quote is '"' or '\'')
        {
            index++;
            var start = index;
            while (index < text.Length && text[index] != quote)
                index++;

            var value = text.Substring(start, index - start);
            Expect(text, ref index, quote);
            return value;
        }

        var unquotedStart = index;
        while (index < text.Length)
        {
            var current = text[index];
            if (char.IsWhiteSpace(current) || current is '>' or '/')
                break;
            index++;
        }

        return text.Substring(unquotedStart, index - unquotedStart);
    }

    private static bool StartsWith(string text, int index, string value)
        => index >= 0 &&
           index + value.Length <= text.Length &&
           string.Compare(text, index, value, 0, value.Length, StringComparison.Ordinal) == 0;

    private static void Expect(string text, ref int index, char expected)
    {
        if (index >= text.Length || text[index] != expected)
            throw new InvalidOperationException($"Expected '{expected}' in static markup.");

        index++;
    }

    private sealed class StaticElementBuilder(
        string tagName,
        ImmutableArray<RazorVueAttributeEntry> attributes,
        ImmutableArray<RazorVueSourceOrigin> origins)
    {
        public string TagName { get; } = tagName;
        public ImmutableArray<RazorVueAttributeEntry> Attributes { get; } = attributes;
        public ImmutableArray<RazorVueSourceOrigin> Origins { get; } = origins;
        public ImmutableArray<RazorVueRenderNode>.Builder Children { get; } = ImmutableArray.CreateBuilder<RazorVueRenderNode>();

        public RazorVueElementNode Build()
            => new(
                TagName,
                Key: null,
                Attributes,
                new RazorVueRenderFragment(Children.ToImmutable()),
                Origins);
    }
}
