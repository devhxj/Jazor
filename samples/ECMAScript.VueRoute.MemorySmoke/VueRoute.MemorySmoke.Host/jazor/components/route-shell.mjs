import { matchedRouteKey, onBeforeRouteLeave, onBeforeRouteUpdate, routerViewLocationKey, useLink, useRoute, useRouter, viewDepthKey } from "npm:vue-router@4";
import { computed, defineComponent, h, inject, toRef, triggerRef } from "npm:vue@3";
import { componentGuardLog } from "router/memory-router.mjs";
export let homeView = defineComponent({ name: "MemorySmokeHomeView", setup: createHomeSetup });
export let detailView = defineComponent({
  name: "MemorySmokeDetailView",
  props: ["id", "source"],
  setup: createDetailSetup
});
export let queryView = defineComponent({
  name: "MemorySmokeQueryView",
  props: ["tab"],
  setup: createQuerySetup
});
export let blockedView = defineComponent({ name: "MemorySmokeBlockedView", setup: () => {
  return () => {
    return h("section", { class: "route-card route-card--blocked" }, [h("h2", "Blocked"), h("p", "Navigation should not land here while the guard is active.")]);
  };
} });
export let routeProbe = defineComponent({ name: "MemorySmokeRouteProbe", setup: createProbeSetup });
function createHomeSetup() {
  let router = useRouter();
  let route = useRoute();
  let greeting = computed(() => {
    return "home:" + route.name;
  });
  let navigateToDetail = () => {
    router.push({
      name: "detail",
      params: { id: "7" },
      query: { via: "button" },
      hash: "#summary"
    });
    return;
  };
  return () => {
    return h("section", { class: "route-card route-card--home" }, [h("h2", "Home"), h("p", "current name: " + route.name), h("p", "current path: " + route.path), h("p", "message: " + greeting.value), createButton("Go detail", "action-button action-button--accent", navigateToDetail)]);
  };
}
function createDetailSetup(props, context) {
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
    componentGuardLog.value = "update:" + from.path + "->" + to.path;
    return true;
  });
  onBeforeRouteLeave((to, from) => {
    componentGuardLog.value = "leave:" + from.path + "->" + to.path;
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
    return h("section", { class: "route-card route-card--detail" }, [
      h("h2", "Detail " + props.id),
      h("p", "source: " + props.source),
      h("p", "route path: " + route.path),
      h("p", "query via: " + (route.query["via"] ?? "")),
      h("p", "matched path: " + (matchedRoute.value == null ? "" : matchedRoute.value.path)),
      h("p", "view depth: " + viewDepth),
      h("p", "injected route path: " + routedLocation.value.path),
      h("p", "useLink href: " + composedLink.href.value),
      h("div", { class: "route-actions" }, [createButton("Blocked target", "action-button", pushBlocked), createButton("Follow composed link", "action-button action-button--accent", followComposedLink)]),
      h("div", { class: "route-slot-probe" }, [h("strong", "slot keys visible: "), h("span", context.slots.default == null ? "none" : "default")])
    ]);
  };
}
function createQuerySetup(props, context) {
  context;
  let route = useRoute();
  return () => {
    return h("section", { class: "route-card route-card--query" }, [h("h2", "Query"), h("p", "tab prop: " + props.tab), h("p", "hash: " + route.hash), h("p", "fullPath: " + route.fullPath)]);
  };
}
function createProbeSetup() {
  let route = useRoute();
  let router = useRouter();
  let currentRoute = router.currentRoute;
  triggerRef(currentRoute);
  return () => {
    return h("aside", { class: "route-probe" }, [h("p", "probe current route: " + currentRoute.value.fullPath), h("p", "probe useRoute path: " + route.path)]);
  };
}
function createButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=route-shell.mjs.map
