import { acceptHMRUpdate } from "pinia";
import { UseCounterStore, UseProjectedCounterStore } from "stores/counter-store.mjs";
import { defineComponent, h } from "vue";
export let Component = defineComponent({ name: "PiniaCounterHmrCookbook", setup: Setup });
export function CreateCounterHotHandler(hot) {
  return acceptHMRUpdate(UseCounterStore, hot);
}
export function CreateProjectedCounterHotHandler(hot) {
  return acceptHMRUpdate(UseProjectedCounterStore, hot);
}
export function ResolveCounterStore(pinia, hot) {
  return UseCounterStore(pinia, hot);
}
export function ResolveProjectedCounterStore(pinia, hot) {
  return UseProjectedCounterStore(pinia, hot);
}
function Setup() {
  let store = UseCounterStore();
  let projectedStore = UseProjectedCounterStore();
  let customState = projectedStore.$state;
  let installHotSnapshot = () => {
    customState.PersistedAt = "hmr:" + projectedStore.$id;
    return;
  };
  return () => {
    return h("section", { class: "counter-hmr-shell" }, [h("h2", "HMR cookbook"), h("p", "acceptHMRUpdate(useStore, hot) and storeDefinition.Use(pinia, hot) stay explicit in C# so Vite/Jolt hot-module wiring can remain a host concern instead of hidden compiler magic."), h("ul", { class: "counter-notes" }, [h("li", "store id: " + store.$id), h("li", "auditTag: " + projectedStore.AuditTag), h("li", "persistedAt: " + customState.PersistedAt), h("li", "consumer bridge calls import.meta.hot.accept(createCounterHotHandler(import.meta.hot))")]), h("div", { class: "counter-actions" }, [CreateActionButton("Prime HMR snapshot", "action-button action-button--accent", installHotSnapshot)])]);
  };
}
function CreateActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-hmr.mjs.map
