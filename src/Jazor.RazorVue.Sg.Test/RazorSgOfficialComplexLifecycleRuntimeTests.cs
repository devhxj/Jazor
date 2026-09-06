using System;
using System.Threading.Tasks;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Exercises the lifecycle cases that cannot be established by static module shape alone:
/// rejected async hooks, cancellation and queued work after unmount, repeated render hooks, and
/// an async lifecycle/disposal race. These fixtures enter through the official Razor source
/// generator.
/// </summary>
[TestClass]
public sealed class RazorSgOfficialComplexLifecycleRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_AsyncInitializationFailureReachesNextRender()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/AsyncInitializationFailure.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/async-initialization-failure")]
            public partial class AsyncInitializationFailure : ComponentBase, IVueComponent
            {
                private string Log { get; set; } = "created|";

                protected override async Task OnInitializedAsync()
                {
                    Log += "started|";
                    await Task.CompletedTask;
                    // A null throw is the supported CLR-to-JS failure shape for this fixture;
                    // the lifecycle bridge must preserve the authored rejection value.
                    throw null!;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AsyncInitializationFailure");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "recordLifecycleFailure", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "lifecycleFailure", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/async-initialization-failure.mjs",
            observation.ModuleText,
            "official-async-initialization-failure.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/async-initialization-failure.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("a rejected OnInitializedAsync task is surfaced by the next render", async () => {
                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "created|started|");

                await settle();
                await settle();

                assert.throws(() => render(), error => error === null);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_SsrPrefetchAwaitsInitializationAndParameterLifecycle()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/AsyncInitializationOrder.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/async-initialization-order")]
            public partial class AsyncInitializationOrder : ComponentBase, IVueComponent
            {
                private string Log { get; set; } = "";

                protected override async Task OnInitializedAsync()
                {
                    Log += "init-start|";
                    await Task.Delay(10);
                    Log += "init-done|";
                }

                protected override void OnParametersSet()
                    => Log += "parameters|";

                protected override async Task OnParametersSetAsync()
                {
                    await Task.Delay(10);
                    Log += "parameters-async|";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AsyncInitializationOrder");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/async-initialization-order.mjs",
            observation.ModuleText,
            "official-async-initialization-order.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runServerPrefetch } from "vue";

            import component from "./components/async-initialization-order.mjs";

            test("SSR prefetch awaits the complete initial lifecycle chain", async () => {
                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "init-start|");

                await __runServerPrefetch();

                assert.equal(render().children, "init-start|init-done|parameters|parameters-async|");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_SynchronousInitializationThrowIsCapturedForNextRender()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SynchronousInitializationThrow.razor"),
            documentText: "<p>failure</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/synchronous-initialization-throw")]
            public partial class SynchronousInitializationThrow : ComponentBase, IVueComponent
            {
                protected override Task OnInitializedAsync()
                    => throw null!;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SynchronousInitializationThrow");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/synchronous-initialization-throw.mjs",
            observation.ModuleText,
            "official-synchronous-initialization-throw.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/synchronous-initialization-throw.mjs";

            test("a synchronous lifecycle throw is captured instead of escaping setup", async () => {
                const render = component.setup({}, { slots: {} });
                await new Promise(resolve => setTimeout(resolve, 0));
                assert.throws(() => render(), error => error === null);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_CanceledParameterLifecycleAfterUnmountDoesNotInvalidate()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CanceledParameterLifecycle.razor"),
            documentText: "<p>canceled</p>",
            codeBehindSource:
            """
            using System.Threading;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/canceled-parameter-lifecycle")]
            public partial class CanceledParameterLifecycle : ComponentBase, IVueComponent, System.IDisposable
            {
                [Parameter]
                public CancellationToken Cancellation { get; set; }

                protected override Task OnParametersSetAsync()
                    => Task.Delay(25, Cancellation);

                public void Dispose()
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CanceledParameterLifecycle");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "parametersSetAsyncGen", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "if (disposed) {", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/canceled-parameter-lifecycle.mjs",
            observation.ModuleText,
            "official-canceled-parameter-lifecycle.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runUnmounted } from "vue";

            import component from "./components/canceled-parameter-lifecycle.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("cancellation caused by unmount is consumed without a post-dispose render", async () => {
                const controller = new AbortController();
                const render = component.setup({ Cancellation: controller.signal }, { slots: {} });
                assert.equal(render().children, "<p>canceled</p>");

                __runUnmounted();
                controller.abort();
                await settle();
                await settle();

                assert.doesNotThrow(() => render());
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_QueuedParameterLifecycleDoesNotStartAfterUnmount()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/QueuedParameterLifecycle.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/queued-parameter-lifecycle")]
            public partial class QueuedParameterLifecycle : ComponentBase, IVueComponent, System.IDisposable
            {
                [Parameter]
                public int Value { get; set; }

                private string Log { get; set; } = "";

                protected override async Task OnParametersSetAsync()
                {
                    Log += Value == 1 ? "first-start|" : "second-start|";
                    await Task.Delay(25);
                    Log += "done|";
                }

                public void Dispose()
                    => Log += "dispose|";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.QueuedParameterLifecycle");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "if (disposed) {", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/queued-parameter-lifecycle.mjs",
            observation.ModuleText,
            "official-queued-parameter-lifecycle.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runUnmounted, __runWatchers } from "vue";

            import component from "./components/queued-parameter-lifecycle.mjs";

            const settle = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));

            test("a queued parameter lifecycle does not enter authored code after unmount", async () => {
                const props = { Value: 1 };
                const render = component.setup(props, { slots: {} });

                await settle(0);
                assert.equal(render().children, "first-start|");

                props.Value = 2;
                __runWatchers();
                __runUnmounted();

                await settle(50);

                assert.equal(render().children, "first-start|dispose|done|");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_StaleParameterLifecycleFailureStillReachesNextRender()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/StaleParameterFailure.razor"),
            documentText: "<p>failure</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/stale-parameter-failure")]
            public partial class StaleParameterFailure : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Value { get; set; }

                protected override async Task OnParametersSetAsync()
                {
                    var value = Value;
                    await Task.Delay(value == 1 ? 25 : 0);
                    if (value == 1)
                        throw null!;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.StaleParameterFailure");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "recordLifecycleFailure", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/stale-parameter-failure.mjs",
            observation.ModuleText,
            "official-stale-parameter-failure.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/stale-parameter-failure.mjs";

            const settle = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));

            test("a stale parameter rejection is still surfaced after a newer generation starts", async () => {
                const props = { Value: 1 };
                const render = component.setup(props, { slots: {} });

                await settle(0);
                props.Value = 2;
                __runWatchers();

                await settle(50);

                assert.throws(() => render(), error => error === null);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_RepeatedRenderDoesNotRepeatAfterRenderAsyncHook()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/RepeatedAfterRender.razor"),
            documentText: "<p>@Count</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/repeated-after-render")]
            public partial class RepeatedAfterRender : ComponentBase, IVueComponent
            {
                private int Count { get; set; }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    Count++;
                    return Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.RepeatedAfterRender");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "onMounted(() => {", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onUpdated(() => {", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "recordLifecycleFailure(error)", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/repeated-after-render.mjs",
            observation.ModuleText,
            "official-repeated-after-render.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runMounted, __runUpdated } from "vue";

            import component from "./components/repeated-after-render.mjs";

            test("render calls do not replay OnAfterRenderAsync; Vue hooks do", async () => {
                const render = component.setup({}, { slots: {} });
                assert.deepEqual(render().children, [0]);
                assert.deepEqual(render().children, [0]);

                await __runMounted();
                assert.deepEqual(render().children, [1]);

                await __runUpdated();
                await new Promise(resolve => setTimeout(resolve, 0));
                await __runUpdated();
                await new Promise(resolve => setTimeout(resolve, 0));
                await new Promise(resolve => setTimeout(resolve, 0));
                // Each Vue flush admits one callback. The callback's own state mutation is
                // consumed by the same flush and does not recursively schedule another hook.
                assert.deepEqual(render().children, [2]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_AsyncLifecycleCompletionAfterAsyncUnmountIsIgnored()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/AsyncUnmountRace.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/async-unmount-race")]
            public partial class AsyncUnmountRace : ComponentBase, IVueComponent, IAsyncDisposable
            {
                private string Log { get; set; } = "";

                protected override async Task OnInitializedAsync()
                {
                    Log += "init-start|";
                    await Task.Delay(10);
                    Log += "init-done|";
                }

                public async ValueTask DisposeAsync()
                {
                    Log += "dispose-start|";
                    await Task.Delay(10);
                    Log += "dispose-done|";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AsyncUnmountRace");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "disposed = true;", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "recordLifecycleFailure", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/async-unmount-race.mjs",
            observation.ModuleText,
            "official-async-unmount-race.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runUnmounted } from "vue";

            import component from "./components/async-unmount-race.mjs";

            const settle = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));

            test("pending lifecycle completion and async disposal settle after unmount", async () => {
                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "init-start|");

                __runUnmounted();
                __runUnmounted();
                assert.equal(render().children, "init-start|dispose-start|");

                await settle(35);

                assert.equal(render().children, "init-start|dispose-start|init-done|dispose-done|");
            });
            """);
    }
}
