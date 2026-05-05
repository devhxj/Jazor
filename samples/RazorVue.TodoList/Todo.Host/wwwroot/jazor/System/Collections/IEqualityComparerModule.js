import { EqualityComparerT1Module } from "System/Collections/Generic/EqualityComparerT1Module.js";
export function _eb0a1792ad8b44b7(instance, x, y) {
  EqualityComparerT1Module.ensureComparerInstance(instance);
  return EqualityComparerT1Module.equalsCore(x, y);
}
export function _8f16da840d40722e(instance, obj) {
  EqualityComparerT1Module.ensureComparerInstance(instance);
  return EqualityComparerT1Module.getHashCodeCore(obj);
}
export const IEqualityComparerModule = { _eb0a1792ad8b44b7, _8f16da840d40722e };
