using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativeHeaderBarTests
{
    [TestMethod]
    public void Vben_HeaderBar_WithoutTitles_DoesNotRenderEmptyTitlesContainer()
    {
        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(new VbenHeaderBar());

        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__main"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__titles"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__title"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__subtitle"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__actions"));
    }

    [TestMethod]
    public void Vben_HeaderBar_WithoutActionsOrUserRegion_DoesNotRenderEmptyRightRegion()
    {
        var component = new VbenHeaderBar();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenHeaderBar.Title), "Workbench");
        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__actions"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__toolbar"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__user-region"));
    }

    [TestMethod]
    public void Vben_HeaderBar_WithActionsAndUserRegion_RendersDedicatedSemanticWrappers()
    {
        var component = new VbenHeaderBar();
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenHeaderBar.Actions),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "header-action");
                builder.AddContent(2, "Refresh");
                builder.CloseElement();
            }));
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenHeaderBar.UserRegion),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "current-user");
                builder.AddContent(2, "Alice");
                builder.CloseElement();
            }));
        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-header__main"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-header__actions"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-header__toolbar"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-header__user-region"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("button", "header-action"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("span", "current-user"));
    }

    [TestMethod]
    public void Vben_HeaderBar_WithLogoAndTitle_RendersMainRegion()
    {
        var component = new VbenHeaderBar();
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenHeaderBar.Logo),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "brand-logo");
                builder.AddContent(2, "J");
                builder.CloseElement();
            }));
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenHeaderBar.Title), "Workbench");

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-header__main"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-header__logo"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-header__titles"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("span", "brand-logo"));
    }
}
