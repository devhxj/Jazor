namespace ECMAScript;

public static partial class Global
{
	extension(Math)
	{
		///// <summary>
		///// The mathematical constant e. This is Euler's number, the base of natural logarithms.
		///// </summary>
		//public extern static Number E { get; }

		/// <summary>
		/// The natural logarithm of 10.
		/// </summary>
		public extern static Number LN10 { get; }

		/// <summary>
		/// The natural logarithm of 2.
		/// </summary>
		public extern static Number LN2 { get; }

		/// <summary>
		/// The base-2 logarithm of e.
		/// </summary>
		public extern static Number LOG2E { get; }

		/// <summary>
		/// The base-10 logarithm of e.
		/// </summary>
		public extern static Number LOG10E { get; }

		/// <summary>
		/// Pi. This is the ratio of the circumference of a circle to its diameter.
		/// </summary>
		public extern static Number PI { get; }

		/// <summary>
		/// The square root of 0.5, or, equivalently, one divided by the square root of 2.
		/// </summary>
		public extern static Number SQRT1_2 { get; }

		/// <summary>
		/// The square root of 2.
		/// </summary>
		public extern static Number SQRT2 { get; }

		/// <summary>
		/// Returns the absolute value of a number (the value without regard to whether it is positive or negative).
		/// For example, the absolute value of -5 is the same as the absolute value of 5.
		/// </summary>
		/// <param name="x">A numeric expression for which the absolute value is needed.</param>
		/// <returns></returns>
		public extern static Number abs(Number x);

		/// <summary>
		/// Returns the arc cosine (or inverse cosine) of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number acos(Number x);

		/// <summary>
		/// 返回一个数的反双曲余弦值
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number acosh(Number x);

		/// <summary>
		/// Returns the arcsine of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number asin(Number x);

		/// <summary>
		/// 返回一个数值的反双曲正弦值
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number asinh(Number x);

		/// <summary>
		/// Returns the arctangent of a number.
		/// </summary>
		/// <param name="x">A numeric expression for which the arctangent is needed.</param>
		/// <returns></returns>
		public extern static Number atan(Number x);

		/// <summary>
		/// Returns the angle (in radians) between the X axis and the line going through both the origin and the given point.
		/// </summary>
		/// <param name="y">A numeric expression representing the cartesian y-coordinate.</param>
		/// <param name="x">A numeric expression representing the cartesian x-coordinate.</param>
		/// <returns></returns>
		public extern static Number atan2(Number y, Number x);

		/// <summary>
		/// 返回一个数值反双曲正切值
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number atanh(Number x);

		/// <summary>
		/// 返回任意数字的立方根
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number cbrt(Number x);

		/// <summary>
		/// Returns the smallest integer greater than or equal to its numeric argument.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number ceil(Number x);

		/// <summary>
		/// Returns the number of leading zero bits in the 32-bit binary representation of a number.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number clz32(Number x);

		/// <summary>
		/// Returns the cosine of a number.
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		public extern static Number cos(Number x);

		/// <summary>
		/// 返回数值的双曲余弦函数
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number cosh(Number x);

		/// <summary>
		/// Returns e (the base of natural logarithms) raised to a power.
		/// </summary>
		/// <param name="x">A numeric expression representing the power of e.</param>
		/// <returns></returns>
		public extern static Number exp(Number x);

		/// <summary>
		/// Calculates e^x - 1, where e is the base of natural logarithms.
		/// </summary>
		/// <remarks>This method provides an optimized calculation for small values of <paramref name="x"/> to improve
		/// precision. For larger values, it uses the standard exponential calculation.</remarks>
		/// <param name="x">The exponent value for which to calculate e^x - 1.</param>
		/// <returns>The result of e^x - 1. Returns <see cref="double.NaN"/> if <paramref name="x"/> is <see cref="double.NaN"/>.
		/// Returns <see cref="double.PositiveInfinity"/> if <paramref name="x"/> is positive infinity. Returns -1 if <paramref
		/// name="x"/> is negative infinity.</returns>
		public extern static Number expm1(Number x);

		/// <summary>
		/// Returns the greatest integer less than or equal to its numeric argument.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number floor(Number x);

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
		public extern static Number fround(Number x);

		/// <summary>
		/// returns the square root of the sum of squares of its arguments.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public extern static Number hypot(params Number[] values);

		/// <summary>
		/// Returns the result of the C-like 32-bit multiplication of the two parameters.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns></returns>
		public extern static Number imul(Number a, Number b);

		/// <summary>
		/// Returns the natural logarithm (base e) of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number log(Number x);

		/// <summary>
		/// Returns the base 10 logarithm of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number log10(Number x);

		/// <summary>
		/// Returns the natural logarithm (base e) of 1 + x, where x is the argument.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number log1p(Number x);

		/// <summary>
		/// Returns the base 2 logarithm of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number log2(Number x);

		/// <summary>
		/// Returns the larger of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="val1">Numeric expressions to be evaluated.</param>
		/// <param name="val2">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		public extern static Number max(Number val1, Number val2);

		/// <summary>
		/// Returns the larger of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="values">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		public extern static Number max(params Number[] values);

		/// <summary>
		/// Returns the smaller of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="values">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		public extern static Number min(params Number[] values);

		/// <summary>
		/// Returns the value of a base expression taken to a specified power.
		/// </summary>
		/// <param name="x">The base value of the expression.</param>
		/// <param name="y">The exponent value of the expression.</param>
		/// <returns></returns>
		public extern static Number pow(Number x, Number y);

		/// <summary>
		/// Returns a pseudorandom number between 0 and 1.
		/// </summary>
		/// <returns></returns>
		public extern static Number random();

		/// <summary>
		/// Returns a supplied numeric expression rounded to the nearest integer.
		/// </summary>
		/// <param name="x">The value to be rounded to the nearest integer.</param>
		/// <returns></returns>
		public extern static Number round(Number x);

		/// <summary>
		/// Returns 1 or -1, indicating the sign of the number passed as argument. If the input is 0 or -0, it will be returned as-is.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number sign(Number x);

		/// <summary>
		/// Returns the sine of a number.
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		public extern static Number sin(Number x);

		/// <summary>
		/// Returns the hyperbolic sine of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number sinh(Number x);

		/// <summary>
		/// Returns the square root of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		public extern static Number sqrt(Number x);

		/// <summary>
		/// Returns the tangent of a number.
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		public extern static Number tan(Number x);

		/// <summary>
		/// Returns the hyperbolic tangent of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number tanh(Number x);

		/// <summary>
		/// Returns the integer part of a number by removing any fractional digits.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public extern static Number trunc(Number x);
	}
}
