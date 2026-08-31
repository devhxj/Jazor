import { IsReadOnlyDictionaryCarrier } from "System/RuntimeModule.js";
function EnsureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
function EnsureWritable(instance) {
  EnsureInstance(instance);
  if (IsReadOnlyDictionaryCarrier(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
}
/*jazor:clr-member System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get*/
export function _371fad9265e864a1(instance, key) {
  EnsureInstance(instance);
  if (!instance.has(key))
    throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
  return instance.get(key);
}
/*jazor:clr-member System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].set*/
export function _f3b177bfce76ed5c(instance, key, value) {
  EnsureInstance(instance);
  if (IsReadOnlyDictionaryCarrier(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
  instance.set(key, value);
}
/*jazor:clr-member System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)*/
export function _93efc3872e59b431(instance, key, value) {
  EnsureWritable(instance);
  if (instance.has(key))
    throw new Error("ArgumentException: An item with the same key has already been added.");
  instance.set(key, value);
}
/*jazor:clr-member System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)*/
export function _fc84b7a31e5cdfe4(instance, key) {
  EnsureWritable(instance);
  return instance.delete(key);
}
/*jazor:clr-member System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)*/
export function _ebaafc4d4a520807(instance, key) {
  EnsureInstance(instance);
  if (instance.has(key))
    return [true, instance.get(key)];
  return [false, null];
}
