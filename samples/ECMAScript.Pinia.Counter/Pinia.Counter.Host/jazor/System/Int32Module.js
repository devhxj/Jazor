import { _5ad63706a889c294 } from "System/StringModule.js";
function compareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function tryParseInt32Core(s, value) {
  value = 0;
  if (s === null)
    return [false, value];
  let trimmed = s.trim();
  if (trimmed.length === 0)
    return [false, value];
  let start = 0;
  let first = _5ad63706a889c294(trimmed, 0);
  if (first === "+" || first === "-") {
    if (trimmed.length === 1)
      return [false, value];
    start = 1;
  }
  for (let i = start; i < trimmed.length; i++) {
    let ch = _5ad63706a889c294(trimmed, i);
    if (ch < "0" || ch > "9")
      return [false, value];
  }
  let parsed = Number(trimmed);
  if (isNaN(parsed) || Math.floor(parsed) !== parsed)
    return [false, value];
  if (parsed < -2147483648 || parsed > 2147483647)
    return [false, value];
  value = parsed;
  return [true, value];
}
export function _b03337a2a71c762d(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Int32.");
  return compareCore(instance, value);
}
export function _151ccc6045162f8f(s) {
  let value, __ref$43560db76b1df673fcb7d2bf;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$43560db76b1df673fcb7d2bf = tryParseInt32Core(s, value), value = __ref$43560db76b1df673fcb7d2bf[1], __ref$43560db76b1df673fcb7d2bf[0]))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid Int32.`);
  return value;
}
export function _16e2a901535b765e(s, result) {
  let value, __ref$a561a4aa2f8c7241897bc3ac;
  if (!(__ref$a561a4aa2f8c7241897bc3ac = tryParseInt32Core(s, value), value = __ref$a561a4aa2f8c7241897bc3ac[1], __ref$a561a4aa2f8c7241897bc3ac[0]))
    return [false, 0];
  return [true, value];
}
export function _d4cc9914e60e5643(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.trunc(left / right);
  let remainder = left - quotient * right;
  return { quotient: quotient, remainder: remainder };
}
export function _e04660fe6cb92bf1(value) {
  let v = value;
  v = v - (v >> 1 & 1431655765);
  v = (v & 858993459) + (v >> 2 & 858993459);
  v = v + (v >> 4) & 252645135;
  v = v + (v >> 8);
  v = v + (v >> 16);
  return v & 63;
}
export function _a36b4a6dbd50fa77(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return compareCore(x, y) >= 0 ? x : y;
}
export function _d0c6a74fd11d24bf(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return compareCore(x, y) <= 0 ? x : y;
}
