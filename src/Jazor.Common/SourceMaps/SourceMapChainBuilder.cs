using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ECMAScript.Contract.SourceMaps;

public sealed class SourceMapChainBuilder
{
    public SourceMapDocument Chain(SourceMapDocument bundleMap, IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath)
    {
        if (bundleMap is null)
            throw new ArgumentNullException(nameof(bundleMap));
        if (moduleMapsByPath is null)
            throw new ArgumentNullException(nameof(moduleMapsByPath));

        var moduleMaps = NormalizeLookup(moduleMapsByPath);
        var sources = new List<SourceMapSource>();
        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var segments = new List<SourceMapSegment>();

        foreach (var bundleSegment in bundleMap.Segments)
        {
            if (bundleSegment.SourceIndex < 0 || bundleSegment.SourceIndex >= bundleMap.Sources.Count)
                continue;

            var bundleSource = bundleMap.Sources[bundleSegment.SourceIndex];
            var resolved = ResolveSource(
                bundleSource.Path,
                bundleSource.Content,
                bundleSegment.SourceLine,
                bundleSegment.SourceColumn,
                moduleMaps,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, resolved.Path, resolved.Content);

            segments.Add(new SourceMapSegment(
                bundleSegment.GeneratedLine,
                bundleSegment.GeneratedColumn,
                sourceIndex,
                resolved.SourceLine,
                resolved.SourceColumn));
        }

        var orderedSegments = segments
            .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn, segment.SourceIndex, segment.SourceLine, segment.SourceColumn))
            .Select(static group => group.First())
            .OrderBy(static segment => segment.GeneratedLine)
            .ThenBy(static segment => segment.GeneratedColumn)
            .ToArray();

        return new SourceMapDocument(bundleMap.File, sources, orderedSegments);
    }

    public SourceMapDocument Chain(string bundleMapJson, IReadOnlyDictionary<string, string> moduleMapJsonByPath)
    {
        if (string.IsNullOrWhiteSpace(bundleMapJson))
            throw new ArgumentException("Bundle source map json is required.", nameof(bundleMapJson));
        if (moduleMapJsonByPath is null)
            throw new ArgumentNullException(nameof(moduleMapJsonByPath));

        var bundleMap = Parse(bundleMapJson);
        var moduleMaps = moduleMapJsonByPath.ToDictionary(
            static pair => pair.Key,
            static pair => Parse(pair.Value),
            StringComparer.OrdinalIgnoreCase);

        return Chain(bundleMap, moduleMaps);
    }

    private static IReadOnlyDictionary<string, SourceMapDocument> NormalizeLookup(IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath)
    {
        var lookup = new Dictionary<string, SourceMapDocument>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in moduleMapsByPath)
        {
            lookup[NormalizePath(pair.Key)] = pair.Value;
            if (!string.IsNullOrWhiteSpace(pair.Value.File))
                lookup[NormalizePath(pair.Value.File)] = pair.Value;
        }

        return lookup;
    }

    private static ResolvedSource ResolveSource(
        string path,
        string? content,
        int sourceLine,
        int sourceColumn,
        IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath,
        HashSet<string> visited)
    {
        var normalizedPath = NormalizePath(path);
        if (!TryGetModuleMap(normalizedPath, moduleMapsByPath, out var moduleMap) || !visited.Add(normalizedPath))
            return new ResolvedSource(path, content, sourceLine, sourceColumn);

        try
        {
            var chainedSegment = FindLastSegmentAtOrBefore(moduleMap.Segments, sourceLine, sourceColumn);
            if (chainedSegment is null || chainedSegment.SourceIndex < 0 || chainedSegment.SourceIndex >= moduleMap.Sources.Count)
                return new ResolvedSource(path, content, sourceLine, sourceColumn);

            var moduleSource = moduleMap.Sources[chainedSegment.SourceIndex];
            return ResolveSource(
                moduleSource.Path,
                moduleSource.Content,
                chainedSegment.SourceLine,
                chainedSegment.SourceColumn,
                moduleMapsByPath,
                visited);
        }
        finally
        {
            visited.Remove(normalizedPath);
        }
    }

    private static bool TryGetModuleMap(string normalizedPath, IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath, out SourceMapDocument moduleMap)
    {
        if (moduleMapsByPath.TryGetValue(normalizedPath, out moduleMap!))
            return true;

        foreach (var pair in moduleMapsByPath)
        {
            if (PathMatches(pair.Key, normalizedPath))
            {
                moduleMap = pair.Value;
                return true;
            }
        }

        moduleMap = null!;
        return false;
    }

    private static bool PathMatches(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        return left.EndsWith("/" + right, StringComparison.OrdinalIgnoreCase)
            || right.EndsWith("/" + left, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetOrAddSourceIndex(List<SourceMapSource> sources, Dictionary<string, int> sourceIndexByPath, string path, string? content)
    {
        var normalizedPath = NormalizePath(path);
        if (sourceIndexByPath.TryGetValue(normalizedPath, out var existingIndex))
        {
            if (sources[existingIndex].Content is null && content is not null)
                sources[existingIndex] = sources[existingIndex] with { Content = content };

            return existingIndex;
        }

        var index = sources.Count;
        sources.Add(new SourceMapSource(path, content));
        sourceIndexByPath[normalizedPath] = index;
        return index;
    }

    private static SourceMapSegment? FindLastSegmentAtOrBefore(IReadOnlyList<SourceMapSegment> segments, int generatedLine, int generatedColumn)
    {
        SourceMapSegment? candidate = null;
        foreach (var segment in segments)
        {
            if (segment.GeneratedLine > generatedLine)
                break;
            if (segment.GeneratedLine == generatedLine && segment.GeneratedColumn > generatedColumn)
                break;

            candidate = segment;
        }

        return candidate;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) && uri.IsFile)
            normalized = uri.LocalPath;

        normalized = normalized.Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        return normalized;
    }

    private static SourceMapDocument Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var file = root.TryGetProperty("file", out var fileElement)
            ? fileElement.GetString() ?? string.Empty
            : string.Empty;
        var sourcesArray = root.GetProperty("sources");
        var sourcesContentArray = root.TryGetProperty("sourcesContent", out var contentElement) && contentElement.ValueKind == JsonValueKind.Array
            ? contentElement
            : default;
        var sources = new List<SourceMapSource>(sourcesArray.GetArrayLength());
        for (var index = 0; index < sourcesArray.GetArrayLength(); index++)
        {
            var sourcePath = sourcesArray[index].GetString() ?? string.Empty;
            string? content = null;
            if (sourcesContentArray.ValueKind == JsonValueKind.Array && index < sourcesContentArray.GetArrayLength())
                content = sourcesContentArray[index].ValueKind == JsonValueKind.Null ? null : sourcesContentArray[index].GetString();

            sources.Add(new SourceMapSource(sourcePath, content));
        }

        var mappings = root.TryGetProperty("mappings", out var mappingsElement)
            ? mappingsElement.GetString() ?? string.Empty
            : string.Empty;
        return new SourceMapDocument(file, sources, DecodeMappings(mappings));
    }

    private static IReadOnlyList<SourceMapSegment> DecodeMappings(string mappings)
    {
        var segments = new List<SourceMapSegment>();
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
                continue;

            var sourceIndex = previousSourceIndex + DecodeVlq(mappings, ref position);
            var sourceLine = previousSourceLine + DecodeVlq(mappings, ref position);
            var sourceColumn = previousSourceColumn + DecodeVlq(mappings, ref position);
            previousSourceIndex = sourceIndex;
            previousSourceLine = sourceLine;
            previousSourceColumn = sourceColumn;

            if (position < mappings.Length && mappings[position] != ',' && mappings[position] != ';')
                _ = DecodeVlq(mappings, ref position);

            segments.Add(new SourceMapSegment(generatedLine, generatedColumn, sourceIndex, sourceLine, sourceColumn));
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
                throw new InvalidOperationException("Unexpected end of VLQ mapping.");

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
    {
        return value switch
        {
            >= 'A' and <= 'Z' => value - 'A',
            >= 'a' and <= 'z' => value - 'a' + 26,
            >= '0' and <= '9' => value - '0' + 52,
            '+' => 62,
            '/' => 63,
            _ => throw new InvalidOperationException($"Invalid base64 VLQ digit '{value}'.")
        };
    }

    private sealed record ResolvedSource(string Path, string? Content, int SourceLine, int SourceColumn);
}
