using Jazor.Compiler;
using System.Text.Json;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class GeneratedSourceMapWriterScenarioTests
{
    public static IEnumerable<TestDataRow<GeneratedSourceMapWriterScenario>> Cases
        => GeneratedSourceMapWriterScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<GeneratedSourceMapWriterScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<GeneratedSourceMapWriterValidationScenario>> ValidationCases
        => GeneratedSourceMapWriterScenarioCatalog.Validations.Select(static testCase =>
            new TestDataRow<GeneratedSourceMapWriterValidationScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsAndInputs()
    {
        var allIds = GeneratedSourceMapWriterScenarioCatalog.All.Select(static testCase => testCase.Id)
            .Concat(GeneratedSourceMapWriterScenarioCatalog.Validations.Select(static testCase => testCase.Id))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("source-map-writer.", StringComparison.Ordinal)));
        Assert.IsTrue(GeneratedSourceMapWriterScenarioCatalog.All.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(GeneratedSourceMapWriterScenarioCatalog.Validations.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.HasCount(
            GeneratedSourceMapWriterScenarioCatalog.All.Count,
            GeneratedSourceMapWriterScenarioCatalog.All
                .Select(static testCase => testCase.InputIdentity)
                .Distinct(StringComparer.Ordinal));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Write_EmitsCanonicalJsonAndMappings(GeneratedSourceMapWriterScenario testCase)
    {
        var sourceMap = new GeneratedSourceMap(
            testCase.File,
            testCase.Sources.Select(static source => source.Create()).ToArray(),
            testCase.Segments.Select(static segment => segment.Create()).ToArray());

        var actual = new GeneratedSourceMapWriter().Write(sourceMap);

        Assert.AreEqual(testCase.ExpectedJson, actual, testCase.Id);
        using var document = JsonDocument.Parse(actual);
        var root = document.RootElement;
        Assert.AreEqual(3, root.GetProperty("version").GetInt32(), testCase.Id);
        Assert.AreEqual(testCase.File, root.GetProperty("file").GetString(), testCase.Id);
        CollectionAssert.AreEqual(
            testCase.Sources.Select(static source => source.Path).ToArray(),
            root.GetProperty("sources").EnumerateArray().Select(static source => source.GetString()).ToArray(),
            testCase.Id);
        Assert.HasCount(0, root.GetProperty("names").EnumerateArray().ToArray(), testCase.Id);
        Assert.AreEqual(testCase.ExpectedMappings, root.GetProperty("mappings").GetString(), testCase.Id);
        AssertSourcesContent(root, testCase);
    }

    [TestMethod]
    [DynamicData(nameof(ValidationCases))]
    public void Write_RejectsInvalidInput(GeneratedSourceMapWriterValidationScenario testCase)
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
            new GeneratedSourceMapWriter().Write(null!));

        Assert.AreEqual(testCase.ExpectedParameterName, exception.ParamName, testCase.Id);
    }

    private static void AssertSourcesContent(
        JsonElement root,
        GeneratedSourceMapWriterScenario testCase)
    {
        var expected = testCase.Sources.Select(static source => source.Content).ToArray();
        if (expected.All(static content => content is null))
        {
            Assert.IsFalse(root.TryGetProperty("sourcesContent", out _), testCase.Id);
            return;
        }

        Assert.IsTrue(root.TryGetProperty("sourcesContent", out var sourcesContent), testCase.Id);
        CollectionAssert.AreEqual(
            expected,
            sourcesContent.EnumerateArray()
                .Select(static content => content.ValueKind == JsonValueKind.Null ? null : content.GetString())
                .ToArray(),
            testCase.Id);
    }
}

public sealed record GeneratedSourceMapWriterScenario(
    string Id,
    string Dimension,
    string File,
    IReadOnlyList<GeneratedSourceMapSourceSpec> Sources,
    IReadOnlyList<GeneratedSourceMapSegmentSpec> Segments,
    string ExpectedMappings,
    string ExpectedJson)
{
    public string InputIdentity
        => string.Join(
            "|",
            File,
            string.Join(";", Sources.Select(static source => $"{source.Path}:{source.Content}")),
            string.Join(";", Segments.Select(static segment =>
                $"{segment.GeneratedLine},{segment.GeneratedColumn},{segment.SourceIndex},{segment.SourceLine},{segment.SourceColumn}")));
}

public sealed record GeneratedSourceMapSourceSpec(string Path, string? Content)
{
    internal GeneratedSourceMapSource Create()
        => new(Path, Content);
}

