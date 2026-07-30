namespace Jazor.CLR;

/// <summary>
/// System.UInt128 映射为 JavaScript BigInt，沿用 UInt64Module 的整数 lowering 约定。
/// </summary>
[ECMAScriptModule("System/UInt128Module.js")]
[Jazor(Op.Alias, "System.UInt128", "BigInt")]
public static class UInt128Module
{
	private static BigInt MinValueCore => BigInt.Zero;
	private static BigInt MaxValueCore => BigIntFn("340282366920938463463374607431768211455");
	private static BigInt Mask => MaxValueCore;
	private static BigInt Modulus => BigIntFn("340282366920938463463374607431768211456");
	[Jazor(Op.Discard ,"System.UInt128.UInt128()")]
	public extern static BigInt _8c61bda013f8b908();

	///<summary>Initializes a new instance of the <see cref="T:System.UInt128" /> struct.</summary>
	[Jazor(Op.Inline, "System.UInt128.UInt128(ulong, ulong)", "((__arg1 << 64n) | __arg2)")]
	public extern static BigInt _460dd8437a181f67(BigInt upper, BigInt lower);

	///<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
	[Jazor(Op.Import, "System.UInt128.CompareTo(object)")]
	public static Number _c1dc559553950096(BigInt instance, object? value)
		=> BigIntIntegerRuntime.CompareToObject(instance, value, "UInt128");

