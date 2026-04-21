using Jazor.Vue;

namespace Jolt.Lsp.Routing;

internal sealed class DocumentRegionClassifier
{
    private static readonly string[] TopLevelDirectives =
    [
        "@attribute",
        "@functions",
        "@implements",
        "@inherits",
        "@inject",
        "@layout",
        "@model",
        "@module",
        "@namespace",
        "@page",
        "@preservewhitespace",
        "@rendermode",
        "@typeparam",
        "@using",
        "@code"
    ];

    private static readonly string[] CodeBlockDirectives =
    [
        "@code",
        "@functions"
    ];

    public DocumentRegionKind Classify(string text, int offset)
    {
        var clampedOffset = Math.Max(0, Math.Min(offset, text.Length));
        var templateRange = FindTagBlock(text, "<template", "</template>");
        if (InRange(templateRange, clampedOffset))
        {
            return DocumentRegionKind.Template;
        }

        var codeRange = FindCodeBlock(text, clampedOffset);
        if (InRange(codeRange, clampedOffset))
        {
            return DocumentRegionKind.Code;
        }

        var markupBoundary = GetMarkupBoundary(text);
        return clampedOffset < markupBoundary
            || (markupBoundary == text.Length && clampedOffset == text.Length)
            ? DocumentRegionKind.Directive
            : DocumentRegionKind.Template;
    }

    private static (int Start, int End) FindTagBlock(string text, string openTag, string closeTag)
    {
        var openIndex = text.IndexOf(openTag, StringComparison.OrdinalIgnoreCase);
        if (openIndex < 0)
        {
            return (-1, -1);
        }

        var closeIndex = text.IndexOf(closeTag, openIndex, StringComparison.OrdinalIgnoreCase);
        if (closeIndex < 0)
        {
            return (openIndex, text.Length);
        }

        return (openIndex, closeIndex + closeTag.Length);
    }

    private static (int Start, int End) FindCodeBlock(string text, int offset)
    {
        foreach (var match in RazorBlockDirectiveLocator.EnumerateDirectiveBlocks(text, CodeBlockDirectives))
        {
            if (match.DirectiveIndex > offset)
            {
                break;
            }

            if (!match.HasBlockBody)
            {
                continue;
            }

            var range = GetBlockRange(match, text.Length);
            if (InRange(range, offset))
            {
                return range;
            }
        }

        return (-1, -1);
    }

    private static (int Start, int End) GetBlockRange(RazorBlockDirectiveMatch match, int textLength)
        => (match.DirectiveIndex, match.IsClosed ? match.ClosingBraceIndex + 1 : textLength);

    private static int GetMarkupBoundary(string text)
    {
        var lineStart = 0;
        while (lineStart < text.Length)
        {
            if (TrySkipPreambleDelimitedComment(text, lineStart, "/*", "*/", out var nextLineStart)
                || TrySkipPreambleDelimitedComment(text, lineStart, "@*", "*@", out nextLineStart))
            {
                lineStart = nextLineStart;
                continue;
            }

            var lineEnd = lineStart;
            while (lineEnd < text.Length
                   && text[lineEnd] != '\r'
                   && text[lineEnd] != '\n')
            {
                lineEnd++;
            }

            var line = text.AsSpan(lineStart, lineEnd - lineStart);
            if (IsIgnorablePreambleLine(line))
            {
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            return lineStart;
        }

        return text.Length;
    }

    private static bool TrySkipPreambleDelimitedComment(
        string text,
        int lineStart,
        string startToken,
        string endToken,
        out int nextLineStart)
    {
        var contentStart = GetLineContentStart(text, lineStart);
        if (!StartsWith(text, contentStart, startToken))
        {
            nextLineStart = -1;
            return false;
        }

        var commentEnd = text.IndexOf(endToken, contentStart + startToken.Length, StringComparison.Ordinal);
        if (commentEnd < 0)
        {
            nextLineStart = text.Length;
            return true;
        }

        var lineEnd = commentEnd + endToken.Length;
        while (lineEnd < text.Length
               && text[lineEnd] != '\r'
               && text[lineEnd] != '\n')
        {
            if (!char.IsWhiteSpace(text[lineEnd]))
            {
                nextLineStart = -1;
                return false;
            }

            lineEnd++;
        }

        nextLineStart = GetNextLineStart(text, lineEnd);
        return true;
    }

    private static int GetNextLineStart(string text, int lineEnd)
    {
        if (lineEnd >= text.Length)
        {
            return text.Length;
        }

        if (text[lineEnd] == '\r'
            && lineEnd + 1 < text.Length
            && text[lineEnd + 1] == '\n')
        {
            return lineEnd + 2;
        }

        return lineEnd + 1;
    }

    private static int GetLineContentStart(string text, int lineStart)
    {
        var index = lineStart;
        while (index < text.Length
               && text[index] != '\r'
               && text[index] != '\n'
               && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    private static bool IsWhitespace(ReadOnlySpan<char> line)
    {
        foreach (var character in line)
        {
            if (!char.IsWhiteSpace(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsIgnorablePreambleLine(ReadOnlySpan<char> line)
        => IsWhitespace(line)
            || IsTopLevelDirective(line)
            || IsSingleLineComment(line);

    private static bool IsSingleLineComment(ReadOnlySpan<char> line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("//", StringComparison.Ordinal);
    }

    private static bool IsTopLevelDirective(ReadOnlySpan<char> line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.IsEmpty || trimmed[0] != '@')
        {
            return false;
        }

        if (trimmed.Length == 1)
        {
            return true;
        }

        foreach (var directive in TopLevelDirectives)
        {
            var directiveSpan = directive.AsSpan();
            if (trimmed.StartsWith(directiveSpan, StringComparison.Ordinal)
                || directiveSpan.StartsWith(trimmed, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool StartsWith(
        string text,
        int index,
        string value,
        StringComparison comparison = StringComparison.Ordinal)
        => index >= 0
            && index + value.Length <= text.Length
            && text.AsSpan(index, value.Length).Equals(value.AsSpan(), comparison);

    private static bool InRange((int Start, int End) range, int offset)
        => range.Start >= 0
            && range.End >= range.Start
            && offset >= range.Start
            && offset <= range.End;
}
