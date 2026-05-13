import { _5ad63706a889c294 } from "System/StringModule.js";
function compareCore(left, right) {
  let leftChar = _5ad63706a889c294(left, 0);
  let rightChar = _5ad63706a889c294(right, 0);
  return leftChar < rightChar ? -1 : leftChar > rightChar ? 1 : 0;
}
function getCodeUnit(value) {
  return value.charCodeAt(0);
}
function getCodeUnitFromChar(value) {
  return value.charCodeAt(0);
}
function isControlCode(code) {
  return code < 32 || code >= 127 && code <= 159;
}
function isWhiteSpaceCode(code) {
  return code >= 9 && code <= 13 || code === 32 || code === 133 || code === 160 || code === 5760 || code >= 8192 && code <= 8202 || code === 8232 || code === 8233 || code === 8239 || code === 8287 || code === 12288;
}
export function _ddf9c5affdc041df(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "string")
    throw new Error("ArgumentException: Object must be of type Char.");
  return compareCore(instance, value);
}
export function _d89999df761a6d2e(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (s.length !== 1)
    throw new Error("FormatException: String must be exactly one character long.");
  return s.substring(0, 0 + 1);
}
export function _9450f84427428db0(s, result) {
  if (s !== null && s.length === 1)
    return [true, s.substring(0, 0 + 1)];
  return [false, "\0"];
}
export function _16e351e6f7b127f7(c) {
  return isWhiteSpaceCode(getCodeUnit(c));
}
export function _c12d0a40e2ed8650(c) {
  return isControlCode(getCodeUnit(c));
}
export function _68e189abbb5497dc(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  return isControlCode(c);
}
export function _52eb020022da112b(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= "0" && c <= "9";
}
export function _e7ee64c732d21cd5(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= "A" && c <= "Z" || c >= "a" && c <= "z";
}
export function _d752ce4eaadf7612(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= "A" && c <= "Z" || c >= "a" && c <= "z" || c >= "0" && c <= "9";
}
export function _6ebe08db86ea37a2(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= "a" && c <= "z";
}
export function _bca1b50c85e48723(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  return c >= 55296 && c <= 57343;
}
export function _1ae24de44f4b499e(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = _5ad63706a889c294(s, index);
  return c >= "A" && c <= "Z";
}
export function _a21dd6de62be7b75(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  return isWhiteSpaceCode(getCodeUnitFromChar(_5ad63706a889c294(s, index)));
}
export function _d86c1e9964250116(c) {
  let code = getCodeUnit(c);
  if (code >= 48 && code <= 57)
    return code - 48;
  return -1;
}
export function _938251f1b1fc7bc8(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  if (c >= 48 && c <= 57)
    return c - 48;
  return -1;
}
export function _311485d1745ce294(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  return c >= 55296 && c <= 56319;
}
export function _1d56cdc9a261e948(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  return c >= 56320 && c <= 57343;
}
export function _27c9fca9c829cc5e(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length - 1)
    return false;
  let c1 = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  let c2 = getCodeUnitFromChar(_5ad63706a889c294(s, index + 1));
  return c1 >= 55296 && c1 <= 56319 && (c2 >= 56320 && c2 <= 57343);
}
export function _d9f7c3c03ea64580(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = getCodeUnitFromChar(_5ad63706a889c294(s, index));
  if (c >= 55296 && c <= 56319) {
    if (index + 1 >= s.length)
      throw new Error("ArgumentException: Missing low surrogate");
    let low = getCodeUnitFromChar(_5ad63706a889c294(s, index + 1));
    if (low < 56320 || low > 57343)
      throw new Error("ArgumentException: Invalid low surrogate");
    return (c - 55296 << 10) + (low - 56320) + 65536;
  }
  return c;
}
