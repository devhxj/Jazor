using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("ushort", WhiteListOp.Allowed, null, "System/UInt16Module.js")]
public static class UInt16Module
{
	//ushort.MaxValue = 65535;

	//ushort.MinValue = 0;

	[WhiteList("ushort.UInt16()", WhiteListOp.Discard)]
	public extern static Number _2b4f1af6b7fc0173();

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[WhiteList("ushort.CompareTo(object)", WhiteListOp.CompareTo)]
	public extern static Number _d8d8b9cba9bd3347(Number instance, Object? value);

	///<summary>Compares this instance to a specified 16-bit unsigned integer and returns an indication of their relative values.</summary>
	[WhiteList("ushort.CompareTo(ushort)", WhiteListOp.CompareTo)]
	public extern static Number _2ca53dc375a8ff3d(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override ushort.Equals(object)", WhiteListOp.Equals)]
	public extern static bool _c13e06040702dab1(Number instance, Object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.UInt16" /> value.</summary>
	[WhiteList("ushort.Equals(ushort)", WhiteListOp.Equals)]
	public extern static bool _0faff9447540bf0f(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override ushort.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _1289c3b26567b431(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[WhiteList("override ushort.ToString()", WhiteListOp.Replace, "toString")]
	public extern static string _97b1f766a137a176(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("ushort.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _54f6d55d2ab58603(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
	[WhiteList("ushort.ToString(string)", WhiteListOp.Discard)]
	public extern static string _6f22376b1343fe81(Number instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[WhiteList("ushort.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _a995cb7019a823da(Number instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current unsigned short number instance into the provided span of characters.</summary>
	[WhiteList("ushort.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _72607726c0ca8cb0(Number instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("ushort.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _c8d9586ea188f250(Number instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 16-bit unsigned integer equivalent.</summary>
	[WhiteList("static ushort.Parse(string)", WhiteListOp.Import)]
	public static Number _bfae72f49db4f3c9(string s)
	{
		return Number(s);
	}

	///<summary>Converts the string representation of a number in a specified style to its 16-bit unsigned integer equivalent. This method is not CLS-compliant. The CLS-compliant alternative is <see cref="M:System.Int32.Parse(System.String,System.Globalization.NumberStyles)" />.</summary>
	[WhiteList("static ushort.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static Number _fa01aff4be2733da(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 16-bit unsigned integer equivalent.</summary>
	[WhiteList("static ushort.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _c90f18e22ef793ae(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent.</summary>
	[WhiteList("static ushort.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _2d47dd2f7572ac82(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent.</summary>
	[WhiteList("static ushort.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _e0537feda3434747(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static ushort.TryParse(string, out ushort)", WhiteListOp.Import)]
	public static bool _2efd27d401f7def7(string s, Box<Number> result)
	{
		try
		{
			result.Value = Number(s);
			return true;
		}
		catch
		{
			return false;
		}
	}

	///<summary>Tries to convert the span representation of a number to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static ushort.TryParse(System.ReadOnlySpan<char>, out ushort)", WhiteListOp.Discard)]
	public extern static bool _0103a8bec9e9dfd7(Uint32Array s, Box<Number> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 16-bit unsigned integer equivalent.</summary>
	[WhiteList("static ushort.TryParse(System.ReadOnlySpan<byte>, out ushort)", WhiteListOp.Discard)]
	public extern static bool _f90ee83a31a4d447(Uint8Array utf8Text, Box<Number> result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static ushort.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)", WhiteListOp.Discard)]
	public extern static bool _0427e1fa823cd14c(object s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static ushort.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)", WhiteListOp.Discard)]
	public extern static bool _e1ac1ed9e4df0694(Uint32Array s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt16" />.</summary>
	[WhiteList("ushort.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _496bf7ba2bb081f6(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[WhiteList("static ushort.DivRem(ushort, ushort)", WhiteListOp.Discard)]
	public extern static (ushort Quotient, ushort Remainder) _80e78c0aa0b98fef(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[WhiteList("static ushort.LeadingZeroCount(ushort)", WhiteListOp.Discard)]
	public extern static Number _680a923d09b804b9(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[WhiteList("static ushort.PopCount(ushort)", WhiteListOp.Discard)]
	public extern static Number _2ea0cab4f3f489d9(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[WhiteList("static ushort.RotateLeft(ushort, int)", WhiteListOp.Discard)]
	public extern static Number _81462814a6e17f8a(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[WhiteList("static ushort.RotateRight(ushort, int)", WhiteListOp.Discard)]
	public extern static Number _68cb080f188abe14(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[WhiteList("static ushort.TrailingZeroCount(ushort)", WhiteListOp.Discard)]
	public extern static Number _08ec622fc4aabafb(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[WhiteList("static ushort.IsPow2(ushort)", WhiteListOp.Discard)]
	public extern static bool _5e7a013434210fd3(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[WhiteList("static ushort.Log2(ushort)", WhiteListOp.Discard)]
	public extern static Number _3e54056b3d1e32ad(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[WhiteList("static ushort.Clamp(ushort, ushort, ushort)", WhiteListOp.Discard)]
	public extern static Number _cfa99d1fe078f42e(Number value, Number min, Number max);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static ushort.Max(ushort, ushort)", WhiteListOp.Replace, "max")]
	public extern static Number _baf95be10fbe1b99(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static ushort.Min(ushort, ushort)", WhiteListOp.Replace, "min")]
	public extern static Number _5bde9c15f7f8b2f9(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[WhiteList("static ushort.Sign(ushort)", WhiteListOp.Replace, "sign")]
	public extern static Number _40243528ed598d7c(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static ushort.CreateChecked<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _5f125252b32ddf67<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static ushort.CreateSaturating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _d885c6bcbc91e10a<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static ushort.CreateTruncating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _e7b18638be92c02a<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[WhiteList("static ushort.IsEvenInteger(ushort)", WhiteListOp.Discard)]
	public extern static bool _9efbbf8cbd046a16(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[WhiteList("static ushort.IsOddInteger(ushort)", WhiteListOp.Discard)]
	public extern static bool _fc6357bc14bbd89b(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[WhiteList("static ushort.TryParse(string, System.IFormatProvider, out ushort)", WhiteListOp.Discard)]
	public extern static bool _815a123a217a57dc(object s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("static ushort.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _37538c358921bcf3(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[WhiteList("static ushort.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out ushort)", WhiteListOp.Discard)]
	public extern static bool _57f6f9049f0201c4(Uint32Array s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static ushort.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _e04a106a21529984(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static ushort.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)", WhiteListOp.Discard)]
	public extern static bool _8b4f59ba7c1bec8d(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static ushort.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _b0cfeeee7dd4575a(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static ushort.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out ushort)", WhiteListOp.Discard)]
	public extern static bool _9a6ea927f4cb63da(Uint8Array utf8Text, Intl.NumberFormat? provider, Box<Number> result);
}
