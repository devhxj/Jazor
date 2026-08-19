namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialConditionalSlotRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalChildContent_SelectsTheActiveDefaultSlotContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleasePanelPage.razor"),
            documentText:
            """
            @using Demo.Components

            <ReleasePanel>
                @if (ShowHistory)
                {
                    <strong data-release="@ReleaseName">@ReleaseName</strong>
                    <span data-state="history">@ReleaseName history available</span>
                }
                else
                {
                    <span data-release="@ReleaseName">History hidden</span>
                }
            </ReleasePanel>
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-panel-conditional-slot-runtime")]
                public sealed class ReleasePanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#default")] public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-panel-page-conditional-slot-runtime")]
                public partial class ReleasePanelPage : ComponentBase, IVueComponent
                {
                    [Parameter] public string ReleaseName { get; set; } = "Orders API";

                    [Parameter] public bool ShowHistory { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleasePanelPage");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "default:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "props.ShowHistory", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-panel-page-conditional-slot-runtime.mjs",
            observation.ModuleText,
            "official-release-panel-conditional-slot-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { Fragment } from "vue";

            import component from "./components/release-panel-page-conditional-slot-runtime.mjs";
            import releasePanel from "./components/release-panel-conditional-slot-runtime.mjs";

            test("official Razor conditional ChildContent keeps the active slot shape", () => {
                const historyPanel = component.setup({
                    ReleaseName: "Orders API",
                    ShowHistory: true
                }, { slots: {} })();
                assert.equal(historyPanel.name, releasePanel);
                assert.equal(typeof historyPanel.children.default, "function");

                const history = historyPanel.children.default();
                assert.equal(history.length, 1);
                assert.equal(history[0].name, Fragment);
                const heading = history[0].children.find(node => node?.name === "strong");
                const status = history[0].children.find(node => node?.name === "span");
                assert.ok(heading);
                assert.ok(status);
                assert.equal(heading.props["data-release"], "Orders API");
                assert.equal(heading.children, "Orders API");
                assert.equal(status.props["data-state"], "history");
                assert.deepEqual(status.children, [
                    { name: "__text", children: "Orders API", patchFlag: 1 },
                    { name: "__text", children: " history available", patchFlag: undefined }
                ]);

                const hiddenPanel = component.setup({
                    ReleaseName: "Orders API",
                    ShowHistory: false
                }, { slots: {} })();
                const hidden = hiddenPanel.children.default();
                assert.equal(hidden.length, 1);
                assert.equal(hidden[0].name, "span");
                assert.equal(hidden[0].props["data-release"], "Orders API");
                assert.deepEqual(hidden[0].children, [{ name: "__text", children: "History hidden", patchFlag: undefined }]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-panel-conditional-slot-runtime.mjs"] = "export default { name: \"release-panel-conditional-slot-runtime\" };"
            });
    }
}
