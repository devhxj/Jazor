import { HashSetT1Module } from "System/Collections/Generic/HashSetT1Module.js";
function ensureWritable(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (SetCarrierRuntime.isReadOnlyCarrier(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
}
export function _fa512a510bd763de(instance, item) {
  ensureWritable(instance);
  return HashSetT1Module._e1d2ba750a2788cb(instance, item);
}
export function _d9af20d6b8c5e775(instance, other) {
  ensureWritable(instance);
  HashSetT1Module.unionWithCore(instance, other);
}
export function _202b815f92a32e5d(instance, other) {
  ensureWritable(instance);
  HashSetT1Module.intersectWithCore(instance, other);
}
export function _ac98ad1e0ac9efb5(instance, other) {
  ensureWritable(instance);
  HashSetT1Module.exceptWithCore(instance, other);
}
export function _07907f6b669e590a(instance, other) {
  ensureWritable(instance);
  HashSetT1Module.symmetricExceptWithCore(instance, other);
}
export function _bcd9e5c5cd4a65e1(instance, other) {
  return HashSetT1Module.isSubsetOfCore(instance, other);
}
export function _a64ad5f437ed3887(instance, other) {
  return HashSetT1Module.isSupersetOfCore(instance, other);
}
export function _f7d6687c6a479566(instance, other) {
  return HashSetT1Module.isProperSupersetOfCore(instance, other);
}
export function _bf1a417a69fffcb2(instance, other) {
  return HashSetT1Module.isProperSubsetOfCore(instance, other);
}
export function _45e2e920f151fad2(instance, other) {
  return HashSetT1Module.overlapsCore(instance, other);
}
export function _afabf76c0df51242(instance, other) {
  return HashSetT1Module.setEqualsCore(instance, other);
}
export const ISetT1Module = {
  ensureWritable,
  _fa512a510bd763de,
  _d9af20d6b8c5e775,
  _202b815f92a32e5d,
  _ac98ad1e0ac9efb5,
  _07907f6b669e590a,
  _bcd9e5c5cd4a65e1,
  _a64ad5f437ed3887,
  _f7d6687c6a479566,
  _bf1a417a69fffcb2,
  _45e2e920f151fad2,
  _afabf76c0df51242
};
