import { createApp } from "vue";
import { createVuetify } from "vuetify";
import "vuetify/styles";
import { assertHostRequirements, createTodoRootComponent } from "./runtime-common.js";

export function mountTodoConsumer(components, hostRequirements, selector = "#app") {
  assertHostRequirements(hostRequirements);

  const TodoApp = components?.TodoApp;
  if (typeof TodoApp !== "object" && typeof TodoApp !== "function") {
    throw new Error("RazorVue Todo consumer expected a TodoApp component export.");
  }

  const app = createApp(createTodoRootComponent(TodoApp));
  app.use(createVuetify());
  app.mount(selector);
  return app;
}

export function mountRootComponent(rootComponent, hostRequirements, selector = "#app") {
  return mountTodoConsumer({ TodoApp: rootComponent }, hostRequirements, selector);
}
