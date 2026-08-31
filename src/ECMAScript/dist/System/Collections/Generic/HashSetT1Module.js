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
function EnsureOther(other) {
  if (other == null)
    throw new Error("ArgumentNullException: other is null");
}
function GetHashCode(state, value) {
  return state.Comparer === null ? GetHashCodeCore(value) : _f53ff8f6435182d7(state.Comparer, value);
}
function Equals(state, left, right) {
  return state.Comparer === null ? EqualsCore(left, right) : _dae184550b995be1(state.Comparer, left, right);
}
function FindEquivalentIndex(bucket, value, state) {
  for (let index = 0; index < bucket.length; index++) {
    if (Equals(state, bucket[index], value))
      return index;
  }
  return -1;
}
function GetOrCreateBucket(state, hashCode) {
  if (state.ValuesByHash.has(hashCode))
    return state.ValuesByHash.get(hashCode);
  let bucket = new Array;
  state.ValuesByHash.set(hashCode, bucket);
  return bucket;
}
function NativeAdd(instance, item) {
  let add = Reflect.get(Set.prototype, "add");
  if (add === null)
    throw new Error("MissingMethodException: Set.prototype.add is unavailable.");
  Reflect.apply(add, instance, [item]);
}
function NativeDelete(instance, item) {
  let __binding$5a7451464edc053e = Reflect.get(Set.prototype, "delete");
  if (__binding$5a7451464edc053e === null)
    throw new Error("MissingMethodException: Set.prototype.delete is unavailable.");
  return Reflect.apply(__binding$5a7451464edc053e, instance, [item]);
}
function NativeClear(instance) {
  let clear = Reflect.get(Set.prototype, "clear");
  if (clear === null)
    throw new Error("MissingMethodException: Set.prototype.clear is unavailable.");
  Reflect.apply(clear, instance, []);
}
function AddCore_1ee62986859618a4(instance, item, state) {
  let hashCode = GetHashCode(state, item);
  let bucket = GetOrCreateBucket(state, hashCode);
  if (FindEquivalentIndex(bucket, item, state) >= 0)
    return instance;
  EnsureEntryCapacity(instance, instance.size + 1);
  bucket.push(item);
  NativeAdd(instance, item);
  return instance;
}
function HasCore(instance, item, state) {
  let hashCode = GetHashCode(state, item);
  if (!state.ValuesByHash.has(hashCode))
    return false;
  return FindEquivalentIndex(state.ValuesByHash.get(hashCode), item, state) >= 0;
}
function DeleteCore(instance, item, state) {
  let hashCode = GetHashCode(state, item);
  if (!state.ValuesByHash.has(hashCode))
    return false;
  let bucket = state.ValuesByHash.get(hashCode);
  let index = FindEquivalentIndex(bucket, item, state);
  if (index < 0)
    return false;
  let representative = bucket[index];
  bucket.splice(index, 1);
  if (bucket.length === 0)
    state.ValuesByHash.delete(hashCode);
  return NativeDelete(instance, representative);
}
function ClearCore(instance, state) {
  state.ValuesByHash.clear();
  NativeClear(instance);
}
export function Create_2c5622046787c7f9(comparer) {
  return Create_55889c9f002a3b17(comparer, 0);
}
function Create_55889c9f002a3b17(comparer, capacity) {
  let normalizedCapacity = GetHashCollectionCapacity(capacity);
  let instance = new Set;
  Capacities.set(instance, normalizedCapacity);
  if (comparer == null)
    return instance;
  let state = { Comparer: comparer, ValuesByHash: new Map };
  States.set(instance, state);
  Object.defineProperty(instance, "add", {
    value: item => {
      return AddCore_1ee62986859618a4(instance, item, state);
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  Object.defineProperty(instance, "has", {
    value: item => {
      return HasCore(instance, item, state);
    },
    enumerable: false,
    writable: false,
    configurable: true
  });
  Object.defineProperty(instance, "delete", {
    value: item => {
      return DeleteCore(instance, item, state);
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
function CreateWithCapacity_1efbba7f541fd2e5(capacity, comparer) {
  return Create_55889c9f002a3b17(comparer, capacity);
}
export function CreateFrom(values, comparer) {
  let set, array;
  EnsureOther(values);
  let initialCapacity = Array.isArray(values) && (array = values, true) ? array.length : values instanceof Set && (set = values, true) ? set.size : 0;
  let lookup = Create_55889c9f002a3b17(comparer, initialCapacity);
  for (let value of values)
    AddCore_c1d44e5d9916d4b9(lookup, value);
  return lookup;
}
export function CreateFromSet(values, comparer) {
  EnsureInstance(values);
  let lookup = Create_55889c9f002a3b17(comparer, values.size);
  for (let value of values)
    AddCore_c1d44e5d9916d4b9(lookup, value);
  return lookup;
}
export function AddCore_c1d44e5d9916d4b9(instance, item) {
  EnsureInstance(instance);
  let size = instance.size;
  instance.add(item);
  if (!States.has(instance) && instance.size > size)
    EnsureEntryCapacity(instance, instance.size);
  return instance.size > size;
}
export function GetComparer(instance) {
  EnsureInstance(instance);
  return States.has(instance) ? States.get(instance).Comparer : null;
}
function TryGetValueCore(instance, equalValue) {
  EnsureInstance(instance);
  if (!States.has(instance))
    return instance.has(equalValue) ? [true, equalValue] : [false, null];
  let state = States.get(instance);
  let hashCode = GetHashCode(state, equalValue);
  if (!state.ValuesByHash.has(hashCode))
    return [false, null];
  let bucket = state.ValuesByHash.get(hashCode);
  let index = FindEquivalentIndex(bucket, equalValue, state);
  return index < 0 ? [false, null] : [true, bucket[index]];
}
function CopyToCore(instance, array, arrayIndex, count) {
  EnsureInstance(instance);
  if (array === null)
    throw new Error("ArgumentNullException: array is null.");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
  if (count < 0 || count > instance.size)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
  if (arrayIndex + count > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
  let written = 0;
  for (let item of instance) {
    if (written === count)
      return;
    array[arrayIndex + written] = item;
    written++;
  }
}
function RemoveWhereCore(instance, match) {
  EnsureInstance(instance);
  if (match === null)
    throw new Error("ArgumentNullException: match is null.");
  let snapshot = new Array;
  for (let item of instance)
    snapshot.push(item);
  let removed = 0;
  for (let item of snapshot) {
    let value = item;
    if (!match(value))
      continue;
    if (instance.delete(value))
      removed++;
  }
  return removed;
}
export function UnionWithCore(instance, other) {
  EnsureInstance(instance);
  EnsureOther(other);
  for (let item of other)
    AddCore_c1d44e5d9916d4b9(instance, item);
}
export function IntersectWithCore(instance, other) {
  EnsureInstance(instance);
  let lookup = CreateFrom(other, GetComparer(instance));
  for (let item of instance) {
    let current = item;
    if (!lookup.has(current))
      instance.delete(current);
  }
}
export function ExceptWithCore(instance, other) {
  EnsureInstance(instance);
  EnsureOther(other);
  for (let item of other)
    instance.delete(item);
}
export function SymmetricExceptWithCore(instance, other) {
  EnsureInstance(instance);
  let lookup = CreateFrom(other, GetComparer(instance));
  for (let item of lookup) {
    let current = item;
    if (instance.has(current))
      instance.delete(current);
    else
      AddCore_c1d44e5d9916d4b9(instance, current);
  }
}
export function IsSubsetOfCore(instance, other) {
  EnsureInstance(instance);
  let lookup = CreateFrom(other, GetComparer(instance));
  for (let item of instance) {
    let current = item;
    if (!lookup.has(current))
      return false;
  }
  return true;
}
export function IsProperSubsetOfCore(instance, other) {
  EnsureInstance(instance);
  let lookup = CreateFrom(other, GetComparer(instance));
  if (instance.size >= lookup.size)
    return false;
  for (let item of instance) {
    let current = item;
    if (!lookup.has(current))
      return false;
  }
  return true;
}
export function IsSupersetOfCore(instance, other) {
  EnsureInstance(instance);
  EnsureOther(other);
  for (let item of other) {
    if (!instance.has(item))
      return false;
  }
  return true;
}
export function IsProperSupersetOfCore(instance, other) {
  EnsureInstance(instance);
  let lookup = CreateFrom(other, GetComparer(instance));
  if (instance.size <= lookup.size)
    return false;
  for (let item of lookup) {
    let current = item;
    if (!instance.has(current))
      return false;
  }
  return true;
}
export function OverlapsCore(instance, other) {
  EnsureInstance(instance);
  EnsureOther(other);
  for (let item of other) {
    if (instance.has(item))
      return true;
  }
  return false;
}
export function SetEqualsCore(instance, other) {
  EnsureInstance(instance);
  let lookup = CreateFrom(other, GetComparer(instance));
  if (instance.size !== lookup.size)
    return false;
  for (let item of lookup) {
    let current = item;
    if (!instance.has(current))
      return false;
  }
  return true;
}
function SetComparerEquals(left, right) {
  if (Object.is(left, right))
    return true;
  if (left === null || right === null)
    return false;
  if (left.size !== right.size)
    return false;
  for (let item of left) {
    if (!right.has(item))
      return false;
  }
  return true;
}
function SetComparerHashCode(instance) {
  if (instance === null)
    return 0;
  let hashCode = 0;
  for (let item of instance)
    hashCode ^= GetHashCodeCore(item);
  return hashCode;
}
function CreateSetComparerCore() {
  let comparer = Object.create(null);
  Reflect.set(comparer, "equals", SetComparerEquals);
  Reflect.set(comparer, "getHashCode", SetComparerHashCode);
  return comparer;
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.HashSet()*/
export function createDefault() {
  return Create_2c5622046787c7f9(null);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)*/
export function createWithComparer(comparer) {
  return Create_2c5622046787c7f9(comparer);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.HashSet(int)*/
export function createWithCapacity(capacity) {
  return CreateWithCapacity_1efbba7f541fd2e5(capacity, null);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>)*/
export function createFromCollection(collection) {
  return CreateFrom(collection, null);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)*/
export function createFromWithComparer(collection, comparer) {
  return CreateFrom(collection, comparer);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)*/
export function createWithCapacityAndComparer(capacity, comparer) {
  return CreateWithCapacity_1efbba7f541fd2e5(capacity, comparer);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.Capacity.get*/
export function getCapacityMember(instance) {
  return GetCapacity(instance);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.Add(T)*/
export function _e1d2ba750a2788cb(instance, item) {
  return AddCore_c1d44e5d9916d4b9(instance, item);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)*/
export function _20eb460b32c63404(instance, equalValue) {
  return TryGetValueCore(instance, equalValue);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)*/
export function _b2bd5d22aadd44a8(instance, other) {
  UnionWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)*/
export function _3a6a072035334578(instance, other) {
  IntersectWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)*/
export function _373e2e9ed1fb3f5b(instance, other) {
  ExceptWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)*/
export function _a22fe44dc0ae9ad2(instance, other) {
  SymmetricExceptWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _23c8bcfc6b71d2b1(instance, other) {
  return IsSubsetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _fb8566ae66aa9591(instance, other) {
  return IsProperSubsetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _3be7fbb1d68799fb(instance, other) {
  return IsSupersetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _cc0cc2d0f5be70db(instance, other) {
  return IsProperSupersetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)*/
export function _84709aa8ff70a52a(instance, other) {
  return OverlapsCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)*/
export function _55425d259e5f54ea(instance, other) {
  return SetEqualsCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.CopyTo(T[])*/
export function _614185e6ff9ff9fd(instance, array) {
  EnsureInstance(instance);
  CopyToCore(instance, array, 0, instance.size);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.CopyTo(T[], int)*/
export function _9ac2dfb153a1d53c(instance, array, arrayIndex) {
  EnsureInstance(instance);
  CopyToCore(instance, array, arrayIndex, instance.size);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)*/
export function _622a881b75871c97(instance, array, arrayIndex, count) {
  CopyToCore(instance, array, arrayIndex, count);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)*/
export function _112079825eb01119(instance, match) {
  return RemoveWhereCore(instance, match);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.Comparer.get*/
export function _0c0d81e2205a9cb9(instance) {
  return GetComparer(instance) ?? getDefault();
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.EnsureCapacity(int)*/
export function ensureCapacity(instance, capacity) {
  return EnsureCapacityCore(instance, capacity);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.TrimExcess()*/
export function _09f9b6aba126decb(instance) {
  let capacity = GetCapacity(instance);
  let trimmed = GetHashCollectionCapacity(instance.size);
  if (trimmed < capacity)
    Capacities.set(instance, trimmed);
}
/*jazor:clr-member System.Collections.Generic.HashSet<T>.TrimExcess(int)*/
export function _e4dd8faf507013ad(instance, capacity) {
  let current = GetCapacity(instance);
  if (capacity < instance.size)
    throw new Error("ArgumentOutOfRangeException: capacity cannot be less than Count.");
  let trimmed = GetHashCollectionCapacity(capacity);
  if (trimmed < current)
    Capacities.set(instance, trimmed);
}
/*jazor:clr-member static System.Collections.Generic.HashSet<T>.CreateSetComparer()*/
export function _2d028c1bc3e2f479() {
  return CreateSetComparerCore();
}
