using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("uint", WhiteListOp.Allowed, null,"System/UInt32Module.js")]
public static class UInt32Module
{
	//uint.MaxValue = 4294967295;

	//uint.MinValue = 0;

	[WhiteList("uint.UInt32()", WhiteListOp.Discard)]
	public extern static Number _3221bd6546b20843();

	///<summary>Produces the full product of two unsigned 32-bit numbers.</summary>
	[WhiteList("static uint.BigMul(uint, uint)", WhiteListOp.Discard)]
	public extern static BigInt _e37a28b31d6aed2a(Number left, Number right);

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[WhiteList("uint.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _75ff3ca18f13f709(Number instance, Object? value);

	///<summary>Compares this instance to a specified 32-bit unsigned integer and returns an indication of their relative values.</summary>
	[WhiteList("uint.CompareTo(uint)", WhiteListOp.Discard)]
	public extern static Number _7a5a26a8548c61fe(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override uint.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _ab3e546a9bf4a9ed(Number instance, Object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.UInt32" />.</summary>
	[WhiteList("uint.Equals(uint)", WhiteListOp.Discard)]
	public extern static bool _cb191ad5776dddb3(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override uint.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _d42f9fcffa604eb2(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[WhiteList("override uint.ToString()", WhiteListOp.Discard)]
	public extern static string _d124667433f8250d(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("uint.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _500b36e328db064b(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
	[WhiteList("uint.ToString(string)", WhiteListOp.Discard)]
	public extern static string _4302afe1e5cd00ac(Number instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[WhiteList("uint.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _fe3cdafc7f93e6fe(Number instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current unsigned integer number instance into the provided span of characters.</summary>
	[WhiteList("uint.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _519529f606407c2c(Number instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("uint.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _b67b688ee02ca4a7(Number instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 32-bit unsigned integer equivalent.</summary>
	[WhiteList("static uint.Parse(string)", WhiteListOp.Discard)]
	public extern static Number _eb335b8243aba32a(object s);

	///<summary>Converts the string representation of a number in a specified style to its 32-bit unsigned integer equivalent.</summary>
	[WhiteList("static uint.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static Number _fa26f9c9f654b5c1(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 32-bit unsigned integer equivalent.</summary>
	[WhiteList("static uint.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _1d4807141f77fb88(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent.</summary>
	[WhiteList("static uint.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _6cdc33a0f7e151b0(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent.</summary>
	[WhiteList("static uint.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _88d9113a364b2858(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static uint.TryParse(string, out uint)", WhiteListOp.Discard)]
	public extern static bool _ad4f3364f146e5da(object s, Box<Number> result);

	///<summary>Tries to convert the span representation of a number to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static uint.TryParse(System.ReadOnlySpan<char>, out uint)", WhiteListOp.Discard)]
	public extern static bool _104b334d48c2aecd(Uint32Array s, Box<Number> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 32-bit unsigned integer equivalent.</summary>
	[WhiteList("static uint.TryParse(System.ReadOnlySpan<byte>, out uint)", WhiteListOp.Discard)]
	public extern static bool _2526f7e27fec4657(Uint8Array utf8Text, Box<Number> result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static uint.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out uint)", WhiteListOp.Discard)]
	public extern static bool _b3e8340b7e951baf(object s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static uint.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out uint)", WhiteListOp.Discard)]
	public extern static bool _11ae080219d3fb62(Uint32Array s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt32" />.</summary>
	[WhiteList("uint.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _64eb872ab8e376c7(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[WhiteList("static uint.DivRem(uint, uint)", WhiteListOp.Discard)]
	public extern static (uint Quotient, uint Remainder) _8a073d758132b5bb(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[WhiteList("static uint.LeadingZeroCount(uint)", WhiteListOp.Discard)]
	public extern static Number _6ca4bd298f6f135e(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[WhiteList("static uint.PopCount(uint)", WhiteListOp.Discard)]
	public extern static Number _96cd49e102b39e5b(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[WhiteList("static uint.RotateLeft(uint, int)", WhiteListOp.Discard)]
	public extern static Number _580f8710a620f39b(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[WhiteList("static uint.RotateRight(uint, int)", WhiteListOp.Discard)]
	public extern static Number _465afaf2de09680f(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[WhiteList("static uint.TrailingZeroCount(uint)", WhiteListOp.Discard)]
	public extern static Number _769ecbbaac253539(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[WhiteList("static uint.IsPow2(uint)", WhiteListOp.Discard)]
	public extern static bool _8beae23a85345e63(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[WhiteList("static uint.Log2(uint)", WhiteListOp.Discard)]
	public extern static Number _6cb21d474b7a30db(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[WhiteList("static uint.Clamp(uint, uint, uint)", WhiteListOp.Discard)]
	public extern static Number _3693c701aa9899c6(Number value, Number min, Number max);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static uint.Max(uint, uint)", WhiteListOp.Discard)]
	public extern static Number _f284eae007e1fb6d(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static uint.Min(uint, uint)", WhiteListOp.Discard)]
	public extern static Number _4f3e77f684e65319(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[WhiteList("static uint.Sign(uint)", WhiteListOp.Discard)]
	public extern static Number _5942eb8a5b8a3bcc(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static uint.CreateChecked<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _6af9e09d7ede9ef2<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static uint.CreateSaturating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _7235beab29d2d5ee<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static uint.CreateTruncating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _a70daf7a8645e3f0<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[WhiteList("static uint.IsEvenInteger(uint)", WhiteListOp.Discard)]
	public extern static bool _e2d0c1e7c0661ad2(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[WhiteList("static uint.IsOddInteger(uint)", WhiteListOp.Discard)]
	public extern static bool _9c66512cee42f6d9(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[WhiteList("static uint.TryParse(string, System.IFormatProvider, out uint)", WhiteListOp.Discard)]
	public extern static bool _69bfc426d401ae5e(object s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("static uint.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _526ccc55a20da9a9(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[WhiteList("static uint.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out uint)", WhiteListOp.Discard)]
	public extern static bool _2a0e1fb1dbc0c5ec(Uint32Array s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static uint.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _33fc7a36a7feaa04(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static uint.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out uint)", WhiteListOp.Discard)]
	public extern static bool _fdfb10ed1305e83d(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static uint.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _594553ddcab879cd(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static uint.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out uint)", WhiteListOp.Discard)]
	public extern static bool _515b8388710d931d(Uint8Array utf8Text, Intl.NumberFormat? provider, Box<Number> result);
}
