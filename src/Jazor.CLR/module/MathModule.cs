namespace Jazor.CLR;

/// <summary>
/// System.Math 类型模块映射规则
///
/// C# Math 与 JavaScript Math 的对应关系：
/// - 大多数静态方法可以直接映射到 JS Math 对象
///
/// Op 类型选择原则：
/// - Inline: 常量 (E, PI, Tau)
/// - Alias: JS Math 有同名方法
/// - Import: 需要额外逻辑的方法
/// - Discard: 不支持的方法（如某些重载）
/// </summary>
[ECMAScriptModule("System/MathModule.js")]
[Jazor(Op.Alias, "System.Math", "Math")]
public static class MathModule
{
	private static Number CompareCore(Number left, Number right)
	{
		if (IsNaN(left))
			return IsNaN(right) ? 0 : -1;
		if (IsNaN(right))
			return 1;
		if (left < right)
			return -1;
		if (left > right)
			return 1;

		return 0;
	}

	private static Number MaxMagnitudeCore(Number x, Number y)
	{
		if (IsNaN(x) || IsNaN(y))
			return Number.NaN;

		var absX = Math.AbsFn(x);
		var absY = Math.AbsFn(y);
		if (absX > absY)
			return x;
		if (absX < absY)
			return y;

		// .NET 在同绝对值 tie-break 时会保留数值更大的那个，这里顺带修正 +0 / -0。
		return Math.MaxFn(x, y);
	}

	private static Number MinMagnitudeCore(Number x, Number y)
	{
		if (IsNaN(x) || IsNaN(y))
			return Number.NaN;

		var absX = Math.AbsFn(x);
		var absY = Math.AbsFn(y);
		if (absX < absY)
			return x;
		if (absX > absY)
			return y;

		// .NET 在同绝对值 tie-break 时会保留数值更小的那个，这里顺带修正 +0 / -0。
		return Math.MinFn(x, y);
	}

	private static (sbyte Quotient, sbyte Remainder) DivRemSByteCore(Number left, Number right)
	{
		if (right == 0)
			throw new Error("DivideByZeroException");
		if (left == -128 && right == -1)
			throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");

		var quotient = Math.TruncFn(left / right);
		return ((sbyte)quotient, (sbyte)(left - quotient * right));
	}

	private static Array<object?> BigMulUnsigned64(BigInt left, BigInt right)
	{
		var product = left * right;
		var low = BigInt.AsUintN(64, product);
		var high = BigInt.AsUintN(64, product >> BigIntFn(64));
		return [high, low];
	}

	private static Array<object?> BigMulSigned64(BigInt left, BigInt right)
	{
		var product = left * right;
		var low = BigInt.AsIntN(64, product);
		var high = BigInt.AsIntN(64, product >> BigIntFn(64));
		return [high, low];
	}

	// 常量 - 使用 Op.Inline
	/// <summary>
	/// C#: Math.E
	/// JS: Math.E
	/// </summary>
	[Jazor(Op.Inline, "static System.Math.E", "Math.E")]
	public extern static Number _e();

	/// <summary>
	/// C#: Math.PI
	/// JS: Math.PI
	/// </summary>
	[Jazor(Op.Inline, "static System.Math.PI", "Math.PI")]
	public extern static Number _pi();

	/// <summary>
	/// C#: Math.Tau
	/// JS: Math.PI * 2
	/// </summary>
	[Jazor(Op.Inline, "static System.Math.Tau", "(Math.PI * 2)")]
	public extern static Number _tau();

	///<summary>Returns the angle whose cosine is the specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Acos(double)", "acos")]
	public extern static Number _473e58e8c04acfd3(Number d);

	///<summary>Returns the angle whose hyperbolic cosine is the specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Acosh(double)", "acosh")]
	public extern static Number _46ecb0a75e5ba94e(Number d);

	///<summary>Returns the angle whose sine is the specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Asin(double)", "asin")]
	public extern static Number _31a8579686d23c98(Number d);

