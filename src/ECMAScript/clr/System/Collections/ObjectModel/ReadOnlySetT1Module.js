import { Create_2c5622046787c7f9, IsProperSubsetOfCore, IsProperSupersetOfCore, IsSubsetOfCore, IsSupersetOfCore, OverlapsCore, SetEqualsCore } from "System/Collections/Generic/HashSetT1Module.js";
import { MarkAsReadOnlySetCarrier } from "System/RuntimeModule.js";
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)*/
export function _aede400efbd05842(set) {
  if (set === null)
    throw new Error("ArgumentNullException: set is null.");
  return MarkAsReadOnlySetCarrier(set);
}
/*jazor:clr-member static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get*/
export function _843cd8664672a9f8() {
  return MarkAsReadOnlySetCarrier(Create_2c5622046787c7f9(null));
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _8745918ab865b9f0(instance, other) {
  return IsProperSubsetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _ab53c8c15a545026(instance, other) {
  return IsProperSupersetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _f72f25db872c4c11(instance, other) {
  return IsSubsetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _e7d6617cc0e3119e(instance, other) {
  return IsSupersetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)*/
export function _520d7f31ddf30fea(instance, other) {
  return OverlapsCore(instance, other);
}
/*jazor:clr-member System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)*/
export function _eb16d835e6822ba0(instance, other) {
  return SetEqualsCore(instance, other);
}
