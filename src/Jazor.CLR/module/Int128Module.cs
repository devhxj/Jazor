namespace Jazor.CLR;

/// <summary>
/// System.Int128 映射为 JavaScript BigInt，沿用 Int64Module 的整数 lowering 约定。
/// </summary>
[ECMAScriptModule("System/Int128Module.js")]
[Jazor(Op.Alias, "System.Int128", "BigInt")]
public static class Int128Module
{
	private static BigInt MinValueCore => BigIntValue("-170141183460469231731687303715884105728");
	private static BigInt MaxValueCore => BigIntValue("170141183460469231731687303715884105727");
	private static BigInt Mask => BigIntValue("340282366920938463463374607431768211455");
	private static BigInt Modulus => BigIntValue("340282366920938463463374607431768211456");
	private static BigInt SignBit => BigIntValue("170141183460469231731687303715884105728");
	private static BigInt DecimalMinValue => BigIntValue("-79228162514264337593543950335");
	private static BigInt DecimalMaxValue => BigIntValue("79228162514264337593543950335");
	[Jazor(Op.Inline ,"System.Int128.Int128()", "0n")]
	public extern static BigInt _ed2ce49c470c9c69();

	///<summary>Initializes a new instance of the <see cref="T:System.Int128" /> struct.</summary>
	[Jazor(Op.Inline, "System.Int128.Int128(ulong, ulong)", "BigInt.asIntN(128, (__arg1 << 64n) | __arg2)")]
	public extern static BigInt _bd38a63415786b75(BigInt upper, BigInt lower);

	///<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
	[Jazor(Op.Import, "System.Int128.CompareTo(object)")]
	public static Number _b7fcdacf2f88dea3(BigInt instance, object? value)
		=> BigIntIntegerRuntime.CompareToObject(instance, value, "Int128");

