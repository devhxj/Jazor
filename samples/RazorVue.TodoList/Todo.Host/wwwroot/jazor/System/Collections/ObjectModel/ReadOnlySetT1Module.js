import { HashSetT1Module } from "System/Collections/Generic/HashSetT1Module.js";
export function _aede400efbd05842(set) {
  if (set === null)
    throw new Error("ArgumentNullException: set is null");
  let snapshot = new Set(set);
  return SetCarrierRuntime.markAsReadOnlyCarrier(snapshot);
}
export function _843cd8664672a9f8() {
  return SetCarrierRuntime.markAsReadOnlyCarrier(new Set);
}
export function _8745918ab865b9f0(instance, other) {
  return HashSetT1Module.isProperSubsetOfCore(instance, other);
}
export function _ab53c8c15a545026(instance, other) {
  return HashSetT1Module.isProperSupersetOfCore(instance, other);
}
export function _f72f25db872c4c11(instance, other) {
  return HashSetT1Module.isSubsetOfCore(instance, other);
}
export function _e7d6617cc0e3119e(instance, other) {
  return HashSetT1Module.isSupersetOfCore(instance, other);
}
export function _520d7f31ddf30fea(instance, other) {
  return HashSetT1Module.overlapsCore(instance, other);
}
export function _eb16d835e6822ba0(instance, other) {
  return HashSetT1Module.setEqualsCore(instance, other);
}
export const ReadOnlySetT1Module = {
  _aede400efbd05842,
  _843cd8664672a9f8,
  _8745918ab865b9f0,
  _ab53c8c15a545026,
  _f72f25db872c4c11,
  _e7d6617cc0e3119e,
  _520d7f31ddf30fea,
  _eb16d835e6822ba0
};
