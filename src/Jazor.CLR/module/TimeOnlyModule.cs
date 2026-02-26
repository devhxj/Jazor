namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.TimeOnly","System/TimeOnlyModule.js")]
public static class TimeOnlyModule
{
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly()")]
	public extern static Number _9f78f92d0753f4cf();

	[Jazor(Op.Discard ,"static System.TimeOnly.MinValue.get")]
	public extern static Number _5a02197e2ef2252f();

	[Jazor(Op.Discard ,"static System.TimeOnly.MaxValue.get")]
	public extern static Number _b1d0e19d91dbb54a();

	///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour and the minute.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly(int, int)")]
	public extern static Number _62d395c56c4c299d(Number hour, Number minute);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, and second.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly(int, int, int)")]
	public extern static Number _e9a3481b3456aad4(Number hour, Number minute, Number second);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, second, and millisecond.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly(int, int, int, int)")]
	public extern static Number _335167098e226ccf(Number hour, Number minute, Number second, Number millisecond);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, second, millisecond, and microsecond.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly(int, int, int, int, int)")]
	public extern static Number _28c8cb012fe0e547(Number hour, Number minute, Number second, Number millisecond, Number microsecond);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure using a specified number of ticks.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly(long)")]
	public extern static Number _b8b3b95e8b848f44(BigInt ticks);

	[Jazor(Op.Discard ,"System.TimeOnly.Hour.get")]
	public extern static Number _201ef41481f4e3fb(Number instance);

	[Jazor(Op.Discard ,"System.TimeOnly.Minute.get")]
	public extern static Number _009addd612610031(Number instance);

	[Jazor(Op.Discard ,"System.TimeOnly.Second.get")]
	public extern static Number _b9481eedd6cbeb99(Number instance);

	[Jazor(Op.Discard ,"System.TimeOnly.Millisecond.get")]
	public extern static Number _3c789a48d39d0010(Number instance);

	[Jazor(Op.Discard ,"System.TimeOnly.Microsecond.get")]
	public extern static Number _a091b803b851e27e(Number instance);

	[Jazor(Op.Discard ,"System.TimeOnly.Nanosecond.get")]
	public extern static Number _656df0ee12e92399(Number instance);

	[Jazor(Op.Discard ,"System.TimeOnly.Ticks.get")]
	public extern static BigInt _2fd46050126234ac(Number instance);

	///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the value of the specified time span to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Add(System.TimeSpan)")]
	public extern static Number _4c935b985e7b6e02(Number instance, BigInt value);

	///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the value of the specified time span to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Add(System.TimeSpan, out int)")]
	public extern static Array<object?> _31bb07d031379025(Number instance, BigInt value, Number wrappedDays);

	///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of hours to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.AddHours(double)")]
	public extern static Number _8e71fa0d2695e84f(Number instance, Number value);

	///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of hours to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.AddHours(double, out int)")]
	public extern static Array<object?> _ad6cad38823a5ef6(Number instance, Number value, Number wrappedDays);

	///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of minutes to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.AddMinutes(double)")]
	public extern static Number _77bd7db30cbf3bc9(Number instance, Number value);

	///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of minutes to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.AddMinutes(double, out int)")]
	public extern static Array<object?> _e698cb9920401887(Number instance, Number value, Number wrappedDays);

	///<summary>Determines if a time falls within the range provided.            Supports both "normal" ranges such as 10:00-12:00, and ranges that span midnight such as 23:00-01:00.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _da64e8d379a7e47c(Number instance, Number start, Number end);

	///<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>are equal.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator ==(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _8e47d4212be3070c(Number left, Number right);

	///<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator !=(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _b3b712e75fff0050(Number left, Number right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator >(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _341a3f0fbcda5677(Number left, Number right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator >=(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _0656cf79f08fd69b(Number left, Number right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator <(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _9b001b8f9a72a57d(Number left, Number right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator <=(System.TimeOnly, System.TimeOnly)")]
	public extern static bool _cd098f438100d4cb(Number left, Number right);

