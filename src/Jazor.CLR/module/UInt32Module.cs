namespace Jazor.CLR;

/// <summary>
/// System.UInt32 (uint) 类型模块映射规则
///
/// C# uint 与 JavaScript Number 的对应关系：
/// - C# uint 是 32 位无符号整数 (0 到 4294967295)
/// - JavaScript Number 可以精确表示 32 位整数
///
/// Op 类型选择原则：
/// - Inline: 简单比较和运算
/// - Alias: JS Number 方法
/// - Import: 需要完整实现的复杂逻辑（Parse/TryParse）
/// - Discard: 不支持的功能
/// </summary>
[ECMAScriptModule("System/UInt32Module.js")]
[Jazor(Op.Alias, "uint", "Number")]
public static class UInt32Module
{
	private static bool TryParseUInt32Core(string? s, out Number value)
	{
		value = 0;
		if (s == null)
			return false;

		var trimmed = s.Trim();
		if (trimmed.Length == 0)
			return false;

		var start = 0;
		var first = trimmed[0];
		if (first == '+' || first == '-')
		{
			if (trimmed.Length == 1)
				return false;

			start = 1;
		}

		for (var i = start; i < trimmed.Length; i++)
		{
			var ch = trimmed[i];
			if (ch < '0' || ch > '9')
				return false;
		}

		var parsed = NumberValue(trimmed);
		if (IsNaN(parsed) || Math.FloorFunc(parsed) != parsed)
			return false;
		if (parsed < 0 || parsed > 4294967295)
			return false;

		value = parsed;
		return true;
	}

	private static bool IsOverflowUInt32Text(string? s)
	{
		if (s == null)
			return false;

		var trimmed = s.Trim();
		if (trimmed.Length == 0)
			return false;

		var start = 0;
		var first = trimmed[0];
		if (first == '+' || first == '-')
		{
			if (trimmed.Length == 1)
				return false;

			start = 1;
		}

		for (var i = start; i < trimmed.Length; i++)
		{
			var ch = trimmed[i];
			if (ch < '0' || ch > '9')
				return false;
		}

		var parsed = NumberValue(trimmed);
		if (IsNaN(parsed) || Math.FloorFunc(parsed) != parsed)
			return false;

		return parsed < 0 || parsed > 4294967295;
	}

	/// <summary>
	/// C#: uint.MaxValue
	/// JS: 4294967295
	/// </summary>
	[Jazor(Op.Inline, "static uint.MaxValue", "4294967295")]
	public extern static Number _8aa4e14b6f65b46f();

	/// <summary>
	/// C#: uint.MinValue
	/// JS: 0
	/// </summary>
	[Jazor(Op.Inline, "static uint.MinValue", "0")]
	public extern static Number _0e4e92f6bb2f0389();

	[Jazor(Op.Inline ,"uint.UInt32()", "0")]
	public extern static Number _3221bd6546b20843();

