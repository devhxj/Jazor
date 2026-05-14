import { createApp } from "vue";
import { createVuetify } from "vuetify";
import "vuetify/styles";
import { assertHostRequirements, createTodoRootComponent } from "./runtime-common.js";

export function mountTodoConsumer(components, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  assertHostRequirements(hostRequirements);

  const TodoApp = components?.TodoApp;
  if (typeof TodoApp !== "object" && typeof TodoApp !== "function") {
    throw new Error("RazorVue Todo consumer expected a TodoApp component export.");
  }

  const hasExplicitRoutes = Array.isArray(routesOrSelector);
  const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;
  const app = createApp(createTodoRootComponent(TodoApp));
  app.use(createVuetify());
  app.mount(selector);
  return app;
}

export function mountRootComponent(rootComponent, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  return mountTodoConsumer({ TodoApp: rootComponent }, hostRequirements, routesOrSelector, maybeSelector);
}
