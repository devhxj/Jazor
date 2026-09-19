import { _be8b149ea0e1d76b } from "System/DecimalModule.js";
import { IsFiniteCore } from "System/DoubleModule.js";
let DecimalIntegerPattern = new RegExp("^[+-]?\\d+$");
export function CompareToObject(instance, value, typeName) {
  if (value === null)
    return 1;
  if (typeof value !== "bigint")
    throw new Error(`ArgumentException: Object must be of type ${typeName ?? ""}.`);
  let other = value;
  return instance < other ? -1 : instance > other ? 1 : 0;
}
export function Parse(text, minValue, maxValue, typeName) {
  if (text === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let trimmed = text.trim();
  if (trimmed.length === 0 || !DecimalIntegerPattern.test(trimmed))
    throw new Error(`FormatException: String '${text ?? ""}' was not recognized as a valid ${typeName ?? ""}.`);
  let value = BigInt(trimmed);
  if (value < minValue || value > maxValue)
    throw new Error(`OverflowException: Value '${text ?? ""}' was either too large or too small for a ${typeName ?? ""}.`);
  return value;
}
export function TryParse(text, minValue, maxValue) {
  if (text === null)
    return [false, 0n];
  let trimmed = text.trim();
  if (trimmed.length === 0 || !DecimalIntegerPattern.test(trimmed))
    return [false, 0n];
  let value = BigInt(trimmed);
  if (value < minValue || value > maxValue)
    return [false, 0n];
  return [true, value];
}
export function EnsureRange(value, minValue, maxValue) {
  if (value < minValue || value > maxValue)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return value;
}
export function ToCheckedNumber(value, minValue, maxValue) {
  return Number(EnsureRange(value, minValue, maxValue));
}
export function FromFloatingChecked(value, minValue, maxValue) {
  if (!IsFiniteCore(value))
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return EnsureRange(BigInt(Math.trunc(value)), minValue, maxValue);
}
export function FromFloatingCheckedUInt128(value, maxValue) {
  if (value < 0)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return FromFloatingChecked(value, 0n, maxValue);
}
export function FromFloatingSaturatingSigned(value, minValue, maxValue) {
  if (isNaN(value))
    return 0n;
  if (!IsFiniteCore(value))
    return value < 0 ? minValue : maxValue;
  let integer = BigInt(Math.trunc(value));
  return integer < minValue ? minValue : integer > maxValue ? maxValue : integer;
}
export function FromFloatingSaturatingUnsigned(value, maxValue) {
  if (isNaN(value) || value <= 0)
    return 0n;
  if (!IsFiniteCore(value))
    return maxValue;
  let integer = BigInt(Math.trunc(value));
  return integer > maxValue ? maxValue : integer;
}
export function ToDecimal(value, minValue, maxValue) {
  return EnsureRange(value, minValue, maxValue).toString() ?? "";
}
export function FromDecimal(value, minValue, maxValue) {
  let integral = BigInt(_be8b149ea0e1d76b(value));
  return EnsureRange(integral, minValue, maxValue);
}
export function DivRemSigned(left, right, minValue) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  if (left === minValue && right === -1n)
    throw new Error("OverflowException");
  return { Quotient: left / right, Remainder: left % right };
}
export function DivRemUnsigned(left, right) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  return { Quotient: left / right, Remainder: left % right };
}
export function DivideSigned(left, right, minValue) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  if (left === minValue && right === -1n)
    throw new Error("OverflowException");
  return left / right;
}
export function DivideUnsigned(left, right) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  return left / right;
}
export function RemainderSigned(left, right, minValue) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  if (left === minValue && right === -1n)
    throw new Error("OverflowException");
  return left % right;
}
export function RemainderUnsigned(left, right) {
  if (right === 0n)
    throw new Error("DivideByZeroException");
  return left % right;
}
export function AbsSigned(value, minValue) {
  if (value === minValue)
    throw new Error("OverflowException");
  return value < 0n ? -value : value;
}
export function CopySignSigned(value, sign, minValue) {
  if (value === minValue) {
    if (sign >= 0n)
      throw new Error("OverflowException");
    return minValue;
  }
  let magnitude = value < 0n ? -value : value;
  return sign < 0n ? -magnitude : magnitude;
}
export function Clamp(value, min, max) {
  if (min > max)
    throw new Error("ArgumentException: 'min' cannot be greater than max.");
  return value < min ? min : value > max ? max : value;
}
export function LeadingZeroCount(value, bitWidth, mask) {
  let normalized = value & mask;
  if (normalized === 0n)
    return BigInt(bitWidth);
  let significantBits = 0n;
  while (normalized > 0n) {
    normalized = normalized >> 1n;
    significantBits = significantBits + 1n;
  }
  return BigInt(bitWidth) - significantBits;
}
export function PopCount(value, mask) {
  let count = 0n;
  let normalized = value & mask;
  while (normalized > 0n) {
    count = count + (normalized & 1n);
    normalized = normalized >> 1n;
  }
  return count;
}
export function RotateLeft(value, rotateAmount, bitWidth, mask, modulus, signBit, signed) {
  let amount = NormalizeRotateAmount(rotateAmount, bitWidth);
  let shift = BigInt(amount);
  let normalized = value & mask;
  if (shift !== 0n) {
    let width = BigInt(bitWidth);
    normalized = (normalized << shift | normalized >> width - shift) & mask;
  }
  return RestoreSignedValue(normalized, modulus, signBit, signed);
}
export function RotateRight(value, rotateAmount, bitWidth, mask, modulus, signBit, signed) {
  let amount = NormalizeRotateAmount(rotateAmount, bitWidth);
  let shift = BigInt(amount);
  let normalized = value & mask;
  if (shift !== 0n) {
    let width = BigInt(bitWidth);
    normalized = (normalized >> shift | normalized << width - shift) & mask;
  }
  return RestoreSignedValue(normalized, modulus, signBit, signed);
}
export function TrailingZeroCount(value, bitWidth, mask) {
  let normalized = value & mask;
  if (normalized === 0n)
    return BigInt(bitWidth);
  let count = 0n;
  while ((normalized & 1n) === 0n) {
    normalized = normalized >> 1n;
    count = count + 1n;
  }
  return count;
}
export function Log2Signed(value) {
  if (value < 0n)
    throw new Error("ArgumentOutOfRangeException: value must be non-negative.");
  if (value === 0n)
    return 0n;
  let result = -1n;
  while (value > 0n) {
    value = value >> 1n;
    result = result + 1n;
  }
  return result;
}
export function Log10(value) {
  if (value < 0n)
    throw new Error("ArgumentOutOfRangeException: value must be non-negative.");
  return value === 0n ? 0n : BigInt(value.toString().length - 1);
}
export function BigMulSigned(left, right, bitWidth) {
  let product = left * right;
  let shift = BigInt(bitWidth);
  return [BigInt.asIntN(bitWidth, product >> shift), BigInt.asIntN(bitWidth, product)];
}
export function BigMulUnsigned(left, right, bitWidth) {
  let product = left * right;
  let shift = BigInt(bitWidth);
  return [BigInt.asUintN(bitWidth, product >> shift), BigInt.asUintN(bitWidth, product)];
}
export function MaxMagnitude(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
export function MinMagnitude(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return x < y ? x : y;
}
function NormalizeRotateAmount(rotateAmount, bitWidth) {
  let amount = rotateAmount % bitWidth;
  return amount < 0 ? amount + bitWidth : amount;
}
function RestoreSignedValue(normalized, modulus, signBit, signed) {
  return signed && normalized >= signBit ? normalized - modulus : normalized;
}
