import { _5ad63706a889c294 } from "System/StringModule.js";
function compareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function tryParseInt16Core(s, value) {
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
  if (parsed < -32768 || parsed > 32767)
    return [false, value];
  value = parsed;
  return [true, value];
}
function maxMagnitudeCore(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return compareCore(x, y) >= 0 ? x : y;
}
function minMagnitudeCore(x, y) {
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return compareCore(x, y) <= 0 ? x : y;
}
export function _16417ddcfd71e8e5(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Int16.");
  return compareCore(instance, value);
}
export function _8a975b9eda8ac957(s) {
  let value, __ref$9dae204b3a531ddfc3e70f76;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$9dae204b3a531ddfc3e70f76 = tryParseInt16Core(s, value), value = __ref$9dae204b3a531ddfc3e70f76[1], __ref$9dae204b3a531ddfc3e70f76[0]))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid Int16.`);
  return value;
}
export function _65bc2566851a5ef7(s, result) {
  let value, __ref$6a98130726a3ac651c9cc188;
  if (!(__ref$6a98130726a3ac651c9cc188 = tryParseInt16Core(s, value), value = __ref$6a98130726a3ac651c9cc188[1], __ref$6a98130726a3ac651c9cc188[0]))
    return [false, 0];
  return [true, value];
}
export function _b2c1f15fae072110(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.trunc(left / right);
  let remainder = left - quotient * right;
  return { quotient: quotient, remainder: remainder };
}
export function _1636c956519f95fa(value) {
  let v = value;
  v = v - (v >> 1 & 21845);
  v = (v & 13107) + (v >> 2 & 13107);
  v = v + (v >> 4) & 3855;
  v = v + (v >> 8);
  return v & 31;
}
export function _ea75510d32bc8099(x, y) {
  return maxMagnitudeCore(x, y);
}
export function _63d3d54252a49e29(x, y) {
  return minMagnitudeCore(x, y);
}
