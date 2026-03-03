namespace Jazor.CLR;

/// <summary>
/// System.SByte (sbyte) 类型模块映射规则
/// </summary>
[ECMAScriptModule("System/SByteModule.js")]
[Jazor(Op.Alias, "sbyte", "Number")]
public static class SByteModule
{
	/// <summary>
	/// C#: sbyte.MaxValue
	/// JS: 127
	/// </summary>
	[Jazor(Op.Inline, "static sbyte.MaxValue", "127")]
	public extern static Number _maxValue();

	/// <summary>
	/// C#: sbyte.MinValue
	/// JS: -128
	/// </summary>
	[Jazor(Op.Inline, "static sbyte.MinValue", "-128")]
	public extern static Number _minValue();

	[Jazor(Op.Discard ,"sbyte.SByte()")]
	public extern static Number _0b5843a5a69b4fde();

	/// <summary>
	/// C#: sbyte.CompareTo(object)
	/// JS: instance - (obj ?? 0)
	/// </summary>
	[Jazor(Op.Inline, "sbyte.CompareTo(object)", "(@#{0} - (@#{1} ?? 0))")]
	public extern static Number _f8a387725694962f(Number instance, object? obj);

	/// <summary>
	/// C#: sbyte.CompareTo(sbyte)
	/// JS: instance - value
	/// </summary>
	[Jazor(Op.Inline, "sbyte.CompareTo(sbyte)", "(@#{0} - @#{1})")]
	public extern static Number _a0ff7e0ac34c91a8(Number instance, Number value);

	/// <summary>
	/// C#: sbyte.Equals(object)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override sbyte.Equals(object)", "(@#{0} === @#{1})")]
	public extern static bool _74c9452fa767096f(Number instance, object? obj);

	/// <summary>
	/// C#: sbyte.Equals(sbyte)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "sbyte.Equals(sbyte)", "(@#{0} === @#{1})")]
	public extern static bool _4105db2840795661(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override sbyte.GetHashCode()")]
	public extern static Number _5131b1d6df49bbfb(Number instance);

	/// <summary>
	/// C#: sbyte.ToString()
	/// JS: instance.toString()
	/// </summary>
	[Jazor(Op.Alias, "override sbyte.ToString()", "toString")]
	public extern static string _99cd65a77e5cb1e0(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"sbyte.ToString(string)")]
	public extern static string _f1581e4c3d9629b5(Number instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"sbyte.ToString(System.IFormatProvider)")]
	public extern static string _2835ffcd09fe2083(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"sbyte.ToString(string, System.IFormatProvider)")]
	public extern static string _e06a6af137f4a848(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current 8-bit signed integer instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"sbyte.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _cc044b52a705b83a(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"sbyte.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _08ca5484266e1a7b(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: sbyte.Parse(s)
	/// JS: parseInt(s, 10) with validation (-128 to 127)
	/// </summary>
	[Jazor(Op.Import, "static sbyte.Parse(string)")]
	public static Number _fc6fdbb937cb390a(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		var trimmed = s.Trim();
		var num = ParseInt(trimmed, 10);
		if (IsNaN(num))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid SByte.");
		if (num < -128 || num > 127)
			throw new Error($"OverflowException: Value '{s}' was either too large or too small for an SByte.");
		return num;
	}

	///<summary>Converts the string representation of a number in a specified style to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _302c7b4fcff325d8(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string, System.IFormatProvider)")]
	public extern static Number _28a6ad10aa689a4f(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number that is in a specified style and culture-specific format to its 8-bit signed equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _8885d6602b6a8ecd(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number that is in a specified style and culture-specific format to its 8-bit signed equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _49c3ab5496122405(Uint32Array s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: sbyte.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static sbyte.TryParse(string, out sbyte)")]
	public static Array<object?> _d9082c2537283f95(string? s, Number result)
	{
		if (s == null)
			return [false, 0];
		var trimmed = s.Trim();
		var num = ParseInt(trimmed, 10);
		if (IsNaN(num) || num < -128 || num > 127)
			return [false, 0];
		return [true, num];
	}

	///<summary>Tries to convert the span representation of a number to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<char>, out sbyte)")]
	public extern static Array<object?> _a3ccaa03549862bc(Uint32Array s, Number result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<byte>, out sbyte)")]
	public extern static Array<object?> _f25602df99a7ca89(Uint8Array utf8Text, Number result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out sbyte)")]
	public extern static Array<object?> _b5d3ab86487e1092(string? s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out sbyte)")]
	public extern static Array<object?> _9d5e37148ebfe7f5(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.SByte" />.</summary>
	[Jazor(Op.Discard ,"sbyte.GetTypeCode()")]
	public extern static System.TypeCode _05739d4cc5ffd426(Number instance);
}
