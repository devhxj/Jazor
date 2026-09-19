import { AbsSigned, DivRemSigned, LeadingZeroCount, Log2Signed, Parse, PopCount, RotateLeft, RotateRight, TrailingZeroCount, TryParse } from "System/Numerics/BigIntIntegerRuntime.js";
import { GetInt64HashCode, TryDecodeUtf8 } from "System/RuntimeModule.js";
function get_RotateMask() {
  return BigInt("18446744073709551615");
}
function get_RotateModulus() {
  return BigInt("18446744073709551616");
}
function get_RotateSignBit() {
  return BigInt("9223372036854775808");
}
function get_MinValueCore() {
  return BigInt("-9223372036854775808");
}
/*jazor:clr-member long.CompareTo(object)*/
export function _a108636b79b7c8d2(instance, value) {
  let bigIntValue;
  if (value === null)
    return 1;
  if (typeof value === "bigint" && (bigIntValue = value, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type Int64.");
}
/*jazor:clr-member override long.GetHashCode()*/
export function _a6f06b90e3618c16(instance) {
  return GetInt64HashCode(instance);
}
/*jazor:clr-member static long.Parse(string)*/
export function _4174bb5b72e448a6(s) {
  return Parse(s, get_MinValueCore(), BigInt("9223372036854775807"), "Int64");
}
/*jazor:clr-member static long.TryParse(string, out long)*/
export function _2cba636c245c1675(s, result) {
  return TryParse(s, get_MinValueCore(), BigInt("9223372036854775807"));
}
/*jazor:clr-member static long.TryParse(System.ReadOnlySpan<char>, out long)*/
export function _f65dcae3cb8d9ffc(s, result) {
  return _2cba636c245c1675(s, result);
}
/*jazor:clr-member static long.TryParse(System.ReadOnlySpan<byte>, out long)*/
export function _8bee07df79eb3a90(utf8Text, result) {
  return _2cba636c245c1675(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static long.DivRem(long, long)*/
export function _28273cd350760efe(left, right) {
  return DivRemSigned(left, right, get_MinValueCore());
}
/*jazor:clr-member static long.LeadingZeroCount(long)*/
export function _f67b17bf5c4120f2(value) {
  return LeadingZeroCount(value, 64, get_RotateMask());
}
/*jazor:clr-member static long.PopCount(long)*/
export function _77fd605bbb6ce669(value) {
  return PopCount(value, get_RotateMask());
}
/*jazor:clr-member static long.RotateLeft(long, int)*/
export function _62ef461b6a515b85(value, rotateAmount) {
  return RotateLeft(value, rotateAmount, 64, get_RotateMask(), get_RotateModulus(), get_RotateSignBit(), true);
}
/*jazor:clr-member static long.RotateRight(long, int)*/
export function _6a70bc88f689ce73(value, rotateAmount) {
  return RotateRight(value, rotateAmount, 64, get_RotateMask(), get_RotateModulus(), get_RotateSignBit(), true);
}
/*jazor:clr-member static long.TrailingZeroCount(long)*/
export function _df6d7288bc845b53(value) {
  return TrailingZeroCount(value, 64, get_RotateMask());
}
/*jazor:clr-member static long.Log2(long)*/
export function _e90fc1096a04c8f9(value) {
  return Log2Signed(value);
}
/*jazor:clr-member static long.Abs(long)*/
export function _6ae5b36df368d1e5(value) {
  return AbsSigned(value, get_MinValueCore());
}
/*jazor:clr-member static long.MaxMagnitude(long, long)*/
export function _9618dc0d855ee729(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
/*jazor:clr-member static long.MinMagnitude(long, long)*/
export function _bfad1ee52075b36e(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return x < y ? x : y;
}
