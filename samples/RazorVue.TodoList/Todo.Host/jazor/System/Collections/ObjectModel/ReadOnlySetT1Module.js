import { isProperSubsetOfCore, isProperSupersetOfCore, isSubsetOfCore, isSupersetOfCore, overlapsCore, setEqualsCore } from "System/Collections/Generic/HashSetT1Module.js";
import { markAsReadOnlySetCarrier } from "System/RuntimeModule.js";
export function _aede400efbd05842(set) {
  if (set === null)
    throw new Error("ArgumentNullException: set is null");
  let snapshot = new Set(set);
  return markAsReadOnlySetCarrier(snapshot);
}
export function _843cd8664672a9f8() {
  return markAsReadOnlySetCarrier(new Set);
}
export function _8745918ab865b9f0(instance, other) {
  return isProperSubsetOfCore(instance, other);
}
export function _ab53c8c15a545026(instance, other) {
  return isProperSupersetOfCore(instance, other);
}
export function _f72f25db872c4c11(instance, other) {
  return isSubsetOfCore(instance, other);
}
export function _e7d6617cc0e3119e(instance, other) {
  return isSupersetOfCore(instance, other);
}
export function _520d7f31ddf30fea(instance, other) {
  return overlapsCore(instance, other);
}
export function _eb16d835e6822ba0(instance, other) {
  return setEqualsCore(instance, other);
}
