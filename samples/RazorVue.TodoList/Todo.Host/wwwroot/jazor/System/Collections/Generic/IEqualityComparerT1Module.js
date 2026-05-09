import { ensureComparerInstance, equalsCore, getHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
export function _dae184550b995be1(instance, x, y) {
  ensureComparerInstance(instance);
  return equalsCore(x, y);
}
export function _f53ff8f6435182d7(instance, obj) {
  ensureComparerInstance(instance);
  return getHashCodeCore(obj);
}
