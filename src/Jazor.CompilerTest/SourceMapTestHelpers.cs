using System.Text;
using System.Text.Json;

namespace Jazor.CompilerTest;

internal static class SourceMapTestHelpers
{
    public readonly record struct SourceMapLocation(int SourceIndex, int SourceLine);
    public readonly record struct SourceMapSegment(
        int GeneratedLine,
        int GeneratedColumn,
        int SourceIndex,
        int SourceLine,
        int SourceColumn);

    public static void AssertGeneratedLineMapsToSourceLine(
        string generatedText,
        string generatedNeedle,
        string sourceText,
        string sourceNeedle,
        IReadOnlyDictionary<int, int> mappedLines)
    {
        var generatedLine = GetLineIndexContaining(generatedText, generatedNeedle);
        Assert.IsTrue(
            mappedLines.TryGetValue(generatedLine, out var sourceLine),
            $"Expected generated line containing '{generatedNeedle}' to have a source-map segment.");
        Assert.AreEqual(GetLineIndexContaining(sourceText, sourceNeedle), sourceLine);
    }

    public static void AssertGeneratedLineMapsToSource(
        string generatedText,
        string generatedNeedle,
        JsonElement sourceMap,
        string expectedSourcePath,
        string sourceText,
        string sourceNeedle,
        IReadOnlyDictionary<int, SourceMapLocation> mappedLocations)
    {
        var generatedLine = GetLineIndexContaining(generatedText, generatedNeedle);
        Assert.IsTrue(
            mappedLocations.TryGetValue(generatedLine, out var sourceLocation),
            $"Expected generated line containing '{generatedNeedle}' to have a source-map segment.");

        Assert.AreEqual(FindSourceIndexContaining(sourceMap, expectedSourcePath), sourceLocation.SourceIndex);
        Assert.AreEqual(GetLineIndexContaining(sourceText, sourceNeedle), sourceLocation.SourceLine);
    }

    public static IReadOnlyDictionary<int, int> DecodeGeneratedLineToSourceLine(JsonElement sourceMap)
    {
        return DecodeGeneratedLineToSourceLocation(sourceMap)
            .ToDictionary(static entry => entry.Key, static entry => entry.Value.SourceLine);
    }

    public static IReadOnlyDictionary<int, SourceMapLocation> DecodeGeneratedLineToSourceLocation(JsonElement sourceMap)
    {
        var result = new Dictionary<int, SourceMapLocation>();
        foreach (var segment in DecodeSegments(sourceMap))
            result.TryAdd(segment.GeneratedLine, new SourceMapLocation(segment.SourceIndex, segment.SourceLine));

        return result;
    }

    public static IReadOnlyList<SourceMapSegment> DecodeSegments(JsonElement sourceMap)
    {
        const string base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        var mappings = sourceMap.GetProperty("mappings").GetString() ?? string.Empty;
        var result = new List<SourceMapSegment>();
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

            previousGeneratedColumn += DecodeVlq(mappings, ref position, base64Digits);
            if (position >= mappings.Length || mappings[position] == ',' || mappings[position] == ';')
            {
                continue;
            }

            previousSourceIndex += DecodeVlq(mappings, ref position, base64Digits);
            previousSourceLine += DecodeVlq(mappings, ref position, base64Digits);
            previousSourceColumn += DecodeVlq(mappings, ref position, base64Digits);
            result.Add(new SourceMapSegment(
                GeneratedLine: generatedLine,
                GeneratedColumn: previousGeneratedColumn,
                SourceIndex: previousSourceIndex,
                SourceLine: previousSourceLine,
                SourceColumn: previousSourceColumn));

            if (position < mappings.Length && mappings[position] != ',' && mappings[position] != ';')
            {
                _ = DecodeVlq(mappings, ref position, base64Digits);
            }
        }

        return result;
    }

    public static string CreateSingleSourceLineMap(
        string fileName,
        string sourceText,
        IReadOnlyList<int> sourceLines)
    {
        var mappings = new StringBuilder();
        var previousSourceLine = 0;
        for (var index = 0; index < sourceLines.Count; index++)
        {
            if (index > 0)
            {
                mappings.Append(';');
            }

            mappings.Append(EncodeVlq(0));
            mappings.Append(EncodeVlq(0));
            mappings.Append(EncodeVlq(sourceLines[index] - previousSourceLine));
            mappings.Append(EncodeVlq(0));
            previousSourceLine = sourceLines[index];
        }

        return JsonSerializer.Serialize(new
        {
            version = 3,
            sources = new[] { fileName },
            sourcesContent = new[] { sourceText },
            names = Array.Empty<string>(),
            mappings = mappings.ToString(),
            file = Path.ChangeExtension(fileName, ".js")
        });
    }

    public static int GetLineIndexContaining(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        Assert.IsTrue(index >= 0, $"Expected to find '{value}'.");

        var line = 0;
        for (var position = 0; position < index; position++)
        {
            if (text[position] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    public static (int Line, int Column) GetLineColumnContaining(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        Assert.IsTrue(index >= 0, $"Expected to find '{value}'.");

        var line = 0;
        var column = 0;
        var lastWasCarriageReturn = false;
        for (var position = 0; position < index; position++)
        {
            var ch = text[position];
            switch (ch)
            {
                case '\r':
                    line++;
                    column = 0;
                    lastWasCarriageReturn = true;
                    break;
                case '\n':
                    if (!lastWasCarriageReturn)
                        line++;

                    column = 0;
                    lastWasCarriageReturn = false;
                    break;
                default:
                    column++;
                    lastWasCarriageReturn = false;
                    break;
            }
        }

        return (line, column);
    }

    public static int FindSourceIndexContaining(JsonElement sourceMap, string expectedSourcePath)
    {
        var normalizedExpectedPath = expectedSourcePath.Replace('\\', '/');
        var sources = sourceMap.GetProperty("sources");
        for (var index = 0; index < sources.GetArrayLength(); index++)
        {
            var source = sources[index].GetString();
            if (string.IsNullOrWhiteSpace(source))
            {
                continue;
            }

            var normalizedSource = source.Replace('\\', '/');
            if (normalizedSource.EndsWith(normalizedExpectedPath, StringComparison.OrdinalIgnoreCase)
                || normalizedSource.Contains("/" + normalizedExpectedPath, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        Assert.Fail(
            $"Expected source map sources to contain '{expectedSourcePath}', but found: {string.Join(", ", sources.EnumerateArray().Select(static source => source.GetString()))}");
        return -1;
    }

    private static string EncodeVlq(int value)
    {
        const string base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        var vlq = value < 0 ? ((-value) << 1) + 1 : value << 1;
        var builder = new StringBuilder();
        do
        {
            var digit = vlq & 31;
            vlq >>= 5;
            if (vlq > 0)
            {
                digit |= 32;
            }

            builder.Append(base64Digits[digit]);
        }
        while (vlq > 0);

        return builder.ToString();
    }

    private static int DecodeVlq(string mappings, ref int position, string base64Digits)
    {
        var result = 0;
        var shift = 0;
        int digit;
        do
        {
            digit = base64Digits.IndexOf(mappings[position], StringComparison.Ordinal);
            Assert.IsTrue(digit >= 0, $"Invalid source-map VLQ digit '{mappings[position]}'.");
            position++;
            var continuation = (digit & 32) != 0;
            digit &= 31;
            result += digit << shift;
            shift += 5;
            if (!continuation)
            {
                break;
            }
        }
        while (position < mappings.Length);

        var negative = (result & 1) == 1;
        result >>= 1;
        return negative ? -result : result;
    }
}