	///<summary>Returns the angle whose hyperbolic sine is the specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Asinh(double)", "asinh")]
	public extern static Number _fac652d6d6a2503b(Number d);

	///<summary>Returns the angle whose tangent is the specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Atan(double)", "atan")]
	public extern static Number _64bb4dcf5871842b(Number d);

	///<summary>Returns the angle whose hyperbolic tangent is the specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Atanh(double)", "atanh")]
	public extern static Number _8093e8210867a45e(Number d);

	///<summary>Returns the angle whose tangent is the quotient of two specified numbers.</summary>
	[Jazor(Op.Alias, "static System.Math.Atan2(double, double)", "atan2")]
	public extern static Number _cc6b2bb857d27648(Number y, Number x);

	///<summary>Returns the cube root of a specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Cbrt(double)", "cbrt")]
	public extern static Number _9369c8e8f81372b6(Number d);

	///<summary>Returns the smallest integral value that is greater than or equal to the specified double-precision floating-point number.</summary>
	[Jazor(Op.Alias, "static System.Math.Ceiling(double)", "ceil")]
	public extern static Number _d7be7c95bfefd788(Number a);

	///<summary>Returns the cosine of the specified angle.</summary>
	[Jazor(Op.Alias, "static System.Math.Cos(double)", "cos")]
	public extern static Number _b6b312cfcefe789c(Number d);

	///<summary>Returns the hyperbolic cosine of the specified angle.</summary>
	[Jazor(Op.Alias, "static System.Math.Cosh(double)", "cosh")]
	public extern static Number _c6f1b8664a086e13(Number value);

	///<summary>Returns <see langword="e" /> raised to the specified power.</summary>
	[Jazor(Op.Alias, "static System.Math.Exp(double)", "exp")]
	public extern static Number _d5b39999cc90e482(Number d);

	///<summary>Returns the largest integral value less than or equal to the specified double-precision floating-point number.</summary>
	[Jazor(Op.Alias, "static System.Math.Floor(double)", "floor")]
	public extern static Number _a43200909dff4bc0(Number d);

	///<summary>Returns (x * y) + z, rounded as one ternary operation.</summary>
	[Jazor(Op.Inline, "static System.Math.FusedMultiplyAdd(double, double, double)", "(__arg1 * __arg2 + __arg3)")]
	public extern static Number _52c95df2ad20c3bd(Number x, Number y, Number z);

	///<summary>Returns the natural (base <see langword="e" />) logarithm of a specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Log(double)", "log")]
	public extern static Number _c65770c0fcbed4b6(Number d);

	///<summary>Returns the base 2 logarithm of a specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Log2(double)", "log2")]
	public extern static Number _e622dc98a98720f4(Number x);

	///<summary>Returns the base 10 logarithm of a specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Log10(double)", "log10")]
	public extern static Number _a882de08086ccec9(Number d);

	///<summary>Returns a specified number raised to the specified power.</summary>
	[Jazor(Op.Alias, "static System.Math.Pow(double, double)", "pow")]
	public extern static Number _fd439387b010bb99(Number x, Number y);

	///<summary>Returns the sine of the specified angle.</summary>
	[Jazor(Op.Alias, "static System.Math.Sin(double)", "sin")]
	public extern static Number _f1029100ea8114ab(Number a);

	///<summary>Returns the sine and cosine of the specified angle.</summary>
	[Jazor(Op.Import, "static System.Math.SinCos(double)")]
	public static (double Sin, double Cos) _4dcadff583296186(Number x)
		=> (Sin: Math.SinFn(x), Cos: Math.CosFn(x));

	///<summary>Returns the hyperbolic sine of the specified angle.</summary>
	[Jazor(Op.Alias, "static System.Math.Sinh(double)", "sinh")]
	public extern static Number _f48ae51bac192bdf(Number value);

