import { createApp } from "vue";
import { createVuetify } from "vuetify";
import "vuetify/styles";
import { assertHostRequirements, createTodoRootComponent } from "./runtime-common.js";

export function mountTodoApp(TodoApp, hostRequirements, selector = "#app") {
  assertHostRequirements(hostRequirements);

  const app = createApp(createTodoRootComponent(TodoApp));
  app.use(createVuetify());
  app.mount(selector);
  return app;
}
