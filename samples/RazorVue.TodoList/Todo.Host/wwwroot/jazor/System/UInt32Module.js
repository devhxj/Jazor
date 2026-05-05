import { _5ad63706a889c294 } from "System/StringModule.js";
function tryParseUInt32Core(s, value) {
  value = 0;
  if (s === null)
    return [false, value];
  let trimmed = s.trim();
  if (trimmed.length === 0)
    return [false, value];
  let start = 0;
  if (_5ad63706a889c294(trimmed, 0) === "+") {
    if (trimmed.length === 1)
      return [false, value];
    start = 1;
  }
  else if (_5ad63706a889c294(trimmed, 0) === "-")
    return [false, value];
  for (let i = start; i < trimmed.length; i++) {
    let ch = _5ad63706a889c294(trimmed, i);
    if (ch < "0" || ch > "9")
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
export function _75ff3ca18f13f709(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type UInt32.");
  let number = value;
  return instance < number ? -1 : instance > number ? 1 : 0;
}
export function _eb335b8243aba32a(s) {
  let value, __ref$54a0536247479dae1cafdf1d;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$54a0536247479dae1cafdf1d = tryParseUInt32Core(s, value), value = __ref$54a0536247479dae1cafdf1d[1], __ref$54a0536247479dae1cafdf1d[0]))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid UInt32.`);
  return value;
}
export function _ad4f3364f146e5da(s, result) {
  let value, __ref$9f402bdad939fc6d0d38c61e;
  if (!(__ref$9f402bdad939fc6d0d38c61e = tryParseUInt32Core(s, value), value = __ref$9f402bdad939fc6d0d38c61e[1], __ref$9f402bdad939fc6d0d38c61e[0]))
    return [false, 0];
  return [true, value];
}
export function _8a073d758132b5bb(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.floor(left / right);
  let remainder = left % right;
  return { quotient: quotient, remainder: remainder };
}
export function _96cd49e102b39e5b(value) {
  let v = value;
  v = v - (v >> 1 & 1431655765);
  v = (v & 858993459) + (v >> 2 & 858993459);
  v = v + (v >> 4) & 252645135;
  v = v + (v >> 8);
  v = v + (v >> 16);
  return v & 63;
}
export const UInt32Module = {
  tryParseUInt32Core,
  _75ff3ca18f13f709,
  _eb335b8243aba32a,
  _ad4f3364f146e5da,
  _8a073d758132b5bb,
  _96cd49e102b39e5b
};
