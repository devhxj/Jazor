namespace ECMAScript.Vben.Test;

public sealed partial class VbenContainerInjectTests
{
    [TestMethod]
    public void Vben_AdminLayout_DefaultNativeComponent_WithoutHeaderContent_DoesNotLowerEmptyHeaderRegion()
    {
        var context = CreateContext(
            """
            using ECMAScript;
            using ECMAScript.Vben;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/layout-without-header")]
                public sealed class LayoutWithoutHeader : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenAdminLayout));
                        builder.AddComponentParameter(1, nameof(VbenAdminLayout.Mode), VbenLayoutMode.Top);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "LayoutWithoutHeader");
        var sfcArtifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var pipelineArtifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "LayoutWithoutHeader");

        Assert.IsFalse(sfcArtifact.TemplateText.Contains("vben-shell__header", StringComparison.Ordinal), sfcArtifact.TemplateText);
        Assert.IsFalse(sfcArtifact.TemplateText.Contains("VbenHeaderBarComponent", StringComparison.Ordinal), sfcArtifact.TemplateText);

        Assert.IsFalse(pipelineArtifact.ModuleCode.Contains("vben-shell__header", StringComparison.Ordinal), pipelineArtifact.ModuleCode);
        Assert.IsFalse(pipelineArtifact.ModuleCode.Contains("VbenHeaderBarComponent", StringComparison.Ordinal), pipelineArtifact.ModuleCode);
    }

    [TestMethod]
    public void Vben_HeaderBar_DefaultNativeComponent_WithOnlyUserRegion_DoesNotLowerEmptyMainRegion()
    {
        var context = CreateContext(
            """
            using ECMAScript;
            using ECMAScript.Vben;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/header-with-user-region-only")]
                public sealed class HeaderWithUserRegionOnly : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? UserRegionContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenHeaderBar));
                        builder.AddComponentParameter(1, nameof(VbenHeaderBar.UserRegion), UserRegionContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "HeaderWithUserRegionOnly");
        var sfcArtifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var pipelineArtifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "HeaderWithUserRegionOnly");

        Assert.IsFalse(sfcArtifact.TemplateText.Contains("vben-header__main", StringComparison.Ordinal), sfcArtifact.TemplateText);
        StringAssert.Contains(sfcArtifact.TemplateText, "<slot name=\"userRegionContent\" />");

        Assert.IsFalse(pipelineArtifact.ModuleCode.Contains("vben-header__main", StringComparison.Ordinal), pipelineArtifact.ModuleCode);
        StringAssert.Contains(
            pipelineArtifact.ModuleCode,
            "userRegion: () => slots.userRegionContent ? slots.userRegionContent() : null");
    }

    [TestMethod]
    public void Vben_PageContainer_DefaultNativeComponent_WithOnlyExtra_DoesNotLowerEmptyTitlesRegion()
    {
        var context = CreateContext(
            """
            using ECMAScript;
            using ECMAScript.Vben;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/page-with-extra-only")]
                public sealed class PageWithExtraOnly : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? ExtraContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, typeof(VbenPageContainer));
                        builder.AddComponentParameter(1, nameof(VbenPageContainer.Extra), ExtraContent);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "PageWithExtraOnly");
        var sfcArtifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);
        var pipelineArtifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "PageWithExtraOnly");

        Assert.IsFalse(sfcArtifact.TemplateText.Contains("vben-page__titles", StringComparison.Ordinal), sfcArtifact.TemplateText);
        StringAssert.Contains(sfcArtifact.TemplateText, "<slot name=\"extraContent\" />");

        Assert.IsFalse(pipelineArtifact.ModuleCode.Contains("vben-page__titles", StringComparison.Ordinal), pipelineArtifact.ModuleCode);
        StringAssert.Contains(
            pipelineArtifact.ModuleCode,
            "extra: () => slots.extraContent ? slots.extraContent() : null");
    }

    [TestMethod]
    public void Vben_MultiShell_DefaultNativeComponents_LowerIntoVueSfcArtifact()
    {
        var context = CreateNativeMultiShellContext();
        var snapshot = context.CreateSemanticSnapshots()
            .Single(static item => item.ComponentSymbol.Name == "ShellPage");
        var artifact = CreateBuildRenderTreeArtifactFactory().Lower(context, snapshot);

        StringAssert.Contains(artifact.ScriptSetupText, "import VbenAdminLayoutComponent from \"../components/vben-admin-layout.vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "import VbenHeaderBarComponent from \"../components/vben-header-bar.vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "import VbenSidebarMenuComponent from \"../components/vben-sidebar-menu.vue\";");
        StringAssert.Contains(artifact.ScriptSetupText, "import VbenPageContainerComponent from \"../components/vben-page-container.vue\";");

        StringAssert.Contains(artifact.TemplateText, "<VbenAdminLayoutComponent mode=\"mixed\"");
        StringAssert.Contains(artifact.TemplateText, "<template #header>");
        StringAssert.Contains(artifact.TemplateText, "<VbenHeaderBarComponent title=\"Workbench\" subtitle=\"Operations\">");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"headerLogo\" />");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"headerActions\" />");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"userRegion\" />");
        StringAssert.Contains(artifact.TemplateText, "<template #sidebar>");
        StringAssert.Contains(artifact.TemplateText, "<VbenSidebarMenuComponent :collapsed=\"props.collapsed\"");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"sidebarLogo\" />");
        StringAssert.Contains(artifact.TemplateText, "<VbenPageContainerComponent title=\"Dashboard\" subtitle=\"Realtime\">");
        StringAssert.Contains(artifact.TemplateText, "<slot name=\"pageExtra\" />");
        StringAssert.Contains(artifact.TemplateText, "<slot />");

        CollectionAssert.AreEquivalent(
            new[]
            {
                "vue",
                "../components/vben-admin-layout.vue",
                "../components/vben-header-bar.vue",
                "../components/vben-sidebar-menu.vue",
                "../components/vben-page-container.vue"
            },
            artifact.Imports.ToArray());
        Assert.AreEqual(0, artifact.Styles.Count(), artifact.SfcText);
        Assert.AreEqual(0, artifact.PluginRequirements.Count(), artifact.SfcText);
    }

    [TestMethod]
    public void Vben_MultiShell_DefaultNativeComponents_LowerIntoPipelineArtifact()
    {
        var context = CreateNativeMultiShellContext();
        var artifact = CreateBuildRenderTreePipeline().Execute(context).Artifacts
            .Single(static item => item.ComponentName == "ShellPage");

        StringAssert.Contains(artifact.ModuleCode, "import VbenAdminLayoutComponent from \"./components/vben-admin-layout.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "import VbenHeaderBarComponent from \"./components/vben-header-bar.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "import VbenSidebarMenuComponent from \"./components/vben-sidebar-menu.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "import VbenPageContainerComponent from \"./components/vben-page-container.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "\"mode\": \"mixed\"");
        StringAssert.Contains(artifact.ModuleCode, "\"collapsed\": props.collapsed");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:collapsed\": (__value) => emit(\"update:collapsed\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "\"selectedKey\": props.selectedKey");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:selectedKey\": (__value) => emit(\"update:selectedKey\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "\"expandedKeys\": props.expandedKeys");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:expandedKeys\": (__value) => emit(\"update:expandedKeys\", __value)");
        StringAssert.Contains(artifact.ModuleCode, "header: () => h(VbenHeaderBarComponent, { \"title\": \"Workbench\", \"subtitle\": \"Operations\" }, { logo: () => slots.headerLogo ? slots.headerLogo() : null, actions: () => slots.headerActions ? slots.headerActions() : null, userRegion: () => slots.userRegion ? slots.userRegion() : null })");
        StringAssert.Contains(artifact.ModuleCode, "sidebar: () => h(VbenSidebarMenuComponent, { \"collapsed\": props.collapsed, \"selectedKey\": props.selectedKey, \"onUpdate:selectedKey\": (__value) => emit(\"update:selectedKey\", __value), \"expandedKeys\": props.expandedKeys, \"onUpdate:expandedKeys\": (__value) => emit(\"update:expandedKeys\", __value) }, { logo: () => slots.sidebarLogo ? slots.sidebarLogo() : null })");
        StringAssert.Contains(artifact.ModuleCode, "default: () => h(VbenPageContainerComponent, { \"title\": \"Dashboard\", \"subtitle\": \"Realtime\" }, { extra: () => slots.pageExtra ? slots.pageExtra() : null, default: () => slots.default ? slots.default() : null })");

        CollectionAssert.AreEquivalent(
            new[]
            {
                "vue",
                "./components/vben-admin-layout.mjs",
                "./components/vben-header-bar.mjs",
                "./components/vben-sidebar-menu.mjs",
                "./components/vben-page-container.mjs"
            },
            artifact.Imports.ToArray());
        Assert.AreEqual(0, artifact.Styles.Count(), artifact.ModuleCode);
        Assert.AreEqual(0, artifact.PluginRequirements.Count(), artifact.ModuleCode);
    }

    private static RazorVueCompilationContext CreateNativeMultiShellContext()
        => CreateContext(
            """
            using ECMAScript;
            using ECMAScript.Vben;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./pages/shell-page")]
                public sealed class ShellPage : ComponentBase, IVueComponent
                {
                    [Parameter] public bool Collapsed { get; set; }
                    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
                    [Parameter] public string? SelectedKey { get; set; }
                    [Parameter] public EventCallback<string> SelectedKeyChanged { get; set; }
                    [Parameter] public string[]? ExpandedKeys { get; set; }
                    [Parameter] public EventCallback<string[]> ExpandedKeysChanged { get; set; }
                    [Parameter] public RenderFragment? HeaderLogo { get; set; }
                    [Parameter] public RenderFragment? HeaderActions { get; set; }
                    [Parameter] public RenderFragment? UserRegion { get; set; }
                    [Parameter] public RenderFragment? SidebarLogo { get; set; }
                    [Parameter] public RenderFragment? PageExtra { get; set; }
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
                        builder.AddComponentParameter(8, nameof(VbenAdminLayout.Header), (RenderFragment)(headerBuilder =>
                        {
                            headerBuilder.OpenComponent(0, typeof(VbenHeaderBar));
                            headerBuilder.AddComponentParameter(1, nameof(VbenHeaderBar.Title), "Workbench");
                            headerBuilder.AddComponentParameter(2, nameof(VbenHeaderBar.Subtitle), "Operations");
                            headerBuilder.AddComponentParameter(3, nameof(VbenHeaderBar.Logo), HeaderLogo);
                            headerBuilder.AddComponentParameter(4, nameof(VbenHeaderBar.Actions), HeaderActions);
                            headerBuilder.AddComponentParameter(5, nameof(VbenHeaderBar.UserRegion), UserRegion);
                            headerBuilder.CloseComponent();
                        }));
                        builder.AddComponentParameter(9, nameof(VbenAdminLayout.Sidebar), (RenderFragment)(sidebarBuilder =>
                        {
                            sidebarBuilder.OpenComponent(0, typeof(VbenSidebarMenu));
                            sidebarBuilder.AddComponentParameter(1, nameof(VbenSidebarMenu.Collapsed), Collapsed);
                            sidebarBuilder.AddComponentParameter(2, nameof(VbenSidebarMenu.SelectedKey), SelectedKey);
                            sidebarBuilder.AddComponentParameter(3, nameof(VbenSidebarMenu.SelectedKeyChanged), SelectedKeyChanged);
                            sidebarBuilder.AddComponentParameter(4, nameof(VbenSidebarMenu.ExpandedKeys), ExpandedKeys);
                            sidebarBuilder.AddComponentParameter(5, nameof(VbenSidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
                            sidebarBuilder.AddComponentParameter(6, nameof(VbenSidebarMenu.Logo), SidebarLogo);
                            sidebarBuilder.CloseComponent();
                        }));
                        builder.AddComponentParameter(10, nameof(VbenContentComponentBase.ChildContent), (RenderFragment)(contentBuilder =>
                        {
                            contentBuilder.OpenComponent(0, typeof(VbenPageContainer));
                            contentBuilder.AddComponentParameter(1, nameof(VbenPageContainer.Title), "Dashboard");
                            contentBuilder.AddComponentParameter(2, nameof(VbenPageContainer.Subtitle), "Realtime");
                            contentBuilder.AddComponentParameter(3, nameof(VbenPageContainer.Extra), PageExtra);
                            contentBuilder.AddComponentParameter(4, nameof(VbenContentComponentBase.ChildContent), ChildContent);
                            contentBuilder.CloseComponent();
                        }));
                        builder.CloseComponent();
                    }
                }
            }
            """);
}
