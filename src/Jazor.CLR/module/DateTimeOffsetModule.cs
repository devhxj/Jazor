namespace Jazor.CLR;

[ECMAScriptModule("System/DateTimeOffsetModule.js")]
[Jazor(Op.Alias, "System.DateTimeOffset","Date")]
public static class DateTimeOffsetModule
{
	/// <summary>
	/// C#: DateTimeOffset.MinValue
	/// JS: new Date(-8640000000000000)
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.DateTimeOffset.MinValue", "new Date(-8640000000000000)")]
	public extern static Date _77107f0c23675b69();

	/// <summary>
	/// C#: DateTimeOffset.MaxValue
	/// JS: new Date(8640000000000000)
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.DateTimeOffset.MaxValue", "new Date(8640000000000000)")]
	public extern static Date _d45d439f0b97ae0e();

	/// <summary>
	/// C#: DateTimeOffset.UnixEpoch
	/// JS: new Date(0)
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.DateTimeOffset.UnixEpoch", "new Date(0)")]
	public extern static Date _087cabaedc1b5cc2();

	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset()")]
	public extern static Date _12b4f3f1dc14bea9();

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified number of ticks and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)")]
	public extern static Date _1e9c5d2a64e6d41d(BigInt ticks, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(System.DateTime)")]
	public extern static Date _7adf69a53659433a(Date dateTime);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(System.DateTime, System.TimeSpan)")]
	public extern static Date _106dabc0cc502aa4(Date dateTime, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="date" />, <paramref name="time" />, and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(System.DateOnly, System.TimeOnly, System.TimeSpan)")]
	public extern static Date _8f1aab77eeb6f786(Date date, Number time, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)")]
	public extern static Date _d90dce0e1d2f06e4(Number year, Number month, Number day, Number hour, Number minute, Number second, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.TimeSpan)")]
	public extern static Date _6abaa2b2082f575c(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset of a specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")]
	public extern static Date _61ea80919619bab9(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.TimeSpan)")]
	public extern static Date _04123d597aa761a3(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")]
	public extern static Date _d027561c1f6af451(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, BigInt offset);

	/// <summary>
	/// C#: DateTimeOffset.UtcNow
	/// JS: new Date() - current UTC time
	/// </summary>
	[Jazor(Op.Inline, "static System.DateTimeOffset.UtcNow.get", "new Date()")]
	public extern static Date _7f444d9ce7391e15();

	/// <summary>
	/// C#: instance.DateTime
	/// JS: instance (Date object)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.DateTime.get", "__arg1")]
	public extern static Date _2b7dd675863ae961(Date instance);

	/// <summary>
	/// C#: instance.UtcDateTime
	/// JS: new Date(instance.getTime()) - convert to UTC
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.UtcDateTime.get", "new Date(__arg1.getTime())")]
	public extern static Date _703902cecd7f61dd(Date instance);

	/// <summary>
	/// C#: instance.LocalDateTime
	/// JS: new Date(instance.getTime() + new Date().getTimezoneOffset() * 60000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.LocalDateTime.get", "new Date(__arg1.getTime() - __arg1.getTimezoneOffset() * 60000)")]
	public extern static Date _ffbfe7b660ff0527(Date instance);

	/// <summary>
	/// C#: instance.ToOffset(offset)
	/// JS: new Date(instance.getTime() + offset)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.ToOffset(System.TimeSpan)", "new Date(__arg1.getTime() + Number(__arg2) / 10000)")]
	public extern static Date _d1996f02ed3fa243(Date instance, BigInt offset);

	/// <summary>
	/// C#: instance.Date
	/// JS: new Date(instance.getFullYear(), instance.getMonth(), instance.getDate())
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Date.get", "new Date(__arg1.getFullYear(), __arg1.getMonth(), __arg1.getDate())")]
	public extern static Date _d7098a1eabebc945(Date instance);

	/// <summary>
	/// C#: instance.Day
	/// JS: instance.getDate()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Day.get", "__arg1.getDate()")]
	public extern static Number _ba8df912681fe784(Date instance);

	/// <summary>
	/// C#: instance.DayOfWeek
	/// JS: instance.getDay()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.DayOfWeek.get", "__arg1.getDay()")]
	public extern static System.DayOfWeek _17d30a204818ce34(Date instance);

	/// <summary>
	/// C#: instance.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.DayOfYear.get")]
	public static Number _b69ef2b7d0abde1a(Date instance)
	{
		var start = new Date(instance.GetFullYear(), 0, 0);
		var diff = instance.GetTime() - start.GetTime();
		var oneDay = 1000 * 60 * 60 * 24;
		return Math.Floor_(diff / oneDay);
	}

	/// <summary>
	/// C#: instance.Hour
	/// JS: instance.getHours()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Hour.get", "__arg1.getHours()")]
	public extern static Number _b7fc65477ef4df45(Date instance);

	/// <summary>
	/// C#: instance.Millisecond
	/// JS: instance.getMilliseconds()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Millisecond.get", "__arg1.getMilliseconds()")]
	public extern static Number _0c1b2675cd7a2faa(Date instance);

	/// <summary>
	/// C#: instance.Microsecond
	/// JS: instance.getMilliseconds() * 1000 (approximation)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Microsecond.get", "(__arg1.getMilliseconds() % 1) * 1000")]
	public extern static Number _ae3a48995f0953ed(Date instance);

	/// <summary>
	/// C#: instance.Nanosecond
	/// JS: 0 (JavaScript Date does not support nanoseconds)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Nanosecond.get", "0")]
	public extern static Number _f9acef215c7d5168(Date instance);

	/// <summary>
	/// C#: instance.Minute
	/// JS: instance.getMinutes()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Minute.get", "__arg1.getMinutes()")]
	public extern static Number _0fe8054b55f9f1c7(Date instance);

	/// <summary>
	/// C#: instance.Month
	/// JS: instance.getMonth() + 1
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Month.get", "(__arg1.getMonth() + 1)")]
	public extern static Number _79eb4c93cea58d59(Date instance);

	/// <summary>
	/// C#: instance.Offset
	/// JS: instance.getTimezoneOffset() * -600000000 (ticks)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Offset.get", "BigInt(__arg1.getTimezoneOffset() * -600000000)")]
	public extern static BigInt _2400298964c553b6(Date instance);

	/// <summary>
	/// C#: instance.TotalOffsetMinutes
	/// JS: -instance.getTimezoneOffset()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.TotalOffsetMinutes.get", "(-__arg1.getTimezoneOffset())")]
	public extern static Number _cad0683315440ded(Date instance);

	/// <summary>
	/// C#: instance.Second
	/// JS: instance.getSeconds()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Second.get", "__arg1.getSeconds()")]
	public extern static Number _822de224fed5bb6b(Date instance);

	/// <summary>
	/// C#: instance.Ticks
	/// JS: BigInt(instance.getTime()) * 10000n + 621355968000000000n
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Ticks.get", "(BigInt(__arg1.getTime()) * 10000n + 621355968000000000n)")]
	public extern static BigInt _584068ab15dcf3c9(Date instance);

	/// <summary>
	/// C#: instance.UtcTicks
	/// JS: BigInt(instance.getTime()) * 10000n + 621355968000000000n
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.UtcTicks.get", "(BigInt(__arg1.getTime()) * 10000n + 621355968000000000n)")]
	public extern static BigInt _056adc0ac251ebd3(Date instance);

	/// <summary>
	/// C#: instance.TimeOfDay
	/// JS: (instance.getTime() % 86400000) * 10000n
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.TimeOfDay.get", "(BigInt(__arg1.getTime() % 86400000) * 10000n)")]
	public extern static BigInt _90401f92f6a9141e(Date instance);

	/// <summary>
	/// C#: instance.Year
	/// JS: instance.getFullYear()
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Year.get", "__arg1.getFullYear()")]
	public extern static Number _127105b7a40a7665(Date instance);

	/// <summary>
	/// C#: instance.Add(timeSpan)
	/// JS: new Date(instance.getTime() + Number(timeSpan) / 10000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.Add(System.TimeSpan)", "new Date(__arg1.getTime() + Number(__arg2) / 10000)")]
	public extern static Date _09a94b0e7945eda6(Date instance, BigInt timeSpan);

	/// <summary>
	/// C#: instance.AddDays(days)
	/// JS: new Date(instance.getTime() + days * 86400000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddDays(double)", "new Date(__arg1.getTime() + __arg2 * 86400000)")]
	public extern static Date _7fd735ce2102a3cc(Date instance, Number days);

	/// <summary>
	/// C#: instance.AddHours(hours)
	/// JS: new Date(instance.getTime() + hours * 3600000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddHours(double)", "new Date(__arg1.getTime() + __arg2 * 3600000)")]
	public extern static Date _309c83b8a2fbc988(Date instance, Number hours);

	/// <summary>
	/// C#: instance.AddMilliseconds(milliseconds)
	/// JS: new Date(instance.getTime() + milliseconds)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddMilliseconds(double)", "new Date(__arg1.getTime() + __arg2)")]
	public extern static Date _1528b452af6dd41d(Date instance, Number milliseconds);

	/// <summary>
	/// C#: instance.AddMicroseconds(microseconds)
	/// JS: new Date(instance.getTime() + microseconds / 1000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddMicroseconds(double)", "new Date(__arg1.getTime() + __arg2 / 1000)")]
	public extern static Date _4775ccfee8ed671f(Date instance, Number microseconds);

	/// <summary>
	/// C#: instance.AddMinutes(minutes)
	/// JS: new Date(instance.getTime() + minutes * 60000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddMinutes(double)", "new Date(__arg1.getTime() + __arg2 * 60000)")]
	public extern static Date _97aff1e2f4740394(Date instance, Number minutes);

	/// <summary>
	/// C#: instance.AddMonths(months)
	/// JS: new Date(instance.setMonth(instance.getMonth() + months))
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddMonths(int)")]
	public static Date _db8ffdb562d3ac68(Date instance, Number months)
	{
		var result = new Date(instance.GetTime());
		result.SetMonth(result.GetMonth() + months);
		return result;
	}

	/// <summary>
	/// C#: instance.AddSeconds(seconds)
	/// JS: new Date(instance.getTime() + seconds * 1000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddSeconds(double)", "new Date(__arg1.getTime() + __arg2 * 1000)")]
	public extern static Date _54a4d6d554458fdb(Date instance, Number seconds);

	/// <summary>
	/// C#: instance.AddTicks(ticks)
	/// JS: new Date(instance.getTime() + Number(ticks) / 10000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTimeOffset.AddTicks(long)", "new Date(__arg1.getTime() + Number(__arg2) / 10000)")]
	public extern static Date _804f8bd2dc1e9443(Date instance, BigInt ticks);

	/// <summary>
	/// C#: instance.AddYears(years)
	/// JS: new Date(instance.setFullYear(instance.getFullYear() + years))
	/// </summary>
	[Jazor(Op.Import, "System.DateTimeOffset.AddYears(int)")]
	public static Date _f4ea4e123d38eaa5(Date instance, Number years)
	{
		var result = new Date(instance.GetTime());
		result.SetFullYear(result.GetFullYear() + years);
		return result;
	}

	///<summary>Compares two <see cref="T:System.DateTimeOffset" /> objects and indicates whether the first is earlier than the second, equal to the second, or later than the second.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Compare(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static Number _56ac26a94d0f9bca(Date first, Date second);

	///<summary>Compares the current <see cref="T:System.DateTimeOffset" /> object to a specified <see cref="T:System.DateTimeOffset" /> object and indicates whether the current object is earlier than, the same as, or later than the second <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.CompareTo(System.DateTimeOffset)")]
	public extern static Number _255c7bf4a2c3c663(Date instance, Date other);

	///<summary>Determines whether a <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified object.</summary>
	[Jazor(Op.Discard ,"override System.DateTimeOffset.Equals(object)")]
	public extern static bool _fbec90dd4b315acd(Date instance, object? obj);

	///<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Equals(System.DateTimeOffset)")]
	public extern static bool _5a55745cbe84c163(Date instance, Date other);

	///<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same time and has the same offset as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.EqualsExact(System.DateTimeOffset)")]
	public extern static bool _d4a929178865b462(Date instance, Date other);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Equals(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _817d2f7b0e423bec(Date first, Date second);

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.FromFileTime(long)")]
	public extern static Date _1185de87a3489deb(BigInt fileTime);

	///<summary>Converts a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.FromUnixTimeSeconds(long)")]
	public extern static Date _fb7d72712794a2e4(BigInt seconds);

	///<summary>Converts a Unix time expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.FromUnixTimeMilliseconds(long)")]
	public extern static Date _89071e7da78164f5(BigInt milliseconds);

	///<summary>Returns the hash code for the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"override System.DateTimeOffset.GetHashCode()")]
	public extern static Number _484d626eb36d071d(Date instance);

	///<summary>Converts the specified string representation of a date, time, and offset to its <see cref="T:System.DateTimeOffset" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(string)")]
	public extern static Date _25187a24d190d864(string input);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(string, System.IFormatProvider)")]
	public extern static Date _fbb732b1255fdd38(string input, Intl.NumberFormat? formatProvider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _277a1a2c7845bcdc(string input, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _948a165174740d96(string input, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider)")]
	public extern static Date _ef9349ca95c1e050(string input, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _6da8f452a2644e91(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts a character span that represents a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _cec804cac90222fc(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _d8c615ebc8c99180(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts a character span that contains the string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _9eaf2ad9372cd2ec(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Subtracts a <see cref="T:System.DateTimeOffset" /> value that represents a specific date and time from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Subtract(System.DateTimeOffset)")]
	public extern static BigInt _f1e08916de33ed2a(Date instance, Date value);

	///<summary>Subtracts a specified time interval from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Subtract(System.TimeSpan)")]
	public extern static Date _2636ae85f21cd963(Date instance, BigInt value);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to a Windows file time.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToFileTime()")]
	public extern static BigInt _d638010bc91ffd47(Date instance);

	///<summary>Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToUnixTimeSeconds()")]
	public extern static BigInt _8bc213443653978d(Date instance);

	///<summary>Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToUnixTimeMilliseconds()")]
	public extern static BigInt _e63166ec11d88ce1(Date instance);

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> object that represents the local time.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToLocalTime()")]
	public extern static Date _c45ea6b7c8ed9501(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override System.DateTimeOffset.ToString()")]
	public extern static string _2aaccc10061a3bb0(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToString(string)")]
	public extern static string _9b46cc87f855c6ba(Date instance, string? format);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToString(System.IFormatProvider)")]
	public extern static string _f0d70d071309b539(Date instance, Intl.NumberFormat? formatProvider);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToString(string, System.IFormatProvider)")]
	public extern static string _e856edbfd7db0646(Date instance, string? format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current datetime offset instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _f899d2eb7dcfcfe9(Date instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _ec001bad537a3ce9(Date instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> value that represents the Coordinated Universal Time (UTC).</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToUniversalTime()")]
	public extern static Date _cbe0bd9bc2e35d83(Date instance);

	///<summary>Tries to converts a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)")]
	public extern static Array<object?> _2fd90dc37b274014(string? input, Date result);

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, out System.DateTimeOffset)")]
	public extern static Array<object?> _c7957aa2e68f8218(string input, Date result);

	///<summary>Tries to convert a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _62fe5aa144f2c9e1(string? input, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _9dd0fca0c6a9a4de(string input, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _a99669f1d632e166(string? input, string? format, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _639a2d041804058b(string input, string format, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _39ec2cd456e46b13(string? input, object formats, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _d09753e75937de75(string input, object formats, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Defines an implicit conversion of a <see cref="T:System.DateTime" /> object to a <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.implicit operator System.DateTimeOffset(System.DateTime)")]
	public extern static Date _31bbd12ed57f4f76();

	///<summary>Adds a specified time interval to a <see cref="T:System.DateTimeOffset" /> object that has a specified date and time, and yields a <see cref="T:System.DateTimeOffset" /> object that has new a date and time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator +(System.DateTimeOffset, System.TimeSpan)")]
	public extern static Date _b8dd85346f7718fe(Date dateTimeOffset, BigInt timeSpan);

	///<summary>Subtracts a specified time interval from a specified date and time, and yields a new date and time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator -(System.DateTimeOffset, System.TimeSpan)")]
	public extern static Date _267065e6d921c80f(Date dateTimeOffset, BigInt timeSpan);

	///<summary>Subtracts one <see cref="T:System.DateTimeOffset" /> object from another and yields a time interval.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator -(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static BigInt _d1af541d3a7181e8(Date left, Date right);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator ==(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _553dcbd8f7ea1a16(Date left, Date right);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects refer to different points in time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator !=(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _9f6eec56175d9528(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator <(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _43aa45c9517f4d47(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator <=(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _a6755e7fc2ead5b5(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than (or later than) a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator >(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _84d1b669e69cd9bf(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than or equal to a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator >=(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _1cb1a326e417bc9b(Date left, Date right);

	///<summary>Deconstructs this <see cref="T:System.DateTimeOffset" /> instance by <see cref="T:System.DateOnly" />, <see cref="T:System.TimeOnly" />, and <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Deconstruct(out System.DateOnly, out System.TimeOnly, out System.TimeSpan)")]
	public extern static Array<object?> _6ec7dc3f674ff16c(Date instance, Date date, Number time, BigInt offset);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, out System.DateTimeOffset)")]
	public extern static Array<object?> _61ef673e0dd00ab0(string? s, Intl.NumberFormat? provider, Date result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Date _b0967252268296ed(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTimeOffset)")]
	public extern static Array<object?> _c9e042e683205a8b(string s, Intl.NumberFormat? provider, Date result);

	[Jazor(Op.Discard ,"static System.DateTimeOffset.Now.get")]
	public extern static Date _e679a7abf50cf648();
}
