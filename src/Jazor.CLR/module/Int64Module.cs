namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "long","System/Int64Module.js")]
public static class Int64Module
{
	//long.MaxValue = 9223372036854775807;

	//long.MinValue = -9223372036854775808;

	[Jazor(Op.Discard ,"long.Int64()")]
	public extern static BigInt _74cd360dde6bde69();

	///<summary>Produces the full product of two 64-bit numbers.</summary>
	[Jazor(Op.Discard ,"static long.BigMul(long, long)")]
	public extern static BigInt _62ebef6eaaff4810(BigInt left, BigInt right);

	///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"long.CompareTo(object)")]
	public extern static Number _a108636b79b7c8d2(BigInt instance, object? value);

	///<summary>Compares this instance to a specified 64-bit signed integer and returns an indication of their relative values.</summary>
	[Jazor(Op.Discard ,"long.CompareTo(long)")]
	public extern static Number _e862e3c68f06f9e2(BigInt instance, BigInt value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override long.Equals(object)")]
	public extern static bool _3fc2378cd670be8a(BigInt instance, object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int64" /> value.</summary>
	[Jazor(Op.Discard ,"long.Equals(long)")]
	public extern static bool _73c4e4cb572f07f1(BigInt instance, BigInt obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override long.GetHashCode()")]
	public extern static Number _a6f06b90e3618c16(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override long.ToString()")]
	public extern static string _56beebc0ed49cbc9(BigInt instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"long.ToString(System.IFormatProvider)")]
	public extern static string _c9ee1b2e169f61aa(BigInt instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"long.ToString(string)")]
	public extern static string _db383e0ca3b051e6(BigInt instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"long.ToString(string, System.IFormatProvider)")]
	public extern static string _88dfcf515abb66f6(BigInt instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current long number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"long.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _de37ec26b22b5ff8(BigInt instance, Uint32Array destination, out Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"long.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _a7f530e78a14a037(BigInt instance, Uint8Array utf8Destination, out Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static long.Parse(string)")]
	public extern static BigInt _4174bb5b72e448a6(string s);

	///<summary>Converts the string representation of a number in a specified style to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static long.Parse(string, System.Globalization.NumberStyles)")]
	public extern static BigInt _481fbf6d32029fcb(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static long.Parse(string, System.IFormatProvider)")]
	public extern static BigInt _cb7366fbf6242a6a(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static long.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _540038b3f55a1010(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static long.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _78d6c19de30b5937(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(string, out long)")]
	public extern static Array<object?> _2cba636c245c1675(string? s, out BigInt result);

	///<summary>Converts the span representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(System.ReadOnlySpan<char>, out long)")]
	public extern static Array<object?> _f65dcae3cb8d9ffc(Uint32Array s, out BigInt result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 64-bit signed integer equivalent.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(System.ReadOnlySpan<byte>, out long)")]
	public extern static Array<object?> _8bee07df79eb3a90(Uint8Array utf8Text, out BigInt result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out long)")]
	public extern static Array<object?> _de4d5fc73e6f5f38(string? s, object style, Intl.NumberFormat? provider, out BigInt result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out long)")]
	public extern static Array<object?> _c1dce355b4dded70(Uint32Array s, object style, Intl.NumberFormat? provider, out BigInt result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int64" />.</summary>
	[Jazor(Op.Discard ,"long.GetTypeCode()")]
	public extern static System.TypeCode _5efdcf3dff57ebdf(BigInt instance);

	///<summary>Computes the quotient and remainder of two values.</summary>
	[Jazor(Op.Discard ,"static long.DivRem(long, long)")]
	public extern static (long Quotient, long Remainder) _28273cd350760efe(BigInt left, BigInt right);

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Discard ,"static long.LeadingZeroCount(long)")]
	public extern static BigInt _f67b17bf5c4120f2(BigInt value);

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Discard ,"static long.PopCount(long)")]
	public extern static BigInt _77fd605bbb6ce669(BigInt value);

	///<summary>Rotates a value left by a given amount.</summary>
	[Jazor(Op.Discard ,"static long.RotateLeft(long, int)")]
	public extern static BigInt _62ef461b6a515b85(BigInt value, Number rotateAmount);

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Discard ,"static long.RotateRight(long, int)")]
	public extern static BigInt _6a70bc88f689ce73(BigInt value, Number rotateAmount);

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Discard ,"static long.TrailingZeroCount(long)")]
	public extern static BigInt _df6d7288bc845b53(BigInt value);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard ,"static long.IsPow2(long)")]
	public extern static bool _fd78c89cf0a7feff(BigInt value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static long.Log2(long)")]
	public extern static BigInt _e90fc1096a04c8f9(BigInt value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static long.Clamp(long, long, long)")]
	public extern static BigInt _8e63712ecf0da200(BigInt value, BigInt min, BigInt max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Discard ,"static long.CopySign(long, long)")]
	public extern static BigInt _dd2c6c8297bd4df3(BigInt value, BigInt sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static long.Max(long, long)")]
	public extern static BigInt _2c60dae3f93fedef(BigInt x, BigInt y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static long.Min(long, long)")]
	public extern static BigInt _e9f5fe363044ceda(BigInt x, BigInt y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static long.Sign(long)")]
	public extern static Number _003e583f1faf343b(BigInt value);

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Discard ,"static long.Abs(long)")]
	public extern static BigInt _6ae5b36df368d1e5(BigInt value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static long.CreateChecked<TOther>(TOther)")]
	public extern static BigInt _a7b7a24d0da5bf7e<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static long.CreateSaturating<TOther>(TOther)")]
	public extern static BigInt _bef5211a1d823672<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static long.CreateTruncating<TOther>(TOther)")]
	public extern static BigInt _363a470fd9444076<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static long.IsEvenInteger(long)")]
	public extern static bool _203dfc08764b3516(BigInt value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Discard ,"static long.IsNegative(long)")]
	public extern static bool _cac37e2db2e55b1b(BigInt value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static long.IsOddInteger(long)")]
	public extern static bool _23594f30886ac699(BigInt value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Discard ,"static long.IsPositive(long)")]
	public extern static bool _3c8be08897a76569(BigInt value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static long.MaxMagnitude(long, long)")]
	public extern static BigInt _9618dc0d855ee729(BigInt x, BigInt y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static long.MinMagnitude(long, long)")]
	public extern static BigInt _bfad1ee52075b36e(BigInt x, BigInt y);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(string, System.IFormatProvider, out long)")]
	public extern static Array<object?> _6f90bee529e2eb6c(string? s, Intl.NumberFormat? provider, out BigInt result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static long.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static BigInt _22b931abaca743ae(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out long)")]
	public extern static Array<object?> _1fa9b46a2b1345f4(Uint32Array s, Intl.NumberFormat? provider, out BigInt result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static long.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _37d384b6ca28fb02(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out long)")]
	public extern static Array<object?> _0ea07687b9ce11f1(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, out BigInt result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static long.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static BigInt _45277b7b17f7a046(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static long.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out long)")]
	public extern static Array<object?> _232a05c0262521da(Uint8Array utf8Text, Intl.NumberFormat? provider, out BigInt result);
}
