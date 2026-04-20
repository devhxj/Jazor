using Jazor.VueContracts.Protocol;
using Jolt.Jazor.Projection;
using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Mapping;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltProjectionMapTests
{
    [TestMethod]
    public void ProjectionMap_TryMapToProjectedOffset_MapsAcrossSegments()
    {
        var map = new ProjectionMap(
            "source.jazor",
            "virtual:source.jazor.g.vue",
            [
                new ProjectionSegment(0, 5, 10, 5),
                new ProjectionSegment(8, 4, 20, 4)
            ]);

        Assert.IsTrue(map.TryMapToProjectedOffset(0, out var firstStart));
        Assert.AreEqual(10, firstStart);

        Assert.IsTrue(map.TryMapToProjectedOffset(4, out var firstEnd));
        Assert.AreEqual(14, firstEnd);

        Assert.IsTrue(map.TryMapToProjectedOffset(9, out var secondMiddle));
        Assert.AreEqual(21, secondMiddle);

        Assert.IsFalse(map.TryMapToProjectedOffset(6, out _));
    }

    [TestMethod]
    public void ProjectionMap_TryMapToOriginalOffset_MapsAcrossSegments()
    {
        var map = new ProjectionMap(
            "source.jazor",
            "virtual:source.jazor.g.vue",
            [
                new ProjectionSegment(0, 5, 10, 5),
                new ProjectionSegment(8, 4, 20, 4)
            ]);

        Assert.IsTrue(map.TryMapToOriginalOffset(10, out var firstStart));
        Assert.AreEqual(0, firstStart);

        Assert.IsTrue(map.TryMapToOriginalOffset(14, out var firstEnd));
        Assert.AreEqual(4, firstEnd);

        Assert.IsTrue(map.TryMapToOriginalOffset(23, out var secondEnd));
        Assert.AreEqual(11, secondEnd);

        Assert.IsFalse(map.TryMapToOriginalOffset(16, out _));
    }

    [TestMethod]
    public void ProjectionMap_TryMapToProjectedPosition_UsesProjectedText()
    {
        const string sourceText = "ab\ncd\nef";
        const string projectedText = "__ab\nXY\ncd\nef";
        var map = new ProjectionMap(
            "source.jazor",
            "virtual:source.jazor.g.vue",
            [
                new ProjectionSegment(0, 8, 2, 8)
            ]);

        var sourcePosition = new LspPosition { Line = 1, Character = 1 };
        var mapped = map.TryMapToProjectedPosition(sourceText, sourcePosition, projectedText, out var projectedPosition);

        Assert.IsTrue(mapped);
        Assert.AreEqual(1, projectedPosition.Line);
        Assert.AreEqual(1, projectedPosition.Character);
    }

    [TestMethod]
    public void ProjectionMap_TryMapToOriginalPosition_UsesSourceText()
    {
        const string sourceText = "ab\ncd\nef";
        const string projectedText = "__ab\nXY\ncd\nef";
        var map = new ProjectionMap(
            "source.jazor",
            "virtual:source.jazor.g.vue",
            [
                new ProjectionSegment(0, 8, 2, 8)
            ]);

        var projectedPosition = new LspPosition { Line = 2, Character = 1 };
        var mapped = map.TryMapToOriginalPosition(projectedText, projectedPosition, sourceText, out var originalPosition);

        Assert.IsTrue(mapped);
        Assert.AreEqual(2, originalPosition.Line);
        Assert.AreEqual(1, originalPosition.Character);
    }

    [TestMethod]
    public void ProjectionMap_TryMapToProjectedRange_MapsRangeBoundaries()
    {
        const string sourceText = "abcd\nefgh";
        const string projectedText = "12abcd\nefgh34";
        var map = new ProjectionMap(
            "source.jazor",
            "virtual:source.jazor.g.vue",
            [
                new ProjectionSegment(0, sourceText.Length, 2, sourceText.Length)
            ]);

        var sourceRange = new LspRange
        {
            Start = new LspPosition { Line = 0, Character = 2 },
            End = new LspPosition { Line = 1, Character = 2 }
        };

        var mapped = map.TryMapToProjectedRange(sourceText, sourceRange, projectedText, out var projectedRange);

        Assert.IsTrue(mapped);
        Assert.AreEqual(0, projectedRange.Start.Line);
        Assert.AreEqual(4, projectedRange.Start.Character);
        Assert.AreEqual(1, projectedRange.End.Line);
        Assert.AreEqual(2, projectedRange.End.Character);
    }

    [TestMethod]
    public async Task JazorProjectionService_ProjectAsync_BuildsSegmentAwareVueProjectionMap()
    {
        var document = new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            """
            <template>
              <UserCard />
            </template>

            @code {
                public string Title { get; set; } = "";
            }
            """,
            "1");
        var service = new JazorProjectionService();

        var projectedDocuments = await service.ProjectAsync(document, CancellationToken.None);
        var vueDocument = projectedDocuments.Single(candidate => candidate.Identity.DocumentKind == VirtualDocumentKind.Vue);
        var csharpDocument = projectedDocuments.Single(candidate => candidate.Identity.DocumentKind == VirtualDocumentKind.CSharp);
        var templateOffset = document.Text.IndexOf("UserCard", StringComparison.Ordinal);
        var codeOffset = document.Text.IndexOf("Title", StringComparison.Ordinal);

        Assert.IsTrue(vueDocument.ProjectionMap.Segments.Count >= 2);
        Assert.IsTrue(vueDocument.ProjectionMap.TryMapToProjectedOffset(templateOffset, out var projectedTemplateOffset));
        Assert.AreEqual("UserCard", vueDocument.Text.Substring(projectedTemplateOffset, "UserCard".Length));

        Assert.IsTrue(vueDocument.ProjectionMap.TryMapToProjectedOffset(codeOffset, out var projectedCodeOffset));
        Assert.AreEqual("Title", vueDocument.Text.Substring(projectedCodeOffset, "Title".Length));

        Assert.IsTrue(csharpDocument.ProjectionMap.Segments.Count >= 1);
        Assert.IsTrue(csharpDocument.ProjectionMap.TryMapToProjectedOffset(codeOffset, out var projectedCSharpOffset));
        Assert.AreEqual("Title", csharpDocument.Text.Substring(projectedCSharpOffset, "Title".Length));
    }

    [TestMethod]
    public async Task DocumentProjectionResolver_ResolveAsync_RoutesTemplateRequestsToSourceFrontendDocument()
    {
        var document = new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            """
            <template>
              <UserCard />
            </template>
            """,
            "1");
        var service = new JazorProjectionService();
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(await service.ProjectCodeAsync(document, CancellationToken.None), CancellationToken.None);
        var resolver = new DocumentProjectionResolver(new DocumentRegionClassifier(), registry);

        var target = await resolver.ResolveAsync(
            document,
            new LspPosition { Line = 1, Character = 5 },
            CancellationToken.None);

        Assert.AreEqual(LaneKind.Volar, target.LaneKind);
        Assert.AreEqual(DocumentRegionKind.Template, target.RegionKind);
        Assert.IsFalse(target.IsProjected);
        Assert.AreEqual(document.DocumentPath, target.ProjectedDocumentPath);
        Assert.AreEqual(document.DocumentPath, target.MappingId);
        Assert.AreEqual(1, target.ProjectedPosition!.Line);
        Assert.AreEqual(5, target.ProjectedPosition.Character);
    }
}
