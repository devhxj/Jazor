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
        => RazorSgDirectRenderMatrixAssertions.AssertEmission(testCase);
}

internal static class RazorSgDirectRenderMatrixAssertions
{
    public static void AssertEmission(DirectRenderCase testCase)
    {
        var observation = RazorSgDirectRenderMatrixTestHost.Emit(testCase);
        // Direct emit now has an explicit module-static region. Matrix cases assert the full
        // artifact surface so static prop/VNode hoists do not hide authored literals.
        var emitted = observation.ArtifactExpression;

        AssertExpectedFragment(emitted, testCase.ExpectedFragment);
        if (testCase.AdditionalExpectedFragment is not null)
            AssertExpectedFragment(emitted, testCase.AdditionalExpectedFragment);
        if (testCase.TertiaryExpectedFragment is not null)
            AssertExpectedFragment(emitted, testCase.TertiaryExpectedFragment);
        if (testCase.UnexpectedFragment is not null)
            Assert.IsFalse(emitted.Contains(testCase.UnexpectedFragment, StringComparison.Ordinal), emitted);
        AssertFeatureMetadata(testCase, observation);
        Assert.AreEqual(testCase.UsesProps, observation.UsesProps);
        Assert.AreEqual(testCase.UsesSlots, observation.UsesSlots);
        Assert.AreEqual(testCase.ImportCount, observation.ImportCount);
        Assert.IsFalse(emitted.Contains("buildRenderTree", StringComparison.Ordinal));
        Assert.IsFalse(emitted.Contains("builder.", StringComparison.Ordinal));
    }

    /// <summary>
    /// The catalog predates the direct Vue fast paths and intentionally describes source
    /// semantics, not one frozen VNode spelling. Keep the original phrase when it is still
    /// emitted; otherwise allow only the exact E0/E1/E2 replacement that carries the same
    /// expression/loop payload. Dedicated contract tests pin the conservative fallbacks.
    /// 矩阵验证语义锚点；性能路径的具体 AST 形状由专门契约测试锁定。
    /// </summary>
    public static void AssertExpectedFragment(string emitted, string expected)
    {
        if (emitted.Contains(expected, StringComparison.Ordinal))
            return;

        if (expected.StartsWith("Array.from(", StringComparison.Ordinal))
        {
            var collection = expected.Substring("Array.from(".Length);
            var nullFallback = collection.IndexOf(" ?? []", StringComparison.Ordinal);
            if (nullFallback >= 0)
                collection = collection[..nullFallback];

            if (emitted.Contains("renderList(" + collection + ",", StringComparison.Ordinal))
                return;

            // C# string enumeration is character enumeration. The compiler-owned foreach
            // lowering therefore selects split("") rather than the generic iterable path.
            if (emitted.Contains("renderList(" + collection + ".split(\"\"),", StringComparison.Ordinal))
                return;
        }

        if (expected.StartsWith("h(\"", StringComparison.Ordinal) &&
            emitted.Contains("createElementBlock(" + expected.Substring(2), StringComparison.Ordinal))
        {
            return;
        }

        if (string.Equals(expected, "createStaticVNode", StringComparison.Ordinal) &&
            emitted.Contains("__jazor$createRawMarkup", StringComparison.Ordinal))
        {
            return;
        }

        Assert.Fail("Expected semantic fragment '" + expected + "' or its approved Vue fast-path form.\n" + emitted);
    }

    public static void AssertFeatureMetadata(DirectRenderCase testCase, DirectRenderObservation observation)
    {
        // E2 introduces a fragment for every proven renderList path. Static markup inside a
        // non-hoistable loop/slot scope stays runtime-owned and therefore does not require the
        // createStaticVNode helper even when the source catalog marks the content as static.
        // feature metadata 描述最终 artifact helper，不是 Razor 源码分类。
        Assert.AreEqual(
            testCase.UsesFragment || observation.ArtifactExpression.Contains("renderList(", StringComparison.Ordinal),
            observation.UsesFragment);
        Assert.AreEqual(
            observation.ArtifactExpression.Contains("createStaticVNode", StringComparison.Ordinal),
            observation.UsesStaticVNode);
    }
}
