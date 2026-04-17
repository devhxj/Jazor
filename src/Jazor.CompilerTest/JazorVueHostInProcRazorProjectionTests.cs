using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Razor.InProc;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostInProcRazorProjectionTests
{
    [TestMethod]
    public void RazorDesignTimeCodeProjectionService_TryCreateProjection_ReturnsSegmentMappedCSharpDocument()
    {
        var service = new RazorDesignTimeCodeProjectionService();
        var document = new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            """
            <h1>Counter</h1>

            @code {
                private int count;
            }
            """,
            "1");

        var created = service.TryCreateProjection(document, out var projection);
        var codeOffset = document.Text.IndexOf("count", StringComparison.Ordinal);

        Assert.IsTrue(created);
        Assert.IsTrue(projection.ProjectionMap.Segments.Count > 0);
        Assert.IsTrue(projection.SourceText.Contains("count", StringComparison.Ordinal));
        Assert.IsTrue(codeOffset >= 0);
        Assert.IsTrue(projection.ProjectionMap.TryMapToProjectedOffset(codeOffset, out var projectedOffset));
        Assert.AreEqual("count", projection.SourceText.Substring(projectedOffset, "count".Length));
    }

    [TestMethod]
    public void RazorDesignTimeCodeProjectionService_TryCreateProjection_WithSlashNormalizedPath_ReturnsSegmentMappedCSharpDocument()
    {
        var service = new RazorDesignTimeCodeProjectionService();
        var document = new DocumentSnapshot(
            "D:/temp/Counter.jazor",
            DocumentKind.Jazor,
            """
            <h1>Counter</h1>

            @code {
                private int count;
            }
            """,
            "1");

        var created = service.TryCreateProjection(document, out var projection);
        var codeOffset = document.Text.IndexOf("count", StringComparison.Ordinal);

        Assert.IsTrue(created);
        Assert.IsTrue(projection.ProjectionMap.Segments.Count > 0);
        Assert.IsTrue(projection.SourceText.Contains("count", StringComparison.Ordinal));
        Assert.IsTrue(codeOffset >= 0);
        Assert.IsTrue(projection.ProjectionMap.TryMapToProjectedOffset(codeOffset, out var projectedOffset));
        Assert.AreEqual("count", projection.SourceText.Substring(projectedOffset, "count".Length));
    }

    [TestMethod]
    public void RazorDesignTimeCodeProjectionService_TryCreateProjection_RejectsUnsupportedDocuments()
    {
        var service = new RazorDesignTimeCodeProjectionService();
        var nonJazorDocument = new DocumentSnapshot(
            @"D:\temp\Counter.vue",
            DocumentKind.Vue,
            "<template><div>Counter</div></template>",
            "1");
        var emptyJazorDocument = new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            string.Empty,
            "1");

        Assert.IsFalse(service.TryCreateProjection(nonJazorDocument, out _));
        Assert.IsFalse(service.TryCreateProjection(emptyJazorDocument, out _));
    }
}
