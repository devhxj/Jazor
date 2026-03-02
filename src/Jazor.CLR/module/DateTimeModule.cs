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
/// - Replace: JS Date 原生方法（如 getFullYear、getMonth）
/// - Import: 需要完整实现的复杂逻辑
/// - Discard: 不支持或极少使用
/// </summary>
[ECMAScriptModule]
[Jazor(Op.Import, "System.DateTime","System/DateTimeModule.js")]
public static class DateTimeModule
{
	/// <summary>
	/// C#: DateTime.MinValue
	/// JS: new Date(-8640000000000000)
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.DateTime.MinValue", "new Date(-8640000000000000)")]
	public extern static Date _fad0c74e1c9df5bb();

	/// <summary>
	/// C#: DateTime.MaxValue
	/// JS: new Date(8640000000000000)
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.DateTime.MaxValue", "new Date(8640000000000000)")]
	public extern static Date _eb38dc04224730ea();

	/// <summary>
	/// C#: DateTime.UnixEpoch
	/// JS: new Date(0)
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.DateTime.UnixEpoch", "new Date(0)")]
	public extern static Date _878591efc9a51388();

	[Jazor(Op.Discard ,"System.DateTime.DateTime()")]
	public extern static Date _bfa8ee5dd46e2005();

	/// <summary>
	/// C#: new DateTime(ticks)
	/// JS: new Date(Number((ticks - 621355968000000000n) / 10000n))
	/// </summary>
	[Jazor(Op.Inline, "System.DateTime.DateTime(long)", "new Date(Number((@#{0} - 621355968000000000n) / 10000n))")]
	public extern static Date _1ba9ed95dd0eab48(BigInt ticks);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks and to Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(long, System.DateTimeKind)")]
	public extern static Date _eda1c8bf8e1e617b(BigInt ticks, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />. The new instance will have the <see cref="F:System.DateTimeKind.Unspecified" /> kind.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(System.DateOnly, System.TimeOnly)")]
	public extern static Date _4fef4795bcbef97f(Date date, Number time);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" /> and respecting the specified <see cref="T:System.DateTimeKind" />.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)")]
	public extern static Date _85602323793168a5(Date date, Number time, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day.</summary>
	[Jazor(Op.Inline, "System.DateTime.DateTime(int, int, int)", "new Date(@#{0}, @#{1} - 1, @#{2})")]
	public extern static Date _4cb33a818161a3e1(Number year, Number month, Number day);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)")]
	public extern static Date _a515b8bb82ad96b7(Number year, Number month, Number day, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)")]
	public extern static Date _bd2c430e6327a2cc(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second.</summary>
	[Jazor(Op.Inline, "System.DateTime.DateTime(int, int, int, int, int, int)", "new Date(@#{0}, @#{1} - 1, @#{2}, @#{3}, @#{4}, @#{5})")]
	public extern static Date _4903723bbf8a0a2f(Number year, Number month, Number day, Number hour, Number minute, Number second);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)")]
	public extern static Date _f83be88cfb3fbce0(Number year, Number month, Number day, Number hour, Number minute, Number second, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static Date _29bb943b21806bd9(Number year, Number month, Number day, Number hour, Number minute, Number second, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond.</summary>
	[Jazor(Op.Inline, "System.DateTime.DateTime(int, int, int, int, int, int, int)", "new Date(@#{0}, @#{1} - 1, @#{2}, @#{3}, @#{4}, @#{5}, @#{6})")]
	public extern static Date _5822b271bb635d64(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)")]
	public extern static Date _c52eec5e681a0b8b(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static Date _8a4d2d51b716bb36(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int)")]
	public extern static Date _9117d26d23769ad1(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)")]
	public extern static Date _e84671346e2b9972(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static Date _bd13792ce57e1964(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)")]
	public extern static Date _cd0b8f2bce1e09ed(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, object kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the value of the specified <see cref="T:System.TimeSpan" /> to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTime.Add(System.TimeSpan)")]
	public extern static Date _34a77be7365c459f(Date instance, BigInt value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of days to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddDays(double)")]
	public static Date _558a3f189d9149d7(Date instance, Number value)
	{
		var result = new Date(instance.GetTime() + value * 86400000);
		return result;
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of hours to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddHours(double)")]
	public static Date _101af978213c19c5(Date instance, Number value)
	{
		var result = new Date(instance.GetTime() + value * 3600000);
		return result;
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of milliseconds to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMilliseconds(double)")]
	public static Date _2b29e4c11fa12daa(Date instance, Number value)
	{
		var result = new Date(instance.GetTime() + value);
		return result;
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of microseconds to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTime.AddMicroseconds(double)")]
	public extern static Date _2b47368c73a3e1f2(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of minutes to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMinutes(double)")]
	public static Date _8bdc25943cf2d39b(Date instance, Number value)
	{
		var result = new Date(instance.GetTime() + value * 60000);
		return result;
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of months to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMonths(int)")]
	public static Date _aae197b95f9024a4(Date instance, Number months)
	{
		var result = new Date(instance.GetTime());
		result.SetMonth(result.GetMonth() + months);
		return result;
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of seconds to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddSeconds(double)")]
	public static Date _57045f93edac1460(Date instance, Number value)
	{
		var result = new Date(instance.GetTime() + value * 1000);
		return result;
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of ticks to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTime.AddTicks(long)")]
	public extern static Date _d2e74845b174a889(Date instance, BigInt value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of years to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddYears(int)")]
	public static Date _3353d31b02f2bed8(Date instance, Number value)
	{
		var result = new Date(instance.GetTime());
		result.SetFullYear(result.GetFullYear() + value);
		return result;
	}

	///<summary>Compares two instances of <see cref="T:System.DateTime" /> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.</summary>
	[Jazor(Op.Import, "static System.DateTime.Compare(System.DateTime, System.DateTime)")]
	public static Number _0edfd00dcc8d70d0(Date t1, Date t2)
	{
		var diff = t1.GetTime() - t2.GetTime();
		if (diff < 0) return -1;
		if (diff > 0) return 1;
		return 0;
	}

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateTime" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Discard ,"System.DateTime.CompareTo(object)")]
	public extern static Number _f7b2337bfa9864d9(Date instance, object? value);

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateTime" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Import, "System.DateTime.CompareTo(System.DateTime)")]
	public static Number _40c6426fdc505e97(Date instance, Date value)
	{
		var diff = instance.GetTime() - value.GetTime();
		if (diff < 0) return -1;
		if (diff > 0) return 1;
		return 0;
	}

	///<summary>Returns the number of days in the specified month and year.</summary>
	[Jazor(Op.Import, "static System.DateTime.DaysInMonth(int, int)")]
	public static Number _38ef7423971afb7f(Number year, Number month)
	{
		return new Date(year, month, 0).GetDate();
	}

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Inline, "override System.DateTime.Equals(object)", "(@#{0} instanceof Date && @#{0}.getTime() === (@#{1}?.getTime?.() ?? NaN))")]
	public extern static bool _f6903c1af8944917(Date instance, object? value);

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateTime" /> instance.</summary>
	[Jazor(Op.Inline, "System.DateTime.Equals(System.DateTime)", "(@#{0}.getTime() === @#{1}.getTime())")]
	public extern static bool _c29ca32a998c517c(Date instance, Date value);

	///<summary>Returns a value indicating whether two <see cref="T:System.DateTime" /> instances  have the same date and time value.</summary>
	[Jazor(Op.Inline, "static System.DateTime.Equals(System.DateTime, System.DateTime)", "(@#{0}.getTime() === @#{1}.getTime())")]
	public extern static bool _4937ff8bec81ddea(Date t1, Date t2);

	///<summary>Deserializes a 64-bit binary value and recreates an original serialized <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.FromBinary(long)")]
	public extern static Date _f437fad61f0046c7(BigInt dateData);

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.FromFileTime(long)")]
	public extern static Date _df025c273bde0e50(BigInt fileTime);

	///<summary>Converts the specified Windows file time to an equivalent UTC time.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.FromFileTimeUtc(long)")]
	public extern static Date _93886aebedb72920(BigInt fileTime);

	///<summary>Returns a <see cref="T:System.DateTime" /> equivalent to the specified OLE Automation Date.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.FromOADate(double)")]
	public extern static Date _12520a637fb85a70(Number d);

	///<summary>Indicates whether this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the current time zone.</summary>
	[Jazor(Op.Discard ,"System.DateTime.IsDaylightSavingTime()")]
	public extern static bool _d3b1cc7e750c6bc3(Date instance);

	///<summary>Creates a new <see cref="T:System.DateTime" /> object that has the same number of ticks as the specified <see cref="T:System.DateTime" />, but is designated as either local time, Coordinated Universal Time (UTC), or neither, as indicated by the specified <see cref="T:System.DateTimeKind" /> value.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)")]
	public extern static Date _a99826a92073614e(Date value, object kind);

	///<summary>Serializes the current <see cref="T:System.DateTime" /> object to a 64-bit binary value that subsequently can be used to recreate the <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToBinary()")]
	public extern static BigInt _9cea54115c704cf7(Date instance);

	[Jazor(Op.Inline, "System.DateTime.Date.get", "new Date(@#{0}.getFullYear(), @#{0}.getMonth(), @#{0}.getDate())")]
	public extern static Date _d77d20d9d04e2b6b(Date instance);

	[Jazor(Op.Replace, "System.DateTime.Day.get", "getDate")]
	public extern static Number _3b9ecf5fd3c301db(Date instance);

	[Jazor(Op.Replace, "System.DateTime.DayOfWeek.get", "getDay")]
	public extern static System.DayOfWeek _6070f1709c491634(Date instance);

	/// <summary>
	/// C#: DateTime.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.DayOfYear.get")]
	public static Number _4f6ca20bf1aaa2d3(Date instance)
	{
		var start = new Date(instance.GetFullYear(), 0, 0);
		var diff = instance.GetTime() - start.GetTime();
		var oneDay = 1000 * 60 * 60 * 24;
		return Math.Floor_(diff / oneDay);
	}

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override System.DateTime.GetHashCode()")]
	public extern static Number _d3529b55e30e2a12(Date instance);

	[Jazor(Op.Replace, "System.DateTime.Hour.get", "getHours")]
	public extern static Number _f263cff61e6628a9(Date instance);

	[Jazor(Op.Discard ,"System.DateTime.Kind.get")]
	public extern static System.DateTimeKind _551add245db0b701(Date instance);

	[Jazor(Op.Replace, "System.DateTime.Millisecond.get", "getMilliseconds")]
	public extern static Number _742a8bcf918b97e6(Date instance);

	[Jazor(Op.Discard ,"System.DateTime.Microsecond.get")]
	public extern static Number _34d05014c270366f(Date instance);

	[Jazor(Op.Discard ,"System.DateTime.Nanosecond.get")]
	public extern static Number _46e11fe2eb2ee869(Date instance);

	[Jazor(Op.Replace, "System.DateTime.Minute.get", "getMinutes")]
	public extern static Number _f4ca5de4f63aa097(Date instance);

	[Jazor(Op.Inline, "System.DateTime.Month.get", "(@#{0}.getMonth() + 1)")]
	public extern static Number _a8a6b6e36a0ea736(Date instance);

	[Jazor(Op.Inline, "static System.DateTime.Now.get", "new Date()")]
	public extern static Date _ee9dd166a34a2fa5();

	[Jazor(Op.Replace, "System.DateTime.Second.get", "getSeconds")]
	public extern static Number _10a94eacb3b7fd2d(Date instance);

	/// <summary>
	/// C#: DateTime.Ticks
	/// JS: instance.getTime() * 10000 + 621355968000000000 (从公元1年1月1日开始的ticks)
	/// </summary>
	[Jazor(Op.Inline, "System.DateTime.Ticks.get", "(BigInt(@#{0}.getTime()) * 10000n + 621355968000000000n)")]
	public extern static BigInt _bcde32e170f49354(Date instance);

	/// <summary>
	/// C#: DateTime.TimeOfDay
	/// JS: 返回自午夜以来的时间（ticks）
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.TimeOfDay.get")]
	public static BigInt _2efdc237be2f31aa(Date instance)
	{
		var ms = instance.GetTime() % 86400000;
		return BigInt_(ms * 10000);
	}

	[Jazor(Op.Inline, "static System.DateTime.Today.get", "new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())")]
	public extern static Date _4b250155b7c688bb();

	[Jazor(Op.Replace, "System.DateTime.Year.get", "getFullYear")]
	public extern static Number _9d56b09432f81c05(Date instance);

	///<summary>Returns an indication whether the specified year is a leap year.</summary>
	[Jazor(Op.Import, "static System.DateTime.IsLeapYear(int)")]
	public static bool _4a9da83e9cb28c1a(Number year)
	{
		return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
	}

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Import, "static System.DateTime.Parse(string)")]
	public static Date _a8a015c2d2bff2f6(string s)
	{
		var date = new Date(s);
		if (IsNaN(date.GetTime()))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid DateTime.");
		return date;
	}

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.Parse(string, System.IFormatProvider)")]
	public extern static Date _e0128ef45cc8584e(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _7372e5e0d8ba24a6(string s, Intl.NumberFormat? provider, object styles);

	///<summary>Converts a memory span that contains string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _2c85f5b20ae7559e(string s, Intl.NumberFormat? provider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string, System.IFormatProvider)")]
	public extern static Date _7f3dce20074d610f(string s, string format, Intl.NumberFormat? provider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _75cd4a49bd890e13(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _da7c1ef7b418c87d(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _f47f23f5482d6f56(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _32afd1b56d3b1c77(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> that subtracts the specified date and time from the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTime.Subtract(System.DateTime)")]
	public extern static BigInt _4f5d235cac779f38(Date instance, Date value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that subtracts the specified duration from the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTime.Subtract(System.TimeSpan)")]
	public extern static Date _20a406afebff2025(Date instance, BigInt value);

	///<summary>Converts the value of this instance to the equivalent OLE Automation date.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToOADate()")]
	public extern static Number _fb61bb2ccf4b10b6(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToFileTime()")]
	public extern static BigInt _37ee48ca629793fa(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToFileTimeUtc()")]
	public extern static BigInt _c02c49ea68661175(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to local time.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToLocalTime()")]
	public extern static Date _db842725d5fd1ca0(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long date string representation.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToLongDateString()")]
	public extern static string _6e78dc03eecdd423(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long time string representation.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToLongTimeString()")]
	public extern static string _ab161bb1563732af(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short date string representation.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToShortDateString()")]
	public extern static string _6a67d54f5c865e5e(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short time string representation.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToShortTimeString()")]
	public extern static string _af2d02ec0c0a300d(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the formatting conventions of the current culture.</summary>
	[Jazor(Op.Replace, "override System.DateTime.ToString()", "toString")]
	public extern static string _6659b3b5d1f081dd(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToString(string)")]
	public extern static string _3ee3e9478fe9a1fb(Date instance, string? format);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToString(System.IFormatProvider)")]
	public extern static string _606066f0ee1488c6(Date instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToString(string, System.IFormatProvider)")]
	public extern static string _85393faf5839b9ef(Date instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current datetime instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateTime.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _b50913efa5ca8082(Date instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateTime.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _100d184b5413769d(Date instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to Coordinated Universal Time (UTC).</summary>
	[Jazor(Op.Discard ,"System.DateTime.ToUniversalTime()")]
	public extern static Date _b62871088df3ca8f(Date instance);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import, "static System.DateTime.TryParse(string, out System.DateTime)")]
	public static Array<object?> _fa25ca318f086bb6(string? s, Date result)
	{
		if (s == null || s.Length == 0)
			return [false, new Date(0)];
		try
		{
			var date = new Date(s);
			if (IsNaN(date.GetTime()))
				return [false, new Date(0)];
			return [true, date];
		}
		catch
		{
			return [false, new Date(0)];
		}
	}

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)")]
	public extern static Array<object?> _8658c3be6edb9d2c(string s, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _34043b1eb3a8183a(string? s, Intl.NumberFormat? provider, object styles, Date result);

	///<summary>Converts the span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _6e8546b461b48646(string s, Intl.NumberFormat? provider, object styles, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _79e29a1615b41471(string? s, string? format, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _d8720f2bb55cf0af(string s, string format, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _d1eb33e53764ee27(string? s, object formats, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _685c4ca5481f00e7(string s, object formats, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Adds a specified time interval to a specified date and time, yielding a new date and time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator +(System.DateTime, System.TimeSpan)")]
	public extern static Date _d48b23d7c5f7c2aa(Date d, BigInt t);

	///<summary>Subtracts a specified time interval from a specified date and time and returns a new date and time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator -(System.DateTime, System.TimeSpan)")]
	public extern static Date _8d9ea66839ce392a(Date d, BigInt t);

	///<summary>Subtracts a specified date and time from another specified date and time and returns a time interval.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator -(System.DateTime, System.DateTime)")]
	public extern static BigInt _85b6d162b092ce0e(Date d1, Date d2);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are equal.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator ==(System.DateTime, System.DateTime)")]
	public extern static bool _37d87f65292f7083(Date d1, Date d2);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator !=(System.DateTime, System.DateTime)")]
	public extern static bool _89406f797d33e566(Date d1, Date d2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator <(System.DateTime, System.DateTime)")]
	public extern static bool _5a97e2aec50193b3(Date t1, Date t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator <=(System.DateTime, System.DateTime)")]
	public extern static bool _a8b15168323b118c(Date t1, Date t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator >(System.DateTime, System.DateTime)")]
	public extern static bool _e98b0598f4980bcc(Date t1, Date t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.DateTime.operator >=(System.DateTime, System.DateTime)")]
	public extern static bool _91697ebd6031bb97(Date t1, Date t2);

	///<summary>Deconstructs this <see cref="T:System.DateTime" /> instance by <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />.</summary>
	[Jazor(Op.Discard ,"System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)")]
	public extern static Array<object?> _bcf4183bef96ea21(Date instance, Date date, Number time);

	///<summary>Deconstructs this <see cref="T:System.DateOnly" /> instance by <see cref="P:System.DateTime.Year" />, <see cref="P:System.DateTime.Month" />, and <see cref="P:System.DateTime.Day" />.</summary>
	[Jazor(Op.Discard ,"System.DateTime.Deconstruct(out int, out int, out int)")]
	public extern static Array<object?> _5f721827cf6b8105(Date instance, Number year, Number month, Number day);

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats()")]
	public extern static string[] _8022abe7c2a9b946(Date instance);

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers and the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(System.IFormatProvider)")]
	public extern static string[] _65e4d2ad2f14918c(Date instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(char)")]
	public extern static string[] _daa9858a8adf981d(Date instance, Number format);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier and culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(char, System.IFormatProvider)")]
	public extern static string[] _10c081a451aa4b71(Date instance, Number format, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetTypeCode()")]
	public extern static System.TypeCode _9164c7979da236d5(Date instance);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)")]
	public extern static Array<object?> _6c36c46db30aacc1(string? s, Intl.NumberFormat? provider, Date result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Date _41dcf008ea7cf6d9(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)")]
	public extern static Array<object?> _63fd53f09ba16132(string s, Intl.NumberFormat? provider, Date result);

	[Jazor(Op.Inline, "static System.DateTime.UtcNow.get", "new Date()")]
	public extern static Date _d4c39bdf47f391cf();
}
