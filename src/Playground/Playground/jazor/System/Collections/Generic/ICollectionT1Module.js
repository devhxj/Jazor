function ensureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName} must be a whole number.`);
}
export function _03c4a0ae3554065f(instance, array, arrayIndex) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  ensureWholeNumber(arrayIndex, "arrayIndex");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
  if (arrayIndex + instance.length > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
  for (let i = 0; i < instance.length; i++)
    array[arrayIndex + i] = instance[i];
}
