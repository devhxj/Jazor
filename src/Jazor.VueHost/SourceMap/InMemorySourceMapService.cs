using System.Text.Json;

namespace Jazor.VueHost.SourceMap;

internal sealed class InMemorySourceMapService : ISourceMapService
{
    private readonly Dictionary<string, RegisteredSourceMap> _maps = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    public void Register(string generatedPath, string sourceMapJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceMapJson);

        var parsed = Parse(generatedPath, sourceMapJson);
        lock (_gate)
        {
            _maps[NormalizePath(generatedPath)] = parsed;
        }
    }

    public void Unregister(string generatedPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);

        lock (_gate)
        {
            _maps.Remove(NormalizePath(generatedPath));
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _maps.Clear();
        }
    }

    public string? GetSourceMapJson(string generatedPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);

        lock (_gate)
        {
            return _maps.TryGetValue(NormalizePath(generatedPath), out var sourceMap)
                ? sourceMap.RawJson
                : null;
        }
    }

    public OriginalPosition? OriginalPositionFor(string generatedPath, int line, int column)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);

        RegisteredSourceMap sourceMap;
        lock (_gate)
        {
            if (!_maps.TryGetValue(NormalizePath(generatedPath), out sourceMap))
            {
                return null;
            }
        }

        var segment = FindLastSegmentAtOrBefore(sourceMap.Segments, line, column);
        if (!segment.HasValue || segment.Value.SourceIndex < 0 || segment.Value.SourceIndex >= sourceMap.Sources.Count)
        {
            return null;
        }

        var source = sourceMap.Sources[segment.Value.SourceIndex];
        return new OriginalPosition(source.Path, segment.Value.SourceLine, segment.Value.SourceColumn, segment.Value.SourceIndex);
    }

    public GeneratedPosition? GeneratedPositionFor(string sourcePath, int line, int column)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        RegisteredSourceMap[] sourceMaps;
        lock (_gate)
        {
            sourceMaps = _maps.Values.ToArray();
        }

        Candidate? bestCandidate = null;
        foreach (var sourceMap in sourceMaps)
        {
            foreach (var segment in sourceMap.Segments)
            {
                if (segment.SourceIndex < 0 || segment.SourceIndex >= sourceMap.Sources.Count)
                {
                    continue;
                }

                var source = sourceMap.Sources[segment.SourceIndex];
                if (!PathMatches(source.Path, sourcePath))
                {
                    continue;
                }

                var candidate = new Candidate(
                    sourceMap.GeneratedPath,
                    segment.GeneratedLine,
                    segment.GeneratedColumn,
                    segment.SourceLine - line,
                    segment.SourceColumn - column);
                if (bestCandidate is null || candidate.CompareTo(bestCandidate.Value) < 0)
                {
                    bestCandidate = candidate;
                }
            }
        }

        return bestCandidate is null
            ? null
            : new GeneratedPosition(bestCandidate.Value.GeneratedPath, bestCandidate.Value.GeneratedLine, bestCandidate.Value.GeneratedColumn);
    }

    public string? GetSourceContent(string generatedPath, int sourceIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedPath);

        RegisteredSourceMap sourceMap;
        lock (_gate)
        {
            if (!_maps.TryGetValue(NormalizePath(generatedPath), out sourceMap))
            {
                return null;
            }
        }

        if (sourceIndex < 0 || sourceIndex >= sourceMap.Sources.Count)
        {
            return null;
        }

        return sourceMap.Sources[sourceIndex].Content;
    }

    private static RegisteredSourceMap Parse(string generatedPath, string sourceMapJson)
    {
        using var document = JsonDocument.Parse(sourceMapJson);
        var root = document.RootElement;
        var sourcesArray = root.TryGetProperty("sources", out var sourcesElement) && sourcesElement.ValueKind == JsonValueKind.Array
            ? sourcesElement
            : default;
        var sourcesContentArray = root.TryGetProperty("sourcesContent", out var contentElement) && contentElement.ValueKind == JsonValueKind.Array
            ? contentElement
            : default;
        var sources = new List<RegisteredSource>(sourcesArray.ValueKind == JsonValueKind.Array ? sourcesArray.GetArrayLength() : 0);
        if (sourcesArray.ValueKind == JsonValueKind.Array)
        {
            for (var index = 0; index < sourcesArray.GetArrayLength(); index++)
            {
                var sourcePath = sourcesArray[index].GetString() ?? string.Empty;
                string? content = null;
                if (sourcesContentArray.ValueKind == JsonValueKind.Array && index < sourcesContentArray.GetArrayLength())
                {
                    content = sourcesContentArray[index].ValueKind == JsonValueKind.Null
                        ? null
                        : sourcesContentArray[index].GetString();
                }

                sources.Add(new RegisteredSource(sourcePath, content));
            }
        }

        var mappings = root.TryGetProperty("mappings", out var mappingsElement)
            ? mappingsElement.GetString() ?? string.Empty
            : string.Empty;
        return new RegisteredSourceMap(
            NormalizePath(generatedPath),
            sourceMapJson,
            sources.ToArray(),
            DecodeMappings(mappings));
    }

    private static IReadOnlyList<RegisteredSegment> DecodeMappings(string mappings)
    {
        var segments = new List<RegisteredSegment>();
        var generatedLine = 0;
        var previousGeneratedColumn = 0;
        var previousSourceIndex = 0;
        var previousSourceLine = 0;
        var previousSourceColumn = 0;
        var position = 0;

        while (position < mappings.Length)
        {
            var current = mappings[position];
            if (current == ';')
            {
                generatedLine++;
                previousGeneratedColumn = 0;
                position++;
                continue;
            }

            if (current == ',')
            {
                position++;
                continue;
            }

            var generatedColumn = previousGeneratedColumn + DecodeVlq(mappings, ref position);
            previousGeneratedColumn = generatedColumn;

            if (position >= mappings.Length || mappings[position] == ',' || mappings[position] == ';')
            {
                continue;
            }

            var sourceIndex = previousSourceIndex + DecodeVlq(mappings, ref position);
            var sourceLine = previousSourceLine + DecodeVlq(mappings, ref position);
            var sourceColumn = previousSourceColumn + DecodeVlq(mappings, ref position);
            previousSourceIndex = sourceIndex;
            previousSourceLine = sourceLine;
            previousSourceColumn = sourceColumn;

            if (position < mappings.Length && mappings[position] != ',' && mappings[position] != ';')
            {
                _ = DecodeVlq(mappings, ref position);
            }

            segments.Add(new RegisteredSegment(generatedLine, generatedColumn, sourceIndex, sourceLine, sourceColumn));
        }

        return segments;
    }

    private static int DecodeVlq(string mappings, ref int position)
    {
        var result = 0;
        var shift = 0;
        var continuation = true;
        while (continuation)
        {
            if (position >= mappings.Length)
            {
                throw new InvalidOperationException("Unexpected end of VLQ mapping.");
            }

            var digit = DecodeBase64(mappings[position++]);
            continuation = (digit & 32) != 0;
            digit &= 31;
            result += digit << shift;
            shift += 5;
        }

        var isNegative = (result & 1) == 1;
        result >>= 1;
        return isNegative ? -result : result;
    }

    private static int DecodeBase64(char value)
        => value switch
        {
            >= 'A' and <= 'Z' => value - 'A',
            >= 'a' and <= 'z' => value - 'a' + 26,
            >= '0' and <= '9' => value - '0' + 52,
            '+' => 62,
            '/' => 63,
            _ => throw new InvalidOperationException($"Invalid base64 VLQ digit '{value}'.")
        };

    private static RegisteredSegment? FindLastSegmentAtOrBefore(IReadOnlyList<RegisteredSegment> segments, int generatedLine, int generatedColumn)
    {
        RegisteredSegment? candidate = null;
        foreach (var segment in segments)
        {
            if (segment.GeneratedLine > generatedLine)
            {
                break;
            }

            if (segment.GeneratedLine == generatedLine && segment.GeneratedColumn > generatedColumn)
            {
                break;
            }

            candidate = segment;
        }

        return candidate;
    }

    private static bool PathMatches(string left, string right)
    {
        var normalizedLeft = NormalizePath(left);
        var normalizedRight = NormalizePath(right);
        if (string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(normalizedLeft) || string.IsNullOrWhiteSpace(normalizedRight))
        {
            return false;
        }

        return normalizedLeft.EndsWith("/" + normalizedRight, StringComparison.OrdinalIgnoreCase)
            || normalizedRight.EndsWith("/" + normalizedLeft, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var normalized = path.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            normalized = uri.LocalPath;
        }

        normalized = normalized.Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized;
    }

    private readonly record struct RegisteredSource(string Path, string? Content);

    private readonly record struct RegisteredSegment(
        int GeneratedLine,
        int GeneratedColumn,
        int SourceIndex,
        int SourceLine,
        int SourceColumn);

    private readonly record struct RegisteredSourceMap(
        string GeneratedPath,
        string RawJson,
        IReadOnlyList<RegisteredSource> Sources,
        IReadOnlyList<RegisteredSegment> Segments);

    private readonly record struct Candidate(
        string GeneratedPath,
        int GeneratedLine,
        int GeneratedColumn,
        int LineDelta,
        int ColumnDelta) : IComparable<Candidate>
    {
        public int CompareTo(Candidate other)
        {
            var exactLineScore = LineDelta == 0 ? 0 : 1;
            var otherExactLineScore = other.LineDelta == 0 ? 0 : 1;
            var comparison = exactLineScore.CompareTo(otherExactLineScore);
            if (comparison != 0)
            {
                return comparison;
            }

            var forwardLinePenalty = LineDelta >= 0 ? LineDelta : int.MaxValue / 2 + Math.Abs(LineDelta);
            var otherForwardLinePenalty = other.LineDelta >= 0 ? other.LineDelta : int.MaxValue / 2 + Math.Abs(other.LineDelta);
            comparison = forwardLinePenalty.CompareTo(otherForwardLinePenalty);
            if (comparison != 0)
            {
                return comparison;
            }

            var forwardColumnPenalty = ColumnDelta >= 0 ? ColumnDelta : int.MaxValue / 2 + Math.Abs(ColumnDelta);
            var otherForwardColumnPenalty = other.ColumnDelta >= 0 ? other.ColumnDelta : int.MaxValue / 2 + Math.Abs(other.ColumnDelta);
            comparison = forwardColumnPenalty.CompareTo(otherForwardColumnPenalty);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = GeneratedLine.CompareTo(other.GeneratedLine);
            if (comparison != 0)
            {
                return comparison;
            }

            return GeneratedColumn.CompareTo(other.GeneratedColumn);
        }
    }
}
