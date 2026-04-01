namespace Jazor.CLR;

/// <summary>
/// CLR module for System.TimeOnly，映射成 JavaScript 中的 Number 类型，表示自午夜以来的毫秒数
/// </summary>
[ECMAScriptModule("System/TimeOnlyModule.js")]
[Jazor(Op.Alias, "System.TimeOnly", "Number")]
public static class TimeOnlyModule
{
	[Jazor(Op.Discard ,"System.TimeOnly.TimeOnly()")]
	public extern static Number _9f78f92d0753f4cf();

	/// <summary>
	/// C#: TimeOnly.MinValue (00:00:00)
	/// JS: 0 (ticks as Number)
	/// </summary>
	[Jazor(Op.Inline, "static System.TimeOnly.MinValue.get", "0")]
	public extern static Number _5a02197e2ef2252f();

	/// <summary>
	/// C#: TimeOnly.MaxValue (23:59:59.9999999)
	/// JS: 863999999999 (ticks as Number, but exceeds Number precision, use BigInt)
	/// Note: TimeOnly maps to Number (milliseconds) for simplicity, MaxValue may lose precision
	/// </summary>
	[Jazor(Op.Inline, "static System.TimeOnly.MaxValue.get", "863999999999")]
	public extern static Number _b1d0e19d91dbb54a();

	/// <summary>
	/// C#: new TimeOnly(hour, minute)
	/// JS: hour * 3600000 + minute * 60000 (milliseconds since midnight)
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.TimeOnly(int, int)", "(__arg1 * 3600000 + __arg2 * 60000)")]
	public extern static Number _62d395c56c4c299d(Number hour, Number minute);

	/// <summary>
	/// C#: new TimeOnly(hour, minute, second)
	/// JS: hour * 3600000 + minute * 60000 + second * 1000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.TimeOnly(int, int, int)", "(__arg1 * 3600000 + __arg2 * 60000 + __arg3 * 1000)")]
	public extern static Number _e9a3481b3456aad4(Number hour, Number minute, Number second);

	/// <summary>
	/// C#: new TimeOnly(hour, minute, second, millisecond)
	/// JS: hour * 3600000 + minute * 60000 + second * 1000 + millisecond
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.TimeOnly(int, int, int, int)", "(__arg1 * 3600000 + __arg2 * 60000 + __arg3 * 1000 + __arg4)")]
	public extern static Number _335167098e226ccf(Number hour, Number minute, Number second, Number millisecond);

	/// <summary>
	/// C#: new TimeOnly(hour, minute, second, millisecond, microsecond)
	/// JS: hour * 3600000 + minute * 60000 + second * 1000 + millisecond + microsecond / 1000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.TimeOnly(int, int, int, int, int)", "(__arg1 * 3600000 + __arg2 * 60000 + __arg3 * 1000 + __arg4 + __arg5 / 1000)")]
	public extern static Number _28c8cb012fe0e547(Number hour, Number minute, Number second, Number millisecond, Number microsecond);

	/// <summary>
	/// C#: new TimeOnly(ticks)
	/// JS: Number(ticks) / 10000 (convert ticks to milliseconds)
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.TimeOnly(long)", "Number(__arg1) / 10000")]
	public extern static Number _b8b3b95e8b848f44(BigInt ticks);

	/// <summary>
	/// C#: instance.Hour
	/// JS: Math.floor(instance / 3600000) % 24
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Hour.get", "(Math.floor(__arg1 / 3600000) % 24)")]
	public extern static Number _201ef41481f4e3fb(Number instance);

	/// <summary>
	/// C#: instance.Minute
	/// JS: Math.floor(instance / 60000) % 60
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Minute.get", "(Math.floor(__arg1 / 60000) % 60)")]
	public extern static Number _009addd612610031(Number instance);

	/// <summary>
	/// C#: instance.Second
	/// JS: Math.floor(instance / 1000) % 60
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Second.get", "(Math.floor(__arg1 / 1000) % 60)")]
	public extern static Number _b9481eedd6cbeb99(Number instance);

