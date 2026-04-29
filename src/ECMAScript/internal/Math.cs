namespace ECMAScript;

public static partial class Global
{
	/// <summary>
	/// Projection of the JavaScript Math object onto C# extension members.
	/// A trailing <c>_</c> is used only when C# naming rules or framework conflicts
	/// require a host-side escape hatch; the emitted JavaScript name still comes from
	/// the configured alias and remains aligned with the JavaScript runtime.
	/// </summary>
	extension(Math)
	{
		/// <summary>
		/// Euler's number as exposed by JavaScript <c>Math.E</c>.
		/// This remains on the <c>Math</c> host so the runtime shape stays aligned with JavaScript rather than drifting into CLR helper APIs.
		/// </summary>
		public extern static Number E { get; }

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
		[Description("@#abs")]
		public extern static Number AbsFn(Number x);

		/// <summary>
		/// Returns the arc cosine (or inverse cosine) of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#acos")]
		public extern static Number AcosFn(Number x);

		/// <summary>
		/// 返回一个数的反双曲余弦值
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#acosh")]
		public extern static Number AcoshFn(Number x);

		/// <summary>
		/// Returns the arcsine of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#asin")]
		public extern static Number AsinFn(Number x);

		/// <summary>
		/// 返回一个数值的反双曲正弦值
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#asinh")]
		public extern static Number AsinhFn(Number x);

		/// <summary>
		/// Returns the arctangent of a number.
		/// </summary>
		/// <param name="x">A numeric expression for which the arctangent is needed.</param>
		/// <returns></returns>
		[Description("@#atan")]
		public extern static Number AtanFn(Number x);

		/// <summary>
		/// Returns the angle (in radians) between the X axis and the line going through both the origin and the given point.
		/// </summary>
		/// <param name="y">A numeric expression representing the cartesian y-coordinate.</param>
		/// <param name="x">A numeric expression representing the cartesian x-coordinate.</param>
		/// <returns></returns>
		[Description("@#atan2")]
		public extern static Number Atan2Fn(Number y, Number x);

		/// <summary>
		/// 返回一个数值反双曲正切值
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#atanh")]
		public extern static Number AtanhFn(Number x);

		/// <summary>
		/// 返回任意数字的立方根
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#cbrt")]
		public extern static Number CbrtFn(Number x);

		/// <summary>
		/// Returns the smallest integer greater than or equal to its numeric argument.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#ceil")]
		public extern static Number CeilFn(Number x);

		/// <summary>
		/// Returns the number of leading zero bits in the 32-bit binary representation of a number.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#clz32")]
		public extern static Number Clz32Fn(Number x);

		/// <summary>
		/// Returns the cosine of a number.
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		[Description("@#cos")]
		public extern static Number CosFn(Number x);

		/// <summary>
		/// 返回数值的双曲余弦函数
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#cosh")]
		public extern static Number CoshFn(Number x);

		/// <summary>
		/// Returns e (the base of natural logarithms) raised to a power.
		/// </summary>
		/// <param name="x">A numeric expression representing the power of e.</param>
		/// <returns></returns>
		[Description("@#exp")]
		public extern static Number ExpFn(Number x);

		/// <summary>
		/// Calculates e^x - 1, where e is the base of natural logarithms.
		/// </summary>
		/// <remarks>This method provides an optimized calculation for small values of <paramref name="x"/> to improve
		/// precision. For larger values, it uses the standard exponential calculation.</remarks>
		/// <param name="x">The exponent value for which to calculate e^x - 1.</param>
		/// <returns>The result of e^x - 1. Returns <see cref="double.NaN"/> if <paramref name="x"/> is <see cref="double.NaN"/>.
		/// Returns <see cref="double.PositiveInfinity"/> if <paramref name="x"/> is positive infinity. Returns -1 if <paramref
		/// name="x"/> is negative infinity.</returns>
		[Description("@#expm1")]
		public extern static Number Expm1Fn(Number x);

		/// <summary>
		/// Returns the greatest integer less than or equal to its numeric argument.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#floor")]
		public extern static Number FloorFn(Number x);

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
		[Description("@#fround")]
		public extern static Number FroundFn(Number x);

