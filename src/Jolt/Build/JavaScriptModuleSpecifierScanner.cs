using System.Text;

namespace Jolt.Build;

internal enum JavaScriptModuleSpecifierKind
{
    StaticImport,
    ReExport,
    DynamicImport
}

internal readonly record struct JavaScriptModuleSpecifier(
    JavaScriptModuleSpecifierKind Kind,
    int ValueStart,
    int ValueLength,
    string Value,
    int ExpressionStart,
    int ExpressionLength);

internal static class JavaScriptModuleSpecifierScanner
{
    public static IEnumerable<JavaScriptModuleSpecifier> EnumerateSpecifiers(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        for (var index = 0; index < source.Length;)
        {
            if (TrySkipLiteralOrComment(source, ref index))
            {
                index++;
                continue;
            }

            if (!IsIdentifierStart(source[index]))
            {
                index++;
                continue;
            }

            var identifierStart = index;
            index++;
            while (index < source.Length && IsIdentifierPart(source[index]))
            {
                index++;
            }

            var identifier = source[identifierStart..index];
            if (string.Equals(identifier, "import", StringComparison.Ordinal)
                && TryReadImportSpecifier(source, identifierStart, index, out var importSpecifier))
            {
                yield return importSpecifier;
                index = Math.Max(index, GetSkipEnd(importSpecifier));
                continue;
            }

            if (string.Equals(identifier, "export", StringComparison.Ordinal)
                && TryReadReExportSpecifier(source, identifierStart, index, out var exportSpecifier))
            {
                yield return exportSpecifier;
                index = Math.Max(index, GetSkipEnd(exportSpecifier));
                continue;
            }
        }
    }

