import { TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function CompareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function TryParseInt16Core(s, value) {
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
  if (parsed < -32768 || parsed > 32767)
    return [false, value];
  value = parsed;
  return [true, value];
}
function MaxMagnitudeCore(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return CompareCore(x, y) >= 0 ? x : y;
}
function MinMagnitudeCore(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return CompareCore(x, y) <= 0 ? x : y;
}
/*jazor:clr-member short.CompareTo(object)*/
export function _16417ddcfd71e8e5(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Int16.");
  return CompareCore(instance, value);
}
/*jazor:clr-member static short.Parse(string)*/
export function _8a975b9eda8ac957(s) {
  let value, __ref$d2b3fb179ee50df6ddd7dd8b;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$d2b3fb179ee50df6ddd7dd8b = TryParseInt16Core(s, undefined), value = __ref$d2b3fb179ee50df6ddd7dd8b[1], __ref$d2b3fb179ee50df6ddd7dd8b[0]))
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid Int16.`);
  return value;
}
/*jazor:clr-member static short.TryParse(string, out short)*/
export function _65bc2566851a5ef7(s, result) {
  let value, __ref$bed872104e7303c71cef6e93;
  if (!(__ref$bed872104e7303c71cef6e93 = TryParseInt16Core(s, undefined), value = __ref$bed872104e7303c71cef6e93[1], __ref$bed872104e7303c71cef6e93[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static short.TryParse(System.ReadOnlySpan<char>, out short)*/
export function _f06bf367c8a26691(s, result) {
  return _65bc2566851a5ef7(s, result);
}
/*jazor:clr-member static short.TryParse(System.ReadOnlySpan<byte>, out short)*/
export function _af732a8ac69b6f6e(utf8Text, result) {
  return _65bc2566851a5ef7(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static short.DivRem(short, short)*/
export function _b2c1f15fae072110(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  if (left === -32768 && right === -1)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  let quotient = Math.trunc(left / right);
  let remainder = left - quotient * right;
  return { Quotient: quotient, Remainder: remainder };
}
/*jazor:clr-member static short.PopCount(short)*/
export function _1636c956519f95fa(value) {
  let v = value;
  v = v - (v >> 1 & 21845);
  v = (v & 13107) + (v >> 2 & 13107);
  v = v + (v >> 4) & 3855;
  v = v + (v >> 8);
  return v & 31;
}
/*jazor:clr-member static short.Abs(short)*/
export function _8ce36b36c4abd947(value) {
  if (value === -32768)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return Math.abs(value);
}
/*jazor:clr-member static short.MaxMagnitude(short, short)*/
export function _ea75510d32bc8099(x, y) {
  return MaxMagnitudeCore(x, y);
}
/*jazor:clr-member static short.MinMagnitude(short, short)*/
export function _63d3d54252a49e29(x, y) {
  return MinMagnitudeCore(x, y);
}
