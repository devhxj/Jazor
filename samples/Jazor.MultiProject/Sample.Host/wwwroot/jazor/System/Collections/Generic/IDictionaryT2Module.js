import { isReadOnlyDictionaryCarrier } from "System/RuntimeModule.js";
function ensureInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
export function _371fad9265e864a1(instance, key) {
  ensureInstance(instance);
  if (!instance.has(key))
    throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
  return instance.get(key);
}
export function _f3b177bfce76ed5c(instance, key, value) {
  ensureInstance(instance);
  if (isReadOnlyDictionaryCarrier(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
  instance.set(key, value);
}
export function _ebaafc4d4a520807(instance, key) {
  ensureInstance(instance);
  if (instance.has(key))
    return [true, instance.get(key)];
  return [false, null];
}
