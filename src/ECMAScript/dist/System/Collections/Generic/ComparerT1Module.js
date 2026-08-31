let DefaultInstance = null;
export function EnsureComparerInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
export function CompareObjectsCore(x, y) {
  if (Object.is(x, y))
    return 0;
  if (x == null)
    return -1;
  if (y == null)
    return 1;
  if (typeof x === "number" && typeof y === "number") {
    let leftNumber = x;
    let rightNumber = y;
    if (isNaN(leftNumber))
      return isNaN(rightNumber) ? 0 : -1;
    if (isNaN(rightNumber))
      return 1;
    if (leftNumber < rightNumber)
      return -1;
    if (leftNumber > rightNumber)
      return 1;
    return 0;
  }
  if (typeof x === "string" && typeof y === "string") {
    let leftString = x;
    let rightString = y;
    if (leftString < rightString)
      return -1;
    if (leftString > rightString)
      return 1;
    return 0;
  }
  if (typeof x === "boolean" && typeof y === "boolean") {
    let leftBool = x;
    let rightBool = y;
    return leftBool === rightBool ? 0 : leftBool ? 1 : -1;
  }
  if (typeof x === "bigint" && typeof y === "bigint") {
    let leftBigInt = x;
    let rightBigInt = y;
    if (leftBigInt < rightBigInt)
      return -1;
    if (leftBigInt > rightBigInt)
      return 1;
    return 0;
  }
  throw new Error("ArgumentException: At least one object must implement IComparable.");
}
export function CompareCore(x, y) {
  return CompareObjectsCore(x, y);
}
export function CompareInstance(instance, x, y) {
  EnsureComparerInstance(instance);
  let compare = Reflect.get(instance, "compare");
  if (compare === null)
    throw new Error("MissingMethodException: comparer does not expose compare.");
  return Reflect.apply(compare, instance, [x, y]);
}
/*jazor:clr-member static System.Collections.Generic.Comparer<T>.Default.get*/
export function getDefault() {
  if (DefaultInstance === null) {
    let instance = Object.create(null);
    Reflect.set(instance, "compare", CompareCore);
    DefaultInstance = instance;
  }
  return DefaultInstance;
}
/*jazor:clr-member virtual System.Collections.Generic.Comparer<T>.Compare(T, T)*/
export function _a4222c99b516b861(instance, x, y) {
  return CompareInstance(instance, x, y);
}
