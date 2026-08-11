namespace ECMAScript.WebIDL.Generator;

internal static class WebIdlNaming
{
    private static readonly HashSet<string> Keywords =
    [
        "event", "namespace", "default", "this", "params", "base", "lock", "async", "await", "static", "virtual",
        "class", "interface", "enum", "record", "struct", "using", "public", "private", "protected", "internal",
        "string", "int", "float", "double", "bool", "long", "short", "ushort", "uint", "ulong", "byte", "sbyte",
        "ref", "out", "in"
    ];

    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        if (input == "uuid")
        {
            return "UUID";
        }

        if (input == "uuids")
        {
            return "UUIDs";
        }

        if (input.All(static ch => char.IsUpper(ch) || ch == '_'))
        {
            return input;
        }

        var separated = input
            .Replace("-", " ")
            .Replace("_", " ");

        var builder = new List<string>();
        var token = new List<char>();

        for (var i = 0; i < separated.Length; i++)
        {
            var current = separated[i];
            if (!char.IsLetterOrDigit(current))
            {
                FlushToken(token, builder);
                continue;
            }

            if (token.Count > 0)
            {
                var previous = token[^1];
                var next = i + 1 < separated.Length ? separated[i + 1] : '\0';
                if ((char.IsLower(previous) && char.IsUpper(current))
                    || (char.IsUpper(previous) && char.IsUpper(current) && next != '\0' && char.IsLower(next)))
                {
                    FlushToken(token, builder);
                }
            }

            token.Add(current);
        }

        FlushToken(token, builder);

        var result = string.Concat(builder.Select(static part =>
            part.All(char.IsUpper)
                ? part
                : char.ToUpperInvariant(part[0]) + part[1..]));

        if (result.Length > 0 && char.IsDigit(result[0]))
        {
            result = "_" + result;
        }

        return result;
    }

    /// <summary>
    /// Projects a Web IDL declaration or reference into the public C# type name.
    /// This is deliberately separate from <see cref="ToPascalCase"/>: members keep
    /// their normal C# casing, while a type can avoid a collision with a commonly
    /// imported BCL type. Web IDL / JavaScript binding names continue to be emitted
    /// through <c>Description("@#...")</c> by the binding emitter.
    ///
    /// 将 Web IDL 的声明或引用投影为公开的 C# 类型名。这里必须与
    /// <see cref="ToPascalCase"/> 分离：成员仍使用普通 C# 大小写规则，只有类型
    /// 才需要规避常用 BCL 类型冲突；实际 Web IDL / JavaScript 名称仍由生成器的
    /// <c>Description("@#...")</c> 特性保留。
    /// </summary>
    public static string ToTypeName(string input)
    {
        var typeName = ToPascalCase(input);
        return typeName == "File" ? "Files" : typeName;
    }

    public static string ToCamelCase(string input)
    {
        var pascal = ToPascalCase(input);
        if (string.IsNullOrEmpty(pascal))
        {
            return string.Empty;
        }

        var camel = char.ToLowerInvariant(pascal[0]) + pascal[1..];
        return Keywords.Contains(camel) ? "@" + camel : camel;
    }

    public static string EscapeKeyword(string input)
    {
        return Keywords.Contains(input) ? "@" + input : input;
    }

    private static void FlushToken(List<char> token, List<string> builder)
    {
        if (token.Count == 0)
        {
            return;
        }

        builder.Add(new string([.. token]));
        token.Clear();
    }
}
