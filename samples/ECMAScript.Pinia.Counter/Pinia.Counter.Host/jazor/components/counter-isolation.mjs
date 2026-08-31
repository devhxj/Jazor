import { CreateConfiguredPinia, CreatePiniaInstallationApp } from "host/app.mjs";
import { disposePinia } from "pinia";
import { UseCounterStore, UseProjectedCounterStore } from "stores/counter-store.mjs";
import { defineComponent, h, onUnmounted, ref } from "vue";
export let Component = defineComponent({ name: "PiniaCounterIsolationCookbook", setup: Setup });
export function CompareIsolatedRoots() {
  let leftPinia = CreateInstalledConfiguredPinia();
  let rightPinia = CreateInstalledConfiguredPinia();
  try {
    let leftStore = UseCounterStore(leftPinia);
    let leftProjected = UseProjectedCounterStore(leftPinia);
    let rightStore = UseCounterStore(rightPinia);
    let rightProjected = UseProjectedCounterStore(rightPinia);
    leftStore.Increment();
    leftProjected.$state.PersistedAt = "isolated:left:" + leftStore.$id;
    return leftStore.Count + "|" + rightStore.Count + "|" + leftProjected.AuditTag + "|" + rightProjected.AuditTag + "|" + leftProjected.$state.PersistedAt + "|" + rightProjected.$state.PersistedAt;
  } finally {
    disposePinia(leftPinia);
    disposePinia(rightPinia);
  }
}
function Setup() {
  let leftPinia = CreateInstalledConfiguredPinia();
  let rightPinia = CreateInstalledConfiguredPinia();
  let leftStore = UseCounterStore(leftPinia);
  let leftProjected = UseProjectedCounterStore(leftPinia);
  let rightStore = UseCounterStore(rightPinia);
  let rightProjected = UseProjectedCounterStore(rightPinia);
  let snapshot = ref(DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected));
  let incrementLeftOnly = () => {
    leftStore.Increment();
    leftProjected.$state.PersistedAt = "isolated:left:" + leftStore.Count;
    snapshot.value = DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
    return;
  };
  let incrementRightOnly = () => {
    rightStore.Increment();
    rightProjected.$state.PersistedAt = "isolated:right:" + rightStore.Count;
    snapshot.value = DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
    return;
  };
  onUnmounted(() => {
    disposePinia(leftPinia);
    disposePinia(rightPinia);
    return;
  });
  return () => {
    return h("section", { class: "counter-isolation-shell" }, [h("h2", "Root isolation cookbook"), h("p", "Explicit StoreDefinition.Use(pinia) resolution keeps multiple Pinia roots isolated even when they reuse the same generated store definition and plugin projection contract."), h("ul", { class: "counter-notes" }, [h("li", "snapshot: " + snapshot.value), h("li", "left persistedAt: " + leftProjected.$state.PersistedAt), h("li", "right persistedAt: " + rightProjected.$state.PersistedAt)]), h("div", { class: "counter-actions" }, [CreateActionButton("Increment left root", "action-button action-button--accent", incrementLeftOnly), CreateActionButton("Increment right root", "action-button", incrementRightOnly)])]);
  };
}
function CreateInstalledConfiguredPinia() {
  let pinia = CreateConfiguredPinia();
  CreatePiniaInstallationApp(pinia);
  return pinia;
}
function DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected) {
  return "left=" + leftStore.Count + ", right=" + rightStore.Count + ", leftAudit=" + leftProjected.AuditTag + ", rightAudit=" + rightProjected.AuditTag;
}
function CreateActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-isolation.mjs.map
