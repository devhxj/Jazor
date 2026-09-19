import { EnsureComparerInstance } from "System/Collections/Generic/ComparerT1Module.js";
/*jazor:clr-member System.Collections.IComparer.Compare(object, object)*/
export function _7dffdd7244581cc5(instance, x, y) {
  EnsureComparerInstance(instance);
  let compare = Reflect.get(instance, "compare");
  if (compare === null)
    throw new Error("MissingMethodException: comparer does not expose compare.");
  return Reflect.apply(compare, instance, [x, y]);
}
