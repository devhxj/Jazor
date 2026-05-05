import { DoubleModule } from "System/DoubleModule.js";
import { JTimeSpan, RuntimeModule } from "System/RuntimeModule.js";
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
function get_MinTimeSpanTicks() {
  return BigInt("-9223372036854775808");
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
  if (instance.ticks === minTimeSpanTicks)
    throw new Error("OverflowException: Negating the minimum TimeSpan value is invalid.");
  return new JTimeSpan(-instance.ticks);
}
function createFromRoundedTicks(value) {
  if (DoubleModule._24e14b276e0c7e30(value))
    throw new Error("ArgumentException: TimeSpan value cannot be NaN.");
  if (!DoubleModule._aed2927097617729(value))
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  let rounded = Math.round(value);
  if (!DoubleModule._aed2927097617729(rounded))
    throw new Error("OverflowException: TimeSpan is too long or too short.");
  return new JTimeSpan(BigInt(rounded));
}
function create(days, hours, minutes, seconds, milliseconds, microseconds) {
  let ticks = days * ticksPerDay + hours * ticksPerHour + minutes * ticksPerMinute + seconds * ticksPerSecond + milliseconds * ticksPerMillisecond + microseconds * ticksPerMicrosecond;
  return new JTimeSpan(ticks);
}
function Create(days, hours, minutes, seconds, milliseconds, microseconds) {
  return create(toWholeBigInt(days, "ArgumentOutOfRangeException: Days must be a whole number."), toWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), toWholeBigInt(minutes, "ArgumentOutOfRangeException: Minutes must be a whole number."), toWholeBigInt(seconds, "ArgumentOutOfRangeException: Seconds must be a whole number."), toWholeBigInt(milliseconds, "ArgumentOutOfRangeException: Milliseconds must be a whole number."), toWholeBigInt(microseconds, "ArgumentOutOfRangeException: Microseconds must be a whole number."));
}
function multiplyByDouble(instance, factor) {
  return createFromRoundedTicks(Number(instance.ticks) * factor);
}
function divideByDouble(instance, divisor) {
  return createFromRoundedTicks(Number(instance.ticks) / divisor);
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
  let fractionTicks = BigInt.zero;
  if (fractionText.length > 0) {
    if (fractionText.length > 7 || !isDigits(fractionText))
      throw new Error(`FormatException: String '${input}' was not recognized as a valid TimeSpan.`);
    while (fractionText.length < 7)
      fractionText += "0";
    fractionTicks = BigInt(fractionText);
  }
  let totalTicks = BigInt(days) * ticksPerDay + BigInt(hours) * ticksPerHour + BigInt(minutes) * ticksPerMinute + BigInt(seconds) * ticksPerSecond + fractionTicks;
  return new JTimeSpan(negative ? -totalTicks : totalTicks);
}
export function _e5548fcde33957a6() {
  return new JTimeSpan(BigInt.zero);
}
export function _15e7c0dd01e25108() {
  return new JTimeSpan(BigInt("9223372036854775807"));
}
export function _3205534506581110() {
  return new JTimeSpan(BigInt("-9223372036854775808"));
}
export function _5af0f6ad850e6702() {
  return new JTimeSpan(BigInt.zero);
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
  return createFromRoundedTicks(value * 864000000000);
}
export function _eeb4ad83b79a892c(instance) {
  return instance.ticks < BigInt.zero ? negateChecked(instance) : new JTimeSpan(instance.ticks);
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
  return RuntimeModule.getInt64HashCode(instance.ticks);
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
  return create(BigInt.zero, toWholeBigInt(hours, "ArgumentOutOfRangeException: Hours must be a whole number."), minutes, seconds, milliseconds, microseconds);
}
export function _059d32e87cf36f24(minutes) {
  return create(BigInt.zero, BigInt.zero, minutes, BigInt.zero, BigInt.zero, BigInt.zero);
}
export function _f07d6f07ee70a1bd(minutes, seconds, milliseconds, microseconds) {
  return create(BigInt.zero, BigInt.zero, minutes, seconds, milliseconds, microseconds);
}
export function _e0c33d45a9703e74(seconds) {
  return create(BigInt.zero, BigInt.zero, BigInt.zero, seconds, BigInt.zero, BigInt.zero);
}
export function _60df3ea4b8b2693c(seconds, milliseconds, microseconds) {
  return create(BigInt.zero, BigInt.zero, BigInt.zero, seconds, milliseconds, microseconds);
}
export function _9dc3c54535eb1333(milliseconds) {
  return create(BigInt.zero, BigInt.zero, BigInt.zero, BigInt.zero, milliseconds, BigInt.zero);
}
export function _4bf16885c28b9c57(milliseconds, microseconds) {
  return create(BigInt.zero, BigInt.zero, BigInt.zero, BigInt.zero, milliseconds, microseconds);
}
export function _5864e2e6b3820640(microseconds) {
  return create(BigInt.zero, BigInt.zero, BigInt.zero, BigInt.zero, BigInt.zero, microseconds);
}
export function _105dc0462f9876d6(value) {
  return createFromRoundedTicks(value * 36000000000);
}
export function _a6de3a3b561d553b(value) {
  return createFromRoundedTicks(value * 10000);
}
export function _e05c52466faba973(value) {
  return createFromRoundedTicks(value * 10);
}
export function _2af67432bdd77d15(value) {
  return createFromRoundedTicks(value * 600000000);
}
export function _63a8d2e980965d93(instance) {
  return negateChecked(instance);
}
export function _77a04fa2e0b66990(value) {
  return createFromRoundedTicks(value * 10000000);
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
    return [false, new JTimeSpan(BigInt.zero)];
  try {
    return [true, parseCore(s)];
  } catch {
    return [false, new JTimeSpan(BigInt.zero)];
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
export const TimeSpanModule = {
  get_TicksPerMicrosecond,
  get_TicksPerMillisecond,
  get_TicksPerSecond,
  get_TicksPerMinute,
  get_TicksPerHour,
  get_TicksPerDay,
  get_MinTimeSpanTicks,
  isAsciiDigit,
  isDigits,
  ensureWholeNumber,
  toWholeBigInt,
  negateChecked,
  createFromRoundedTicks,
  create,
  create: Create,
  multiplyByDouble,
  divideByDouble,
  parseCore,
  _e5548fcde33957a6,
  _15e7c0dd01e25108,
  _3205534506581110,
  _5af0f6ad850e6702,
  _d4ecddf3bf0f01b8,
  _6f22e268aec62fe7,
  _13098d82160f45dc,
  _d5283dec9fea7d04,
  _baceecc82b7d48ba,
  _72d4a471ef1a968f,
  _a980180cac17c195,
  _e1126ea3789ed210,
  _af6dae8b5cdc7078,
  _b5ff892bced87c7a,
  _95472c42904823fa,
  _f84ed3952defaf6d,
  _f3cdc3642c68ede1,
  _3709bd5d7e02854b,
  _b4c8b94ce8b8d996,
  _b73ebb6b17996726,
  _48066d805fb56409,
  _c34f00910f115965,
  _265f245f5ef9d2ed,
  _d3a0d6dab09b85a6,
  _0f42e55865af8fbf,
  _06719a9a062fc7ca,
  _224114f954c0aa27,
  _810426c1d7c3f64f,
  _174093cb4f47884f,
  _eeb4ad83b79a892c,
  _c6b8a216cf6205b9,
  _6b7d08559c6c9859,
  _77a10002dccedd59,
  _650390adf244b5eb,
  _1ef0cc8c95c82bc4,
  _3e2fa32df3160e87,
  _98fc150ce35e78d8,
  _f307370e05d16ca3,
  _059d32e87cf36f24,
  _f07d6f07ee70a1bd,
  _e0c33d45a9703e74,
  _60df3ea4b8b2693c,
  _9dc3c54535eb1333,
  _4bf16885c28b9c57,
  _5864e2e6b3820640,
  _105dc0462f9876d6,
  _a6de3a3b561d553b,
  _e05c52466faba973,
  _2af67432bdd77d15,
  _63a8d2e980965d93,
  _77a04fa2e0b66990,
  _3c5049382d7807a8,
  _a1b4efac0485c39e,
  _871609175f846ae9,
  _ca7e20ad5bf4a61a,
  _a43571552d95203d,
  _7b8fc48a806ecb54,
  _55da737da6ee6a65,
  _f2cd45773b91a418,
  _6fb85ef4d11b9143,
  _11fc2c166b0126e3,
  _0d5a8bac05463d1f,
  _5eae656c46346343,
  _95c4c385ed7aa2da,
  _49fbba4d75df94f7,
  _e8e884a7b14ce4b4,
  _0228a4c011d04780,
  _6c2fe85d341763c7,
  _24670e70abc0feb8,
  _f2a4ea62d054d8a3,
  _90eaec13ec0f9fea,
  _eba9e2c9c23d7df9,
  _f857571e543b3b87,
  _cb0f1b7f98578d6e,
  _20d19f6d7c8824a6,
  _7b0fd798871f70d1,
  _8d936a645fdca63f,
  _99f4b8243dbe421d,
  _60fd1bb34b700faa
};
