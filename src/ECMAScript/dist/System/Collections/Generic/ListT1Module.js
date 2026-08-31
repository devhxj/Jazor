import { CompareCore } from "System/Collections/Generic/ComparerT1Module.js";
import { _0289dcf579b8a65e } from "System/Collections/Generic/IComparerT1Module.js";
import { CreateReadOnlyArrayView, MarkAsMutableListCarrier } from "System/RuntimeModule.js";
let MaxListCapacity = 2147483591;
let Capacities = new WeakMap;
function EnsureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
function GetCapacity(instance) {
  EnsureInstance(instance);
  if (!Capacities.has(instance))
    Capacities.set(instance, instance.length);
  return Capacities.get(instance);
}
function CreateWithCapacity(capacity) {
  EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
  if (capacity < 0)
    throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
  if (capacity > MaxListCapacity)
    throw new Error("OutOfMemoryException: requested list capacity is too large.");
  let instance = MarkAsMutableListCarrier(new Array);
  Capacities.set(instance, capacity);
  return instance;
}
function ExpandCapacity(currentCapacity, requiredCapacity) {
  if (requiredCapacity > MaxListCapacity)
    throw new Error("OutOfMemoryException: requested list capacity is too large.");
  let expanded = currentCapacity === 0 ? 4 : currentCapacity * 2;
  if (expanded > MaxListCapacity)
    expanded = MaxListCapacity;
  return expanded < requiredCapacity ? requiredCapacity : expanded;
}
function EnsureCapacityCore(instance, capacity) {
  EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
  if (capacity < 0)
    throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
  let current = GetCapacity(instance);
  if (capacity <= current)
    return current;
  let expanded = ExpandCapacity(current, capacity);
  Capacities.set(instance, expanded);
  return expanded;
}
function AddCore(instance, item) {
  EnsureInstance(instance);
  EnsureCapacityCore(instance, instance.length + 1);
  instance.push(item);
}
function CreateFrom(collection) {
  let set, array;
  if (collection == null)
    throw new Error("ArgumentNullException: collection is null.");
  let capacity = Array.isArray(collection) && (array = collection, true) ? array.length : collection instanceof Set && (set = collection, true) ? set.size : 0;
  let result = CreateWithCapacity(capacity);
  for (let item of collection)
    AddCore(result, item);
  return result;
}
function EnsureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(message);
}
function EnsureTargetArray(array) {
  if (array == null)
    throw new Error("ArgumentNullException: array is null");
}
function EnsureTargetIndex(array, arrayIndex) {
  EnsureWholeNumber(arrayIndex, "ArgumentOutOfRangeException: arrayIndex must be a whole number.");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
}
function EnsureCopyCapacity(array, arrayIndex, copyCount) {
  if (arrayIndex + copyCount > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
}
function EnsureInsertIndex(instance, index) {
  EnsureInstance(instance);
  EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
  if (index < 0 || index > instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
}
function EnsureExistingIndex(instance, index) {
  EnsureInstance(instance);
  EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
}
function EnsureRemoveRange(instance, index, count) {
  EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  EnsureInsertIndex(instance, index);
  if (count < 0 || index + count > instance.length)
    throw new Error("ArgumentException: offset and length were out of bounds for the list.");
}
function EnsureMatch(match) {
  if (match == null)
    throw new Error("ArgumentNullException: match is null");
}
function EnsureForwardSearchStartIndex(instance, startIndex) {
  EnsureInsertIndex(instance, startIndex);
}
function EnsureForwardSearchRange(instance, startIndex, count) {
  EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  EnsureForwardSearchStartIndex(instance, startIndex);
  if (count < 0 || startIndex + count > instance.length)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
}
function EnsureLastSearchStartIndex(instance, startIndex, parameterName) {
  EnsureInstance(instance);
  EnsureWholeNumber(startIndex, `ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
  if (instance.length === 0) {
    if (startIndex !== -1)
      throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} is out of range.`);
    return;
  }
  if (startIndex < 0 || startIndex >= instance.length)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} is out of range.`);
}
function EnsureLastSearchRange(instance, startIndex, count, startIndexName) {
  EnsureLastSearchStartIndex(instance, startIndex, startIndexName);
  EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  if (count < 0)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
  if (instance.length === 0) {
    if (count !== 0)
      throw new Error("ArgumentOutOfRangeException: count is out of range.");
    return;
  }
  if (count > startIndex + 1)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
}
function CompareWith(comparer, left, right) {
  return comparer == null ? CompareCore(left, right) : _0289dcf579b8a65e(comparer, left, right);
}
function BinarySearchCore(instance, index, count, item, comparer) {
  EnsureRemoveRange(instance, index, count);
  let lower = index;
  let upper = index + count - 1;
  while (lower <= upper) {
    let midpoint = lower + Math.floor((upper - lower) / 2);
    let comparison = CompareWith(comparer, instance[midpoint], item);
    if (comparison === 0)
      return midpoint;
    if (comparison < 0)
      lower = midpoint + 1;
    else
      upper = midpoint - 1;
  }
  return ~lower;
}
function EqualsForListSearch(left, right) {
  let leftNumber, rightNumber;
  if (Object.is(left, right))
    return true;
  if (typeof left === "number" && (leftNumber = left, true) && (typeof right === "number" && (rightNumber = right, true)))
    return leftNumber === rightNumber;
  return false;
}
function AppendRange(instance, collection) {
  let source;
  EnsureInstance(instance);
  if (collection == null)
    throw new Error("ArgumentNullException: collection is null");
  if (Array.isArray(collection) && (source = collection, true) && Object.is(instance, source)) {
    let originalLength = source.length;
    for (let i = 0; i < originalLength; i++)
      AddCore(instance, source[i]);
    return;
  }
  for (let item of collection)
    AddCore(instance, item);
}
/*jazor:clr-member System.Collections.Generic.List<T>.List()*/
export function createDefault() {
  return CreateWithCapacity(0);
}
/*jazor:clr-member System.Collections.Generic.List<T>.List(int)*/
export function createWithInitialCapacity(capacity) {
  return CreateWithCapacity(capacity);
}
/*jazor:clr-member System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)*/
export function createFromCollection(collection) {
  return CreateFrom(collection);
}
/*jazor:clr-member System.Collections.Generic.List<T>.Capacity.get*/
export function getCapacityMember(instance) {
  return GetCapacity(instance);
}
/*jazor:clr-member System.Collections.Generic.List<T>.Capacity.set*/
export function setCapacity(instance, value) {
  EnsureWholeNumber(value, "ArgumentOutOfRangeException: capacity must be a whole number.");
  if (value < instance.length)
    throw new Error("ArgumentOutOfRangeException: capacity cannot be less than Count.");
  if (value > MaxListCapacity)
    throw new Error("OutOfMemoryException: requested list capacity is too large.");
  GetCapacity(instance);
  Capacities.set(instance, value);
}
/*jazor:clr-member System.Collections.Generic.List<T>.this[int].get*/
export function _d389c31d59037b42(instance, index) {
  EnsureExistingIndex(instance, index);
  return instance[index];
}
/*jazor:clr-member System.Collections.Generic.List<T>.this[int].set*/
export function _c16a7960302ea054(instance, index, value) {
  EnsureExistingIndex(instance, index);
  instance[index] = value;
}
/*jazor:clr-member System.Collections.Generic.List<T>.Add(T)*/
export function add(instance, item) {
  AddCore(instance, item);
}
/*jazor:clr-member System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)*/
export function _a2660853a4ebc1f6(instance, collection) {
  AppendRange(instance, collection);
}
/*jazor:clr-member System.Collections.Generic.List<T>.AsReadOnly()*/
export function _f7981b5a4cd02bdb(instance) {
  EnsureInstance(instance);
  return CreateReadOnlyArrayView(instance, "NullReferenceException: instance is null.");
}
/*jazor:clr-member System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)*/
export function _95ada27dd960bae5(instance, index, count, item, comparer) {
  return BinarySearchCore(instance, index, count, item, comparer);
}
/*jazor:clr-member System.Collections.Generic.List<T>.BinarySearch(T)*/
export function _3d21965eedc9916f(instance, item) {
  EnsureInstance(instance);
  return BinarySearchCore(instance, 0, instance.length, item, null);
}
/*jazor:clr-member System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)*/
export function _65e239056cc65177(instance, item, comparer) {
  EnsureInstance(instance);
  return BinarySearchCore(instance, 0, instance.length, item, comparer);
}
/*jazor:clr-member System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)*/
export function _098c2e027f3a5996(instance, converter) {
  EnsureInstance(instance);
  if (converter == null)
    throw new Error("ArgumentNullException: converter is null.");
  return MarkAsMutableListCarrier(instance.map(converter));
}
/*jazor:clr-member System.Collections.Generic.List<T>.CopyTo(T[])*/
export function _9a3a4817585dded1(instance, array) {
  EnsureInstance(instance);
  EnsureTargetArray(array);
  EnsureCopyCapacity(array, 0, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[i] = instance[i];
}
/*jazor:clr-member System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)*/
export function _0fdf1627d283f8ae(instance, index, array, arrayIndex, count) {
  EnsureInstance(instance);
  EnsureTargetArray(array);
  EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
  EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  EnsureTargetIndex(array, arrayIndex);
  if (index < 0 || index > instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  if (count < 0)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
  if (index + count > instance.length)
    throw new Error("ArgumentException: source index and count are out of range.");
  EnsureCopyCapacity(array, arrayIndex, count);
  for (let i = 0; i < count; i++)
    array[arrayIndex + i] = instance[index + i];
}
/*jazor:clr-member System.Collections.Generic.List<T>.CopyTo(T[], int)*/
export function _3559b1ff2a643922(instance, array, arrayIndex) {
  EnsureInstance(instance);
  EnsureTargetArray(array);
  EnsureTargetIndex(array, arrayIndex);
  EnsureCopyCapacity(array, arrayIndex, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[arrayIndex + i] = instance[i];
}
/*jazor:clr-member System.Collections.Generic.List<T>.EnsureCapacity(int)*/
export function ensureCapacity(instance, capacity) {
  return EnsureCapacityCore(instance, capacity);
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindAll(System.Predicate<T>)*/
export function findAll(instance, match) {
  EnsureInstance(instance);
  EnsureMatch(match);
  let result = CreateWithCapacity(0);
  for (let index = 0; index < instance.length; index++) {
    if (match(instance[index]))
      AddCore(result, instance[index]);
  }
  return result;
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindIndex(int, System.Predicate<T>)*/
export function _db9b68fbc73e342b(instance, startIndex, match) {
  EnsureMatch(match);
  EnsureForwardSearchStartIndex(instance, startIndex);
  if (startIndex === instance.length)
    return -1;
  for (let i = startIndex; i < instance.length; i++) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindIndex(int, int, System.Predicate<T>)*/
export function _41b337b09c5daf75(instance, startIndex, count, match) {
  EnsureMatch(match);
  EnsureForwardSearchRange(instance, startIndex, count);
  if (count === 0)
    return -1;
  let end = startIndex + count;
  for (let i = startIndex; i < end; i++) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindLast(System.Predicate<T>)*/
export function _de0943e496e36f2d(instance, match) {
  EnsureInstance(instance);
  EnsureMatch(match);
  for (let i = instance.length; i > 0; i--) {
    if (match(instance[i - 1]))
      return instance[i - 1];
  }
  return null;
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindLastIndex(System.Predicate<T>)*/
export function _ae1a0b59c73f2b1a(instance, match) {
  EnsureInstance(instance);
  EnsureMatch(match);
  for (let i = instance.length; i > 0; i--) {
    if (match(instance[i - 1]))
      return i - 1;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindLastIndex(int, System.Predicate<T>)*/
export function _081aa9ae0b09d058(instance, startIndex, match) {
  EnsureMatch(match);
  EnsureLastSearchStartIndex(instance, startIndex, "startIndex");
  if (instance.length === 0)
    return -1;
  for (let i = startIndex; i >= 0; i--) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.FindLastIndex(int, int, System.Predicate<T>)*/
export function _58cc54dc07e440c4(instance, startIndex, count, match) {
  EnsureMatch(match);
  EnsureLastSearchRange(instance, startIndex, count, "startIndex");
  if (count === 0)
    return -1;
  let start = startIndex - count + 1;
  for (let i = startIndex; i >= start; i--) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.GetRange(int, int)*/
export function _c35c9c99a23ff96a(instance, index, count) {
  EnsureRemoveRange(instance, index, count);
  let result = CreateWithCapacity(count);
  for (let offset = 0; offset < count; offset++)
    AddCore(result, instance[index + offset]);
  return result;
}
/*jazor:clr-member System.Collections.Generic.List<T>.Slice(int, int)*/
export function slice(instance, start, length) {
  return _c35c9c99a23ff96a(instance, start, length);
}
/*jazor:clr-member System.Collections.Generic.List<T>.IndexOf(T, int)*/
export function _71ee35e0e260eb27(instance, item, index) {
  EnsureForwardSearchStartIndex(instance, index);
  if (index === instance.length)
    return -1;
  for (let i = index; i < instance.length; i++) {
    if (EqualsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.IndexOf(T, int, int)*/
export function _5ee52e4e4fc54e6d(instance, item, index, count) {
  EnsureForwardSearchRange(instance, index, count);
  if (count === 0)
    return -1;
  let end = index + count;
  for (let i = index; i < end; i++) {
    if (EqualsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.Insert(int, T)*/
export function _0dc538197c677986(instance, index, item) {
  EnsureInsertIndex(instance, index);
  EnsureCapacityCore(instance, instance.length + 1);
  instance.splice(index, 0, item);
}
/*jazor:clr-member System.Collections.Generic.List<T>.InsertRange(int, System.Collections.Generic.IEnumerable<T>)*/
export function _56ef9aefabac7c09(instance, index, collection) {
  EnsureInsertIndex(instance, index);
  if (collection == null)
    throw new Error("ArgumentNullException: collection is null");
  let values = new Array;
  for (let item of collection)
    values.push(item);
  if (values.length === 0)
    return;
  let originalLength = instance.length;
  EnsureCapacityCore(instance, originalLength + values.length);
  for (let read = originalLength; read > index; read--)
    instance[read + values.length - 1] = instance[read - 1];
  for (let offset = 0; offset < values.length; offset++)
    instance[index + offset] = values[offset];
}
/*jazor:clr-member System.Collections.Generic.List<T>.LastIndexOf(T, int)*/
export function _279befda6399cda5(instance, item, index) {
  EnsureLastSearchStartIndex(instance, index, "index");
  if (instance.length === 0)
    return -1;
  for (let i = index; i >= 0; i--) {
    if (EqualsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.LastIndexOf(T, int, int)*/
export function _b2f1955b62962812(instance, item, index, count) {
  EnsureLastSearchRange(instance, index, count, "index");
  if (count === 0)
    return -1;
  let start = index - count + 1;
  for (let i = index; i >= start; i--) {
    if (EqualsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
/*jazor:clr-member System.Collections.Generic.List<T>.Remove(T)*/
export function _562f832fd220e768(instance, item) {
  EnsureInstance(instance);
  for (let i = 0; i < instance.length; i++) {
    if (EqualsForListSearch(instance[i], item)) {
      instance.splice(i, 1);
      return true;
    }
  }
  return false;
}
/*jazor:clr-member System.Collections.Generic.List<T>.RemoveAll(System.Predicate<T>)*/
export function _b864beda26f186e2(instance, match) {
  EnsureInstance(instance);
  EnsureMatch(match);
  let write = 0;
  let count = 0;
  for (let read = 0; read < instance.length; read++) {
    let item = instance[read];
    if (match(item)) {
      count++;
      continue;
    }
    instance[write++] = item;
  }
  if (count > 0)
    instance.splice(write, count);
  return count;
}
/*jazor:clr-member System.Collections.Generic.List<T>.RemoveAt(int)*/
export function _a5e8c6b27df6470b(instance, index) {
  EnsureExistingIndex(instance, index);
  instance.splice(index, 1);
}
/*jazor:clr-member System.Collections.Generic.List<T>.RemoveRange(int, int)*/
export function _8425758ef4e7b6f9(instance, index, count) {
  EnsureRemoveRange(instance, index, count);
  if (count === 0)
    return;
  instance.splice(index, count);
}
/*jazor:clr-member System.Collections.Generic.List<T>.Reverse(int, int)*/
export function _56dc1af8af32e484(instance, index, count) {
  EnsureRemoveRange(instance, index, count);
  if (count <= 1)
    return;
  let start = index;
  let end = index + count - 1;
  while (start < end) {
    let temp = instance[start];
    instance[start] = instance[end];
    instance[end] = temp;
    start++;
    end--;
  }
}
function CompareDefault(left, right) {
  return CompareCore(left, right);
}
/*jazor:clr-member System.Collections.Generic.List<T>.Sort()*/
export function _36a478f36b41a6d2(instance) {
  EnsureInstance(instance);
  instance.sort((left, right) => {
    return CompareDefault(left, right);
  });
}
/*jazor:clr-member System.Collections.Generic.List<T>.Sort(System.Collections.Generic.IComparer<T>)*/
export function _5fa599e721e252ff(instance, comparer) {
  EnsureInstance(instance);
  if (comparer == null)
    instance.sort((left, right) => {
      return CompareDefault(left, right);
    });
  else
    instance.sort((left, right) => {
      return _0289dcf579b8a65e(comparer, left, right);
    });
}
/*jazor:clr-member System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)*/
export function _19207851b52a5287(instance, index, count, comparer) {
  EnsureInstance(instance);
  EnsureRemoveRange(instance, index, count);
  if (count <= 1)
    return;
  let subArray = instance.slice(index, index + count);
  if (comparer !== null)
    subArray.sort((a, b) => {
      return _0289dcf579b8a65e(comparer, a, b);
    });
  else
    subArray.sort((left, right) => {
      return CompareDefault(left, right);
    });
  for (let i = 0; i < count; i++)
    instance[index + i] = subArray[i];
}
/*jazor:clr-member System.Collections.Generic.List<T>.TrimExcess()*/
export function _27c95e83eced65e9(instance) {
  let capacity = GetCapacity(instance);
  if (instance.length < Math.floor(capacity * 0.9))
    Capacities.set(instance, instance.length);
}
