namespace Jolt.Lsp;

internal static class LspSemanticTokenLegend
{
    public static readonly string[] TokenTypes =
    [
        "class",
        "method",
        "property",
        "parameter",
        "variable",
        "keyword",
        "string",
        "number",
        "decorator",
        "type",
        "function",
        "enum",
        "interface",
        "namespace"
    ];

    public static readonly string[] TokenModifiers =
    [
        "declaration",
        "static",
        "readonly",
        "abstract",
        "async",
        "modification"
    ];

    public static LspSemanticTokensLegendDescriptor CreateDescriptor()
        => new()
        {
            TokenTypes = TokenTypes,
            TokenModifiers = TokenModifiers
        };

    public static LspSemanticTokensResult Encode(IReadOnlyList<LspSemanticToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var data = new List<int>(tokens.Count * 5);
        var previousLine = 0;
        var previousCharacter = 0;
        var isFirstToken = true;
        foreach (var token in tokens)
        {
            var deltaLine = isFirstToken ? token.Line : token.Line - previousLine;
            var deltaCharacter = isFirstToken || deltaLine != 0
                ? token.Character
                : token.Character - previousCharacter;
            data.Add(deltaLine);
            data.Add(deltaCharacter);
            data.Add(token.Length);
            data.Add(GetTokenTypeIndex(token.TokenType));
            data.Add(GetTokenModifierBitset(token.TokenModifiers));

            previousLine = token.Line;
            previousCharacter = token.Character;
            isFirstToken = false;
        }

        return new LspSemanticTokensResult
        {
            Data = data.ToArray()
        };
    }

    public static IReadOnlyList<LspSemanticToken> Decode(IReadOnlyList<int> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count % 5 != 0)
        {
            throw new ArgumentException("Semantic token data must be encoded in groups of five integers.", nameof(data));
        }

        var tokens = new List<LspSemanticToken>(data.Count / 5);
        var line = 0;
        var character = 0;
        for (var index = 0; index < data.Count; index += 5)
        {
            var deltaLine = data[index];
            var deltaCharacter = data[index + 1];
            var length = data[index + 2];
            var tokenTypeIndex = data[index + 3];
            var modifierBitset = data[index + 4];

            line += deltaLine;
            character = deltaLine == 0 ? character + deltaCharacter : deltaCharacter;
            tokens.Add(new LspSemanticToken
            {
                Line = line,
                Character = character,
                Length = length,
                TokenType = TokenTypes[tokenTypeIndex],
                TokenModifiers = DecodeModifiers(modifierBitset)
            });
        }

        return tokens;
    }

    public static int GetTokenTypeIndex(string tokenType)
        => Array.IndexOf(TokenTypes, tokenType) switch
        {
            >= 0 and var index => index,
            _ => throw new InvalidOperationException($"Unsupported semantic token type '{tokenType}'.")
        };

    public static int GetTokenModifierBitset(IReadOnlyList<string>? tokenModifiers)
    {
        if (tokenModifiers is null || tokenModifiers.Count == 0)
        {
            return 0;
        }

        var bitset = 0;
        foreach (var modifier in tokenModifiers)
        {
            var index = Array.IndexOf(TokenModifiers, modifier);
            if (index < 0)
            {
                throw new InvalidOperationException($"Unsupported semantic token modifier '{modifier}'.");
            }

            bitset |= 1 << index;
        }

        return bitset;
    }

    private static string[] DecodeModifiers(int modifierBitset)
    {
        if (modifierBitset == 0)
        {
            return [];
        }

        var modifiers = new List<string>();
        for (var index = 0; index < TokenModifiers.Length; index++)
        {
            if ((modifierBitset & (1 << index)) != 0)
            {
                modifiers.Add(TokenModifiers[index]);
            }
        }

        return modifiers.ToArray();
    }
}
