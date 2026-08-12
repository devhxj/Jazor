import { _3353d31b02f2bed8, _aae197b95f9024a4, _eb38dc04224730ea, _fad0c74e1c9df5bb } from "System/DateTimeModule.js";
import { JDateTime, JGregorianCalendar, createLocalDateTime, getDaysInMonth } from "System/RuntimeModule.js";
function get_CurrentEra() {
  return 0;
}
function get_DefaultTwoDigitYearMax() {
  return 2049;
}
function get_LocalizedCalendarType() {
  return 1;
}
function get_USEnglishCalendarType() {
  return 2;
}
function get_MiddleEastFrenchCalendarType() {
  return 9;
}
function get_ArabicCalendarType() {
  return 10;
}
function get_TransliteratedEnglishCalendarType() {
  return 11;
}
function get_TransliteratedFrenchCalendarType() {
  return 12;
}
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function getCalendarTypeValue(type) {
  let numberType, enumType;
  if (typeof type === "number" && (numberType = type, true))
    return numberType;
  if (typeof type === "number" && (enumType = type, true))
    return Number(enumType);
  throw new Error("ArgumentException: Invalid GregorianCalendarTypes value.");
}
function validateCalendarType(type) {
  let value = getCalendarTypeValue(type);
  ensureWholeNumber(value, "ArgumentOutOfRangeException: GregorianCalendarTypes value was not valid.");
  if (value !== get_LocalizedCalendarType() && value !== get_USEnglishCalendarType() && value !== get_MiddleEastFrenchCalendarType() && value !== get_ArabicCalendarType() && value !== get_TransliteratedEnglishCalendarType() && value !== get_TransliteratedFrenchCalendarType())
    throw new Error("ArgumentOutOfRangeException: GregorianCalendarTypes value was not valid.");
}
function validateEra(era) {
  ensureWholeNumber(era, "ArgumentOutOfRangeException: Era must be a whole number.");
  if (era !== get_CurrentEra() && era !== _fa491b52106d378d())
    throw new Error("ArgumentOutOfRangeException: Era value was not valid.");
}
function validateYear(year) {
  ensureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number.");
  if (year < 1 || year > 9999)
    throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
}
function validateMonth(year, month) {
  validateYear(year);
  ensureWholeNumber(month, "ArgumentOutOfRangeException: Month must be a whole number.");
  if (month < 1 || month > 12)
    throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
}
function validateDate(year, month, day) {
  validateMonth(year, month);
  ensureWholeNumber(day, "ArgumentOutOfRangeException: Day must be a whole number.");
  if (day < 1 || day > getDaysInMonth(year, month))
    throw new Error("ArgumentOutOfRangeException: Day is out of range for the specified month and year.");
}
export function _13ca7ecb3e3aade5(instance) {
  return _fad0c74e1c9df5bb();
}
export function _7ba83b2ccdd567b5(instance) {
  return _eb38dc04224730ea();
}
export function _2c293866a460d9ea(instance) {
  return 1;
}
export function _23b9e8d671b5210e() {
  return new JGregorianCalendar(get_LocalizedCalendarType(), get_DefaultTwoDigitYearMax());
}
export function _c043a86ee7a70c81(type) {
  validateCalendarType(type);
  return new JGregorianCalendar(getCalendarTypeValue(type), get_DefaultTwoDigitYearMax());
}
export function _33a82cf70a73ecdd(instance) {
  return instance.calendarType;
}
export function _ab29134350e86147(instance, value) {
  validateCalendarType(value);
  instance.calendarType = getCalendarTypeValue(value);
}
export function _1c4bd410ce12db05(instance, time, months) {
  return _aae197b95f9024a4(time, months);
}
export function _705c207141cada42(instance, time, years) {
  return _3353d31b02f2bed8(time, years);
}
export function _5f5d0a874674bdea(instance, time) {
  return time.date.getDate();
}
export function _6cdddcc68587ea95(instance, time) {
  return time.date.getDay();
}
export function _81e475ed63f62602(instance, time) {
  let year = time.date.getFullYear();
  let start = Date.UTC(year, 0, 0);
  let current = Date.UTC(year, time.date.getMonth(), time.date.getDate());
  return Math.floor((current - start) / 86400000);
}
export function _ce58c7d4d1c36fe3(instance, year, month, era) {
  validateEra(era);
  return getDaysInMonth(year, month);
}
export function _7545c4d66f0f3604(instance, year, era) {
  validateEra(era);
  validateYear(year);
  return _4c3723e9b82aa507(instance, year, era) ? 366 : 365;
}
export function _21a6ebc60ed3b388(instance, time) {
  return 1;
}
export function _c01c2927eaf2fefe(instance) {
  return [1];
}
export function _ce76f400b1aa26d3(instance, time) {
  return time.date.getMonth() + 1;
}
export function _5df8d3230f9681b9(instance, year, era) {
  validateEra(era);
  validateYear(year);
  return 12;
}
export function _fd5a2cde6fb4d6f5(instance, time) {
  return time.date.getFullYear();
}
export function _10c29328b0ef4014(instance, year, month, day, era) {
  validateEra(era);
  validateDate(year, month, day);
  return month === 2 && day === 29 && _4c3723e9b82aa507(instance, year, era);
}
export function _91a08597c1c93445(instance, year, era) {
  validateEra(era);
  validateYear(year);
  return 0;
}
export function _9917941c9da950b5(instance, year, month, era) {
  validateEra(era);
  validateMonth(year, month);
  return false;
}
export function _4c3723e9b82aa507(instance, year, era) {
  validateEra(era);
  validateYear(year);
  return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
}
export function _29ccd13d5e5508f8(instance, year, month, day, hour, minute, second, millisecond, era) {
  validateEra(era);
  return new JDateTime(createLocalDateTime(year, month, day, hour, minute, second, millisecond), 0);
}
export function _e32c11e11fbe2e3b(instance) {
  return instance.twoDigitYearMax;
}
export function _9537b0490ec80689(instance, value) {
  ensureWholeNumber(value, "ArgumentOutOfRangeException: TwoDigitYearMax must be a whole number.");
  if (value < 99 || value > 9999)
    throw new Error("ArgumentOutOfRangeException: TwoDigitYearMax must be between 99 and 9999.");
  instance.twoDigitYearMax = value;
}
export function _cca1b99b56b6a322(instance, year) {
  ensureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number.");
  if (year < 0 || year > 9999)
    throw new Error("ArgumentOutOfRangeException: Year must be between 0 and 9999.");
  if (year >= 100)
    return year;
  let century = Math.floor(instance.twoDigitYearMax / 100) * 100;
  let pivot = instance.twoDigitYearMax % 100;
  return year <= pivot ? century + year : century - 100 + year;
}
