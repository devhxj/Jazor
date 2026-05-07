import { defineComponent, h } from "npm:vue@3";
import { mapActions, mapState } from "pinia";
import { useProjectedCounterStore, useProjectedCounterStoreRefs } from "stores/counter-store.mjs";
export let component = defineComponent({
  name: "PiniaCounterCookbook",
  computed: createComputed(),
  methods: createMethods(),
  setup: setup
});
function createComputed() {
  return mapState(useProjectedCounterStore, ["count", "status", "auditTag"]);
}
function createMethods() {
  return mapActions(useProjectedCounterStore, ["increment", "decrement"]);
}
function setup() {
  let projectedStore = useProjectedCounterStore();
  let refs = useProjectedCounterStoreRefs(projectedStore);
  let baseStore = projectedStore;
  let customState = projectedStore.$state;
  customState.persistedAt = "component:" + baseStore.$id;
  return () => {
    return h("section", { class: "counter-cookbook-shell" }, [h("h2", "Projected plugin cookbook"), h("p", "Projected store definitions flow through storeToRefs(), Options API helpers, and direct custom-property/custom-state projections without inventing a separate runtime object."), h("ul", [h("li", "auditTag: " + projectedStore.auditTag), h("li", "persistedAt: " + projectedStore.$state.persistedAt), h("li", "countRef: " + refs["count"].value), h("li", "statusRef: " + refs["status"].value)]), h("div", { class: "counter-actions" }, [createActionButton("Projected increment", "action-button action-button--accent", baseStore.increment.bind(baseStore)), createActionButton("Projected decrement", "action-button", baseStore.decrement.bind(baseStore))]), h("p", { class: "counter-status" }, "Options API helpers are configured through CreateComputed()/CreateMethods(); the live card shows the projected store + projected refs path.")]);
  };
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-cookbook.mjs.map
