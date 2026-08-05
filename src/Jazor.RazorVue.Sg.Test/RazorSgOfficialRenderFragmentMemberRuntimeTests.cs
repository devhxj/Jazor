namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentMemberRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentMembers_ProjectPropertyAndMethodGroupSlotsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseSummaryMembers.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@ReleaseHeader" Footer="@RenderReleaseFooter" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-render-fragment-members")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Header { get; set; }

                    [Parameter] public RenderFragment? Footer { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-summary-members")]
                public partial class ReleaseSummaryMembers : ComponentBase, IVueComponent
                {
                    private string ReleaseName { get; } = "June deployment";

                    private RenderFragment ReleaseHeader
                    {
                        get
                        {
                            return builder =>
                            {
                                builder.OpenElement(0, "strong");
                                builder.AddAttribute(1, "data-summary-part", "header");
                                builder.AddContent(2, ReleaseName);
                                builder.CloseElement();
                            };
                        }
                    }

                    private void RenderReleaseFooter(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(3, "small");
                        builder.AddAttribute(4, "data-summary-part", "footer");
                        builder.AddContent(5, "Ready");
                        builder.CloseElement();
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseSummaryMembers");

        StringAssert.Contains(observation.GeneratedCSharp, "ReleaseHeader", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RenderReleaseFooter", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "footer:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-summary-members.mjs",
            observation.ModuleText,
            "official-release-summary-members-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-summary-members.mjs";
            import slotPanel from "./components/slot-panel-render-fragment-members.mjs";

            test("official Razor RenderFragment members provide both named slot callbacks", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, slotPanel);
                assert.equal(typeof panel.children.header, "function");
                assert.equal(typeof panel.children.footer, "function");

                const header = panel.children.header()[0];
                assert.equal(header.name, "strong");
                assert.equal(header.props["data-summary-part"], "header");
                assert.deepEqual(header.children, ["June deployment"]);

                const footer = panel.children.footer()[0];
                assert.equal(footer.name, "small");
                assert.equal(footer.props["data-summary-part"], "footer");
                assert.deepEqual(footer.children, ["Ready"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-render-fragment-members.mjs"] = "export default { name: \"slot-panel-render-fragment-members\" };"
            });
    }
}
