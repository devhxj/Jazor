using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("float", WhiteListOp.Allowed, null, "System/SingleModule.js")]
public static class SingleModule
{
	//float.MinValue = -3.4028235E+38;

	//float.MaxValue = 3.4028235E+38;

	//float.Epsilon = 1E-45;

	//float.NegativeInfinity = -Infinity;

	//float.PositiveInfinity = Infinity;

	//float.NaN = NaN;

	[WhiteList("float.Single()", WhiteListOp.Discard)]
	public extern static Number _a6b96ca392da4917();

	///<summary>Determines whether the specified value is finite (zero, subnormal or normal).</summary>
	[WhiteList("static float.IsFinite(float)", WhiteListOp.Replace, "isFinite")]
	public extern static bool _00118f159d09918d(Number f);

	///<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
	[WhiteList("static float.IsInfinity(float)", WhiteListOp.Discard)]
	public extern static bool _47887f5e1e35e199(Number f);

	///<summary>Returns a value that indicates whether the specified value is not a number (NaN).</summary>
	[WhiteList("static float.IsNaN(float)", WhiteListOp.Replace, "isNaN")]
	public extern static bool _8c3d7a2e3b690c9a(Number f);

	///<summary>Determines whether the specified value is negative.</summary>
	[WhiteList("static float.IsNegative(float)", WhiteListOp.Discard)]
	public extern static bool _846e9450c3f550b6(Number f);

	///<summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
	[WhiteList("static float.IsNegativeInfinity(float)", WhiteListOp.Discard)]
	public extern static bool _8b4a47cad79ef70b(Number f);

	///<summary>Determines whether the specified value is normal.</summary>
	[WhiteList("static float.IsNormal(float)", WhiteListOp.Discard)]
	public extern static bool _cbc5abbbccc623b6(Number f);

	///<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
	[WhiteList("static float.IsPositiveInfinity(float)", WhiteListOp.Discard)]
	public extern static bool _b2b89b81c87952dc(Number f);

	///<summary>Determines whether the specified value is subnormal.</summary>
	[WhiteList("static float.IsSubnormal(float)", WhiteListOp.Discard)]
	public extern static bool _8e1067f50ae732cb(Number f);

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	[WhiteList("float.CompareTo(object)", WhiteListOp.CompareTo)]
	public extern static Number _0b80f2f2f1a3c1a6(Number instance, Object? value);

