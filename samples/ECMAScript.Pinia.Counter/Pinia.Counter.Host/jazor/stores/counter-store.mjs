import { defineStore, storeToRefs } from "pinia";
const CounterStoreId = "counter";
const SeedCount = 2;
export let UseCounterStore = defineStore("counter", {
  state: CreateState,
  getters: { DoubleCount: (__cb => function() {
    return __cb(this, ...arguments);
  })(ReadDoubleCount) },
  actions: { Increment: (__cb => function() {
    return __cb(this, ...arguments);
  })(Increment), Decrement: (__cb => function() {
    return __cb(this, ...arguments);
  })(Decrement) }
});
export let UseProjectedCounterStore = UseCounterStore;
export function UseCounterStoreRefs(store) {
  return storeToRefs(store);
}
export function UseProjectedCounterStoreRefs(store) {
  return storeToRefs(store);
}
export function InstallAuditPlugin(context) {
  if (context.store.$id !== "counter") {
    return null;
  }
  let projectedStore = context.store;
  let customState = projectedStore.$state;
  customState.PersistedAt = "plugin:" + context.store.$id;
  return { AuditTag: context.store.$id + ":audited" };
}
function CreateState() {
  return { Count: 2, Status: "Store seeded through defineStore()." };
}
function ReadDoubleCount(self) {
  return self.Count * 2;
}
function Increment(self) {
  self.Count += 1;
  self.Status = "increment() updated the store.";
}
function Decrement(self) {
  if (self.Count > 0) {
    self.Count -= 1;
    self.Status = "decrement() updated the store.";
    return;
  }
  self.Status = "decrement() is clamped at zero.";
}
//# sourceMappingURL=counter-store.mjs.map
