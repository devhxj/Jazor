import { TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function CompareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
function TryParseSByteCore(s, value) {
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
  if (parsed < -128 || parsed > 127)
    return [false, value];
  value = parsed;
  return [true, value];
}
/*jazor:clr-member sbyte.CompareTo(object)*/
export function _f8a387725694962f(instance, obj) {
  if (obj === null)
    return 1;
  if (typeof obj !== "number")
    throw new Error("ArgumentException: Object must be of type SByte.");
  return CompareCore(instance, obj);
}
/*jazor:clr-member static sbyte.Parse(string)*/
export function _fc6fdbb937cb390a(s) {
  let value, __ref$bf72be6abef644ac5ea29081;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$bf72be6abef644ac5ea29081 = TryParseSByteCore(s, undefined), value = __ref$bf72be6abef644ac5ea29081[1], __ref$bf72be6abef644ac5ea29081[0]))
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid SByte.`);
  return value;
}
/*jazor:clr-member static sbyte.TryParse(string, out sbyte)*/
export function _d9082c2537283f95(s, result) {
  let value, __ref$d249fdaeb3703acec0089f3e;
  if (!(__ref$d249fdaeb3703acec0089f3e = TryParseSByteCore(s, undefined), value = __ref$d249fdaeb3703acec0089f3e[1], __ref$d249fdaeb3703acec0089f3e[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static sbyte.TryParse(System.ReadOnlySpan<char>, out sbyte)*/
export function _a3ccaa03549862bc(s, result) {
  return _d9082c2537283f95(s, result);
}
/*jazor:clr-member static sbyte.TryParse(System.ReadOnlySpan<byte>, out sbyte)*/
export function _f25602df99a7ca89(utf8Text, result) {
  return _d9082c2537283f95(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static sbyte.Abs(sbyte)*/
export function _f0d5d38874458f27(value) {
  if (value === -128)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return Math.abs(value);
}