	/// <summary>
	/// C#: instance.Millisecond
	/// JS: instance % 1000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Millisecond.get", "(__arg1 % 1000)")]
	public extern static Number _3c789a48d39d0010(Number instance);

	/// <summary>
	/// C#: instance.Microsecond
	/// JS: (instance % 1) * 1000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Microsecond.get", "((__arg1 % 1) * 1000)")]
	public extern static Number _a091b803b851e27e(Number instance);

	/// <summary>
	/// C#: instance.Nanosecond
	/// JS: 0 (not supported in JS)
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Nanosecond.get", "0")]
	public extern static Number _656df0ee12e92399(Number instance);

	/// <summary>
	/// C#: instance.Ticks
	/// JS: BigInt(instance) * 10000n
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Ticks.get", "(BigInt(__arg1) * 10000n)")]
	public extern static BigInt _2fd46050126234ac(Number instance);

	/// <summary>
	/// C#: instance.Add(value)
	/// JS: (instance + Number(value) / 10000) % 86400000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.Add(System.TimeSpan)", "((__arg1 + Number(__arg2) / 10000) % 86400000)")]
	public extern static Number _4c935b985e7b6e02(Number instance, BigInt value);

	/// <summary>
	/// C#: instance.Add(value, out wrappedDays)
	/// JS: 返回 [result, wrappedDays]
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Add(System.TimeSpan, out int)")]
	public static Array<object?> _31bb07d031379025(Number instance, BigInt value, Number wrappedDays)
	{
		var total = instance + Number_(value) / 10000;
		var wrapped = Math.Floor_(total / 86400000);
		var result = total % 86400000;
		if (result < 0)
		{
			result += 86400000;
			wrapped--;
		}
		return [result, wrapped];
	}

	/// <summary>
	/// C#: instance.AddHours(value)
	/// JS: (instance + value * 3600000) % 86400000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.AddHours(double)", "((__arg1 + __arg2 * 3600000) % 86400000)")]
	public extern static Number _8e71fa0d2695e84f(Number instance, Number value);

	/// <summary>
	/// C#: instance.AddHours(value, out wrappedDays)
	/// JS: 返回 [result, wrappedDays]
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.AddHours(double, out int)")]
	public static Array<object?> _ad6cad38823a5ef6(Number instance, Number value, Number wrappedDays)
	{
		var total = instance + value * 3600000;
		var wrapped = Math.Floor_(total / 86400000);
		var result = total % 86400000;
		if (result < 0)
		{
			result = result + 86400000;
			wrapped = wrapped - 1;
		}
		return [result, wrapped];
	}

	/// <summary>
	/// C#: instance.AddMinutes(value)
	/// JS: (instance + value * 60000) % 86400000
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.AddMinutes(double)", "((__arg1 + __arg2 * 60000) % 86400000)")]
	public extern static Number _77bd7db30cbf3bc9(Number instance, Number value);

	/// <summary>
	/// C#: instance.AddMinutes(value, out wrappedDays)
	/// JS: 返回 [result, wrappedDays]
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.AddMinutes(double, out int)")]
	public static Array<object?> _e698cb9920401887(Number instance, Number value, Number wrappedDays)
	{
		var total = instance + value * 60000;
		var wrapped = Math.Floor_(total / 86400000);
		var result = total % 86400000;
		if (result < 0)
		{
			result = result + 86400000;
			wrapped = wrapped - 1;
		}
		return [result, wrapped];
	}

	/// <summary>
	/// C#: instance.IsBetween(start, end)
	/// JS: 支持跨午夜的范围检查
	/// </summary>
	[Jazor(Op.Inline, "System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)", "(__arg2 < __arg3 ? (__arg1 >= __arg2 && __arg1 < __arg3) : (__arg1 >= __arg2 || __arg1 < __arg3))")]
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
