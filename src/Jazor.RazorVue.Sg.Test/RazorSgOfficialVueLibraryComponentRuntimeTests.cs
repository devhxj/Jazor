namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueLibraryComponentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_InfersModelUpdateFromParameterPair()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TDesignSelectUsage.razor",
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

                [VueLibraryComponent("tdesign-vue-next", "Select")]
                public sealed class TSelect : ComponentBase
                {
                    [Parameter]
                    public string Selected { get; set; } = string.Empty;

                    [Parameter]
                    public EventCallback<string> SelectedChanged { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { Select } from \"tdesign-vue-next\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "selected: state.selected", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "\"onUpdate:selected\": __value => state.selected = __value",
            StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("selectedChanged", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_InfersNamedSlotsFromRenderFragmentParameters()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TDesignPanelUsage.razor",
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

                [VueLibraryComponent("tdesign-vue-next", "Panel")]
                public sealed class TPanel : ComponentBase
                {
                    [Parameter]
                    public RenderFragment? TitleContent { get; set; }

                    [Parameter]
                    public RenderFragment? PrependItem { get; set; }

                    [Parameter]
                    public RenderFragment? DefaultContent { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "title: () =>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "\"prepend-item\": () =>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "default: () =>", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("titleContent", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("prependItem", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("defaultContent", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponent_UsesOnPrefixForOrdinaryEvents()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TDesignActionUsage.razor",
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

                [VueLibraryComponent("tdesign-vue-next", "Action")]
                public sealed class TAction : ComponentBase
                {
                    [Parameter]
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
            documentPath: @"D:\repo\Demo\Pages\TDesignSelectModelUsage.razor",
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

                [VueLibraryComponent("tdesign-vue-next", "Select")]
                [VueProp(nameof(SelectedValue), Name = "modelValue")]
                public sealed class TSelect : ComponentBase
                {
                    [Parameter]
                    public string SelectedValue { get; set; } = string.Empty;

                    [Parameter]
                    public EventCallback<string> SelectedValueChanged { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "modelValue: state.selected", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "\"onUpdate:modelValue\": __value => state.selected = __value",
            StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("selectedValue", StringComparison.Ordinal), observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignComponent_UsesNamedLibraryImport()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TDesignButtonUsage.razor",
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

                [VueLibraryComponent("tdesign-vue-next", "Button")]
                public sealed class TButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string Status { get; set; } = string.Empty;
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { Button } from \"tdesign-vue-next\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "status: props.status", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "h(Button, { status: props.status })", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("createRenderContext", StringComparison.Ordinal), observation.ModuleText);
    }
}
