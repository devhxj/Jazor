import { TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function TryParseUInt16Core(s, value) {
  value = 0;
  if (s === null)
    return [false, value];
  let trimmed = s.trim();
  if (trimmed.length === 0)
    return [false, value];
  let start = 0;
  let first = _5ad63706a889c294(trimmed, 0);
  if (first.charCodeAt(0) === "+".charCodeAt(0) || first.charCodeAt(0) === "-".charCodeAt(0)) {
    if (trimmed.length === 1)
      return [false, value];
    start = 1;
  }
  for (let i = start; i < trimmed.length; i++) {
    let ch = _5ad63706a889c294(trimmed, i);
    if (ch.charCodeAt(0) < "0".charCodeAt(0) || ch.charCodeAt(0) > "9".charCodeAt(0))
      return [false, value];
  }
  let parsed = Number(trimmed);
  if (isNaN(parsed) || Math.floor(parsed) !== parsed)
    return [false, value];
  if (parsed < 0 || parsed > 65535)
    return [false, value];
  value = parsed;
  return [true, value];
}
function IsOverflowUInt16Text(s) {
  if (s === null)
    return false;
  let trimmed = s.trim();
  if (trimmed.length === 0)
    return false;
  let start = 0;
  let first = _5ad63706a889c294(trimmed, 0);
  if (first.charCodeAt(0) === "+".charCodeAt(0) || first.charCodeAt(0) === "-".charCodeAt(0)) {
    if (trimmed.length === 1)
      return false;
    start = 1;
  }
  for (let i = start; i < trimmed.length; i++) {
    let ch = _5ad63706a889c294(trimmed, i);
    if (ch.charCodeAt(0) < "0".charCodeAt(0) || ch.charCodeAt(0) > "9".charCodeAt(0))
      return false;
  }
  let parsed = Number(trimmed);
  if (isNaN(parsed) || Math.floor(parsed) !== parsed)
    return false;
  return parsed < 0 || parsed > 65535;
}
/*jazor:clr-member ushort.CompareTo(object)*/
export function _d8d8b9cba9bd3347(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type UInt16.");
  let number = value;
  return instance < number ? -1 : instance > number ? 1 : 0;
}
/*jazor:clr-member static ushort.Parse(string)*/
export function _bfae72f49db4f3c9(s) {
  let value, __ref$96debf9897d8ca53720e7bef;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$96debf9897d8ca53720e7bef = TryParseUInt16Core(s, undefined), value = __ref$96debf9897d8ca53720e7bef[1], __ref$96debf9897d8ca53720e7bef[0])) {
    if (IsOverflowUInt16Text(s))
      throw new Error("OverflowException: Value was either too large or too small for a UInt16.");
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid UInt16.`);
  }
  return value;
}
/*jazor:clr-member static ushort.TryParse(string, out ushort)*/
export function _2efd27d401f7def7(s, result) {
  let value, __ref$3bacacb581f9fbcc556b4ddf;
  if (!(__ref$3bacacb581f9fbcc556b4ddf = TryParseUInt16Core(s, undefined), value = __ref$3bacacb581f9fbcc556b4ddf[1], __ref$3bacacb581f9fbcc556b4ddf[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static ushort.TryParse(System.ReadOnlySpan<char>, out ushort)*/
export function _0103a8bec9e9dfd7(s, result) {
  return _2efd27d401f7def7(s, result);
}
/*jazor:clr-member static ushort.TryParse(System.ReadOnlySpan<byte>, out ushort)*/
export function _f90ee83a31a4d447(utf8Text, result) {
  return _2efd27d401f7def7(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static ushort.DivRem(ushort, ushort)*/
export function _80e78c0aa0b98fef(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.floor(left / right);
  let remainder = left % right;
  return { Quotient: quotient, Remainder: remainder };
}
/*jazor:clr-member static ushort.PopCount(ushort)*/
export function _2ea0cab4f3f489d9(value) {
  let v = value;
  v = v - (v >> 1 & 21845);
  v = (v & 13107) + (v >> 2 & 13107);
  v = v + (v >> 4) & 3855;
  v = v + (v >> 8);
  return v & 31;
}
