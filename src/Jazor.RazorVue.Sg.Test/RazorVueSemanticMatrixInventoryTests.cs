namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueSemanticMatrixInventoryTests
{
    [TestMethod]
    public void SemanticMatrix_Contains1942UniqueCasesAcrossOwnedBoundaries()
    {
        Assert.HasCount(1776, DirectRenderCaseCatalog.SuccessCases);
        Assert.HasCount(
            192,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Surface));
        Assert.HasCount(
            64,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Component));
        Assert.HasCount(
            48,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.ControlFlow));
        Assert.HasCount(
            448,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Extended));
        Assert.HasCount(
            1024,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Advanced));
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
        AssertDirectRenderCaseCount(64, "expression_content_");
        AssertDirectRenderCaseCount(64, "dynamic_attribute_");
        AssertDirectRenderCaseCount(64, "tree_composition_");
        AssertDirectRenderCaseCount(64, "splat_key_");
        AssertDirectRenderCaseCount(64, "dom_event_");
        AssertDirectRenderCaseCount(64, "reference_capture_");
        AssertDirectRenderCaseCount(64, "structured_component_");
        AssertDirectRenderCaseCount(64, "advanced_region_");
        AssertDirectRenderCaseCount(64, "advanced_roots_");
        AssertDirectRenderCaseCount(64, "advanced_local_");
        AssertDirectRenderCaseCount(64, "advanced_helper_");
        AssertDirectRenderCaseCount(64, "advanced_fragment_");
        AssertDirectRenderCaseCount(64, "advanced_generic_fragment_");
        AssertDirectRenderCaseCount(64, "advanced_conditional_element_");
        AssertDirectRenderCaseCount(64, "advanced_conditional_component_");
        AssertDirectRenderCaseCount(64, "advanced_nested_control_");
        AssertDirectRenderCaseCount(64, "advanced_attribute_collection_");
        AssertDirectRenderCaseCount(64, "advanced_event_metadata_");
        AssertDirectRenderCaseCount(64, "advanced_slot_composition_");
        AssertDirectRenderCaseCount(64, "advanced_component_expression_");
        AssertDirectRenderCaseCount(64, "advanced_mixed_static_");
        AssertDirectRenderCaseCount(64, "advanced_return_guard_");
        AssertDirectRenderCaseCount(64, "advanced_builder_mutation_");
        Assert.HasCount(32, SlotExpressionCase.All);
        Assert.HasCount(40, ComponentCandidateCase.All);
        Assert.HasCount(30, VueInjectCase.All);
        Assert.HasCount(64, DirectRenderFailureCaseCatalog.All);
        var failureFamilies = DirectRenderFailureCaseCatalog.All
            .GroupBy(static item => item.Id[..item.Id.LastIndexOf('_')], StringComparer.Ordinal)
            .ToArray();
        Assert.HasCount(16, failureFamilies);
        Assert.IsTrue(failureFamilies.All(static family => family.Count() == 4));

        var ids = DirectRenderCaseCatalog.SuccessCases.Select(static item => "render:" + item.Id)
            .Concat(SlotExpressionCase.All.Select(static item => "slot:" + item.Id))
            .Concat(ComponentCandidateCase.All.Select(static item => "candidate:" + item.Id))
            .Concat(VueInjectCase.All.Select(static item => "inject:" + item.Id))
            .Concat(DirectRenderFailureCaseCatalog.All.Select(static item => "failure:" + item.Id))
            .ToArray();
        Assert.HasCount(1942, ids);
        Assert.HasCount(ids.Length, ids.Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderCaseCatalog.SuccessCases.Count,
            DirectRenderCaseCatalog.SuccessCases.Select(static item => item.TypeName).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderCaseCatalog.SuccessCases.Count,
            DirectRenderCaseCatalog.SuccessCases.Select(static item => item.Body).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderFailureCaseCatalog.All.Count,
            DirectRenderFailureCaseCatalog.All.Select(static item => item.TypeName).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderFailureCaseCatalog.All.Count,
            DirectRenderFailureCaseCatalog.All
                .Select(static item => item.Body + "\n" + item.Members)
                .Distinct(StringComparer.Ordinal));
    }

    private static void AssertDirectRenderCaseCount(int expected, params string[] idPrefixes)
        => Assert.HasCount(
            expected,
            DirectRenderCaseCatalog.SuccessCases.Where(item =>
                idPrefixes.Any(prefix => item.Id.StartsWith(prefix, StringComparison.Ordinal))));
}
