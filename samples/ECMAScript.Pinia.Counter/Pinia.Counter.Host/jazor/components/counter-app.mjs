import { UseCounterStore, UseCounterStoreRefs } from "stores/counter-store.mjs";
import { defineComponent, h } from "vue";
export let Component = defineComponent({ name: "PiniaCounterApp", setup: Setup });
function Setup() {
  let store = UseCounterStore();
  let refs = UseCounterStoreRefs(store);
  let patchPlusFive = () => {
    store.$patch({ Count: store.Count + 5, Status: "Applied $patch({ ... }) from the component." });
    return;
  };
  let resetStore = store.$reset.bind(store);
  return () => {
    return h("section", { class: "counter-shell" }, [
      h("p", { class: "counter-kicker" }, "ECMAScript.Pinia sample"),
      h("h1", { class: "counter-title" }, "Typed Pinia store authored in C#"),
      h("p", { class: "counter-copy" }, "The store comes from defineStore(), is resolved through StoreDefinition.Use(), and is read via storeToRefs()."),
      h("div", { class: "counter-grid" }, [CreateMetricCard("count", refs.Count.value, "metric-card metric-card--primary"), CreateMetricCard("doubleCount", refs.DoubleCount.value, "metric-card metric-card--secondary")]),
      h("p", { class: "counter-status" }, refs.Status.value),
      h("div", { class: "counter-actions" }, [CreateActionButton("Increment", "action-button action-button--accent", store.Increment.bind(store)), CreateActionButton("Decrement", "action-button", store.Decrement.bind(store)), CreateActionButton("Patch +5", "action-button", patchPlusFive), CreateActionButton("Reset", "action-button action-button--ghost", resetStore)]),
      h("ul", { class: "counter-notes" }, [h("li", "createPinia() stays a normal external runtime import."), h("li", "StoreDefinition<TStore>.Use() keeps the callable store factory explicit in C#."), h("li", "storeToRefs() returns typed refs for both state and getters.")])
    ]);
  };
}
function CreateMetricCard(label, value, className) {
  return h("article", { class: className }, [h("span", { class: "metric-label" }, label), h("strong", { class: "metric-value" }, value)]);
}
function CreateActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-app.mjs.map
