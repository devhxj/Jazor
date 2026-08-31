import { _0dc538197c677986, _562f832fd220e768, _a5e8c6b27df6470b, add } from "System/Collections/Generic/ListT1Module.js";
import { IsMutableListCarrier, RequireMutableListCarrier } from "System/RuntimeModule.js";
function EnsureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
}
function EnsureExistingIndex(instance, index) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
}
/*jazor:clr-member System.Collections.IList.this[int].get*/
export function _049fed3e1cad6543(instance, index) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return instance[index];
}
/*jazor:clr-member System.Collections.IList.this[int].set*/
export function _d1d1f177e5b9f8db(instance, index, value) {
  EnsureExistingIndex(instance, index);
  instance[index] = value;
}
/*jazor:clr-member System.Collections.IList.Add(object)*/
export function _436bcdacebfc9159(instance, value) {
  RequireMutableListCarrier(instance);
  let index = instance.length;
  add(instance, value);
  return index;
}
/*jazor:clr-member System.Collections.IList.Clear()*/
export function _00d8476a94b1a75c(instance) {
  RequireMutableListCarrier(instance);
  instance.splice(0, instance.length);
}
/*jazor:clr-member System.Collections.IList.IsReadOnly.get*/
export function _2ce407a9d9be8186(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  return !IsMutableListCarrier(instance);
}
/*jazor:clr-member System.Collections.IList.IsFixedSize.get*/
export function _b17a6c1583e0a5af(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  return !IsMutableListCarrier(instance);
}
/*jazor:clr-member System.Collections.IList.Insert(int, object)*/
export function _9e2711121aad1093(instance, index, value) {
  RequireMutableListCarrier(instance);
  _0dc538197c677986(instance, index, value);
}
/*jazor:clr-member System.Collections.IList.Remove(object)*/
export function _305c8313418aa043(instance, value) {
  RequireMutableListCarrier(instance);
  _562f832fd220e768(instance, value);
}
/*jazor:clr-member System.Collections.IList.RemoveAt(int)*/
export function _72d07d6eb16afece(instance, index) {
  RequireMutableListCarrier(instance);
  _a5e8c6b27df6470b(instance, index);
}
