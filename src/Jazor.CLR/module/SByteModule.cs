namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "sbyte","System/SByteModule.js")]
public static class SByteModule
{
	//sbyte.MaxValue = 127;

	//sbyte.MinValue = -128;

	[Jazor(Op.Discard ,"sbyte.SByte()")]
	public extern static Number _0b5843a5a69b4fde();

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"sbyte.CompareTo(object)")]
	public extern static Number _f8a387725694962f(Number instance, Object? obj);

	///<summary>Compares this instance to a specified 8-bit signed integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"sbyte.CompareTo(sbyte)")]
	public extern static Number _a0ff7e0ac34c91a8(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override sbyte.Equals(object)")]
	public extern static bool _74c9452fa767096f(Number instance, Object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.SByte" /> value.</summary>
	[Jazor(Op.Discard ,"sbyte.Equals(sbyte)")]
	public extern static bool _4105db2840795661(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override sbyte.GetHashCode()")]
	public extern static Number _5131b1d6df49bbfb(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override sbyte.ToString()")]
	public extern static string _99cd65a77e5cb1e0(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"sbyte.ToString(string)")]
	public extern static string _f1581e4c3d9629b5(Number instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"sbyte.ToString(System.IFormatProvider)")]
	public extern static string _2835ffcd09fe2083(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"sbyte.ToString(string, System.IFormatProvider)")]
	public extern static string _e06a6af137f4a848(Number instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current 8-bit signed integer instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"sbyte.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static bool _cc044b52a705b83a(Number instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"sbyte.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static bool _08ca5484266e1a7b(Number instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string)")]
	public extern static Number _fc6fdbb937cb390a(object s);

	///<summary>Converts the string representation of a number in a specified style to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _302c7b4fcff325d8(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string, System.IFormatProvider)")]
	public extern static Number _28a6ad10aa689a4f(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number that is in a specified style and culture-specific format to its 8-bit signed equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _8885d6602b6a8ecd(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number that is in a specified style and culture-specific format to its 8-bit signed equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _49c3ab5496122405(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(string, out sbyte)")]
	public extern static bool _d9082c2537283f95(object s, Box<Number> result);

	///<summary>Tries to convert the span representation of a number to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<char>, out sbyte)")]
	public extern static bool _a3ccaa03549862bc(Uint32Array s, Box<Number> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 8-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<byte>, out sbyte)")]
	public extern static bool _f25602df99a7ca89(Uint8Array utf8Text, Box<Number> result);

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out sbyte)")]
	public extern static bool _b5d3ab86487e1092(object s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.SByte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out sbyte)")]
	public extern static bool _9d5e37148ebfe7f5(Uint32Array s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.SByte" />.</summary>
	[Jazor(Op.Discard ,"sbyte.GetTypeCode()")]
	public extern static System.TypeCode _05739d4cc5ffd426(Number instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Discard ,"static sbyte.DivRem(sbyte, sbyte)")]
	public extern static (sbyte Quotient, sbyte Remainder) _b77d7bfe141b3f05(Number left, Number right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.LeadingZeroCount(sbyte)")]
	public extern static Number _b15d784594c3c77a(Number value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.PopCount(sbyte)")]
	public extern static Number _18bf827131a4d1f2(Number value);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Discard ,"static sbyte.RotateLeft(sbyte, int)")]
	public extern static Number _a156afdf9d66378b(Number value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Discard ,"static sbyte.RotateRight(sbyte, int)")]
	public extern static Number _a8c2cb9a92de8efd(Number value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.TrailingZeroCount(sbyte)")]
	public extern static Number _c68b30466f995072(Number value);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard ,"static sbyte.IsPow2(sbyte)")]
	public extern static bool _25fac8c1c0089367(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Log2(sbyte)")]
	public extern static Number _dba579eec9ba3de5(Number value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Clamp(sbyte, sbyte, sbyte)")]
	public extern static Number _b8fd62c157dfa221(Number value, Number min, Number max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Discard ,"static sbyte.CopySign(sbyte, sbyte)")]
	public extern static Number _14e4ea7e74086ad7(Number value, Number sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static sbyte.Max(sbyte, sbyte)")]
	public extern static Number _77fa5be291628cd5(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static sbyte.Min(sbyte, sbyte)")]
	public extern static Number _b9b655261540ef89(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Sign(sbyte)")]
	public extern static Number _8c50aab12919fd23(Number value);

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Abs(sbyte)")]
	public extern static Number _08da3784dbe3da67(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static sbyte.CreateChecked<TOther>(TOther)")]
	public extern static Number _501bd486a2bc7fa1<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static sbyte.CreateSaturating<TOther>(TOther)")]
	public extern static Number _ee8e2108052a9077<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static sbyte.CreateTruncating<TOther>(TOther)")]
	public extern static Number _af0b5dd1926072c2<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static sbyte.IsEvenInteger(sbyte)")]
	public extern static bool _774b4b6369e38721(Number value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Discard ,"static sbyte.IsNegative(sbyte)")]
	public extern static bool _05e5ab5a1229717a(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static sbyte.IsOddInteger(sbyte)")]
	public extern static bool _6166df44a8170b3d(Number value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Discard ,"static sbyte.IsPositive(sbyte)")]
	public extern static bool _6d4962564b03c732(Number value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static sbyte.MaxMagnitude(sbyte, sbyte)")]
	public extern static Number _739529a82a66a4ac(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static sbyte.MinMagnitude(sbyte, sbyte)")]
	public extern static Number _2b180f3969fde348(Number x, Number y);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(string, System.IFormatProvider, out sbyte)")]
	public extern static bool _eb0b5e4bda3cf5a8(object s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _f0c24922fba904dc(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out sbyte)")]
	public extern static bool _9c15d03f28f55ad0(Uint32Array s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _da4b7921ed508906(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out sbyte)")]
	public extern static bool _bb5b59fba854851f(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _fad48943b004f2cf(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static sbyte.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out sbyte)")]
	public extern static bool _88a4e6839132acad(Uint8Array utf8Text, Intl.NumberFormat? provider, Box<Number> result);
}