	///<summary>Returns the square root of a specified number.</summary>
	[Jazor(Op.Alias, "static System.Math.Sqrt(double)", "sqrt")]
	public extern static Number _b303f709d2b283f0(Number d);

	///<summary>Returns the tangent of the specified angle.</summary>
	[Jazor(Op.Alias, "static System.Math.Tan(double)", "tan")]
	public extern static Number _5f9763f3b0176663(Number a);

	///<summary>Returns the hyperbolic tangent of the specified angle.</summary>
	[Jazor(Op.Alias, "static System.Math.Tanh(double)", "tanh")]
	public extern static Number _d198ea5fec4f6c8a(Number value);

	///<summary>Returns the absolute value of a 16-bit signed integer.</summary>
	[Jazor(Op.Alias, "static System.Math.Abs(short)", "abs")]
	public extern static Number _81a80e1bfb516bfb(Number value);

	///<summary>Returns the absolute value of a 32-bit signed integer.</summary>
	[Jazor(Op.Alias, "static System.Math.Abs(int)", "abs")]
	public extern static Number _0aaf1073fc70e405(Number value);

	///<summary>Returns the absolute value of a 64-bit signed integer.</summary>
	[Jazor(Op.Inline, "static System.Math.Abs(long)", "((__arg1 < 0n) ? -__arg1 : __arg1)")]
	public extern static BigInt _2f5b0b713dde9501(BigInt value);

	///<summary>Returns the absolute value of a native signed integer.</summary>
	[Jazor(Op.Discard ,"static System.Math.Abs(nint)")]
	public extern static nint _6de080191221a07d(object value);

	///<summary>Returns the absolute value of an 8-bit signed integer.</summary>
	[Jazor(Op.Alias, "static System.Math.Abs(sbyte)", "abs")]
	public extern static Number _6ed2ee0733ac7051(Number value);

	///<summary>Returns the absolute value of a <see cref="T:System.Decimal" /> number.</summary>
	[Jazor(Op.Import ,"static System.Math.Abs(decimal)")]
	public static string _eab3564b2663dff6(string value)
		=> DecimalModule._e85678b4de2283e8(value);

	///<summary>Returns the absolute value of a double-precision floating-point number.</summary>
	[Jazor(Op.Alias, "static System.Math.Abs(double)", "abs")]
	public extern static Number _6a0f94e87051cd5f(Number value);

	///<summary>Returns the absolute value of a single-precision floating-point number.</summary>
	[Jazor(Op.Alias, "static System.Math.Abs(float)", "abs")]
	public extern static Number _3e86488d0112bcd3(Number value);

	///<summary>Produces the full product of two unsigned 32-bit numbers.</summary>
	[Jazor(Op.Inline, "static System.Math.BigMul(uint, uint)", "(BigInt(__arg1) * BigInt(__arg2))")]
	public extern static BigInt _6683ad6f7ac7c14c(Number a, Number b);

	///<summary>Produces the full product of two 32-bit numbers.</summary>
	[Jazor(Op.Inline, "static System.Math.BigMul(int, int)", "(BigInt(__arg1) * BigInt(__arg2))")]
	public extern static BigInt _f8dfabc9cf61c7c8(Number a, Number b);

	///<summary>Produces the full product of two unsigned 64-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.BigMul(ulong, ulong, out ulong)")]
	public static Array<object?> _99697fddb05f0646(BigInt a, BigInt b, BigInt low)
		=> BigMulUnsigned64(a, b);

	///<summary>Produces the full product of two 64-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.BigMul(long, long, out long)")]
	public static Array<object?> _1f2b3fb549b0a774(BigInt a, BigInt b, BigInt low)
		=> BigMulSigned64(a, b);

	///<summary>Produces the full product of two unsigned 64-bit numbers.</summary>
	[Jazor(Op.Inline, "static System.Math.BigMul(ulong, ulong)", "(__arg1 * __arg2)")]
	public extern static BigInt _d2fa7191b8139e97(BigInt a, BigInt b);

