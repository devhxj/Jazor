import { CompareCore } from "System/Collections/Generic/ComparerT1Module.js";
import { EqualsCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
/*jazor:clr-member static System.Nullable.Compare<T>(T?, T?)*/
export function _fcbe94e0f2cfc6f4(n1, n2) {
  if (!(n1 !== null && n1 !== undefined))
    return n2 !== null && n2 !== undefined ? -1 : 0;
  if (!(n2 !== null && n2 !== undefined))
    return 1;
  return CompareCore(n1 ?? (() => {
    throw new Error("InvalidOperationException: Nullable object must have a value.");
  })(), n2 ?? (() => {
    throw new Error("InvalidOperationException: Nullable object must have a value.");
  })());
}
/*jazor:clr-member static System.Nullable.Equals<T>(T?, T?)*/
export function _55d5a6397d48a134(n1, n2) {
  if (!(n1 !== null && n1 !== undefined))
    return !(n2 !== null && n2 !== undefined);
  if (!(n2 !== null && n2 !== undefined))
    return false;
  return EqualsCore(n1 ?? (() => {
    throw new Error("InvalidOperationException: Nullable object must have a value.");
  })(), n2 ?? (() => {
    throw new Error("InvalidOperationException: Nullable object must have a value.");
  })());
}
