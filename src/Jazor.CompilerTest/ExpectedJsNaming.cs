using System.Text;

namespace Jazor.ComplierTest;

internal static class ExpectedJsNaming
{
    public static string Normalize(string text)
    {
        var builder = new StringBuilder(text.Length);
        var state = ParseState.Code;
        var templateDepth = 0;

        for (var index = 0; index < text.Length;)
        {
            var ch = text[index];
            switch (state)
            {
                case ParseState.Code:
                case ParseState.TemplateExpression:
                    if (ch == '\'')
                    {
                        AppendQuoted(text, builder, ref index, '\'');
                        continue;
                    }

                    if (ch == '"')
                    {
                        AppendDoubleQuoted(text, builder, ref index);
                        continue;
                    }

                    if (state == ParseState.Code && ch == '`')
                    {
                        builder.Append(ch);
                        index++;
                        state = ParseState.TemplateText;
                        continue;
                    }

                    if (IsIdentifierStart(ch))
                    {
                        var start = index;
                        index++;
                        while (index < text.Length && IsIdentifierPart(text[index]))
                            index++;

                        var token = text[start..index];
                        var previous = PreviousSignificantChar(text, start - 1);
                        var nextIndex = SkipWhitespace(text, index);
                        var next = nextIndex < text.Length ? text[nextIndex] : '\0';
                        var isPropertyKey = next == ':' && IsPropertyKeyContext(text, start - 1);
                        if (previous == '.' || isPropertyKey)
                            token = Camel(token);

                        builder.Append(token);
                        continue;
                    }

                    builder.Append(ch);
                    index++;

                    if (state == ParseState.TemplateExpression)
                    {
                        if (ch == '{')
                            templateDepth++;
                        else if (ch == '}')
                        {
                            templateDepth--;
                            if (templateDepth == 0)
                                state = ParseState.TemplateText;
                        }
                    }
                    continue;

                case ParseState.TemplateText:
                    builder.Append(ch);
                    index++;
                    if (ch == '`')
                    {
                        state = ParseState.Code;
                        continue;
                    }

                    if (ch == '$' && index < text.Length && text[index] == '{')
                    {
                        builder.Append('{');
                        index++;
                        state = ParseState.TemplateExpression;
                        templateDepth = 1;
                    }
                    continue;

                default:
                    throw new InvalidOperationException("Unexpected parser state.");
            }
        }

        return builder.ToString();
    }

    private static void AppendDoubleQuoted(string text, StringBuilder builder, ref int index)
    {
        var start = index;
        index++;
        var contentStart = index;
        var escaped = false;
        while (index < text.Length)
        {
            var ch = text[index];
            if (escaped)
            {
                escaped = false;
                index++;
                continue;
            }

            if (ch == '\\')
            {
                escaped = true;
                index++;
                continue;
            }

            if (ch == '"')
                break;

            index++;
        }

        if (index >= text.Length)
        {
            builder.Append(text[start..]);
            return;
        }

        var content = text[contentStart..index];
        var previous = PreviousSignificantChar(text, start - 1);
        var nextIndex = SkipWhitespace(text, index + 1);
        if (IsSimpleIdentifier(content) &&
            (StartsWith(text, nextIndex, "in") || (previous == '[' && nextIndex < text.Length && text[nextIndex] == ']')))
            content = Camel(content);

        builder.Append('"');
        builder.Append(content);
        builder.Append('"');
        index++;
    }

    private static void AppendQuoted(string text, StringBuilder builder, ref int index, char quote)
    {
        builder.Append(quote);
        index++;
        var escaped = false;
        while (index < text.Length)
        {
            var ch = text[index];
            builder.Append(ch);
            index++;
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (ch == '\\')
            {
                escaped = true;
                continue;
            }

            if (ch == quote)
                return;
        }
    }

    private static bool IsPropertyKeyContext(string text, int index)
    {
        var previous = PreviousSignificantChar(text, index);
        return previous == '{' || previous == ',' || previous == '[';
    }

    private static bool StartsWith(string text, int index, string value)
    {
        if (index + value.Length > text.Length)
            return false;

        for (var i = 0; i < value.Length; i++)
        {
            if (text[index + i] != value[i])
                return false;
        }

        return true;
    }

    private static int SkipWhitespace(string text, int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
        return index;
    }

    private static char PreviousSignificantChar(string text, int index)
    {
        while (index >= 0)
        {
            var ch = text[index];
            if (!char.IsWhiteSpace(ch))
                return ch;
            index--;
        }

        return '\0';
    }

    private static bool IsSimpleIdentifier(string text)
    {
        if (string.IsNullOrEmpty(text) || !IsIdentifierStart(text[0]))
            return false;

        for (var index = 1; index < text.Length; index++)
        {
            if (!IsIdentifierPart(text[index]))
                return false;
        }

        return true;
    }

    private static bool IsIdentifierStart(char ch)
        => char.IsLetter(ch) || ch == '_' || ch == '$';

    private static bool IsIdentifierPart(char ch)
        => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$';

    private static string Camel(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        var underscoreIndex = name.IndexOf('_');
        if (underscoreIndex >= 0)
        {
            var prefix = name[..underscoreIndex];
            if (prefix.Any(char.IsLower))
                return Camel(prefix) + name[underscoreIndex..];

            return name;
        }

        if (name.Length == 1)
            return char.ToLowerInvariant(name[0]).ToString();

        var chars = name.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);
        for (var index = 1; index < chars.Length; index++)
        {
            if (!char.IsUpper(chars[index]))
                break;

            var hasNext = index + 1 < chars.Length;
            if (hasNext && !char.IsUpper(chars[index + 1]))
                break;

            chars[index] = char.ToLowerInvariant(chars[index]);
        }

        return new string(chars);
    }

    private enum ParseState
    {
        Code,
        TemplateText,
        TemplateExpression,
    }
}
