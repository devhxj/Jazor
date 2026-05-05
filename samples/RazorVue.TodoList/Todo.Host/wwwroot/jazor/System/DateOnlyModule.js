import { JDateOnly, JDateTime, RuntimeModule } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function get_MaxDayNumber() {
  return 3652058;
}
function get_AllowedDateTimeStylesMask() {
  return 7;
}
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function addMonthsCore(instance, months) {
  ensureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");
  let monthIndex = (instance.year - 1) * 12 + (instance.month - 1) + months;
  let newYear = Math.floor(monthIndex / 12) + 1;
  let newMonthIndex = monthIndex % 12;
  if (newMonthIndex < 0)
    newMonthIndex += 12;
  let newMonth = newMonthIndex + 1;
  let daysInMonth = RuntimeModule.getDaysInMonth(newYear, newMonth);
  let newDay = instance.day > daysInMonth ? daysInMonth : instance.day;
  return new JDateOnly(newYear, newMonth, newDay);
}
function createFromDayNumber(dayNumber) {
  ensureWholeNumber(dayNumber, "ArgumentOutOfRangeException: Day number must be a whole number.");
  if (dayNumber < 0 || dayNumber > maxDayNumber)
    throw new Error("ArgumentOutOfRangeException: Day number must be within the range of DateOnly.");
  let date = RuntimeModule.createUtcDate(1, 1, 1);
  date.setUTCDate(date.getUTCDate() + dayNumber);
  return new JDateOnly(date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}
function getDateTimeKind(kind) {
  let value = Number(kind);
  if (value !== 0 && value !== 1 && value !== 2)
    throw new Error("ArgumentException: Invalid DateTimeKind value.");
  return value;
}
function isAsciiDigit(value) {
  return value >= "0" && value <= "9";
}
function tryParseIsoDate(text, year, month, day) {
  year = 0;
  month = 0;
  day = 0;
  if (text.length !== 10 || _5ad63706a889c294(text, 4) !== "-" || _5ad63706a889c294(text, 7) !== "-")
    return [false, year, month, day];
  for (let i = 0; i < text.length; i++) {
    if (i === 4 || i === 7)
      continue;
    if (!isAsciiDigit(_5ad63706a889c294(text, i)))
      return [false, year, month, day];
  }
  year = Number(text.substring(0, 0 + 4));
  month = Number(text.substring(5, 5 + 2));
  day = Number(text.substring(8, 8 + 2));
  if (year < 1 || year > 9999 || month < 1 || month > 12)
    return [false, year, month, day];
  let daysInMonth = RuntimeModule.getDaysInMonth(year, month);
  return [day >= 1 && day <= daysInMonth, year, month, day];
}
function parseCore(s) {
  let year, month, day, __ref$fd5a5c31803cdb92f525e38a;
  let text = s.trim();
  if (text.length === 0)
    throw new Error("FormatException: String was not recognized as a valid DateOnly.");
  if (__ref$fd5a5c31803cdb92f525e38a = tryParseIsoDate(text, year, month, day), year = __ref$fd5a5c31803cdb92f525e38a[1], month = __ref$fd5a5c31803cdb92f525e38a[2], day = __ref$fd5a5c31803cdb92f525e38a[3], __ref$fd5a5c31803cdb92f525e38a[0])
    return new JDateOnly(year, month, day);
  let parsed = new Date(text);
  if (isNaN(parsed.getTime()))
    throw new Error(`FormatException: String '${s}' was not recognized as a valid DateOnly.`);
  return new JDateOnly(parsed.getFullYear(), parsed.getMonth() + 1, parsed.getDate());
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
export function _5f8053a9657a0844() {
  return new JDateOnly(1, 1, 1);
}
export function _4ab7a6677b34a52b() {
  return new JDateOnly(1, 1, 1);
}
export function _d3542025e0317ea5() {
  return new JDateOnly(9999, 12, 31);
}
export function _8c5a25d777626c6c(year, month, day) {
  return new JDateOnly(year, month, day);
}
export function _c0568bfa1df0ef59(year, month, day, calendar) {
  return new JDateOnly(year, month, day);
}
export function _96a80b211a70154c(dayNumber) {
  return createFromDayNumber(dayNumber);
}
export function _eeb6f43b5386f459(instance) {
  return instance.year;
}
export function _c189199a72fa745c(instance) {
  return instance.month;
}
export function _fa637ab5d7ac92a4(instance) {
  return instance.day;
}
export function _faf7aaba77d4de0c(instance) {
  let date = RuntimeModule.createUtcDate(instance.year, instance.month, instance.day);
  return date.getUTCDay();
}
export function _6eb4f28206445ae2(instance) {
  let date = RuntimeModule.createUtcDate(instance.year, instance.month, instance.day);
  let start = RuntimeModule.createUtcDate(instance.year, 1, 0);
  let diff = date.getTime() - start.getTime();
  let oneDay = 1000 * 60 * 60 * 24;
  return Math.floor(diff / oneDay);
}
export function _04663ba34bb3359d(instance) {
  return instance.dayNumber;
}
export function _cb25738994c034e6(instance, value) {
  ensureWholeNumber(value, "ArgumentOutOfRangeException: Days value must be a whole number.");
  let date = RuntimeModule.createUtcDate(instance.year, instance.month, instance.day);
  date.setUTCDate(date.getUTCDate() + value);
  return new JDateOnly(date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}
export function _48134214e63fd9f3(instance, value) {
  return addMonthsCore(instance, value);
}
export function _267d01eded65ff1c(instance, value) {
  ensureWholeNumber(value, "ArgumentOutOfRangeException: Years value must be a whole number.");
  return addMonthsCore(instance, value * 12);
}
export function _82086262cc7cfc9f(left, right) {
  return left.dayNumber === right.dayNumber;
}
export function _56cd63706d2066a6(left, right) {
  return left.dayNumber !== right.dayNumber;
}
export function _9b5d78026d232bd9(left, right) {
  return left.dayNumber > right.dayNumber;
}
export function _0c9d48e09790b085(left, right) {
  return left.dayNumber >= right.dayNumber;
}
export function _5384e5a8b5389bd2(left, right) {
  return left.dayNumber < right.dayNumber;
}
export function _ba9123a74024d518(left, right) {
  return left.dayNumber <= right.dayNumber;
}
export function _87be25300884e7c8(instance, year, month, day) {
  return [instance.year, instance.month, instance.day];
}
export function _877770696b013f43(instance, time) {
  let totalMilliseconds = Number(time.ticks / BigInt(10000));
  let subMillisecondTicks = time.ticks % BigInt(10000);
  let hour = Math.floor(totalMilliseconds / 3600000);
  let minute = Math.floor(totalMilliseconds / 60000) % 60;
  let second = Math.floor(totalMilliseconds / 1000) % 60;
  let millisecond = totalMilliseconds % 1000;
  return new JDateTime(RuntimeModule.createLocalDateTime(instance.year, instance.month, instance.day, hour, minute, second, millisecond), 0, subMillisecondTicks);
}
export function _458cbe4dafb71f56(instance, time, kind) {
  let result = _877770696b013f43(instance, time);
  return new JDateTime(result.date, getDateTimeKind(kind), result.subMillisecondTicks);
}
export function _8aa4a7a01276329d(dateTime) {
  return new JDateOnly(dateTime.date.getFullYear(), dateTime.date.getMonth() + 1, dateTime.date.getDate());
}
export function _e80970d38580b553(instance, value) {
  if (instance.dayNumber < value.dayNumber)
    return -1;
  if (instance.dayNumber > value.dayNumber)
    return 1;
  return 0;
}
export function _519a37b30f165f47(instance, value) {
  if (value === null)
    return 1;
  let other = value;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type DateOnly.");
  return _e80970d38580b553(instance, other);
}
export function _3c738069b4f977d8(instance, value) {
  return instance.dayNumber === value.dayNumber;
}
export function _48e30250a65786cc(instance, value) {
  let other = value;
  return other !== null && _3c738069b4f977d8(instance, other);
}
export function _6ea6fdcc8ab0282e(instance) {
  return instance.dayNumber;
}
export function _ec2f441fb253f83c(s, provider, style) {
  let styleValue = getDateTimeStylesValue(style);
  if (!isSupportedDateTimeStyles(styleValue))
    throw new Error("ArgumentException: The only supported DateTimeStyles values are AllowLeadingWhite, AllowTrailingWhite, AllowInnerWhite, and AllowWhiteSpaces.");
  return parseCore(s);
}
export function _e2640560d207afce(s) {
  return parseCore(s);
}
export function _60b758dae2c14037(s, provider, style) {
  return _ec2f441fb253f83c(s, provider, style);
}
export function _589f2bd8e9539a93(s, result) {
  return _b14e4d5a572477d0(s, result);
}
export function _0df2e2de9cba3b73(s, provider, style, result) {
  let styleValue = getDateTimeStylesValue(style);
  if (!isSupportedDateTimeStyles(styleValue))
    return [false, new JDateOnly(1, 1, 1)];
  return _b14e4d5a572477d0(s, result);
}
export function _b14e4d5a572477d0(s, result) {
  if (s === null || s.length === 0)
    return [false, new JDateOnly(1, 1, 1)];
  try {
    return [true, parseCore(s)];
  } catch {
    return [false, new JDateOnly(1, 1, 1)];
  }
}
export function _025d467c3006d36b(s, provider, style, result) {
  let styleValue = getDateTimeStylesValue(style);
  if (!isSupportedDateTimeStyles(styleValue))
    return [false, new JDateOnly(1, 1, 1)];
  return _b14e4d5a572477d0(s, result);
}
export function _28b00aeb94d7ea8a(instance) {
  return instance.toString();
}
export function _2853e304d94edbd5(instance) {
  return instance.toString();
}
export function _5dd96e58e55f801c(instance, format) {
  return instance.toString();
}
export function _4a8e04add813d3bc(instance, provider) {
  return instance.toString();
}
export function _6135867fb7290a07(instance, format, provider) {
  return instance.toString();
}
export function _90dcc7a43f944613(s, provider) {
  return parseCore(s);
}
export function _09af445002e82710(s, provider, result) {
  return _b14e4d5a572477d0(s, result);
}
export function _18323464e5af4054(s, provider) {
  return parseCore(s);
}
export function _e876a9d582a79f6a(s, provider, result) {
  return _b14e4d5a572477d0(s, result);
}
export const DateOnlyModule = {
  get_MaxDayNumber,
  get_AllowedDateTimeStylesMask,
  ensureWholeNumber,
  addMonthsCore,
  createFromDayNumber,
  getDateTimeKind,
  isAsciiDigit,
  tryParseIsoDate,
  parseCore,
  getDateTimeStylesValue,
  isSupportedDateTimeStyles,
  _5f8053a9657a0844,
  _4ab7a6677b34a52b,
  _d3542025e0317ea5,
  _8c5a25d777626c6c,
  _c0568bfa1df0ef59,
  _96a80b211a70154c,
  _eeb6f43b5386f459,
  _c189199a72fa745c,
  _fa637ab5d7ac92a4,
  _faf7aaba77d4de0c,
  _6eb4f28206445ae2,
  _04663ba34bb3359d,
  _cb25738994c034e6,
  _48134214e63fd9f3,
  _267d01eded65ff1c,
  _82086262cc7cfc9f,
  _56cd63706d2066a6,
  _9b5d78026d232bd9,
  _0c9d48e09790b085,
  _5384e5a8b5389bd2,
  _ba9123a74024d518,
  _87be25300884e7c8,
  _877770696b013f43,
  _458cbe4dafb71f56,
  _8aa4a7a01276329d,
  _e80970d38580b553,
  _519a37b30f165f47,
  _3c738069b4f977d8,
  _48e30250a65786cc,
  _6ea6fdcc8ab0282e,
  _ec2f441fb253f83c,
  _e2640560d207afce,
  _60b758dae2c14037,
  _589f2bd8e9539a93,
  _0df2e2de9cba3b73,
  _b14e4d5a572477d0,
  _025d467c3006d36b,
  _28b00aeb94d7ea8a,
  _2853e304d94edbd5,
  _5dd96e58e55f801c,
  _4a8e04add813d3bc,
  _6135867fb7290a07,
  _90dcc7a43f944613,
  _09af445002e82710,
  _18323464e5af4054,
  _e876a9d582a79f6a
};
