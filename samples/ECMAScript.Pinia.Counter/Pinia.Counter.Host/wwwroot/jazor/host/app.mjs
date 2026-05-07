import { component } from "components/counter-app.mjs";
import { component as i$664c8e623adaf1eb } from "components/counter-cookbook.mjs";
import { component as i$e0c603ce19ae1b14 } from "components/counter-hmr.mjs";
import { component as i$e4cea2f9595f963b, seedInitialOptionStoreState } from "components/counter-hydration.mjs";
import { component as i$b22c5533e7b472d2 } from "components/counter-isolation.mjs";
import { component as i$9b430160106ff344 } from "components/counter-multi-store.mjs";
import { component as i$9a577bacd5f6df58 } from "components/counter-subscription.mjs";
import { createApp, defineComponent, h } from "npm:vue@3";
import { createPinia, disposePinia, setActivePinia } from "pinia";
import { installAuditPlugin } from "stores/counter-store.mjs";
export function createConfiguredApp() {
  let app = createApp(defineComponent({ name: "PiniaCounterRoot", render: renderRoot }));
  let pinia = createConfiguredPinia();
  app.use(pinia);
  app.onUnmount(() => {
    disposePinia(pinia);
    return;
  });
  return app;
}
export function createConfiguredPinia() {
  let pinia = createPinia().use(installAuditPlugin);
  seedInitialOptionStoreState(pinia);
  return pinia;
}
export function createPiniaInstallationApp(pinia) {
  let app = createApp(defineComponent({ name: "PiniaConfiguredRootShell", render: renderPiniaInstallationShell }));
  app.use(pinia);
  return app;
}
export function clearConfiguredActivePinia() {
  return setActivePinia(undefined);
}
export function boot(selector) {
  let app = createConfiguredApp();
  app.mount(selector);
}
function renderPiniaInstallationShell() {
  return h("div");
}
function renderRoot() {
  return h("main", { class: "counter-root" }, [h("section", { class: "counter-hero" }, [h("p", { class: "counter-kicker" }, "ECMAScript.Pinia production sample"), h("h1", { class: "counter-title" }, "Typed Pinia stores, projected plugins, multi-store helpers, subscriptions, and testing"), h("p", { class: "counter-copy" }, "The sample keeps Pinia as a normal external runtime while exercising authoring paths that matter in production code: defineStore(), storeToRefs(), plugin projections, mapStores(), $subscribe(), acceptHMRUpdate(), and createTestingPinia().")]), h("div", { class: "counter-stack" }, [
    h(component),
    h(i$664c8e623adaf1eb),
    h(i$9b430160106ff344),
    h(i$9a577bacd5f6df58),
    h(i$e4cea2f9595f963b),
    h(i$b22c5533e7b472d2),
    h(i$e0c603ce19ae1b14)
  ])]);
}
//# sourceMappingURL=app.mjs.map
