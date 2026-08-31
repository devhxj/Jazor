import { DivRemUnsigned, LeadingZeroCount, Parse, PopCount, RotateLeft, RotateRight, TrailingZeroCount, TryParse } from "System/Numerics/BigIntIntegerRuntime.js";
import { GetInt64HashCode, TryDecodeUtf8 } from "System/RuntimeModule.js";
function get_Mask() {
  return BigInt("18446744073709551615");
}
function get_Modulus() {
  return BigInt("18446744073709551616");
}
function get_MaxValueCore() {
  return BigInt("18446744073709551615");
}
/*jazor:clr-member ulong.CompareTo(object)*/
export function _b50ba86b85d8ac33(instance, value) {
  let bigIntValue;
  if (value === null)
    return 1;
  if (typeof value === "bigint" && (bigIntValue = value, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type UInt64.");
}
/*jazor:clr-member override ulong.GetHashCode()*/
export function _19d2adbbe01a8cf8(instance) {
  return GetInt64HashCode(instance);
}
/*jazor:clr-member static ulong.Parse(string)*/
export function _ab08b15d1ba56047(s) {
  return Parse(s, 0n, get_MaxValueCore(), "UInt64");
}
/*jazor:clr-member static ulong.TryParse(string, out ulong)*/
export function _a2771534d71206bd(s, result) {
  return TryParse(s, 0n, get_MaxValueCore());
}
/*jazor:clr-member static ulong.TryParse(System.ReadOnlySpan<char>, out ulong)*/
export function _6563986efd5413c0(s, result) {
  return _a2771534d71206bd(s, result);
}
/*jazor:clr-member static ulong.TryParse(System.ReadOnlySpan<byte>, out ulong)*/
export function _908c702d612b8a82(utf8Text, result) {
  return _a2771534d71206bd(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static ulong.DivRem(ulong, ulong)*/
export function _fbae7adf5aedb1a5(left, right) {
  return DivRemUnsigned(left, right);
}
/*jazor:clr-member static ulong.LeadingZeroCount(ulong)*/
export function _cc30bd61ff8ae745(value) {
  return LeadingZeroCount(value, 64, get_Mask());
}
/*jazor:clr-member static ulong.PopCount(ulong)*/
export function _c09e2e8cf64d343e(value) {
  return PopCount(value, get_Mask());
}
/*jazor:clr-member static ulong.RotateLeft(ulong, int)*/
export function _642261af29c95cb4(value, rotateAmount) {
  return RotateLeft(value, rotateAmount, 64, get_Mask(), get_Modulus(), 0n, false);
}
/*jazor:clr-member static ulong.RotateRight(ulong, int)*/
export function _1a784d80426cfa87(value, rotateAmount) {
  return RotateRight(value, rotateAmount, 64, get_Mask(), get_Modulus(), 0n, false);
}
/*jazor:clr-member static ulong.TrailingZeroCount(ulong)*/
export function _bb2bc7ee16cb0d6d(value) {
  return TrailingZeroCount(value, 64, get_Mask());
}
