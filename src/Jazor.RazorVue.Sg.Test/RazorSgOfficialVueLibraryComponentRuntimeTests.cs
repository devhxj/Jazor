namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueLibraryComponentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_UsesExplicitModelUpdateMetadata()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignSelectUsage.razor"),
            documentText:
            """
            @using Demo.Library

            <TSelect @bind-Selected="Selected" />
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-select-usage")]
            public partial class TDesignSelectUsage : ComponentBase, IVueComponent
            {
                private string Selected { get; set; } = "ready";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignSelectUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TSelect.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [ECMAScript("tdesign-vue-next", Transform.Component, "Select")]
                public sealed class TSelect : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("selected")]
                    public string Selected { get; set; } = string.Empty;

                    [Parameter, ECMAScriptName("onUpdate:selected")]
                    public EventCallback<string> SelectedChanged { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { Select } from \"tdesign-vue-next\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "selected: state.Selected", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "\"onUpdate:selected\": __jazor$handlerCache[0] || (__jazor$handlerCache[0] = __value => state.Selected = __value)",
            StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("selectedChanged", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_UsesExplicitNamedSlotMetadata()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignPanelUsage.razor"),
            documentText:
            """
            @using Demo.Library

            <TPanel>
                <TitleContent>title</TitleContent>
                <PrependItem>first</PrependItem>
                <DefaultContent>body</DefaultContent>
            </TPanel>
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-panel-usage")]
            public partial class TDesignPanelUsage : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignPanelUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TPanel.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [ECMAScript("tdesign-vue-next", Transform.Component, "Panel")]
                public sealed class TPanel : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("title")]
                    public RenderFragment? TitleContent { get; set; }

                    [Parameter, ECMAScriptName("prepend-item")]
                    public RenderFragment? PrependItem { get; set; }

                    [Parameter, ECMAScriptName("default")]
                    public RenderFragment? DefaultContent { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "title: withCtx(() =>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "\"prepend-item\": withCtx(() =>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "default: withCtx(() =>", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("titleContent", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("prependItem", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("defaultContent", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_AllowsPropAndSlotWithSameVueName()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignSubmenuUsage.razor"),
            documentText:
            """
            @using Demo.Library

            <TSubmenu Title="Menu title">
                <TitleContent>Slot title</TitleContent>
            </TSubmenu>
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-submenu-usage")]
            public partial class TDesignSubmenuUsage : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignSubmenuUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TSubmenu.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [ECMAScript("tdesign-vue-next", Transform.Component, "Submenu")]
                public sealed class TSubmenu : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("title")]
                    public string Title { get; set; } = string.Empty;

                    [Parameter, ECMAScriptName("title")]
                    public RenderFragment? TitleContent { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "title: \"Menu title\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "title: withCtx(() =>", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("titleContent", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_UsesExplicitOrdinaryEventName()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignActionUsage.razor"),
            documentText:
            """
            @using Demo.Library

            <TAction OnSave="Save" />
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-action-usage")]
            public partial class TDesignActionUsage : ComponentBase, IVueComponent
            {
                private void Save()
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignActionUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TAction.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [ECMAScript("tdesign-vue-next", Transform.Component, "Action")]
                public sealed class TAction : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("onSave")]
                    public EventCallback OnSave { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "onSave:", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("on-save", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_UsesExplicitModelPropertyNameWithoutDescriptorKinds()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignSelectModelUsage.razor"),
            documentText:
            """
            @using Demo.Library

            <TSelect @bind-SelectedValue="Selected" />
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-select-model-usage")]
            public partial class TDesignSelectModelUsage : ComponentBase, IVueComponent
            {
                private string Selected { get; set; } = "ready";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignSelectModelUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TSelect.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [ECMAScript("tdesign-vue-next", Transform.Component, "Select")]
                public sealed class TSelect : ComponentBase, IVueComponent
                {
                    [Parameter]
                    [ECMAScriptName("modelValue")]
                    public string SelectedValue { get; set; } = string.Empty;

                    [Parameter, ECMAScriptName("onUpdate:modelValue")]
                    public EventCallback<string> SelectedValueChanged { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "modelValue: state.Selected", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "\"onUpdate:modelValue\": __jazor$handlerCache[0] || (__jazor$handlerCache[0] = __value => state.Selected = __value)",
            StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("selectedValue", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignComponent_UsesNamedLibraryImport()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignButtonUsage.razor"),
            documentText:
            """
            @using Demo.Library

            <TButton Status="@Status" />
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-button-usage")]
            public partial class TDesignButtonUsage : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Status { get; set; } = "ready";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignButtonUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TButton.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [ECMAScript("tdesign-vue-next", Transform.Component, "Button")]
                public sealed class TButton : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("status")]
                    public string Status { get; set; } = string.Empty;
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { Button } from \"tdesign-vue-next\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "status: props.Status", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "openBlock(), createBlock(Button, { status: props.Status }, null, 8, [\"status\"])",
            StringComparison.Ordinal);
    }
}
