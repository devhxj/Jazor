import { GetObjectHashCode } from "System/RuntimeModule.js";
let DefaultInstance = null;
export function EnsureComparerInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
export function EqualsCore(left, right) {
  if (Object.is(left, right))
    return true;
  if (typeof left === "number" && typeof right === "number")
    return left === right;
  return false;
}
export function GetHashCodeCore(value) {
  return GetObjectHashCode(value);
}
export function EqualsInstance(instance, x, y) {
  EnsureComparerInstance(instance);
  let equals = Reflect.get(instance, "equals");
  if (equals === null)
    throw new Error("MissingMethodException: comparer does not expose equals.");
  return Reflect.apply(equals, instance, [x, y]);
}
export function GetHashCodeInstance(instance, value) {
  EnsureComparerInstance(instance);
  let getHashCode = Reflect.get(instance, "getHashCode");
  if (getHashCode === null)
    throw new Error("MissingMethodException: comparer does not expose getHashCode.");
  return Reflect.apply(getHashCode, instance, [value]);
}
/*jazor:clr-member static System.Collections.Generic.EqualityComparer<T>.Default.get*/
export function getDefault() {
  if (DefaultInstance === null) {
    let instance = Object.create(null);
    Reflect.set(instance, "equals", EqualsCore);
    Reflect.set(instance, "getHashCode", GetHashCodeCore);
    DefaultInstance = instance;
  }
  return DefaultInstance;
}
/*jazor:clr-member virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)*/
export function _4614e5ce6b42a7ad(instance, x, y) {
  return EqualsInstance(instance, x, y);
}
/*jazor:clr-member virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)*/
export function _2c3736bd7d205921(instance, obj) {
  return GetHashCodeInstance(instance, obj);
}
