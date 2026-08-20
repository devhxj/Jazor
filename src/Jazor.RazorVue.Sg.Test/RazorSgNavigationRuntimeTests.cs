namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Verifies that ordinary NavigationManager authoring uses the browser service adapter without
/// exposing Vue Router or a page-side JavaScript bridge.
/// </summary>
[TestClass]
public sealed class RazorSgNavigationRuntimeTests
{
    [TestMethod]
    public async Task NavigationManager_UsesBrowserAdapterForUriAndNavigateTo()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NavigationRuntime.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="Go">@Current</button>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/navigation-runtime")]
            public partial class NavigationRuntime : ComponentBase, IVueComponent
            {
                [Inject]
                public NavigationManager Navigation { get; set; } = null!;

                private string Current { get; set; } = "unset";

                protected override void OnInitialized()
                {
                    Current = Navigation.ToBaseRelativePath(Navigation.Uri) + "|" +
                        Navigation.GetUriWithQueryParameter("filter", "open") + "|" +
                        Navigation.GetUriWithFragment("details");
                    Navigation.ToAbsoluteUri("orders");
                }

                private void Go()
                {
                    Navigation.NavigateTo("/orders?id=1", false, true);
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NavigationRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "getUri(", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "toBaseRelativePath(", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "navigateToForceLoadReplace(", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/navigation-runtime.mjs",
            observation.ModuleText,
            "navigation-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/navigation-runtime.mjs";
            import { __serviceProvider } from "vue";

            test("NavigationManager maps to the browser service", () => {
                const calls = [];
                if (typeof URL.parse !== "function")
                    URL.parse = (value, base) => {
                        try { return new URL(value, base); }
                        catch { return null; }
                    };
                const browserLocation = {
                    origin: "https://example.test",
                    href: "https://example.test/orders/42",
                    pathname: "/orders/42",
                    search: "",
                    hash: "",
                    assign(uri) { calls.push(["assign", uri]); },
                    replace(uri) { calls.push(["replace", uri]); },
                };
                class BrowserHistory {
                    state = null;
                    replaceState(state, _title, target) {
                        this.state = state;
                        calls.push(["replaceState", target]);
                    }
                    pushState(state, _title, target) {
                        this.state = state;
                        calls.push(["pushState", target]);
                    }
                }
                const browserHistory = new BrowserHistory();
                globalThis.History = BrowserHistory;
                globalThis.window = { location: browserLocation, history: browserHistory };
                globalThis.location = browserLocation;
                globalThis.history = browserHistory;
                globalThis.document = {
                    querySelector() { return { getAttribute() { return "/"; } }; },
                };
                __serviceProvider("jazor:service:Microsoft.AspNetCore.Components.NavigationManager", {
                    uri: "https://example.test/orders/42",
                });

                const render = component.setup({}, { slots: {} });
                const vnode = render();
                assert.equal(vnode.children, "orders/42|https://example.test/orders/42?filter=open|https://example.test/orders/42#details");
                vnode.props.onClick();
                assert.deepEqual(calls, [["replaceState", "/orders?id=1"]]);
            });
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["@jazor/vue-runtime/routes.mjs"] = "export const routes = [];\n"
            },
            vueRuntimeSource: """
            const providers = new Map();
            export function __serviceProvider(key, value) { providers.set(key, value); }
            export function defineComponent(options) { return options; }
            export function inject(key) { return providers.get(key); }
            export function provide() {}
            export function reactive(value) { return value; }
            export function onUnmounted() {}
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function h(name, props, children) { return { name, props, children }; }
            """
        );
    }

    [TestMethod]
    public async Task RoutingAdapter_UsesBaseHrefReplaceStateAndLocationChanged()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/RouterHost.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Routing

            <Router AppAssembly="@typeof(Program).Assembly">
                <Found Context="routeData"><p>found</p></Found>
            </Router>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;

            namespace Demo.Pages;

            public sealed class Program;

            [ECMAScriptModule("./components/router-host")]
            public partial class RouterHost : ComponentBase, IVueComponent;
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.RouterHost");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/router-host.mjs",
            observation.ModuleText,
            "router-host.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/router-host.mjs";
            import { Router } from "@jazor/vue-runtime/blazor-routing.mjs";
            import { __getProvider } from "vue";

            test("routing adapter preserves base URI and history/event semantics", () => {
                const listeners = {};
                const historyCalls = [];
                globalThis.location = {
                    origin: "https://example.test",
                    href: "https://example.test/app/start",
                    pathname: "/app/start",
                    search: "",
                    hash: "",
                };
                globalThis.document = {
                    querySelector() { return { getAttribute() { return "/app/"; } }; },
                };
                globalThis.addEventListener = (name, callback) => { listeners[name] = callback; };
                globalThis.removeEventListener = () => {};
                class BrowserHistory {
                    constructor() {
                        this.state = null;
                    }
                    replaceState(state, _title, target) {
                        this.state = state;
                        historyCalls.push(["replace", target, state]);
                        globalThis.location.pathname = target.split("?")[0];
                        globalThis.location.search = target.includes("?") ? "?" + target.split("?")[1] : "";
                        globalThis.location.href = "https://example.test" + target;
                    }
                    pushState(state, _title, target) {
                        this.state = state;
                        historyCalls.push(["push", target, state]);
                        globalThis.location.pathname = target.split("?")[0];
                        globalThis.location.search = target.includes("?") ? "?" + target.split("?")[1] : "";
                        globalThis.location.href = "https://example.test" + target;
                    }
                }
                if (typeof URL.parse !== "function")
                    URL.parse = (value, base) => {
                        try { return new URL(value, base); }
                        catch { return null; }
                    };
                const browserHistory = new BrowserHistory();
                globalThis.History = BrowserHistory;
                globalThis.history = browserHistory;
                globalThis.window = { location: globalThis.location, history: browserHistory };

                component.setup({}, { slots: {} });
                Router.setup({}, { slots: {} });
                const navigation = __getProvider("jazor:service:Microsoft.AspNetCore.Components.NavigationManager");
                assert.equal(navigation.baseUri, "https://example.test/app/");
                assert.equal(navigation.toBaseRelativePath(navigation.uri), "start");

                const events = [];
                navigation.addLocationChanged((_sender, args) => events.push(args));
                navigation.navigateTo("/app/next?x=1", { replace: true });
                assert.deepEqual(historyCalls, [["replace", "/app/next?x=1", null]]);
                assert.equal(events[0].location, "https://example.test/app/next?x=1");
                assert.equal(events[0].isNavigationIntercepted, true);
                assert.equal(events[0].historyEntryState, null);

                navigation.navigateTo("/app/state", { historyEntryState: "detail" });
                assert.deepEqual(historyCalls[1], ["push", "/app/state", "detail"]);
                assert.equal(events[1].historyEntryState, "detail");

                globalThis.location.pathname = "/app/back";
                globalThis.location.search = "";
                globalThis.location.href = "https://example.test/app/back";
                listeners.popstate();
                assert.equal(events[2].location, "https://example.test/app/back");
                assert.equal(events[2].isNavigationIntercepted, false);
            });
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["@jazor/vue-runtime/routes.mjs"] = "export const routes = [];\n"
            },
            vueRuntimeSource: """
            const providers = new Map();
            export function __getProvider(key) { return providers.get(key); }
            export const Fragment = Symbol("Fragment");
            export function defineComponent(options) { return options; }
            export function reactive(value) { return value; }
            export function provide(key, value) { providers.set(key, value); }
            export function inject(key, fallback) { return providers.get(key) ?? fallback; }
            export function onUnmounted() {}
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function createCommentVNode(text) { return { name: "__comment", children: text }; }
            export function openBlock() {}
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function withCtx(slot) { return slot; }
            export function h(name, props, children) { return { name, props, children }; }
            """
        );
    }

    [TestMethod]
    public async Task NavigationManager_LocationChangedSubscription_UsesStandardEventShape()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/LocationChanged.razor"),
            documentText: "<p>@Current</p>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/location-changed")]
            public partial class LocationChanged : ComponentBase, IVueComponent, IDisposable
            {
                [Inject]
                public NavigationManager Navigation { get; set; } = null!;

                private string Current { get; set; } = "unset";

                protected override void OnInitialized()
                {
                    Navigation.LocationChanged += OnLocationChanged;
                }

                private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
                {
                    Current = args.Location;
                }

                public void Dispose()
                {
                    Navigation.LocationChanged -= OnLocationChanged;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LocationChanged");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, ".addLocationChanged", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, ".removeLocationChanged", StringComparison.Ordinal);
    }
}
