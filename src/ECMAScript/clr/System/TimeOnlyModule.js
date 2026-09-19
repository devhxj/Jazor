import { IsFiniteCore, IsNaNCore } from "System/DoubleModule.js";
import { GetInt64HashCode, JTimeOnly, JTimeSpan } from "System/RuntimeModule.js";
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
function ValidateTimeParts(hour, minute, second, millisecond, microsecond) {
  if (Math.floor(hour) !== hour || Math.floor(minute) !== minute || Math.floor(second) !== second || Math.floor(millisecond) !== millisecond || Math.floor(microsecond) !== microsecond)
    throw new Error("ArgumentOutOfRangeException: TimeOnly components must be whole numbers.");
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999 || microsecond < 0 || microsecond > 999)
    throw new Error("ArgumentOutOfRangeException: One or more TimeOnly components are out of range.");
}
function CreateTimeOnly(hour, minute, second, millisecond, microsecond) {
  ValidateTimeParts(hour, minute, second, millisecond, microsecond);
  return new JTimeOnly(BigInt(hour) * get_TicksPerHour() + BigInt(minute) * get_TicksPerMinute() + BigInt(second) * get_TicksPerSecond() + BigInt(millisecond) * BigInt("10000") + BigInt(microsecond) * BigInt(10));
}
function CreateTruncatedTicksFromDouble(value) {
  if (IsNaNCore(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!IsFiniteCore(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  return BigInt(Math.trunc(value));
}
function AddWithWrappedDays(instance, deltaTicks) {
  let total = instance.ticks + deltaTicks;
  let wrapped = Number(total / get_TicksPerDay());
  let result = total % get_TicksPerDay();
  if (result < 0n) {
    result += get_TicksPerDay();
    wrapped--;
  }
  return [new JTimeOnly(result), wrapped];
}
function CreateTimeOnlyFromTicks(ticks) {
  if (ticks < 0n || ticks >= get_TicksPerDay())
    throw new Error("ArgumentOutOfRangeException: TimeOnly ticks must be within a single day.");
  return new JTimeOnly(ticks);
}
function ParseCore(s) {
  let text = s.trim();
  if (text.length === 0)
    throw new Error("FormatException: String was not recognized as a valid TimeOnly.");
  let first = text.indexOf(":");
  if (first < 0)
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid TimeOnly.`);
  let second = text.indexOf(":", first + 1);
  let hourText = text.substring(0, 0 + first);
  let minuteText = second < 0 ? text.substring(first + 1) : ((__jz_arg0, __jz_arg1, __jz_arg2) => __jz_arg0.substring(__jz_arg1, __jz_arg1 + __jz_arg2))(text, first + 1, second - first - 1);
  if (!IsDigits(hourText) || !IsDigits(minuteText))
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid TimeOnly.`);
  let hour = Number(hourText);
  let minute = Number(minuteText);
  let secondValue = 0;
  let fractionTicks = 0n;
  if (second >= 0) {
    let fractionIndex = text.indexOf(".", second + 1);
    let secondText = fractionIndex < 0 ? text.substring(second + 1) : ((__jz_arg0, __jz_arg1, __jz_arg2) => __jz_arg0.substring(__jz_arg1, __jz_arg1 + __jz_arg2))(text, second + 1, fractionIndex - second - 1);
    if (!IsDigits(secondText))
      throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid TimeOnly.`);
    secondValue = Number(secondText);
    if (fractionIndex >= 0) {
      let fractionText = text.substring(fractionIndex + 1);
      if (fractionText.length === 0 || fractionText.length > 7 || !IsDigits(fractionText))
        throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid TimeOnly.`);
      while (fractionText.length < 7)
        fractionText += "0";
      fractionTicks = BigInt(fractionText);
    }
  }
  if (isNaN(hour) || isNaN(minute) || isNaN(secondValue))
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid TimeOnly.`);
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || secondValue < 0 || secondValue > 59)
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid TimeOnly.`);
  return new JTimeOnly(BigInt(hour) * get_TicksPerHour() + BigInt(minute) * get_TicksPerMinute() + BigInt(secondValue) * get_TicksPerSecond() + fractionTicks);
}
function GetDateTimeStylesValue(style) {
  let numberStyle;
  if (typeof style === "number" && (numberStyle = style, true))
    return numberStyle;
  if (style === null)
    return 0;
  throw new Error("ArgumentException: Invalid DateTimeStyles value.");
}
function IsSupportedDateTimeStyles(style) {
  return style >= 0 && Math.floor(style) === style && (style & ~get_AllowedDateTimeStylesMask()) === 0;
}
/*jazor:clr-member System.TimeOnly.TimeOnly()*/
export function _9f78f92d0753f4cf() {
  return new JTimeOnly(0n);
}
/*jazor:clr-member static System.TimeOnly.MinValue.get*/
export function _5a02197e2ef2252f() {
  return new JTimeOnly(0n);
}
/*jazor:clr-member static System.TimeOnly.MaxValue.get*/
export function _b1d0e19d91dbb54a() {
  return new JTimeOnly(BigInt("863999999999"));
}
/*jazor:clr-member System.TimeOnly.TimeOnly(int, int)*/
export function _62d395c56c4c299d(hour, minute) {
  return CreateTimeOnly(hour, minute, 0, 0, 0);
}
/*jazor:clr-member System.TimeOnly.TimeOnly(int, int, int)*/
export function _e9a3481b3456aad4(hour, minute, second) {
  return CreateTimeOnly(hour, minute, second, 0, 0);
}
/*jazor:clr-member System.TimeOnly.TimeOnly(int, int, int, int)*/
export function _335167098e226ccf(hour, minute, second, millisecond) {
  return CreateTimeOnly(hour, minute, second, millisecond, 0);
}
/*jazor:clr-member System.TimeOnly.TimeOnly(int, int, int, int, int)*/
export function _28c8cb012fe0e547(hour, minute, second, millisecond, microsecond) {
  return CreateTimeOnly(hour, minute, second, millisecond, microsecond);
}
/*jazor:clr-member System.TimeOnly.TimeOnly(long)*/
export function _b8b3b95e8b848f44(ticks) {
  return CreateTimeOnlyFromTicks(ticks);
}
/*jazor:clr-member System.TimeOnly.Hour.get*/
export function _201ef41481f4e3fb(instance) {
  return Number(instance.ticks / get_TicksPerHour() % BigInt(24));
}
/*jazor:clr-member System.TimeOnly.Minute.get*/
export function _009addd612610031(instance) {
  return Number(instance.ticks / get_TicksPerMinute() % BigInt(60));
}
/*jazor:clr-member System.TimeOnly.Second.get*/
export function _b9481eedd6cbeb99(instance) {
  return Number(instance.ticks / get_TicksPerSecond() % BigInt(60));
}
/*jazor:clr-member System.TimeOnly.Millisecond.get*/
export function _3c789a48d39d0010(instance) {
  return Number(instance.ticks / BigInt(10000) % BigInt(1000));
}
/*jazor:clr-member System.TimeOnly.Microsecond.get*/
export function _a091b803b851e27e(instance) {
  return Number(instance.ticks / BigInt(10) % BigInt(1000));
}
/*jazor:clr-member System.TimeOnly.Nanosecond.get*/
export function _656df0ee12e92399(instance) {
  return Number(instance.ticks % BigInt(10) * BigInt(100));
}
/*jazor:clr-member System.TimeOnly.Ticks.get*/
export function _2fd46050126234ac(instance) {
  return instance.ticks;
}
/*jazor:clr-member System.TimeOnly.Add(System.TimeSpan)*/
export function _4c935b985e7b6e02(instance, value) {
  return new JTimeOnly(instance.ticks + value.ticks);
}
/*jazor:clr-member System.TimeOnly.Add(System.TimeSpan, out int)*/
export function _31bb07d031379025(instance, value, wrappedDays) {
  let total = instance.ticks + value.ticks;
  let wrapped = Number(total / get_TicksPerDay());
  let result = total % get_TicksPerDay();
  if (result < 0n) {
    result += get_TicksPerDay();
    wrapped--;
  }
  return [new JTimeOnly(result), wrapped];
}
/*jazor:clr-member System.TimeOnly.AddHours(double)*/
export function _8e71fa0d2695e84f(instance, value) {
  let delta = new JTimeSpan(CreateTruncatedTicksFromDouble(value * 36000000000));
  return _4c935b985e7b6e02(instance, delta);
}
/*jazor:clr-member System.TimeOnly.AddHours(double, out int)*/
export function _ad6cad38823a5ef6(instance, value, wrappedDays) {
  return AddWithWrappedDays(instance, CreateTruncatedTicksFromDouble(value * 36000000000));
}
/*jazor:clr-member System.TimeOnly.AddMinutes(double)*/
export function _77bd7db30cbf3bc9(instance, value) {
  let delta = new JTimeSpan(CreateTruncatedTicksFromDouble(value * 600000000));
  return _4c935b985e7b6e02(instance, delta);
}
/*jazor:clr-member System.TimeOnly.AddMinutes(double, out int)*/
export function _e698cb9920401887(instance, value, wrappedDays) {
  return AddWithWrappedDays(instance, CreateTruncatedTicksFromDouble(value * 600000000));
}
/*jazor:clr-member System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)*/
export function _da64e8d379a7e47c(instance, start, end) {
  return start.ticks < end.ticks ? instance.ticks >= start.ticks && instance.ticks < end.ticks : instance.ticks >= start.ticks || instance.ticks < end.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator ==(System.TimeOnly, System.TimeOnly)*/
export function _8e47d4212be3070c(left, right) {
  return left.ticks === right.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator !=(System.TimeOnly, System.TimeOnly)*/
export function _b3b712e75fff0050(left, right) {
  return left.ticks !== right.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator >(System.TimeOnly, System.TimeOnly)*/
export function _341a3f0fbcda5677(left, right) {
  return left.ticks > right.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator >=(System.TimeOnly, System.TimeOnly)*/
export function _0656cf79f08fd69b(left, right) {
  return left.ticks >= right.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator <(System.TimeOnly, System.TimeOnly)*/
export function _9b001b8f9a72a57d(left, right) {
  return left.ticks < right.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator <=(System.TimeOnly, System.TimeOnly)*/
export function _cd098f438100d4cb(left, right) {
  return left.ticks <= right.ticks;
}
/*jazor:clr-member static System.TimeOnly.operator -(System.TimeOnly, System.TimeOnly)*/
export function _888a9b439de5e7c1(t1, t2) {
  let diff = t1.ticks - t2.ticks;
  return new JTimeSpan(diff < 0n ? diff + get_TicksPerDay() : diff);
}
/*jazor:clr-member System.TimeOnly.Deconstruct(out int, out int)*/
export function _d6170153a1f10bc3(instance, hour, minute) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance)];
}
/*jazor:clr-member System.TimeOnly.Deconstruct(out int, out int, out int)*/
export function _d36793074735968e(instance, hour, minute, second) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance)];
}
/*jazor:clr-member System.TimeOnly.Deconstruct(out int, out int, out int, out int)*/
export function _b349a5fd892d33be(instance, hour, minute, second, millisecond) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance), _3c789a48d39d0010(instance)];
}
/*jazor:clr-member System.TimeOnly.Deconstruct(out int, out int, out int, out int, out int)*/
export function _1f5bb15cea73f15b(instance, hour, minute, second, millisecond, microsecond) {
  return [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance), _3c789a48d39d0010(instance), _a091b803b851e27e(instance)];
}
/*jazor:clr-member static System.TimeOnly.FromTimeSpan(System.TimeSpan)*/
export function _df2fe8c100ae98f0(timeSpan) {
  return CreateTimeOnlyFromTicks(timeSpan.ticks);
}
/*jazor:clr-member static System.TimeOnly.FromDateTime(System.DateTime)*/
export function _a305982aa6859677(dateTime) {
  let milliseconds = ((dateTime.date.getHours() * 60 + dateTime.date.getMinutes()) * 60 + dateTime.date.getSeconds()) * 1000 + dateTime.date.getMilliseconds();
  return new JTimeOnly(BigInt(milliseconds) * BigInt("10000") + dateTime.subMillisecondTicks);
}
/*jazor:clr-member System.TimeOnly.ToTimeSpan()*/
export function _3ae6313d263b390f(instance) {
  return new JTimeSpan(instance.ticks);
}
/*jazor:clr-member System.TimeOnly.CompareTo(System.TimeOnly)*/
export function _b08fb6c2056f6cd2(instance, value) {
  if (instance.ticks < value.ticks)
    return -1;
  if (instance.ticks > value.ticks)
    return 1;
  return 0;
}
/*jazor:clr-member System.TimeOnly.CompareTo(object)*/
export function _fa5c092641b8d1d5(instance, value) {
  if (value === null)
    return 1;
  let other = value instanceof JTimeOnly ? value : null;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type TimeOnly.");
  return _b08fb6c2056f6cd2(instance, other);
}
/*jazor:clr-member System.TimeOnly.Equals(System.TimeOnly)*/
export function _f6e2f8f76d2b030d(instance, value) {
  return instance.ticks === value.ticks;
}
/*jazor:clr-member override System.TimeOnly.Equals(object)*/
export function _f70c423884fcb611(instance, value) {
  let other = value instanceof JTimeOnly ? value : null;
  return other !== null && _f6e2f8f76d2b030d(instance, other);
}
/*jazor:clr-member override System.TimeOnly.GetHashCode()*/
export function _ec44c7db9ffc5397(instance) {
  return GetInt64HashCode(instance.ticks);
}
/*jazor:clr-member static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)*/
export function _5c89b5211b528926(s, provider, style) {
  let styleValue = GetDateTimeStylesValue(style);
  if (!IsSupportedDateTimeStyles(styleValue))
    throw new Error("ArgumentException: The only supported DateTimeStyles values are AllowLeadingWhite, AllowTrailingWhite, AllowInnerWhite, and AllowWhiteSpaces.");
  return ParseCore(s);
}
/*jazor:clr-member static System.TimeOnly.Parse(string)*/
export function _c2335ab7e556bf0b(s) {
  return ParseCore(s);
}
/*jazor:clr-member static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)*/
export function _b10aeed232e37ce3(s, provider, style) {
  return _5c89b5211b528926(s, provider, style);
}
/*jazor:clr-member static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, out System.TimeOnly)*/
export function _94c68599373e4134(s, result) {
  return _ee7de3e005ab6751(s, result);
}
/*jazor:clr-member static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)*/
export function _33c24989822cc33a(s, provider, style, result) {
  let styleValue = GetDateTimeStylesValue(style);
  if (!IsSupportedDateTimeStyles(styleValue))
    return [false, new JTimeOnly(0n)];
  return _ee7de3e005ab6751(s, result);
}
/*jazor:clr-member static System.TimeOnly.TryParse(string, out System.TimeOnly)*/
export function _ee7de3e005ab6751(s, result) {
  if (s === null || s.length === 0)
    return [false, new JTimeOnly(0n)];
  try {
    return [true, ParseCore(s)];
  } catch {
    return [false, new JTimeOnly(0n)];
  }
}
/*jazor:clr-member static System.TimeOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)*/
export function _c9d76d7d723eb7f2(s, provider, style, result) {
  let styleValue = GetDateTimeStylesValue(style);
  if (!IsSupportedDateTimeStyles(styleValue))
    return [false, new JTimeOnly(0n)];
  return _ee7de3e005ab6751(s, result);
}
/*jazor:clr-member System.TimeOnly.ToLongTimeString()*/
export function _237d7e75836b3e58(instance) {
  return instance.toString();
}
/*jazor:clr-member System.TimeOnly.ToShortTimeString()*/
export function _656ad6fcd28355ef(instance) {
  return instance.toString();
}
/*jazor:clr-member System.TimeOnly.ToString(string)*/
export function _b95bf75d8e4cc6af(instance, format) {
  return instance.toString();
}
/*jazor:clr-member System.TimeOnly.ToString(System.IFormatProvider)*/
export function _c2fe4568a7f1bbeb(instance, provider) {
  return instance.toString();
}
/*jazor:clr-member System.TimeOnly.ToString(string, System.IFormatProvider)*/
export function _dd80539f727e11c1(instance, format, provider) {
  return instance.toString();
}
/*jazor:clr-member static System.TimeOnly.Parse(string, System.IFormatProvider)*/
export function _ef54bbdfdbe24915(s, provider) {
  return ParseCore(s);
}
/*jazor:clr-member static System.TimeOnly.TryParse(string, System.IFormatProvider, out System.TimeOnly)*/
export function _8fea7e8fcaae2f91(s, provider, result) {
  return _ee7de3e005ab6751(s, result);
}
/*jazor:clr-member static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _ae9862bc80a4bba9(s, provider) {
  return ParseCore(s);
}
/*jazor:clr-member static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeOnly)*/
export function _1c2553fed0fac496(s, provider, result) {
  return _ee7de3e005ab6751(s, result);
}
