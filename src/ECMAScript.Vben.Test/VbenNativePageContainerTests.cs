using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativePageContainerTests
{
    [TestMethod]
    public void Vben_PageContainer_WithoutHeaderContent_DoesNotRenderEmptyHeaderOrTitlesContainers()
    {
        var frames = RenderPageContainer(new VbenPageContainer());

        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-page__header"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-page__titles"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-page__actions"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-page__body"));
    }

    [TestMethod]
    public void Vben_PageContainer_WithOnlyExtra_RendersActionsRegionWithoutEmptyTitlesContainer()
    {
        var component = new VbenPageContainer();
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenPageContainer.Extra),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "page-extra");
                builder.AddContent(2, "Refresh");
                builder.CloseElement();
            }));

        var frames = RenderPageContainer(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-page__header"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("div", "vben-page__titles"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-page__actions"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("button", "page-extra"));
    }

    [TestMethod]
    public void Vben_PageContainer_BreadcrumbHref_RendersNavigableAnchor()
    {
        var breadcrumb = new VbenBreadcrumbItem
        {
            Key = "dashboard",
            Title = "Dashboard",
            Target = "/dashboard"
        };

        var frames = RenderBreadcrumbItem(breadcrumb);

        Assert.IsTrue(frames.ContainsElement("a"));
        Assert.IsTrue(frames.ContainsAttribute("href", "/dashboard"));
        Assert.IsFalse(frames.ContainsAttribute("aria-disabled"));
    }

    [TestMethod]
    public void Vben_PageContainer_BreadcrumbRouteHash_RendersResolvedHref()
    {
        var breadcrumb = new VbenBreadcrumbItem
        {
            Key = "logs",
            Title = "Logs",
            Target = new VbenRouteLocation
            {
                Path = "/ops",
                Hash = "logs"
            }
        };

        var frames = RenderBreadcrumbItem(breadcrumb);

        Assert.IsTrue(frames.ContainsElement("a"));
        Assert.IsTrue(frames.ContainsAttribute("href", "/ops#logs"));
    }

    [TestMethod]
    public void Vben_PageContainer_DisabledBreadcrumb_DoesNotRenderNavigableAnchor()
    {
        var breadcrumb = new VbenBreadcrumbItem
        {
            Key = "reports",
            Title = "Reports",
            Disabled = true,
            Target = "/reports"
        };

        var frames = RenderBreadcrumbItem(breadcrumb);

        Assert.IsFalse(frames.ContainsElement("a"));
        Assert.IsTrue(frames.ContainsElement("span"));
        Assert.IsTrue(frames.ContainsAttribute("aria-disabled"));
        Assert.IsFalse(frames.ContainsAttribute("href", "/reports"));
    }

    [TestMethod]
    public void Vben_PageContainer_ActionHref_RendersNavigableAnchor()
    {
        var action = new VbenPageAction
        {
            Key = "create",
            Text = "Create",
            Kind = VbenPageActionKind.Primary,
            Target = "/create"
        };

        var frames = RenderAction(action);

        Assert.IsTrue(frames.ContainsElement("a"));
        Assert.IsTrue(frames.ContainsAttribute("href", "/create"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action--primary"));
    }

    [TestMethod]
    public void Vben_PageContainer_ActionRouteHash_RendersResolvedHref()
    {
        var action = new VbenPageAction
        {
            Key = "preview",
            Text = "Preview",
            Kind = VbenPageActionKind.Link,
            Target = new VbenRouteLocation
            {
                Hash = "#preview"
            }
        };

        var frames = RenderAction(action);

        Assert.IsTrue(frames.ContainsElement("a"));
        Assert.IsTrue(frames.ContainsAttribute("href", "#preview"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action--link"));
    }

    [TestMethod]
    public void Vben_PageContainer_DisabledActionWithTarget_DoesNotRenderNavigableAnchor()
    {
        var action = new VbenPageAction
        {
            Key = "danger",
            Text = "Delete",
            Kind = VbenPageActionKind.Danger,
            Disabled = true,
            Target = "/danger"
        };

        var frames = RenderAction(action);

        Assert.IsFalse(frames.ContainsElement("a"));
        Assert.IsTrue(frames.ContainsElement("button"));
        Assert.IsTrue(frames.ContainsAttribute("disabled"));
        Assert.IsTrue(frames.ContainsAttribute("aria-disabled"));
        Assert.IsFalse(frames.ContainsAttribute("href", "/danger"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action--danger"));
        Assert.IsTrue(frames.ContainsClassToken("is-disabled"));
    }

    [TestMethod]
    public void Vben_PageContainer_NullBreadcrumbAndActionEntries_AreIgnoredDuringRender()
    {
        var component = new VbenPageContainer();
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenPageContainer.BreadcrumbItems),
            new VbenBreadcrumbItem[]
            {
                null!,
                new()
                {
                    Key = "dashboard",
                    Title = "Dashboard",
                    Target = "/dashboard"
                }
            });
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenPageContainer.Actions),
            new VbenPageAction[]
            {
                null!,
                new()
                {
                    Key = "create",
                    Text = "Create",
                    Kind = VbenPageActionKind.Primary,
                    Target = "/create"
                }
            });

        var frames = RenderPageContainer(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("nav", "vben-page__breadcrumb"));
        Assert.IsTrue(frames.ContainsAttribute("href", "/dashboard"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-page__actions"));
        Assert.IsTrue(frames.ContainsAttribute("href", "/create"));
        Assert.IsTrue(frames.ContainsClassToken("vben-page__action--primary"));
    }

    private static NativeRenderTreeSnapshot RenderPageContainer(VbenPageContainer component)
        => VbenNativeRenderTreeTestHelper.RenderComponent(component);

    private static NativeRenderTreeSnapshot RenderBreadcrumbItem(VbenBreadcrumbItem item)
    {
        return VbenNativeRenderTreeTestHelper.RenderFragmentFromInstanceMethod(
            new VbenPageContainer(),
            "RenderBreadcrumbItem",
            item);
    }

    private static NativeRenderTreeSnapshot RenderAction(VbenPageAction action)
    {
        return VbenNativeRenderTreeTestHelper.RenderFragmentFromInstanceMethod(
            new VbenPageContainer(),
            "RenderAction",
            action);
    }
}
