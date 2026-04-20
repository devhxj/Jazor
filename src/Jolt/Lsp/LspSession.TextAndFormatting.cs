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
        foreach (Match match in TagNamePattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success
                || !string.Equals(nameGroup.Value, tagName, StringComparison.Ordinal))
            {
                continue;
            }

            ranges.Add(LspProtocolHelpers.ToRange(text, nameGroup.Index, nameGroup.Length));
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
        foreach (Match match in TagNamePattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success)
            {
                continue;
            }

            if (offset < nameGroup.Index || offset > nameGroup.Index + nameGroup.Length)
            {
                continue;
            }

            tagName = nameGroup.Value;
            return true;
        }

        tagName = string.Empty;
        return false;
    }

    private static string FormatText(
        string text,
        LspFormattingOptions? options,
        bool ensureFinalNewline)
    {
        var newline = text.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(static line => line.TrimEnd(' ', '\t'))
            .ToArray();
        var formatted = string.Join(newline, lines);
        var shouldInsertFinalNewline = options?.InsertFinalNewline ?? ensureFinalNewline;
        if (shouldInsertFinalNewline
            && !formatted.EndsWith(newline, StringComparison.Ordinal))
        {
            formatted += newline;
        }

        return formatted;
    }

    private static bool IsWordCharacter(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}
