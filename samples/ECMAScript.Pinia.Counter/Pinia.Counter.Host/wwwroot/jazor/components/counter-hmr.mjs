import { defineComponent, h } from "npm:vue@3";
import { acceptHMRUpdate } from "pinia";
import { useCounterStore, useProjectedCounterStore } from "stores/counter-store.mjs";
export let component = defineComponent({ name: "PiniaCounterHmrCookbook", setup: setup });
export function createCounterHotHandler(hot) {
  return acceptHMRUpdate(useCounterStore, hot);
}
export function createProjectedCounterHotHandler(hot) {
  return acceptHMRUpdate(useProjectedCounterStore, hot);
}
export function resolveCounterStore(pinia, hot) {
  return useCounterStore(pinia, hot);
}
export function resolveProjectedCounterStore(pinia, hot) {
  return useProjectedCounterStore(pinia, hot);
}
function setup() {
  let store = useCounterStore();
  let projectedStore = useProjectedCounterStore();
  let customState = projectedStore.$state;
  let installHotSnapshot = () => {
    customState.persistedAt = "hmr:" + projectedStore.$id;
    return;
  };
  return () => {
    return h("section", { class: "counter-hmr-shell" }, [h("h2", "HMR cookbook"), h("p", "acceptHMRUpdate(useStore, hot) and storeDefinition.Use(pinia, hot) stay explicit in C# so Vite/Jolt hot-module wiring can remain a host concern instead of hidden compiler magic."), h("ul", { class: "counter-notes" }, [h("li", "store id: " + store.$id), h("li", "auditTag: " + projectedStore.auditTag), h("li", "persistedAt: " + customState.persistedAt), h("li", "consumer bridge calls import.meta.hot.accept(createCounterHotHandler(import.meta.hot))")]), h("div", { class: "counter-actions" }, [createActionButton("Prime HMR snapshot", "action-button action-button--accent", installHotSnapshot)])]);
  };
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-hmr.mjs.map
