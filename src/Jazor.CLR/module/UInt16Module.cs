namespace Jazor.CLR;

/// <summary>
/// System.UInt16 (ushort) 类型模块映射规则
///
/// C# ushort 与 JavaScript 的对应关系：
/// - C# ushort 是 16 位无符号整数 (0-65535)
/// - JavaScript Number 是 64 位浮点数，可以精确表示 16 位整数
///
/// Op 类型选择原则：
/// - Allowed: 操作符（+ - * / % == != &lt; &gt; &lt;= &gt;=）
/// - Inline: 简单比较和运算
/// - Alias: JS Math 方法
/// - Import: 需要完整实现的复杂逻辑（Parse/TryParse）
/// - Discard: 不支持的功能
/// </summary>
[ECMAScriptModule("System/UInt16Module.js")]
[Jazor(Op.Alias, "ushort", "Number")]
public static class UInt16Module
{
	/// <summary>
	/// C#: ushort.MaxValue
	/// JS: 65535
	/// </summary>
	[Jazor(Op.Inline, "static ushort.MaxValue", "65535")]
	public extern static Number _maxValue();

	/// <summary>
	/// C#: ushort.MinValue
	/// JS: 0
	/// </summary>
	[Jazor(Op.Inline, "static ushort.MinValue", "0")]
	public extern static Number _minValue();

	[Jazor(Op.Discard ,"ushort.UInt16()")]
	public extern static Number _2b4f1af6b7fc0173();

	/// <summary>
	/// C#: ushort.CompareTo(object)
	/// JS: (instance < value ? -1 : (instance > value ? 1 : 0))
	/// </summary>
	[Jazor(Op.Inline, "ushort.CompareTo(object)", "(@#{0} < (@#{1} ?? 0) ? -1 : (@#{0} > (@#{1} ?? 0) ? 1 : 0))")]
	public extern static Number _d8d8b9cba9bd3347(Number instance, object? value);

	/// <summary>
	/// C#: ushort.CompareTo(ushort)
	/// JS: (instance < value ? -1 : (instance > value ? 1 : 0))
	/// </summary>
	[Jazor(Op.Inline, "ushort.CompareTo(ushort)", "(@#{0} < @#{1} ? -1 : (@#{0} > @#{1} ? 1 : 0))")]
	public extern static Number _2ca53dc375a8ff3d(Number instance, Number value);

	/// <summary>
	/// C#: ushort.Equals(object)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override ushort.Equals(object)", "(@#{0} === @#{1})")]
	public extern static bool _c13e06040702dab1(Number instance, object? obj);