	///<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
	[Jazor(Op.Inline, "System.UInt128.CompareTo(System.UInt128)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _91bc1016db0da25b(BigInt instance, BigInt value);

	///<summary>Determines whether the specified object is equal to the current object.</summary>
	[Jazor(Op.Inline, "override System.UInt128.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _0d272eef1d8d95cb(BigInt instance, object? value);

	///<summary>Indicates whether the current object is equal to another object of the same type.</summary>
	[Jazor(Op.Inline, "System.UInt128.Equals(System.UInt128)", "(__arg1 === __arg2)")]
	public extern static bool _599bc5ece092c79f(BigInt instance, BigInt value);

	///<summary>Serves as the default hash function.</summary>
	[Jazor(Op.Discard ,"override System.UInt128.GetHashCode()")]
	public extern static Number _bd5a3a9523f573e7(BigInt instance);

	///<summary>Returns a string that represents the current object.</summary>
	[Jazor(Op.Alias, "override System.UInt128.ToString()", "toString")]
	public extern static string _2ea689aef6636a36(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.UInt128.ToString(System.IFormatProvider)")]
	public extern static string _0c1a603ac1899034(BigInt instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"System.UInt128.ToString(string)")]
	public extern static string _44e0941d5883f6c8(BigInt instance, string? format);

	///<summary>Formats the value of the current instance using the specified format.</summary>
	[Jazor(Op.Discard ,"System.UInt128.ToString(string, System.IFormatProvider)")]
	public extern static string _bae671fcc030f76a(BigInt instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.UInt128.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _029205a5f1310ecf(BigInt instance, string destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.UInt128.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _03bb4d378248cadd(BigInt instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.Parse(string)")]
	public static BigInt _30fed79ec71cc7e4(string? text)
		=> BigIntIntegerRuntime.Parse(text, MinValueCore, MaxValueCore, "UInt128");

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Parse(string, System.Globalization.NumberStyles)")]
	public extern static BigInt _0f1308db09adb315(string s, object style);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.Parse(string, System.IFormatProvider)")]
	public static BigInt _6d4342f227a4fbad(string? text, Intl.NumberFormat? provider)
		=> _30fed79ec71cc7e4(text);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _a58539dfaa0aa547(string s, object style, Intl.NumberFormat? provider);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _0080af67cc571b72(string s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.TryParse(string, out System.UInt128)")]
	public static Array<object?> _8845ce18c94ffbb4(string? text, BigInt result)
		=> BigIntIntegerRuntime.TryParse(text, MinValueCore, MaxValueCore);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(System.ReadOnlySpan<char>, out System.UInt128)")]
	public extern static Array<object?> _4d3bd14dc2810a3c(string s, BigInt result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 128-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(System.ReadOnlySpan<byte>, out System.UInt128)")]
	public extern static Array<object?> _6b11c1fbc39c3749(Uint8Array utf8Text, BigInt result);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.UInt128)")]
	public extern static Array<object?> _48fc1f3242ea3e1e(string? s, object style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.UInt128)")]
	public extern static Array<object?> _07f5c4340bb74419(string s, object style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Byte" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator byte(System.UInt128)")]
	public extern static Number _ec72a9ccd5bd9a8d();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked byte(System.UInt128)")]
	public extern static Number _64e60de5b1e03760();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Char" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator char(System.UInt128)")]
	public extern static Number _e15ea70aeec221be();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked char(System.UInt128)")]
	public extern static Number _b68867a4bbf792ed();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator decimal(System.UInt128)")]
	public extern static string _cfc7a729e04a71ab();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Double" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator double(System.UInt128)")]
	public extern static Number _cd6d53ea42e52f42();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Half" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.Half(System.UInt128)")]
	public extern static Number _ebc69a5a022fe3e9();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int16" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator short(System.UInt128)")]
	public extern static Number _00a7733415bd9a50();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked short(System.UInt128)")]
	public extern static Number _5efef087d1235b8b();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int32" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator int(System.UInt128)")]
	public extern static Number _0ab9aeb11107ae84();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked int(System.UInt128)")]
	public extern static Number _ab4813fe5941ad49();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int64" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator long(System.UInt128)")]
	public extern static BigInt _b230f48381ed749f();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked long(System.UInt128)")]
	public extern static BigInt _191ebf43930db2a5();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int128" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.Int128(System.UInt128)")]
	public extern static BigInt _a8ded488b275f658();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.Int128(System.UInt128)")]
	public extern static BigInt _c572f7b29eaf324c();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.IntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator nint(System.UInt128)")]
	public extern static nint _b74d6c6f2fe3373f();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked nint(System.UInt128)")]
	public extern static nint _b810b3011b0b57b0();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.SByte" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator sbyte(System.UInt128)")]
	public extern static Number _a5c6bf0c046035c1();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked sbyte(System.UInt128)")]
	public extern static Number _95c576d9e4841566();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Single" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator float(System.UInt128)")]
	public extern static Number _2d1b34588d4f3a11();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UInt16" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator ushort(System.UInt128)")]
	public extern static Number _7cb9a373a2b731ae();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked ushort(System.UInt128)")]
	public extern static Number _b68ba902309cfb9a();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UInt32" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator uint(System.UInt128)")]
	public extern static Number _6a569faa11d6516c();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked uint(System.UInt128)")]
	public extern static Number _4b86a17a8f47b33f();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UInt64" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator ulong(System.UInt128)")]
	public extern static BigInt _f9acee955d63d389();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked ulong(System.UInt128)")]
	public extern static BigInt _b7d11ef0703deabf();

	///<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UIntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator nuint(System.UInt128)")]
	public extern static nuint _4ed9a24ef89a2ec1();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked nuint(System.UInt128)")]
	public extern static nuint _4f5d29c8feefce8e();

	///<summary>Explicitly converts a <see cref="T:System.Decimal" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(decimal)")]
	public extern static BigInt _7a73b169cb4a8694();

	///<summary>Explicitly converts a <see cref="T:System.Double" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(double)")]
	public extern static BigInt _8a2ad347ec233b35();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(double)")]
	public extern static BigInt _5d464c2acf139edb();

	///<summary>Explicitly converts a <see cref="T:System.Int16" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(short)")]
	public extern static BigInt _1260da042a15cd4d();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(short)")]
	public extern static BigInt _958e84ffc74ece86();

	///<summary>Explicitly converts a <see cref="T:System.Int32" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(int)")]
	public extern static BigInt _3fc4a35a82073e71();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(int)")]
	public extern static BigInt _06d213d11ddf681c();

	///<summary>Explicitly converts a <see cref="T:System.Int64" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(long)")]
	public extern static BigInt _326147fc1f07f877();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(long)")]
	public extern static BigInt _1ef649fc443738a2();

	///<summary>Explicitly converts a <see cref="T:System.IntPtr" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(nint)")]
	public extern static BigInt _09f191a4670066de();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(nint)")]
	public extern static BigInt _af6df204728f788a();

	///<summary>Explicitly converts a <see cref="T:System.SByte" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(sbyte)")]
	public extern static BigInt _53303fb5506255e9();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(sbyte)")]
	public extern static BigInt _8366585a071ba8b1();

	///<summary>Explicitly converts a <see cref="T:System.Single" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.explicit operator System.UInt128(float)")]
	public extern static BigInt _5ac67fecfe01fee0();

	[Jazor(Op.Discard ,"static System.UInt128.explicit operator checked System.UInt128(float)")]
	public extern static BigInt _dec2fe2225e51e70();

	///<summary>Implicitly converts a <see cref="T:System.Byte" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.implicit operator System.UInt128(byte)")]
	public extern static BigInt _98daec1f69c50f9c();

	///<summary>Implicitly converts a <see cref="T:System.Char" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.implicit operator System.UInt128(char)")]
	public extern static BigInt _5e848b2f01adace3();

	///<summary>Implicitly converts a <see cref="T:System.UInt16" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.implicit operator System.UInt128(ushort)")]
	public extern static BigInt _6fab8bffd4b7f89c();

	///<summary>Implicitly converts a <see cref="T:System.UInt32" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.implicit operator System.UInt128(uint)")]
	public extern static BigInt _fb1429c669cf366b();

	///<summary>Implicitly converts a <see cref="T:System.UInt64" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.implicit operator System.UInt128(ulong)")]
	public extern static BigInt _bff36faddf999794();

	///<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a 128-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.implicit operator System.UInt128(nuint)")]
	public extern static BigInt _c7000f2dfee0777c();

	///<summary>Adds two values together to compute their sum.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator +(System.UInt128, System.UInt128)", "BigInt.asUintN(128, __arg1 + __arg2)")]
	public extern static BigInt _fd527b44b0db5c70(BigInt left, BigInt right);

	///<summary>Adds two values together to compute their sum.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked +(System.UInt128, System.UInt128)")]
	public extern static BigInt _c754a5da22221b5c(BigInt left, BigInt right);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Import, "static System.UInt128.DivRem(System.UInt128, System.UInt128)")]
	public static (BigInt Quotient, BigInt Remainder) _8796a5402e48210c(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.DivRemUnsigned(left, right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.LeadingZeroCount(System.UInt128)")]
	public static BigInt _76106db43126b9b5(BigInt value)
		=> BigIntIntegerRuntime.LeadingZeroCount(value, 128, Mask);

	[Jazor(Op.Discard ,"static System.UInt128.Log10(System.UInt128)")]
	public extern static BigInt _4ae42163ca5ab057(BigInt value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.PopCount(System.UInt128)")]
	public static BigInt _e60df5c8bf2adf5c(BigInt value)
		=> BigIntIntegerRuntime.PopCount(value, Mask);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Import, "static System.UInt128.RotateLeft(System.UInt128, int)")]
	public static BigInt _d743d2ddded2abe5(BigInt value, Number rotateAmount)
		=> BigIntIntegerRuntime.RotateLeft(value, rotateAmount, 128, Mask, Modulus, BigInt.Zero, false);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Import, "static System.UInt128.RotateRight(System.UInt128, int)")]
	public static BigInt _a2bab5c9eaffb253(BigInt value, Number rotateAmount)
		=> BigIntIntegerRuntime.RotateRight(value, rotateAmount, 128, Mask, Modulus, BigInt.Zero, false);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.TrailingZeroCount(System.UInt128)")]
	public static BigInt _f5f31da639f5ea89(BigInt value)
		=> BigIntIntegerRuntime.TrailingZeroCount(value, 128, Mask);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Inline, "static System.UInt128.IsPow2(System.UInt128)", "(__arg1 > 0n && (__arg1 & (__arg1 - 1n)) === 0n)")]
	public extern static bool _841b21ea8d8d4958(BigInt value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Log2(System.UInt128)")]
	public extern static BigInt _44031589e94ab825(BigInt value);

	///<summary>Computes the bitwise-and of two values.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator &(System.UInt128, System.UInt128)", "BigInt.asUintN(128, __arg1 & __arg2)")]
	public extern static BigInt _96b8e5ae109a1ff0(BigInt left, BigInt right);

	///<summary>Computes the bitwise-or of two values.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator |(System.UInt128, System.UInt128)", "BigInt.asUintN(128, __arg1 | __arg2)")]
	public extern static BigInt _d208584e5e031050(BigInt left, BigInt right);

	///<summary>Computes the exclusive-or of two values.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator ^(System.UInt128, System.UInt128)", "BigInt.asUintN(128, __arg1 ^ __arg2)")]
	public extern static BigInt _c1355590879666a7(BigInt left, BigInt right);

	///<summary>Computes the ones-complement representation of a given value.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator ~(System.UInt128)", "BigInt.asUintN(128, ~__arg1)")]
	public extern static BigInt _f4f575ec9a0a472a(BigInt value);

	///<summary>Compares two values to determine which is less.</summary>
	[Jazor(Op.Allowed, "static System.UInt128.operator <(System.UInt128, System.UInt128)")]
	public extern static bool _b39d9b2d9c7479e3(BigInt left, BigInt right);

	///<summary>Compares two values to determine which is less or equal.</summary>
	[Jazor(Op.Allowed, "static System.UInt128.operator <=(System.UInt128, System.UInt128)")]
	public extern static bool _5976a0a34fbfe19a(BigInt left, BigInt right);

	///<summary>Compares two values to determine which is greater.</summary>
	[Jazor(Op.Allowed, "static System.UInt128.operator >(System.UInt128, System.UInt128)")]
	public extern static bool _a5d136c7ac6d9d21(BigInt left, BigInt right);

	///<summary>Compares two values to determine which is greater or equal.</summary>
	[Jazor(Op.Allowed, "static System.UInt128.operator >=(System.UInt128, System.UInt128)")]
	public extern static bool _8ae7181f4f5684f5(BigInt left, BigInt right);

	///<summary>Decrements a value.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator --(System.UInt128)", "BigInt.asUintN(128, __arg1 - 1n)")]
	public extern static BigInt _9576b4fa37800283(BigInt value);

	///<summary>Decrements a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked --(System.UInt128)")]
	public extern static BigInt _2570268944e834ba(BigInt value);

	///<summary>Divides two values together to compute their quotient.</summary>
	[Jazor(Op.Import, "static System.UInt128.operator /(System.UInt128, System.UInt128)")]
	public static BigInt _30e28339559d8888(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.DivideUnsigned(left, right);

	///<summary>Divides two values together to compute their quotient.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked /(System.UInt128, System.UInt128)")]
	public extern static BigInt _b0d1618f64eba0cd(BigInt left, BigInt right);

	///<summary>Compares two values to determine equality.</summary>
	[Jazor(Op.Allowed, "static System.UInt128.operator ==(System.UInt128, System.UInt128)")]
	public extern static bool _e3fe1ff91364288e(BigInt left, BigInt right);

	///<summary>Compares two values to determine inequality.</summary>
	[Jazor(Op.Allowed, "static System.UInt128.operator !=(System.UInt128, System.UInt128)")]
	public extern static bool _38d10160fd6e7017(BigInt left, BigInt right);

	///<summary>Increments a value.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator ++(System.UInt128)", "BigInt.asUintN(128, __arg1 + 1n)")]
	public extern static BigInt _0121bfc5e52ac327(BigInt value);

	///<summary>Increments a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked ++(System.UInt128)")]
	public extern static BigInt _cf08bccf56129f82(BigInt value);

	[Jazor(Op.Inline, "static System.UInt128.MinValue.get", "0n")]
	public extern static BigInt _0b7d00260a524531();

	[Jazor(Op.Inline, "static System.UInt128.MaxValue.get", "340282366920938463463374607431768211455n")]
	public extern static BigInt _f0d23ddd466a780b();

	///<summary>Divides two values together to compute their modulus or remainder.</summary>
	[Jazor(Op.Import, "static System.UInt128.operator %(System.UInt128, System.UInt128)")]
	public static BigInt _4541585272909795(BigInt left, BigInt right)
		=> BigIntIntegerRuntime.Remainder(left, right);

	///<summary>Multiplies two values together to compute their product.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator *(System.UInt128, System.UInt128)", "BigInt.asUintN(128, __arg1 * __arg2)")]
	public extern static BigInt _c1612a3b4558628b(BigInt left, BigInt right);

	///<summary>Multiplies two values together to compute their product.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked *(System.UInt128, System.UInt128)")]
	public extern static BigInt _7b7dc120501d3144(BigInt left, BigInt right);

	[Jazor(Op.Discard ,"static System.UInt128.BigMul(System.UInt128, System.UInt128, out System.UInt128)")]
	public extern static Array<object?> _08f69578289009db(BigInt left, BigInt right, BigInt lower);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Import, "static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)")]
	public static BigInt _a545c5c1dd9b956a(BigInt value, BigInt min, BigInt max)
		=> BigIntIntegerRuntime.Clamp(value, min, max);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Inline, "static System.UInt128.Max(System.UInt128, System.UInt128)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _fe718fcf9ea5e7c2(BigInt x, BigInt y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Inline, "static System.UInt128.Min(System.UInt128, System.UInt128)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _9b8aa52a420963fd(BigInt x, BigInt y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Inline, "static System.UInt128.Sign(System.UInt128)", "(__arg1 === 0n ? 0 : 1)")]
	public extern static Number _f9135bb711742dbc(BigInt value);

	[Jazor(Op.Inline, "static System.UInt128.One.get", "1n")]
	public extern static BigInt _8f31c1f8717c0095();

	[Jazor(Op.Inline, "static System.UInt128.Zero.get", "0n")]
	public extern static BigInt _26fb05b39e23ffb6();

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.CreateChecked<TOther>(TOther)")]
	public extern static BigInt _6b99cde9ef76edf1<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.CreateSaturating<TOther>(TOther)")]
	public extern static BigInt _bc9cc7899a1e35e1<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.CreateTruncating<TOther>(TOther)")]
	public extern static BigInt _97c9e3166e089937<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Inline, "static System.UInt128.IsEvenInteger(System.UInt128)", "(__arg1 % 2n === 0n)")]
	public extern static bool _f413e72394669d0a(BigInt value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Inline, "static System.UInt128.IsOddInteger(System.UInt128)", "(__arg1 % 2n !== 0n)")]
	public extern static bool _db80c70118467db9(BigInt value);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import, "static System.UInt128.TryParse(string, System.IFormatProvider, out System.UInt128)")]
	public static Array<object?> _201a443b1608c214(string? text, Intl.NumberFormat? provider, BigInt result)
		=> _8845ce18c94ffbb4(text, result);

	///<summary>Shifts a value left by a given amount.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator <<(System.UInt128, int)", "BigInt.asUintN(128, __arg1 << BigInt(__arg2 & 127))")]
	public extern static BigInt _0f03623e7f627eca(BigInt value, Number shiftAmount);

	///<summary>Shifts a value right by a given amount.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator >>(System.UInt128, int)", "(__arg1 >> BigInt(__arg2 & 127))")]
	public extern static BigInt _85b70c1560acb52e(BigInt value, Number shiftAmount);

	///<summary>Shifts a value right by a given amount.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator >>>(System.UInt128, int)", "(__arg1 >> BigInt(__arg2 & 127))")]
	public extern static BigInt _e9352047e6007a39(BigInt value, Number shiftAmount);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static BigInt _c88639ae1d5401bd(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.UInt128)")]
	public extern static Array<object?> _76b9708fc50ff818(string s, Intl.NumberFormat? provider, BigInt result);

	///<summary>Subtracts two values to compute their difference.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator -(System.UInt128, System.UInt128)", "BigInt.asUintN(128, __arg1 - __arg2)")]
	public extern static BigInt _892ff8736bbd8e4e(BigInt left, BigInt right);

	///<summary>Subtracts two values to compute their difference.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked -(System.UInt128, System.UInt128)")]
	public extern static BigInt _9b4d82822297f055(BigInt left, BigInt right);

	///<summary>Computes the unary negation of a value.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator -(System.UInt128)", "BigInt.asUintN(128, -__arg1)")]
	public extern static BigInt _e29c8b28c70d54d4(BigInt value);

	///<summary>Computes the unary negation of a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.operator checked -(System.UInt128)")]
	public extern static BigInt _86264fa0bd6d25be(BigInt value);

	///<summary>Computes the unary plus of a value.</summary>
	[Jazor(Op.Inline, "static System.UInt128.operator +(System.UInt128)", "__arg1")]
	public extern static BigInt _01935e48d0078b16(BigInt value);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _3a273da9611bdfc5(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out System.UInt128)")]
	public extern static Array<object?> _40d6510086406c74(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, BigInt result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static BigInt _8c6b3ee07c4c9ea5(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.UInt128.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out System.UInt128)")]
	public extern static Array<object?> _4f6644b18a22d5e1(Uint8Array utf8Text, Intl.NumberFormat? provider, BigInt result);
}
