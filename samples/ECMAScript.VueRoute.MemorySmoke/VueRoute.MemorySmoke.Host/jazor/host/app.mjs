import { routeProbe } from "components/route-shell.mjs";
import { RouterLink, RouterView } from "npm:vue-router@4";
import { createApp, defineComponent, h } from "npm:vue@3";
import { afterEachLog, componentGuardLog, createRouterRuntime, globalGuardLog, lastLoadedPath } from "router/memory-router.mjs";
export function createConfiguredApp() {
  let router = createRouterRuntime();
  return CreateConfiguredApp(router);
}
export function boot(selector) {
  let router = createRouterRuntime();
  let app = CreateConfiguredApp(router);
  router.isReady().then(() => {
    app.mount(selector);
    return;
  });
}
function CreateConfiguredApp(router) {
  let app = createApp(defineComponent({ name: "VueRouteMemorySmokeRoot", render: renderRoot }));
  app.use(router);
  router.push("/");
  app.onUnmount(() => {
    router.clearRoutes();
    return;
  });
  return app;
}
function createNavLinkSlots(label) {
  return { default: scope => {
    return [h("span", label + " " + scope.href)];
  } };
}
function renderRoot() {
  return h("main", { class: "route-root" }, [h("section", { class: "route-hero" }, [h("p", { class: "route-kicker" }, "ECMAScript.VueRoute production sample"), h("h1", { class: "route-title" }, "Typed Vue Router authoring, guards, links, router-view slots, and runtime smoke coverage"), h("p", { class: "route-copy" }, "The sample keeps Vue Router as a normal external runtime while exercising strongly typed route objects, route props, global and component guards, useLink(), RouterLink, RouterView, injection keys, and loadRouteLocation().")]), h(routeProbe), h("nav", { class: "route-nav" }, [h(RouterLink, {
    to: "/",
    activeClass: "route-link--active",
    exactActiveClass: "route-link--exact"
  }, createNavLinkSlots("Home")), h(RouterLink, {
    to: {
      name: "detail",
      params: { id: "5" },
      query: { via: "nav" }
    },
    activeClass: "route-link--active",
    exactActiveClass: "route-link--exact"
  }, createNavLinkSlots("Detail 5")), h(RouterLink, {
    to: { name: "query", query: { tab: "overview" } },
    activeClass: "route-link--active",
    exactActiveClass: "route-link--exact"
  }, createNavLinkSlots("Query"))]), h("section", { class: "route-state" }, [h("p", "global guard: " + globalGuardLog.value), h("p", "afterEach: " + afterEachLog.value), h("p", "component guard: " + componentGuardLog.value), h("p", "loaded path: " + lastLoadedPath.value)]), h(RouterView)]);
}
//# sourceMappingURL=app.mjs.map
