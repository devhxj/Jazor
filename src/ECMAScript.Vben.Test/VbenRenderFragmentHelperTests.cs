using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenRenderFragmentHelperTests
{
    [TestMethod]
    public void Vben_RenderFragmentHelper_Normalize_RenderableFragment_ReplaysWithoutReinvokingOriginal()
    {
        var evaluations = 0;
        RenderFragment original = builder =>
        {
            evaluations++;
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "captured-fragment");
            builder.AddContent(2, "Visible");
            builder.CloseElement();
        };

        var normalized = InvokeNormalize(original);

        Assert.AreEqual(1, evaluations);
        Assert.IsNotNull(normalized);

        var frames = VbenNativeRenderTreeTestHelper.RenderFragment(normalized!);

        Assert.AreEqual(1, evaluations);
        Assert.IsTrue(frames.ContainsElementWithClassToken("span", "captured-fragment"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "unexpected"));
    }

    [TestMethod]
    public void Vben_RenderFragmentHelper_Normalize_WhitespaceOnlyFragment_ReturnsNullAndEvaluatesOnce()
    {
        var evaluations = 0;
        RenderFragment original = builder =>
        {
            evaluations++;
            builder.AddContent(0, "   ");
        };

        var normalized = InvokeNormalize(original);

        Assert.AreEqual(1, evaluations);
        Assert.IsNull(normalized);
    }

    [TestMethod]
    public void Vben_RenderFragmentHelper_Normalize_CommentOnlyMarkupFragment_ReturnsNullAndEvaluatesOnce()
    {
        var evaluations = 0;
        RenderFragment original = builder =>
        {
            evaluations++;
            builder.AddMarkupContent(0, "  <!-- preserved only for source comments -->  ");
        };

        var normalized = InvokeNormalize(original);

        Assert.AreEqual(1, evaluations);
        Assert.IsNull(normalized);
    }

    private static RenderFragment? InvokeNormalize(RenderFragment? fragment)
    {
        var helperType = typeof(VbenAdminLayout).Assembly.GetType("ECMAScript.Vben.VbenRenderFragmentHelper");
        Assert.IsNotNull(helperType);

        var normalizeMethod = helperType!.GetMethod("Normalize", BindingFlags.Static | BindingFlags.Public);
        Assert.IsNotNull(normalizeMethod);

        return (RenderFragment?)normalizeMethod!.Invoke(null, [fragment]);
    }
}
