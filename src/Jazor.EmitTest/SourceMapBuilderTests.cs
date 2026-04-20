using Jazor.Emit;
using Jazor.Emit.SourceMaps;
using Jazor.SourceMaps;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SourceMapBuilderTests
{
    [TestMethod]
    public void BuildModuleMap_UsesGeneratedSpansToCreateLineMappings()
    {
        var moduleCode = "export const a = 1;\nexport const b = 2;\n";
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 12,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 19,
                StartLine: 2,
                StartColumn: 4,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation),
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 24,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 20,
                GeneratedSpanLength: 19,
                StartLine: 3,
                StartColumn: 4,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode,
            origins,
            path => path == "Counter.razor" ? "<Counter />" : null);

        Assert.AreEqual("components/counter-card.mjs", document.File);
        Assert.AreEqual(1, document.Sources.Count);
        Assert.AreEqual("Counter.razor", document.Sources[0].Path);
        Assert.AreEqual("<Counter />", document.Sources[0].Content);
        Assert.AreEqual(2, document.Segments.Count);
        Assert.AreEqual(0, document.Segments[0].GeneratedLine);
        Assert.AreEqual(1, document.Segments[1].GeneratedLine);
        Assert.AreEqual(1, document.Segments[0].SourceLine);
        Assert.AreEqual(2, document.Segments[1].SourceLine);
    }

    [TestMethod]
    public void BuildModuleMap_AdvancesSourceLineAcrossMultiLineSpan()
    {
        var moduleCode = "line 1\nline 2\nline 3\n";
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 20,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: moduleCode.Length,
                StartLine: 10,
                StartColumn: 4,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode,
            origins,
            _ => null);

        CollectionAssert.AreEqual(new[] { 9, 10, 11 }, document.Segments.Select(static segment => segment.SourceLine).ToArray());
        CollectionAssert.AreEqual(new[] { 3, 0, 0 }, document.Segments.Select(static segment => segment.SourceColumn).ToArray());
    }

    [TestMethod]
    public void BuildModuleMap_SkipsZeroLengthOrigins()
    {
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 0,
                StartLine: 2,
                StartColumn: 4,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode: "export const a = 1;\n",
            origins,
            _ => null);

        Assert.AreEqual(0, document.Segments.Count);
    }

    [TestMethod]
    public void BuildModuleMap_SkipsOutOfRangeOrigins()
    {
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 100,
                GeneratedSpanLength: 8,
                StartLine: 2,
                StartColumn: 4,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode: "export const a = 1;\n",
            origins,
            _ => null);

        Assert.AreEqual(0, document.Segments.Count);
    }

    [TestMethod]
    public void BuildModuleMap_SkipsOriginsWhenGeneratedCodeIsEmpty()
    {
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 8,
                StartLine: 2,
                StartColumn: 4,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode: string.Empty,
            origins,
            _ => null);

        Assert.AreEqual(0, document.Segments.Count);
    }

    [TestMethod]
    public void BuildModuleMap_OnlyIncludesSourcesForRequestedGeneratedFile()
    {
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 8,
                StartLine: 2,
                StartColumn: 1,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation),
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Other.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 8,
                GeneratedFilePath: "components/other-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 8,
                StartLine: 5,
                StartColumn: 1,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode: "counter();\n",
            origins,
            path => path);

        Assert.AreEqual(1, document.Sources.Count);
        Assert.AreEqual("Counter.razor", document.Sources[0].Path);
        Assert.AreEqual(1, document.Segments.Count);
        Assert.AreEqual(0, document.Segments[0].SourceIndex);
    }

    [TestMethod]
    public void BuildModuleMap_PreservesMultipleSegmentsOnSameGeneratedLine()
    {
        var moduleCode = "alpha beta gamma\n";
        var origins = new[]
        {
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 0,
                SourceSpanLength: 5,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 0,
                GeneratedSpanLength: 5,
                StartLine: 2,
                StartColumn: 1,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation),
            new RazorVueEmitSourceOriginRecord(
                SourceFilePath: "Counter.razor",
                SourceSpanStart: 6,
                SourceSpanLength: 4,
                GeneratedFilePath: "components/counter-card.mjs",
                GeneratedSpanStart: 6,
                GeneratedSpanLength: 4,
                StartLine: 3,
                StartColumn: 2,
                MappingQuality: RazorVueMappingQualityRecord.MappedFromGenerated,
                Provenance: RazorVueOriginProvenanceRecord.GeneratedSyntaxLocation)
        };

        var builder = new SourceMapBuilder();
        var document = builder.BuildModuleMap(
            generatedFileName: "components/counter-card.mjs",
            moduleCode,
            origins,
            _ => null);

        Assert.AreEqual(2, document.Segments.Count);
        CollectionAssert.AreEqual(new[] { 0, 6 }, document.Segments.Select(static segment => segment.GeneratedColumn).ToArray());
    }
}
