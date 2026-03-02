namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.DateOnly","System/DateOnlyModule.js")]
public static class DateOnlyModule
{
	[Jazor(Op.Discard ,"System.DateOnly.DateOnly()")]
	public extern static Date _5f8053a9657a0844();

	/// <summary>
	/// C#: DateOnly.MinValue (0001-01-01)
	/// JS: new Date(1, 0, 1)
	/// </summary>
	[Jazor(Op.Inline, "static System.DateOnly.MinValue.get", "new Date(1, 0, 1)")]
	public extern static Date _4ab7a6677b34a52b();

	/// <summary>
	/// C#: DateOnly.MaxValue (9999-12-31)
	/// JS: new Date(9999, 11, 31)
	/// </summary>
	[Jazor(Op.Inline, "static System.DateOnly.MaxValue.get", "new Date(9999, 11, 31)")]
	public extern static Date _d3542025e0317ea5();

	/// <summary>
	/// C#: new DateOnly(year, month, day)
	/// JS: new Date(year, month - 1, day)
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.DateOnly(int, int, int)", "new Date(@#{0}, @#{1} - 1, @#{2})")]
	public extern static Date _8c5a25d777626c6c(Number year, Number month, Number day);

	/// <summary>
	/// C#: new DateOnly(year, month, day, calendar)
	/// JS: new Date(year, month - 1, day)
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)", "new Date(@#{0}, @#{1} - 1, @#{2})")]
	public extern static Date _c0568bfa1df0ef59(Number year, Number month, Number day, GregorianCalendar calendar);

	/// <summary>
	/// C#: DateOnly.FromDayNumber(dayNumber)
	/// JS: new Date(1, 0, 1 + dayNumber)
	/// </summary>
	[Jazor(Op.Inline, "static System.DateOnly.FromDayNumber(int)", "new Date(1, 0, 1 + @#{0})")]
	public extern static Date _96a80b211a70154c(Number dayNumber);

	/// <summary>
	/// C#: instance.Year
	/// JS: instance.getFullYear()
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.Year.get", "@#{0}.getFullYear()")]
	public extern static Number _eeb6f43b5386f459(Date instance);

	/// <summary>
	/// C#: instance.Month
	/// JS: instance.getMonth() + 1
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.Month.get", "(@#{0}.getMonth() + 1)")]
	public extern static Number _c189199a72fa745c(Date instance);

	/// <summary>
	/// C#: instance.Day
	/// JS: instance.getDate()
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.Day.get", "@#{0}.getDate()")]
	public extern static Number _fa637ab5d7ac92a4(Date instance);

	/// <summary>
	/// C#: instance.DayOfWeek
	/// JS: instance.getDay()
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.DayOfWeek.get", "@#{0}.getDay()")]
	public extern static System.DayOfWeek _faf7aaba77d4de0c(Date instance);

	/// <summary>
	/// C#: instance.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DayOfYear.get")]
	public static Number _6eb4f28206445ae2(Date instance)
	{
		var start = new Date(instance.GetFullYear(), 0, 0);
		var diff = instance.GetTime() - start.GetTime();
		var oneDay = 1000 * 60 * 60 * 24;
		return Math.Floor_(diff / oneDay);
	}

	/// <summary>
	/// C#: instance.DayNumber
	/// JS: 计算从0001-01-01开始的天数
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.DayNumber.get")]
	public static Number _04663ba34bb3359d(Date instance)
	{
		var start = new Date(1, 0, 1);
		var diff = instance.GetTime() - start.GetTime();
		var oneDay = 1000 * 60 * 60 * 24;
		return Math.Floor_(diff / oneDay);
	}

	/// <summary>
	/// C#: instance.AddDays(value)
	/// JS: new Date(instance.getTime() + value * 86400000)
	/// </summary>
	[Jazor(Op.Inline, "System.DateOnly.AddDays(int)", "new Date(@#{0}.getTime() + @#{1} * 86400000)")]
	public extern static Date _cb25738994c034e6(Date instance, Number value);

	/// <summary>
	/// C#: instance.AddMonths(value)
	/// JS: new Date(instance.setMonth(instance.getMonth() + value))
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.AddMonths(int)")]
	public static Date _48134214e63fd9f3(Date instance, Number value)
	{
		var result = new Date(instance.GetTime());
		result.SetMonth(result.GetMonth() + value);
		return result;
	}

	/// <summary>
	/// C#: instance.AddYears(value)
	/// JS: new Date(instance.setFullYear(instance.getFullYear() + value))
	/// </summary>
	[Jazor(Op.Import, "System.DateOnly.AddYears(int)")]
	public static Date _267d01eded65ff1c(Date instance, Number value)
	{
		var result = new Date(instance.GetTime());
		result.SetFullYear(result.GetFullYear() + value);
		return result;
	}

	///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are equal.</summary>
	[Jazor(Op.Allowed ,"static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)")]
	public extern static bool _82086262cc7cfc9f(Date left, Date right);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)")]
	public extern static bool _56cd63706d2066a6(Date left, Date right);

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is later than another specified DateTime.</summary>
	[Jazor(Op.Allowed ,"static System.DateOnly.operator >(System.DateOnly, System.DateOnly)")]
	public extern static bool _9b5d78026d232bd9(Date left, Date right);

	///<summary>Determines whether one specified DateOnly represents a date that is the same as or later than another specified <see cref="T:System.DateOnly" />.</summary>
	[Jazor(Op.Allowed ,"static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)")]
	public extern static bool _0c9d48e09790b085(Date left, Date right);

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is earlier than another specified <see cref="T:System.DateOnly" />.</summary>
	[Jazor(Op.Allowed ,"static System.DateOnly.operator <(System.DateOnly, System.DateOnly)")]
	public extern static bool _5384e5a8b5389bd2(Date left, Date right);

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> represents a date that is the same as or earlier than another specified <see cref="T:System.DateOnly" />.</summary>
	[Jazor(Op.Allowed ,"static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)")]
	public extern static bool _ba9123a74024d518(Date left, Date right);

	///<summary>Deconstructs <see cref="T:System.DateOnly" /> by <see cref="P:System.DateOnly.Year" />, <see cref="P:System.DateOnly.Month" />, and <see cref="P:System.DateOnly.Day" />.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.Deconstruct(out int, out int, out int)")]
	public extern static Array<object?> _87be25300884e7c8(Date instance, Number year, Number month, Number day);

	///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToDateTime(System.TimeOnly)")]
	public extern static Date _877770696b013f43(Date instance, Number time);

	///<summary>Returns a <see cref="T:System.DateTime" /> instance with the specified input kind that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)")]
	public extern static Date _458cbe4dafb71f56(Date instance, Number time, object kind);

	///<summary>Returns a <see cref="T:System.DateOnly" /> instance that is set to the date part of the specified <paramref name="dateTime" />.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.FromDateTime(System.DateTime)")]
	public extern static Date _8aa4a7a01276329d(Date dateTime);

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateOnly" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.CompareTo(System.DateOnly)")]
	public extern static Number _e80970d38580b553(Date instance, Date value);

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.CompareTo(object)")]
	public extern static Number _519a37b30f165f47(Date instance, object? value);

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateOnly" /> instance.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.Equals(System.DateOnly)")]
	public extern static bool _3c738069b4f977d8(Date instance, Date value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override System.DateOnly.Equals(object)")]
	public extern static bool _48e30250a65786cc(Date instance, object? value);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override System.DateOnly.GetHashCode()")]
	public extern static Number _6ea6fdcc8ab0282e(Date instance);

	///<summary>Converts a memory span that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _ec2f441fb253f83c(string s, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _d26bf763250fffed(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[])")]
	public extern static Date _87edc293654333fc(string s, object formats);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _6a107ddeb5c38aec(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.Parse(string)")]
	public extern static Date _e2640560d207afce(string s);

	///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _60b758dae2c14037(string s, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string)")]
	public extern static Date _350d290351e50952(string s, string format);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _f626c308f69f76e8(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string[])")]
	public extern static Date _cf94a659a6885bb2(string s, object formats);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _930ff81377f0d857(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)")]
	public extern static Array<object?> _589f2bd8e9539a93(string s, Date result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _0df2e2de9cba3b73(string s, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.DateOnly)")]
	public extern static Array<object?> _73f1ae967191e31e(string s, string format, Date result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" />equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _c9bb733ce9acfea6(string s, string format, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.DateOnly)")]
	public extern static Array<object?> _8f1847f9d4121feb(string s, object formats, Date result);

	///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _de5feefce32f12d9(string s, object formats, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParse(string, out System.DateOnly)")]
	public extern static Array<object?> _b14e4d5a572477d0(string? s, Date result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _025d467c3006d36b(string? s, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string, out System.DateOnly)")]
	public extern static Array<object?> _7c0f60b3f5622bbb(string? s, string? format, Date result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _19011c99380ebcfa(string? s, string? format, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string[], out System.DateOnly)")]
	public extern static Array<object?> _c86325a1740751c5(string? s, object formats, Date result);

	///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
	public extern static Array<object?> _5326a681dc11fed4(string? s, object formats, Intl.NumberFormat? provider, object style, Date result);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent long date string representation.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToLongDateString()")]
	public extern static string _28b00aeb94d7ea8a(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent short date string representation.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToShortDateString()")]
	public extern static string _2853e304d94edbd5(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the formatting conventions of the current culture.            The <see cref="T:System.DateOnly" /> object will be formatted in short form.</summary>
	[Jazor(Op.Discard ,"override System.DateOnly.ToString()")]
	public extern static string _a44c07083341cf3a(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToString(string)")]
	public extern static string _5dd96e58e55f801c(Date instance, string? format);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToString(System.IFormatProvider)")]
	public extern static string _4a8e04add813d3bc(Date instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.ToString(string, System.IFormatProvider)")]
	public extern static string _6135867fb7290a07(Date instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current <see cref="T:System.DateOnly" /> instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _7bef8f375eb344b2(Date instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _435ac9e098a3389c(Date instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.Parse(string, System.IFormatProvider)")]
	public extern static Date _90dcc7a43f944613(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)")]
	public extern static Array<object?> _09af445002e82710(string? s, Intl.NumberFormat? provider, Date result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Date _18323464e5af4054(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)")]
	public extern static Array<object?> _e876a9d582a79f6a(string s, Intl.NumberFormat? provider, Date result);
}
