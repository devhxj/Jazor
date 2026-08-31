import { TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function CompareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function TryParseInt32Core(s, value, overflow) {
  value = 0;
  overflow = false;
  if (s === null)
    return [false, value, overflow];
  let trimmed = s.trim();
  if (trimmed.length === 0)
    return [false, value, overflow];
  let start = 0;
  let first = _5ad63706a889c294(trimmed, 0);
  if (first.charCodeAt(0) === "+".charCodeAt(0) || first.charCodeAt(0) === "-".charCodeAt(0)) {
    if (trimmed.length === 1)
      return [false, value, overflow];
    start = 1;
  }
  for (let i = start; i < trimmed.length; i++) {
    let ch = _5ad63706a889c294(trimmed, i);
    if (ch.charCodeAt(0) < "0".charCodeAt(0) || ch.charCodeAt(0) > "9".charCodeAt(0))
      return [false, value, overflow];
  }
  let parsed = Number(trimmed);
  if (isNaN(parsed) || Math.floor(parsed) !== parsed)
    return [false, value, overflow];
  if (parsed < -2147483648 || parsed > 2147483647) {
    overflow = true;
    return [false, value, overflow];
  }
  value = parsed === 0 ? 0 : parsed;
  return [true, value, overflow];
}
/*jazor:clr-member int.CompareTo(object)*/
export function _b03337a2a71c762d(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Int32.");
  return CompareCore(instance, value);
}
/*jazor:clr-member static int.Parse(string)*/
export function _151ccc6045162f8f(s) {
  let value, overflow, __ref$2df46e0ca9e10430b329f711;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$2df46e0ca9e10430b329f711 = TryParseInt32Core(s, undefined, undefined), value = __ref$2df46e0ca9e10430b329f711[1], overflow = __ref$2df46e0ca9e10430b329f711[2], __ref$2df46e0ca9e10430b329f711[0])) {
    if (overflow)
      throw new Error(`OverflowException: Value '${s ?? ""}' was either too large or too small for an Int32.`);
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid Int32.`);
  }
  return value;
}
/*jazor:clr-member static int.TryParse(string, out int)*/
export function _16e2a901535b765e(s, result) {
  let value, overflow, __ref$ee89d23c04a63c342a820560;
  if (!(__ref$ee89d23c04a63c342a820560 = TryParseInt32Core(s, undefined, undefined), value = __ref$ee89d23c04a63c342a820560[1], overflow = __ref$ee89d23c04a63c342a820560[2], __ref$ee89d23c04a63c342a820560[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static int.TryParse(System.ReadOnlySpan<char>, out int)*/
export function _f6a664534980b0f4(s, result) {
  return _16e2a901535b765e(s, result);
}
/*jazor:clr-member static int.TryParse(System.ReadOnlySpan<byte>, out int)*/
export function _2acff5418dba43bd(utf8Text, result) {
  return _16e2a901535b765e(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static int.DivRem(int, int)*/
export function _d4cc9914e60e5643(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  if (left === -2147483648 && right === -1)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  let quotient = Math.trunc(left / right);
  let remainder = left - quotient * right;
  return { Quotient: quotient, Remainder: remainder };
}
/*jazor:clr-member static int.PopCount(int)*/
export function _e04660fe6cb92bf1(value) {
  let v = value;
  v = v - (v >> 1 & 1431655765);
  v = (v & 858993459) + (v >> 2 & 858993459);
  v = v + (v >> 4) & 252645135;
  v = v + (v >> 8);
  v = v + (v >> 16);
  return v & 63;
}
/*jazor:clr-member static int.Abs(int)*/
export function _49bf8261f5cf3a4b(value) {
  if (value === -2147483648)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return Math.abs(value);
}
/*jazor:clr-member static int.MaxMagnitude(int, int)*/
export function _a36b4a6dbd50fa77(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return CompareCore(x, y) >= 0 ? x : y;
}
/*jazor:clr-member static int.MinMagnitude(int, int)*/
export function _d0c6a74fd11d24bf(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return CompareCore(x, y) <= 0 ? x : y;
}
