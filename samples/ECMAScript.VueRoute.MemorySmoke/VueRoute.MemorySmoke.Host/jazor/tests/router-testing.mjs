import { loadRouteLocation } from "npm:vue-router@4";
import { createRouterRuntime, globalGuardLog, navigateScenario } from "router/memory-router.mjs";
export function createTestingRouter() {
  return createRouterRuntime();
}
export function runScenario() {
  let router = createTestingRouter();
  return navigateScenario(router);
}
export function navigateLegacyRedirect() {
  let router = createTestingRouter();
  return router.push("/legacy/11#relay").then(() => {
    return loadRouteLocation(router.currentRoute.value);
  }).then(loaded => {
    return loaded.fullPath;
  });
}
export function navigateBlockedPath() {
  let router = createTestingRouter();
  return router.push("/").then(() => {
    return router.push("/blocked");
  }).then(_ => {
    return router.currentRoute.value.path + "|" + globalGuardLog.value;
  });
}
//# sourceMappingURL=router-testing.mjs.map
