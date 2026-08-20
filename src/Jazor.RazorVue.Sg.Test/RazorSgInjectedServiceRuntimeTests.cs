namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Proves that ordinary Blazor [Inject] properties stay ordinary authored properties while the
/// generated Vue setup owns only the browser provider lookup and activation error.
/// </summary>
[TestClass]
public sealed class RazorSgInjectedServiceRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_InjectsBrowserServiceBeforeInitializedLifecycle()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/InjectedServiceRuntime.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScript]
            public sealed class BrowserClock
            {
                public string Label { get; set; } = "unset";
            }

            [ECMAScriptModule("./components/injected-service-runtime")]
            public partial class InjectedServiceRuntime : ComponentBase, IVueComponent
            {
                [Inject]
                public BrowserClock Clock { get; set; } = null!;

                private string log = "";

                private string Log => log;

                protected override void OnInitialized()
                {
                    log = Clock.Label;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InjectedServiceRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "inject", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "jazor:service:Demo.Pages.BrowserClock", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/injected-service-runtime.mjs",
            observation.ModuleText,
            "injected-service-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/injected-service-runtime.mjs";
            import { __serviceProvider } from "vue";

            test("Blazor [Inject] is resolved before OnInitialized", () => {
                __serviceProvider("jazor:service:Demo.Pages.BrowserClock", { Label: "browser-clock" });
                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "browser-clock");
            });
            """,
            vueRuntimeSource: """
            const providers = new Map();

            export function __serviceProvider(key, value) {
                providers.set(key, value);
            }

            export function defineComponent(options) {
                return options;
            }

            export function inject(key) {
                return providers.get(key);
            }

            export function reactive(value) {
                return value;
            }

            export function createStaticVNode(html, count) {
                return { name: "__static", props: { html, count }, children: html };
            }

            export function h(name, props, children) {
                return { name, props, children };
            }

            export function openBlock() {
                return null;
            }

            export function createElementBlock(name, props, children) {
                return { name, props, children };
            }

            export function createBlock(name, props, children) {
                return { name, props, children };
            }
            """);
    }

    [TestMethod]
    public async Task BuildComponent_MissingInjectedBrowserServiceFailsActivationClearly()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/MissingInjectedServiceRuntime.razor"),
            documentText: "<p>missing</p>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;

            namespace Demo.Pages;

            public sealed class BrowserClient;

            [ECMAScriptModule("./components/missing-injected-service-runtime")]
            public partial class MissingInjectedServiceRuntime : ComponentBase, IVueComponent
            {
                [Inject]
                public BrowserClient Client { get; set; } = null!;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.MissingInjectedServiceRuntime");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/missing-injected-service-runtime.mjs",
            observation.ModuleText,
            "missing-injected-service-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/missing-injected-service-runtime.mjs";

            test("missing provider is an activation error, never undefined state", () => {
                assert.throws(
                    () => component.setup({}, { slots: {} }),
                    /could not resolve injected service.*Demo.Pages.BrowserClient/);
            });
            """,
            vueRuntimeSource: """
            export function defineComponent(options) {
                return options;
            }

            export function inject(_key) {
                return undefined;
            }

            export function reactive(value) {
                return value;
            }

            export function createStaticVNode(html, count) {
                return { name: "__static", props: { html, count }, children: html };
            }

            export function h(name, props, children) {
                return { name, props, children };
            }

            export function openBlock() {
                return null;
            }

            export function createElementBlock(name, props, children) {
                return { name, props, children };
            }

            export function createBlock(name, props, children) {
                return { name, props, children };
            }
            """);
    }
}
