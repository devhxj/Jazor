namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderAdvancedMatrixTests
{
    public static IEnumerable<TestDataRow<DirectRenderCase>> Cases
        => DirectRenderCaseCatalog.SuccessCases
            .Where(static testCase => testCase.Group == DirectRenderCaseGroup.Advanced)
            .Select(static testCase => new TestDataRow<DirectRenderCase>(testCase)
            {
                DisplayName = "DirectRender_" + testCase.Id
            });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void TryEmit_LowersAdvancedSemanticFamiliesToAst(DirectRenderCase testCase)
    {
        var observation = RazorSgDirectRenderMatrixTestHost.Emit(testCase);
        var emitted = observation.Prelude + "\n" + observation.RenderExpression;

        StringAssert.Contains(emitted, testCase.ExpectedFragment, StringComparison.Ordinal);
        if (testCase.AdditionalExpectedFragment is not null)
            StringAssert.Contains(emitted, testCase.AdditionalExpectedFragment, StringComparison.Ordinal);
        if (testCase.UnexpectedFragment is not null)
            Assert.IsFalse(emitted.Contains(testCase.UnexpectedFragment, StringComparison.Ordinal), emitted);
        Assert.AreEqual(testCase.UsesFragment, observation.UsesFragment);
        Assert.AreEqual(testCase.UsesStaticVNode, observation.UsesStaticVNode);
        Assert.AreEqual(testCase.UsesProps, observation.UsesProps);
        Assert.AreEqual(testCase.UsesSlots, observation.UsesSlots);
        Assert.AreEqual(testCase.ImportCount, observation.ImportCount);
        Assert.IsFalse(emitted.Contains("createRenderContext", StringComparison.Ordinal));
        Assert.IsFalse(emitted.Contains("buildRenderTree", StringComparison.Ordinal));
        Assert.IsFalse(emitted.Contains("builder.", StringComparison.Ordinal));
    }
}
