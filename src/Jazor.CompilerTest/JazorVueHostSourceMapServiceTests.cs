using Jazor.VueHost.SourceMap;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostSourceMapServiceTests
{
    [TestMethod]
    public void SourceMapService_RegisterAndLookupOriginalAndGeneratedPositions()
    {
        var service = new InMemorySourceMapService();
        const string sourceText = "line1\nline2";
        const string sourceMapJson = """
            {"version":3,"sources":["Counter.jazor"],"sourcesContent":["line1\nline2"],"names":[],"mappings":"AAAA;AACA","file":"Counter.js"}
            """;

        service.Register("/Counter.jazor", sourceMapJson);

        Assert.AreEqual(sourceMapJson, service.GetSourceMapJson("/Counter.jazor"));
        Assert.AreEqual(sourceText, service.GetSourceContent("/Counter.jazor", 0));

        var original = service.OriginalPositionFor("/Counter.jazor", 1, 0);
        Assert.IsNotNull(original);
        Assert.AreEqual("Counter.jazor", original.SourcePath);
        Assert.AreEqual(1, original.Line);
        Assert.AreEqual(0, original.Column);

        var generated = service.GeneratedPositionFor(@"D:\temp\Counter.jazor", 1, 0);
        Assert.IsNotNull(generated);
        Assert.AreEqual("/Counter.jazor", generated.GeneratedPath);
        Assert.AreEqual(1, generated.Line);
        Assert.AreEqual(0, generated.Column);
    }

    [TestMethod]
    public void SourceMapService_UnregisterAndClear_RemoveEntries()
    {
        var service = new InMemorySourceMapService();
        const string sourceMapJson = """
            {"version":3,"sources":["Counter.jazor"],"sourcesContent":["line1"],"names":[],"mappings":"AAAA","file":"Counter.js"}
            """;

        service.Register("/Counter.jazor", sourceMapJson);
        service.Unregister("/Counter.jazor");

        Assert.IsNull(service.GetSourceMapJson("/Counter.jazor"));
        Assert.IsNull(service.OriginalPositionFor("/Counter.jazor", 0, 0));

        service.Register("/Counter.jazor", sourceMapJson);
        service.Clear();

        Assert.IsNull(service.GetSourceMapJson("/Counter.jazor"));
        Assert.IsNull(service.GeneratedPositionFor("Counter.jazor", 0, 0));
    }

    [TestMethod]
    public void SourceMapService_OriginalPositionFor_HttpGeneratedUrl_IgnoresHostAndQueryString()
    {
        var service = new InMemorySourceMapService();
        const string sourceMapJson = """
            {"version":3,"sources":["main.ts"],"sourcesContent":["export const version = 1;"],"names":[],"mappings":"AAAA","file":"main.js"}
            """;
        service.Register("/main.ts", sourceMapJson);

        var original = service.OriginalPositionFor("http://127.0.0.1:5173/main.ts?t=1710000000", 0, 0);

        Assert.IsNotNull(original);
        Assert.AreEqual("main.ts", original.SourcePath);
        Assert.AreEqual(0, original.Line);
        Assert.AreEqual(0, original.Column);
    }
}
