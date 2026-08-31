let ActiveStorages = new WeakMap;
function EnsureInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function EnsureKey(key) {
  if (key === null)
    throw new Error("ArgumentNullException: key is null.");
}
function GetStorage(instance) {
  EnsureInstance(instance);
  if (!ActiveStorages.has(instance))
    ActiveStorages.set(instance, instance);
  return ActiveStorages.get(instance);
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)*/
export function _8360443cbe5b1f88(instance, key, value) {
  EnsureKey(key);
  let storage = GetStorage(instance);
  if (!storage.has(key))
    return [false, null];
  return [true, storage.get(key)];
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)*/
export function _c013f77a250570ce(instance, key, value) {
  EnsureKey(key);
  let storage = GetStorage(instance);
  if (storage.has(key))
    throw new Error("ArgumentException: An item with the same key has already been added.");
  storage.set(key, value);
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)*/
export function _6a785a77d1b78937(instance, key, value) {
  EnsureKey(key);
  let storage = GetStorage(instance);
  if (storage.has(key))
    return false;
  storage.set(key, value);
  return true;
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.AddOrUpdate(TKey, TValue)*/
export function _3e5ae776a9edba7b(instance, key, value) {
  EnsureKey(key);
  GetStorage(instance).set(key, value);
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)*/
export function _0b5841f143b2e9e7(instance, key) {
  EnsureKey(key);
  return GetStorage(instance).delete(key);
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)*/
export function _14e40010b1fd2993(instance, key, value) {
  EnsureKey(key);
  let storage = GetStorage(instance);
  if (!storage.has(key))
    return [false, null];
  let currentValue = storage.get(key);
  storage.delete(key);
  return [true, currentValue];
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()*/
export function _57912eda7fd377bb(instance) {
  EnsureInstance(instance);
  ActiveStorages.set(instance, new WeakMap);
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)*/
export function _8e3321f2e6fa2499(instance, key, value) {
  EnsureKey(key);
  let storage = GetStorage(instance);
  if (storage.has(key))
    return storage.get(key);
  storage.set(key, value);
  return value;
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, System.Func<TKey, TValue>)*/
export function _ed09a626bf4f3ea8(instance, key, valueFactory) {
  EnsureInstance(instance);
  EnsureKey(key);
  if (valueFactory === null)
    throw new Error("ArgumentNullException: valueFactory is null.");
  let storage = GetStorage(instance);
  if (storage.has(key))
    return storage.get(key);
  let value = valueFactory(key);
  storage.set(key, value);
  return value;
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)*/
export function _eaeddd47f4a65d81(instance, key, valueFactory, factoryArgument) {
  EnsureInstance(instance);
  EnsureKey(key);
  if (valueFactory === null)
    throw new Error("ArgumentNullException: valueFactory is null.");
  let storage = GetStorage(instance);
  if (storage.has(key))
    return storage.get(key);
  let value = valueFactory(key, factoryArgument);
  storage.set(key, value);
  return value;
}
/*jazor:clr-member System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetValue(TKey, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback)*/
export function _43edc29b01c6a1f0(instance, key, createValueCallback) {
  return _ed09a626bf4f3ea8(instance, key, createValueCallback);
}
