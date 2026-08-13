namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInheritedDescriptorContractRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorDerivedComponent_InheritsBasePropAndEmitDescriptorsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\InheritedReleasePanel.razor",
            documentText:
            """
            @inherits Demo.Components.InheritedReleasePanelBase

            <section data-title="@Title"></section>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public abstract class InheritedReleasePanelBase : ComponentBase
                {
                    [ECMAScriptName("heading")]
                    [Parameter] public string Title { get; set; } = string.Empty;
                    [ECMAScriptName("onTitleChange")]
                    [Parameter] public EventCallback<string> TitleChanged { get; set; }
                }

                [ECMAScriptModule("./components/inherited-release-panel-self-contract-runtime")]
                public partial class InheritedReleasePanel : InheritedReleasePanelBase, IVueComponent
                {
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.InheritedReleasePanel");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props: [\"heading\", \"onTitleChange\"]", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("emits:", StringComparison.Ordinal), observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.heading", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/inherited-release-panel-self-contract-runtime.mjs",
            observation.ModuleText,
            "official-inherited-self-descriptor-contract-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/inherited-release-panel-self-contract-runtime.mjs";

            test("official Razor derived component exposes base Vue descriptors", () => {
                const root = component.setup({ heading: "Queued release" }, { slots: {} })();
                assert.equal(root.name, "section");
                assert.equal(root.props["data-title"], "Queued release");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInheritedComponentContract_UsesBasePropEmitAndSlotDescriptorsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseEditor.razor",
            documentText:
            """
            @using Demo.Components

            <section data-page="release-editor">
                <InheritedReleasePanel Title="@Title" TitleChanged="HandleTitleChanged">
                    <Header>
                        <strong>@Title</strong>
                    </Header>
                </InheritedReleasePanel>
                <p data-last-title="@LastTitle"></p>
            </section>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public abstract class InheritedReleasePanelBase : ComponentBase
                {
                    [ECMAScriptName("heading")]
                    [Parameter] public string Title { get; set; } = string.Empty;
                    [ECMAScriptName("onTitleChange")]
                    [Parameter] public EventCallback<string> TitleChanged { get; set; }
                    [ECMAScriptName("header")]
                    [Parameter] public RenderFragment? Header { get; set; }
                }

                [ECMAScriptModule("./components/inherited-release-panel-contract-runtime")]
                public sealed class InheritedReleasePanel : InheritedReleasePanelBase, IVueComponent
                {
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-editor-inherited-contract-runtime")]
                public partial class ReleaseEditor : ComponentBase, IVueComponent
                {
                    [Parameter] public string Title { get; set; } = "Draft release";

                    private string LastTitle { get; set; } = "none";

                    private void HandleTitleChanged(string title)
                    {
                        LastTitle = title;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseEditor");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "heading: props.Title", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onTitleChange", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-editor-inherited-contract-runtime.mjs",
            observation.ModuleText,
            "official-inherited-descriptor-contract-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-editor-inherited-contract-runtime.mjs";
            import inheritedReleasePanel from "./components/inherited-release-panel-contract-runtime.mjs";

            test("official Razor inherited descriptors preserve the component contract", () => {
                const render = component.setup({ Title: "Draft release" }, { slots: {} });
                const root = render();
                assert.equal(root.name, "section");
                assert.equal(root.props["data-page"], "release-editor");

                const panel = root.children[0];
                assert.equal(panel.name, inheritedReleasePanel);
                assert.equal(panel.props.heading, "Draft release");
                assert.equal(typeof panel.props.onTitleChange, "function");

                const headerNodes = panel.children.header();
                assert.equal(headerNodes[0].name, "strong");
                assert.equal(headerNodes[0].children, "Draft release");

                panel.props.onTitleChange("Approved release");
                const updated = render();
                const status = updated.children.find(child => child?.name === "p");
                assert.equal(status.props["data-last-title"], "Approved release");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/inherited-release-panel-contract-runtime.mjs"] = "export default { name: \"inherited-release-panel-contract-runtime\" };"
            });
    }
}
