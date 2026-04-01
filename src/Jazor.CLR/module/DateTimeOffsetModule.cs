namespace Jazor.CLR;

[ECMAScriptModule("System/DateTimeOffsetModule.js")]
[Jazor(Op.Alias, "System.DateTimeOffset","Object")]
public static class DateTimeOffsetModule
{
	private static BigInt ZeroTicks => BigInt.Zero;
	private static BigInt UnixEpochTicks => BigInt_("621355968000000000");
	private static BigInt FileTimeUnixEpochTicks => BigInt_("116444736000000000");
	private static BigInt TicksPerMillisecond => BigInt_("10000");
	private static BigInt OffsetMinuteTicks => BigInt_("600000000");
	private static BigInt MaxOffsetTicks => BigInt_("504000000000");
	private static Number DateTimeKindUtc => 1;
	private static Number DateTimeKindLocal => 2;

	private static RuntimeModule.JDateTimeOffset CreateDateTimeOffset(Date utcDateTime, BigInt offsetTicks)
		=> new(utcDateTime, offsetTicks);

	private static RuntimeModule.JDateTimeOffset CreateDateTimeOffset(Date utcDateTime, BigInt offsetTicks, BigInt utcSubMillisecondTicks)
		=> new(utcDateTime, offsetTicks, utcSubMillisecondTicks);

	private static BigInt GetDateTimeInstantTicks(RuntimeModule.JDateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKindUtc)
		{
			var date = dateTime.Date;
			return BigInt_(Date.UTC(
				date.GetFullYear(),
				date.GetMonth(),
				date.GetDate(),
				date.GetHours(),
				date.GetMinutes(),
				date.GetSeconds(),
				date.GetMilliseconds())) * TicksPerMillisecond + dateTime.SubMillisecondTicks + UnixEpochTicks;
		}

