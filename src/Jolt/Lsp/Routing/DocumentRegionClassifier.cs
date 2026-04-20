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

        return clampedOffset < GetMarkupBoundary(text)
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
        foreach (var directive in CodeBlockDirectives)
        {
            var searchStart = 0;
            while (searchStart < text.Length)
            {
                var directiveIndex = text.IndexOf(directive, searchStart, StringComparison.OrdinalIgnoreCase);
                if (directiveIndex < 0)
                {
                    break;
                }

                if (directiveIndex > offset)
                {
                    break;
                }

                var range = FindBraceDelimitedDirectiveBlock(text, directiveIndex);
                if (InRange(range, offset))
                {
                    return range;
                }

                searchStart = directiveIndex + directive.Length;
            }
        }

        return (-1, -1);
    }

    private static (int Start, int End) FindBraceDelimitedDirectiveBlock(string text, int directiveIndex)
    {
        var braceIndex = text.IndexOf('{', directiveIndex);
        if (braceIndex < 0)
        {
            return (directiveIndex, text.Length);
        }

        var depth = 1;
        for (var index = braceIndex + 1; index < text.Length; index++)
        {
            if (TrySkipQuotedLiteral(text, ref index)
                || TrySkipComment(text, ref index))
            {
                continue;
            }

            switch (text[index])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return (directiveIndex, index + 1);
                    }

                    break;
            }
        }

        return (directiveIndex, text.Length);
    }

    private static bool TrySkipComment(string text, ref int index)
    {
        if (text[index] != '/' || index + 1 >= text.Length)
        {
            return false;
        }

        if (text[index + 1] == '/')
        {
            index = SkipLineComment(text, index + 2);
            return true;
        }

        if (text[index + 1] == '*')
        {
            index = SkipBlockComment(text, index + 2);
            return true;
        }

        return false;
    }

    private static bool TrySkipQuotedLiteral(string text, ref int index)
    {
        if (text[index] == '\'')
        {
            index = SkipRegularQuotedLiteral(text, index, '\'');
            return true;
        }

        if (TryGetRawStringDelimiterLength(text, index, out var rawStringStartIndex, out var rawStringDelimiterLength))
        {
            index = SkipRawStringLiteral(text, rawStringStartIndex, rawStringDelimiterLength);
            return true;
        }

        if (TryGetVerbatimStringStart(text, index, out var verbatimStringStartIndex))
        {
            index = SkipVerbatimStringLiteral(text, verbatimStringStartIndex);
            return true;
        }

        if (TryGetRegularStringStart(text, index, out var regularStringStartIndex))
        {
            index = SkipRegularQuotedLiteral(text, regularStringStartIndex, '"');
            return true;
        }

        return false;
    }

    private static int SkipLineComment(string text, int index)
    {
        while (index < text.Length
               && text[index] != '\r'
               && text[index] != '\n')
        {
            index++;
        }

        return Math.Max(0, index - 1);
    }

    private static int SkipBlockComment(string text, int index)
    {
        while (index + 1 < text.Length)
        {
            if (text[index] == '*' && text[index + 1] == '/')
            {
                return index + 1;
            }

            index++;
        }

        return text.Length - 1;
    }

    private static bool TryGetRegularStringStart(string text, int index, out int stringStartIndex)
    {
        if (text[index] == '"')
        {
            stringStartIndex = index;
            return true;
        }

        if (text[index] == '$'
            && index + 1 < text.Length
            && text[index + 1] == '"')
        {
            stringStartIndex = index + 1;
            return true;
        }

        stringStartIndex = -1;
        return false;
    }

    private static bool TryGetVerbatimStringStart(string text, int index, out int stringStartIndex)
    {
        if (text[index] == '@'
            && index + 1 < text.Length
            && text[index + 1] == '"')
        {
            stringStartIndex = index + 1;
            return true;
        }

        if (text[index] == '$'
            && index + 2 < text.Length
            && text[index + 1] == '@'
            && text[index + 2] == '"')
        {
            stringStartIndex = index + 2;
            return true;
        }

        if (text[index] == '@'
            && index + 2 < text.Length
            && text[index + 1] == '$'
            && text[index + 2] == '"')
        {
            stringStartIndex = index + 2;
            return true;
        }

        stringStartIndex = -1;
        return false;
    }

    private static bool TryGetRawStringDelimiterLength(
        string text,
        int index,
        out int rawStringStartIndex,
        out int delimiterLength)
    {
        var current = index;
        while (current < text.Length && text[current] == '$')
        {
            current++;
        }

        var quoteRunLength = CountConsecutiveQuotes(text, current);
        if (quoteRunLength < 3)
        {
            rawStringStartIndex = -1;
            delimiterLength = 0;
            return false;
        }

        rawStringStartIndex = current;
        delimiterLength = quoteRunLength;
        return true;
    }

    private static int SkipRegularQuotedLiteral(string text, int stringStartIndex, char quote)
    {
        var escaped = false;
        for (var index = stringStartIndex + 1; index < text.Length; index++)
        {
            var character = text[index];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == '\\')
            {
                escaped = true;
                continue;
            }

            if (character == quote)
            {
                return index;
            }
        }

        return text.Length - 1;
    }

    private static int SkipVerbatimStringLiteral(string text, int stringStartIndex)
    {
        for (var index = stringStartIndex + 1; index < text.Length; index++)
        {
            if (text[index] != '"')
            {
                continue;
            }

            if (index + 1 < text.Length && text[index + 1] == '"')
            {
                index++;
                continue;
            }

            return index;
        }

        return text.Length - 1;
    }

    private static int SkipRawStringLiteral(string text, int stringStartIndex, int delimiterLength)
    {
        for (var index = stringStartIndex + delimiterLength; index < text.Length; index++)
        {
            var quoteRunLength = CountConsecutiveQuotes(text, index);
            if (quoteRunLength < delimiterLength)
            {
                continue;
            }

            return index + delimiterLength - 1;
        }

        return text.Length - 1;
    }

    private static int CountConsecutiveQuotes(string text, int index)
    {
        var count = 0;
        while (index + count < text.Length && text[index + count] == '"')
        {
            count++;
        }

        return count;
    }

    private static int GetMarkupBoundary(string text)
    {
        var lineStart = 0;
        while (lineStart < text.Length)
        {
            var lineEnd = lineStart;
            while (lineEnd < text.Length
                   && text[lineEnd] != '\r'
                   && text[lineEnd] != '\n')
            {
                lineEnd++;
            }

            var line = text.AsSpan(lineStart, lineEnd - lineStart);
            if (IsWhitespace(line) || IsTopLevelDirective(line))
            {
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            return lineStart;
        }

        return text.Length;
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

    private static bool InRange((int Start, int End) range, int offset)
        => range.Start >= 0
            && range.End >= range.Start
            && offset >= range.Start
            && offset <= range.End;
}
