import { _24e14b276e0c7e30, _aed2927097617729 } from "System/DoubleModule.js";
import { JTimeSpan, getInt64HashCode } from "System/RuntimeModule.js";
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
function isAsciiDigit(value) {
  return value >= "0" && value <= "9";
}
function isDigits(text) {
  if (text.length === 0)
    return false;
  for (let i = 0; i < text.length; i++) {
    if (!isAsciiDigit(_5ad63706a889c294(text, i)))
      return false;
  }
  return true;
}
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function toWholeBigInt(value, message) {
  ensureWholeNumber(value, message);
  return BigInt(value);
}
function negateChecked(instance) {
  if (instance.ticks === get_MinTimeSpanTicks())
    throw new Error("OverflowException: Negating the minimum TimeSpan value is invalid.");
  return new JTimeSpan(-instance.ticks);
}
function roundToEven(value) {
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
function createFromTruncatedTicks(value) {
  if (_24e14b276e0c7e30(value))
    throw new Error("ArgumentException: TimeSpan value cannot be NaN.");
  if (!_aed2927097617729(value))
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  if (value > get_MaxTimeSpanTicksAsDouble() || value < get_MinTimeSpanTicksAsDouble())
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  if (value === get_MaxTimeSpanTicksAsDouble())
    return new JTimeSpan(get_MaxTimeSpanTicks());
  return new JTimeSpan(BigInt(Math.trunc(value)));
}
function createFromRoundedTicks(value) {
  if (_24e14b276e0c7e30(value))
    throw new Error("ArgumentException: TimeSpan value cannot be NaN.");
  if (!_aed2927097617729(value))
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  let rounded = roundToEven(value);
  if (rounded > get_MaxTimeSpanTicksAsDouble() || rounded < get_MinTimeSpanTicksAsDouble())
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  if (rounded === get_MaxTimeSpanTicksAsDouble())
    return new JTimeSpan(get_MaxTimeSpanTicks());
  return new JTimeSpan(BigInt(rounded));
}
function getFiniteDoubleRatio(value) {
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
function createFromRoundedRationalTicks(numerator, denominator) {
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
function create(days, hours, minutes, seconds, milliseconds, microseconds) {
  let ticks = days * get_TicksPerDay() + hours * get_TicksPerHour() + minutes * get_TicksPerMinute() + seconds * get_TicksPerSecond() + milliseconds * get_TicksPerMillisecond() + microseconds * get_TicksPerMicrosecond();
  return new JTimeSpan(ticks);
}
function Create(days, hours, minutes, seconds, milliseconds, microseconds) {
  return create(toWholeBigInt(days, "ArgumentOutOfRangeException: Days must be a whole number."), toWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), toWholeBigInt(minutes, "ArgumentOutOfRangeException: Minutes must be a whole number."), toWholeBigInt(seconds, "ArgumentOutOfRangeException: Seconds must be a whole number."), toWholeBigInt(milliseconds, "ArgumentOutOfRangeException: Milliseconds must be a whole number."), toWholeBigInt(microseconds, "ArgumentOutOfRangeException: Microseconds must be a whole number."));
}
function multiplyByDouble(instance, factor) {
  if (_24e14b276e0c7e30(factor) || !_aed2927097617729(factor))
    return createFromRoundedTicks(Number(instance.ticks) * factor);
  let ratio = getFiniteDoubleRatio(factor);
  return createFromRoundedRationalTicks(instance.ticks * ratio[0], ratio[1]);
}
function divideByDouble(instance, divisor) {
  if (_24e14b276e0c7e30(divisor) || !_aed2927097617729(divisor) || divisor === 0)
    return createFromRoundedTicks(Number(instance.ticks) / divisor);
  let ratio = getFiniteDoubleRatio(divisor);
  let numerator = instance.ticks * ratio[1];
  let denominator = ratio[0];
  if (denominator < 0n) {
    numerator = -numerator;
    denominator = -denominator;
  }
  return createFromRoundedRationalTicks(numerator, denominator);
}
function parseCore(input) {
  let s = input.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid TimeSpan.");
  let negative = false;
  if (_5ad63706a889c294(s, 0) === "+" || _5ad63706a889c294(s, 0) === "-") {
    negative = _5ad63706a889c294(s, 0) === "-";
    s = s.substring(1);
  }
  let firstColon = s.indexOf(":");
  if (firstColon < 0)
    throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
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
  let minuteText = lastColon === firstColon ? s.substring(firstColon + 1) : s.substring(firstColon + 1, firstColon + 1 + (lastColon - firstColon - 1));
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
      throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
  }
  if (!isDigits(dayText) || !isDigits(hourText) || !isDigits(minuteText) || !isDigits(secondText))
    throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
  let days = Number(dayText);
  let hours = Number(hourText);
  let minutes = Number(minuteText);
  let seconds = Number(secondText);
  if (isNaN(days) || isNaN(hours) || isNaN(minutes) || isNaN(seconds))
    throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
  if (days < 0 || hours < 0 || minutes < 0 || seconds < 0)
    throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
  if (hasDays && hours > 23)
    throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
  if (minutes > 59 || seconds > 59)
    throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
  let fractionTicks = 0n;
  if (fractionText.length > 0) {
    if (fractionText.length > 7 || !isDigits(fractionText))
      throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
    while (fractionText.length < 7)
      fractionText += "0";
    fractionTicks = BigInt(fractionText);
  }
  let totalTicks = BigInt(days) * get_TicksPerDay() + BigInt(hours) * get_TicksPerHour() + BigInt(minutes) * get_TicksPerMinute() + BigInt(seconds) * get_TicksPerSecond() + fractionTicks;
  return new JTimeSpan(negative ? -totalTicks : totalTicks);
}
export function _e5548fcde33957a6() {
  return new JTimeSpan(0n);
}
export function _15e7c0dd01e25108() {
  return new JTimeSpan(BigInt("9223372036854775807"));
}
export function _3205534506581110() {
  return new JTimeSpan(BigInt("-9223372036854775808"));
}
export function _5af0f6ad850e6702() {
  return new JTimeSpan(0n);
}
export function _d4ecddf3bf0f01b8(ticks) {
  return new JTimeSpan(ticks);
}
export function _6f22e268aec62fe7(hours, minutes, seconds) {
  return Create(0, hours, minutes, seconds, 0, 0);
}
export function _13098d82160f45dc(days, hours, minutes, seconds) {
  return Create(days, hours, minutes, seconds, 0, 0);
}
export function _d5283dec9fea7d04(days, hours, minutes, seconds, milliseconds) {
  return Create(days, hours, minutes, seconds, milliseconds, 0);
}
export function _baceecc82b7d48ba(days, hours, minutes, seconds, milliseconds, microseconds) {
  return Create(days, hours, minutes, seconds, milliseconds, microseconds);
}
export function _72d4a471ef1a968f(instance) {
  return instance.ticks;
}
export function _a980180cac17c195(instance) {
  return Number(instance.ticks / BigInt("864000000000"));
}
export function _e1126ea3789ed210(instance) {
  return Number(instance.ticks / BigInt("36000000000") % BigInt("24"));
}
export function _af6dae8b5cdc7078(instance) {
  return Number(instance.ticks / BigInt(10000) % BigInt(1000));
}
export function _b5ff892bced87c7a(instance) {
  return Number(instance.ticks / BigInt(10) % BigInt(1000000));
}
export function _95472c42904823fa(instance) {
  return Number(instance.ticks * BigInt(100) % BigInt(1000000000));
}
export function _f84ed3952defaf6d(instance) {
  return Number(instance.ticks / BigInt(600000000) % BigInt(60));
}
export function _f3cdc3642c68ede1(instance) {
  return Number(instance.ticks / BigInt(10000000) % BigInt(60));
}
export function _3709bd5d7e02854b(instance) {
  return Number(instance.ticks) / 864000000000;
}
export function _b4c8b94ce8b8d996(instance) {
  return Number(instance.ticks) / 36000000000;
}
export function _b73ebb6b17996726(instance) {
  return Number(instance.ticks) / 10000;
}
export function _48066d805fb56409(instance) {
  return Number(instance.ticks) / 10;
}
export function _c34f00910f115965(instance) {
  return Number(instance.ticks) * 100;
}
export function _265f245f5ef9d2ed(instance) {
  return Number(instance.ticks) / 600000000;
}
export function _d3a0d6dab09b85a6(instance) {
  return Number(instance.ticks) / 10000000;
}
export function _0f42e55865af8fbf(instance, ts) {
  return new JTimeSpan(instance.ticks + ts.ticks);
}
export function _06719a9a062fc7ca(t1, t2) {
  if (t1.ticks < t2.ticks)
    return -1;
  if (t1.ticks > t2.ticks)
    return 1;
  return 0;
}
export function _224114f954c0aa27(instance, value) {
  if (value === null)
    return 1;
  let other = value;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type TimeSpan.");
  return _810426c1d7c3f64f(instance, other);
}
export function _810426c1d7c3f64f(instance, value) {
  if (instance.ticks < value.ticks)
    return -1;
  if (instance.ticks > value.ticks)
    return 1;
  return 0;
}
export function _174093cb4f47884f(value) {
  return createFromTruncatedTicks(value * 864000000000);
}
export function _eeb4ad83b79a892c(instance) {
  return instance.ticks < 0n ? negateChecked(instance) : new JTimeSpan(instance.ticks);
}
export function _c6b8a216cf6205b9(instance, value) {
  let other = value;
  return other !== null && _6b7d08559c6c9859(instance, other);
}
export function _6b7d08559c6c9859(instance, obj) {
  return instance.ticks === obj.ticks;
}
export function _77a10002dccedd59(t1, t2) {
  return t1.ticks === t2.ticks;
}
export function _650390adf244b5eb(instance) {
  return getInt64HashCode(instance.ticks);
}
export function _1ef0cc8c95c82bc4(days) {
  return Create(days, 0, 0, 0, 0, 0);
}
export function _3e2fa32df3160e87(days, hours, minutes, seconds, milliseconds, microseconds) {
  return create(toWholeBigInt(days, "ArgumentOutOfRangeException: Days must be a whole number."), toWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), minutes, seconds, milliseconds, microseconds);
}
export function _98fc150ce35e78d8(hours) {
  return Create(0, hours, 0, 0, 0, 0);
}
export function _f307370e05d16ca3(hours, minutes, seconds, milliseconds, microseconds) {
  return create(0n, toWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), minutes, seconds, milliseconds, microseconds);
}
export function _059d32e87cf36f24(minutes) {
  return create(0n, 0n, minutes, 0n, 0n, 0n);
}
export function _f07d6f07ee70a1bd(minutes, seconds, milliseconds, microseconds) {
  return create(0n, 0n, minutes, seconds, milliseconds, microseconds);
}
export function _e0c33d45a9703e74(seconds) {
  return create(0n, 0n, 0n, seconds, 0n, 0n);
}
export function _60df3ea4b8b2693c(seconds, milliseconds, microseconds) {
  return create(0n, 0n, 0n, seconds, milliseconds, microseconds);
}
export function _9dc3c54535eb1333(milliseconds) {
  return create(0n, 0n, 0n, 0n, milliseconds, 0n);
}
export function _4bf16885c28b9c57(milliseconds, microseconds) {
  return create(0n, 0n, 0n, 0n, milliseconds, microseconds);
}
export function _5864e2e6b3820640(microseconds) {
  return create(0n, 0n, 0n, 0n, 0n, microseconds);
}
export function _105dc0462f9876d6(value) {
  return createFromTruncatedTicks(value * 36000000000);
}
export function _a6de3a3b561d553b(value) {
  return createFromTruncatedTicks(value * 10000);
}
export function _e05c52466faba973(value) {
  return createFromTruncatedTicks(value * 10);
}
export function _2af67432bdd77d15(value) {
  return createFromTruncatedTicks(value * 600000000);
}
export function _63a8d2e980965d93(instance) {
  return negateChecked(instance);
}
export function _77a04fa2e0b66990(value) {
  return createFromTruncatedTicks(value * 10000000);
}
export function _3c5049382d7807a8(instance, ts) {
  return new JTimeSpan(instance.ticks - ts.ticks);
}
export function _a1b4efac0485c39e(instance, factor) {
  return multiplyByDouble(instance, factor);
}
export function _871609175f846ae9(instance, divisor) {
  return divideByDouble(instance, divisor);
}
export function _ca7e20ad5bf4a61a(instance, ts) {
  return Number(instance.ticks) / Number(ts.ticks);
}
export function _a43571552d95203d(value) {
  return new JTimeSpan(value);
}
export function _7b8fc48a806ecb54(s) {
  return parseCore(s);
}
export function _55da737da6ee6a65(input, formatProvider) {
  return parseCore(input);
}
export function _f2cd45773b91a418(input, formatProvider) {
  return parseCore(input);
}
export function _6fb85ef4d11b9143(s, result) {
  if (s === null || s.length === 0)
    return [false, new JTimeSpan(0n)];
  try {
    return [true, parseCore(s)];
  } catch {
    return [false, new JTimeSpan(0n)];
  }
}
export function _11fc2c166b0126e3(s, result) {
  return _6fb85ef4d11b9143(s, result);
}
export function _0d5a8bac05463d1f(input, formatProvider, result) {
  return _6fb85ef4d11b9143(input, result);
}
export function _5eae656c46346343(input, formatProvider, result) {
  return _6fb85ef4d11b9143(input, result);
}
export function _95c4c385ed7aa2da(instance, format) {
  return instance.toString();
}
export function _49fbba4d75df94f7(instance, format, formatProvider) {
  return instance.toString();
}
export function _e8e884a7b14ce4b4(t) {
  return negateChecked(t);
}
export function _0228a4c011d04780(t1, t2) {
  return new JTimeSpan(t1.ticks - t2.ticks);
}
export function _6c2fe85d341763c7(t) {
  return new JTimeSpan(t.ticks);
}
export function _24670e70abc0feb8(t1, t2) {
  return new JTimeSpan(t1.ticks + t2.ticks);
}
export function _f2a4ea62d054d8a3(timeSpan, factor) {
  return multiplyByDouble(timeSpan, factor);
}
export function _90eaec13ec0f9fea(factor, timeSpan) {
  return multiplyByDouble(timeSpan, factor);
}
export function _eba9e2c9c23d7df9(timeSpan, divisor) {
  return divideByDouble(timeSpan, divisor);
}
export function _f857571e543b3b87(t1, t2) {
  return Number(t1.ticks) / Number(t2.ticks);
}
export function _cb0f1b7f98578d6e(t1, t2) {
  return t1.ticks === t2.ticks;
}
export function _20d19f6d7c8824a6(t1, t2) {
  return t1.ticks !== t2.ticks;
}
export function _7b0fd798871f70d1(t1, t2) {
  return t1.ticks < t2.ticks;
}
export function _8d936a645fdca63f(t1, t2) {
  return t1.ticks <= t2.ticks;
}
export function _99f4b8243dbe421d(t1, t2) {
  return t1.ticks > t2.ticks;
}
export function _60fd1bb34b700faa(t1, t2) {
  return t1.ticks >= t2.ticks;
}
