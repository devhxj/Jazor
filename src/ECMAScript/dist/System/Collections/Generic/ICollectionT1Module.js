import { _562f832fd220e768, add } from "System/Collections/Generic/ListT1Module.js";
import { IsMutableListCarrier, RequireMutableListCarrier } from "System/RuntimeModule.js";
function EnsureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
}
/*jazor:clr-member System.Collections.Generic.ICollection<T>.IsReadOnly.get*/
export function _1257c5832793c86d(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  return !IsMutableListCarrier(instance);
}
/*jazor:clr-member System.Collections.Generic.ICollection<T>.Add(T)*/
export function _c0023f4a7a67220a(instance, item) {
  RequireMutableListCarrier(instance);
  add(instance, item);
}
/*jazor:clr-member System.Collections.Generic.ICollection<T>.Clear()*/
export function _d067c092ac624f6a(instance) {
  RequireMutableListCarrier(instance);
  instance.splice(0, instance.length);
}
/*jazor:clr-member System.Collections.Generic.ICollection<T>.CopyTo(T[], int)*/
export function _03c4a0ae3554065f(instance, array, arrayIndex) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
  if (array == null)
    throw new Error("ArgumentNullException: array is null");
  EnsureWholeNumber(arrayIndex, "arrayIndex");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
  if (arrayIndex + instance.length > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
  for (let i = 0; i < instance.length; i++)
    array[arrayIndex + i] = instance[i];
}
/*jazor:clr-member System.Collections.Generic.ICollection<T>.Remove(T)*/
export function _0a859d3497130ea7(instance, item) {
  RequireMutableListCarrier(instance);
  return _562f832fd220e768(instance, item);
}
