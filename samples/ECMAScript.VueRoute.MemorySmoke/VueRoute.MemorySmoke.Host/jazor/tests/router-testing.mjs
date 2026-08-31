import { CreateRouterRuntime, GlobalGuardLog, NavigateScenario } from "router/memory-router.mjs";
import { loadRouteLocation } from "vue-router";
export function CreateTestingRouter() {
  return CreateRouterRuntime();
}
export function RunScenario() {
  let router = CreateTestingRouter();
  return NavigateScenario(router);
}
export function NavigateLegacyRedirect() {
  let router = CreateTestingRouter();
  return router.push("/legacy/11#relay").then(() => {
    return loadRouteLocation(router.currentRoute.value);
  }).then(loaded => {
    return loaded.fullPath;
  });
}
export function NavigateBlockedPath() {
  let router = CreateTestingRouter();
  return router.push("/").then(() => {
    return router.push("/blocked");
  }).then(_ => {
    return router.currentRoute.value.path + "|" + GlobalGuardLog.value;
  });
}
//# sourceMappingURL=router-testing.mjs.map
