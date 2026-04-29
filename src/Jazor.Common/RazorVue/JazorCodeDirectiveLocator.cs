namespace ECMAScript.Contract.RazorVue;

public static class JazorCodeDirectiveLocator
{
    private static readonly string[] CodeDirectives = ["@code"];

    public static IEnumerable<JazorCodeDirectiveMatch> EnumerateCodeDirectives(string text)
    {
        foreach (var match in RazorBlockDirectiveLocator.EnumerateDirectiveBlocks(text, CodeDirectives))
        {
            yield return new JazorCodeDirectiveMatch(
                match.DirectiveIndex,
                match.DirectiveLength,
                match.OpeningBraceIndex,
                match.ClosingBraceIndex);
        }
    }

    public static bool TryFindCodeDirective(string text, out JazorCodeDirectiveMatch match)
    {
        foreach (var directiveMatch in EnumerateCodeDirectives(text))
        {
            match = directiveMatch;
            return true;
        }

        match = default;
        return false;
    }

    public static bool TryFindCodeDirectiveWithBlockBody(string text, out JazorCodeDirectiveMatch match)
    {
        foreach (var directiveMatch in EnumerateCodeDirectives(text))
        {
            if (!directiveMatch.HasBlockBody)
            {
                continue;
            }

            match = directiveMatch;
            return true;
        }

        match = default;
        return false;
    }
}

public static class RazorBlockDirectiveLocator
{
    internal static bool TrySkipCodeLiteralOrComment(string text, ref int index)
        => TrySkipQuotedLiteral(text, ref index)
            || TrySkipComment(text, ref index)
            || TrySkipRazorComment(text, ref index);

    public static IEnumerable<RazorBlockDirectiveMatch> EnumerateDirectiveBlocks(
        string text,
        IReadOnlyList<string> directives)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (directives is null)
        {
            throw new ArgumentNullException(nameof(directives));
        }

        if (directives.Count == 0)
        {
            yield break;
        }

        for (var index = 0; index < text.Length; index++)
        {
            if (TrySkipCodeLiteralOrComment(text, ref index))
            {
                continue;
            }

            if (!TryMatchDirective(text, index, directives, out var directiveLength)
                || !IsDirectiveTokenAtLineStart(text, index, directiveLength))
            {
                continue;
            }

            var braceIndex = index + directiveLength;
            while (braceIndex < text.Length && char.IsWhiteSpace(text[braceIndex]))
            {
                braceIndex++;
            }

            if (braceIndex >= text.Length || text[braceIndex] != '{')
            {
                yield return new RazorBlockDirectiveMatch(index, directiveLength, openingBraceIndex: -1, closingBraceIndex: -1);
                index = Math.Max(index + directiveLength - 1, braceIndex - 1);
                continue;
            }

            var match = new RazorBlockDirectiveMatch(
                index,
                directiveLength,
                braceIndex,
                FindMatchingClosingBrace(text, braceIndex));
            yield return match;

            if (!match.IsClosed)
            {
                yield break;
            }

            index = match.ClosingBraceIndex;
        }
    }

    private static int FindMatchingClosingBrace(string text, int openingBraceIndex)
    {
        var depth = 1;
        for (var index = openingBraceIndex + 1; index < text.Length; index++)
        {
            if (TrySkipCodeLiteralOrComment(text, ref index))
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
                        return index;
                    }

                    break;
            }
        }

        return -1;
    }

    private static bool IsDirectiveTokenAtLineStart(string text, int directiveIndex, int directiveLength)
    {
        var lineStart = directiveIndex;
        while (lineStart > 0
               && text[lineStart - 1] != '\r'
               && text[lineStart - 1] != '\n')
        {
            lineStart--;
        }

        for (var index = lineStart; index < directiveIndex; index++)
        {
            if (!char.IsWhiteSpace(text[index]))
            {
                return false;
            }
        }

        var tokenEnd = directiveIndex + directiveLength;
        if (tokenEnd >= text.Length)
        {
            return true;
        }

        var trailingCharacter = text[tokenEnd];
        return char.IsWhiteSpace(trailingCharacter) || trailingCharacter == '{';
    }

    private static bool TryMatchDirective(
        string text,
        int index,
        IReadOnlyList<string> directives,
        out int directiveLength)
    {
        foreach (var directive in directives)
        {
            if (!StartsWith(text, index, directive, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            directiveLength = directive.Length;
            return true;
        }

        directiveLength = 0;
        return false;
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
            index = SkipDelimitedComment(text, index + 2, '*', '/');
            return true;
        }

        return false;
    }

    private static bool TrySkipRazorComment(string text, ref int index)
    {
        if (text[index] != '@'
            || index + 1 >= text.Length
            || text[index + 1] != '*')
        {
            return false;
        }

        index = SkipDelimitedComment(text, index + 2, '*', '@');
        return true;
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

    private static int SkipDelimitedComment(string text, int index, char closingFirst, char closingSecond)
    {
        while (index + 1 < text.Length)
        {
            if (text[index] == closingFirst && text[index + 1] == closingSecond)
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

    private static bool StartsWith(
        string text,
        int index,
        string value,
        StringComparison comparison)
        => index >= 0
            && index + value.Length <= text.Length
            && text.AsSpan(index, value.Length).Equals(value.AsSpan(), comparison);
}

public readonly struct RazorBlockDirectiveMatch
{
    public RazorBlockDirectiveMatch(int directiveIndex, int directiveLength, int openingBraceIndex, int closingBraceIndex)
    {
        DirectiveIndex = directiveIndex;
        DirectiveLength = directiveLength;
        OpeningBraceIndex = openingBraceIndex;
        ClosingBraceIndex = closingBraceIndex;
    }

    public int DirectiveIndex { get; }

    public int DirectiveLength { get; }

    public int OpeningBraceIndex { get; }

    public int ClosingBraceIndex { get; }

    public bool HasBlockBody => OpeningBraceIndex >= 0;

    public bool IsClosed => ClosingBraceIndex >= 0;
}

public readonly struct JazorCodeDirectiveMatch
{
    public JazorCodeDirectiveMatch(int directiveIndex, int directiveLength, int openingBraceIndex, int closingBraceIndex)
    {
        DirectiveIndex = directiveIndex;
        DirectiveLength = directiveLength;
        OpeningBraceIndex = openingBraceIndex;
        ClosingBraceIndex = closingBraceIndex;
    }

    public int DirectiveIndex { get; }

    public int DirectiveLength { get; }

    public int OpeningBraceIndex { get; }

    public int ClosingBraceIndex { get; }

    public bool HasBlockBody => OpeningBraceIndex >= 0;

    public bool IsClosed => ClosingBraceIndex >= 0;
}
