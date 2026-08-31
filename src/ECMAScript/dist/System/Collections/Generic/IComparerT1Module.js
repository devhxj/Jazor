import { EnsureComparerInstance } from "System/Collections/Generic/ComparerT1Module.js";
/*jazor:clr-member System.Collections.Generic.IComparer<T>.Compare(T, T)*/
export function _0289dcf579b8a65e(instance, x, y) {
  EnsureComparerInstance(instance);
  let compare = Reflect.get(instance, "compare");
  if (compare === null)
    throw new Error("MissingMethodException: comparer does not expose compare.");
  return Reflect.apply(compare, instance, [x, y]);
}
