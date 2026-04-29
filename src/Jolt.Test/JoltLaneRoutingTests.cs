using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Test;

[TestClass]
public sealed class JoltLaneRoutingTests
{
    [TestMethod]
    public void LspLaneRouter_GetOrderedLanes_DoesNotAppendJazorFallbackForFrontendOrRoslynTargets()
    {
        var router = new LspLaneRouter();
        var documentPath = @"D:\temp\Counter.jazor";

        var frontendLanes = router.GetOrderedLanes(new ProjectionTarget(
            LaneKind.Volar,
            DocumentRegionKind.Template,
            documentPath,
            documentPath));
        var roslynLanes = router.GetOrderedLanes(new ProjectionTarget(
            LaneKind.Roslyn,
            DocumentRegionKind.Code,
            documentPath,
            documentPath));

        CollectionAssert.AreEqual(new[] { LaneKind.Volar }, frontendLanes.ToArray());
        CollectionAssert.AreEqual(new[] { LaneKind.Roslyn }, roslynLanes.ToArray());
    }

    [TestMethod]
    public void LspLaneRouter_GetOrderedLanes_DefaultsToJazorForUnknownLane()
    {
        var router = new LspLaneRouter();
        var documentPath = @"D:\temp\Counter.jazor";

        var lanes = router.GetOrderedLanes(new ProjectionTarget(
            LaneKind.Jazor,
            DocumentRegionKind.Template,
            documentPath,
            documentPath));

        CollectionAssert.AreEqual(new[] { LaneKind.Jazor }, lanes.ToArray());
    }

    [TestMethod]
    public void LspLaneRouter_GetDiagnosticLanes_UsesDocumentKindSpecificOrdering()
    {
        var router = new LspLaneRouter();

        var jazorLanes = router.GetDiagnosticLanes(new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            "<Counter />",
            "1"));
        var csharpLanes = router.GetDiagnosticLanes(new DocumentSnapshot(
            @"D:\temp\Counter.cs",
            DocumentKind.CSharp,
            "internal sealed class Counter {}",
            "1"));
        var vueLanes = router.GetDiagnosticLanes(new DocumentSnapshot(
            @"D:\temp\Counter.vue",
            DocumentKind.Vue,
            "<template />",
            "1"));

        CollectionAssert.AreEqual(
            new[] { LaneKind.Jazor, LaneKind.Roslyn, LaneKind.Volar },
            jazorLanes.ToArray());
        CollectionAssert.AreEqual(new[] { LaneKind.Roslyn }, csharpLanes.ToArray());
        CollectionAssert.AreEqual(new[] { LaneKind.Volar }, vueLanes.ToArray());
    }

    [TestMethod]
    public void LspLaneRouter_GetSemanticTokenLanes_UsesDocumentKindSpecificOrdering()
    {
        var router = new LspLaneRouter();

        var jazorLanes = router.GetSemanticTokenLanes(new DocumentSnapshot(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            "<Counter />",
            "1"));
        var csharpLanes = router.GetSemanticTokenLanes(new DocumentSnapshot(
            @"D:\temp\Counter.cs",
            DocumentKind.CSharp,
            "internal sealed class Counter {}",
            "1"));
        var vueLanes = router.GetSemanticTokenLanes(new DocumentSnapshot(
            @"D:\temp\Counter.vue",
            DocumentKind.Vue,
            "<template />",
            "1"));
        var unknownLanes = router.GetSemanticTokenLanes(new DocumentSnapshot(
            @"D:\temp\Counter.unknown",
            DocumentKind.Unknown,
            string.Empty,
            "1"));

        CollectionAssert.AreEqual(
            new[] { LaneKind.Volar, LaneKind.Roslyn },
            jazorLanes.ToArray());
        CollectionAssert.AreEqual(new[] { LaneKind.Roslyn }, csharpLanes.ToArray());
        CollectionAssert.AreEqual(new[] { LaneKind.Volar }, vueLanes.ToArray());
        CollectionAssert.AreEqual(new[] { LaneKind.Jazor }, unknownLanes.ToArray());
    }
}
