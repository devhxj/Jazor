namespace ECMAScript;

/// <remarks>
/// Math 保持 JavaScript 的静态 host shape；成员名通过 Description/ECMAScript 映射到 Math。
/// 这里不模拟 System.Math 的全部溢出、整数或 decimal 语义，调用方应按 Number 精度边界使用。
/// Math retains the JavaScript static host shape; members map to <c>Math</c> through Description/ECMAScript metadata.
/// It does not emulate all <c>System.Math</c> overflow, integer, or decimal behavior; callers should use JavaScript Number precision boundaries.
/// </remarks>
public static partial class Global
{
	/// <summary>
	/// Projection of the JavaScript Math object onto C# extension members.
	/// A trailing <c>_</c> is used only when C# naming rules or framework conflicts
	/// require a host-side escape hatch; the emitted JavaScript name still comes from
	/// the configured alias and remains aligned with the JavaScript runtime.
	/// 将 JavaScript Math 对象投影到 C# 扩展成员。仅在 C# 命名规则或框架冲突时添加 <c>_</c> 或 <c>Fn</c> 后缀，
	/// 生成的 JavaScript 名称仍通过别名保持与 <c>Math.*</c> 一致。
	/// </summary>
	extension(Math)
	{
		/// <summary>
		/// Euler's number as exposed by JavaScript <c>Math.E</c>.
	/// This remains on the <c>Math</c> host so the runtime shape stays aligned with JavaScript rather than drifting into CLR helper APIs.
	/// JavaScript <c>Math.E</c> 的欧拉数常量；保留在 Math 宿主上以保持运行时形状，不转为 CLR 帮助 API。
		/// </summary>
		[Description("@#E")]
		public extern static Number E { get; }

		/// <summary>
	/// The natural logarithm of 10.
	/// 10 的自然对数，对应 JavaScript <c>Math.LN10</c>。
		/// </summary>
		[Description("@#LN10")]
		public extern static Number LN10 { get; }

		/// <summary>
	/// The natural logarithm of 2.
	/// 2 的自然对数，对应 JavaScript <c>Math.LN2</c>。
		/// </summary>
		[Description("@#LN2")]
		public extern static Number LN2 { get; }

		/// <summary>
	/// The base-2 logarithm of e.
	/// e 的以 2 为底对数，对应 JavaScript <c>Math.LOG2E</c>。
		/// </summary>
		[Description("@#LOG2E")]
		public extern static Number LOG2E { get; }

		/// <summary>
	/// The base-10 logarithm of e.
	/// e 的以 10 为底对数，对应 JavaScript <c>Math.LOG10E</c>。
		/// </summary>
		[Description("@#LOG10E")]
		public extern static Number LOG10E { get; }

		/// <summary>
	/// Pi. This is the ratio of the circumference of a circle to its diameter.
	/// 圆周率，对应 JavaScript <c>Math.PI</c>。
		/// </summary>
		[Description("@#PI")]
		public extern static Number PI { get; }

		/// <summary>
	/// The square root of 0.5, or, equivalently, one divided by the square root of 2.
	/// 0.5 的平方根（即 1/sqrt(2)），对应 JavaScript <c>Math.SQRT1_2</c>。
		/// </summary>
		[Description("@#SQRT1_2")]
		public extern static Number SQRT1_2 { get; }

		/// <summary>
	/// The square root of 2.
	/// 2 的平方根，对应 JavaScript <c>Math.SQRT2</c>。
		/// </summary>
		[Description("@#SQRT2")]
		public extern static Number SQRT2 { get; }

		/// <summary>
		/// Returns the absolute value of a number (the value without regard to whether it is positive or negative).
	/// For example, the absolute value of -5 is the same as the absolute value of 5.
	/// 返回 Number 的绝对值；<c>AbsFn</c> 后缀仅为 C# 命名适配，实际映射 JavaScript <c>Math.abs</c>。
		/// </summary>
		/// <param name="x">A numeric expression for which the absolute value is needed.</param>
		/// <returns></returns>
		[Description("@#abs")]
		public extern static Number AbsFn(Number x);

