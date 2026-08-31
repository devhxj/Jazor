import { CreateLocalDateTime, CreateUtcDate, GetDaysInMonth, JDateOnly, JDateTime } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function get_MaxDayNumber() {
  return 3652058;
}
function get_AllowedDateTimeStylesMask() {
  return 7;
}
function EnsureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function AddMonthsCore(instance, months) {
  EnsureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");
  let monthIndex = (instance.year - 1) * 12 + (instance.month - 1) + months;
  let newYear = Math.floor(monthIndex / 12) + 1;
  let newMonthIndex = monthIndex % 12;
  if (newMonthIndex < 0)
    newMonthIndex += 12;
  let newMonth = newMonthIndex + 1;
  let daysInMonth = GetDaysInMonth(newYear, newMonth);
  let newDay = instance.day > daysInMonth ? daysInMonth : instance.day;
  return new JDateOnly(newYear, newMonth, newDay);
}
function CreateFromDayNumber(dayNumber) {
  EnsureWholeNumber(dayNumber, "ArgumentOutOfRangeException: Day number must be a whole number.");
  if (dayNumber < 0 || dayNumber > get_MaxDayNumber())
    throw new Error("ArgumentOutOfRangeException: Day number must be within the range of DateOnly.");
  let date = CreateUtcDate(1, 1, 1);
  date.setUTCDate(date.getUTCDate() + dayNumber);
  return new JDateOnly(date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}
function GetDateTimeKind(kind) {
  let value = Number(kind);
  if (value !== 0 && value !== 1 && value !== 2)
    throw new Error("ArgumentException: Invalid DateTimeKind value.");
  return value;
}
function IsAsciiDigit(value) {
  return value.charCodeAt(0) >= "0".charCodeAt(0) && value.charCodeAt(0) <= "9".charCodeAt(0);
}
function TryParseIsoDate(text, year, month, day) {
  year = 0;
  month = 0;
  day = 0;
  if (text.length !== 10 || _5ad63706a889c294(text, 4).charCodeAt(0) !== "-".charCodeAt(0) || _5ad63706a889c294(text, 7).charCodeAt(0) !== "-".charCodeAt(0))
    return [false, year, month, day];
  for (let i = 0; i < text.length; i++) {
    if (i === 4 || i === 7)
      continue;
    if (!IsAsciiDigit(_5ad63706a889c294(text, i)))
      return [false, year, month, day];
  }
  year = Number(text.substring(0, 0 + 4));
  month = Number(text.substring(5, 5 + 2));
  day = Number(text.substring(8, 8 + 2));
  if (year < 1 || year > 9999 || month < 1 || month > 12)
    return [false, year, month, day];
  let daysInMonth = GetDaysInMonth(year, month);
  return [day >= 1 && day <= daysInMonth, year, month, day];
}
function HasIsoDatePrefix(text) {
  return text.length >= 10 && _5ad63706a889c294(text, 4).charCodeAt(0) === "-".charCodeAt(0) && _5ad63706a889c294(text, 7).charCodeAt(0) === "-".charCodeAt(0) && IsAsciiDigit(_5ad63706a889c294(text, 0)) && IsAsciiDigit(_5ad63706a889c294(text, 1)) && IsAsciiDigit(_5ad63706a889c294(text, 2)) && IsAsciiDigit(_5ad63706a889c294(text, 3)) && IsAsciiDigit(_5ad63706a889c294(text, 5)) && IsAsciiDigit(_5ad63706a889c294(text, 6)) && IsAsciiDigit(_5ad63706a889c294(text, 8)) && IsAsciiDigit(_5ad63706a889c294(text, 9));
}
function ParseCore(s) {
  let year, month, day, __ref$49152389c057a51f8f1ea677;
  let text = s.trim();
  if (text.length === 0)
    throw new Error("FormatException: String was not recognized as a valid DateOnly.");
  if (__ref$49152389c057a51f8f1ea677 = TryParseIsoDate(text, undefined, undefined, undefined), year = __ref$49152389c057a51f8f1ea677[1], month = __ref$49152389c057a51f8f1ea677[2], day = __ref$49152389c057a51f8f1ea677[3], __ref$49152389c057a51f8f1ea677[0])
    return new JDateOnly(year, month, day);
  if (HasIsoDatePrefix(text))
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid DateOnly.`);
  let parsed = new Date(text);
  if (isNaN(parsed.getTime()))
    throw new Error(`FormatException: String '${s ?? ""}' was not recognized as a valid DateOnly.`);
  return new JDateOnly(parsed.getFullYear(), parsed.getMonth() + 1, parsed.getDate());
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
/*jazor:clr-member System.DateOnly.DateOnly()*/
export function _5f8053a9657a0844() {
  return new JDateOnly(1, 1, 1);
}
/*jazor:clr-member static System.DateOnly.MinValue.get*/
export function _4ab7a6677b34a52b() {
  return new JDateOnly(1, 1, 1);
}
/*jazor:clr-member static System.DateOnly.MaxValue.get*/
export function _d3542025e0317ea5() {
  return new JDateOnly(9999, 12, 31);
}
/*jazor:clr-member System.DateOnly.DateOnly(int, int, int)*/
export function _8c5a25d777626c6c(year, month, day) {
  return new JDateOnly(year, month, day);
}
/*jazor:clr-member System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)*/
export function _c0568bfa1df0ef59(year, month, day, calendar) {
  return new JDateOnly(year, month, day);
}
/*jazor:clr-member static System.DateOnly.FromDayNumber(int)*/
export function _96a80b211a70154c(dayNumber) {
  return CreateFromDayNumber(dayNumber);
}
/*jazor:clr-member System.DateOnly.Year.get*/
export function _eeb6f43b5386f459(instance) {
  return instance.year;
}
/*jazor:clr-member System.DateOnly.Month.get*/
export function _c189199a72fa745c(instance) {
  return instance.month;
}
/*jazor:clr-member System.DateOnly.Day.get*/
export function _fa637ab5d7ac92a4(instance) {
  return instance.day;
}
/*jazor:clr-member System.DateOnly.DayOfWeek.get*/
export function _faf7aaba77d4de0c(instance) {
  let date = CreateUtcDate(instance.year, instance.month, instance.day);
  return date.getUTCDay();
}
/*jazor:clr-member System.DateOnly.DayOfYear.get*/
export function _6eb4f28206445ae2(instance) {
  let firstDayNumber = new JDateOnly(instance.year, 1, 1).dayNumber;
  return instance.dayNumber - firstDayNumber + 1;
}
/*jazor:clr-member System.DateOnly.DayNumber.get*/
export function _04663ba34bb3359d(instance) {
  return instance.dayNumber;
}
/*jazor:clr-member System.DateOnly.AddDays(int)*/
export function _cb25738994c034e6(instance, value) {
  EnsureWholeNumber(value, "ArgumentOutOfRangeException: Days value must be a whole number.");
  let date = CreateUtcDate(instance.year, instance.month, instance.day);
  date.setUTCDate(date.getUTCDate() + value);
  return new JDateOnly(date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}
/*jazor:clr-member System.DateOnly.AddMonths(int)*/
export function _48134214e63fd9f3(instance, value) {
  return AddMonthsCore(instance, value);
}
/*jazor:clr-member System.DateOnly.AddYears(int)*/
export function _267d01eded65ff1c(instance, value) {
  EnsureWholeNumber(value, "ArgumentOutOfRangeException: Years value must be a whole number.");
  return AddMonthsCore(instance, value * 12);
}
/*jazor:clr-member static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)*/
export function _82086262cc7cfc9f(left, right) {
  return left.dayNumber === right.dayNumber;
}
/*jazor:clr-member static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)*/
export function _56cd63706d2066a6(left, right) {
  return left.dayNumber !== right.dayNumber;
}
/*jazor:clr-member static System.DateOnly.operator >(System.DateOnly, System.DateOnly)*/
export function _9b5d78026d232bd9(left, right) {
  return left.dayNumber > right.dayNumber;
}
/*jazor:clr-member static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)*/
export function _0c9d48e09790b085(left, right) {
  return left.dayNumber >= right.dayNumber;
}
/*jazor:clr-member static System.DateOnly.operator <(System.DateOnly, System.DateOnly)*/
export function _5384e5a8b5389bd2(left, right) {
  return left.dayNumber < right.dayNumber;
}
/*jazor:clr-member static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)*/
export function _ba9123a74024d518(left, right) {
  return left.dayNumber <= right.dayNumber;
}
/*jazor:clr-member System.DateOnly.Deconstruct(out int, out int, out int)*/
export function _87be25300884e7c8(instance, year, month, day) {
  return [instance.year, instance.month, instance.day];
}
/*jazor:clr-member System.DateOnly.ToDateTime(System.TimeOnly)*/
export function _877770696b013f43(instance, time) {
  let totalMilliseconds = Number(time.ticks / BigInt(10000));
  let subMillisecondTicks = time.ticks % BigInt(10000);
  let hour = Math.floor(totalMilliseconds / 3600000);
  let minute = Math.floor(totalMilliseconds / 60000) % 60;
  let second = Math.floor(totalMilliseconds / 1000) % 60;
  let millisecond = totalMilliseconds % 1000;
  return new JDateTime("$ctor_9eb10cd821441a68", CreateLocalDateTime(instance.year, instance.month, instance.day, hour, minute, second, millisecond), 0, subMillisecondTicks);
}
/*jazor:clr-member System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)*/
export function _458cbe4dafb71f56(instance, time, kind) {
  let result = _877770696b013f43(instance, time);
  return new JDateTime("$ctor_9eb10cd821441a68", result.date, GetDateTimeKind(kind), result.subMillisecondTicks);
}
/*jazor:clr-member static System.DateOnly.FromDateTime(System.DateTime)*/
export function _8aa4a7a01276329d(dateTime) {
  return new JDateOnly(dateTime.date.getFullYear(), dateTime.date.getMonth() + 1, dateTime.date.getDate());
}
/*jazor:clr-member System.DateOnly.CompareTo(System.DateOnly)*/
export function _e80970d38580b553(instance, value) {
  if (instance.dayNumber < value.dayNumber)
    return -1;
  if (instance.dayNumber > value.dayNumber)
    return 1;
  return 0;
}
/*jazor:clr-member System.DateOnly.CompareTo(object)*/
export function _519a37b30f165f47(instance, value) {
  if (value === null)
    return 1;
  let other = value instanceof JDateOnly ? value : null;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type DateOnly.");
  return _e80970d38580b553(instance, other);
}
/*jazor:clr-member System.DateOnly.Equals(System.DateOnly)*/
export function _3c738069b4f977d8(instance, value) {
  return instance.dayNumber === value.dayNumber;
}
/*jazor:clr-member override System.DateOnly.Equals(object)*/
export function _48e30250a65786cc(instance, value) {
  let other = value instanceof JDateOnly ? value : null;
  return other !== null && _3c738069b4f977d8(instance, other);
}
/*jazor:clr-member override System.DateOnly.GetHashCode()*/
export function _6ea6fdcc8ab0282e(instance) {
  return instance.dayNumber;
}
/*jazor:clr-member static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)*/
export function _ec2f441fb253f83c(s, provider, style) {
  let styleValue = GetDateTimeStylesValue(style);
  if (!IsSupportedDateTimeStyles(styleValue))
    throw new Error("ArgumentException: The only supported DateTimeStyles values are AllowLeadingWhite, AllowTrailingWhite, AllowInnerWhite, and AllowWhiteSpaces.");
  return ParseCore(s);
}
/*jazor:clr-member static System.DateOnly.Parse(string)*/
export function _e2640560d207afce(s) {
  return ParseCore(s);
}
/*jazor:clr-member static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)*/
export function _60b758dae2c14037(s, provider, style) {
  return _ec2f441fb253f83c(s, provider, style);
}
/*jazor:clr-member static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)*/
export function _589f2bd8e9539a93(s, result) {
  return _b14e4d5a572477d0(s, result);
}
/*jazor:clr-member static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)*/
export function _0df2e2de9cba3b73(s, provider, style, result) {
  let styleValue = GetDateTimeStylesValue(style);
  if (!IsSupportedDateTimeStyles(styleValue))
    return [false, new JDateOnly(1, 1, 1)];
  return _b14e4d5a572477d0(s, result);
}
/*jazor:clr-member static System.DateOnly.TryParse(string, out System.DateOnly)*/
export function _b14e4d5a572477d0(s, result) {
  if (s === null || s.length === 0)
    return [false, new JDateOnly(1, 1, 1)];
  try {
    return [true, ParseCore(s)];
  } catch {
    return [false, new JDateOnly(1, 1, 1)];
  }
}
/*jazor:clr-member static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)*/
export function _025d467c3006d36b(s, provider, style, result) {
  let styleValue = GetDateTimeStylesValue(style);
  if (!IsSupportedDateTimeStyles(styleValue))
    return [false, new JDateOnly(1, 1, 1)];
  return _b14e4d5a572477d0(s, result);
}
/*jazor:clr-member System.DateOnly.ToLongDateString()*/
export function _28b00aeb94d7ea8a(instance) {
  return instance.toString();
}
/*jazor:clr-member System.DateOnly.ToShortDateString()*/
export function _2853e304d94edbd5(instance) {
  return instance.toString();
}
/*jazor:clr-member System.DateOnly.ToString(string)*/
export function _5dd96e58e55f801c(instance, format) {
  return instance.toString();
}
/*jazor:clr-member System.DateOnly.ToString(System.IFormatProvider)*/
export function _4a8e04add813d3bc(instance, provider) {
  return instance.toString();
}
/*jazor:clr-member System.DateOnly.ToString(string, System.IFormatProvider)*/
export function _6135867fb7290a07(instance, format, provider) {
  return instance.toString();
}
/*jazor:clr-member static System.DateOnly.Parse(string, System.IFormatProvider)*/
export function _90dcc7a43f944613(s, provider) {
  return ParseCore(s);
}
/*jazor:clr-member static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)*/
export function _09af445002e82710(s, provider, result) {
  return _b14e4d5a572477d0(s, result);
}
/*jazor:clr-member static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _18323464e5af4054(s, provider) {
  return ParseCore(s);
}
/*jazor:clr-member static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)*/
export function _e876a9d582a79f6a(s, provider, result) {
  return _b14e4d5a572477d0(s, result);
}