		/// <summary>
		/// Returns the nearest IEEE 754 binary16 representation of a number, re-expanded as a JavaScript number.
		/// This is the direct projection of JavaScript <c>Math.f16round</c>.
		/// </summary>
		[Description("@#f16round")]
		public extern static Number F16roundFn(Number x);

		/// <summary>
		/// returns the square root of the sum of squares of its arguments.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		[Description("@#hypot")]
		public extern static Number HypotFn(params Number[] values);

		/// <summary>
		/// Sums an iterable of JavaScript numbers using the runtime's precise summation algorithm.
		/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for the JavaScript iterable input.
		/// This is the direct projection of JavaScript <c>Math.sumPrecise</c>.
		/// </summary>
		[Description("@#sumPrecise")]
		public extern static Number SumPreciseFn(IEnumerable<Number> items);

		/// <summary>
		/// Returns the result of the C-like 32-bit multiplication of the two parameters.
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns></returns>
		[Description("@#imul")]
		public extern static Number ImulFn(Number a, Number b);

		/// <summary>
		/// Returns the natural logarithm (base e) of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#log")]
		public extern static Number LogFn(Number x);

		/// <summary>
		/// Returns the base 10 logarithm of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#log10")]
		public extern static Number Log10Fn(Number x);

		/// <summary>
		/// Returns the natural logarithm (base e) of 1 + x, where x is the argument.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#log1p")]
		public extern static Number Log1pFn(Number x);

		/// <summary>
		/// Returns the base 2 logarithm of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#log2")]
		public extern static Number Log2Fn(Number x);

		/// <summary>
		/// Returns the larger of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="val1">Numeric expressions to be evaluated.</param>
		/// <param name="val2">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#max")]
		public extern static Number MaxFn(Number val1, Number val2);

		/// <summary>
		/// Returns the larger of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="values">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#max")]
		public extern static Number MaxFn(params Number[] values);

		/// <summary>
		/// Returns the smaller of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="val1">Numeric expressions to be evaluated.</param>
		/// <param name="val2">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#min")]
		public extern static Number MinFn(Number val1, Number val2);

		/// <summary>
		/// Returns the smaller of a set of supplied numeric expressions.
		/// </summary>
		/// <param name="values">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#min")]
		public extern static Number MinFn(params Number[] values);

		/// <summary>
		/// Returns the value of a base expression taken to a specified power.
		/// </summary>
		/// <param name="x">The base value of the expression.</param>
		/// <param name="y">The exponent value of the expression.</param>
		/// <returns></returns>
		[Description("@#pow")]
		public extern static Number PowFn(Number x, Number y);

		/// <summary>
		/// Returns a pseudorandom number between 0 and 1.
		/// </summary>
		/// <returns></returns>
		[Description("@#random")]
		public extern static Number RandomFn();
		
		/// <summary>
		/// Returns a supplied numeric expression rounded to the nearest integer.
		/// </summary>
		/// <param name="x">The value to be rounded to the nearest integer.</param>
		/// <returns></returns>
		[Description("@#round")]
		public extern static Number RoundFn(Number x);

		/// <summary>
		/// Returns 1 or -1, indicating the sign of the number passed as argument. If the input is 0 or -0, it will be returned as-is.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#sign")]
		public extern static Number SignFn(Number x);

		/// <summary>
		/// Returns the sine of a number.
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		[Description("@#sin")]
		public extern static Number SinFn(Number x);

		/// <summary>
		/// Returns the hyperbolic sine of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#sinh")]
		public extern static Number SinhFn(Number x);

		/// <summary>
		/// Returns the square root of a number.
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#sqrt")]
		public extern static Number SqrtFn(Number x);

		/// <summary>
		/// Returns the tangent of a number.
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		[Description("@#tan")]
		public extern static Number TanFn(Number x);

		/// <summary>
		/// Returns the hyperbolic tangent of a number. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#tanh")]
		public extern static Number TanhFn(Number x);

		/// <summary>
		/// Returns the integer part of a number by removing any fractional digits.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#trunc")]
		public extern static Number TruncFn(Number x);
	}
}
