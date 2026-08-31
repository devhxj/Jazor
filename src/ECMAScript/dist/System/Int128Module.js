import { FromBigIntCore } from "System/HalfModule.js";
import { AbsSigned, BigMulSigned, Clamp, CompareToObject, CopySignSigned, DivRemSigned, DivideSigned, EnsureRange, FromDecimal, FromFloatingChecked, FromFloatingSaturatingSigned, LeadingZeroCount, Log10, Log2Signed, MaxMagnitude, MinMagnitude, Parse, PopCount, RemainderSigned, RotateLeft, RotateRight, ToCheckedNumber, ToDecimal, TrailingZeroCount, TryParse } from "System/Numerics/BigIntIntegerRuntime.js";
import { GetInt128HashCode, TryDecodeUtf8 } from "System/RuntimeModule.js";
function get_MinValueCore() {
  return BigInt("-170141183460469231731687303715884105728");
}
function get_MaxValueCore() {
  return BigInt("170141183460469231731687303715884105727");
}
function get_Mask() {
  return BigInt("340282366920938463463374607431768211455");
}
function get_Modulus() {
  return BigInt("340282366920938463463374607431768211456");
}
function get_SignBit() {
  return BigInt("170141183460469231731687303715884105728");
}
function get_DecimalMinValue() {
  return BigInt("-79228162514264337593543950335");
}
function get_DecimalMaxValue() {
  return BigInt("79228162514264337593543950335");
}
/*jazor:clr-member System.Int128.CompareTo(object)*/
export function _b7fcdacf2f88dea3(instance, value) {
  return CompareToObject(instance, value, "Int128");
}
/*jazor:clr-member override System.Int128.GetHashCode()*/
export function _2de13ea6377940aa(instance) {
  return GetInt128HashCode(instance);
}
/*jazor:clr-member static System.Int128.Parse(string)*/
export function _e6ba6fd0fe70ed44(text) {
  return Parse(text, get_MinValueCore(), get_MaxValueCore(), "Int128");
}
/*jazor:clr-member static System.Int128.Parse(string, System.IFormatProvider)*/
export function _1a9c00a8ce01999f(text, provider) {
  return _e6ba6fd0fe70ed44(text);
}
/*jazor:clr-member static System.Int128.TryParse(string, out System.Int128)*/
export function _14ac4f353ddae82c(text, result) {
  return TryParse(text, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.TryParse(System.ReadOnlySpan<char>, out System.Int128)*/
export function _b0e356aabfe72ec2(text, result) {
  return TryParse(text, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.TryParse(System.ReadOnlySpan<byte>, out System.Int128)*/
export function _b5211e33c4db2da9(utf8Text, result) {
  return TryParse(TryDecodeUtf8(utf8Text), get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.explicit operator checked byte(System.Int128)*/
export function _75b77707d8797fe4(value) {
  return ToCheckedNumber(value, 0n, BigInt(255));
}
/*jazor:clr-member static System.Int128.explicit operator checked char(System.Int128)*/
export function _f452363cdf448dd6(value) {
  return ToCheckedNumber(value, 0n, BigInt(65535));
}
/*jazor:clr-member static System.Int128.explicit operator decimal(System.Int128)*/
export function _9e21259a765be818(value) {
  return ToDecimal(value, get_DecimalMinValue(), get_DecimalMaxValue());
}
/*jazor:clr-member static System.Int128.explicit operator System.Half(System.Int128)*/
export function _53c418af5874ca57(value) {
  return FromBigIntCore(value);
}
/*jazor:clr-member static System.Int128.explicit operator checked short(System.Int128)*/
export function _2f789a7c53d14d8c(value) {
  return ToCheckedNumber(value, BigInt(-32768), BigInt(32767));
}
/*jazor:clr-member static System.Int128.explicit operator checked int(System.Int128)*/
export function _93c11f1447efb175(value) {
  return ToCheckedNumber(value, BigInt(-2147483648), BigInt(2147483647));
}
/*jazor:clr-member static System.Int128.explicit operator checked long(System.Int128)*/
export function _4d6353a3d3f19b88(value) {
  return EnsureRange(value, BigInt("-9223372036854775808"), BigInt("9223372036854775807"));
}
/*jazor:clr-member static System.Int128.explicit operator checked sbyte(System.Int128)*/
export function _d08bfb41d3ab6ee2(value) {
  return ToCheckedNumber(value, BigInt(-128), BigInt(127));
}
/*jazor:clr-member static System.Int128.explicit operator checked ushort(System.Int128)*/
export function _304df15d6a44df74(value) {
  return ToCheckedNumber(value, 0n, BigInt(65535));
}
/*jazor:clr-member static System.Int128.explicit operator checked uint(System.Int128)*/
export function _0ad5d1d4d4f5f677(value) {
  return ToCheckedNumber(value, 0n, BigInt("4294967295"));
}
/*jazor:clr-member static System.Int128.explicit operator checked ulong(System.Int128)*/
export function _0c7f2cd86870d034(value) {
  return EnsureRange(value, 0n, BigInt("18446744073709551615"));
}
/*jazor:clr-member static System.Int128.explicit operator checked System.UInt128(System.Int128)*/
export function _d9f967e451f57e1b(value) {
  return EnsureRange(value, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.explicit operator System.Int128(decimal)*/
export function _ee13322cacfa030d(value) {
  return FromDecimal(value, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.explicit operator System.Int128(double)*/
export function _fed29180182d65ba(value) {
  return FromFloatingSaturatingSigned(value, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.explicit operator checked System.Int128(double)*/
export function _3d7c10f4becbee0b(value) {
  return FromFloatingChecked(value, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.explicit operator System.Int128(float)*/
export function _f0c48afd1cde425d(value) {
  return FromFloatingSaturatingSigned(value, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.explicit operator checked System.Int128(float)*/
export function _1215d60b3aeb2477(value) {
  return FromFloatingChecked(value, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.operator checked +(System.Int128, System.Int128)*/
export function _5e6d45782cb5e4a5(left, right) {
  return EnsureRange(left + right, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.DivRem(System.Int128, System.Int128)*/
export function _ca96ebfbc2a38481(left, right) {
  return DivRemSigned(left, right, get_MinValueCore());
}
/*jazor:clr-member static System.Int128.LeadingZeroCount(System.Int128)*/
export function _d295dfd29150ae75(value) {
  return LeadingZeroCount(value, 128, get_Mask());
}
/*jazor:clr-member static System.Int128.Log10(System.Int128)*/
export function _f729da8a5282b658(value) {
  return Log10(value);
}
/*jazor:clr-member static System.Int128.PopCount(System.Int128)*/
export function _9d72e9332fd24f23(value) {
  return PopCount(value, get_Mask());
}
/*jazor:clr-member static System.Int128.RotateLeft(System.Int128, int)*/
export function _d432cd8596dae24f(value, rotateAmount) {
  return RotateLeft(value, rotateAmount, 128, get_Mask(), get_Modulus(), get_SignBit(), true);
}
/*jazor:clr-member static System.Int128.RotateRight(System.Int128, int)*/
export function _7adeb1315b95c346(value, rotateAmount) {
  return RotateRight(value, rotateAmount, 128, get_Mask(), get_Modulus(), get_SignBit(), true);
}
/*jazor:clr-member static System.Int128.TrailingZeroCount(System.Int128)*/
export function _7257dc92fb1e4c4c(value) {
  return TrailingZeroCount(value, 128, get_Mask());
}
/*jazor:clr-member static System.Int128.Log2(System.Int128)*/
export function _f1a059f528650ba2(value) {
  return Log2Signed(value);
}
/*jazor:clr-member static System.Int128.operator checked --(System.Int128)*/
export function _1b31f1ebb654733d(value) {
  return EnsureRange(value - 1n, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.operator /(System.Int128, System.Int128)*/
export function _6357de67d5760485(left, right) {
  return DivideSigned(left, right, get_MinValueCore());
}
/*jazor:clr-member static System.Int128.operator checked /(System.Int128, System.Int128)*/
export function _830753b6d4a84cc4(left, right) {
  return DivideSigned(left, right, get_MinValueCore());
}
/*jazor:clr-member static System.Int128.operator checked ++(System.Int128)*/
export function _6dacb4c587ca3df1(value) {
  return EnsureRange(value + 1n, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.operator %(System.Int128, System.Int128)*/
export function _6521eedba51d7990(left, right) {
  return RemainderSigned(left, right, get_MinValueCore());
}
/*jazor:clr-member static System.Int128.operator checked *(System.Int128, System.Int128)*/
export function _056e8fba577b7eeb(left, right) {
  return EnsureRange(left * right, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.BigMul(System.Int128, System.Int128, out System.Int128)*/
export function _d32138c04ddcda2e(left, right, lower) {
  return BigMulSigned(left, right, 128);
}
/*jazor:clr-member static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)*/
export function _587401c79d5e216e(value, min, max) {
  return Clamp(value, min, max);
}
/*jazor:clr-member static System.Int128.CopySign(System.Int128, System.Int128)*/
export function _2f2f3fb10237971f(value, sign) {
  return CopySignSigned(value, sign, get_MinValueCore());
}
/*jazor:clr-member static System.Int128.Abs(System.Int128)*/
export function _bc93f10cc4270d3d(value) {
  return AbsSigned(value, get_MinValueCore());
}
/*jazor:clr-member static System.Int128.MaxMagnitude(System.Int128, System.Int128)*/
export function _829ea04f38a9820e(x, y) {
  return MaxMagnitude(x, y);
}
/*jazor:clr-member static System.Int128.MinMagnitude(System.Int128, System.Int128)*/
export function _ef5bdd18c3a981cf(x, y) {
  return MinMagnitude(x, y);
}
/*jazor:clr-member static System.Int128.TryParse(string, System.IFormatProvider, out System.Int128)*/
export function _c829bcba6a9b9105(text, provider, result) {
  return _14ac4f353ddae82c(text, result);
}
/*jazor:clr-member static System.Int128.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _4d90655f04c3cb26(text, provider) {
  return Parse(text, get_MinValueCore(), get_MaxValueCore(), "Int128");
}
/*jazor:clr-member static System.Int128.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Int128)*/
export function _18dfb394fe14fa70(text, provider, result) {
  return TryParse(text, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.operator checked -(System.Int128, System.Int128)*/
export function _bce2a2f696e0d716(left, right) {
  return EnsureRange(left - right, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.Int128.operator checked -(System.Int128)*/
export function _9f88084238b2cecc(value) {
  return EnsureRange(-value, get_MinValueCore(), get_MaxValueCore());
}
