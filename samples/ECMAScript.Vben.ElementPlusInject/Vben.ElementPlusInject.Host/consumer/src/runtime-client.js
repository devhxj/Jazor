import { createApp } from "vue";
import ElementPlus from "element-plus";
import "element-plus/dist/index.css";
import { assertHostRequirements, createVbenRootComponent } from "./runtime-common.js";

export function mountVbenConsumer(components, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  assertHostRequirements(hostRequirements);

  const VbenDashboardApp = components?.VbenDashboardApp;
  if (typeof VbenDashboardApp !== "object" && typeof VbenDashboardApp !== "function") {
    throw new Error("RazorVue Vben consumer expected a VbenDashboardApp component export.");
  }

  const hasExplicitRoutes = Array.isArray(routesOrSelector);
  const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;
  const app = createApp(createVbenRootComponent(VbenDashboardApp));
  app.use(ElementPlus);
  app.mount(selector);
  return app;
}

export function mountRootComponent(rootComponent, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  return mountVbenConsumer({ VbenDashboardApp: rootComponent }, hostRequirements, routesOrSelector, maybeSelector);
}
