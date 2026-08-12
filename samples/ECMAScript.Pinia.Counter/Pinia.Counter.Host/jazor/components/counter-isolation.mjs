import { createConfiguredPinia, createPiniaInstallationApp } from "host/app.mjs";
import { defineComponent, h, onUnmounted, ref } from "npm:vue@3";
import { disposePinia } from "pinia";
import { useCounterStore, useProjectedCounterStore } from "stores/counter-store.mjs";
export let component = defineComponent({ name: "PiniaCounterIsolationCookbook", setup: setup });
export function compareIsolatedRoots() {
  let leftPinia = createInstalledConfiguredPinia();
  let rightPinia = createInstalledConfiguredPinia();
  try {
    let leftStore = useCounterStore(leftPinia);
    let leftProjected = useProjectedCounterStore(leftPinia);
    let rightStore = useCounterStore(rightPinia);
    let rightProjected = useProjectedCounterStore(rightPinia);
    leftStore.increment();
    leftProjected.$state.persistedAt = "isolated:left:" + leftStore.$id;
    return leftStore.count + "|" + rightStore.count + "|" + leftProjected.auditTag + "|" + rightProjected.auditTag + "|" + leftProjected.$state.persistedAt + "|" + rightProjected.$state.persistedAt;
  } finally {
    disposePinia(leftPinia);
    disposePinia(rightPinia);
  }
}
function setup() {
  let leftPinia = createInstalledConfiguredPinia();
  let rightPinia = createInstalledConfiguredPinia();
  let leftStore = useCounterStore(leftPinia);
  let leftProjected = useProjectedCounterStore(leftPinia);
  let rightStore = useCounterStore(rightPinia);
  let rightProjected = useProjectedCounterStore(rightPinia);
  let snapshot = ref(describeSnapshot(leftStore, rightStore, leftProjected, rightProjected));
  let incrementLeftOnly = () => {
    leftStore.increment();
    leftProjected.$state.persistedAt = "isolated:left:" + leftStore.count;
    snapshot.value = describeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
    return;
  };
  let incrementRightOnly = () => {
    rightStore.increment();
    rightProjected.$state.persistedAt = "isolated:right:" + rightStore.count;
    snapshot.value = describeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
    return;
  };
  onUnmounted(() => {
    disposePinia(leftPinia);
    disposePinia(rightPinia);
    return;
  });
  return () => {
    return h("section", { class: "counter-isolation-shell" }, [h("h2", "Root isolation cookbook"), h("p", "Explicit StoreDefinition.Use(pinia) resolution keeps multiple Pinia roots isolated even when they reuse the same generated store definition and plugin projection contract."), h("ul", { class: "counter-notes" }, [h("li", "snapshot: " + snapshot.value), h("li", "left persistedAt: " + leftProjected.$state.persistedAt), h("li", "right persistedAt: " + rightProjected.$state.persistedAt)]), h("div", { class: "counter-actions" }, [createActionButton("Increment left root", "action-button action-button--accent", incrementLeftOnly), createActionButton("Increment right root", "action-button", incrementRightOnly)])]);
  };
}
function createInstalledConfiguredPinia() {
  let pinia = createConfiguredPinia();
  createPiniaInstallationApp(pinia);
  return pinia;
}
function describeSnapshot(leftStore, rightStore, leftProjected, rightProjected) {
  return "left=" + leftStore.count + ", right=" + rightStore.count + ", leftAudit=" + leftProjected.auditTag + ", rightAudit=" + rightProjected.auditTag;
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-isolation.mjs.map
