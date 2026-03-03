namespace Jazor.CLR;

/// <summary>
/// System.Byte (byte) 类型模块映射规则
///
/// C# byte 与 JavaScript 的对应关系：
/// - C# byte 是 8 位无符号整数 (0-255)
/// - JavaScript Number 是 64 位浮点数，可以精确表示 8 位整数
///
/// Op 类型选择原则：
/// - Allowed: 操作符（+ - * / % == != &lt; &gt; &lt;= &gt;=）
/// - Inline: 简单比较和运算
/// - Alias: JS Math 方法
/// - Import: 需要完整实现的复杂逻辑（Parse/TryParse）
/// - Discard: 不支持的功能
/// </summary>
[ECMAScriptModule("System/ByteModule.js")]
[Jazor(Op.Alias, "byte","Number")]
public static class ByteModule
{
	[Jazor(Op.Discard ,"byte.Byte()")]
	public extern static Number _c16a6a35ab0f1a78();

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Inline, "byte.CompareTo(object)", "(@#{0} - (@#{1} ?? 0))")]
	public extern static Number _7aaf4c67dc6c9c9a(Number instance, object? value);

	///<summary>Compares this instance to a specified 8-bit unsigned integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Inline, "byte.CompareTo(byte)", "(@#{0} - @#{1})")]
	public extern static Number _5c935ae4273a32cf(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Inline, "override byte.Equals(object)", "(@#{0} === @#{1})")]
	public extern static bool _991f10ab45b84c4a(Number instance, object? obj);

	///<summary>Returns a value indicating whether this instance and a specified byte object represent the same value.</summary>
	[Jazor(Op.Inline, "byte.Equals(byte)", "(@#{0} === @#{1})")]
	public extern static bool _4885d24d76ef9f6d(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard, "override byte.GetHashCode()")]
	public extern static Number _0db3f15e7e706cc7(Number instance);

	///<summary>Converts the value of the current byte object to its equivalent string representation.</summary>
	[Jazor(Op.Alias, "override byte.ToString()", "toString")]
	public extern static string _fe5d1bb114dd9985(Number instance);

	///<summary>Converts the value of the current byte object to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard, "byte.ToString(string)")]
	public extern static string _94ac453822a347f8(Number instance, string? format);

	///<summary>Converts the numeric value of the current byte object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard, "byte.ToString(System.IFormatProvider)")]
	public extern static string _0c8b56bfa65bb1f8(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current byte object to its equivalent string representation using the specified format and culture-specific formatting information.</summary>
	[Jazor(Op.Discard, "byte.ToString(string, System.IFormatProvider)")]
	public extern static string _6dae7e6c4a7c6261(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current 8-bit unsigned integer instance into the provided span of characters.</summary>
	[Jazor(Op.Discard, "byte.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _f2fa96775a8b3f25(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard, "byte.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _a2e5b355ba248fcd(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: byte.Parse(s)
	/// JS: parseInt(s, 10) with validation
	/// </summary>
	[Jazor(Op.Import, "static byte.Parse(string)")]
	public static Number _8719e4b3055c5188(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		var trimmed = s.Trim();
		var num = ParseInt(trimmed, 10);

		// Check if parsing succeeded
		if (IsNaN(num))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid Byte.");

		// 验证 byte 范围: 0-255
		if (num < 0 || num > 255)
			throw new Error($"OverflowException: Value '{s}' was either too large or too small for a Byte.");

		return num;
	}

	///<summary>Converts the string representation of a number in a specified style to its byte equivalent.</summary>
	[Jazor(Op.Discard, "static byte.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _82aa6f31e6873ee2(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its byte equivalent.</summary>
	[Jazor(Op.Discard, "static byte.Parse(string, System.IFormatProvider)")]
	public extern static Number _65691bfd885c413a(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its byte equivalent.</summary>
	[Jazor(Op.Discard, "static byte.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _07f65aff65731222(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its byte equivalent.</summary>
	[Jazor(Op.Discard, "static byte.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _ff08be5970881dca(string s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: byte.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static byte.TryParse(string, out byte)")]
	public static Array<object?> _03c07d3f3ee012f9(string? s, Number result)
	{
		if (s == null)
			return [false, 0];

		var trimmed = s.Trim();
		var v = ParseInt(trimmed, 10);

		// Check if parsing succeeded
		if (IsNaN(v))
			return [false, 0];

		// 验证 byte 范围: 0-255
		if (v >= 0 && v <= 255)
			return [true, v];

		return [false, 0];
	}

	///<summary>Tries to convert the span representation of a number to its byte equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(System.ReadOnlySpan<char>, out byte)")]
	public extern static Array<object?> _413c6f7752002edf(string s, Number result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 8-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(System.ReadOnlySpan<byte>, out byte)")]
	public extern static Array<object?> _0e02bd74e5960e4d(Uint8Array utf8Text, Number result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its byte equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _aed06cdaac60f688(string? s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its byte equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _761e5b49fdeccb96(string s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Returns the TypeCode for value type Byte.</summary>
	[Jazor(Op.Discard, "byte.GetTypeCode()")]
	public extern static System.TypeCode _1695fafe88707bc5(Number instance);

	/// <summary>
	/// C#: byte.DivRem(left, right)
	/// JS: [Math.floor(left / right), left % right]
	/// </summary>
	[Jazor(Op.Import, "static byte.DivRem(byte, byte)")]
	public static (Number Quotient, Number Remainder) _42cbe2ef401fb8c9(Number left, Number right)
	{
		if (right == 0)
			throw new Error("DivideByZeroException");
		var quotient = Math.Floor_(left / right);
		var remainder = left % right;
		return (quotient, remainder);
	}

	/// <summary>
	/// C#: byte.MaxValue
	/// JS: 255
	/// </summary>
	[Jazor(Op.Inline, "static byte.MaxValue", "255")]
	public extern static Number _3d6e5c7f8a9b1234();

	/// <summary>
	/// C#: byte.MinValue
	/// JS: 0
	/// </summary>
	[Jazor(Op.Inline, "static byte.MinValue", "0")]
	public extern static Number _4e7f6d8c9b0a2345();

	/// <summary>
	/// C#: byte.LeadingZeroCount(value)
	/// JS: 8 - Math.clz32(value) for 8-bit
	/// Note: Math.clz32 works on 32-bit, for 8-bit we need to adjust
	/// </summary>
	[Jazor(Op.Import, "static byte.LeadingZeroCount(byte)")]
	public static Number _9526f26e93e4c913(Number value)
	{
		// For 8-bit value, leading zeros = 8 - bit position of highest 1
		// Or use clz32 and subtract 24 (since clz32 treats input as 32-bit)
		var v = value;
		if (v == 0) return 8;

		// Count leading zeros in 8-bit value
		int count = 0;
		if ((v & 0xF0) == 0) { count += 4; v <<= 4; }
		if ((v & 0xC0) == 0) { count += 2; v <<= 2; }
		if ((v & 0x80) == 0) { count += 1; }
		return count;
	}

	/// <summary>
	/// C#: byte.PopCount(value)
	/// JS: 使用位运算计算
	/// </summary>
	[Jazor(Op.Import, "static byte.PopCount(byte)")]
	public static Number _c5ae774e00ea2202(Number value)
	{
		// 汉明权重算法 for 8-bit
		int v = value;
		v = v - ((v >> 1) & 0x55);
		v = (v & 0x33) + ((v >> 2) & 0x33);
		v = (v + (v >> 4)) & 0x0F;
		return v;
	}

	/// <summary>
	/// C#: byte.RotateLeft(value, amount)
	/// JS: ((value << (amount & 7)) | (value >>> (8 - (amount & 7)))) & 0xFF
	/// </summary>
	[Jazor(Op.Inline, "static byte.RotateLeft(byte, int)", "(((@#{0} << (@#{1} & 7)) | (@#{0} >>> (8 - (@#{1} & 7)))) & 0xFF)")]
	public extern static Number _0156fdbf291b637d(Number value, Number rotateAmount);

	/// <summary>
	/// C#: byte.RotateRight(value, amount)
	/// JS: ((value >>> (amount & 7)) | (value << (8 - (amount & 7)))) & 0xFF
	/// </summary>
	[Jazor(Op.Inline, "static byte.RotateRight(byte, int)", "(((@#{0} >>> (@#{1} & 7)) | (@#{0} << (8 - (@#{1} & 7)))) & 0xFF)")]
	public extern static Number _872d6a20e2bf8567(Number value, Number rotateAmount);

	/// <summary>
	/// C#: byte.TrailingZeroCount(value)
	/// JS: 使用位运算
	/// </summary>
	[Jazor(Op.Import, "static byte.TrailingZeroCount(byte)")]
	public static Number _88ad71d45f9ffca7(Number value)
	{
		if (value == 0)
			return 8;

		int v = value;
		int count = 0;
		while ((v & 1) == 0)
		{
			v >>= 1;
			count++;
		}
		return count;
	}

	/// <summary>
	/// C#: byte.IsPow2(value)
	/// JS: value > 0 && (value & (value - 1)) === 0
	/// </summary>
	[Jazor(Op.Inline, "static byte.IsPow2(byte)", "(@#{0} > 0 && (@#{0} & (@#{0} - 1)) === 0)")]
	public extern static bool _b10f7588a1920633(Number value);

	/// <summary>
	/// C#: byte.Log2(value)
	/// JS: Math.floor(Math.log2(value))
	/// </summary>
	[Jazor(Op.Inline, "static byte.Log2(byte)", "Math.floor(Math.log2(@#{0}))")]
	public extern static Number _8f1e70f00149e892(Number value);

	/// <summary>
	/// C#: byte.Clamp(value, min, max)
	/// JS: Math.min(Math.max(value, min), max)
	/// </summary>
	[Jazor(Op.Inline, "static byte.Clamp(byte, byte, byte)", "Math.min(Math.max(@#{0}, @#{1}), @#{2})")]
	public extern static Number _d46830318e177655(Number value, Number min, Number max);

	/// <summary>
	/// C#: byte.Max(x, y)
	/// JS: Math.max(x, y)
	/// </summary>
	[Jazor(Op.Alias, "static byte.Max(byte, byte)", "max")]
	public extern static Number _04555e3eb1c7a9ce(Number x, Number y);

	/// <summary>
	/// C#: byte.Min(x, y)
	/// JS: Math.min(x, y)
	/// </summary>
	[Jazor(Op.Alias, "static byte.Min(byte, byte)", "min")]
	public extern static Number _01cc0a43897afd75(Number x, Number y);

	/// <summary>
	/// C#: byte.Sign(value)
	/// JS: Math.sign(value)
	/// Note: For unsigned byte, always returns 0 or 1
	/// </summary>
	[Jazor(Op.Alias, "static byte.Sign(byte)", "sign")]
	public extern static Number _683fdf4d3120d162(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static byte.CreateChecked<TOther>(TOther)")]
	public extern static Number _3be4135a6878c4f6<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static byte.CreateSaturating<TOther>(TOther)")]
	public extern static Number _cb74080a125947ac<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static byte.CreateTruncating<TOther>(TOther)")]
	public extern static Number _c47aae89c9da8a9f<TOther>(object value);

	/// <summary>
	/// C#: byte.IsEvenInteger(value)
	/// JS: (value & 1) === 0
	/// </summary>
	[Jazor(Op.Inline, "static byte.IsEvenInteger(byte)", "((@#{0} & 1) === 0)")]
	public extern static bool _ed30037c45c0e107(Number value);

	/// <summary>
	/// C#: byte.IsOddInteger(value)
	/// JS: (value & 1) !== 0
	/// </summary>
	[Jazor(Op.Inline, "static byte.IsOddInteger(byte)", "((@#{0} & 1) !== 0)")]
	public extern static bool _bb058beaaa7a9d6f(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(string, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _73bacef10db6dd04(string? s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard, "static byte.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _f09faa9402018245(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _44dd755bac10b090(string s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static byte.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _984889e2fd23e5d8(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _77a3faa6c6a9ad83(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static byte.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _63304d5cac2b30b7(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static byte.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _f7f4e5fabad2e9af(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
