using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("double", WhiteListOp.Allowed, null,"System/DoubleModule.js")]
public static class DoubleModule
{
	//double.MinValue = -1.7976931348623157E+308;

	//double.MaxValue = 1.7976931348623157E+308;

	//double.Epsilon = 5E-324;

	//double.NegativeInfinity = -∞;

	//double.PositiveInfinity = ∞;

	//double.NaN = NaN;

	//double.NegativeZero = -0;

	//double.E = 2.718281828459045;

	//double.Pi = 3.141592653589793;

	//double.Tau = 6.283185307179586;

	[WhiteList("double.Double()", WhiteListOp.Discard)]
	public extern static Number _f28ac141e9398355();

	///<summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
	[WhiteList("static double.IsFinite(double)", WhiteListOp.Discard)]
	public extern static bool _aed2927097617729(Number d);

	///<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
	[WhiteList("static double.IsInfinity(double)", WhiteListOp.Discard)]
	public extern static bool _8dab2b2ebaef92eb(Number d);

	///<summary>Returns a value that indicates whether the specified value is not a number (<see cref="F:System.Double.NaN" />).</summary>
	[WhiteList("static double.IsNaN(double)", WhiteListOp.Discard)]
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
	[WhiteList("double.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _b0d483b6deae2278(Number instance, Object? value);

	///<summary>Compares this instance to a specified double-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified double-precision floating-point number.</summary>
	[WhiteList("double.CompareTo(double)", WhiteListOp.Discard)]
	public extern static Number _7b8150796366d2b1(Number instance, Number value);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override double.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _b5f97a04bba189b0(Number instance, Object? obj);

	///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> values are equal.</summary>
	[WhiteList("static double.operator ==(double, double)", WhiteListOp.Allowed)]
	public extern static bool _a4d750aa912f2bd7(Number left, Number right);

	///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> values are not equal.</summary>
	[WhiteList("static double.operator !=(double, double)", WhiteListOp.Allowed)]
	public extern static bool _d17fe84520a83d30(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
	[WhiteList("static double.operator <(double, double)", WhiteListOp.Allowed)]
	public extern static bool _f33377c7d472de67(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
	[WhiteList("static double.operator >(double, double)", WhiteListOp.Allowed)]
	public extern static bool _0ff0091b916b4a34(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
	[WhiteList("static double.operator <=(double, double)", WhiteListOp.Allowed)]
	public extern static bool _cda1ab775e265c7b(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
	[WhiteList("static double.operator >=(double, double)", WhiteListOp.Allowed)]
	public extern static bool _4f7605355b48150a(Number left, Number right);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Double" /> object represent the same value.</summary>
	[WhiteList("double.Equals(double)", WhiteListOp.Discard)]
	public extern static bool _6c01d37504f73181(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override double.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _73dea7106d8085a6(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[WhiteList("override double.ToString()", WhiteListOp.Discard)]
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
	[WhiteList("static double.Parse(string)", WhiteListOp.Discard)]
	public extern static Number _5810f85a3710b88d(object s);

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
	[WhiteList("static double.TryParse(string, out double)", WhiteListOp.Discard)]
	public extern static bool _a29d389185c5e37d(object s, Box<Number> result);

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
	[WhiteList("static double.Log2(double)", WhiteListOp.Discard)]
	public extern static Number _3ca26f53faecc630(Number value);

	///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
	[WhiteList("static double.Exp(double)", WhiteListOp.Discard)]
	public extern static Number _e94626bfb529f1e2(Number x);

	///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
	[WhiteList("static double.ExpM1(double)", WhiteListOp.Discard)]
	public extern static Number _1a8fc1577d8842a1(Number x);

	///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
	[WhiteList("static double.Exp2(double)", WhiteListOp.Discard)]
	public extern static Number _894bcd9f10fe195f(Number x);

	///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
	[WhiteList("static double.Exp2M1(double)", WhiteListOp.Discard)]
	public extern static Number _b2c7a69c53b5558f(Number x);

	///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
	[WhiteList("static double.Exp10(double)", WhiteListOp.Discard)]
	public extern static Number _433ea7f5bfe42847(Number x);

	///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
	[WhiteList("static double.Exp10M1(double)", WhiteListOp.Discard)]
	public extern static Number _aece0b0b794624da(Number x);

	///<summary>Computes the ceiling of a value.</summary>
	[WhiteList("static double.Ceiling(double)", WhiteListOp.Discard)]
	public extern static Number _e435d9759ac9c07d(Number x);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[WhiteList("static double.ConvertToInteger<TInteger>(double)", WhiteListOp.Discard)]
	public extern static TInteger _cf8db91150253994<TInteger>(Number value);

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[WhiteList("static double.ConvertToIntegerNative<TInteger>(double)", WhiteListOp.Discard)]
	public extern static TInteger _869e51717acd1e28<TInteger>(Number value);

	///<summary>Computes the floor of a value.</summary>
	[WhiteList("static double.Floor(double)", WhiteListOp.Discard)]
	public extern static Number _52dffd07187dd0c2(Number x);

	///<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[WhiteList("static double.Round(double)", WhiteListOp.Discard)]
	public extern static Number _0bc6b7459346bc5f(Number x);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[WhiteList("static double.Round(double, int)", WhiteListOp.Discard)]
	public extern static Number _b439595e3752c6a9(Number x, Number digits);

	///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
	[WhiteList("static double.Round(double, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _7aeacc68b27f02f7(Number x, object mode);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[WhiteList("static double.Round(double, int, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _6e429701c9779ef6(Number x, Number digits, object mode);

	///<summary>Truncates a value.</summary>
	[WhiteList("static double.Truncate(double)", WhiteListOp.Discard)]
	public extern static Number _98f3d13b9b717048(Number x);

	///<summary>Computes the arc-tangent of the quotient of two values.</summary>
	[WhiteList("static double.Atan2(double, double)", WhiteListOp.Discard)]
	public extern static Number _d606d02df668235c(Number y, Number x);

	///<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[WhiteList("static double.Atan2Pi(double, double)", WhiteListOp.Discard)]
	public extern static Number _f54e39103ea7d6b5(Number y, Number x);

	///<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
	[WhiteList("static double.BitDecrement(double)", WhiteListOp.Discard)]
	public extern static Number _4ce9474a7b3b7534(Number x);

	///<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
	[WhiteList("static double.BitIncrement(double)", WhiteListOp.Discard)]
	public extern static Number _a83d47e386f63de0(Number x);

	///<summary>Computes the fused multiply-add of three values.</summary>
	[WhiteList("static double.FusedMultiplyAdd(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _a7385e0d1e651c3f(Number left, Number right, Number addend);

	///<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
	[WhiteList("static double.Ieee754Remainder(double, double)", WhiteListOp.Discard)]
	public extern static Number _092bc2bc891d33a8(Number left, Number right);

	///<summary>Computes the integer logarithm of a value.</summary>
	[WhiteList("static double.ILogB(double)", WhiteListOp.Discard)]
	public extern static Number _48628732b1dc8ac9(Number x);

	///<summary>Performs a linear interpolation between two values based on the given weight.</summary>
	[WhiteList("static double.Lerp(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _a5426c98bc8a2df3(Number value1, Number value2, Number amount);

	///<summary>Computes an estimate of the reciprocal of a value.</summary>
	[WhiteList("static double.ReciprocalEstimate(double)", WhiteListOp.Discard)]
	public extern static Number _a07d02f7af20108d(Number x);

	///<summary>Computes an estimate of the reciprocal square root of a value.</summary>
	[WhiteList("static double.ReciprocalSqrtEstimate(double)", WhiteListOp.Discard)]
	public extern static Number _093ed023d5ee163e(Number x);

	///<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
	[WhiteList("static double.ScaleB(double, int)", WhiteListOp.Discard)]
	public extern static Number _efc90b780554b82f(Number x, Number n);

	///<summary>Computes the hyperbolic arc-cosine of a value.</summary>
	[WhiteList("static double.Acosh(double)", WhiteListOp.Discard)]
	public extern static Number _a0e391e3d9aa5827(Number x);

	///<summary>Computes the hyperbolic arc-sine of a value.</summary>
	[WhiteList("static double.Asinh(double)", WhiteListOp.Discard)]
	public extern static Number _57778d867801a120(Number x);

	///<summary>Computes the hyperbolic arc-tangent of a value.</summary>
	[WhiteList("static double.Atanh(double)", WhiteListOp.Discard)]
	public extern static Number _21375f189d937aa8(Number x);

	///<summary>Computes the hyperbolic cosine of a value.</summary>
	[WhiteList("static double.Cosh(double)", WhiteListOp.Discard)]
	public extern static Number _e4a259570c5acab6(Number x);

	///<summary>Computes the hyperbolic sine of a value.</summary>
	[WhiteList("static double.Sinh(double)", WhiteListOp.Discard)]
	public extern static Number _dea96f28cdef92ad(Number x);

	///<summary>Computes the hyperbolic tangent of a value.</summary>
	[WhiteList("static double.Tanh(double)", WhiteListOp.Discard)]
	public extern static Number _5169c7d89ba27c38(Number x);

	///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
	[WhiteList("static double.Log(double)", WhiteListOp.Discard)]
	public extern static Number _f89aa2d9ce52cc5e(Number x);

	///<summary>Computes the logarithm of a value in the specified base.</summary>
	[WhiteList("static double.Log(double, double)", WhiteListOp.Discard)]
	public extern static Number _2367dc158f1f7ec9(Number x, Number newBase);

	///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
	[WhiteList("static double.LogP1(double)", WhiteListOp.Discard)]
	public extern static Number _379f80adec6e897b(Number x);

	///<summary>Computes the base-2 logarithm of a value plus one.</summary>
	[WhiteList("static double.Log2P1(double)", WhiteListOp.Discard)]
	public extern static Number _0f38233678cfefdc(Number x);

	///<summary>Computes the base-10 logarithm of a value.</summary>
	[WhiteList("static double.Log10(double)", WhiteListOp.Discard)]
	public extern static Number _d057b30c2fca7de9(Number x);

	///<summary>Computes the base-10 logarithm of a value plus one.</summary>
	[WhiteList("static double.Log10P1(double)", WhiteListOp.Discard)]
	public extern static Number _f0b78003a9ab01fb(Number x);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[WhiteList("static double.Clamp(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _8a90b4c9a1beefd9(Number value, Number min, Number max);

	[WhiteList("static double.ClampNative(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _ead55aa3a172f045(Number value, Number min, Number max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[WhiteList("static double.CopySign(double, double)", WhiteListOp.Discard)]
	public extern static Number _7d753440d9da2ba5(Number value, Number sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static double.Max(double, double)", WhiteListOp.Discard)]
	public extern static Number _4d275f0cc2087a70(Number x, Number y);

	[WhiteList("static double.MaxNative(double, double)", WhiteListOp.Discard)]
	public extern static Number _a0dd8cfd308fc2ee(Number x, Number y);

	///<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[WhiteList("static double.MaxNumber(double, double)", WhiteListOp.Discard)]
	public extern static Number _ca88bd0ea64fa29f(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static double.Min(double, double)", WhiteListOp.Discard)]
	public extern static Number _8a25c3cdacb6ea23(Number x, Number y);

	[WhiteList("static double.MinNative(double, double)", WhiteListOp.Discard)]
	public extern static Number _2aadcd7ef1e13714(Number x, Number y);

	///<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[WhiteList("static double.MinNumber(double, double)", WhiteListOp.Discard)]
	public extern static Number _d19f0527d6ae110f(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[WhiteList("static double.Sign(double)", WhiteListOp.Discard)]
	public extern static Number _eee146c74a9bc322(Number value);

	///<summary>Computes the absolute of a value.</summary>
	[WhiteList("static double.Abs(double)", WhiteListOp.Discard)]
	public extern static Number _13256ae561a599a8(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static double.CreateChecked<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _ddfc88bb430f2c3e<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static double.CreateSaturating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _5bb76ff1642d9cf8<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("static double.CreateTruncating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _e3a12f862df0ccea<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[WhiteList("static double.IsEvenInteger(double)", WhiteListOp.Discard)]
	public extern static bool _e3c00c1b96ee23bd(Number value);

	///<summary>Determines if a value represents an integral value.</summary>
	[WhiteList("static double.IsInteger(double)", WhiteListOp.Discard)]
	public extern static bool _f0cb8da3d3123834(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[WhiteList("static double.IsOddInteger(double)", WhiteListOp.Discard)]
	public extern static bool _0f52036842645ea9(Number value);

	///<summary>Determines if a value is positive.</summary>
	[WhiteList("static double.IsPositive(double)", WhiteListOp.Discard)]
	public extern static bool _c1220c050b39d180(Number value);

	///<summary>Determines if a value represents a real number.</summary>
	[WhiteList("static double.IsRealNumber(double)", WhiteListOp.Discard)]
	public extern static bool _0e7439da8bbce1ab(Number value);

	///<summary>Compares two values to compute which is greater.</summary>
	[WhiteList("static double.MaxMagnitude(double, double)", WhiteListOp.Discard)]
	public extern static Number _b6202851542d164c(Number x, Number y);

	///<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[WhiteList("static double.MaxMagnitudeNumber(double, double)", WhiteListOp.Discard)]
	public extern static Number _7f7b38b043f3f42f(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[WhiteList("static double.MinMagnitude(double, double)", WhiteListOp.Discard)]
	public extern static Number _bb1daa880a2ad14e(Number x, Number y);

	///<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[WhiteList("static double.MinMagnitudeNumber(double, double)", WhiteListOp.Discard)]
	public extern static Number _315c6cdfa11efcf2(Number x, Number y);

	///<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
	[WhiteList("static double.MultiplyAddEstimate(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _a3676143141ac38a(Number left, Number right, Number addend);

	///<summary>Tries to parse a string into a value.</summary>
	[WhiteList("static double.TryParse(string, System.IFormatProvider, out double)", WhiteListOp.Discard)]
	public extern static bool _f1644d5121fae09c(object s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Computes a value raised to a given power.</summary>
	[WhiteList("static double.Pow(double, double)", WhiteListOp.Discard)]
	public extern static Number _a9ce690fc0374936(Number x, Number y);

	///<summary>Computes the cube-root of a value.</summary>
	[WhiteList("static double.Cbrt(double)", WhiteListOp.Discard)]
	public extern static Number _be2f8c6b23df2f9d(Number x);

	///<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
	[WhiteList("static double.Hypot(double, double)", WhiteListOp.Discard)]
	public extern static Number _7b8e31add532abe8(Number x, Number y);

	///<summary>Computes the n-th root of a value.</summary>
	[WhiteList("static double.RootN(double, int)", WhiteListOp.Discard)]
	public extern static Number _83649fc6ded4d88e(Number x, Number n);

	///<summary>Computes the square-root of a value.</summary>
	[WhiteList("static double.Sqrt(double)", WhiteListOp.Discard)]
	public extern static Number _73df268429011d00(Number x);

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("static double.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _ffac89005f82f8e5(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[WhiteList("static double.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out double)", WhiteListOp.Discard)]
	public extern static bool _55ffdd4c4ffdc9a8(Uint32Array s, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Computes the arc-cosine of a value.</summary>
	[WhiteList("static double.Acos(double)", WhiteListOp.Discard)]
	public extern static Number _1c32d7b441f1bec1(Number x);

	///<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[WhiteList("static double.AcosPi(double)", WhiteListOp.Discard)]
	public extern static Number _4a99593b807868d6(Number x);

	///<summary>Computes the arc-sine of a value.</summary>
	[WhiteList("static double.Asin(double)", WhiteListOp.Discard)]
	public extern static Number _517eb387ef38a60b(Number x);

	///<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[WhiteList("static double.AsinPi(double)", WhiteListOp.Discard)]
	public extern static Number _1a0239dc7bac42d0(Number x);

	///<summary>Computes the arc-tangent of a value.</summary>
	[WhiteList("static double.Atan(double)", WhiteListOp.Discard)]
	public extern static Number _a6a8f60d8be1baab(Number x);

	///<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
	[WhiteList("static double.AtanPi(double)", WhiteListOp.Discard)]
	public extern static Number _fa0c5717daf60a22(Number x);

	///<summary>Computes the cosine of a value.</summary>
	[WhiteList("static double.Cos(double)", WhiteListOp.Discard)]
	public extern static Number _ab249d49b3cb5f87(Number x);

	///<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
	[WhiteList("static double.CosPi(double)", WhiteListOp.Discard)]
	public extern static Number _68646d1a3f7e1c4e(Number x);

	///<summary>Converts a given value from degrees to radians.</summary>
	[WhiteList("static double.DegreesToRadians(double)", WhiteListOp.Discard)]
	public extern static Number _b613a401ab60cfa7(Number degrees);

	///<summary>Converts a given value from radians to degrees.</summary>
	[WhiteList("static double.RadiansToDegrees(double)", WhiteListOp.Discard)]
	public extern static Number _1ed0662536b0a079(Number radians);

	///<summary>Computes the sine of a value.</summary>
	[WhiteList("static double.Sin(double)", WhiteListOp.Discard)]
	public extern static Number _82a42c3870a8a263(Number x);

	///<summary>Computes the sine and cosine of a value.</summary>
	[WhiteList("static double.SinCos(double)", WhiteListOp.Discard)]
	public extern static (double Sin, double Cos) _bc56189e3e1f8a22(Number x);

	///<summary>Computes the sine and cosine of a value.</summary>
	[WhiteList("static double.SinCosPi(double)", WhiteListOp.Discard)]
	public extern static (double SinPi, double CosPi) _0f4aeef5d225794d(Number x);

	///<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
	[WhiteList("static double.SinPi(double)", WhiteListOp.Discard)]
	public extern static Number _364c4226f027481d(Number x);

	///<summary>Computes the tangent of a value.</summary>
	[WhiteList("static double.Tan(double)", WhiteListOp.Discard)]
	public extern static Number _3f5c35650c642d58(Number x);

	///<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
	[WhiteList("static double.TanPi(double)", WhiteListOp.Discard)]
	public extern static Number _c193db8303daa585(Number x);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static double.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _95cf4052dcf1d6d8(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static double.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out double)", WhiteListOp.Discard)]
	public extern static bool _654e8bbd8869bbea(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Box<Number> result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[WhiteList("static double.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _cd8bb3b9e099ef63(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[WhiteList("static double.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out double)", WhiteListOp.Discard)]
	public extern static bool _75fcd554c7fa663e(Uint8Array utf8Text, Intl.NumberFormat? provider, Box<Number> result);
}
