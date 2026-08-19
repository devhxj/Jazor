namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialDirectImportNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorMemberAndLibraryExportNamesCollide_UsesStableFallbackNameOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/DirectImportNameCollision.razor"),
            documentText:
            """
            @using Demo.Library
            @using Microsoft.AspNetCore.Components.Web

            <ReleasePanel Status="@Status" OnClick="QueueRelease" />
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/direct-import-name-collision")]
            public partial class DirectImportNameCollision : ComponentBase, IVueComponent
            {
                private string Status { get; set; } = "ready";

                private void QueueRelease()
                {
                    Status = "queued";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DirectImportNameCollision",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/ReleasePanel.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [VueLibraryComponent("demo-release-library", "queueRelease")]
                public sealed class ReleasePanel : ComponentBase
                {
                    [Parameter, System.ComponentModel.Description("@#status")] public string Status { get; set; } = string.Empty;
                    [Parameter, System.ComponentModel.Description("@#onClick")] public EventCallback OnClick { get; set; }
                }
                """
            });

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::Demo.Library.ReleasePanel>", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { queueRelease } from \"demo-release-library\";", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "function QueueRelease()", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onClick: QueueRelease", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("function queueRelease(", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/direct-import-name-collision.mjs",
            observation.ModuleText,
            "official-direct-import-name-collision-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/direct-import-name-collision.mjs";

            test("official Razor members do not shadow direct-render imports", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name.name, "release-panel");
                assert.equal(initial.props.status, "ready");
                assert.equal(typeof initial.props.onClick, "function");

                initial.props.onClick();

                assert.equal(render().props.status, "queued");
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/demo-release-library/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/demo-release-library/index.mjs"] = """
                export const queueRelease = { name: "release-panel" };
                """
            });
    }
}
