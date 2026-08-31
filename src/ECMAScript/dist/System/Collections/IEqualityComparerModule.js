import { EnsureComparerInstance } from "System/Collections/Generic/EqualityComparerT1Module.js";
/*jazor:clr-member System.Collections.IEqualityComparer.Equals(object, object)*/
export function _eb0a1792ad8b44b7(instance, x, y) {
  EnsureComparerInstance(instance);
  let equals = Reflect.get(instance, "equals");
  if (equals === null)
    throw new Error("MissingMethodException: comparer does not expose equals.");
  return Reflect.apply(equals, instance, [x, y]);
}
/*jazor:clr-member System.Collections.IEqualityComparer.GetHashCode(object)*/
export function _8f16da840d40722e(instance, obj) {
  EnsureComparerInstance(instance);
  let getHashCode = Reflect.get(instance, "getHashCode");
  if (getHashCode === null)
    throw new Error("MissingMethodException: comparer does not expose getHashCode.");
  return Reflect.apply(getHashCode, instance, [obj]);
}
