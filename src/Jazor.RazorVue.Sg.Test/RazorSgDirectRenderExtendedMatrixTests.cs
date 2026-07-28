namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderExtendedMatrixTests
{
    public static IEnumerable<TestDataRow<DirectRenderCase>> Cases
        => DirectRenderCaseCatalog.SuccessCases
            .Where(static testCase => testCase.Group == DirectRenderCaseGroup.Extended)
            .Select(static testCase => new TestDataRow<DirectRenderCase>(testCase)
            {
                DisplayName = "DirectRender_" + testCase.Id
            });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void TryEmit_LowersExtendedSemanticFamiliesToAst(DirectRenderCase testCase)
    {
        var observation = RazorSgDirectRenderMatrixTestHost.Emit(testCase);

        StringAssert.Contains(observation.RenderExpression, testCase.ExpectedFragment, StringComparison.Ordinal);
        if (testCase.AdditionalExpectedFragment is not null)
            StringAssert.Contains(observation.RenderExpression, testCase.AdditionalExpectedFragment, StringComparison.Ordinal);
        Assert.AreEqual(testCase.UsesFragment, observation.UsesFragment);
        Assert.AreEqual(testCase.UsesStaticVNode, observation.UsesStaticVNode);
        Assert.AreEqual(testCase.UsesProps, observation.UsesProps);
        Assert.AreEqual(testCase.UsesSlots, observation.UsesSlots);
        Assert.AreEqual(testCase.ImportCount, observation.ImportCount);
        Assert.IsFalse(observation.RenderExpression.Contains("createRenderContext", StringComparison.Ordinal));
        Assert.IsFalse(observation.RenderExpression.Contains("buildRenderTree", StringComparison.Ordinal));
        Assert.IsFalse(observation.RenderExpression.Contains("builder.", StringComparison.Ordinal));
        Assert.IsFalse(observation.Prelude.Contains("builder.", StringComparison.Ordinal));
    }
}