	///<summary>Produces the full product of two unsigned 32-bit numbers.</summary>
	[Jazor(Op.Inline ,"static uint.BigMul(uint, uint)", "(BigInt(__arg1) * BigInt(__arg2))")]
	public extern static BigInt _e37a28b31d6aed2a(Number left, Number right);

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Import, "uint.CompareTo(object)")]
	public static Number _75ff3ca18f13f709(Number instance, object? value)
	{
		if (value == null)
			return 1;
		if (TypeOf(value) != "number")
			throw new Error("ArgumentException: Object must be of type UInt32.");

		var number = (Number)value;
		return instance < number ? -1 : (instance > number ? 1 : 0);
	}

	///<summary>Compares this instance to a specified 32-bit unsigned integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Inline, "uint.CompareTo(uint)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _7a5a26a8548c61fe(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Inline, "override uint.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _ab3e546a9bf4a9ed(Number instance, object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.UInt32" />.</summary>
	[Jazor(Op.Inline, "uint.Equals(uint)", "(__arg1 === __arg2)")]
	public extern static bool _cb191ad5776dddb3(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Inline, "override uint.GetHashCode()", "(__arg1 | 0)")]
	public extern static Number _d42f9fcffa604eb2(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Alias, "override uint.ToString()", "toString")]
	public extern static string _d124667433f8250d(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"uint.ToString(System.IFormatProvider)")]
	public extern static string _500b36e328db064b(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard ,"uint.ToString(string)")]
	public extern static string _4302afe1e5cd00ac(Number instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"uint.ToString(string, System.IFormatProvider)")]
	public extern static string _fe3cdafc7f93e6fe(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current unsigned integer number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"uint.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _519529f606407c2c(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"uint.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _b67b688ee02ca4a7(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: uint.Parse(s)
	/// JS: 只接受十进制整数字符串，拒绝尾随垃圾字符
	/// </summary>
	[Jazor(Op.Import, "static uint.Parse(string)")]
	public static Number _eb335b8243aba32a(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		if (!TryParseUInt32Core(s, out var value))
		{
			if (IsOverflowUInt32Text(s))
				throw new Error("OverflowException: Value was either too large or too small for a UInt32.");
			throw new Error($"FormatException: String '{s}' was not recognized as a valid UInt32.");
		}

		return value;
	}

	///<summary>Converts the string representation of a number in a specified style to its 32-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _fa26f9c9f654b5c1(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 32-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(string, System.IFormatProvider)")]
	public extern static Number _1d4807141f77fb88(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _6cdc33a0f7e151b0(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _88d9113a364b2858(Uint32Array s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: uint.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static uint.TryParse(string, out uint)")]
	public static Array<object?> _ad4f3364f146e5da(string? s, Number result)
	{
		if (!TryParseUInt32Core(s, out var value))
			return [false, 0];

		return [true, value];
	}

	///<summary>Tries to convert the span representation of a number to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import, "static uint.TryParse(System.ReadOnlySpan<char>, out uint)")]
	public static Array<object?> _104b334d48c2aecd(string s, Number result)
		=> _ad4f3364f146e5da(s, result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 32-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Import, "static uint.TryParse(System.ReadOnlySpan<byte>, out uint)")]
	public static Array<object?> _2526f7e27fec4657(Uint8Array utf8Text, Number result)
		=> _ad4f3364f146e5da(RuntimeModule.TryDecodeUtf8(utf8Text), result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static uint.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out uint)")]
	public extern static Array<object?> _b3e8340b7e951baf(string? s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static uint.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out uint)")]
	public extern static Array<object?> _11ae080219d3fb62(Uint32Array s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt32" />.</summary>
	[Jazor(Op.Inline ,"uint.GetTypeCode()", "10")]
	public extern static System.TypeCode _64eb872ab8e376c7(Number instance);

	/// <summary>
	/// C#: uint.DivRem(left, right)
	/// JS: 无符号整除场景可稳定按 floor 计算商
	/// </summary>
	[Jazor(Op.Import ,"static uint.DivRem(uint, uint)")]
	public static (uint Quotient, uint Remainder) _8a073d758132b5bb(Number left, Number right)
	{
		if (right == 0)
			throw new Error("DivideByZeroException");
		var quotient = Math.FloorFunc(left / right);
		var remainder = left % right;
		return ((uint)quotient, (uint)remainder);
	}

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Inline ,"static uint.LeadingZeroCount(uint)", "Math.clz32(__arg1)")]
	public extern static Number _6ca4bd298f6f135e(Number value);

	/// <summary>
	/// C#: uint.PopCount(value)
	/// JS: 使用 32 位汉明权重算法，helper 比长 Inline 更稳定
	/// </summary>
	[Jazor(Op.Import ,"static uint.PopCount(uint)")]
	public static Number _96cd49e102b39e5b(Number value)
	{
		uint v = (uint)value;
		v = v - ((v >> 1) & 0x55555555);
		v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
		v = (v + (v >> 4)) & 0x0F0F0F0F;
		v = v + (v >> 8);
		v = v + (v >> 16);
		return (int)(v & 0x3F);
	}

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Inline ,"static uint.RotateLeft(uint, int)", "((((__arg1 << (__arg2 & 31)) | (__arg1 >>> (32 - (__arg2 & 31))))) >>> 0)")]
	public extern static Number _580f8710a620f39b(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Inline ,"static uint.RotateRight(uint, int)", "((((__arg1 >>> (__arg2 & 31)) | (__arg1 << (32 - (__arg2 & 31))))) >>> 0)")]
	public extern static Number _465afaf2de09680f(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Inline ,"static uint.TrailingZeroCount(uint)", "(__arg1 === 0 ? 32 : (31 - Math.clz32(((__arg1 >>> 0) & (-(__arg1 >>> 0))))))")]
	public extern static Number _769ecbbaac253539(Number value);

	/// <summary>
	/// C#: uint.IsPow2(value)
	/// JS: value > 0 && (value & (value - 1)) === 0
	/// </summary>
	[Jazor(Op.Inline ,"static uint.IsPow2(uint)", "(__arg1 > 0 && (__arg1 & (__arg1 - 1)) === 0)")]
	public extern static bool _8beae23a85345e63(Number value);

	/// <summary>
	/// C#: uint.Log2(value)
	/// JS: Math.floor(Math.log2(value))
	/// </summary>
	[Jazor(Op.Inline ,"static uint.Log2(uint)", "Math.floor(Math.log2(__arg1))")]
	public extern static Number _6cb21d474b7a30db(Number value);

	/// <summary>
	/// C#: uint.Clamp(value, min, max)
	/// JS: Math.min(Math.max(value, min), max)
	/// </summary>
	[Jazor(Op.Inline ,"static uint.Clamp(uint, uint, uint)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _3693c701aa9899c6(Number value, Number min, Number max);

	/// <summary>
	/// C#: uint.Max(x, y)
	/// JS: Math.max(x, y)
	/// </summary>
	[Jazor(Op.Inline ,"static uint.Max(uint, uint)", "Math.max(__arg1, __arg2)")]
	public extern static Number _f284eae007e1fb6d(Number x, Number y);

	/// <summary>
	/// C#: uint.Min(x, y)
	/// JS: Math.min(x, y)
	/// </summary>
	[Jazor(Op.Inline ,"static uint.Min(uint, uint)", "Math.min(__arg1, __arg2)")]
	public extern static Number _4f3e77f684e65319(Number x, Number y);

	/// <summary>
	/// C#: uint.Sign(value)
	/// JS: value === 0 ? 0 : 1
	/// </summary>
	[Jazor(Op.Inline ,"static uint.Sign(uint)", "(__arg1 === 0 ? 0 : 1)")]
	public extern static Number _5942eb8a5b8a3bcc(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static uint.CreateChecked<TOther>(TOther)")]
	public extern static Number _6af9e09d7ede9ef2<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static uint.CreateSaturating<TOther>(TOther)")]
	public extern static Number _7235beab29d2d5ee<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static uint.CreateTruncating<TOther>(TOther)")]
	public extern static Number _a70daf7a8645e3f0<TOther>(object value);

	/// <summary>
	/// C#: uint.IsEvenInteger(value)
	/// JS: (value & 1) === 0
	/// </summary>
	[Jazor(Op.Inline ,"static uint.IsEvenInteger(uint)", "((__arg1 & 1) === 0)")]
	public extern static bool _e2d0c1e7c0661ad2(Number value);

	/// <summary>
	/// C#: uint.IsOddInteger(value)
	/// JS: (value & 1) !== 0
	/// </summary>
	[Jazor(Op.Inline ,"static uint.IsOddInteger(uint)", "((__arg1 & 1) !== 0)")]
	public extern static bool _9c66512cee42f6d9(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static uint.TryParse(string, System.IFormatProvider, out uint)")]
	public extern static Array<object?> _69bfc426d401ae5e(string? s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _526ccc55a20da9a9(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static uint.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out uint)")]
	public extern static Array<object?> _2a0e1fb1dbc0c5ec(Uint32Array s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _33fc7a36a7feaa04(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static uint.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out uint)")]
	public extern static Array<object?> _fdfb10ed1305e83d(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static uint.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _594553ddcab879cd(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static uint.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out uint)")]
	public extern static Array<object?> _515b8388710d931d(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
