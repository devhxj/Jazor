import { _5ad63706a889c294 } from "System/StringModule.js";
function compareCore(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}
export function _ddf9c5affdc041df(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Char.");
  return compareCore(instance, value);
}
export function _d89999df761a6d2e(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (s.length !== 1)
    throw new Error("FormatException: String must be exactly one character long.");
  return _5ad63706a889c294(s, 0);
}
export function _9450f84427428db0(s, result) {
  if (s !== null && s.length === 1)
    return [true, _5ad63706a889c294(s, 0)];
  return [false, 0];
}
export function _68e189abbb5497dc(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c < 32 || c === 127;
}
export function _52eb020022da112b(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= 48 && c <= 57;
}
export function _6ebe08db86ea37a2(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= 97 && c <= 122;
}
export function _bca1b50c85e48723(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= 55296 && c <= 57343;
}
export function _1ae24de44f4b499e(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= 65 && c <= 90;
}
export function _a21dd6de62be7b75(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c === 32 || c === 9 || c === 10 || c === 13 || c === 12;
}
export function _d86c1e9964250116(c) {
  if (c >= 48 && c <= 57)
    return c - 48;
  return -1;
}
export function _938251f1b1fc7bc8(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  if (c >= 48 && c <= 57)
    return c - 48;
  return -1;
}
export function _311485d1745ce294(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= 55296 && c <= 56319;
}
export function _1d56cdc9a261e948(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= 56320 && c <= 57343;
}
export function _27c9fca9c829cc5e(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length - 1)
    return false;
  let c1 = _5ad63706a889c294(s, index);
  let c2 = _5ad63706a889c294(s, index + 1);
  return c1 >= 55296 && c1 <= 56319 && (c2 >= 56320 && c2 <= 57343);
}
export function _d9f7c3c03ea64580(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  if (c >= 55296 && c <= 56319) {
    if (index + 1 >= s.length)
      throw new Error("ArgumentException: Missing low surrogate");
    let low = _5ad63706a889c294(s, index + 1);
    if (low < 56320 || low > 57343)
      throw new Error("ArgumentException: Invalid low surrogate");
    return (c - 55296 << 10) + (low - 56320) + 65536;
  }
  return c;
}
export const CharModule = {
  compareCore,
  _ddf9c5affdc041df,
  _d89999df761a6d2e,
  _9450f84427428db0,
  _68e189abbb5497dc,
  _52eb020022da112b,
  _6ebe08db86ea37a2,
  _bca1b50c85e48723,
  _1ae24de44f4b499e,
  _a21dd6de62be7b75,
  _d86c1e9964250116,
  _938251f1b1fc7bc8,
  _311485d1745ce294,
  _1d56cdc9a261e948,
  _27c9fca9c829cc5e,
  _d9f7c3c03ea64580
};
