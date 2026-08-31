import { TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function TryParseUInt32Core(s, value) {
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
  if (parsed < 0 || parsed > 4294967295)
    return [false, value];
  value = parsed;
  return [true, value];
}
function IsOverflowUInt32Text(s) {
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
  return parsed < 0 || parsed > 4294967295;
}
/*jazor:clr-member uint.CompareTo(object)*/
export function _75ff3ca18f13f709(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type UInt32.");
  let number = value;
  return instance < number ? -1 : instance > number ? 1 : 0;
}
/*jazor:clr-member static uint.Parse(string)*/
export function _eb335b8243aba32a(s) {
  let value, __ref$d9b8072d8e1ed638347a84de;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$d9b8072d8e1ed638347a84de = TryParseUInt32Core(s, undefined), value = __ref$d9b8072d8e1ed638347a84de[1], __ref$d9b8072d8e1ed638347a84de[0])) {
    if (IsOverflowUInt32Text(s))
      throw new Error("OverflowException: Value was either too large or too small for a UInt32.");
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid UInt32.`);
  }
  return value;
}
/*jazor:clr-member static uint.TryParse(string, out uint)*/
export function _ad4f3364f146e5da(s, result) {
  let value, __ref$713e699c0a77dd8470ba0339;
  if (!(__ref$713e699c0a77dd8470ba0339 = TryParseUInt32Core(s, undefined), value = __ref$713e699c0a77dd8470ba0339[1], __ref$713e699c0a77dd8470ba0339[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static uint.TryParse(System.ReadOnlySpan<char>, out uint)*/
export function _104b334d48c2aecd(s, result) {
  return _ad4f3364f146e5da(s, result);
}
/*jazor:clr-member static uint.TryParse(System.ReadOnlySpan<byte>, out uint)*/
export function _2526f7e27fec4657(utf8Text, result) {
  return _ad4f3364f146e5da(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static uint.DivRem(uint, uint)*/
export function _8a073d758132b5bb(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.floor(left / right);
  let remainder = left % right;
  return { Quotient: quotient, Remainder: remainder };
}
/*jazor:clr-member static uint.PopCount(uint)*/
export function _96cd49e102b39e5b(value) {
  let v = value;
  v = v - (v >> 1 & 1431655765);
  v = (v & 858993459) + (v >> 2 & 858993459);
  v = v + (v >> 4) & 252645135;
  v = v + (v >> 8);
  v = v + (v >> 16);
  return v & 63;
}
