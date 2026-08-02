namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialFormNameRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorFormName_RendersFormWithoutServerOnlyMetadataOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDeploymentForm.razor",
            documentText:
            """
            <form @formname="release-deployment">
                <button type="submit">Deploy</button>
            </form>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-deployment-form")]
            public partial class ReleaseDeploymentForm : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDeploymentForm");

        StringAssert.Contains(observation.GeneratedCSharp, "AddNamedEvent", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("release-deployment", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-deployment-form.mjs",
            observation.ModuleText,
            "official-form-name-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-deployment-form.mjs";

            test("official Razor form names preserve the Vue form tree", () => {
                const form = component.setup({}, { slots: {} })();
                assert.equal(form.name, "form");
                assert.equal(form.props, null);
                assert.equal(form.children.length, 1);
                assert.equal(form.children[0].name, "__static");
                assert.equal(form.children[0].props.html, "<button type=\"submit\">Deploy</button>");
            });
            """);
    }
}
