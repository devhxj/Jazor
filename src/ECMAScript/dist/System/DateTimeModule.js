import { IsFiniteCore, IsNaNCore } from "System/DoubleModule.js";
import { CreateLocalDate as i$bef5a205ba6f9c81, CreateLocalDateTime as i$31ea2871f7c34377, CreateUtcDate, FormatDateOnlyText, GetDaysInMonth, GetInt64HashCode, JDateOnly, JDateTime, JTimeSpan, Pad2, Pad7, PadLeft, RequireGregorianCalendar } from "System/RuntimeModule.js";
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
function get_AllowedDateTimeStylesMask() {
  return 255;
}
function EnsureWholeNumber(value, message) {
  if (isNaN(value) || Math.floor(value) !== value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
    throw new Error(message);
}
function CreateDefaultDateTime() {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(1, 1, 1), get_DateTimeKindUnspecified());
}
function CreateLocalDate(year, month, day) {
  return i$bef5a205ba6f9c81(year, month, day);
}
function CreateLocalDateTime(year, month, day, hour, minute, second, millisecond) {
  return i$31ea2871f7c34377(year, month, day, hour, minute, second, millisecond);
}
function CreateFromTicks_113799a103c38477(ticks) {
  return CreateFromTicks_8e90031c765a4910(ticks, get_DateTimeKindUnspecified());
}
function CreateFromTicks_8e90031c765a4910(ticks, kind) {
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
  return new JDateTime("$ctor_9eb10cd821441a68", CreateLocalDateTime(utc.getUTCFullYear(), utc.getUTCMonth() + 1, utc.getUTCDate(), utc.getUTCHours(), utc.getUTCMinutes(), utc.getUTCSeconds(), utc.getUTCMilliseconds()), kind, subMillisecondTicks);
}
function CreateDateTime_6f4bad8e5307c00a(date, kind) {
  return new JDateTime("$ctor_31a0f1908d992f04", date, kind);
}
function CreateDateTime_19b9b04d7a90c04d(date, kind, subMillisecondTicks) {
  return new JDateTime("$ctor_9eb10cd821441a68", date, kind, subMillisecondTicks);
}
function CreateFromInstantTicks(ticks, kind) {
  if (ticks < get_ZeroTicks() || ticks > get_MaxDateTimeTicks())
    throw new Error("ArgumentOutOfRangeException: Ticks must be within the range of DateTime.");
  if (kind === get_DateTimeKindUtc())
    return CreateFromTicks_8e90031c765a4910(ticks, kind);
  let ticksSinceUnixEpoch = ticks - get_UnixEpochTicks();
  let milliseconds = ticksSinceUnixEpoch / get_TicksPerMillisecond();
  let subMillisecondTicks = ticksSinceUnixEpoch % get_TicksPerMillisecond();
  if (subMillisecondTicks < get_ZeroTicks()) {
    milliseconds -= BigInt(1);
    subMillisecondTicks += get_TicksPerMillisecond();
  }
  return new JDateTime("$ctor_9eb10cd821441a68", new Date(Number(milliseconds)), kind, subMillisecondTicks);
}
function GetKind(kind) {
  let value = Number(kind);
  if (value !== get_DateTimeKindUnspecified() && value !== get_DateTimeKindUtc() && value !== get_DateTimeKindLocal())
    throw new Error("ArgumentException: Invalid DateTimeKind value.");
  return value;
}
function GetMicrosecondTicks(microsecond) {
  if (Math.floor(microsecond) !== microsecond || microsecond < 0 || microsecond > 999)
    throw new Error("ArgumentOutOfRangeException: Microsecond must be between 0 and 999.");
  return BigInt(microsecond) * get_TicksPerMicrosecond();
}
function GetTicks_a125094c105bae75(instance) {
  let date = instance.date;
  let milliseconds = Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds());
  return BigInt(milliseconds) * get_TicksPerMillisecond() + instance.subMillisecondTicks + get_UnixEpochTicks();
}
function GetTicks_2925fdfd7b05f4de(date) {
  return BigInt(date.getTime()) * get_TicksPerMillisecond() + get_UnixEpochTicks();
}
function GetInstantTicks(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return GetTicks_a125094c105bae75(instance);
  return BigInt(instance.date.getTime()) * get_TicksPerMillisecond() + instance.subMillisecondTicks + get_UnixEpochTicks();
}
function CreateUtcNow() {
  let now = new Date;
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDateTime(now.getUTCFullYear(), now.getUTCMonth() + 1, now.getUTCDate(), now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds()), get_DateTimeKindUtc());
}
function GetProviderLocale(provider) {
  let locale, numberFormat;
  if (typeof provider === "string" && (locale = provider, true))
    return locale;
  if (provider instanceof Intl.NumberFormat && (numberFormat = provider, true))
    return numberFormat.resolvedOptions().locale;
  return (new Intl.DateTimeFormat).resolvedOptions().locale;
}
function JoinFormatParts(parts) {
  let text = "";
  for (let i = 0; i < parts.length; i++)
    text += parts[i].value;
  return text;
}
function GetInvariantMonthName(month) {
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
function GetInvariantAbbreviatedMonthName(month) {
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
function GetInvariantDayName(dayOfWeek) {
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
function GetInvariantAbbreviatedDayName(dayOfWeek) {
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
function IsAsciiLetter(value) {
  return value.charCodeAt(0) >= "A".charCodeAt(0) && value.charCodeAt(0) <= "Z".charCodeAt(0) || value.charCodeAt(0) >= "a".charCodeAt(0) && value.charCodeAt(0) <= "z".charCodeAt(0);
}
function GetLocalizedMonthName(locale, month, abbreviated) {
  if (locale.length === 0)
    return abbreviated ? GetInvariantAbbreviatedMonthName(month) : GetInvariantMonthName(month);
  return JoinFormatParts(new Intl.DateTimeFormat(locale, { month: abbreviated ? "short" : "long", timeZone: "UTC" }).formatToParts(new Date(Date.UTC(2000, month - 1, 1))));
}
function GetLocalizedDayName(locale, dayOfWeek, abbreviated) {
  if (locale.length === 0)
    return abbreviated ? GetInvariantAbbreviatedDayName(dayOfWeek) : GetInvariantDayName(dayOfWeek);
  return JoinFormatParts(new Intl.DateTimeFormat(locale, { weekday: abbreviated ? "short" : "long", timeZone: "UTC" }).formatToParts(new Date(Date.UTC(2024, 0, 7 + dayOfWeek))));
}
function GetDateSeparator(locale) {
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
function GetTimeSeparator(locale) {
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
function GetLocalizedDayPeriod(date, locale) {
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
function FormatOffsetTicks(offsetTicks, count) {
  let negative = offsetTicks < 0n;
  let absolute = negative ? -offsetTicks : offsetTicks;
  let totalMinutes = absolute / get_OffsetMinuteTicks();
  let hours = Number(totalMinutes / BigInt(60));
  let minutes = Number(totalMinutes % BigInt(60));
  let sign = negative ? "-" : "+";
  if (count <= 1)
    return sign + hours;
  if (count === 2)
    return sign + Pad2(hours);
  return sign + Pad2(hours) + ":" + Pad2(minutes);
}
function GetRoundtripSuffix(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return "Z";
  if (instance.kind === get_DateTimeKindLocal())
    return FormatOffsetTicks(BigInt(-instance.date.getTimezoneOffset()) * get_OffsetMinuteTicks(), 3);
  return "";
}
function FormatInvariantGeneralDateTime(instance, includeSeconds) {
  let date = instance.date;
  let text = Pad2(date.getMonth() + 1) + "/" + Pad2(date.getDate()) + "/" + PadLeft(date.getFullYear().toString(), 4) + " " + Pad2(date.getHours()) + ":" + Pad2(date.getMinutes());
  if (includeSeconds)
    text += ":" + Pad2(date.getSeconds());
  return text;
}
function FormatInvariantShortDate(instance) {
  let date = instance.date;
  return Pad2(date.getMonth() + 1) + "/" + Pad2(date.getDate()) + "/" + PadLeft(date.getFullYear().toString(), 4);
}
function FormatInvariantLongDate(instance) {
  let date = instance.date;
  return GetInvariantDayName(date.getDay()) + ", " + Pad2(date.getDate()) + " " + GetInvariantMonthName(date.getMonth() + 1) + " " + PadLeft(date.getFullYear().toString(), 4);
}
function FormatInvariantTime(instance, includeSeconds) {
  let date = instance.date;
  let text = Pad2(date.getHours()) + ":" + Pad2(date.getMinutes());
  if (includeSeconds)
    text += ":" + Pad2(date.getSeconds());
  return text;
}
function FormatMonthDay(instance, provider) {
  let locale = GetProviderLocale(provider);
  if (locale.length === 0)
    return GetInvariantMonthName(instance.date.getMonth() + 1) + " " + Pad2(instance.date.getDate());
  return FormatLocaleDateTime(instance.date, locale, { month: "long", day: "2-digit" });
}
function FormatYearMonth(instance, provider) {
  let locale = GetProviderLocale(provider);
  if (locale.length === 0)
    return PadLeft(instance.date.getFullYear().toString(), 4) + " " + GetInvariantMonthName(instance.date.getMonth() + 1);
  return FormatLocaleDateTime(instance.date, locale, { year: "numeric", month: "long" });
}
function FormatFullDateTime(instance, includeSeconds, provider) {
  return FormatLongDate(instance, provider) + " " + FormatTime(instance, includeSeconds, provider);
}
function GetUniversalDateTimeForFormatting(instance) {
  let date = instance.date;
  if (instance.kind === get_DateTimeKindUtc()) {
    return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds()));
  }
  return new Date(date.getTime());
}
function FormatUniversalFullDateTime(instance, provider) {
  let utc = GetUniversalDateTimeForFormatting(instance);
  let locale = GetProviderLocale(provider);
  if (locale.length === 0) {
    return GetInvariantDayName(utc.getUTCDay()) + ", " + Pad2(utc.getUTCDate()) + " " + GetInvariantMonthName(utc.getUTCMonth() + 1) + " " + PadLeft(utc.getUTCFullYear().toString(), 4) + " " + Pad2(utc.getUTCHours()) + ":" + Pad2(utc.getUTCMinutes()) + ":" + Pad2(utc.getUTCSeconds());
  }
  return JoinFormatParts(new Intl.DateTimeFormat(locale, {
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
function FormatRfc1123DateTime(instance) {
  let date = instance.date;
  return GetInvariantAbbreviatedDayName(date.getDay()) + ", " + Pad2(date.getDate()) + " " + GetInvariantAbbreviatedMonthName(date.getMonth() + 1) + " " + PadLeft(date.getFullYear().toString(), 4) + " " + Pad2(date.getHours()) + ":" + Pad2(date.getMinutes()) + ":" + Pad2(date.getSeconds()) + " GMT";
}
function FormatLocaleDateTime(date, locale, options) {
  return JoinFormatParts(new Intl.DateTimeFormat(locale, options).formatToParts(date));
}
function FormatGeneralDateTime(instance, includeSeconds, provider) {
  let locale = GetProviderLocale(provider);
  if (locale.length === 0)
    return FormatInvariantGeneralDateTime(instance, includeSeconds);
  return FormatLocaleDateTime(instance.date, locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: includeSeconds ? "2-digit" : null,
    hour12: false
  });
}
function FormatShortDate(instance, provider) {
  let locale = GetProviderLocale(provider);
  if (locale.length === 0)
    return FormatInvariantShortDate(instance);
  return FormatLocaleDateTime(instance.date, locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  });
}
function FormatLongDate(instance, provider) {
  let locale = GetProviderLocale(provider);
  if (locale.length === 0)
    return FormatInvariantLongDate(instance);
  return FormatLocaleDateTime(instance.date, locale, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "2-digit"
  });
}
function FormatTime(instance, includeSeconds, provider) {
  let locale = GetProviderLocale(provider);
  if (locale.length === 0)
    return FormatInvariantTime(instance, includeSeconds);
  if (!includeSeconds) {
    return FormatLocaleDateTime(instance.date, locale, {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false
    });
  }
  return FormatLocaleDateTime(instance.date, locale, {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: false
  });
}
function TryParseAsciiNumber(text, value) {
  value = 0;
  if (text.length === 0)
    return [false, value];
  for (let index = 0; index < text.length; index++) {
    let character = _5ad63706a889c294(text, index);
    if (character.charCodeAt(0) < "0".charCodeAt(0) || character.charCodeAt(0) > "9".charCodeAt(0))
      return [false, value];
    value = value * 10 + character.charCodeAt(0) - "0".charCodeAt(0);
  }
  return [true, value];
}
function FindNextLiteral(parts, startIndex) {
  for (let index = startIndex; index < parts.length; index++) {
    if (parts[index].type === "literal" && parts[index].value.length > 0)
      return parts[index].value;
  }
  return null;
}
function TryGetLocalizedLongMonth(formatter, text, month) {
  for (let candidate = 1; candidate <= 12; candidate++) {
    let parts = formatter.formatToParts(CreateLocalDate(2006, candidate, 22));
    for (let index = 0; index < parts.length; index++) {
      if (parts[index].type === "month" && parts[index].value === text) {
        month = candidate;
        return [true, month];
      }
    }
  }
  month = 0;
  return [false, month];
}
function TryParseLocalizedLongDate(text, year, month, day) {
  year = 0;
  month = 0;
  day = 0;
  let locale = GetProviderLocale(null);
  if (locale.length === 0)
    return [false, year, month, day];
  let formatter = new Intl.DateTimeFormat(locale, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "2-digit"
  });
  let parts = formatter.formatToParts(CreateLocalDate(2006, 11, 22));
  let offset = 0;
  for (let index = 0; index < parts.length; index++) {
    let part = parts[index];
    if (part.type === "literal") {
      if (!text.substring(offset).startsWith(part.value))
        return [false, year, month, day];
      offset += part.value.length;
      continue;
    }
    let nextLiteral = FindNextLiteral(parts, index + 1);
    let end = nextLiteral === null ? text.length : text.indexOf(nextLiteral, offset);
    if (end < offset)
      return [false, year, month, day];
    let value = text.substring(offset, offset + (end - offset));
    offset = end;
    if (part.type === "year") {
      let __ref$05d25652f46e931f9f971089;
      if (!(__ref$05d25652f46e931f9f971089 = TryParseAsciiNumber(value, undefined), year = __ref$05d25652f46e931f9f971089[1], __ref$05d25652f46e931f9f971089[0]))
        return [false, year, month, day];
    }
    else if (part.type === "day") {
      let __ref$86e18dac26a381ef26dbc5a3;
      if (!(__ref$86e18dac26a381ef26dbc5a3 = TryParseAsciiNumber(value, undefined), day = __ref$86e18dac26a381ef26dbc5a3[1], __ref$86e18dac26a381ef26dbc5a3[0]))
        return [false, year, month, day];
    }
    else if (part.type === "month") {
      let __ref$459f5a801d66481bd162d783, __ref$02e2c649a492c0b34dd5c2c2;
      if (!(__ref$459f5a801d66481bd162d783 = TryParseAsciiNumber(value, undefined), month = __ref$459f5a801d66481bd162d783[1], __ref$459f5a801d66481bd162d783[0]) && !(__ref$02e2c649a492c0b34dd5c2c2 = TryGetLocalizedLongMonth(formatter, value, undefined), month = __ref$02e2c649a492c0b34dd5c2c2[1], __ref$02e2c649a492c0b34dd5c2c2[0]))
        return [false, year, month, day];
    }
  }
  if (offset !== text.length || year < 1 || month < 1 || month > 12 || day < 1)
    return [false, year, month, day];
  let date = CreateLocalDate(year, month, day);
  return [date.getFullYear() === year && date.getMonth() + 1 === month && date.getDate() === day, year, month, day];
}
function FormatRoundtripDateTime(instance) {
  let date = instance.date;
  return FormatDateOnlyText(date.getFullYear(), date.getMonth() + 1, date.getDate()) + "T" + Pad2(date.getHours()) + ":" + Pad2(date.getMinutes()) + ":" + Pad2(date.getSeconds()) + "." + Pad7(BigInt(date.getMilliseconds()) * get_TicksPerMillisecond() + instance.subMillisecondTicks) + GetRoundtripSuffix(instance);
}
function FormatSortableDateTime(instance) {
  let date = instance.date;
  return FormatDateOnlyText(date.getFullYear(), date.getMonth() + 1, date.getDate()) + "T" + Pad2(date.getHours()) + ":" + Pad2(date.getMinutes()) + ":" + Pad2(date.getSeconds());
}
function FormatUniversalSortableDateTime(instance) {
  return FormatSortableDateTime(instance).replaceAll("T", " ") + "Z";
}
function FormatFraction(fraction, count, trimTrailingZeros) {
  let text = Pad7(fraction);
  if (count < 7)
    text = text.substring(0, 0 + count);
  if (!trimTrailingZeros)
    return text;
  while (text.length > 0 && _5ad63706a889c294(text, text.length - 1).charCodeAt(0) === "0".charCodeAt(0))
    text = text.substring(0, 0 + (text.length - 1));
  return text;
}
function FormatCustomToken(instance, token, count, locale, dateSeparator, timeSeparator) {
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
  let suffix = GetRoundtripSuffix(instance);
  switch (token) {
    case "y":
      if (count === 2)
        return Pad2(year % 100);
      return PadLeft(year.toString(), count < 4 ? 4 : count);
    case "M":
      if (count === 1)
        return month.toString();
      if (count === 2)
        return Pad2(month);
      if (count === 3)
        return GetLocalizedMonthName(locale, month, true);
      return GetLocalizedMonthName(locale, month, false);
    case "d":
      if (count === 1)
        return day.toString();
      if (count === 2)
        return Pad2(day);
      if (count === 3)
        return GetLocalizedDayName(locale, date.getDay(), true);
      return GetLocalizedDayName(locale, date.getDay(), false);
    case "H":
      return count === 1 ? hour.toString() : Pad2(hour);
    case "h":
      return count === 1 ? hour12.toString() : Pad2(hour12);
    case "m":
      return count === 1 ? minute.toString() : Pad2(minute);
    case "s":
      return count === 1 ? second.toString() : Pad2(second);
    case "t":
      let dayPeriod = GetLocalizedDayPeriod(date, locale);
      return count === 1 ? dayPeriod.substring(0, 0 + 1) : dayPeriod;
    case "f":
      return FormatFraction(fraction, count, false);
    case "F":
      return FormatFraction(fraction, count, true);
    case "z":
      return instance.kind === get_DateTimeKindLocal() ? FormatOffsetTicks(offset, count) : "";
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
function FormatCustomDateTime(instance, format, provider) {
  let locale = GetProviderLocale(provider);
  let dateSeparator = GetDateSeparator(locale);
  let timeSeparator = GetTimeSeparator(locale);
  let text = "";
  for (let i = 0; i < format.length; ) {
    let token = _5ad63706a889c294(format, i);
    if (token.charCodeAt(0) === "%".charCodeAt(0)) {
      if (i + 1 >= format.length || _5ad63706a889c294(format, i + 1).charCodeAt(0) === "%".charCodeAt(0))
        throw new Error("FormatException: Input string was not in a correct format.");
      text += FormatCustomToken(instance, _5ad63706a889c294(format, i + 1), 1, locale, dateSeparator, timeSeparator);
      i += 2;
      continue;
    }
    if (token.charCodeAt(0) === "\\".charCodeAt(0)) {
      if (i + 1 < format.length)
        text += _5ad63706a889c294(format, i + 1);
      i += 2;
      continue;
    }
    if (token.charCodeAt(0) === "'".charCodeAt(0) || token.charCodeAt(0) === "\"".charCodeAt(0)) {
      let quote = token;
      i++;
      while (i < format.length && _5ad63706a889c294(format, i).charCodeAt(0) !== quote.charCodeAt(0)) {
        text += _5ad63706a889c294(format, i);
        i++;
      }
      if (i < format.length)
        i++;
      continue;
    }
    let count = 1;
    while (i + count < format.length && _5ad63706a889c294(format, i + count).charCodeAt(0) === token.charCodeAt(0))
      count++;
    text += FormatCustomToken(instance, token, count, locale, dateSeparator, timeSeparator);
    i += count;
  }
  return text;
}
function FormatDateTime(instance, format, provider) {
  if (format === null || format.length === 0)
    return FormatGeneralDateTime(instance, true, provider);
  if (format.length === 1) {
    switch (_5ad63706a889c294(format, 0)) {
      case "f":
        return FormatFullDateTime(instance, false, provider);
      case "F":
        return FormatFullDateTime(instance, true, provider);
      case "O":
      case "o":
        return FormatRoundtripDateTime(instance);
      case "G":
        return FormatGeneralDateTime(instance, true, provider);
      case "g":
        return FormatGeneralDateTime(instance, false, provider);
      case "M":
      case "m":
        return FormatMonthDay(instance, provider);
      case "R":
      case "r":
        return FormatRfc1123DateTime(instance);
      case "d":
        return FormatShortDate(instance, provider);
      case "D":
        return FormatLongDate(instance, provider);
      case "t":
        return FormatTime(instance, false, provider);
      case "T":
        return FormatTime(instance, true, provider);
      case "s":
        return FormatSortableDateTime(instance);
      case "u":
        return FormatUniversalSortableDateTime(instance);
      case "U":
        return FormatUniversalFullDateTime(instance, provider);
      case "Y":
      case "y":
        return FormatYearMonth(instance, provider);
      default:
        if (IsAsciiLetter(_5ad63706a889c294(format, 0)))
          throw new Error("FormatException: Input string was not in a correct format.");
        break;
    }
  }
  return FormatCustomDateTime(instance, format, provider);
}
function IsAsciiDigit(value) {
  return value.charCodeAt(0) >= "0".charCodeAt(0) && value.charCodeAt(0) <= "9".charCodeAt(0);
}
function TryParseTwoDigits(text, start, value) {
  value = 0;
  if (start < 0 || start + 2 > text.length)
    return [false, value];
  if (!IsAsciiDigit(_5ad63706a889c294(text, start)) || !IsAsciiDigit(_5ad63706a889c294(text, start + 1)))
    return [false, value];
  value = Number(text.substring(start, start + 2));
  return [true, value];
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
  if (text.length < 10 || _5ad63706a889c294(text, 4).charCodeAt(0) !== "-".charCodeAt(0) || _5ad63706a889c294(text, 7).charCodeAt(0) !== "-".charCodeAt(0))
    return false;
  for (let i = 0; i < 10; i++) {
    if (i === 4 || i === 7)
      continue;
    if (!IsAsciiDigit(_5ad63706a889c294(text, i)))
      return false;
  }
  return true;
}
function CreateUtcDateTimeTicks(year, month, day, hour, minute, second, millisecond) {
  let utc = CreateUtcDate(year, month, day);
  utc.setUTCHours(hour, minute, second, millisecond);
  return BigInt(utc.getTime()) * get_TicksPerMillisecond() + get_UnixEpochTicks();
}
function TryParseIsoDateTime(text, year, month, day, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks) {
  let __ref$df68ca28dd6531f31a419891, __ref$785c2c1a21f56fd642b9fc4b, __ref$0fb2f79392fa6fd96812067d;
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
  if (!(__ref$df68ca28dd6531f31a419891 = TryParseIsoDate(text.substring(0, 0 + 10), undefined, undefined, undefined), year = __ref$df68ca28dd6531f31a419891[1], month = __ref$df68ca28dd6531f31a419891[2], day = __ref$df68ca28dd6531f31a419891[3], __ref$df68ca28dd6531f31a419891[0]))
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
  if (separator.charCodeAt(0) !== "T".charCodeAt(0) && separator.charCodeAt(0) !== " ".charCodeAt(0))
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
  if (!(__ref$785c2c1a21f56fd642b9fc4b = TryParseTwoDigits(text, 11, undefined), hour = __ref$785c2c1a21f56fd642b9fc4b[1], __ref$785c2c1a21f56fd642b9fc4b[0]) || _5ad63706a889c294(text, 13).charCodeAt(0) !== ":".charCodeAt(0) || !(__ref$0fb2f79392fa6fd96812067d = TryParseTwoDigits(text, 14, undefined), minute = __ref$0fb2f79392fa6fd96812067d[1], __ref$0fb2f79392fa6fd96812067d[0]))
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
  if (index < text.length && _5ad63706a889c294(text, index).charCodeAt(0) === ":".charCodeAt(0)) {
    let __ref$cef5f3c26bb6503514cb4b66;
    if (!(__ref$cef5f3c26bb6503514cb4b66 = TryParseTwoDigits(text, index + 1, undefined), second = __ref$cef5f3c26bb6503514cb4b66[1], __ref$cef5f3c26bb6503514cb4b66[0]))
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
  if (index < text.length && _5ad63706a889c294(text, index).charCodeAt(0) === ".".charCodeAt(0)) {
    index++;
    let fractionStart = index;
    while (index < text.length && IsAsciiDigit(_5ad63706a889c294(text, index)))
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
  if (index === text.length - 1 && (_5ad63706a889c294(text, index).charCodeAt(0) === "Z".charCodeAt(0) || _5ad63706a889c294(text, index).charCodeAt(0) === "z".charCodeAt(0))) {
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
  if (sign.charCodeAt(0) !== "+".charCodeAt(0) && sign.charCodeAt(0) !== "-".charCodeAt(0))
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
    let __ref$02ee7cb66cc939f0c9e44686;
    if (!(__ref$02ee7cb66cc939f0c9e44686 = TryParseTwoDigits(text, index + 1, undefined), offsetHours = __ref$02ee7cb66cc939f0c9e44686[1], __ref$02ee7cb66cc939f0c9e44686[0]))
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
    let __ref$02ee7cb66cc939f0c9e44686, __ref$a3fc711d2137fd6d45e645c0;
    if (!(__ref$02ee7cb66cc939f0c9e44686 = TryParseTwoDigits(text, index + 1, undefined), offsetHours = __ref$02ee7cb66cc939f0c9e44686[1], __ref$02ee7cb66cc939f0c9e44686[0]) || !(__ref$a3fc711d2137fd6d45e645c0 = TryParseTwoDigits(text, index + 3, undefined), offsetMinutes = __ref$a3fc711d2137fd6d45e645c0[1], __ref$a3fc711d2137fd6d45e645c0[0]))
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
  else if (remaining === 5 && _5ad63706a889c294(text, index + 3).charCodeAt(0) === ":".charCodeAt(0)) {
    let __ref$02ee7cb66cc939f0c9e44686, __ref$47e33b86713c879fe13144c2;
    if (!(__ref$02ee7cb66cc939f0c9e44686 = TryParseTwoDigits(text, index + 1, undefined), offsetHours = __ref$02ee7cb66cc939f0c9e44686[1], __ref$02ee7cb66cc939f0c9e44686[0]) || !(__ref$47e33b86713c879fe13144c2 = TryParseTwoDigits(text, index + 4, undefined), offsetMinutes = __ref$47e33b86713c879fe13144c2[1], __ref$47e33b86713c879fe13144c2[0]))
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
  if (sign.charCodeAt(0) === "-".charCodeAt(0))
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
function TryParseTimeOnly(text, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks) {
  let __ref$2a3e1697d21cba0ae4532637, __ref$41063146d69994e20aa0fa6c;
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
  if (!(__ref$2a3e1697d21cba0ae4532637 = TryParseTwoDigits(text, 0, undefined), hour = __ref$2a3e1697d21cba0ae4532637[1], __ref$2a3e1697d21cba0ae4532637[0]) || _5ad63706a889c294(text, 2).charCodeAt(0) !== ":".charCodeAt(0) || !(__ref$41063146d69994e20aa0fa6c = TryParseTwoDigits(text, 3, undefined), minute = __ref$41063146d69994e20aa0fa6c[1], __ref$41063146d69994e20aa0fa6c[0]))
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
  if (index < text.length && _5ad63706a889c294(text, index).charCodeAt(0) === ":".charCodeAt(0)) {
    let __ref$aacc5489b0c85c0946be82b6;
    if (!(__ref$aacc5489b0c85c0946be82b6 = TryParseTwoDigits(text, index + 1, undefined), second = __ref$aacc5489b0c85c0946be82b6[1], __ref$aacc5489b0c85c0946be82b6[0]))
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
  if (index < text.length && _5ad63706a889c294(text, index).charCodeAt(0) === ".".charCodeAt(0)) {
    index++;
    let fractionStart = index;
    while (index < text.length && IsAsciiDigit(_5ad63706a889c294(text, index)))
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
  if (index === text.length - 1 && (_5ad63706a889c294(text, index).charCodeAt(0) === "Z".charCodeAt(0) || _5ad63706a889c294(text, index).charCodeAt(0) === "z".charCodeAt(0))) {
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
  if (sign.charCodeAt(0) !== "+".charCodeAt(0) && sign.charCodeAt(0) !== "-".charCodeAt(0))
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
    let __ref$c749f6aa5d8c1eafff51846a;
    if (!(__ref$c749f6aa5d8c1eafff51846a = TryParseTwoDigits(text, index + 1, undefined), offsetHours = __ref$c749f6aa5d8c1eafff51846a[1], __ref$c749f6aa5d8c1eafff51846a[0]))
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
    let __ref$c749f6aa5d8c1eafff51846a, __ref$6fdaed36e52beb5277b9c4b1;
    if (!(__ref$c749f6aa5d8c1eafff51846a = TryParseTwoDigits(text, index + 1, undefined), offsetHours = __ref$c749f6aa5d8c1eafff51846a[1], __ref$c749f6aa5d8c1eafff51846a[0]) || !(__ref$6fdaed36e52beb5277b9c4b1 = TryParseTwoDigits(text, index + 3, undefined), offsetMinutes = __ref$6fdaed36e52beb5277b9c4b1[1], __ref$6fdaed36e52beb5277b9c4b1[0]))
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
  else if (remaining === 5 && _5ad63706a889c294(text, index + 3).charCodeAt(0) === ":".charCodeAt(0)) {
    let __ref$c749f6aa5d8c1eafff51846a, __ref$3a02b978764034c46294047c;
    if (!(__ref$c749f6aa5d8c1eafff51846a = TryParseTwoDigits(text, index + 1, undefined), offsetHours = __ref$c749f6aa5d8c1eafff51846a[1], __ref$c749f6aa5d8c1eafff51846a[0]) || !(__ref$3a02b978764034c46294047c = TryParseTwoDigits(text, index + 4, undefined), offsetMinutes = __ref$3a02b978764034c46294047c[1], __ref$3a02b978764034c46294047c[0]))
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
  if (sign.charCodeAt(0) === "-".charCodeAt(0))
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
function CreateRoundedTicksFromDouble(value) {
  if (IsNaNCore(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!IsFiniteCore(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  let rounded = Math.round(value);
  if (!IsFiniteCore(rounded))
    throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTime range.");
  return BigInt(rounded);
}
function CreateAddUnitTicks(value, ticksPerUnit) {
  if (IsNaNCore(value))
    throw new Error("ArgumentException: Value cannot be NaN.");
  if (!IsFiniteCore(value))
    throw new Error("ArgumentOutOfRangeException: Value must be finite.");
  let maxUnitCount = Number(get_MaxDateTimeTicks()) / Number(ticksPerUnit);
  if (Math.abs(value) > maxUnitCount)
    throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTime range.");
  let integralPart = Math.trunc(value);
  let fractionalPart = value - integralPart;
  return BigInt(integralPart) * ticksPerUnit + BigInt(Math.trunc(fractionalPart * Number(ticksPerUnit)));
}
function GetDateTimeStylesValue(styles) {
  return Number(styles);
}
function ValidateDateTimeStyles(styles) {
  if (styles < 0 || Math.floor(styles) !== styles || (styles & ~get_AllowedDateTimeStylesMask()) !== 0)
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
function ApplyDateTimeStyles(value, input, styles) {
  let hour, minute, second, millisecond, timeOnlySubTicks, timeOnlyKind, timeOnlyOffsetTicks, __ref$79220410ba231a08fa945c3c;
  let styleValue = GetDateTimeStylesValue(styles);
  ValidateDateTimeStyles(styleValue);
  let text = input.trim();
  let hasUtcSuffix = HasUtcSuffix(text);
  let hasExplicitOffset = HasExplicitOffset(text);
  let hasExplicitZone = hasUtcSuffix || hasExplicitOffset;
  let noCurrentDateDefault = (styleValue & get_DateTimeStylesNoCurrentDateDefault()) !== 0;
  let adjustToUniversal = (styleValue & get_DateTimeStylesAdjustToUniversal()) !== 0;
  let assumeLocal = (styleValue & get_DateTimeStylesAssumeLocal()) !== 0;
  let assumeUniversal = (styleValue & get_DateTimeStylesAssumeUniversal()) !== 0;
  let roundtripKind = (styleValue & get_DateTimeStylesRoundtripKind()) !== 0;
  if (noCurrentDateDefault && (__ref$79220410ba231a08fa945c3c = TryParseTimeOnly(text, undefined, undefined, undefined, undefined, undefined, undefined, undefined), hour = __ref$79220410ba231a08fa945c3c[1], minute = __ref$79220410ba231a08fa945c3c[2], second = __ref$79220410ba231a08fa945c3c[3], millisecond = __ref$79220410ba231a08fa945c3c[4], timeOnlySubTicks = __ref$79220410ba231a08fa945c3c[5], timeOnlyKind = __ref$79220410ba231a08fa945c3c[6], timeOnlyOffsetTicks = __ref$79220410ba231a08fa945c3c[7], __ref$79220410ba231a08fa945c3c[0])) {
    if (timeOnlyKind === get_DateTimeKindUnspecified()) {
      value = CreateDateTime_19b9b04d7a90c04d(CreateLocalDateTime(1, 1, 1, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), timeOnlySubTicks);
    }
    else {
      let utcTicks = CreateUtcDateTimeTicks(1, 1, 1, hour, minute, second, millisecond) + timeOnlySubTicks - timeOnlyOffsetTicks;
      value = CreateFromInstantTicks(utcTicks, get_DateTimeKindLocal());
    }
  }
  if (hasExplicitZone) {
    if (adjustToUniversal || roundtripKind && hasUtcSuffix)
      return CreateFromInstantTicks(GetInstantTicks(value), get_DateTimeKindUtc());
    return value;
  }
  if (value.kind !== get_DateTimeKindUnspecified())
    return value;
  if (assumeUniversal) {
    let assumedUtcTicks = GetTicks_a125094c105bae75(value);
    if (adjustToUniversal)
      return CreateFromTicks_8e90031c765a4910(assumedUtcTicks, get_DateTimeKindUtc());
    return CreateFromInstantTicks(assumedUtcTicks, get_DateTimeKindLocal());
  }
  if (assumeLocal) {
    if (adjustToUniversal)
      return CreateFromInstantTicks(GetInstantTicks(value), get_DateTimeKindUtc());
    return CreateDateTime_19b9b04d7a90c04d(value.date, get_DateTimeKindLocal(), value.subMillisecondTicks);
  }
  return value;
}
function AddMonthsCore(instance, months) {
  EnsureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");
  let year = instance.date.getFullYear();
  let monthIndex = (year - 1) * 12 + instance.date.getMonth() + months;
  let newYear = Math.floor(monthIndex / 12) + 1;
  let newMonthIndex = monthIndex % 12;
  if (newMonthIndex < 0)
    newMonthIndex += 12;
  let newMonth = newMonthIndex + 1;
  let day = instance.date.getDate();
  let daysInMonth = GetDaysInMonth(newYear, newMonth);
  let newDay = day > daysInMonth ? daysInMonth : day;
  return CreateDateTime_19b9b04d7a90c04d(CreateLocalDateTime(newYear, newMonth, newDay, instance.date.getHours(), instance.date.getMinutes(), instance.date.getSeconds(), instance.date.getMilliseconds()), instance.kind, instance.subMillisecondTicks);
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
    if ((sign.charCodeAt(0) === "+".charCodeAt(0) || sign.charCodeAt(0) === "-".charCodeAt(0)) && _5ad63706a889c294(input, input.length - 3).charCodeAt(0) === ":".charCodeAt(0) && signIndex > timeIndex)
      return true;
  }
  if (input.length >= 5) {
    let signIndex = input.length - 5;
    let sign = _5ad63706a889c294(input, signIndex);
    if ((sign.charCodeAt(0) === "+".charCodeAt(0) || sign.charCodeAt(0) === "-".charCodeAt(0)) && signIndex > timeIndex)
      return true;
  }
  if (input.length >= 3) {
    let signIndex = input.length - 3;
    let sign = _5ad63706a889c294(input, signIndex);
    if ((sign.charCodeAt(0) === "+".charCodeAt(0) || sign.charCodeAt(0) === "-".charCodeAt(0)) && signIndex > timeIndex)
      return true;
  }
  return false;
}
function ExtractSubMillisecondTicks(input) {
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
    if (c.charCodeAt(0) < "0".charCodeAt(0) || c.charCodeAt(0) > "9".charCodeAt(0)) {
      end = i;
      break;
    }
  }
  let digits = ((__jz_arg0, __jz_arg1, __jz_arg2) => __jz_arg0.substring(__jz_arg1, __jz_arg1 + __jz_arg2))(input, fractionIndex + 1, end - fractionIndex - 1);
  if (digits.length === 0 || digits.length > 7)
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid DateTime.`);
  while (digits.length < 7)
    digits += "0";
  return BigInt(digits.substring(3, 3 + 4));
}
function ParseCore(input) {
  let __ref$4d187309f25eefa26e2ed3a2, timeHour, timeMinute, timeSecond, timeMillisecond, timeSubMillisecondTicks, timeKind, timeOffsetTicks, __ref$4976bdb5dd393f6d47224591, year, month, day, __ref$1bbcfa0fbe317dc31d1fd71d, localizedYear, localizedMonth, localizedDay, __ref$315451b3c36a8c676be3bf2c, hour, minute, second, millisecond, subMillisecondTicks, kind, offsetTicks, __ref$b63255e9cabac4e673832d5d;
  if (input === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let s = input.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid DateTime.");
  if (HasIsoDatePrefix(s) && !(__ref$4d187309f25eefa26e2ed3a2 = TryParseIsoDate(s.substring(0, 0 + 10), undefined, undefined, undefined), __ref$4d187309f25eefa26e2ed3a2[0]))
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid DateTime.`);
  if (__ref$4976bdb5dd393f6d47224591 = TryParseTimeOnly(s, undefined, undefined, undefined, undefined, undefined, undefined, undefined), timeHour = __ref$4976bdb5dd393f6d47224591[1], timeMinute = __ref$4976bdb5dd393f6d47224591[2], timeSecond = __ref$4976bdb5dd393f6d47224591[3], timeMillisecond = __ref$4976bdb5dd393f6d47224591[4], timeSubMillisecondTicks = __ref$4976bdb5dd393f6d47224591[5], timeKind = __ref$4976bdb5dd393f6d47224591[6], timeOffsetTicks = __ref$4976bdb5dd393f6d47224591[7], __ref$4976bdb5dd393f6d47224591[0]) {
    let now = new Date;
    let currentYear = now.getFullYear();
    let currentMonth = now.getMonth() + 1;
    let currentDay = now.getDate();
    if (timeKind === get_DateTimeKindUnspecified())
      return new JDateTime("$ctor_9eb10cd821441a68", CreateLocalDateTime(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond), get_DateTimeKindUnspecified(), timeSubMillisecondTicks);
    let utcTicks = CreateUtcDateTimeTicks(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond) + timeSubMillisecondTicks - timeOffsetTicks;
    return CreateFromInstantTicks(utcTicks, get_DateTimeKindLocal());
  }
  if (__ref$1bbcfa0fbe317dc31d1fd71d = TryParseIsoDate(s, undefined, undefined, undefined), year = __ref$1bbcfa0fbe317dc31d1fd71d[1], month = __ref$1bbcfa0fbe317dc31d1fd71d[2], day = __ref$1bbcfa0fbe317dc31d1fd71d[3], __ref$1bbcfa0fbe317dc31d1fd71d[0])
    return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(year, month, day), get_DateTimeKindUnspecified());
  if (__ref$315451b3c36a8c676be3bf2c = TryParseLocalizedLongDate(s, undefined, undefined, undefined), localizedYear = __ref$315451b3c36a8c676be3bf2c[1], localizedMonth = __ref$315451b3c36a8c676be3bf2c[2], localizedDay = __ref$315451b3c36a8c676be3bf2c[3], __ref$315451b3c36a8c676be3bf2c[0])
    return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(localizedYear, localizedMonth, localizedDay), get_DateTimeKindUnspecified());
  if (__ref$b63255e9cabac4e673832d5d = TryParseIsoDateTime(s, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined), year = __ref$b63255e9cabac4e673832d5d[1], month = __ref$b63255e9cabac4e673832d5d[2], day = __ref$b63255e9cabac4e673832d5d[3], hour = __ref$b63255e9cabac4e673832d5d[4], minute = __ref$b63255e9cabac4e673832d5d[5], second = __ref$b63255e9cabac4e673832d5d[6], millisecond = __ref$b63255e9cabac4e673832d5d[7], subMillisecondTicks = __ref$b63255e9cabac4e673832d5d[8], kind = __ref$b63255e9cabac4e673832d5d[9], offsetTicks = __ref$b63255e9cabac4e673832d5d[10], __ref$b63255e9cabac4e673832d5d[0]) {
    if (kind === get_DateTimeKindUnspecified())
      return new JDateTime("$ctor_9eb10cd821441a68", CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), subMillisecondTicks);
    let utcTicks = CreateUtcDateTimeTicks(year, month, day, hour, minute, second, millisecond) + subMillisecondTicks - offsetTicks;
    return CreateFromInstantTicks(utcTicks, get_DateTimeKindLocal());
  }
  let parsed = new Date(s);
  if (isNaN(parsed.getTime()))
    throw new Error(`FormatException: String '${input ?? ""}' was not recognized as a valid DateTime.`);
  let parsedSubMillisecondTicks = ExtractSubMillisecondTicks(s);
  if (HasUtcSuffix(s))
    return CreateFromInstantTicks(BigInt(parsed.getTime()) * get_TicksPerMillisecond() + parsedSubMillisecondTicks + get_UnixEpochTicks(), get_DateTimeKindLocal());
  if (HasExplicitOffset(s))
    return new JDateTime("$ctor_9eb10cd821441a68", new Date(parsed.getTime()), get_DateTimeKindLocal(), parsedSubMillisecondTicks);
  return new JDateTime("$ctor_9eb10cd821441a68", parsed, get_DateTimeKindUnspecified(), parsedSubMillisecondTicks);
}
/*jazor:clr-member static readonly System.DateTime.MinValue*/
export function _fad0c74e1c9df5bb() {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(1, 1, 1), get_DateTimeKindUnspecified());
}
/*jazor:clr-member static readonly System.DateTime.MaxValue*/
export function _eb38dc04224730ea() {
  return CreateDateTime_19b9b04d7a90c04d(CreateLocalDateTime(9999, 12, 31, 23, 59, 59, 999), get_DateTimeKindUnspecified(), BigInt("9999"));
}
/*jazor:clr-member static readonly System.DateTime.UnixEpoch*/
export function _878591efc9a51388() {
  return CreateFromTicks_8e90031c765a4910(get_UnixEpochTicks(), get_DateTimeKindUtc());
}
/*jazor:clr-member System.DateTime.DateTime()*/
export function _bfa8ee5dd46e2005() {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(1, 1, 1), get_DateTimeKindUnspecified());
}
/*jazor:clr-member System.DateTime.DateTime(long)*/
export function _1ba9ed95dd0eab48(ticks) {
  return CreateFromTicks_8e90031c765a4910(ticks, get_DateTimeKindUnspecified());
}
/*jazor:clr-member System.DateTime.DateTime(long, System.DateTimeKind)*/
export function _eda1c8bf8e1e617b(ticks, kind) {
  return CreateFromTicks_8e90031c765a4910(ticks, GetKind(kind));
}
/*jazor:clr-member System.DateTime.DateTime(System.DateOnly, System.TimeOnly)*/
export function _4fef4795bcbef97f(date, time) {
  let milliseconds = Number(time.ticks / get_TicksPerMillisecond());
  let subMillisecondTicks = time.ticks % get_TicksPerMillisecond();
  let hour = Math.floor(milliseconds / 3600000);
  let minute = Math.floor(milliseconds / 60000) % 60;
  let second = Math.floor(milliseconds / 1000) % 60;
  let millisecond = milliseconds % 1000;
  return CreateDateTime_19b9b04d7a90c04d(CreateLocalDateTime(date.year, date.month, date.day, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), subMillisecondTicks);
}
/*jazor:clr-member System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)*/
export function _85602323793168a5(date, time, kind) {
  let result = _4fef4795bcbef97f(date, time);
  return CreateDateTime_19b9b04d7a90c04d(result.date, GetKind(kind), result.subMillisecondTicks);
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int)*/
export function _4cb33a818161a3e1(year, month, day) {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(year, month, day), get_DateTimeKindUnspecified());
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)*/
export function _a515b8bb82ad96b7(year, month, day, calendar) {
  RequireGregorianCalendar(calendar);
  return _4cb33a818161a3e1(year, month, day);
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)*/
export function _bd2c430e6327a2cc(year, month, day, hour, minute, second, millisecond, calendar, kind) {
  RequireGregorianCalendar(calendar);
  return _c52eec5e681a0b8b(year, month, day, hour, minute, second, millisecond, kind);
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int)*/
export function _4903723bbf8a0a2f(year, month, day, hour, minute, second) {
  return new JDateTime("$ctor_5f7a68d76534e272", CreateLocalDateTime(year, month, day, hour, minute, second, 0));
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)*/
export function _f83be88cfb3fbce0(year, month, day, hour, minute, second, kind) {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDateTime(year, month, day, hour, minute, second, 0), GetKind(kind));
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)*/
export function _29bb943b21806bd9(year, month, day, hour, minute, second, calendar) {
  RequireGregorianCalendar(calendar);
  return _4903723bbf8a0a2f(year, month, day, hour, minute, second);
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int)*/
export function _5822b271bb635d64(year, month, day, hour, minute, second, millisecond) {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), get_DateTimeKindUnspecified());
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)*/
export function _c52eec5e681a0b8b(year, month, day, hour, minute, second, millisecond, kind) {
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), GetKind(kind));
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)*/
export function _8a4d2d51b716bb36(year, month, day, hour, minute, second, millisecond, calendar) {
  RequireGregorianCalendar(calendar);
  return _5822b271bb635d64(year, month, day, hour, minute, second, millisecond);
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, int)*/
export function _9117d26d23769ad1(year, month, day, hour, minute, second, millisecond, microsecond) {
  return CreateDateTime_19b9b04d7a90c04d(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), get_DateTimeKindUnspecified(), GetMicrosecondTicks(microsecond));
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)*/
export function _e84671346e2b9972(year, month, day, hour, minute, second, millisecond, microsecond, kind) {
  return CreateDateTime_19b9b04d7a90c04d(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), GetKind(kind), GetMicrosecondTicks(microsecond));
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)*/
export function _bd13792ce57e1964(year, month, day, hour, minute, second, millisecond, microsecond, calendar) {
  RequireGregorianCalendar(calendar);
  return _9117d26d23769ad1(year, month, day, hour, minute, second, millisecond, microsecond);
}
/*jazor:clr-member System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)*/
export function _cd0b8f2bce1e09ed(year, month, day, hour, minute, second, millisecond, microsecond, calendar, kind) {
  RequireGregorianCalendar(calendar);
  return _e84671346e2b9972(year, month, day, hour, minute, second, millisecond, microsecond, kind);
}
/*jazor:clr-member System.DateTime.Add(System.TimeSpan)*/
export function _34a77be7365c459f(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + value.ticks, instance.kind);
}
/*jazor:clr-member System.DateTime.AddDays(double)*/
export function _558a3f189d9149d7(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + CreateAddUnitTicks(value, get_TicksPerDay()), instance.kind);
}
/*jazor:clr-member System.DateTime.AddHours(double)*/
export function _101af978213c19c5(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + CreateAddUnitTicks(value, get_TicksPerHour()), instance.kind);
}
/*jazor:clr-member System.DateTime.AddMilliseconds(double)*/
export function _2b29e4c11fa12daa(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + CreateAddUnitTicks(value, get_TicksPerMillisecond()), instance.kind);
}
/*jazor:clr-member System.DateTime.AddMicroseconds(double)*/
export function _2b47368c73a3e1f2(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + CreateAddUnitTicks(value, get_TicksPerMicrosecond()), instance.kind);
}
/*jazor:clr-member System.DateTime.AddMinutes(double)*/
export function _8bdc25943cf2d39b(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + CreateAddUnitTicks(value, get_TicksPerMinute()), instance.kind);
}
/*jazor:clr-member System.DateTime.AddMonths(int)*/
export function _aae197b95f9024a4(instance, months) {
  return AddMonthsCore(instance, months);
}
/*jazor:clr-member System.DateTime.AddSeconds(double)*/
export function _57045f93edac1460(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + CreateAddUnitTicks(value, get_TicksPerSecond()), instance.kind);
}
/*jazor:clr-member System.DateTime.AddTicks(long)*/
export function _d2e74845b174a889(instance, value) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(instance) + value, instance.kind);
}
/*jazor:clr-member System.DateTime.AddYears(int)*/
export function _3353d31b02f2bed8(instance, value) {
  EnsureWholeNumber(value, "ArgumentOutOfRangeException: Years value must be a whole number.");
  return AddMonthsCore(instance, value * 12);
}
/*jazor:clr-member static System.DateTime.Compare(System.DateTime, System.DateTime)*/
export function _0edfd00dcc8d70d0(t1, t2) {
  let ticks1 = GetTicks_a125094c105bae75(t1);
  let ticks2 = GetTicks_a125094c105bae75(t2);
  if (ticks1 < ticks2)
    return -1;
  if (ticks1 > ticks2)
    return 1;
  return 0;
}
/*jazor:clr-member System.DateTime.CompareTo(object)*/
export function _f7b2337bfa9864d9(instance, value) {
  if (value === null)
    return 1;
  let other = value instanceof JDateTime ? value : null;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type DateTime.");
  return _40c6426fdc505e97(instance, other);
}
/*jazor:clr-member System.DateTime.CompareTo(System.DateTime)*/
export function _40c6426fdc505e97(instance, value) {
  let ticks = GetTicks_a125094c105bae75(instance);
  let otherTicks = GetTicks_a125094c105bae75(value);
  if (ticks < otherTicks)
    return -1;
  if (ticks > otherTicks)
    return 1;
  return 0;
}
/*jazor:clr-member static System.DateTime.DaysInMonth(int, int)*/
export function _38ef7423971afb7f(year, month) {
  return GetDaysInMonth(year, month);
}
/*jazor:clr-member override System.DateTime.Equals(object)*/
export function _f6903c1af8944917(instance, value) {
  let other = value instanceof JDateTime ? value : null;
  return other !== null && GetTicks_a125094c105bae75(instance) === GetTicks_a125094c105bae75(other);
}
/*jazor:clr-member System.DateTime.Equals(System.DateTime)*/
export function _c29ca32a998c517c(instance, value) {
  return GetTicks_a125094c105bae75(instance) === GetTicks_a125094c105bae75(value);
}
/*jazor:clr-member static System.DateTime.Equals(System.DateTime, System.DateTime)*/
export function _4937ff8bec81ddea(t1, t2) {
  return GetTicks_a125094c105bae75(t1) === GetTicks_a125094c105bae75(t2);
}
/*jazor:clr-member static System.DateTime.FromBinary(long)*/
export function _f437fad61f0046c7(dateData) {
  let unsignedData = dateData < get_ZeroTicks() ? dateData + get_BinaryUnsignedOverflow() : dateData;
  let kindBits = unsignedData & get_BinaryKindMask();
  let ticks = unsignedData & get_BinaryTicksMask();
  if (kindBits === get_BinaryLocalMask() || kindBits === get_BinaryKindMask())
    return CreateFromInstantTicks(ticks, get_DateTimeKindLocal());
  if (kindBits === get_BinaryKindShift())
    return CreateFromTicks_8e90031c765a4910(ticks, get_DateTimeKindUtc());
  return CreateFromTicks_8e90031c765a4910(ticks, get_DateTimeKindUnspecified());
}
/*jazor:clr-member static System.DateTime.FromFileTime(long)*/
export function _df025c273bde0e50(fileTime) {
  if (fileTime < get_ZeroTicks())
    throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");
  return CreateFromInstantTicks(fileTime - get_FileTimeUnixEpochTicks() + get_UnixEpochTicks(), get_DateTimeKindLocal());
}
/*jazor:clr-member static System.DateTime.FromFileTimeUtc(long)*/
export function _93886aebedb72920(fileTime) {
  if (fileTime < get_ZeroTicks())
    throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");
  return CreateFromTicks_8e90031c765a4910(fileTime - get_FileTimeUnixEpochTicks() + get_UnixEpochTicks(), get_DateTimeKindUtc());
}
/*jazor:clr-member static System.DateTime.FromOADate(double)*/
export function _12520a637fb85a70(d) {
  return CreateFromTicks_8e90031c765a4910(CreateRoundedTicksFromDouble((d - get_OADateUnixOffsetDays()) * get_MillisecondsPerDay()) * get_TicksPerMillisecond() + get_UnixEpochTicks(), get_DateTimeKindUnspecified());
}
/*jazor:clr-member System.DateTime.IsDaylightSavingTime()*/
export function _d3b1cc7e750c6bc3(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return false;
  let year = instance.date.getFullYear();
  let januaryOffset = CreateLocalDate(year, 1, 1).getTimezoneOffset();
  let julyOffset = CreateLocalDate(year, 7, 1).getTimezoneOffset();
  let standardOffset = januaryOffset > julyOffset ? januaryOffset : julyOffset;
  return instance.date.getTimezoneOffset() < standardOffset;
}
/*jazor:clr-member static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)*/
export function _a99826a92073614e(value, kind) {
  return CreateDateTime_19b9b04d7a90c04d(value.date, GetKind(kind), value.subMillisecondTicks);
}
/*jazor:clr-member System.DateTime.ToBinary()*/
export function _9cea54115c704cf7(instance) {
  if (instance.kind === get_DateTimeKindLocal())
    return GetInstantTicks(instance) + get_BinaryLocalMask();
  return GetTicks_a125094c105bae75(instance) + BigInt(instance.kind) * get_BinaryKindShift();
}
/*jazor:clr-member System.DateTime.Date.get*/
export function _d77d20d9d04e2b6b(instance) {
  return CreateDateTime_6f4bad8e5307c00a(CreateLocalDate(instance.date.getFullYear(), instance.date.getMonth() + 1, instance.date.getDate()), instance.kind);
}
/*jazor:clr-member System.DateTime.Day.get*/
export function _3b9ecf5fd3c301db(instance) {
  return instance.date.getDate();
}
/*jazor:clr-member System.DateTime.DayOfWeek.get*/
export function _6070f1709c491634(instance) {
  return instance.date.getDay();
}
/*jazor:clr-member System.DateTime.DayOfYear.get*/
export function _4f6ca20bf1aaa2d3(instance) {
  let year = instance.date.getFullYear();
  let start = Date.UTC(year, 0, 0);
  let current = Date.UTC(year, instance.date.getMonth(), instance.date.getDate());
  return Math.floor((current - start) / 86400000);
}
/*jazor:clr-member override System.DateTime.GetHashCode()*/
export function _d3529b55e30e2a12(instance) {
  return GetInt64HashCode(GetTicks_a125094c105bae75(instance));
}
/*jazor:clr-member System.DateTime.Hour.get*/
export function _f263cff61e6628a9(instance) {
  return instance.date.getHours();
}
/*jazor:clr-member System.DateTime.Kind.get*/
export function _551add245db0b701(instance) {
  return instance.kind;
}
/*jazor:clr-member System.DateTime.Millisecond.get*/
export function _742a8bcf918b97e6(instance) {
  return instance.date.getMilliseconds();
}
/*jazor:clr-member System.DateTime.Microsecond.get*/
export function _34d05014c270366f(instance) {
  return Number(instance.subMillisecondTicks / get_TicksPerMicrosecond() % BigInt(1000));
}
/*jazor:clr-member System.DateTime.Nanosecond.get*/
export function _46e11fe2eb2ee869(instance) {
  return Number(instance.subMillisecondTicks % get_TicksPerMicrosecond() * BigInt(100));
}
/*jazor:clr-member System.DateTime.Minute.get*/
export function _f4ca5de4f63aa097(instance) {
  return instance.date.getMinutes();
}
/*jazor:clr-member System.DateTime.Month.get*/
export function _a8a6b6e36a0ea736(instance) {
  return instance.date.getMonth() + 1;
}
/*jazor:clr-member static System.DateTime.Now.get*/
export function _ee9dd166a34a2fa5() {
  return new JDateTime("$ctor_31a0f1908d992f04", new Date, get_DateTimeKindLocal());
}
/*jazor:clr-member System.DateTime.Second.get*/
export function _10a94eacb3b7fd2d(instance) {
  return instance.date.getSeconds();
}
/*jazor:clr-member System.DateTime.Ticks.get*/
export function _bcde32e170f49354(instance) {
  return GetTicks_a125094c105bae75(instance);
}
/*jazor:clr-member System.DateTime.TimeOfDay.get*/
export function _2efdc237be2f31aa(instance) {
  let ms = ((instance.date.getHours() * 60 + instance.date.getMinutes()) * 60 + instance.date.getSeconds()) * 1000 + instance.date.getMilliseconds();
  return new JTimeSpan(BigInt(ms) * get_TicksPerMillisecond() + instance.subMillisecondTicks);
}
/*jazor:clr-member static System.DateTime.Today.get*/
export function _4b250155b7c688bb() {
  let now = new Date;
  return new JDateTime("$ctor_31a0f1908d992f04", CreateLocalDate(now.getFullYear(), now.getMonth() + 1, now.getDate()), get_DateTimeKindLocal());
}
/*jazor:clr-member System.DateTime.Year.get*/
export function _9d56b09432f81c05(instance) {
  return instance.date.getFullYear();
}
/*jazor:clr-member static System.DateTime.IsLeapYear(int)*/
export function _4a9da83e9cb28c1a(year) {
  EnsureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number between 1 and 9999.");
  if (year < 1 || year > 9999)
    throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
  return year % 4 === 0 && year % 100 !== 0 || year % 400 === 0;
}
/*jazor:clr-member static System.DateTime.Parse(string)*/
export function _a8a015c2d2bff2f6(s) {
  return ParseCore(s);
}
/*jazor:clr-member static System.DateTime.Parse(string, System.IFormatProvider)*/
export function _e0128ef45cc8584e(s, provider) {
  return ParseCore(s);
}
/*jazor:clr-member static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)*/
export function _7372e5e0d8ba24a6(s, provider, styles) {
  return ApplyDateTimeStyles(ParseCore(s), s, styles);
}
/*jazor:clr-member static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)*/
export function _2c85f5b20ae7559e(s, provider, styles) {
  return ApplyDateTimeStyles(ParseCore(s), s, styles);
}
/*jazor:clr-member System.DateTime.Subtract(System.DateTime)*/
export function _4f5d235cac779f38(instance, value) {
  return _85b6d162b092ce0e(instance, value);
}
/*jazor:clr-member System.DateTime.Subtract(System.TimeSpan)*/
export function _20a406afebff2025(instance, value) {
  return _8d9ea66839ce392a(instance, value);
}
/*jazor:clr-member System.DateTime.ToOADate()*/
export function _fb61bb2ccf4b10b6(instance) {
  return Number(GetTicks_a125094c105bae75(instance) - get_UnixEpochTicks()) / 10000 / get_MillisecondsPerDay() + get_OADateUnixOffsetDays();
}
/*jazor:clr-member System.DateTime.ToFileTime()*/
export function _37ee48ca629793fa(instance) {
  return GetInstantTicks(instance) - get_UnixEpochTicks() + get_FileTimeUnixEpochTicks();
}
/*jazor:clr-member System.DateTime.ToFileTimeUtc()*/
export function _c02c49ea68661175(instance) {
  return _37ee48ca629793fa(instance);
}
/*jazor:clr-member System.DateTime.ToLocalTime()*/
export function _db842725d5fd1ca0(instance) {
  if (instance.kind === get_DateTimeKindLocal())
    return CreateDateTime_19b9b04d7a90c04d(instance.date, get_DateTimeKindLocal(), instance.subMillisecondTicks);
  let instantTicks = instance.kind === get_DateTimeKindUnspecified() ? GetTicks_a125094c105bae75(instance) : GetInstantTicks(instance);
  return CreateFromInstantTicks(instantTicks, get_DateTimeKindLocal());
}
/*jazor:clr-member System.DateTime.ToLongDateString()*/
export function _6e78dc03eecdd423(instance) {
  return FormatDateTime(instance, "D", null);
}
/*jazor:clr-member System.DateTime.ToLongTimeString()*/
export function _ab161bb1563732af(instance) {
  return FormatDateTime(instance, "T", null);
}
/*jazor:clr-member System.DateTime.ToShortDateString()*/
export function _6a67d54f5c865e5e(instance) {
  return FormatDateTime(instance, "d", null);
}
/*jazor:clr-member System.DateTime.ToShortTimeString()*/
export function _af2d02ec0c0a300d(instance) {
  return FormatDateTime(instance, "t", null);
}
/*jazor:clr-member override System.DateTime.ToString()*/
export function _6659b3b5d1f081dd(instance) {
  return FormatDateTime(instance, null, null);
}
/*jazor:clr-member System.DateTime.ToString(string)*/
export function _3ee3e9478fe9a1fb(instance, format) {
  return FormatDateTime(instance, format, null);
}
/*jazor:clr-member System.DateTime.ToString(System.IFormatProvider)*/
export function _606066f0ee1488c6(instance, provider) {
  return FormatDateTime(instance, null, provider);
}
/*jazor:clr-member System.DateTime.ToString(string, System.IFormatProvider)*/
export function _85393faf5839b9ef(instance, format, provider) {
  return FormatDateTime(instance, format, provider);
}
/*jazor:clr-member System.DateTime.ToUniversalTime()*/
export function _b62871088df3ca8f(instance) {
  if (instance.kind === get_DateTimeKindUtc())
    return CreateDateTime_19b9b04d7a90c04d(instance.date, get_DateTimeKindUtc(), instance.subMillisecondTicks);
  return CreateFromInstantTicks(GetInstantTicks(instance), get_DateTimeKindUtc());
}
/*jazor:clr-member static System.DateTime.TryParse(string, out System.DateTime)*/
export function _fa25ca318f086bb6(s, result) {
  if (s === null || s.length === 0)
    return [false, CreateDefaultDateTime()];
  try {
    return [true, ParseCore(s)];
  } catch {
    return [false, CreateDefaultDateTime()];
  }
}
/*jazor:clr-member static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)*/
export function _8658c3be6edb9d2c(s, result) {
  return _fa25ca318f086bb6(s, result);
}
/*jazor:clr-member static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)*/
export function _34043b1eb3a8183a(s, provider, styles, result) {
  ValidateDateTimeStyles(GetDateTimeStylesValue(styles));
  if (s === null || s.length === 0)
    return [false, CreateDefaultDateTime()];
  try {
    return [true, ApplyDateTimeStyles(ParseCore(s), s, styles)];
  } catch {
    return [false, CreateDefaultDateTime()];
  }
}
/*jazor:clr-member static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)*/
export function _6e8546b461b48646(s, provider, styles, result) {
  return _34043b1eb3a8183a(s, provider, styles, result);
}
/*jazor:clr-member static System.DateTime.operator +(System.DateTime, System.TimeSpan)*/
export function _d48b23d7c5f7c2aa(d, t) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(d) + t.ticks, d.kind);
}
/*jazor:clr-member static System.DateTime.operator -(System.DateTime, System.TimeSpan)*/
export function _8d9ea66839ce392a(d, t) {
  return CreateFromTicks_8e90031c765a4910(GetTicks_a125094c105bae75(d) - t.ticks, d.kind);
}
/*jazor:clr-member static System.DateTime.operator -(System.DateTime, System.DateTime)*/
export function _85b6d162b092ce0e(d1, d2) {
  return new JTimeSpan(GetTicks_a125094c105bae75(d1) - GetTicks_a125094c105bae75(d2));
}
/*jazor:clr-member static System.DateTime.operator ==(System.DateTime, System.DateTime)*/
export function _37d87f65292f7083(d1, d2) {
  return GetTicks_a125094c105bae75(d1) === GetTicks_a125094c105bae75(d2);
}
/*jazor:clr-member static System.DateTime.operator !=(System.DateTime, System.DateTime)*/
export function _89406f797d33e566(d1, d2) {
  return GetTicks_a125094c105bae75(d1) !== GetTicks_a125094c105bae75(d2);
}
/*jazor:clr-member static System.DateTime.operator <(System.DateTime, System.DateTime)*/
export function _5a97e2aec50193b3(t1, t2) {
  return GetTicks_a125094c105bae75(t1) < GetTicks_a125094c105bae75(t2);
}
/*jazor:clr-member static System.DateTime.operator <=(System.DateTime, System.DateTime)*/
export function _a8b15168323b118c(t1, t2) {
  return GetTicks_a125094c105bae75(t1) <= GetTicks_a125094c105bae75(t2);
}
/*jazor:clr-member static System.DateTime.operator >(System.DateTime, System.DateTime)*/
export function _e98b0598f4980bcc(t1, t2) {
  return GetTicks_a125094c105bae75(t1) > GetTicks_a125094c105bae75(t2);
}
/*jazor:clr-member static System.DateTime.operator >=(System.DateTime, System.DateTime)*/
export function _91697ebd6031bb97(t1, t2) {
  return GetTicks_a125094c105bae75(t1) >= GetTicks_a125094c105bae75(t2);
}
/*jazor:clr-member System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)*/
export function _bcf4183bef96ea21(instance, date, time) {
  return [new JDateOnly(instance.date.getFullYear(), instance.date.getMonth() + 1, instance.date.getDate()), _a305982aa6859677(instance)];
}
/*jazor:clr-member System.DateTime.Deconstruct(out int, out int, out int)*/
export function _5f721827cf6b8105(instance, year, month, day) {
  return [instance.date.getFullYear(), instance.date.getMonth() + 1, instance.date.getDate()];
}
/*jazor:clr-member static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)*/
export function _6c36c46db30aacc1(s, provider, result) {
  return _fa25ca318f086bb6(s, result);
}
/*jazor:clr-member static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _41dcf008ea7cf6d9(s, provider) {
  return _a8a015c2d2bff2f6(s);
}
/*jazor:clr-member static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)*/
export function _63fd53f09ba16132(s, provider, result) {
  return _fa25ca318f086bb6(s, result);
}
/*jazor:clr-member static System.DateTime.UtcNow.get*/
export function _d4c39bdf47f391cf() {
  return CreateUtcNow();
}
