namespace Jazor.CLR;

/// <summary>
/// System.DateTime 类型模块映射规则
///
/// C# DateTime 与 JavaScript Date 的对应关系：
/// - 都表示日期和时间
/// - C# DateTime 是值类型，JavaScript Date 是对象
/// - 大部分方法可以直接映射
///
/// Op 类型选择原则：
/// - Inline: 简单表达式（如 Now、Today）
/// - Alias: JS Date 原生方法（如 getFullYear、getMonth）
/// - Import: 需要完整实现的复杂逻辑
/// - Discard: 不支持或极少使用
/// </summary>
[ECMAScriptModule("System/DateTimeModule.js")]
[Jazor(Op.Alias, "System.DateTime","Object")]
public static class DateTimeModule
{
	private static BigInt UnixEpochTicks => BigIntFn("621355968000000000");
	private static BigInt FileTimeUnixEpochTicks => BigIntFn("116444736000000000");
	private static BigInt TicksPerMicrosecond => BigIntFn("10");
	private static BigInt TicksPerMillisecond => BigIntFn("10000");
	private static BigInt TicksPerSecond => BigIntFn("10000000");
	private static BigInt TicksPerMinute => BigIntFn("600000000");
	private static BigInt TicksPerHour => BigIntFn("36000000000");
	private static BigInt TicksPerDay => BigIntFn("864000000000");
	private static BigInt OffsetMinuteTicks => BigIntFn("600000000");
	private static BigInt ZeroTicks => BigInt.Zero;
	private static BigInt BinaryKindShift => BigIntFn("4611686018427387904");
	private static BigInt BinaryLocalMask => BigIntFn("9223372036854775808");
	private static BigInt BinaryKindMask => BigIntFn("13835058055282163712");
	private static BigInt BinaryUnsignedOverflow => BigIntFn("18446744073709551616");
	private static BigInt BinaryTicksMask => BigIntFn("4611686018427387903");
	private static BigInt MaxDateTimeTicks => BigIntFn("3155378975999999999");
	private static Number OADateUnixOffsetDays => 25569d;
	private static Number MillisecondsPerDay => 86400000d;
	private static Number DateTimeKindUnspecified => 0;
	private static Number DateTimeKindUtc => 1;
	private static Number DateTimeKindLocal => 2;
	private static Number DateTimeStylesNoCurrentDateDefault => 8;
	private static Number DateTimeStylesAdjustToUniversal => 16;
	private static Number DateTimeStylesAssumeLocal => 32;
	private static Number DateTimeStylesAssumeUniversal => 64;
	private static Number DateTimeStylesRoundtripKind => 128;

	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
			throw new Error(message);
	}

	private static RuntimeModule.JDateTime CreateDefaultDateTime()
		=> new(CreateLocalDate(1, 1, 1), DateTimeKindUnspecified);

	private static Date CreateLocalDate(Number year, Number month, Number day)
		=> RuntimeModule.CreateLocalDate(year, month, day);

	private static Date CreateLocalDateTime(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
		=> RuntimeModule.CreateLocalDateTime(year, month, day, hour, minute, second, millisecond);

	private static RuntimeModule.JDateTime CreateFromTicks(BigInt ticks)
		=> CreateFromTicks(ticks, DateTimeKindUnspecified);

	private static RuntimeModule.JDateTime CreateFromTicks(BigInt ticks, Number kind)
	{
		if (ticks < ZeroTicks || ticks > MaxDateTimeTicks)
			throw new Error("ArgumentOutOfRangeException: Ticks must be within the range of DateTime.");

		var ticksSinceUnixEpoch = ticks - UnixEpochTicks;
		var milliseconds = ticksSinceUnixEpoch / TicksPerMillisecond;
		var subMillisecondTicks = ticksSinceUnixEpoch % TicksPerMillisecond;
		if (subMillisecondTicks < ZeroTicks)
		{
			milliseconds -= BigIntFn(1);
			subMillisecondTicks += TicksPerMillisecond;
		}

		var utc = new Date(NumberFn(milliseconds));
		return new RuntimeModule.JDateTime(
			CreateLocalDateTime(
				utc.GetUTCFullYear(),
				utc.GetUTCMonth() + 1,
				utc.GetUTCDate(),
				utc.GetUTCHours(),
				utc.GetUTCMinutes(),
				utc.GetUTCSeconds(),
				utc.GetUTCMilliseconds()),
			kind,
			subMillisecondTicks);
	}

	private static RuntimeModule.JDateTime CreateDateTime(Date date, Number kind)
		=> new(date, kind);

	private static RuntimeModule.JDateTime CreateDateTime(Date date, Number kind, BigInt subMillisecondTicks)
		=> new(date, kind, subMillisecondTicks);

	private static RuntimeModule.JDateTime CreateFromInstantTicks(BigInt ticks, Number kind)
	{
		if (ticks < ZeroTicks || ticks > MaxDateTimeTicks)
			throw new Error("ArgumentOutOfRangeException: Ticks must be within the range of DateTime.");

		if (kind == DateTimeKindUtc)
			return CreateFromTicks(ticks, kind);

		var ticksSinceUnixEpoch = ticks - UnixEpochTicks;
		var milliseconds = ticksSinceUnixEpoch / TicksPerMillisecond;
		var subMillisecondTicks = ticksSinceUnixEpoch % TicksPerMillisecond;
		if (subMillisecondTicks < ZeroTicks)
		{
			milliseconds -= BigIntFn(1);
			subMillisecondTicks += TicksPerMillisecond;
		}

		return new RuntimeModule.JDateTime(new Date(NumberFn(milliseconds)), kind, subMillisecondTicks);
	}

	private static Number GetKind(System.DateTimeKind kind)
	{
		var value = NumberFn((int)kind);
		if (value != DateTimeKindUnspecified && value != DateTimeKindUtc && value != DateTimeKindLocal)
			throw new Error("ArgumentException: Invalid DateTimeKind value.");

		return value;
	}

	private static BigInt GetMicrosecondTicks(Number microsecond)
	{
		if (Math.FloorFn(microsecond) != microsecond || microsecond < 0 || microsecond > 999)
			throw new Error("ArgumentOutOfRangeException: Microsecond must be between 0 and 999.");

		return BigIntFn(microsecond) * TicksPerMicrosecond;
	}

	private static BigInt GetTicks(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		var milliseconds = Date.UTC(
			date.GetFullYear(),
			date.GetMonth(),
			date.GetDate(),
			date.GetHours(),
			date.GetMinutes(),
			date.GetSeconds(),
			date.GetMilliseconds());
		return BigIntFn(milliseconds) * TicksPerMillisecond + instance.SubMillisecondTicks + UnixEpochTicks;
	}

	private static BigInt GetTicks(Date date)
	{
		var milliseconds = Date.UTC(
			date.GetFullYear(),
			date.GetMonth(),
			date.GetDate(),
			date.GetHours(),
			date.GetMinutes(),
			date.GetSeconds(),
			date.GetMilliseconds());
		return BigIntFn(milliseconds) * TicksPerMillisecond + UnixEpochTicks;
	}

	private static BigInt GetInstantTicks(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindUtc)
			return GetTicks(instance);

		return BigIntFn(instance.Date.GetTime()) * TicksPerMillisecond + instance.SubMillisecondTicks + UnixEpochTicks;
	}

	private static RuntimeModule.JDateTime CreateUtcNow()
	{
		var now = new Date();
		return new RuntimeModule.JDateTime(
			CreateLocalDateTime(
				now.GetUTCFullYear(),
				now.GetUTCMonth() + 1,
				now.GetUTCDate(),
				now.GetUTCHours(),
				now.GetUTCMinutes(),
				now.GetUTCSeconds(),
				now.GetUTCMilliseconds()),
			DateTimeKindUtc);
	}

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
			return date.GetHours() < 12 ? "AM" : "PM";

		var parts = new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Hour: Intl.NumericTwoDigit.Numeric,
				Hour12: true)).FormatToParts(date);
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i]!;
			if (part.Type == "dayPeriod")
				return part.Value;
		}

		return date.GetHours() < 12 ? "AM" : "PM";
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

	private static string GetRoundtripSuffix(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindUtc)
			return "Z";
		if (instance.Kind == DateTimeKindLocal)
			return FormatOffsetTicks(BigIntFn(-instance.Date.GetTimezoneOffset()) * OffsetMinuteTicks, 3);

		return "";
	}

	private static string FormatInvariantGeneralDateTime(RuntimeModule.JDateTime instance, bool includeSeconds)
	{
		var date = instance.Date;
		var text = RuntimeModule.Pad2(date.GetMonth() + 1)
			+ "/"
			+ RuntimeModule.Pad2(date.GetDate())
			+ "/"
			+ RuntimeModule.PadLeft(date.GetFullYear().ToString()!, 4)
			+ " "
			+ RuntimeModule.Pad2(date.GetHours())
			+ ":"
			+ RuntimeModule.Pad2(date.GetMinutes());
		if (includeSeconds)
			text += ":" + RuntimeModule.Pad2(date.GetSeconds());

		return text;
	}

	private static string FormatInvariantShortDate(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		return RuntimeModule.Pad2(date.GetMonth() + 1)
			+ "/"
			+ RuntimeModule.Pad2(date.GetDate())
			+ "/"
			+ RuntimeModule.PadLeft(date.GetFullYear().ToString()!, 4);
	}

	private static string FormatInvariantLongDate(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		return GetInvariantDayName(date.GetDay())
			+ ", "
			+ RuntimeModule.Pad2(date.GetDate())
			+ " "
			+ GetInvariantMonthName(date.GetMonth() + 1)
			+ " "
			+ RuntimeModule.PadLeft(date.GetFullYear().ToString()!, 4);
	}

	private static string FormatInvariantTime(RuntimeModule.JDateTime instance, bool includeSeconds)
	{
		var date = instance.Date;
		var text = RuntimeModule.Pad2(date.GetHours())
			+ ":"
			+ RuntimeModule.Pad2(date.GetMinutes());
		if (includeSeconds)
			text += ":" + RuntimeModule.Pad2(date.GetSeconds());

		return text;
	}

	private static string FormatMonthDay(RuntimeModule.JDateTime instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return GetInvariantMonthName(instance.Date.GetMonth() + 1) + " " + RuntimeModule.Pad2(instance.Date.GetDate());

		return FormatLocaleDateTime(
			instance.Date,
			locale,
			new Intl.DateTimeFormatOptions(
				Month: Intl.LongShortNarrow.Long,
				Day: Intl.NumericTwoDigit.TwoDigit));
	}

	private static string FormatYearMonth(RuntimeModule.JDateTime instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return RuntimeModule.PadLeft(instance.Date.GetFullYear().ToString()!, 4) + " " + GetInvariantMonthName(instance.Date.GetMonth() + 1);

		return FormatLocaleDateTime(
			instance.Date,
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.LongShortNarrow.Long));
	}

	private static string FormatFullDateTime(RuntimeModule.JDateTime instance, bool includeSeconds, object? provider)
		=> FormatLongDate(instance, provider) + " " + FormatTime(instance, includeSeconds, provider);

	private static Date GetUniversalDateTimeForFormatting(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		if (instance.Kind == DateTimeKindUtc)
		{
			return new Date(Date.UTC(
				date.GetFullYear(),
				date.GetMonth(),
				date.GetDate(),
				date.GetHours(),
				date.GetMinutes(),
				date.GetSeconds(),
				date.GetMilliseconds()));
		}

		return new Date(date.GetTime());
	}

	private static string FormatUniversalFullDateTime(RuntimeModule.JDateTime instance, object? provider)
	{
		var utc = GetUniversalDateTimeForFormatting(instance);
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
		{
			return GetInvariantDayName(utc.GetUTCDay())
				+ ", "
				+ RuntimeModule.Pad2(utc.GetUTCDate())
				+ " "
				+ GetInvariantMonthName(utc.GetUTCMonth() + 1)
				+ " "
				+ RuntimeModule.PadLeft(utc.GetUTCFullYear().ToString()!, 4)
				+ " "
				+ RuntimeModule.Pad2(utc.GetUTCHours())
				+ ":"
				+ RuntimeModule.Pad2(utc.GetUTCMinutes())
				+ ":"
				+ RuntimeModule.Pad2(utc.GetUTCSeconds());
		}

		return JoinFormatParts(new Intl.DateTimeFormat(
			locale,
			new Intl.DateTimeFormatOptions(
				Weekday: Intl.LongShortNarrow.Long,
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.LongShortNarrow.Long,
				Day: Intl.NumericTwoDigit.TwoDigit,
				Hour: Intl.NumericTwoDigit.TwoDigit,
				Minute: Intl.NumericTwoDigit.TwoDigit,
				Second: Intl.NumericTwoDigit.TwoDigit,
				Hour12: false,
				TimeZone: "UTC")).FormatToParts(utc));
	}

	private static string FormatRfc1123DateTime(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		return GetInvariantAbbreviatedDayName(date.GetDay())
			+ ", "
			+ RuntimeModule.Pad2(date.GetDate())
			+ " "
			+ GetInvariantAbbreviatedMonthName(date.GetMonth() + 1)
			+ " "
			+ RuntimeModule.PadLeft(date.GetFullYear().ToString()!, 4)
			+ " "
			+ RuntimeModule.Pad2(date.GetHours())
			+ ":"
			+ RuntimeModule.Pad2(date.GetMinutes())
			+ ":"
			+ RuntimeModule.Pad2(date.GetSeconds())
			+ " GMT";
	}

	private static string FormatLocaleDateTime(Date date, string locale, Intl.DateTimeFormatOptions options)
		=> JoinFormatParts(new Intl.DateTimeFormat(locale, options).FormatToParts(date));

	private static string FormatGeneralDateTime(RuntimeModule.JDateTime instance, bool includeSeconds, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantGeneralDateTime(instance, includeSeconds);

		return FormatLocaleDateTime(
			instance.Date,
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.NumericTwoDigit.TwoDigit,
				Day: Intl.NumericTwoDigit.TwoDigit,
				Hour: Intl.NumericTwoDigit.TwoDigit,
				Minute: Intl.NumericTwoDigit.TwoDigit,
				Second: includeSeconds ? Intl.NumericTwoDigit.TwoDigit : null,
				Hour12: false));
	}

	private static string FormatShortDate(RuntimeModule.JDateTime instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantShortDate(instance);

		return FormatLocaleDateTime(
			instance.Date,
			locale,
			new Intl.DateTimeFormatOptions(
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.NumericTwoDigit.TwoDigit,
				Day: Intl.NumericTwoDigit.TwoDigit));
	}

	private static string FormatLongDate(RuntimeModule.JDateTime instance, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantLongDate(instance);

		return FormatLocaleDateTime(
			instance.Date,
			locale,
			new Intl.DateTimeFormatOptions(
				Weekday: Intl.LongShortNarrow.Long,
				Year: Intl.NumericTwoDigit.Numeric,
				Month: Intl.LongShortNarrow.Long,
				Day: Intl.NumericTwoDigit.TwoDigit));
	}

	private static string FormatTime(RuntimeModule.JDateTime instance, bool includeSeconds, object? provider)
	{
		var locale = GetProviderLocale(provider);
		if (locale.Length == 0)
			return FormatInvariantTime(instance, includeSeconds);

		return FormatLocaleDateTime(
			instance.Date,
			locale,
			new Intl.DateTimeFormatOptions(
				Hour: Intl.NumericTwoDigit.TwoDigit,
				Minute: Intl.NumericTwoDigit.TwoDigit,
				Second: includeSeconds ? Intl.NumericTwoDigit.TwoDigit : null,
				Hour12: false));
	}

	private static string FormatRoundtripDateTime(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		return RuntimeModule.FormatDateOnlyText(date.GetFullYear(), date.GetMonth() + 1, date.GetDate())
			+ "T"
			+ RuntimeModule.Pad2(date.GetHours())
			+ ":"
			+ RuntimeModule.Pad2(date.GetMinutes())
			+ ":"
			+ RuntimeModule.Pad2(date.GetSeconds())
			+ "."
			+ RuntimeModule.Pad7(BigIntFn(date.GetMilliseconds()) * TicksPerMillisecond + instance.SubMillisecondTicks)
			+ GetRoundtripSuffix(instance);
	}

	private static string FormatSortableDateTime(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		return RuntimeModule.FormatDateOnlyText(date.GetFullYear(), date.GetMonth() + 1, date.GetDate())
			+ "T"
			+ RuntimeModule.Pad2(date.GetHours())
			+ ":"
			+ RuntimeModule.Pad2(date.GetMinutes())
			+ ":"
			+ RuntimeModule.Pad2(date.GetSeconds());
	}

	private static string FormatUniversalSortableDateTime(RuntimeModule.JDateTime instance)
		=> FormatSortableDateTime(instance).Replace("T", " ") + "Z";

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

	private static string FormatCustomToken(RuntimeModule.JDateTime instance, char token, int count, string locale, string dateSeparator, string timeSeparator)
	{
		var date = instance.Date;
		var year = date.GetFullYear();
		var month = date.GetMonth() + 1;
		var day = date.GetDate();
		var hour = date.GetHours();
		var hour12 = hour % 12;
		if (hour12 == 0)
			hour12 = 12;
		var minute = date.GetMinutes();
		var second = date.GetSeconds();
		var fraction = BigIntFn(date.GetMilliseconds()) * TicksPerMillisecond + instance.SubMillisecondTicks;
		var offset = instance.Kind == DateTimeKindLocal
			? BigIntFn(-date.GetTimezoneOffset()) * OffsetMinuteTicks
			: BigInt.Zero;
		var suffix = GetRoundtripSuffix(instance);

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
					return GetLocalizedDayName(locale, date.GetDay(), true);
				return GetLocalizedDayName(locale, date.GetDay(), false);
			case 'H':
				return count == 1 ? hour.ToString()! : RuntimeModule.Pad2(hour);
			case 'h':
				return count == 1 ? hour12.ToString()! : RuntimeModule.Pad2(hour12);
			case 'm':
				return count == 1 ? minute.ToString()! : RuntimeModule.Pad2(minute);
			case 's':
				return count == 1 ? second.ToString()! : RuntimeModule.Pad2(second);
			case 't':
				var dayPeriod = GetLocalizedDayPeriod(date, locale);
				return count == 1
					? dayPeriod.Substring(0, 1)
					: dayPeriod;
			case 'f':
				return FormatFraction(fraction, count, false);
			case 'F':
				return FormatFraction(fraction, count, true);
			case 'z':
				return instance.Kind == DateTimeKindLocal ? FormatOffsetTicks(offset, count) : "";
			case 'K':
				return suffix;
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

	private static string FormatCustomDateTime(RuntimeModule.JDateTime instance, string format, object? provider)
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

	private static string FormatDateTime(RuntimeModule.JDateTime instance, string? format, object? provider)
	{
		if (format == null || format.Length == 0)
			return FormatGeneralDateTime(instance, true, provider);

		if (format.Length == 1)
		{
			switch (format[0])
			{
				case 'f':
					return FormatFullDateTime(instance, false, provider);
				case 'F':
					return FormatFullDateTime(instance, true, provider);
				case 'O':
				case 'o':
					return FormatRoundtripDateTime(instance);
				case 'G':
					return FormatGeneralDateTime(instance, true, provider);
				case 'g':
					return FormatGeneralDateTime(instance, false, provider);
				case 'M':
				case 'm':
					return FormatMonthDay(instance, provider);
				case 'R':
				case 'r':
					return FormatRfc1123DateTime(instance);
				case 'd':
					return FormatShortDate(instance, provider);
				case 'D':
					return FormatLongDate(instance, provider);
				case 't':
					return FormatTime(instance, false, provider);
				case 'T':
					return FormatTime(instance, true, provider);
				case 's':
					return FormatSortableDateTime(instance);
				case 'u':
					return FormatUniversalSortableDateTime(instance);
				case 'U':
					return FormatUniversalFullDateTime(instance, provider);
				case 'Y':
				case 'y':
					return FormatYearMonth(instance, provider);
				default:
					if (IsAsciiLetter(format[0]))
						throw new Error("FormatException: Input string was not in a correct format.");
					break;
			}
		}

		return FormatCustomDateTime(instance, format, provider);
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

	private static BigInt CreateUtcDateTimeTicks(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
	{
		var utc = RuntimeModule.CreateUtcDate(year, month, day);
		utc.SetUTCHours(hour, minute, second, millisecond);
		return BigIntFn(utc.GetTime()) * TicksPerMillisecond + UnixEpochTicks;
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
		out Number kind,
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
		kind = DateTimeKindUnspecified;
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

		if (index == text.Length - 1 && (text[index] == 'Z' || text[index] == 'z'))
		{
			kind = DateTimeKindUtc;
			return true;
		}

		var sign = text[index];
		if (sign != '+' && sign != '-')
			return false;

		kind = DateTimeKindLocal;
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

		offsetTicks = BigIntFn(offsetHours * 60 + offsetMinutes) * BigIntFn("600000000");
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
		out Number kind,
		out BigInt offsetTicks)
	{
		hour = 0;
		minute = 0;
		second = 0;
		millisecond = 0;
		subMillisecondTicks = ZeroTicks;
		kind = DateTimeKindUnspecified;
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

		if (index == text.Length - 1 && (text[index] == 'Z' || text[index] == 'z'))
		{
			kind = DateTimeKindLocal;
			return true;
		}

		var sign = text[index];
		if (sign != '+' && sign != '-')
			return false;

		kind = DateTimeKindLocal;
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

		offsetTicks = BigIntFn(offsetHours * 60 + offsetMinutes) * BigIntFn("600000000");
		if (sign == '-')
			offsetTicks = -offsetTicks;

		return true;
	}

	private static BigInt CreateRoundedTicksFromDouble(Number value)
	{
		if (DoubleModule._24e14b276e0c7e30(value))
			throw new Error("ArgumentException: Value cannot be NaN.");

		if (!DoubleModule._aed2927097617729(value))
			throw new Error("ArgumentOutOfRangeException: Value must be finite.");

		var rounded = Math.RoundFn(value);
		if (!DoubleModule._aed2927097617729(rounded))
			throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTime range.");

		return BigIntFn(rounded);
	}

	private static BigInt CreateAddUnitTicks(Number value, BigInt ticksPerUnit)
	{
		if (DoubleModule._24e14b276e0c7e30(value))
			throw new Error("ArgumentException: Value cannot be NaN.");

		if (!DoubleModule._aed2927097617729(value))
			throw new Error("ArgumentOutOfRangeException: Value must be finite.");

		var maxUnitCount = NumberFn(MaxDateTimeTicks) / NumberFn(ticksPerUnit);
		if (Math.AbsFn(value) > maxUnitCount)
			throw new Error("ArgumentOutOfRangeException: Value is outside the supported DateTime range.");

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

		var hasRoundtripKind = (styles & DateTimeStylesRoundtripKind) != 0;
		var hasAdjustToUniversal = (styles & DateTimeStylesAdjustToUniversal) != 0;
		var hasAssumeLocal = (styles & DateTimeStylesAssumeLocal) != 0;
		var hasAssumeUniversal = (styles & DateTimeStylesAssumeUniversal) != 0;
		if (hasRoundtripKind && (hasAdjustToUniversal || hasAssumeLocal || hasAssumeUniversal))
			throw new Error("ArgumentException: RoundtripKind cannot be combined with AssumeLocal, AssumeUniversal, or AdjustToUniversal.");
		if (hasAssumeLocal && hasAssumeUniversal)
			throw new Error("ArgumentException: AssumeLocal and AssumeUniversal cannot both be set.");
	}

	private static RuntimeModule.JDateTime ApplyDateTimeStyles(RuntimeModule.JDateTime value, string input, object styles)
	{
		var styleValue = GetDateTimeStylesValue(styles);
		ValidateDateTimeStyles(styleValue);

		var text = input.Trim();
		var hasUtcSuffix = HasUtcSuffix(text);
		var hasExplicitOffset = HasExplicitOffset(text);
		var hasExplicitZone = hasUtcSuffix || hasExplicitOffset;
		var noCurrentDateDefault = (styleValue & DateTimeStylesNoCurrentDateDefault) != 0;
		var adjustToUniversal = (styleValue & DateTimeStylesAdjustToUniversal) != 0;
		var assumeLocal = (styleValue & DateTimeStylesAssumeLocal) != 0;
		var assumeUniversal = (styleValue & DateTimeStylesAssumeUniversal) != 0;
		var roundtripKind = (styleValue & DateTimeStylesRoundtripKind) != 0;

		if (noCurrentDateDefault && TryParseTimeOnly(text, out var hour, out var minute, out var second, out var millisecond, out var timeOnlySubTicks, out var timeOnlyKind, out var timeOnlyOffsetTicks))
		{
			if (timeOnlyKind == DateTimeKindUnspecified)
			{
				value = CreateDateTime(CreateLocalDateTime(1, 1, 1, hour, minute, second, millisecond), DateTimeKindUnspecified, timeOnlySubTicks);
			}
			else
			{
				var utcTicks = CreateUtcDateTimeTicks(1, 1, 1, hour, minute, second, millisecond) + timeOnlySubTicks - timeOnlyOffsetTicks;
				value = CreateFromInstantTicks(utcTicks, DateTimeKindLocal);
			}
		}

		if (hasExplicitZone)
		{
			if (adjustToUniversal || (roundtripKind && hasUtcSuffix))
				return CreateFromInstantTicks(GetInstantTicks(value), DateTimeKindUtc);

			return value;
		}

		if (value.Kind != DateTimeKindUnspecified)
			return value;

		if (assumeUniversal)
		{
			var assumedUtcTicks = GetTicks(value);
			if (adjustToUniversal)
				return CreateFromTicks(assumedUtcTicks, DateTimeKindUtc);

			return CreateFromInstantTicks(assumedUtcTicks, DateTimeKindLocal);
		}

		if (assumeLocal)
		{
			if (adjustToUniversal)
				return CreateFromInstantTicks(GetInstantTicks(value), DateTimeKindUtc);

			return CreateDateTime(value.Date, DateTimeKindLocal, value.SubMillisecondTicks);
		}

		return value;
	}

	private static RuntimeModule.JDateTime AddMonthsCore(RuntimeModule.JDateTime instance, Number months)
	{
		EnsureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");

		var year = instance.Date.GetFullYear();
		var monthIndex = (year - 1) * 12 + instance.Date.GetMonth() + months;
		var newYear = Math.FloorFn(monthIndex / 12) + 1;
		var newMonthIndex = monthIndex % 12;
		if (newMonthIndex < 0)
			newMonthIndex += 12;

		var newMonth = newMonthIndex + 1;
		var day = instance.Date.GetDate();
		var daysInMonth = RuntimeModule.GetDaysInMonth(newYear, newMonth);
		var newDay = day > daysInMonth ? daysInMonth : day;
		return CreateDateTime(
			CreateLocalDateTime(
				newYear,
				newMonth,
				newDay,
				instance.Date.GetHours(),
				instance.Date.GetMinutes(),
				instance.Date.GetSeconds(),
				instance.Date.GetMilliseconds()),
			instance.Kind,
			instance.SubMillisecondTicks);
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
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTime.");

		while (digits.Length < 7)
			digits += "0";

		return BigIntFn(digits.Substring(3, 4));
	}

	private static RuntimeModule.JDateTime ParseCore(string input)
	{
		var s = input.Trim();
		if (s.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid DateTime.");

		if (TryParseTimeOnly(s, out var timeHour, out var timeMinute, out var timeSecond, out var timeMillisecond, out var timeSubMillisecondTicks, out var timeKind, out var timeOffsetTicks))
		{
			var now = new Date();
			var currentYear = now.GetFullYear();
			var currentMonth = now.GetMonth() + 1;
			var currentDay = now.GetDate();
			if (timeKind == DateTimeKindUnspecified)
				return new RuntimeModule.JDateTime(CreateLocalDateTime(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond), DateTimeKindUnspecified, timeSubMillisecondTicks);

			// .NET DateTime.Parse 默认会把显式时区输入转换到本地时间，Kind 也落到 Local。
			// 只有配合 RoundtripKind 时，ApplyDateTimeStyles 才会把 "Z" 保留成 Utc。
			var utcTicks = CreateUtcDateTimeTicks(currentYear, currentMonth, currentDay, timeHour, timeMinute, timeSecond, timeMillisecond) + timeSubMillisecondTicks - timeOffsetTicks;
			return CreateFromInstantTicks(utcTicks, DateTimeKindLocal);
		}

		if (TryParseIsoDate(s, out var year, out var month, out var day))
			return new RuntimeModule.JDateTime(CreateLocalDate(year, month, day), DateTimeKindUnspecified);

		if (TryParseIsoDateTime(s, out year, out month, out day, out var hour, out var minute, out var second, out var millisecond, out var subMillisecondTicks, out var kind, out var offsetTicks))
		{
			if (kind == DateTimeKindUnspecified)
				return new RuntimeModule.JDateTime(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), DateTimeKindUnspecified, subMillisecondTicks);

			// 带显式时区的输入同样先归一到本地 DateTime，后续样式再决定是否提升为 Utc。
			var utcTicks = CreateUtcDateTimeTicks(year, month, day, hour, minute, second, millisecond) + subMillisecondTicks - offsetTicks;
			return CreateFromInstantTicks(utcTicks, DateTimeKindLocal);
		}

		var parsed = new Date(s);
		if (IsNaN(parsed.GetTime()))
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTime.");

		var parsedSubMillisecondTicks = ExtractSubMillisecondTicks(s);

		if (HasUtcSuffix(s))
			// 与 .NET 一致：DateTime.Parse(..., DateTimeStyles.None) 遇到 "Z" 先变成本地时间。
			return CreateFromInstantTicks(BigIntFn(parsed.GetTime()) * TicksPerMillisecond + parsedSubMillisecondTicks + UnixEpochTicks, DateTimeKindLocal);

		if (HasExplicitOffset(s))
			return new RuntimeModule.JDateTime(new Date(parsed.GetTime()), DateTimeKindLocal, parsedSubMillisecondTicks);

		return new RuntimeModule.JDateTime(parsed, DateTimeKindUnspecified, parsedSubMillisecondTicks);
	}

	/// <summary>
	/// C#: DateTime.MinValue
	/// JS: wrapper for 0001-01-01 00:00:00
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTime.MinValue")]
	public static RuntimeModule.JDateTime _fad0c74e1c9df5bb() => new(CreateLocalDate(1, 1, 1), DateTimeKindUnspecified);

	/// <summary>
	/// C#: DateTime.MaxValue
	/// JS: wrapper for 9999-12-31 23:59:59.999
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTime.MaxValue")]
	public static RuntimeModule.JDateTime _eb38dc04224730ea() => CreateDateTime(CreateLocalDateTime(9999, 12, 31, 23, 59, 59, 999), DateTimeKindUnspecified, BigIntFn("9999"));

	/// <summary>
	/// C#: DateTime.UnixEpoch
	/// JS: new Date(0)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTime.UnixEpoch")]
	public static RuntimeModule.JDateTime _878591efc9a51388() => CreateFromTicks(UnixEpochTicks, DateTimeKindUtc);

	[Jazor(Op.Import ,"System.DateTime.DateTime()")]
	public static RuntimeModule.JDateTime _bfa8ee5dd46e2005() => new(CreateLocalDate(1, 1, 1), DateTimeKindUnspecified);

	/// <summary>
	/// C#: new DateTime(ticks)
	/// JS: new Date(Number((ticks - 621355968000000000n) / 10000n))
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(long)")]
	public static RuntimeModule.JDateTime _1ba9ed95dd0eab48(BigInt ticks)
		=> CreateFromTicks(ticks, DateTimeKindUnspecified);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks and to Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(long, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _eda1c8bf8e1e617b(BigInt ticks, System.DateTimeKind kind)
		=> CreateFromTicks(ticks, GetKind(kind));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />. The new instance will have the <see cref="F:System.DateTimeKind.Unspecified" /> kind.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(System.DateOnly, System.TimeOnly)")]
	public static RuntimeModule.JDateTime _4fef4795bcbef97f(RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time)
	{
		var milliseconds = NumberFn(time.Ticks / TicksPerMillisecond);
		var subMillisecondTicks = time.Ticks % TicksPerMillisecond;
		var hour = Math.FloorFn(milliseconds / 3600000);
		var minute = Math.FloorFn(milliseconds / 60000) % 60;
		var second = Math.FloorFn(milliseconds / 1000) % 60;
		var millisecond = milliseconds % 1000;
		return CreateDateTime(CreateLocalDateTime(date.Year, date.Month, date.Day, hour, minute, second, millisecond), DateTimeKindUnspecified, subMillisecondTicks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" /> and respecting the specified <see cref="T:System.DateTimeKind" />.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _85602323793168a5(RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time, System.DateTimeKind kind)
	{
		var result = _4fef4795bcbef97f(date, time);
		return CreateDateTime(result.Date, GetKind(kind), result.SubMillisecondTicks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day.</summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(int, int, int)")]
	public static RuntimeModule.JDateTime _4cb33a818161a3e1(Number year, Number month, Number day) => new(CreateLocalDate(year, month, day), DateTimeKindUnspecified);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _a515b8bb82ad96b7(Number year, Number month, Number day, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)")]
	public extern static RuntimeModule.JDateTime _bd2c430e6327a2cc(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, System.DateTimeKind kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second.</summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _4903723bbf8a0a2f(Number year, Number month, Number day, Number hour, Number minute, Number second)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, 0));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _f83be88cfb3fbce0(Number year, Number month, Number day, Number hour, Number minute, Number second, System.DateTimeKind kind)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, 0), GetKind(kind));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _29bb943b21806bd9(Number year, Number month, Number day, Number hour, Number minute, Number second, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond.</summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(int, int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _5822b271bb635d64(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), DateTimeKindUnspecified);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _c52eec5e681a0b8b(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, System.DateTimeKind kind)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), GetKind(kind));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _8a4d2d51b716bb36(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _9117d26d23769ad1(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond)
		=> CreateDateTime(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), DateTimeKindUnspecified, GetMicrosecondTicks(microsecond));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _e84671346e2b9972(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, System.DateTimeKind kind)
		=> CreateDateTime(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), GetKind(kind), GetMicrosecondTicks(microsecond));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _bd13792ce57e1964(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)")]
	public extern static RuntimeModule.JDateTime _cd0b8f2bce1e09ed(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, System.DateTimeKind kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the value of the specified <see cref="T:System.TimeSpan" /> to the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.Add(System.TimeSpan)")]
	public static RuntimeModule.JDateTime _34a77be7365c459f(RuntimeModule.JDateTime instance, RuntimeModule.JTimeSpan value)
		=> CreateFromTicks(GetTicks(instance) + value.Ticks, instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of days to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddDays(double)")]
	public static RuntimeModule.JDateTime _558a3f189d9149d7(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + CreateAddUnitTicks(value, TicksPerDay), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of hours to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddHours(double)")]
	public static RuntimeModule.JDateTime _101af978213c19c5(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + CreateAddUnitTicks(value, TicksPerHour), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of milliseconds to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMilliseconds(double)")]
	public static RuntimeModule.JDateTime _2b29e4c11fa12daa(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + CreateAddUnitTicks(value, TicksPerMillisecond), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of microseconds to the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.AddMicroseconds(double)")]
	public static RuntimeModule.JDateTime _2b47368c73a3e1f2(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + CreateAddUnitTicks(value, TicksPerMicrosecond), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of minutes to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMinutes(double)")]
	public static RuntimeModule.JDateTime _8bdc25943cf2d39b(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + CreateAddUnitTicks(value, TicksPerMinute), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of months to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMonths(int)")]
	public static RuntimeModule.JDateTime _aae197b95f9024a4(RuntimeModule.JDateTime instance, Number months)
		=> AddMonthsCore(instance, months);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of seconds to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddSeconds(double)")]
	public static RuntimeModule.JDateTime _57045f93edac1460(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + CreateAddUnitTicks(value, TicksPerSecond), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of ticks to the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.AddTicks(long)")]
	public static RuntimeModule.JDateTime _d2e74845b174a889(RuntimeModule.JDateTime instance, BigInt value)
		=> CreateFromTicks(GetTicks(instance) + value, instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of years to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddYears(int)")]
	public static RuntimeModule.JDateTime _3353d31b02f2bed8(RuntimeModule.JDateTime instance, Number value)
	{
		EnsureWholeNumber(value, "ArgumentOutOfRangeException: Years value must be a whole number.");
		return AddMonthsCore(instance, value * 12);
	}

	///<summary>Compares two instances of <see cref="T:System.DateTime" /> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.</summary>
	[Jazor(Op.Import, "static System.DateTime.Compare(System.DateTime, System.DateTime)")]
	public static Number _0edfd00dcc8d70d0(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
	{
		var ticks1 = GetTicks(t1);
		var ticks2 = GetTicks(t2);
		if (ticks1 < ticks2) return -1;
		if (ticks1 > ticks2) return 1;
		return 0;
	}

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateTime" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Import ,"System.DateTime.CompareTo(object)")]
	public static Number _f7b2337bfa9864d9(RuntimeModule.JDateTime instance, object? value)
	{
		if (value == null)
			return 1;

		var other = value as RuntimeModule.JDateTime;
		if (other == null)
			throw new Error("ArgumentException: Object must be of type DateTime.");

		return _40c6426fdc505e97(instance, other);
	}

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateTime" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Import, "System.DateTime.CompareTo(System.DateTime)")]
	public static Number _40c6426fdc505e97(RuntimeModule.JDateTime instance, RuntimeModule.JDateTime value)
	{
		var ticks = GetTicks(instance);
		var otherTicks = GetTicks(value);
		if (ticks < otherTicks) return -1;
		if (ticks > otherTicks) return 1;
		return 0;
	}

	///<summary>Returns the number of days in the specified month and year.</summary>
	[Jazor(Op.Import, "static System.DateTime.DaysInMonth(int, int)")]
	public static Number _38ef7423971afb7f(Number year, Number month)
		=> RuntimeModule.GetDaysInMonth(year, month);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Import, "override System.DateTime.Equals(object)")]
	public static bool _f6903c1af8944917(RuntimeModule.JDateTime instance, object? value)
	{
		var other = value as RuntimeModule.JDateTime;
		return other != null && GetTicks(instance) == GetTicks(other);
	}

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateTime" /> instance.</summary>
	[Jazor(Op.Import, "System.DateTime.Equals(System.DateTime)")]
	public static bool _c29ca32a998c517c(RuntimeModule.JDateTime instance, RuntimeModule.JDateTime value)
		=> GetTicks(instance) == GetTicks(value);

	///<summary>Returns a value indicating whether two <see cref="T:System.DateTime" /> instances  have the same date and time value.</summary>
	[Jazor(Op.Import, "static System.DateTime.Equals(System.DateTime, System.DateTime)")]
	public static bool _4937ff8bec81ddea(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) == GetTicks(t2);

	///<summary>Deserializes a 64-bit binary value and recreates an original serialized <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromBinary(long)")]
	public static RuntimeModule.JDateTime _f437fad61f0046c7(BigInt dateData)
	{
		var unsignedData = dateData < ZeroTicks ? dateData + BinaryUnsignedOverflow : dateData;
		var kindBits = unsignedData & BinaryKindMask;
		var ticks = unsignedData & BinaryTicksMask;
		if (kindBits == BinaryLocalMask || kindBits == BinaryKindMask)
			return CreateFromInstantTicks(ticks, DateTimeKindLocal);
		if (kindBits == BinaryKindShift)
			return CreateFromTicks(ticks, DateTimeKindUtc);

		return CreateFromTicks(ticks, DateTimeKindUnspecified);
	}

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromFileTime(long)")]
	public static RuntimeModule.JDateTime _df025c273bde0e50(BigInt fileTime)
	{
		if (fileTime < ZeroTicks)
			throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");

		return CreateFromInstantTicks(fileTime - FileTimeUnixEpochTicks + UnixEpochTicks, DateTimeKindLocal);
	}

	///<summary>Converts the specified Windows file time to an equivalent UTC time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromFileTimeUtc(long)")]
	public static RuntimeModule.JDateTime _93886aebedb72920(BigInt fileTime)
	{
		if (fileTime < ZeroTicks)
			throw new Error("ArgumentOutOfRangeException: File time must be non-negative.");

		return CreateFromTicks(fileTime - FileTimeUnixEpochTicks + UnixEpochTicks, DateTimeKindUtc);
	}

	///<summary>Returns a <see cref="T:System.DateTime" /> equivalent to the specified OLE Automation Date.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromOADate(double)")]
	public static RuntimeModule.JDateTime _12520a637fb85a70(Number d)
		=> CreateFromTicks(CreateRoundedTicksFromDouble((d - OADateUnixOffsetDays) * MillisecondsPerDay) * TicksPerMillisecond + UnixEpochTicks, DateTimeKindUnspecified);

	///<summary>Indicates whether this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the current time zone.</summary>
	[Jazor(Op.Import ,"System.DateTime.IsDaylightSavingTime()")]
	public static bool _d3b1cc7e750c6bc3(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindUtc)
			return false;

		var year = instance.Date.GetFullYear();
		var januaryOffset = CreateLocalDate(year, 1, 1).GetTimezoneOffset();
		var julyOffset = CreateLocalDate(year, 7, 1).GetTimezoneOffset();
		var standardOffset = januaryOffset > julyOffset ? januaryOffset : julyOffset;
		return instance.Date.GetTimezoneOffset() < standardOffset;
	}

	///<summary>Creates a new <see cref="T:System.DateTime" /> object that has the same number of ticks as the specified <see cref="T:System.DateTime" />, but is designated as either local time, Coordinated Universal Time (UTC), or neither, as indicated by the specified <see cref="T:System.DateTimeKind" /> value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _a99826a92073614e(RuntimeModule.JDateTime value, System.DateTimeKind kind)
		=> CreateDateTime(value.Date, GetKind(kind), value.SubMillisecondTicks);

	///<summary>Serializes the current <see cref="T:System.DateTime" /> object to a 64-bit binary value that subsequently can be used to recreate the <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToBinary()")]
	public static BigInt _9cea54115c704cf7(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindLocal)
			return GetInstantTicks(instance) + BinaryLocalMask;

		return GetTicks(instance) + BigIntFn(instance.Kind) * BinaryKindShift;
	}

	[Jazor(Op.Import, "System.DateTime.Date.get")]
	public static RuntimeModule.JDateTime _d77d20d9d04e2b6b(RuntimeModule.JDateTime instance)
		=> CreateDateTime(CreateLocalDate(instance.Date.GetFullYear(), instance.Date.GetMonth() + 1, instance.Date.GetDate()), instance.Kind);

	[Jazor(Op.Import, "System.DateTime.Day.get")]
	public static Number _3b9ecf5fd3c301db(RuntimeModule.JDateTime instance)
		=> instance.Date.GetDate();

	[Jazor(Op.Import, "System.DateTime.DayOfWeek.get")]
	public static System.DayOfWeek _6070f1709c491634(RuntimeModule.JDateTime instance)
		=> (System.DayOfWeek)(int)instance.Date.GetDay();

	/// <summary>
	/// C#: DateTime.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.DayOfYear.get")]
	public static Number _4f6ca20bf1aaa2d3(RuntimeModule.JDateTime instance)
	{
		var year = instance.Date.GetFullYear();
		var start = Date.UTC(year, 0, 0);
		var current = Date.UTC(year, instance.Date.GetMonth(), instance.Date.GetDate());
		return Math.FloorFn((current - start) / 86400000);
	}

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import ,"override System.DateTime.GetHashCode()")]
	public static Number _d3529b55e30e2a12(RuntimeModule.JDateTime instance)
		=> RuntimeModule.GetInt64HashCode(GetTicks(instance));

	[Jazor(Op.Import, "System.DateTime.Hour.get")]
	public static Number _f263cff61e6628a9(RuntimeModule.JDateTime instance)
		=> instance.Date.GetHours();

	[Jazor(Op.Import ,"System.DateTime.Kind.get")]
	public static System.DateTimeKind _551add245db0b701(RuntimeModule.JDateTime instance)
		=> (System.DateTimeKind)(int)instance.Kind;

	[Jazor(Op.Import, "System.DateTime.Millisecond.get")]
	public static Number _742a8bcf918b97e6(RuntimeModule.JDateTime instance)
		=> instance.Date.GetMilliseconds();

	[Jazor(Op.Import ,"System.DateTime.Microsecond.get")]
	public static Number _34d05014c270366f(RuntimeModule.JDateTime instance)
		=> NumberFn((instance.SubMillisecondTicks / TicksPerMicrosecond) % BigIntFn(1000));

	[Jazor(Op.Import ,"System.DateTime.Nanosecond.get")]
	public static Number _46e11fe2eb2ee869(RuntimeModule.JDateTime instance)
		=> NumberFn((instance.SubMillisecondTicks % TicksPerMicrosecond) * BigIntFn(100));

	[Jazor(Op.Import, "System.DateTime.Minute.get")]
	public static Number _f4ca5de4f63aa097(RuntimeModule.JDateTime instance)
		=> instance.Date.GetMinutes();

	[Jazor(Op.Import, "System.DateTime.Month.get")]
	public static Number _a8a6b6e36a0ea736(RuntimeModule.JDateTime instance)
		=> instance.Date.GetMonth() + 1;

	[Jazor(Op.Import, "static System.DateTime.Now.get")]
	public static RuntimeModule.JDateTime _ee9dd166a34a2fa5() => new(new Date(), DateTimeKindLocal);

	[Jazor(Op.Import, "System.DateTime.Second.get")]
	public static Number _10a94eacb3b7fd2d(RuntimeModule.JDateTime instance)
		=> instance.Date.GetSeconds();

	/// <summary>
	/// C#: DateTime.Ticks
	/// JS: instance.getTime() * 10000 + 621355968000000000 (从公元1年1月1日开始的ticks)
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.Ticks.get")]
	public static BigInt _bcde32e170f49354(RuntimeModule.JDateTime instance)
		=> GetTicks(instance);

	/// <summary>
	/// C#: DateTime.TimeOfDay
	/// JS: 返回自午夜以来的时间（ticks）
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.TimeOfDay.get")]
	public static RuntimeModule.JTimeSpan _2efdc237be2f31aa(RuntimeModule.JDateTime instance)
	{
		var ms = (((instance.Date.GetHours() * 60 + instance.Date.GetMinutes()) * 60 + instance.Date.GetSeconds()) * 1000) + instance.Date.GetMilliseconds();
		return new RuntimeModule.JTimeSpan(BigIntFn(ms) * TicksPerMillisecond + instance.SubMillisecondTicks);
	}

	[Jazor(Op.Import, "static System.DateTime.Today.get")]
	public static RuntimeModule.JDateTime _4b250155b7c688bb()
	{
		var now = new Date();
		return new RuntimeModule.JDateTime(CreateLocalDate(now.GetFullYear(), now.GetMonth() + 1, now.GetDate()), DateTimeKindLocal);
	}

	[Jazor(Op.Import, "System.DateTime.Year.get")]
	public static Number _9d56b09432f81c05(RuntimeModule.JDateTime instance)
		=> instance.Date.GetFullYear();

	///<summary>Returns an indication whether the specified year is a leap year.</summary>
	[Jazor(Op.Import, "static System.DateTime.IsLeapYear(int)")]
	public static bool _4a9da83e9cb28c1a(Number year)
	{
		EnsureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number between 1 and 9999.");
		if (year < 1 || year > 9999)
			throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");

		return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
	}

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Import, "static System.DateTime.Parse(string)")]
	public static RuntimeModule.JDateTime _a8a015c2d2bff2f6(string s)
		=> ParseCore(s);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(string, System.IFormatProvider)")]
	public static RuntimeModule.JDateTime _e0128ef45cc8584e(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTime _7372e5e0d8ba24a6(string s, Intl.NumberFormat? provider, object styles)
		=> ApplyDateTimeStyles(ParseCore(s), s, styles);

	///<summary>Converts a memory span that contains string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTime _2c85f5b20ae7559e(string s, Intl.NumberFormat? provider, object styles)
		=> ApplyDateTimeStyles(ParseCore(s), s, styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string, System.IFormatProvider)")]
	public extern static RuntimeModule.JDateTime _7f3dce20074d610f(string s, string format, Intl.NumberFormat? provider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _75cd4a49bd890e13(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _da7c1ef7b418c87d(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _f47f23f5482d6f56(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _32afd1b56d3b1c77(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> that subtracts the specified date and time from the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.Subtract(System.DateTime)")]
	public static RuntimeModule.JTimeSpan _4f5d235cac779f38(RuntimeModule.JDateTime instance, RuntimeModule.JDateTime value)
		=> _85b6d162b092ce0e(instance, value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that subtracts the specified duration from the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.Subtract(System.TimeSpan)")]
	public static RuntimeModule.JDateTime _20a406afebff2025(RuntimeModule.JDateTime instance, RuntimeModule.JTimeSpan value)
		=> _8d9ea66839ce392a(instance, value);

	///<summary>Converts the value of this instance to the equivalent OLE Automation date.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToOADate()")]
	public static Number _fb61bb2ccf4b10b6(RuntimeModule.JDateTime instance)
		=> NumberFn(GetTicks(instance) - UnixEpochTicks) / 10000d / MillisecondsPerDay + OADateUnixOffsetDays;

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToFileTime()")]
	public static BigInt _37ee48ca629793fa(RuntimeModule.JDateTime instance)
		=> GetInstantTicks(instance) - UnixEpochTicks + FileTimeUnixEpochTicks;

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToFileTimeUtc()")]
	public static BigInt _c02c49ea68661175(RuntimeModule.JDateTime instance)
		=> _37ee48ca629793fa(instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToLocalTime()")]
	public static RuntimeModule.JDateTime _db842725d5fd1ca0(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindLocal)
			return CreateDateTime(instance.Date, DateTimeKindLocal, instance.SubMillisecondTicks);

		var instantTicks = instance.Kind == DateTimeKindUnspecified
			? GetTicks(instance)
			: GetInstantTicks(instance);
		return CreateFromInstantTicks(instantTicks, DateTimeKindLocal);
	}

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long date string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToLongDateString()")]
	public static string _6e78dc03eecdd423(RuntimeModule.JDateTime instance)
		=> FormatDateTime(instance, "D", null);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long time string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToLongTimeString()")]
	public static string _ab161bb1563732af(RuntimeModule.JDateTime instance)
		=> FormatDateTime(instance, "T", null);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short date string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToShortDateString()")]
	public static string _6a67d54f5c865e5e(RuntimeModule.JDateTime instance)
		=> FormatDateTime(instance, "d", null);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short time string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToShortTimeString()")]
	public static string _af2d02ec0c0a300d(RuntimeModule.JDateTime instance)
		=> FormatDateTime(instance, "t", null);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the formatting conventions of the current culture.</summary>
	[Jazor(Op.Import, "override System.DateTime.ToString()")]
	public static string _6659b3b5d1f081dd(RuntimeModule.JDateTime instance)
		=> FormatDateTime(instance, null, null);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToString(string)")]
	public static string _3ee3e9478fe9a1fb(RuntimeModule.JDateTime instance, string? format)
		=> FormatDateTime(instance, format, null);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToString(System.IFormatProvider)")]
	public static string _606066f0ee1488c6(RuntimeModule.JDateTime instance, Intl.NumberFormat? provider)
		=> FormatDateTime(instance, null, provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToString(string, System.IFormatProvider)")]
	public static string _85393faf5839b9ef(RuntimeModule.JDateTime instance, string? format, Intl.NumberFormat? provider)
		=> FormatDateTime(instance, format, provider);

	///<summary>Tries to format the value of the current datetime instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateTime.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _b50913efa5ca8082(RuntimeModule.JDateTime instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateTime.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _100d184b5413769d(RuntimeModule.JDateTime instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to Coordinated Universal Time (UTC).</summary>
	[Jazor(Op.Import ,"System.DateTime.ToUniversalTime()")]
	public static RuntimeModule.JDateTime _b62871088df3ca8f(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindUtc)
			return CreateDateTime(instance.Date, DateTimeKindUtc, instance.SubMillisecondTicks);

		return CreateFromInstantTicks(GetInstantTicks(instance), DateTimeKindUtc);
	}

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import, "static System.DateTime.TryParse(string, out System.DateTime)")]
	public static Array<object?> _fa25ca318f086bb6(string? s, RuntimeModule.JDateTime result)
	{
		if (s == null || s.Length == 0)
			return [false, CreateDefaultDateTime()];
		try
		{
			return [true, ParseCore(s)];
		}
		catch
		{
			return [false, CreateDefaultDateTime()];
		}
	}

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)")]
	public static Array<object?> _8658c3be6edb9d2c(string s, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public static Array<object?> _34043b1eb3a8183a(string? s, Intl.NumberFormat? provider, object styles, RuntimeModule.JDateTime result)
	{
		ValidateDateTimeStyles(GetDateTimeStylesValue(styles));
		if (s == null || s.Length == 0)
			return [false, CreateDefaultDateTime()];
		try
		{
			return [true, ApplyDateTimeStyles(ParseCore(s), s, styles)];
		}
		catch
		{
			return [false, CreateDefaultDateTime()];
		}
	}

	///<summary>Converts the span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public static Array<object?> _6e8546b461b48646(string s, Intl.NumberFormat? provider, object styles, RuntimeModule.JDateTime result)
		=> _34043b1eb3a8183a(s, provider, styles, result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _79e29a1615b41471(string? s, string? format, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _d8720f2bb55cf0af(string s, string format, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _d1eb33e53764ee27(string? s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _685c4ca5481f00e7(string s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Adds a specified time interval to a specified date and time, yielding a new date and time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator +(System.DateTime, System.TimeSpan)")]
	public static RuntimeModule.JDateTime _d48b23d7c5f7c2aa(RuntimeModule.JDateTime d, RuntimeModule.JTimeSpan t)
		=> CreateFromTicks(GetTicks(d) + t.Ticks, d.Kind);

	///<summary>Subtracts a specified time interval from a specified date and time and returns a new date and time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator -(System.DateTime, System.TimeSpan)")]
	public static RuntimeModule.JDateTime _8d9ea66839ce392a(RuntimeModule.JDateTime d, RuntimeModule.JTimeSpan t)
		=> CreateFromTicks(GetTicks(d) - t.Ticks, d.Kind);

	///<summary>Subtracts a specified date and time from another specified date and time and returns a time interval.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator -(System.DateTime, System.DateTime)")]
	public static RuntimeModule.JTimeSpan _85b6d162b092ce0e(RuntimeModule.JDateTime d1, RuntimeModule.JDateTime d2)
		=> new(GetTicks(d1) - GetTicks(d2));

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are equal.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator ==(System.DateTime, System.DateTime)")]
	public static bool _37d87f65292f7083(RuntimeModule.JDateTime d1, RuntimeModule.JDateTime d2)
		=> GetTicks(d1) == GetTicks(d2);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are not equal.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator !=(System.DateTime, System.DateTime)")]
	public static bool _89406f797d33e566(RuntimeModule.JDateTime d1, RuntimeModule.JDateTime d2)
		=> GetTicks(d1) != GetTicks(d2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator <(System.DateTime, System.DateTime)")]
	public static bool _5a97e2aec50193b3(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) < GetTicks(t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator <=(System.DateTime, System.DateTime)")]
	public static bool _a8b15168323b118c(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) <= GetTicks(t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator >(System.DateTime, System.DateTime)")]
	public static bool _e98b0598f4980bcc(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) > GetTicks(t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator >=(System.DateTime, System.DateTime)")]
	public static bool _91697ebd6031bb97(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) >= GetTicks(t2);

	///<summary>Deconstructs this <see cref="T:System.DateTime" /> instance by <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />.</summary>
	[Jazor(Op.Import ,"System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)")]
	public static Array<object?> _bcf4183bef96ea21(RuntimeModule.JDateTime instance, RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time)
		=> [new RuntimeModule.JDateOnly(instance.Date.GetFullYear(), instance.Date.GetMonth() + 1, instance.Date.GetDate()), TimeOnlyModule._a305982aa6859677(instance)];

	///<summary>Deconstructs this <see cref="T:System.DateOnly" /> instance by <see cref="P:System.DateTime.Year" />, <see cref="P:System.DateTime.Month" />, and <see cref="P:System.DateTime.Day" />.</summary>
	[Jazor(Op.Import ,"System.DateTime.Deconstruct(out int, out int, out int)")]
	public static Array<object?> _5f721827cf6b8105(RuntimeModule.JDateTime instance, Number year, Number month, Number day)
		=> [instance.Date.GetFullYear(), instance.Date.GetMonth() + 1, instance.Date.GetDate()];

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats()")]
	public extern static string[] _8022abe7c2a9b946(RuntimeModule.JDateTime instance);

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers and the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(System.IFormatProvider)")]
	public extern static string[] _65e4d2ad2f14918c(RuntimeModule.JDateTime instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(char)")]
	public extern static string[] _daa9858a8adf981d(RuntimeModule.JDateTime instance, Number format);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier and culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(char, System.IFormatProvider)")]
	public extern static string[] _10c081a451aa4b71(RuntimeModule.JDateTime instance, Number format, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Inline ,"System.DateTime.GetTypeCode()", "16")]
	public extern static System.TypeCode _9164c7979da236d5(RuntimeModule.JDateTime instance);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)")]
	public static Array<object?> _6c36c46db30aacc1(string? s, Intl.NumberFormat? provider, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static RuntimeModule.JDateTime _41dcf008ea7cf6d9(string s, Intl.NumberFormat? provider)
		=> _a8a015c2d2bff2f6(s);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)")]
	public static Array<object?> _63fd53f09ba16132(string s, Intl.NumberFormat? provider, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	[Jazor(Op.Import, "static System.DateTime.UtcNow.get")]
	public static RuntimeModule.JDateTime _d4c39bdf47f391cf() => CreateUtcNow();
}
