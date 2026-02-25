namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "byte","System/ByteModule.js")]
public static class ByteModule
{
	//byte.MaxValue = 255;

	//byte.MinValue = 0;

	[Jazor(Op.Discard ,"byte.Byte()")]
	public extern static Number _c16a6a35ab0f1a78();

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"byte.CompareTo(object)")]
	public extern static Number _7aaf4c67dc6c9c9a(Number instance, object? value);

	///<summary>Compares this instance to a specified 8-bit unsigned integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"byte.CompareTo(byte)")]
	public extern static Number _5c935ae4273a32cf(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override byte.Equals(object)")]
	public extern static bool _991f10ab45b84c4a(Number instance, object? obj);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Byte" /> object represent the same value.</summary>
	[Jazor(Op.Discard ,"byte.Equals(byte)")]
	public extern static bool _4885d24d76ef9f6d(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override byte.GetHashCode()")]
	public extern static Number _0db3f15e7e706cc7(Number instance);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Byte" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(string)")]
	public extern static Number _8719e4b3055c5188(string s);

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Byte" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _82aa6f31e6873ee2(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(string, System.IFormatProvider)")]
	public extern static Number _65691bfd885c413a(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _07f65aff65731222(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _ff08be5970881dca(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Byte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(string, out byte)")]
	public extern static Array<object?> _03c07d3f3ee012f9(string? s, out Number result);

	///<summary>Tries to convert the span representation of a number to its <see cref="T:System.Byte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(System.ReadOnlySpan<char>, out byte)")]
	public extern static Array<object?> _413c6f7752002edf(Uint32Array s, out Number result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 8-bit unsigned integer equivalent.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(System.ReadOnlySpan<byte>, out byte)")]
	public extern static Array<object?> _0e02bd74e5960e4d(Uint8Array utf8Text, out Number result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _aed06cdaac60f688(string? s, object style, Intl.NumberFormat? provider, out Number result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _761e5b49fdeccb96(Uint32Array s, object style, Intl.NumberFormat? provider, out Number result);

	///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override byte.ToString()")]
	public extern static string _fe5d1bb114dd9985(Number instance);

	///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard ,"byte.ToString(string)")]
	public extern static string _94ac453822a347f8(Number instance, string? format);

	///<summary>Converts the numeric value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"byte.ToString(System.IFormatProvider)")]
	public extern static string _0c8b56bfa65bb1f8(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified format and culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"byte.ToString(string, System.IFormatProvider)")]
	public extern static string _6dae7e6c4a7c6261(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current 8-bit unsigned integer instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"byte.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _f2fa96775a8b3f25(Number instance, Uint32Array destination, out Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"byte.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _a2e5b355ba248fcd(Number instance, Uint8Array utf8Destination, out Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Byte" />.</summary>
	[Jazor(Op.Discard ,"byte.GetTypeCode()")]
	public extern static System.TypeCode _1695fafe88707bc5(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Discard ,"static byte.DivRem(byte, byte)")]
	public extern static (byte Quotient, byte Remainder) _42cbe2ef401fb8c9(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard ,"static byte.LeadingZeroCount(byte)")]
	public extern static Number _9526f26e93e4c913(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Discard ,"static byte.PopCount(byte)")]
	public extern static Number _c5ae774e00ea2202(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Discard ,"static byte.RotateLeft(byte, int)")]
	public extern static Number _0156fdbf291b637d(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Discard ,"static byte.RotateRight(byte, int)")]
	public extern static Number _872d6a20e2bf8567(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Discard ,"static byte.TrailingZeroCount(byte)")]
	public extern static Number _88ad71d45f9ffca7(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard ,"static byte.IsPow2(byte)")]
	public extern static bool _b10f7588a1920633(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static byte.Log2(byte)")]
	public extern static Number _8f1e70f00149e892(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static byte.Clamp(byte, byte, byte)")]
	public extern static Number _d46830318e177655(Number value, Number min, Number max);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static byte.Max(byte, byte)")]
	public extern static Number _04555e3eb1c7a9ce(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static byte.Min(byte, byte)")]
	public extern static Number _01cc0a43897afd75(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static byte.Sign(byte)")]
	public extern static Number _683fdf4d3120d162(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static byte.CreateChecked<TOther>(TOther)")]
	public extern static Number _3be4135a6878c4f6<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static byte.CreateSaturating<TOther>(TOther)")]
	public extern static Number _cb74080a125947ac<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static byte.CreateTruncating<TOther>(TOther)")]
	public extern static Number _c47aae89c9da8a9f<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static byte.IsEvenInteger(byte)")]
	public extern static bool _ed30037c45c0e107(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static byte.IsOddInteger(byte)")]
	public extern static bool _bb058beaaa7a9d6f(Number value);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(string, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _73bacef10db6dd04(string? s, Intl.NumberFormat? provider, out Number result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _f09faa9402018245(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _44dd755bac10b090(Uint32Array s, Intl.NumberFormat? provider, out Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _984889e2fd23e5d8(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _77a3faa6c6a9ad83(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, out Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static byte.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _63304d5cac2b30b7(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static byte.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out byte)")]
	public extern static Array<object?> _f7f4e5fabad2e9af(Uint8Array utf8Text, Intl.NumberFormat? provider, out Number result);
}
