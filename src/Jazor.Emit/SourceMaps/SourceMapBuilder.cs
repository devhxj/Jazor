using Jazor.Common.SourceMaps;

namespace Jazor.Emit.SourceMaps;

internal sealed class SourceMapBuilder
{
    public SourceMapDocument BuildModuleMap(
        string generatedFileName,
        string moduleCode,
        IReadOnlyList<RazorVueEmitSourceOriginRecord> origins,
        Func<string, string?> readSourceContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedFileName);
        ArgumentNullException.ThrowIfNull(origins);
        ArgumentNullException.ThrowIfNull(readSourceContent);

        var relevantOrigins = origins
            .Where(origin => string.Equals(origin.GeneratedFilePath, generatedFileName, StringComparison.Ordinal))
            .ToArray();

        var sources = relevantOrigins
            .Where(static origin => !string.IsNullOrWhiteSpace(origin.SourceFilePath))
            .Select(static origin => origin.SourceFilePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new SourceMapSource(path, readSourceContent(path)))
            .ToArray();

        var sourceIndex = sources
            .Select((source, index) => (source.Path, index))
            .ToDictionary(static item => item.Path, static item => item.index, StringComparer.OrdinalIgnoreCase);

        var lineStarts = ComputeLineStarts(moduleCode ?? string.Empty);
        var contentLength = moduleCode?.Length ?? 0;
        var segments = new List<SourceMapSegment>();

        foreach (var origin in relevantOrigins.OrderBy(static origin => origin.GeneratedSpanStart ?? int.MaxValue))
        {
            if (origin.GeneratedSpanStart is null || origin.GeneratedSpanLength is null)
                continue;
            if (!sourceIndex.TryGetValue(origin.SourceFilePath, out var index))
                continue;
            if (origin.GeneratedSpanLength.Value <= 0)
                continue;

            var generatedStart = origin.GeneratedSpanStart.Value;
            if (generatedStart < 0 || generatedStart >= contentLength)
                continue;

            var generatedEndExclusive = generatedStart + origin.GeneratedSpanLength.Value;
            var generatedEnd = Math.Min(Math.Max(generatedEndExclusive - 1, generatedStart), Math.Max(contentLength - 1, 0));
            var startLine = FindLineIndex(lineStarts, generatedStart);
            var endLine = FindLineIndex(lineStarts, generatedEnd);

            for (var line = startLine; line <= endLine; line++)
            {
                var generatedColumn = line == startLine
                    ? Math.Max(generatedStart - lineStarts[line], 0)
                    : 0;
                var sourceColumn = line == startLine
                    ? Math.Max(origin.StartColumn - 1, 0)
                    : 0;

                segments.Add(new SourceMapSegment(
                    GeneratedLine: line,
                    GeneratedColumn: generatedColumn,
                    SourceIndex: index,
                    SourceLine: Math.Max(origin.StartLine - 1 + (line - startLine), 0),
                    SourceColumn: sourceColumn));
            }
        }

        var orderedSegments = segments
            .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn, segment.SourceIndex, segment.SourceLine, segment.SourceColumn))
            .Select(static group => group.First())
            .OrderBy(static segment => segment.GeneratedLine)
            .ThenBy(static segment => segment.GeneratedColumn)
            .ToArray();

        return new SourceMapDocument(generatedFileName, sources, orderedSegments);
    }

    private static int[] ComputeLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                starts.Add(i + 1);
        }

        return starts.ToArray();
    }

    private static int FindLineIndex(int[] lineStarts, int position)
    {
        var index = Array.BinarySearch(lineStarts, position);
        return index >= 0 ? index : Math.Max(~index - 1, 0);
    }
}
