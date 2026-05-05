import { _0289dcf579b8a65e } from "System/Collections/Generic/IComparerT1Module.js";
import { _7dffdd7244581cc5 } from "System/Collections/IComparerModule.js";
import { _e16eea9fe3891a62 } from "System/StringModule.js";
export function _abd52ebcdb6fefcb(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  return Object.freeze(array.slice());
}
export function _127013d39cf5bff9(array, newSize) {
  if (newSize < 0)
    throw new Error("ArgumentOutOfRangeException: newSize is less than zero");
  let newArray = [newSize];
  if (array === null)
    return [newArray];
  let copyLength = Math.min(array.length, newSize);
  for (let i = 0; i < copyLength; i++)
    newArray[i] = array[i];
  return [newArray];
}
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
export function _e2bd26f0b897dcdc(sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
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
export function _0c9e99640a975a5b(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let __cacc$2238e84921769dda78685d1d, __cacc$6c1ada83463e101cabceb32d;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = _e16eea9fe3891a62((__cacc$2238e84921769dda78685d1d = array[mid], __cacc$2238e84921769dda78685d1d == null ? undefined : __cacc$2238e84921769dda78685d1d.toString()), (__cacc$6c1ada83463e101cabceb32d = value, __cacc$6c1ada83463e101cabceb32d == null ? undefined : __cacc$6c1ada83463e101cabceb32d.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
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
    let __cacc$3733a2cc521fdb2a50e5b233, __cacc$6ad4c844449d233cea5ddeda;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = _e16eea9fe3891a62((__cacc$3733a2cc521fdb2a50e5b233 = array[mid], __cacc$3733a2cc521fdb2a50e5b233 == null ? undefined : __cacc$3733a2cc521fdb2a50e5b233.toString()), (__cacc$6ad4c844449d233cea5ddeda = value, __cacc$6ad4c844449d233cea5ddeda == null ? undefined : __cacc$6ad4c844449d233cea5ddeda.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
export function _c453dd981ecbb5c5(array, value, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let __cacc$7cff9395d83da6ff2670111b, __cacc$b816545c043816d7fc6c13ee;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _7dffdd7244581cc5(comparer, array[mid], value) : _e16eea9fe3891a62((__cacc$7cff9395d83da6ff2670111b = array[mid], __cacc$7cff9395d83da6ff2670111b == null ? undefined : __cacc$7cff9395d83da6ff2670111b.toString()), (__cacc$b816545c043816d7fc6c13ee = value, __cacc$b816545c043816d7fc6c13ee == null ? undefined : __cacc$b816545c043816d7fc6c13ee.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
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
    let __cacc$becc8b2a5311b0bfdc66642b, __cacc$cfefc198df9a8b673972d177;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _7dffdd7244581cc5(comparer, array[mid], value) : _e16eea9fe3891a62((__cacc$becc8b2a5311b0bfdc66642b = array[mid], __cacc$becc8b2a5311b0bfdc66642b == null ? undefined : __cacc$becc8b2a5311b0bfdc66642b.toString()), (__cacc$cfefc198df9a8b673972d177 = value, __cacc$cfefc198df9a8b673972d177 == null ? undefined : __cacc$cfefc198df9a8b673972d177.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
export function _75258b66e0bba01a(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let __cacc$fc27c7728c29931b5f38b3ca, __cacc$dfc0800e5910d0fe8b58f21d;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = _e16eea9fe3891a62((__cacc$fc27c7728c29931b5f38b3ca = array[mid], __cacc$fc27c7728c29931b5f38b3ca == null ? undefined : __cacc$fc27c7728c29931b5f38b3ca.toString()), (__cacc$dfc0800e5910d0fe8b58f21d = value, __cacc$dfc0800e5910d0fe8b58f21d == null ? undefined : __cacc$dfc0800e5910d0fe8b58f21d.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
export function _87f2af26c36fed01(array, value, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (array.length === 0)
    return -1;
  let left = 0;
  let right = array.length - 1;
  while (left <= right) {
    let __cacc$0922bba4640f373c379494e9, __cacc$ac4d3844a0122d5d8c07f267;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _0289dcf579b8a65e(comparer, array[mid], value) : _e16eea9fe3891a62((__cacc$0922bba4640f373c379494e9 = array[mid], __cacc$0922bba4640f373c379494e9 == null ? undefined : __cacc$0922bba4640f373c379494e9.toString()), (__cacc$ac4d3844a0122d5d8c07f267 = value, __cacc$ac4d3844a0122d5d8c07f267 == null ? undefined : __cacc$ac4d3844a0122d5d8c07f267.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
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
    let __cacc$a8674b0befa0e87a4dbca873, __cacc$b93506786f41fde19faf46bf;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = _e16eea9fe3891a62((__cacc$a8674b0befa0e87a4dbca873 = array[mid], __cacc$a8674b0befa0e87a4dbca873 == null ? undefined : __cacc$a8674b0befa0e87a4dbca873.toString()), (__cacc$b93506786f41fde19faf46bf = value, __cacc$b93506786f41fde19faf46bf == null ? undefined : __cacc$b93506786f41fde19faf46bf.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
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
    let __cacc$8bc86142bc4477da944010a9, __cacc$befee0a76725d095e43b8d34;
    let mid = left + Math.floor((right - left) / 2);
    let cmp = comparer !== null ? _0289dcf579b8a65e(comparer, array[mid], value) : _e16eea9fe3891a62((__cacc$8bc86142bc4477da944010a9 = array[mid], __cacc$8bc86142bc4477da944010a9 == null ? undefined : __cacc$8bc86142bc4477da944010a9.toString()), (__cacc$befee0a76725d095e43b8d34 = value, __cacc$befee0a76725d095e43b8d34 == null ? undefined : __cacc$befee0a76725d095e43b8d34.toString()));
    if (cmp === 0)
      return mid;
    if (cmp < 0)
      left = mid + 1;
    else
      right = mid - 1;
  }
  return ~left;
}
export function _a73f4ff0bddcc6f6(array, converter) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (converter === null)
    throw new Error("ArgumentNullException: converter is null");
  return array.map(converter);
}
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
export function _02714528e8c676b0(instance, array, index) {
  if (instance === null)
    throw new Error("ArgumentNullException: instance is null");
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < BigInt.zero)
    throw new Error("ArgumentOutOfRangeException: index is less than zero");
  if (Number(index) + instance.length > array.length)
    throw new Error("ArgumentException: not enough space in destination array");
  for (let i = 0; i < instance.length; i++)
    array[Number(index) + i] = instance[i];
}
export function _3795c9344e3fe39f(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.some(match);
}
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
export function _1dfc77048ccf0234(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.find(match);
}
export function _b373eb093e6c7b63(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.filter(match);
}
export function _64f5a7fd5c436edb(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.findIndex(match);
}
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
export function _ad1c39ab55fe27b9(array, action) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (action === null)
    throw new Error("ArgumentNullException: action is null");
  array.forEach(action);
}
export function _cde8d7a78af8dc9a(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (value === null)
    return -1;
  return array.indexOf(value);
}
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
export function _34e8668cac3c06fa(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  return array.indexOf(value);
}
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
export function _85801a2dbc247f17(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (value === null)
    return -1;
  return array.lastIndexOf(value);
}
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
export function _198d0f4fcb1c0679(array, value) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  return array.lastIndexOf(value);
}
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
export function _e2b02681782c394b(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  array.reverse();
}
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
export function _07ee8311aaf13b6b(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  array.sort();
}
export function _4df21ca760120c59(keys, items) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (items !== null && keys.length !== items.length)
    throw new Error("ArgumentException: keys and items have different lengths");
  keys.sort();
}
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
  subArray.sort();
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
export function _12789d2affa27035(keys, items, index, length) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (index < 0 || length < 0 || index + length > keys.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = keys.slice(index, index + length);
  subArray.sort();
  for (let i = 0; i < length; i++)
    keys[index + i] = subArray[i];
}
export function _093c373956602c04(array, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (comparer === null)
    array.sort();
  else
    array.sort((a, b) => {
      return _7dffdd7244581cc5(comparer, a, b);
    });
}
export function _122404a1fc2867ba(keys, items, comparer) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (comparer === null)
    keys.sort();
  else
    keys.sort((a, b) => {
      return _7dffdd7244581cc5(comparer, a, b);
    });
}
export function _b2141b8c013bc1b0(array, index, length, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0 || length < 0 || index + length > array.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = array.slice(index, index + length);
  if (comparer === null)
    subArray.sort();
  else
    subArray.sort((a, b) => {
      return _7dffdd7244581cc5(comparer, a, b);
    });
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
export function _a95c3f83e8cd4623(keys, items, index, length, comparer) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (index < 0 || length < 0 || index + length > keys.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = keys.slice(index, index + length);
  if (comparer === null)
    subArray.sort();
  else
    subArray.sort((a, b) => {
      return _7dffdd7244581cc5(comparer, a, b);
    });
  for (let i = 0; i < length; i++)
    keys[index + i] = subArray[i];
}
export function _382add2bad872f67(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  array.sort();
}
export function _1a3ebd994898c67c(keys, items) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  keys.sort();
}
export function _80e6f8922ae8703c(array, index, length) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0 || length < 0 || index + length > array.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = array.slice(index, index + length);
  subArray.sort();
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
export function _9b803c8e781cf3c0(keys, items, index, length) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (index < 0 || length < 0 || index + length > keys.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = keys.slice(index, index + length);
  subArray.sort();
  for (let i = 0; i < length; i++)
    keys[index + i] = subArray[i];
}
export function _92474aed4e4823f3(array, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (comparer === null)
    array.sort();
  else
    array.sort((__mref$9000162b8044a0a1ff86d2fb$0, __mref$9000162b8044a0a1ff86d2fb$1, __mref$9000162b8044a0a1ff86d2fb$2) => _0289dcf579b8a65e(__mref$9000162b8044a0a1ff86d2fb$0, __mref$9000162b8044a0a1ff86d2fb$1, __mref$9000162b8044a0a1ff86d2fb$2));
}
export function _dfd5fefaaa03a228(keys, items, comparer) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (comparer === null)
    keys.sort();
  else
    keys.sort((__mref$a78b583a65719b4e1fa96b40$0, __mref$a78b583a65719b4e1fa96b40$1, __mref$a78b583a65719b4e1fa96b40$2) => _0289dcf579b8a65e(__mref$a78b583a65719b4e1fa96b40$0, __mref$a78b583a65719b4e1fa96b40$1, __mref$a78b583a65719b4e1fa96b40$2));
}
export function _55dbc52295bd7984(array, index, length, comparer) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (index < 0 || length < 0 || index + length > array.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = array.slice(index, index + length);
  if (comparer === null)
    subArray.sort();
  else
    subArray.sort((__mref$5d703019b825ead303437e3b$0, __mref$5d703019b825ead303437e3b$1, __mref$5d703019b825ead303437e3b$2) => _0289dcf579b8a65e(__mref$5d703019b825ead303437e3b$0, __mref$5d703019b825ead303437e3b$1, __mref$5d703019b825ead303437e3b$2));
  for (let i = 0; i < length; i++)
    array[index + i] = subArray[i];
}
export function _f3e7263659ac2e30(keys, items, index, length, comparer) {
  if (keys === null)
    throw new Error("ArgumentNullException: keys is null");
  if (index < 0 || length < 0 || index + length > keys.length)
    throw new Error("ArgumentException: invalid index or length");
  let subArray = keys.slice(index, index + length);
  if (comparer === null)
    subArray.sort();
  else
    subArray.sort((__mref$2daf1c7be64afc5a6d938af2$0, __mref$2daf1c7be64afc5a6d938af2$1, __mref$2daf1c7be64afc5a6d938af2$2) => _0289dcf579b8a65e(__mref$2daf1c7be64afc5a6d938af2$0, __mref$2daf1c7be64afc5a6d938af2$1, __mref$2daf1c7be64afc5a6d938af2$2));
  for (let i = 0; i < length; i++)
    keys[index + i] = subArray[i];
}
export function _c8fcae59a3aca6f6(array, comparison) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (comparison === null)
    throw new Error("ArgumentNullException: comparison is null");
  array.sort(comparison);
}
export function _7deb21b3fbe579c9(array, match) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
  return array.every(match);
}
export const ArrayModule = {
  _abd52ebcdb6fefcb,
  _127013d39cf5bff9,
  _7a3d7a78ff429283,
  _e2bd26f0b897dcdc,
  _e83857a6975e2bca,
  _236e3a8894f7381f,
  _5afb5659a201668f,
  _0c9e99640a975a5b,
  _fa538add1f784012,
  _c453dd981ecbb5c5,
  _f1fb5c20cf9ffd4d,
  _75258b66e0bba01a,
  _87f2af26c36fed01,
  _60003ac825620c60,
  _42b1da24db771714,
  _a73f4ff0bddcc6f6,
  _559d75b1e44b3eb0,
  _02714528e8c676b0,
  _3795c9344e3fe39f,
  _8edf171ab37f3a05,
  _1dfc77048ccf0234,
  _b373eb093e6c7b63,
  _64f5a7fd5c436edb,
  _42e008ba24b77e94,
  _fdfc005bdc859fff,
  _2786abe2cff245fa,
  _ea3118f38aa5f363,
  _56359f972a00ab73,
  _6b63489e941ef0f0,
  _ad1c39ab55fe27b9,
  _cde8d7a78af8dc9a,
  _2151f4cd0a63b0a2,
  _c419efc216312a6a,
  _34e8668cac3c06fa,
  _d7a4d17a98a17e7e,
  _e3d80b27a67e8a0d,
  _85801a2dbc247f17,
  _6b23455f7b2f95ff,
  _7f5af90fd2a084fe,
  _198d0f4fcb1c0679,
  _5c2c6aa99d0e0549,
  _b5bf131d8947c855,
  _36c04f95b4ffdfd5,
  _e2b02681782c394b,
  _5b0cbdf276c63339,
  _07ee8311aaf13b6b,
  _4df21ca760120c59,
  _4e10132b81a43421,
  _12789d2affa27035,
  _093c373956602c04,
  _122404a1fc2867ba,
  _b2141b8c013bc1b0,
  _a95c3f83e8cd4623,
  _382add2bad872f67,
  _1a3ebd994898c67c,
  _80e6f8922ae8703c,
  _9b803c8e781cf3c0,
  _92474aed4e4823f3,
  _dfd5fefaaa03a228,
  _55dbc52295bd7984,
  _f3e7263659ac2e30,
  _c8fcae59a3aca6f6,
  _7deb21b3fbe579c9
};
