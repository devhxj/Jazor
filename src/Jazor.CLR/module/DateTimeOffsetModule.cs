namespace Jazor.CLR;

[ECMAScriptModule("System/DateTimeOffsetModule.js")]
[Jazor(Op.Alias, "System.DateTimeOffset","Object")]
public static class DateTimeOffsetModule
{
	private static BigInt ZeroTicks => BigInt.Zero;
	private static BigInt UnixEpochTicks => BigIntFn("621355968000000000");
	private static BigInt FileTimeUnixEpochTicks => BigIntFn("116444736000000000");
	private static BigInt TicksPerMicrosecond => BigIntFn("10");
	private static BigInt TicksPerMillisecond => BigIntFn("10000");
	private static BigInt TicksPerSecond => BigIntFn("10000000");
	private static BigInt TicksPerMinute => BigIntFn("600000000");
	private static BigInt TicksPerHour => BigIntFn("36000000000");
	private static BigInt TicksPerDay => BigIntFn("864000000000");
	private static BigInt OffsetMinuteTicks => BigIntFn("600000000");
	private static BigInt MaxOffsetTicks => BigIntFn("504000000000");
	private static BigInt MaxDateTimeTicks => BigIntFn("3155378975999999999");
	private static BigInt MinUnixTimeMilliseconds => BigIntFn("-62135596800000");
	private static BigInt MaxUnixTimeMilliseconds => BigIntFn("253402300799999");
	private static BigInt MinUnixTimeSeconds => BigIntFn("-62135596800");
	private static BigInt MaxUnixTimeSeconds => BigIntFn("253402300799");
	private static Number DateTimeKindUtc => 1;
	private static Number DateTimeKindLocal => 2;
	private static Number MinValueMilliseconds => -62135596800000d;
	private static Number DateTimeStylesNoCurrentDateDefault => 8;
	private static Number DateTimeStylesAdjustToUniversal => 16;
	private static Number DateTimeStylesAssumeLocal => 32;
	private static Number DateTimeStylesAssumeUniversal => 64;

	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
			throw new Error(message);
	}

	private static RuntimeModule.JDateTimeOffset CreateDateTimeOffset(Date utcDateTime, BigInt offsetTicks)
		=> CreateDateTimeOffset(utcDateTime, offsetTicks, ZeroTicks);

	private static RuntimeModule.JDateTimeOffset CreateDateTimeOffset(Date utcDateTime, BigInt offsetTicks, BigInt utcSubMillisecondTicks)
	{
		var utcTicks = BigIntFn(utcDateTime.GetTime()) * TicksPerMillisecond + utcSubMillisecondTicks + UnixEpochTicks;
		ValidateDateTimeOffsetRange(utcTicks, offsetTicks);
		return new(utcDateTime, offsetTicks, utcSubMillisecondTicks);
	}

	private static RuntimeModule.JDateTimeOffset CreateDefaultDateTimeOffset()
		=> CreateDateTimeOffset(new Date(MinValueMilliseconds), ZeroTicks);

	private static void ValidateDateTimeOffsetRange(BigInt utcTicks, BigInt offsetTicks)
	{
		ValidateOffsetTicks(offsetTicks);

		var ticks = utcTicks + offsetTicks;
		if (ticks < ZeroTicks || ticks > MaxDateTimeTicks)
			throw new Error("ArgumentOutOfRangeException: The UTC time and offset must produce a DateTimeOffset within range.");
	}

	private static void ValidateOffsetTicks(BigInt offsetTicks)
	{
		if (offsetTicks % OffsetMinuteTicks != BigInt.Zero)
			throw new Error("ArgumentException: Offset must be specified in whole minutes.");
		if (offsetTicks < -MaxOffsetTicks || offsetTicks > MaxOffsetTicks)
			throw new Error("ArgumentOutOfRangeException: Offset must be within plus or minus 14 hours.");
	}

	private static void ValidateMicrosecond(Number microsecond)
	{
		if (Math.FloorFn(microsecond) != microsecond || microsecond < 0 || microsecond > 999)
			throw new Error("ArgumentOutOfRangeException: Microsecond must be between 0 and 999.");
	}

	private static BigInt CreateLocalTicks(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
	{
		var utc = RuntimeModule.CreateUtcDate(year, month, day);
		utc.SetUTCHours(hour, minute, second, millisecond);
		if (utc.GetUTCFullYear() != year
			|| utc.GetUTCMonth() + 1 != month
			|| utc.GetUTCDate() != day
			|| utc.GetUTCHours() != hour
			|| utc.GetUTCMinutes() != minute
			|| utc.GetUTCSeconds() != second
			|| utc.GetUTCMilliseconds() != millisecond)
			throw new Error("ArgumentOutOfRangeException: The supplied date or time component is out of range.");

		return BigIntFn(utc.GetTime()) * TicksPerMillisecond + UnixEpochTicks;
	}

	private static BigInt GetDateTimeInstantTicks(RuntimeModule.JDateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKindUtc)
		{
			var date = dateTime.Date;
			return BigIntFn(Date.UTC(
				date.GetFullYear(),
				date.GetMonth(),
				date.GetDate(),
				date.GetHours(),
				date.GetMinutes(),
				date.GetSeconds(),
				date.GetMilliseconds())) * TicksPerMillisecond + dateTime.SubMillisecondTicks + UnixEpochTicks;
		}

		return BigIntFn(dateTime.Date.GetTime()) * TicksPerMillisecond + dateTime.SubMillisecondTicks + UnixEpochTicks;
	}

	private static BigInt GetDateTimeTicks(RuntimeModule.JDateTime dateTime)
	{
		var date = dateTime.Date;
		return BigIntFn(Date.UTC(
			date.GetFullYear(),
			date.GetMonth(),
			date.GetDate(),
			date.GetHours(),
			date.GetMinutes(),
			date.GetSeconds(),
			date.GetMilliseconds())) * TicksPerMillisecond + dateTime.SubMillisecondTicks + UnixEpochTicks;
	}

	private static BigInt GetUtcTicks(RuntimeModule.JDateTimeOffset instance)
		=> BigIntFn(instance.UtcDateTime.GetTime()) * TicksPerMillisecond + instance.UtcSubMillisecondTicks + UnixEpochTicks;

	private static BigInt GetTicks(RuntimeModule.JDateTimeOffset instance)
		=> GetUtcTicks(instance) + instance.OffsetTicks;

	private static RuntimeModule.JDateTimeOffset CreateFromUtcTicks(BigInt utcTicks, BigInt offsetTicks)
	{
		ValidateDateTimeOffsetRange(utcTicks, offsetTicks);

		var ticksSinceUnixEpoch = utcTicks - UnixEpochTicks;
		var milliseconds = ticksSinceUnixEpoch / TicksPerMillisecond;
		var utcSubMillisecondTicks = ticksSinceUnixEpoch % TicksPerMillisecond;
		if (utcSubMillisecondTicks < ZeroTicks)
		{
			milliseconds -= BigIntFn(1);
			utcSubMillisecondTicks += TicksPerMillisecond;
		}

		return CreateDateTimeOffset(new Date(NumberFn(milliseconds)), offsetTicks, utcSubMillisecondTicks);
	}

	private static BigInt NormalizeSubMillisecondTicks(BigInt ticks)
	{
		var remainder = (ticks - UnixEpochTicks) % TicksPerMillisecond;
		return remainder < ZeroTicks ? remainder + TicksPerMillisecond : remainder;
	}

	private static void ValidateOffset(RuntimeModule.JTimeSpan offset)
		=> ValidateOffsetTicks(offset.Ticks);

	private static RuntimeModule.JDateTimeOffset AddMonthsCore(RuntimeModule.JDateTimeOffset instance, Number months)
	{
		EnsureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");

		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		var year = local.GetUTCFullYear();
		var monthIndex = (year - 1) * 12 + local.GetUTCMonth() + months;
		var newYear = Math.FloorFn(monthIndex / 12) + 1;
		var newMonthIndex = monthIndex % 12;
		if (newMonthIndex < 0)
			newMonthIndex += 12;

		var newMonth = newMonthIndex + 1;
		var day = local.GetUTCDate();
		var daysInMonth = RuntimeModule.GetDaysInMonth(newYear, newMonth);
		var newDay = day > daysInMonth ? daysInMonth : day;
		var localTicks = CreateLocalTicks(
			newYear,
			newMonth,
			newDay,
			local.GetUTCHours(),
			local.GetUTCMinutes(),
			local.GetUTCSeconds(),
			local.GetUTCMilliseconds()) + instance.UtcSubMillisecondTicks;
		return CreateFromUtcTicks(localTicks - instance.OffsetTicks, instance.OffsetTicks);
	}

	private static RuntimeModule.JDateTimeOffset CreateWithLocalOffset(Date utcDateTime)
	{
		var offsetTicks = BigIntFn(-utcDateTime.GetTimezoneOffset()) * OffsetMinuteTicks;
		return CreateDateTimeOffset(utcDateTime, offsetTicks);
	}

	private static RuntimeModule.JDateTimeOffset CreateWithLocalOffset(BigInt utcTicks)
	{
		var utcDateTime = CreateFromUtcTicks(utcTicks, ZeroTicks).UtcDateTime;
		var offsetTicks = BigIntFn(-utcDateTime.GetTimezoneOffset()) * OffsetMinuteTicks;
		return CreateFromUtcTicks(utcTicks, offsetTicks);
	}

	private static Date GetOffsetLocalDate(RuntimeModule.JDateTimeOffset instance)
		=> new(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);

	private static string GetProviderLocale(object? provider)
	{
		if (provider is string locale)
			return locale;
		if (provider is Intl.NumberFormat numberFormat)
			return numberFormat.ResolvedOptions().Locale;

		return new Intl.DateTimeFormat().ResolvedOptions().Locale;
	}

	private static string JoinFormatParts(Array<Intl.FormatPart> parts)
	{
		var text = "";
		for (var i = 0; i < parts.Length; i++)
			text += parts[i]!.Value;

		return text;
	}

	private static string GetInvariantMonthName(Number month)
	{
		switch (month | 0)
		{
			case 1: return "January";
			case 2: return "February";
			case 3: return "March";
			case 4: return "April";
			case 5: return "May";
			case 6: return "June";
			case 7: return "July";
			case 8: return "August";
			case 9: return "September";
			case 10: return "October";
			case 11: return "November";
			case 12: return "December";
			default: throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
		}
	}

	private static string GetInvariantAbbreviatedMonthName(Number month)
	{
		switch (month | 0)
		{
			case 1: return "Jan";
			case 2: return "Feb";
			case 3: return "Mar";
			case 4: return "Apr";
			case 5: return "May";
			case 6: return "Jun";
			case 7: return "Jul";
			case 8: return "Aug";
			case 9: return "Sep";
			case 10: return "Oct";
			case 11: return "Nov";
			case 12: return "Dec";
			default: throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
		}
	}

	private static string GetInvariantDayName(Number dayOfWeek)
	{
		switch (dayOfWeek | 0)
		{
			case 0: return "Sunday";
			case 1: return "Monday";
			case 2: return "Tuesday";
			case 3: return "Wednesday";
			case 4: return "Thursday";
			case 5: return "Friday";
			case 6: return "Saturday";
			default: throw new Error("ArgumentOutOfRangeException: DayOfWeek must be between 0 and 6.");
		}
	}

	private static string GetInvariantAbbreviatedDayName(Number dayOfWeek)
	{
		switch (dayOfWeek | 0)
		{
			case 0: return "Sun";
			case 1: return "Mon";
			case 2: return "Tue";
			case 3: return "Wed";
			case 4: return "Thu";
			case 5: return "Fri";
			case 6: return "Sat";
			default: throw new Error("ArgumentOutOfRangeException: DayOfWeek must be between 0 and 6.");
		}
	}

	private static bool IsAsciiLetter(char value)
		=> (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');

	private static string GetLocalizedMonthName(string locale, Number month, bool abbreviated)
	{
		if (locale.Length == 0)
			return abbreviated ? GetInvariantAbbreviatedMonthName(month) : GetInvariantMonthName(month);

		return JoinFormatParts(new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Month: abbreviated ? Intl.LongShortNarrow.Short : Intl.LongShortNarrow.Long,
				TimeZone: "UTC")).FormatToParts(new Date(Date.UTC(2000, month - 1, 1))));
	}

	private static string GetLocalizedDayName(string locale, Number dayOfWeek, bool abbreviated)
	{
		if (locale.Length == 0)
			return abbreviated ? GetInvariantAbbreviatedDayName(dayOfWeek) : GetInvariantDayName(dayOfWeek);

		return JoinFormatParts(new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Weekday: abbreviated ? Intl.LongShortNarrow.Short : Intl.LongShortNarrow.Long,
				TimeZone: "UTC")).FormatToParts(new Date(Date.UTC(2024, 0, 7 + dayOfWeek))));
	}

	private static string GetDateSeparator(string locale)
	{
		if (locale.Length == 0)
			return "/";

		var parts = new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.NumericTwoDigit.TwoDigit,
				Day: Intl.NumericTwoDigit.TwoDigit,
				TimeZone: "UTC")).FormatToParts(new Date(Date.UTC(2000, 0, 2)));
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i]!;
			if (part.Type == "literal" && part.Value.Length != 0)
				return part.Value;
		}

		return "/";
	}

	private static string GetTimeSeparator(string locale)
	{
		if (locale.Length == 0)
			return ":";

		var parts = new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Hour: Intl.NumericTwoDigit.TwoDigit,
				Minute: Intl.NumericTwoDigit.TwoDigit,
				Hour12: false,
				TimeZone: "UTC")).FormatToParts(new Date(Date.UTC(2000, 0, 2, 3, 4, 5)));
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i]!;
			if (part.Type == "literal" && part.Value.Length != 0)
				return part.Value;
		}

		return ":";
	}

	private static string GetLocalizedDayPeriod(Date date, string locale)
	{
		if (locale.Length == 0)
			return date.GetUTCHours() < 12 ? "AM" : "PM";

		var parts = new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Hour: Intl.NumericTwoDigit.Numeric,
				Hour12: true,
				TimeZone: "UTC")).FormatToParts(date);
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i]!;
			if (part.Type == "dayPeriod")
				return part.Value;
		}

		return date.GetUTCHours() < 12 ? "AM" : "PM";
	}

	private static string FormatOffsetTicks(BigInt offsetTicks, int count)
	{
		var negative = offsetTicks < BigInt.Zero;
		var absolute = negative ? -offsetTicks : offsetTicks;
		var totalMinutes = absolute / OffsetMinuteTicks;
		var hours = NumberFn(totalMinutes / BigIntFn(60));
		var minutes = NumberFn(totalMinutes % BigIntFn(60));
		var sign = negative ? "-" : "+";

		if (count <= 1)
			return sign + hours;
		if (count == 2)
			return sign + RuntimeModule.Pad2(hours);

		return sign + RuntimeModule.Pad2(hours) + ":" + RuntimeModule.Pad2(minutes);
	}

	private static string FormatInvariantGeneralDateTimeOffset(RuntimeModule.JDateTimeOffset instance, bool includeSeconds, bool includeOffset)
	{
		var local = GetOffsetLocalDate(instance);
		var text = RuntimeModule.Pad2(local.GetUTCMonth() + 1)
			+ "/"
			+ RuntimeModule.Pad2(local.GetUTCDate())
			+ "/"
			+ RuntimeModule.PadLeft(local.GetUTCFullYear().ToString()!, 4)
			+ " "
			+ RuntimeModule.Pad2(local.GetUTCHours())
			+ ":"
			+ RuntimeModule.Pad2(local.GetUTCMinutes());
		if (includeSeconds)
			text += ":" + RuntimeModule.Pad2(local.GetUTCSeconds());
		if (includeOffset)
			text += " " + FormatOffsetTicks(instance.OffsetTicks, 3);

		return text;
	}

	private static string FormatInvariantShortDate(RuntimeModule.JDateTimeOffset instance)
	{
		var local = GetOffsetLocalDate(instance);
		return RuntimeModule.Pad2(local.GetUTCMonth() + 1)
			+ "/"
			+ RuntimeModule.Pad2(local.GetUTCDate())
			+ "/"
			+ RuntimeModule.PadLeft(local.GetUTCFullYear().ToString()!, 4);
	}

	private static string FormatInvariantLongDate(RuntimeModule.JDateTimeOffset instance)
	{
		var local = GetOffsetLocalDate(instance);
		return GetInvariantDayName(local.GetUTCDay())
			+ ", "
			+ RuntimeModule.Pad2(local.GetUTCDate())
			+ " "
			+ GetInvariantMonthName(local.GetUTCMonth() + 1)
			+ " "
			+ RuntimeModule.PadLeft(local.GetUTCFullYear().ToString()!, 4);
	}

	private static string FormatInvariantTime(RuntimeModule.JDateTimeOffset instance, bool includeSeconds)
	{
		var local = GetOffsetLocalDate(instance);
		var text = RuntimeModule.Pad2(local.GetUTCHours())
			+ ":"
			+ RuntimeModule.Pad2(local.GetUTCMinutes());
		if (includeSeconds)
			text += ":" + RuntimeModule.Pad2(local.GetUTCSeconds());

		return text;
	}

	private static string FormatMonthDay(RuntimeModule.JDateTimeOffset instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
		{
			var local = GetOffsetLocalDate(instance);
			return GetInvariantMonthName(local.GetUTCMonth() + 1) + " " + RuntimeModule.Pad2(local.GetUTCDate());
		}

		return FormatOffsetLocaleDateTime(
			GetOffsetLocalDate(instance),
			locale,
			new Intl.DateTimeFormatOptions(
				Month: Intl.LongShortNarrow.Long,
				Day: Intl.NumericTwoDigit.TwoDigit));
	}

	private static string FormatYearMonth(RuntimeModule.JDateTimeOffset instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
		{
			var local = GetOffsetLocalDate(instance);
			return RuntimeModule.PadLeft(local.GetUTCFullYear().ToString()!, 4) + " " + GetInvariantMonthName(local.GetUTCMonth() + 1);
		}

		return FormatOffsetLocaleDateTime(
			GetOffsetLocalDate(instance),
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.LongShortNarrow.Long));
	}

	private static string FormatFullDateTime(RuntimeModule.JDateTimeOffset instance, bool includeSeconds, object? provider)
		=> FormatLongDate(instance, provider) + " " + FormatTime(instance, includeSeconds, provider);

	private static string FormatRfc1123DateTimeOffset(RuntimeModule.JDateTimeOffset instance)
	{
		var utc = instance.UtcDateTime;
		return GetInvariantAbbreviatedDayName(utc.GetUTCDay())
			+ ", "
			+ RuntimeModule.Pad2(utc.GetUTCDate())
			+ " "
			+ GetInvariantAbbreviatedMonthName(utc.GetUTCMonth() + 1)
			+ " "
			+ RuntimeModule.PadLeft(utc.GetUTCFullYear().ToString()!, 4)
			+ " "
			+ RuntimeModule.Pad2(utc.GetUTCHours())
			+ ":"
			+ RuntimeModule.Pad2(utc.GetUTCMinutes())
			+ ":"
			+ RuntimeModule.Pad2(utc.GetUTCSeconds())
			+ " GMT";
	}

	private static string FormatLocaleDateTime(Date date, string locale, Intl.DateTimeFormatOptions options)
		=> JoinFormatParts(new Intl.DateTimeFormat(locale, options).FormatToParts(date));

	private static string FormatOffsetLocaleDateTime(Date date, string locale, Intl.DateTimeFormatOptions options)
		// GetOffsetLocalDate 已经把 offset 对应的墙上时间编码进 UTC 字段了。
		// 这里必须固定用 UTC 读取这些字段，否则 Intl 会再套一层宿主本地时区，导致非本地 offset 输出错位。
		=> JoinFormatParts(new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				LocaleMatcher: options.LocaleMatcher,
				Weekday: options.Weekday,
				Era: options.Era,
				Year: options.Year,
				Month: options.Month,
				Day: options.Day,
				Hour: options.Hour,
				Minute: options.Minute,
				Second: options.Second,
				TimeZoneName: options.TimeZoneName,
				FormatMatcher: options.FormatMatcher,
				Hour12: options.Hour12,
				TimeZone: "UTC")).FormatToParts(date));

	private static string FormatGeneralDateTimeOffset(RuntimeModule.JDateTimeOffset instance, bool includeSeconds, bool includeOffset, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantGeneralDateTimeOffset(instance, includeSeconds, includeOffset);

		var text = FormatOffsetLocaleDateTime(
			GetOffsetLocalDate(instance),
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.NumericTwoDigit.TwoDigit,
				Day: Intl.NumericTwoDigit.TwoDigit,
				Hour: Intl.NumericTwoDigit.TwoDigit,
				Minute: Intl.NumericTwoDigit.TwoDigit,
				Second: includeSeconds ? Intl.NumericTwoDigit.TwoDigit : null,
				Hour12: false));
		if (includeOffset)
			text += " " + FormatOffsetTicks(instance.OffsetTicks, 3);

		return text;
	}

	private static string FormatShortDate(RuntimeModule.JDateTimeOffset instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantShortDate(instance);

		return FormatOffsetLocaleDateTime(
			GetOffsetLocalDate(instance),
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.NumericTwoDigit.TwoDigit,
				Day: Intl.NumericTwoDigit.TwoDigit));
	}

	private static string FormatLongDate(RuntimeModule.JDateTimeOffset instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantLongDate(instance);

		return FormatOffsetLocaleDateTime(
			GetOffsetLocalDate(instance),
			locale,
			new Intl.DateTimeFormatOptions(
				Weekday: Intl.LongShortNarrow.Long,
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.LongShortNarrow.Long,
				Day: Intl.NumericTwoDigit.TwoDigit));
	}

	private static string FormatTime(RuntimeModule.JDateTimeOffset instance, bool includeSeconds, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantTime(instance, includeSeconds);

		return FormatOffsetLocaleDateTime(
			GetOffsetLocalDate(instance),
			locale,
			new Intl.DateTimeFormatOptions(
				Hour: Intl.NumericTwoDigit.TwoDigit,
				Minute: Intl.NumericTwoDigit.TwoDigit,
				Second: includeSeconds ? Intl.NumericTwoDigit.TwoDigit : null,
				Hour12: false));
	}

	private static string FormatUniversalSortableDateTimeOffset(RuntimeModule.JDateTimeOffset instance)
	{
		var utc = instance.UtcDateTime;
		return RuntimeModule.FormatDateOnlyText(utc.GetUTCFullYear(), utc.GetUTCMonth() + 1, utc.GetUTCDate())
			+ " "
			+ RuntimeModule.Pad2(utc.GetUTCHours())
			+ ":"
			+ RuntimeModule.Pad2(utc.GetUTCMinutes())
			+ ":"
			+ RuntimeModule.Pad2(utc.GetUTCSeconds())
			+ "Z";
	}

	private static string FormatSortableDateTimeOffset(RuntimeModule.JDateTimeOffset instance)
	{
		var local = GetOffsetLocalDate(instance);
		return RuntimeModule.FormatDateOnlyText(local.GetUTCFullYear(), local.GetUTCMonth() + 1, local.GetUTCDate())
			+ "T"
			+ RuntimeModule.Pad2(local.GetUTCHours())
			+ ":"
			+ RuntimeModule.Pad2(local.GetUTCMinutes())
			+ ":"
			+ RuntimeModule.Pad2(local.GetUTCSeconds());
	}

	private static string FormatFraction(BigInt fraction, int count, bool trimTrailingZeros)
	{
		var text = RuntimeModule.Pad7(fraction);
		if (count < 7)
			text = text.Substring(0, count);
		if (!trimTrailingZeros)
			return text;

		while (text.Length > 0 && text[text.Length - 1] == '0')
			text = text.Substring(0, text.Length - 1);

		return text;
	}

	private static string FormatCustomToken(RuntimeModule.JDateTimeOffset instance, char token, int count, string locale, string dateSeparator, string timeSeparator)
	{
		var local = GetOffsetLocalDate(instance);
		var year = local.GetUTCFullYear();
		var month = local.GetUTCMonth() + 1;
		var day = local.GetUTCDate();
		var hour = local.GetUTCHours();
		var hour12 = hour % 12;
		if (hour12 == 0)
			hour12 = 12;
		var minute = local.GetUTCMinutes();
		var second = local.GetUTCSeconds();
		var fraction = BigIntFn(local.GetUTCMilliseconds()) * TicksPerMillisecond + instance.UtcSubMillisecondTicks;

		switch (token)
		{
			case 'y':
				if (count == 2)
					return RuntimeModule.Pad2(year % 100);
				return RuntimeModule.PadLeft(year.ToString()!, count < 4 ? 4 : count);
			case 'M':
				if (count == 1)
					return month.ToString()!;
				if (count == 2)
					return RuntimeModule.Pad2(month);
				if (count == 3)
					return GetLocalizedMonthName(locale, month, true);
				return GetLocalizedMonthName(locale, month, false);
			case 'd':
				if (count == 1)
					return day.ToString()!;
				if (count == 2)
					return RuntimeModule.Pad2(day);
				if (count == 3)
					return GetLocalizedDayName(locale, local.GetUTCDay(), true);
				return GetLocalizedDayName(locale, local.GetUTCDay(), false);
			case 'H':
				return count == 1 ? hour.ToString()! : RuntimeModule.Pad2(hour);
			case 'h':
				return count == 1 ? hour12.ToString()! : RuntimeModule.Pad2(hour12);
			case 'm':
				return count == 1 ? minute.ToString()! : RuntimeModule.Pad2(minute);
			case 's':
				return count == 1 ? second.ToString()! : RuntimeModule.Pad2(second);
			case 't':
				var dayPeriod = GetLocalizedDayPeriod(local, locale);
				return count == 1
					? dayPeriod.Substring(0, 1)
					: dayPeriod;
			case 'f':
				return FormatFraction(fraction, count, false);
			case 'F':
				return FormatFraction(fraction, count, true);
			case 'z':
				return FormatOffsetTicks(instance.OffsetTicks, count);
			case 'K':
				return FormatOffsetTicks(instance.OffsetTicks, 3);
			case ':':
				return timeSeparator;
			case '/':
				return dateSeparator;
			default:
				var text = "";
				for (var j = 0; j < count; j++)
					text += token;
				return text;
		}
	}

	private static string FormatCustomDateTimeOffset(RuntimeModule.JDateTimeOffset instance, string format, object? provider)
	{
		var locale = GetProviderLocale(provider);
		var dateSeparator = GetDateSeparator(locale);
		var timeSeparator = GetTimeSeparator(locale);
		var text = "";

		for (var i = 0; i < format.Length;)
		{
			var token = format[i];
			if (token == '%')
			{
				if (i + 1 >= format.Length || format[i + 1] == '%')
					throw new Error("FormatException: Input string was not in a correct format.");

				text += FormatCustomToken(instance, format[i + 1], 1, locale, dateSeparator, timeSeparator);

				i += 2;
				continue;
			}

			if (token == '\\')
			{
				if (i + 1 < format.Length)
					text += format[i + 1];

				i += 2;
				continue;
			}

			if (token == '\'' || token == '"')
			{
				var quote = token;
				i++;
				while (i < format.Length && format[i] != quote)
				{
					text += format[i];
					i++;
				}

				if (i < format.Length)
					i++;

				continue;
			}

			var count = 1;
			while (i + count < format.Length && format[i + count] == token)
				count++;

			text += FormatCustomToken(instance, token, count, locale, dateSeparator, timeSeparator);

			i += count;
		}

		return text;
	}

	private static string FormatDateTimeOffset(RuntimeModule.JDateTimeOffset instance, string? format, object? formatProvider)
	{
		if (format == null || format.Length == 0)
			return FormatGeneralDateTimeOffset(instance, true, true, formatProvider);

		if (format.Length == 1)
		{
			switch (format[0])
			{
				case 'f':
					return FormatFullDateTime(instance, false, formatProvider);
				case 'F':
					return FormatFullDateTime(instance, true, formatProvider);
				case 'M':
				case 'm':
					return FormatMonthDay(instance, formatProvider);
				case 'O':
				case 'o':
					return instance.ToString();
				case 'G':
					return FormatGeneralDateTimeOffset(instance, true, false, formatProvider);
				case 'g':
					return FormatGeneralDateTimeOffset(instance, false, false, formatProvider);
				case 'R':
				case 'r':
					return FormatRfc1123DateTimeOffset(instance);
				case 'd':
					return FormatShortDate(instance, formatProvider);
				case 'D':
					return FormatLongDate(instance, formatProvider);
				case 't':
					return FormatTime(instance, false, formatProvider);
				case 'T':
					return FormatTime(instance, true, formatProvider);
				case 's':
					return FormatSortableDateTimeOffset(instance);
				case 'u':
					return FormatUniversalSortableDateTimeOffset(instance);
				case 'Y':
				case 'y':
					return FormatYearMonth(instance, formatProvider);
				default:
					if (IsAsciiLetter(format[0]))
						throw new Error("FormatException: Input string was not in a correct format.");
					break;
			}
		}

		return FormatCustomDateTimeOffset(instance, format, formatProvider);
	}

	private static bool HasUtcSuffix(string input)
		=> input.EndsWith("Z") || input.EndsWith("z");

	private static bool HasExplicitOffset(string input)
	{
		var timeIndex = input.LastIndexOf('T');
		var spaceIndex = input.LastIndexOf(' ');
		if (spaceIndex > timeIndex)
			timeIndex = spaceIndex;

		if (input.Length >= 6)
		{
			var signIndex = input.Length - 6;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && input[input.Length - 3] == ':' && signIndex > timeIndex)
				return true;
		}

		if (input.Length >= 5)
		{
			var signIndex = input.Length - 5;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && signIndex > timeIndex)
				return true;
		}

		if (input.Length >= 3)
		{
			var signIndex = input.Length - 3;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && signIndex > timeIndex)
				return true;
		}

		return false;
	}

	private static bool IsAsciiDigit(char value)
		=> value >= '0' && value <= '9';

	private static bool TryParseTwoDigits(string text, int start, out Number value)
	{
		value = 0;
		if (start < 0 || start + 2 > text.Length)
			return false;
		if (!IsAsciiDigit(text[start]) || !IsAsciiDigit(text[start + 1]))
			return false;

		value = NumberFn(text.Substring(start, 2));
		return true;
	}

	private static bool TryParseIsoDate(string text, out Number year, out Number month, out Number day)
	{
		year = 0;
		month = 0;
		day = 0;

		if (text.Length != 10 || text[4] != '-' || text[7] != '-')
			return false;

		for (var i = 0; i < text.Length; i++)
		{
			if (i == 4 || i == 7)
				continue;

			if (!IsAsciiDigit(text[i]))
				return false;
		}

		year = NumberFn(text.Substring(0, 4));
		month = NumberFn(text.Substring(5, 2));
		day = NumberFn(text.Substring(8, 2));
		if (year < 1 || year > 9999 || month < 1 || month > 12)
			return false;

		var daysInMonth = RuntimeModule.GetDaysInMonth(year, month);
		return day >= 1 && day <= daysInMonth;
	}

	private static bool TryParseIsoDateTime(
		string text,
		out Number year,
		out Number month,
		out Number day,
		out Number hour,
		out Number minute,
		out Number second,
		out Number millisecond,
		out BigInt subMillisecondTicks,
		out bool hasExplicitOffset,
		out BigInt offsetTicks)
	{
		year = 0;
		month = 0;
		day = 0;
		hour = 0;
		minute = 0;
		second = 0;
		millisecond = 0;
		subMillisecondTicks = ZeroTicks;
		hasExplicitOffset = false;
		offsetTicks = ZeroTicks;

		if (text.Length < 16)
			return false;
		if (!TryParseIsoDate(text.Substring(0, 10), out year, out month, out day))
			return false;

		var separator = text[10];
		if (separator != 'T' && separator != ' ')
			return false;
		if (!TryParseTwoDigits(text, 11, out hour) || text[13] != ':' || !TryParseTwoDigits(text, 14, out minute))
			return false;
		if (hour > 23 || minute > 59)
			return false;

		var index = 16;
		if (index < text.Length && text[index] == ':')
		{
			if (!TryParseTwoDigits(text, index + 1, out second))
				return false;
			if (second > 59)
				return false;
			index += 3;
		}

		if (index < text.Length && text[index] == '.')
		{
			index++;
			var fractionStart = index;
			while (index < text.Length && IsAsciiDigit(text[index]))
				index++;

			var digits = text.Substring(fractionStart, index - fractionStart);
			if (digits.Length == 0 || digits.Length > 7)
				return false;

			while (digits.Length < 7)
				digits += "0";

			millisecond = NumberFn(digits.Substring(0, 3));
			subMillisecondTicks = BigIntFn(digits.Substring(3, 4));
		}

		if (index == text.Length)
			return true;

		hasExplicitOffset = true;
		if (index == text.Length - 1 && (text[index] == 'Z' || text[index] == 'z'))
			return true;

		var sign = text[index];
		if (sign != '+' && sign != '-')
			return false;

		var remaining = text.Length - index - 1;
		Number offsetHours;
		Number offsetMinutes;
		if (remaining == 2)
		{
			if (!TryParseTwoDigits(text, index + 1, out offsetHours))
				return false;
			offsetMinutes = 0;
		}
		else if (remaining == 4)
		{
			if (!TryParseTwoDigits(text, index + 1, out offsetHours) || !TryParseTwoDigits(text, index + 3, out offsetMinutes))
				return false;
		}
		else if (remaining == 5 && text[index + 3] == ':')
		{
			if (!TryParseTwoDigits(text, index + 1, out offsetHours) || !TryParseTwoDigits(text, index + 4, out offsetMinutes))
				return false;
		}
		else
		{
			return false;
		}

		if (offsetHours > 14 || offsetMinutes > 59 || (offsetHours == 14 && offsetMinutes != 0))
			return false;

		offsetTicks = BigIntFn(offsetHours * 60 + offsetMinutes) * OffsetMinuteTicks;
		if (sign == '-')
			offsetTicks = -offsetTicks;

		return true;
	}

	private static bool TryParseTimeOnly(
		string text,
		out Number hour,
		out Number minute,
		out Number second,
		out Number millisecond,
		out BigInt subMillisecondTicks,
		out bool hasExplicitOffset,
		out BigInt offsetTicks)
	{
		hour = 0;
		minute = 0;
		second = 0;
		millisecond = 0;
		subMillisecondTicks = ZeroTicks;
		hasExplicitOffset = false;
		offsetTicks = ZeroTicks;

		if (text.Length < 5)
			return false;
		if (!TryParseTwoDigits(text, 0, out hour) || text[2] != ':' || !TryParseTwoDigits(text, 3, out minute))
			return false;
		if (hour > 23 || minute > 59)
			return false;

		var index = 5;
		if (index < text.Length && text[index] == ':')
		{
			if (!TryParseTwoDigits(text, index + 1, out second))
				return false;
			if (second > 59)
				return false;
			index += 3;
		}

		if (index < text.Length && text[index] == '.')
		{
			index++;
			var fractionStart = index;
			while (index < text.Length && IsAsciiDigit(text[index]))
				index++;

			var digits = text.Substring(fractionStart, index - fractionStart);
			if (digits.Length == 0 || digits.Length > 7)
				return false;

			while (digits.Length < 7)
				digits += "0";

			millisecond = NumberFn(digits.Substring(0, 3));
			subMillisecondTicks = BigIntFn(digits.Substring(3, 4));
		}

		if (index == text.Length)
			return true;

		hasExplicitOffset = true;
		if (index == text.Length - 1 && (text[index] == 'Z' || text[index] == 'z'))
			return true;

		var sign = text[index];
		if (sign != '+' && sign != '-')
			return false;

		var remaining = text.Length - index - 1;
		Number offsetHours;
		Number offsetMinutes;
		if (remaining == 2)
		{
			if (!TryParseTwoDigits(text, index + 1, out offsetHours))
				return false;
			offsetMinutes = 0;
		}
		else if (remaining == 4)
		{
			if (!TryParseTwoDigits(text, index + 1, out offsetHours) || !TryParseTwoDigits(text, index + 3, out offsetMinutes))
				return false;
		}
		else if (remaining == 5 && text[index + 3] == ':')
		{
			if (!TryParseTwoDigits(text, index + 1, out offsetHours) || !TryParseTwoDigits(text, index + 4, out offsetMinutes))
				return false;
		}
		else
		{
			return false;
		}

		if (offsetHours > 14 || offsetMinutes > 59 || (offsetHours == 14 && offsetMinutes != 0))
			return false;

		offsetTicks = BigIntFn(offsetHours * 60 + offsetMinutes) * OffsetMinuteTicks;
		if (sign == '-')
			offsetTicks = -offsetTicks;

		return true;
	}

	private static BigInt CreateAddUnitTicks(Number value, BigInt ticksPerUnit)
	{
		if (DoubleModule.IsNaNCore(value))
			throw new Error("ArgumentException: Value cannot be NaN.");

		if (!DoubleModule.IsFiniteCore(value))
			throw new Error("ArgumentOutOfRangeException: Value must be finite.");

		var maxUnitCount = NumberFn(MaxDateTimeTicks) / NumberFn(ticksPerUnit);
		if (Math.AbsFn(value) > maxUnitCount)
			throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTimeOffset range.");

		var integralPart = Math.TruncFn(value);
		var fractionalPart = value - integralPart;
		return BigIntFn(integralPart) * ticksPerUnit + BigIntFn(Math.TruncFn(fractionalPart * NumberFn(ticksPerUnit)));
	}

	private static Number GetDateTimeStylesValue(object styles)
	{
		if (styles is Number numberStyle)
			return numberStyle;
		if (styles is System.Globalization.DateTimeStyles enumStyle)
			return NumberFn((int)enumStyle);
		if (styles == null)
			return 0;

		throw new Error("ArgumentException: Invalid DateTimeStyles value.");
	}

	private static void ValidateDateTimeStyles(Number styles)
	{
		if (styles < 0 || Math.FloorFn(styles) != styles)
			throw new Error("ArgumentException: Invalid DateTimeStyles value.");

		if ((styles & DateTimeStylesNoCurrentDateDefault) != 0)
			throw new Error("ArgumentException: NoCurrentDateDefault is not allowed when parsing DateTimeOffset.");

		var hasAssumeLocal = (styles & DateTimeStylesAssumeLocal) != 0;
		var hasAssumeUniversal = (styles & DateTimeStylesAssumeUniversal) != 0;
		if (hasAssumeLocal && hasAssumeUniversal)
			throw new Error("ArgumentException: AssumeLocal and AssumeUniversal cannot both be set.");
	}

	private static RuntimeModule.JDateTimeOffset ApplyDateTimeStyles(RuntimeModule.JDateTimeOffset value, string input, object styles)
	{
		var styleValue = GetDateTimeStylesValue(styles);
		ValidateDateTimeStyles(styleValue);

		var text = input.Trim();
		var hasExplicitZone = HasUtcSuffix(text) || HasExplicitOffset(text);
		var adjustToUniversal = (styleValue & DateTimeStylesAdjustToUniversal) != 0;
		var assumeUniversal = (styleValue & DateTimeStylesAssumeUniversal) != 0;

		var result = value;
		if (!hasExplicitZone && assumeUniversal)
			result = CreateFromUtcTicks(GetTicks(value), ZeroTicks);

		if (adjustToUniversal)
			return CreateFromUtcTicks(GetUtcTicks(result), ZeroTicks);

		return result;
	}

	private static BigInt ResolveParsedOffsetTicks(string input, Date parsedDate)
	{
		var timeIndex = input.LastIndexOf('T');
		var spaceIndex = input.LastIndexOf(' ');
		if (spaceIndex > timeIndex)
			timeIndex = spaceIndex;

		if (input.EndsWith("Z") || input.EndsWith("z"))
			return ZeroTicks;

		if (input.Length >= 6)
		{
			var signIndex = input.Length - 6;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && input[input.Length - 3] == ':')
			{
				var hours = NumberFn(input.Substring(input.Length - 5, 2));
				var minutes = NumberFn(input.Substring(input.Length - 2, 2));
				if (signIndex > timeIndex && !IsNaN(hours) && !IsNaN(minutes) && minutes >= 0 && minutes < 60)
				{
					var ticks = BigIntFn(hours * 60 + minutes) * OffsetMinuteTicks;
					return sign == '-' ? -ticks : ticks;
				}
			}
		}

		if (input.Length >= 5)
		{
			var signIndex = input.Length - 5;
			var sign = input[signIndex];
			if (sign == '+' || sign == '-')
			{
				var hours = NumberFn(input.Substring(input.Length - 4, 2));
				var minutes = NumberFn(input.Substring(input.Length - 2, 2));
				if (signIndex > timeIndex && !IsNaN(hours) && !IsNaN(minutes) && minutes >= 0 && minutes < 60)
				{
					var ticks = BigIntFn(hours * 60 + minutes) * OffsetMinuteTicks;
					return sign == '-' ? -ticks : ticks;
				}
			}
		}

		if (input.Length >= 3)
		{
			var signIndex = input.Length - 3;
			var sign = input[signIndex];
			if (sign == '+' || sign == '-')
			{
				var hours = NumberFn(input.Substring(input.Length - 2, 2));
				if (signIndex > timeIndex && !IsNaN(hours))
				{
					var ticks = BigIntFn(hours * 60) * OffsetMinuteTicks;
					return sign == '-' ? -ticks : ticks;
				}
			}
		}

		return BigIntFn(-parsedDate.GetTimezoneOffset()) * OffsetMinuteTicks;
	}

	private static BigInt ExtractSubMillisecondTicks(string input)
	{
		var timeIndex = input.LastIndexOf('T');
		var spaceIndex = input.LastIndexOf(' ');
		if (spaceIndex > timeIndex)
			timeIndex = spaceIndex;

		var fractionIndex = input.IndexOf('.', timeIndex + 1);
		if (fractionIndex < 0)
			return ZeroTicks;

		var end = input.Length;
		for (var i = fractionIndex + 1; i < input.Length; i++)
		{
			var c = input[i];
			if (c < '0' || c > '9')
			{
				end = i;
				break;
			}
		}

		var digits = input.Substring(fractionIndex + 1, end - fractionIndex - 1);
		if (digits.Length == 0 || digits.Length > 7)
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTimeOffset.");

		while (digits.Length < 7)
			digits += "0";

		return BigIntFn(digits.Substring(3, 4));
	}

	private static BigInt FloorDiv(BigInt value, BigInt divisor)
	{
		var quotient = value / divisor;
		var remainder = value % divisor;
		if (remainder < ZeroTicks)
			return quotient - BigIntFn(1);

		return quotient;
	}

	private static RuntimeModule.JDateTimeOffset ParseCore(string input)
	{
		var s = input.Trim();
		if (s.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid DateTimeOffset.");

		if (TryParseTimeOnly(s, out var timeHour, out var timeMinute, out var timeSecond, out var timeMillisecond, out var timeSubMillisecondTicks, out var timeHasExplicitOffset, out var timeOffsetTicks))
		{
			var now = new Date();
			var currentYear = now.GetFullYear();
			var currentMonth = now.GetMonth() + 1;
			var currentDay = now.GetDate();
			if (!timeHasExplicitOffset)
			{
				var localDateTime = RuntimeModule.CreateLocalDateTime(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond);
				var localOffsetTicks = BigIntFn(-localDateTime.GetTimezoneOffset()) * OffsetMinuteTicks;
				var utcTicks = BigIntFn(localDateTime.GetTime()) * TicksPerMillisecond + timeSubMillisecondTicks + UnixEpochTicks;
				return CreateFromUtcTicks(utcTicks, localOffsetTicks);
			}

			var localTicks = CreateLocalTicks(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond) + timeSubMillisecondTicks;
			return CreateFromUtcTicks(localTicks - timeOffsetTicks, timeOffsetTicks);
		}

		if (TryParseIsoDate(s, out var year, out var month, out var day))
		{
			var date = RuntimeModule.CreateLocalDate(year, month, day);
			var utcTicks = BigIntFn(date.GetTime()) * TicksPerMillisecond + UnixEpochTicks;
			return CreateWithLocalOffset(utcTicks);
		}

		if (TryParseIsoDateTime(s, out year, out month, out day, out var hour, out var minute, out var second, out var millisecond, out var subMillisecondTicks, out var hasExplicitOffset, out var offsetTicks))
		{
			if (!hasExplicitOffset)
			{
				var localDateTime = RuntimeModule.CreateLocalDateTime(year, month, day, hour, minute, second, millisecond);
				var localOffsetTicks = BigIntFn(-localDateTime.GetTimezoneOffset()) * OffsetMinuteTicks;
				var utcTicks = BigIntFn(localDateTime.GetTime()) * TicksPerMillisecond + subMillisecondTicks + UnixEpochTicks;
				return CreateFromUtcTicks(utcTicks, localOffsetTicks);
			}

			var localTicks = CreateLocalTicks(year, month, day, hour, minute, second, millisecond) + subMillisecondTicks;
			return CreateFromUtcTicks(localTicks - offsetTicks, offsetTicks);
		}

		var parsed = new Date(s);
		if (IsNaN(parsed.GetTime()))
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTimeOffset.");

		var resolvedOffsetTicks = ResolveParsedOffsetTicks(s, parsed);
		if (resolvedOffsetTicks < -MaxOffsetTicks || resolvedOffsetTicks > MaxOffsetTicks)
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTimeOffset.");

		var parsedSubMillisecondTicks = ExtractSubMillisecondTicks(s);
		return CreateFromUtcTicks(BigIntFn(parsed.GetTime()) * TicksPerMillisecond + parsedSubMillisecondTicks + UnixEpochTicks, resolvedOffsetTicks);
	}

	/// <summary>
	/// C#: DateTimeOffset.MinValue
	/// JS: new Date(-8640000000000000)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTimeOffset.MinValue")]
	public static RuntimeModule.JDateTimeOffset _77107f0c23675b69() => CreateDefaultDateTimeOffset();

	/// <summary>
	/// C#: DateTimeOffset.MaxValue
	/// JS: new Date(8640000000000000)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTimeOffset.MaxValue")]
	public static RuntimeModule.JDateTimeOffset _d45d439f0b97ae0e() => CreateDateTimeOffset(new Date(253402300799999d), ZeroTicks, BigIntFn("9999"));

	/// <summary>
	/// C#: DateTimeOffset.UnixEpoch
	/// JS: new Date(0)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTimeOffset.UnixEpoch")]
	public static RuntimeModule.JDateTimeOffset _087cabaedc1b5cc2() => CreateDateTimeOffset(new Date(0), ZeroTicks);

	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset()")]
	public static RuntimeModule.JDateTimeOffset _12b4f3f1dc14bea9() => CreateDefaultDateTimeOffset();

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified number of ticks and offset.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _1e9c5d2a64e6d41d(BigInt ticks, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		return CreateFromUtcTicks(ticks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(System.DateTime)")]
	public static RuntimeModule.JDateTimeOffset _7adf69a53659433a(RuntimeModule.JDateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKindUtc)
			return CreateFromUtcTicks(GetDateTimeInstantTicks(dateTime), ZeroTicks);

		var instantTicks = GetDateTimeInstantTicks(dateTime);
		return CreateWithLocalOffset(instantTicks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value and offset.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(System.DateTime, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _106dabc0cc502aa4(RuntimeModule.JDateTime dateTime, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		if (dateTime.Kind == DateTimeKindUtc)
		{
			if (offset.Ticks != ZeroTicks)
				throw new Error("ArgumentException: The UTC Offset for Utc DateTime instances must be 0.");

			return CreateFromUtcTicks(GetDateTimeInstantTicks(dateTime), ZeroTicks);
		}

		if (dateTime.Kind == DateTimeKindLocal)
		{
			var expectedOffset = BigIntFn(-dateTime.Date.GetTimezoneOffset()) * OffsetMinuteTicks;
			if (expectedOffset != offset.Ticks)
				throw new Error("ArgumentException: The UTC Offset of the local dateTime parameter does not match the offset argument.");
		}

		return CreateFromUtcTicks(GetDateTimeTicks(dateTime) - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="date" />, <paramref name="time" />, and <paramref name="offset" />.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(System.DateOnly, System.TimeOnly, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _8f1aab77eeb6f786(RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		var localTicks = CreateLocalTicks(date.Year, date.Month, date.Day, 0, 0, 0, 0) + time.Ticks;
		return CreateFromUtcTicks(localTicks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, and offset.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _d90dce0e1d2f06e4(Number year, Number month, Number day, Number hour, Number minute, Number second, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		var localTicks = CreateLocalTicks(year, month, day, hour, minute, second, 0);
		return CreateFromUtcTicks(localTicks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _6abaa2b2082f575c(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		var localTicks = CreateLocalTicks(year, month, day, hour, minute, second, millisecond);
		return CreateFromUtcTicks(localTicks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset of a specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")]
	public extern static RuntimeModule.JDateTimeOffset _61ea80919619bab9(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, RuntimeModule.JTimeSpan offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _04123d597aa761a3(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		ValidateMicrosecond(microsecond);
		var localTicks = CreateLocalTicks(year, month, day, hour, minute, second, millisecond) + BigIntFn(microsecond) * BigIntFn("10");
		return CreateFromUtcTicks(localTicks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")]
	public extern static RuntimeModule.JDateTimeOffset _d027561c1f6af451(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, RuntimeModule.JTimeSpan offset);

	/// <summary>
	/// C#: DateTimeOffset.UtcNow
	/// JS: new Date() - current UTC time
	/// </summary>
	[Jazor(Op.Import, "static System.DateTimeOffset.UtcNow.get")]
	public static RuntimeModule.JDateTimeOffset _7f444d9ce7391e15() => CreateDateTimeOffset(new Date(), ZeroTicks);

	/// <summary>
	/// C#: instance.DateTime
	/// JS: 构造一个新的 JDateTime，保留 offset 对应的墙上时间，Kind 为 Unspecified
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DateTime.get")]
	public static RuntimeModule.JDateTime _2b7dd675863ae961(RuntimeModule.JDateTimeOffset instance)
	{
		var localTicks = GetTicks(instance);
		return new RuntimeModule.JDateTime(
			RuntimeModule.CreateLocalDateTime(
				NumberFn(_127105b7a40a7665(instance)),
				NumberFn(_79eb4c93cea58d59(instance)),
				NumberFn(_ba8df912681fe784(instance)),
				NumberFn(_b7fc65477ef4df45(instance)),
				NumberFn(_0fe8054b55f9f1c7(instance)),
				NumberFn(_822de224fed5bb6b(instance)),
				NumberFn(_0c1b2675cd7a2faa(instance))),
			0,
			NormalizeSubMillisecondTicks(localTicks));
	}

	/// <summary>
	/// C#: instance.UtcDateTime
	/// JS: 构造一个新的 JDateTime，表示同一瞬时点的 UTC 墙上时间，Kind 为 Utc
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.UtcDateTime.get")]
	public static RuntimeModule.JDateTime _703902cecd7f61dd(RuntimeModule.JDateTimeOffset instance)
	{
		var utc = instance.UtcDateTime;
		return new RuntimeModule.JDateTime(RuntimeModule.CreateLocalDateTime(utc.GetUTCFullYear(), utc.GetUTCMonth() + 1, utc.GetUTCDate(), utc.GetUTCHours(), utc.GetUTCMinutes(), utc.GetUTCSeconds(), utc.GetUTCMilliseconds()), DateTimeKindUtc, instance.UtcSubMillisecondTicks);
	}

	/// <summary>
	/// C#: instance.LocalDateTime
	/// JS: 构造一个新的 JDateTime，表示同一瞬时点在宿主本地时区下的墙上时间，Kind 为 Local
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.LocalDateTime.get")]
	public static RuntimeModule.JDateTime _ffbfe7b660ff0527(RuntimeModule.JDateTimeOffset instance)
		=> new(new Date(instance.UtcDateTime.GetTime()), DateTimeKindLocal, instance.UtcSubMillisecondTicks);

	/// <summary>
	/// C#: instance.ToOffset(offset)
	/// JS: new Date(instance.getTime() + offset)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.ToOffset(System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _d1996f02ed3fa243(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		return CreateDateTimeOffset(new Date(instance.UtcDateTime.GetTime()), offset.Ticks, instance.UtcSubMillisecondTicks);
	}

	/// <summary>
	/// C#: instance.Date
	/// JS: 构造一个新的 JDateTime，保留 offset 对应的日期部分并截断到午夜，Kind 为 Unspecified
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Date.get")]
	public static RuntimeModule.JDateTime _d7098a1eabebc945(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return new RuntimeModule.JDateTime(RuntimeModule.CreateLocalDate(local.GetUTCFullYear(), local.GetUTCMonth() + 1, local.GetUTCDate()), 0);
	}

	/// <summary>
	/// C#: instance.Day
	/// JS: instance.getDate()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Day.get")]
	public static Number _ba8df912681fe784(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCDate();
	}

	/// <summary>
	/// C#: instance.DayOfWeek
	/// JS: instance.getDay()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DayOfWeek.get")]
	public static System.DayOfWeek _17d30a204818ce34(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return (System.DayOfWeek)(int)local.GetUTCDay();
	}

	/// <summary>
	/// C#: instance.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DayOfYear.get")]
	public static Number _b69ef2b7d0abde1a(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		var start = Date.UTC(local.GetUTCFullYear(), 0, 0);
		return Math.FloorFn((local.GetTime() - start) / 86400000);
	}

	/// <summary>
	/// C#: instance.Hour
	/// JS: instance.getHours()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Hour.get")]
	public static Number _b7fc65477ef4df45(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCHours();
	}

	/// <summary>
	/// C#: instance.Millisecond
	/// JS: instance.getMilliseconds()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Millisecond.get")]
	public static Number _0c1b2675cd7a2faa(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCMilliseconds();
	}

	/// <summary>
	/// C#: instance.Microsecond
	/// JS: instance.getMilliseconds() * 1000 (approximation)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Microsecond.get")]
	public static Number _ae3a48995f0953ed(RuntimeModule.JDateTimeOffset instance)
		=> NumberFn((instance.UtcSubMillisecondTicks / BigIntFn("10")) % BigIntFn(1000));

	/// <summary>
	/// C#: instance.Nanosecond
	/// JS: 0 (JavaScript Date does not support nanoseconds)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Nanosecond.get")]
	public static Number _f9acef215c7d5168(RuntimeModule.JDateTimeOffset instance)
		=> NumberFn((instance.UtcSubMillisecondTicks % BigIntFn("10")) * BigIntFn(100));

	/// <summary>
	/// C#: instance.Minute
	/// JS: instance.getMinutes()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Minute.get")]
	public static Number _0fe8054b55f9f1c7(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCMinutes();
	}

	/// <summary>
	/// C#: instance.Month
	/// JS: instance.getMonth() + 1
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Month.get")]
	public static Number _79eb4c93cea58d59(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCMonth() + 1;
	}

	/// <summary>
	/// C#: instance.Offset
	/// JS: instance.getTimezoneOffset() * -600000000 (ticks)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Offset.get")]
	public static RuntimeModule.JTimeSpan _2400298964c553b6(RuntimeModule.JDateTimeOffset instance)
		=> new(instance.OffsetTicks);

	/// <summary>
	/// C#: instance.Second
	/// JS: instance.getSeconds()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Second.get")]
	public static Number _822de224fed5bb6b(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCSeconds();
	}

	/// <summary>
	/// C#: instance.Ticks
	/// JS: BigInt(instance.getTime()) * 10000n + 621355968000000000n
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Ticks.get")]
	public static BigInt _584068ab15dcf3c9(RuntimeModule.JDateTimeOffset instance)
		=> GetTicks(instance);

	/// <summary>
	/// C#: instance.UtcTicks
	/// JS: BigInt(instance.getTime()) * 10000n + 621355968000000000n
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.UtcTicks.get")]
	public static BigInt _056adc0ac251ebd3(RuntimeModule.JDateTimeOffset instance)
		=> GetUtcTicks(instance);

	/// <summary>
	/// C#: instance.TimeOfDay
	/// JS: (instance.getTime() % 86400000) * 10000n
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.TimeOfDay.get")]
	public static RuntimeModule.JTimeSpan _90401f92f6a9141e(RuntimeModule.JDateTimeOffset instance)
	{
		var normalized = GetTicks(instance) % BigIntFn("864000000000");
		return new RuntimeModule.JTimeSpan(normalized < ZeroTicks ? normalized + BigIntFn("864000000000") : normalized);
	}

	/// <summary>
	/// C#: instance.Year
	/// JS: instance.getFullYear()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Year.get")]
	public static Number _127105b7a40a7665(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + NumberFn(instance.OffsetTicks) / 10000);
		return local.GetUTCFullYear();
	}

	/// <summary>
	/// C#: instance.Add(timeSpan)
	/// JS: new Date(instance.getTime() + Number(timeSpan) / 10000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Add(System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _09a94b0e7945eda6(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JTimeSpan timeSpan)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + timeSpan.Ticks, instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddDays(days)
	/// JS: new Date(instance.getTime() + days * 86400000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddDays(double)")]
	public static RuntimeModule.JDateTimeOffset _7fd735ce2102a3cc(RuntimeModule.JDateTimeOffset instance, Number days)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + CreateAddUnitTicks(days, TicksPerDay), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddHours(hours)
	/// JS: new Date(instance.getTime() + hours * 3600000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddHours(double)")]
	public static RuntimeModule.JDateTimeOffset _309c83b8a2fbc988(RuntimeModule.JDateTimeOffset instance, Number hours)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + CreateAddUnitTicks(hours, TicksPerHour), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMilliseconds(milliseconds)
	/// JS: new Date(instance.getTime() + milliseconds)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMilliseconds(double)")]
	public static RuntimeModule.JDateTimeOffset _1528b452af6dd41d(RuntimeModule.JDateTimeOffset instance, Number milliseconds)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + CreateAddUnitTicks(milliseconds, TicksPerMillisecond), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMicroseconds(microseconds)
	/// JS: new Date(instance.getTime() + microseconds / 1000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMicroseconds(double)")]
	public static RuntimeModule.JDateTimeOffset _4775ccfee8ed671f(RuntimeModule.JDateTimeOffset instance, Number microseconds)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + CreateAddUnitTicks(microseconds, TicksPerMicrosecond), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMinutes(minutes)
	/// JS: new Date(instance.getTime() + minutes * 60000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMinutes(double)")]
	public static RuntimeModule.JDateTimeOffset _97aff1e2f4740394(RuntimeModule.JDateTimeOffset instance, Number minutes)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + CreateAddUnitTicks(minutes, TicksPerMinute), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMonths(months)
	/// JS: new Date(instance.setMonth(instance.getMonth() + months))
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMonths(int)")]
	public static RuntimeModule.JDateTimeOffset _db8ffdb562d3ac68(RuntimeModule.JDateTimeOffset instance, Number months)
		=> AddMonthsCore(instance, months);

	/// <summary>
	/// C#: instance.AddSeconds(seconds)
	/// JS: new Date(instance.getTime() + seconds * 1000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddSeconds(double)")]
	public static RuntimeModule.JDateTimeOffset _54a4d6d554458fdb(RuntimeModule.JDateTimeOffset instance, Number seconds)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + CreateAddUnitTicks(seconds, TicksPerSecond), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddTicks(ticks)
	/// JS: new Date(instance.getTime() + Number(ticks) / 10000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddTicks(long)")]
	public static RuntimeModule.JDateTimeOffset _804f8bd2dc1e9443(RuntimeModule.JDateTimeOffset instance, BigInt ticks)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + ticks, instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddYears(years)
	/// JS: new Date(instance.setFullYear(instance.getFullYear() + years))
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddYears(int)")]
	public static RuntimeModule.JDateTimeOffset _f4ea4e123d38eaa5(RuntimeModule.JDateTimeOffset instance, Number years)
	{
		EnsureWholeNumber(years, "ArgumentOutOfRangeException: Years value must be a whole number.");
		return AddMonthsCore(instance, years * 12);
	}

	///<summary>Compares two <see cref="T:System.DateTimeOffset" /> objects and indicates whether the first is earlier than the second, equal to the second, or later than the second.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Compare(System.DateTimeOffset, System.DateTimeOffset)")]
	public static Number _56ac26a94d0f9bca(RuntimeModule.JDateTimeOffset first, RuntimeModule.JDateTimeOffset second)
	{
		var diff = GetUtcTicks(first) - GetUtcTicks(second);
		if (diff < ZeroTicks)
			return -1;
		if (diff > ZeroTicks)
			return 1;
		return 0;
	}

	///<summary>Compares the current <see cref="T:System.DateTimeOffset" /> object to a specified <see cref="T:System.DateTimeOffset" /> object and indicates whether the current object is earlier than, the same as, or later than the second <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.CompareTo(System.DateTimeOffset)")]
	public static Number _255c7bf4a2c3c663(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JDateTimeOffset other)
		=> _56ac26a94d0f9bca(instance, other);

	///<summary>Compares the current <see cref="T:System.DateTimeOffset" /> object to a specified object and indicates whether the current object is earlier than, the same as, or later than the second object.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.CompareTo(object)")]
	public static Number _f7f499e8872c8e8a(RuntimeModule.JDateTimeOffset instance, object? other)
	{
		if (other == null)
			return 1;

		var value = other as RuntimeModule.JDateTimeOffset;
		if (value == null)
			throw new Error("ArgumentException: Object must be of type DateTimeOffset.");

		return _56ac26a94d0f9bca(instance, value);
	}

	///<summary>Determines whether a <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified object.</summary>
	[Jazor(Op.Import ,"override System.DateTimeOffset.Equals(object)")]
	public static bool _fbec90dd4b315acd(RuntimeModule.JDateTimeOffset instance, object? obj)
	{
		var other = obj as RuntimeModule.JDateTimeOffset;
		if (other == null)
			return false;

		return _5a55745cbe84c163(instance, other);
	}

	///<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.Equals(System.DateTimeOffset)")]
	public static bool _5a55745cbe84c163(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JDateTimeOffset other)
		=> GetUtcTicks(instance) == GetUtcTicks(other);

	///<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same time and has the same offset as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.EqualsExact(System.DateTimeOffset)")]
	public static bool _d4a929178865b462(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JDateTimeOffset other)
		=> GetUtcTicks(instance) == GetUtcTicks(other) && instance.OffsetTicks == other.OffsetTicks;

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Equals(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _817d2f7b0e423bec(RuntimeModule.JDateTimeOffset first, RuntimeModule.JDateTimeOffset second)
		=> _5a55745cbe84c163(first, second);

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.FromFileTime(long)")]
	public static RuntimeModule.JDateTimeOffset _1185de87a3489deb(BigInt fileTime)
	{
		if (fileTime < ZeroTicks)
			throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");

		return CreateWithLocalOffset(fileTime - FileTimeUnixEpochTicks + UnixEpochTicks);
	}

	///<summary>Converts a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.FromUnixTimeSeconds(long)")]
	public static RuntimeModule.JDateTimeOffset _fb7d72712794a2e4(BigInt seconds)
	{
		if (seconds < MinUnixTimeSeconds || seconds > MaxUnixTimeSeconds)
			throw new Error("ArgumentOutOfRangeException: Unix time seconds must be within the range of DateTimeOffset.");

		return CreateDateTimeOffset(new Date(NumberFn(seconds * BigIntFn(1000))), ZeroTicks);
	}

	///<summary>Converts a Unix time expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.FromUnixTimeMilliseconds(long)")]
	public static RuntimeModule.JDateTimeOffset _89071e7da78164f5(BigInt milliseconds)
	{
		if (milliseconds < MinUnixTimeMilliseconds || milliseconds > MaxUnixTimeMilliseconds)
			throw new Error("ArgumentOutOfRangeException: Unix time milliseconds must be within the range of DateTimeOffset.");

		return CreateDateTimeOffset(new Date(NumberFn(milliseconds)), ZeroTicks);
	}

	///<summary>Returns the hash code for the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"override System.DateTimeOffset.GetHashCode()")]
	public static Number _484d626eb36d071d(RuntimeModule.JDateTimeOffset instance)
		=> RuntimeModule.GetInt64HashCode(GetUtcTicks(instance));

	///<summary>Converts the specified string representation of a date, time, and offset to its <see cref="T:System.DateTimeOffset" /> equivalent.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Parse(string)")]
	public static RuntimeModule.JDateTimeOffset _25187a24d190d864(string input)
		=> ParseCore(input);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Parse(string, System.IFormatProvider)")]
	public static RuntimeModule.JDateTimeOffset _fbb732b1255fdd38(string input, Intl.NumberFormat? formatProvider)
		=> ParseCore(input);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTimeOffset _277a1a2c7845bcdc(string input, Intl.NumberFormat? formatProvider, object styles)
		=> ApplyDateTimeStyles(ParseCore(input), input, styles);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTimeOffset _948a165174740d96(string input, Intl.NumberFormat? formatProvider, object styles)
		=> ApplyDateTimeStyles(ParseCore(input), input, styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider)")]
	public extern static RuntimeModule.JDateTimeOffset _ef9349ca95c1e050(string input, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTimeOffset _6da8f452a2644e91(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts a character span that represents a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTimeOffset _cec804cac90222fc(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTimeOffset _d8c615ebc8c99180(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts a character span that contains the string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTimeOffset _9eaf2ad9372cd2ec(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Subtracts a <see cref="T:System.DateTimeOffset" /> value that represents a specific date and time from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.Subtract(System.DateTimeOffset)")]
	public static RuntimeModule.JTimeSpan _f1e08916de33ed2a(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JDateTimeOffset value)
		=> new(GetUtcTicks(instance) - GetUtcTicks(value));

	///<summary>Subtracts a specified time interval from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.Subtract(System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _2636ae85f21cd963(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JTimeSpan value)
		=> CreateFromUtcTicks(GetUtcTicks(instance) - value.Ticks, instance.OffsetTicks);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to a Windows file time.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToFileTime()")]
	public static BigInt _d638010bc91ffd47(RuntimeModule.JDateTimeOffset instance)
		=> GetUtcTicks(instance) - UnixEpochTicks + FileTimeUnixEpochTicks;

	///<summary>Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToUnixTimeSeconds()")]
	public static BigInt _8bc213443653978d(RuntimeModule.JDateTimeOffset instance)
		=> FloorDiv(GetUtcTicks(instance) - UnixEpochTicks, BigIntFn("10000000"));

	///<summary>Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToUnixTimeMilliseconds()")]
	public static BigInt _e63166ec11d88ce1(RuntimeModule.JDateTimeOffset instance)
		=> FloorDiv(GetUtcTicks(instance) - UnixEpochTicks, TicksPerMillisecond);

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> object that represents the local time.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToLocalTime()")]
	public static RuntimeModule.JDateTimeOffset _c45ea6b7c8ed9501(RuntimeModule.JDateTimeOffset instance)
		=> CreateWithLocalOffset(GetUtcTicks(instance));

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Import ,"override System.DateTimeOffset.ToString()")]
	public static string _2aaccc10061a3bb0(RuntimeModule.JDateTimeOffset instance)
		=> FormatDateTimeOffset(instance, null, null);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToString(string)")]
	public static string _9b46cc87f855c6ba(RuntimeModule.JDateTimeOffset instance, string? format)
		=> FormatDateTimeOffset(instance, format, null);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToString(System.IFormatProvider)")]
	public static string _f0d70d071309b539(RuntimeModule.JDateTimeOffset instance, Intl.NumberFormat? formatProvider)
		=> FormatDateTimeOffset(instance, null, formatProvider);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToString(string, System.IFormatProvider)")]
	public static string _e856edbfd7db0646(RuntimeModule.JDateTimeOffset instance, string? format, Intl.NumberFormat? formatProvider)
		=> FormatDateTimeOffset(instance, format, formatProvider);

	///<summary>Tries to format the value of the current datetime offset instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _f899d2eb7dcfcfe9(RuntimeModule.JDateTimeOffset instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _ec001bad537a3ce9(RuntimeModule.JDateTimeOffset instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> value that represents the Coordinated Universal Time (UTC).</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToUniversalTime()")]
	public static RuntimeModule.JDateTimeOffset _cbe0bd9bc2e35d83(RuntimeModule.JDateTimeOffset instance)
		=> CreateDateTimeOffset(new Date(instance.UtcDateTime.GetTime()), ZeroTicks, instance.UtcSubMillisecondTicks);

	///<summary>Tries to converts a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)")]
	public static Array<object?> _2fd90dc37b274014(string? input, RuntimeModule.JDateTimeOffset result)
	{
		if (input == null || input.Length == 0)
			return [false, CreateDefaultDateTimeOffset()];

		try
		{
			return [true, ParseCore(input)];
		}
		catch
		{
			return [false, CreateDefaultDateTimeOffset()];
		}
	}

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, out System.DateTimeOffset)")]
	public static Array<object?> _c7957aa2e68f8218(string input, RuntimeModule.JDateTimeOffset result)
		=> _2fd90dc37b274014(input, result);

	///<summary>Tries to convert a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public static Array<object?> _62fe5aa144f2c9e1(string? input, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result)
	{
		ValidateDateTimeStyles(GetDateTimeStylesValue(styles));
		if (input == null || input.Length == 0)
			return [false, CreateDefaultDateTimeOffset()];

		try
		{
			return [true, ApplyDateTimeStyles(ParseCore(input), input, styles)];
		}
		catch
		{
			return [false, CreateDefaultDateTimeOffset()];
		}
	}

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public static Array<object?> _9dd0fca0c6a9a4de(string input, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result)
		=> _62fe5aa144f2c9e1(input, formatProvider, styles, result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _a99669f1d632e166(string? input, string? format, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result);

	///<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _639a2d041804058b(string input, string format, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _39ec2cd456e46b13(string? input, object formats, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result);

	///<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _d09753e75937de75(string input, object formats, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result);

	///<summary>Defines an implicit conversion of a <see cref="T:System.DateTime" /> object to a <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.implicit operator System.DateTimeOffset(System.DateTime)")]
	public static RuntimeModule.JDateTimeOffset _31bbd12ed57f4f76(RuntimeModule.JDateTime value)
	{
		if (value.Kind == DateTimeKindUtc)
			return CreateFromUtcTicks(GetDateTimeInstantTicks(value), ZeroTicks);

		return CreateWithLocalOffset(GetDateTimeInstantTicks(value));
	}

	///<summary>Adds a specified time interval to a <see cref="T:System.DateTimeOffset" /> object that has a specified date and time, and yields a <see cref="T:System.DateTimeOffset" /> object that has new a date and time.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator +(System.DateTimeOffset, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _b8dd85346f7718fe(RuntimeModule.JDateTimeOffset dateTimeOffset, RuntimeModule.JTimeSpan timeSpan)
		=> _09a94b0e7945eda6(dateTimeOffset, timeSpan);

	///<summary>Subtracts a specified time interval from a specified date and time, and yields a new date and time.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator -(System.DateTimeOffset, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _267065e6d921c80f(RuntimeModule.JDateTimeOffset dateTimeOffset, RuntimeModule.JTimeSpan timeSpan)
		=> _09a94b0e7945eda6(dateTimeOffset, new RuntimeModule.JTimeSpan(-timeSpan.Ticks));

	///<summary>Subtracts one <see cref="T:System.DateTimeOffset" /> object from another and yields a time interval.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator -(System.DateTimeOffset, System.DateTimeOffset)")]
	public static RuntimeModule.JTimeSpan _d1af541d3a7181e8(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> new(GetUtcTicks(left) - GetUtcTicks(right));

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator ==(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _553dcbd8f7ea1a16(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> GetUtcTicks(left) == GetUtcTicks(right);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects refer to different points in time.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator !=(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _9f6eec56175d9528(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> GetUtcTicks(left) != GetUtcTicks(right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator <(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _43aa45c9517f4d47(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> GetUtcTicks(left) < GetUtcTicks(right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator <=(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _a6755e7fc2ead5b5(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> GetUtcTicks(left) <= GetUtcTicks(right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than (or later than) a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator >(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _84d1b669e69cd9bf(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> GetUtcTicks(left) > GetUtcTicks(right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than or equal to a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.operator >=(System.DateTimeOffset, System.DateTimeOffset)")]
	public static bool _1cb1a326e417bc9b(RuntimeModule.JDateTimeOffset left, RuntimeModule.JDateTimeOffset right)
		=> GetUtcTicks(left) >= GetUtcTicks(right);

	///<summary>Deconstructs this <see cref="T:System.DateTimeOffset" /> instance by <see cref="T:System.DateOnly" />, <see cref="T:System.TimeOnly" />, and <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.Deconstruct(out System.DateOnly, out System.TimeOnly, out System.TimeSpan)")]
	public static Array<object?> _6ec7dc3f674ff16c(RuntimeModule.JDateTimeOffset instance, RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time, RuntimeModule.JTimeSpan offset)
		=> [new RuntimeModule.JDateOnly(_127105b7a40a7665(instance), _79eb4c93cea58d59(instance), _ba8df912681fe784(instance)), new RuntimeModule.JTimeOnly(_90401f92f6a9141e(instance).Ticks), new RuntimeModule.JTimeSpan(instance.OffsetTicks)];

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, out System.DateTimeOffset)")]
	public static Array<object?> _61ef673e0dd00ab0(string? s, Intl.NumberFormat? provider, RuntimeModule.JDateTimeOffset result)
		=> _2fd90dc37b274014(s, result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static RuntimeModule.JDateTimeOffset _b0967252268296ed(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTimeOffset)")]
	public static Array<object?> _c9e042e683205a8b(string s, Intl.NumberFormat? provider, RuntimeModule.JDateTimeOffset result)
		=> _2fd90dc37b274014(s, result);

	[Jazor(Op.Import ,"static System.DateTimeOffset.Now.get")]
	public static RuntimeModule.JDateTimeOffset _e679a7abf50cf648()
	{
		var now = new Date();
		var offsetTicks = BigIntFn(-now.GetTimezoneOffset()) * OffsetMinuteTicks;
		return CreateDateTimeOffset(new Date(now.GetTime()), offsetTicks);
	}
}
