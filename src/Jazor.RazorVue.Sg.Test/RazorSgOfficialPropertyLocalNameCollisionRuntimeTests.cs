namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialPropertyLocalNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLocalAndComputedPropertyNameCollision_PreservesBothValuesOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComputedStatus.razor",
            documentText:
            """
            @{
                var status = "local";
            }
            <section data-kind="release-status">@status|@this.Status</section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/computed-status")]
            public partial class ComputedStatus : ComponentBase, IVueComponent
            {
                private string Status => "state";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComputedStatus");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "\"local\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "\"state\"", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/computed-status.mjs",
            observation.ModuleText,
            "official-property-local-name-collision-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/computed-status.mjs";

            test("Razor local names do not shadow computed component properties", () => {
                const section = component.setup({}, { slots: {} })();
                assert.equal(section.name, "section");
                assert.equal(section.props["data-kind"], "release-status");
                assert.deepEqual(section.children, ["local", "|", "state"]);
            });
            """);
    }
}
