import { IsFiniteCore, IsNaNCore } from "System/DoubleModule.js";
import { GetInt64HashCode, JTimeSpan, Pad2, Pad7 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function get_TicksPerMicrosecond() {
  return BigInt("10");
}
function get_TicksPerMillisecond() {
  return BigInt("10000");
}
function get_TicksPerSecond() {
  return BigInt("10000000");
}
function get_TicksPerMinute() {
  return BigInt("600000000");
}
function get_TicksPerHour() {
  return BigInt("36000000000");
}
function get_TicksPerDay() {
  return BigInt("864000000000");
}
function get_MaxTimeSpanTicks() {
  return BigInt("9223372036854775807");
}
function get_MinTimeSpanTicks() {
  return BigInt("-9223372036854775808");
}
function get_MaxTimeSpanTicksAsDouble() {
  return 9.223372036854776E+18;
}
function get_MinTimeSpanTicksAsDouble() {
  return -9.223372036854776E+18;
}
function IsAsciiDigit(value) {
  return value.charCodeAt(0) >= "0".charCodeAt(0) && value.charCodeAt(0) <= "9".charCodeAt(0);
}
function IsDigits(text) {
  if (text.length === 0)
    return false;
  for (let i = 0; i < text.length; i++) {
    if (!IsAsciiDigit(_5ad63706a889c294(text, i)))
      return false;
  }
  return true;
}
function EnsureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function ToWholeBigInt(value, message) {
  EnsureWholeNumber(value, message);
  return BigInt(value);
}
function NegateChecked(instance) {
  if (instance.ticks === get_MinTimeSpanTicks())
    throw new Error("OverflowException: Negating the minimum TimeSpan value is invalid.");
  return new JTimeSpan(-instance.ticks);
}
function RoundToEven(value) {
  let truncated = Math.trunc(value);
  let difference = value - truncated;
  if (difference > 0.5)
    return truncated + 1;
  if (difference < -0.5)
    return truncated - 1;
  if (difference > -0.5 && difference < 0.5)
    return truncated;
  return (BigInt(Math.abs(truncated)) & 1n) === 1n ? difference > 0 ? truncated + 1 : truncated - 1 : truncated;
}
function CreateFromTruncatedTicks(value) {
  if (IsNaNCore(value))
    throw new Error("ArgumentException: TimeSpan value cannot be NaN.");
  if (!IsFiniteCore(value))
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  if (value > get_MaxTimeSpanTicksAsDouble() || value < get_MinTimeSpanTicksAsDouble())
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  if (value === get_MaxTimeSpanTicksAsDouble())
    return new JTimeSpan(get_MaxTimeSpanTicks());
  return new JTimeSpan(BigInt(Math.trunc(value)));
}
function CreateFromRoundedTicks(value) {
  if (IsNaNCore(value))
    throw new Error("ArgumentException: TimeSpan value cannot be NaN.");
  if (!IsFiniteCore(value))
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  let rounded = RoundToEven(value);
  if (rounded > get_MaxTimeSpanTicksAsDouble() || rounded < get_MinTimeSpanTicksAsDouble())
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  if (rounded === get_MaxTimeSpanTicksAsDouble())
    return new JTimeSpan(get_MaxTimeSpanTicks());
  return new JTimeSpan(BigInt(rounded));
}
function GetFiniteDoubleRatio(value) {
  let buffer = new ArrayBuffer(8);
  let view = new DataView(buffer);
  view.setFloat64(0, value, false);
  let high = view.getUint32(0, false);
  let low = view.getUint32(4, false);
  let sign = high >= 2147483648 ? -1 : 1;
  let exponentBits = Math.floor(high / 1048576) % 2048;
  let mantissa = BigInt(high % 1048576) << BigInt(32) | BigInt(low);
  let significand;
  let exponent;
  if (exponentBits === 0) {
    significand = mantissa;
    exponent = -1074;
  }
  else {
    significand = 1n << BigInt(52) | mantissa;
    exponent = exponentBits - 1075;
  }
  if (sign < 0)
    significand = -significand;
  if (exponent >= 0)
    return [significand << BigInt(exponent), 1n];
  return [significand, 1n << BigInt(-exponent)];
}
function CreateFromRoundedRationalTicks(numerator, denominator) {
  if (denominator <= 0n)
    throw new Error("ArgumentException: Denominator must be positive.");
  let isNegative = numerator < 0n;
  let magnitude = isNegative ? -numerator : numerator;
  let quotient = magnitude / denominator;
  let remainder = magnitude % denominator;
  let doubledRemainder = remainder << 1n;
  if (doubledRemainder > denominator || doubledRemainder === denominator && (quotient & 1n) === 1n)
    quotient += 1n;
  let rounded = isNegative ? -quotient : quotient;
  if (rounded > get_MaxTimeSpanTicks() || rounded < get_MinTimeSpanTicks())
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  return new JTimeSpan(rounded);
}
function Create_624c8896c26af7db(days, hours, minutes, seconds, milliseconds, microseconds) {
  let ticks = days * get_TicksPerDay() + hours * get_TicksPerHour() + minutes * get_TicksPerMinute() + seconds * get_TicksPerSecond() + milliseconds * get_TicksPerMillisecond() + microseconds * get_TicksPerMicrosecond();
  return new JTimeSpan(ticks);
}
function Create_0d902f9f2178216f(days, hours, minutes, seconds, milliseconds, microseconds) {
  return Create_624c8896c26af7db(ToWholeBigInt(days, "ArgumentOutOfRangeException: Days must be a whole number."), ToWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), ToWholeBigInt(minutes, "ArgumentOutOfRangeException: Minutes must be a whole number."), ToWholeBigInt(seconds, "ArgumentOutOfRangeException: Seconds must be a whole number."), ToWholeBigInt(milliseconds, "ArgumentOutOfRangeException: Milliseconds must be a whole number."), ToWholeBigInt(microseconds, "ArgumentOutOfRangeException: Microseconds must be a whole number."));
}
function MultiplyByDouble(instance, factor) {
  if (IsNaNCore(factor) || !IsFiniteCore(factor))
    return CreateFromRoundedTicks(Number(instance.ticks) * factor);
  let ratio = GetFiniteDoubleRatio(factor);
  return CreateFromRoundedRationalTicks(instance.ticks * ratio[0], ratio[1]);
}
function DivideByDouble(instance, divisor) {
  if (IsNaNCore(divisor) || !IsFiniteCore(divisor) || divisor === 0)
    return CreateFromRoundedTicks(Number(instance.ticks) / divisor);
  let ratio = GetFiniteDoubleRatio(divisor);
  let numerator = instance.ticks * ratio[1];
  let denominator = ratio[0];
  if (denominator < 0n) {
    numerator = -numerator;
    denominator = -denominator;
  }
  return CreateFromRoundedRationalTicks(numerator, denominator);
}
function ParseCore(input) {
  let s = input.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid TimeSpan.");
  let negative = false;
  if (_5ad63706a889c294(s, 0).charCodeAt(0) === "+".charCodeAt(0) || _5ad63706a889c294(s, 0).charCodeAt(0) === "-".charCodeAt(0)) {
    negative = _5ad63706a889c294(s, 0).charCodeAt(0) === "-".charCodeAt(0);
    s = s.substring(1);
  }
  let firstColon = s.indexOf(":");
  if (firstColon < 0)
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  let lastColon = s.lastIndexOf(":");
  let prefix = s.substring(0, 0 + firstColon);
  let daySeparator = prefix.indexOf(".");
  let hasDays = daySeparator >= 0;
  let dayText = "0";
  let hourText = prefix;
  if (hasDays) {
    dayText = prefix.substring(0, 0 + daySeparator);
    hourText = prefix.substring(daySeparator + 1);
  }
  let minuteText = lastColon === firstColon ? s.substring(firstColon + 1) : ((__jz_arg0, __jz_arg1, __jz_arg2) => __jz_arg0.substring(__jz_arg1, __jz_arg1 + __jz_arg2))(s, firstColon + 1, lastColon - firstColon - 1);
  let secondText = "0";
  let fractionText = "";
  if (lastColon !== firstColon) {
    let tail = s.substring(lastColon + 1);
    let fractionSeparator = tail.indexOf(".");
    if (fractionSeparator < 0) {
      secondText = tail;
    }
    else {
      secondText = tail.substring(0, 0 + fractionSeparator);
      fractionText = tail.substring(fractionSeparator + 1);
    }
  }
  else {
    let fractionSeparator = minuteText.indexOf(".");
    if (fractionSeparator >= 0)
      throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  }
  if (!IsDigits(dayText) || !IsDigits(hourText) || !IsDigits(minuteText) || !IsDigits(secondText))
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  let days = Number(dayText);
  let hours = Number(hourText);
  let minutes = Number(minuteText);
  let seconds = Number(secondText);
  if (isNaN(days) || isNaN(hours) || isNaN(minutes) || isNaN(seconds))
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  if (days < 0 || hours < 0 || minutes < 0 || seconds < 0)
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  if (hasDays && hours > 23)
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  if (minutes > 59 || seconds > 59)
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
  let fractionTicks = 0n;
  if (fractionText.length > 0) {
    if (fractionText.length > 7 || !IsDigits(fractionText))
      throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid TimeSpan.`);
    while (fractionText.length < 7)
      fractionText += "0";
    fractionTicks = BigInt(fractionText);
  }
  let totalTicks = BigInt(days) * get_TicksPerDay() + BigInt(hours) * get_TicksPerHour() + BigInt(minutes) * get_TicksPerMinute() + BigInt(seconds) * get_TicksPerSecond() + fractionTicks;
  return new JTimeSpan(negative ? -totalTicks : totalTicks);
}
function FormatCore(instance, format) {
  if (format === null || format.length === 0 || format === "c")
    return instance.toString();
  if (format !== "g" && format !== "G")
    return instance.toString();
  let negative = instance.ticks < 0n;
  let absolute = negative ? -instance.ticks : instance.ticks;
  let days = absolute / get_TicksPerDay();
  let hours = Number(absolute / get_TicksPerHour() % BigInt(24));
  let minutes = Number(absolute / get_TicksPerMinute() % BigInt(60));
  let seconds = Number(absolute / get_TicksPerSecond() % BigInt(60));
  let fraction = absolute % get_TicksPerSecond();
  let text = negative ? "-" : "";
  if (format === "G") {
    return text + days.toString() + ":" + Pad2(hours) + ":" + Pad2(minutes) + ":" + Pad2(seconds) + "." + Pad7(fraction);
  }
  if (days > 0n)
    text += days.toString() + ":";
  text += hours.toString() + ":" + Pad2(minutes) + ":" + Pad2(seconds);
  if (fraction === 0n)
    return text;
  let fractionText = Pad7(fraction);
  while (fractionText.endsWith("0"))
    fractionText = fractionText.substring(0, 0 + (fractionText.length - 1));
  return text + "." + fractionText;
}
/*jazor:clr-member static readonly System.TimeSpan.Zero*/
export function _e5548fcde33957a6() {
  return new JTimeSpan(0n);
}
/*jazor:clr-member static readonly System.TimeSpan.MaxValue*/
export function _15e7c0dd01e25108() {
  return new JTimeSpan(BigInt("9223372036854775807"));
}
/*jazor:clr-member static readonly System.TimeSpan.MinValue*/
export function _3205534506581110() {
  return new JTimeSpan(BigInt("-9223372036854775808"));
}
/*jazor:clr-member System.TimeSpan.TimeSpan()*/
export function _5af0f6ad850e6702() {
  return new JTimeSpan(0n);
}
/*jazor:clr-member System.TimeSpan.TimeSpan(long)*/
export function _d4ecddf3bf0f01b8(ticks) {
  return new JTimeSpan(ticks);
}
/*jazor:clr-member System.TimeSpan.TimeSpan(int, int, int)*/
export function _6f22e268aec62fe7(hours, minutes, seconds) {
  return Create_0d902f9f2178216f(0, hours, minutes, seconds, 0, 0);
}
/*jazor:clr-member System.TimeSpan.TimeSpan(int, int, int, int)*/
export function _13098d82160f45dc(days, hours, minutes, seconds) {
  return Create_0d902f9f2178216f(days, hours, minutes, seconds, 0, 0);
}
/*jazor:clr-member System.TimeSpan.TimeSpan(int, int, int, int, int)*/
export function _d5283dec9fea7d04(days, hours, minutes, seconds, milliseconds) {
  return Create_0d902f9f2178216f(days, hours, minutes, seconds, milliseconds, 0);
}
/*jazor:clr-member System.TimeSpan.TimeSpan(int, int, int, int, int, int)*/
export function _baceecc82b7d48ba(days, hours, minutes, seconds, milliseconds, microseconds) {
  return Create_0d902f9f2178216f(days, hours, minutes, seconds, milliseconds, microseconds);
}
/*jazor:clr-member System.TimeSpan.Ticks.get*/
export function _72d4a471ef1a968f(instance) {
  return instance.ticks;
}
/*jazor:clr-member System.TimeSpan.Days.get*/
export function _a980180cac17c195(instance) {
  return Number(instance.ticks / BigInt("864000000000"));
}
/*jazor:clr-member System.TimeSpan.Hours.get*/
export function _e1126ea3789ed210(instance) {
  return Number(instance.ticks / BigInt("36000000000") % BigInt("24"));
}
/*jazor:clr-member System.TimeSpan.Milliseconds.get*/
export function _af6dae8b5cdc7078(instance) {
  return Number(instance.ticks / BigInt(10000) % BigInt(1000));
}
/*jazor:clr-member System.TimeSpan.Microseconds.get*/
export function _b5ff892bced87c7a(instance) {
  return Number(instance.ticks / BigInt(10) % BigInt(1000));
}
/*jazor:clr-member System.TimeSpan.Nanoseconds.get*/
export function _95472c42904823fa(instance) {
  return Number(instance.ticks * BigInt(100) % BigInt(1000));
}
/*jazor:clr-member System.TimeSpan.Minutes.get*/
export function _f84ed3952defaf6d(instance) {
  return Number(instance.ticks / BigInt(600000000) % BigInt(60));
}
/*jazor:clr-member System.TimeSpan.Seconds.get*/
export function _f3cdc3642c68ede1(instance) {
  return Number(instance.ticks / BigInt(10000000) % BigInt(60));
}
/*jazor:clr-member System.TimeSpan.TotalDays.get*/
export function _3709bd5d7e02854b(instance) {
  return Number(instance.ticks) / 864000000000;
}
/*jazor:clr-member System.TimeSpan.TotalHours.get*/
export function _b4c8b94ce8b8d996(instance) {
  return Number(instance.ticks) / 36000000000;
}
/*jazor:clr-member System.TimeSpan.TotalMilliseconds.get*/
export function _b73ebb6b17996726(instance) {
  return Number(instance.ticks) / 10000;
}
/*jazor:clr-member System.TimeSpan.TotalMicroseconds.get*/
export function _48066d805fb56409(instance) {
  return Number(instance.ticks) / 10;
}
/*jazor:clr-member System.TimeSpan.TotalNanoseconds.get*/
export function _c34f00910f115965(instance) {
  return Number(instance.ticks) * 100;
}
/*jazor:clr-member System.TimeSpan.TotalMinutes.get*/
export function _265f245f5ef9d2ed(instance) {
  return Number(instance.ticks) / 600000000;
}
/*jazor:clr-member System.TimeSpan.TotalSeconds.get*/
export function _d3a0d6dab09b85a6(instance) {
  return Number(instance.ticks) / 10000000;
}
/*jazor:clr-member System.TimeSpan.Add(System.TimeSpan)*/
export function _0f42e55865af8fbf(instance, ts) {
  return new JTimeSpan(instance.ticks + ts.ticks);
}
/*jazor:clr-member static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)*/
export function _06719a9a062fc7ca(t1, t2) {
  if (t1.ticks < t2.ticks)
    return -1;
  if (t1.ticks > t2.ticks)
    return 1;
  return 0;
}
/*jazor:clr-member System.TimeSpan.CompareTo(object)*/
export function _224114f954c0aa27(instance, value) {
  if (value === null)
    return 1;
  let other = value instanceof JTimeSpan ? value : null;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type TimeSpan.");
  return _810426c1d7c3f64f(instance, other);
}
/*jazor:clr-member System.TimeSpan.CompareTo(System.TimeSpan)*/
export function _810426c1d7c3f64f(instance, value) {
  if (instance.ticks < value.ticks)
    return -1;
  if (instance.ticks > value.ticks)
    return 1;
  return 0;
}
/*jazor:clr-member static System.TimeSpan.FromDays(double)*/
export function _174093cb4f47884f(value) {
  return CreateFromTruncatedTicks(value * 864000000000);
}
/*jazor:clr-member System.TimeSpan.Duration()*/
export function _eeb4ad83b79a892c(instance) {
  return instance.ticks < 0n ? NegateChecked(instance) : new JTimeSpan(instance.ticks);
}
/*jazor:clr-member override System.TimeSpan.Equals(object)*/
export function _c6b8a216cf6205b9(instance, value) {
  let other = value instanceof JTimeSpan ? value : null;
  return other !== null && _6b7d08559c6c9859(instance, other);
}
/*jazor:clr-member System.TimeSpan.Equals(System.TimeSpan)*/
export function _6b7d08559c6c9859(instance, obj) {
  return instance.ticks === obj.ticks;
}
/*jazor:clr-member static System.TimeSpan.Equals(System.TimeSpan, System.TimeSpan)*/
export function _77a10002dccedd59(t1, t2) {
  return t1.ticks === t2.ticks;
}
/*jazor:clr-member override System.TimeSpan.GetHashCode()*/
export function _650390adf244b5eb(instance) {
  return GetInt64HashCode(instance.ticks);
}
/*jazor:clr-member static System.TimeSpan.FromDays(int)*/
export function _1ef0cc8c95c82bc4(days) {
  return Create_0d902f9f2178216f(days, 0, 0, 0, 0, 0);
}
/*jazor:clr-member static System.TimeSpan.FromDays(int, int, long, long, long, long)*/
export function _3e2fa32df3160e87(days, hours, minutes, seconds, milliseconds, microseconds) {
  return Create_624c8896c26af7db(ToWholeBigInt(days, "ArgumentOutOfRangeException: Days must be a whole number."), ToWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), minutes, seconds, milliseconds, microseconds);
}
/*jazor:clr-member static System.TimeSpan.FromHours(int)*/
export function _98fc150ce35e78d8(hours) {
  return Create_0d902f9f2178216f(0, hours, 0, 0, 0, 0);
}
/*jazor:clr-member static System.TimeSpan.FromHours(int, long, long, long, long)*/
export function _f307370e05d16ca3(hours, minutes, seconds, milliseconds, microseconds) {
  return Create_624c8896c26af7db(0n, ToWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), minutes, seconds, milliseconds, microseconds);
}
/*jazor:clr-member static System.TimeSpan.FromMinutes(long)*/
export function _059d32e87cf36f24(minutes) {
  return Create_624c8896c26af7db(0n, 0n, minutes, 0n, 0n, 0n);
}
/*jazor:clr-member static System.TimeSpan.FromMinutes(long, long, long, long)*/
export function _f07d6f07ee70a1bd(minutes, seconds, milliseconds, microseconds) {
  return Create_624c8896c26af7db(0n, 0n, minutes, seconds, milliseconds, microseconds);
}
/*jazor:clr-member static System.TimeSpan.FromSeconds(long)*/
export function _e0c33d45a9703e74(seconds) {
  return Create_624c8896c26af7db(0n, 0n, 0n, seconds, 0n, 0n);
}
/*jazor:clr-member static System.TimeSpan.FromSeconds(long, long, long)*/
export function _60df3ea4b8b2693c(seconds, milliseconds, microseconds) {
  return Create_624c8896c26af7db(0n, 0n, 0n, seconds, milliseconds, microseconds);
}
/*jazor:clr-member static System.TimeSpan.FromMilliseconds(long)*/
export function _9dc3c54535eb1333(milliseconds) {
  return Create_624c8896c26af7db(0n, 0n, 0n, 0n, milliseconds, 0n);
}
/*jazor:clr-member static System.TimeSpan.FromMilliseconds(long, long)*/
export function _4bf16885c28b9c57(milliseconds, microseconds) {
  return Create_624c8896c26af7db(0n, 0n, 0n, 0n, milliseconds, microseconds);
}
/*jazor:clr-member static System.TimeSpan.FromMicroseconds(long)*/
export function _5864e2e6b3820640(microseconds) {
  return Create_624c8896c26af7db(0n, 0n, 0n, 0n, 0n, microseconds);
}
/*jazor:clr-member static System.TimeSpan.FromHours(double)*/
export function _105dc0462f9876d6(value) {
  return CreateFromTruncatedTicks(value * 36000000000);
}
/*jazor:clr-member static System.TimeSpan.FromMilliseconds(double)*/
export function _a6de3a3b561d553b(value) {
  return CreateFromTruncatedTicks(value * 10000);
}
/*jazor:clr-member static System.TimeSpan.FromMicroseconds(double)*/
export function _e05c52466faba973(value) {
  return CreateFromTruncatedTicks(value * 10);
}
/*jazor:clr-member static System.TimeSpan.FromMinutes(double)*/
export function _2af67432bdd77d15(value) {
  return CreateFromTruncatedTicks(value * 600000000);
}
/*jazor:clr-member System.TimeSpan.Negate()*/
export function _63a8d2e980965d93(instance) {
  return NegateChecked(instance);
}
/*jazor:clr-member static System.TimeSpan.FromSeconds(double)*/
export function _77a04fa2e0b66990(value) {
  return CreateFromTruncatedTicks(value * 10000000);
}
/*jazor:clr-member System.TimeSpan.Subtract(System.TimeSpan)*/
export function _3c5049382d7807a8(instance, ts) {
  return new JTimeSpan(instance.ticks - ts.ticks);
}
/*jazor:clr-member System.TimeSpan.Multiply(double)*/
export function _a1b4efac0485c39e(instance, factor) {
  return MultiplyByDouble(instance, factor);
}
/*jazor:clr-member System.TimeSpan.Divide(double)*/
export function _871609175f846ae9(instance, divisor) {
  return DivideByDouble(instance, divisor);
}
/*jazor:clr-member System.TimeSpan.Divide(System.TimeSpan)*/
export function _ca7e20ad5bf4a61a(instance, ts) {
  return Number(instance.ticks) / Number(ts.ticks);
}
/*jazor:clr-member static System.TimeSpan.FromTicks(long)*/
export function _a43571552d95203d(value) {
  return new JTimeSpan(value);
}
/*jazor:clr-member static System.TimeSpan.Parse(string)*/
export function _7b8fc48a806ecb54(s) {
  return ParseCore(s);
}
/*jazor:clr-member static System.TimeSpan.Parse(string, System.IFormatProvider)*/
export function _55da737da6ee6a65(input, formatProvider) {
  return ParseCore(input);
}
/*jazor:clr-member static System.TimeSpan.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _f2cd45773b91a418(input, formatProvider) {
  return ParseCore(input);
}
/*jazor:clr-member static System.TimeSpan.TryParse(string, out System.TimeSpan)*/
export function _6fb85ef4d11b9143(s, result) {
  if (s === null || s.length === 0)
    return [false, new JTimeSpan(0n)];
  try {
    return [true, ParseCore(s)];
  } catch {
    return [false, new JTimeSpan(0n)];
  }
}
/*jazor:clr-member static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, out System.TimeSpan)*/
export function _11fc2c166b0126e3(s, result) {
  return _6fb85ef4d11b9143(s, result);
}
/*jazor:clr-member static System.TimeSpan.TryParse(string, System.IFormatProvider, out System.TimeSpan)*/
export function _0d5a8bac05463d1f(input, formatProvider, result) {
  return _6fb85ef4d11b9143(input, result);
}
/*jazor:clr-member static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)*/
export function _5eae656c46346343(input, formatProvider, result) {
  return _6fb85ef4d11b9143(input, result);
}
/*jazor:clr-member System.TimeSpan.ToString(string)*/
export function _95c4c385ed7aa2da(instance, format) {
  return FormatCore(instance, format);
}
/*jazor:clr-member System.TimeSpan.ToString(string, System.IFormatProvider)*/
export function _49fbba4d75df94f7(instance, format, formatProvider) {
  return FormatCore(instance, format);
}
/*jazor:clr-member static System.TimeSpan.operator -(System.TimeSpan)*/
export function _e8e884a7b14ce4b4(t) {
  return NegateChecked(t);
}
/*jazor:clr-member static System.TimeSpan.operator -(System.TimeSpan, System.TimeSpan)*/
export function _0228a4c011d04780(t1, t2) {
  return new JTimeSpan(t1.ticks - t2.ticks);
}
/*jazor:clr-member static System.TimeSpan.operator +(System.TimeSpan)*/
export function _6c2fe85d341763c7(t) {
  return new JTimeSpan(t.ticks);
}
/*jazor:clr-member static System.TimeSpan.operator +(System.TimeSpan, System.TimeSpan)*/
export function _24670e70abc0feb8(t1, t2) {
  return new JTimeSpan(t1.ticks + t2.ticks);
}
/*jazor:clr-member static System.TimeSpan.operator *(System.TimeSpan, double)*/
export function _f2a4ea62d054d8a3(timeSpan, factor) {
  return MultiplyByDouble(timeSpan, factor);
}
/*jazor:clr-member static System.TimeSpan.operator *(double, System.TimeSpan)*/
export function _90eaec13ec0f9fea(factor, timeSpan) {
  return MultiplyByDouble(timeSpan, factor);
}
/*jazor:clr-member static System.TimeSpan.operator /(System.TimeSpan, double)*/
export function _eba9e2c9c23d7df9(timeSpan, divisor) {
  return DivideByDouble(timeSpan, divisor);
}
/*jazor:clr-member static System.TimeSpan.operator /(System.TimeSpan, System.TimeSpan)*/
export function _f857571e543b3b87(t1, t2) {
  return Number(t1.ticks) / Number(t2.ticks);
}
/*jazor:clr-member static System.TimeSpan.operator ==(System.TimeSpan, System.TimeSpan)*/
export function _cb0f1b7f98578d6e(t1, t2) {
  return t1.ticks === t2.ticks;
}
/*jazor:clr-member static System.TimeSpan.operator !=(System.TimeSpan, System.TimeSpan)*/
export function _20d19f6d7c8824a6(t1, t2) {
  return t1.ticks !== t2.ticks;
}
/*jazor:clr-member static System.TimeSpan.operator <(System.TimeSpan, System.TimeSpan)*/
export function _7b0fd798871f70d1(t1, t2) {
  return t1.ticks < t2.ticks;
}
/*jazor:clr-member static System.TimeSpan.operator <=(System.TimeSpan, System.TimeSpan)*/
export function _8d936a645fdca63f(t1, t2) {
  return t1.ticks <= t2.ticks;
}
/*jazor:clr-member static System.TimeSpan.operator >(System.TimeSpan, System.TimeSpan)*/
export function _99f4b8243dbe421d(t1, t2) {
  return t1.ticks > t2.ticks;
}
/*jazor:clr-member static System.TimeSpan.operator >=(System.TimeSpan, System.TimeSpan)*/
export function _60fd1bb34b700faa(t1, t2) {
  return t1.ticks >= t2.ticks;
}
