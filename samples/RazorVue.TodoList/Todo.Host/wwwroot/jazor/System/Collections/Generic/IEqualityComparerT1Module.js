import { EqualityComparerT1Module } from "System/Collections/Generic/EqualityComparerT1Module.js";
export function _dae184550b995be1(instance, x, y) {
  EqualityComparerT1Module.ensureComparerInstance(instance);
  return EqualityComparerT1Module.equalsCore(x, y);
}
export function _f53ff8f6435182d7(instance, obj) {
  EqualityComparerT1Module.ensureComparerInstance(instance);
  return EqualityComparerT1Module.getHashCodeCore(obj);
}
export const IEqualityComparerT1Module = { _dae184550b995be1, _f53ff8f6435182d7 };
