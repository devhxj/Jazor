namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialConditionalEventCallbackRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalEventCallback_SelectsCurrentActionOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseActionControl.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button data-action="@LastAction" @onclick="@(IsDeploying ? RequestRollback : RequestDeploy)">Run action</button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-action-control")]
            public partial class ReleaseActionControl : ComponentBase, IVueComponent
            {
                [Parameter] public bool IsDeploying { get; set; }

                private string LastAction { get; set; } = "idle";

                private void RequestDeploy()
                {
                    LastAction = "deploy";
                }

                private void RequestRollback()
                {
                    LastAction = "rollback";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseActionControl");

        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.IsDeploying", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-action-control.mjs",
            observation.ModuleText,
            "official-conditional-event-callback-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-action-control.mjs";

            test("official Razor conditional callbacks dispatch the selected release action", async () => {
                const deployRender = component.setup({ IsDeploying: false }, { slots: {} });
                const deploy = deployRender();
                assert.equal(deploy.props["data-action"], "idle");
                await Promise.resolve(deploy.props.onClick());
                assert.equal(deployRender().props["data-action"], "deploy");

                const rollbackRender = component.setup({ IsDeploying: true }, { slots: {} });
                const rollback = rollbackRender();
                assert.equal(rollback.props["data-action"], "idle");
                await Promise.resolve(rollback.props.onClick());
                assert.equal(rollbackRender().props["data-action"], "rollback");
            });
            """);
    }
}
