import { CompareCore, CompareObjectsCore } from "System/Collections/Generic/ComparerT1Module.js";
import { _0289dcf579b8a65e } from "System/Collections/Generic/IComparerT1Module.js";
import { _7dffdd7244581cc5 } from "System/Collections/IComparerModule.js";
import { CreateReadOnlyArrayView } from "System/RuntimeModule.js";
function CompareDefault(left, right) {
  return CompareCore(left, right);
}
function CompareDefaultObject(left, right) {
  return CompareObjectsCore(left, right);
}
function CompareDefaultKey(left, right) {
  return CompareCore(left, right);
}
function RequireSingleDimensionArray(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  return instance;
}
function RequireSingleIndex_7f9f146f88dd75dd(instance, indices) {
  if (indices === null)
    throw new Error("ArgumentNullException: indices is null.");
  if (indices.length !== 1)
    throw new Error("ArgumentException: Indices length does not match array rank.");
  let index = indices[0];
  if (isNaN(index) || Math.floor(index) !== index || index < 0 || index >= instance.length)
    throw new Error("IndexOutOfRangeException: index is out of range.");
  return index;
}
function RequireSingleIndex_5f28cbbd29da3e94(instance, indices) {
  if (indices === null)
    throw new Error("ArgumentNullException: indices is null.");
  if (indices.length !== 1)
    throw new Error("ArgumentException: Indices length does not match array rank.");
  let index = indices[0];
  if (index < 0n || index >= BigInt(instance.length))
    throw new Error("IndexOutOfRangeException: index is out of range.");
  return Number(index);
}
/*jazor:clr-member static System.Array.AsReadOnly<T>(T[])*/
export function _abd52ebcdb6fefcb(array) {
  return CreateReadOnlyArrayView(array, "ArgumentNullException: array is null.");
}
/*jazor:clr-member static System.Array.Resize<T>(ref T[], int)*/
export function _127013d39cf5bff9(array, newSize) {
  if (newSize < 0)
    throw new Error("ArgumentOutOfRangeException: newSize is less than zero");
  let newArray = new Array(newSize);
  if (array === null)
    return [newArray];
  let copyLength = Math.min(array.length, newSize);
  for (let i = 0; i < copyLength; i++)
    newArray[i] = array[i];
  return [newArray];
}
/*jazor:clr-member static System.Array.Copy(System.Array, System.Array, long)*/
export function _7a3d7a78ff429283(sourceArray, destinationArray, length) {
  if (sourceArray === null)
    throw new Error("ArgumentNullException: sourceArray is null");
  if (destinationArray === null)
    throw new Error("ArgumentNullException: destinationArray is null");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (length > sourceArray.length)
    throw new Error("ArgumentException: length is greater than sourceArray length");
  if (length > destinationArray.length)
    throw new Error("ArgumentException: length is greater than destinationArray length");
  for (let i = 0; i < length; i++)
    destinationArray[i] = sourceArray[i];
}
/*jazor:clr-member static System.Array.Copy(System.Array, long, System.Array, long, long)*/
export function _e2bd26f0b897dcdc(sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
  if (sourceArray === null)
    throw new Error("ArgumentNullException: sourceArray is null");
  if (destinationArray === null)
    throw new Error("ArgumentNullException: destinationArray is null");
  let sourceOffset = Number(sourceIndex);
  let destinationOffset = Number(destinationIndex);
  let copyLength = Number(length);
  if (copyLength < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (sourceOffset < 0 || sourceOffset + copyLength > sourceArray.length)
    throw new Error("ArgumentOutOfRangeException: sourceIndex is out of range");
  if (destinationOffset < 0 || destinationOffset + copyLength > destinationArray.length)
    throw new Error("ArgumentOutOfRangeException: destinationIndex is out of range");
  for (let i = 0; i < copyLength; i++)
    destinationArray[destinationOffset + i] = sourceArray[sourceOffset + i];
}
/*jazor:clr-member static System.Array.ConstrainedCopy(System.Array, int, System.Array, int, int)*/
export function _e83857a6975e2bca(sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
  if (sourceArray === null || destinationArray === null)
    throw new Error("ArgumentNullException: array is null");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (sourceIndex < 0 || sourceIndex + length > sourceArray.length)
    throw new Error("ArgumentOutOfRangeException: sourceIndex is out of range");
  if (destinationIndex < 0 || destinationIndex + length > destinationArray.length)
    throw new Error("ArgumentOutOfRangeException: destinationIndex is out of range");
  for (let i = 0; i < length; i++)
    destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
}
/*jazor:clr-member static System.Array.Copy(System.Array, System.Array, int)*/
export function _236e3a8894f7381f(sourceArray, destinationArray, length) {
  if (sourceArray === null)
    throw new Error("ArgumentNullException: sourceArray is null");
  if (destinationArray === null)
    throw new Error("ArgumentNullException: destinationArray is null");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (length > sourceArray.length)
    throw new Error("ArgumentException: length is greater than sourceArray length");
  if (length > destinationArray.length)
    throw new Error("ArgumentException: length is greater than destinationArray length");
  for (let i = 0; i < length; i++)
    destinationArray[i] = sourceArray[i];
}
/*jazor:clr-member static System.Array.Copy(System.Array, int, System.Array, int, int)*/
export function _5afb5659a201668f(sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
  if (sourceArray === null)
    throw new Error("ArgumentNullException: sourceArray is null");
  if (destinationArray === null)
    throw new Error("ArgumentNullException: destinationArray is null");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (sourceIndex < 0 || sourceIndex + length > sourceArray.length)
    throw new Error("ArgumentOutOfRangeException: sourceIndex is out of range");
  if (destinationIndex < 0 || destinationIndex + length > destinationArray.length)
    throw new Error("ArgumentOutOfRangeException: destinationIndex is out of range");
  for (let i = 0; i < length; i++)
    destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
}
/*jazor:clr-member System.Array.GetValue(params int[])*/
export function _e938260256ca4a08(instance, indices) {
  let target = RequireSingleDimensionArray(instance);
  return target[RequireSingleIndex_7f9f146f88dd75dd(target, indices)];
}
/*jazor:clr-member System.Array.SetValue(object, params int[])*/
export function _8752076a83fbb3f1(instance, value, indices) {
  let target = RequireSingleDimensionArray(instance);
  target[RequireSingleIndex_7f9f146f88dd75dd(target, indices)] = value;
}
/*jazor:clr-member System.Array.GetValue(params long[])*/
export function _6a12948779406121(instance, indices) {
  let target = RequireSingleDimensionArray(instance);
  return target[RequireSingleIndex_5f28cbbd29da3e94(target, indices)];
}
/*jazor:clr-member System.Array.SetValue(object, params long[])*/
export function _e3923681669a96b5(instance, value, indices) {
  let target = RequireSingleDimensionArray(instance);
  target[RequireSingleIndex_5f28cbbd29da3e94(target, indices)] = value;
}
/*jazor:clr-member static System.Array.BinarySearch(System.Array, object)*/
export function _0c9e99640a975a5b(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = CompareDefaultObject(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch(System.Array, int, int, object)*/
export function _fa538add1f784012(array, index, length, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length is greater than array length");
  if (length === 0)
    return ~index;
  let left = index;
  let right = index + length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = CompareDefaultObject(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch(System.Array, object, System.Collections.IComparer)*/
export function _c453dd981ecbb5c5(array, value, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _7dffdd7244581cc5(comparer, array[mid], value) : CompareDefaultObject(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch(System.Array, int, int, object, System.Collections.IComparer)*/
export function _f1fb5c20cf9ffd4d(array, index, length, value, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length is greater than array length");
  if (length === 0)
    return ~index;
  let left = index;
  let right = index + length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _7dffdd7244581cc5(comparer, array[mid], value) : CompareDefaultObject(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch<T>(T[], T)*/
export function _75258b66e0bba01a(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = CompareDefault(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch<T>(T[], T, System.Collections.Generic.IComparer<T>)*/
export function _87f2af26c36fed01(array, value, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _0289dcf579b8a65e(comparer, array[mid], value) : CompareDefault(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch<T>(T[], int, int, T)*/
export function _60003ac825620c60(array, index, length, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length is greater than array length");
  if (length === 0)
    return ~index;
  let left = index;
  let right = index + length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = CompareDefault(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.BinarySearch<T>(T[], int, int, T, System.Collections.Generic.IComparer<T>)*/
export function _42b1da24db771714(array, index, length, value, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length is greater than array length");
  if (length === 0)
    return ~index;
  let left = index;
  let right = index + length - 1;
  while (left <= right) {
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _0289dcf579b8a65e(comparer, array[mid], value) : CompareDefault(array[mid], value);
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
/*jazor:clr-member static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)*/
export function _a73f4ff0bddcc6f6(array, converter) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (converter === null)
    throw new Error("ArgumentNullException: converter is null");
  return array.map(converter);
}
/*jazor:clr-member System.Array.CopyTo(System.Array, int)*/
export function _559d75b1e44b3eb0(instance, array, index) {
  if (instance === null)
    throw new Error("ArgumentNullException: instance is null");
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (index + instance.length > array.length)
    throw new Error("ArgumentException: not enough space in destination array");
  for (let i = 0; i < instance.length; i++)
    array[index + i] = instance[i];
}
/*jazor:clr-member System.Array.CopyTo(System.Array, long)*/
export function _02714528e8c676b0(instance, array, index) {
  if (instance === null)
    throw new Error("ArgumentNullException: instance is null");
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0n)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (Number(index) + instance.length > array.length)
    throw new Error("ArgumentException: not enough space in destination array");
  for (let i = 0; i < instance.length; i++)
    array[Number(index) + i] = instance[i];
}
/*jazor:clr-member static System.Array.Exists<T>(T[], System.Predicate<T>)*/
export function _3795c9344e3fe39f(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.some(match);
}
/*jazor:clr-member static System.Array.Fill<T>(T[], T, int, int)*/
export function _8edf171ab37f3a05(array, value, startIndex, count) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < 0)
    throw new Error("ArgumentOutOfRangeException: startIndex is less than zero");
  if (count < 0)
    throw new Error("ArgumentOutOfRangeException: count is less than zero");
  if (startIndex + count > array.length)
    throw new Error("ArgumentException: startIndex + count exceeds array length");
  array.fill(value, startIndex, startIndex + count);
}
/*jazor:clr-member static System.Array.Find<T>(T[], System.Predicate<T>)*/
export function _1dfc77048ccf0234(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.find(match) ?? null;
}
/*jazor:clr-member static System.Array.FindAll<T>(T[], System.Predicate<T>)*/
export function _b373eb093e6c7b63(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.filter(match);
}
/*jazor:clr-member static System.Array.FindIndex<T>(T[], System.Predicate<T>)*/
export function _64f5a7fd5c436edb(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.findIndex(match);
}
/*jazor:clr-member static System.Array.FindIndex<T>(T[], int, System.Predicate<T>)*/
export function _42e008ba24b77e94(array, startIndex, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  if (startIndex < 0 || startIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  for (let i = startIndex; i < array.length; i++) {
    if (match(array[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.FindIndex<T>(T[], int, int, System.Predicate<T>)*/
export function _fdfc005bdc859fff(array, startIndex, count, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  if (startIndex < 0 || startIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  if (count < 0 || startIndex + count > array.length)
    throw new Error("ArgumentOutOfRangeException: count is out of range");
  for (let i = startIndex; i < startIndex + count; i++) {
    if (match(array[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.FindLast<T>(T[], System.Predicate<T>)*/
export function _2786abe2cff245fa(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  for (let i = array.length - 1; i >= 0; i--) {
    if (match(array[i]))
      return array[i];
  }
  return null;
}
/*jazor:clr-member static System.Array.FindLastIndex<T>(T[], System.Predicate<T>)*/
export function _ea3118f38aa5f363(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  for (let i = array.length - 1; i >= 0; i--) {
    if (match(array[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.FindLastIndex<T>(T[], int, System.Predicate<T>)*/
export function _56359f972a00ab73(array, startIndex, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  if (startIndex < -1 || startIndex >= array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  for (let i = startIndex; i >= 0; i--) {
    if (match(array[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.FindLastIndex<T>(T[], int, int, System.Predicate<T>)*/
export function _6b63489e941ef0f0(array, startIndex, count, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  if (startIndex < -1 || startIndex >= array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  if (count < 0 || count > startIndex + 1)
    throw new Error("ArgumentOutOfRangeException: count is out of range");
  let endIndex = startIndex - count + 1;
  for (let i = startIndex; i >= endIndex; i--) {
    if (match(array[i]))
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.ForEach<T>(T[], System.Action<T>)*/
export function _ad1c39ab55fe27b9(array, action) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (action === null)
    throw new Error("ArgumentNullException: action is null");
  array.forEach(action);
}
/*jazor:clr-member static System.Array.IndexOf(System.Array, object)*/
export function _cde8d7a78af8dc9a(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (value == null)
    return -1;
  return array.indexOf(value);
}
/*jazor:clr-member static System.Array.IndexOf(System.Array, object, int)*/
export function _2151f4cd0a63b0a2(array, value, startIndex) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < 0 || startIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  for (let i = startIndex; i < array.length; i++) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.IndexOf(System.Array, object, int, int)*/
export function _c419efc216312a6a(array, value, startIndex, count) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < 0 || startIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  if (count < 0 || startIndex + count > array.length)
    throw new Error("ArgumentOutOfRangeException: count is out of range");
  for (let i = startIndex; i < startIndex + count; i++) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.IndexOf<T>(T[], T)*/
export function _34e8668cac3c06fa(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  return array.indexOf(value);
}
/*jazor:clr-member static System.Array.IndexOf<T>(T[], T, int)*/
export function _d7a4d17a98a17e7e(array, value, startIndex) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < 0 || startIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  for (let i = startIndex; i < array.length; i++) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.IndexOf<T>(T[], T, int, int)*/
export function _e3d80b27a67e8a0d(array, value, startIndex, count) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < 0 || startIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  if (count < 0 || startIndex + count > array.length)
    throw new Error("ArgumentOutOfRangeException: count is out of range");
  for (let i = startIndex; i < startIndex + count; i++) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.LastIndexOf(System.Array, object)*/
export function _85801a2dbc247f17(array, value) {
  if (array == null)
    throw new Error("ArgumentNullException: array is null");
  if (value == null)
    return -1;
  return array.lastIndexOf(value);
}
/*jazor:clr-member static System.Array.LastIndexOf(System.Array, object, int)*/
export function _6b23455f7b2f95ff(array, value, startIndex) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < -1 || startIndex >= array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  for (let i = startIndex; i >= 0; i--) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.LastIndexOf(System.Array, object, int, int)*/
export function _7f5af90fd2a084fe(array, value, startIndex, count) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < -1 || startIndex >= array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  if (count < 0 || count > startIndex + 1)
    throw new Error("ArgumentOutOfRangeException: count is out of range");
  let endIndex = startIndex - count + 1;
  for (let i = startIndex; i >= endIndex; i--) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.LastIndexOf<T>(T[], T)*/
export function _198d0f4fcb1c0679(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  return array.lastIndexOf(value);
}
/*jazor:clr-member static System.Array.LastIndexOf<T>(T[], T, int)*/
export function _5c2c6aa99d0e0549(array, value, startIndex) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < -1 || startIndex >= array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  for (let i = startIndex; i >= 0; i--) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.LastIndexOf<T>(T[], T, int, int)*/
export function _b5bf131d8947c855(array, value, startIndex, count) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (startIndex < -1 || startIndex >= array.length)
    throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
  if (count < 0 || count > startIndex + 1)
    throw new Error("ArgumentOutOfRangeException: count is out of range");
  let endIndex = startIndex - count + 1;
  for (let i = startIndex; i >= endIndex; i--) {
    if (array[i] === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member static System.Array.Reverse(System.Array, int, int)*/
export function _36c04f95b4ffdfd5(array, index, length) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length exceeds array length");
  let endIndex = index + length - 1;
  while (index < endIndex) {
    let temp = array[index];
    array[index] = array[endIndex];
    array[endIndex] = temp;
    index++;
    endIndex--;
  }
}
/*jazor:clr-member static System.Array.Reverse<T>(T[])*/
export function _e2b02681782c394b(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  array.reverse();
}
/*jazor:clr-member static System.Array.Reverse<T>(T[], int, int)*/
export function _5b0cbdf276c63339(array, index, length) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length exceeds array length");
  let endIndex = index + length - 1;
  while (index < endIndex) {
    let temp = array[index];
    array[index] = array[endIndex];
    array[endIndex] = temp;
    index++;
    endIndex--;
  }
}
function SortKeyItemRange(keys, items, index, length, comparison) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > keys.length)
    throw new Error("ArgumentException: index + length exceeds keys length");
  if (items !== null && index + length > items.length)
    throw new Error("ArgumentException: index + length exceeds items length");
  let order = new Array;
  for (let offset = 0; offset < length; offset++)
    order.push(offset);
  order.sort((left, right) => {
    return comparison(keys[index + left], keys[index + right]);
  });
  let sortedKeys = new Array;
  let sortedItems = items === null ? null : new Array;
  for (let offset = 0; offset < length; offset++) {
    let sourceOffset = order[offset];
    sortedKeys.push(keys[index + sourceOffset]);
    if (sortedItems !== null)
      sortedItems.push(items[index + sourceOffset]);
  }
  for (let offset = 0; offset < length; offset++) {
    keys[index + offset] = sortedKeys[offset];
    if (sortedItems !== null)
      items[index + offset] = sortedItems[offset];
  }
}
/*jazor:clr-member static System.Array.Sort(System.Array)*/
export function _07ee8311aaf13b6b(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  array.sort((left, right) => {
    return CompareDefault(left, right);
  });
}
/*jazor:clr-member static System.Array.Sort(System.Array, System.Array)*/
export function _4df21ca760120c59(keys, items) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  SortKeyItemRange(keys, items, 0, keys.length, (left, right) => {
    return CompareDefault(left, right);
  });
}
/*jazor:clr-member static System.Array.Sort(System.Array, int, int)*/
export function _4e10132b81a43421(array, index, length) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (length < 0)
    throw new Error("ArgumentOutOfRangeException: length is less than zero");
  if (index + length > array.length)
    throw new Error("ArgumentException: index + length exceeds array length");
  let subArray = array.slice(index, index + length);
  subArray.sort((left, right) => {
    return CompareDefault(left, right);
  });
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
/*jazor:clr-member static System.Array.Sort(System.Array, System.Array, int, int)*/
export function _12789d2affa27035(keys, items, index, length) {
  SortKeyItemRange(keys, items, index, length, (left, right) => {
    return CompareDefault(left, right);
  });
}
/*jazor:clr-member static System.Array.Sort(System.Array, System.Collections.IComparer)*/
export function _093c373956602c04(array, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (comparer === null)
    array.sort((left, right) => {
      return CompareDefault(left, right);
    });
  else
    array.sort((a, b) => {
      return _7dffdd7244581cc5(comparer, a, b);
    });
}
/*jazor:clr-member static System.Array.Sort(System.Array, System.Array, System.Collections.IComparer)*/
export function _122404a1fc2867ba(keys, items, comparer) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  SortKeyItemRange(keys, items, 0, keys.length, (left, right) => {
    return comparer === null ? CompareDefault(left, right) : _7dffdd7244581cc5(comparer, left, right);
  });
}
/*jazor:clr-member static System.Array.Sort(System.Array, int, int, System.Collections.IComparer)*/
export function _b2141b8c013bc1b0(array, index, length, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0 || length < 0 || index + length > array.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = array.slice(index, index + length);
  if (comparer === null)
    subArray.sort((left, right) => {
      return CompareDefault(left, right);
    });
  else
    subArray.sort((a, b) => {
      return _7dffdd7244581cc5(comparer, a, b);
    });
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
/*jazor:clr-member static System.Array.Sort(System.Array, System.Array, int, int, System.Collections.IComparer)*/
export function _a95c3f83e8cd4623(keys, items, index, length, comparer) {
  SortKeyItemRange(keys, items, index, length, (left, right) => {
    return comparer === null ? CompareDefault(left, right) : _7dffdd7244581cc5(comparer, left, right);
  });
}
/*jazor:clr-member static System.Array.Sort<T>(T[])*/
export function _382add2bad872f67(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  array.sort((left, right) => {
    return CompareDefault(left, right);
  });
}
/*jazor:clr-member static System.Array.Sort<TKey, TValue>(TKey[], TValue[])*/
export function _1a3ebd994898c67c(keys, items) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  SortKeyItemRange(keys, items, 0, keys.length, (left, right) => {
    return CompareDefaultKey(left, right);
  });
}
/*jazor:clr-member static System.Array.Sort<T>(T[], int, int)*/
export function _80e6f8922ae8703c(array, index, length) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0 || length < 0 || index + length > array.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = array.slice(index, index + length);
  subArray.sort((left, right) => {
    return CompareDefault(left, right);
  });
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
/*jazor:clr-member static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int)*/
export function _9b803c8e781cf3c0(keys, items, index, length) {
  SortKeyItemRange(keys, items, index, length, (left, right) => {
    return CompareDefaultKey(left, right);
  });
}
/*jazor:clr-member static System.Array.Sort<T>(T[], System.Collections.Generic.IComparer<T>)*/
export function _92474aed4e4823f3(array, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (comparer === null)
    array.sort((left, right) => {
      return CompareDefault(left, right);
    });
  else
    array.sort((__mref$e807ab5c712dcb19aa252ac8$0, __mref$e807ab5c712dcb19aa252ac8$1, __mref$e807ab5c712dcb19aa252ac8$2) => _0289dcf579b8a65e(__mref$e807ab5c712dcb19aa252ac8$0, __mref$e807ab5c712dcb19aa252ac8$1, __mref$e807ab5c712dcb19aa252ac8$2));
}
/*jazor:clr-member static System.Array.Sort<TKey, TValue>(TKey[], TValue[], System.Collections.Generic.IComparer<TKey>)*/
export function _dfd5fefaaa03a228(keys, items, comparer) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  SortKeyItemRange(keys, items, 0, keys.length, (left, right) => {
    return comparer === null ? CompareDefaultKey(left, right) : _0289dcf579b8a65e(comparer, left, right);
  });
}
/*jazor:clr-member static System.Array.Sort<T>(T[], int, int, System.Collections.Generic.IComparer<T>)*/
export function _55dbc52295bd7984(array, index, length, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0 || length < 0 || index + length > array.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = array.slice(index, index + length);
  if (comparer === null)
    subArray.sort((left, right) => {
      return CompareDefault(left, right);
    });
  else
    subArray.sort((__mref$b750c15f31e72bb2ece90fc6$0, __mref$b750c15f31e72bb2ece90fc6$1, __mref$b750c15f31e72bb2ece90fc6$2) => _0289dcf579b8a65e(__mref$b750c15f31e72bb2ece90fc6$0, __mref$b750c15f31e72bb2ece90fc6$1, __mref$b750c15f31e72bb2ece90fc6$2));
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
/*jazor:clr-member static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int, System.Collections.Generic.IComparer<TKey>)*/
export function _f3e7263659ac2e30(keys, items, index, length, comparer) {
  SortKeyItemRange(keys, items, index, length, (left, right) => {
    return comparer === null ? CompareDefaultKey(left, right) : _0289dcf579b8a65e(comparer, left, right);
  });
}
/*jazor:clr-member static System.Array.Sort<T>(T[], System.Comparison<T>)*/
export function _c8fcae59a3aca6f6(array, comparison) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (comparison === null)
    throw new Error("ArgumentNullException: comparison is null");
  array.sort(comparison);
}
/*jazor:clr-member static System.Array.TrueForAll<T>(T[], System.Predicate<T>)*/
export function _7deb21b3fbe579c9(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.every(match);
}
