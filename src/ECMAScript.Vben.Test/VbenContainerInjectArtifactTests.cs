using Jazor.RazorVue.Artifacts;

namespace ECMAScript.Vben.Test;

public sealed partial class VbenContainerInjectTests
{
    [TestMethod]
    public void Vben_AdminLayout_ContainerInject_LowersInjectedRuntimeShape_IntoVueSfcArtifact()
    {
        var context = CreateContext(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
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
                    [Parameter] public VbenLayoutMode Mode { get; set; }
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public VbenNavItems? NavItems { get; set; }
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Header { get; set; }
                    [Parameter] public RenderFragment? Sidebar { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/layout-page")]
                public sealed class LayoutPage : ComponentBase, IVueComponent
                {
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenAdminLayout));
                        builder.AddComponentParameter(1, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Mixed);
                        builder.AddComponentParameter(2, nameof(VbenAdminLayout.Collapsed), Collapsed);
                        builder.AddComponentParameter(3, nameof(VbenAdminLayout.CollapsedChanged), CollapsedChanged);
                        builder.AddComponentParameter(4, nameof(VbenAdminLayout.SelectedKey), SelectedKey);
                        builder.AddComponentParameter(5, nameof(VbenAdminLayout.SelectedKeyChanged), SelectedKeyChanged);
                        builder.AddComponentParameter(6, nameof(VbenAdminLayout.ExpandedKeys), ExpandedKeys);
                        builder.AddComponentParameter(7, nameof(VbenAdminLayout.ExpandedKeysChanged), ExpandedKeysChanged);
                        builder.AddComponentParameter(8, nameof(VbenAdminLayout.Title), "Workbench");
                        builder.AddComponentParameter(9, nameof(VbenAdminLayout.Subtitle), "Operations");
                        builder.AddComponentParameter(10, nameof(VbenAdminLayout.HeaderActions), HeaderActions);
                        builder.AddComponentParameter(11, nameof(VbenAdminLayout.UserRegion), UserRegion);
                        builder.AddComponentParameter(12, nameof(VbenContentComponentBase.ChildContent), ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "LayoutPage");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import { ElContainer as ElContainer } from \"element-plus\";");
        StringAssert.Contains(artifact.TemplateText, "<ElContainer layout-mode=\"mixed\" :menuCollapsed=\"props.collapsed\" @update:menuCollapsed=\"(__value) =&gt; emit(&quot;update:collapsed&quot;, __value)\"");
        StringAssert.Contains(artifact.TemplateText, ":activeMenu=\"props.selectedKey\"");
        StringAssert.Contains(artifact.TemplateText, "@update:activeMenu=\"(__value) =&gt; emit(&quot;update:selectedKey&quot;, __value)\"");
        StringAssert.Contains(artifact.TemplateText, ":openMenus=\"props.expandedKeys\"");
        StringAssert.Contains(artifact.TemplateText, "@update:openMenus=\"(__value) =&gt; emit(&quot;update:expandedKeys&quot;, __value)\"");
        StringAssert.Contains(artifact.TemplateText, "brand-title=\"Workbench\"");
        StringAssert.Contains(artifact.TemplateText, "brand-subtitle=\"Operations\"");
        StringAssert.Contains(artifact.TemplateText, "<template #header-actions>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"headerActions\" />");
        StringAssert.Contains(artifact.TemplateText, "<template #user-region>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"userRegion\" />");
        StringAssert.Contains(artifact.TemplateText, "<slot />");
        Assert.IsFalse(artifact.TemplateText.Contains("collapsed=\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("selectedKey=", StringComparison.Ordinal), artifact.TemplateText);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-container.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void Vben_PageContainer_ContainerInject_LowersInjectedRuntimeShape_IntoVueSfcArtifact()
    {
        var context = CreateContext(
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
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }
                    [Parameter] public VbenPageAction[]? Actions { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Extra { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.AddComponentParameter(1, nameof(VbenPageContainer.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(VbenPageContainer.Subtitle), "Shell");
                        builder.AddComponentParameter(3, nameof(VbenPageContainer.Extra), Extra);
                        builder.AddComponentParameter(4, nameof(VbenContentComponentBase.ChildContent), ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "DashboardPage");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import { ElCard as ElCard } from \"element-plus\";");
        StringAssert.Contains(artifact.TemplateText, "<ElCard header=\"Overview\" subtitle-text=\"Shell\">");
        StringAssert.Contains(artifact.TemplateText, "<template #header-extra>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"extra\" />");
        StringAssert.Contains(artifact.TemplateText, "<slot />");
        Assert.IsFalse(artifact.TemplateText.Contains("title=\"Overview\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("subtitle=\"Shell\"", StringComparison.Ordinal), artifact.TemplateText);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-card.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void Vben_HeaderBar_ContainerInject_LowersInjectedRuntimeShape_IntoVueSfcArtifact()
    {
        var context = CreateContext(
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
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Actions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/header-page")]
                public sealed class HeaderPage : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Actions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenHeaderBar));
                        builder.AddComponentParameter(1, nameof(VbenHeaderBar.Title), "Workbench");
                        builder.AddComponentParameter(2, nameof(VbenHeaderBar.Subtitle), "Operations");
                        builder.AddComponentParameter(3, nameof(VbenHeaderBar.Logo), Logo);
                        builder.AddComponentParameter(4, nameof(VbenHeaderBar.Actions), Actions);
                        builder.AddComponentParameter(5, nameof(VbenHeaderBar.UserRegion), UserRegion);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HeaderPage");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import { AppHeader as AppHeader } from \"demo-shell/components\";");
        StringAssert.Contains(artifact.TemplateText, "<AppHeader title-text=\"Workbench\" subtitle-text=\"Operations\">");
        StringAssert.Contains(artifact.TemplateText, "<template #brand>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"logo\" />");
        StringAssert.Contains(artifact.TemplateText, "<template #toolbar>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"actions\" />");
        StringAssert.Contains(artifact.TemplateText, "<template #user-menu>");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"userRegion\" />");
        Assert.IsFalse(artifact.TemplateText.Contains("title=\"Workbench\"", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains("subtitle=\"Operations\"", StringComparison.Ordinal), artifact.TemplateText);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "demo-shell/components");
        CollectionAssert.AreEqual(new[] { "demo-shell/styles/header.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-shell" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void Vben_HeaderBar_ContainerInject_LowersInjectedRuntimeShape_IntoPipelineArtifact()
    {
        var context = CreateContext(
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
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Actions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/header-page")]
                public sealed class HeaderPage : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Actions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenHeaderBar));
                        builder.AddComponentParameter(1, nameof(VbenHeaderBar.Title), "Workbench");
                        builder.AddComponentParameter(2, nameof(VbenHeaderBar.Subtitle), "Operations");
                        builder.AddComponentParameter(3, nameof(VbenHeaderBar.Logo), Logo);
                        builder.AddComponentParameter(4, nameof(VbenHeaderBar.Actions), Actions);
                        builder.AddComponentParameter(5, nameof(VbenHeaderBar.UserRegion), UserRegion);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "HeaderPage");

        StringAssert.Contains(artifact.ModuleCode, "import { AppHeader as VbenHeaderBarComponent } from \"demo-shell/components\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(VbenHeaderBarComponent, { \"title-text\": \"Workbench\", \"subtitle-text\": \"Operations\" }, { brand: () => slots.logo ? slots.logo() : null, toolbar: () => slots.actions ? slots.actions() : null, \"user-menu\": () => slots.userRegion ? slots.userRegion() : null });");
        Assert.IsFalse(artifact.ModuleCode.Contains("\"title\": \"Workbench\"", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"subtitle\": \"Operations\"", StringComparison.Ordinal), artifact.ModuleCode);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "demo-shell/components");
        CollectionAssert.AreEqual(new[] { "demo-shell/styles/header.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "demo-shell" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void Vben_PageContainer_ContainerInject_LowersInjectedRuntimeShape_IntoPipelineArtifact()
    {
        var context = CreateContext(
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
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }
                    [Parameter] public VbenPageAction[]? Actions { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Extra { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/dashboard-page")]
                public sealed class DashboardPage : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.AddComponentParameter(1, nameof(VbenPageContainer.Title), "Overview");
                        builder.AddComponentParameter(2, nameof(VbenPageContainer.Subtitle), "Shell");
                        builder.AddComponentParameter(3, nameof(VbenPageContainer.Extra), Extra);
                        builder.AddComponentParameter(4, nameof(VbenContentComponentBase.ChildContent), ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "DashboardPage");

        StringAssert.Contains(artifact.ModuleCode, "import { ElCard as VbenPageContainerComponent } from \"element-plus\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(VbenPageContainerComponent, { \"header\": \"Overview\", \"subtitle-text\": \"Shell\" }, { \"header-extra\": () => slots.extra ? slots.extra() : null, default: () => slots.default ? slots.default() : null });");
        Assert.IsFalse(artifact.ModuleCode.Contains("\"title\": \"Overview\"", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"subtitle\": \"Shell\"", StringComparison.Ordinal), artifact.ModuleCode);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-card.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void Vben_AdminLayout_ContainerInject_LowersInjectedRuntimeShape_IntoPipelineArtifact()
    {
        var context = CreateContext(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
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
                    [Parameter] public VbenLayoutMode Mode { get; set; }
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public VbenNavItems? NavItems { get; set; }
                    [Parameter] public string? Title { get; set; }
                    [Parameter] public string? Subtitle { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                    [Parameter] public RenderFragment? Header { get; set; }
                    [Parameter] public RenderFragment? Sidebar { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/layout-page")]
                public sealed class LayoutPage : ComponentBase, IVueComponent
                {
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenAdminLayout));
                        builder.AddComponentParameter(1, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Mixed);
                        builder.AddComponentParameter(2, nameof(VbenAdminLayout.Collapsed), Collapsed);
                        builder.AddComponentParameter(3, nameof(VbenAdminLayout.CollapsedChanged), CollapsedChanged);
                        builder.AddComponentParameter(4, nameof(VbenAdminLayout.SelectedKey), SelectedKey);
                        builder.AddComponentParameter(5, nameof(VbenAdminLayout.SelectedKeyChanged), SelectedKeyChanged);
                        builder.AddComponentParameter(6, nameof(VbenAdminLayout.ExpandedKeys), ExpandedKeys);
                        builder.AddComponentParameter(7, nameof(VbenAdminLayout.ExpandedKeysChanged), ExpandedKeysChanged);
                        builder.AddComponentParameter(8, nameof(VbenAdminLayout.Title), "Workbench");
                        builder.AddComponentParameter(9, nameof(VbenAdminLayout.Subtitle), "Operations");
                        builder.AddComponentParameter(10, nameof(VbenAdminLayout.HeaderActions), HeaderActions);
                        builder.AddComponentParameter(11, nameof(VbenAdminLayout.UserRegion), UserRegion);
                        builder.AddComponentParameter(12, nameof(VbenContentComponentBase.ChildContent), ChildContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "LayoutPage");

        StringAssert.Contains(artifact.ModuleCode, "import { ElContainer as VbenAdminLayoutComponent } from \"element-plus\";");
        StringAssert.Contains(artifact.ModuleCode, "\"layout-mode\": \"mixed\"");
        StringAssert.Contains(artifact.ModuleCode, "\"menuCollapsed\": props.collapsed");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:menuCollapsed\": (__value) => emit(\"update:collapsed\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "\"activeMenu\": props.selectedKey");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:activeMenu\": (__value) => emit(\"update:selectedKey\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "\"openMenus\": props.expandedKeys");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:openMenus\": (__value) => emit(\"update:expandedKeys\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "\"brand-title\": \"Workbench\"");
        StringAssert.Contains(artifact.ModuleCode, "\"brand-subtitle\": \"Operations\"");
        StringAssert.Contains(artifact.ModuleCode, "\"header-actions\": () => slots.headerActions ? slots.headerActions() : null");
        StringAssert.Contains(artifact.ModuleCode, "\"user-region\": () => slots.userRegion ? slots.userRegion() : null");
        StringAssert.Contains(artifact.ModuleCode, "default: () => slots.default ? slots.default() : null");
        Assert.IsFalse(artifact.ModuleCode.Contains("\"collapsed\": props.collapsed", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"selectedKey\": props.selectedKey", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"expandedKeys\": props.expandedKeys", StringComparison.Ordinal), artifact.ModuleCode);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-container.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }

    [TestMethod]
    public void Vben_SidebarMenu_ContainerInject_LowersInjectedRuntimeShape_IntoPipelineArtifact()
    {
        var context = CreateContext(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.Vben;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
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
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public VbenNavItems? Items { get; set; }
                    [Parameter] public VueClassValue? CssClass { get; set; }
                    [Parameter] public VueStyleValue? CssStyle { get; set; }
                    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/sidebar-page")]
                public sealed class SidebarPage : ComponentBase, IVueComponent
                {
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public RenderFragment? Logo { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenSidebarMenu));
                        builder.AddComponentParameter(1, nameof(VbenSidebarMenu.Collapsed), true);
                        builder.AddComponentParameter(2, nameof(VbenSidebarMenu.SelectedKey), SelectedKey);
                        builder.AddComponentParameter(3, nameof(VbenSidebarMenu.SelectedKeyChanged), SelectedKeyChanged);
                        builder.AddComponentParameter(4, nameof(VbenSidebarMenu.ExpandedKeys), ExpandedKeys);
                        builder.AddComponentParameter(5, nameof(VbenSidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
                        builder.AddComponentParameter(6, nameof(VbenSidebarMenu.Logo), Logo);
                        builder.CloseComponent();
                    }
                }
            }
            """);

        var artifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "SidebarPage");

        StringAssert.Contains(artifact.ModuleCode, "import { ElMenu as VbenSidebarMenuComponent } from \"element-plus\";");
        StringAssert.Contains(artifact.ModuleCode, "return () => h(VbenSidebarMenuComponent, { \"collapse\": true, \"selected-key\": props.selectedKey, \"onUpdate:selected-key\": (__value) => emit(\"update:selectedKey\", __value), \"expanded-keys\": props.expandedKeys, \"onUpdate:expanded-keys\": (__value) => emit(\"update:expandedKeys\", __value) }, { logo: () => slots.logo ? slots.logo() : null });");
        Assert.IsFalse(artifact.ModuleCode.Contains("\"selectedKey\": props.selectedKey", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("\"expandedKeys\": props.expandedKeys", StringComparison.Ordinal), artifact.ModuleCode);
        CollectionAssert.Contains(artifact.Imports.ToArray(), "element-plus");
        CollectionAssert.AreEqual(new[] { "element-plus/theme-chalk/el-menu.css" }, artifact.Styles.ToArray());
        CollectionAssert.AreEqual(new[] { "element-plus" }, artifact.PluginRequirements.ToArray());
    }
}