	///<summary>Gives the elapsed time between two points on a circular clock, which will always be a positive value.</summary>
	[Jazor(Op.Allowed ,"static System.TimeOnly.operator -(System.TimeOnly, System.TimeOnly)")]
	public extern static BigInt _888a9b439de5e7c1(Number t1, Number t2);

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" /> and <see cref="P:System.TimeOnly.Minute" />.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Deconstruct(out int, out int)")]
	public extern static Array<object?> _d6170153a1f10bc3(Number instance, Number hour, Number minute);

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, and <see cref="P:System.TimeOnly.Second" />.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Deconstruct(out int, out int, out int)")]
	public extern static Array<object?> _d36793074735968e(Number instance, Number hour, Number minute, Number second);

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, and <see cref="P:System.TimeOnly.Millisecond" />.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Deconstruct(out int, out int, out int, out int)")]
	public extern static Array<object?> _b349a5fd892d33be(Number instance, Number hour, Number minute, Number second, Number millisecond);

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, <see cref="P:System.TimeOnly.Millisecond" />, and <see cref="P:System.TimeOnly.Microsecond" />.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Deconstruct(out int, out int, out int, out int, out int)")]
	public extern static Array<object?> _1f5bb15cea73f15b(Number instance, Number hour, Number minute, Number second, Number millisecond, Number microsecond);

	///<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a time span representing the time elapsed since midnight.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.FromTimeSpan(System.TimeSpan)")]
	public extern static Number _df2fe8c100ae98f0(BigInt timeSpan);

	///<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a <see cref="T:System.DateTime" /> representing the time of the day in this <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.FromDateTime(System.DateTime)")]
	public extern static Number _a305982aa6859677(Date dateTime);

	///<summary>Convert the current <see cref="T:System.TimeOnly" /> instance to a <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.ToTimeSpan()")]
	public extern static BigInt _3ae6313d263b390f(Number instance);

	///<summary>Compares the value of this instance to a specified <see cref="T:System.TimeOnly" /> value and indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.CompareTo(System.TimeOnly)")]
	public extern static Number _b08fb6c2056f6cd2(Number instance, Number value);

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.TimeOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.CompareTo(object)")]
	public extern static Number _fa5c092641b8d1d5(Number instance, object? value);

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.TimeOnly" /> instance.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.Equals(System.TimeOnly)")]
	public extern static bool _f6e2f8f76d2b030d(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override System.TimeOnly.Equals(object)")]
	public extern static bool _f70c423884fcb611(Number instance, object? value);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override System.TimeOnly.GetHashCode()")]
	public extern static Number _ec44c7db9ffc5397(Number instance);

	///<summary>Converts a memory span that contains string representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Number _5c89b5211b528926(string s, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Number _7c5c52c213c7d2e0(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[])")]
	public extern static Number _fe05a1ffa3020076(string s, object formats);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Number _b22aa6d58a65860e(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.Parse(string)")]
	public extern static Number _c2335ab7e556bf0b(string s);

	///<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Number _b10aeed232e37ce3(string s, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string)")]
	public extern static Number _716638d6af9e1f50(string s, string format);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Number _464a80539f893705(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span to a <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string[])")]
	public extern static Number _732d047579691da6(string s, object formats);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Number _a753be3cfd781575(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a time to its TimeOnly equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, out System.TimeOnly)")]
	public extern static Array<object?> _94c68599373e4134(string s, Number result);

	///<summary>Converts the specified span representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _33c24989822cc33a(string s, Intl.NumberFormat? provider, object style, Number result);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.TimeOnly)")]
	public extern static Array<object?> _e2de5093ab6411a5(string s, string format, Number result);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _533e30052a71b943(string s, string format, Intl.NumberFormat? provider, object style, Number result);

	///<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.TimeOnly)")]
	public extern static Array<object?> _7949d623f32a801f(string s, object formats, Number result);

	///<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _c88c8d59055208af(string s, object formats, Intl.NumberFormat? provider, object style, Number result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParse(string, out System.TimeOnly)")]
	public extern static Array<object?> _ee7de3e005ab6751(string? s, Number result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _c9d76d7d723eb7f2(string? s, Intl.NumberFormat? provider, object style, Number result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string, out System.TimeOnly)")]
	public extern static Array<object?> _635f76a219a898ce(string? s, string? format, Number result);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _5d909e2eac7e90ea(string? s, string? format, Intl.NumberFormat? provider, object style, Number result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string[], out System.TimeOnly)")]
	public extern static Array<object?> _c464924dd070f03b(string? s, object formats, Number result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _a8c2964fb6e24ce0(string? s, object formats, Intl.NumberFormat? provider, object style, Number result);

	///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent long date string representation.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.ToLongTimeString()")]
	public extern static string _237d7e75836b3e58(Number instance);

	///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.ToShortTimeString()")]
	public extern static string _656ad6fcd28355ef(Number instance);

	///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation using the formatting conventions of the current culture.</summary>
	[Jazor(Op.Discard ,"override System.TimeOnly.ToString()")]
	public extern static string _95a460669a453469(Number instance);

	///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.ToString(string)")]
	public extern static string _b95bf75d8e4cc6af(Number instance, string? format);

	///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.ToString(System.IFormatProvider)")]
	public extern static string _c2fe4568a7f1bbeb(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.ToString(string, System.IFormatProvider)")]
	public extern static string _dd80539f727e11c1(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current TimeOnly instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _d3c7ece118e478fa(Number instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _98dcae3d77df54e1(Number instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.Parse(string, System.IFormatProvider)")]
	public extern static Number _ef54bbdfdbe24915(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParse(string, System.IFormatProvider, out System.TimeOnly)")]
	public extern static Array<object?> _8fea7e8fcaae2f91(string? s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _ae9862bc80a4bba9(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeOnly)")]
	public extern static Array<object?> _1c2553fed0fac496(string s, Intl.NumberFormat? provider, Number result);
}
