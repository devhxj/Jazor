namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueSemanticMatrixInventoryTests
{
    [TestMethod]
    public void SemanticMatrix_Contains4062UniqueCasesAndCompleteCatalogCoverage()
    {
        Assert.HasCount(3324, DirectRenderCaseCatalog.SuccessCases);
        Assert.HasCount(
            195,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Surface));
        Assert.HasCount(
            66,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Component));
        Assert.HasCount(
            49,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.ControlFlow));
        Assert.HasCount(
            449,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Extended));
        Assert.HasCount(
            1025,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Advanced));
        Assert.HasCount(
            1540,
            DirectRenderCaseCatalog.SuccessCases.Where(static item => item.Group == DirectRenderCaseGroup.Coverage));
        AssertDirectRenderCaseCount(48, "content_");
        AssertDirectRenderCaseCount(33, "markup_");
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
        AssertDirectRenderCaseCount(1, "control_single_statement_conditional_attribute");
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
        AssertDirectRenderCaseCount(1, "render_fragment_method_group_component_slot");
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
        AssertDirectRenderCaseCount(4, "coverage_emitter_");
        Assert.HasCount(32, SlotExpressionCase.All);
        Assert.HasCount(40, ComponentCandidateCase.All);
        Assert.HasCount(30, VueInjectCase.All);
        Assert.HasCount(636, DirectRenderFailureCaseCatalog.All);
        var baselineFailureCases = DirectRenderFailureCaseCatalog.All
            .Where(static testCase => testCase.Scenario is null)
            .ToArray();
        Assert.HasCount(124, baselineFailureCases);
        var failureFamilies = baselineFailureCases
            .GroupBy(static item => item.Id[..item.Id.LastIndexOf('_')], StringComparer.Ordinal)
            .ToArray();
        Assert.HasCount(31, failureFamilies);
        Assert.IsTrue(failureFamilies.All(static family => family.Count() == 4));
        Assert.HasCount(
            512,
            DirectRenderFailureCaseCatalog.All.Where(static testCase => testCase.Scenario is not null));

        var ids = DirectRenderCaseCatalog.SuccessCases.Select(static item => "render:" + item.Id)
            .Concat(SlotExpressionCase.All.Select(static item => "slot:" + item.Id))
            .Concat(ComponentCandidateCase.All.Select(static item => "candidate:" + item.Id))
            .Concat(VueInjectCase.All.Select(static item => "inject:" + item.Id))
            .Concat(DirectRenderFailureCaseCatalog.All.Select(static item => "failure:" + item.Id))
            .ToArray();
        Assert.HasCount(4062, ids);
        Assert.HasCount(ids.Length, ids.Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderCaseCatalog.SuccessCases.Count,
            DirectRenderCaseCatalog.SuccessCases.Select(static item => item.TypeName).Distinct(StringComparer.Ordinal));
        var duplicateSuccessBodies = DirectRenderCaseCatalog.SuccessCases
            .GroupBy(static item => item.Body, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => string.Join(", ", group.Select(static item => item.Id)) + ": " + group.Key)
            .ToArray();
        Assert.IsEmpty(duplicateSuccessBodies, string.Join(Environment.NewLine, duplicateSuccessBodies));
        Assert.HasCount(
            DirectRenderFailureCaseCatalog.All.Count,
            DirectRenderFailureCaseCatalog.All.Select(static item => item.TypeName).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            DirectRenderFailureCaseCatalog.All.Count,
            DirectRenderFailureCaseCatalog.All
                .Select(static item => item.Body + "\n" + item.Members)
                .Distinct(StringComparer.Ordinal));

        AssertUsageScenarioCoverage();
    }

    private static void AssertUsageScenarioCoverage()
    {
        var definitions = RazorVueUsageScenarioCatalog.All;
        Assert.HasCount(256, definitions);
        Assert.HasCount(
            definitions.Count,
            definitions.Select(static definition => definition.Id).Distinct());

        var definitionById = definitions.ToDictionary(static definition => definition.Id);
        var successfulObservations = DirectRenderCaseCatalog.SuccessCases
            .Where(static testCase => testCase.Scenario is not null)
            .Select(static testCase => (
                Scenario: testCase.Scenario!.Value,
                testCase.Body,
                Expectation: RazorVueUsageScenarioExpectation.Emission))
            .ToArray();
        Assert.HasCount(1536, successfulObservations);
        Assert.IsTrue(DirectRenderCaseCatalog.SuccessCases
            .Where(static testCase => testCase.Scenario is not null)
            .All(static testCase => testCase.Group == DirectRenderCaseGroup.Coverage));
        var diagnosticObservations = DirectRenderFailureCaseCatalog.All
            .Where(static testCase => testCase.Scenario is not null)
            .Select(static testCase => (
                Scenario: testCase.Scenario!.Value,
                testCase.Body,
                Expectation: RazorVueUsageScenarioExpectation.Diagnostic))
            .ToArray();
        Assert.HasCount(512, diagnosticObservations);

        var observed = successfulObservations.Concat(diagnosticObservations).ToArray();
        Assert.HasCount(2048, observed);
        Assert.IsTrue(observed.All(item => definitionById.ContainsKey(item.Scenario)));
        Assert.IsTrue(observed.All(item => definitionById[item.Scenario].Expectation == item.Expectation));

        var observedByScenario = observed
            .GroupBy(static testCase => testCase.Scenario)
            .ToDictionary(static group => group.Key, static group => group.ToArray());
        Assert.IsTrue(definitions.All(definition => observedByScenario.ContainsKey(definition.Id)));
        Assert.IsTrue(observedByScenario.Values.All(static cases => cases.Length == 8));
        Assert.IsTrue(observedByScenario.Values.All(static cases =>
            cases.Select(static testCase => testCase.Body).Distinct(StringComparer.Ordinal).Count() == 8));

        var coveredDefinitions = definitions.Count(definition => observedByScenario.ContainsKey(definition.Id));
        var coverage = (double)coveredDefinitions / definitions.Count;
        Assert.IsGreaterThanOrEqualTo(0.95, coverage);
        Assert.AreEqual(1d, coverage);
    }

    private static void AssertDirectRenderCaseCount(int expected, params string[] idPrefixes)
        => Assert.HasCount(
            expected,
            DirectRenderCaseCatalog.SuccessCases.Where(item =>
                idPrefixes.Any(prefix => item.Id.StartsWith(prefix, StringComparison.Ordinal))));
}
