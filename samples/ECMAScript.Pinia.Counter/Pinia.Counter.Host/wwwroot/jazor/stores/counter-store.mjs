import { defineStore, storeToRefs } from "pinia";
const counterStoreId = "counter";
const seedCount = 2;
export let useCounterStore = defineStore("counter", {
  state: createState,
  getters: { doubleCount: (__cb => function() {
    return __cb(this, ...arguments);
  })(readDoubleCount) },
  actions: { increment: (__cb => function() {
    return __cb(this, ...arguments);
  })(increment), decrement: (__cb => function() {
    return __cb(this, ...arguments);
  })(decrement) }
});
export let useProjectedCounterStore = useCounterStore;
export function useCounterStoreRefs(store) {
  return storeToRefs(store);
}
export function useProjectedCounterStoreRefs(store) {
  return storeToRefs(store);
}
export function installAuditPlugin(context) {
  if (context.store.$id !== "counter") {
    return null;
  }
  let projectedStore = context.store;
  let customState = projectedStore.$state;
  customState.persistedAt = "plugin:" + context.store.$id;
  return { auditTag: context.store.$id + ":audited" };
}
function createState() {
  return { count: 2, status: "Store seeded through defineStore()." };
}
function readDoubleCount(self) {
  return self.count * 2;
}
function increment(self) {
  self.count += 1;
  self.status = "increment() updated the store.";
}
function decrement(self) {
  if (self.count > 0) {
    self.count -= 1;
    self.status = "decrement() updated the store.";
    return;
  }
  self.status = "decrement() is clamped at zero.";
}
//# sourceMappingURL=counter-store.mjs.map
