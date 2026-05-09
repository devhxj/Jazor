import { _24e14b276e0c7e30, _aed2927097617729 } from "System/DoubleModule.js";
import { JDateOnly, JDateTime, JDateTimeOffset, JTimeOnly, JTimeSpan, createLocalDate, createLocalDateTime, createUtcDate, formatDateOnlyText, getDaysInMonth, getInt64HashCode, pad2, pad7, padLeft } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function get_ZeroTicks() {
  return 0n;
}
function get_UnixEpochTicks() {
  return BigInt("621355968000000000");
}
function get_FileTimeUnixEpochTicks() {
  return BigInt("116444736000000000");
}
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
function get_OffsetMinuteTicks() {
  return BigInt("600000000");
}
function get_MaxOffsetTicks() {
  return BigInt("504000000000");
}
function get_MaxDateTimeTicks() {
  return BigInt("3155378975999999999");
}
function get_MinUnixTimeMilliseconds() {
  return BigInt("-62135596800000");
}
function get_MaxUnixTimeMilliseconds() {
  return BigInt("253402300799999");
}
function get_MinUnixTimeSeconds() {
  return BigInt("-62135596800");
}
function get_MaxUnixTimeSeconds() {
  return BigInt("253402300799");
}
function get_DateTimeKindUtc() {
  return 1;
}
function get_DateTimeKindLocal() {
  return 2;
}
function get_MinValueMilliseconds() {
  return -62135596800000;
}
function get_DateTimeStylesNoCurrentDateDefault() {
  return 8;
}
function get_DateTimeStylesAdjustToUniversal() {
  return 16;
}
function get_DateTimeStylesAssumeLocal() {
  return 32;
}
function get_DateTimeStylesAssumeUniversal() {
  return 64;
}
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function createDateTimeOffset(utcDateTime, offsetTicks) {
  return CreateDateTimeOffset(utcDateTime, offsetTicks, get_ZeroTicks());
}
function CreateDateTimeOffset(utcDateTime, offsetTicks, utcSubMillisecondTicks) {
  let utcTicks = BigInt(utcDateTime.getTime()) * get_TicksPerMillisecond() + utcSubMillisecondTicks + get_UnixEpochTicks();
  validateDateTimeOffsetRange(utcTicks, offsetTicks);
  return new JDateTimeOffset(utcDateTime, offsetTicks, utcSubMillisecondTicks);
}
function createDefaultDateTimeOffset() {
  return createDateTimeOffset(new Date(get_MinValueMilliseconds()), get_ZeroTicks());
}
function validateDateTimeOffsetRange(utcTicks, offsetTicks) {
  validateOffsetTicks(offsetTicks);
  let ticks = utcTicks + offsetTicks;
  if (ticks < get_ZeroTicks() || ticks > get_MaxDateTimeTicks())
    throw new Error("ArgumentOutOfRangeException: The UTC time and offset must produce a DateTimeOffset within range.");
}
function validateOffsetTicks(offsetTicks) {
  if (offsetTicks % get_OffsetMinuteTicks() !== 0n)
    throw new Error("ArgumentException: Offset must be specified in whole minutes.");
  if (offsetTicks < -get_MaxOffsetTicks() || offsetTicks > get_MaxOffsetTicks())
    throw new Error("ArgumentOutOfRangeException: Offset must be within plus or minus 14 hours.");
}
function validateMicrosecond(microsecond) {
  if (Math.floor(microsecond) !== microsecond || microsecond < 0 || microsecond > 999)
    throw new Error("ArgumentOutOfRangeException: Microsecond must be between 0 and 999.");
}
function createLocalTicks(year, month, day, hour, minute, second, millisecond) {
  let utc = createUtcDate(year, month, day);
  utc.setUTCHours(hour, minute, second, millisecond);
  if (utc.getUTCFullYear() !== year || utc.getUTCMonth() + 1 !== month || utc.getUTCDate() !== day || utc.getUTCHours() !== hour || utc.getUTCMinutes() !== minute || utc.getUTCSeconds() !== second || utc.getUTCMilliseconds() !== millisecond)
    throw new Error("ArgumentOutOfRangeException: The supplied date or time component is out of range.");
  return BigInt(utc.getTime()) * get_TicksPerMillisecond() + get_UnixEpochTicks();
}
function getDateTimeInstantTicks(dateTime) {
  if (dateTime.kind === get_DateTimeKindUtc()) {
    let date = dateTime.date;
    return BigInt(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds())) * get_TicksPerMillisecond() + dateTime.subMillisecondTicks + get_UnixEpochTicks();
  }
  return BigInt(dateTime.date.getTime()) * get_TicksPerMillisecond() + dateTime.subMillisecondTicks + get_UnixEpochTicks();
}
function getDateTimeTicks(dateTime) {
  let date = dateTime.date;
  return BigInt(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds())) * get_TicksPerMillisecond() + dateTime.subMillisecondTicks + get_UnixEpochTicks();
}
function getUtcTicks(instance) {
  return BigInt(instance.utcDateTime.getTime()) * get_TicksPerMillisecond() + instance.utcSubMillisecondTicks + get_UnixEpochTicks();
}
function getTicks(instance) {
  return getUtcTicks(instance) + instance.offsetTicks;
}
function createFromUtcTicks(utcTicks, offsetTicks) {
  validateDateTimeOffsetRange(utcTicks, offsetTicks);
  let ticksSinceUnixEpoch = utcTicks - get_UnixEpochTicks();
  let milliseconds = ticksSinceUnixEpoch / get_TicksPerMillisecond();
  let utcSubMillisecondTicks = ticksSinceUnixEpoch % get_TicksPerMillisecond();
  if (utcSubMillisecondTicks < get_ZeroTicks()) {
    milliseconds -= BigInt(1);
    utcSubMillisecondTicks += get_TicksPerMillisecond();
  }
  return CreateDateTimeOffset(new Date(Number(milliseconds)), offsetTicks, utcSubMillisecondTicks);
}
function normalizeSubMillisecondTicks(ticks) {
  let remainder = (ticks - get_UnixEpochTicks()) % get_TicksPerMillisecond();
  return remainder < get_ZeroTicks() ? remainder + get_TicksPerMillisecond() : remainder;
}
function validateOffset(offset) {
  validateOffsetTicks(offset.ticks);
}
function addMonthsCore(instance, months) {
  ensureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  let year = local.getUTCFullYear();
  let monthIndex = (year - 1) * 12 + local.getUTCMonth() + months;
  let newYear = Math.floor(monthIndex / 12) + 1;
  let newMonthIndex = monthIndex % 12;
  if (newMonthIndex < 0)
    newMonthIndex += 12;
  let newMonth = newMonthIndex + 1;
  let day = local.getUTCDate();
  let daysInMonth = getDaysInMonth(newYear, newMonth);
  let newDay = day > daysInMonth ? daysInMonth : day;
  let localTicks = createLocalTicks(newYear, newMonth, newDay, local.getUTCHours(), local.getUTCMinutes(), local.getUTCSeconds(), local.getUTCMilliseconds()) + instance.utcSubMillisecondTicks;
  return createFromUtcTicks(localTicks - instance.offsetTicks, instance.offsetTicks);
}
function createWithLocalOffset(utcDateTime) {
  let offsetTicks = BigInt(-utcDateTime.getTimezoneOffset()) * get_OffsetMinuteTicks();
  return createDateTimeOffset(utcDateTime, offsetTicks);
}
function CreateWithLocalOffset(utcTicks) {
  let utcDateTime = createFromUtcTicks(utcTicks, get_ZeroTicks()).utcDateTime;
  let offsetTicks = BigInt(-utcDateTime.getTimezoneOffset()) * get_OffsetMinuteTicks();
  return createFromUtcTicks(utcTicks, offsetTicks);
}
function getOffsetLocalDate(instance) {
  return new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
}
function getProviderLocale(provider) {
  let locale, numberFormat;
  if (typeof provider === "string" && (locale = provider, true))
    return locale;
  if (provider instanceof Intl.NumberFormat && (numberFormat = provider, true))
    return numberFormat.resolvedOptions().locale;
  return (new Intl.DateTimeFormat).resolvedOptions().locale;
}
function joinFormatParts(parts) {
  let text = "";
  for (let i = 0; i < parts.length; i++)
    text += parts[i].value;
  return text;
}
function getInvariantMonthName(month) {
  switch (month | 0) {
    case 1:
      return "January";
    case 2:
      return "February";
    case 3:
      return "March";
    case 4:
      return "April";
    case 5:
      return "May";
    case 6:
      return "June";
    case 7:
      return "July";
    case 8:
      return "August";
    case 9:
      return "September";
    case 10:
      return "October";
    case 11:
      return "November";
    case 12:
      return "December";
    default:
      throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
  }
}
function getInvariantAbbreviatedMonthName(month) {
  switch (month | 0) {
    case 1:
      return "Jan";
    case 2:
      return "Feb";
    case 3:
      return "Mar";
    case 4:
      return "Apr";
    case 5:
      return "May";
    case 6:
      return "Jun";
    case 7:
      return "Jul";
    case 8:
      return "Aug";
    case 9:
      return "Sep";
    case 10:
      return "Oct";
    case 11:
      return "Nov";
    case 12:
      return "Dec";
    default:
      throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
  }
}
function getInvariantDayName(dayOfWeek) {
  switch (dayOfWeek | 0) {
    case 0:
      return "Sunday";
    case 1:
      return "Monday";
    case 2:
      return "Tuesday";
    case 3:
      return "Wednesday";
    case 4:
      return "Thursday";
    case 5:
      return "Friday";
    case 6:
      return "Saturday";
    default:
      throw new Error("ArgumentOutOfRangeException: DayOfWeek must be between 0 and 6.");
  }
}
function getInvariantAbbreviatedDayName(dayOfWeek) {
  switch (dayOfWeek | 0) {
    case 0:
      return "Sun";
    case 1:
      return "Mon";
    case 2:
      return "Tue";
    case 3:
      return "Wed";
    case 4:
      return "Thu";
    case 5:
      return "Fri";
    case 6:
      return "Sat";
    default:
      throw new Error("ArgumentOutOfRangeException: DayOfWeek must be between 0 and 6.");
  }
}
function isAsciiLetter(value) {
  return value >= "A" && value <= "Z" || value >= "a" && value <= "z";
}
function getLocalizedMonthName(locale, month, abbreviated) {
  if (locale.length === 0)
    return abbreviated ? getInvariantAbbreviatedMonthName(month) : getInvariantMonthName(month);
  return joinFormatParts(new Intl.DateTimeFormat(locale, { month: abbreviated ? "short" : "long", timeZone: "UTC" }).formatToParts(new Date(Date.UTC(2000, month - 1, 1))));
}
function getLocalizedDayName(locale, dayOfWeek, abbreviated) {
  if (locale.length === 0)
    return abbreviated ? getInvariantAbbreviatedDayName(dayOfWeek) : getInvariantDayName(dayOfWeek);
  return joinFormatParts(new Intl.DateTimeFormat(locale, { weekday: abbreviated ? "short" : "long", timeZone: "UTC" }).formatToParts(new Date(Date.UTC(2024, 0, 7 + dayOfWeek))));
}
function getDateSeparator(locale) {
  if (locale.length === 0)
    return "/";
  let parts = new Intl.DateTimeFormat(locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    timeZone: "UTC"
  }).formatToParts(new Date(Date.UTC(2000, 0, 2)));
  for (let i = 0; i < parts.length; i++) {
    let part = parts[i];
    if (part.type === "literal" && part.value.length !== 0)
      return part.value;
  }
  return "/";
}
function getTimeSeparator(locale) {
  if (locale.length === 0)
    return ":";
  let parts = new Intl.DateTimeFormat(locale, {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: "UTC"
  }).formatToParts(new Date(Date.UTC(2000, 0, 2, 3, 4, 5)));
  for (let i = 0; i < parts.length; i++) {
    let part = parts[i];
    if (part.type === "literal" && part.value.length !== 0)
      return part.value;
  }
  return ":";
}
function getLocalizedDayPeriod(date, locale) {
  if (locale.length === 0)
    return date.getUTCHours() < 12 ? "AM" : "PM";
  let parts = new Intl.DateTimeFormat(locale, {
    hour: "numeric",
    hour12: true,
    timeZone: "UTC"
  }).formatToParts(date);
  for (let i = 0; i < parts.length; i++) {
    let part = parts[i];
    if (part.type === "dayPeriod")
      return part.value;
  }
  return date.getUTCHours() < 12 ? "AM" : "PM";
}
function formatOffsetTicks(offsetTicks, count) {
  let negative = offsetTicks < 0n;
  let absolute = negative ? -offsetTicks : offsetTicks;
  let totalMinutes = absolute / get_OffsetMinuteTicks();
  let hours = Number(totalMinutes / BigInt(60));
  let minutes = Number(totalMinutes % BigInt(60));
  let sign = negative ? "-" : "+";
  if (count <= 1)
    return sign + hours;
  if (count === 2)
    return sign + pad2(hours);
  return sign + pad2(hours) + ":" + pad2(minutes);
}
function formatInvariantGeneralDateTimeOffset(instance, includeSeconds, includeOffset) {
  let local = getOffsetLocalDate(instance);
  let text = pad2(local.getUTCMonth() + 1) + "/" + pad2(local.getUTCDate()) + "/" + padLeft(local.getUTCFullYear().toString(), 4) + " " + pad2(local.getUTCHours()) + ":" + pad2(local.getUTCMinutes());
  if (includeSeconds)
    text += ":" + pad2(local.getUTCSeconds());
  if (includeOffset)
    text += " " + formatOffsetTicks(instance.offsetTicks, 3);
  return text;
}
function formatInvariantShortDate(instance) {
  let local = getOffsetLocalDate(instance);
  return pad2(local.getUTCMonth() + 1) + "/" + pad2(local.getUTCDate()) + "/" + padLeft(local.getUTCFullYear().toString(), 4);
}
function formatInvariantLongDate(instance) {
  let local = getOffsetLocalDate(instance);
  return getInvariantDayName(local.getUTCDay()) + ", " + pad2(local.getUTCDate()) + " " + getInvariantMonthName(local.getUTCMonth() + 1) + " " + padLeft(local.getUTCFullYear().toString(), 4);
}
function formatInvariantTime(instance, includeSeconds) {
  let local = getOffsetLocalDate(instance);
  let text = pad2(local.getUTCHours()) + ":" + pad2(local.getUTCMinutes());
  if (includeSeconds)
    text += ":" + pad2(local.getUTCSeconds());
  return text;
}
function formatMonthDay(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0) {
    let local = getOffsetLocalDate(instance);
    return getInvariantMonthName(local.getUTCMonth() + 1) + " " + pad2(local.getUTCDate());
  }
  return formatOffsetLocaleDateTime(getOffsetLocalDate(instance), locale, { month: "long", day: "2-digit" });
}
function formatYearMonth(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0) {
    let local = getOffsetLocalDate(instance);
    return padLeft(local.getUTCFullYear().toString(), 4) + " " + getInvariantMonthName(local.getUTCMonth() + 1);
  }
  return formatOffsetLocaleDateTime(getOffsetLocalDate(instance), locale, { year: "numeric", month: "long" });
}
function formatFullDateTime(instance, includeSeconds, provider) {
  return formatLongDate(instance, provider) + " " + formatTime(instance, includeSeconds, provider);
}
function formatRfc1123DateTimeOffset(instance) {
  let utc = instance.utcDateTime;
  return getInvariantAbbreviatedDayName(utc.getUTCDay()) + ", " + pad2(utc.getUTCDate()) + " " + getInvariantAbbreviatedMonthName(utc.getUTCMonth() + 1) + " " + padLeft(utc.getUTCFullYear().toString(), 4) + " " + pad2(utc.getUTCHours()) + ":" + pad2(utc.getUTCMinutes()) + ":" + pad2(utc.getUTCSeconds()) + " GMT";
}
function formatLocaleDateTime(date, locale, options) {
  return joinFormatParts(new Intl.DateTimeFormat(locale, options).formatToParts(date));
}
function formatOffsetLocaleDateTime(date, locale, options) {
  return joinFormatParts(new Intl.DateTimeFormat(locale, {
    localeMatcher: options.localeMatcher,
    weekday: options.weekday,
    era: options.era,
    year: options.year,
    month: options.month,
    day: options.day,
    hour: options.hour,
    minute: options.minute,
    second: options.second,
    timeZoneName: options.timeZoneName,
    formatMatcher: options.formatMatcher,
    hour12: options.hour12,
    timeZone: "UTC"
  }).formatToParts(date));
}
function formatGeneralDateTimeOffset(instance, includeSeconds, includeOffset, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantGeneralDateTimeOffset(instance, includeSeconds, includeOffset);
  let text = formatOffsetLocaleDateTime(getOffsetLocalDate(instance), locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: includeSeconds ? "2-digit" : null,
    hour12: false
  });
  if (includeOffset)
    text += " " + formatOffsetTicks(instance.offsetTicks, 3);
  return text;
}
function formatShortDate(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantShortDate(instance);
  return formatOffsetLocaleDateTime(getOffsetLocalDate(instance), locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  });
}
function formatLongDate(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantLongDate(instance);
  return formatOffsetLocaleDateTime(getOffsetLocalDate(instance), locale, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "2-digit"
  });
}
function formatTime(instance, includeSeconds, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantTime(instance, includeSeconds);
  return formatOffsetLocaleDateTime(getOffsetLocalDate(instance), locale, {
    hour: "2-digit",
    minute: "2-digit",
    second: includeSeconds ? "2-digit" : null,
    hour12: false
  });
}
function formatUniversalSortableDateTimeOffset(instance) {
  let utc = instance.utcDateTime;
  return formatDateOnlyText(utc.getUTCFullYear(), utc.getUTCMonth() + 1, utc.getUTCDate()) + " " + pad2(utc.getUTCHours()) + ":" + pad2(utc.getUTCMinutes()) + ":" + pad2(utc.getUTCSeconds()) + "Z";
}
function formatSortableDateTimeOffset(instance) {
  let local = getOffsetLocalDate(instance);
  return formatDateOnlyText(local.getUTCFullYear(), local.getUTCMonth() + 1, local.getUTCDate()) + "T" + pad2(local.getUTCHours()) + ":" + pad2(local.getUTCMinutes()) + ":" + pad2(local.getUTCSeconds());
}
function formatFraction(fraction, count, trimTrailingZeros) {
  let text = pad7(fraction);
  if (count < 7)
    text = text.substring(0, 0 + count);
  if (!trimTrailingZeros)
    return text;
  while (text.length > 0 && _5ad63706a889c294(text, text.length - 1) === "0")
    text = text.substring(0, 0 + (text.length - 1));
  return text;
}
function formatCustomToken(instance, token, count, locale, dateSeparator, timeSeparator) {
  let local = getOffsetLocalDate(instance);
  let year = local.getUTCFullYear();
  let month = local.getUTCMonth() + 1;
  let day = local.getUTCDate();
  let hour = local.getUTCHours();
  let hour12 = hour % 12;
  if (hour12 === 0)
    hour12 = 12;
  let minute = local.getUTCMinutes();
  let second = local.getUTCSeconds();
  let fraction = BigInt(local.getUTCMilliseconds()) * get_TicksPerMillisecond() + instance.utcSubMillisecondTicks;
  switch (token) {
    case "y":
      if (count === 2)
        return pad2(year % 100);
      return padLeft(year.toString(), count < 4 ? 4 : count);
    case "M":
      if (count === 1)
        return month.toString();
      if (count === 2)
        return pad2(month);
      if (count === 3)
        return getLocalizedMonthName(locale, month, true);
      return getLocalizedMonthName(locale, month, false);
    case "d":
      if (count === 1)
        return day.toString();
      if (count === 2)
        return pad2(day);
      if (count === 3)
        return getLocalizedDayName(locale, local.getUTCDay(), true);
      return getLocalizedDayName(locale, local.getUTCDay(), false);
    case "H":
      return count === 1 ? hour.toString() : pad2(hour);
    case "h":
      return count === 1 ? hour12.toString() : pad2(hour12);
    case "m":
      return count === 1 ? minute.toString() : pad2(minute);
    case "s":
      return count === 1 ? second.toString() : pad2(second);
    case "t":
      let dayPeriod = getLocalizedDayPeriod(local, locale);
      return count === 1 ? dayPeriod.substring(0, 0 + 1) : dayPeriod;
    case "f":
      return formatFraction(fraction, count, false);
    case "F":
      return formatFraction(fraction, count, true);
    case "z":
      return formatOffsetTicks(instance.offsetTicks, count);
    case "K":
      return formatOffsetTicks(instance.offsetTicks, 3);
    case ":":
      return timeSeparator;
    case "/":
      return dateSeparator;
    default:
      let text = "";
      for (let j = 0; j < count; j++)
        text += token;
      return text;
  }
}
function formatCustomDateTimeOffset(instance, format, provider) {
  let locale = getProviderLocale(provider);
  let dateSeparator = getDateSeparator(locale);
  let timeSeparator = getTimeSeparator(locale);
  let text = "";
  for (let i = 0; i < format.length; ) {
    let token = _5ad63706a889c294(format, i);
    if (token === "%") {
      if (i + 1 >= format.length || _5ad63706a889c294(format, i + 1) === "%")
        throw new Error("FormatException: Input string was not in a correct format.");
      text += formatCustomToken(instance, _5ad63706a889c294(format, i + 1), 1, locale, dateSeparator, timeSeparator);
      i += 2;
      continue;
    }
    if (token === "\\") {
      if (i + 1 < format.length)
        text += _5ad63706a889c294(format, i + 1);
      i += 2;
      continue;
    }
    if (token === "'" || token === "\"") {
      let quote = token;
      i++;
      while (i < format.length && _5ad63706a889c294(format, i) !== quote) {
        text += _5ad63706a889c294(format, i);
        i++;
      }
      if (i < format.length)
        i++;
      continue;
    }
    let count = 1;
    while (i + count < format.length && _5ad63706a889c294(format, i + count) === token)
      count++;
    text += formatCustomToken(instance, token, count, locale, dateSeparator, timeSeparator);
    i += count;
  }
  return text;
}
function formatDateTimeOffset(instance, format, formatProvider) {
  if (format === null || format.length === 0)
    return formatGeneralDateTimeOffset(instance, true, true, formatProvider);
  if (format.length === 1) {
    switch (_5ad63706a889c294(format, 0)) {
      case "f":
        return formatFullDateTime(instance, false, formatProvider);
      case "F":
        return formatFullDateTime(instance, true, formatProvider);
      case "M":
      case "m":
        return formatMonthDay(instance, formatProvider);
      case "O":
      case "o":
        return instance.toString();
      case "G":
        return formatGeneralDateTimeOffset(instance, true, false, formatProvider);
      case "g":
        return formatGeneralDateTimeOffset(instance, false, false, formatProvider);
      case "R":
      case "r":
        return formatRfc1123DateTimeOffset(instance);
      case "d":
        return formatShortDate(instance, formatProvider);
      case "D":
        return formatLongDate(instance, formatProvider);
      case "t":
        return formatTime(instance, false, formatProvider);
      case "T":
        return formatTime(instance, true, formatProvider);
      case "s":
        return formatSortableDateTimeOffset(instance);
      case "u":
        return formatUniversalSortableDateTimeOffset(instance);
      case "Y":
      case "y":
        return formatYearMonth(instance, formatProvider);
      default:
        if (isAsciiLetter(_5ad63706a889c294(format, 0)))
          throw new Error("FormatException: Input string was not in a correct format.");
        break;
    }
  }
  return formatCustomDateTimeOffset(instance, format, formatProvider);
}
function hasUtcSuffix(input) {
  return input.endsWith("Z") || input.endsWith("z");
}
function HasExplicitOffset(input) {
  let timeIndex = input.lastIndexOf("T");
  let spaceIndex = input.lastIndexOf(" ");
  if (spaceIndex > timeIndex)
    timeIndex = spaceIndex;
  if (input.length >= 6) {
    let signIndex = input.length - 6;
    let sign = _5ad63706a889c294(input, signIndex);
    if ((sign === "+" || sign === "-") && _5ad63706a889c294(input, input.length - 3) === ":" && signIndex > timeIndex)
      return true;
  }
  if (input.length >= 5) {
    let signIndex = input.length - 5;
    let sign = _5ad63706a889c294(input, signIndex);
    if ((sign === "+" || sign === "-") && signIndex > timeIndex)
      return true;
  }
  if (input.length >= 3) {
    let signIndex = input.length - 3;
    let sign = _5ad63706a889c294(input, signIndex);
    if ((sign === "+" || sign === "-") && signIndex > timeIndex)
      return true;
  }
  return false;
}
function isAsciiDigit(value) {
  return value >= "0" && value <= "9";
}
function tryParseTwoDigits(text, start, value) {
  value = 0;
  if (start < 0 || start + 2 > text.length)
    return [false, value];
  if (!isAsciiDigit(_5ad63706a889c294(text, start)) || !isAsciiDigit(_5ad63706a889c294(text, start + 1)))
    return [false, value];
  value = Number(text.substring(start, start + 2));
  return [true, value];
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
  let daysInMonth = getDaysInMonth(year, month);
  return [day >= 1 && day <= daysInMonth, year, month, day];
}
function tryParseIsoDateTime(text, year, month, day, hour, minute, second, millisecond, subMillisecondTicks, hasExplicitOffset, offsetTicks) {
  let __ref$1598f4a39ea5406602d9c015, __ref$e3d1f2494dc188a9c3c3fb6c, __ref$9a3b5a426d06f5f0bb7b8557;
  year = 0;
  month = 0;
  day = 0;
  hour = 0;
  minute = 0;
  second = 0;
  millisecond = 0;
  subMillisecondTicks = get_ZeroTicks();
  hasExplicitOffset = false;
  offsetTicks = get_ZeroTicks();
  if (text.length < 16)
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  if (!(__ref$1598f4a39ea5406602d9c015 = tryParseIsoDate(text.substring(0, 0 + 10), year, month, day), year = __ref$1598f4a39ea5406602d9c015[1], month = __ref$1598f4a39ea5406602d9c015[2], day = __ref$1598f4a39ea5406602d9c015[3], __ref$1598f4a39ea5406602d9c015[0]))
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let separator = _5ad63706a889c294(text, 10);
  if (separator !== "T" && separator !== " ")
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  if (!(__ref$e3d1f2494dc188a9c3c3fb6c = tryParseTwoDigits(text, 11, hour), hour = __ref$e3d1f2494dc188a9c3c3fb6c[1], __ref$e3d1f2494dc188a9c3c3fb6c[0]) || _5ad63706a889c294(text, 13) !== ":" || !(__ref$9a3b5a426d06f5f0bb7b8557 = tryParseTwoDigits(text, 14, minute), minute = __ref$9a3b5a426d06f5f0bb7b8557[1], __ref$9a3b5a426d06f5f0bb7b8557[0]))
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  if (hour > 23 || minute > 59)
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let index = 16;
  if (index < text.length && _5ad63706a889c294(text, index) === ":") {
    let __ref$4da59fb5ccbf8683bd090542;
    if (!(__ref$4da59fb5ccbf8683bd090542 = tryParseTwoDigits(text, index + 1, second), second = __ref$4da59fb5ccbf8683bd090542[1], __ref$4da59fb5ccbf8683bd090542[0]))
      return [
        false,
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    if (second > 59)
      return [
        false,
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    index += 3;
  }
  if (index < text.length && _5ad63706a889c294(text, index) === ".") {
    index++;
    let fractionStart = index;
    while (index < text.length && isAsciiDigit(_5ad63706a889c294(text, index)))
      index++;
    let digits = text.substring(fractionStart, fractionStart + (index - fractionStart));
    if (digits.length === 0 || digits.length > 7)
      return [
        false,
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    while (digits.length < 7)
      digits += "0";
    millisecond = Number(digits.substring(0, 0 + 3));
    subMillisecondTicks = BigInt(digits.substring(3, 3 + 4));
  }
  if (index === text.length)
    return [
      true,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  hasExplicitOffset = true;
  if (index === text.length - 1 && (_5ad63706a889c294(text, index) === "Z" || _5ad63706a889c294(text, index) === "z"))
    return [
      true,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let sign = _5ad63706a889c294(text, index);
  if (sign !== "+" && sign !== "-")
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let remaining = text.length - index - 1;
  let offsetHours;
  let offsetMinutes;
  if (remaining === 2) {
    let __ref$cecf5e440a9bf16488614c1d;
    if (!(__ref$cecf5e440a9bf16488614c1d = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$cecf5e440a9bf16488614c1d[1], __ref$cecf5e440a9bf16488614c1d[0]))
      return [
        false,
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    offsetMinutes = 0;
  }
  else if (remaining === 4) {
    let __ref$cecf5e440a9bf16488614c1d, __ref$768b004349857c0911b90100;
    if (!(__ref$cecf5e440a9bf16488614c1d = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$cecf5e440a9bf16488614c1d[1], __ref$cecf5e440a9bf16488614c1d[0]) || !(__ref$768b004349857c0911b90100 = tryParseTwoDigits(text, index + 3, offsetMinutes), offsetMinutes = __ref$768b004349857c0911b90100[1], __ref$768b004349857c0911b90100[0]))
      return [
        false,
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
  }
  else if (remaining === 5 && _5ad63706a889c294(text, index + 3) === ":") {
    let __ref$cecf5e440a9bf16488614c1d, __ref$9fad4f969403f2523aadd5fb;
    if (!(__ref$cecf5e440a9bf16488614c1d = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$cecf5e440a9bf16488614c1d[1], __ref$cecf5e440a9bf16488614c1d[0]) || !(__ref$9fad4f969403f2523aadd5fb = tryParseTwoDigits(text, index + 4, offsetMinutes), offsetMinutes = __ref$9fad4f969403f2523aadd5fb[1], __ref$9fad4f969403f2523aadd5fb[0]))
      return [
        false,
        year,
        month,
        day,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
  }
  else {
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  }
  if (offsetHours > 14 || offsetMinutes > 59 || offsetHours === 14 && offsetMinutes !== 0)
    return [
      false,
      year,
      month,
      day,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  offsetTicks = BigInt(offsetHours * 60 + offsetMinutes) * get_OffsetMinuteTicks();
  if (sign === "-")
    offsetTicks = -offsetTicks;
  return [
    true,
    year,
    month,
    day,
    hour,
    minute,
    second,
    millisecond,
    subMillisecondTicks,
    hasExplicitOffset,
    offsetTicks
  ];
}
function tryParseTimeOnly(text, hour, minute, second, millisecond, subMillisecondTicks, hasExplicitOffset, offsetTicks) {
  let __ref$b3bbd6e07bb306f016a7cc95, __ref$cb87fefbc56ece7ba2aa2318;
  hour = 0;
  minute = 0;
  second = 0;
  millisecond = 0;
  subMillisecondTicks = get_ZeroTicks();
  hasExplicitOffset = false;
  offsetTicks = get_ZeroTicks();
  if (text.length < 5)
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  if (!(__ref$b3bbd6e07bb306f016a7cc95 = tryParseTwoDigits(text, 0, hour), hour = __ref$b3bbd6e07bb306f016a7cc95[1], __ref$b3bbd6e07bb306f016a7cc95[0]) || _5ad63706a889c294(text, 2) !== ":" || !(__ref$cb87fefbc56ece7ba2aa2318 = tryParseTwoDigits(text, 3, minute), minute = __ref$cb87fefbc56ece7ba2aa2318[1], __ref$cb87fefbc56ece7ba2aa2318[0]))
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  if (hour > 23 || minute > 59)
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let index = 5;
  if (index < text.length && _5ad63706a889c294(text, index) === ":") {
    let __ref$d6db3a5f1a4e4f151f07cfc8;
    if (!(__ref$d6db3a5f1a4e4f151f07cfc8 = tryParseTwoDigits(text, index + 1, second), second = __ref$d6db3a5f1a4e4f151f07cfc8[1], __ref$d6db3a5f1a4e4f151f07cfc8[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    if (second > 59)
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    index += 3;
  }
  if (index < text.length && _5ad63706a889c294(text, index) === ".") {
    index++;
    let fractionStart = index;
    while (index < text.length && isAsciiDigit(_5ad63706a889c294(text, index)))
      index++;
    let digits = text.substring(fractionStart, fractionStart + (index - fractionStart));
    if (digits.length === 0 || digits.length > 7)
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    while (digits.length < 7)
      digits += "0";
    millisecond = Number(digits.substring(0, 0 + 3));
    subMillisecondTicks = BigInt(digits.substring(3, 3 + 4));
  }
  if (index === text.length)
    return [
      true,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  hasExplicitOffset = true;
  if (index === text.length - 1 && (_5ad63706a889c294(text, index) === "Z" || _5ad63706a889c294(text, index) === "z"))
    return [
      true,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let sign = _5ad63706a889c294(text, index);
  if (sign !== "+" && sign !== "-")
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  let remaining = text.length - index - 1;
  let offsetHours;
  let offsetMinutes;
  if (remaining === 2) {
    let __ref$3e496c548081be1e28623d68;
    if (!(__ref$3e496c548081be1e28623d68 = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$3e496c548081be1e28623d68[1], __ref$3e496c548081be1e28623d68[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
    offsetMinutes = 0;
  }
  else if (remaining === 4) {
    let __ref$3e496c548081be1e28623d68, __ref$7ace6ceeef5560443c522d87;
    if (!(__ref$3e496c548081be1e28623d68 = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$3e496c548081be1e28623d68[1], __ref$3e496c548081be1e28623d68[0]) || !(__ref$7ace6ceeef5560443c522d87 = tryParseTwoDigits(text, index + 3, offsetMinutes), offsetMinutes = __ref$7ace6ceeef5560443c522d87[1], __ref$7ace6ceeef5560443c522d87[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
  }
  else if (remaining === 5 && _5ad63706a889c294(text, index + 3) === ":") {
    let __ref$3e496c548081be1e28623d68, __ref$e1eba571b0a0ed1646b03be2;
    if (!(__ref$3e496c548081be1e28623d68 = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$3e496c548081be1e28623d68[1], __ref$3e496c548081be1e28623d68[0]) || !(__ref$e1eba571b0a0ed1646b03be2 = tryParseTwoDigits(text, index + 4, offsetMinutes), offsetMinutes = __ref$e1eba571b0a0ed1646b03be2[1], __ref$e1eba571b0a0ed1646b03be2[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        hasExplicitOffset,
        offsetTicks
      ];
  }
  else {
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  }
  if (offsetHours > 14 || offsetMinutes > 59 || offsetHours === 14 && offsetMinutes !== 0)
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      hasExplicitOffset,
      offsetTicks
    ];
  offsetTicks = BigInt(offsetHours * 60 + offsetMinutes) * get_OffsetMinuteTicks();
  if (sign === "-")
    offsetTicks = -offsetTicks;
  return [
    true,
    hour,
    minute,
    second,
    millisecond,
    subMillisecondTicks,
    hasExplicitOffset,
    offsetTicks
  ];
}
function createAddUnitTicks(value, ticksPerUnit) {
  if (_24e14b276e0c7e30(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!_aed2927097617729(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  let maxUnitCount = Number(get_MaxDateTimeTicks()) / Number(ticksPerUnit);
  if (Math.abs(value) > maxUnitCount)
    throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTimeOffset range.");
  let integralPart = Math.trunc(value);
  let fractionalPart = value - integralPart;
  return BigInt(integralPart) * ticksPerUnit + BigInt(Math.trunc(fractionalPart * Number(ticksPerUnit)));
}
function getDateTimeStylesValue(styles) {
  let numberStyle, enumStyle;
  if (typeof styles === "number" && (numberStyle = styles, true))
    return numberStyle;
  if (typeof styles === "number" && (enumStyle = styles, true))
    return Number(enumStyle);
  if (styles === null)
    return 0;
  throw new Error("ArgumentException: Invalid DateTimeStyles value.");
}
function validateDateTimeStyles(styles) {
  if (styles < 0 || Math.floor(styles) !== styles)
    throw new Error("ArgumentException: Invalid DateTimeStyles value.");
  if ((styles & get_DateTimeStylesNoCurrentDateDefault()) !== 0)
    throw new Error("ArgumentException: NoCurrentDateDefault is not allowed when parsing DateTimeOffset.");
  let hasAssumeLocal = (styles & get_DateTimeStylesAssumeLocal()) !== 0;
  let hasAssumeUniversal = (styles & get_DateTimeStylesAssumeUniversal()) !== 0;
  if (hasAssumeLocal && hasAssumeUniversal)
    throw new Error("ArgumentException: AssumeLocal and AssumeUniversal cannot both be set.");
}
function applyDateTimeStyles(value, input, styles) {
  let styleValue = getDateTimeStylesValue(styles);
  validateDateTimeStyles(styleValue);
  let text = input.trim();
  let hasExplicitZone = hasUtcSuffix(text) || HasExplicitOffset(text);
  let adjustToUniversal = (styleValue & get_DateTimeStylesAdjustToUniversal()) !== 0;
  let assumeUniversal = (styleValue & get_DateTimeStylesAssumeUniversal()) !== 0;
  let result = value;
  if (!hasExplicitZone && assumeUniversal)
    result = createFromUtcTicks(getTicks(value), get_ZeroTicks());
  if (adjustToUniversal)
    return createFromUtcTicks(getUtcTicks(result), get_ZeroTicks());
  return result;
}
function resolveParsedOffsetTicks(input, parsedDate) {
  let timeIndex = input.lastIndexOf("T");
  let spaceIndex = input.lastIndexOf(" ");
  if (spaceIndex > timeIndex)
    timeIndex = spaceIndex;
  if (input.endsWith("Z") || input.endsWith("z"))
    return get_ZeroTicks();
  if (input.length >= 6) {
    let signIndex = input.length - 6;
    let sign = _5ad63706a889c294(input, signIndex);
    if ((sign === "+" || sign === "-") && _5ad63706a889c294(input, input.length - 3) === ":") {
      let hours = Number(input.substring(input.length - 5, input.length - 5 + 2));
      let minutes = Number(input.substring(input.length - 2, input.length - 2 + 2));
      if (signIndex > timeIndex && !isNaN(hours) && !isNaN(minutes) && minutes >= 0 && minutes < 60) {
        let ticks = BigInt(hours * 60 + minutes) * get_OffsetMinuteTicks();
        return sign === "-" ? -ticks : ticks;
      }
    }
  }
  if (input.length >= 5) {
    let signIndex = input.length - 5;
    let sign = _5ad63706a889c294(input, signIndex);
    if (sign === "+" || sign === "-") {
      let hours = Number(input.substring(input.length - 4, input.length - 4 + 2));
      let minutes = Number(input.substring(input.length - 2, input.length - 2 + 2));
      if (signIndex > timeIndex && !isNaN(hours) && !isNaN(minutes) && minutes >= 0 && minutes < 60) {
        let ticks = BigInt(hours * 60 + minutes) * get_OffsetMinuteTicks();
        return sign === "-" ? -ticks : ticks;
      }
    }
  }
  if (input.length >= 3) {
    let signIndex = input.length - 3;
    let sign = _5ad63706a889c294(input, signIndex);
    if (sign === "+" || sign === "-") {
      let hours = Number(input.substring(input.length - 2, input.length - 2 + 2));
      if (signIndex > timeIndex && !isNaN(hours)) {
        let ticks = BigInt(hours * 60) * get_OffsetMinuteTicks();
        return sign === "-" ? -ticks : ticks;
      }
    }
  }
  return BigInt(-parsedDate.getTimezoneOffset()) * get_OffsetMinuteTicks();
}
function extractSubMillisecondTicks(input) {
  let timeIndex = input.lastIndexOf("T");
  let spaceIndex = input.lastIndexOf(" ");
  if (spaceIndex > timeIndex)
    timeIndex = spaceIndex;
  let fractionIndex = input.indexOf(".", timeIndex + 1);
  if (fractionIndex < 0)
    return get_ZeroTicks();
  let end = input.length;
  for (let i = fractionIndex + 1; i < input.length; i++) {
    let c = _5ad63706a889c294(input, i);
    if (c < "0" || c > "9") {
      end = i;
      break;
    }
  }
  let digits = input.substring(fractionIndex + 1, fractionIndex + 1 + (end - fractionIndex - 1));
  if (digits.length === 0 || digits.length > 7)
    throw new Error(`FormatException: String '${input}' was not recognized as a valid DateTimeOffset.`);
  while (digits.length < 7)
    digits += "0";
  return BigInt(digits.substring(3, 3 + 4));
}
function floorDiv(value, divisor) {
  let quotient = value / divisor;
  let remainder = value % divisor;
  if (remainder < get_ZeroTicks())
    return quotient - BigInt(1);
  return quotient;
}
function parseCore(input) {
  let timeHour, timeMinute, timeSecond, timeMillisecond, timeSubMillisecondTicks, timeHasExplicitOffset, timeOffsetTicks, __ref$1306ca7647dcf73502954427, year, month, day, __ref$1f7bc41fe2e6775199bcf0e8, hour, minute, second, millisecond, subMillisecondTicks, hasExplicitOffset, offsetTicks, __ref$04911c9a3c2af607e600522e;
  let s = input.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid DateTimeOffset.");
  if (__ref$1306ca7647dcf73502954427 = tryParseTimeOnly(s, timeHour, timeMinute, timeSecond, timeMillisecond, timeSubMillisecondTicks, timeHasExplicitOffset, timeOffsetTicks), timeHour = __ref$1306ca7647dcf73502954427[1], timeMinute = __ref$1306ca7647dcf73502954427[2], timeSecond = __ref$1306ca7647dcf73502954427[3], timeMillisecond = __ref$1306ca7647dcf73502954427[4], timeSubMillisecondTicks = __ref$1306ca7647dcf73502954427[5], timeHasExplicitOffset = __ref$1306ca7647dcf73502954427[6], timeOffsetTicks = __ref$1306ca7647dcf73502954427[7], __ref$1306ca7647dcf73502954427[0]) {
    let now = new Date;
    let currentYear = now.getFullYear();
    let currentMonth = now.getMonth() + 1;
    let currentDay = now.getDate();
    if (!timeHasExplicitOffset) {
      let localDateTime = createLocalDateTime(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond);
      let localOffsetTicks = BigInt(-localDateTime.getTimezoneOffset()) * get_OffsetMinuteTicks();
      let utcTicks = BigInt(localDateTime.getTime()) * get_TicksPerMillisecond() + timeSubMillisecondTicks + get_UnixEpochTicks();
      return createFromUtcTicks(utcTicks, localOffsetTicks);
    }
    let localTicks = createLocalTicks(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond) + timeSubMillisecondTicks;
    return createFromUtcTicks(localTicks - timeOffsetTicks, timeOffsetTicks);
  }
  if (__ref$1f7bc41fe2e6775199bcf0e8 = tryParseIsoDate(s, year, month, day), year = __ref$1f7bc41fe2e6775199bcf0e8[1], month = __ref$1f7bc41fe2e6775199bcf0e8[2], day = __ref$1f7bc41fe2e6775199bcf0e8[3], __ref$1f7bc41fe2e6775199bcf0e8[0]) {
    let date = createLocalDate(year, month, day);
    let utcTicks = BigInt(date.getTime()) * get_TicksPerMillisecond() + get_UnixEpochTicks();
    return CreateWithLocalOffset(utcTicks);
  }
  if (__ref$04911c9a3c2af607e600522e = tryParseIsoDateTime(s, year, month, day, hour, minute, second, millisecond, subMillisecondTicks, hasExplicitOffset, offsetTicks), year = __ref$04911c9a3c2af607e600522e[1], month = __ref$04911c9a3c2af607e600522e[2], day = __ref$04911c9a3c2af607e600522e[3], hour = __ref$04911c9a3c2af607e600522e[4], minute = __ref$04911c9a3c2af607e600522e[5], second = __ref$04911c9a3c2af607e600522e[6], millisecond = __ref$04911c9a3c2af607e600522e[7], subMillisecondTicks = __ref$04911c9a3c2af607e600522e[8], hasExplicitOffset = __ref$04911c9a3c2af607e600522e[9], offsetTicks = __ref$04911c9a3c2af607e600522e[10], __ref$04911c9a3c2af607e600522e[0]) {
    if (!hasExplicitOffset) {
      let localDateTime = createLocalDateTime(year, month, day, hour, minute, second, millisecond);
      let localOffsetTicks = BigInt(-localDateTime.getTimezoneOffset()) * get_OffsetMinuteTicks();
      let utcTicks = BigInt(localDateTime.getTime()) * get_TicksPerMillisecond() + subMillisecondTicks + get_UnixEpochTicks();
      return createFromUtcTicks(utcTicks, localOffsetTicks);
    }
    let localTicks = createLocalTicks(year, month, day, hour, minute, second, millisecond) + subMillisecondTicks;
    return createFromUtcTicks(localTicks - offsetTicks, offsetTicks);
  }
  let parsed = new Date(s);
  if (isNaN(parsed.getTime()))
    throw new Error(`FormatException: String '${input}' was not recognized as a valid DateTimeOffset.`);
  let resolvedOffsetTicks = resolveParsedOffsetTicks(s, parsed);
  if (resolvedOffsetTicks < -get_MaxOffsetTicks() || resolvedOffsetTicks > get_MaxOffsetTicks())
    throw new Error(`FormatException: String '${input}' was not recognized as a valid DateTimeOffset.`);
  let parsedSubMillisecondTicks = extractSubMillisecondTicks(s);
  return createFromUtcTicks(BigInt(parsed.getTime()) * get_TicksPerMillisecond() + parsedSubMillisecondTicks + get_UnixEpochTicks(), resolvedOffsetTicks);
}
export function _77107f0c23675b69() {
  return createDefaultDateTimeOffset();
}
export function _d45d439f0b97ae0e() {
  return CreateDateTimeOffset(new Date(253402300799999), get_ZeroTicks(), BigInt("9999"));
}
export function _087cabaedc1b5cc2() {
  return createDateTimeOffset(new Date(0), get_ZeroTicks());
}
export function _12b4f3f1dc14bea9() {
  return createDefaultDateTimeOffset();
}
export function _1e9c5d2a64e6d41d(ticks, offset) {
  validateOffset(offset);
  return createFromUtcTicks(ticks - offset.ticks, offset.ticks);
}
export function _7adf69a53659433a(dateTime) {
  if (dateTime.kind === get_DateTimeKindUtc())
    return createFromUtcTicks(getDateTimeInstantTicks(dateTime), get_ZeroTicks());
  let instantTicks = getDateTimeInstantTicks(dateTime);
  return CreateWithLocalOffset(instantTicks);
}
export function _106dabc0cc502aa4(dateTime, offset) {
  validateOffset(offset);
  if (dateTime.kind === get_DateTimeKindUtc()) {
    if (offset.ticks !== get_ZeroTicks())
      throw new Error("ArgumentException: The UTC Offset for Utc DateTime instances must be 0.");
    return createFromUtcTicks(getDateTimeInstantTicks(dateTime), get_ZeroTicks());
  }
  if (dateTime.kind === get_DateTimeKindLocal()) {
    let expectedOffset = BigInt(-dateTime.date.getTimezoneOffset()) * get_OffsetMinuteTicks();
    if (expectedOffset !== offset.ticks)
      throw new Error("ArgumentException: The UTC Offset of the local dateTime parameter does not match the offset argument.");
  }
  return createFromUtcTicks(getDateTimeTicks(dateTime) - offset.ticks, offset.ticks);
}
export function _8f1aab77eeb6f786(date, time, offset) {
  validateOffset(offset);
  let localTicks = createLocalTicks(date.year, date.month, date.day, 0, 0, 0, 0) + time.ticks;
  return createFromUtcTicks(localTicks - offset.ticks, offset.ticks);
}
export function _d90dce0e1d2f06e4(year, month, day, hour, minute, second, offset) {
  validateOffset(offset);
  let localTicks = createLocalTicks(year, month, day, hour, minute, second, 0);
  return createFromUtcTicks(localTicks - offset.ticks, offset.ticks);
}
export function _6abaa2b2082f575c(year, month, day, hour, minute, second, millisecond, offset) {
  validateOffset(offset);
  let localTicks = createLocalTicks(year, month, day, hour, minute, second, millisecond);
  return createFromUtcTicks(localTicks - offset.ticks, offset.ticks);
}
export function _04123d597aa761a3(year, month, day, hour, minute, second, millisecond, microsecond, offset) {
  validateOffset(offset);
  validateMicrosecond(microsecond);
  let localTicks = createLocalTicks(year, month, day, hour, minute, second, millisecond) + BigInt(microsecond) * BigInt("10");
  return createFromUtcTicks(localTicks - offset.ticks, offset.ticks);
}
export function _7f444d9ce7391e15() {
  return createDateTimeOffset(new Date, get_ZeroTicks());
}
export function _2b7dd675863ae961(instance) {
  let localTicks = getTicks(instance);
  return new JDateTime(createLocalDateTime(Number(_127105b7a40a7665(instance)), Number(_79eb4c93cea58d59(instance)), Number(_ba8df912681fe784(instance)), Number(_b7fc65477ef4df45(instance)), Number(_0fe8054b55f9f1c7(instance)), Number(_822de224fed5bb6b(instance)), Number(_0c1b2675cd7a2faa(instance))), 0, normalizeSubMillisecondTicks(localTicks));
}
export function _703902cecd7f61dd(instance) {
  let utc = instance.utcDateTime;
  return new JDateTime(createLocalDateTime(utc.getUTCFullYear(), utc.getUTCMonth() + 1, utc.getUTCDate(), utc.getUTCHours(), utc.getUTCMinutes(), utc.getUTCSeconds(), utc.getUTCMilliseconds()), get_DateTimeKindUtc(), instance.utcSubMillisecondTicks);
}
export function _ffbfe7b660ff0527(instance) {
  return new JDateTime(new Date(instance.utcDateTime.getTime()), get_DateTimeKindLocal(), instance.utcSubMillisecondTicks);
}
export function _d1996f02ed3fa243(instance, offset) {
  validateOffset(offset);
  return CreateDateTimeOffset(new Date(instance.utcDateTime.getTime()), offset.ticks, instance.utcSubMillisecondTicks);
}
export function _d7098a1eabebc945(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return new JDateTime(createLocalDate(local.getUTCFullYear(), local.getUTCMonth() + 1, local.getUTCDate()), 0);
}
export function _ba8df912681fe784(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCDate();
}
export function _17d30a204818ce34(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCDay();
}
export function _b69ef2b7d0abde1a(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  let start = Date.UTC(local.getUTCFullYear(), 0, 0);
  return Math.floor((local.getTime() - start) / 86400000);
}
export function _b7fc65477ef4df45(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCHours();
}
export function _0c1b2675cd7a2faa(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCMilliseconds();
}
export function _ae3a48995f0953ed(instance) {
  return Number(instance.utcSubMillisecondTicks / BigInt("10") % BigInt(1000));
}
export function _f9acef215c7d5168(instance) {
  return Number(instance.utcSubMillisecondTicks % BigInt("10") * BigInt(100));
}
export function _0fe8054b55f9f1c7(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCMinutes();
}
export function _79eb4c93cea58d59(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCMonth() + 1;
}
export function _2400298964c553b6(instance) {
  return new JTimeSpan(instance.offsetTicks);
}
export function _822de224fed5bb6b(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCSeconds();
}
export function _584068ab15dcf3c9(instance) {
  return getTicks(instance);
}
export function _056adc0ac251ebd3(instance) {
  return getUtcTicks(instance);
}
export function _90401f92f6a9141e(instance) {
  let normalized = getTicks(instance) % BigInt("864000000000");
  return new JTimeSpan(normalized < get_ZeroTicks() ? normalized + BigInt("864000000000") : normalized);
}
export function _127105b7a40a7665(instance) {
  let local = new Date(instance.utcDateTime.getTime() + Number(instance.offsetTicks) / 10000);
  return local.getUTCFullYear();
}
export function _09a94b0e7945eda6(instance, timeSpan) {
  return createFromUtcTicks(getUtcTicks(instance) + timeSpan.ticks, instance.offsetTicks);
}
export function _7fd735ce2102a3cc(instance, days) {
  return createFromUtcTicks(getUtcTicks(instance) + createAddUnitTicks(days, get_TicksPerDay()), instance.offsetTicks);
}
export function _309c83b8a2fbc988(instance, hours) {
  return createFromUtcTicks(getUtcTicks(instance) + createAddUnitTicks(hours, get_TicksPerHour()), instance.offsetTicks);
}
export function _1528b452af6dd41d(instance, milliseconds) {
  return createFromUtcTicks(getUtcTicks(instance) + createAddUnitTicks(milliseconds, get_TicksPerMillisecond()), instance.offsetTicks);
}
export function _4775ccfee8ed671f(instance, microseconds) {
  return createFromUtcTicks(getUtcTicks(instance) + createAddUnitTicks(microseconds, get_TicksPerMicrosecond()), instance.offsetTicks);
}
export function _97aff1e2f4740394(instance, minutes) {
  return createFromUtcTicks(getUtcTicks(instance) + createAddUnitTicks(minutes, get_TicksPerMinute()), instance.offsetTicks);
}
export function _db8ffdb562d3ac68(instance, months) {
  return addMonthsCore(instance, months);
}
export function _54a4d6d554458fdb(instance, seconds) {
  return createFromUtcTicks(getUtcTicks(instance) + createAddUnitTicks(seconds, get_TicksPerSecond()), instance.offsetTicks);
}
export function _804f8bd2dc1e9443(instance, ticks) {
  return createFromUtcTicks(getUtcTicks(instance) + ticks, instance.offsetTicks);
}
export function _f4ea4e123d38eaa5(instance, years) {
  ensureWholeNumber(years, "ArgumentOutOfRangeException: Years value must be a whole number.");
  return addMonthsCore(instance, years * 12);
}
export function _56ac26a94d0f9bca(first, second) {
  let diff = getUtcTicks(first) - getUtcTicks(second);
  if (diff < get_ZeroTicks())
    return -1;
  if (diff > get_ZeroTicks())
    return 1;
  return 0;
}
export function _255c7bf4a2c3c663(instance, other) {
  return _56ac26a94d0f9bca(instance, other);
}
export function _f7f499e8872c8e8a(instance, other) {
  if (other === null)
    return 1;
  let value = other;
  if (value === null)
    throw new Error("ArgumentException: Object must be of type DateTimeOffset.");
  return _56ac26a94d0f9bca(instance, value);
}
export function _fbec90dd4b315acd(instance, obj) {
  let other = obj;
  if (other === null)
    return false;
  return _5a55745cbe84c163(instance, other);
}
export function _5a55745cbe84c163(instance, other) {
  return getUtcTicks(instance) === getUtcTicks(other);
}
export function _d4a929178865b462(instance, other) {
  return getUtcTicks(instance) === getUtcTicks(other) && instance.offsetTicks === other.offsetTicks;
}
export function _817d2f7b0e423bec(first, second) {
  return _5a55745cbe84c163(first, second);
}
export function _1185de87a3489deb(fileTime) {
  if (fileTime < get_ZeroTicks())
    throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");
  return CreateWithLocalOffset(fileTime - get_FileTimeUnixEpochTicks() + get_UnixEpochTicks());
}
export function _fb7d72712794a2e4(seconds) {
  if (seconds < get_MinUnixTimeSeconds() || seconds > get_MaxUnixTimeSeconds())
    throw new Error("ArgumentOutOfRangeException: Unix time seconds must be within the range of DateTimeOffset.");
  return createDateTimeOffset(new Date(Number(seconds * BigInt(1000))), get_ZeroTicks());
}
export function _89071e7da78164f5(milliseconds) {
  if (milliseconds < get_MinUnixTimeMilliseconds() || milliseconds > get_MaxUnixTimeMilliseconds())
    throw new Error("ArgumentOutOfRangeException: Unix time milliseconds must be within the range of DateTimeOffset.");
  return createDateTimeOffset(new Date(Number(milliseconds)), get_ZeroTicks());
}
export function _484d626eb36d071d(instance) {
  return getInt64HashCode(getUtcTicks(instance));
}
export function _25187a24d190d864(input) {
  return parseCore(input);
}
export function _fbb732b1255fdd38(input, formatProvider) {
  return parseCore(input);
}
export function _277a1a2c7845bcdc(input, formatProvider, styles) {
  return applyDateTimeStyles(parseCore(input), input, styles);
}
export function _948a165174740d96(input, formatProvider, styles) {
  return applyDateTimeStyles(parseCore(input), input, styles);
}
export function _f1e08916de33ed2a(instance, value) {
  return new JTimeSpan(getUtcTicks(instance) - getUtcTicks(value));
}
export function _2636ae85f21cd963(instance, value) {
  return createFromUtcTicks(getUtcTicks(instance) - value.ticks, instance.offsetTicks);
}
export function _d638010bc91ffd47(instance) {
  return getUtcTicks(instance) - get_UnixEpochTicks() + get_FileTimeUnixEpochTicks();
}
export function _8bc213443653978d(instance) {
  return floorDiv(getUtcTicks(instance) - get_UnixEpochTicks(), BigInt("10000000"));
}
export function _e63166ec11d88ce1(instance) {
  return floorDiv(getUtcTicks(instance) - get_UnixEpochTicks(), get_TicksPerMillisecond());
}
export function _c45ea6b7c8ed9501(instance) {
  return CreateWithLocalOffset(getUtcTicks(instance));
}
export function _2aaccc10061a3bb0(instance) {
  return formatDateTimeOffset(instance, null, null);
}
export function _9b46cc87f855c6ba(instance, format) {
  return formatDateTimeOffset(instance, format, null);
}
export function _f0d70d071309b539(instance, formatProvider) {
  return formatDateTimeOffset(instance, null, formatProvider);
}
export function _e856edbfd7db0646(instance, format, formatProvider) {
  return formatDateTimeOffset(instance, format, formatProvider);
}
export function _cbe0bd9bc2e35d83(instance) {
  return CreateDateTimeOffset(new Date(instance.utcDateTime.getTime()), get_ZeroTicks(), instance.utcSubMillisecondTicks);
}
export function _2fd90dc37b274014(input, result) {
  if (input === null || input.length === 0)
    return [false, createDefaultDateTimeOffset()];
  try {
    return [true, parseCore(input)];
  } catch {
    return [false, createDefaultDateTimeOffset()];
  }
}
export function _c7957aa2e68f8218(input, result) {
  return _2fd90dc37b274014(input, result);
}
export function _62fe5aa144f2c9e1(input, formatProvider, styles, result) {
  validateDateTimeStyles(getDateTimeStylesValue(styles));
  if (input === null || input.length === 0)
    return [false, createDefaultDateTimeOffset()];
  try {
    return [true, applyDateTimeStyles(parseCore(input), input, styles)];
  } catch {
    return [false, createDefaultDateTimeOffset()];
  }
}
export function _9dd0fca0c6a9a4de(input, formatProvider, styles, result) {
  return _62fe5aa144f2c9e1(input, formatProvider, styles, result);
}
export function _31bbd12ed57f4f76(value) {
  if (value.kind === get_DateTimeKindUtc())
    return createFromUtcTicks(getDateTimeInstantTicks(value), get_ZeroTicks());
  return CreateWithLocalOffset(getDateTimeInstantTicks(value));
}
export function _b8dd85346f7718fe(dateTimeOffset, timeSpan) {
  return _09a94b0e7945eda6(dateTimeOffset, timeSpan);
}
export function _267065e6d921c80f(dateTimeOffset, timeSpan) {
  return _09a94b0e7945eda6(dateTimeOffset, new JTimeSpan(-timeSpan.ticks));
}
export function _d1af541d3a7181e8(left, right) {
  return new JTimeSpan(getUtcTicks(left) - getUtcTicks(right));
}
export function _553dcbd8f7ea1a16(left, right) {
  return getUtcTicks(left) === getUtcTicks(right);
}
export function _9f6eec56175d9528(left, right) {
  return getUtcTicks(left) !== getUtcTicks(right);
}
export function _43aa45c9517f4d47(left, right) {
  return getUtcTicks(left) < getUtcTicks(right);
}
export function _a6755e7fc2ead5b5(left, right) {
  return getUtcTicks(left) <= getUtcTicks(right);
}
export function _84d1b669e69cd9bf(left, right) {
  return getUtcTicks(left) > getUtcTicks(right);
}
export function _1cb1a326e417bc9b(left, right) {
  return getUtcTicks(left) >= getUtcTicks(right);
}
export function _6ec7dc3f674ff16c(instance, date, time, offset) {
  return [new JDateOnly(_127105b7a40a7665(instance), _79eb4c93cea58d59(instance), _ba8df912681fe784(instance)), new JTimeOnly(_90401f92f6a9141e(instance).ticks), new JTimeSpan(instance.offsetTicks)];
}
export function _61ef673e0dd00ab0(s, provider, result) {
  return _2fd90dc37b274014(s, result);
}
export function _b0967252268296ed(s, provider) {
  return parseCore(s);
}
export function _c9e042e683205a8b(s, provider, result) {
  return _2fd90dc37b274014(s, result);
}
export function _e679a7abf50cf648() {
  let now = new Date;
  let offsetTicks = BigInt(-now.getTimezoneOffset()) * get_OffsetMinuteTicks();
  return createDateTimeOffset(new Date(now.getTime()), offsetTicks);
}
