import { EqualsCore, GetHashCodeCore, getDefault } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { _dae184550b995be1, _f53ff8f6435182d7 } from "System/Collections/Generic/IEqualityComparerT1Module.js";
import { ExpandHashCollectionCapacity, GetHashCollectionCapacity } from "System/RuntimeModule.js";
let States = new WeakMap;
let Capacities = new WeakMap;
function EnsureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
function GetCapacity(instance) {
  EnsureInstance(instance);
  if (!Capacities.has(instance))
    Capacities.set(instance, GetHashCollectionCapacity(instance.size));
  return Capacities.get(instance);
}
function EnsureEntryCapacity(instance, requiredCount) {
  let capacity = GetCapacity(instance);
  if (requiredCount > capacity)
    Capacities.set(instance, ExpandHashCollectionCapacity(capacity));
}
function EnsureCapacityCore(instance, capacity) {
  let requested = GetHashCollectionCapacity(capacity);
  let current = GetCapacity(instance);
  if (requested <= current)
    return current;
  Capacities.set(instance, requested);
  return requested;
}
function GetHashCode(state, key) {
  return state.Comparer === null ? GetHashCodeCore(key) : _f53ff8f6435182d7(state.Comparer, key);
}
function Equals(state, left, right) {
  return state.Comparer === null ? EqualsCore(left, right) : _dae184550b995be1(state.Comparer, left, right);
}
function FindEquivalentIndex(bucket, key, state) {
  for (let index = 0; index < bucket.length; index++) {
    if (Equals(state, bucket[index], key))
      return index;
  }
  return -1;
}
function GetOrCreateBucket(state, hashCode) {
  if (state.KeysByHash.has(hashCode))
    return state.KeysByHash.get(hashCode);
  let bucket = new Array;
  state.KeysByHash.set(hashCode, bucket);
  return bucket;
}
function NativeSet(instance, key, value) {
  let set = Reflect.get(Map.prototype, "set");
  if (set === null)
    throw new Error("MissingMethodException: Map.prototype.set is unavailable.");
  Reflect.apply(set, instance, [key, value]);
}
function NativeGet(instance, key) {
  let get = Reflect.get(Map.prototype, "get");
  if (get === null)
    throw new Error("MissingMethodException: Map.prototype.get is unavailable.");
  return Reflect.apply(get, instance, [key]);
}
function NativeDelete(instance, key) {
  let __binding$8a064abad2db1d3e = Reflect.get(Map.prototype, "delete");
  if (__binding$8a064abad2db1d3e === null)
    throw new Error("MissingMethodException: Map.prototype.delete is unavailable.");
  return Reflect.apply(__binding$8a064abad2db1d3e, instance, [key]);
}
function NativeClear(instance) {
  let clear = Reflect.get(Map.prototype, "clear");
  if (clear === null)
    throw new Error("MissingMethodException: Map.prototype.clear is unavailable.");
  Reflect.apply(clear, instance, []);
}
function SetCore(instance, key, value, state) {
  let hashCode = GetHashCode(state, key);
  let bucket = GetOrCreateBucket(state, hashCode);
  let index = FindEquivalentIndex(bucket, key, state);
  if (index >= 0) {
    NativeSet(instance, bucket[index], value);
    return instance;
  }
  EnsureEntryCapacity(instance, instance.size + 1);
  bucket.push(key);
  NativeSet(instance, key, value);
  return instance;
}
function HasCore(key, state) {
  let hashCode = GetHashCode(state, key);
  if (!state.KeysByHash.has(hashCode))
    return false;
  return FindEquivalentIndex(state.KeysByHash.get(hashCode), key, state) >= 0;
}
function GetCore(instance, key, state) {
  let hashCode = GetHashCode(state, key);
  if (!state.KeysByHash.has(hashCode))
    return NativeGet(instance, key);
  let bucket = state.KeysByHash.get(hashCode);
  let index = FindEquivalentIndex(bucket, key, state);
  return index < 0 ? NativeGet(instance, key) : NativeGet(instance, bucket[index]);
}
function DeleteCore(instance, key, state) {
  let hashCode = GetHashCode(state, key);
  if (!state.KeysByHash.has(hashCode))
    return false;
  let bucket = state.KeysByHash.get(hashCode);
  let index = FindEquivalentIndex(bucket, key, state);
  if (index < 0)
    return false;
  let representative = bucket[index];
  bucket.splice(index, 1);
  if (bucket.length === 0)
    state.KeysByHash.delete(hashCode);
  return NativeDelete(instance, representative);
}
function ClearCore(instance, state) {
  state.KeysByHash.clear();
  NativeClear(instance);
}
export function Create_9a1218e69f90a6ca(comparer) {
  return Create_abb39be6cc4d68c2(comparer, 0);
}
function Create_abb39be6cc4d68c2(comparer, capacity) {
  let normalizedCapacity = GetHashCollectionCapacity(capacity);
  let instance = new Map;
  Capacities.set(instance, normalizedCapacity);
  if (comparer == null)
    return instance;
  let state = { Comparer: comparer, KeysByHash: new Map };
  States.set(instance, state);
  Object.defineProperty(instance, "set", {
    value: (key, value) => {
      return SetCore(instance, key, value, state);
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  Object.defineProperty(instance, "get", {
    value: key => {
      return GetCore(instance, key, state);
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  Object.defineProperty(instance, "has", {
    value: key => {
      return HasCore(key, state);
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  Object.defineProperty(instance, "delete", {
    value: key => {
      return DeleteCore(instance, key, state);
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  Object.defineProperty(instance, "clear", {
    value: () => {
      ClearCore(instance, state);
      return;
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  return instance;
}
function SetItemCore(instance, key, value) {
  EnsureInstance(instance);
  if (!instance.has(key))
    EnsureEntryCapacity(instance, instance.size + 1);
  instance.set(key, value);
}
export function CreateFromMap(source, comparer) {
  if (source === null)
    throw new Error("ArgumentNullException: dictionary is null");
  let result = Create_abb39be6cc4d68c2(comparer, source.size);
  for (let key of source.keys())
    result.set(key, source.get(key));
  return result;
}
function CreateFromPairs_1e1f3b883e25dfac(source, comparer) {
  let values;
  if (source === null)
    throw new Error("ArgumentNullException: collection is null.");
  let initialCapacity = Array.isArray(source) && (values = source, true) ? values.length : 0;
  let result = Create_abb39be6cc4d68c2(comparer, initialCapacity);
  for (let [key, value] of source) {
    if (result.has(key))
      throw new Error("ArgumentException: An item with the same key has already been added.");
    SetItemCore(result, key, value);
  }
  return result;
}
export function GetComparer(instance) {
  EnsureInstance(instance);
  return States.has(instance) ? States.get(instance).Comparer : null;
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()*/
export function createDefault() {
  return Create_9a1218e69f90a6ca(null);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)*/
export function createWithCapacity(capacity) {
  return Create_abb39be6cc4d68c2(null, capacity);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)*/
export function createWithComparer(comparer) {
  return Create_9a1218e69f90a6ca(comparer);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function createWithCapacityAndComparer(capacity, comparer) {
  return Create_abb39be6cc4d68c2(comparer, capacity);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)*/
export function createFromDictionary(dictionary) {
  return CreateFromMap(dictionary, null);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function createFromDictionaryWithComparer(dictionary, comparer) {
  return CreateFromMap(dictionary, comparer);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)*/
export function createFromPairs(collection) {
  return CreateFromPairs_1e1f3b883e25dfac(collection, null);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)*/
export function createFromPairsWithComparer(collection, comparer) {
  return CreateFromPairs_1e1f3b883e25dfac(collection, comparer);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get*/
export function _1a4a1b31526edb7a(instance) {
  return GetComparer(instance) ?? getDefault();
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get*/
export function getCapacityMember(instance) {
  return GetCapacity(instance);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get*/
export function _e73dbdff85c46ddc(instance, key) {
  EnsureInstance(instance);
  if (!instance.has(key))
    throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
  return instance.get(key);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set*/
export function setItem(instance, key, value) {
  SetItemCore(instance, key, value);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)*/
export function _39d6e632c4c102f9(instance, key, value) {
  EnsureInstance(instance);
  if (instance.has(key))
    throw new Error("ArgumentException: An item with the same key has already been added.");
  EnsureEntryCapacity(instance, instance.size + 1);
  instance.set(key, value);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)*/
export function _d6ac89338dff5e3b(instance, key) {
  EnsureInstance(instance);
  if (instance.has(key)) {
    let value = instance.get(key);
    instance.delete(key);
    return [true, value];
  }
  return [false, null];
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)*/
export function _7db4d9112b4ba3c4(instance, key) {
  EnsureInstance(instance);
  if (instance.has(key))
    return [true, instance.get(key)];
  return [false, null];
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)*/
export function _61b63b2c7b14f06a(instance, key, value) {
  EnsureInstance(instance);
  if (instance.has(key))
    return false;
  EnsureEntryCapacity(instance, instance.size + 1);
  instance.set(key, value);
  return true;
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)*/
export function ensureCapacity(instance, capacity) {
  return EnsureCapacityCore(instance, capacity);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()*/
export function _44cc5aa04712525c(instance) {
  let capacity = GetCapacity(instance);
  let trimmed = GetHashCollectionCapacity(instance.size);
  if (trimmed < capacity)
    Capacities.set(instance, trimmed);
}
/*jazor:clr-member System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)*/
export function _dd7fceb710b10915(instance, capacity) {
  let current = GetCapacity(instance);
  if (capacity < instance.size)
    throw new Error("ArgumentOutOfRangeException: capacity cannot be less than Count.");
  let trimmed = GetHashCollectionCapacity(capacity);
  if (trimmed < current)
    Capacities.set(instance, trimmed);
}
