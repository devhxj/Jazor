using System.Collections.Immutable;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.CSharp;

namespace ECMAScript.Vben.Test;

[TestClass]
public sealed partial class VbenContainerInjectTests
{
    [TestMethod]
    public void Vben_NativeShellComponents_AreDeclaredAsContainerContracts()
    {
        Assert.IsTrue(typeof(IVueContainerComponent).IsAssignableFrom(typeof(VbenAdminLayout)));
        Assert.IsTrue(typeof(IVueContainerComponent).IsAssignableFrom(typeof(VbenSidebarMenu)));
        Assert.IsTrue(typeof(IVueContainerComponent).IsAssignableFrom(typeof(VbenHeaderBar)));
        Assert.IsTrue(typeof(IVueContainerComponent).IsAssignableFrom(typeof(VbenPageContainer)));
    }

    [TestMethod]
    public void Vben_PageContainer_DefaultComponentRegistryResolution_UsesNativeImplementation()
    {
        var context = CreateContext(
            """
            using ECMAScript;
            using ECMAScript.Vben;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.AddComponentParameter(1, nameof(VbenPageContainer.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(VbenPageContainer.Subtitle), "Shell");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "DashboardPage");
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var resolvedComponents = RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);
        var descriptor = resolvedComponents[nameof(VbenPageContainer)];

        Assert.AreEqual("ECMAScript.Vben.VbenPageContainer", descriptor.FullName);
        Assert.AreEqual(VueComponentSourceKind.UserComponent, descriptor.SourceKind);
        Assert.AreEqual("./components/vben-page-container.mjs", descriptor.ImportSpecifier);
        Assert.AreEqual("default", descriptor.ExportName);
        Assert.AreEqual("ECMAScript.Vben.VbenPageContainer", descriptor.ContainerContractFullName);

        var titleProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenPageContainer.Title));
        Assert.AreEqual("title", titleProp.Name);
        Assert.IsFalse(titleProp.CaptureUnmatchedValues);

        var additionalAttributesProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenComponentBase.AdditionalAttributes));
        Assert.AreEqual("additionalAttributes", additionalAttributesProp.Name);
        Assert.IsTrue(additionalAttributesProp.CaptureUnmatchedValues);

        var extraSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenPageContainer.Extra));
        Assert.AreEqual("extra", extraSlot.Name);
        Assert.IsFalse(extraSlot.IsDefault);

        var childContentSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenContentComponentBase.ChildContent));
        Assert.AreEqual("default", childContentSlot.Name);
        Assert.IsTrue(childContentSlot.IsDefault);
    }

    [TestMethod]
    public void Vben_PageContainer_ContainerInject_UsesConfiguredLibraryImplementationWhileKeepingVbenContract()
    {
        var descriptor = ResolveComponentDescriptor(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenPageContainer),
                typeof(Demo.Implementations.ElementPageContainer))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElCard")]
                [VueLibraryStyle("element-plus/theme-chalk/el-card.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(VbenPageContainer.Title), Name = "header")]
                [VueProp(nameof(VbenPageContainer.Subtitle), Name = "subtitle-text")]
                [VueProp(nameof(VbenPageContainer.BreadcrumbItems), Name = "breadcrumbs")]
                [VueProp(nameof(VbenPageContainer.Actions), Name = "page-actions")]
                [VueProp(nameof(VbenComponentBase.CssClass), Name = "class")]
                [VueProp(nameof(VbenComponentBase.CssStyle), Name = "style")]
                [VueProp(nameof(VbenComponentBase.AdditionalAttributes), Name = "attrs")]
                [VueSlot(nameof(VbenPageContainer.Extra), Name = "header-extra")]
                [VueSlot(nameof(VbenContentComponentBase.ChildContent), Name = "default", IsDefault = true)]
                public sealed class ElementPageContainer : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenPageContainer>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    [Parameter]
                    public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }

                    [Parameter]
                    public VbenPageAction[]? Actions { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.AddComponentParameter(1, nameof(VbenPageContainer.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(VbenPageContainer.Subtitle), "Shell");
                        builder.CloseComponent();
                    }
                }
            }
            """);

        Assert.AreEqual("ECMAScript.Vben.VbenPageContainer", descriptor.FullName);
        Assert.AreEqual(nameof(VbenPageContainer), descriptor.Name);
        Assert.AreEqual("ECMAScript.Vben", descriptor.ResolutionNamespace);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, descriptor.SourceKind);
        Assert.AreEqual("element-plus", descriptor.ImportSpecifier);
        Assert.AreEqual("ElCard", descriptor.ExportName);
        Assert.AreEqual("ECMAScript.Vben.VbenPageContainer", descriptor.ContainerContractFullName);
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-card.css" }, descriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, descriptor.PluginRequirements.ToArray());

