import { defineComponent, h } from "npm:vue@3";
import { useCounterStore, useCounterStoreRefs } from "stores/counter-store.mjs";
export let component = defineComponent({ name: "PiniaCounterApp", setup: setup });
function setup() {
  let store = useCounterStore();
  let refs = useCounterStoreRefs(store);
  let patchPlusFive = () => {
    store.$patch({ count: store.count + 5, status: "Applied $patch({ ... }) from the component." });
    return;
  };
  let resetStore = store.$reset.bind(store);
  return () => {
    return h("section", { class: "counter-shell" }, [
      h("p", { class: "counter-kicker" }, "ECMAScript.Pinia sample"),
      h("h1", { class: "counter-title" }, "Typed Pinia store authored in C#"),
      h("p", { class: "counter-copy" }, "The store comes from defineStore(), is resolved through StoreDefinition.Use(), and is read via storeToRefs()."),
      h("div", { class: "counter-grid" }, [createMetricCard("count", refs.count.value, "metric-card metric-card--primary"), createMetricCard("doubleCount", refs.doubleCount.value, "metric-card metric-card--secondary")]),
      h("p", { class: "counter-status" }, refs.status.value),
      h("div", { class: "counter-actions" }, [createActionButton("Increment", "action-button action-button--accent", store.increment.bind(store)), createActionButton("Decrement", "action-button", store.decrement.bind(store)), createActionButton("Patch +5", "action-button", patchPlusFive), createActionButton("Reset", "action-button action-button--ghost", resetStore)]),
      h("ul", { class: "counter-notes" }, [h("li", "createPinia() stays a normal external runtime import."), h("li", "StoreDefinition<TStore>.Use() keeps the callable store factory explicit in C#."), h("li", "storeToRefs() returns typed refs for both state and getters.")])
    ]);
  };
}
function createMetricCard(label, value, className) {
  return h("article", { class: className }, [h("span", { class: "metric-label" }, label), h("strong", { class: "metric-value" }, value)]);
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-app.mjs.map
