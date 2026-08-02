namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialComponentDispatchRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentBaseDispatch_UpdatesStateThroughInvokeAsyncOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentDispatchRuntime.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="RefreshAsync" data-count="@count">Refresh</button>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/component-dispatch-runtime")]
            public partial class ComponentDispatchRuntime : ComponentBase, IVueComponent
            {
                private int count;

                private async Task RefreshAsync()
                {
                    await InvokeAsync(RefreshCoreAsync);
                }

                private async Task RefreshCoreAsync()
                {
                    count++;
                    StateHasChanged();
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentDispatchRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "invokeAsync(refreshCoreAsync)", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "stateHasChanged", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("this.", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/component-dispatch-runtime.mjs",
            observation.ModuleText,
            "official-component-dispatch-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/component-dispatch-runtime.mjs";

            test("official Razor ComponentBase dispatch awaits work and exposes updated component state", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.props["data-count"], 0);
                assert.equal(typeof initial.props.onClick, "function");

                await initial.props.onClick();

                const updated = render();
                assert.equal(updated.props["data-count"], 1);
            });
            """);
    }
}
