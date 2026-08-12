function ensureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName} must be a whole number.`);
}
function ensureSource(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function ensureTarget(array) {
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
}
function ensureTargetIndex(array, arrayIndex) {
  ensureWholeNumber(arrayIndex, "arrayIndex");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
}
function ensureCopyCapacity(array, arrayIndex, copyCount) {
  if (arrayIndex + copyCount > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
}
export function _d4e5f6a7b8c9d0e1(list) {
  if (list === null)
    throw new Error("ArgumentNullException: list is null");
  let snapshot = Array.from(list);
  return Object.freeze(snapshot);
}
export function _e5f6a7b8c9d0e1f2() {
  return Object.freeze(new Array);
}
export function _b8c9d0e1f2a3b4c5(instance, index) {
  ensureSource(instance);
  ensureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return instance[index];
}
export function _d0e1f2a3b4c5d6e7(instance, array) {
  ensureSource(instance);
  ensureTarget(array);
  ensureCopyCapacity(array, 0, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[i] = instance[i];
}
export function _e1f2a3b4c5d6e7f8(instance, array, arrayIndex) {
  ensureSource(instance);
  ensureTarget(array);
  ensureTargetIndex(array, arrayIndex);
  ensureCopyCapacity(array, arrayIndex, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[arrayIndex + i] = instance[i];
}
export function _f2a3b4c5d6e7f8a9(instance, index, array, arrayIndex, count) {
  ensureSource(instance);
  ensureTarget(array);
  ensureWholeNumber(index, "index");
  ensureWholeNumber(count, "count");
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