		/// <summary>
	/// Returns the arc cosine (or inverse cosine) of a number.
	/// 返回反余弦值（弧度）；超出 [-1, 1] 的输入按 JavaScript 返回 <c>NaN</c>。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#acos")]
		public extern static Number AcosFn(Number x);

		/// <summary>
	/// Returns the inverse hyperbolic cosine of a number.
	/// 返回一个数的反双曲余弦值；小于 1 的输入按 JavaScript 返回 <c>NaN</c>。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#acosh")]
		public extern static Number AcoshFn(Number x);

		/// <summary>
	/// Returns the arcsine of a number.
	/// 返回反正弦值（弧度）；超出 [-1, 1] 的输入按 JavaScript 返回 <c>NaN</c>。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#asin")]
		public extern static Number AsinFn(Number x);

		/// <summary>
	/// Returns the inverse hyperbolic sine of a number.
	/// 返回一个数值的反双曲正弦值。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#asinh")]
		public extern static Number AsinhFn(Number x);

		/// <summary>
	/// Returns the arctangent of a number.
	/// 返回反正切值（弧度）。
		/// </summary>
		/// <param name="x">A numeric expression for which the arctangent is needed.</param>
		/// <returns></returns>
		[Description("@#atan")]
		public extern static Number AtanFn(Number x);

		/// <summary>
	/// Returns the angle (in radians) between the X axis and the line going through both the origin and the given point.
	/// 返回从 X 轴到 (x, y) 点的角度（弧度）；参数顺序保持 JavaScript <c>Math.atan2(y, x)</c>。
		/// </summary>
		/// <param name="y">A numeric expression representing the cartesian y-coordinate.</param>
		/// <param name="x">A numeric expression representing the cartesian x-coordinate.</param>
		/// <returns></returns>
		[Description("@#atan2")]
		public extern static Number Atan2Fn(Number y, Number x);

		/// <summary>
	/// Returns the inverse hyperbolic tangent of a number.
	/// 返回一个数值的反双曲正切值；绝对值大于 1 的输入按 JavaScript 返回 <c>NaN</c>。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#atanh")]
		public extern static Number AtanhFn(Number x);

		/// <summary>
	/// Returns the cube root of a number.
	/// 返回任意数字的立方根，保留负数的符号。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#cbrt")]
		public extern static Number CbrtFn(Number x);

		/// <summary>
	/// Returns the smallest integer greater than or equal to its numeric argument.
	/// 返回大于等于输入的最小整数；结果仍是 JavaScript Number。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#ceil")]
		public extern static Number CeilFn(Number x);

		/// <summary>
	/// Returns the number of leading zero bits in the 32-bit binary representation of a number.
	/// 将输入按 JavaScript ToUint32 转换后，返回其 32 位表示中的前导零位数。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#clz32")]
		public extern static Number Clz32Fn(Number x);

		/// <summary>
	/// Returns the cosine of a number.
	/// 返回弧度输入的余弦值。
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		[Description("@#cos")]
		public extern static Number CosFn(Number x);

		/// <summary>
	/// Returns the hyperbolic cosine of a number.
	/// 返回数值的双曲余弦函数。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#cosh")]
		public extern static Number CoshFn(Number x);

		/// <summary>
	/// Returns e (the base of natural logarithms) raised to a power.
	/// 返回 e 的 x 次幂，对应 JavaScript <c>Math.exp</c>。
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
	/// JavaScript <c>Math.expm1</c> 投影；用于精确计算 e^x - 1，尤其改善接近零输入的精度。</remarks>
		/// <param name="x">The exponent value for which to calculate e^x - 1.</param>
		/// <returns>The result of e^x - 1. Returns <see cref="double.NaN"/> if <paramref name="x"/> is <see cref="double.NaN"/>.
		/// Returns <see cref="double.PositiveInfinity"/> if <paramref name="x"/> is positive infinity. Returns -1 if <paramref
		/// name="x"/> is negative infinity.</returns>
		[Description("@#expm1")]
		public extern static Number Expm1Fn(Number x);

