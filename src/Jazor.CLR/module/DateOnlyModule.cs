namespace Jazor.CLR;

/// <summary>
/// 将 System.DateOnly 映射为不含时区和时间部分的日期结构。
/// </summary>
/// <remarks>
/// DateOnly 不能直接等同于 JavaScript Date，因为后者包含时间和时区解释。
/// 模块内部以稳定的 day number/年月日结构执行运算，再按白名单约定发射到 runtime carrier。
/// </remarks>
[ECMAScriptModule("System/DateOnlyModule.js")]
[Jazor(Op.Alias, "System.DateOnly","Object")]
public static class DateOnlyModule
{
	private static Number MaxDayNumber => 3652058;
	private static Number AllowedDateTimeStylesMask => 7;

	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFunc(value) != value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
			throw new Error(message);
	}

	private static RuntimeModule.JDateOnly AddMonthsCore(RuntimeModule.JDateOnly instance, Number months)
	{
		EnsureWholeNumber(months, "ArgumentOutOfRangeException: Months value must be a whole number.");

		var monthIndex = (instance.Year - 1) * 12 + (instance.Month - 1) + months;
		var newYear = Math.FloorFunc(monthIndex / 12) + 1;
		var newMonthIndex = monthIndex % 12;
		if (newMonthIndex < 0)
			newMonthIndex += 12;

		var newMonth = newMonthIndex + 1;
		var daysInMonth = RuntimeModule.GetDaysInMonth(newYear, newMonth);
		var newDay = instance.Day > daysInMonth ? daysInMonth : instance.Day;
		return new RuntimeModule.JDateOnly(newYear, newMonth, newDay);
	}

	private static RuntimeModule.JDateOnly CreateFromDayNumber(Number dayNumber)
	{
		EnsureWholeNumber(dayNumber, "ArgumentOutOfRangeException: Day number must be a whole number.");
		if (dayNumber < 0 || dayNumber > MaxDayNumber)
			throw new Error("ArgumentOutOfRangeException: Day number must be within the range of DateOnly.");

		var date = RuntimeModule.CreateUtcDate(1, 1, 1);
		date.SetUTCDate(date.GetUTCDate() + dayNumber);
		return new RuntimeModule.JDateOnly(date.GetUTCFullYear(), date.GetUTCMonth() + 1, date.GetUTCDate());
	}

	private static Number GetDateTimeKind(System.DateTimeKind kind)
	{
		var value = NumberValue((int)kind);
		if (value != 0 && value != 1 && value != 2)
			throw new Error("ArgumentException: Invalid DateTimeKind value.");

		return value;
	}

	private static bool IsAsciiDigit(char value)
		=> value >= '0' && value <= '9';

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

		year = NumberValue(text.Substring(0, 4));
		month = NumberValue(text.Substring(5, 2));
		day = NumberValue(text.Substring(8, 2));
		if (year < 1 || year > 9999 || month < 1 || month > 12)
			return false;

		var daysInMonth = RuntimeModule.GetDaysInMonth(year, month);
		return day >= 1 && day <= daysInMonth;
	}

	private static bool HasIsoDatePrefix(string text)
		=> text.Length >= 10
			&& text[4] == '-'
			&& text[7] == '-'
			&& IsAsciiDigit(text[0])
			&& IsAsciiDigit(text[1])
			&& IsAsciiDigit(text[2])
			&& IsAsciiDigit(text[3])
			&& IsAsciiDigit(text[5])
			&& IsAsciiDigit(text[6])
			&& IsAsciiDigit(text[8])
			&& IsAsciiDigit(text[9]);

	private static RuntimeModule.JDateOnly ParseCore(string s)
	{
		var text = s.Trim();
		if (text.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid DateOnly.");

		if (TryParseIsoDate(text, out var year, out var month, out var day))
			return new RuntimeModule.JDateOnly(year, month, day);

		// JS Date 会归一化无效 ISO 日期；DateOnly 必须保留 CLR 的严格日历边界。
		if (HasIsoDatePrefix(text))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid DateOnly.");

		var parsed = new Date(text);
		if (IsNaN(parsed.GetTime()))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid DateOnly.");

		return new RuntimeModule.JDateOnly(parsed.GetFullYear(), parsed.GetMonth() + 1, parsed.GetDate());
	}

	private static Number GetDateTimeStylesValue(object style)
	{
		// DateTimeStyles 在当前 lowering 中会擦除为数值字面量。
		if (style is Number numberStyle)
			return numberStyle;
		if (style == null)
			return 0;

		throw new Error("ArgumentException: Invalid DateTimeStyles value.");
	}

	private static bool IsSupportedDateTimeStyles(Number style)
		=> style >= 0 && Math.FloorFunc(style) == style && (style & ~AllowedDateTimeStylesMask) == 0;

	[Jazor(Op.Import ,"System.DateOnly.DateOnly()")]
	public static RuntimeModule.JDateOnly _5f8053a9657a0844() => new(1, 1, 1);

	/// <summary>
	/// C#: DateOnly.MinValue (0001-01-01)
	/// JS: wrapper for 0001-01-01
	/// </summary>
	[Jazor(Op.Import, "static System.DateOnly.MinValue.get")]
	public static RuntimeModule.JDateOnly _4ab7a6677b34a52b() => new(1, 1, 1);

	/// <summary>
	/// C#: DateOnly.MaxValue (9999-12-31)
	/// JS: wrapper for 9999-12-31
	/// </summary>
	[Jazor(Op.Import, "static System.DateOnly.MaxValue.get")]
	public static RuntimeModule.JDateOnly _d3542025e0317ea5() => new(9999, 12, 31);

	/// <summary>
	/// C#: new DateOnly(year, month, day)
	/// JS: create wrapper
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DateOnly(int, int, int)")]
	public static RuntimeModule.JDateOnly _8c5a25d777626c6c(Number year, Number month, Number day) => new(year, month, day);

	/// <summary>
	/// C#: new DateOnly(year, month, day, calendar)
	/// JS: create wrapper
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)")]
	public static RuntimeModule.JDateOnly _c0568bfa1df0ef59(Number year, Number month, Number day, object calendar) => new(year, month, day);

	/// <summary>
	/// C#: DateOnly.FromDayNumber(dayNumber)
	/// JS: add dayNumber days to 0001-01-01
	/// </summary>
	[Jazor(Op.Import, "static System.DateOnly.FromDayNumber(int)")]
	public static RuntimeModule.JDateOnly _96a80b211a70154c(Number dayNumber)
		=> CreateFromDayNumber(dayNumber);

	/// <summary>
	/// C#: instance.Year
	/// JS: instance.year
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.Year.get")]
	public static Number _eeb6f43b5386f459(RuntimeModule.JDateOnly instance)
		=> instance.Year;

	/// <summary>
	/// C#: instance.Month
	/// JS: instance.month
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.Month.get")]
	public static Number _c189199a72fa745c(RuntimeModule.JDateOnly instance)
		=> instance.Month;

	/// <summary>
	/// C#: instance.Day
	/// JS: instance.day
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.Day.get")]
	public static Number _fa637ab5d7ac92a4(RuntimeModule.JDateOnly instance)
		=> instance.Day;

	/// <summary>
	/// C#: instance.DayOfWeek
	/// JS: compute from wrapper date fields
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DayOfWeek.get")]
	public static System.DayOfWeek _faf7aaba77d4de0c(RuntimeModule.JDateOnly instance)
	{
		var date = RuntimeModule.CreateUtcDate(instance.Year, instance.Month, instance.Day);
		return (System.DayOfWeek)(int)date.GetUTCDay();
	}

	/// <summary>
	/// C#: instance.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DayOfYear.get")]
	public static Number _6eb4f28206445ae2(RuntimeModule.JDateOnly instance)
	{
		var firstDayNumber = new RuntimeModule.JDateOnly(instance.Year, 1, 1).DayNumber;
		return instance.DayNumber - firstDayNumber + 1;
	}

	/// <summary>
	/// C#: instance.DayNumber
	/// JS: instance.dayNumber
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DayNumber.get")]
	public static Number _04663ba34bb3359d(RuntimeModule.JDateOnly instance)
		=> instance.DayNumber;

	/// <summary>
	/// C#: instance.AddDays(value)
	/// JS: use UTC day arithmetic to avoid month rollover bugs
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.AddDays(int)")]
	public static RuntimeModule.JDateOnly _cb25738994c034e6(RuntimeModule.JDateOnly instance, Number value)
	{
		EnsureWholeNumber(value, "ArgumentOutOfRangeException: Days value must be a whole number.");
		var date = RuntimeModule.CreateUtcDate(instance.Year, instance.Month, instance.Day);
		date.SetUTCDate(date.GetUTCDate() + value);
		return new RuntimeModule.JDateOnly(date.GetUTCFullYear(), date.GetUTCMonth() + 1, date.GetUTCDate());
	}

	/// <summary>
	/// C#: instance.AddMonths(value)
	/// JS: use UTC month arithmetic
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.AddMonths(int)")]
	public static RuntimeModule.JDateOnly _48134214e63fd9f3(RuntimeModule.JDateOnly instance, Number value)
		=> AddMonthsCore(instance, value);

	/// <summary>
	/// C#: instance.AddYears(value)
	/// JS: use UTC year arithmetic
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.AddYears(int)")]
	public static RuntimeModule.JDateOnly _267d01eded65ff1c(RuntimeModule.JDateOnly instance, Number value)
	{
		EnsureWholeNumber(value, "ArgumentOutOfRangeException: Years value must be a whole number.");
		return AddMonthsCore(instance, value * 12);
	}

	///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are equal.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)")]
	public static bool _82086262cc7cfc9f(RuntimeModule.JDateOnly left, RuntimeModule.JDateOnly right)
		=> left.DayNumber == right.DayNumber;

	///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are not equal.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)")]
	public static bool _56cd63706d2066a6(RuntimeModule.JDateOnly left, RuntimeModule.JDateOnly right)
		=> left.DayNumber != right.DayNumber;

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is later than another specified DateTime.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.operator >(System.DateOnly, System.DateOnly)")]
	public static bool _9b5d78026d232bd9(RuntimeModule.JDateOnly left, RuntimeModule.JDateOnly right)
		=> left.DayNumber > right.DayNumber;

	///<summary>Determines whether one specified DateOnly represents a date that is the same as or later than another specified <see cref="T:System.DateOnly" />.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)")]
	public static bool _0c9d48e09790b085(RuntimeModule.JDateOnly left, RuntimeModule.JDateOnly right)
		=> left.DayNumber >= right.DayNumber;

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is earlier than another specified <see cref="T:System.DateOnly" />.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.operator <(System.DateOnly, System.DateOnly)")]
	public static bool _5384e5a8b5389bd2(RuntimeModule.JDateOnly left, RuntimeModule.JDateOnly right)
		=> left.DayNumber < right.DayNumber;

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> represents a date that is the same as or earlier than another specified <see cref="T:System.DateOnly" />.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)")]
	public static bool _ba9123a74024d518(RuntimeModule.JDateOnly left, RuntimeModule.JDateOnly right)
		=> left.DayNumber <= right.DayNumber;

	///<summary>Deconstructs <see cref="T:System.DateOnly" /> by <see cref="P:System.DateOnly.Year" />, <see cref="P:System.DateOnly.Month" />, and <see cref="P:System.DateOnly.Day" />.</summary>
	[Jazor(Op.Import ,"System.DateOnly.Deconstruct(out int, out int, out int)")]
	public static Array<object?> _87be25300884e7c8(RuntimeModule.JDateOnly instance, Number year, Number month, Number day)
		=> [instance.Year, instance.Month, instance.Day];

	///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToDateTime(System.TimeOnly)")]
	public static RuntimeModule.JDateTime _877770696b013f43(RuntimeModule.JDateOnly instance, RuntimeModule.JTimeOnly time)
	{
		var totalMilliseconds = NumberValue(time.Ticks / BigIntValue(10000));
		var subMillisecondTicks = time.Ticks % BigIntValue(10000);
		var hour = Math.FloorFunc(totalMilliseconds / 3600000);
		var minute = Math.FloorFunc(totalMilliseconds / 60000) % 60;
		var second = Math.FloorFunc(totalMilliseconds / 1000) % 60;
		var millisecond = totalMilliseconds % 1000;
		return new RuntimeModule.JDateTime(RuntimeModule.CreateLocalDateTime(instance.Year, instance.Month, instance.Day, hour, minute, second, millisecond), 0, subMillisecondTicks);
	}

	///<summary>Returns a <see cref="T:System.DateTime" /> instance with the specified input kind that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _458cbe4dafb71f56(RuntimeModule.JDateOnly instance, RuntimeModule.JTimeOnly time, System.DateTimeKind kind)
	{
		var result = _877770696b013f43(instance, time);
		return new RuntimeModule.JDateTime(result.Date, GetDateTimeKind(kind), result.SubMillisecondTicks);
	}

	///<summary>Returns a <see cref="T:System.DateOnly" /> instance that is set to the date part of the specified <paramref name="dateTime" />.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.FromDateTime(System.DateTime)")]
	public static RuntimeModule.JDateOnly _8aa4a7a01276329d(RuntimeModule.JDateTime dateTime)
	{
		return new RuntimeModule.JDateOnly(dateTime.Date.GetFullYear(), dateTime.Date.GetMonth() + 1, dateTime.Date.GetDate());
	}

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateOnly" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
	[Jazor(Op.Import ,"System.DateOnly.CompareTo(System.DateOnly)")]
	public static Number _e80970d38580b553(RuntimeModule.JDateOnly instance, RuntimeModule.JDateOnly value)
	{
		if (instance.DayNumber < value.DayNumber)
			return -1;
		if (instance.DayNumber > value.DayNumber)
			return 1;
		return 0;
	}

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
	[Jazor(Op.Import ,"System.DateOnly.CompareTo(object)")]
	public static Number _519a37b30f165f47(RuntimeModule.JDateOnly instance, object? value)
	{
		if (value == null)
			return 1;

		var other = value as RuntimeModule.JDateOnly;
		if (other == null)
			throw new Error("ArgumentException: Object must be of type DateOnly.");

		return _e80970d38580b553(instance, other);
	}

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateOnly" /> instance.</summary>
	[Jazor(Op.Import ,"System.DateOnly.Equals(System.DateOnly)")]
	public static bool _3c738069b4f977d8(RuntimeModule.JDateOnly instance, RuntimeModule.JDateOnly value)
		=> instance.DayNumber == value.DayNumber;

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Import ,"override System.DateOnly.Equals(object)")]
	public static bool _48e30250a65786cc(RuntimeModule.JDateOnly instance, object? value)
	{
		var other = value as RuntimeModule.JDateOnly;
		return other != null && _3c738069b4f977d8(instance, other);
	}

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import ,"override System.DateOnly.GetHashCode()")]
	public static Number _6ea6fdcc8ab0282e(RuntimeModule.JDateOnly instance)
		=> instance.DayNumber;

	///<summary>Converts a memory span that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateOnly _ec2f441fb253f83c(string s, Intl.NumberFormat? provider, object style)
	{
		var styleValue = GetDateTimeStylesValue(style);
		if (!IsSupportedDateTimeStyles(styleValue))
			throw new Error("ArgumentException: The only supported DateTimeStyles values are AllowLeadingWhite, AllowTrailingWhite, AllowInnerWhite, and AllowWhiteSpaces.");

		return ParseCore(s);
	}

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateOnly _d26bf763250fffed(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[])")]
	public extern static RuntimeModule.JDateOnly _87edc293654333fc(string s, object formats);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateOnly _6a107ddeb5c38aec(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.Parse(string)")]
	public static RuntimeModule.JDateOnly _e2640560d207afce(string s)
		=> ParseCore(s);

	///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateOnly _60b758dae2c14037(string s, Intl.NumberFormat? provider, object style)
		=> _ec2f441fb253f83c(s, provider, style);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string)")]
	public extern static RuntimeModule.JDateOnly _350d290351e50952(string s, string format);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateOnly _f626c308f69f76e8(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string[])")]
	public extern static RuntimeModule.JDateOnly _cf94a659a6885bb2(string s, object formats);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateOnly _930ff81377f0d857(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)")]
	public static Array<object?> _589f2bd8e9539a93(string s, RuntimeModule.JDateOnly result)
		=> _b14e4d5a572477d0(s, result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public static Array<object?> _0df2e2de9cba3b73(string s, Intl.NumberFormat? provider, object style, RuntimeModule.JDateOnly result)
	{
		var styleValue = GetDateTimeStylesValue(style);
		if (!IsSupportedDateTimeStyles(styleValue))
			return [false, new RuntimeModule.JDateOnly(1, 1, 1)];

		return _b14e4d5a572477d0(s, result);
	}

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.DateOnly)")]
	public extern static Array<object?> _73f1ae967191e31e(string s, string format, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" />equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _c9bb733ce9acfea6(string s, string format, Intl.NumberFormat? provider, object style, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.DateOnly)")]
	public extern static Array<object?> _8f1847f9d4121feb(string s, object formats, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _de5feefce32f12d9(string s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.TryParse(string, out System.DateOnly)")]
	public static Array<object?> _b14e4d5a572477d0(string? s, RuntimeModule.JDateOnly result)
	{
		if (s == null || s.Length == 0)
			return [false, new RuntimeModule.JDateOnly(1, 1, 1)];

		try
		{
			return [true, ParseCore(s)];
		}
		catch
		{
			return [false, new RuntimeModule.JDateOnly(1, 1, 1)];
		}
	}

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public static Array<object?> _025d467c3006d36b(string? s, Intl.NumberFormat? provider, object style, RuntimeModule.JDateOnly result)
	{
		var styleValue = GetDateTimeStylesValue(style);
		if (!IsSupportedDateTimeStyles(styleValue))
			return [false, new RuntimeModule.JDateOnly(1, 1, 1)];

		return _b14e4d5a572477d0(s, result);
	}

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string, out System.DateOnly)")]
	public extern static Array<object?> _7c0f60b3f5622bbb(string? s, string? format, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _19011c99380ebcfa(string? s, string? format, Intl.NumberFormat? provider, object style, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string[], out System.DateOnly)")]
	public extern static Array<object?> _c86325a1740751c5(string? s, object formats, RuntimeModule.JDateOnly result);

	///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _5326a681dc11fed4(string? s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JDateOnly result);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent long date string representation.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToLongDateString()")]
	public static string _28b00aeb94d7ea8a(RuntimeModule.JDateOnly instance)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent short date string representation.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToShortDateString()")]
	public static string _2853e304d94edbd5(RuntimeModule.JDateOnly instance)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the formatting conventions of the current culture.            The <see cref="T:System.DateOnly" /> object will be formatted in short form.</summary>
	[Jazor(Op.Alias ,"override System.DateOnly.ToString()", "toString")]
	public extern static string _a44c07083341cf3a(RuntimeModule.JDateOnly instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToString(string)")]
	public static string _5dd96e58e55f801c(RuntimeModule.JDateOnly instance, string? format)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToString(System.IFormatProvider)")]
	public static string _4a8e04add813d3bc(RuntimeModule.JDateOnly instance, Intl.NumberFormat? provider)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateOnly.ToString(string, System.IFormatProvider)")]
	public static string _6135867fb7290a07(RuntimeModule.JDateOnly instance, string? format, Intl.NumberFormat? provider)
		=> instance.ToString();

	///<summary>Tries to format the value of the current <see cref="T:System.DateOnly" /> instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _7bef8f375eb344b2(RuntimeModule.JDateOnly instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _435ac9e098a3389c(RuntimeModule.JDateOnly instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.Parse(string, System.IFormatProvider)")]
	public static RuntimeModule.JDateOnly _90dcc7a43f944613(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)")]
	public static Array<object?> _09af445002e82710(string? s, Intl.NumberFormat? provider, RuntimeModule.JDateOnly result)
		=> _b14e4d5a572477d0(s, result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static RuntimeModule.JDateOnly _18323464e5af4054(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)")]
	public static Array<object?> _e876a9d582a79f6a(string s, Intl.NumberFormat? provider, RuntimeModule.JDateOnly result)
		=> _b14e4d5a572477d0(s, result);
}
