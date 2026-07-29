namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderCoverageMatrixTests
{
    public static IEnumerable<TestDataRow<DirectRenderCase>> Cases
        => DirectRenderCaseCatalog.SuccessCases
            .Where(static testCase => testCase.Group == DirectRenderCaseGroup.Coverage)
            .Select(static testCase => new TestDataRow<DirectRenderCase>(testCase)
            {
                DisplayName = "DirectRender_" + testCase.Id
            });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void TryEmit_CoversCataloguedUsageScenario(DirectRenderCase testCase)
        => RazorSgDirectRenderMatrixAssertions.AssertEmission(testCase);
}
