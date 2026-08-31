import { BlockedView, DetailView, HomeView, QueryView } from "components/route-shell.mjs";
import { ref } from "vue";
import { createMemoryHistory, createRouter, loadRouteLocation } from "vue-router";
export let GlobalGuardLog = ref("guard:idle");
export let AfterEachLog = ref("after:idle");
export let ErrorLog = ref("error:none");
export let ComponentGuardLog = ref("component:idle");
export let LastLoadedPath = ref("");
export function CreateRouterRuntime() {
  GlobalGuardLog.value = "guard:idle";
  AfterEachLog.value = "after:idle";
  ErrorLog.value = "error:none";
  ComponentGuardLog.value = "component:idle";
  LastLoadedPath.value = "";
  let detailProps = to => {
    return { id: to.path.replaceAll("/users/", ""), source: "route-props" };
  };
  let queryProps = to => {
    return { tab: to.fullPath.replaceAll("/query?tab=", "").replaceAll("#focus", "") };
  };
  let legacyRedirect = (to, from) => {
    return {
      path: "/users/11",
      query: { via: "legacy-redirect" },
      hash: "#relay"
    };
  };
  let router = createRouter({
    history: createMemoryHistory("/memory-smoke"),
    linkActiveClass: "route-link--active",
    linkExactActiveClass: "route-link--exact",
    routes: [{
      path: "/",
      name: "home",
      component: HomeView,
      meta: { section: "home", requiresAudit: true }
    }, {
      path: "/users/:id",
      name: "detail",
      component: DetailView,
      props: detailProps,
      beforeEnter: (to, from) => {
        GlobalGuardLog.value = "beforeEnter:" + from.path + "->" + to.path;
        return true;
      }
    }, {
      path: "/query",
      name: "query",
      component: QueryView,
      props: queryProps
    }, { path: "/legacy/:id", redirect: legacyRedirect }, {
      path: "/blocked",
      name: "blocked",
      component: BlockedView
    }]
  });
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
export function Snapshot(router) {
  let currentRoute = router.currentRoute.value;
  return {
    currentPath: currentRoute.path,
    currentFullPath: currentRoute.fullPath,
    globalGuard: GlobalGuardLog.value,
    afterEach: AfterEachLog.value,
    componentGuard: ComponentGuardLog.value,
    loadedPath: LastLoadedPath.value,
    isReady: router.listening
  };
}
export function NavigateScenario(router) {
  return router.push({
    name: "detail",
    params: { id: "42" },
    query: { via: "scenario" },
    hash: "#summary"
  }).then(() => {
    return loadRouteLocation(router.currentRoute.value);
  }).then(loaded => {
    LastLoadedPath.value = loaded.path;
    return router.replace("/query?tab=summary#focus");
  }).then(() => {
    return Snapshot(router);
  });
}
//# sourceMappingURL=memory-router.mjs.map
