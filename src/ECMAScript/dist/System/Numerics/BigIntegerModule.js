import { GetHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { _be8b149ea0e1d76b } from "System/DecimalModule.js";
import { IsFiniteCore } from "System/DoubleModule.js";
import { FromBigIntCore } from "System/HalfModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function ParseBytesCore(bytes, isUnsigned, isBigEndian) {
  if (bytes.length === 0)
    return 0n;
  let value = 0n;
  if (isBigEndian) {
    for (let i = 0; i < bytes.length; i++)
      value = value << BigInt(8) | BigInt(bytes[i]);
  }
  else {
    for (let i = bytes.length - 1; i >= 0; i--)
      value = value << BigInt(8) | BigInt(bytes[i]);
  }
  let signByte = isBigEndian ? bytes[0] : bytes[bytes.length - 1];
  if (!isUnsigned && (signByte & 128) !== 0)
    value -= 1n << BigInt(bytes.length * 8);
  return value;
}
function GetByteCountCore(value, isUnsigned) {
  if (isUnsigned && value < 0n)
    throw new Error("OverflowException: Negative values do not have an unsigned representation.");
  let bitLength = GetBitLengthCore(value) + (isUnsigned ? 0n : 1n);
  let byteCount = (bitLength + BigInt(7)) / BigInt(8);
  return Number(byteCount > 0n ? byteCount : 1n);
}
function GetBytesCore(value, isUnsigned, isBigEndian) {
  let byteCount = GetByteCountCore(value, isUnsigned);
  let encoded = value;
  if (value < 0n)
    encoded += 1n << BigInt(byteCount * 8);
  let bytes = new Array;
  for (let i = 0; i < byteCount; i++) {
    bytes.push(Number(encoded & BigInt(255)));
    encoded >>= BigInt(8);
  }
  if (isBigEndian)
    bytes.reverse();
  return bytes;
}
function GetWordWidthCore(value, minimumWidth) {
  let magnitude = value < 0n ? -value : value;
  let bitLength = GetBitLengthCore(magnitude);
  let wordSize = BigInt(32);
  let width = (bitLength + wordSize - 1n) / wordSize * wordSize;
  return width < minimumWidth ? minimumWidth : width;
}
function RotateCore(value, rotateAmount, rotateLeft) {
  if (value === 0n)
    return 0n;
  let width = GetWordWidthCore(value, BigInt(32));
  let widthNumber = Number(width);
  let amount = rotateAmount % widthNumber;
  if (amount < 0)
    amount += widthNumber;
  if (amount === 0)
    return value;
  let modulus = 1n << width;
  let mask = modulus - 1n;
  let bits = value < 0n ? modulus + value : value;
  let shift = BigInt(amount);
  let complementShift = width - shift;
  let rotated = rotateLeft ? (bits << shift | bits >> complementShift) & mask : (bits >> shift | bits << complementShift) & mask;
  if (value < 0n && (rotated & 1n << width - 1n) !== 0n)
    return rotated - modulus;
  return rotated;
}
function FromFloatingPointCore(value) {
  if (!IsFiniteCore(value))
    throw new Error("OverflowException: Cannot convert a non-finite floating-point value to BigInteger.");
  return BigInt(Math.trunc(value));
}
function FromDecimalCore(value) {
  return BigInt(_be8b149ea0e1d76b(value));
}
function RequireRangeCore(value, min, max) {
  if (value < min || value > max)
    throw new Error("OverflowException: Value was either too large or too small for the target type.");
  return value;
}
function RequireNumberRangeCore(value, min, max) {
  return Number(RequireRangeCore(value, min, max));
}
function ToDecimalCore(value) {
  let limit = BigInt("79228162514264337593543950335");
  return RequireRangeCore(value, -limit, limit).toString();
}
/*jazor:clr-member System.Numerics.BigInteger.BigInteger(float)*/
export function _cfd2038efd505e1f(value) {
  return FromFloatingPointCore(value);
}
/*jazor:clr-member System.Numerics.BigInteger.BigInteger(double)*/
export function _38c7caccfd5e120e(value) {
  return FromFloatingPointCore(value);
}
/*jazor:clr-member System.Numerics.BigInteger.BigInteger(decimal)*/
export function _f715f85cc5dcfe92(value) {
  return FromDecimalCore(value);
}
/*jazor:clr-member System.Numerics.BigInteger.BigInteger(byte[])*/
export function _c1e724fa6dbf63eb(value) {
  return ParseBytesCore(value, false, false);
}
/*jazor:clr-member System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)*/
export function _9c321a7400e5ff9b(value, isUnsigned, isBigEndian) {
  return ParseBytesCore(value, isUnsigned, isBigEndian);
}
/*jazor:clr-member System.Numerics.BigInteger.Sign.get*/
export function _734290a188c5bc5a(instance) {
  if (instance > 0n)
    return 1;
  if (instance < 0n)
    return -1;
  return 0;
}
function GetBitLengthCore(instance) {
  if (instance === 0n)
    return 0n;
  let isNegative = instance < 0n;
  let value = isNegative ? -instance - 1n : instance;
  let bitLength = 0n;
  while (value > 0n) {
    bitLength += 1n;
    value >>= 1n;
  }
  return bitLength;
}
function ComputePositiveLog(value, baseValue) {
  if (value <= BigInt(Number.MAX_SAFE_INTEGER))
    return Math.log(Number(value)) / Math.log(baseValue);
  let bitLength = Number(GetBitLengthCore(value));
  let shift = bitLength - 64;
  let x = shift > 0 ? value >> BigInt(shift) : value << BigInt(-shift);
  return Math.log(Number(x)) / Math.log(baseValue) + shift / (Math.log(baseValue) / Math.log(2));
}
function TryParseCore(text, value) {
  value = 0n;
  if (text === null)
    return [false, value];
  let trimmed = text.trim();
  if (!RegExp("^[+-]?\\d+$").test(trimmed))
    return [false, value];
  try {
    value = BigInt(trimmed);
    return [true, value];
  } catch {
    return [false, value];
  }
}
/*jazor:clr-member static System.Numerics.BigInteger.Parse(string)*/
export function _155212572c9a3297(value) {
  let result, __ref$46fbec58d54f868cb2f7e9b6;
  if (value === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$46fbec58d54f868cb2f7e9b6 = TryParseCore(value, undefined), result = __ref$46fbec58d54f868cb2f7e9b6[1], __ref$46fbec58d54f868cb2f7e9b6[0]))
    throw new Error(`FormatException: The input string '${value ?? ""}' was not in a correct format.`);
  return result;
}
/*jazor:clr-member static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)*/
export function _59acea2facdaa757(value, result) {
  let parsed, __ref$439c15fe7f09e89c83302db2;
  if (!(__ref$439c15fe7f09e89c83302db2 = TryParseCore(value, undefined), parsed = __ref$439c15fe7f09e89c83302db2[1], __ref$439c15fe7f09e89c83302db2[0]))
    return [false, 0n];
  return [true, parsed];
}
/*jazor:clr-member static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)*/
export function _ded03bf84977945f(value, result) {
  return _59acea2facdaa757(value, result);
}
/*jazor:clr-member static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)*/
export function _598611fb2b8a064a(dividend, divisor, remainder) {
  if (divisor === 0n)
    throw new Error("DivideByZeroException: Attempted to divide by zero.");
  let quotient = dividend / divisor;
  let rem = dividend % divisor;
  return [quotient, rem];
}
/*jazor:clr-member static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)*/
export function _fb5a811e7a32a324(value) {
  return _acb5aef300c8db0c(value, Math.E);
}
/*jazor:clr-member static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)*/
export function _acb5aef300c8db0c(value, baseValue) {
  if (value < 0n || baseValue === 1)
    return Number.NaN;
  if (baseValue === Number.POSITIVE_INFINITY)
    return value === 1n ? 0 : Number.NaN;
  if (baseValue === 0 && value !== 1n)
    return Number.NaN;
  if (value === 0n)
    return Math.log(0) / Math.log(baseValue);
  return ComputePositiveLog(value, baseValue);
}
/*jazor:clr-member static System.Numerics.BigInteger.Log10(System.Numerics.BigInteger)*/
export function _f276cbd7c3b305ea(value) {
  return _acb5aef300c8db0c(value, 10);
}
/*jazor:clr-member static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)*/
export function _7555649a5efc7b79(left, right) {
  let a = left < 0n ? -left : left;
  let b = right < 0n ? -right : right;
  if (a === 0n)
    return b;
  if (b === 0n)
    return a;
  while (b !== 0n) {
    let temp = b;
    b = a % b;
    a = temp;
  }
  return a;
}
/*jazor:clr-member static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)*/
export function _ec6961a106ca5bf3(value, exponent, modulus) {
  if (exponent < 0n)
    throw new Error("ArgumentOutOfRangeException: Exponent must be non-negative.");
  if (modulus === 0n)
    throw new Error("DivideByZeroException");
  let modulusMagnitude = modulus < 0n ? -modulus : modulus;
  if (modulusMagnitude === 1n)
    return 0n;
  let result = 1n;
  let negativeResult = value < 0n && (exponent & 1n) === 1n;
  let val = (value < 0n ? -value : value) % modulusMagnitude;
  let exp = exponent;
  while (exp > 0n) {
    if ((exp & 1n) === 1n)
      result = result * val % modulusMagnitude;
    exp >>= 1n;
    val = val * val % modulusMagnitude;
  }
  return negativeResult && result !== 0n ? -result : result;
}
/*jazor:clr-member static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)*/
export function _31cf4d89164dee40(value, exponent) {
  if (exponent < 0 || !Number.isInteger(exponent))
    throw new Error("ArgumentOutOfRangeException: Exponent must be a non-negative integer.");
  let result = 1n;
  let current = value;
  let exp = exponent;
  while (exp > 0) {
    if (exp % 2 === 1)
      result *= current;
    current *= current;
    exp = Math.floor(exp / 2);
  }
  return result;
}
/*jazor:clr-member override System.Numerics.BigInteger.GetHashCode()*/
export function _fe64082374302a77(instance) {
  return GetHashCodeCore(instance);
}
/*jazor:clr-member System.Numerics.BigInteger.CompareTo(object)*/
export function _9f7b3705890bed98(instance, obj) {
  let bigIntValue;
  if (obj === null)
    return 1;
  if (typeof obj === "bigint" && (bigIntValue = obj, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type BigInteger.");
}
/*jazor:clr-member System.Numerics.BigInteger.ToByteArray()*/
export function _ca46777d5c8cc9b9(instance) {
  return GetBytesCore(instance, false, false);
}
/*jazor:clr-member System.Numerics.BigInteger.ToByteArray(bool, bool)*/
export function _11ed9d474ccf2419(instance, isUnsigned, isBigEndian) {
  return GetBytesCore(instance, isUnsigned, isBigEndian);
}
/*jazor:clr-member System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)*/
export function _76ae4e496fc976fd(instance, destination, bytesWritten, isUnsigned, isBigEndian) {
  let bytes = GetBytesCore(instance, isUnsigned, isBigEndian);
  if (destination.length < bytes.length)
    return [false, 0];
  for (let i = 0; i < bytes.length; i++)
    destination[i] = bytes[i];
  return [true, bytes.length];
}
/*jazor:clr-member System.Numerics.BigInteger.GetByteCount(bool)*/
export function _c1393b267008395c(instance, isUnsigned) {
  return GetByteCountCore(instance, isUnsigned);
}
/*jazor:clr-member System.Numerics.BigInteger.ToString(System.IFormatProvider)*/
export function _fe4c3211e57446e7(instance, provider) {
  if (provider == null)
    return instance.toString();
  let isNegative = instance < 0n;
  let absValue = isNegative ? -instance : instance;
  let strValue = absValue.toString();
  try {
    if (absValue <= BigInt(Number.MAX_SAFE_INTEGER)) {
      let formatted = provider.format(Number(absValue));
      return isNegative ? `-${formatted ?? ""}` : formatted;
    }
    let sample = provider.format(1000.1);
    let groupChar = sample.includes("1,000") ? "," : sample.includes("1.000") ? "." : sample.includes("1 000") ? " " : ",";
    let result = "";
    let i = strValue.length;
    let groupCount = 0;
    while (i > 0) {
      if (groupCount > 0 && groupCount % 3 === 0) {
        result = groupChar + result;
      }
      result = _5ad63706a889c294(strValue, --i) + result;
      groupCount++;
    }
    return isNegative ? `-${result ?? ""}` : result;
  } catch {
    return instance.toString();
  }
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)*/
export function _c1afe3218f0f82f9(value) {
  return RequireNumberRangeCore(value, 0n, BigInt(255));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator char(System.Numerics.BigInteger)*/
export function _ac2920ee8216c023(value) {
  return RequireNumberRangeCore(value, 0n, BigInt(65535));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator decimal(System.Numerics.BigInteger)*/
export function _9d2085a2aa8febea(value) {
  return ToDecimalCore(value);
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)*/
export function _7c41bbf7746a0266(value) {
  return FromBigIntCore(value);
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator short(System.Numerics.BigInteger)*/
export function _c57fc79b767bf069(value) {
  return RequireNumberRangeCore(value, BigInt(-32768), BigInt(32767));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator int(System.Numerics.BigInteger)*/
export function _7c261f922cc43235(value) {
  return RequireNumberRangeCore(value, BigInt(-2147483648), BigInt(2147483647));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)*/
export function _15fe350cf299c580(value) {
  return RequireRangeCore(value, BigInt("-9223372036854775808"), BigInt("9223372036854775807"));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)*/
export function _5958070a15559320(value) {
  return RequireRangeCore(value, BigInt("-170141183460469231731687303715884105728"), BigInt("170141183460469231731687303715884105727"));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator sbyte(System.Numerics.BigInteger)*/
export function _63d8cc7789144528(value) {
  return RequireNumberRangeCore(value, BigInt(-128), BigInt(127));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator ushort(System.Numerics.BigInteger)*/
export function _b2311568a6faa3b8(value) {
  return RequireNumberRangeCore(value, 0n, BigInt(65535));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator uint(System.Numerics.BigInteger)*/
export function _385437ecb9a2b10a(value) {
  return RequireNumberRangeCore(value, 0n, BigInt("4294967295"));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator ulong(System.Numerics.BigInteger)*/
export function _6043725cddf263dd(value) {
  return RequireRangeCore(value, 0n, BigInt("18446744073709551615"));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)*/
export function _f8ae8a4213449843(value) {
  return RequireRangeCore(value, 0n, BigInt("340282366920938463463374607431768211455"));
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(decimal)*/
export function _8e505e0ce7efa99c(value) {
  return FromDecimalCore(value);
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)*/
export function _933b3164355c792a(value) {
  return FromFloatingPointCore(value);
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Half)*/
export function _c186238bc3a46d2b(value) {
  return FromFloatingPointCore(value);
}
/*jazor:clr-member static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(float)*/
export function _212b6e60ce4e6836(value) {
  return FromFloatingPointCore(value);
}
/*jazor:clr-member System.Numerics.BigInteger.GetBitLength()*/
export function _41fe76dfb4ee2ab2(instance) {
  return GetBitLengthCore(instance);
}
/*jazor:clr-member static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)*/
export function _22a21ffe19479f32(left, right) {
  if (right === 0n)
    throw new Error("DivideByZeroException: Attempted to divide by zero.");
  let quotient = left / right;
  let remainder = left % right;
  return [quotient, remainder];
}
/*jazor:clr-member static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)*/
export function _276680abacb93277(value) {
  if (value === 0n)
    return BigInt(32);
  if (value < 0n)
    return 0n;
  let remainder = GetBitLengthCore(value) % BigInt(32);
  if (remainder === 0n)
    return 0n;
  return BigInt(32) - remainder;
}
/*jazor:clr-member static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)*/
export function _5e476c376aca56ae(value) {
  if (value === 0n)
    return 0n;
  let count = 0n;
  let n = value;
  if (value < 0n) {
    let bitLength = GetBitLengthCore(value) + 1n;
    let wordSize = BigInt(32);
    let width = (bitLength + wordSize - 1n) / wordSize * wordSize;
    if (width < wordSize)
      width = wordSize;
    n = (1n << width) + value;
  }
  while (n > 0n) {
    n &= n - 1n;
    count += 1n;
  }
  return count;
}
/*jazor:clr-member static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)*/
export function _ae7b1dd18af32f04(value, rotateAmount) {
  return RotateCore(value, rotateAmount, true);
}
/*jazor:clr-member static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)*/
export function _dc8cc860511e78b3(value, rotateAmount) {
  return RotateCore(value, rotateAmount, false);
}
/*jazor:clr-member static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)*/
export function _696502aae4b6e182(value) {
  if (value === 0n)
    return BigInt(32);
  let count = 0n;
  let temp = value;
  while ((temp & 1n) === 0n) {
    count++;
    temp >>= 1n;
  }
  return count;
}
/*jazor:clr-member static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)*/
export function _c0651d019a4b12b1(value) {
  if (value <= 0n)
    return false;
  let minusOne = value - 1n;
  let result = (value & minusOne) === 0n;
  return result;
}
/*jazor:clr-member static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)*/
export function _c29a05a989ec3b33(value) {
  if (value < 0n)
    throw new Error("ArgumentOutOfRangeException: Value must be non-negative.");
  if (value === 0n)
    return 0n;
  let result = 0n;
  let temp = value;
  while (temp > 1n) {
    result++;
    temp >>= 1n;
  }
  return result;
}
/*jazor:clr-member static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)*/
export function _8548cc83c4d947f5(value, min, max) {
  if (min > max)
    throw new Error("ArgumentException: min must be less than or equal to max.");
  let result = value;
  if (result < min)
    result = min;
  if (result > max)
    result = max;
  return result;
}
/*jazor:clr-member static System.Numerics.BigInteger.CreateChecked<TOther>(TOther)*/
export function _8cbca5624f4a6cc0(value) {
  let b, n, s, bl;
  if (typeof value === "bigint" && (b = value, true))
    return b;
  if (typeof value === "number" && (n = value, true)) {
    if (!Number.isInteger(n))
      throw new RangeError("Value must be an integer");
    if (n < Number.MIN_SAFE_INTEGER || n > Number.MAX_SAFE_INTEGER)
      throw new RangeError("Value is outside safe integer range");
    return BigInt(n);
  }
  if (typeof value === "string" && (s = value, true)) {
    let trimmed = s.trim();
    if (!RegExp("^-?\\d+$").test(trimmed))
      throw new RangeError("String must represent a valid integer");
    try {
      return BigInt(trimmed);
    } catch {
      throw new RangeError("Invalid integer string");
    }
  }
  if (typeof value === "boolean" && (bl = value, true))
    return bl ? 1n : 0n;
  if (value == null)
    throw new RangeError("Value cannot be null or undefined");
  throw new RangeError("Unsupported type for conversion to BigInt");
}
/*jazor:clr-member static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)*/
export function _d305de2c64e85995(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
/*jazor:clr-member static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)*/
export function _fef56ccd17b22e88(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return x < y ? x : y;
}
/*jazor:clr-member static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)*/
export function _49adf7adfc1228f8(value, shiftAmount) {
  if (shiftAmount < 0)
    return value << BigInt(-shiftAmount);
  let shift = BigInt(shiftAmount);
  if (value >= 0n)
    return value >> shift;
  let width = GetWordWidthCore(value, BigInt(64));
  if (shift >= width)
    return 0n;
  return (1n << width) + value >> shift;
}
