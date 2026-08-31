import { JIndex, JRange } from "System/RuntimeModule.js";
function EqualsCore(left, right) {
  return left.value === right.value && left.fromEnd === right.fromEnd;
}
function GetIndexHashCode(instance) {
  return instance.value * 2 + (instance.fromEnd ? 1 : 0);
}
function GetIndexText(instance) {
  return instance.fromEnd ? "^" + instance.value : "" + instance.value;
}
/*jazor:clr-member System.Range.Range()*/
export function _d5659647559c2c27() {
  return new JRange(new JIndex(0, false), new JIndex(0, false));
}
/*jazor:clr-member System.Range.Start.get*/
export function _ff879b9ef9597efb(instance) {
  return instance.start;
}
/*jazor:clr-member System.Range.End.get*/
export function _0be235222ad447c5(instance) {
  return instance.end;
}
/*jazor:clr-member System.Range.Range(System.Index, System.Index)*/
export function _fc3dfc5dbaa397eb(start, end) {
  return new JRange(start, end);
}
/*jazor:clr-member override System.Range.Equals(object)*/
export function _31b6c9a4877f04c4(instance, value) {
  let other = value instanceof JRange ? value : null;
  return other !== null && EqualsCore(instance.start, other.start) && EqualsCore(instance.end, other.end);
}
/*jazor:clr-member System.Range.Equals(System.Range)*/
export function _f858c453f3829489(instance, other) {
  return EqualsCore(instance.start, other.start) && EqualsCore(instance.end, other.end);
}
/*jazor:clr-member override System.Range.GetHashCode()*/
export function _7fc0f3cc7ec542d3(instance) {
  return GetIndexHashCode(instance.start) * 397 ^ GetIndexHashCode(instance.end);
}
/*jazor:clr-member override System.Range.ToString()*/
export function _1c286146a6526629(instance) {
  return GetIndexText(instance.start) + ".." + GetIndexText(instance.end);
}
/*jazor:clr-member static System.Range.StartAt(System.Index)*/
export function _2cc8d1f98d9f4b16(start) {
  return new JRange(start, new JIndex(0, true));
}
/*jazor:clr-member static System.Range.EndAt(System.Index)*/
export function _1df4ded30f6797b5(end) {
  return new JRange(new JIndex(0, false), end);
}
/*jazor:clr-member static System.Range.All.get*/
export function _9fb8edf805e88967() {
  return new JRange(new JIndex(0, false), new JIndex(0, true));
}
/*jazor:clr-member System.Range.GetOffsetAndLength(int)*/
export function _1c7a1e658ed790ff(instance, length) {
  return instance.GetOffsetAndLength(length);
}
