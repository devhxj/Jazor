namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueSemanticMatrixInventoryTests
{
    [TestMethod]
    public void SemanticMatrix_Contains406UniqueCasesAcrossOwnedBoundaries()
    {
        Assert.HasCount(304, DirectRenderCaseCatalog.SuccessCases);
        Assert.HasCount(
            192,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Surface));
        Assert.HasCount(
            64,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Component));
        Assert.HasCount(
            48,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.ControlFlow));
        AssertDirectRenderCaseCount(48, "content_");
        AssertDirectRenderCaseCount(32, "markup_");
        AssertDirectRenderCaseCount(48, "element_");
        AssertDirectRenderCaseCount(64, "attribute_");
        AssertDirectRenderCaseCount(32, "component_prop_");
        AssertDirectRenderCaseCount(8, "component_event_");
        AssertDirectRenderCaseCount(8, "component_bind_");
        AssertDirectRenderCaseCount(
            16,
            "component_default_slot_",
            "component_header_slot_",
            "component_item_slot_");
        AssertDirectRenderCaseCount(16, "control_conditional_content_");
        AssertDirectRenderCaseCount(16, "control_conditional_attribute_");
        AssertDirectRenderCaseCount(16, "control_foreach_");
        Assert.HasCount(32, SlotExpressionCase.All);
        Assert.HasCount(40, ComponentCandidateCase.All);
        Assert.HasCount(30, VueInjectCase.All);

        var ids = DirectRenderCaseCatalog.SuccessCases.Select(static item => "render:" + item.Id)
            .Concat(SlotExpressionCase.All.Select(static item => "slot:" + item.Id))
            .Concat(ComponentCandidateCase.All.Select(static item => "candidate:" + item.Id))
            .Concat(VueInjectCase.All.Select(static item => "inject:" + item.Id))
            .ToArray();
        Assert.HasCount(406, ids);
        Assert.HasCount(ids.Length, ids.Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderCaseCatalog.SuccessCases.Count,
            DirectRenderCaseCatalog.SuccessCases.Select(static item => item.TypeName).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderCaseCatalog.SuccessCases.Count,
            DirectRenderCaseCatalog.SuccessCases.Select(static item => item.Body).Distinct(StringComparer.Ordinal));
    }

    private static void AssertDirectRenderCaseCount(int expected, params string[] idPrefixes)
        => Assert.HasCount(
            expected,
            DirectRenderCaseCatalog.SuccessCases.Where(item =>
                idPrefixes.Any(prefix => item.Id.StartsWith(prefix, StringComparison.Ordinal))));
}
