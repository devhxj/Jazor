using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.DateOnly", WhiteListOp.Allowed, null,"System/DateOnlyModule.js")]
public static class DateOnlyModule
{
	[WhiteList("System.DateOnly.DateOnly()", WhiteListOp.Discard)]
	public extern static Date _5f8053a9657a0844();

	[WhiteList("static System.DateOnly.MinValue.get", WhiteListOp.Discard)]
	public extern static Date _4ab7a6677b34a52b(Date instance);

	[WhiteList("static System.DateOnly.MaxValue.get", WhiteListOp.Discard)]
	public extern static Date _d3542025e0317ea5(Date instance);

	///<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified year, month, and day.</summary>
	[WhiteList("System.DateOnly.DateOnly(int, int, int)", WhiteListOp.Discard)]
	public extern static Date _8c5a25d777626c6c(Number year, Number month, Number day);

	///<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified year, month, and day for the specified calendar.</summary>
	[WhiteList("System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)", WhiteListOp.Discard)]
	public extern static Date _c0568bfa1df0ef59(Number year, Number month, Number day, GregorianCalendar calendar);

	///<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified number of days.</summary>
	[WhiteList("static System.DateOnly.FromDayNumber(int)", WhiteListOp.Discard)]
	public extern static Date _96a80b211a70154c(Number dayNumber);

	[WhiteList("System.DateOnly.Year.get", WhiteListOp.Discard)]
	public extern static Number _eeb6f43b5386f459(Date instance);

	[WhiteList("System.DateOnly.Month.get", WhiteListOp.Discard)]
	public extern static Number _c189199a72fa745c(Date instance);

	[WhiteList("System.DateOnly.Day.get", WhiteListOp.Discard)]
	public extern static Number _fa637ab5d7ac92a4(Date instance);

	[WhiteList("System.DateOnly.DayOfWeek.get", WhiteListOp.Discard)]
	public extern static System.DayOfWeek _faf7aaba77d4de0c(Date instance);

	[WhiteList("System.DateOnly.DayOfYear.get", WhiteListOp.Discard)]
	public extern static Number _6eb4f28206445ae2(Date instance);

	[WhiteList("System.DateOnly.DayNumber.get", WhiteListOp.Discard)]
	public extern static Number _04663ba34bb3359d(Date instance);

	///<summary>Adds the specified number of days to the value of this instance.</summary>
	[WhiteList("System.DateOnly.AddDays(int)", WhiteListOp.Discard)]
	public extern static Date _cb25738994c034e6(Date instance, Number value);

	///<summary>Adds the specified number of months to the value of this instance.</summary>
	[WhiteList("System.DateOnly.AddMonths(int)", WhiteListOp.Discard)]
	public extern static Date _48134214e63fd9f3(Date instance, Number value);

	///<summary>Adds the specified number of years to the value of this instance.</summary>
	[WhiteList("System.DateOnly.AddYears(int)", WhiteListOp.Discard)]
	public extern static Date _267d01eded65ff1c(Date instance, Number value);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are equal.</summary>
	[WhiteList("static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)", WhiteListOp.Allowed)]
	public extern static bool _82086262cc7cfc9f(Date left, Date right);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are not equal.</summary>
	[WhiteList("static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)", WhiteListOp.Allowed)]
	public extern static bool _56cd63706d2066a6(Date left, Date right);

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is later than another specified DateTime.</summary>
	[WhiteList("static System.DateOnly.operator >(System.DateOnly, System.DateOnly)", WhiteListOp.Allowed)]
	public extern static bool _9b5d78026d232bd9(Date left, Date right);

	///<summary>Determines whether one specified DateOnly represents a date that is the same as or later than another specified <see cref="T:System.DateOnly" />.</summary>
	[WhiteList("static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)", WhiteListOp.Allowed)]
	public extern static bool _0c9d48e09790b085(Date left, Date right);

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is earlier than another specified <see cref="T:System.DateOnly" />.</summary>
	[WhiteList("static System.DateOnly.operator <(System.DateOnly, System.DateOnly)", WhiteListOp.Allowed)]
	public extern static bool _5384e5a8b5389bd2(Date left, Date right);

	///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> represents a date that is the same as or earlier than another specified <see cref="T:System.DateOnly" />.</summary>
	[WhiteList("static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)", WhiteListOp.Allowed)]
	public extern static bool _ba9123a74024d518(Date left, Date right);

	///<summary>Deconstructs <see cref="T:System.DateOnly" /> by <see cref="P:System.DateOnly.Year" />, <see cref="P:System.DateOnly.Month" />, and <see cref="P:System.DateOnly.Day" />.</summary>
	[WhiteList("System.DateOnly.Deconstruct(out int, out int, out int)", WhiteListOp.Discard)]
	public extern static void _87be25300884e7c8(Date instance, Box<Number> year, Box<Number> month, Box<Number> day);

