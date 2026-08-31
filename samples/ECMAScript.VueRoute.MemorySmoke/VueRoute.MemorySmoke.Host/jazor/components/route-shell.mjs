import { ComponentGuardLog } from "router/memory-router.mjs";
import { computed, defineComponent, h, inject, toRef, triggerRef } from "vue";
import { matchedRouteKey, onBeforeRouteLeave, onBeforeRouteUpdate, routerViewLocationKey, useLink, useRoute, useRouter, viewDepthKey } from "vue-router";
export let HomeView = defineComponent({ name: "MemorySmokeHomeView", setup: CreateHomeSetup });
export let DetailView = defineComponent({
  name: "MemorySmokeDetailView",
  props: ["id", "source"],
  setup: CreateDetailSetup
});
export let QueryView = defineComponent({
  name: "MemorySmokeQueryView",
  props: ["tab"],
  setup: CreateQuerySetup
});
export let BlockedView = defineComponent({ name: "MemorySmokeBlockedView", setup: () => {
  return () => {
    return h("section", { class: "route-card route-card--blocked" }, [h("h2", "Blocked"), h("p", "Navigation should not land here while the guard is active.")]);
  };
} });
export let RouteProbe = defineComponent({ name: "MemorySmokeRouteProbe", setup: CreateProbeSetup });
function CreateHomeSetup() {
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
    return h("section", { class: "route-card route-card--detail" }, [
      h("h2", "Detail " + props.id),
      h("p", "source: " + props.source),
      h("p", "route path: " + route.path),
      h("p", "query via: " + (route.query["via"] ?? "")),
      h("p", "matched path: " + (matchedRoute.value == null ? "" : matchedRoute.value.path)),
      h("p", "view depth: " + viewDepth),
      h("p", "injected route path: " + routedLocation.value.path),
      h("p", "useLink href: " + composedLink.href.value),
      h("div", { class: "route-actions" }, [CreateButton("Blocked target", "action-button", pushBlocked), CreateButton("Follow composed link", "action-button action-button--accent", followComposedLink)]),
      h("div", { class: "route-slot-probe" }, [h("strong", "slot keys visible: "), h("span", context.slots.default == null ? "none" : "default")])
    ]);
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
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=route-shell.mjs.map