		return BigInt_(dateTime.Date.GetTime()) * TicksPerMillisecond + dateTime.SubMillisecondTicks + UnixEpochTicks;
	}

	private static BigInt GetDateTimeTicks(RuntimeModule.JDateTime dateTime)
	{
		var date = dateTime.Date;
		return BigInt_(Date.UTC(
			date.GetFullYear(),
			date.GetMonth(),
			date.GetDate(),
			date.GetHours(),
			date.GetMinutes(),
			date.GetSeconds(),
			date.GetMilliseconds())) * TicksPerMillisecond + dateTime.SubMillisecondTicks + UnixEpochTicks;
	}

	private static BigInt GetUtcTicks(RuntimeModule.JDateTimeOffset instance)
		=> BigInt_(instance.UtcDateTime.GetTime()) * TicksPerMillisecond + instance.UtcSubMillisecondTicks + UnixEpochTicks;

	private static BigInt GetTicks(RuntimeModule.JDateTimeOffset instance)
		=> GetUtcTicks(instance) + instance.OffsetTicks;

	private static RuntimeModule.JDateTimeOffset CreateFromUtcTicks(BigInt utcTicks, BigInt offsetTicks)
	{
		var ticksSinceUnixEpoch = utcTicks - UnixEpochTicks;
		var milliseconds = ticksSinceUnixEpoch / TicksPerMillisecond;
		var utcSubMillisecondTicks = ticksSinceUnixEpoch % TicksPerMillisecond;
		if (utcSubMillisecondTicks < ZeroTicks)
		{
			milliseconds -= BigInt_(1);
			utcSubMillisecondTicks += TicksPerMillisecond;
		}

		return CreateDateTimeOffset(new Date(Number_(milliseconds)), offsetTicks, utcSubMillisecondTicks);
	}

	private static BigInt NormalizeSubMillisecondTicks(BigInt ticks)
	{
		var remainder = (ticks - UnixEpochTicks) % TicksPerMillisecond;
		return remainder < ZeroTicks ? remainder + TicksPerMillisecond : remainder;
	}

	private static void ValidateOffset(RuntimeModule.JTimeSpan offset)
	{
		if (offset.Ticks % OffsetMinuteTicks != BigInt.Zero)
			throw new Error("ArgumentException: Offset must be specified in whole minutes.");

		if (offset.Ticks < -MaxOffsetTicks || offset.Ticks > MaxOffsetTicks)
			throw new Error("ArgumentOutOfRangeException: Offset must be within plus or minus 14 hours.");
	}

	private static RuntimeModule.JDateTimeOffset CreateWithLocalOffset(Date utcDateTime)
	{
		var offsetTicks = BigInt_(-utcDateTime.GetTimezoneOffset()) * OffsetMinuteTicks;
		return CreateDateTimeOffset(utcDateTime, offsetTicks);
	}

	private static RuntimeModule.JDateTimeOffset CreateWithLocalOffset(BigInt utcTicks)
	{
		var utcDateTime = CreateFromUtcTicks(utcTicks, ZeroTicks).UtcDateTime;
		var offsetTicks = BigInt_(-utcDateTime.GetTimezoneOffset()) * OffsetMinuteTicks;
		return CreateFromUtcTicks(utcTicks, offsetTicks);
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
				var hours = Number_(input.Substring(input.Length - 5, 2));
				var minutes = Number_(input.Substring(input.Length - 2, 2));
				if (signIndex > timeIndex && !IsNaN(hours) && !IsNaN(minutes))
				{
					var ticks = BigInt_(hours * 60 + minutes) * OffsetMinuteTicks;
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
				var hours = Number_(input.Substring(input.Length - 4, 2));
				var minutes = Number_(input.Substring(input.Length - 2, 2));
				if (signIndex > timeIndex && !IsNaN(hours) && !IsNaN(minutes))
				{
					var ticks = BigInt_(hours * 60 + minutes) * OffsetMinuteTicks;
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
				var hours = Number_(input.Substring(input.Length - 2, 2));
				if (signIndex > timeIndex && !IsNaN(hours))
				{
					var ticks = BigInt_(hours * 60) * OffsetMinuteTicks;
					return sign == '-' ? -ticks : ticks;
				}
			}
		}

		return BigInt_(-parsedDate.GetTimezoneOffset()) * OffsetMinuteTicks;
	}

	private static RuntimeModule.JDateTimeOffset ParseCore(string input)
	{
		var parsed = new Date(input);
		if (IsNaN(parsed.GetTime()))
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTimeOffset.");

		var offsetTicks = ResolveParsedOffsetTicks(input, parsed);
		return CreateDateTimeOffset(new Date(parsed.GetTime()), offsetTicks);
	}

	/// <summary>
	/// C#: DateTimeOffset.MinValue
	/// JS: new Date(-8640000000000000)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTimeOffset.MinValue")]
	public static RuntimeModule.JDateTimeOffset _77107f0c23675b69() => CreateDateTimeOffset(new Date(-62135596800000d), ZeroTicks);

	/// <summary>
	/// C#: DateTimeOffset.MaxValue
	/// JS: new Date(8640000000000000)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTimeOffset.MaxValue")]
	public static RuntimeModule.JDateTimeOffset _d45d439f0b97ae0e() => CreateDateTimeOffset(new Date(253402300799999d), ZeroTicks, BigInt_("9999"));

	/// <summary>
	/// C#: DateTimeOffset.UnixEpoch
	/// JS: new Date(0)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTimeOffset.UnixEpoch")]
	public static RuntimeModule.JDateTimeOffset _087cabaedc1b5cc2() => CreateDateTimeOffset(new Date(0), ZeroTicks);

	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset()")]
	public static RuntimeModule.JDateTimeOffset _12b4f3f1dc14bea9() => CreateDateTimeOffset(new Date(-62135596800000d), ZeroTicks);

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
			var expectedOffset = BigInt_(-dateTime.Date.GetTimezoneOffset()) * OffsetMinuteTicks;
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
		var localTicks = BigInt_(Date.UTC(date.Year, date.Month - 1, date.Day)) * TicksPerMillisecond + time.Ticks + UnixEpochTicks;
		return CreateFromUtcTicks(localTicks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, and offset.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _d90dce0e1d2f06e4(Number year, Number month, Number day, Number hour, Number minute, Number second, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		var localTicks = BigInt_(Date.UTC(year, month - 1, day, hour, minute, second)) * TicksPerMillisecond + UnixEpochTicks;
		return CreateFromUtcTicks(localTicks - offset.Ticks, offset.Ticks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.TimeSpan)")]
	public static RuntimeModule.JDateTimeOffset _6abaa2b2082f575c(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, RuntimeModule.JTimeSpan offset)
	{
		ValidateOffset(offset);
		var localTicks = BigInt_(Date.UTC(year, month - 1, day, hour, minute, second, millisecond)) * TicksPerMillisecond + UnixEpochTicks;
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
		var localTicks = BigInt_(Date.UTC(year, month - 1, day, hour, minute, second, millisecond)) * TicksPerMillisecond + BigInt_(microsecond) * BigInt_("10") + UnixEpochTicks;
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
	/// JS: instance (Date object)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DateTime.get")]
	public static RuntimeModule.JDateTime _2b7dd675863ae961(RuntimeModule.JDateTimeOffset instance)
	{
		var localTicks = GetTicks(instance);
		return new RuntimeModule.JDateTime(
			new Date(
				Number_(_127105b7a40a7665(instance)),
				Number_(_79eb4c93cea58d59(instance)) - 1,
				Number_(_ba8df912681fe784(instance)),
				Number_(_b7fc65477ef4df45(instance)),
				Number_(_0fe8054b55f9f1c7(instance)),
				Number_(_822de224fed5bb6b(instance)),
				Number_(_0c1b2675cd7a2faa(instance))),
			0,
			NormalizeSubMillisecondTicks(localTicks));
	}

	/// <summary>
	/// C#: instance.UtcDateTime
	/// JS: new Date(instance.getTime()) - convert to UTC
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.UtcDateTime.get")]
	public static RuntimeModule.JDateTime _703902cecd7f61dd(RuntimeModule.JDateTimeOffset instance)
	{
		var utc = instance.UtcDateTime;
		return new RuntimeModule.JDateTime(new Date(utc.GetUTCFullYear(), utc.GetUTCMonth(), utc.GetUTCDate(), utc.GetUTCHours(), utc.GetUTCMinutes(), utc.GetUTCSeconds(), utc.GetUTCMilliseconds()), DateTimeKindUtc, instance.UtcSubMillisecondTicks);
	}

	/// <summary>
	/// C#: instance.LocalDateTime
	/// JS: new Date(instance.getTime() + new Date().getTimezoneOffset() * 60000)
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
	/// JS: new Date(instance.getFullYear(), instance.getMonth(), instance.getDate())
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Date.get")]
	public static RuntimeModule.JDateTime _d7098a1eabebc945(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		return new RuntimeModule.JDateTime(new Date(local.GetUTCFullYear(), local.GetUTCMonth(), local.GetUTCDate()), 0);
	}

	/// <summary>
	/// C#: instance.Day
	/// JS: instance.getDate()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Day.get")]
	public static Number _ba8df912681fe784(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		return local.GetUTCDate();
	}

	/// <summary>
	/// C#: instance.DayOfWeek
	/// JS: instance.getDay()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DayOfWeek.get")]
	public static Number _17d30a204818ce34(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		return local.GetUTCDay();
	}

	/// <summary>
	/// C#: instance.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DayOfYear.get")]
	public static Number _b69ef2b7d0abde1a(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		var start = Date.UTC(local.GetUTCFullYear(), 0, 0);
		return Math.Floor_((local.GetTime() - start) / 86400000);
	}

	/// <summary>
	/// C#: instance.Hour
	/// JS: instance.getHours()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Hour.get")]
	public static Number _b7fc65477ef4df45(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		return local.GetUTCHours();
	}

	/// <summary>
	/// C#: instance.Millisecond
	/// JS: instance.getMilliseconds()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Millisecond.get")]
	public static Number _0c1b2675cd7a2faa(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		return local.GetUTCMilliseconds();
	}

	/// <summary>
	/// C#: instance.Microsecond
	/// JS: instance.getMilliseconds() * 1000 (approximation)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Microsecond.get")]
	public static Number _ae3a48995f0953ed(RuntimeModule.JDateTimeOffset instance)
		=> Number_((instance.UtcSubMillisecondTicks / BigInt_("10")) % BigInt_(1000));

	/// <summary>
	/// C#: instance.Nanosecond
	/// JS: 0 (JavaScript Date does not support nanoseconds)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Nanosecond.get")]
	public static Number _f9acef215c7d5168(RuntimeModule.JDateTimeOffset instance)
		=> Number_((instance.UtcSubMillisecondTicks % BigInt_("10")) * BigInt_(100));

	/// <summary>
	/// C#: instance.Minute
	/// JS: instance.getMinutes()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Minute.get")]
	public static Number _0fe8054b55f9f1c7(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		return local.GetUTCMinutes();
	}

	/// <summary>
	/// C#: instance.Month
	/// JS: instance.getMonth() + 1
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Month.get")]
	public static Number _79eb4c93cea58d59(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
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
	/// C#: instance.TotalOffsetMinutes
	/// JS: -instance.getTimezoneOffset()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.TotalOffsetMinutes.get")]
	public static Number _cad0683315440ded(RuntimeModule.JDateTimeOffset instance)
		=> Number_(instance.OffsetTicks / OffsetMinuteTicks);

	/// <summary>
	/// C#: instance.Second
	/// JS: instance.getSeconds()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Second.get")]
	public static Number _822de224fed5bb6b(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
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
		var normalized = GetTicks(instance) % BigInt_("864000000000");
		return new RuntimeModule.JTimeSpan(normalized < ZeroTicks ? normalized + BigInt_("864000000000") : normalized);
	}

	/// <summary>
	/// C#: instance.Year
	/// JS: instance.getFullYear()
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.Year.get")]
	public static Number _127105b7a40a7665(RuntimeModule.JDateTimeOffset instance)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
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
		=> CreateFromUtcTicks(GetUtcTicks(instance) + BigInt_(Math.Round_(days * 864000000000d)), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddHours(hours)
	/// JS: new Date(instance.getTime() + hours * 3600000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddHours(double)")]
	public static RuntimeModule.JDateTimeOffset _309c83b8a2fbc988(RuntimeModule.JDateTimeOffset instance, Number hours)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + BigInt_(Math.Round_(hours * 36000000000d)), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMilliseconds(milliseconds)
	/// JS: new Date(instance.getTime() + milliseconds)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMilliseconds(double)")]
	public static RuntimeModule.JDateTimeOffset _1528b452af6dd41d(RuntimeModule.JDateTimeOffset instance, Number milliseconds)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + BigInt_(Math.Round_(milliseconds * 10000d)), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMicroseconds(microseconds)
	/// JS: new Date(instance.getTime() + microseconds / 1000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMicroseconds(double)")]
	public static RuntimeModule.JDateTimeOffset _4775ccfee8ed671f(RuntimeModule.JDateTimeOffset instance, Number microseconds)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + BigInt_(Math.Round_(microseconds * 10d)), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMinutes(minutes)
	/// JS: new Date(instance.getTime() + minutes * 60000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMinutes(double)")]
	public static RuntimeModule.JDateTimeOffset _97aff1e2f4740394(RuntimeModule.JDateTimeOffset instance, Number minutes)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + BigInt_(Math.Round_(minutes * 600000000d)), instance.OffsetTicks);

	/// <summary>
	/// C#: instance.AddMonths(months)
	/// JS: new Date(instance.setMonth(instance.getMonth() + months))
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMonths(int)")]
	public static RuntimeModule.JDateTimeOffset _db8ffdb562d3ac68(RuntimeModule.JDateTimeOffset instance, Number months)
	{
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		local.SetUTCMonth(local.GetUTCMonth() + months);
		return CreateDateTimeOffset(new Date(local.GetTime() - Number_(instance.OffsetTicks) / 10000), instance.OffsetTicks, instance.UtcSubMillisecondTicks);
	}

	/// <summary>
	/// C#: instance.AddSeconds(seconds)
	/// JS: new Date(instance.getTime() + seconds * 1000)
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddSeconds(double)")]
	public static RuntimeModule.JDateTimeOffset _54a4d6d554458fdb(RuntimeModule.JDateTimeOffset instance, Number seconds)
		=> CreateFromUtcTicks(GetUtcTicks(instance) + BigInt_(Math.Round_(seconds * 10000000d)), instance.OffsetTicks);

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
		var local = new Date(instance.UtcDateTime.GetTime() + Number_(instance.OffsetTicks) / 10000);
		local.SetUTCFullYear(local.GetUTCFullYear() + years);
		return CreateDateTimeOffset(new Date(local.GetTime() - Number_(instance.OffsetTicks) / 10000), instance.OffsetTicks, instance.UtcSubMillisecondTicks);
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
		return CreateWithLocalOffset(fileTime - FileTimeUnixEpochTicks + UnixEpochTicks);
	}

	///<summary>Converts a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.FromUnixTimeSeconds(long)")]
	public static RuntimeModule.JDateTimeOffset _fb7d72712794a2e4(BigInt seconds)
		=> CreateDateTimeOffset(new Date(Number_(seconds * BigInt_(1000))), ZeroTicks);

	///<summary>Converts a Unix time expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.FromUnixTimeMilliseconds(long)")]
	public static RuntimeModule.JDateTimeOffset _89071e7da78164f5(BigInt milliseconds)
		=> CreateDateTimeOffset(new Date(Number_(milliseconds)), ZeroTicks);

	///<summary>Returns the hash code for the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Import ,"override System.DateTimeOffset.GetHashCode()")]
	public static Number _484d626eb36d071d(RuntimeModule.JDateTimeOffset instance)
		=> Number_(GetUtcTicks(instance) % BigInt_("2147483647"));

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
		=> ParseCore(input);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTimeOffset _948a165174740d96(string input, Intl.NumberFormat? formatProvider, object styles)
		=> ParseCore(input);

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
		=> (GetUtcTicks(instance) - UnixEpochTicks) / BigInt_("10000000");

	///<summary>Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToUnixTimeMilliseconds()")]
	public static BigInt _e63166ec11d88ce1(RuntimeModule.JDateTimeOffset instance)
		=> (GetUtcTicks(instance) - UnixEpochTicks) / TicksPerMillisecond;

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> object that represents the local time.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToLocalTime()")]
	public static RuntimeModule.JDateTimeOffset _c45ea6b7c8ed9501(RuntimeModule.JDateTimeOffset instance)
		=> CreateWithLocalOffset(GetUtcTicks(instance));

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Alias ,"override System.DateTimeOffset.ToString()", "toString")]
	public extern static string _2aaccc10061a3bb0(RuntimeModule.JDateTimeOffset instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToString(string)")]
	public static string _9b46cc87f855c6ba(RuntimeModule.JDateTimeOffset instance, string? format)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToString(System.IFormatProvider)")]
	public static string _f0d70d071309b539(RuntimeModule.JDateTimeOffset instance, Intl.NumberFormat? formatProvider)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateTimeOffset.ToString(string, System.IFormatProvider)")]
	public static string _e856edbfd7db0646(RuntimeModule.JDateTimeOffset instance, string? format, Intl.NumberFormat? formatProvider)
		=> instance.ToString();

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
			return [false, CreateDateTimeOffset(new Date(0), ZeroTicks)];

		try
		{
			return [true, ParseCore(input)];
		}
		catch
		{
			return [false, CreateDateTimeOffset(new Date(0), ZeroTicks)];
		}
	}

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, out System.DateTimeOffset)")]
	public static Array<object?> _c7957aa2e68f8218(string input, RuntimeModule.JDateTimeOffset result)
		=> _2fd90dc37b274014(input, result);

	///<summary>Tries to convert a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public static Array<object?> _62fe5aa144f2c9e1(string? input, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result)
		=> _2fd90dc37b274014(input, result);

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public static Array<object?> _9dd0fca0c6a9a4de(string input, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JDateTimeOffset result)
		=> _2fd90dc37b274014(input, result);

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
		var offsetTicks = BigInt_(-now.GetTimezoneOffset()) * OffsetMinuteTicks;
		return CreateDateTimeOffset(new Date(now.GetTime()), offsetTicks);
	}
}
