namespace ECMAScript;

/// <summary>
/// The Maths namespace object contains static properties and methods for mathematical constants and functions.
/// Maths works with the Number type.It doesn't work with BigInt.
/// </summary>
[ECMAScript]
public static class Maths
{
	/// <summary>
	/// The mathematical constant e. This is Euler's number, the base of natural logarithms.
	/// </summary>
	public readonly static Number E = 2.7182818284590451;

	/// <summary>
	/// The natural logarithm of 10.
	/// </summary>
	public readonly static Number LN10 = 2.302585092994046;

	/// <summary>
	/// The natural logarithm of 2.
	/// </summary>
	public readonly static Number LN2 = 0.6931471805599453;

	/// <summary>
	/// The base-2 logarithm of e.
	/// </summary>
	public readonly static Number LOG2E = 1.4426950408889634;

	/// <summary>
	/// The base-10 logarithm of e.
	/// </summary>
	public readonly static Number LOG10E = 0.4342944819032518;

	/// <summary>
	/// Pi. This is the ratio of the circumference of a circle to its diameter.
	/// </summary>
	public readonly static Number PI = 3.1415926535897931;

	/// <summary>
	/// The square root of 0.5, or, equivalently, one divided by the square root of 2.
	/// </summary>
	public readonly static Number SQRT1_2 = 0.7071067811865476;

	/// <summary>
	/// The square root of 2.
	/// </summary>
	public readonly static Number SQRT2 = 1.4142135623730951;

	/// <summary>
	/// Returns the absolute value of a number (the value without regard to whether it is positive or negative).
	/// For example, the absolute value of -5 is the same as the absolute value of 5.
	/// </summary>
	/// <param name="x">A numeric expression for which the absolute value is needed.</param>
	/// <returns></returns>
	public extern static Number Abs(Number x);

	/// <summary>
	/// Returns the arc cosine (or inverse cosine) of a number.
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Acos(Number x);

	/// <summary>
	/// 返回一个数的反双曲余弦值
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Acosh(Number x);

	/// <summary>
	/// Returns the arcsine of a number.
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Asin(Number x);

	/// <summary>
	/// 返回一个数值的反双曲正弦值
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Asinh(Number x);

	/// <summary>
	/// Returns the arctangent of a number.
	/// </summary>
	/// <param name="x">A numeric expression for which the arctangent is needed.</param>
	/// <returns></returns>
	public extern static Number Atan(Number x);

	/// <summary>
	/// Returns the angle (in radians) between the X axis and the line going through both the origin and the given point.
	/// </summary>
	/// <param name="y">A numeric expression representing the cartesian y-coordinate.</param>
	/// <param name="x">A numeric expression representing the cartesian x-coordinate.</param>
	/// <returns></returns>
	public extern static Number Atan2(Number y, Number x);

	/// <summary>
	/// 返回一个数值反双曲正切值
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Atanh(Number x);

	/// <summary>
	/// 返回任意数字的立方根
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Cbrt(Number x);

	/// <summary>
	/// Returns the smallest integer greater than or equal to its numeric argument.
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Ceil(Number x);

	/// <summary>
	/// Returns the number of leading zero bits in the 32-bit binary representation of a number.
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Clz32(Number x);

	/// <summary>
	/// Returns the cosine of a number.
	/// </summary>
	/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
	/// <returns></returns>
	public extern static Number Cos(Number x);

	/// <summary>
	/// 返回数值的双曲余弦函数
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Cosh(Number x);

	/// <summary>
	/// Returns e (the base of natural logarithms) raised to a power.
	/// </summary>
	/// <param name="x">A numeric expression representing the power of e.</param>
	/// <returns></returns>
	public extern static Number Exp(Number x);