public sealed record GeneratedSourceMapSegmentSpec(
    int GeneratedLine,
    int GeneratedColumn,
    int SourceIndex,
    int SourceLine,
    int SourceColumn)
{
    internal GeneratedSourceMapSegment Create()
        => new(GeneratedLine, GeneratedColumn, SourceIndex, SourceLine, SourceColumn);
}

public sealed record GeneratedSourceMapWriterValidationScenario(
    string Id,
    string Dimension,
    string ExpectedParameterName);

internal static class GeneratedSourceMapWriterScenarioCatalog
{
    public static IReadOnlyList<GeneratedSourceMapWriterScenario> All { get; } =
    [
        Case(
            "empty",
            "empty-sources-and-mappings",
            "app.mjs",
            [],
            [],
            string.Empty,
            """{"version":3,"file":"app.mjs","sources":[],"names":[],"mappings":""}"""),
        Case(
            "embedded-source",
            "single-embedded-source",
            "counter.mjs",
            [Source("Counter.razor", "<Counter />")],
            [Segment(0, 0, 0, 0, 0)],
            "AAAA",
            """{"version":3,"file":"counter.mjs","sources":["Counter.razor"],"sourcesContent":["<Counter />"],"names":[],"mappings":"AAAA"}"""),
        Case(
            "mixed-source-content",
            "partial-sources-content-null-preservation",
            "bundle.mjs",
            [
                Source("Component.razor", "<p>Hello</p>"),
                Source("Generated.cs", content: null)
            ],
            [],
            string.Empty,
            """{"version":3,"file":"bundle.mjs","sources":["Component.razor","Generated.cs"],"sourcesContent":["<p>Hello</p>",null],"names":[],"mappings":""}"""),
        Case(
            "json-escaping",
            "json-string-control-character-escaping",
            "out\"\\\b\f\n\r\t\u0001.mjs",
            [Source("source.cs", "line\"\\\b\f\n\r\t\u0002中文")],
            [],
            string.Empty,
            """{"version":3,"file":"out\"\\\b\f\n\r\t\u0001.mjs","sources":["source.cs"],"sourcesContent":["line\"\\\b\f\n\r\t\u0002中文"],"names":[],"mappings":""}"""),
        Case(
            "same-line-order",
            "generated-column-order-and-delta",
            "ordered.mjs",
            [Source("Source.cs", content: null)],
            [
                Segment(0, 10, 0, 0, 0),
                Segment(0, 2, 0, 0, 0)
            ],
            "EAAA,QAAA",
            """{"version":3,"file":"ordered.mjs","sources":["Source.cs"],"names":[],"mappings":"EAAA,QAAA"}"""),
        Case(
            "line-gap-negative-delta",
            "line-gap-and-negative-source-deltas",
            "gaps.mjs",
            [
                Source("First.cs", content: null),
                Source("Second.cs", content: null)
            ],
            [
                Segment(0, 4, 1, 10, 20),
                Segment(2, 1, 0, 8, 3)
            ],
            "ICUoB;;CDFjB",
            """{"version":3,"file":"gaps.mjs","sources":["First.cs","Second.cs"],"names":[],"mappings":"ICUoB;;CDFjB"}"""),
        Case(
            "multi-byte-vlq",
            "continuation-bit-vlq-encoding",
            "large-column.mjs",
            [Source("Source.cs", content: null)],
            [Segment(0, 32, 0, 0, 0)],
            "gCAAA",
            """{"version":3,"file":"large-column.mjs","sources":["Source.cs"],"names":[],"mappings":"gCAAA"}""")
    ];

    public static IReadOnlyList<GeneratedSourceMapWriterValidationScenario> Validations { get; } =
    [
        new(
            "source-map-writer.validation.null-map",
            "null-source-map-validation",
            "sourceMap")
    ];

    private static GeneratedSourceMapWriterScenario Case(
        string id,
        string dimension,
        string file,
        IReadOnlyList<GeneratedSourceMapSourceSpec> sources,
        IReadOnlyList<GeneratedSourceMapSegmentSpec> segments,
        string expectedMappings,
        string expectedJson)
        => new(
            $"source-map-writer.{id}",
            dimension,
            file,
            sources,
            segments,
            expectedMappings,
            expectedJson);

    private static GeneratedSourceMapSourceSpec Source(string path, string? content)
        => new(path, content);

    private static GeneratedSourceMapSegmentSpec Segment(
        int generatedLine,
        int generatedColumn,
        int sourceIndex,
        int sourceLine,
        int sourceColumn)
        => new(generatedLine, generatedColumn, sourceIndex, sourceLine, sourceColumn);
}
