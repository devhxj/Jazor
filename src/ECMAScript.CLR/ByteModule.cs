using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("byte", WhiteListOp.Allowed, null, "System/ByteModule.js")]
public static class ByteModule
{
	//byte.MaxValue = 255;

	//byte.MinValue = 0;

	[WhiteList("byte.Byte()", WhiteListOp.Discard)]
	public extern static Number _c16a6a35ab0f1a78();

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[WhiteList("byte.CompareTo(object)", WhiteListOp.CompareTo)]
	public extern static Number _7aaf4c67dc6c9c9a(Number instance, Object? value);

	///<summary>Compares this instance to a specified 8-bit unsigned integer and returns an indication of their relative values.</summary>
	[WhiteList("byte.CompareTo(byte)", WhiteListOp.CompareTo)]
	public extern static Number _5c935ae4273a32cf(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override byte.Equals(object)", WhiteListOp.Equals)]
	public extern static bool _991f10ab45b84c4a(Number instance, Object? obj);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Byte" /> object represent the same value.</summary>
	[WhiteList("byte.Equals(byte)", WhiteListOp.Equals)]
	public extern static bool _4885d24d76ef9f6d(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override byte.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _0db3f15e7e706cc7(Number instance);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Byte" /> equivalent.</summary>
	[WhiteList("static byte.Parse(string)", WhiteListOp.Import)]
	public static Number _8719e4b3055c5188(string s)
	{
		return Number(s);
	}

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Byte" /> equivalent.</summary>
	[WhiteList("static byte.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static Number _82aa6f31e6873ee2(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
	[WhiteList("static byte.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _65691bfd885c413a(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
	[WhiteList("static byte.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _07f65aff65731222(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
	[WhiteList("static byte.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _ff08be5970881dca(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Byte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static byte.TryParse(string, out byte)", WhiteListOp.Import)]
	public static bool _03c07d3f3ee012f9(string s, Box<Number> result)
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

	///<summary>Tries to convert the span representation of a number to its <see cref="T:System.Byte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static byte.TryParse(System.ReadOnlySpan<char>, out byte)", WhiteListOp.Discard)]
	public extern static bool _413c6f7752002edf(Uint32Array s, Box<Number> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 8-bit unsigned integer equivalent.</summary>
	[WhiteList("static byte.TryParse(System.ReadOnlySpan<byte>, out byte)", WhiteListOp.Discard)]
	public extern static bool _0e02bd74e5960e4d(Uint8Array utf8Text, Box<Number> result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static byte.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out byte)", WhiteListOp.Discard)]
	public extern static bool _aed06cdaac60f688(object s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static byte.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)", WhiteListOp.Discard)]
	public extern static bool _761e5b49fdeccb96(Uint32Array s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation.</summary>
	[WhiteList("override byte.ToString()", WhiteListOp.Replace, "toString")]
	public extern static string _fe5d1bb114dd9985(Number instance);

	///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified format.</summary>
	[WhiteList("byte.ToString(string)", WhiteListOp.Discard)]
	public extern static string _94ac453822a347f8(Number instance, object format);

	///<summary>Converts the numeric value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[WhiteList("byte.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _0c8b56bfa65bb1f8(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified format and culture-specific formatting information.</summary>
	[WhiteList("byte.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _6dae7e6c4a7c6261(Number instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current 8-bit unsigned integer instance into the provided span of characters.</summary>
	[WhiteList("byte.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _f2fa96775a8b3f25(Number instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("byte.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _a2e5b355ba248fcd(Number instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Byte" />.</summary>
	[WhiteList("byte.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _1695fafe88707bc5(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[WhiteList("static byte.DivRem(byte, byte)", WhiteListOp.Discard)]
	public extern static (byte Quotient, byte Remainder) _42cbe2ef401fb8c9(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[WhiteList("static byte.LeadingZeroCount(byte)", WhiteListOp.Discard)]
	public extern static Number _9526f26e93e4c913(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[WhiteList("static byte.PopCount(byte)", WhiteListOp.Discard)]
	public extern static Number _c5ae774e00ea2202(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[WhiteList("static byte.RotateLeft(byte, int)", WhiteListOp.Discard)]
	public extern static Number _0156fdbf291b637d(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[WhiteList("static byte.RotateRight(byte, int)", WhiteListOp.Discard)]
	public extern static Number _872d6a20e2bf8567(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[WhiteList("static byte.TrailingZeroCount(byte)", WhiteListOp.Discard)]
	public extern static Number _88ad71d45f9ffca7(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[WhiteList("static byte.IsPow2(byte)", WhiteListOp.Discard)]
	public extern static bool _b10f7588a1920633(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[WhiteList("static byte.Log2(byte)", WhiteListOp.Discard)]
	public extern static Number _8f1e70f00149e892(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[WhiteList("static byte.Clamp(byte, byte, byte)", WhiteListOp.Discard)]
	public extern static Number _d46830318e177655(Number value, Number min, Number max);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static byte.Max(byte, byte)", WhiteListOp.Replace, "max")]
	public extern static Number _04555e3eb1c7a9ce(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static byte.Min(byte, byte)", WhiteListOp.Replace, "min")]
	public extern static Number _01cc0a43897afd75(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[WhiteList("static byte.Sign(byte)", WhiteListOp.Replace, "sign")]
	public extern static Number _683fdf4d3120d162(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static byte.CreateChecked<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _3be4135a6878c4f6<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static byte.CreateSaturating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _cb74080a125947ac<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static byte.CreateTruncating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _c47aae89c9da8a9f<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[WhiteList("static byte.IsEvenInteger(byte)", WhiteListOp.Discard)]
	public extern static bool _ed30037c45c0e107(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[WhiteList("static byte.IsOddInteger(byte)", WhiteListOp.Discard)]
	public extern static bool _bb058beaaa7a9d6f(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[WhiteList("static byte.TryParse(string, System.IFormatProvider, out byte)", WhiteListOp.Discard)]
	public extern static bool _73bacef10db6dd04(object s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("static byte.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _f09faa9402018245(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[WhiteList("static byte.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out byte)", WhiteListOp.Discard)]
	public extern static bool _44dd755bac10b090(Uint32Array s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static byte.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _984889e2fd23e5d8(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static byte.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)", WhiteListOp.Discard)]
	public extern static bool _77a3faa6c6a9ad83(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static byte.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _63304d5cac2b30b7(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static byte.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out byte)", WhiteListOp.Discard)]
	public extern static bool _f7f4e5fabad2e9af(Uint8Array utf8Text, Intl.NumberFormat? provider, Box<Number> result);
}
