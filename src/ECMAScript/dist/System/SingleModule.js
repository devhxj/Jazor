import { GetHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { RoundSingleCore, RoundToEvenCore as i$93aeebca0c7018b8 } from "System/DoubleModule.js";
import { GetHighestSetBit, TryDecodeUtf8 } from "System/RuntimeModule.js";
function AreEqualCore(left, right) {
  if (isNaN(left) || isNaN(right))
    return isNaN(left) && isNaN(right);
  return !(left < right) && !(left > right);
}
function CompareCore(left, right) {
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
function IsFiniteCore(value) {
  return !isNaN(value) && value !== Number.POSITIVE_INFINITY && value !== Number.NEGATIVE_INFINITY;
}
function TryParseCore(text, value) {
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
function IsPow2Core(value) {
  if (!IsFiniteCore(value) || value <= 0)
    return false;
  let exponent = Math.log2(value);
  return IsFiniteCore(exponent) && Math.floor(exponent) === exponent && Math.fround(Math.pow(2, exponent)) === Math.fround(value);
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
function RoundToEvenCore(value) {
  return Math.fround(i$93aeebca0c7018b8(value));
}
export function RoundCore(value, digits, mode) {
  if (digits < 0 || digits > 6)
    throw new Error("ArgumentOutOfRangeException: digits must be between 0 and 6.");
  if (mode < 0 || mode > 4)
    throw new Error("ArgumentException: Invalid MidpointRounding value.");
  return RoundSingleCore(value, digits, mode);
}
function Ieee754RemainderCore(left, right) {
  if (isNaN(left))
    return left;
  if (isNaN(right))
    return right;
  let regular = Math.fround(left % right);
  if (isNaN(regular))
    return Number.NaN;
  if (regular === 0)
    return left < 0 || Object.is(left, -0) ? -0 : 0;
  let alternative = Math.fround(regular - Math.abs(right) * (left < 0 ? -1 : 1));
  let regularMagnitude = Math.abs(regular);
  let alternativeMagnitude = Math.abs(alternative);
  if (alternativeMagnitude === regularMagnitude) {
    let quotient = Math.fround(left / right);
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
function ILogBCore(value) {
  if (isNaN(value) || !IsFiniteCore(value))
    return 2147483647;
  if (value === 0)
    return -2147483648;
  let buffer = new ArrayBuffer(4);
  let view = new DataView(buffer);
  view.setFloat32(0, Math.abs(value), false);
  let bits = view.getUint32(0, false);
  let exponentBits = Math.floor(bits / 8388608) % 256;
  if (exponentBits !== 0)
    return exponentBits - 127;
  return GetHighestSetBit(bits % 8388608) - 149;
}
function BitIncrementCore(value) {
  return OffsetAdjacentCore(value, true);
}
function BitDecrementCore(value) {
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
    return increment ? 1.401298464324817E-45 : -1.401298464324817E-45;
  let buffer = new ArrayBuffer(4);
  let view = new DataView(buffer);
  view.setFloat32(0, value, false);
  let bits = view.getUint32(0, false);
  let increaseBits = increment ? value > 0 : value < 0;
  bits = increaseBits ? bits + 1 : bits - 1;
  view.setUint32(0, bits, false);
  return view.getFloat32(0, false);
}
function MaxMagnitudeCore(x, y) {
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
function MinMagnitudeCore(x, y) {
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
function MaxMagnitudeNumberCore(x, y) {
  if (isNaN(x))
    return y;
  if (isNaN(y))
    return x;
  return MaxMagnitudeCore(x, y);
}
function MinMagnitudeNumberCore(x, y) {
  if (isNaN(x))
    return y;
  if (isNaN(y))
    return x;
  return MinMagnitudeCore(x, y);
}
/*jazor:clr-member float.CompareTo(object)*/
export function _0b80f2f2f1a3c1a6(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Single.");
  return CompareCore(instance, value);
}
/*jazor:clr-member override float.Equals(object)*/
export function _eb69b50c7032a809(instance, obj) {
  if (obj === null || typeof obj !== "number")
    return false;
  return AreEqualCore(instance, obj);
}
/*jazor:clr-member override float.GetHashCode()*/
export function _96e065ea302b67da(instance) {
  return GetHashCodeCore(instance);
}
/*jazor:clr-member static float.Parse(string)*/
export function _d0492a7790d81596(s) {
  let result, __ref$ddd447da291ec78d43ca426e;
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$ddd447da291ec78d43ca426e = TryParseCore(s, undefined), result = __ref$ddd447da291ec78d43ca426e[1], __ref$ddd447da291ec78d43ca426e[0]))
    throw new Error(`FormatException: The input string '${s ?? ""}' was not in a correct format.`);
  return result;
}
/*jazor:clr-member static float.TryParse(string, out float)*/
export function _ced8b209dbd75890(s, result) {
  let value, __ref$79c2551ed35d1f55048427ed;
  if (!(__ref$79c2551ed35d1f55048427ed = TryParseCore(s, undefined), value = __ref$79c2551ed35d1f55048427ed[1], __ref$79c2551ed35d1f55048427ed[0]))
    return [false, 0];
  return [true, value];
}
/*jazor:clr-member static float.TryParse(System.ReadOnlySpan<char>, out float)*/
export function _8f337f9f610204bb(s, result) {
  return _ced8b209dbd75890(s, result);
}
/*jazor:clr-member static float.TryParse(System.ReadOnlySpan<byte>, out float)*/
export function _35fa5333706d7ec4(utf8Text, result) {
  return _ced8b209dbd75890(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static float.IsPow2(float)*/
export function _0dcf89ab5d6bd60c(value) {
  return IsPow2Core(value);
}
/*jazor:clr-member static float.Round(float)*/
export function _99c8e34b34aa762c(x) {
  return RoundToEvenCore(x);
}
/*jazor:clr-member static float.Round(float, int)*/
export function _a0ef44092a5b0a96(x, digits) {
  return RoundCore(x, digits, 0);
}
/*jazor:clr-member static float.Round(float, System.MidpointRounding)*/
export function _34bdf4b36464daa4(x, mode) {
  return RoundCore(x, 0, mode);
}
/*jazor:clr-member static float.Round(float, int, System.MidpointRounding)*/
export function _b0f1294dc766b202(x, digits, mode) {
  return RoundCore(x, digits, mode);
}
/*jazor:clr-member static float.BitDecrement(float)*/
export function _9840b2a560428b4a(x) {
  return BitDecrementCore(x);
}
/*jazor:clr-member static float.BitIncrement(float)*/
export function _eac91380a48fb7bd(x) {
  return BitIncrementCore(x);
}
/*jazor:clr-member static float.Ieee754Remainder(float, float)*/
export function _e54bb5d6b1fb386d(left, right) {
  return Ieee754RemainderCore(left, right);
}
/*jazor:clr-member static float.ILogB(float)*/
export function _390f9dfb01584a29(x) {
  return ILogBCore(x);
}
/*jazor:clr-member static float.ClampNative(float, float, float)*/
export function _e50ccb4182ec0a52(value, min, max) {
  return ClampNativeCore(value, min, max);
}
/*jazor:clr-member static float.Sign(float)*/
export function _323a6b94e62b2729(value) {
  return SignCore(value);
}
/*jazor:clr-member static float.MaxMagnitude(float, float)*/
export function _7c146ff0a50e958f(x, y) {
  return MaxMagnitudeCore(x, y);
}
/*jazor:clr-member static float.MaxMagnitudeNumber(float, float)*/
export function _b7b1d7781578b7e0(x, y) {
  return MaxMagnitudeNumberCore(x, y);
}
/*jazor:clr-member static float.MinMagnitude(float, float)*/
export function _e5a7b14f707c69f7(x, y) {
  return MinMagnitudeCore(x, y);
}
/*jazor:clr-member static float.MinMagnitudeNumber(float, float)*/
export function _4a2ec5d010e27cb1(x, y) {
  return MinMagnitudeNumberCore(x, y);
}
/*jazor:clr-member static float.RootN(float, int)*/
export function _9a3da74ee8bdf7c6(x, n) {
  return Math.fround(RootNCore(x, n));
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
/*jazor:clr-member static float.SinCos(float)*/
export function _9905e3952bca67bc(x) {
  return { Sin: Math.sin(x), Cos: Math.cos(x) };
}
/*jazor:clr-member static float.SinCosPi(float)*/
export function _2c792a5d6ef88cd1(x) {
  let angle = x * Math.PI;
  return { SinPi: Math.sin(angle), CosPi: Math.cos(angle) };
}
