import * as _77f2a60e from "./vendor/vue3/3.5.13/dist/vue.runtime.esm-browser.prod.js";
import * as _83e47fb1 from "./vendor/vue-router/4.6.4/dist/vue-router.esm-browser.prod.js";
const __m = { 0: (module, exports, require) => {
  module.exports = _77f2a60e.__esModule ? _77f2a60e : Object.assign({}, _77f2a60e.default, _77f2a60e);
}, 1: (module, exports, require) => {
  module.exports = _83e47fb1.__esModule ? _83e47fb1 : Object.assign({}, _83e47fb1.default, _83e47fb1);
}, 3: (module, exports, require) => {
  const { BlockedView: BlockedView, DetailView: DetailView, HomeView: HomeView, QueryView: QueryView } = require(2);
  const { ref: ref } = require(0);
  const { createMemoryHistory: createMemoryHistory, createRouter: createRouter, loadRouteLocation: loadRouteLocation } = require(1);
  let GlobalGuardLog = ref("guard:idle");
  exports.GlobalGuardLog = GlobalGuardLog;
  let AfterEachLog = ref("after:idle");
  exports.AfterEachLog = AfterEachLog;
  let ErrorLog = ref("error:none");
  exports.ErrorLog = ErrorLog;
  let ComponentGuardLog = ref("component:idle");
  exports.ComponentGuardLog = ComponentGuardLog;
  let LastLoadedPath = ref("");
  exports.LastLoadedPath = LastLoadedPath;
  function CreateRouterRuntime() {
    GlobalGuardLog.value = "guard:idle";
    AfterEachLog.value = "after:idle";
    ErrorLog.value = "error:none";
    ComponentGuardLog.value = "component:idle";
    LastLoadedPath.value = "";
    let detailProps = (to) => {
      return { id: to.path.replaceAll("/users/", ""), source: "route-props" };
    };
    let queryProps = (to) => {
      return { tab: to.fullPath.replaceAll("/query?tab=", "").replaceAll("#focus", "") };
    };
    let legacyRedirect = (to, from) => {
      return { path: "/users/11", query: { via: "legacy-redirect" }, hash: "#relay" };
    };
    let router = createRouter({ history: createMemoryHistory("/memory-smoke"), linkActiveClass: "route-link--active", linkExactActiveClass: "route-link--exact", routes: [{ path: "/", name: "home", component: HomeView, meta: { section: "home", requiresAudit: true } }, { path: "/users/:id", name: "detail", component: DetailView, props: detailProps, beforeEnter: (to, from) => {
      GlobalGuardLog.value = "beforeEnter:" + from.path + "->" + to.path;
      return true;
    } }, { path: "/query", name: "query", component: QueryView, props: queryProps }, { path: "/legacy/:id", redirect: legacyRedirect }, { path: "/blocked", name: "blocked", component: BlockedView }] });
    router.beforeEach((to, from) => {
      GlobalGuardLog.value = "beforeEach:" + from.path + "->" + to.path;
      if (to.path === "/blocked") {
        GlobalGuardLog.value = "beforeEach:blocked";
        return false;
      }
      return true;
    });
    router.beforeResolve((to, from) => {
      GlobalGuardLog.value = "beforeResolve:" + from.path + "->" + to.path;
      return Promise.resolve(true);
    });
    router.afterEach((to, from, failure) => {
      AfterEachLog.value = "afterEach:" + from.path + "->" + to.path + ":" + (failure == null ? "ok" : "failure");
      return;
    });
    router.onError((error, to, from) => {
      ErrorLog.value = error.message + "|" + from.path + "->" + to.path;
      return;
    });
    return router;
  }
  exports.CreateRouterRuntime = CreateRouterRuntime;
  function Snapshot(router) {
    let currentRoute = router.currentRoute.value;
    return { currentPath: currentRoute.path, currentFullPath: currentRoute.fullPath, globalGuard: GlobalGuardLog.value, afterEach: AfterEachLog.value, componentGuard: ComponentGuardLog.value, loadedPath: LastLoadedPath.value, isReady: router.listening };
  }
  exports.Snapshot = Snapshot;
  function NavigateScenario(router) {
    return router.push({ name: "detail", params: { id: "42" }, query: { via: "scenario" }, hash: "#summary" }).then(() => {
      return loadRouteLocation(router.currentRoute.value);
    }).then((loaded) => {
      LastLoadedPath.value = loaded.path;
      return router.replace("/query?tab=summary#focus");
    }).then(() => {
      return Snapshot(router);
    });
  }
  exports.NavigateScenario = NavigateScenario;
}, 2: (module, exports, require) => {
  const { ComponentGuardLog: ComponentGuardLog } = require(3);
  const { computed: computed, defineComponent: defineComponent, h: h, inject: inject, toRef: toRef, triggerRef: triggerRef } = require(0);
  const { matchedRouteKey: matchedRouteKey, onBeforeRouteLeave: onBeforeRouteLeave, onBeforeRouteUpdate: onBeforeRouteUpdate, routerViewLocationKey: routerViewLocationKey, useLink: useLink, useRoute: useRoute, useRouter: useRouter, viewDepthKey: viewDepthKey } = require(1);
  let HomeView = defineComponent({ name: "MemorySmokeHomeView", setup: CreateHomeSetup });
  exports.HomeView = HomeView;
  let DetailView = defineComponent({ name: "MemorySmokeDetailView", props: ["id", "source"], setup: CreateDetailSetup });
  exports.DetailView = DetailView;
  let QueryView = defineComponent({ name: "MemorySmokeQueryView", props: ["tab"], setup: CreateQuerySetup });
  exports.QueryView = QueryView;
  let BlockedView = defineComponent({ name: "MemorySmokeBlockedView", setup: () => {
    return () => {
      return h("section", { class: "route-card route-card--blocked" }, [h("h2", "Blocked"), h("p", "Navigation should not land here while the guard is active.")]);
    };
  } });
  exports.BlockedView = BlockedView;
  let RouteProbe = defineComponent({ name: "MemorySmokeRouteProbe", setup: CreateProbeSetup });
  exports.RouteProbe = RouteProbe;
  function CreateHomeSetup() {
    let router = useRouter();
    let route = useRoute();
    let greeting = computed(() => {
      return "home:" + route.name;
    });
    let navigateToDetail = () => {
      router.push({ name: "detail", params: { id: "7" }, query: { via: "button" }, hash: "#summary" });
      return;
    };
    return () => {
      return h("section", { class: "route-card route-card--home" }, [h("h2", "Home"), h("p", "current name: " + route.name), h("p", "current path: " + route.path), h("p", "message: " + greeting.value), CreateButton("Go detail", "action-button action-button--accent", navigateToDetail)]);
    };
  }
  function CreateDetailSetup(props, context) {
    let router = useRouter();
    let route = useRoute();
    let viewDepth = inject(viewDepthKey);
    let matchedRoute = inject(matchedRouteKey);
    let routedLocation = inject(routerViewLocationKey);
    let composedLink = useLink({ to: toRef(() => {
      return { name: "query", query: { tab: props.id + "-details" } };
    }), replace: computed(() => {
      return false;
    }) });
    onBeforeRouteUpdate((to, from) => {
      ComponentGuardLog.value = "update:" + from.path + "->" + to.path;
      return true;
    });
    onBeforeRouteLeave((to, from) => {
      ComponentGuardLog.value = "leave:" + from.path + "->" + to.path;
      return true;
    });
    let pushBlocked = () => {
      router.push("/blocked");
      return;
    };
    let followComposedLink = () => {
      composedLink.navigate();
      return;
    };
    return () => {
      return h("section", { class: "route-card route-card--detail" }, [h("h2", "Detail " + props.id), h("p", "source: " + props.source), h("p", "route path: " + route.path), h("p", "query via: " + (route.query["via"] ?? "")), h("p", "matched path: " + (matchedRoute.value == null ? "" : matchedRoute.value.path)), h("p", "view depth: " + viewDepth), h("p", "injected route path: " + routedLocation.value.path), h("p", "useLink href: " + composedLink.href.value), h("div", { class: "route-actions" }, [CreateButton("Blocked target", "action-button", pushBlocked), CreateButton("Follow composed link", "action-button action-button--accent", followComposedLink)]), h("div", { class: "route-slot-probe" }, [h("strong", "slot keys visible: "), h("span", context.slots.default == null ? "none" : "default")])]);
    };
  }
  function CreateQuerySetup(props, context) {
    context;
    let route = useRoute();
    return () => {
      return h("section", { class: "route-card route-card--query" }, [h("h2", "Query"), h("p", "tab prop: " + props.tab), h("p", "hash: " + route.hash), h("p", "fullPath: " + route.fullPath)]);
    };
  }
  function CreateProbeSetup() {
    let route = useRoute();
    let router = useRouter();
    let currentRoute = router.currentRoute;
    triggerRef(currentRoute);
    return () => {
      return h("aside", { class: "route-probe" }, [h("p", "probe current route: " + currentRoute.value.fullPath), h("p", "probe useRoute path: " + route.path)]);
    };
  }
  function CreateButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 4: (module, exports, require) => {
  const { RouteProbe: RouteProbe } = require(2);
  const { AfterEachLog: AfterEachLog, ComponentGuardLog: ComponentGuardLog, CreateRouterRuntime: CreateRouterRuntime, GlobalGuardLog: GlobalGuardLog, LastLoadedPath: LastLoadedPath } = require(3);
  const { createApp: createApp, defineComponent: defineComponent, h: h } = require(0);
  const { RouterLink: RouterLink, RouterView: RouterView } = require(1);
  function CreateConfiguredApp() {
    let router = CreateRouterRuntime();
    return m$9e51e1bf30db08e6(router);
  }
  exports.CreateConfiguredApp = CreateConfiguredApp;
  function Boot(selector) {
    let router = CreateRouterRuntime();
    let app = m$9e51e1bf30db08e6(router);
    router.isReady().then(() => {
      app.mount(selector);
      return;
    });
  }
  exports.Boot = Boot;
  function m$9e51e1bf30db08e6(router) {
    let app = createApp(defineComponent({ name: "VueRouteMemorySmokeRoot", render: RenderRoot }));
    app.use(router);
    router.push("/");
    app.onUnmount(() => {
      router.clearRoutes();
      return;
    });
    return app;
  }
  function CreateNavLinkSlots(label) {
    return { default: (scope) => {
      return [h("span", label + " " + scope.href)];
    } };
  }
  function RenderRoot() {
    return h("main", { class: "route-root" }, [h("section", { class: "route-hero" }, [h("p", { class: "route-kicker" }, "ECMAScript.VueRoute production sample"), h("h1", { class: "route-title" }, "Typed Vue Router authoring, guards, links, router-view slots, and runtime smoke coverage"), h("p", { class: "route-copy" }, "The sample keeps Vue Router as a normal external runtime while exercising strongly typed route objects, route props, global and component guards, useLink(), RouterLink, RouterView, injection keys, and loadRouteLocation().")]), h(RouteProbe), h("nav", { class: "route-nav" }, [h(RouterLink, { to: "/", activeClass: "route-link--active", exactActiveClass: "route-link--exact" }, CreateNavLinkSlots("Home")), h(RouterLink, { to: { name: "detail", params: { id: "5" }, query: { via: "nav" } }, activeClass: "route-link--active", exactActiveClass: "route-link--exact" }, CreateNavLinkSlots("Detail 5")), h(RouterLink, { to: { name: "query", query: { tab: "overview" } }, activeClass: "route-link--active", exactActiveClass: "route-link--exact" }, CreateNavLinkSlots("Query"))]), h("section", { class: "route-state" }, [h("p", "global guard: " + GlobalGuardLog.value), h("p", "afterEach: " + AfterEachLog.value), h("p", "component guard: " + ComponentGuardLog.value), h("p", "loaded path: " + LastLoadedPath.value)]), h(RouterView)]);
  }
}, 5: (module, exports, require) => {
  const { CreateRouterRuntime: CreateRouterRuntime, GlobalGuardLog: GlobalGuardLog, NavigateScenario: NavigateScenario } = require(3);
  const { loadRouteLocation: loadRouteLocation } = require(1);
  function CreateTestingRouter() {
    return CreateRouterRuntime();
  }
  exports.CreateTestingRouter = CreateTestingRouter;
  function RunScenario() {
    let router = CreateTestingRouter();
    return NavigateScenario(router);
  }
  exports.RunScenario = RunScenario;
  function NavigateLegacyRedirect() {
    let router = CreateTestingRouter();
    return router.push("/legacy/11#relay").then(() => {
      return loadRouteLocation(router.currentRoute.value);
    }).then((loaded) => {
      return loaded.fullPath;
    });
  }
  exports.NavigateLegacyRedirect = NavigateLegacyRedirect;
  function NavigateBlockedPath() {
    let router = CreateTestingRouter();
    return router.push("/").then(() => {
      return router.push("/blocked");
    }).then((_) => {
      return router.currentRoute.value.path + "|" + GlobalGuardLog.value;
    });
  }
  exports.NavigateBlockedPath = NavigateBlockedPath;
}, 6: (module, exports, require) => {
  Object.assign(exports, require(2));
  Object.assign(exports, require(4));
  Object.assign(exports, require(3));
  Object.assign(exports, require(5));
} };
var __c = {};
function __r(id) {
  var mod = __c[id];
  if (mod)
    return mod.exports;
  mod = __c[id] = { exports: {} };
  __m[id](mod, mod.exports, __r);
  var e = mod.exports;
  if (e && (typeof e == "object" || typeof e == "function") && e.default === void 0)
    e.default = e;
  return e;
}
export default __r(6);
//# sourceMappingURL=bundle.js.map
