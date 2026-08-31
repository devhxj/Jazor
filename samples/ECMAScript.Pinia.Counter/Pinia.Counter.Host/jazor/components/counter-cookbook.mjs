import { mapActions, mapState } from "pinia";
import { UseProjectedCounterStore, UseProjectedCounterStoreRefs } from "stores/counter-store.mjs";
import { defineComponent, h } from "vue";
export let Component = defineComponent({
  name: "PiniaCounterCookbook",
  computed: CreateComputed(),
  methods: CreateMethods(),
  setup: Setup
});
function CreateComputed() {
  return mapState(UseProjectedCounterStore, {
    Count: "Count",
    Status: "Status",
    DoubleCount: "DoubleCount",
    TripleCount: ReadTripleCount,
    AuditTag: "AuditTag"
  });
}
function CreateMethods() {
  return mapActions(UseProjectedCounterStore, ["Increment", "Decrement"]);
}
function Setup() {
  let projectedStore = UseProjectedCounterStore();
  let refs = UseProjectedCounterStoreRefs(projectedStore);
  let baseStore = projectedStore;
  let customState = projectedStore.$state;
  customState.PersistedAt = "component:" + baseStore.$id;
  return () => {
    return h("section", { class: "counter-cookbook-shell" }, [h("h2", "Projected plugin cookbook"), h("p", "Projected store definitions flow through storeToRefs(), Options API helpers, and direct custom-property/custom-state projections without inventing a separate runtime object."), h("ul", [h("li", "auditTag: " + projectedStore.AuditTag), h("li", "persistedAt: " + projectedStore.$state.PersistedAt), h("li", "countRef: " + refs["Count"].value), h("li", "statusRef: " + refs["Status"].value), h("li", "doubleCount: " + projectedStore.DoubleCount), h("li", "tripleCount: " + ReadTripleCount(projectedStore))]), h("div", { class: "counter-actions" }, [CreateActionButton("Projected increment", "action-button action-button--accent", baseStore.Increment.bind(baseStore)), CreateActionButton("Projected decrement", "action-button", baseStore.Decrement.bind(baseStore))]), h("p", { class: "counter-status" }, "Options API helpers are configured through CreateComputed()/CreateMethods(); the live card shows the projected store + projected refs path.")]);
  };
}
function CreateActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
function ReadTripleCount(store) {
  return store.Count * 3;
}
//# sourceMappingURL=counter-cookbook.mjs.map