	///<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
	[Jazor(Op.Inline, "System.Int128.CompareTo(System.Int128)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _b5794ebe23a72285(BigInt instance, BigInt value);

	///<summary>Determines whether the specified object is equal to the current object.</summary>
	[Jazor(Op.Inline, "override System.Int128.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _3bfa5dfd4837a79e(BigInt instance, object? value);

	///<summary>Indicates whether the current object is equal to another object of the same type.</summary>
	[Jazor(Op.Inline, "System.Int128.Equals(System.Int128)", "(__arg1 === __arg2)")]
	public extern static bool _4031b3e3e167888e(BigInt instance, BigInt value);

	///<summary>Serves as the default hash function.</summary>
	[Jazor(Op.Import, "override System.Int128.GetHashCode()")]
	public static Number _2de13ea6377940aa(BigInt instance)
		=> RuntimeModule.GetInt128HashCode(instance);

	///<summary>Returns a string that represents the current object.</summary>
	[Jazor(Op.Alias, "override System.Int128.ToString()", "toString")]
	public extern static string _0cd70012444338f6(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.Int128.ToString(System.IFormatProvider)")]
	public extern static string _5ea3d4988a658ce9(BigInt instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"System.Int128.ToString(string)")]
	public extern static string _d1745b5899c82324(BigInt instance, string? format);

	///<summary>Formats the value of the current instance using the specified format.</summary>
	[Jazor(Op.Discard ,"System.Int128.ToString(string, System.IFormatProvider)")]
	public extern static string _97d31060bf8b1daf(BigInt instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.Int128.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _e8941fdbfbed9434(BigInt instance, string destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.Int128.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _e9b19593523937bf(BigInt instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.Parse(string)")]
	public static BigInt _e6ba6fd0fe70ed44(string text)
		=> BigIntIntegerRuntime.Parse(text, MinValueCore, MaxValueCore, "Int128");

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.Parse(string, System.Globalization.NumberStyles)")]
	public extern static BigInt _936bf5a339c27f5b(string s, global::System.Globalization.NumberStyles style);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.Parse(string, System.IFormatProvider)")]
	public static BigInt _1a9c00a8ce01999f(string text, Intl.NumberFormat? provider)
		=> _e6ba6fd0fe70ed44(text);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _d4e73c2c718e1112(string s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _7af8b2902ab50959(string s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.TryParse(string, out System.Int128)")]
	public static Array<object?> _14ac4f353ddae82c(string? text, BigInt result)
		=> BigIntIntegerRuntime.TryParse(text, MinValueCore, MaxValueCore);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.TryParse(System.ReadOnlySpan<char>, out System.Int128)")]
	public static Array<object?> _b0e356aabfe72ec2(string text, BigInt result)
		=> BigIntIntegerRuntime.TryParse(text, MinValueCore, MaxValueCore);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 128-bit signed integer equivalent.</summary>
	[Jazor(Op.Import, "static System.Int128.TryParse(System.ReadOnlySpan<byte>, out System.Int128)")]
	public static Array<object?> _b5211e33c4db2da9(Uint8Array utf8Text, BigInt result)
		=> BigIntIntegerRuntime.TryParse(RuntimeModule.TryDecodeUtf8(utf8Text), MinValueCore, MaxValueCore);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Int128)")]
	public extern static Array<object?> _50e334c622e3b4c0(string? s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Int128)")]
	public extern static Array<object?> _8dcf679cab70cfcc(string s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Byte" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator byte(System.Int128)", "Number(BigInt.asUintN(8, __arg1))")]
	public extern static Number _681cce7b9dc3e457(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked byte(System.Int128)")]
	public static Number _75b77707d8797fe4(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigInt.Zero, BigIntValue(255));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Char" /> value.</summary>
	[Jazor(Op.Inline ,"static System.Int128.explicit operator char(System.Int128)", "Number(BigInt.asUintN(16, __arg1))")]
	public extern static Number _2fe34d368b81e0ae(BigInt value);

	[Jazor(Op.Import ,"static System.Int128.explicit operator checked char(System.Int128)")]
	public static Number _f452363cdf448dd6(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigInt.Zero, BigIntValue(65535));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Import, "static System.Int128.explicit operator decimal(System.Int128)")]
	public static string _9e21259a765be818(BigInt value)
		=> BigIntIntegerRuntime.ToDecimal(value, DecimalMinValue, DecimalMaxValue);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Double" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator double(System.Int128)", "Number(__arg1)")]
	public extern static Number _05f30bc6677c8446(BigInt value);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Half" /> value.</summary>
	[Jazor(Op.Import, "static System.Int128.explicit operator System.Half(System.Int128)")]
	public static Number _53c418af5874ca57(BigInt value)
		=> HalfModule.FromBigIntCore(value);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Int16" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator short(System.Int128)", "Number(BigInt.asIntN(16, __arg1))")]
	public extern static Number _f8ee91da89bfbc71(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked short(System.Int128)")]
	public static Number _2f789a7c53d14d8c(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigIntValue(-32768), BigIntValue(32767));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Int32" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator int(System.Int128)", "Number(BigInt.asIntN(32, __arg1))")]
	public extern static Number _ce0386e19232c2f6(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked int(System.Int128)")]
	public static Number _93c11f1447efb175(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigIntValue(-2147483648), BigIntValue(2147483647));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Int64" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator long(System.Int128)", "BigInt.asIntN(64, __arg1)")]
	public extern static BigInt _25359af432a2c2e1(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked long(System.Int128)")]
	public static BigInt _4d6353a3d3f19b88(BigInt value)
		=> BigIntIntegerRuntime.EnsureRange(value, BigIntValue("-9223372036854775808"), BigIntValue("9223372036854775807"));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.IntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.explicit operator nint(System.Int128)")]
	public extern static nint _5c8c8b45c9b929e4(BigInt value);

	[Jazor(Op.Discard ,"static System.Int128.explicit operator checked nint(System.Int128)")]
	public extern static nint _1e364bd0c6e20318(BigInt value);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.SByte" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator sbyte(System.Int128)", "Number(BigInt.asIntN(8, __arg1))")]
	public extern static Number _dd4a635494a253cd(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked sbyte(System.Int128)")]
	public static Number _d08bfb41d3ab6ee2(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigIntValue(-128), BigIntValue(127));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Single" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator float(System.Int128)", "Math.fround(Number(__arg1))")]
	public extern static Number _68d0e51d50e84c44(BigInt value);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt16" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator ushort(System.Int128)", "Number(BigInt.asUintN(16, __arg1))")]
	public extern static Number _ad0dd19a52ac3d36(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked ushort(System.Int128)")]
	public static Number _304df15d6a44df74(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigInt.Zero, BigIntValue(65535));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt32" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator uint(System.Int128)", "Number(BigInt.asUintN(32, __arg1))")]
	public extern static Number _e51f817cdfd73059(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked uint(System.Int128)")]
	public static Number _0ad5d1d4d4f5f677(BigInt value)
		=> BigIntIntegerRuntime.ToCheckedNumber(value, BigInt.Zero, BigIntValue("4294967295"));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt64" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator ulong(System.Int128)", "BigInt.asUintN(64, __arg1)")]
	public extern static BigInt _4f4ad4e5fea9827f(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked ulong(System.Int128)")]
	public static BigInt _0c7f2cd86870d034(BigInt value)
		=> BigIntIntegerRuntime.EnsureRange(value, BigInt.Zero, BigIntValue("18446744073709551615"));

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt128" /> value.</summary>
	[Jazor(Op.Inline, "static System.Int128.explicit operator System.UInt128(System.Int128)", "BigInt.asUintN(128, __arg1)")]
	public extern static BigInt _435090974b9cc147(BigInt value);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked System.UInt128(System.Int128)")]
	public static BigInt _d9f967e451f57e1b(BigInt value)
		=> BigIntIntegerRuntime.EnsureRange(value, BigInt.Zero, MaxValueCore);

	///<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UIntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.explicit operator nuint(System.Int128)")]
	public extern static nuint _59cf51650b95aaab(BigInt value);

	[Jazor(Op.Discard ,"static System.Int128.explicit operator checked nuint(System.Int128)")]
	public extern static nuint _72a141beb21e4813(BigInt value);

	///<summary>Explicitly converts a <see cref="T:System.Decimal" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Import, "static System.Int128.explicit operator System.Int128(decimal)")]
	public static BigInt _ee13322cacfa030d(string value)
		=> BigIntIntegerRuntime.FromDecimal(value, MinValueCore, MaxValueCore);

	///<summary>Explicitly converts a <see cref="T:System.Double" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Import, "static System.Int128.explicit operator System.Int128(double)")]
	public static BigInt _fed29180182d65ba(Number value)
		=> BigIntIntegerRuntime.FromFloatingSaturatingSigned(value, MinValueCore, MaxValueCore);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked System.Int128(double)")]
	public static BigInt _3d7c10f4becbee0b(Number value)
		=> BigIntIntegerRuntime.FromFloatingChecked(value, MinValueCore, MaxValueCore);

	///<summary>Explicitly converts a <see cref="T:System.Single" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Import, "static System.Int128.explicit operator System.Int128(float)")]
	public static BigInt _f0c48afd1cde425d(Number value)
		=> BigIntIntegerRuntime.FromFloatingSaturatingSigned(value, MinValueCore, MaxValueCore);

	[Jazor(Op.Import, "static System.Int128.explicit operator checked System.Int128(float)")]
	public static BigInt _1215d60b3aeb2477(Number value)
		=> BigIntIntegerRuntime.FromFloatingChecked(value, MinValueCore, MaxValueCore);

	///<summary>Implicitly converts a <see cref="T:System.Byte" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(byte)", "BigInt(__arg1)")]
	public extern static BigInt _6c5b5cce56b6a31a(Number value);

	///<summary>Implicitly converts a <see cref="T:System.Char" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline ,"static System.Int128.implicit operator System.Int128(char)", "BigInt(__arg1)")]
	public extern static BigInt _84a75ee38ffb54f3(Number value);

	///<summary>Implicitly converts a <see cref="T:System.Int16" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(short)", "BigInt(__arg1)")]
	public extern static BigInt _aa36c61698e86024(Number value);

	///<summary>Implicitly converts a <see cref="T:System.Int32" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(int)", "BigInt(__arg1)")]
	public extern static BigInt _2692bf3363e99c1b(Number value);

	///<summary>Implicitly converts a <see cref="T:System.Int64" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(long)", "__arg1")]
	public extern static BigInt _d0c6553702fcf78f(BigInt value);

	///<summary>Implicitly converts a <see cref="T:System.IntPtr" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static System.Int128.implicit operator System.Int128(nint)")]
	public extern static BigInt _3a03aa02661aebc0(nint value);

	///<summary>Implicitly converts a <see cref="T:System.SByte" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(sbyte)", "BigInt(__arg1)")]
	public extern static BigInt _405d300a8a4894d7(Number value);

	///<summary>Implicitly converts a <see cref="T:System.UInt16" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(ushort)", "BigInt(__arg1)")]
	public extern static BigInt _992311e2df4638e5(Number value);

	///<summary>Implicitly converts a <see cref="T:System.UInt32" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(uint)", "BigInt(__arg1)")]
	public extern static BigInt _f6497b94c3678d10(Number value);

	///<summary>Implicitly converts a <see cref="T:System.UInt64" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Int128.implicit operator System.Int128(ulong)", "__arg1")]
	public extern static BigInt _fec01f2ce2f5e153(BigInt value);

	///<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a 128-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static System.Int128.implicit operator System.Int128(nuint)")]
	public extern static BigInt _3225d701adcc7f88(nuint value);

	///<summary>Adds two values together to compute their sum.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator +(System.Int128, System.Int128)", "BigInt.asIntN(128, __arg1 + __arg2)")]
	public extern static BigInt _c67744f8c5d96c2b(BigInt left, BigInt right);

	///<summary>Adds two values together to compute their sum.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked +(System.Int128, System.Int128)")]
	public static BigInt _5e6d45782cb5e4a5(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.EnsureRange(left + right, MinValueCore, MaxValueCore);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Import, "static System.Int128.DivRem(System.Int128, System.Int128)")]
	public static (BigInt Quotient, BigInt Remainder) _ca96ebfbc2a38481(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.DivRemSigned(left, right, MinValueCore);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Import, "static System.Int128.LeadingZeroCount(System.Int128)")]
	public static BigInt _d295dfd29150ae75(BigInt value)
		=> BigIntIntegerRuntime.LeadingZeroCount(value, 128, Mask);

	[Jazor(Op.Import, "static System.Int128.Log10(System.Int128)")]
	public static BigInt _f729da8a5282b658(BigInt value)
		=> BigIntIntegerRuntime.Log10(value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Import, "static System.Int128.PopCount(System.Int128)")]
	public static BigInt _9d72e9332fd24f23(BigInt value)
		=> BigIntIntegerRuntime.PopCount(value, Mask);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Import, "static System.Int128.RotateLeft(System.Int128, int)")]
	public static BigInt _d432cd8596dae24f(BigInt value, Number rotateAmount)
		=> BigIntIntegerRuntime.RotateLeft(value, rotateAmount, 128, Mask, Modulus, SignBit, true);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Import, "static System.Int128.RotateRight(System.Int128, int)")]
	public static BigInt _7adeb1315b95c346(BigInt value, Number rotateAmount)
		=> BigIntIntegerRuntime.RotateRight(value, rotateAmount, 128, Mask, Modulus, SignBit, true);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Import, "static System.Int128.TrailingZeroCount(System.Int128)")]
	public static BigInt _7257dc92fb1e4c4c(BigInt value)
		=> BigIntIntegerRuntime.TrailingZeroCount(value, 128, Mask);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Inline, "static System.Int128.IsPow2(System.Int128)", "(__arg1 > 0n && (__arg1 & (__arg1 - 1n)) === 0n)")]
	public extern static bool _d04628a14db21e34(BigInt value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Import, "static System.Int128.Log2(System.Int128)")]
	public static BigInt _f1a059f528650ba2(BigInt value)
		=> BigIntIntegerRuntime.Log2Signed(value);

	///<summary>Computes the bitwise-and of two values.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator &(System.Int128, System.Int128)", "BigInt.asIntN(128, __arg1 & __arg2)")]
	public extern static BigInt _68ca38dcf867541d(BigInt left, BigInt right);

	///<summary>Computes the bitwise-or of two values.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator |(System.Int128, System.Int128)", "BigInt.asIntN(128, __arg1 | __arg2)")]
	public extern static BigInt _a0d88d43c412365e(BigInt left, BigInt right);

	///<summary>Computes the exclusive-or of two values.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator ^(System.Int128, System.Int128)", "BigInt.asIntN(128, __arg1 ^ __arg2)")]
	public extern static BigInt _46659df631c3627f(BigInt left, BigInt right);

	///<summary>Computes the ones-complement representation of a given value.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator ~(System.Int128)", "BigInt.asIntN(128, ~__arg1)")]
	public extern static BigInt _406d8b09e6ec4129(BigInt value);

	///<summary>Compares two values to determine which is less.</summary>
	[Jazor(Op.Allowed, "static System.Int128.operator <(System.Int128, System.Int128)")]
	public extern static bool _3631f568b169b219(BigInt left, BigInt right);

	///<summary>Compares two values to determine which is less or equal.</summary>
	[Jazor(Op.Allowed, "static System.Int128.operator <=(System.Int128, System.Int128)")]
	public extern static bool _7383f0483f670772(BigInt left, BigInt right);

	///<summary>Compares two values to determine which is greater.</summary>
	[Jazor(Op.Allowed, "static System.Int128.operator >(System.Int128, System.Int128)")]
	public extern static bool _811c6d073ef6ca6e(BigInt left, BigInt right);

	///<summary>Compares two values to determine which is greater or equal.</summary>
	[Jazor(Op.Allowed, "static System.Int128.operator >=(System.Int128, System.Int128)")]
	public extern static bool _47979bbf00a44dc5(BigInt left, BigInt right);

	///<summary>Decrements a value.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator --(System.Int128)", "BigInt.asIntN(128, __arg1 - 1n)")]
	public extern static BigInt _76d6ddd943af6ff1(BigInt value);

	///<summary>Decrements a value.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked --(System.Int128)")]
	public static BigInt _1b31f1ebb654733d(BigInt value)
		=> BigIntIntegerRuntime.EnsureRange(value - BigInt.One, MinValueCore, MaxValueCore);

	///<summary>Divides two values together to compute their quotient.</summary>
	[Jazor(Op.Import, "static System.Int128.operator /(System.Int128, System.Int128)")]
	public static BigInt _6357de67d5760485(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.DivideSigned(left, right, MinValueCore);

	///<summary>Divides two values together to compute their quotient.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked /(System.Int128, System.Int128)")]
	public static BigInt _830753b6d4a84cc4(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.DivideSigned(left, right, MinValueCore);

	///<summary>Compares two values to determine equality.</summary>
	[Jazor(Op.Allowed, "static System.Int128.operator ==(System.Int128, System.Int128)")]
	public extern static bool _371d707661ecc52c(BigInt left, BigInt right);

	///<summary>Compares two values to determine inequality.</summary>
	[Jazor(Op.Allowed, "static System.Int128.operator !=(System.Int128, System.Int128)")]
	public extern static bool _299ca1abf18c4811(BigInt left, BigInt right);

	///<summary>Increments a value.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator ++(System.Int128)", "BigInt.asIntN(128, __arg1 + 1n)")]
	public extern static BigInt _8dab4bca565b4529(BigInt value);

	///<summary>Increments a value.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked ++(System.Int128)")]
	public static BigInt _6dacb4c587ca3df1(BigInt value)
		=> BigIntIntegerRuntime.EnsureRange(value + BigInt.One, MinValueCore, MaxValueCore);

	[Jazor(Op.Inline, "static System.Int128.MinValue.get", "-170141183460469231731687303715884105728n")]
	public extern static BigInt _9bb56306acf5a086();

	[Jazor(Op.Inline, "static System.Int128.MaxValue.get", "170141183460469231731687303715884105727n")]
	public extern static BigInt _0f41854e8fe45c4a();

	///<summary>Divides two values together to compute their modulus or remainder.</summary>
	[Jazor(Op.Import, "static System.Int128.operator %(System.Int128, System.Int128)")]
	public static BigInt _6521eedba51d7990(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.RemainderSigned(left, right, MinValueCore);

	///<summary>Multiplies two values together to compute their product.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator *(System.Int128, System.Int128)", "BigInt.asIntN(128, __arg1 * __arg2)")]
	public extern static BigInt _7823e0b640baf5e3(BigInt left, BigInt right);

	///<summary>Multiplies two values together to compute their product.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked *(System.Int128, System.Int128)")]
	public static BigInt _056e8fba577b7eeb(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.EnsureRange(left * right, MinValueCore, MaxValueCore);

	[Jazor(Op.Import, "static System.Int128.BigMul(System.Int128, System.Int128, out System.Int128)")]
	public static Array<BigInt> _d32138c04ddcda2e(BigInt left, BigInt right, BigInt lower)
		=> BigIntIntegerRuntime.BigMulSigned(left, right, 128);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Import, "static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)")]
	public static BigInt _587401c79d5e216e(BigInt value, BigInt min, BigInt max)
		=> BigIntIntegerRuntime.Clamp(value, min, max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Import, "static System.Int128.CopySign(System.Int128, System.Int128)")]
	public static BigInt _2f2f3fb10237971f(BigInt value, BigInt sign)
		=> BigIntIntegerRuntime.CopySignSigned(value, sign, MinValueCore);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Inline, "static System.Int128.Max(System.Int128, System.Int128)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _bbbede4a8d6a94d0(BigInt x, BigInt y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Inline, "static System.Int128.Min(System.Int128, System.Int128)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _b3776eca350d4ad5(BigInt x, BigInt y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Inline, "static System.Int128.Sign(System.Int128)", "(__arg1 > 0n ? 1 : (__arg1 < 0n ? -1 : 0))")]
	public extern static Number _635630e4489249c0(BigInt value);

	[Jazor(Op.Inline, "static System.Int128.One.get", "1n")]
	public extern static BigInt _c1bcc15342fa30d0();

	[Jazor(Op.Inline, "static System.Int128.Zero.get", "0n")]
	public extern static BigInt _69aaad155ef75bb3();

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Import, "static System.Int128.Abs(System.Int128)")]
	public static BigInt _bc93f10cc4270d3d(BigInt value)
		=> BigIntIntegerRuntime.AbsSigned(value, MinValueCore);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Int128.CreateChecked<TOther>(TOther)")]
	public extern static BigInt _44ad6bcbe8d6480c<TOther>(TOther value)
		where TOther : global::System.Numerics.INumberBase<TOther>;

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Int128.CreateSaturating<TOther>(TOther)")]
	public extern static BigInt _81379c94dbf23e09<TOther>(TOther value)
		where TOther : global::System.Numerics.INumberBase<TOther>;

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Int128.CreateTruncating<TOther>(TOther)")]
	public extern static BigInt _2fbfa53df417f6f1<TOther>(TOther value)
		where TOther : global::System.Numerics.INumberBase<TOther>;

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Inline, "static System.Int128.IsEvenInteger(System.Int128)", "(__arg1 % 2n === 0n)")]
	public extern static bool _6b8a91b15afb966d(BigInt value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Inline, "static System.Int128.IsNegative(System.Int128)", "(__arg1 < 0n)")]
	public extern static bool _9027f9d901e94b3a(BigInt value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Inline, "static System.Int128.IsOddInteger(System.Int128)", "(__arg1 % 2n !== 0n)")]
	public extern static bool _265a23c7352a4445(BigInt value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Inline, "static System.Int128.IsPositive(System.Int128)", "(__arg1 >= 0n)")]
	public extern static bool _ab537fdef4fbd602(BigInt value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Import, "static System.Int128.MaxMagnitude(System.Int128, System.Int128)")]
	public static BigInt _829ea04f38a9820e(BigInt x, BigInt y)
		=> BigIntIntegerRuntime.MaxMagnitude(x, y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Import, "static System.Int128.MinMagnitude(System.Int128, System.Int128)")]
	public static BigInt _ef5bdd18c3a981cf(BigInt x, BigInt y)
		=> BigIntIntegerRuntime.MinMagnitude(x, y);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.TryParse(string, System.IFormatProvider, out System.Int128)")]
	public static Array<object?> _c829bcba6a9b9105(string? text, Intl.NumberFormat? provider, BigInt result)
		=> _14ac4f353ddae82c(text, result);

	///<summary>Shifts a value left by a given amount.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator <<(System.Int128, int)", "BigInt.asIntN(128, __arg1 << BigInt(__arg2 & 127))")]
	public extern static BigInt _df6cfd9e1caeef21(BigInt value, Number shiftAmount);

	///<summary>Shifts a value right by a given amount.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator >>(System.Int128, int)", "(__arg1 >> BigInt(__arg2 & 127))")]
	public extern static BigInt _aa3dd6025b84b3af(BigInt value, Number shiftAmount);

	///<summary>Shifts a value right by a given amount.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator >>>(System.Int128, int)", "BigInt.asIntN(128, BigInt.asUintN(128, __arg1) >> BigInt(__arg2 & 127))")]
	public extern static BigInt _9759894c554ab989(BigInt value, Number shiftAmount);

	[Jazor(Op.Inline, "static System.Int128.NegativeOne.get", "-1n")]
	public extern static BigInt _b43cb7b43fce0d14();

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static BigInt _4d90655f04c3cb26(string text, Intl.NumberFormat? provider)
		=> BigIntIntegerRuntime.Parse(text, MinValueCore, MaxValueCore, "Int128");

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import, "static System.Int128.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Int128)")]
	public static Array<object?> _18dfb394fe14fa70(string text, Intl.NumberFormat? provider, BigInt result)
		=> BigIntIntegerRuntime.TryParse(text, MinValueCore, MaxValueCore);

	///<summary>Subtracts two values to compute their difference.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator -(System.Int128, System.Int128)", "BigInt.asIntN(128, __arg1 - __arg2)")]
	public extern static BigInt _88fc4b8cb4eaa1bb(BigInt left, BigInt right);

	///<summary>Subtracts two values to compute their difference.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked -(System.Int128, System.Int128)")]
	public static BigInt _bce2a2f696e0d716(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.EnsureRange(left - right, MinValueCore, MaxValueCore);

	///<summary>Computes the unary negation of a value.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator -(System.Int128)", "BigInt.asIntN(128, -__arg1)")]
	public extern static BigInt _7287b47decce69d8(BigInt value);

	///<summary>Computes the unary negation of a value.</summary>
	[Jazor(Op.Import, "static System.Int128.operator checked -(System.Int128)")]
	public static BigInt _9f88084238b2cecc(BigInt value)
		=> BigIntIntegerRuntime.EnsureRange(-value, MinValueCore, MaxValueCore);

	///<summary>Computes the unary plus of a value.</summary>
	[Jazor(Op.Inline, "static System.Int128.operator +(System.Int128)", "__arg1")]
	public extern static BigInt _03c5cd4887db7285(BigInt value);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _42de94cc986e1c0b(Uint8Array utf8Text, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Int128)")]
	public extern static Array<object?> _345775a0bab572a9(Uint8Array utf8Text, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static BigInt _a68f252adc28b1db(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Int128.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out System.Int128)")]
	public extern static Array<object?> _35d67a7f4feee9b2(Uint8Array utf8Text, Intl.NumberFormat? provider, BigInt result);
}