		/// <summary>
	/// Returns the greatest integer less than or equal to its numeric argument.
	/// 返回小于等于输入的最大整数；结果仍是 JavaScript Number。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#floor")]
		public extern static Number FloorFn(Number x);

		/// <summary>
	/// Returns the nearest IEEE-754 binary32 representation of a number, re-expanded as a JavaScript number.
	/// 返回最接近的 IEEE-754 binary32 表示并重新扩展为 JavaScript Number。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#fround")]
		public extern static Number FroundFn(Number x);

		/// <summary>
		/// Returns the nearest IEEE 754 binary16 representation of a number, re-expanded as a JavaScript number.
	/// This is the direct projection of JavaScript <c>Math.f16round</c>.
	/// 返回最接近的 IEEE-754 binary16 表示后再扩展为 JavaScript Number，直接映射 <c>Math.f16round</c>。
		/// </summary>
		[Description("@#f16round")]
		public extern static Number F16roundFn(Number x);

		/// <summary>
	/// Returns the square root of the sum of squares of its arguments.
	/// 返回所有参数平方和的平方根，使用 JavaScript <c>Math.hypot</c> 的数值稳定实现。
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		[Description("@#hypot")]
		public extern static Number HypotFn(params Number[] values);

		/// <summary>
		/// Sums an iterable of JavaScript numbers using the runtime's precise summation algorithm.
		/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for the JavaScript iterable input.
	/// This is the direct projection of JavaScript <c>Math.sumPrecise</c>.
	/// 使用运行时的精确求和算法累加 JavaScript Number iterable；<see cref="IEnumerable{T}"/> 是输入 iterable 的 C# 宿主表面。
		/// </summary>
		[Description("@#sumPrecise")]
		public extern static Number SumPreciseFn(IEnumerable<Number> items);

		/// <summary>
	/// Returns the result of the C-like 32-bit multiplication of the two parameters.
	/// 按 JavaScript ToInt32 语义执行类似 C 的 32 位乘法并返回 Number。
		/// </summary>
		/// <param name="a">First number.</param>
		/// <param name="b">Second number.</param>
		/// <returns></returns>
		[Description("@#imul")]
		public extern static Number ImulFn(Number a, Number b);

		/// <summary>
	/// Returns the natural logarithm (base e) of a number.
	/// 返回以 e 为底的对数；负数输入按 JavaScript 返回 <c>NaN</c>。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#log")]
		public extern static Number LogFn(Number x);

		/// <summary>
		/// Returns the base 10 logarithm of a number.
	/// 返回以 10 为底的对数；非正输入遵循 JavaScript Number 边界行为。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#log10")]
		public extern static Number Log10Fn(Number x);

		/// <summary>
	/// Returns the natural logarithm (base e) of 1 + x, where x is the argument.
	/// 返回 ln(1+x)，对接近零的 x 比直接计算 ln(1+x) 更精确。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#log1p")]
		public extern static Number Log1pFn(Number x);

		/// <summary>
		/// Returns the base 2 logarithm of a number.
	/// 返回以 2 为底的对数；非正输入遵循 JavaScript Number 边界行为。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#log2")]
		public extern static Number Log2Fn(Number x);

		/// <summary>
	/// Returns the larger of a set of supplied numeric expressions.
	/// 返回两个 Number 中较大者；<c>NaN</c> 传播和有符号零处理遵循 JavaScript <c>Math.max</c>。
		/// </summary>
		/// <param name="val1">Numeric expressions to be evaluated.</param>
		/// <param name="val2">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#max")]
		public extern static Number MaxFn(Number val1, Number val2);

		/// <summary>
	/// Returns the larger of a set of supplied numeric expressions.
	/// 返回参数中的最大值；空参数、<c>NaN</c> 和有符号零均遵循 JavaScript <c>Math.max</c>。
		/// </summary>
		/// <param name="values">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#max")]
		public extern static Number MaxFn(params Number[] values);

