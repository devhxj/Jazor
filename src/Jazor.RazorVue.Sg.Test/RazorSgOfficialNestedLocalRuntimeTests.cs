namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNestedLocalRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNestedCompileTimeLocal_InlinesTheValueInsideTheOpenElementFrameOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDetailsLabel.razor",
            documentText:
            """
            <section data-region="release-details">
                @{
                    const string label = "Release details";
                }
                <span data-label="@label">@label</span>
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-details-label-nested-local-runtime")]
            public partial class ReleaseDetailsLabel : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDetailsLabel");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "data-label", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("const label", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-details-label-nested-local-runtime.mjs",
            observation.ModuleText,
            "official-release-details-label-nested-local-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-details-label-nested-local-runtime.mjs";

            test("official Razor nested compile-time locals preserve their markup position", () => {
                const root = component.setup({}, { slots: {} })();
                assert.equal(root.name, "section");
                assert.equal(root.props["data-region"], "release-details");
                assert.equal(root.children.length, 1);
                assert.equal(root.children[0].name, "span");
                assert.equal(root.children[0].props["data-label"], "Release details");
                assert.deepEqual(root.children[0].children, ["Release details"]);
            });
            """);
    }
}
