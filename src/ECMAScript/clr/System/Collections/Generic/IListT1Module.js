import { _0dc538197c677986, _a5e8c6b27df6470b } from "System/Collections/Generic/ListT1Module.js";
import { RequireMutableListCarrier } from "System/RuntimeModule.js";
function EnsureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
}
/*jazor:clr-member System.Collections.Generic.IList<T>.this[int].get*/
export function _8b52bea1dfb9f9ba(instance, index) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return instance[index];
}
/*jazor:clr-member System.Collections.Generic.IList<T>.this[int].set*/
export function _72c3ada14c4b312e(instance, index, value) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  instance[index] = value;
}
/*jazor:clr-member System.Collections.Generic.IList<T>.Insert(int, T)*/
export function _ad668b5fd142c4f4(instance, index, item) {
  RequireMutableListCarrier(instance);
  _0dc538197c677986(instance, index, item);
}
/*jazor:clr-member System.Collections.Generic.IList<T>.RemoveAt(int)*/
export function _d5f628d4cac6dafb(instance, index) {
  RequireMutableListCarrier(instance);
  _a5e8c6b27df6470b(instance, index);
}
