namespace Jazor.VueHost.Lsp.Routing;

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
        "@namespace",
        "@page",
        "@preservewhitespace",
        "@rendermode",
        "@typeparam",
        "@using",
        "@code"
    ];

    public DocumentRegionKind Classify(string text, int offset)
    {
        var clampedOffset = Math.Max(0, Math.Min(offset, text.Length));
        var templateRange = FindTagBlock(text, "<template", "</template>");
        if (InRange(templateRange, clampedOffset))
        {
            return DocumentRegionKind.Template;
        }

        var codeRange = FindCodeBlock(text);
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

    private static (int Start, int End) FindCodeBlock(string text)
    {
        var codeIndex = text.IndexOf("@code", StringComparison.OrdinalIgnoreCase);
        if (codeIndex < 0)
        {
            return (-1, -1);
        }

        var braceIndex = text.IndexOf('{', codeIndex);
        if (braceIndex < 0)
        {
            return (codeIndex, text.Length);
        }

        var depth = 1;
        for (var index = braceIndex + 1; index < text.Length; index++)
        {
            switch (text[index])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return (codeIndex, index + 1);
                    }

                    break;
            }
        }

        return (codeIndex, text.Length);
    }

    private static int GetMarkupBoundary(string text)
    {
        var offset = 0;
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        foreach (var line in normalized.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                offset += line.Length + 1;
                continue;
            }

            if (IsTopLevelDirective(line))
            {
                offset += line.Length + 1;
                continue;
            }

            return Math.Min(offset, text.Length);
        }

        return text.Length;
    }

    private static bool IsTopLevelDirective(string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith("@", StringComparison.Ordinal))
        {
            return false;
        }

        return TopLevelDirectives.Any(trimmed.StartsWith);
    }

    private static bool InRange((int Start, int End) range, int offset)
        => range.Start >= 0
            && range.End >= range.Start
            && offset >= range.Start
            && offset <= range.End;
}
