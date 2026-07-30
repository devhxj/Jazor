using System;
using System.Globalization;
using System.Text;
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

    private static bool IsDecimalDigit(char value)
        => value >= '0' && value <= '9';

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
