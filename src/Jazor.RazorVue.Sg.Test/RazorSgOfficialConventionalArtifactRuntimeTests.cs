namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialConventionalArtifactRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorWithoutModuleAttribute_UsesTheConventionalArtifactContractOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ConventionalArtifactPage.razor",
            documentText:
            """
            <main data-area="releases">
                <h1>@Title</h1>
                <p>@Status</p>
            </main>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            public partial class ConventionalArtifactPage : ComponentBase, IVueComponent
            {
                private string Title { get; } = "Release overview";

                private string Status { get; } = "Ready";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ConventionalArtifactPage");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "\"data-area\": \"releases\"", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/conventional-artifact-page-runtime.mjs",
            observation.ModuleText,
            "official-conventional-artifact-page-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/conventional-artifact-page-runtime.mjs";

            test("official Razor components without explicit module metadata retain the render artifact contract", () => {
                const page = component.setup({}, { slots: {} })();
                assert.equal(page.name, "main");
                assert.equal(page.props["data-area"], "releases");
                const heading = page.children.find(node => node?.name === "h1");
                const status = page.children.find(node => node?.name === "p");
                assert.ok(heading);
                assert.ok(status);
                assert.deepEqual(heading.children, ["Release overview"]);
                assert.deepEqual(status.children, ["Ready"]);
            });
            """);
    }
}
