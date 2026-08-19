namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialOptionalEventCallbackRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorOptionalEventCallback_HandlesMissingAndAsyncListenersOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Components/DismissiblePanel.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="DismissAsync" data-dismiss-count="@DismissCount">Dismiss</button>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Components;

            [ECMAScriptModule("./components/dismissible-panel-runtime")]
            public partial class DismissiblePanel : ComponentBase, IVueComponent
            {
                [Parameter]
                public EventCallback OnDismiss { get; set; }

                private int DismissCount { get; set; }

                private async Task DismissAsync()
                {
                    await OnDismiss.InvokeAsync();
                    DismissCount++;
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.DismissiblePanel");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this,",
            StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "await props.OnDismiss?.();", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.DismissCount++;", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/dismissible-panel-runtime.mjs",
            observation.ModuleText,
            "official-optional-event-callback-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/dismissible-panel-runtime.mjs";

            test("official Razor EventCallback remains optional and awaits a subscribed listener", async () => {
                const withoutListener = component.setup({}, { slots: {} });
                assert.equal(withoutListener().props["data-dismiss-count"], 0);

                await withoutListener().props.onClick();
                assert.equal(withoutListener().props["data-dismiss-count"], 1);

                let releaseListener;
                const calls = [];
                const withListener = component.setup({
                    OnDismiss: async () => {
                        calls.push("started");
                        await new Promise(resolve => { releaseListener = resolve; });
                        calls.push("finished");
                    }
                }, { slots: {} });

                const pending = withListener().props.onClick();
                await Promise.resolve();
                assert.deepEqual(calls, ["started"]);
                assert.equal(withListener().props["data-dismiss-count"], 0);

                releaseListener();
                await pending;

                assert.deepEqual(calls, ["started", "finished"]);
                assert.equal(withListener().props["data-dismiss-count"], 1);
            });
            """);
    }
}