	///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
	[WhiteList("System.DateOnly.ToDateTime(System.TimeOnly)", WhiteListOp.Discard)]
	public extern static Date _877770696b013f43(Date instance, Number time);

	///<summary>Returns a <see cref="T:System.DateTime" /> instance with the specified input kind that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
	[WhiteList("System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _458cbe4dafb71f56(Date instance, Number time, object kind);

	///<summary>Returns a <see cref="T:System.DateOnly" /> instance that is set to the date part of the specified <paramref name="dateTime" />.</summary>
	[WhiteList("static System.DateOnly.FromDateTime(System.DateTime)", WhiteListOp.Discard)]
	public extern static Date _8aa4a7a01276329d(Date dateTime);

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateOnly" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
	[WhiteList("System.DateOnly.CompareTo(System.DateOnly)", WhiteListOp.Discard)]
	public extern static Number _e80970d38580b553(Date instance, Date value);

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
	[WhiteList("System.DateOnly.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _519a37b30f165f47(Date instance, Object? value);

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateOnly" /> instance.</summary>
	[WhiteList("System.DateOnly.Equals(System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _3c738069b4f977d8(Date instance, Date value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override System.DateOnly.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _48e30250a65786cc(Date instance, Object? value);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override System.DateOnly.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _6ea6fdcc8ab0282e(Date instance);

	///<summary>Converts a memory span that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[WhiteList("static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _ec2f441fb253f83c(Uint32Array s, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _d26bf763250fffed(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[])", WhiteListOp.Discard)]
	public extern static Date _87edc293654333fc(Uint32Array s, object formats);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _6a107ddeb5c38aec(Uint32Array s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using the conventions of the current culture.</summary>
	[WhiteList("static System.DateOnly.Parse(string)", WhiteListOp.Discard)]
	public extern static Date _e2640560d207afce(object s);

	///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[WhiteList("static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _60b758dae2c14037(object s, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(string, string)", WhiteListOp.Discard)]
	public extern static Date _350d290351e50952(object s, object format);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _f626c308f69f76e8(object s, object format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(string, string[])", WhiteListOp.Discard)]
	public extern static Date _cf94a659a6885bb2(object s, object formats);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _930ff81377f0d857(object s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _589f2bd8e9539a93(Uint32Array s, Box<Date> result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _0df2e2de9cba3b73(Uint32Array s, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _73f1ae967191e31e(Uint32Array s, Uint32Array format, Box<Date> result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" />equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _c9bb733ce9acfea6(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _8f1847f9d4121feb(Uint32Array s, object formats, Box<Date> result);

	///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _de5feefce32f12d9(Uint32Array s, object formats, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParse(string, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _b14e4d5a572477d0(object s, Box<Date> result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _025d467c3006d36b(object s, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(string, string, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _7c0f60b3f5622bbb(object s, object format, Box<Date> result);

	///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _19011c99380ebcfa(object s, object format, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(string, string[], out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _c86325a1740751c5(object s, object formats, Box<Date> result);

	///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _5326a681dc11fed4(object s, object formats, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent long date string representation.</summary>
	[WhiteList("System.DateOnly.ToLongDateString()", WhiteListOp.Discard)]
	public extern static string _28b00aeb94d7ea8a(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent short date string representation.</summary>
	[WhiteList("System.DateOnly.ToShortDateString()", WhiteListOp.Discard)]
	public extern static string _2853e304d94edbd5(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the formatting conventions of the current culture.            The <see cref="T:System.DateOnly" /> object will be formatted in short form.</summary>
	[WhiteList("override System.DateOnly.ToString()", WhiteListOp.Discard)]
	public extern static string _a44c07083341cf3a(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[WhiteList("System.DateOnly.ToString(string)", WhiteListOp.Discard)]
	public extern static string _5dd96e58e55f801c(Date instance, object format);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("System.DateOnly.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _4a8e04add813d3bc(Date instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("System.DateOnly.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _6135867fb7290a07(Date instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current <see cref="T:System.DateOnly" /> instance into the provided span of characters.</summary>
	[WhiteList("System.DateOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _7bef8f375eb344b2(Date instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("System.DateOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _435ac9e098a3389c(Date instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[WhiteList("static System.DateOnly.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Date _90dcc7a43f944613(object s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a string into a value.</summary>
	[WhiteList("static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _09af445002e82710(object s, Intl.NumberFormat? provider, Box<Date> result);

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Date _18323464e5af4054(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[WhiteList("static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)", WhiteListOp.Discard)]
	public extern static bool _e876a9d582a79f6a(Uint32Array s, Intl.NumberFormat? provider, Box<Date> result);
}
