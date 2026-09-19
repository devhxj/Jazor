import { TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function CompareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function TryParseByteCore(s, value) {
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
  if (parsed < 0 || parsed > 255)
    return [false, value];
  value = parsed;
  return [true, value];
}
function IsOverflowByteText(s) {
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
  return parsed < 0 || parsed > 255;
}
/*jazor:clr-member byte.CompareTo(object)*/
export function _7aaf4c67dc6c9c9a(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Byte.");
  return CompareCore(instance, value);
}
/*jazor:clr-member static byte.Parse(string)*/
export function _8719e4b3055c5188(s) {
  let value, __ref$43aa0c8b75b68fa2071f5a09;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$43aa0c8b75b68fa2071f5a09 = TryParseByteCore(s, undefined), value = __ref$43aa0c8b75b68fa2071f5a09[1], __ref$43aa0c8b75b68fa2071f5a09[0])) {
    if (IsOverflowByteText(s))
      throw new Error("OverflowException: Value was either too large or too small for an unsigned byte.");
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid Byte.`);
  }
  return value;
}
/*jazor:clr-member static byte.TryParse(string, out byte)*/
export function _03c07d3f3ee012f9(s, result) {
  let value, __ref$63fe91ec8cd446b2e228d1e3;
  if (!(__ref$63fe91ec8cd446b2e228d1e3 = TryParseByteCore(s, undefined), value = __ref$63fe91ec8cd446b2e228d1e3[1], __ref$63fe91ec8cd446b2e228d1e3[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static byte.TryParse(System.ReadOnlySpan<char>, out byte)*/
export function _413c6f7752002edf(s, result) {
  return _03c07d3f3ee012f9(s, result);
}
/*jazor:clr-member static byte.TryParse(System.ReadOnlySpan<byte>, out byte)*/
export function _0e02bd74e5960e4d(utf8Text, result) {
  return _03c07d3f3ee012f9(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static byte.DivRem(byte, byte)*/
export function _42cbe2ef401fb8c9(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.floor(left / right);
  let remainder = left % right;
  return { Quotient: quotient, Remainder: remainder };
}
