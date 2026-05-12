import { _5ad63706a889c294 } from "System/StringModule.js";
function compareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function tryParseByteCore(s, value) {
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
  if (parsed < 0 || parsed > 255)
    return [false, value];
  value = parsed;
  return [true, value];
}
function isOverflowByteText(s) {
  if (s === null)
    return false;
  let trimmed = s.trim();
  if (trimmed.length === 0)
    return false;
  let start = 0;
  let first = _5ad63706a889c294(trimmed, 0);
  if (first === "+" || first === "-") {
    if (trimmed.length === 1)
      return false;
    start = 1;
  }
  for (let i = start; i < trimmed.length; i++) {
    let ch = _5ad63706a889c294(trimmed, i);
    if (ch < "0" || ch > "9")
      return false;
  }
  let parsed = Number(trimmed);
  if (isNaN(parsed) || Math.floor(parsed) !== parsed)
    return false;
  return parsed < 0 || parsed > 255;
}
export function _7aaf4c67dc6c9c9a(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Byte.");
  return compareCore(instance, value);
}
export function _8719e4b3055c5188(s) {
  let value, __ref$9a450ab644f843a519a71d1d;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$9a450ab644f843a519a71d1d = tryParseByteCore(s, value), value = __ref$9a450ab644f843a519a71d1d[1], __ref$9a450ab644f843a519a71d1d[0])) {
    if (isOverflowByteText(s))
      throw new Error("OverflowException: Value was either too large or too small for an unsigned byte.");
    throw new Error(`FormatException: String '${s}' was not recognized as a valid Byte.`);
  }
  return value;
}
export function _03c07d3f3ee012f9(s, result) {
  let value, __ref$dbd53fa148e2838366d8c262;
  if (!(__ref$dbd53fa148e2838366d8c262 = tryParseByteCore(s, value), value = __ref$dbd53fa148e2838366d8c262[1], __ref$dbd53fa148e2838366d8c262[0]))
    return [false, 0];
  return [true, value];
}
export function _42cbe2ef401fb8c9(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.floor(left / right);
  let remainder = left % right;
  return { quotient: quotient, remainder: remainder };
}