	/// <summary>
	/// C#: ushort.Equals(ushort)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "ushort.Equals(ushort)", "(@#{0} === @#{1})")]
	public extern static bool _0faff9447540bf0f(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard, "override ushort.GetHashCode()")]
	public extern static Number _1289c3b26567b431(Number instance);

	/// <summary>
	/// C#: ushort.ToString()
	/// JS: instance.toString()
	/// </summary>
	[Jazor(Op.Alias, "override ushort.ToString()", "toString")]
	public extern static string _97b1f766a137a176(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard, "ushort.ToString(System.IFormatProvider)")]
	public extern static string _54f6d55d2ab58603(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard, "ushort.ToString(string)")]
	public extern static string _6f22376b1343fe81(Number instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard, "ushort.ToString(string, System.IFormatProvider)")]
	public extern static string _a995cb7019a823da(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current unsigned short number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard, "ushort.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _72607726c0ca8cb0(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard, "ushort.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _c8d9586ea188f250(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: ushort.Parse(s)
	/// JS: parseInt(s, 10) with validation
	/// </summary>
	[Jazor(Op.Import, "static ushort.Parse(string)")]
	public static Number _bfae72f49db4f3c9(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		var trimmed = s.Trim();
		var num = ParseInt(trimmed, 10);

		// Check if parsing succeeded
		if (IsNaN(num))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid UInt16.");

		// 验证 ushort 范围: 0-65535
		if (num < 0 || num > 65535)
			throw new Error($"OverflowException: Value '{s}' was either too large or too small for a UInt16.");

		return num;
	}

	///<summary>Converts the string representation of a number in a specified style to its 16-bit unsigned integer equivalent. This method is not CLS-compliant. The CLS-compliant alternative is <see cref="M:System.Int32.Parse(System.String,System.Globalization.NumberStyles)" />.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _fa01aff4be2733da(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 16-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(string, System.IFormatProvider)")]
	public extern static Number _c90f18e22ef793ae(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _2d47dd2f7572ac82(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _e0537feda3434747(Uint32Array s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: ushort.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static ushort.TryParse(string, out ushort)")]
	public static Array<object?> _2efd27d401f7def7(string? s, Number result)
	{
		if (s == null)
			return [false, 0];

		var trimmed = s.Trim();
		var v = ParseInt(trimmed, 10);

		// Check if parsing succeeded
		if (IsNaN(v))
			return [false, 0];

		// 验证 ushort 范围: 0-65535
		if (v >= 0 && v <= 65535)
			return [true, v];

		return [false, 0];
	}

	///<summary>Tries to convert the span representation of a number to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(System.ReadOnlySpan<char>, out ushort)")]
	public extern static Array<object?> _0103a8bec9e9dfd7(Uint32Array s, Number result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 16-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(System.ReadOnlySpan<byte>, out ushort)")]
	public extern static Array<object?> _f90ee83a31a4d447(Uint8Array utf8Text, Number result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)")]
	public extern static Array<object?> _0427e1fa823cd14c(string? s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)")]
	public extern static Array<object?> _e1ac1ed9e4df0694(Uint32Array s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt16" />.</summary>
	[Jazor(Op.Discard, "ushort.GetTypeCode()")]
	public extern static System.TypeCode _496bf7ba2bb081f6(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Discard, "static ushort.DivRem(ushort, ushort)")]
	public extern static (ushort Quotient, ushort Remainder) _80e78c0aa0b98fef(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard, "static ushort.LeadingZeroCount(ushort)")]
	public extern static Number _680a923d09b804b9(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Discard, "static ushort.PopCount(ushort)")]
	public extern static Number _2ea0cab4f3f489d9(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Discard, "static ushort.RotateLeft(ushort, int)")]
	public extern static Number _81462814a6e17f8a(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Discard, "static ushort.RotateRight(ushort, int)")]
	public extern static Number _68cb080f188abe14(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Discard, "static ushort.TrailingZeroCount(ushort)")]
	public extern static Number _08ec622fc4aabafb(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard, "static ushort.IsPow2(ushort)")]
	public extern static bool _5e7a013434210fd3(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard, "static ushort.Log2(ushort)")]
	public extern static Number _3e54056b3d1e32ad(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard, "static ushort.Clamp(ushort, ushort, ushort)")]
	public extern static Number _cfa99d1fe078f42e(Number value, Number min, Number max);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard, "static ushort.Max(ushort, ushort)")]
	public extern static Number _baf95be10fbe1b99(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard, "static ushort.Min(ushort, ushort)")]
	public extern static Number _5bde9c15f7f8b2f9(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard, "static ushort.Sign(ushort)")]
	public extern static Number _40243528ed598d7c(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static ushort.CreateChecked<TOther>(TOther)")]
	public extern static Number _5f125252b32ddf67<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static ushort.CreateSaturating<TOther>(TOther)")]
	public extern static Number _d885c6bcbc91e10a<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static ushort.CreateTruncating<TOther>(TOther)")]
	public extern static Number _e7b18638be92c02a<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard, "static ushort.IsEvenInteger(ushort)")]
	public extern static bool _9efbbf8cbd046a16(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard, "static ushort.IsOddInteger(ushort)")]
	public extern static bool _fc6357bc14bbd89b(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(string, System.IFormatProvider, out ushort)")]
	public extern static Array<object?> _815a123a217a57dc(string? s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _37538c358921bcf3(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out ushort)")]
	public extern static Array<object?> _57f6f9049f0201c4(Uint32Array s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _e04a106a21529984(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)")]
	public extern static Array<object?> _8b4f59ba7c1bec8d(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static ushort.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _b0cfeeee7dd4575a(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static ushort.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out ushort)")]
	public extern static Array<object?> _9a6ea927f4cb63da(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
