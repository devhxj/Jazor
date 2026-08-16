namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLocalMemberNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLocalAndStateMemberWithSameName_PreservesBothValuesOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\LocalMemberNameCollision.razor",
            documentText:
            """
            @{
                var status = "local";
            }

            <section data-kind="release-status">@status|@ReadStatus()</section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/local-member-name-collision")]
                public partial class LocalMemberNameCollision : ComponentBase, IVueComponent
                {
                    private string status = "state";

                    private string ReadStatus() => status;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LocalMemberNameCollision");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "local", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "readStatus", StringComparison.OrdinalIgnoreCase);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/local-member-name-collision.mjs",
            observation.ModuleText,
            "official-local-member-name-collision.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/local-member-name-collision.mjs";

            test("Razor local variables do not shadow component state members", () => {
                const section = component.setup({}, { slots: {} })();
                assert.equal(section.name, "section");
                assert.equal(section.props["data-kind"], "release-status");
                assert.deepEqual(section.children, [
                    { name: "__text", children: "local", patchFlag: 1 },
                    { name: "__text", children: "|", patchFlag: undefined },
                    { name: "__text", children: "state", patchFlag: 1 }
                ]);
            });
            """);
    }
}
