namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialBooleanAttributeRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorBooleanAttribute_TracksParameterStateOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDeployButton.razor",
            documentText:
            """
            <button data-action="deploy" disabled="@IsDeploying">Deploy</button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-deploy-button")]
            public partial class ReleaseDeployButton : ComponentBase, IVueComponent
            {
                [Parameter]
                public bool IsDeploying { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDeployButton");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.IsDeploying", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-deploy-button.mjs",
            observation.ModuleText,
            "official-boolean-attribute-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-deploy-button.mjs";

            test("Razor boolean attributes follow the deployment parameter", () => {
                const idle = component.setup({ IsDeploying: false }, { slots: {} })();
                assert.equal(idle.name, "button");
                assert.equal(idle.props["data-action"], "deploy");
                assert.equal(Boolean(idle.props.disabled), false);

                const deploying = component.setup({ IsDeploying: true }, { slots: {} })();
                assert.equal(deploying.props.disabled, true);
                assert.deepEqual(deploying.children, ["Deploy"]);
            });
            """);
    }
}
