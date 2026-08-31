import { GetHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { AreEqualCore, CompareCore, IsFiniteCore, IsPow2Core, MaxMagnitudeCore, MaxMagnitudeNumberCore, MinMagnitudeCore, MinMagnitudeNumberCore, SignCore, TryParseCore } from "System/DoubleModule.js";
import { FromFloatingChecked, FromFloatingCheckedUInt128, FromFloatingSaturatingSigned, FromFloatingSaturatingUnsigned } from "System/Numerics/BigIntIntegerRuntime.js";
import { GetHighestSetBit, TryDecodeUtf8 } from "System/RuntimeModule.js";
import { RoundCore } from "System/SingleModule.js";
function RoundToHalf(value) {
  return Math.f16round(value);
}
export function FromBigIntCore(value) {
  return RoundToHalf(Number(value));
}
function CheckedToNumberCore(value, min, max) {
  if (!IsFiniteCore(value))
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  let truncated = Math.trunc(value);
  if (truncated < min || truncated > max)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  return truncated === 0 ? 0 : truncated;
}
function UncheckedToInt32Core(value) {
  if (isNaN(value))
    return 0;
  if (value >= 2147483647)
    return 2147483647;
  if (value <= -2147483648)
    return -2147483648;
  let truncated = Math.trunc(value);
  return truncated === 0 ? 0 : truncated;
}
function UncheckedToNarrowCore(value, width, signed) {
  let integer = BigInt(UncheckedToInt32Core(value));
  return Number(signed ? BigInt.asIntN(width, integer) : BigInt.asUintN(width, integer));
}
function UncheckedToUnsignedNumberCore(value, max) {
  if (isNaN(value) || value <= 0)
    return 0;
  if (value >= max)
    return max;
  return Math.trunc(value);
}
function RoundToEven(value) {
  if (!IsFiniteCore(value) || value === 0)
    return value;
  let floor = Math.floor(value);
  let fraction = value - floor;
  let rounded = fraction < 0.5 ? floor : fraction > 0.5 ? floor + 1 : floor % 2 === 0 ? floor : floor + 1;
  return rounded === 0 && value < 0 ? -0 : rounded;
}
function Ieee754RemainderCore(left, right) {
  if (isNaN(left))
    return left;
  if (isNaN(right))
    return right;
  let regular = RoundToHalf(left % right);
  if (isNaN(regular))
    return Number.NaN;
  if (regular === 0)
    return left < 0 || Object.is(left, -0) ? -0 : 0;
  let alternative = RoundToHalf(regular - Math.abs(right) * (left < 0 ? -1 : 1));
  let regularMagnitude = Math.abs(regular);
  let alternativeMagnitude = Math.abs(alternative);
  if (alternativeMagnitude === regularMagnitude) {
    let quotient = RoundToHalf(left / right);
    return Math.abs(RoundToEven(quotient)) > Math.abs(quotient) ? alternative : regular;
  }
  return alternativeMagnitude < regularMagnitude ? alternative : regular;
}
function ILogBCore(value) {
  if (isNaN(value) || !IsFiniteCore(value))
    return 2147483647;
  if (value === 0)
    return -2147483648;
  let magnitudeBits = GetHalfBitsCore(value) % 32768;
  let exponentBits = Math.floor(magnitudeBits / 1024);
  if (exponentBits !== 0)
    return exponentBits - 15;
  return GetHighestSetBit(magnitudeBits % 1024) - 24;
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
    return increment ? 5.960464477539063E-08 : -5.960464477539063E-08;
  let bits = GetHalfBitsCore(value);
  let increaseBits = increment ? value > 0 : value < 0;
  return FromHalfBitsCore(increaseBits ? bits + 1 : bits - 1);
}
function GetHalfBitsCore(value) {
  let negative = value < 0 || Object.is(value, -0);
  let signBits = negative ? 32768 : 0;
  let magnitude = Math.abs(value);
  if (magnitude === Number.POSITIVE_INFINITY)
    return signBits + 31744;
  if (magnitude === 0)
    return signBits;
  if (magnitude < 6.103515625E-05)
    return signBits + Math.floor(magnitude / 5.960464477539063E-08 + 0.5);
  let exponent = -14;
  let scale = 6.103515625E-05;
  while (magnitude >= scale * 2) {
    scale *= 2;
    exponent++;
  }
  let mantissa = Math.floor((magnitude / scale - 1) * 1024 + 0.5);
  return signBits + (exponent + 15) * 1024 + mantissa;
}
function FromHalfBitsCore(bits) {
  let negative = bits >= 32768;
  let magnitudeBits = bits % 32768;
  let exponentBits = Math.floor(magnitudeBits / 1024);
  let mantissa = magnitudeBits % 1024;
  let value;
  if (exponentBits === 0) {
    value = mantissa * 5.960464477539063E-08;
  }
  else if (exponentBits === 31) {
    value = mantissa === 0 ? Number.POSITIVE_INFINITY : Number.NaN;
  }
  else {
    value = (1 + mantissa / 1024) * Math.pow(2, exponentBits - 15);
  }
  return negative ? -value : value;
}
function ClampCore(value, min, max) {
  if (min > max)
    throw new Error("ArgumentException: 'min' cannot be greater than max.");
  return value < min ? min : value > max ? max : value;
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
function RootNCore(value, degree) {
  if (degree === 0)
    return Number.NaN;
  let oddDegree = degree % 2 !== 0;
  if (value < 0 && !oddDegree)
    return Number.NaN;
  let magnitude = Math.pow(Math.abs(value), 1 / degree);
  let negativeResult = oddDegree && (value < 0 || Object.is(value, -0));
  return RoundToHalf(negativeResult ? -magnitude : magnitude);
}
/*jazor:clr-member static System.Half.Parse(string)*/
export function _14d80007aa3543a1(text) {
  let value, __ref$9103559b6c41a89cb4a5283e;
  if (text === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (!(__ref$9103559b6c41a89cb4a5283e = TryParseCore(text, undefined), value = __ref$9103559b6c41a89cb4a5283e[1], __ref$9103559b6c41a89cb4a5283e[0]))
    throw new Error(`FormatException: The input string '${text ?? ""}' was not in a correct format.`);
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.Parse(string, System.IFormatProvider)*/
export function _92b036ecc84de08d(text, provider) {
  return _14d80007aa3543a1(text);
}
/*jazor:clr-member static System.Half.TryParse(string, out System.Half)*/
export function _83de0b9fe4433805(text, result) {
  let value, __ref$e560d80fc32631a995774fb6;
  if (!(__ref$e560d80fc32631a995774fb6 = TryParseCore(text, undefined), value = __ref$e560d80fc32631a995774fb6[1], __ref$e560d80fc32631a995774fb6[0]))
    return [false, 0];
  return [true, RoundToHalf(value)];
}
/*jazor:clr-member static System.Half.TryParse(System.ReadOnlySpan<char>, out System.Half)*/
export function _f5bea48e2d45cf92(s, result) {
  return _83de0b9fe4433805(s, result);
}
/*jazor:clr-member static System.Half.TryParse(System.ReadOnlySpan<byte>, out System.Half)*/
export function _8ed5272b36771f32(utf8Text, result) {
  return _83de0b9fe4433805(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member System.Half.CompareTo(object)*/
export function _8a86be5e4541e5ce(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Half.");
  return CompareCore(instance, value);
}
/*jazor:clr-member override System.Half.Equals(object)*/
export function _3a07dad87c237b05(instance, value) {
  if (value === null || typeof value !== "number")
    return false;
  return AreEqualCore(instance, value);
}
/*jazor:clr-member override System.Half.GetHashCode()*/
export function _f9dc2d5b5c5cdf31(instance) {
  return GetHashCodeCore(instance);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(char)*/
export function _688015ce7a06d3a3(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(double)*/
export function _c15dbcdc3a5121a4(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(short)*/
export function _5235d3bf6d040ead(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(int)*/
export function _83d328837e0849f2(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(long)*/
export function _54cc35a643b3964a(value) {
  return FromBigIntCore(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(float)*/
export function _c698784c1b652292(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(ushort)*/
export function _66978b13cd9c4d2c(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(uint)*/
export function _5fe8cbd0191a1261(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator System.Half(ulong)*/
export function _7cde86a6784147b9(value) {
  return FromBigIntCore(value);
}
/*jazor:clr-member static System.Half.explicit operator byte(System.Half)*/
export function _4eda3983a0238fe6(value) {
  return UncheckedToNarrowCore(value, 8, false);
}
/*jazor:clr-member static System.Half.explicit operator checked byte(System.Half)*/
export function _17127d121cc23462(value) {
  return CheckedToNumberCore(value, 0, 255);
}
/*jazor:clr-member static System.Half.explicit operator char(System.Half)*/
export function _a51addf0541517b0(value) {
  return UncheckedToNarrowCore(value, 16, false);
}
/*jazor:clr-member static System.Half.explicit operator checked char(System.Half)*/
export function _0ce814bef1ddcd6b(value) {
  return CheckedToNumberCore(value, 0, 65535);
}
/*jazor:clr-member static System.Half.explicit operator short(System.Half)*/
export function _f3478913297420e6(value) {
  return UncheckedToNarrowCore(value, 16, true);
}
/*jazor:clr-member static System.Half.explicit operator checked short(System.Half)*/
export function _a97f96a06c928768(value) {
  return CheckedToNumberCore(value, -32768, 32767);
}
/*jazor:clr-member static System.Half.explicit operator int(System.Half)*/
export function _b72c1f59dbe70e00(value) {
  return UncheckedToInt32Core(value);
}
/*jazor:clr-member static System.Half.explicit operator checked int(System.Half)*/
export function _70697b238a197bc2(value) {
  return CheckedToNumberCore(value, -2147483648, 2147483647);
}
/*jazor:clr-member static System.Half.explicit operator long(System.Half)*/
export function _1d590a5b31b1ced4(value) {
  return FromFloatingSaturatingSigned(value, BigInt("-9223372036854775808"), BigInt("9223372036854775807"));
}
/*jazor:clr-member static System.Half.explicit operator checked long(System.Half)*/
export function _b245ca9db3ecb868(value) {
  return FromFloatingChecked(value, BigInt("-9223372036854775808"), BigInt("9223372036854775807"));
}
/*jazor:clr-member static System.Half.explicit operator System.Int128(System.Half)*/
export function _24b890794cafdd5b(value) {
  return FromFloatingSaturatingSigned(value, BigInt("-170141183460469231731687303715884105728"), BigInt("170141183460469231731687303715884105727"));
}
/*jazor:clr-member static System.Half.explicit operator checked System.Int128(System.Half)*/
export function _ad10a10a383b6b8c(value) {
  return FromFloatingChecked(value, BigInt("-170141183460469231731687303715884105728"), BigInt("170141183460469231731687303715884105727"));
}
/*jazor:clr-member static System.Half.explicit operator sbyte(System.Half)*/
export function _0c7451f23f55d772(value) {
  return UncheckedToNarrowCore(value, 8, true);
}
/*jazor:clr-member static System.Half.explicit operator checked sbyte(System.Half)*/
export function _d68498a3229ff278(value) {
  return CheckedToNumberCore(value, -128, 127);
}
/*jazor:clr-member static System.Half.explicit operator ushort(System.Half)*/
export function _5506dadf5b952671(value) {
  return UncheckedToNarrowCore(value, 16, false);
}
/*jazor:clr-member static System.Half.explicit operator checked ushort(System.Half)*/
export function _d7ccb4b5709ce4ea(value) {
  return CheckedToNumberCore(value, 0, 65535);
}
/*jazor:clr-member static System.Half.explicit operator uint(System.Half)*/
export function _6d14496c702de03c(value) {
  return UncheckedToUnsignedNumberCore(value, 4294967295);
}
/*jazor:clr-member static System.Half.explicit operator checked uint(System.Half)*/
export function _8e635ebf316e6be7(value) {
  return CheckedToNumberCore(value, 0, 4294967295);
}
/*jazor:clr-member static System.Half.explicit operator ulong(System.Half)*/
export function _368654d3a116fc21(value) {
  return FromFloatingSaturatingUnsigned(value, BigInt("18446744073709551615"));
}
/*jazor:clr-member static System.Half.explicit operator checked ulong(System.Half)*/
export function _8d52fe89e6ca9452(value) {
  return FromFloatingChecked(value, 0n, BigInt("18446744073709551615"));
}
/*jazor:clr-member static System.Half.explicit operator System.UInt128(System.Half)*/
export function _de1cee73a929bf8e(value) {
  return FromFloatingSaturatingUnsigned(value, BigInt("340282366920938463463374607431768211455"));
}
/*jazor:clr-member static System.Half.explicit operator checked System.UInt128(System.Half)*/
export function _bd3cc1c48165dbab(value) {
  return FromFloatingCheckedUInt128(value, BigInt("340282366920938463463374607431768211455"));
}
/*jazor:clr-member static System.Half.implicit operator System.Half(byte)*/
export function _b5ec2ce7adbc5cd7(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.implicit operator System.Half(sbyte)*/
export function _e9ab5db75451afaa(value) {
  return RoundToHalf(value);
}
/*jazor:clr-member static System.Half.explicit operator double(System.Half)*/
export function _0cce99536d7741bb(value) {
  return value;
}
/*jazor:clr-member static System.Half.explicit operator float(System.Half)*/
export function _e5c3410a6fc7ae9a(value) {
  return value;
}
/*jazor:clr-member static System.Half.IsPow2(System.Half)*/
export function _8b5f0cb98ef4522c(value) {
  return IsPow2Core(value);
}
/*jazor:clr-member static System.Half.Round(System.Half)*/
export function _8654f1427404f736(value) {
  return RoundToEven(value);
}
/*jazor:clr-member static System.Half.Round(System.Half, int)*/
export function _a977225c7ea195c2(x, digits) {
  return RoundToHalf(RoundCore(x, digits, 0));
}
/*jazor:clr-member static System.Half.Round(System.Half, System.MidpointRounding)*/
export function _a3bd625b8647d19e(x, mode) {
  return RoundToHalf(RoundCore(x, 0, mode));
}
/*jazor:clr-member static System.Half.Round(System.Half, int, System.MidpointRounding)*/
export function _df8d144bad4e8a0b(x, digits, mode) {
  return RoundToHalf(RoundCore(x, digits, mode));
}
/*jazor:clr-member static System.Half.BitDecrement(System.Half)*/
export function _c976c1d81370babf(x) {
  return BitDecrementCore(x);
}
/*jazor:clr-member static System.Half.BitIncrement(System.Half)*/
export function _3bbda0fdee7bad1d(x) {
  return BitIncrementCore(x);
}
/*jazor:clr-member static System.Half.Ieee754Remainder(System.Half, System.Half)*/
export function _18006f6446bcf954(left, right) {
  return Ieee754RemainderCore(left, right);
}
/*jazor:clr-member static System.Half.ILogB(System.Half)*/
export function _32ebc25218ce32e0(value) {
  return ILogBCore(value);
}
/*jazor:clr-member static System.Half.Clamp(System.Half, System.Half, System.Half)*/
export function _6335905a4e3a886f(value, min, max) {
  return ClampCore(value, min, max);
}
/*jazor:clr-member static System.Half.ClampNative(System.Half, System.Half, System.Half)*/
export function _de3198267b6b5ced(value, min, max) {
  return ClampNativeCore(value, min, max);
}
/*jazor:clr-member static System.Half.Sign(System.Half)*/
export function _86dc947c6d8aa31a(value) {
  return SignCore(value);
}
/*jazor:clr-member static System.Half.MaxMagnitude(System.Half, System.Half)*/
export function _62245f7092999e63(x, y) {
  return MaxMagnitudeCore(x, y);
}
/*jazor:clr-member static System.Half.MaxMagnitudeNumber(System.Half, System.Half)*/
export function _52991fa82e7974ee(x, y) {
  return MaxMagnitudeNumberCore(x, y);
}
/*jazor:clr-member static System.Half.MinMagnitude(System.Half, System.Half)*/
export function _ceb58186d4c7edf0(x, y) {
  return MinMagnitudeCore(x, y);
}
/*jazor:clr-member static System.Half.MinMagnitudeNumber(System.Half, System.Half)*/
export function _d6bec8db0dff7ab7(x, y) {
  return MinMagnitudeNumberCore(x, y);
}
/*jazor:clr-member static System.Half.TryParse(string, System.IFormatProvider, out System.Half)*/
export function _53367d1aaf68b5df(text, provider, result) {
  return _83de0b9fe4433805(text, result);
}
/*jazor:clr-member static System.Half.RootN(System.Half, int)*/
export function _7d0e51fe4ac37ce8(value, root) {
  return RootNCore(value, root);
}
/*jazor:clr-member static System.Half.SinCos(System.Half)*/
export function _7bdc16d36920d5d9(value) {
  return { Sin: RoundToHalf(Math.sin(value)), Cos: RoundToHalf(Math.cos(value)) };
}
/*jazor:clr-member static System.Half.SinCosPi(System.Half)*/
export function _a1628326328dadd0(value) {
  let angle = value * Math.PI;
  return { SinPi: RoundToHalf(Math.sin(angle)), CosPi: RoundToHalf(Math.cos(angle)) };
}
