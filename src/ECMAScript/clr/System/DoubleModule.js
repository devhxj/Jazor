import { GetHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { GetHighestSetBit, TryDecodeUtf8 } from "System/RuntimeModule.js";
function IsInfinityCore(value) {
  return Object.is(value, Number.POSITIVE_INFINITY) || Object.is(value, Number.NEGATIVE_INFINITY);
}
export function IsFiniteCore(value) {
  return !isNaN(value) && !IsInfinityCore(value);
}
export function IsNaNCore(value) {
  return isNaN(value);
}
export function TryParseCore(text, value) {
  value = 0;
  if (text === null)
    return [false, value];
  let trimmed = text.trim();
  if (trimmed.length === 0)
    return [false, value];
  let token = trimmed.toLowerCase();
  if (token === "nan") {
    value = Number.NaN;
    return [true, value];
  }
  if (token === "infinity" || token === "+infinity") {
    value = Number.POSITIVE_INFINITY;
    return [true, value];
  }
  if (token === "-infinity") {
    value = Number.NEGATIVE_INFINITY;
    return [true, value];
  }
  let decimalPattern = new RegExp("^[+-]?(?:(?:\\d+|\\d{1,3}(?:,\\d{3})+)(?:\\.\\d*)?|\\.\\d+)(?:[eE][+-]?\\d+)?$");
  if (!decimalPattern.test(trimmed))
    return [false, value];
  value = Number(trimmed.replaceAll(",", ""));
  return [true, value];
}
export function AreEqualCore(left, right) {
  if (isNaN(left) || isNaN(right))
    return isNaN(left) && isNaN(right);
  return !(left < right) && !(left > right);
}
export function CompareCore(left, right) {
  if (isNaN(left))
    return isNaN(right) ? 0 : -1;
  if (isNaN(right))
    return 1;
  if (left < right)
    return -1;
  if (left > right)
    return 1;
  return 0;
}
export function IsPow2Core(value) {
  if (!IsFiniteCore(value) || value <= 0)
    return false;
  let exponent = Math.log2(value);
  return IsFiniteCore(exponent) && Math.floor(exponent) === exponent && Math.pow(2, exponent) === value;
}
export function SignCore(value) {
  if (isNaN(value))
    throw new Error("ArithmeticException: Function does not accept floating point Not-a-Number values.");
  if (value > 0)
    return 1;
  if (value < 0)
    return -1;
  return 0;
}
export function RoundToEvenCore(value) {
  if (!IsFiniteCore(value) || value === 0)
    return value;
  let truncated = Math.trunc(value);
  let difference = Math.abs(value - truncated);
  if (difference < 0.5)
    return truncated;
  if (difference > 0.5)
    return truncated + (value < 0 ? -1 : 1);
  return truncated % 2 === 0 ? truncated : truncated + (value < 0 ? -1 : 1);
}
export function RoundIntegralCore(value, mode) {
  if (mode === 0)
    return RoundToEvenCore(value);
  if (mode === 1) {
    if (!IsFiniteCore(value) || value === 0)
      return value;
    let truncated = Math.trunc(value);
    return Math.abs(value - truncated) < 0.5 ? truncated : truncated + (value < 0 ? -1 : 1);
  }
  if (mode === 2)
    return Math.trunc(value);
  if (mode === 3)
    return Math.floor(value);
  return Math.ceil(value);
}
export function RoundCore(value, digits, mode) {
  if (digits < 0 || digits > 15)
    throw new Error("ArgumentOutOfRangeException: digits must be between 0 and 15.");
  if (mode < 0 || mode > 4)
    throw new Error("ArgumentException: Invalid MidpointRounding value.");
  return RoundBinaryCore(value, GetDoubleBitsCore(value), 52, 11, 1023, -1074, 4503599627370496, digits, mode);
}
export function RoundSingleCore(value, digits, mode) {
  let singleValue = Math.fround(value);
  return Math.fround(RoundBinaryCore(singleValue, GetSingleBitsCore(singleValue), 23, 8, 127, -149, 8388608, digits, mode));
}
function RoundBinaryCore(value, bits, fractionBitCount, exponentBitCount, exponentBias, subnormalExponent, integerBoundary, digits, mode) {
  if (!IsFiniteCore(value) || value === 0 || Math.abs(value) >= integerBoundary)
    return value;
  let exponentMask = (1n << BigInt(exponentBitCount)) - 1n;
  let fractionMask = (1n << BigInt(fractionBitCount)) - 1n;
  let exponentField = bits >> BigInt(fractionBitCount) & exponentMask;
  let significand = bits & fractionMask;
  let exponent = subnormalExponent;
  if (exponentField !== 0n) {
    significand |= 1n << BigInt(fractionBitCount);
    exponent = Number(exponentField) - exponentBias - fractionBitCount;
  }
  if (exponent + digits >= 0)
    return value;
  let numerator = significand * PowerOfTenCore(digits);
  let denominator = 1n << BigInt(-exponent);
  let floor = numerator / denominator;
  let remainder = numerator % denominator;
  let hasRemainder = remainder !== 0n;
  let doubledRemainder = remainder << 1n;
  let midpointComparison = doubledRemainder < denominator ? -1 : doubledRemainder > denominator ? 1 : 0;
  let isFloorOdd = (floor & 1n) !== 0n;
  let isNegative = value < 0;
  if (ShouldRoundUpCore(mode, midpointComparison, isFloorOdd, hasRemainder, isNegative))
    floor += 1n;
  let rounded = Number(floor.toString() + "e-" + digits);
  return isNegative ? -rounded : rounded;
}
function GetDoubleBitsCore(value) {
  let buffer = new ArrayBuffer(8);
  let view = new DataView(buffer);
  view.setFloat64(0, value, false);
  return view.getBigUint64(0, false);
}
function GetSingleBitsCore(value) {
  let buffer = new ArrayBuffer(4);
  let view = new DataView(buffer);
  view.setFloat32(0, value, false);
  return BigInt(view.getUint32(0, false));
}
function PowerOfTenCore(digits) {
  let power = 1n;
  for (let index = 0; index < digits; index++)
    power *= BigInt(10);
  return power;
}
function ShouldRoundUpCore(mode, midpointComparison, isFloorOdd, hasRemainder, isNegative) {
  if (mode === 0)
    return midpointComparison > 0 || hasRemainder && midpointComparison === 0 && isFloorOdd;
  if (mode === 1)
    return midpointComparison > 0 || hasRemainder && midpointComparison === 0;
  if (mode === 2)
    return false;
  if (mode === 3)
    return isNegative && hasRemainder;
  return !isNegative && hasRemainder;
}
export function Ieee754RemainderCore(left, right) {
  if (isNaN(left))
    return left;
  if (isNaN(right))
    return right;
  let regular = left % right;
  if (isNaN(regular))
    return Number.NaN;
  if (regular === 0)
    return left < 0 || Object.is(left, -0) ? -0 : 0;
  let alternative = regular - Math.abs(right) * (left < 0 ? -1 : 1);
  let regularMagnitude = Math.abs(regular);
  let alternativeMagnitude = Math.abs(alternative);
  if (alternativeMagnitude === regularMagnitude) {
    let quotient = left / right;
    return Math.abs(RoundToEvenCore(quotient)) > Math.abs(quotient) ? alternative : regular;
  }
  return alternativeMagnitude < regularMagnitude ? alternative : regular;
}
function MaxNativeCore(left, right) {
  return left > right ? left : right;
}
function MinNativeCore(left, right) {
  return left < right ? left : right;
}
function ClampNativeCore(value, min, max) {
  if (min > max)
    throw new Error("ArgumentException: 'min' cannot be greater than max.");
  return MinNativeCore(MaxNativeCore(value, min), max);
}
export function ILogBCore(value) {
  if (isNaN(value) || !IsFiniteCore(value))
    return 2147483647;
  if (value === 0)
    return -2147483648;
  let buffer = new ArrayBuffer(8);
  let view = new DataView(buffer);
  view.setFloat64(0, Math.abs(value), false);
  let high = view.getUint32(0, false);
  let low = view.getUint32(4, false);
  let exponentBits = Math.floor(high / 1048576) % 2048;
  if (exponentBits !== 0)
    return exponentBits - 1023;
  let highMantissa = high % 1048576;
  return highMantissa !== 0 ? GetHighestSetBit(highMantissa) - 1042 : GetHighestSetBit(low) - 1074;
}
export function BitIncrementCore(value) {
  return OffsetAdjacentCore(value, true);
}
export function BitDecrementCore(value) {
  return OffsetAdjacentCore(value, false);
}
function OffsetAdjacentCore(value, increment) {
  if (isNaN(value))
    return value;
  if (increment && value === Number.POSITIVE_INFINITY)
    return value;
  if (!increment && value === Number.NEGATIVE_INFINITY)
    return value;
  if (value === 0)
    return increment ? Number.MIN_VALUE : -Number.MIN_VALUE;
  let buffer = new ArrayBuffer(8);
  let view = new DataView(buffer);
  view.setFloat64(0, value, false);
  let high = view.getUint32(0, false);
  let low = view.getUint32(4, false);
  let increaseBits = increment ? value > 0 : value < 0;
  if (increaseBits) {
    if (low === 4294967295) {
      low = 0;
      high++;
    }
    else {
      low++;
    }
  }
  else if (low === 0) {
    low = 4294967295;
    high--;
  }
  else {
    low--;
  }
  view.setUint32(0, high, false);
  view.setUint32(4, low, false);
  return view.getFloat64(0, false);
}
export function MaxMagnitudeCore(x, y) {
  if (isNaN(x) || isNaN(y))
    return Number.NaN;
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return Math.max(x, y);
}
export function MinMagnitudeCore(x, y) {
  if (isNaN(x) || isNaN(y))
    return Number.NaN;
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return Math.min(x, y);
}
export function MaxMagnitudeNumberCore(x, y) {
  if (isNaN(x))
    return y;
  if (isNaN(y))
    return x;
  return MaxMagnitudeCore(x, y);
}
export function MinMagnitudeNumberCore(x, y) {
  if (isNaN(x))
    return y;
  if (isNaN(y))
    return x;
  return MinMagnitudeCore(x, y);
}
/*jazor:clr-member double.CompareTo(object)*/
export function _b0d483b6deae2278(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Double.");
  return CompareCore(instance, value);
}
/*jazor:clr-member override double.Equals(object)*/
export function _b5f97a04bba189b0(instance, obj) {
  if (obj === null || typeof obj !== "number")
    return false;
  return AreEqualCore(instance, obj);
}
/*jazor:clr-member override double.GetHashCode()*/
export function _73dea7106d8085a6(instance) {
  return GetHashCodeCore(instance);
}
/*jazor:clr-member static double.Parse(string)*/
export function _5810f85a3710b88d(s) {
  let result, __ref$89ecc165ce81b99e7ba7d890;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$89ecc165ce81b99e7ba7d890 = TryParseCore(s, undefined), result = __ref$89ecc165ce81b99e7ba7d890[1], __ref$89ecc165ce81b99e7ba7d890[0]))
    throw new Error(`FormatException: The input string '${s ?? ""}' was not in a correct format.`);
  return result;
}
/*jazor:clr-member static double.TryParse(string, out double)*/
export function _a29d389185c5e37d(s, result) {
  let value, __ref$2cf547673420ca0d5d857c7f;
  if (!(__ref$2cf547673420ca0d5d857c7f = TryParseCore(s, undefined), value = __ref$2cf547673420ca0d5d857c7f[1], __ref$2cf547673420ca0d5d857c7f[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static double.TryParse(System.ReadOnlySpan<char>, out double)*/
export function _059799e0a3b763c1(s, result) {
  return _a29d389185c5e37d(s, result);
}
/*jazor:clr-member static double.TryParse(System.ReadOnlySpan<byte>, out double)*/
export function _ec88293b6cb03791(utf8Text, result) {
  return _a29d389185c5e37d(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static double.IsPow2(double)*/
export function _0f9f49a802919a8f(value) {
  return IsPow2Core(value);
}
/*jazor:clr-member static double.Round(double)*/
export function _0bc6b7459346bc5f(x) {
  return RoundToEvenCore(x);
}
/*jazor:clr-member static double.Round(double, int)*/
export function _b439595e3752c6a9(x, digits) {
  return RoundCore(x, digits, 0);
}
/*jazor:clr-member static double.Round(double, System.MidpointRounding)*/
export function _7aeacc68b27f02f7(x, mode) {
  return RoundCore(x, 0, mode);
}
/*jazor:clr-member static double.Round(double, int, System.MidpointRounding)*/
export function _6e429701c9779ef6(x, digits, mode) {
  return RoundCore(x, digits, mode);
}
/*jazor:clr-member static double.BitDecrement(double)*/
export function _4ce9474a7b3b7534(x) {
  return BitDecrementCore(x);
}
/*jazor:clr-member static double.BitIncrement(double)*/
export function _a83d47e386f63de0(x) {
  return BitIncrementCore(x);
}
/*jazor:clr-member static double.Ieee754Remainder(double, double)*/
export function _092bc2bc891d33a8(left, right) {
  return Ieee754RemainderCore(left, right);
}
/*jazor:clr-member static double.ILogB(double)*/
export function _48628732b1dc8ac9(x) {
  return ILogBCore(x);
}
/*jazor:clr-member static double.ClampNative(double, double, double)*/
export function _ead55aa3a172f045(value, min, max) {
  return ClampNativeCore(value, min, max);
}
/*jazor:clr-member static double.Sign(double)*/
export function _eee146c74a9bc322(value) {
  return SignCore(value);
}
/*jazor:clr-member static double.MaxMagnitude(double, double)*/
export function _b6202851542d164c(x, y) {
  return MaxMagnitudeCore(x, y);
}
/*jazor:clr-member static double.MaxMagnitudeNumber(double, double)*/
export function _7f7b38b043f3f42f(x, y) {
  return MaxMagnitudeNumberCore(x, y);
}
/*jazor:clr-member static double.MinMagnitude(double, double)*/
export function _bb1daa880a2ad14e(x, y) {
  return MinMagnitudeCore(x, y);
}
/*jazor:clr-member static double.MinMagnitudeNumber(double, double)*/
export function _315c6cdfa11efcf2(x, y) {
  return MinMagnitudeNumberCore(x, y);
}
/*jazor:clr-member static double.TryParse(string, System.IFormatProvider, out double)*/
export function _f1644d5121fae09c(s, provider, result) {
  let value, __ref$d274fb34e7ed52f39becf48a;
  if (!(__ref$d274fb34e7ed52f39becf48a = TryParseCore(s, undefined), value = __ref$d274fb34e7ed52f39becf48a[1], __ref$d274fb34e7ed52f39becf48a[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static double.RootN(double, int)*/
export function _83649fc6ded4d88e(x, n) {
  return RootNCore(x, n);
}
function RootNCore(value, degree) {
  if (degree === 0)
    return Number.NaN;
  let oddDegree = degree % 2 !== 0;
  if (value < 0 && !oddDegree)
    return Number.NaN;
  let magnitude = Math.pow(Math.abs(value), 1 / degree);
  let negativeResult = oddDegree && (value < 0 || Object.is(value, -0));
  return negativeResult ? -magnitude : magnitude;
}
/*jazor:clr-member static double.SinCos(double)*/
export function _bc56189e3e1f8a22(x) {
  return { Sin: Math.sin(x), Cos: Math.cos(x) };
}
/*jazor:clr-member static double.SinCosPi(double)*/
export function _0f4aeef5d225794d(x) {
  let angle = x * Math.PI;
  return { SinPi: Math.sin(angle), CosPi: Math.cos(angle) };
}