    public static string RewriteSpecifiers(
        string source,
        Func<JavaScriptModuleSpecifier, string?> rewriteSpecifier)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rewriteSpecifier);

        var builder = new StringBuilder(source.Length);
        var lastIndex = 0;
        var changed = false;
        foreach (var specifier in EnumerateSpecifiers(source))
        {
            var replacement = rewriteSpecifier(specifier);
            if (replacement is null || string.Equals(replacement, specifier.Value, StringComparison.Ordinal))
            {
                continue;
            }

            builder.Append(source, lastIndex, specifier.ValueStart - lastIndex);
            builder.Append(replacement);
            lastIndex = specifier.ValueStart + specifier.ValueLength;
            changed = true;
        }

        if (!changed)
        {
            return source;
        }

        builder.Append(source, lastIndex, source.Length - lastIndex);
        return builder.ToString();
    }

    public static string RewriteDynamicImportExpressions(
        string source,
        Func<JavaScriptModuleSpecifier, string?> rewriteExpression)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rewriteExpression);

        var builder = new StringBuilder(source.Length);
        var lastIndex = 0;
        var changed = false;
        foreach (var specifier in EnumerateSpecifiers(source)
                     .Where(static specifier => specifier.Kind == JavaScriptModuleSpecifierKind.DynamicImport))
        {
            var replacement = rewriteExpression(specifier);
            if (replacement is null)
            {
                continue;
            }

            builder.Append(source, lastIndex, specifier.ExpressionStart - lastIndex);
            builder.Append(replacement);
            lastIndex = specifier.ExpressionStart + specifier.ExpressionLength;
            changed = true;
        }

        if (!changed)
        {
            return source;
        }

        builder.Append(source, lastIndex, source.Length - lastIndex);
        return builder.ToString();
    }

    public static (string Path, string Suffix) SplitPathAndSuffix(string specifier)
    {
        var suffixIndex = specifier.IndexOfAny(['?', '#']);
        return suffixIndex < 0
            ? (specifier, string.Empty)
            : (specifier[..suffixIndex], specifier[suffixIndex..]);
    }

    private static int GetSkipEnd(JavaScriptModuleSpecifier specifier)
        => specifier.ExpressionLength > 0
            ? specifier.ExpressionStart + specifier.ExpressionLength
            : specifier.ValueStart + specifier.ValueLength + 1;

    private static bool TryReadImportSpecifier(
        string source,
        int keywordStart,
        int keywordEnd,
        out JavaScriptModuleSpecifier specifier)
    {
        var cursor = SkipWhitespaceAndComments(source, keywordEnd);
        if (cursor >= source.Length || source[cursor] == '.')
        {
            specifier = default;
            return false;
        }

        if (source[cursor] == '(')
        {
            return TryReadDynamicImportSpecifier(source, keywordStart, cursor, out specifier);
        }

        if (TryReadStringLiteral(source, cursor, out var valueStart, out var valueLength, out _))
        {
            specifier = new JavaScriptModuleSpecifier(
                JavaScriptModuleSpecifierKind.StaticImport,
                valueStart,
                valueLength,
                source.Substring(valueStart, valueLength),
                keywordStart,
                ExpressionLength: 0);
            return true;
        }

        return TryReadFromSpecifier(
            source,
            keywordStart,
            cursor,
            JavaScriptModuleSpecifierKind.StaticImport,
            out specifier);
    }

    private static bool TryReadReExportSpecifier(
        string source,
        int keywordStart,
        int keywordEnd,
        out JavaScriptModuleSpecifier specifier)
        => TryReadFromSpecifier(
            source,
            keywordStart,
            SkipWhitespaceAndComments(source, keywordEnd),
            JavaScriptModuleSpecifierKind.ReExport,
            out specifier);

    private static bool TryReadDynamicImportSpecifier(
        string source,
        int keywordStart,
        int openParenIndex,
        out JavaScriptModuleSpecifier specifier)
    {
        var cursor = SkipWhitespaceAndComments(source, openParenIndex + 1);
        if (!TryReadStringLiteral(source, cursor, out var valueStart, out var valueLength, out var literalEnd))
        {
            specifier = default;
            return false;
        }

        cursor = SkipWhitespaceAndComments(source, literalEnd + 1);
        if (cursor >= source.Length || source[cursor] != ')')
        {
            specifier = default;
            return false;
        }

        specifier = new JavaScriptModuleSpecifier(
            JavaScriptModuleSpecifierKind.DynamicImport,
            valueStart,
            valueLength,
            source.Substring(valueStart, valueLength),
            keywordStart,
            cursor - keywordStart + 1);
        return true;
    }

    private static bool TryReadFromSpecifier(
        string source,
        int keywordStart,
        int searchStart,
        JavaScriptModuleSpecifierKind kind,
        out JavaScriptModuleSpecifier specifier)
    {
        var depth = 0;
        for (var index = searchStart; index < source.Length; index++)
        {
            if (TrySkipLiteralOrComment(source, ref index))
            {
                continue;
            }

            if (depth == 0 && IsKeywordAt(source, index, "from"))
            {
                var cursor = SkipWhitespaceAndComments(source, index + "from".Length);
                if (TryReadStringLiteral(source, cursor, out var valueStart, out var valueLength, out _))
                {
                    specifier = new JavaScriptModuleSpecifier(
                        kind,
                        valueStart,
                        valueLength,
                        source.Substring(valueStart, valueLength),
                        keywordStart,
                        ExpressionLength: 0);
                    return true;
                }
            }

            switch (source[index])
            {
                case '{':
                case '[':
                case '(':
                    depth++;
                    break;
                case '}':
                case ']':
                case ')':
                    depth = Math.Max(0, depth - 1);
                    break;
                case ';' when depth == 0:
                    specifier = default;
                    return false;
            }
        }

        specifier = default;
        return false;
    }

    private static int SkipWhitespaceAndComments(string source, int index)
    {
        while (index < source.Length)
        {
            if (char.IsWhiteSpace(source[index]))
            {
                index++;
                continue;
            }

            var skippedEnd = index;
            if (TrySkipComment(source, ref skippedEnd))
            {
                index = skippedEnd + 1;
                continue;
            }

            break;
        }

        return index;
    }

    private static bool TryReadStringLiteral(
        string source,
        int index,
        out int valueStart,
        out int valueLength,
        out int literalEnd)
    {
        if (index >= source.Length || source[index] is not ('\'' or '"'))
        {
            valueStart = -1;
            valueLength = 0;
            literalEnd = -1;
            return false;
        }

        literalEnd = SkipQuotedLiteral(source, index, source[index]);
        valueStart = index + 1;
        valueLength = Math.Max(0, literalEnd - valueStart);
        return literalEnd < source.Length && source[literalEnd] == source[index];
    }

    private static bool TrySkipLiteralOrComment(string source, ref int index)
    {
        if (source[index] is '\'' or '"')
        {
            index = SkipQuotedLiteral(source, index, source[index]);
            return true;
        }

        if (source[index] == '`')
        {
            index = SkipTemplateLiteral(source, index);
            return true;
        }

        return TrySkipComment(source, ref index);
    }

    private static bool TrySkipComment(string source, ref int index)
    {
        if (source[index] != '/' || index + 1 >= source.Length)
        {
            return false;
        }

        if (source[index + 1] == '/')
        {
            index += 2;
            while (index < source.Length && source[index] is not ('\r' or '\n'))
            {
                index++;
            }

            index = Math.Max(0, index - 1);
            return true;
        }

        if (source[index + 1] != '*')
        {
            return false;
        }

        index += 2;
        while (index + 1 < source.Length)
        {
            if (source[index] == '*' && source[index + 1] == '/')
            {
                index++;
                return true;
            }

            index++;
        }

        index = source.Length - 1;
        return true;
    }

    private static int SkipQuotedLiteral(string source, int index, char delimiter)
    {
        var escaped = false;
        for (var cursor = index + 1; cursor < source.Length; cursor++)
        {
            var character = source[cursor];
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

            if (character == delimiter)
            {
                return cursor;
            }
        }

        return source.Length - 1;
    }

    private static int SkipTemplateLiteral(string source, int index)
    {
        var escaped = false;
        for (var cursor = index + 1; cursor < source.Length; cursor++)
        {
            var character = source[cursor];
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

            if (character == '`')
            {
                return cursor;
            }
        }

        return source.Length - 1;
    }

    private static bool IsKeywordAt(string source, int index, string keyword)
        => index >= 0
            && index + keyword.Length <= source.Length
            && source.AsSpan(index, keyword.Length).Equals(keyword.AsSpan(), StringComparison.Ordinal)
            && (index == 0 || !IsIdentifierPart(source[index - 1]))
            && (index + keyword.Length >= source.Length || !IsIdentifierPart(source[index + keyword.Length]));

    private static bool IsIdentifierStart(char character)
        => char.IsLetter(character) || character is '_' or '$';

    private static bool IsIdentifierPart(char character)
        => char.IsLetterOrDigit(character) || character is '_' or '$';
}
