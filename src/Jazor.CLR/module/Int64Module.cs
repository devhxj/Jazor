namespace Jazor.CLR;

/// <summary>
/// System.Int64 (long) 类型模块映射规则
///
/// C# long 与 JavaScript BigInt 的对应关系：
/// - C# long 是 64 位有符号整数
/// - JavaScript BigInt 可以精确表示任意大小的整数
///
/// Op 类型选择原则：
/// - Inline: 简单比较和运算
/// - Alias: JS BigInt 方法
/// - Import: 需要完整实现的复杂逻辑（Parse/TryParse）
/// - Discard: 不支持的功能
/// </summary>
[ECMAScriptModule("System/Int64Module.js")]
[Jazor(Op.Alias, "long","BigInt")]
public static class Int64Module
{
	/// <summary>
	/// C#: long.MaxValue
	/// JS: 9223372036854775807n
	/// </summary>
	[Jazor(Op.Inline, "static long.MaxValue", "9223372036854775807n")]
	public extern static BigInt _8aa4e14b6f65b46f();

	/// <summary>
	/// C#: long.MinValue
	/// JS: -9223372036854775808n
	/// </summary>
	[Jazor(Op.Inline, "static long.MinValue", "-9223372036854775808n")]
	public extern static BigInt _0e4e92f6bb2f0389();

	[Jazor(Op.Discard ,"long.Int64()")]
	public extern static BigInt _74cd360dde6bde69();

