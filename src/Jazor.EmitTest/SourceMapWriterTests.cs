using System.Text.Json;
using Jazor.Emit.SourceMaps;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SourceMapWriterTests
{
    [TestMethod]
    public void Write_ProducesExternalSourceMapJson()
    {
        var document = new SourceMapDocument(
            File: "components/counter-card.mjs",
            Sources:
            [
                new SourceMapSource("Counter.razor", "<Counter />")
            ],
            Segments:
            [
                new SourceMapSegment(0, 0, 0, 1, 3),
                new SourceMapSegment(1, 0, 0, 2, 3)
            ]);

        var writer = new SourceMapWriter();
        var json = writer.Write(document);
        using var parsed = JsonDocument.Parse(json);

        Assert.AreEqual(3, parsed.RootElement.GetProperty("version").GetInt32());
        Assert.AreEqual("components/counter-card.mjs", parsed.RootElement.GetProperty("file").GetString());
        Assert.AreEqual("Counter.razor", parsed.RootElement.GetProperty("sources")[0].GetString());
        Assert.AreEqual("<Counter />", parsed.RootElement.GetProperty("sourcesContent")[0].GetString());
        Assert.AreNotEqual(string.Empty, parsed.RootElement.GetProperty("mappings").GetString());
        Assert.IsTrue(json.Contains('\n'));
    }

    [TestMethod]
    public void Write_ProducesExpectedVlqMappings()
    {
        var document = new SourceMapDocument(
            File: "components/counter-card.mjs",
            Sources:
            [
                new SourceMapSource("Counter.razor", "<Counter />")
            ],
            Segments:
            [
                new SourceMapSegment(0, 0, 0, 1, 3),
                new SourceMapSegment(1, 0, 0, 2, 3)
            ]);

        var writer = new SourceMapWriter();
        var json = writer.Write(document);
        using var parsed = JsonDocument.Parse(json);

        Assert.AreEqual("AACG;AACA", parsed.RootElement.GetProperty("mappings").GetString());
    }

    [TestMethod]
    public void AppendSourceMappingUrl_NormalizesTrailingNewlines()
    {
        var writer = new SourceMapWriter();
        var code = writer.AppendSourceMappingUrl("export const a = 1;\n\n", "counter-card.mjs.map");

        Assert.AreEqual(
            "export const a = 1;" + Environment.NewLine +
            "//# sourceMappingURL=counter-card.mjs.map" + Environment.NewLine,
            code);
    }

    [TestMethod]
    public void AppendSourceMappingUrl_EmitsOnlyCommentForEmptyInput()
    {
        var writer = new SourceMapWriter();
        var code = writer.AppendSourceMappingUrl(string.Empty, "counter-card.mjs.map");

        Assert.AreEqual(
            "//# sourceMappingURL=counter-card.mjs.map" + Environment.NewLine,
            code);
    }

    [TestMethod]
    public void AppendSourceMappingUrl_AppendsComment()
    {
        var writer = new SourceMapWriter();
        var code = writer.AppendSourceMappingUrl("export const a = 1;\n", "counter-card.mjs.map");

        StringAssert.Contains(code, "//# sourceMappingURL=counter-card.mjs.map");
        Assert.AreEqual(1, code.Split("sourceMappingURL=").Length - 1);
    }
}
