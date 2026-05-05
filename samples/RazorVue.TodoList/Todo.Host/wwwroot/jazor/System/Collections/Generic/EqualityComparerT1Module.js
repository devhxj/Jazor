import { _5ad63706a889c294 } from "System/StringModule.js";
export function ensureComparerInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function hashStringCore(text) {
  let hash = 17;
  for (let i = 0; i < text.length; i++)
    hash = hash * 31 + _5ad63706a889c294(text, i) | 0;
  return hash;
}
export function equalsCore(left, right) {
  let leftNumber, rightNumber;
  if (Object.is(left, right))
    return true;
  if (typeof left === "number" && (leftNumber = left, true) && (typeof right === "number" && (rightNumber = right, true)))
    return leftNumber === rightNumber;
  return false;
}
export function getHashCodeCore(value) {
  let boolValue, numberValue, stringValue, bigIntValue;
  if (value === null)
    return 0;
  if (typeof value === "boolean" && (boolValue = value, true))
    return boolValue ? 1 : 0;
  if (typeof value === "number" && (numberValue = value, true)) {
    if (isNaN(numberValue) || numberValue === 0)
      return 0;
    if (Math.floor(numberValue) === numberValue && numberValue >= -2147483648 && numberValue <= 2147483647)
      return numberValue | 0;
    return hashStringCore(numberValue.toString());
  }
  if (typeof value === "string" && (stringValue = value, true))
    return hashStringCore(stringValue);
  if (typeof value === "bigint" && (bigIntValue = value, true))
    return hashStringCore(bigIntValue.toString());
  let text = value.toString();
  return text === null ? 0 : hashStringCore(text);
}
export function _4614e5ce6b42a7ad(instance, x, y) {
  ensureComparerInstance(instance);
  return equalsCore(x, y);
}
export function _2c3736bd7d205921(instance, obj) {
  ensureComparerInstance(instance);
  return getHashCodeCore(obj);
}
export const EqualityComparerT1Module = {
  ensureComparerInstance,
  hashStringCore,
  equalsCore,
  getHashCodeCore,
  _4614e5ce6b42a7ad,
  _2c3736bd7d205921
};
