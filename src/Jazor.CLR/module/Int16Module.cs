namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "short","System/Int16Module.js")]
public static class Int16Module
{
	//short.MaxValue = 32767;

	//short.MinValue = -32768;

	[Jazor(Op.Discard ,"short.Int16()")]
	public extern static Number _562bb08ad63be5d7();

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the object.</summary>
	[Jazor(Op.Discard ,"short.CompareTo(object)")]
	public extern static Number _16417ddcfd71e8e5(Number instance, object? value);

	///<summary>Compares this instance to a specified 16-bit signed integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified 16-bit signed integer.</summary>
	[Jazor(Op.Discard ,"short.CompareTo(short)")]
	public extern static Number _4ee8d8c1e1a45502(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override short.Equals(object)")]
	public extern static bool _22027e397eeeadf4(Number instance, object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int16" /> value.</summary>
	[Jazor(Op.Discard ,"short.Equals(short)")]
	public extern static bool _cc018b8cb5a7c74c(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override short.GetHashCode()")]
	public extern static Number _b813268a9990cfbe(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override short.ToString()")]
	public extern static string _300da933adcd7412(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"short.ToString(System.IFormatProvider)")]
	public extern static string _46ad91354004146c(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"short.ToString(string)")]
	public extern static string _700b60c63bd82c5d(Number instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"short.ToString(string, System.IFormatProvider)")]
	public extern static string _ffb38f7355a8b434(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current short number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"short.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _545cfe8d9fec0470(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"short.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _cf56eee1e0199bff(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 16-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static short.Parse(string)")]
	public extern static Number _8a975b9eda8ac957(string s);

	///<summary>Converts the string representation of a number in a specified style to its 16-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static short.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _64bcec0f7b8ae902(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 16-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static short.Parse(string, System.IFormatProvider)")]
	public extern static Number _4f63dd7e755ab151(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static short.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _8457b89fab66282c(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static short.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _c23c80430bf1bf6a(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(string, out short)")]
	public extern static Array<object?> _65bc2566851a5ef7(string? s, Number result);

	///<summary>Converts the span representation of a number in a culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(System.ReadOnlySpan<char>, out short)")]
	public extern static Array<object?> _f06bf367c8a26691(Uint32Array s, Number result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 16-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(System.ReadOnlySpan<byte>, out short)")]
	public extern static Array<object?> _af732a8ac69b6f6e(Uint8Array utf8Text, Number result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
	public extern static Array<object?> _cb5aaf07104e3199(string? s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
	public extern static Array<object?> _74bca5547a182d94(Uint32Array s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int16" />.</summary>
	[Jazor(Op.Discard ,"short.GetTypeCode()")]
	public extern static System.TypeCode _40232ebb0dcadbf1(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Discard ,"static short.DivRem(short, short)")]
	public extern static (short Quotient, short Remainder) _b2c1f15fae072110(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard ,"static short.LeadingZeroCount(short)")]
	public extern static Number _52aba2834bccd915(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Discard ,"static short.PopCount(short)")]
	public extern static Number _1636c956519f95fa(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Discard ,"static short.RotateLeft(short, int)")]
	public extern static Number _bae87098d1a8d51f(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Discard ,"static short.RotateRight(short, int)")]
	public extern static Number _9d0ea1985ea5d86c(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Discard ,"static short.TrailingZeroCount(short)")]
	public extern static Number _34f7d9d508f3d3fa(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard ,"static short.IsPow2(short)")]
	public extern static bool _7f2d59a3c443c4ad(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static short.Log2(short)")]
	public extern static Number _35f45babf0c06295(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static short.Clamp(short, short, short)")]
	public extern static Number _ab81977f8ce898b6(Number value, Number min, Number max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Discard ,"static short.CopySign(short, short)")]
	public extern static Number _84dbfd61502b67c2(Number value, Number sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static short.Max(short, short)")]
	public extern static Number _3373f84658d4d175(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static short.Min(short, short)")]
	public extern static Number _02506ba99181e464(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static short.Sign(short)")]
	public extern static Number _566e8c96791a4a93(Number value);

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Discard ,"static short.Abs(short)")]
	public extern static Number _8ce36b36c4abd947(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static short.CreateChecked<TOther>(TOther)")]
	public extern static Number _5fc26fbc77170159<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static short.CreateSaturating<TOther>(TOther)")]
	public extern static Number _0803cae0198e4e4a<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static short.CreateTruncating<TOther>(TOther)")]
	public extern static Number _4da6b11d651bbbb0<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static short.IsEvenInteger(short)")]
	public extern static bool _316df8d3092665d2(Number value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Discard ,"static short.IsNegative(short)")]
	public extern static bool _1d7ab190b3eef427(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static short.IsOddInteger(short)")]
	public extern static bool _e35c3640561ad6e4(Number value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Discard ,"static short.IsPositive(short)")]
	public extern static bool _f65c31648c1c40d7(Number value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static short.MaxMagnitude(short, short)")]
	public extern static Number _ea75510d32bc8099(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static short.MinMagnitude(short, short)")]
	public extern static Number _63d3d54252a49e29(Number x, Number y);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(string, System.IFormatProvider, out short)")]
	public extern static Array<object?> _1726573b3ed2620b(string? s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static short.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _68a3295a7ebacac9(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out short)")]
	public extern static Array<object?> _5849d879c5ca8c59(Uint32Array s, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static short.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _0795bea51a359cfe(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
	public extern static Array<object?> _c09ca931ddd2f2ca(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static short.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _50cc08bf7c6985cb(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static short.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out short)")]
	public extern static Array<object?> _91d5e9e62716bef1(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
