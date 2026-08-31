import { RouteProbe } from "components/route-shell.mjs";
import { AfterEachLog, ComponentGuardLog, CreateRouterRuntime, GlobalGuardLog, LastLoadedPath } from "router/memory-router.mjs";
import { createApp, defineComponent, h } from "vue";
import { RouterLink, RouterView } from "vue-router";
export function CreateConfiguredApp_21d4b33e13561d30() {
  let router = CreateRouterRuntime();
  return CreateConfiguredApp_9e51e1bf30db08e6(router);
}
export function Boot(selector) {
  let router = CreateRouterRuntime();
  let app = CreateConfiguredApp_9e51e1bf30db08e6(router);
  router.isReady().then(() => {
    app.mount(selector);
    return;
  });
}
function CreateConfiguredApp_9e51e1bf30db08e6(router) {
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
  return { default: scope => {
    return [h("span", label + " " + scope.href)];
  } };
}
function RenderRoot() {
  return h("main", { class: "route-root" }, [h("section", { class: "route-hero" }, [h("p", { class: "route-kicker" }, "ECMAScript.VueRoute production sample"), h("h1", { class: "route-title" }, "Typed Vue Router authoring, guards, links, router-view slots, and runtime smoke coverage"), h("p", { class: "route-copy" }, "The sample keeps Vue Router as a normal external runtime while exercising strongly typed route objects, route props, global and component guards, useLink(), RouterLink, RouterView, injection keys, and loadRouteLocation().")]), h(RouteProbe), h("nav", { class: "route-nav" }, [h(RouterLink, {
    to: "/",
    activeClass: "route-link--active",
    exactActiveClass: "route-link--exact"
  }, CreateNavLinkSlots("Home")), h(RouterLink, {
    to: {
      name: "detail",
      params: { id: "5" },
      query: { via: "nav" }
    },
    activeClass: "route-link--active",
    exactActiveClass: "route-link--exact"
  }, CreateNavLinkSlots("Detail 5")), h(RouterLink, {
    to: { name: "query", query: { tab: "overview" } },
    activeClass: "route-link--active",
    exactActiveClass: "route-link--exact"
  }, CreateNavLinkSlots("Query"))]), h("section", { class: "route-state" }, [h("p", "global guard: " + GlobalGuardLog.value), h("p", "afterEach: " + AfterEachLog.value), h("p", "component guard: " + ComponentGuardLog.value), h("p", "loaded path: " + LastLoadedPath.value)]), h(RouterView)]);
}
//# sourceMappingURL=app.mjs.map
