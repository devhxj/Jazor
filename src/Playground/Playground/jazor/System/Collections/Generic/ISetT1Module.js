import { _e1d2ba750a2788cb, exceptWithCore, intersectWithCore, isProperSubsetOfCore, isProperSupersetOfCore, isSubsetOfCore, isSupersetOfCore, overlapsCore, setEqualsCore, symmetricExceptWithCore, unionWithCore } from "System/Collections/Generic/HashSetT1Module.js";
import { isReadOnlySetCarrier } from "System/RuntimeModule.js";
function ensureWritable(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (isReadOnlySetCarrier(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
}
export function _fa512a510bd763de(instance, item) {
  ensureWritable(instance);
  return _e1d2ba750a2788cb(instance, item);
}
export function _d9af20d6b8c5e775(instance, other) {
  ensureWritable(instance);
  unionWithCore(instance, other);
}
export function _202b815f92a32e5d(instance, other) {
  ensureWritable(instance);
  intersectWithCore(instance, other);
}
export function _ac98ad1e0ac9efb5(instance, other) {
  ensureWritable(instance);
  exceptWithCore(instance, other);
}
export function _07907f6b669e590a(instance, other) {
  ensureWritable(instance);
  symmetricExceptWithCore(instance, other);
}
export function _bcd9e5c5cd4a65e1(instance, other) {
  return isSubsetOfCore(instance, other);
}
export function _a64ad5f437ed3887(instance, other) {
  return isSupersetOfCore(instance, other);
}
export function _f7d6687c6a479566(instance, other) {
  return isProperSupersetOfCore(instance, other);
}
export function _bf1a417a69fffcb2(instance, other) {
  return isProperSubsetOfCore(instance, other);
}
export function _45e2e920f151fad2(instance, other) {
  return overlapsCore(instance, other);
}
export function _afabf76c0df51242(instance, other) {
  return setEqualsCore(instance, other);
}
