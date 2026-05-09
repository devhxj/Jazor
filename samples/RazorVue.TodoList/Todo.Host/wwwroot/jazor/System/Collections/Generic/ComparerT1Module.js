export function ensureComparerInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
export function compareObjectsCore(x, y) {
  let leftNumber, rightNumber, leftString, rightString, leftBool, rightBool, leftBigInt, rightBigInt;
  if (Object.is(x, y))
    return 0;
  if (x === null)
    return -1;
  if (y === null)
    return 1;
  if (typeof x === "number" && (leftNumber = x, true) && (typeof y === "number" && (rightNumber = y, true))) {
    if (isNaN(leftNumber))
      return isNaN(rightNumber) ? 0 : 1;
    if (isNaN(rightNumber))
      return -1;
    if (leftNumber < rightNumber)
      return -1;
    if (leftNumber > rightNumber)
      return 1;
    return 0;
  }
  if (typeof x === "string" && (leftString = x, true) && (typeof y === "string" && (rightString = y, true))) {
    if (leftString < rightString)
      return -1;
    if (leftString > rightString)
      return 1;
    return 0;
  }
  if (typeof x === "boolean" && (leftBool = x, true) && (typeof y === "boolean" && (rightBool = y, true)))
    return leftBool === rightBool ? 0 : leftBool ? 1 : -1;
  if (typeof x === "bigint" && (leftBigInt = x, true) && (typeof y === "bigint" && (rightBigInt = y, true))) {
    if (leftBigInt < rightBigInt)
      return -1;
    if (leftBigInt > rightBigInt)
      return 1;
    return 0;
  }
  throw new Error("ArgumentException: At least one object must implement IComparable.");
}
export function compareCore(x, y) {
  return compareObjectsCore(x, y);
}
export function _a4222c99b516b861(instance, x, y) {
  ensureComparerInstance(instance);
  return compareCore(x, y);
}
