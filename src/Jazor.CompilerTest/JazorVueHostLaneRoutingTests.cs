using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostLaneRoutingTests
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
}