	///<summary>Produces the full product of two 64-bit numbers.</summary>
	[Jazor(Op.Inline, "static long.BigMul(long, long)", "(__arg1 * __arg2)")]
	public extern static BigInt _62ebef6eaaff4810(BigInt left, BigInt right);

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Import, "long.CompareTo(object)")]
	public static Number _a108636b79b7c8d2(BigInt instance, object? value)
	{
		if (value == null)
			return 1;
		// Check if value is a BigInt
		if (value is BigInt bigIntValue)
			return instance < bigIntValue ? -1 : (instance > bigIntValue ? 1 : 0);
		throw new Error("ArgumentException: Object must be of type Int64.");
	}

	///<summary>Compares this instance to a specified 64-bit signed integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Inline, "long.CompareTo(long)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _e862e3c68f06f9e2(BigInt instance, BigInt value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Inline, "override long.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _3fc2378cd670be8a(BigInt instance, object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int64" /> value.</summary>
	[Jazor(Op.Inline, "long.Equals(long)", "(__arg1 === __arg2)")]
	public extern static bool _73c4e4cb572f07f1(BigInt instance, BigInt obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard, "override long.GetHashCode()")]
	public extern static Number _a6f06b90e3618c16(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Alias, "override long.ToString()", "toString")]
	public extern static string _56beebc0ed49cbc9(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard, "long.ToString(System.IFormatProvider)")]
	public extern static string _c9ee1b2e169f61aa(BigInt instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard, "long.ToString(string)")]
	public extern static string _db383e0ca3b051e6(BigInt instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard, "long.ToString(string, System.IFormatProvider)")]
	public extern static string _88dfcf515abb66f6(BigInt instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current long number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard, "long.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _de37ec26b22b5ff8(BigInt instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard, "long.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _a7f530e78a14a037(BigInt instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: long.Parse(s)
	/// JS: BigInt(s) with validation
	/// </summary>
	[Jazor(Op.Import, "static long.Parse(string)")]
	public static BigInt _4174bb5b72e448a6(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		var trimmed = s.Trim();
		try
		{
			var result = BigInt_(trimmed);
			// Check long range: -9223372036854775808 to 9223372036854775807
			var minValue = BigInt_("-9223372036854775808");
			var maxValue = BigInt_("9223372036854775807");
			if (result < minValue || result > maxValue)
				throw new Error($"OverflowException: Value '{s}' was either too large or too small for an Int64.");
			return result;
		}
		catch
		{
			throw new Error($"FormatException: String '{s}' was not recognized as a valid Int64.");
		}
	}

	///<summary>Converts the string representation of a number in a specified style to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard, "static long.Parse(string, System.Globalization.NumberStyles)")]
	public extern static BigInt _481fbf6d32029fcb(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard, "static long.Parse(string, System.IFormatProvider)")]
	public extern static BigInt _cb7366fbf6242a6a(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard, "static long.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _540038b3f55a1010(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard, "static long.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _78d6c19de30b5937(Uint32Array s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: long.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static long.TryParse(string, out long)")]
	public static Array<object?> _2cba636c245c1675(string? s, BigInt result)
	{
		if (s == null)
			return [false, BigInt.Zero];

		var trimmed = s.Trim();
		try
		{
			var parsed = BigInt_(trimmed);
			// Check long range
			var minValue = BigInt_("-9223372036854775808");
			var maxValue = BigInt_("9223372036854775807");
			if (parsed < minValue || parsed > maxValue)
				return [false, BigInt.Zero];
			return [true, parsed];
		}
		catch
		{
			return [false, BigInt.Zero];
		}
	}

	///<summary>Converts the span representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static long.TryParse(System.ReadOnlySpan<char>, out long)")]
	public extern static Array<object?> _f65dcae3cb8d9ffc(Uint32Array s, BigInt result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard, "static long.TryParse(System.ReadOnlySpan<byte>, out long)")]
	public extern static Array<object?> _8bee07df79eb3a90(Uint8Array utf8Text, BigInt result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static long.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out long)")]
	public extern static Array<object?> _de4d5fc73e6f5f38(string? s, object style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard, "static long.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out long)")]
	public extern static Array<object?> _c1dce355b4dded70(Uint32Array s, object style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int64" />.</summary>
	[Jazor(Op.Discard, "long.GetTypeCode()")]
	public extern static System.TypeCode _5efdcf3dff57ebdf(BigInt instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Import, "static long.DivRem(long, long)")]
	public static (BigInt Quotient, BigInt Remainder) _28273cd350760efe(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");
		var quotient = left / right;
		var remainder = left % right;
		return (quotient, remainder);
	}

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard, "static long.LeadingZeroCount(long)")]
	public extern static BigInt _f67b17bf5c4120f2(BigInt value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Import, "static long.PopCount(long)")]
	public static BigInt _77fd605bbb6ce669(BigInt value)
	{
		var count = BigInt.Zero;
		var v = value;
		while (v > BigInt.Zero)
		{
			count = count + (v & BigInt.One);
			v = v >> BigInt.One;
		}
		return count;
	}

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Import, "static long.RotateLeft(long, int)")]
	public static BigInt _62ef461b6a515b85(BigInt value, Number rotateAmount)
	{
		var amount = BigInt_(rotateAmount % 64);
		if (amount < BigInt.Zero)
			amount = amount + BigInt_(64);
		return (value << amount) | (value >> (BigInt_(64) - amount));
	}

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Import, "static long.RotateRight(long, int)")]
	public static BigInt _6a70bc88f689ce73(BigInt value, Number rotateAmount)
	{
		var amount = BigInt_(rotateAmount % 64);
		if (amount < BigInt.Zero)
			amount = amount + BigInt_(64);
		return (value >> amount) | (value << (BigInt_(64) - amount));
	}

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Import, "static long.TrailingZeroCount(long)")]
	public static BigInt _df6d7288bc845b53(BigInt value)
	{
		if (value == BigInt.Zero)
			return BigInt_(64);
		var count = BigInt.Zero;
		var v = value;
		while ((v & BigInt.One) == BigInt.Zero)
		{
			v = v >> BigInt.One;
			count = count + BigInt.One;
		}
		return count;
	}

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Inline, "static long.IsPow2(long)", "(__arg1 > 0n && (__arg1 & (__arg1 - 1n)) === 0n)")]
	public extern static bool _fd78c89cf0a7feff(BigInt value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard, "static long.Log2(long)")]
	public extern static BigInt _e90fc1096a04c8f9(BigInt value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Import, "static long.Clamp(long, long, long)")]
	public static BigInt _8e63712ecf0da200(BigInt value, BigInt min, BigInt max)
	{
		if (value < min) return min;
		if (value > max) return max;
		return value;
	}

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Import, "static long.CopySign(long, long)")]
	public static BigInt _dd2c6c8297bd4df3(BigInt value, BigInt sign)
	{
		if ((value >= BigInt.Zero) == (sign >= BigInt.Zero))
			return value;
		return -value;
	}

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Inline, "static long.Max(long, long)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _2c60dae3f93fedef(BigInt x, BigInt y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Inline, "static long.Min(long, long)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _e9f5fe363044ceda(BigInt x, BigInt y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Import, "static long.Sign(long)")]
	public static Number _003e583f1faf343b(BigInt value)
	{
		if (value > BigInt.Zero) return 1;
		if (value < BigInt.Zero) return -1;
		return 0;
	}

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Inline, "static long.Abs(long)", "(__arg1 < 0n ? -__arg1 : __arg1)")]
	public extern static BigInt _6ae5b36df368d1e5(BigInt value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static long.CreateChecked<TOther>(TOther)")]
	public extern static BigInt _a7b7a24d0da5bf7e<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static long.CreateSaturating<TOther>(TOther)")]
	public extern static BigInt _bef5211a1d823672<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard, "static long.CreateTruncating<TOther>(TOther)")]
	public extern static BigInt _363a470fd9444076<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Inline, "static long.IsEvenInteger(long)", "(__arg1 % 2n === 0n)")]
	public extern static bool _203dfc08764b3516(BigInt value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Inline, "static long.IsNegative(long)", "(__arg1 < 0n)")]
	public extern static bool _cac37e2db2e55b1b(BigInt value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Inline, "static long.IsOddInteger(long)", "(__arg1 % 2n !== 0n)")]
	public extern static bool _23594f30886ac699(BigInt value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Inline, "static long.IsPositive(long)", "(__arg1 > 0n)")]
	public extern static bool _3c8be08897a76569(BigInt value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Import, "static long.MaxMagnitude(long, long)")]
	public static BigInt _9618dc0d855ee729(BigInt x, BigInt y)
	{
		var absX = x < BigInt.Zero ? -x : x;
		var absY = y < BigInt.Zero ? -y : y;
		return absX >= absY ? x : y;
	}

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Import, "static long.MinMagnitude(long, long)")]
	public static BigInt _bfad1ee52075b36e(BigInt x, BigInt y)
	{
		var absX = x < BigInt.Zero ? -x : x;
		var absY = y < BigInt.Zero ? -y : y;
		return absX <= absY ? x : y;
	}

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard, "static long.TryParse(string, System.IFormatProvider, out long)")]
	public extern static Array<object?> _6f90bee529e2eb6c(string? s, Intl.NumberFormat? provider, BigInt result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard, "static long.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static BigInt _22b931abaca743ae(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard, "static long.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out long)")]
	public extern static Array<object?> _1fa9b46a2b1345f4(Uint32Array s, Intl.NumberFormat? provider, BigInt result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static long.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _37d384b6ca28fb02(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static long.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out long)")]
	public extern static Array<object?> _0ea07687b9ce11f1(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static long.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static BigInt _45277b7b17f7a046(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard, "static long.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out long)")]
	public extern static Array<object?> _232a05c0262521da(Uint8Array utf8Text, Intl.NumberFormat? provider, BigInt result);
}
