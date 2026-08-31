import { Component } from "components/counter-app.mjs";
import { Component as i$171bc28d43220ee7 } from "components/counter-cookbook.mjs";
import { Component as i$fff0fa1c3f1985a1 } from "components/counter-hmr.mjs";
import { Component as i$28ac66ff45fa890b, SeedInitialOptionStoreState } from "components/counter-hydration.mjs";
import { Component as i$c82170163c5ef2dd } from "components/counter-isolation.mjs";
import { Component as i$e8238f960a1622b8 } from "components/counter-multi-store.mjs";
import { Component as i$02b3d5fcbbfb3568 } from "components/counter-subscription.mjs";
import { createPinia, disposePinia, setActivePinia } from "pinia";
import { InstallAuditPlugin } from "stores/counter-store.mjs";
import { createApp, defineComponent, h } from "vue";
export function CreateConfiguredApp() {
  let app = createApp(defineComponent({ name: "PiniaCounterRoot", render: RenderRoot }));
  let pinia = CreateConfiguredPinia();
  app.use(pinia);
  app.onUnmount(() => {
    disposePinia(pinia);
    return;
  });
  return app;
}
export function CreateConfiguredPinia() {
  let pinia = createPinia().use(InstallAuditPlugin);
  SeedInitialOptionStoreState(pinia);
  return pinia;
}
export function CreatePiniaInstallationApp(pinia) {
  let app = createApp(defineComponent({ name: "PiniaConfiguredRootShell", render: RenderPiniaInstallationShell }));
  app.use(pinia);
  return app;
}
export function ClearConfiguredActivePinia() {
  return setActivePinia(undefined);
}
export function Boot(selector) {
  let app = CreateConfiguredApp();
  app.mount(selector);
}
function RenderPiniaInstallationShell() {
  return h("div");
}
function RenderRoot() {
  return h("main", { class: "counter-root" }, [h("section", { class: "counter-hero" }, [h("p", { class: "counter-kicker" }, "ECMAScript.Pinia production sample"), h("h1", { class: "counter-title" }, "Typed Pinia stores, projected plugins, multi-store helpers, subscriptions, and testing"), h("p", { class: "counter-copy" }, "The sample keeps Pinia as a normal external runtime while exercising authoring paths that matter in production code: defineStore(), storeToRefs(), plugin projections, mapStores(), $subscribe(), acceptHMRUpdate(), and createTestingPinia().")]), h("div", { class: "counter-stack" }, [
    h(Component),
    h(i$171bc28d43220ee7),
    h(i$e8238f960a1622b8),
    h(i$02b3d5fcbbfb3568),
    h(i$28ac66ff45fa890b),
    h(i$c82170163c5ef2dd),
    h(i$fff0fa1c3f1985a1)
  ])]);
}
//# sourceMappingURL=app.mjs.map
