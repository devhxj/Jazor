namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialExplicitLifecycleRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_ExplicitDisposableLifecyclesRunOnDenoHostUnmount()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ExplicitLifecycleRuntime.razor",
            documentText:
            """
            <p>@Log</p>
            """,
            codeBehindSource:
            """
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/explicit-lifecycle-runtime")]
            public partial class ExplicitLifecycleRuntime : ComponentBase, IVueComponent, IDisposable, IAsyncDisposable
            {
                private string log = "";

                private string Log => log;

                void IDisposable.Dispose()
                {
                    log += "dispose|";
                }

                async ValueTask IAsyncDisposable.DisposeAsync()
                {
                    log += "disposeAsync|";
                    await Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ExplicitLifecycleRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "onUnmounted(", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("system.IDisposable", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("system.IAsyncDisposable", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/explicit-lifecycle-runtime.mjs",
            observation.ModuleText,
            "official-explicit-lifecycle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runUnmounted } from "vue";

            import component from "./components/explicit-lifecycle-runtime.mjs";

            test("official Razor explicit disposal lifecycles run during unmount", async () => {
                const render = component.setup({}, { slots: {} });
                assert.deepEqual(render().children, [""]);

                __runUnmounted();
                await Promise.resolve();

                assert.deepEqual(render().children, ["dispose|disposeAsync|"]);
            });
            """);
    }
}
