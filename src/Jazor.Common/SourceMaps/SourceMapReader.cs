using System.Collections.Generic;
using System.Text.Json;

namespace Jazor.Common.SourceMaps;

public sealed class SourceMapReader
{
    private const string Base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    public SourceMapDocument Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("Source map json is required.", nameof(json));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var file = root.TryGetProperty("file", out var fileElement)
            ? fileElement.GetString() ?? string.Empty
            : string.Empty;
        var sourcesArray = root.GetProperty("sources");
        var sourcesContentArray = root.TryGetProperty("sourcesContent", out var contentElement) &&
                                  contentElement.ValueKind == JsonValueKind.Array
            ? contentElement
            : default;
        var sources = new List<SourceMapSource>(sourcesArray.GetArrayLength());

        for (var index = 0; index < sourcesArray.GetArrayLength(); index++)
        {
            var sourcePath = sourcesArray[index].GetString() ?? string.Empty;
            string? content = null;
            if (sourcesContentArray.ValueKind == JsonValueKind.Array &&
                index < sourcesContentArray.GetArrayLength())
            {
                content = sourcesContentArray[index].ValueKind == JsonValueKind.Null
                    ? null
                    : sourcesContentArray[index].GetString();
            }

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
        var index = Base64Digits.IndexOf(value);
        if (index < 0)
            throw new InvalidOperationException($"Invalid base64 VLQ digit '{value}'.");

        return index;
    }
}
