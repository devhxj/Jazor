import { component } from "components/counter-app.mjs";
import { createApp } from "npm:vue@3";
import { createPinia } from "pinia";
export function boot(selector) {
  let app = createApp(component);
  let pinia = createPinia();
  app.use(pinia);
  app.mount(selector);
}
//# sourceMappingURL=app.mjs.map
