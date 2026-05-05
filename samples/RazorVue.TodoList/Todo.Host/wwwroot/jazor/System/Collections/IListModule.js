function ensureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName} must be a whole number.`);
}
export function _049fed3e1cad6543(instance, index) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  ensureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return instance[index];
}
export const IListModule = { ensureWholeNumber, _049fed3e1cad6543 };
