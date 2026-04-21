using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using Jolt.Extensions;
using Jazor.VueContracts.Protocol;
using Jolt.Jazor.Projection;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Lsp;

internal sealed partial class LspSession
{
    private static LspRange GetWordRangeAtPosition(string text, int offset)
    {
        var (start, length) = GetWordBounds(text, offset);
        return LspProtocolHelpers.ToRange(text, start, length);
    }

    private static string ExtractWord(string text, int offset)
    {
        var (start, length) = GetWordBounds(text, offset);
        return text.Substring(start, length);
    }

    private static (int start, int length) GetWordBounds(string text, int offset)
    {
        if (offset < 0 || offset >= text.Length)
        {
            return (Math.Max(0, offset), 0);
        }

        var start = offset;
        while (start > 0 && IsWordCharacter(text[start - 1]))
        {
            start--;
        }

        var end = offset;
        while (end < text.Length && IsWordCharacter(text[end]))
        {
            end++;
        }

        return (start, end - start);
    }

    private static LspSelectionRange CreateSelectionRange(string text, LspPosition position)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        var wordRange = GetWordRangeAtPosition(text, offset);
        var lineRange = GetLineRangeAtOffset(text, offset);
        var documentRange = LspProtocolHelpers.ToRange(text, 0, text.Length);

        var lineSelection = new LspSelectionRange
        {
            Range = lineRange,
            Parent = new LspSelectionRange
            {
                Range = documentRange
            }
        };

        return new LspSelectionRange
        {
            Range = wordRange,
            Parent = lineSelection
        };
    }

    private static LspRange GetLineRangeAtOffset(string text, int offset)
    {
        var boundedOffset = Math.Clamp(offset, 0, text.Length);
        var lineStart = boundedOffset;
        while (lineStart > 0 && text[lineStart - 1] != '\n')
        {
            lineStart--;
        }

        var lineEnd = boundedOffset;
        while (lineEnd < text.Length && text[lineEnd] != '\n')
        {
            lineEnd++;
        }

        return LspProtocolHelpers.ToRange(text, lineStart, Math.Max(0, lineEnd - lineStart));
    }

    private static IReadOnlyList<LspRange> CollectLinkedEditingRanges(string text, LspPosition position)
    {
        if (!TryFindTagNameAtPosition(text, position, out var tagName))
        {
            return Array.Empty<LspRange>();
        }

        var ranges = new List<LspRange>();
        foreach (var tag in EnumerateTagNameSpans(text))
        {
            if (!string.Equals(tag.Name, tagName, StringComparison.Ordinal))
            {
                continue;
            }

            ranges.Add(LspProtocolHelpers.ToRange(text, tag.Index, tag.Length));
        }

        return ranges
            .GroupBy(
                static range => $"{range.Start.Line}:{range.Start.Character}:{range.End.Line}:{range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private static bool TryFindTagNameAtPosition(
        string text,
        LspPosition position,
        out string tagName)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (var tag in EnumerateTagNameSpans(text))
        {
            if (offset < tag.Index || offset > tag.Index + tag.Length)
            {
                continue;
            }

            tagName = tag.Name;
            return true;
        }

        tagName = string.Empty;
        return false;
    }

    private static IEnumerable<(int Index, int Length, string Name)> EnumerateTagNameSpans(string text)
    {
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] != '<')
            {
                continue;
            }

            var nameStart = index + 1;
            if (nameStart < text.Length && text[nameStart] == '/')
            {
                nameStart++;
            }

            if (nameStart >= text.Length || !char.IsLetter(text[nameStart]))
            {
                continue;
            }

            var nameEnd = nameStart + 1;
            while (nameEnd < text.Length && IsTagNameCharacter(text[nameEnd]))
            {
                nameEnd++;
            }

            yield return (nameStart, nameEnd - nameStart, text[nameStart..nameEnd]);
            index = nameEnd - 1;
        }
    }

    private static bool IsTagNameCharacter(char character)
        => char.IsLetterOrDigit(character) || character is '_' or '-' or ':';

    private static string FormatText(
        string text,
        LspFormattingOptions? options,
        bool ensureFinalNewline)
    {
        var trimTrailingWhitespace = options?.TrimTrailingWhitespace ?? true;
        var fallbackNewline = GetDominantNewline(text);
        var formatted = string.Concat(EnumerateLineParts(text)
            .Select(part => (trimTrailingWhitespace ? part.Text.TrimEnd(' ', '\t') : part.Text) + part.Newline));
        var shouldInsertFinalNewline = options?.InsertFinalNewline ?? ensureFinalNewline;
        if (shouldInsertFinalNewline
            && !EndsWithNewline(formatted))
        {
            formatted += fallbackNewline;
        }

        return formatted;
    }

    private static IEnumerable<(string Text, string Newline)> EnumerateLineParts(string text)
    {
        var start = 0;
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (character != '\r' && character != '\n')
            {
                continue;
            }

            var newlineLength = character == '\r'
                && index + 1 < text.Length
                && text[index + 1] == '\n'
                    ? 2
                    : 1;
            yield return (
                text[start..index],
                text.Substring(index, newlineLength));
            index += newlineLength - 1;
            start = index + 1;
        }

        if (start <= text.Length)
        {
            yield return (text[start..], string.Empty);
        }
    }

    private static bool EndsWithNewline(string text)
        => text.EndsWith("\n", StringComparison.Ordinal)
            || text.EndsWith("\r", StringComparison.Ordinal);

    private static string GetDominantNewline(string text)
    {
        var crlfCount = 0;
        var lfCount = 0;
        var crCount = 0;
        foreach (var part in EnumerateLineParts(text))
        {
            if (part.Newline == "\r\n")
            {
                crlfCount++;
            }
            else if (part.Newline == "\n")
            {
                lfCount++;
            }
            else if (part.Newline == "\r")
            {
                crCount++;
            }
        }

        if (crlfCount >= lfCount && crlfCount >= crCount && crlfCount > 0)
        {
            return "\r\n";
        }

        return crCount > lfCount
            ? "\r"
            : "\n";
    }

    private static bool IsWordCharacter(char c)
        => char.IsLetterOrDigit(c) || c == '_';

    private static int GetRenameProbeOffset(string text, int offset)
    {
        var boundedOffset = Math.Clamp(offset, 0, text.Length);
        if (text.Length == 0)
        {
            return 0;
        }

        if (boundedOffset == text.Length || !IsRenameCharacter(text[boundedOffset]))
        {
            if (boundedOffset == 0 || !IsRenameCharacter(text[boundedOffset - 1]))
            {
                return boundedOffset;
            }

            return boundedOffset - 1;
        }

        return boundedOffset;
    }

    private static (int start, int length) GetRenameTokenBounds(string text, int offset)
    {
        if (text.Length == 0)
        {
            return (0, 0);
        }

        var probeOffset = GetRenameProbeOffset(text, offset);
        if (probeOffset < 0
            || probeOffset >= text.Length
            || !IsRenameCharacter(text[probeOffset]))
        {
            return (Math.Clamp(offset, 0, text.Length), 0);
        }

        var start = probeOffset;
        while (start > 0 && IsRenameCharacter(text[start - 1]))
        {
            start--;
        }

        var end = probeOffset + 1;
        while (end < text.Length && IsRenameCharacter(text[end]))
        {
            end++;
        }

        return (start, end - start);
    }

    private static bool IsRenameCharacter(char c)
        => char.IsLetterOrDigit(c) || c is '_' or '-' or ':';
}
