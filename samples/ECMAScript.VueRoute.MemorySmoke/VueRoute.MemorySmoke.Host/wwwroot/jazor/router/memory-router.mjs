import { blockedView, detailView, homeView, queryView } from "components/route-shell.mjs";
import { createMemoryHistory, createRouter, loadRouteLocation } from "npm:vue-router@4";
import { ref } from "npm:vue@3";
export let globalGuardLog = ref("guard:idle");
export let afterEachLog = ref("after:idle");
export let errorLog = ref("error:none");
export let componentGuardLog = ref("component:idle");
export let lastLoadedPath = ref("");
export function createRouterRuntime() {
  globalGuardLog.value = "guard:idle";
  afterEachLog.value = "after:idle";
  errorLog.value = "error:none";
  componentGuardLog.value = "component:idle";
  lastLoadedPath.value = "";
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
      component: homeView,
      meta: { section: "home", requiresAudit: true }
    }, {
      path: "/users/:id",
      name: "detail",
      component: detailView,
      props: detailProps,
      beforeEnter: (to, from) => {
        globalGuardLog.value = "beforeEnter:" + from.path + "->" + to.path;
        return true;
      }
    }, {
      path: "/query",
      name: "query",
      component: queryView,
      props: queryProps
    }, { path: "/legacy/:id", redirect: legacyRedirect }, {
      path: "/blocked",
      name: "blocked",
      component: blockedView
    }]
  });
  router.beforeEach((to, from) => {
    globalGuardLog.value = "beforeEach:" + from.path + "->" + to.path;
    if (to.path === "/blocked") {
      globalGuardLog.value = "beforeEach:blocked";
      return false;
    }
    return true;
  });
  router.beforeResolve((to, from) => {
    globalGuardLog.value = "beforeResolve:" + from.path + "->" + to.path;
    return Promise.resolve(true);
  });
  router.afterEach((to, from, failure) => {
    afterEachLog.value = "afterEach:" + from.path + "->" + to.path + ":" + (failure == null ? "ok" : "failure");
    return;
  });
  router.onError((error, to, from) => {
    errorLog.value = error.message + "|" + from.path + "->" + to.path;
    return;
  });
  return router;
}
export function snapshot(router) {
  let currentRoute = router.currentRoute.value;
  return {
    currentPath: currentRoute.path,
    currentFullPath: currentRoute.fullPath,
    globalGuard: globalGuardLog.value,
    afterEach: afterEachLog.value,
    componentGuard: componentGuardLog.value,
    loadedPath: lastLoadedPath.value,
    isReady: router.listening
  };
}
export function navigateScenario(router) {
  return router.push({
    name: "detail",
    params: { id: "42" },
    query: { via: "scenario" },
    hash: "#summary"
  }).then(() => {
    return loadRouteLocation(router.currentRoute.value);
  }).then(loaded => {
    lastLoadedPath.value = loaded.path;
    return router.replace("/query?tab=summary#focus");
  }).then(() => {
    return snapshot(router);
  });
}
//# sourceMappingURL=memory-router.mjs.map
