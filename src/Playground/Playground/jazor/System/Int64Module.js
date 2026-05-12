function get_RotateBitWidth() {
  return BigInt(64);
}
function get_RotateMask() {
  return BigInt("18446744073709551615");
}
function get_RotateModulus() {
  return BigInt("18446744073709551616");
}
function get_RotateSignBit() {
  return BigInt("9223372036854775808");
}
function normalizeRotateBits(value) {
  return value & get_RotateMask();
}
function normalizeSignedRotateResult(value) {
  let masked = normalizeRotateBits(value);
  return masked >= get_RotateSignBit() ? masked - get_RotateModulus() : masked;
}
export function _a108636b79b7c8d2(instance, value) {
  let bigIntValue;
  if (value === null)
    return 1;
  if (typeof value === "bigint" && (bigIntValue = value, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type Int64.");
}
export function _4174bb5b72e448a6(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let trimmed = s.trim();
  try {
    let result = BigInt(trimmed);
    let minValue = BigInt("-9223372036854775808");
    let maxValue = BigInt("9223372036854775807");
    if (result < minValue || result > maxValue)
      throw new Error(`OverflowException: Value '${s}' was either too large or too small for an Int64.`);
    return result;
  } catch {
    throw new Error(`FormatException: String '${s}' was not recognized as a valid Int64.`);
  }
}
export function _2cba636c245c1675(s, result) {
  if (s === null)
    return [false, 0n];
  let trimmed = s.trim();
  try {
    let parsed = BigInt(trimmed);
    let minValue = BigInt("-9223372036854775808");
    let maxValue = BigInt("9223372036854775807");
    if (parsed < minValue || parsed > maxValue)
      return [false, 0n];
    return [true, parsed];
  } catch {
    return [false, 0n];
  }
}
export function _28273cd350760efe(left, right) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  let quotient = left / right;
  let remainder = left % right;
  return { quotient: quotient, remainder: remainder };
}
export function _77fd605bbb6ce669(value) {
  let count = 0n;
  let v = value;
  while (v > 0n) {
    count = count + (v & 1n);
    v = v >> 1n;
  }
  return count;
}
export function _62ef461b6a515b85(value, rotateAmount) {
  let amount = rotateAmount % 64;
  if (amount < 0)
    amount += 64;
  let shift = BigInt(amount);
  let normalized = normalizeRotateBits(value);
  if (shift === 0)
    return normalizeSignedRotateResult(normalized);
  let rotated = (normalized << shift | normalized >> get_RotateBitWidth() - shift) & get_RotateMask();
  return normalizeSignedRotateResult(rotated);
}
export function _6a70bc88f689ce73(value, rotateAmount) {
  let amount = rotateAmount % 64;
  if (amount < 0)
    amount += 64;
  let shift = BigInt(amount);
  let normalized = normalizeRotateBits(value);
  if (shift === 0)
    return normalizeSignedRotateResult(normalized);
  let rotated = (normalized >> shift | normalized << get_RotateBitWidth() - shift) & get_RotateMask();
  return normalizeSignedRotateResult(rotated);
}
export function _df6d7288bc845b53(value) {
  if (value === 0n)
    return BigInt(64);
  let count = 0n;
  let v = value;
  while ((v & 1n) === 0n) {
    v = v >> 1n;
    count = count + 1n;
  }
  return count;
}
export function _9618dc0d855ee729(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
export function _bfad1ee52075b36e(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return x < y ? x : y;
}
