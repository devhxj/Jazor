using Jazor.Emit.SourceMaps;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SourceMapChainBuilderTests
{
    [TestMethod]
    public void Chain_RewritesBundleSegmentsToOriginalSources()
    {
        var builder = new SourceMapChainBuilder();
        var bundleMap = new SourceMapDocument(
            "bundle.js",
            [new SourceMapSource("host/app.mjs", null)],
            [
                new SourceMapSegment(0, 0, 0, 0, 0),
                new SourceMapSegment(1, 0, 0, 1, 0)
            ]);
        var moduleMap = new SourceMapDocument(
            "host/app.mjs",
            [new SourceMapSource("Counter.razor", "<Counter />")],
            [
                new SourceMapSegment(0, 0, 0, 10, 2),
                new SourceMapSegment(1, 0, 0, 11, 0)
            ]);

        var chained = builder.Chain(bundleMap, new Dictionary<string, SourceMapDocument>(StringComparer.OrdinalIgnoreCase)
        {
            ["host/app.mjs"] = moduleMap
        });

        Assert.AreEqual("bundle.js", chained.File);
        Assert.AreEqual(1, chained.Sources.Count);
        Assert.AreEqual("Counter.razor", chained.Sources[0].Path);
        Assert.AreEqual("<Counter />", chained.Sources[0].Content);
        CollectionAssert.AreEqual(
            new[]
            {
                new SourceMapSegment(0, 0, 0, 10, 2),
                new SourceMapSegment(1, 0, 0, 11, 0)
            },
            chained.Segments.ToArray());
    }

    [TestMethod]
    public void Chain_MissingModuleMap_PreservesBundleSource()
    {
        var builder = new SourceMapChainBuilder();
        var bundleMap = new SourceMapDocument(
            "bundle.js",
            [new SourceMapSource("host/app.mjs", "export const value = 1;")],
            [new SourceMapSegment(0, 0, 0, 0, 0)]);

        var chained = builder.Chain(bundleMap, new Dictionary<string, SourceMapDocument>(StringComparer.OrdinalIgnoreCase));

        Assert.AreEqual(1, chained.Sources.Count);
        Assert.AreEqual("host/app.mjs", chained.Sources[0].Path);
        Assert.AreEqual("export const value = 1;", chained.Sources[0].Content);
        CollectionAssert.AreEqual(bundleMap.Segments.ToArray(), chained.Segments.ToArray());
    }

    [TestMethod]
    public void Chain_PreservesOriginalSourcesContent()
    {
        var builder = new SourceMapChainBuilder();
        var bundleMap = new SourceMapDocument(
            "bundle.js",
            [new SourceMapSource("host/app.mjs", null)],
            [new SourceMapSegment(0, 0, 0, 0, 4)]);
        var moduleMap = new SourceMapDocument(
            "host/app.mjs",
            [new SourceMapSource("Pages/Counter.razor", "<button>@count</button>")],
            [new SourceMapSegment(0, 0, 0, 4, 8)]);

        var chained = builder.Chain(bundleMap, new Dictionary<string, SourceMapDocument>(StringComparer.OrdinalIgnoreCase)
        {
            ["host/app.mjs"] = moduleMap
        });

        Assert.AreEqual(1, chained.Sources.Count);
        Assert.AreEqual("Pages/Counter.razor", chained.Sources[0].Path);
        Assert.AreEqual("<button>@count</button>", chained.Sources[0].Content);
    }

    [TestMethod]
    public void Chain_RecursivelyResolvesIntermediateEntryMaps()
    {
        var builder = new SourceMapChainBuilder();
        var bundleMap = new SourceMapDocument(
            "bundle.js",
            [new SourceMapSource("__jazor_bundle_entry__.mjs", null)],
            [new SourceMapSegment(0, 0, 0, 0, 0)]);
        var entryMap = new SourceMapDocument(
            "__jazor_bundle_entry__.mjs",
            [new SourceMapSource("host/app.mjs", null)],
            [new SourceMapSegment(0, 0, 0, 0, 0)]);
        var moduleMap = new SourceMapDocument(
            "host/app.mjs",
            [new SourceMapSource("Counter.razor", "<Counter />")],
            [new SourceMapSegment(0, 0, 0, 8, 1)]);

        var chained = builder.Chain(bundleMap, new Dictionary<string, SourceMapDocument>(StringComparer.OrdinalIgnoreCase)
        {
            ["__jazor_bundle_entry__.mjs"] = entryMap,
            ["host/app.mjs"] = moduleMap
        });

        Assert.AreEqual(1, chained.Sources.Count);
        Assert.AreEqual("Counter.razor", chained.Sources[0].Path);
        Assert.AreEqual("<Counter />", chained.Sources[0].Content);
        Assert.AreEqual(new SourceMapSegment(0, 0, 0, 8, 1), chained.Segments[0]);
    }
}
