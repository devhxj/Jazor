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
    public async Task NavigationManagerHost_UsesBaseHrefReplaceStateAndLocationChanged()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NavigationHost.razor"),
            documentText:
            """
            <p>@Current</p>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/navigation-host")]
            public partial class NavigationHost : ComponentBase, IVueComponent
            {
                [Inject]
                public NavigationManager Navigation { get; set; } = null!;

                private string Current { get; set; } = "unset";

                protected override void OnInitialized()
                {
                    Current = Navigation.BaseUri + "|" + Navigation.ToBaseRelativePath(Navigation.Uri);
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NavigationHost");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/navigation-host.mjs",
            observation.ModuleText,
            "navigation-host.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/navigation-host.mjs";
            import { CreateNavigationManager, navigateToForceLoadReplace } from "Microsoft/AspNetCore/Components/NavigationManagerModule.js";
            import { __getProvider, __setProvider, reactive } from "vue";

            test("NavigationManager host preserves base URI and history/event semantics", () => {
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
                globalThis.addEventListener = () => {};
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

                const navigation = CreateNavigationManager(() => {});
                __setProvider("jazor:service:Microsoft.AspNetCore.Components.NavigationManager", navigation);
                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "https://example.test/app/|start");
                assert.equal(
                    __getProvider("jazor:service:Microsoft.AspNetCore.Components.NavigationManager"),
                    navigation);
                assert.equal(navigation.baseUri, "https://example.test/app/");
                assert.equal(navigation.toBaseRelativePath(navigation.uri), "start");

                const events = [];
                navigation.addLocationChanged((_sender, args) => events.push(args));
                navigation.navigateTo("/app/next?x=1", { replaceHistoryEntry: true });
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
                navigation.notifyLocationChanged(false);
                assert.equal(events[2].location, "https://example.test/app/back");
                assert.equal(events[2].isNavigationIntercepted, false);

                // Generated handlers pass state.Navigation to imported CLR calls, so model Vue's
                // nested proxy behavior here instead of accidentally testing the raw object.
                const refreshes = [];
                const reactiveNavigation = CreateNavigationManager(() => refreshes.push("refresh"));
                const state = reactive({ Navigation: reactiveNavigation });
                assert.equal(state.Navigation, reactiveNavigation);
                navigateToForceLoadReplace(state.Navigation, "/app/proxied?x=1", false, true);
                assert.deepEqual(refreshes, ["refresh"]);
            });
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["@jazor/vue-runtime/routes.mjs"] = "export const routes = [];\n"
            },
            vueRuntimeSource: """
            const providers = new Map();
            export function __getProvider(key) { return providers.get(key); }
            export function __setProvider(key, value) { providers.set(key, value); }
            export const Fragment = Symbol("Fragment");
            export function defineComponent(options) { return options; }
            const reactiveProxies = new WeakMap();
            function wrapReactiveValue(value) {
                if (value === null || typeof value !== "object" || value.__v_skip === true)
                    return value;
                const existing = reactiveProxies.get(value);
                if (existing !== undefined)
                    return existing;
                const proxy = new Proxy(value, {
                    get(target, key, receiver) {
                        return wrapReactiveValue(Reflect.get(target, key, receiver));
                    },
                    set(target, key, next, receiver) {
                        return Reflect.set(target, key, next, receiver);
                    }
                });
                reactiveProxies.set(value, proxy);
                return proxy;
            }
            export function reactive(value) { return wrapReactiveValue(value); }
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

    [TestMethod]
    public async Task NavigationManager_NotFoundSubscription_DispatchesEventArgsAndUriMembers()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NotFoundRuntime.razor"),
            documentText: "<p>@Current</p>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/not-found-runtime")]
            public partial class NotFoundRuntime : ComponentBase, IVueComponent, IDisposable
            {
                [Inject]
                public NavigationManager Navigation { get; set; } = null!;

                private string Current { get; set; } = "unset";

                protected override void OnInitialized()
                {
                    Navigation.OnNotFound += OnNotFound;
                    Uri absolute = Navigation.ToAbsoluteUri("orders?state=open");
                    Current = absolute.Scheme + "|" + absolute.Host + "|" + absolute.Port +
                        "|" + absolute.PathAndQuery;
                    Navigation.NotFound();
                }

                private void OnNotFound(object? sender, NotFoundEventArgs args)
                {
                    Current = Current + "|not-found:" + (args.Path ?? "none");
                }

                public void Dispose()
                {
                    Navigation.OnNotFound -= OnNotFound;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NotFoundRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "addOnNotFound", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "removeOnNotFound", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "notFound", StringComparison.Ordinal);
        // System.Uri lowers to the browser URL carrier, so the absolute URI stays usable through
        // the UriModule members instead of degrading to a bare href string.
        StringAssert.Contains(observation.ModuleText, "getPathAndQuery", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "getPort", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/not-found-runtime.mjs",
            observation.ModuleText,
            "not-found-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/not-found-runtime.mjs";
            import { __serviceProvider } from "vue";
            import { CreateNavigationManager } from "Microsoft/AspNetCore/Components/NavigationManagerModule.js";

            test("OnNotFound dispatches fresh event args and Uri members resolve", () => {
                if (typeof URL.parse !== "function")
                    URL.parse = (value, base) => {
                        try { return new URL(value, base); }
                        catch { return null; }
                    };
                const browserLocation = {
                    origin: "https://example.test",
                    href: "https://example.test/app/start",
                    pathname: "/app/start",
                    search: "",
                    hash: "",
                };
                globalThis.location = browserLocation;
                globalThis.history = { state: null };
                globalThis.window = {
                    location: browserLocation,
                    history: globalThis.history,
                    addEventListener() {},
                    removeEventListener() {},
                };
                globalThis.addEventListener = () => {};
                globalThis.removeEventListener = () => {};
                globalThis.document = {
                    querySelector() { return { getAttribute() { return "/app/"; } }; },
                };
                // The real browser adapter owns the OnNotFound invocation list, so the service
                // registration uses it instead of a hand-written stub object.
                __serviceProvider(
                    "jazor:service:Microsoft.AspNetCore.Components.NavigationManager",
                    CreateNavigationManager(() => {}));

                const render = component.setup({}, { slots: {} });
                const vnode = render();
                assert.equal(
                    vnode.children,
                    "https|example.test|443|/app/orders?state=open|not-found:none");
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
    public async Task NavigationManager_LocationChangingHandler_PreventsCommitAndUnregistersOnDispose()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/LocationChangingRuntime.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/location-changing-runtime")]
            public partial class LocationChangingRuntime : ComponentBase, IVueComponent, IDisposable
            {
                [Inject]
                public NavigationManager Navigation { get; set; } = null!;

                // 注册句柄在 OnInitialized 中一定被赋值，所以不需要条件访问。
                private IDisposable Registration { get; set; } = null!;

                private int Visits { get; set; }

                private string Log { get; set; } = "unset";

                protected override void OnInitialized()
                {
                    Registration = Navigation.RegisterLocationChangingHandler(OnLocationChanging);
                }

                private ValueTask OnLocationChanging(LocationChangingContext context)
                {
                    Visits = Visits + 1;
                    Log = Log + "|" + context.TargetLocation + ":" + Visits;
                    // 第一次导航取消、第二次放行，用来把「handler 真的跑了」和
                    // 「PreventNavigation 真的挡住了提交」区分成两个可观察事实。
                    if (Visits == 1)
                        context.PreventNavigation();
                    return ValueTask.CompletedTask;
                }

                public void Dispose()
                {
                    Registration.Dispose();
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LocationChangingRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "registerLocationChangingHandler", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "preventNavigation", StringComparison.Ordinal);
        // ValueTask 与 Task 共用 Promise carrier，所以 handler 的返回值不需要额外运行时载体。
        StringAssert.Contains(observation.ModuleText, "Promise.resolve()", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/location-changing-runtime.mjs",
            observation.ModuleText,
            "location-changing-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/location-changing-runtime.mjs";
            import { __serviceProvider, __unmount } from "vue";
            import { CreateNavigationManager } from "Microsoft/AspNetCore/Components/NavigationManagerModule.js";

            test("location-changing handlers gate internal navigation and stop after dispose", async () => {
                if (typeof URL.parse !== "function")
                    URL.parse = (value, base) => {
                        try { return new URL(value, base); }
                        catch { return null; }
                    };
                const browserLocation = {
                    origin: "https://example.test",
                    href: "https://example.test/app/start",
                    pathname: "/app/start",
                    search: "",
                    hash: "",
                };
                const pushed = [];
                const browserHistory = {
                    state: null,
                    pushState(state, title, route) {
                        pushed.push(route);
                        this.state = state;
                        browserLocation.href = browserLocation.origin + route;
                        browserLocation.pathname = route;
                    },
                    replaceState(state, title, route) {
                        this.pushState(state, title, route);
                    },
                };
                globalThis.location = browserLocation;
                globalThis.history = browserHistory;
                globalThis.window = {
                    location: browserLocation,
                    history: browserHistory,
                    addEventListener() {},
                    removeEventListener() {},
                };
                globalThis.addEventListener = () => {};
                globalThis.removeEventListener = () => {};
                globalThis.document = {
                    querySelector() { return { getAttribute() { return "/app/"; } }; },
                };
                // NavigateTo 是 fire-and-forget：handler 同步启动，提交推迟到 promise 链结算之后，
                // 所以每次断言提交结果前都要让出一次宏任务。
                const drain = () => new Promise(resolve => setTimeout(resolve, 0));
                const navigation = CreateNavigationManager(() => {});
                __serviceProvider(
                    "jazor:service:Microsoft.AspNetCore.Components.NavigationManager",
                    navigation);

                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "unset");

                navigation.navigateTo("/app/blocked", false);
                assert.equal(render().children, "unset|https://example.test/app/blocked:1");
                assert.deepEqual(pushed, []);
                await drain();
                assert.deepEqual(pushed, []);

                navigation.navigateTo("/app/allowed", false);
                await drain();
                assert.deepEqual(pushed, ["/app/allowed"]);
                assert.equal(
                    render().children,
                    "unset|https://example.test/app/blocked:1|https://example.test/app/allowed:2");

                // onUnmounted 触发组件 Dispose()，句柄释放后 invocation list 为空，
                // 内部导航重新走同步提交路径。
                __unmount();
                navigation.navigateTo("/app/after-dispose", false);
                assert.deepEqual(pushed, ["/app/allowed", "/app/after-dispose"]);
                assert.equal(
                    render().children,
                    "unset|https://example.test/app/blocked:1|https://example.test/app/allowed:2");
            });
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["@jazor/vue-runtime/routes.mjs"] = "export const routes = [];\n"
            },
            vueRuntimeSource: """
            const providers = new Map();
            const unmountedHandlers = [];
            export function __serviceProvider(key, value) { providers.set(key, value); }
            export function __unmount() { unmountedHandlers.splice(0).forEach(handler => handler()); }
            export function defineComponent(options) { return options; }
            export function inject(key) { return providers.get(key); }
            export function provide() {}
            export function reactive(value) { return value; }
            export function onUnmounted(handler) { unmountedHandlers.push(handler); }
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function h(name, props, children) { return { name, props, children }; }
            """
        );
    }

    /// <summary>
    /// 后一次内部导航取代前一次仍在挂起的 location-changing dispatch。
    /// </summary>
    /// <remarks>
    /// dispatch 的提交被推迟到 handler 的异步段结算之后，所以两次导航可以同时处于挂起状态。
    /// 前一次若照常提交，会在后一次之后把过期的目标写回 history，页面地址与最终导航目标不一致。
    /// 这里用 <c>CancellationToken.Register</c> 把「前一次的 token 真的被取消」变成可观察事实，
    /// 再用 <c>pushed</c> 固定「只有最后一次导航落到 history」。
    /// </remarks>
    [TestMethod]
    public async Task NavigationManager_LocationChangingHandler_SupersededDispatchDoesNotCommit()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/LocationChangingSupersede.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Routing;
            using ECMAScript;
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/location-changing-supersede")]
            public partial class LocationChangingSupersede : ComponentBase, IVueComponent
            {
                [Inject]
                public NavigationManager Navigation { get; set; } = null!;

                private string Log { get; set; } = "unset";

                protected override void OnInitialized()
                {
                    Navigation.RegisterLocationChangingHandler(OnLocationChanging);
                }

                private ValueTask OnLocationChanging(LocationChangingContext context)
                {
                    var target = context.TargetLocation;
                    Log = Log + "|start:" + target;
                    context.CancellationToken.Register(() => Log = Log + "|canceled:" + target);
                    // handler 的异步段跨一个宏任务：第一次 dispatch 仍然挂起时第二次导航就会到来。
                    return new ValueTask(Task.Delay(0));
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LocationChangingSupersede");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "registerLocationChangingHandler", StringComparison.Ordinal);
        // dispatch 的 token 就是宿主 AbortSignal，注册走取消链模块而不是导航模块自己的表。
        StringAssert.Contains(
            observation.ModuleText,
            "System/Threading/CancellationTokenModule.js",
            StringComparison.Ordinal);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/location-changing-supersede.mjs",
            observation.ModuleText,
            "location-changing-supersede.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/location-changing-supersede.mjs";
            import { __serviceProvider } from "vue";
            import { CreateNavigationManager } from "Microsoft/AspNetCore/Components/NavigationManagerModule.js";

            test("a later internal navigation supersedes the pending location-changing dispatch", async () => {
                if (typeof URL.parse !== "function")
                    URL.parse = (value, base) => {
                        try { return new URL(value, base); }
                        catch { return null; }
                    };
                const browserLocation = {
                    origin: "https://example.test",
                    href: "https://example.test/app/start",
                    pathname: "/app/start",
                    search: "",
                    hash: "",
                };
                const pushed = [];
                const browserHistory = {
                    state: null,
                    pushState(state, title, route) {
                        pushed.push(route);
                        this.state = state;
                        browserLocation.href = browserLocation.origin + route;
                        browserLocation.pathname = route;
                    },
                    replaceState(state, title, route) {
                        this.pushState(state, title, route);
                    },
                };
                globalThis.location = browserLocation;
                globalThis.history = browserHistory;
                globalThis.window = {
                    location: browserLocation,
                    history: browserHistory,
                    addEventListener() {},
                    removeEventListener() {},
                };
                globalThis.addEventListener = () => {};
                globalThis.removeEventListener = () => {};
                globalThis.document = {
                    querySelector() { return { getAttribute() { return "/app/"; } }; },
                };
                // handler 的异步段是一个 0ms 定时器，所以让出一次宏任务足够让两次 dispatch 都结算。
                const drain = () => new Promise(resolve => setTimeout(resolve, 0));
                const navigation = CreateNavigationManager(() => {});
                __serviceProvider(
                    "jazor:service:Microsoft.AspNetCore.Components.NavigationManager",
                    navigation);

                const render = component.setup({}, { slots: {} });
                assert.equal(render().children, "unset");

                // 两次导航都在同一个同步段发起：第一次的 handler 已经跑完同步部分但还没提交。
                navigation.navigateTo("/app/first", false);
                navigation.navigateTo("/app/second", false);
                // 取代是同步发生的：第二次导航启动前就撤销了第一次的 token。
                assert.equal(
                    render().children,
                    "unset|start:https://example.test/app/first"
                    + "|canceled:https://example.test/app/first"
                    + "|start:https://example.test/app/second");
                assert.deepEqual(pushed, []);

                await drain();
                await drain();

                // 被取代的 dispatch 结算后不提交，因此 history 里只有最后一次导航的目标。
                assert.deepEqual(pushed, ["/app/second"]);
                assert.equal(browserLocation.pathname, "/app/second");
                // 第二次的 token 正常结束，不会被自己的提交撤销。
                assert.equal(
                    render().children,
                    "unset|start:https://example.test/app/first"
                    + "|canceled:https://example.test/app/first"
                    + "|start:https://example.test/app/second");
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
}