        var titleProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenPageContainer.Title));
        Assert.AreEqual("header", titleProp.Name);
        Assert.AreEqual("string?", titleProp.TypeName);

        var subtitleProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenPageContainer.Subtitle));
        Assert.AreEqual("subtitle-text", subtitleProp.Name);

        var additionalAttributesProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenComponentBase.AdditionalAttributes));
        Assert.AreEqual("attrs", additionalAttributesProp.Name);
        Assert.IsTrue(additionalAttributesProp.CaptureUnmatchedValues);

        var extraSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenPageContainer.Extra));
        Assert.AreEqual("header-extra", extraSlot.Name);

        var childContentSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenContentComponentBase.ChildContent));
        Assert.AreEqual("default", childContentSlot.Name);
        Assert.IsTrue(childContentSlot.IsDefault);
    }

    [TestMethod]
    public void Vben_AdminLayout_ContainerInject_UsesConfiguredLibraryImplementationWithModelPropsEmitsAndSlots()
    {
        var descriptor = ResolveComponentDescriptor(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenAdminLayout),
                typeof(Demo.Implementations.ElementAdminLayout))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElContainer")]
                [VueLibraryStyle("element-plus/theme-chalk/el-container.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(VbenAdminLayout.Mode), Name = "layout-mode")]
                [VueProp(nameof(VbenAdminLayout.Collapsed), VuePropKind.Model, Name = "menuCollapsed", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(VbenAdminLayout.CollapsedChanged), VueEmitKind.ModelUpdate, Name = "update:menuCollapsed", PayloadTypeName = "System.Boolean")]
                [VueProp(nameof(VbenAdminLayout.SelectedKey), VuePropKind.Model, Name = "activeMenu", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(VbenAdminLayout.SelectedKeyChanged), VueEmitKind.ModelUpdate, Name = "update:activeMenu", PayloadTypeName = "System.String")]
                [VueProp(nameof(VbenAdminLayout.ExpandedKeys), VuePropKind.Model, Name = "openMenus", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(VbenAdminLayout.ExpandedKeysChanged), VueEmitKind.ModelUpdate, Name = "update:openMenus", PayloadTypeName = "System.String[]")]
                [VueProp(nameof(VbenAdminLayout.NavItems), Name = "nav-items")]
                [VueProp(nameof(VbenAdminLayout.Title), Name = "brand-title")]
                [VueProp(nameof(VbenAdminLayout.Subtitle), Name = "brand-subtitle")]
                [VueProp(nameof(VbenComponentBase.CssClass), Name = "class")]
                [VueProp(nameof(VbenComponentBase.CssStyle), Name = "style")]
                [VueProp(nameof(VbenComponentBase.AdditionalAttributes), Name = "attrs")]
                [VueSlot(nameof(VbenAdminLayout.Logo), Name = "logo")]
                [VueSlot(nameof(VbenAdminLayout.Header), Name = "header")]
                [VueSlot(nameof(VbenAdminLayout.Sidebar), Name = "sidebar")]
                [VueSlot(nameof(VbenAdminLayout.HeaderActions), Name = "header-actions")]
                [VueSlot(nameof(VbenAdminLayout.UserRegion), Name = "user-region")]
                [VueSlot(nameof(VbenContentComponentBase.ChildContent), Name = "default", IsDefault = true)]
                public sealed class ElementAdminLayout : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenAdminLayout>
                {
                    [Parameter]
                    public VbenLayoutMode Mode { get; set; }

                    [Parameter]
                    public bool Collapsed { get; set; }

                    [Parameter]
                    public EventCallback<bool> CollapsedChanged { get; set; }

                    [Parameter]
                    public string? SelectedKey { get; set; }

                    [Parameter]
                    public EventCallback<string> SelectedKeyChanged { get; set; }

                    [Parameter]
                    public string[]? ExpandedKeys { get; set; }

                    [Parameter]
                    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

                    [Parameter]
                    public VbenNavItems? NavItems { get; set; }

                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Logo { get; set; }

                    [Parameter]
                    public RenderFragment? Header { get; set; }

                    [Parameter]
                    public RenderFragment? Sidebar { get; set; }

                    [Parameter]
                    public RenderFragment? HeaderActions { get; set; }

                    [Parameter]
                    public RenderFragment? UserRegion { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/layout-page")]
                public sealed class LayoutPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenAdminLayout));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "LayoutPage",
            componentName: nameof(VbenAdminLayout));

        Assert.AreEqual("ECMAScript.Vben.VbenAdminLayout", descriptor.FullName);
        Assert.AreEqual(nameof(VbenAdminLayout), descriptor.Name);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, descriptor.SourceKind);
        Assert.AreEqual("element-plus", descriptor.ImportSpecifier);
        Assert.AreEqual("ElContainer", descriptor.ExportName);
        Assert.AreEqual("ECMAScript.Vben.VbenAdminLayout", descriptor.ContainerContractFullName);
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-container.css" }, descriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, descriptor.PluginRequirements.ToArray());

        var collapsedProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenAdminLayout.Collapsed));
        Assert.AreEqual("menuCollapsed", collapsedProp.Name);
        Assert.AreEqual(VuePropKind.Model, collapsedProp.Kind);
        Assert.IsTrue(collapsedProp.AcceptsBinding);

        var selectedKeyProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenAdminLayout.SelectedKey));
        Assert.AreEqual("activeMenu", selectedKeyProp.Name);
        Assert.AreEqual(VuePropKind.Model, selectedKeyProp.Kind);
        Assert.IsTrue(selectedKeyProp.AcceptsBinding);

        var additionalAttributesProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenComponentBase.AdditionalAttributes));
        Assert.AreEqual("attrs", additionalAttributesProp.Name);
        Assert.IsTrue(additionalAttributesProp.CaptureUnmatchedValues);

        var collapsedChangedEmit = descriptor.Emits.Single(static item => item.RazorAlias == nameof(VbenAdminLayout.CollapsedChanged));
        Assert.AreEqual("update:menuCollapsed", collapsedChangedEmit.Name);
        Assert.AreEqual("bool", collapsedChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, collapsedChangedEmit.Kind);

        var selectedKeyChangedEmit = descriptor.Emits.Single(static item => item.RazorAlias == nameof(VbenAdminLayout.SelectedKeyChanged));
        Assert.AreEqual("update:activeMenu", selectedKeyChangedEmit.Name);
        Assert.AreEqual("string", selectedKeyChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, selectedKeyChangedEmit.Kind);

        var expandedKeysChangedEmit = descriptor.Emits.Single(static item => item.RazorAlias == nameof(VbenAdminLayout.ExpandedKeysChanged));
        Assert.AreEqual("update:openMenus", expandedKeysChangedEmit.Name);
        Assert.AreEqual("string[]", expandedKeysChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, expandedKeysChangedEmit.Kind);

        var headerActionsSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenAdminLayout.HeaderActions));
        Assert.AreEqual("header-actions", headerActionsSlot.Name);

        var childContentSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenContentComponentBase.ChildContent));
        Assert.AreEqual("default", childContentSlot.Name);
        Assert.IsTrue(childContentSlot.IsDefault);
    }

    [TestMethod]
    public void Vben_HeaderBar_ContainerInject_UsesConfiguredLibraryImplementationWhileKeepingSlotContract()
    {
        var descriptor = ResolveComponentDescriptor(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenHeaderBar),
                typeof(Demo.Implementations.LibraryHeaderBar))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("demo-shell/components", "AppHeader")]
                [VueLibraryStyle("demo-shell/styles/header.css")]
                [VueLibraryPluginRequirement("demo-shell")]
                [VueProp(nameof(VbenHeaderBar.Title), Name = "title-text")]
                [VueProp(nameof(VbenHeaderBar.Subtitle), Name = "subtitle-text")]
                [VueProp(nameof(VbenComponentBase.CssClass), Name = "class")]
                [VueProp(nameof(VbenComponentBase.CssStyle), Name = "style")]
                [VueProp(nameof(VbenComponentBase.AdditionalAttributes), Name = "attrs")]
                [VueSlot(nameof(VbenHeaderBar.Logo), Name = "brand")]
                [VueSlot(nameof(VbenHeaderBar.Actions), Name = "toolbar")]
                [VueSlot(nameof(VbenHeaderBar.UserRegion), Name = "user-menu")]
                public sealed class LibraryHeaderBar : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenHeaderBar>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Logo { get; set; }

                    [Parameter]
                    public RenderFragment? Actions { get; set; }

                    [Parameter]
                    public RenderFragment? UserRegion { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/header-page")]
                public sealed class HeaderPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenHeaderBar));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "HeaderPage",
            componentName: nameof(VbenHeaderBar));

        Assert.AreEqual("ECMAScript.Vben.VbenHeaderBar", descriptor.FullName);
        Assert.AreEqual(nameof(VbenHeaderBar), descriptor.Name);
        Assert.AreEqual("ECMAScript.Vben", descriptor.ResolutionNamespace);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, descriptor.SourceKind);
        Assert.AreEqual("demo-shell/components", descriptor.ImportSpecifier);
        Assert.AreEqual("AppHeader", descriptor.ExportName);
        Assert.AreEqual("ECMAScript.Vben.VbenHeaderBar", descriptor.ContainerContractFullName);
        CollectionAssert.AreEqual(new[] { "demo-shell/styles/header.css" }, descriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-shell" }, descriptor.PluginRequirements.ToArray());

        var titleProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenHeaderBar.Title));
        Assert.AreEqual("title-text", titleProp.Name);

        var subtitleProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenHeaderBar.Subtitle));
        Assert.AreEqual("subtitle-text", subtitleProp.Name);

        var additionalAttributesProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenComponentBase.AdditionalAttributes));
        Assert.AreEqual("attrs", additionalAttributesProp.Name);
        Assert.IsTrue(additionalAttributesProp.CaptureUnmatchedValues);

        var logoSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenHeaderBar.Logo));
        Assert.AreEqual("brand", logoSlot.Name);

        var actionsSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenHeaderBar.Actions));
        Assert.AreEqual("toolbar", actionsSlot.Name);

        var userRegionSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenHeaderBar.UserRegion));
        Assert.AreEqual("user-menu", userRegionSlot.Name);
    }

    [TestMethod]
    public void Vben_SidebarMenu_ContainerInject_UsesConfiguredLibraryImplementationWithModelPropsAndEmits()
    {
        var descriptor = ResolveComponentDescriptor(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(
                typeof(ECMAScript.Vben.VbenSidebarMenu),
                typeof(Demo.Implementations.ElementSidebarMenu))]

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueLibraryStyle("element-plus/theme-chalk/el-menu.css")]
                [VueLibraryPluginRequirement("element-plus")]
                [VueProp(nameof(VbenSidebarMenu.Collapsed), Name = "collapse")]
                [VueProp(nameof(VbenSidebarMenu.SelectedKey), VuePropKind.Model, Name = "selected-key", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(VbenSidebarMenu.SelectedKeyChanged), VueEmitKind.ModelUpdate, Name = "update:selected-key", PayloadTypeName = "System.String")]
                [VueProp(nameof(VbenSidebarMenu.ExpandedKeys), VuePropKind.Model, Name = "expanded-keys", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(VbenSidebarMenu.ExpandedKeysChanged), VueEmitKind.ModelUpdate, Name = "update:expanded-keys", PayloadTypeName = "System.String[]")]
                [VueProp(nameof(VbenSidebarMenu.Items), Name = "menus")]
                [VueProp(nameof(VbenComponentBase.CssClass), Name = "class")]
                [VueProp(nameof(VbenComponentBase.CssStyle), Name = "style")]
                [VueProp(nameof(VbenComponentBase.AdditionalAttributes), Name = "attrs")]
                [VueSlot(nameof(VbenSidebarMenu.Logo), Name = "logo")]
                public sealed class ElementSidebarMenu : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<VbenSidebarMenu>
                {
                    [Parameter]
                    public bool Collapsed { get; set; }

                    [Parameter]
                    public string? SelectedKey { get; set; }

                    [Parameter]
                    public EventCallback<string> SelectedKeyChanged { get; set; }

                    [Parameter]
                    public string[]? ExpandedKeys { get; set; }

                    [Parameter]
                    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

                    [Parameter]
                    public VbenNavItems? Items { get; set; }

                    [Parameter]
                    public VueClassValue? CssClass { get; set; }

                    [Parameter]
                    public VueStyleValue? CssStyle { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                    [Parameter]
                    public RenderFragment? Logo { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/sidebar-page")]
                public sealed class SidebarPage : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenSidebarMenu));
                        builder.CloseComponent();
                    }
                }
            }
            """,
            pageComponentName: "SidebarPage",
            componentName: nameof(VbenSidebarMenu));

        Assert.AreEqual("ECMAScript.Vben.VbenSidebarMenu", descriptor.FullName);
        Assert.AreEqual(nameof(VbenSidebarMenu), descriptor.Name);
        Assert.AreEqual(VueComponentSourceKind.LibraryComponent, descriptor.SourceKind);
        Assert.AreEqual("element-plus", descriptor.ImportSpecifier);
        Assert.AreEqual("ElMenu", descriptor.ExportName);
        Assert.AreEqual("ECMAScript.Vben.VbenSidebarMenu", descriptor.ContainerContractFullName);
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-menu.css" }, descriptor.StyleDependencies.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, descriptor.PluginRequirements.ToArray());

        var collapsedProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenSidebarMenu.Collapsed));
        Assert.AreEqual("collapse", collapsedProp.Name);
        Assert.AreEqual(VuePropKind.Normal, collapsedProp.Kind);
        Assert.IsFalse(collapsedProp.AcceptsBinding);

        var selectedKeyProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenSidebarMenu.SelectedKey));
        Assert.AreEqual("selected-key", selectedKeyProp.Name);
        Assert.AreEqual(VuePropKind.Model, selectedKeyProp.Kind);
        Assert.IsTrue(selectedKeyProp.AcceptsBinding);

        var expandedKeysProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenSidebarMenu.ExpandedKeys));
        Assert.AreEqual("expanded-keys", expandedKeysProp.Name);
        Assert.AreEqual(VuePropKind.Model, expandedKeysProp.Kind);
        Assert.IsTrue(expandedKeysProp.AcceptsBinding);

        var selectedKeyChangedEmit = descriptor.Emits.Single(static item => item.RazorAlias == nameof(VbenSidebarMenu.SelectedKeyChanged));
        Assert.AreEqual("update:selected-key", selectedKeyChangedEmit.Name);
        Assert.AreEqual("string", selectedKeyChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, selectedKeyChangedEmit.Kind);

        var expandedKeysChangedEmit = descriptor.Emits.Single(static item => item.RazorAlias == nameof(VbenSidebarMenu.ExpandedKeysChanged));
        Assert.AreEqual("update:expanded-keys", expandedKeysChangedEmit.Name);
        Assert.AreEqual("string[]", expandedKeysChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, expandedKeysChangedEmit.Kind);

        var additionalAttributesProp = descriptor.Props.Single(static item => item.PublicName == nameof(VbenComponentBase.AdditionalAttributes));
        Assert.AreEqual("attrs", additionalAttributesProp.Name);
        Assert.IsTrue(additionalAttributesProp.CaptureUnmatchedValues);

        var logoSlot = descriptor.Slots.Single(static item => item.PublicName == nameof(VbenSidebarMenu.Logo));
        Assert.AreEqual("logo", logoSlot.Name);
    }

    private static VueComponentDescriptor ResolveComponentDescriptor(
        string source,
        string pageComponentName = "DashboardPage",
        string componentName = nameof(VbenPageContainer))
    {
        var context = CreateContext(source);
        var snapshot = context.CreateSemanticSnapshots()
            .Single(item => item.ComponentSymbol.Name == pageComponentName);
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);
        var resolvedComponents = RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);

        Assert.IsTrue(resolvedComponents.TryGetValue(componentName, out var descriptor), componentName);
        Assert.IsNotNull(descriptor, componentName);
        return descriptor!;
    }

    private static RazorVueCompilationIssueException ResolveInvalidContainerInject(
        string source,
        string pageComponentName = "DashboardPage")
    {
        var context = CreateContext(source);
        var snapshot = context.CreateSemanticSnapshots()
            .Single(item => item.ComponentSymbol.Name == pageComponentName);
        var renderTree = BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        return Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree));
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "ECMAScript.Vben.ContainerInject.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToImmutableArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context!;
    }
}
