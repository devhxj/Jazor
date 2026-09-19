import { _42cbe2ef401fb8c9 } from "System/ByteModule.js";
import { _09ee3a4652dbe73c, _4a816369b59f1ca3, _518facaaeeb29ead, _84028a6e79626057, _872018e11335480a, _a334f7e82122cfc2, _bc3a974d51c694ab, _be8b149ea0e1d76b, _ceb21f954af742e7, _e85678b4de2283e8, _e886400fbfdbdaaa, _ed803cf9c8c052f1 } from "System/DecimalModule.js";
import { BitDecrementCore, BitIncrementCore, ILogBCore, Ieee754RemainderCore, RoundCore, RoundToEvenCore, SignCore } from "System/DoubleModule.js";
import { _8ce36b36c4abd947, _b2c1f15fae072110 } from "System/Int16Module.js";
import { _49bf8261f5cf3a4b, _d4cc9914e60e5643 } from "System/Int32Module.js";
import { _28273cd350760efe, _6ae5b36df368d1e5 } from "System/Int64Module.js";
import { _f0d5d38874458f27 } from "System/SByteModule.js";
import { SignCore as i$13da0788953b6235 } from "System/SingleModule.js";
import { _80e78c0aa0b98fef } from "System/UInt16Module.js";
import { _8a073d758132b5bb } from "System/UInt32Module.js";
import { _fbae7adf5aedb1a5 } from "System/UInt64Module.js";
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
function DivRemSByteCore(left, right) {
  if (right === 0)
    throw new Error("DivideByZeroException");
  if (left === -128 && right === -1)
    throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");
  let quotient = Math.trunc(left / right);
  return { Quotient: quotient, Remainder: left - quotient * right };
}
function BigMulUnsigned64(left, right) {
  let product = left * right;
  let low = BigInt.asUintN(64, product);
  let high = BigInt.asUintN(64, product >> BigInt(64));
  return [high, low];
}
function BigMulSigned64(left, right) {
  let product = left * right;
  let low = BigInt.asIntN(64, product);
  let high = BigInt.asIntN(64, product >> BigInt(64));
  return [high, low];
}
/*jazor:clr-member static System.Math.SinCos(double)*/
export function _4dcadff583296186(x) {
  return { Sin: Math.sin(x), Cos: Math.cos(x) };
}
/*jazor:clr-member static System.Math.Abs(short)*/
export function _81a80e1bfb516bfb(value) {
  return _8ce36b36c4abd947(value);
}
/*jazor:clr-member static System.Math.Abs(int)*/
export function _0aaf1073fc70e405(value) {
  return _49bf8261f5cf3a4b(value);
}
/*jazor:clr-member static System.Math.Abs(long)*/
export function _2f5b0b713dde9501(value) {
  return _6ae5b36df368d1e5(value);
}
/*jazor:clr-member static System.Math.Abs(sbyte)*/
export function _6ed2ee0733ac7051(value) {
  return _f0d5d38874458f27(value);
}
/*jazor:clr-member static System.Math.Abs(decimal)*/
export function _eab3564b2663dff6(value) {
  return _e85678b4de2283e8(value);
}
/*jazor:clr-member static System.Math.BigMul(ulong, ulong, out ulong)*/
export function _99697fddb05f0646(a, b, low) {
  return BigMulUnsigned64(a, b);
}
/*jazor:clr-member static System.Math.BigMul(long, long, out long)*/
export function _1f2b3fb549b0a774(a, b, low) {
  return BigMulSigned64(a, b);
}
/*jazor:clr-member static System.Math.BitDecrement(double)*/
export function _bc28ec82e8385202(x) {
  return BitDecrementCore(x);
}
/*jazor:clr-member static System.Math.BitIncrement(double)*/
export function _655bd4d428ca20ea(x) {
  return BitIncrementCore(x);
}
/*jazor:clr-member static System.Math.DivRem(int, int, out int)*/
export function _2a90cb0f64781864(a, b, result) {
  let pair = _d4cc9914e60e5643(a, b);
  return [pair.Quotient, pair.Remainder];
}
/*jazor:clr-member static System.Math.DivRem(long, long, out long)*/
export function _1961d3558bd76ea4(a, b, result) {
  let pair = _28273cd350760efe(a, b);
  return [pair.Quotient, pair.Remainder];
}
/*jazor:clr-member static System.Math.DivRem(sbyte, sbyte)*/
export function _e0661118fd9ce98d(left, right) {
  return DivRemSByteCore(left, right);
}
/*jazor:clr-member static System.Math.DivRem(byte, byte)*/
export function _09ec2eababe53085(left, right) {
  return _42cbe2ef401fb8c9(left, right);
}
/*jazor:clr-member static System.Math.DivRem(short, short)*/
export function _f6eb115003bc623f(left, right) {
  return _b2c1f15fae072110(left, right);
}
/*jazor:clr-member static System.Math.DivRem(ushort, ushort)*/
export function _267e04d7693208d4(left, right) {
  return _80e78c0aa0b98fef(left, right);
}
/*jazor:clr-member static System.Math.DivRem(int, int)*/
export function _45a4ab35fd8b6be8(left, right) {
  return _d4cc9914e60e5643(left, right);
}
/*jazor:clr-member static System.Math.DivRem(uint, uint)*/
export function _c8e57fe110813408(left, right) {
  return _8a073d758132b5bb(left, right);
}
/*jazor:clr-member static System.Math.DivRem(long, long)*/
export function _96f1b2c20bd2e40b(left, right) {
  return _28273cd350760efe(left, right);
}
/*jazor:clr-member static System.Math.DivRem(ulong, ulong)*/
export function _4d9536a1220a7365(left, right) {
  return _fbae7adf5aedb1a5(left, right);
}
/*jazor:clr-member static System.Math.Ceiling(decimal)*/
export function _84cbc0eaf2d899af(d) {
  return _84028a6e79626057(d);
}
/*jazor:clr-member static System.Math.Clamp(decimal, decimal, decimal)*/
export function _735e24a467fce432(value, min, max) {
  return _e886400fbfdbdaaa(value, min, max);
}
/*jazor:clr-member static System.Math.Floor(decimal)*/
export function _b12193a7b6647a82(d) {
  return _518facaaeeb29ead(d);
}
/*jazor:clr-member static System.Math.IEEERemainder(double, double)*/
export function _288c181b5d9cf968(x, y) {
  return Ieee754RemainderCore(x, y);
}
/*jazor:clr-member static System.Math.ILogB(double)*/
export function _51e4d6005e6e11ef(x) {
  return ILogBCore(x);
}
/*jazor:clr-member static System.Math.Max(decimal, decimal)*/
export function _68326de2fcd99278(val1, val2) {
  return _872018e11335480a(val1, val2);
}
/*jazor:clr-member static System.Math.MaxMagnitude(double, double)*/
export function _7922e74207558715(x, y) {
  return MaxMagnitudeCore(x, y);
}
/*jazor:clr-member static System.Math.Min(decimal, decimal)*/
export function _87f14d6593efd87f(val1, val2) {
  return _ceb21f954af742e7(val1, val2);
}
/*jazor:clr-member static System.Math.MinMagnitude(double, double)*/
export function _44776725ec896ede(x, y) {
  return MinMagnitudeCore(x, y);
}
/*jazor:clr-member static System.Math.Round(decimal)*/
export function _257741f3e4260d82(d) {
  return _4a816369b59f1ca3(d);
}
/*jazor:clr-member static System.Math.Round(decimal, int)*/
export function _10e883cf6d89b70c(d, decimals) {
  return _bc3a974d51c694ab(d, decimals);
}
/*jazor:clr-member static System.Math.Round(decimal, System.MidpointRounding)*/
export function _584a7b2219b578fa(d, mode) {
  return _a334f7e82122cfc2(d, mode);
}
/*jazor:clr-member static System.Math.Round(decimal, int, System.MidpointRounding)*/
export function _b955eff4c2d1fa63(d, decimals, mode) {
  return _09ee3a4652dbe73c(d, decimals, mode);
}
/*jazor:clr-member static System.Math.Round(double)*/
export function _6cd7f67f98eae0bc(a) {
  return RoundToEvenCore(a);
}
/*jazor:clr-member static System.Math.Round(double, int)*/
export function _dab059b61a5b7428(value, digits) {
  return RoundCore(value, digits, 0);
}
/*jazor:clr-member static System.Math.Round(double, System.MidpointRounding)*/
export function _a7f99c51d0db12b5(value, mode) {
  return RoundCore(value, 0, mode);
}
/*jazor:clr-member static System.Math.Round(double, int, System.MidpointRounding)*/
export function _ef441dda2abcc022(value, digits, mode) {
  return RoundCore(value, digits, mode);
}
/*jazor:clr-member static System.Math.Sign(decimal)*/
export function _8d626104a531d041(value) {
  return _ed803cf9c8c052f1(value);
}
/*jazor:clr-member static System.Math.Sign(double)*/
export function _9a554cfca79bdc59(value) {
  return SignCore(value);
}
/*jazor:clr-member static System.Math.Sign(float)*/
export function _c0668680ba7ef96e(value) {
  return i$13da0788953b6235(value);
}
/*jazor:clr-member static System.Math.Truncate(decimal)*/
export function _abd9211e1e7514b4(d) {
  return _be8b149ea0e1d76b(d);
}
