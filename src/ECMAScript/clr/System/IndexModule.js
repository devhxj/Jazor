import { JIndex } from "System/RuntimeModule.js";
/*jazor:clr-member System.Index.Index()*/
export function _94a150c0b38bdd79() {
  return new JIndex(0, false);
}
/*jazor:clr-member System.Index.Index(int, bool)*/
export function _f406c4c734b11d38(value, fromEnd) {
  return new JIndex(value, fromEnd);
}
/*jazor:clr-member static System.Index.Start.get*/
export function _c6ec2b575aff2e24() {
  return new JIndex(0, false);
}
/*jazor:clr-member static System.Index.End.get*/
export function _0ba7c760bb17a58f() {
  return new JIndex(0, true);
}
/*jazor:clr-member static System.Index.FromStart(int)*/
export function _1b0e1c2ab6c4cd39(value) {
  return new JIndex(value, false);
}
/*jazor:clr-member static System.Index.FromEnd(int)*/
export function _ce8b9229a41c8545(value) {
  return new JIndex(value, true);
}
/*jazor:clr-member System.Index.Value.get*/
export function _71953783d6b61ae1(instance) {
  return instance.value;
}
/*jazor:clr-member System.Index.IsFromEnd.get*/
export function _b141712b3756cf57(instance) {
  return instance.fromEnd;
}
/*jazor:clr-member System.Index.GetOffset(int)*/
export function _9b817e75f3f8f58f(instance, length) {
  return instance.GetOffset(length);
}
function GetHashCodeCore(instance) {
  return instance.value * 2 + (instance.fromEnd ? 1 : 0);
}
/*jazor:clr-member override System.Index.Equals(object)*/
export function _2910b3afb47ad8b1(instance, value) {
  let other = value instanceof JIndex ? value : null;
  return other !== null && instance.value === other.value && instance.fromEnd === other.fromEnd;
}
/*jazor:clr-member System.Index.Equals(System.Index)*/
export function _83db7aa629254762(instance, other) {
  return instance.value === other.value && instance.fromEnd === other.fromEnd;
}
/*jazor:clr-member override System.Index.GetHashCode()*/
export function _1c7f7405a620c971(instance) {
  return GetHashCodeCore(instance);
}
/*jazor:clr-member static System.Index.implicit operator System.Index(int)*/
export function _1e1b56e4e760a5d5(value) {
  return new JIndex(value, false);
}
/*jazor:clr-member override System.Index.ToString()*/
export function _0fb768c390456f95(instance) {
  return instance.fromEnd ? "^" + instance.value : "" + instance.value;
}
