namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialStaticRenderFragmentMethodRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorStaticRenderFragmentMethodGroup_ProjectsNamedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseTermsPage.razor",
            documentText:
            """
            @using Demo.Components

            <SlotHost Content="@RenderReleaseTerms" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-host-static-render-fragment-runtime")]
                public sealed class SlotHost : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#content")] public RenderFragment? Content { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-terms-static-render-fragment-runtime")]
                public partial class ReleaseTermsPage : ComponentBase, IVueComponent
                {
                    private static void RenderReleaseTerms(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "small");
                        builder.AddAttribute(1, "data-terms", "release");
                        builder.AddContent(2, "Deployment requires approval.");
                        builder.CloseElement();
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseTermsPage");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderReleaseTerms", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "content:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-terms-static-render-fragment-runtime.mjs",
            observation.ModuleText,
            "official-release-terms-static-render-fragment-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-terms-static-render-fragment-runtime.mjs";
            import slotHost from "./components/slot-host-static-render-fragment-runtime.mjs";

            test("official Razor static RenderFragment method groups provide a named slot callback", () => {
                const host = component.setup({}, { slots: {} })();
                assert.equal(host.name, slotHost);
                assert.equal(typeof host.children.content, "function");

                const terms = host.children.content()[0];
                assert.equal(terms.name, "small");
                assert.equal(terms.props["data-terms"], "release");
                assert.deepEqual(terms.children, ["Deployment requires approval."]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-host-static-render-fragment-runtime.mjs"] = "export default { name: \"slot-host-static-render-fragment-runtime\" };"
            });
    }
}
