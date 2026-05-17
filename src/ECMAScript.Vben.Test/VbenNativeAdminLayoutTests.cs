using ECMAScript.Vben;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed class VbenNativeAdminLayoutTests
{
    [TestMethod]
    public void Vben_AdminLayout_TopMode_DoesNotRenderSidebarRegionOrDefaultSidebarMenu()
    {
        VbenNavItems navItems =
        [
            new VbenNavItem
            {
                Key = "dashboard",
                Title = "Dashboard"
            }
        ];

        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Top);
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.NavItems), navItems);
        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsFalse(frames.ContainsElementWithClassToken("aside", "vben-shell__sidebar"));
        Assert.IsFalse(frames.ContainsComponent<VbenSidebarMenu>());
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-shell__main"));
        Assert.IsFalse(frames.ContainsElementWithClassToken("header", "vben-shell__header"));
        Assert.IsFalse(frames.ContainsComponent<VbenHeaderBar>());
        Assert.IsTrue(frames.ContainsElementWithClassToken("main", "vben-shell__content"));
    }

    [TestMethod]
    public void Vben_AdminLayout_SidebarMode_RendersDefaultSidebarMenuWhenNavItemsExist()
    {
        VbenNavItems navItems =
        [
            new VbenNavItem
            {
                Key = "dashboard",
                Title = "Dashboard"
            }
        ];

        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Sidebar);
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.NavItems), navItems);
        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("aside", "vben-shell__sidebar"));
        Assert.IsTrue(frames.ContainsComponent<VbenSidebarMenu>());
    }

    [TestMethod]
    public void Vben_AdminLayout_SidebarMode_WithoutEffectiveSidebarContent_DoesNotRenderEmptySidebarRegion()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Sidebar);
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.NavItems), new VbenNavItems([null!]));

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsFalse(frames.ContainsElementWithClassToken("aside", "vben-shell__sidebar"));
        Assert.IsFalse(frames.ContainsComponent<VbenSidebarMenu>());
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-shell__main"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("main", "vben-shell__content"));
    }

    [TestMethod]
    public void Vben_AdminLayout_SidebarMode_WhitespaceOnlyTitleNavItems_DoNotRenderEmptySidebarRegion()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Sidebar);
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenAdminLayout.NavItems),
            new VbenNavItems(
            [
                new VbenNavItem
                {
                    Key = "empty",
                    Title = "   ",
                    Target = "/ignored"
                }
            ]));

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsFalse(frames.ContainsElementWithClassToken("aside", "vben-shell__sidebar"));
        Assert.IsFalse(frames.ContainsComponent<VbenSidebarMenu>());
        Assert.IsFalse(frames.ContainsAttribute("href", "/ignored"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("div", "vben-shell__main"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("main", "vben-shell__content"));
    }

    [TestMethod]
    public void Vben_AdminLayout_SidebarMode_WithLogoOnly_RendersSidebarRegionAndDefaultSidebarMenu()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Sidebar);
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenAdminLayout.Logo),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "layout-logo");
                builder.AddContent(2, "J");
                builder.CloseElement();
            }));

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("aside", "vben-shell__sidebar"));
        Assert.IsTrue(frames.ContainsComponent<VbenSidebarMenu>());
    }

    [TestMethod]
    public void Vben_AdminLayout_SidebarMode_WithLogoOnly_DoesNotRenderDefaultHeaderRegion()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Sidebar);
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenAdminLayout.Logo),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "layout-logo");
                builder.AddContent(2, "J");
                builder.CloseElement();
            }));

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsFalse(frames.ContainsElementWithClassToken("header", "vben-shell__header"));
        Assert.IsFalse(frames.ContainsComponent<VbenHeaderBar>());
    }

    [TestMethod]
    public void Vben_AdminLayout_MixedMode_PreservesSidebarRegion()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Mixed);
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenAdminLayout.Sidebar),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "section");
                builder.AddAttribute(1, "class", "custom-sidebar");
                builder.AddContent(2, "Custom sidebar");
                builder.CloseElement();
            }));
        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("aside", "vben-shell__sidebar"));
        Assert.IsTrue(frames.ContainsElementWithClassToken("section", "custom-sidebar"));
    }

    [TestMethod]
    public void Vben_AdminLayout_DefaultHeaderContent_RendersHeaderRegionAndDefaultHeaderBar()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Top);
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Title), "Workbench");

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("header", "vben-shell__header"));
        Assert.IsTrue(frames.ContainsComponent<VbenHeaderBar>());
    }

    [TestMethod]
    public void Vben_AdminLayout_CustomHeader_RendersHeaderRegionWithoutDefaultHeaderBar()
    {
        var component = new VbenAdminLayout();
        VbenNativeRenderTreeTestHelper.SetParameter(component, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Top);
        VbenNativeRenderTreeTestHelper.SetParameter(
            component,
            nameof(VbenAdminLayout.Header),
            (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "section");
                builder.AddAttribute(1, "class", "custom-header");
                builder.AddContent(2, "Header");
                builder.CloseElement();
            }));

        var frames = VbenNativeRenderTreeTestHelper.RenderComponent(component);

        Assert.IsTrue(frames.ContainsElementWithClassToken("header", "vben-shell__header"));
        Assert.IsFalse(frames.ContainsComponent<VbenHeaderBar>());
        Assert.IsTrue(frames.ContainsElementWithClassToken("section", "custom-header"));
    }
}