	/// <summary>
	/// Calculates e^x - 1, where e is the base of natural logarithms.
	/// </summary>
	/// <remarks>This method provides an optimized calculation for small values of <paramref name="x"/> to improve
	/// precision. For larger values, it uses the standard exponential calculation.</remarks>
	/// <param name="x">The exponent value for which to calculate e^x - 1.</param>
	/// <returns>The result of e^x - 1. Returns <see cref="double.NaN"/> if <paramref name="x"/> is <see cref="double.NaN"/>.
	/// Returns <see cref="double.PositiveInfinity"/> if <paramref name="x"/> is positive infinity. Returns -1 if <paramref
	/// name="x"/> is negative infinity.</returns>
	public extern static Number Expm1(Number x);

	/// <summary>
	/// Returns the greatest integer less than or equal to its numeric argument.
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Floor(Number x);

	/// <summary>
	/// returns the nearest 32-bit single precision float representation of a number.
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	/// <summary>
	/// 
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Fround(Number x);

	/// <summary>
	/// returns the square root of the sum of squares of its arguments.
	/// </summary>
	/// <param name="values"></param>
	/// <returns></returns>
	public extern static Number Hypot(params Number[] values);

	/// <summary>
	/// Returns the result of the C-like 32-bit multiplication of the two parameters.
	/// </summary>
	/// <param name="a">First number.</param>
	/// <param name="b">Second number.</param>
	/// <returns></returns>
	public extern static Number Imul(Number a, Number b);

	/// <summary>
	/// Returns the natural logarithm (base e) of a number.
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Log(Number x);

	/// <summary>
	/// Returns the base 10 logarithm of a number. 
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Log10(Number x);

	/// <summary>
	/// Returns the natural logarithm (base e) of 1 + x, where x is the argument.
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Log1p(Number x);

	/// <summary>
	/// Returns the base 2 logarithm of a number. 
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Log2(Number x);

	/// <summary>
	/// Returns the larger of a set of supplied numeric expressions.
	/// </summary>
	/// <param name="values">Numeric expressions to be evaluated.</param>
	/// <returns></returns>
	public extern static Number Max(params Number[] values);

	/// <summary>
	/// Returns the smaller of a set of supplied numeric expressions.
	/// </summary>
	/// <param name="values">Numeric expressions to be evaluated.</param>
	/// <returns></returns>
	public extern static Number Min(params Number[] values);

	/// <summary>
	/// Returns the value of a base expression taken to a specified power.
	/// </summary>
	/// <param name="x">The base value of the expression.</param>
	/// <param name="y">The exponent value of the expression.</param>
	/// <returns></returns>
	public extern static Number Pow(Number x, Number y);

	/// <summary>
	/// Returns a pseudorandom number between 0 and 1.
	/// </summary>
	/// <returns></returns>
	public extern static Number Random();

	/// <summary>
	/// Returns a supplied numeric expression rounded to the nearest integer.
	/// </summary>
	/// <param name="x">The value to be rounded to the nearest integer.</param>
	/// <returns></returns>
	public extern static Number Round(Number x);

	/// <summary>
	/// Returns 1 or -1, indicating the sign of the number passed as argument. If the input is 0 or -0, it will be returned as-is.
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Sign(Number x);

	/// <summary>
	/// Returns the sine of a number.
	/// </summary>
	/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
	/// <returns></returns>
	public extern static Number Sin(Number x);

	/// <summary>
	/// Returns the hyperbolic sine of a number. 
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Sinh(Number x);

	/// <summary>
	/// Returns the square root of a number.
	/// </summary>
	/// <param name="x">A numeric expression.</param>
	/// <returns></returns>
	public extern static Number Sqrt(Number x);

	/// <summary>
	/// Returns the tangent of a number.
	/// </summary>
	/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
	/// <returns></returns>
	public extern static Number Tan(Number x);

	/// <summary>
	/// Returns the hyperbolic tangent of a number. 
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Tanh(Number x);

	/// <summary>
	/// Returns the integer part of a number by removing any fractional digits.
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	public extern static Number Trunc(Number x);
}

