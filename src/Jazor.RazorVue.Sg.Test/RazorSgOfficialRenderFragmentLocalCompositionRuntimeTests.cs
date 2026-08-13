namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentLocalCompositionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentFactory_ComposesLocalTemplateBeforeReturningSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseFooterComposition.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Footer="@CreateReleaseFooter()" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-footer-slot-panel-runtime")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#footer")] public RenderFragment? Footer { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-footer-composition-runtime")]
                public partial class ReleaseFooterComposition : ComponentBase, IVueComponent
                {
                    [Parameter] public string ReleaseName { get; set; } = "Payments API";

                    private RenderFragment CreateReleaseFooter()
                    {
                        RenderFragment statusPrefix = prefixBuilder =>
                            prefixBuilder.AddContent(0, "Queued: ");

                        return footerBuilder =>
                        {
                            statusPrefix(footerBuilder);
                            footerBuilder.OpenElement(1, "strong");
                            footerBuilder.AddAttribute(2, "data-role", "release-name");
                            footerBuilder.AddContent(3, ReleaseName);
                            footerBuilder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseFooterComposition");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateReleaseFooter()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "footer:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Queued: ", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "props.ReleaseName", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-footer-composition-runtime.mjs",
            observation.ModuleText,
            "official-release-footer-composition-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { Fragment } from "vue";

            import component from "./components/release-footer-composition-runtime.mjs";
            import slotPanel from "./components/release-footer-slot-panel-runtime.mjs";

            test("official Razor local RenderFragment composition produces the footer slot", () => {
                const panel = component.setup({ ReleaseName: "Accounts API" }, { slots: {} })();
                assert.equal(panel.name, slotPanel);
                assert.equal(typeof panel.children.footer, "function");

                const nodes = panel.children.footer();
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, Fragment);
                assert.equal(nodes[0].children.length, 2);
                assert.equal(nodes[0].children[0], "Queued: ");
                assert.equal(nodes[0].children[1].name, "strong");
                assert.equal(nodes[0].children[1].props["data-role"], "release-name");
                assert.equal(nodes[0].children[1].children, "Accounts API");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-footer-slot-panel-runtime.mjs"] = "export default { name: \"release-footer-slot-panel-runtime\" };"
            });
    }
}
