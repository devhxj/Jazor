function EnsureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
}
/*jazor:clr-member System.Collections.Generic.IReadOnlyList<T>.this[int].get*/
export function _b6ea5fe846ef1d65(instance, index) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return instance[index];
}
