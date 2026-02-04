using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("double", WhiteListOp.Allowed, null, "System/DoubleModule.js")]
public static class DoubleModule
{
	//double.MinValue = -1.7976931348623157E+308;
	//double.MaxValue = 1.7976931348623157E+308;
	//double.Epsilon = 5E-324;
	//double.NegativeInfinity = -Infinity;
	//double.PositiveInfinity = Infinity;
	//double.NaN = NaN;

	[WhiteList("double.Double()", WhiteListOp.Discard)]
	public extern static Number _f28ac141e9398355();

	///<summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
	[WhiteList("static double.IsFinite(double)", WhiteListOp.Replace, "isFinite")]
	public extern static bool _aed2927097617729(Number d);

	///<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
	[WhiteList("static double.IsInfinity(double)", WhiteListOp.Discard)]
	public extern static bool _8dab2b2ebaef92eb(Number d);

	///<summary>Returns a value that indicates whether the specified value is not a number (NaN).</summary>
	[WhiteList("static double.IsNaN(double)", WhiteListOp.Replace, "isNaN")]
	public extern static bool _24e14b276e0c7e30(Number d);

	///<summary>Determines whether the specified value is negative.</summary>
	[WhiteList("static double.IsNegative(double)", WhiteListOp.Discard)]
	public extern static bool _2f6ba4398ec15d8d(Number d);

	///<summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
	[WhiteList("static double.IsNegativeInfinity(double)", WhiteListOp.Discard)]
	public extern static bool _f0fb1d1302b488d6(Number d);

	///<summary>Determines whether the specified value is normal.</summary>
	[WhiteList("static double.IsNormal(double)", WhiteListOp.Discard)]
	public extern static bool _9b3adc853b9cfe8f(Number d);

	///<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
	[WhiteList("static double.IsPositiveInfinity(double)", WhiteListOp.Discard)]
	public extern static bool _d15ff5d4064e951a(Number d);

	///<summary>Determines whether the specified value is subnormal.</summary>
	[WhiteList("static double.IsSubnormal(double)", WhiteListOp.Discard)]
	public extern static bool _a48f9d7298aa7e76(Number d);

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	[WhiteList("double.CompareTo(object)", WhiteListOp.CompareTo)]
	public extern static Number _b0d483b6deae2278(Number instance, Object? value);

