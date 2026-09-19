import { FromBigIntCore } from "System/HalfModule.js";
import { BigMulUnsigned, Clamp, CompareToObject, DivRemUnsigned, DivideUnsigned, EnsureRange, FromDecimal, FromFloatingCheckedUInt128, FromFloatingSaturatingUnsigned, LeadingZeroCount, Log10, Parse, PopCount, RemainderUnsigned, RotateLeft, RotateRight, ToCheckedNumber, ToDecimal, TrailingZeroCount, TryParse } from "System/Numerics/BigIntIntegerRuntime.js";
import { GetInt128HashCode, TryDecodeUtf8 } from "System/RuntimeModule.js";
function get_MinValueCore() {
  return 0n;
}
function get_MaxValueCore() {
  return BigInt("340282366920938463463374607431768211455");
}
function get_Mask() {
  return get_MaxValueCore();
}
function get_Modulus() {
  return BigInt("340282366920938463463374607431768211456");
}
function get_Int128MaxValue() {
  return BigInt("170141183460469231731687303715884105727");
}
function get_DecimalMaxValue() {
  return BigInt("79228162514264337593543950335");
}
/*jazor:clr-member System.UInt128.CompareTo(object)*/
export function _c1dc559553950096(instance, value) {
  return CompareToObject(instance, value, "UInt128");
}
/*jazor:clr-member override System.UInt128.GetHashCode()*/
export function _bd5a3a9523f573e7(instance) {
  return GetInt128HashCode(instance);
}
/*jazor:clr-member static System.UInt128.Parse(string)*/
export function _30fed79ec71cc7e4(text) {
  return Parse(text, get_MinValueCore(), get_MaxValueCore(), "UInt128");
}
/*jazor:clr-member static System.UInt128.Parse(string, System.IFormatProvider)*/
export function _6d4342f227a4fbad(text, provider) {
  return _30fed79ec71cc7e4(text);
}
/*jazor:clr-member static System.UInt128.TryParse(string, out System.UInt128)*/
export function _8845ce18c94ffbb4(text, result) {
  return TryParse(text, get_MinValueCore(), get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.TryParse(System.ReadOnlySpan<char>, out System.UInt128)*/
export function _4d3bd14dc2810a3c(text, result) {
  return TryParse(text, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.TryParse(System.ReadOnlySpan<byte>, out System.UInt128)*/
export function _6b11c1fbc39c3749(utf8Text, result) {
  return TryParse(TryDecodeUtf8(utf8Text), 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked byte(System.UInt128)*/
export function _64e60de5b1e03760(value) {
  return ToCheckedNumber(value, 0n, BigInt(255));
}
/*jazor:clr-member static System.UInt128.explicit operator checked char(System.UInt128)*/
export function _b68867a4bbf792ed(value) {
  return ToCheckedNumber(value, 0n, BigInt(65535));
}
/*jazor:clr-member static System.UInt128.explicit operator decimal(System.UInt128)*/
export function _cfc7a729e04a71ab(value) {
  return ToDecimal(value, 0n, get_DecimalMaxValue());
}
/*jazor:clr-member static System.UInt128.explicit operator System.Half(System.UInt128)*/
export function _ebc69a5a022fe3e9(value) {
  return FromBigIntCore(value);
}
/*jazor:clr-member static System.UInt128.explicit operator checked short(System.UInt128)*/
export function _5efef087d1235b8b(value) {
  return ToCheckedNumber(value, 0n, BigInt(32767));
}
/*jazor:clr-member static System.UInt128.explicit operator checked int(System.UInt128)*/
export function _ab4813fe5941ad49(value) {
  return ToCheckedNumber(value, 0n, BigInt(2147483647));
}
/*jazor:clr-member static System.UInt128.explicit operator checked long(System.UInt128)*/
export function _191ebf43930db2a5(value) {
  return EnsureRange(value, 0n, BigInt("9223372036854775807"));
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.Int128(System.UInt128)*/
export function _c572f7b29eaf324c(value) {
  return EnsureRange(value, 0n, get_Int128MaxValue());
}
/*jazor:clr-member static System.UInt128.explicit operator checked sbyte(System.UInt128)*/
export function _95c576d9e4841566(value) {
  return ToCheckedNumber(value, 0n, BigInt(127));
}
/*jazor:clr-member static System.UInt128.explicit operator checked ushort(System.UInt128)*/
export function _b68ba902309cfb9a(value) {
  return ToCheckedNumber(value, 0n, BigInt(65535));
}
/*jazor:clr-member static System.UInt128.explicit operator checked uint(System.UInt128)*/
export function _4b86a17a8f47b33f(value) {
  return ToCheckedNumber(value, 0n, BigInt("4294967295"));
}
/*jazor:clr-member static System.UInt128.explicit operator checked ulong(System.UInt128)*/
export function _b7d11ef0703deabf(value) {
  return EnsureRange(value, 0n, BigInt("18446744073709551615"));
}
/*jazor:clr-member static System.UInt128.explicit operator System.UInt128(decimal)*/
export function _7a73b169cb4a8694(value) {
  return FromDecimal(value, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator System.UInt128(double)*/
export function _8a2ad347ec233b35(value) {
  return FromFloatingSaturatingUnsigned(value, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.UInt128(double)*/
export function _5d464c2acf139edb(value) {
  return FromFloatingCheckedUInt128(value, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.UInt128(short)*/
export function _958e84ffc74ece86(value) {
  return EnsureRange(BigInt(value), 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.UInt128(int)*/
export function _06d213d11ddf681c(value) {
  return EnsureRange(BigInt(value), 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.UInt128(long)*/
export function _1ef649fc443738a2(value) {
  return EnsureRange(value, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.UInt128(sbyte)*/
export function _8366585a071ba8b1(value) {
  return EnsureRange(BigInt(value), 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator System.UInt128(float)*/
export function _5ac67fecfe01fee0(value) {
  return FromFloatingSaturatingUnsigned(value, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.explicit operator checked System.UInt128(float)*/
export function _dec2fe2225e51e70(value) {
  return FromFloatingCheckedUInt128(value, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.operator checked +(System.UInt128, System.UInt128)*/
export function _c754a5da22221b5c(left, right) {
  return EnsureRange(left + right, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.DivRem(System.UInt128, System.UInt128)*/
export function _8796a5402e48210c(left, right) {
  return DivRemUnsigned(left, right);
}
/*jazor:clr-member static System.UInt128.LeadingZeroCount(System.UInt128)*/
export function _76106db43126b9b5(value) {
  return LeadingZeroCount(value, 128, get_Mask());
}
/*jazor:clr-member static System.UInt128.Log10(System.UInt128)*/
export function _4ae42163ca5ab057(value) {
  return Log10(value);
}
/*jazor:clr-member static System.UInt128.PopCount(System.UInt128)*/
export function _e60df5c8bf2adf5c(value) {
  return PopCount(value, get_Mask());
}
/*jazor:clr-member static System.UInt128.RotateLeft(System.UInt128, int)*/
export function _d743d2ddded2abe5(value, rotateAmount) {
  return RotateLeft(value, rotateAmount, 128, get_Mask(), get_Modulus(), 0n, false);
}
/*jazor:clr-member static System.UInt128.RotateRight(System.UInt128, int)*/
export function _a2bab5c9eaffb253(value, rotateAmount) {
  return RotateRight(value, rotateAmount, 128, get_Mask(), get_Modulus(), 0n, false);
}
/*jazor:clr-member static System.UInt128.TrailingZeroCount(System.UInt128)*/
export function _f5f31da639f5ea89(value) {
  return TrailingZeroCount(value, 128, get_Mask());
}
/*jazor:clr-member static System.UInt128.operator checked --(System.UInt128)*/
export function _2570268944e834ba(value) {
  return EnsureRange(value - 1n, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.operator /(System.UInt128, System.UInt128)*/
export function _30e28339559d8888(left, right) {
  return DivideUnsigned(left, right);
}
/*jazor:clr-member static System.UInt128.operator checked /(System.UInt128, System.UInt128)*/
export function _b0d1618f64eba0cd(left, right) {
  return DivideUnsigned(left, right);
}
/*jazor:clr-member static System.UInt128.operator checked ++(System.UInt128)*/
export function _cf08bccf56129f82(value) {
  return EnsureRange(value + 1n, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.operator %(System.UInt128, System.UInt128)*/
export function _4541585272909795(left, right) {
  return RemainderUnsigned(left, right);
}
/*jazor:clr-member static System.UInt128.operator checked *(System.UInt128, System.UInt128)*/
export function _7b7dc120501d3144(left, right) {
  return EnsureRange(left * right, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.BigMul(System.UInt128, System.UInt128, out System.UInt128)*/
export function _08f69578289009db(left, right, lower) {
  return BigMulUnsigned(left, right, 128);
}
/*jazor:clr-member static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)*/
export function _a545c5c1dd9b956a(value, min, max) {
  return Clamp(value, min, max);
}
/*jazor:clr-member static System.UInt128.TryParse(string, System.IFormatProvider, out System.UInt128)*/
export function _201a443b1608c214(text, provider, result) {
  return _8845ce18c94ffbb4(text, result);
}
/*jazor:clr-member static System.UInt128.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _c88639ae1d5401bd(text, provider) {
  return Parse(text, 0n, get_MaxValueCore(), "UInt128");
}
/*jazor:clr-member static System.UInt128.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.UInt128)*/
export function _76b9708fc50ff818(text, provider, result) {
  return TryParse(text, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.operator checked -(System.UInt128, System.UInt128)*/
export function _9b4d82822297f055(left, right) {
  return EnsureRange(left - right, 0n, get_MaxValueCore());
}
/*jazor:clr-member static System.UInt128.operator checked -(System.UInt128)*/
export function _86264fa0bd6d25be(value) {
  return EnsureRange(-value, 0n, get_MaxValueCore());
}