	///<summary>Produces the full product of two 64-bit numbers.</summary>
	[Jazor(Op.Inline, "static System.Math.BigMul(long, long)", "(__arg1 * __arg2)")]
	public extern static BigInt _9eceeda3d33f938a(BigInt a, BigInt b);

	///<summary>Returns the largest value that compares less than a specified value.</summary>
	[Jazor(Op.Import, "static System.Math.BitDecrement(double)")]
	public static Number _bc28ec82e8385202(Number x)
		=> DoubleModule.BitDecrementCore(x);

	///<summary>Returns the smallest value that compares greater than a specified value.</summary>
	[Jazor(Op.Import, "static System.Math.BitIncrement(double)")]
	public static Number _655bd4d428ca20ea(Number x)
		=> DoubleModule.BitIncrementCore(x);

	///<summary>Returns a value with the magnitude of <paramref name="x" /> and the sign of <paramref name="y" />.</summary>
	[Jazor(Op.Inline, "static System.Math.CopySign(double, double)", "((__arg2 < 0 || Object.is(__arg2, -0)) ? -Math.abs(__arg1) : Math.abs(__arg1))")]
	public extern static Number _f51bc6e5d8ce272b(Number x, Number y);

	///<summary>Calculates the quotient of two 32-bit signed integers and also returns the remainder in an output parameter.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(int, int, out int)")]
	public static Array<object?> _2a90cb0f64781864(Number a, Number b, Number result)
	{
		var pair = Int32Module._d4cc9914e60e5643(a, b);
		return [pair.Quotient, pair.Remainder];
	}

	///<summary>Calculates the quotient of two 64-bit signed integers and also returns the remainder in an output parameter.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(long, long, out long)")]
	public static Array<object?> _1961d3558bd76ea4(BigInt a, BigInt b, BigInt result)
	{
		var pair = Int64Module._28273cd350760efe(a, b);
		return [pair.Quotient, pair.Remainder];
	}

	///<summary>Produces the quotient and the remainder of two signed 8-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(sbyte, sbyte)")]
	public static (sbyte Quotient, sbyte Remainder) _e0661118fd9ce98d(Number left, Number right)
		=> DivRemSByteCore(left, right);

	///<summary>Produces the quotient and the remainder of two unsigned 8-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(byte, byte)")]
	public static (Number Quotient, Number Remainder) _09ec2eababe53085(Number left, Number right)
		=> ByteModule._42cbe2ef401fb8c9(left, right);

	///<summary>Produces the quotient and the remainder of two signed 16-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(short, short)")]
	public static (short Quotient, short Remainder) _f6eb115003bc623f(Number left, Number right)
		=> Int16Module._b2c1f15fae072110(left, right);

	///<summary>Produces the quotient and the remainder of two unsigned 16-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(ushort, ushort)")]
	public static (ushort Quotient, ushort Remainder) _267e04d7693208d4(Number left, Number right)
		=> UInt16Module._80e78c0aa0b98fef(left, right);

	///<summary>Produces the quotient and the remainder of two signed 32-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(int, int)")]
	public static (int Quotient, int Remainder) _45a4ab35fd8b6be8(Number left, Number right)
		=> Int32Module._d4cc9914e60e5643(left, right);

	///<summary>Produces the quotient and the remainder of two unsigned 32-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(uint, uint)")]
	public static (uint Quotient, uint Remainder) _c8e57fe110813408(Number left, Number right)
		=> UInt32Module._8a073d758132b5bb(left, right);

	///<summary>Produces the quotient and the remainder of two signed 64-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(long, long)")]
	public static (BigInt Quotient, BigInt Remainder) _96f1b2c20bd2e40b(BigInt left, BigInt right)
		=> Int64Module._28273cd350760efe(left, right);

	///<summary>Produces the quotient and the remainder of two unsigned 64-bit numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.DivRem(ulong, ulong)")]
	public static (BigInt Quotient, BigInt Remainder) _4d9536a1220a7365(BigInt left, BigInt right)
		=> UInt64Module._fbae7adf5aedb1a5(left, right);

