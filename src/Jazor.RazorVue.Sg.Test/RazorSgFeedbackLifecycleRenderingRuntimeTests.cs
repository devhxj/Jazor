namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Executes feedback G27/G28 against Vue's actual scheduler and DOM renderer, rather than a
/// VNode stub. Happy DOM supplies the DOM; lifecycle registration and range ownership stay Vue's.
/// </summary>
[TestClass]
public sealed class RazorSgFeedbackLifecycleRenderingRuntimeTests
{
    private const string RealVueModule = "npm:vue@3.5.42/dist/vue.runtime.esm-browser.prod.js";

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task BuildComponent_CodeBehindLifecycleHooks_ReleaseResourcesAcrossRemount(bool inConstructor)
    {
        var registration = "Vue.OnMounted(HandleMounted); OnUnmounted(() => Unmounted());";
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/FeedbackLifecycle.razor"),
            documentText: "<p>resource owner</p>",
            codeBehindSource: $$"""
            namespace Demo.Pages;

            [ECMAScriptModule("./components/feedback-lifecycle")]
            public partial class FeedbackLifecycle : ComponentBase, IVueComponent
            {
                [Parameter] public System.Action Mounted { get; set; } = default!;
                [Parameter] public System.Action Unmounted { get; set; } = default!;
                {{(inConstructor
                    ? "public FeedbackLifecycle() { " + registration + " }"
                    : "protected override void OnInitialized() { " + registration + " }")}}
                private void HandleMounted() => Mounted();
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.FeedbackLifecycle");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/feedback-lifecycle.js",
            observation.ModuleText,
            "feedback-lifecycle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import { Window } from "npm:happy-dom@20.10.6";

            const window = new Window();
            for (const name of ["window", "document", "Node", "Element", "HTMLElement", "SVGElement"])
                globalThis[name] = name === "window" ? window : window[name];
            const { createApp, nextTick } = await import("vue");
            const { default: component } = await import("./components/feedback-lifecycle.js");

            Deno.test("each component instance registers and releases exactly its own resource", async () => {
                const host = document.createElement("main");
                document.body.append(host);
                const events = [];
                let activeResources = 0;
                const props = {
                    Mounted() { events.push("mount"); activeResources++; },
                    Unmounted() { events.push("unmount"); activeResources--; }
                };
                for (let cycle = 0; cycle < 3; cycle++) {
                    const app = createApp(component, props);
                    app.mount(host);
                    await nextTick();
                    assert.equal(activeResources, 1);
                    assert.equal(host.textContent, "resource owner");
                    app.unmount();
                    await nextTick();
                    assert.equal(activeResources, 0);
                    assert.equal(host.childNodes.length, 0);
                }
                assert.deepEqual(events, ["mount", "unmount", "mount", "unmount", "mount", "unmount"]);
                window.happyDOM.abort();
            });
            """,
            vueModuleSpecifier: RealVueModule,
            restoreNpmDependencies: true);
    }

    [TestMethod]
    public async Task BuildComponent_RawMarkupPollingBranches_KeepDomRangesAcrossRepeatedTransitionsAndRemount()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/FeedbackPolling.razor"),
            documentText:
            """
            <section id="terminal">
                @if (Phase == 0)
                {
                    <p class="loading">Loading</p>
                }
                else if (Phase == 1)
                {
                    <p class="empty">Empty</p>
                }
                else
                {
                    <pre>@Output</pre>
                    @foreach (var item in Items)
                    {
                        <p @key="item">@item</p>
                    }
                }
                <footer>@Phase</footer>
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/feedback-polling")]
            public partial class FeedbackPolling : ComponentBase, IVueComponent
            {
                [Parameter] public int Phase { get; set; }
                [Parameter] public string Output { get; set; } = string.Empty;
                [Parameter] public string[] Items { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.FeedbackPolling");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/feedback-polling.js",
            observation.ModuleText,
            "feedback-polling-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import { Window } from "npm:happy-dom@20.10.6";

            const window = new Window();
            for (const name of ["window", "document", "Node", "Element", "HTMLElement", "SVGElement"])
                globalThis[name] = name === "window" ? window : window[name];
            const { createApp, h, reactive, nextTick } = await import("vue");
            const { default: component } = await import("./components/feedback-polling.js");

            Deno.test("loading/empty/data transitions preserve each DOM range and adjacent footer", async () => {
                const host = document.createElement("main");
                document.body.append(host);
                const errors = [];
                const warnings = [];
                for (let mount = 0; mount < 3; mount++) {
                    const state = reactive({ Phase: 0, Output: "", Items: [] });
                    const app = createApp({ render: () => h(component, state) });
                    app.config.errorHandler = error => errors.push(String(error));
                    app.config.warnHandler = warning => warnings.push(warning);
                    app.mount(host);
                    for (let update = 0; update < 90; update++) {
                        // Repeat unchanged branches as well as transitions in both directions.
                        state.Phase = [0, 1, 2, 2, 0, 2, 1, 0, 1][update % 9];
                        state.Output = `line ${mount}:${update}`;
                        state.Items = ["first", "second", `tail ${update}`];
                        await nextTick();
                        const terminal = host.querySelector("#terminal");
                        assert.ok(terminal);
                        assert.equal(terminal.querySelector("footer").textContent, String(state.Phase));
                        if (state.Phase === 0) {
                            assert.equal(terminal.querySelector(".loading").textContent, "Loading");
                            assert.equal(terminal.querySelector("pre"), null);
                        } else if (state.Phase === 1) {
                            assert.equal(terminal.querySelector(".empty").textContent, "Empty");
                            assert.equal(terminal.querySelector(".loading"), null);
                            assert.equal(terminal.querySelector("pre"), null);
                        } else {
                            assert.equal(terminal.querySelector("pre").textContent, state.Output);
                            assert.deepEqual(Array.from(terminal.querySelectorAll("p"), node => node.textContent), state.Items);
                            assert.equal(terminal.querySelector(".loading"), null);
                            assert.equal(terminal.querySelector(".empty"), null);
                        }
                    }
                    app.unmount();
                    await nextTick();
                    assert.equal(host.childNodes.length, 0);
                }
                assert.deepEqual(errors, []);
                assert.deepEqual(warnings, []);
                window.happyDOM.abort();
            });
            """,
            vueModuleSpecifier: RealVueModule,
            restoreNpmDependencies: true);
    }
}
