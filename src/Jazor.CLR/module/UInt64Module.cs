namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "ulong","System/UInt64Module.js")]
public static class UInt64Module
{
	//ulong.MaxValue = 18446744073709551615;

	//ulong.MinValue = 0;

	[Jazor(Op.Discard ,"ulong.UInt64()")]
	public extern static BigInt _6e7ac89a8d6e0188();

	///<summary>Produces the full product of two unsigned 64-bit numbers.</summary>
	[Jazor(Op.Discard ,"static ulong.BigMul(ulong, ulong)")]
	public extern static BigInt _0b66aa6b0604bed0(BigInt left, BigInt right);

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"ulong.CompareTo(object)")]
	public extern static Number _b50ba86b85d8ac33(BigInt instance, Object? value);

	///<summary>Compares this instance to a specified 64-bit unsigned integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"ulong.CompareTo(ulong)")]
	public extern static Number _46d8680dadd72b04(BigInt instance, BigInt value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override ulong.Equals(object)")]
	public extern static bool _a0651bb3484c4e26(BigInt instance, Object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.UInt64" /> value.</summary>
	[Jazor(Op.Discard ,"ulong.Equals(ulong)")]
	public extern static bool _aefa4fdc77a1c743(BigInt instance, BigInt obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override ulong.GetHashCode()")]
	public extern static Number _19d2adbbe01a8cf8(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override ulong.ToString()")]
	public extern static string _d5be50f364f87ca3(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"ulong.ToString(System.IFormatProvider)")]
	public extern static string _994ab2d96243e4b2(BigInt instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard ,"ulong.ToString(string)")]
	public extern static string _78f33051a5a46010(BigInt instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"ulong.ToString(string, System.IFormatProvider)")]
	public extern static string _495c383939d1c12a(BigInt instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current unsigned long number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"ulong.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static bool _ac73c989b7c43bd0(BigInt instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"ulong.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static bool _037cf8cd2c632d87(BigInt instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 64-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(string)")]
	public extern static BigInt _ab08b15d1ba56047(object s);

	///<summary>Converts the string representation of a number in a specified style to its 64-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(string, System.Globalization.NumberStyles)")]
	public extern static BigInt _a65275d8a812ca38(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 64-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(string, System.IFormatProvider)")]
	public extern static BigInt _4e58859b2b591f89(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _a7ca48cb01ea9685(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _a23571df8c6c19c9(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its 64-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(string, out ulong)")]
	public extern static bool _a2771534d71206bd(object s, Box<BigInt> result);

	///<summary>Tries to convert the span representation of a number to its 64-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(System.ReadOnlySpan<char>, out ulong)")]
	public extern static bool _6563986efd5413c0(Uint32Array s, Box<BigInt> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 64-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(System.ReadOnlySpan<byte>, out ulong)")]
	public extern static bool _908c702d612b8a82(Uint8Array utf8Text, Box<BigInt> result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 64-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out ulong)")]
	public extern static bool _3013e933b3a2fe7d(object s, object style, Intl.NumberFormat? provider, Box<BigInt> result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 64-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out ulong)")]
	public extern static bool _988cf0fe6e5934e4(Uint32Array s, object style, Intl.NumberFormat? provider, Box<BigInt> result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt64" />.</summary>
	[Jazor(Op.Discard ,"ulong.GetTypeCode()")]
	public extern static System.TypeCode _84c4fbd7bbbd131e(BigInt instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Discard ,"static ulong.DivRem(ulong, ulong)")]
	public extern static (ulong Quotient, ulong Remainder) _fbae7adf5aedb1a5(BigInt left, BigInt right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard ,"static ulong.LeadingZeroCount(ulong)")]
	public extern static BigInt _cc30bd61ff8ae745(BigInt value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Discard ,"static ulong.PopCount(ulong)")]
	public extern static BigInt _c09e2e8cf64d343e(BigInt value);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Discard ,"static ulong.RotateLeft(ulong, int)")]
	public extern static BigInt _642261af29c95cb4(BigInt value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Discard ,"static ulong.RotateRight(ulong, int)")]
	public extern static BigInt _1a784d80426cfa87(BigInt value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Discard ,"static ulong.TrailingZeroCount(ulong)")]
	public extern static BigInt _bb2bc7ee16cb0d6d(BigInt value);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard ,"static ulong.IsPow2(ulong)")]
	public extern static bool _c80fbfb65612a342(BigInt value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static ulong.Log2(ulong)")]
	public extern static BigInt _d20ed6ab8300965c(BigInt value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static ulong.Clamp(ulong, ulong, ulong)")]
	public extern static BigInt _e24be08e46ae3b3d(BigInt value, BigInt min, BigInt max);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static ulong.Max(ulong, ulong)")]
	public extern static BigInt _111d38c016458f17(BigInt x, BigInt y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static ulong.Min(ulong, ulong)")]
	public extern static BigInt _a48607bf4fa7c1ee(BigInt x, BigInt y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static ulong.Sign(ulong)")]
	public extern static Number _ab7319ddbba9bccc(BigInt value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static ulong.CreateChecked<TOther>(TOther)")]
	public extern static BigInt _9f6b08ec37818cca<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static ulong.CreateSaturating<TOther>(TOther)")]
	public extern static BigInt _2a11c49c0ff0e6f2<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static ulong.CreateTruncating<TOther>(TOther)")]
	public extern static BigInt _5ad09c91e9747ed2<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static ulong.IsEvenInteger(ulong)")]
	public extern static bool _789a47bcce335ad4(BigInt value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static ulong.IsOddInteger(ulong)")]
	public extern static bool _211da5b4be2dd676(BigInt value);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(string, System.IFormatProvider, out ulong)")]
	public extern static bool _21e729b071b97244(object s, Intl.NumberFormat? provider, Box<BigInt> result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static BigInt _be67df6353a859ef(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out ulong)")]
	public extern static bool _7710533f2f6f68a2(Uint32Array s, Intl.NumberFormat? provider, Box<BigInt> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _72578f7a915257ba(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out ulong)")]
	public extern static bool _81e9cf07471323b0(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Box<BigInt> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static BigInt _42f86597f085ca60(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static ulong.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out ulong)")]
	public extern static bool _eeed90f8132830af(Uint8Array utf8Text, Intl.NumberFormat? provider, Box<BigInt> result);
}
