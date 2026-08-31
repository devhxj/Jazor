import { AddCore_c1d44e5d9916d4b9, Create_2c5622046787c7f9 } from "System/Collections/Generic/HashSetT1Module.js";
import { CreateReadOnlyArrayView, MarkAsReadOnlySetCarrier } from "System/RuntimeModule.js";
function EnsureWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a whole number.`);
}
function EnsureSource(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
function EnsureTarget(array) {
  if (array == null)
    throw new Error("ArgumentNullException: array is null");
}
function EnsureTargetIndex(array, arrayIndex) {
  EnsureWholeNumber(arrayIndex, "arrayIndex");
  if (arrayIndex < 0 || arrayIndex > array.length)
    throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
}
function EnsureCopyCapacity(array, arrayIndex, copyCount) {
  if (arrayIndex + copyCount > array.length)
    throw new Error("ArgumentException: Not enough space in destination array.");
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)*/
export function _d4e5f6a7b8c9d0e1(list) {
  return CreateReadOnlyArrayView(list, "ArgumentNullException: list is null.");
}
/*jazor:clr-member static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get*/
export function _e5f6a7b8c9d0e1f2() {
  return Object.freeze(new Array);
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get*/
export function _b8c9d0e1f2a3b4c5(instance, index) {
  EnsureSource(instance);
  EnsureWholeNumber(index, "index");
  if (index < 0 || index >= instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  return instance[index];
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])*/
export function _d0e1f2a3b4c5d6e7(instance, array) {
  EnsureSource(instance);
  EnsureTarget(array);
  EnsureCopyCapacity(array, 0, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[i] = instance[i];
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)*/
export function _e1f2a3b4c5d6e7f8(instance, array, arrayIndex) {
  EnsureSource(instance);
  EnsureTarget(array);
  EnsureTargetIndex(array, arrayIndex);
  EnsureCopyCapacity(array, arrayIndex, instance.length);
  for (let i = 0; i < instance.length; i++)
    array[arrayIndex + i] = instance[i];
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)*/
export function _f2a3b4c5d6e7f8a9(instance, index, array, arrayIndex, count) {
  EnsureSource(instance);
  EnsureTarget(array);
  EnsureWholeNumber(index, "index");
  EnsureWholeNumber(count, "count");
  EnsureTargetIndex(array, arrayIndex);
  if (index < 0 || index > instance.length)
    throw new Error("ArgumentOutOfRangeException: index is out of range.");
  if (count < 0)
    throw new Error("ArgumentOutOfRangeException: count is out of range.");
  if (index + count > instance.length)
    throw new Error("ArgumentException: source index and count are out of range.");
  EnsureCopyCapacity(array, arrayIndex, count);
  for (let i = 0; i < count; i++)
    array[arrayIndex + i] = instance[index + i];
}
/*jazor:clr-member static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)*/
export function _a0cccd63a3a3eee1(values) {
  return Object.freeze(values.slice());
}
/*jazor:clr-member static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)*/
export function _b80678a096dde585(values) {
  let result = Create_2c5622046787c7f9(null);
  for (let index = 0; index < values.length; index++)
    AddCore_c1d44e5d9916d4b9(result, values[index]);
  return MarkAsReadOnlySetCarrier(result);
}
