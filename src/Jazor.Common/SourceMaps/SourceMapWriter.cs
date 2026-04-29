using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ECMAScript.Contract.SourceMaps;

public sealed class SourceMapWriter
{
    private const string Base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    public string Write(SourceMapDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var payload = new
        {
            version = 3,
            file = document.File,
            sources = document.Sources.Select(static source => source.Path).ToArray(),
            sourcesContent = document.Sources.Select(static source => source.Content).ToArray(),
            names = Array.Empty<string>(),
            mappings = BuildMappings(document.Segments)
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public string AppendSourceMappingUrl(string code, string mapFileName)
    {
        if (string.IsNullOrWhiteSpace(mapFileName))
            throw new ArgumentException("Map file name is required.", nameof(mapFileName));

        var normalized = (code ?? string.Empty).TrimEnd('\r', '\n');
        if (normalized.Length == 0)
            return $"//# sourceMappingURL={mapFileName}\n";

        return normalized + "\n" + $"//# sourceMappingURL={mapFileName}\n";
    }

    private static string BuildMappings(IReadOnlyList<SourceMapSegment> segments)
    {
        if (segments.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        var generatedLine = 0;
        var previousGeneratedColumn = 0;
        var previousSourceIndex = 0;
        var previousSourceLine = 0;
        var previousSourceColumn = 0;

        foreach (var group in segments.GroupBy(static segment => segment.GeneratedLine).OrderBy(static group => group.Key))
        {
            while (generatedLine < group.Key)
            {
                builder.Append(';');
                generatedLine++;
                previousGeneratedColumn = 0;
            }

            var first = true;
            foreach (var segment in group.OrderBy(static segment => segment.GeneratedColumn))
            {
                if (!first)
                    builder.Append(',');
                first = false;

                builder.Append(EncodeVlq(segment.GeneratedColumn - previousGeneratedColumn));
                builder.Append(EncodeVlq(segment.SourceIndex - previousSourceIndex));
                builder.Append(EncodeVlq(segment.SourceLine - previousSourceLine));
                builder.Append(EncodeVlq(segment.SourceColumn - previousSourceColumn));

                previousGeneratedColumn = segment.GeneratedColumn;
                previousSourceIndex = segment.SourceIndex;
                previousSourceLine = segment.SourceLine;
                previousSourceColumn = segment.SourceColumn;
            }
        }

        return builder.ToString();
    }

    private static string EncodeVlq(int value)
    {
        var vlq = value < 0 ? ((-value) << 1) + 1 : (value << 1);
        var builder = new StringBuilder();
        do
        {
            var digit = vlq & 31;
            vlq >>= 5;
            if (vlq > 0)
                digit |= 32;
            builder.Append(Base64Digits[digit]);
        }
        while (vlq > 0);

        return builder.ToString();
    }
}
