import { compareCore } from "System/Collections/Generic/ComparerT1Module.js";
import { _0289dcf579b8a65e } from "System/Collections/Generic/IComparerT1Module.js";
function ensureInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(message);
}
function ensureTargetArray(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
}
function ensureTargetIndex(array, arrayIndex) {
  ensureWholeNumber(arrayIndex, "ArgumentOutOfRangeException: arrayIndex must be a whole number.");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
}
function ensureCopyCapacity(array, arrayIndex, copyCount) {
  if (arrayIndex + copyCount > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
}
function ensureInsertIndex(instance, index) {
  ensureInstance(instance);
  ensureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
  if (index < 0 || index > instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
}
function ensureExistingIndex(instance, index) {
  ensureInstance(instance);
  ensureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
}
function ensureRemoveRange(instance, index, count) {
  ensureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  ensureInsertIndex(instance, index);
  if (count < 0 || index + count > instance.length)
    throw new Error("ArgumentException: offset and length were out of bounds for the list.");
}
function ensureMatch(match) {
  if (match === null)
    throw new Error("ArgumentNullException: match is null");
}
function ensureForwardSearchStartIndex(instance, startIndex) {
  ensureInsertIndex(instance, startIndex);
}
function ensureForwardSearchRange(instance, startIndex, count) {
  ensureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  ensureForwardSearchStartIndex(instance, startIndex);
  if (count < 0 || startIndex + count > instance.length)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
}
function ensureLastSearchStartIndex(instance, startIndex, parameterName) {
  ensureInstance(instance);
  ensureWholeNumber(startIndex, `ArgumentOutOfRangeException: ${parameterName} must be a whole number.`);
  if (instance.length === 0) {
    if (startIndex !== -1)
      throw new Error(`ArgumentOutOfRangeException: ${parameterName} is out of range.`);
    return;
  }
  if (startIndex < 0 || startIndex >= instance.length)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName} is out of range.`);
}
function ensureLastSearchRange(instance, startIndex, count, startIndexName) {
  ensureLastSearchStartIndex(instance, startIndex, startIndexName);
  ensureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
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
function equalsForListSearch(left, right) {
  let leftNumber, rightNumber;
  if (Object.is(left, right))
    return true;
  if (typeof left === "number" && (leftNumber = left, true) && (typeof right === "number" && (rightNumber = right, true)))
    return leftNumber === rightNumber;
  return false;
}
function appendRange(instance, collection) {
  let source;
  ensureInstance(instance);
  if (collection === null)
    throw new Error("ArgumentNullException: collection is null");
  if (Array.isArray(collection) && (source = collection, true) && Object.is(instance, source)) {
    let originalLength = source.length;
    for (let i = 0; i < originalLength; i++)
      instance.push(source[i]);
    return;
  }
  for (let item of collection)
    instance.push(item);
}
export function _d389c31d59037b42(instance, index) {
  ensureExistingIndex(instance, index);
  return instance[index];
}
export function _c16a7960302ea054(instance, index, value) {
  ensureExistingIndex(instance, index);
  instance[index] = value;
}
export function _a2660853a4ebc1f6(instance, collection) {
  appendRange(instance, collection);
}
export function _9a3a4817585dded1(instance, array) {
  ensureInstance(instance);
  ensureTargetArray(array);
  ensureCopyCapacity(array, 0, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[i] = instance[i];
}
export function _0fdf1627d283f8ae(instance, index, array, arrayIndex, count) {
  ensureInstance(instance);
  ensureTargetArray(array);
  ensureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
  ensureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
  ensureTargetIndex(array, arrayIndex);
  if (index < 0 || index > instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  if (count < 0)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
  if (index + count > instance.length)
    throw new Error("ArgumentException: source index and count are out of range.");
  ensureCopyCapacity(array, arrayIndex, count);
  for (let i = 0; i < count; i++)
    array[arrayIndex + i] = instance[index + i];
}
export function _3559b1ff2a643922(instance, array, arrayIndex) {
  ensureInstance(instance);
  ensureTargetArray(array);
  ensureTargetIndex(array, arrayIndex);
  ensureCopyCapacity(array, arrayIndex, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[arrayIndex + i] = instance[i];
}
export function _db9b68fbc73e342b(instance, startIndex, match) {
  ensureMatch(match);
  ensureForwardSearchStartIndex(instance, startIndex);
  if (startIndex === instance.length)
    return -1;
  for (let i = startIndex; i < instance.length; i++) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
export function _41b337b09c5daf75(instance, startIndex, count, match) {
  ensureMatch(match);
  ensureForwardSearchRange(instance, startIndex, count);
  if (count === 0)
    return -1;
  let end = startIndex + count;
  for (let i = startIndex; i < end; i++) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
export function _de0943e496e36f2d(instance, match) {
  ensureInstance(instance);
  ensureMatch(match);
  for (let i = instance.length; i > 0; i--) {
    if (match(instance[i - 1]))
      return instance[i - 1];
  }
  return null;
}
export function _ae1a0b59c73f2b1a(instance, match) {
  ensureInstance(instance);
  ensureMatch(match);
  for (let i = instance.length; i > 0; i--) {
    if (match(instance[i - 1]))
      return i - 1;
  }
  return -1;
}
export function _081aa9ae0b09d058(instance, startIndex, match) {
  ensureMatch(match);
  ensureLastSearchStartIndex(instance, startIndex, "startIndex");
  if (instance.length === 0)
    return -1;
  for (let i = startIndex; i >= 0; i--) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
export function _58cc54dc07e440c4(instance, startIndex, count, match) {
  ensureMatch(match);
  ensureLastSearchRange(instance, startIndex, count, "startIndex");
  if (count === 0)
    return -1;
  let start = startIndex - count + 1;
  for (let i = startIndex; i >= start; i--) {
    if (match(instance[i]))
      return i;
  }
  return -1;
}
export function _c35c9c99a23ff96a(instance, index, count) {
  ensureRemoveRange(instance, index, count);
  return instance.slice(index, index + count);
}
export function _71ee35e0e260eb27(instance, item, index) {
  ensureForwardSearchStartIndex(instance, index);
  if (index === instance.length)
    return -1;
  for (let i = index; i < instance.length; i++) {
    if (equalsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
export function _5ee52e4e4fc54e6d(instance, item, index, count) {
  ensureForwardSearchRange(instance, index, count);
  if (count === 0)
    return -1;
  let end = index + count;
  for (let i = index; i < end; i++) {
    if (equalsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
export function _0dc538197c677986(instance, index, item) {
  ensureInsertIndex(instance, index);
  instance.splice(index, 0, item);
}
export function _56ef9aefabac7c09(instance, index, collection) {
  let source;
  ensureInsertIndex(instance, index);
  if (collection === null)
    throw new Error("ArgumentNullException: collection is null");
  if (Array.isArray(collection) && (source = collection, true) && Object.is(instance, source)) {
    let snapshot = new Array;
    for (let i = 0; i < source.length; i++)
      snapshot.push(source[i]);
    let selfInsertionIndex = index;
    for (let i = 0; i < snapshot.length; i++) {
      instance.splice(selfInsertionIndex, 0, snapshot[i]);
      selfInsertionIndex++;
    }
    return;
  }
  let insertionIndex = index;
  for (let item of collection) {
    instance.splice(insertionIndex, 0, item);
    insertionIndex++;
  }
}
export function _279befda6399cda5(instance, item, index) {
  ensureLastSearchStartIndex(instance, index, "index");
  if (instance.length === 0)
    return -1;
  for (let i = index; i >= 0; i--) {
    if (equalsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
export function _b2f1955b62962812(instance, item, index, count) {
  ensureLastSearchRange(instance, index, count, "index");
  if (count === 0)
    return -1;
  let start = index - count + 1;
  for (let i = index; i >= start; i--) {
    if (equalsForListSearch(instance[i], item))
      return i;
  }
  return -1;
}
export function _562f832fd220e768(instance, item) {
  ensureInstance(instance);
  for (let i = 0; i < instance.length; i++) {
    if (equalsForListSearch(instance[i], item)) {
      instance.splice(i, 1);
      return true;
    }
  }
  return false;
}
export function _b864beda26f186e2(instance, match) {
  ensureInstance(instance);
  ensureMatch(match);
  let count = 0;
  for (let i = instance.length; i > 0; i--) {
    if (match(instance[i - 1])) {
      instance.splice(i - 1, 1);
      count++;
    }
  }
  return count;
}
export function _a5e8c6b27df6470b(instance, index) {
  ensureExistingIndex(instance, index);
  instance.splice(index, 1);
}
export function _8425758ef4e7b6f9(instance, index, count) {
  ensureRemoveRange(instance, index, count);
  if (count === 0)
    return;
  instance.splice(index, count);
}
export function _56dc1af8af32e484(instance, index, count) {
  ensureRemoveRange(instance, index, count);
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
function compareDefault(left, right) {
  return compareCore(left, right);
}
export function _36a478f36b41a6d2(instance) {
  ensureInstance(instance);
  instance.sort((left, right) => {
    return compareDefault(left, right);
  });
}
export function _5fa599e721e252ff(instance, comparer) {
  ensureInstance(instance);
  if (comparer === null)
    instance.sort((left, right) => {
      return compareDefault(left, right);
    });
  else
    instance.sort((left, right) => {
      return _0289dcf579b8a65e(comparer, left, right);
    });
}
export function _19207851b52a5287(instance, index, count, comparer) {
  ensureInstance(instance);
  ensureRemoveRange(instance, index, count);
  if (count <= 1)
    return;
  let subArray = instance.slice(index, index + count);
  if (comparer !== null)
    subArray.sort((a, b) => {
      return _0289dcf579b8a65e(comparer, a, b);
    });
  else
    subArray.sort((left, right) => {
      return compareDefault(left, right);
    });
  for (let i = 0; i < count; i++)
    instance[index + i] = subArray[i];
}