	///<summary>Produces the quotient and the remainder of two signed native-size numbers.</summary>
	[Jazor(Op.Discard ,"static System.Math.DivRem(nint, nint)")]
	public extern static (nint Quotient, nint Remainder) _98ac53eebed8e823(object left, object right);

	///<summary>Produces the quotient and the remainder of two unsigned native-size numbers.</summary>
	[Jazor(Op.Discard ,"static System.Math.DivRem(nuint, nuint)")]
	public extern static (nuint Quotient, nuint Remainder) _1b2439f6e0d31865(object left, object right);

	///<summary>Returns the smallest integral value that is greater than or equal to the specified decimal number.</summary>
	[Jazor(Op.Import ,"static System.Math.Ceiling(decimal)")]
	public static string _84cbc0eaf2d899af(string d)
		=> DecimalModule._84028a6e79626057(d);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(byte, byte, byte)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _8921213084b6685c(Number value, Number min, Number max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Import ,"static System.Math.Clamp(decimal, decimal, decimal)")]
	public static string _735e24a467fce432(string value, string min, string max)
		=> DecimalModule._e886400fbfdbdaaa(value, min, max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(double, double, double)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _a416f1414d77c0fa(Number value, Number min, Number max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(short, short, short)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _86bd53ebc62ad520(Number value, Number min, Number max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(int, int, int)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _ac5962f496c6acc0(Number value, Number min, Number max);

	/// <summary>
	/// C#: Math.Clamp(long)
	/// JS: BigInt 比较后返回边界或原值
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Clamp(long, long, long)", "(__arg1 < __arg2 ? __arg2 : (__arg1 > __arg3 ? __arg3 : __arg1))")]
	public extern static BigInt _d74b585d391b448a(BigInt value, BigInt min, BigInt max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Discard ,"static System.Math.Clamp(nint, nint, nint)")]
	public extern static nint _63803d1734456eee(object value, object min, object max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(sbyte, sbyte, sbyte)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _f2a0d82587b4e02a(Number value, Number min, Number max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(float, float, float)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _751a0e2d62df6aff(Number value, Number min, Number max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(ushort, ushort, ushort)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _74d6735122ecb151(Number value, Number min, Number max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Inline, "static System.Math.Clamp(uint, uint, uint)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _8322034639d6a05c(Number value, Number min, Number max);

	/// <summary>
	/// C#: Math.Clamp(ulong)
	/// JS: BigInt 比较后返回边界或原值
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Clamp(ulong, ulong, ulong)", "(__arg1 < __arg2 ? __arg2 : (__arg1 > __arg3 ? __arg3 : __arg1))")]
	public extern static BigInt _f1743d6e0c7a2101(BigInt value, BigInt min, BigInt max);

	///<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
	[Jazor(Op.Discard ,"static System.Math.Clamp(nuint, nuint, nuint)")]
	public extern static nuint _25b262a1a57d5d06(object value, object min, object max);

	///<summary>Returns the largest integral value less than or equal to the specified decimal number.</summary>
	[Jazor(Op.Import ,"static System.Math.Floor(decimal)")]
	public static string _b12193a7b6647a82(string d)
		=> DecimalModule._518facaaeeb29ead(d);

	///<summary>Returns the remainder resulting from the division of a specified number by another specified number.</summary>
	[Jazor(Op.Import, "static System.Math.IEEERemainder(double, double)")]
	public static Number _288c181b5d9cf968(Number x, Number y)
		=> DoubleModule.Ieee754RemainderCore(x, y);

	///<summary>Returns the base 2 integer logarithm of a specified number.</summary>
	[Jazor(Op.Import, "static System.Math.ILogB(double)")]
	public static Number _51e4d6005e6e11ef(Number x)
		=> DoubleModule.ILogBCore(x);

	///<summary>Returns the logarithm of a specified number in a specified base.</summary>
	[Jazor(Op.Inline, "static System.Math.Log(double, double)", "(Math.log(__arg1) / Math.log(__arg2))")]
	public extern static Number _da091a35a0d7bc64(Number a, Number newBase);

	///<summary>Returns the larger of two 8-bit unsigned integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(byte, byte)", "max")]
	public extern static Number _a26e415f31a1dd41(Number val1, Number val2);

	///<summary>Returns the larger of two decimal numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.Max(decimal, decimal)")]
	public static string _68326de2fcd99278(string val1, string val2)
		=> DecimalModule._872018e11335480a(val1, val2);

	///<summary>Returns the larger of two double-precision floating-point numbers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(double, double)", "max")]
	public extern static Number _1bcd36ee2d1a5261(Number val1, Number val2);

	///<summary>Returns the larger of two 16-bit signed integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(short, short)", "max")]
	public extern static Number _52a2dcd88692950d(Number val1, Number val2);

	///<summary>Returns the larger of two 32-bit signed integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(int, int)", "max")]
	public extern static Number _c89f0321e6ece69a(Number val1, Number val2);

	/// <summary>
	/// C#: Math.Max(long, long)
	/// JS: val1 > val2 ? val1 : val2
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Max(long, long)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _1513b88bb1abfff1(BigInt val1, BigInt val2);

	///<summary>Returns the larger of two native signed integers.</summary>
	[Jazor(Op.Discard ,"static System.Math.Max(nint, nint)")]
	public extern static nint _c03baee2a94d0113(object val1, object val2);

	///<summary>Returns the larger of two 8-bit signed integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(sbyte, sbyte)", "max")]
	public extern static Number _cb1537d45a143e0d(Number val1, Number val2);

	///<summary>Returns the larger of two single-precision floating-point numbers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(float, float)", "max")]
	public extern static Number _5acf698f9a9ada61(Number val1, Number val2);

	///<summary>Returns the larger of two 16-bit unsigned integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(ushort, ushort)", "max")]
	public extern static Number _07de56d6927ee6af(Number val1, Number val2);

	///<summary>Returns the larger of two 32-bit unsigned integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Max(uint, uint)", "max")]
	public extern static Number _6638c647001d2908(Number val1, Number val2);

	/// <summary>
	/// C#: Math.Max(ulong, ulong)
	/// JS: val1 > val2 ? val1 : val2
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Max(ulong, ulong)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _3ac884b966eeb605(BigInt val1, BigInt val2);

	///<summary>Returns the larger of two native unsigned integers.</summary>
	[Jazor(Op.Discard ,"static System.Math.Max(nuint, nuint)")]
	public extern static nuint _7f3becc9b24d51d3(object val1, object val2);

	///<summary>Returns the larger magnitude of two double-precision floating-point numbers.</summary>
	[Jazor(Op.Import, "static System.Math.MaxMagnitude(double, double)")]
	public static Number _7922e74207558715(Number x, Number y)
		=> MaxMagnitudeCore(x, y);

	///<summary>Returns the smaller of two 8-bit unsigned integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(byte, byte)", "min")]
	public extern static Number _f8806316e956dbb8(Number val1, Number val2);

	///<summary>Returns the smaller of two decimal numbers.</summary>
	[Jazor(Op.Import ,"static System.Math.Min(decimal, decimal)")]
	public static string _87f14d6593efd87f(string val1, string val2)
		=> DecimalModule._ceb21f954af742e7(val1, val2);

	///<summary>Returns the smaller of two double-precision floating-point numbers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(double, double)", "min")]
	public extern static Number _d0d428d1a1f7d899(Number val1, Number val2);

	///<summary>Returns the smaller of two 16-bit signed integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(short, short)", "min")]
	public extern static Number _d7a779b3283b34dc(Number val1, Number val2);

	///<summary>Returns the smaller of two 32-bit signed integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(int, int)", "min")]
	public extern static Number _7fb229bda6fa1941(Number val1, Number val2);

	/// <summary>
	/// C#: Math.Min(long, long)
	/// JS: val1 < val2 ? val1 : val2
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Min(long, long)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _b98fea9bd3e4ce52(BigInt val1, BigInt val2);

	///<summary>Returns the smaller of two native signed integers.</summary>
	[Jazor(Op.Discard ,"static System.Math.Min(nint, nint)")]
	public extern static nint _e3cdc59c4e2b3f04(object val1, object val2);

	///<summary>Returns the smaller of two 8-bit signed integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(sbyte, sbyte)", "min")]
	public extern static Number _0f8bf59fee331622(Number val1, Number val2);

	///<summary>Returns the smaller of two single-precision floating-point numbers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(float, float)", "min")]
	public extern static Number _2c1e93a158a72838(Number val1, Number val2);

	///<summary>Returns the smaller of two 16-bit unsigned integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(ushort, ushort)", "min")]
	public extern static Number _3e853af2da5fd862(Number val1, Number val2);

	///<summary>Returns the smaller of two 32-bit unsigned integers.</summary>
	[Jazor(Op.Alias, "static System.Math.Min(uint, uint)", "min")]
	public extern static Number _849b5d874239b92c(Number val1, Number val2);

	/// <summary>
	/// C#: Math.Min(ulong, ulong)
	/// JS: val1 < val2 ? val1 : val2
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Min(ulong, ulong)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _d468e999912e1120(BigInt val1, BigInt val2);

	///<summary>Returns the smaller of two native unsigned integers.</summary>
	[Jazor(Op.Discard ,"static System.Math.Min(nuint, nuint)")]
	public extern static nuint _c03fe2f175939d3a(object val1, object val2);

	///<summary>Returns the smaller magnitude of two double-precision floating-point numbers.</summary>
	[Jazor(Op.Import, "static System.Math.MinMagnitude(double, double)")]
	public static Number _44776725ec896ede(Number x, Number y)
		=> MinMagnitudeCore(x, y);

	///<summary>Returns an estimate of the reciprocal of a specified number.</summary>
	[Jazor(Op.Inline, "static System.Math.ReciprocalEstimate(double)", "(1 / __arg1)")]
	public extern static Number _63ae085718e46139(Number d);

	///<summary>Returns an estimate of the reciprocal square root of a specified number.</summary>
	[Jazor(Op.Inline, "static System.Math.ReciprocalSqrtEstimate(double)", "(1 / Math.sqrt(__arg1))")]
	public extern static Number _5ab45aaeb89fbf4c(Number d);

	///<summary>Rounds a decimal value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
	[Jazor(Op.Import ,"static System.Math.Round(decimal)")]
	public static string _257741f3e4260d82(string d)
		=> DecimalModule._4a816369b59f1ca3(d);

	///<summary>Rounds a decimal value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.</summary>
	[Jazor(Op.Import ,"static System.Math.Round(decimal, int)")]
	public static string _10e883cf6d89b70c(string d, Number decimals)
		=> DecimalModule._bc3a974d51c694ab(d, decimals);

	///<summary>Rounds a decimal value an integer using the specified rounding convention.</summary>
	[Jazor(Op.Import ,"static System.Math.Round(decimal, System.MidpointRounding)")]
	public static string _584a7b2219b578fa(string d, object mode)
		=> DecimalModule._a334f7e82122cfc2(d, mode);

	///<summary>Rounds a decimal value to a specified number of fractional digits using the specified rounding convention.</summary>
	[Jazor(Op.Import ,"static System.Math.Round(decimal, int, System.MidpointRounding)")]
	public static string _b955eff4c2d1fa63(string d, Number decimals, object mode)
		=> DecimalModule._09ee3a4652dbe73c(d, decimals, mode);

	///<summary>Rounds a double-precision floating-point value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
	[Jazor(Op.Import, "static System.Math.Round(double)")]
	public static Number _6cd7f67f98eae0bc(Number a)
		=> DoubleModule.RoundToEvenCore(a);

	///<summary>Rounds a double-precision floating-point value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.</summary>
	[Jazor(Op.Import, "static System.Math.Round(double, int)")]
	public static Number _dab059b61a5b7428(Number value, Number digits)
		=> DoubleModule.RoundCore(value, digits, 0);

	///<summary>Rounds a double-precision floating-point value to an integer using the specified rounding convention.</summary>
	[Jazor(Op.Import, "static System.Math.Round(double, System.MidpointRounding)")]
	public static Number _a7f99c51d0db12b5(Number value, Number mode)
		=> DoubleModule.RoundCore(value, 0, mode);

	///<summary>Rounds a double-precision floating-point value to a specified number of fractional digits using the specified rounding convention.</summary>
	[Jazor(Op.Import, "static System.Math.Round(double, int, System.MidpointRounding)")]
	public static Number _ef441dda2abcc022(Number value, Number digits, Number mode)
		=> DoubleModule.RoundCore(value, digits, mode);

	///<summary>Returns an integer that indicates the sign of a decimal number.</summary>
	[Jazor(Op.Import ,"static System.Math.Sign(decimal)")]
	public static Number _8d626104a531d041(string value)
		=> DecimalModule._ed803cf9c8c052f1(value);

	///<summary>Returns an integer that indicates the sign of a double-precision floating-point number.</summary>
	[Jazor(Op.Import, "static System.Math.Sign(double)")]
	public static Number _9a554cfca79bdc59(Number value)
		=> DoubleModule.SignCore(value);

	///<summary>Returns an integer that indicates the sign of a 16-bit signed integer.</summary>
	[Jazor(Op.Alias, "static System.Math.Sign(short)", "sign")]
	public extern static Number _f8eefd9c948ed90a(Number value);

	///<summary>Returns an integer that indicates the sign of a 32-bit signed integer.</summary>
	[Jazor(Op.Alias, "static System.Math.Sign(int)", "sign")]
	public extern static Number _cfeb8757509066b2(Number value);

	/// <summary>
	/// C#: Math.Sign(long)
	/// JS: value > 0n ? 1 : (value < 0n ? -1 : 0)
	/// </summary>
	[Jazor(Op.Inline ,"static System.Math.Sign(long)", "(__arg1 > 0n ? 1 : (__arg1 < 0n ? -1 : 0))")]
	public extern static Number _5354f93121b296ff(BigInt value);

	///<summary>Returns an integer that indicates the sign of a native sized signed integer.</summary>
	[Jazor(Op.Discard ,"static System.Math.Sign(nint)")]
	public extern static Number _e5d5397dfe870f94(object value);

	///<summary>Returns an integer that indicates the sign of an 8-bit signed integer.</summary>
	[Jazor(Op.Alias, "static System.Math.Sign(sbyte)", "sign")]
	public extern static Number _88575fe160876695(Number value);

	///<summary>Returns an integer that indicates the sign of a single-precision floating-point number.</summary>
	[Jazor(Op.Import, "static System.Math.Sign(float)")]
	public static Number _c0668680ba7ef96e(Number value)
		=> SingleModule.SignCore(value);

	///<summary>Calculates the integral part of a specified decimal number.</summary>
	[Jazor(Op.Import ,"static System.Math.Truncate(decimal)")]
	public static string _abd9211e1e7514b4(string d)
		=> DecimalModule._be8b149ea0e1d76b(d);

	///<summary>Calculates the integral part of a specified double-precision floating-point number.</summary>
	[Jazor(Op.Alias, "static System.Math.Truncate(double)", "trunc")]
	public extern static Number _b74eaf879a3b5fd7(Number d);

	///<summary>Returns x * 2^n computed efficiently.</summary>
	[Jazor(Op.Inline, "static System.Math.ScaleB(double, int)", "(__arg1 * Math.pow(2, __arg2))")]
	public extern static Number _11ce4194425195ad(Number x, Number n);
}
