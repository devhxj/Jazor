import { _5ad63706a889c294 } from "System/StringModule.js";
function tryParseUInt16Core(s, value) {
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
  if (parsed < 0 || parsed > 65535)
    return [false, value];
  value = parsed;
  return [true, value];
}
export function _d8d8b9cba9bd3347(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type UInt16.");
  let number = value;
  return instance < number ? -1 : instance > number ? 1 : 0;
}
export function _bfae72f49db4f3c9(s) {
  let value, __ref$03ff449dcbd03e46f22066c4;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$03ff449dcbd03e46f22066c4 = tryParseUInt16Core(s, value), value = __ref$03ff449dcbd03e46f22066c4[1], __ref$03ff449dcbd03e46f22066c4[0]))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid UInt16.`);
  return value;
}
export function _2efd27d401f7def7(s, result) {
  let value, __ref$61ea39736919f68af91c532b;
  if (!(__ref$61ea39736919f68af91c532b = tryParseUInt16Core(s, value), value = __ref$61ea39736919f68af91c532b[1], __ref$61ea39736919f68af91c532b[0]))
    return [false, 0];
  return [true, value];
}
export function _80e78c0aa0b98fef(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  let quotient = Math.floor(left / right);
  let remainder = left % right;
  return { quotient: quotient, remainder: remainder };
}
export function _2ea0cab4f3f489d9(value) {
  let v = value;
  v = v - (v >> 1 & 21845);
  v = (v & 13107) + (v >> 2 & 13107);
  v = v + (v >> 4) & 3855;
  v = v + (v >> 8);
  return v & 31;
}
export const UInt16Module = {
  tryParseUInt16Core,
  _d8d8b9cba9bd3347,
  _bfae72f49db4f3c9,
  _2efd27d401f7def7,
  _80e78c0aa0b98fef,
  _2ea0cab4f3f489d9
};
