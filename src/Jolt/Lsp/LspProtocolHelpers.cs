namespace Jolt.Lsp;

internal static class LspProtocolHelpers
{
    public static string ToDocumentUri(string documentPath)
    {
        if (Path.IsPathRooted(documentPath))
        {
            return new Uri(Path.GetFullPath(documentPath)).AbsoluteUri;
        }

        return new Uri(Path.GetFullPath(documentPath)).AbsoluteUri;
    }

    public static string ToDocumentPath(string documentUri)
    {
        if (Uri.TryCreate(documentUri, UriKind.Absolute, out var uri)
            && uri.IsFile)
        {
            return Path.GetFullPath(uri.LocalPath).Replace('\\', '/');
        }

        return documentUri.Replace('\\', '/');
    }

    public static int GetOffset(string text, LspPosition position)
    {
        var line = 0;
        var column = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (line == position.Line && column == position.Character)
            {
                return index;
            }

            if (text[index] == '\n')
            {
                line++;
                column = 0;
            }
            else
            {
                column++;
            }
        }

        return text.Length;
    }

    public static LspPosition GetPosition(string text, int offset)
    {
        var clampedOffset = Math.Max(0, Math.Min(offset, text.Length));
        var line = 0;
        var column = 0;
        for (var index = 0; index < clampedOffset; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                column = 0;
            }
            else
            {
                column++;
            }
        }

        return new LspPosition
        {
            Line = line,
            Character = column
        };
    }

    public static LspRange ToRange(string text, int start, int length)
    {
        return new LspRange
        {
            Start = GetPosition(text, start),
            End = GetPosition(text, start + Math.Max(length, 0))
        };
    }
}
