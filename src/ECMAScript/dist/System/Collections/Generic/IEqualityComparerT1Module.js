import { EnsureComparerInstance } from "System/Collections/Generic/EqualityComparerT1Module.js";
/*jazor:clr-member System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)*/
export function _dae184550b995be1(instance, x, y) {
  EnsureComparerInstance(instance);
  let equals = Reflect.get(instance, "equals");
  if (equals === null)
    throw new Error("MissingMethodException: comparer does not expose equals.");
  return Reflect.apply(equals, instance, [x, y]);
}
/*jazor:clr-member System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)*/
export function _f53ff8f6435182d7(instance, obj) {
  EnsureComparerInstance(instance);
  let getHashCode = Reflect.get(instance, "getHashCode");
  if (getHashCode === null)
    throw new Error("MissingMethodException: comparer does not expose getHashCode.");
  return Reflect.apply(getHashCode, instance, [obj]);
}