		/// <summary>
	/// Returns the smaller of a set of supplied numeric expressions.
	/// 返回两个 Number 中较小者；<c>NaN</c> 传播和有符号零处理遵循 JavaScript <c>Math.min</c>。
		/// </summary>
		/// <param name="val1">Numeric expressions to be evaluated.</param>
		/// <param name="val2">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#min")]
		public extern static Number MinFn(Number val1, Number val2);

		/// <summary>
	/// Returns the smaller of a set of supplied numeric expressions.
	/// 返回参数中的最小值；空参数、<c>NaN</c> 和有符号零均遵循 JavaScript <c>Math.min</c>。
		/// </summary>
		/// <param name="values">Numeric expressions to be evaluated.</param>
		/// <returns></returns>
		[Description("@#min")]
		public extern static Number MinFn(params Number[] values);

		/// <summary>
	/// Returns the value of a base expression taken to a specified power.
	/// 返回 x 的 y 次幂；边界情况（负底数、非整数指数、无穷）遵循 JavaScript <c>Math.pow</c>。
		/// </summary>
		/// <param name="x">The base value of the expression.</param>
		/// <param name="y">The exponent value of the expression.</param>
		/// <returns></returns>
		[Description("@#pow")]
		public extern static Number PowFn(Number x, Number y);

		/// <summary>
	/// Returns a pseudorandom number between 0 and 1.
	/// 返回区间 [0, 1) 的伪随机 Number；它不是密码学安全随机源。
		/// </summary>
		/// <returns></returns>
		[Description("@#random")]
		public extern static Number RandomFn();
		
		/// <summary>
	/// Returns a supplied numeric expression rounded to the nearest integer.
	/// 按 JavaScript <c>Math.round</c> 规则舍入到最近整数；与某些 CLR 舍入规则不同，.5 朝正无穷方向处理并保留 -0 情形。
		/// </summary>
		/// <param name="x">The value to be rounded to the nearest integer.</param>
		/// <returns></returns>
		[Description("@#round")]
		public extern static Number RoundFn(Number x);

		/// <summary>
	/// Returns 1 or -1, indicating the sign of the number passed as argument. If the input is 0 or -0, it will be returned as-is.
	/// 返回 Number 的符号；输入为 +0 或 -0 时原样返回，<c>NaN</c> 时返回 <c>NaN</c>。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#sign")]
		public extern static Number SignFn(Number x);

		/// <summary>
	/// Returns the sine of a number.
	/// 返回弧度输入的正弦值。
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		[Description("@#sin")]
		public extern static Number SinFn(Number x);

		/// <summary>
	/// Returns the hyperbolic sine of a number.
	/// 返回数值的双曲正弦函数。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#sinh")]
		public extern static Number SinhFn(Number x);

		/// <summary>
	/// Returns the square root of a number.
	/// 返回平方根；负数输入按 JavaScript 返回 <c>NaN</c>，并保留 -0。
		/// </summary>
		/// <param name="x">A numeric expression.</param>
		/// <returns></returns>
		[Description("@#sqrt")]
		public extern static Number SqrtFn(Number x);

		/// <summary>
	/// Returns the tangent of a number.
	/// 返回弧度输入的正切值。
		/// </summary>
		/// <param name="x">A numeric expression that contains an angle measured in radians.</param>
		/// <returns></returns>
		[Description("@#tan")]
		public extern static Number TanFn(Number x);

		/// <summary>
	/// Returns the hyperbolic tangent of a number.
	/// 返回数值的双曲正切函数。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#tanh")]
		public extern static Number TanhFn(Number x);

		/// <summary>
	/// Returns the integer part of a number by removing any fractional digits.
	/// 移除小数部分并向零截断；输入为 -0 时保留 -0，结果仍是 JavaScript Number。
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		[Description("@#trunc")]
		public extern static Number TruncFn(Number x);
	}
}
