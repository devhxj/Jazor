import { _5ad63706a889c294 } from "System/StringModule.js";
function compareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function tryParseSByteCore(s, value) {
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
  if (parsed < -128 || parsed > 127)
    return [false, value];
  value = parsed;
  return [true, value];
}
export function _f8a387725694962f(instance, obj) {
  if (obj === null)
    return 1;
  if (typeof obj !== "number")
    throw new Error("ArgumentException: Object must be of type SByte.");
  return compareCore(instance, obj);
}
export function _fc6fdbb937cb390a(s) {
  let value, __ref$29cfdb74a7472202291ff457;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$29cfdb74a7472202291ff457 = tryParseSByteCore(s, value), value = __ref$29cfdb74a7472202291ff457[1], __ref$29cfdb74a7472202291ff457[0]))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid SByte.`);
  return value;
}
export function _d9082c2537283f95(s, result) {
  let value, __ref$f45130ddee7767eec1b0fba7;
  if (!(__ref$f45130ddee7767eec1b0fba7 = tryParseSByteCore(s, value), value = __ref$f45130ddee7767eec1b0fba7[1], __ref$f45130ddee7767eec1b0fba7[0]))
    return [false, 0];
  return [true, value];
}
export const SByteModule = {
  compareCore,
  tryParseSByteCore,
  _f8a387725694962f,
  _fc6fdbb937cb390a,
  _d9082c2537283f95
};
