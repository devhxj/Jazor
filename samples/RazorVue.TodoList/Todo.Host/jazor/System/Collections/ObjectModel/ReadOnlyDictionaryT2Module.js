import { markAsReadOnlyDictionaryCarrier } from "System/RuntimeModule.js";
function ensureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
export function _b22e987e1be225aa(dictionary) {
  if (dictionary === null)
    throw new Error("ArgumentNullException: dictionary is null");
  let source = dictionary;
  let snapshot = new Map(source.entries());
  return markAsReadOnlyDictionaryCarrier(snapshot);
}
export function _43b396f1b8e0a68f() {
  return markAsReadOnlyDictionaryCarrier(new Map);
}
export function _19af957975f1546f(instance, key, value) {
  ensureInstance(instance);
  let typedKey = key;
  if (!instance.has(typedKey))
    return [false, null];
  return [true, instance.get(typedKey)];
}
export function _ed4a7913b74bfd87(instance, key) {
  ensureInstance(instance);
  let typedKey = key;
  if (!instance.has(typedKey))
    throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
  return instance.get(typedKey);
}
