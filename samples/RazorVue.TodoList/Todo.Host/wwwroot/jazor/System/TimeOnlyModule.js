import { DoubleModule } from "System/DoubleModule.js";
import { JTimeOnly, JTimeSpan, RuntimeModule } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function get_TicksPerDay() {
  return BigInt("864000000000");
}
function get_TicksPerHour() {
  return BigInt("36000000000");
}
function get_TicksPerMinute() {
  return BigInt("600000000");
}
function get_TicksPerSecond() {
  return BigInt("10000000");
}
function get_AllowedDateTimeStylesMask() {
  return 7;
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
function validateTimeParts(hour, minute, second, millisecond, microsecond) {
  if (Math.floor(hour) !== hour || Math.floor(minute) !== minute || Math.floor(second) !== second || Math.floor(millisecond) !== millisecond || Math.floor(microsecond) !== microsecond)
    throw new Error("ArgumentOutOfRangeException: TimeOnly components must be whole numbers.");
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999 || microsecond < 0 || microsecond > 999)
    throw new Error("ArgumentOutOfRangeException: One or more TimeOnly components are out of range.");
}
function createTimeOnly(hour, minute, second, millisecond, microsecond) {
  validateTimeParts(hour, minute, second, millisecond, microsecond);
  return new JTimeOnly(BigInt(hour) * ticksPerHour + BigInt(minute) * ticksPerMinute + BigInt(second) * ticksPerSecond + BigInt(millisecond) * BigInt("10000") + BigInt(microsecond) * BigInt(10));
}
function createRoundedTicksFromDouble(value) {
  if (DoubleModule._24e14b276e0c7e30(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!DoubleModule._aed2927097617729(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  let rounded = Math.round(value);
  if (!DoubleModule._aed2927097617729(rounded))
    throw new Error("ArgumentOutOfRangeException: Value is outside the supported TimeOnly range.");
  return BigInt(rounded);
}
function addWithWrappedDays(instance, deltaTicks) {
  let total = instance.ticks + deltaTicks;
  let wrapped = Number(total / ticksPerDay);
  let result = total % ticksPerDay;
  if (result < BigInt.zero) {
    result += ticksPerDay;
    wrapped--;
  }
  return [new JTimeOnly(result), wrapped];
}
function createTimeOnlyFromTicks(ticks) {
  if (ticks < BigInt.zero || ticks >= ticksPerDay)
    throw new Error("ArgumentOutOfRangeException: TimeOnly ticks must be within a single day.");
  return new JTimeOnly(ticks);
}
function parseCore(s) {
  let text = s.trim();
  if (text.length === 0)
    throw new Error("FormatException: String was not recognized as a valid TimeOnly.");
  let first = text.indexOf(":");
  if (first < 0)
    throw new Error(`FormatException: String '${s}' was not recognized as a valid TimeOnly.`);
  let second = text.indexOf(":", first + 1);
  let hourText = text.substring(0, 0 + first);
  let minuteText = second < 0 ? text.substring(first + 1) : text.substring(first + 1, first + 1 + (second - first - 1));
  if (!isDigits(hourText) || !isDigits(minuteText))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid TimeOnly.`);
  let hour = Number(hourText);
  let minute = Number(minuteText);
  let secondValue = 0;
  let fractionTicks = BigInt.zero;
  if (second >= 0) {
    let fractionIndex = text.indexOf(".", second + 1);
    let secondText = fractionIndex < 0 ? text.substring(second + 1) : text.substring(second + 1, second + 1 + (fractionIndex - second - 1));
    if (!isDigits(secondText))
      throw new Error(`FormatException: String '${s}' was not recognized as a valid TimeOnly.`);
    secondValue = Number(secondText);
    if (fractionIndex >= 0) {
      let fractionText = text.substring(fractionIndex + 1);
      if (fractionText.length === 0 || fractionText.length > 7 || !isDigits(fractionText))
        throw new Error(`FormatException: String '${s}' was not recognized as a valid TimeOnly.`);
      while (fractionText.length < 7)
        fractionText += "0";
      fractionTicks = BigInt(fractionText);
    }
  }
  if (isNaN(hour) || isNaN(minute) || isNaN(secondValue))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid TimeOnly.`);
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || secondValue < 0 || secondValue > 59)
    throw new Error(`FormatException: String '${s}' was not recognized as a valid TimeOnly.`);
  return new JTimeOnly(BigInt(hour) * ticksPerHour + BigInt(minute) * ticksPerMinute + BigInt(secondValue) * ticksPerSecond + fractionTicks);
}
function getDateTimeStylesValue(style) {
  let numberStyle, enumStyle;
  if (typeof style === "number" && (numberStyle = style, true))
    return numberStyle;
  if (typeof style === "number" && (enumStyle = style, true))
    return Number(enumStyle);
  if (style === null)
    return 0;
  throw new Error("ArgumentException: Invalid DateTimeStyles value.");
}
function isSupportedDateTimeStyles(style) {
  return style >= 0 && Math.floor(style) === style && (style & ~allowedDateTimeStylesMask) === 0;
}
export function _9f78f92d0753f4cf() {
  return new JTimeOnly(BigInt.zero);
}
export function _5a02197e2ef2252f() {
  return new JTimeOnly(BigInt.zero);
}
export function _b1d0e19d91dbb54a() {
  return new JTimeOnly(BigInt("863999999999"));
}
export function _62d395c56c4c299d(hour, minute) {
  return createTimeOnly(hour, minute, 0, 0, 0);
}
export function _e9a3481b3456aad4(hour, minute, second) {
  return createTimeOnly(hour, minute, second, 0, 0);
}
export function _335167098e226ccf(hour, minute, second, millisecond) {
  return createTimeOnly(hour, minute, second, millisecond, 0);
}
export function _28c8cb012fe0e547(hour, minute, second, millisecond, microsecond) {
  return createTimeOnly(hour, minute, second, millisecond, microsecond);
}
export function _b8b3b95e8b848f44(ticks) {
  return createTimeOnlyFromTicks(ticks);
}
export function _201ef41481f4e3fb(instance) {
  return Number(instance.ticks / ticksPerHour % BigInt(24));
}
export function _009addd612610031(instance) {
  return Number(instance.ticks / ticksPerMinute % BigInt(60));
}
export function _b9481eedd6cbeb99(instance) {
  return Number(instance.ticks / ticksPerSecond % BigInt(60));
}
export function _3c789a48d39d0010(instance) {
  return Number(instance.ticks / BigInt(10000) % BigInt(1000));
}
export function _a091b803b851e27e(instance) {
  return Number(instance.ticks / BigInt(10) % BigInt(1000));
}
export function _656df0ee12e92399(instance) {
  return Number(instance.ticks % BigInt(10) * BigInt(100));
}
export function _2fd46050126234ac(instance) {
  return instance.ticks;
}
export function _4c935b985e7b6e02(instance, value) {
  return new JTimeOnly(instance.ticks + value.ticks);
}
export function _31bb07d031379025(instance, value, wrappedDays) {
  let total = instance.ticks + value.ticks;
  let wrapped = Number(total / ticksPerDay);
  let result = total % ticksPerDay;
  if (result < BigInt.zero) {
    result += ticksPerDay;
    wrapped--;
  }
  return [new JTimeOnly(result), wrapped];
}
export function _8e71fa0d2695e84f(instance, value) {
  let delta = new JTimeSpan(createRoundedTicksFromDouble(value * 36000000000));
  return _4c935b985e7b6e02(instance, delta);
}
export function _ad6cad38823a5ef6(instance, value, wrappedDays) {
  return addWithWrappedDays(instance, createRoundedTicksFromDouble(value * 36000000000));
}
export function _77bd7db30cbf3bc9(instance, value) {
  let delta = new JTimeSpan(createRoundedTicksFromDouble(value * 600000000));
  return _4c935b985e7b6e02(instance, delta);
}
export function _e698cb9920401887(instance, value, wrappedDays) {
  return addWithWrappedDays(instance, createRoundedTicksFromDouble(value * 600000000));
}
export function _da64e8d379a7e47c(instance, start, end) {
  return start.ticks < end.ticks ? instance.ticks >= start.ticks && instance.ticks < end.ticks : instance.ticks >= start.ticks || instance.ticks < end.ticks;
}
export function _8e47d4212be3070c(left, right) {
  return left.ticks === right.ticks;
}
export function _b3b712e75fff0050(left, right) {
  return left.ticks !== right.ticks;
}
export function _341a3f0fbcda5677(left, right) {
  return left.ticks > right.ticks;
}
export function _0656cf79f08fd69b(left, right) {
  return left.ticks >= right.ticks;
}
export function _9b001b8f9a72a57d(left, right) {
  return left.ticks < right.ticks;
}
export function _cd098f438100d4cb(left, right) {
  return left.ticks <= right.ticks;
}
export function _888a9b439de5e7c1(t1, t2) {
  let diff = t1.ticks - t2.ticks;
  return new JTimeSpan(diff < BigInt.zero ? diff + ticksPerDay : diff);
}
export function _d6170153a1f10bc3(instance, hour, minute) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance)];
}
export function _d36793074735968e(instance, hour, minute, second) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance)];
}
export function _b349a5fd892d33be(instance, hour, minute, second, millisecond) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance), _3c789a48d39d0010(instance)];
}
export function _1f5bb15cea73f15b(instance, hour, minute, second, millisecond, microsecond) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance), _3c789a48d39d0010(instance), _a091b803b851e27e(instance)];
}
export function _df2fe8c100ae98f0(timeSpan) {
  return createTimeOnlyFromTicks(timeSpan.ticks);
}
export function _a305982aa6859677(dateTime) {
  let milliseconds = ((dateTime.date.getHours() * 60 + dateTime.date.getMinutes()) * 60 + dateTime.date.getSeconds()) * 1000 + dateTime.date.getMilliseconds();
  return new JTimeOnly(BigInt(milliseconds) * BigInt("10000") + dateTime.subMillisecondTicks);
}
export function _3ae6313d263b390f(instance) {
  return new JTimeSpan(instance.ticks);
}
export function _b08fb6c2056f6cd2(instance, value) {
  if (instance.ticks < value.ticks)
    return -1;
  if (instance.ticks > value.ticks)
    return 1;
  return 0;
}
export function _fa5c092641b8d1d5(instance, value) {
  if (value === null)
    return 1;
  let other = value;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type TimeOnly.");
  return _b08fb6c2056f6cd2(instance, other);
}
export function _f6e2f8f76d2b030d(instance, value) {
  return instance.ticks === value.ticks;
}
export function _f70c423884fcb611(instance, value) {
  let other = value;
  return other !== null && _f6e2f8f76d2b030d(instance, other);
}
export function _ec44c7db9ffc5397(instance) {
  return RuntimeModule.getInt64HashCode(instance.ticks);
}
export function _5c89b5211b528926(s, provider, style) {
  let styleValue = getDateTimeStylesValue(style);
  if (!isSupportedDateTimeStyles(styleValue))
    throw new Error("ArgumentException: The only supported DateTimeStyles values are AllowLeadingWhite, AllowTrailingWhite, AllowInnerWhite, and AllowWhiteSpaces.");
  return parseCore(s);
}
export function _c2335ab7e556bf0b(s) {
  return parseCore(s);
}
export function _b10aeed232e37ce3(s, provider, style) {
  return _5c89b5211b528926(s, provider, style);
}
export function _94c68599373e4134(s, result) {
  return _ee7de3e005ab6751(s, result);
}
export function _33c24989822cc33a(s, provider, style, result) {
  let styleValue = getDateTimeStylesValue(style);
  if (!isSupportedDateTimeStyles(styleValue))
    return [false, new JTimeOnly(BigInt.zero)];
  return _ee7de3e005ab6751(s, result);
}
export function _ee7de3e005ab6751(s, result) {
  if (s === null || s.length === 0)
    return [false, new JTimeOnly(BigInt.zero)];
  try {
    return [true, parseCore(s)];
  } catch {
    return [false, new JTimeOnly(BigInt.zero)];
  }
}
export function _c9d76d7d723eb7f2(s, provider, style, result) {
  let styleValue = getDateTimeStylesValue(style);
  if (!isSupportedDateTimeStyles(styleValue))
    return [false, new JTimeOnly(BigInt.zero)];
  return _ee7de3e005ab6751(s, result);
}
export function _237d7e75836b3e58(instance) {
  return instance.toString();
}
export function _656ad6fcd28355ef(instance) {
  return instance.toString();
}
export function _b95bf75d8e4cc6af(instance, format) {
  return instance.toString();
}
export function _c2fe4568a7f1bbeb(instance, provider) {
  return instance.toString();
}
export function _dd80539f727e11c1(instance, format, provider) {
  return instance.toString();
}
export function _ef54bbdfdbe24915(s, provider) {
  return parseCore(s);
}
export function _8fea7e8fcaae2f91(s, provider, result) {
  return _ee7de3e005ab6751(s, result);
}
export function _ae9862bc80a4bba9(s, provider) {
  return parseCore(s);
}
export function _1c2553fed0fac496(s, provider, result) {
  return _ee7de3e005ab6751(s, result);
}
export const TimeOnlyModule = {
  get_TicksPerDay,
  get_TicksPerHour,
  get_TicksPerMinute,
  get_TicksPerSecond,
  get_AllowedDateTimeStylesMask,
  isAsciiDigit,
  isDigits,
  validateTimeParts,
  createTimeOnly,
  createRoundedTicksFromDouble,
  addWithWrappedDays,
  createTimeOnlyFromTicks,
  parseCore,
  getDateTimeStylesValue,
  isSupportedDateTimeStyles,
  _9f78f92d0753f4cf,
  _5a02197e2ef2252f,
  _b1d0e19d91dbb54a,
  _62d395c56c4c299d,
  _e9a3481b3456aad4,
  _335167098e226ccf,
  _28c8cb012fe0e547,
  _b8b3b95e8b848f44,
  _201ef41481f4e3fb,
  _009addd612610031,
  _b9481eedd6cbeb99,
  _3c789a48d39d0010,
  _a091b803b851e27e,
  _656df0ee12e92399,
  _2fd46050126234ac,
  _4c935b985e7b6e02,
  _31bb07d031379025,
  _8e71fa0d2695e84f,
  _ad6cad38823a5ef6,
  _77bd7db30cbf3bc9,
  _e698cb9920401887,
  _da64e8d379a7e47c,
  _8e47d4212be3070c,
  _b3b712e75fff0050,
  _341a3f0fbcda5677,
  _0656cf79f08fd69b,
  _9b001b8f9a72a57d,
  _cd098f438100d4cb,
  _888a9b439de5e7c1,
  _d6170153a1f10bc3,
  _d36793074735968e,
  _b349a5fd892d33be,
  _1f5bb15cea73f15b,
  _df2fe8c100ae98f0,
  _a305982aa6859677,
  _3ae6313d263b390f,
  _b08fb6c2056f6cd2,
  _fa5c092641b8d1d5,
  _f6e2f8f76d2b030d,
  _f70c423884fcb611,
  _ec44c7db9ffc5397,
  _5c89b5211b528926,
  _c2335ab7e556bf0b,
  _b10aeed232e37ce3,
  _94c68599373e4134,
  _33c24989822cc33a,
  _ee7de3e005ab6751,
  _c9d76d7d723eb7f2,
  _237d7e75836b3e58,
  _656ad6fcd28355ef,
  _b95bf75d8e4cc6af,
  _c2fe4568a7f1bbeb,
  _dd80539f727e11c1,
  _ef54bbdfdbe24915,
  _8fea7e8fcaae2f91,
  _ae9862bc80a4bba9,
  _1c2553fed0fac496
};
