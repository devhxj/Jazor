import { _24e14b276e0c7e30, _aed2927097617729 } from "System/DoubleModule.js";
import { JDateOnly, JDateTime, JTimeSpan, createLocalDate as i$814ff24919515685, createLocalDateTime as i$f79d8d61d628c007, createUtcDate, formatDateOnlyText, getDaysInMonth, getInt64HashCode, pad2, pad7, padLeft } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
import { _a305982aa6859677 } from "System/TimeOnlyModule.js";
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
function get_ZeroTicks() {
  return 0n;
}
function get_BinaryKindShift() {
  return BigInt("4611686018427387904");
}
function get_BinaryLocalMask() {
  return BigInt("9223372036854775808");
}
function get_BinaryKindMask() {
  return BigInt("13835058055282163712");
}
function get_BinaryUnsignedOverflow() {
  return BigInt("18446744073709551616");
}
function get_BinaryTicksMask() {
  return BigInt("4611686018427387903");
}
function get_MaxDateTimeTicks() {
  return BigInt("3155378975999999999");
}
function get_OADateUnixOffsetDays() {
  return 25569;
}
function get_MillisecondsPerDay() {
  return 86400000;
}
function get_DateTimeKindUnspecified() {
  return 0;
}
function get_DateTimeKindUtc() {
  return 1;
}
function get_DateTimeKindLocal() {
  return 2;
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
function get_DateTimeStylesRoundtripKind() {
  return 128;
}
function ensureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function createDefaultDateTime() {
  return new JDateTime(createLocalDate(1, 1, 1), get_DateTimeKindUnspecified());
}
function createLocalDate(year, month, day) {
  return i$814ff24919515685(year, month, day);
}
function createLocalDateTime(year, month, day, hour, minute, second, millisecond) {
  return i$f79d8d61d628c007(year, month, day, hour, minute, second, millisecond);
}
function createFromTicks(ticks) {
  return CreateFromTicks(ticks, get_DateTimeKindUnspecified());
}
function CreateFromTicks(ticks, kind) {
  if (ticks < get_ZeroTicks() || ticks > get_MaxDateTimeTicks())
    throw new Error("ArgumentOutOfRangeException: Ticks must be within the range of DateTime.");
  let ticksSinceUnixEpoch = ticks - get_UnixEpochTicks();
  let milliseconds = ticksSinceUnixEpoch / get_TicksPerMillisecond();
  let subMillisecondTicks = ticksSinceUnixEpoch % get_TicksPerMillisecond();
  if (subMillisecondTicks < get_ZeroTicks()) {
    milliseconds -= BigInt(1);
    subMillisecondTicks += get_TicksPerMillisecond();
  }
  let utc = new Date(Number(milliseconds));
  return new JDateTime(createLocalDateTime(utc.getUTCFullYear(), utc.getUTCMonth() + 1, utc.getUTCDate(), utc.getUTCHours(), utc.getUTCMinutes(), utc.getUTCSeconds(), utc.getUTCMilliseconds()), kind, subMillisecondTicks);
}
function createDateTime(date, kind) {
  return new JDateTime(date, kind);
}
function CreateDateTime(date, kind, subMillisecondTicks) {
  return new JDateTime(date, kind, subMillisecondTicks);
}
function createFromInstantTicks(ticks, kind) {
  if (ticks < get_ZeroTicks() || ticks > get_MaxDateTimeTicks())
    throw new Error("ArgumentOutOfRangeException: Ticks must be within the range of DateTime.");
  if (kind === get_DateTimeKindUtc())
    return CreateFromTicks(ticks, kind);
  let ticksSinceUnixEpoch = ticks - get_UnixEpochTicks();
  let milliseconds = ticksSinceUnixEpoch / get_TicksPerMillisecond();
  let subMillisecondTicks = ticksSinceUnixEpoch % get_TicksPerMillisecond();
  if (subMillisecondTicks < get_ZeroTicks()) {
    milliseconds -= BigInt(1);
    subMillisecondTicks += get_TicksPerMillisecond();
  }
  return new JDateTime(new Date(Number(milliseconds)), kind, subMillisecondTicks);
}
function getKind(kind) {
  let value = Number(kind);
  if (value !== get_DateTimeKindUnspecified() && value !== get_DateTimeKindUtc() && value !== get_DateTimeKindLocal())
    throw new Error("ArgumentException: Invalid DateTimeKind value.");
  return value;
}
function getMicrosecondTicks(microsecond) {
  if (Math.floor(microsecond) !== microsecond || microsecond < 0 || microsecond > 999)
    throw new Error("ArgumentOutOfRangeException: Microsecond must be between 0 and 999.");
  return BigInt(microsecond) * get_TicksPerMicrosecond();
}
function getTicks(instance) {
  let date = instance.date;
  let milliseconds = Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds());
  return BigInt(milliseconds) * get_TicksPerMillisecond() + instance.subMillisecondTicks + get_UnixEpochTicks();
}
function GetTicks(date) {
  let milliseconds = Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds());
  return BigInt(milliseconds) * get_TicksPerMillisecond() + get_UnixEpochTicks();
}
function getInstantTicks(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return getTicks(instance);
  return BigInt(instance.date.getTime()) * get_TicksPerMillisecond() + instance.subMillisecondTicks + get_UnixEpochTicks();
}
function createUtcNow() {
  let now = new Date;
  return new JDateTime(createLocalDateTime(now.getUTCFullYear(), now.getUTCMonth() + 1, now.getUTCDate(), now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds()), get_DateTimeKindUtc());
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
    return date.getHours() < 12 ? "AM" : "PM";
  let parts = new Intl.DateTimeFormat(locale, { hour: "numeric", hour12: true }).formatToParts(date);
  for (let i = 0; i < parts.length; i++) {
    let part = parts[i];
    if (part.type === "dayPeriod")
      return part.value;
  }
  return date.getHours() < 12 ? "AM" : "PM";
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
function getRoundtripSuffix(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return "Z";
  if (instance.kind === get_DateTimeKindLocal())
    return formatOffsetTicks(BigInt(-instance.date.getTimezoneOffset()) * get_OffsetMinuteTicks(), 3);
  return "";
}
function formatInvariantGeneralDateTime(instance, includeSeconds) {
  let date = instance.date;
  let text = pad2(date.getMonth() + 1) + "/" + pad2(date.getDate()) + "/" + padLeft(date.getFullYear().toString(), 4) + " " + pad2(date.getHours()) + ":" + pad2(date.getMinutes());
  if (includeSeconds)
    text += ":" + pad2(date.getSeconds());
  return text;
}
function formatInvariantShortDate(instance) {
  let date = instance.date;
  return pad2(date.getMonth() + 1) + "/" + pad2(date.getDate()) + "/" + padLeft(date.getFullYear().toString(), 4);
}
function formatInvariantLongDate(instance) {
  let date = instance.date;
  return getInvariantDayName(date.getDay()) + ", " + pad2(date.getDate()) + " " + getInvariantMonthName(date.getMonth() + 1) + " " + padLeft(date.getFullYear().toString(), 4);
}
function formatInvariantTime(instance, includeSeconds) {
  let date = instance.date;
  let text = pad2(date.getHours()) + ":" + pad2(date.getMinutes());
  if (includeSeconds)
    text += ":" + pad2(date.getSeconds());
  return text;
}
function formatMonthDay(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return getInvariantMonthName(instance.date.getMonth() + 1) + " " + pad2(instance.date.getDate());
  return formatLocaleDateTime(instance.date, locale, { month: "long", day: "2-digit" });
}
function formatYearMonth(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return padLeft(instance.date.getFullYear().toString(), 4) + " " + getInvariantMonthName(instance.date.getMonth() + 1);
  return formatLocaleDateTime(instance.date, locale, { year: "numeric", month: "long" });
}
function formatFullDateTime(instance, includeSeconds, provider) {
  return formatLongDate(instance, provider) + " " + formatTime(instance, includeSeconds, provider);
}
function getUniversalDateTimeForFormatting(instance) {
  let date = instance.date;
  if (instance.kind === get_DateTimeKindUtc()) {
    return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds()));
  }
  return new Date(date.getTime());
}
function formatUniversalFullDateTime(instance, provider) {
  let utc = getUniversalDateTimeForFormatting(instance);
  let locale = getProviderLocale(provider);
  if (locale.length === 0) {
    return getInvariantDayName(utc.getUTCDay()) + ", " + pad2(utc.getUTCDate()) + " " + getInvariantMonthName(utc.getUTCMonth() + 1) + " " + padLeft(utc.getUTCFullYear().toString(), 4) + " " + pad2(utc.getUTCHours()) + ":" + pad2(utc.getUTCMinutes()) + ":" + pad2(utc.getUTCSeconds());
  }
  return joinFormatParts(new Intl.DateTimeFormat(locale, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: false,
    timeZone: "UTC"
  }).formatToParts(utc));
}
function formatRfc1123DateTime(instance) {
  let date = instance.date;
  return getInvariantAbbreviatedDayName(date.getDay()) + ", " + pad2(date.getDate()) + " " + getInvariantAbbreviatedMonthName(date.getMonth() + 1) + " " + padLeft(date.getFullYear().toString(), 4) + " " + pad2(date.getHours()) + ":" + pad2(date.getMinutes()) + ":" + pad2(date.getSeconds()) + " GMT";
}
function formatLocaleDateTime(date, locale, options) {
  return joinFormatParts(new Intl.DateTimeFormat(locale, options).formatToParts(date));
}
function formatGeneralDateTime(instance, includeSeconds, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantGeneralDateTime(instance, includeSeconds);
  return formatLocaleDateTime(instance.date, locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: includeSeconds ? "2-digit" : null,
    hour12: false
  });
}
function formatShortDate(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantShortDate(instance);
  return formatLocaleDateTime(instance.date, locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  });
}
function formatLongDate(instance, provider) {
  let locale = getProviderLocale(provider);
  if (locale.length === 0)
    return formatInvariantLongDate(instance);
  return formatLocaleDateTime(instance.date, locale, {
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
  return formatLocaleDateTime(instance.date, locale, {
    hour: "2-digit",
    minute: "2-digit",
    second: includeSeconds ? "2-digit" : null,
    hour12: false
  });
}
function formatRoundtripDateTime(instance) {
  let date = instance.date;
  return formatDateOnlyText(date.getFullYear(), date.getMonth() + 1, date.getDate()) + "T" + pad2(date.getHours()) + ":" + pad2(date.getMinutes()) + ":" + pad2(date.getSeconds()) + "." + pad7(BigInt(date.getMilliseconds()) * get_TicksPerMillisecond() + instance.subMillisecondTicks) + getRoundtripSuffix(instance);
}
function formatSortableDateTime(instance) {
  let date = instance.date;
  return formatDateOnlyText(date.getFullYear(), date.getMonth() + 1, date.getDate()) + "T" + pad2(date.getHours()) + ":" + pad2(date.getMinutes()) + ":" + pad2(date.getSeconds());
}
function formatUniversalSortableDateTime(instance) {
  return formatSortableDateTime(instance).replaceAll("T", " ") + "Z";
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
  let date = instance.date;
  let year = date.getFullYear();
  let month = date.getMonth() + 1;
  let day = date.getDate();
  let hour = date.getHours();
  let hour12 = hour % 12;
  if (hour12 === 0)
    hour12 = 12;
  let minute = date.getMinutes();
  let second = date.getSeconds();
  let fraction = BigInt(date.getMilliseconds()) * get_TicksPerMillisecond() + instance.subMillisecondTicks;
  let offset = instance.kind === get_DateTimeKindLocal() ? BigInt(-date.getTimezoneOffset()) * get_OffsetMinuteTicks() : 0n;
  let suffix = getRoundtripSuffix(instance);
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
        return getLocalizedDayName(locale, date.getDay(), true);
      return getLocalizedDayName(locale, date.getDay(), false);
    case "H":
      return count === 1 ? hour.toString() : pad2(hour);
    case "h":
      return count === 1 ? hour12.toString() : pad2(hour12);
    case "m":
      return count === 1 ? minute.toString() : pad2(minute);
    case "s":
      return count === 1 ? second.toString() : pad2(second);
    case "t":
      let dayPeriod = getLocalizedDayPeriod(date, locale);
      return count === 1 ? dayPeriod.substring(0, 0 + 1) : dayPeriod;
    case "f":
      return formatFraction(fraction, count, false);
    case "F":
      return formatFraction(fraction, count, true);
    case "z":
      return instance.kind === get_DateTimeKindLocal() ? formatOffsetTicks(offset, count) : "";
    case "K":
      return suffix;
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
function formatCustomDateTime(instance, format, provider) {
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
function formatDateTime(instance, format, provider) {
  if (format === null || format.length === 0)
    return formatGeneralDateTime(instance, true, provider);
  if (format.length === 1) {
    switch (_5ad63706a889c294(format, 0)) {
      case "f":
        return formatFullDateTime(instance, false, provider);
      case "F":
        return formatFullDateTime(instance, true, provider);
      case "O":
      case "o":
        return formatRoundtripDateTime(instance);
      case "G":
        return formatGeneralDateTime(instance, true, provider);
      case "g":
        return formatGeneralDateTime(instance, false, provider);
      case "M":
      case "m":
        return formatMonthDay(instance, provider);
      case "R":
      case "r":
        return formatRfc1123DateTime(instance);
      case "d":
        return formatShortDate(instance, provider);
      case "D":
        return formatLongDate(instance, provider);
      case "t":
        return formatTime(instance, false, provider);
      case "T":
        return formatTime(instance, true, provider);
      case "s":
        return formatSortableDateTime(instance);
      case "u":
        return formatUniversalSortableDateTime(instance);
      case "U":
        return formatUniversalFullDateTime(instance, provider);
      case "Y":
      case "y":
        return formatYearMonth(instance, provider);
      default:
        if (isAsciiLetter(_5ad63706a889c294(format, 0)))
          throw new Error("FormatException: Input string was not in a correct format.");
        break;
    }
  }
  return formatCustomDateTime(instance, format, provider);
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
function createUtcDateTimeTicks(year, month, day, hour, minute, second, millisecond) {
  let utc = createUtcDate(year, month, day);
  utc.setUTCHours(hour, minute, second, millisecond);
  return BigInt(utc.getTime()) * get_TicksPerMillisecond() + get_UnixEpochTicks();
}
function tryParseIsoDateTime(text, year, month, day, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks) {
  let __ref$528019db8b79deae0bba82f3, __ref$46dd353eaeb16f13b78aaa20, __ref$8de0c249e27423a6a19c8cd2;
  year = 0;
  month = 0;
  day = 0;
  hour = 0;
  minute = 0;
  second = 0;
  millisecond = 0;
  subMillisecondTicks = get_ZeroTicks();
  kind = get_DateTimeKindUnspecified();
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
      kind,
      offsetTicks
    ];
  if (!(__ref$528019db8b79deae0bba82f3 = tryParseIsoDate(text.substring(0, 0 + 10), year, month, day), year = __ref$528019db8b79deae0bba82f3[1], month = __ref$528019db8b79deae0bba82f3[2], day = __ref$528019db8b79deae0bba82f3[3], __ref$528019db8b79deae0bba82f3[0]))
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
      kind,
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
      kind,
      offsetTicks
    ];
  if (!(__ref$46dd353eaeb16f13b78aaa20 = tryParseTwoDigits(text, 11, hour), hour = __ref$46dd353eaeb16f13b78aaa20[1], __ref$46dd353eaeb16f13b78aaa20[0]) || _5ad63706a889c294(text, 13) !== ":" || !(__ref$8de0c249e27423a6a19c8cd2 = tryParseTwoDigits(text, 14, minute), minute = __ref$8de0c249e27423a6a19c8cd2[1], __ref$8de0c249e27423a6a19c8cd2[0]))
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
      kind,
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
      kind,
      offsetTicks
    ];
  let index = 16;
  if (index < text.length && _5ad63706a889c294(text, index) === ":") {
    let __ref$29ccd37774962476a807be0f;
    if (!(__ref$29ccd37774962476a807be0f = tryParseTwoDigits(text, index + 1, second), second = __ref$29ccd37774962476a807be0f[1], __ref$29ccd37774962476a807be0f[0]))
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
        kind,
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
        kind,
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
        kind,
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
      kind,
      offsetTicks
    ];
  if (index === text.length - 1 && (_5ad63706a889c294(text, index) === "Z" || _5ad63706a889c294(text, index) === "z")) {
    kind = get_DateTimeKindUtc();
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
      kind,
      offsetTicks
    ];
  }
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
      kind,
      offsetTicks
    ];
  kind = get_DateTimeKindLocal();
  let remaining = text.length - index - 1;
  let offsetHours;
  let offsetMinutes;
  if (remaining === 2) {
    let __ref$ba9f9ac5a8c07c6774052dca;
    if (!(__ref$ba9f9ac5a8c07c6774052dca = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$ba9f9ac5a8c07c6774052dca[1], __ref$ba9f9ac5a8c07c6774052dca[0]))
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
        kind,
        offsetTicks
      ];
    offsetMinutes = 0;
  }
  else if (remaining === 4) {
    let __ref$ba9f9ac5a8c07c6774052dca, __ref$fe1b0ada74e1fb560377be79;
    if (!(__ref$ba9f9ac5a8c07c6774052dca = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$ba9f9ac5a8c07c6774052dca[1], __ref$ba9f9ac5a8c07c6774052dca[0]) || !(__ref$fe1b0ada74e1fb560377be79 = tryParseTwoDigits(text, index + 3, offsetMinutes), offsetMinutes = __ref$fe1b0ada74e1fb560377be79[1], __ref$fe1b0ada74e1fb560377be79[0]))
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
        kind,
        offsetTicks
      ];
  }
  else if (remaining === 5 && _5ad63706a889c294(text, index + 3) === ":") {
    let __ref$ba9f9ac5a8c07c6774052dca, __ref$aa14176bdb0afaa39e9fb72b;
    if (!(__ref$ba9f9ac5a8c07c6774052dca = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$ba9f9ac5a8c07c6774052dca[1], __ref$ba9f9ac5a8c07c6774052dca[0]) || !(__ref$aa14176bdb0afaa39e9fb72b = tryParseTwoDigits(text, index + 4, offsetMinutes), offsetMinutes = __ref$aa14176bdb0afaa39e9fb72b[1], __ref$aa14176bdb0afaa39e9fb72b[0]))
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
        kind,
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
      kind,
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
      kind,
      offsetTicks
    ];
  offsetTicks = BigInt(offsetHours * 60 + offsetMinutes) * BigInt("600000000");
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
    kind,
    offsetTicks
  ];
}
function tryParseTimeOnly(text, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks) {
  let __ref$fcf8ce7667902751b975a0e0, __ref$cc8330d3eb975309ec8d10e1;
  hour = 0;
  minute = 0;
  second = 0;
  millisecond = 0;
  subMillisecondTicks = get_ZeroTicks();
  kind = get_DateTimeKindUnspecified();
  offsetTicks = get_ZeroTicks();
  if (text.length < 5)
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      kind,
      offsetTicks
    ];
  if (!(__ref$fcf8ce7667902751b975a0e0 = tryParseTwoDigits(text, 0, hour), hour = __ref$fcf8ce7667902751b975a0e0[1], __ref$fcf8ce7667902751b975a0e0[0]) || _5ad63706a889c294(text, 2) !== ":" || !(__ref$cc8330d3eb975309ec8d10e1 = tryParseTwoDigits(text, 3, minute), minute = __ref$cc8330d3eb975309ec8d10e1[1], __ref$cc8330d3eb975309ec8d10e1[0]))
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      kind,
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
      kind,
      offsetTicks
    ];
  let index = 5;
  if (index < text.length && _5ad63706a889c294(text, index) === ":") {
    let __ref$0ba41a5c3d60296df1c40e04;
    if (!(__ref$0ba41a5c3d60296df1c40e04 = tryParseTwoDigits(text, index + 1, second), second = __ref$0ba41a5c3d60296df1c40e04[1], __ref$0ba41a5c3d60296df1c40e04[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        kind,
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
        kind,
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
        kind,
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
      kind,
      offsetTicks
    ];
  if (index === text.length - 1 && (_5ad63706a889c294(text, index) === "Z" || _5ad63706a889c294(text, index) === "z")) {
    kind = get_DateTimeKindLocal();
    return [
      true,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      kind,
      offsetTicks
    ];
  }
  let sign = _5ad63706a889c294(text, index);
  if (sign !== "+" && sign !== "-")
    return [
      false,
      hour,
      minute,
      second,
      millisecond,
      subMillisecondTicks,
      kind,
      offsetTicks
    ];
  kind = get_DateTimeKindLocal();
  let remaining = text.length - index - 1;
  let offsetHours;
  let offsetMinutes;
  if (remaining === 2) {
    let __ref$380e1c8152e5c0a06314de4d;
    if (!(__ref$380e1c8152e5c0a06314de4d = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$380e1c8152e5c0a06314de4d[1], __ref$380e1c8152e5c0a06314de4d[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        kind,
        offsetTicks
      ];
    offsetMinutes = 0;
  }
  else if (remaining === 4) {
    let __ref$380e1c8152e5c0a06314de4d, __ref$731551661238b7bf4db10265;
    if (!(__ref$380e1c8152e5c0a06314de4d = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$380e1c8152e5c0a06314de4d[1], __ref$380e1c8152e5c0a06314de4d[0]) || !(__ref$731551661238b7bf4db10265 = tryParseTwoDigits(text, index + 3, offsetMinutes), offsetMinutes = __ref$731551661238b7bf4db10265[1], __ref$731551661238b7bf4db10265[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        kind,
        offsetTicks
      ];
  }
  else if (remaining === 5 && _5ad63706a889c294(text, index + 3) === ":") {
    let __ref$380e1c8152e5c0a06314de4d, __ref$4b240a9e3e4933e854d03eed;
    if (!(__ref$380e1c8152e5c0a06314de4d = tryParseTwoDigits(text, index + 1, offsetHours), offsetHours = __ref$380e1c8152e5c0a06314de4d[1], __ref$380e1c8152e5c0a06314de4d[0]) || !(__ref$4b240a9e3e4933e854d03eed = tryParseTwoDigits(text, index + 4, offsetMinutes), offsetMinutes = __ref$4b240a9e3e4933e854d03eed[1], __ref$4b240a9e3e4933e854d03eed[0]))
      return [
        false,
        hour,
        minute,
        second,
        millisecond,
        subMillisecondTicks,
        kind,
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
      kind,
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
      kind,
      offsetTicks
    ];
  offsetTicks = BigInt(offsetHours * 60 + offsetMinutes) * BigInt("600000000");
  if (sign === "-")
    offsetTicks = -offsetTicks;
  return [
    true,
    hour,
    minute,
    second,
    millisecond,
    subMillisecondTicks,
    kind,
    offsetTicks
  ];
}
function createRoundedTicksFromDouble(value) {
  if (_24e14b276e0c7e30(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!_aed2927097617729(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  let rounded = Math.round(value);
  if (!_aed2927097617729(rounded))
    throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTime range.");
  return BigInt(rounded);
}
function createAddUnitTicks(value, ticksPerUnit) {
  if (_24e14b276e0c7e30(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!_aed2927097617729(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  let maxUnitCount = Number(get_MaxDateTimeTicks()) / Number(ticksPerUnit);
  if (Math.abs(value) > maxUnitCount)
    throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTime range.");
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
  let hasRoundtripKind = (styles & get_DateTimeStylesRoundtripKind()) !== 0;
  let hasAdjustToUniversal = (styles & get_DateTimeStylesAdjustToUniversal()) !== 0;
  let hasAssumeLocal = (styles & get_DateTimeStylesAssumeLocal()) !== 0;
  let hasAssumeUniversal = (styles & get_DateTimeStylesAssumeUniversal()) !== 0;
  if (hasRoundtripKind && (hasAdjustToUniversal || hasAssumeLocal || hasAssumeUniversal))
    throw new Error("ArgumentException: RoundtripKind cannot be combined with AssumeLocal, AssumeUniversal, or AdjustToUniversal.");
  if (hasAssumeLocal && hasAssumeUniversal)
    throw new Error("ArgumentException: AssumeLocal and AssumeUniversal cannot both be set.");
}
function applyDateTimeStyles(value, input, styles) {
  let hour, minute, second, millisecond, timeOnlySubTicks, timeOnlyKind, timeOnlyOffsetTicks, __ref$a36c4bb1e99745149efba700;
  let styleValue = getDateTimeStylesValue(styles);
  validateDateTimeStyles(styleValue);
  let text = input.trim();
  let hasUtcSuffix = HasUtcSuffix(text);
  let hasExplicitOffset = HasExplicitOffset(text);
  let hasExplicitZone = hasUtcSuffix || hasExplicitOffset;
  let noCurrentDateDefault = (styleValue & get_DateTimeStylesNoCurrentDateDefault()) !== 0;
  let adjustToUniversal = (styleValue & get_DateTimeStylesAdjustToUniversal()) !== 0;
  let assumeLocal = (styleValue & get_DateTimeStylesAssumeLocal()) !== 0;
  let assumeUniversal = (styleValue & get_DateTimeStylesAssumeUniversal()) !== 0;
  let roundtripKind = (styleValue & get_DateTimeStylesRoundtripKind()) !== 0;
  if (noCurrentDateDefault && (__ref$a36c4bb1e99745149efba700 = tryParseTimeOnly(text, hour, minute, second, millisecond, timeOnlySubTicks, timeOnlyKind, timeOnlyOffsetTicks), hour = __ref$a36c4bb1e99745149efba700[1], minute = __ref$a36c4bb1e99745149efba700[2], second = __ref$a36c4bb1e99745149efba700[3], millisecond = __ref$a36c4bb1e99745149efba700[4], timeOnlySubTicks = __ref$a36c4bb1e99745149efba700[5], timeOnlyKind = __ref$a36c4bb1e99745149efba700[6], timeOnlyOffsetTicks = __ref$a36c4bb1e99745149efba700[7], __ref$a36c4bb1e99745149efba700[0])) {
    if (timeOnlyKind === get_DateTimeKindUnspecified()) {
      value = CreateDateTime(createLocalDateTime(1, 1, 1, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), timeOnlySubTicks);
    }
    else {
      let utcTicks = createUtcDateTimeTicks(1, 1, 1, hour, minute, second, millisecond) + timeOnlySubTicks - timeOnlyOffsetTicks;
      value = createFromInstantTicks(utcTicks, get_DateTimeKindLocal());
    }
  }
  if (hasExplicitZone) {
    if (adjustToUniversal || roundtripKind && hasUtcSuffix)
      return createFromInstantTicks(getInstantTicks(value), get_DateTimeKindUtc());
    return value;
  }
  if (value.kind !== get_DateTimeKindUnspecified())
    return value;
  if (assumeUniversal) {
    let assumedUtcTicks = getTicks(value);
    if (adjustToUniversal)
      return CreateFromTicks(assumedUtcTicks, get_DateTimeKindUtc());
    return createFromInstantTicks(assumedUtcTicks, get_DateTimeKindLocal());
  }
  if (assumeLocal) {
    if (adjustToUniversal)
      return createFromInstantTicks(getInstantTicks(value), get_DateTimeKindUtc());
    return CreateDateTime(value.date, get_DateTimeKindLocal(), value.subMillisecondTicks);
  }
  return value;
}
function addMonthsCore(instance, months) {
  ensureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");
  let year = instance.date.getFullYear();
  let monthIndex = (year - 1) * 12 + instance.date.getMonth() + months;
  let newYear = Math.floor(monthIndex / 12) + 1;
  let newMonthIndex = monthIndex % 12;
  if (newMonthIndex < 0)
    newMonthIndex += 12;
  let newMonth = newMonthIndex + 1;
  let day = instance.date.getDate();
  let daysInMonth = getDaysInMonth(newYear, newMonth);
  let newDay = day > daysInMonth ? daysInMonth : day;
  return CreateDateTime(createLocalDateTime(newYear, newMonth, newDay, instance.date.getHours(), instance.date.getMinutes(), instance.date.getSeconds(), instance.date.getMilliseconds()), instance.kind, instance.subMillisecondTicks);
}
function HasUtcSuffix(input) {
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
    throw new Error(`FormatException: String '${input}' was not recognized as a valid DateTime.`);
  while (digits.length < 7)
    digits += "0";
  return BigInt(digits.substring(3, 3 + 4));
}
function parseCore(input) {
  let timeHour, timeMinute, timeSecond, timeMillisecond, timeSubMillisecondTicks, timeKind, timeOffsetTicks, __ref$3a7e265ad6e70313a37ac95d, year, month, day, __ref$9b1bb58b1567532334cd345b, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks, __ref$14e2b4da9535e830918ac69e;
  let s = input.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid DateTime.");
  if (__ref$3a7e265ad6e70313a37ac95d = tryParseTimeOnly(s, timeHour, timeMinute, timeSecond, timeMillisecond, timeSubMillisecondTicks, timeKind, timeOffsetTicks), timeHour = __ref$3a7e265ad6e70313a37ac95d[1], timeMinute = __ref$3a7e265ad6e70313a37ac95d[2], timeSecond = __ref$3a7e265ad6e70313a37ac95d[3], timeMillisecond = __ref$3a7e265ad6e70313a37ac95d[4], timeSubMillisecondTicks = __ref$3a7e265ad6e70313a37ac95d[5], timeKind = __ref$3a7e265ad6e70313a37ac95d[6], timeOffsetTicks = __ref$3a7e265ad6e70313a37ac95d[7], __ref$3a7e265ad6e70313a37ac95d[0]) {
    let now = new Date;
    let currentYear = now.getFullYear();
    let currentMonth = now.getMonth() + 1;
    let currentDay = now.getDate();
    if (timeKind === get_DateTimeKindUnspecified())
      return new JDateTime(createLocalDateTime(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond), get_DateTimeKindUnspecified(), timeSubMillisecondTicks);
    let utcTicks = createUtcDateTimeTicks(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond) + timeSubMillisecondTicks - timeOffsetTicks;
    return createFromInstantTicks(utcTicks, get_DateTimeKindLocal());
  }
  if (__ref$9b1bb58b1567532334cd345b = tryParseIsoDate(s, year, month, day), year = __ref$9b1bb58b1567532334cd345b[1], month = __ref$9b1bb58b1567532334cd345b[2], day = __ref$9b1bb58b1567532334cd345b[3], __ref$9b1bb58b1567532334cd345b[0])
    return new JDateTime(createLocalDate(year, month, day), get_DateTimeKindUnspecified());
  if (__ref$14e2b4da9535e830918ac69e = tryParseIsoDateTime(s, year, month, day, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks), year = __ref$14e2b4da9535e830918ac69e[1], month = __ref$14e2b4da9535e830918ac69e[2], day = __ref$14e2b4da9535e830918ac69e[3], hour = __ref$14e2b4da9535e830918ac69e[4], minute = __ref$14e2b4da9535e830918ac69e[5], second = __ref$14e2b4da9535e830918ac69e[6], millisecond = __ref$14e2b4da9535e830918ac69e[7], subMillisecondTicks = __ref$14e2b4da9535e830918ac69e[8], kind = __ref$14e2b4da9535e830918ac69e[9], offsetTicks = __ref$14e2b4da9535e830918ac69e[10], __ref$14e2b4da9535e830918ac69e[0]) {
    if (kind === get_DateTimeKindUnspecified())
      return new JDateTime(createLocalDateTime(year, month, day, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), subMillisecondTicks);
    let utcTicks = createUtcDateTimeTicks(year, month, day, hour, minute, second, millisecond) + subMillisecondTicks - offsetTicks;
    return createFromInstantTicks(utcTicks, get_DateTimeKindLocal());
  }
  let parsed = new Date(s);
  if (isNaN(parsed.getTime()))
    throw new Error(`FormatException: String '${input}' was not recognized as a valid DateTime.`);
  let parsedSubMillisecondTicks = extractSubMillisecondTicks(s);
  if (HasUtcSuffix(s))
    return createFromInstantTicks(BigInt(parsed.getTime()) * get_TicksPerMillisecond() + parsedSubMillisecondTicks + get_UnixEpochTicks(), get_DateTimeKindLocal());
  if (HasExplicitOffset(s))
    return new JDateTime(new Date(parsed.getTime()), get_DateTimeKindLocal(), parsedSubMillisecondTicks);
  return new JDateTime(parsed, get_DateTimeKindUnspecified(), parsedSubMillisecondTicks);
}
export function _fad0c74e1c9df5bb() {
  return new JDateTime(createLocalDate(1, 1, 1), get_DateTimeKindUnspecified());
}
export function _eb38dc04224730ea() {
  return CreateDateTime(createLocalDateTime(9999, 12, 31, 23, 59, 59, 999), get_DateTimeKindUnspecified(), BigInt("9999"));
}
export function _878591efc9a51388() {
  return CreateFromTicks(get_UnixEpochTicks(), get_DateTimeKindUtc());
}
export function _bfa8ee5dd46e2005() {
  return new JDateTime(createLocalDate(1, 1, 1), get_DateTimeKindUnspecified());
}
export function _1ba9ed95dd0eab48(ticks) {
  return CreateFromTicks(ticks, get_DateTimeKindUnspecified());
}
export function _eda1c8bf8e1e617b(ticks, kind) {
  return CreateFromTicks(ticks, getKind(kind));
}
export function _4fef4795bcbef97f(date, time) {
  let milliseconds = Number(time.ticks / get_TicksPerMillisecond());
  let subMillisecondTicks = time.ticks % get_TicksPerMillisecond();
  let hour = Math.floor(milliseconds / 3600000);
  let minute = Math.floor(milliseconds / 60000) % 60;
  let second = Math.floor(milliseconds / 1000) % 60;
  let millisecond = milliseconds % 1000;
  return CreateDateTime(createLocalDateTime(date.year, date.month, date.day, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), subMillisecondTicks);
}
export function _85602323793168a5(date, time, kind) {
  let result = _4fef4795bcbef97f(date, time);
  return CreateDateTime(result.date, getKind(kind), result.subMillisecondTicks);
}
export function _4cb33a818161a3e1(year, month, day) {
  return new JDateTime(createLocalDate(year, month, day), get_DateTimeKindUnspecified());
}
export function _4903723bbf8a0a2f(year, month, day, hour, minute, second) {
  return new JDateTime(createLocalDateTime(year, month, day, hour, minute, second, 0));
}
export function _f83be88cfb3fbce0(year, month, day, hour, minute, second, kind) {
  return new JDateTime(createLocalDateTime(year, month, day, hour, minute, second, 0), getKind(kind));
}
export function _5822b271bb635d64(year, month, day, hour, minute, second, millisecond) {
  return new JDateTime(createLocalDateTime(year, month, day, hour, minute, second, millisecond), get_DateTimeKindUnspecified());
}
export function _c52eec5e681a0b8b(year, month, day, hour, minute, second, millisecond, kind) {
  return new JDateTime(createLocalDateTime(year, month, day, hour, minute, second, millisecond), getKind(kind));
}
export function _9117d26d23769ad1(year, month, day, hour, minute, second, millisecond, microsecond) {
  return CreateDateTime(createLocalDateTime(year, month, day, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), getMicrosecondTicks(microsecond));
}
export function _e84671346e2b9972(year, month, day, hour, minute, second, millisecond, microsecond, kind) {
  return CreateDateTime(createLocalDateTime(year, month, day, hour, minute, second, millisecond), getKind(kind), getMicrosecondTicks(microsecond));
}
export function _34a77be7365c459f(instance, value) {
  return CreateFromTicks(getTicks(instance) + value.ticks, instance.kind);
}
export function _558a3f189d9149d7(instance, value) {
  return CreateFromTicks(getTicks(instance) + createAddUnitTicks(value, get_TicksPerDay()), instance.kind);
}
export function _101af978213c19c5(instance, value) {
  return CreateFromTicks(getTicks(instance) + createAddUnitTicks(value, get_TicksPerHour()), instance.kind);
}
export function _2b29e4c11fa12daa(instance, value) {
  return CreateFromTicks(getTicks(instance) + createAddUnitTicks(value, get_TicksPerMillisecond()), instance.kind);
}
export function _2b47368c73a3e1f2(instance, value) {
  return CreateFromTicks(getTicks(instance) + createAddUnitTicks(value, get_TicksPerMicrosecond()), instance.kind);
}
export function _8bdc25943cf2d39b(instance, value) {
  return CreateFromTicks(getTicks(instance) + createAddUnitTicks(value, get_TicksPerMinute()), instance.kind);
}
export function _aae197b95f9024a4(instance, months) {
  return addMonthsCore(instance, months);
}
export function _57045f93edac1460(instance, value) {
  return CreateFromTicks(getTicks(instance) + createAddUnitTicks(value, get_TicksPerSecond()), instance.kind);
}
export function _d2e74845b174a889(instance, value) {
  return CreateFromTicks(getTicks(instance) + value, instance.kind);
}
export function _3353d31b02f2bed8(instance, value) {
  ensureWholeNumber(value, "ArgumentOutOfRangeException: Years value must be a whole number.");
  return addMonthsCore(instance, value * 12);
}
export function _0edfd00dcc8d70d0(t1, t2) {
  let ticks1 = getTicks(t1);
  let ticks2 = getTicks(t2);
  if (ticks1 < ticks2)
    return -1;
  if (ticks1 > ticks2)
    return 1;
  return 0;
}
export function _f7b2337bfa9864d9(instance, value) {
  if (value === null)
    return 1;
  let other = value;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type DateTime.");
  return _40c6426fdc505e97(instance, other);
}
export function _40c6426fdc505e97(instance, value) {
  let ticks = getTicks(instance);
  let otherTicks = getTicks(value);
  if (ticks < otherTicks)
    return -1;
  if (ticks > otherTicks)
    return 1;
  return 0;
}
export function _38ef7423971afb7f(year, month) {
  return getDaysInMonth(year, month);
}
export function _f6903c1af8944917(instance, value) {
  let other = value;
  return other !== null && getTicks(instance) === getTicks(other);
}
export function _c29ca32a998c517c(instance, value) {
  return getTicks(instance) === getTicks(value);
}
export function _4937ff8bec81ddea(t1, t2) {
  return getTicks(t1) === getTicks(t2);
}
export function _f437fad61f0046c7(dateData) {
  let unsignedData = dateData < get_ZeroTicks() ? dateData + get_BinaryUnsignedOverflow() : dateData;
  let kindBits = unsignedData & get_BinaryKindMask();
  let ticks = unsignedData & get_BinaryTicksMask();
  if (kindBits === get_BinaryLocalMask() || kindBits === get_BinaryKindMask())
    return createFromInstantTicks(ticks, get_DateTimeKindLocal());
  if (kindBits === get_BinaryKindShift())
    return CreateFromTicks(ticks, get_DateTimeKindUtc());
  return CreateFromTicks(ticks, get_DateTimeKindUnspecified());
}
export function _df025c273bde0e50(fileTime) {
  if (fileTime < get_ZeroTicks())
    throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");
  return createFromInstantTicks(fileTime - get_FileTimeUnixEpochTicks() + get_UnixEpochTicks(), get_DateTimeKindLocal());
}
export function _93886aebedb72920(fileTime) {
  if (fileTime < get_ZeroTicks())
    throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");
  return CreateFromTicks(fileTime - get_FileTimeUnixEpochTicks() + get_UnixEpochTicks(), get_DateTimeKindUtc());
}
export function _12520a637fb85a70(d) {
  return CreateFromTicks(createRoundedTicksFromDouble((d - get_OADateUnixOffsetDays()) * get_MillisecondsPerDay()) * get_TicksPerMillisecond() + get_UnixEpochTicks(), get_DateTimeKindUnspecified());
}
export function _d3b1cc7e750c6bc3(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return false;
  let year = instance.date.getFullYear();
  let januaryOffset = createLocalDate(year, 1, 1).getTimezoneOffset();
  let julyOffset = createLocalDate(year, 7, 1).getTimezoneOffset();
  let standardOffset = januaryOffset > julyOffset ? januaryOffset : julyOffset;
  return instance.date.getTimezoneOffset() < standardOffset;
}
export function _a99826a92073614e(value, kind) {
  return CreateDateTime(value.date, getKind(kind), value.subMillisecondTicks);
}
export function _9cea54115c704cf7(instance) {
  if (instance.kind === get_DateTimeKindLocal())
    return getInstantTicks(instance) + get_BinaryLocalMask();
  return getTicks(instance) + BigInt(instance.kind) * get_BinaryKindShift();
}
export function _d77d20d9d04e2b6b(instance) {
  return createDateTime(createLocalDate(instance.date.getFullYear(), instance.date.getMonth() + 1, instance.date.getDate()), instance.kind);
}
export function _3b9ecf5fd3c301db(instance) {
  return instance.date.getDate();
}
export function _6070f1709c491634(instance) {
  return instance.date.getDay();
}
export function _4f6ca20bf1aaa2d3(instance) {
  let year = instance.date.getFullYear();
  let start = Date.UTC(year, 0, 0);
  let current = Date.UTC(year, instance.date.getMonth(), instance.date.getDate());
  return Math.floor((current - start) / 86400000);
}
export function _d3529b55e30e2a12(instance) {
  return getInt64HashCode(getTicks(instance));
}
export function _f263cff61e6628a9(instance) {
  return instance.date.getHours();
}
export function _551add245db0b701(instance) {
  return instance.kind;
}
export function _742a8bcf918b97e6(instance) {
  return instance.date.getMilliseconds();
}
export function _34d05014c270366f(instance) {
  return Number(instance.subMillisecondTicks / get_TicksPerMicrosecond() % BigInt(1000));
}
export function _46e11fe2eb2ee869(instance) {
  return Number(instance.subMillisecondTicks % get_TicksPerMicrosecond() * BigInt(100));
}
export function _f4ca5de4f63aa097(instance) {
  return instance.date.getMinutes();
}
export function _a8a6b6e36a0ea736(instance) {
  return instance.date.getMonth() + 1;
}
export function _ee9dd166a34a2fa5() {
  return new JDateTime(new Date, get_DateTimeKindLocal());
}
export function _10a94eacb3b7fd2d(instance) {
  return instance.date.getSeconds();
}
export function _bcde32e170f49354(instance) {
  return getTicks(instance);
}
export function _2efdc237be2f31aa(instance) {
  let ms = ((instance.date.getHours() * 60 + instance.date.getMinutes()) * 60 + instance.date.getSeconds()) * 1000 + instance.date.getMilliseconds();
  return new JTimeSpan(BigInt(ms) * get_TicksPerMillisecond() + instance.subMillisecondTicks);
}
export function _4b250155b7c688bb() {
  let now = new Date;
  return new JDateTime(createLocalDate(now.getFullYear(), now.getMonth() + 1, now.getDate()), get_DateTimeKindLocal());
}
export function _9d56b09432f81c05(instance) {
  return instance.date.getFullYear();
}
export function _4a9da83e9cb28c1a(year) {
  ensureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number between 1 and 9999.");
  if (year < 1 || year > 9999)
    throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
  return year % 4 === 0 && year % 100 !== 0 || year % 400 === 0;
}
export function _a8a015c2d2bff2f6(s) {
  return parseCore(s);
}
export function _e0128ef45cc8584e(s, provider) {
  return parseCore(s);
}
export function _7372e5e0d8ba24a6(s, provider, styles) {
  return applyDateTimeStyles(parseCore(s), s, styles);
}
export function _2c85f5b20ae7559e(s, provider, styles) {
  return applyDateTimeStyles(parseCore(s), s, styles);
}
export function _4f5d235cac779f38(instance, value) {
  return _85b6d162b092ce0e(instance, value);
}
export function _20a406afebff2025(instance, value) {
  return _8d9ea66839ce392a(instance, value);
}
export function _fb61bb2ccf4b10b6(instance) {
  return Number(getTicks(instance) - get_UnixEpochTicks()) / 10000 / get_MillisecondsPerDay() + get_OADateUnixOffsetDays();
}
export function _37ee48ca629793fa(instance) {
  return getInstantTicks(instance) - get_UnixEpochTicks() + get_FileTimeUnixEpochTicks();
}
export function _c02c49ea68661175(instance) {
  return _37ee48ca629793fa(instance);
}
export function _db842725d5fd1ca0(instance) {
  if (instance.kind === get_DateTimeKindLocal())
    return CreateDateTime(instance.date, get_DateTimeKindLocal(), instance.subMillisecondTicks);
  let instantTicks = instance.kind === get_DateTimeKindUnspecified() ? getTicks(instance) : getInstantTicks(instance);
  return createFromInstantTicks(instantTicks, get_DateTimeKindLocal());
}
export function _6e78dc03eecdd423(instance) {
  return formatDateTime(instance, "D", null);
}
export function _ab161bb1563732af(instance) {
  return formatDateTime(instance, "T", null);
}
export function _6a67d54f5c865e5e(instance) {
  return formatDateTime(instance, "d", null);
}
export function _af2d02ec0c0a300d(instance) {
  return formatDateTime(instance, "t", null);
}
export function _6659b3b5d1f081dd(instance) {
  return formatDateTime(instance, null, null);
}
export function _3ee3e9478fe9a1fb(instance, format) {
  return formatDateTime(instance, format, null);
}
export function _606066f0ee1488c6(instance, provider) {
  return formatDateTime(instance, null, provider);
}
export function _85393faf5839b9ef(instance, format, provider) {
  return formatDateTime(instance, format, provider);
}
export function _b62871088df3ca8f(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return CreateDateTime(instance.date, get_DateTimeKindUtc(), instance.subMillisecondTicks);
  return createFromInstantTicks(getInstantTicks(instance), get_DateTimeKindUtc());
}
export function _fa25ca318f086bb6(s, result) {
  if (s === null || s.length === 0)
    return [false, createDefaultDateTime()];
  try {
    return [true, parseCore(s)];
  } catch {
    return [false, createDefaultDateTime()];
  }
}
export function _8658c3be6edb9d2c(s, result) {
  return _fa25ca318f086bb6(s, result);
}
export function _34043b1eb3a8183a(s, provider, styles, result) {
  validateDateTimeStyles(getDateTimeStylesValue(styles));
  if (s === null || s.length === 0)
    return [false, createDefaultDateTime()];
  try {
    return [true, applyDateTimeStyles(parseCore(s), s, styles)];
  } catch {
    return [false, createDefaultDateTime()];
  }
}
export function _6e8546b461b48646(s, provider, styles, result) {
  return _34043b1eb3a8183a(s, provider, styles, result);
}
export function _d48b23d7c5f7c2aa(d, t) {
  return CreateFromTicks(getTicks(d) + t.ticks, d.kind);
}
export function _8d9ea66839ce392a(d, t) {
  return CreateFromTicks(getTicks(d) - t.ticks, d.kind);
}
export function _85b6d162b092ce0e(d1, d2) {
  return new JTimeSpan(getTicks(d1) - getTicks(d2));
}
export function _37d87f65292f7083(d1, d2) {
  return getTicks(d1) === getTicks(d2);
}
export function _89406f797d33e566(d1, d2) {
  return getTicks(d1) !== getTicks(d2);
}
export function _5a97e2aec50193b3(t1, t2) {
  return getTicks(t1) < getTicks(t2);
}
export function _a8b15168323b118c(t1, t2) {
  return getTicks(t1) <= getTicks(t2);
}
export function _e98b0598f4980bcc(t1, t2) {
  return getTicks(t1) > getTicks(t2);
}
export function _91697ebd6031bb97(t1, t2) {
  return getTicks(t1) >= getTicks(t2);
}
export function _bcf4183bef96ea21(instance, date, time) {
  return [new JDateOnly(instance.date.getFullYear(), instance.date.getMonth() + 1, instance.date.getDate()), _a305982aa6859677(instance)];
}
export function _5f721827cf6b8105(instance, year, month, day) {
  return [instance.date.getFullYear(), instance.date.getMonth() + 1, instance.date.getDate()];
}
export function _6c36c46db30aacc1(s, provider, result) {
  return _fa25ca318f086bb6(s, result);
}
export function _41dcf008ea7cf6d9(s, provider) {
  return _a8a015c2d2bff2f6(s);
}
export function _63fd53f09ba16132(s, provider, result) {
  return _fa25ca318f086bb6(s, result);
}
export function _d4c39bdf47f391cf() {
  return createUtcNow();
}
