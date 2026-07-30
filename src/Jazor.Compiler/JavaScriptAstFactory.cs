using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// 创建编译器使用的 JavaScript AST 基础节点。
/// </summary>
/// <remarks>
/// 这里直接构造 Acornima 节点，不通过字符串拼接或再次解析 JavaScript。
/// 字符串转义必须集中处理，否则模块路径、属性名和用户文本可能生成非法或含义改变的脚本。
/// </remarks>
public static class JavaScriptAstFactory
{
    // ECMAScript modules are strict code, so eval/arguments join keywords and
    // future-reserved words in the set that cannot become local bindings.
    private static readonly HashSet<string> ReservedBindingIdentifiers = new(StringComparer.Ordinal)
    {
        "arguments",
        "await",
        "break",
        "case",
        "catch",
        "class",
        "const",
        "continue",
        "debugger",
        "default",
        "delete",
        "do",
        "else",
        "enum",
        "eval",
        "export",
        "extends",
        "false",
        "finally",
        "for",
        "function",
        "if",
        "implements",
        "import",
        "in",
        "instanceof",
        "interface",
        "let",
        "new",
        "null",
        "package",
        "private",
        "protected",
        "public",
        "return",
        "static",
        "super",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "var",
        "void",
        "while",
        "with",
        "yield"
    };

    internal static bool IsJavaScriptIdentifierName(string? value)
    {
        if (string.IsNullOrEmpty(value) ||
            !IsJavaScriptIdentifierStart(value!, 0, out var width))
        {
            return false;
        }

        var index = width;
        while (index < value!.Length)
        {
            if (!IsJavaScriptIdentifierPart(value, index, out width))
                return false;

            index += width;
        }

        return true;
    }

    internal static bool IsJavaScriptBindingIdentifier(string? value)
        => IsJavaScriptIdentifierName(value) && !ReservedBindingIdentifiers.Contains(value!);

    internal static Expression CreateModuleExportName(string value)
        => IsJavaScriptIdentifierName(value)
            ? new Identifier(value)
            : CreateStringLiteral(value);

    public static StringLiteral CreateStringLiteral(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var raw = new StringBuilder(value.Length + 2);
        raw.Append('"');

        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            switch (current)
            {
                case '"':
                    raw.Append("\\\"");
                    break;
                case '\\':
                    raw.Append("\\\\");
                    break;
                case '\0':
                    raw.Append(index + 1 < value.Length && IsDecimalDigit(value[index + 1])
                        ? "\\x00"
                        : "\\0");
                    break;
                case '\b':
                    raw.Append("\\b");
                    break;
                case '\f':
                    raw.Append("\\f");
                    break;
                case '\n':
                    raw.Append("\\n");
                    break;
                case '\r':
                    raw.Append("\\r");
                    break;
                case '\t':
                    raw.Append("\\t");
                    break;
                case '\v':
                    raw.Append("\\v");
                    break;
                case '\u2028':
                case '\u2029':
                    AppendUnicodeEscape(raw, current);
                    break;
                default:
                    if (current < ' ' || IsUnpairedSurrogate(value, index))
                        AppendUnicodeEscape(raw, current);
                    else
                        raw.Append(current);
                    break;
            }
        }

        raw.Append('"');
        return new StringLiteral(value, raw.ToString());
    }

    internal static Expression CreateNumericExpression(double value, string raw)
    {
        if (double.IsNaN(value))
            return new Identifier("NaN");
        if (double.IsPositiveInfinity(value))
            return new Identifier("Infinity");
        if (double.IsNegativeInfinity(value))
            return new NonUpdateUnaryExpression(Operator.UnaryNegation, new Identifier("Infinity"));

        if (raw.Length > 1 && raw[0] == '-')
        {
            return new NonUpdateUnaryExpression(
                Operator.UnaryNegation,
                new NumericLiteral(-value, raw.Substring(1)));
        }

        return new NumericLiteral(value, raw);
    }

    internal static Expression CreateBigIntExpression(BigInteger value, string raw)
    {
        if (value.Sign < 0)
        {
            return new NonUpdateUnaryExpression(
                Operator.UnaryNegation,
                new BigIntLiteral(BigInteger.Negate(value), raw.Substring(1)));
        }

        return new BigIntLiteral(value, raw);
    }

    private static bool IsDecimalDigit(char value)
        => value >= '0' && value <= '9';

    private static bool IsJavaScriptIdentifierStart(string value, int index, out int width)
    {
        if (!TryReadCodePoint(value, index, out var codePoint, out width))
            return false;

        if (codePoint is '$' or '_' || IsOtherIdentifierStart(codePoint))
            return true;

        return CharUnicodeInfo.GetUnicodeCategory(value, index) is
            UnicodeCategory.UppercaseLetter or
            UnicodeCategory.LowercaseLetter or
            UnicodeCategory.TitlecaseLetter or
            UnicodeCategory.ModifierLetter or
            UnicodeCategory.OtherLetter or
            UnicodeCategory.LetterNumber;
    }

    private static bool IsJavaScriptIdentifierPart(string value, int index, out int width)
    {
        if (!TryReadCodePoint(value, index, out var codePoint, out width))
            return false;

        if (codePoint is '$' or '_' or 0x200C or 0x200D ||
            IsOtherIdentifierStart(codePoint) ||
            IsOtherIdentifierPart(codePoint))
        {
            return true;
        }

        return CharUnicodeInfo.GetUnicodeCategory(value, index) is
            UnicodeCategory.UppercaseLetter or
            UnicodeCategory.LowercaseLetter or
            UnicodeCategory.TitlecaseLetter or
            UnicodeCategory.ModifierLetter or
            UnicodeCategory.OtherLetter or
            UnicodeCategory.LetterNumber or
            UnicodeCategory.NonSpacingMark or
            UnicodeCategory.SpacingCombiningMark or
            UnicodeCategory.DecimalDigitNumber or
            UnicodeCategory.ConnectorPunctuation;
    }

    private static bool TryReadCodePoint(
        string value,
        int index,
        out int codePoint,
        out int width)
    {
        var current = value[index];
        if (char.IsHighSurrogate(current))
        {
            if (index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
            {
                codePoint = char.ConvertToUtf32(current, value[index + 1]);
                width = 2;
                return true;
            }

            codePoint = current;
            width = 1;
            return false;
        }

        codePoint = current;
        width = 1;
        return !char.IsLowSurrogate(current);
    }

    // Unicode Other_ID_Start/Continue supplement the general categories used by ECMAScript.
    private static bool IsOtherIdentifierStart(int codePoint)
        => codePoint is 0x1885 or 0x1886 or 0x2118 or 0x212E or 0x309B or 0x309C;

    private static bool IsOtherIdentifierPart(int codePoint)
        => codePoint is 0x00B7 or 0x0387 or 0x1369 or 0x19DA or 0x30FB;

    private static bool IsUnpairedSurrogate(string value, int index)
    {
        var current = value[index];
        if (char.IsHighSurrogate(current))
            return index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]);
        if (char.IsLowSurrogate(current))
            return index == 0 || !char.IsHighSurrogate(value[index - 1]);
        return false;
    }

    private static void AppendUnicodeEscape(StringBuilder raw, char value)
    {
        raw.Append("\\u");
        raw.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
    }
}