	///<summary>Compares this instance to a specified single-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified single-precision floating-point number.</summary>
	[WhiteList("float.CompareTo(float)", WhiteListOp.CompareTo)]
	public extern static Number _f6880f77edc2efe5(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override float.Equals(object)", WhiteListOp.Equals)]
	public extern static bool _eb69b50c7032a809(Number instance, Object? obj);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Single" /> object represent the same value.</summary>
	[WhiteList("float.Equals(float)", WhiteListOp.Equals)]
	public extern static bool _5c45db76bd764c38(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override float.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _96e065ea302b67da(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[WhiteList("override float.ToString()", WhiteListOp.Replace, "toString")]
	public extern static string _a036f8edeee45300(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("float.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _7343d8ada7c3d925(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[WhiteList("float.ToString(string)", WhiteListOp.Discard)]
	public extern static string _fe0300c4411a1f62(Number instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[WhiteList("float.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _d0d4042bef295e49(Number instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current float number instance into the provided span of characters.</summary>
	[WhiteList("float.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _3f2b511e96922b72(Number instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("float.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _bfce4d32c259361c(Number instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its single-precision floating-point number equivalent.</summary>
	[WhiteList("static float.Parse(string)", WhiteListOp.Import)]
	public static Number _d0492a7790d81596(string s)
	{
		return Number(s);
	}

	///<summary>Converts the string representation of a number in a specified style to its single-precision floating-point number equivalent.</summary>
	[WhiteList("static float.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static Number _77fa7745f751ec69(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its single-precision floating-point number equivalent.</summary>
	[WhiteList("static float.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _2aab5ef8cfa9accc(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	[WhiteList("static float.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _cddcce796b50f037(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	[WhiteList("static float.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _d9762c1528057110(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static float.TryParse(string, out float)", WhiteListOp.Import)]
	public static bool _ced8b209dbd75890(string s, Box<Number> result)
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

	///<summary>Converts the string representation of a number in a character span to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static float.TryParse(System.ReadOnlySpan<char>, out float)", WhiteListOp.Discard)]
	public extern static bool _8f337f9f610204bb(Uint32Array s, Box<Number> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its single-precision floating-point number equivalent.</summary>
	[WhiteList("static float.TryParse(System.ReadOnlySpan<byte>, out float)", WhiteListOp.Discard)]
	public extern static bool _35fa5333706d7ec4(Uint8Array utf8Text, Box<Number> result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static float.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out float)", WhiteListOp.Discard)]
	public extern static bool _6b58aaed45e38509(object s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static float.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out float)", WhiteListOp.Discard)]
	public extern static bool _3a7ff2c98489b96d(Uint32Array s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Single" />.</summary>
	[WhiteList("float.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _e38cf33130abe213(Number instance);

	///<summary>Determines if a value is a power of two.</summary>
	[WhiteList("static float.IsPow2(float)", WhiteListOp.Discard)]
	public extern static bool _0dcf89ab5d6bd60c(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[WhiteList("static float.Log2(float)", WhiteListOp.Replace, "log2")]
	public extern static Number _79aeb4d9a5bd7f76(Number value);

	///<summary>Computes E raised to a given power.</summary>
	[WhiteList("static float.Exp(float)", WhiteListOp.Replace, "exp")]
	public extern static Number _9feb625727b5f8b7(Number x);

	///<summary>Computes 2 raised to a given power.</summary>
	[WhiteList("static float.Exp2(float)", WhiteListOp.Replace, "expm1")]
	public extern static Number _850a2368fd9ebd00(Number x);

	///<summary>Computes the ceiling of a value.</summary>
	[WhiteList("static float.Ceiling(float)", WhiteListOp.Replace, "ceil")]
	public extern static Number _b6616ccde8acba3f(Number x);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[WhiteList("static float.ConvertToInteger<TInteger>(float)", WhiteListOp.Discard)]
	public extern static TInteger _b860c3e3eb3014d6<TInteger>(Number value);

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[WhiteList("static float.ConvertToIntegerNative<TInteger>(float)", WhiteListOp.Discard)]
	public extern static TInteger _59f5214dc916fb61<TInteger>(Number value);

	///<summary>Computes the floor of a value.</summary>
	[WhiteList("static float.Floor(float)", WhiteListOp.Replace, "floor")]
	public extern static Number _32eec2aa95114e61(Number x);

	///<summary>Rounds a value to the nearest integer using the default rounding mode.</summary>
	[WhiteList("static float.Round(float)", WhiteListOp.Replace, "round")]
	public extern static Number _99c8e34b34aa762c(Number x);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode.</summary>
	[WhiteList("static float.Round(float, int)", WhiteListOp.Discard)]
	public extern static Number _a0ef44092a5b0a96(Number x, Number digits);

	///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
	[WhiteList("static float.Round(float, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _34bdf4b36464daa4(Number x, object mode);

	///<summary>Rounds a value to a specified number of fractional-digits using the specified rounding mode.</summary>
	[WhiteList("static float.Round(float, int, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _b0f1294dc766b202(Number x, Number digits, object mode);

	///<summary>Truncates a value.</summary>
	[WhiteList("static float.Truncate(float)", WhiteListOp.Replace, "trunc")]
	public extern static Number _60637f5113854841(Number x);

	///<summary>Computes the arc-tangent of the quotient of two values.</summary>
	[WhiteList("static float.Atan2(float, float)", WhiteListOp.Replace, "atan2")]
	public extern static Number _81fb32cf771b3b93(Number y, Number x);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[WhiteList("static float.Clamp(float, float, float)", WhiteListOp.Discard)]
	public extern static Number _fa04e6b14ed00f24(Number value, Number min, Number max);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static float.Max(float, float)", WhiteListOp.Replace, "max")]
	public extern static Number _b4d95f21e04b4768(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static float.Min(float, float)", WhiteListOp.Replace, "min")]
	public extern static Number _f0e565231f96990c(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[WhiteList("static float.Sign(float)", WhiteListOp.Replace, "sign")]
	public extern static Number _323a6b94e62b2729(Number value);

	///<summary>Computes the absolute of a value.</summary>
	[WhiteList("static float.Abs(float)", WhiteListOp.Replace, "abs")]
	public extern static Number _a520369f28d7dc89(Number value);

	///<summary>Computes a value raised to a given power.</summary>
	[WhiteList("static float.Pow(float, float)", WhiteListOp.Replace, "pow")]
	public extern static Number _9dea84f9daad7225(Number x, Number y);

	///<summary>Computes the square-root of a value.</summary>
	[WhiteList("static float.Sqrt(float)", WhiteListOp.Replace, "sqrt")]
	public extern static Number _daecc788f9d305e5(Number x);

	///<summary>Computes the arc-cosine of a value.</summary>
	[WhiteList("static float.Acos(float)", WhiteListOp.Replace, "acos")]
	public extern static Number _fff14793e0685103(Number x);

	///<summary>Computes the arc-sine of a value.</summary>
	[WhiteList("static float.Asin(float)", WhiteListOp.Replace, "asin")]
	public extern static Number _753afad06a77a6ce(Number x);

	///<summary>Computes the arc-tangent of a value.</summary>
	[WhiteList("static float.Atan(float)", WhiteListOp.Replace, "atan")]
	public extern static Number _d91bd1cce9c18aa3(Number x);

	///<summary>Computes the cosine of a value.</summary>
	[WhiteList("static float.Cos(float)", WhiteListOp.Replace, "cos")]
	public extern static Number _aef0ed870d0a4481(Number x);

	///<summary>Computes the sine of a value.</summary>
	[WhiteList("static float.Sin(float)", WhiteListOp.Replace, "sin")]
	public extern static Number _28ff5aa7214bc112(Number x);

	///<summary>Computes the tangent of a value.</summary>
	[WhiteList("static float.Tan(float)", WhiteListOp.Replace, "tan")]
	public extern static Number _c379df7d9fb9a3bd(Number x);

	///<summary>Computes the natural (base-E) logarithm of a value.</summary>
	[WhiteList("static float.Log(float)", WhiteListOp.Replace, "log")]
	public extern static Number _0311a212e027ef2d(Number x);

	///<summary>Computes the logarithm of a value in the specified base.</summary>
	[WhiteList("static float.Log(float, float)", WhiteListOp.Discard)]
	public extern static Number _2346aa8a14187816(Number x, Number newBase);

	///<summary>Computes the base-10 logarithm of a value.</summary>
	[WhiteList("static float.Log10(float)", WhiteListOp.Replace, "log10")]
	public extern static Number _13b3c426479d8061(Number x);

	// ... 所有其他方法保持 Discard 状态
}
