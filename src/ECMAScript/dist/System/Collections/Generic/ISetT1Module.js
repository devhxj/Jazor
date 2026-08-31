import { ExceptWithCore, IntersectWithCore, IsProperSubsetOfCore, IsProperSupersetOfCore, IsSubsetOfCore, IsSupersetOfCore, OverlapsCore, SetEqualsCore, SymmetricExceptWithCore, UnionWithCore, _e1d2ba750a2788cb } from "System/Collections/Generic/HashSetT1Module.js";
import { IsReadOnlySetCarrier } from "System/RuntimeModule.js";
function EnsureWritable(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
  if (IsReadOnlySetCarrier(instance))
    throw new Error("NotSupportedException: Collection is read-only.");
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.Add(T)*/
export function _fa512a510bd763de(instance, item) {
  EnsureWritable(instance);
  return _e1d2ba750a2788cb(instance, item);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)*/
export function _d9af20d6b8c5e775(instance, other) {
  EnsureWritable(instance);
  UnionWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)*/
export function _202b815f92a32e5d(instance, other) {
  EnsureWritable(instance);
  IntersectWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)*/
export function _ac98ad1e0ac9efb5(instance, other) {
  EnsureWritable(instance);
  ExceptWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)*/
export function _07907f6b669e590a(instance, other) {
  EnsureWritable(instance);
  SymmetricExceptWithCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _bcd9e5c5cd4a65e1(instance, other) {
  return IsSubsetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _a64ad5f437ed3887(instance, other) {
  return IsSupersetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _f7d6687c6a479566(instance, other) {
  return IsProperSupersetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)*/
export function _bf1a417a69fffcb2(instance, other) {
  return IsProperSubsetOfCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)*/
export function _45e2e920f151fad2(instance, other) {
  return OverlapsCore(instance, other);
}
/*jazor:clr-member System.Collections.Generic.ISet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)*/
export function _afabf76c0df51242(instance, other) {
  return SetEqualsCore(instance, other);
}
