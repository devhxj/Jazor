import { Create_9a1218e69f90a6ca } from "System/Collections/Generic/DictionaryT2Module.js";
import { MarkAsReadOnlyDictionaryCarrier } from "System/RuntimeModule.js";
function EnsureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)*/
export function _b22e987e1be225aa(dictionary) {
  if (dictionary === null)
    throw new Error("ArgumentNullException: dictionary is null.");
  return MarkAsReadOnlyDictionaryCarrier(dictionary);
}
/*jazor:clr-member static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get*/
export function _43b396f1b8e0a68f() {
  return MarkAsReadOnlyDictionaryCarrier(Create_9a1218e69f90a6ca(null));
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get*/
export function _4044dececdd2d744(instance) {
  EnsureInstance(instance);
  return Array.from(instance.keys());
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get*/
export function _b39da265738457a5(instance) {
  EnsureInstance(instance);
  return Array.from(instance.values());
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)*/
export function _19af957975f1546f(instance, key, value) {
  EnsureInstance(instance);
  let typedKey = key;
  if (!instance.has(typedKey))
    return [false, null];
  return [true, instance.get(typedKey)];
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get*/
export function _ed4a7913b74bfd87(instance, key) {
  EnsureInstance(instance);
  let typedKey = key;
  if (!instance.has(typedKey))
    throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
  return instance.get(typedKey);
}
