function ensureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName} must be a whole number.`);
}
export function _5d3d00c3ee9d4076(instance, array, index) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (array === null)
    throw new Error("ArgumentNullException: array is null");
  ensureWholeNumber(index, "index");
  if (index < 0 || index > array.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  if (index + instance.length > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
  for (let i = 0; i < instance.length; i++)
    array[index + i] = instance[i];
}
export const ICollectionModule = { ensureWholeNumber, _5d3d00c3ee9d4076 };