	///<summary>Compares this instance to a specified double-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified double-precision floating-point number.</summary>
	[WhiteList("double.CompareTo(double)", WhiteListOp.CompareTo)]
	public extern static Number _7b8150796366d2b1(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override double.Equals(object)", WhiteListOp.Equals)]
	public extern static bool _b5f97a04bba189b0(Number instance, Object? obj);

	///<summary>Returns a value that indicates whether two specified <see cref="T:System.Double" /> values are equal.</summary>
	[WhiteList("static double.operator ==(double, double)", WhiteListOp.Allowed)]
	public extern static bool _a4d750aa912f2bd7(Number left, Number right);

	///<summary>Returns a value that indicates whether two specified <see cref="T:System.Double" /> values are not equal.</summary>
	[WhiteList("static double.operator !=(double, double)", WhiteListOp.Allowed)]
	public extern static bool _d17fe84520a83d30(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <see cref="T:System.Double" /> value is less than another specified <see cref="T:System.Double" /> value.</summary>
	[WhiteList("static double.operator <(double, double)", WhiteListOp.Allowed)]
	public extern static bool _f33377c7d472de67(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <see cref="T:System.Double" /> value is greater than another specified <see cref="T:System.Double" /> value.</summary>
	[WhiteList("static double.operator >(double, double)", WhiteListOp.Allowed)]
	public extern static bool _0ff0091b916b4a34(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <see cref="T:System.Double" /> value is less than or equal to another specified <see cref="T:System.Double" /> value.</summary>
	[WhiteList("static double.operator <=(double, double)", WhiteListOp.Allowed)]
	public extern static bool _cda1ab775e265c7b(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <see cref="T:System.Double" /> value is greater than or equal to another specified <see cref="T:System.Double" /> value.</summary>
	[WhiteList("static double.operator >=(double, double)", WhiteListOp.Allowed)]
	public extern static bool _4f7605355b48150a(Number left, Number right);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Double" /> object represent the same value.</summary>
	[WhiteList("double.Equals(double)", WhiteListOp.Equals)]
	public extern static bool _6c01d37504f73181(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override double.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _73dea7106d8085a6(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[WhiteList("override double.ToString()", WhiteListOp.Replace, "toString")]
	public extern static string _faf4dc1f54bddf75(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[WhiteList("double.ToString(string)", WhiteListOp.Discard)]
	public extern static string _3fdd3b28b5e148e9(Number instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("double.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _060e7930ebdb6c74(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[WhiteList("double.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _3ab59f70a1114579(Number instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current double instance into the provided span of characters.</summary>
	[WhiteList("double.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _10530f8449c5e278(Number instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("double.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _d57e531de43c78e1(Number instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its double-precision floating-point number equivalent.</summary>
	[WhiteList("static double.Parse(string)", WhiteListOp.Import)]
	public static Number _5810f85a3710b88d(string s)
	{
		return Number(s);
	}

	///<summary>Converts the string representation of a number in a specified style to its double-precision floating-point number equivalent.</summary>
	[WhiteList("static double.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static Number _41091ebfff87c5a3(object s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its double-precision floating-point number equivalent.</summary>
	[WhiteList("static double.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _5b091c28760d19a0(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent.</summary>
	[WhiteList("static double.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _e23e5c173e845cc9(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent.</summary>
	[WhiteList("static double.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _1566d690221e91c2(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static double.TryParse(string, out double)", WhiteListOp.Import)]
	public static bool _a29d389185c5e37d(string s, Box<Number> result)
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

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static double.TryParse(System.ReadOnlySpan<char>, out double)", WhiteListOp.Discard)]
	public extern static bool _059799e0a3b763c1(Uint32Array s, Box<Number> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its double-precision floating-point number equivalent.</summary>
	[WhiteList("static double.TryParse(System.ReadOnlySpan<byte>, out double)", WhiteListOp.Discard)]
	public extern static bool _ec88293b6cb03791(Uint8Array utf8Text, Box<Number> result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static double.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out double)", WhiteListOp.Discard)]
	public extern static bool _ac0f50fde0490598(object s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Converts a character span containing the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static double.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out double)", WhiteListOp.Discard)]
	public extern static bool _632e234f0359bd6f(Uint32Array s, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Double" />.</summary>
	[WhiteList("double.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _faf3eda13d4c24c6(Number instance);

	///<summary>Determines if a value is a power of two.</summary>
	[WhiteList("static double.IsPow2(double)", WhiteListOp.Discard)]
	public extern static bool _0f9f49a802919a8f(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[WhiteList("static double.Log2(double)", WhiteListOp.Replace, "log2")]
	public extern static Number _3ca26f53faecc630(Number value);

	///<summary>Computes E raised to a given power.</summary>
	[WhiteList("static double.Exp(double)", WhiteListOp.Replace, "exp")]
	public extern static Number _e94626bfb529f1e2(Number x);

	///<summary>Computes the ceiling of a value.</summary>
	[WhiteList("static double.Ceiling(double)", WhiteListOp.Replace, "ceil")]
	public extern static Number _e435d9759ac9c07d(Number x);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[WhiteList("static double.ConvertToInteger<TInteger>(double)", WhiteListOp.Discard)]
	public extern static TInteger _cf8db91150253994<TInteger>(Number value);

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[WhiteList("static double.ConvertToIntegerNative<TInteger>(double)", WhiteListOp.Discard)]
	public extern static TInteger _869e51717acd1e28<TInteger>(Number value);

	///<summary>Computes the floor of a value.</summary>
	[WhiteList("static double.Floor(double)", WhiteListOp.Replace, "floor")]
	public extern static Number _52dffd07187dd0c2(Number x);

	///<summary>Rounds a value to the nearest integer using the default rounding mode.</summary>
	[WhiteList("static double.Round(double)", WhiteListOp.Replace, "round")]
	public extern static Number _0bc6b7459346bc5f(Number x);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode.</summary>
	[WhiteList("static double.Round(double, int)", WhiteListOp.Discard)]
	public extern static Number _b439595e3752c6a9(Number x, Number digits);

	///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
	[WhiteList("static double.Round(double, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _7aeacc68b27f02f7(Number x, object mode);

	///<summary>Rounds a value to a specified number of fractional-digits using the specified rounding mode.</summary>
	[WhiteList("static double.Round(double, int, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _6e429701c9779ef6(Number x, Number digits, object mode);

	///<summary>Truncates a value.</summary>
	[WhiteList("static double.Truncate(double)", WhiteListOp.Replace, "trunc")]
	public extern static Number _98f3d13b9b717048(Number x);

	///<summary>Computes the arc-tangent of the quotient of two values.</summary>
	[WhiteList("static double.Atan2(double, double)", WhiteListOp.Replace, "atan2")]
	public extern static Number _d606d02df668235c(Number y, Number x);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static double.Max(double, double)", WhiteListOp.Replace, "max")]
	public extern static Number _4d275f0cc2087a70(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static double.Min(double, double)", WhiteListOp.Replace, "min")]
	public extern static Number _8a25c3cdacb6ea23(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[WhiteList("static double.Sign(double)", WhiteListOp.Replace, "sign")]
	public extern static Number _eee146c74a9bc322(Number value);

	///<summary>Computes the absolute of a value.</summary>
	[WhiteList("static double.Abs(double)", WhiteListOp.Replace, "abs")]
	public extern static Number _13256ae561a599a8(Number value);

	///<summary>Computes a value raised to a given power.</summary>
	[WhiteList("static double.Pow(double, double)", WhiteListOp.Replace, "pow")]
	public extern static Number _a9ce690fc0374936(Number x, Number y);

	///<summary>Computes the square-root of a value.</summary>
	[WhiteList("static double.Sqrt(double)", WhiteListOp.Replace, "sqrt")]
	public extern static Number _73df268429011d00(Number x);

	///<summary>Computes the arc-cosine of a value.</summary>
	[WhiteList("static double.Acos(double)", WhiteListOp.Replace, "acos")]
	public extern static Number _1c32d7b441f1bec1(Number x);

	///<summary>Computes the arc-sine of a value.</summary>
	[WhiteList("static double.Asin(double)", WhiteListOp.Replace, "asin")]
	public extern static Number _517eb387ef38a60b(Number x);

	///<summary>Computes the arc-tangent of a value.</summary>
	[WhiteList("static double.Atan(double)", WhiteListOp.Replace, "atan")]
	public extern static Number _a6a8f60d8be1baab(Number x);

	///<summary>Computes the cosine of a value.</summary>
	[WhiteList("static double.Cos(double)", WhiteListOp.Replace, "cos")]
	public extern static Number _ab249d49b3cb5f87(Number x);

	///<summary>Computes the sine of a value.</summary>
	[WhiteList("static double.Sin(double)", WhiteListOp.Replace, "sin")]
	public extern static Number _82a42c3870a8a263(Number x);

	///<summary>Computes the tangent of a value.</summary>
	[WhiteList("static double.Tan(double)", WhiteListOp.Replace, "tan")]
	public extern static Number _3f5c35650c642d58(Number x);

	///<summary>Computes the natural (base-E) logarithm of a value.</summary>
	[WhiteList("static double.Log(double)", WhiteListOp.Replace, "log")]
	public extern static Number _f89aa2d9ce52cc5e(Number x);

	///<summary>Computes the logarithm of a value in the specified base.</summary>
	[WhiteList("static double.Log(double, double)", WhiteListOp.Discard)]
	public extern static Number _2367dc158f1f7ec9(Number x, Number newBase);

	///<summary>Computes the base-10 logarithm of a value.</summary>
	[WhiteList("static double.Log10(double)", WhiteListOp.Replace, "log10")]
	public extern static Number _d057b30c2fca7de9(Number x);

	// ... 所有其他方法保持 Discard 状态
}
