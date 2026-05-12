import { ensureComparerInstance, equalsCore, getHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
export function _eb0a1792ad8b44b7(instance, x, y) {
  ensureComparerInstance(instance);
  return equalsCore(x, y);
}
export function _8f16da840d40722e(instance, obj) {
  ensureComparerInstance(instance);
  return getHashCodeCore(obj);
}
