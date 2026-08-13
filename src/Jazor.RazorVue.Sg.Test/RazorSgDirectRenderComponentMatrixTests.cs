namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderComponentMatrixTests
{
    public static IEnumerable<TestDataRow<DirectRenderCase>> Cases
        => DirectRenderCaseCatalog.SuccessCases
            .Where(static testCase => testCase.Group == DirectRenderCaseGroup.Component)
            .Select(static testCase => new TestDataRow<DirectRenderCase>(testCase)
            {
                DisplayName = "DirectRender_" + testCase.Id
            });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void TryEmit_LowersComponentPropsEventsBindAndSlots(DirectRenderCase testCase)
    {
        var observation = RazorSgDirectRenderMatrixTestHost.Emit(testCase);

        StringAssert.Contains(observation.ArtifactExpression, testCase.ExpectedFragment, StringComparison.Ordinal);
        if (testCase.AdditionalExpectedFragment is not null)
            StringAssert.Contains(observation.ArtifactExpression, testCase.AdditionalExpectedFragment, StringComparison.Ordinal);
        Assert.AreEqual(testCase.UsesFragment, observation.UsesFragment);
        Assert.AreEqual(testCase.UsesStaticVNode, observation.UsesStaticVNode);
        Assert.AreEqual(testCase.UsesProps, observation.UsesProps);
        Assert.AreEqual(testCase.UsesSlots, observation.UsesSlots);
        Assert.AreEqual(testCase.ImportCount, observation.ImportCount);
        if (testCase.ExpectedImportFragment is not null)
            StringAssert.Contains(observation.Imports, testCase.ExpectedImportFragment, StringComparison.Ordinal);
        if (testCase.UnexpectedImportFragment is not null)
            Assert.IsFalse(observation.Imports.Contains(testCase.UnexpectedImportFragment, StringComparison.Ordinal), observation.Imports);
        Assert.IsFalse(observation.ArtifactExpression.Contains("builder.", StringComparison.Ordinal));
    }
}
