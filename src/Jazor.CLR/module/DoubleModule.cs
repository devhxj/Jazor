namespace Jazor.CLR;

/// <summary>
/// System.Double (double) 类型模块映射规则
///
/// C# double 与 JavaScript Number 的对应关系：
/// - 都使用 IEEE 754 双精度浮点数
/// - 可以直接映射
///
/// Op 类型选择原则：
/// - Allowed: 操作符
/// - Inline: 简单表达式和常量
/// - Alias: JS Math 方法
/// - Import: 需要验证的 Parse/TryParse
/// - Discard: 不常用或平台特定的方法
/// </summary>
[ECMAScriptModule("System/DoubleModule.js")]
[Jazor(Op.Alias, "double", "Number")]
public static class DoubleModule
{
	private static bool IsInfinityCore(Number value)
		=> Object.Is(value, Number.POSITIVE_INFINITY) || Object.Is(value, Number.NEGATIVE_INFINITY);

	internal static bool IsFiniteCore(Number value)
		=> !IsNaN(value) && !IsInfinityCore(value);

	internal static bool IsNaNCore(Number value)
		=> IsNaN(value);

	internal static bool TryParseCore(string? text, out Number value)
	{
		value = 0;
		if (text == null)
			return false;

		var trimmed = text.Trim();
		if (trimmed.Length == 0)
			return false;

		var token = trimmed.ToLower();
		if (token == "nan")
		{
			value = Number.NaN;
			return true;
		}
		if (token == "infinity" || token == "+infinity")
		{
			value = Number.POSITIVE_INFINITY;
			return true;
		}
		if (token == "-infinity")
		{
			value = Number.NEGATIVE_INFINITY;
			return true;
		}

		var decimalPattern = new RegExp(@"^[+-]?(?:(?:\d+|\d{1,3}(?:,\d{3})+)(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?$");
		if (!decimalPattern.Test(trimmed))
			return false;

		value = NumberFn(trimmed.Replace(",", ""));
		return true;
	}

	internal static bool AreEqualCore(Number left, Number right)
	{
		if (IsNaN(left) || IsNaN(right))
			return IsNaN(left) && IsNaN(right);

		return !(left < right) && !(left > right);
	}

	internal static Number CompareCore(Number left, Number right)
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

	internal static bool IsPow2Core(Number value)
	{
		if (!IsFiniteCore(value) || value <= 0)
			return false;

		// JS Number 没有可直接复用的 64 位尾数位运算，这里退化为判定 log2 是否为整数。
		var exponent = Math.Log2Fn(value);
		return IsFiniteCore(exponent) && Math.FloorFn(exponent) == exponent;
	}

	internal static Number SignCore(Number value)
	{
		if (IsNaN(value))
			throw new Error("ArithmeticException: Function does not accept floating point Not-a-Number values.");

		if (value > 0)
			return 1;
		if (value < 0)
			return -1;

		return 0;
	}

	internal static Number MaxMagnitudeCore(Number x, Number y)
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

	internal static Number MinMagnitudeCore(Number x, Number y)
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

	internal static Number MaxMagnitudeNumberCore(Number x, Number y)
	{
		if (IsNaN(x))
			return y;
		if (IsNaN(y))
			return x;

		return MaxMagnitudeCore(x, y);
	}

	internal static Number MinMagnitudeNumberCore(Number x, Number y)
	{
		if (IsNaN(x))
			return y;
		if (IsNaN(y))
			return x;

		return MinMagnitudeCore(x, y);
	}

	// 常量 - 使用 Op.Inline
	[Jazor(Op.Inline, "static double.MinValue", "-Number.MAX_VALUE")]
	public extern static Number _minValue();

	[Jazor(Op.Inline, "static double.MaxValue", "Number.MAX_VALUE")]
	public extern static Number _maxValue();

	[Jazor(Op.Inline, "static double.Epsilon", "Number.MIN_VALUE")]
	public extern static Number _epsilon();

	[Jazor(Op.Inline, "static double.NegativeInfinity", "-Infinity")]
	public extern static Number _negativeInfinity();

	[Jazor(Op.Inline, "static double.PositiveInfinity", "Infinity")]
	public extern static Number _positiveInfinity();

	[Jazor(Op.Inline, "static double.NaN", "NaN")]
	public extern static Number _nan();

	[Jazor(Op.Inline, "static double.NegativeZero", "-0")]
	public extern static Number _negativeZero();

	[Jazor(Op.Inline, "static double.E", "Math.E")]
	public extern static Number _e();

	[Jazor(Op.Inline, "static double.Pi", "Math.PI")]
	public extern static Number _pi();

	[Jazor(Op.Inline, "static double.Tau", "(Math.PI * 2)")]
	public extern static Number _tau();

	[Jazor(Op.Discard, "double.Double()")]
	public extern static Number _f28ac141e9398355();

	// 静态判断方法
	[Jazor(Op.Inline, "static double.IsFinite(double)", "Number.isFinite(__arg1)")]
	public extern static bool _aed2927097617729(Number d);

	[Jazor(Op.Inline, "static double.IsInfinity(double)", "(__arg1 === Infinity || __arg1 === -Infinity)")]
	public extern static bool _8dab2b2ebaef92eb(Number d);

	[Jazor(Op.Inline, "static double.IsNaN(double)", "Number.isNaN(__arg1)")]
	public extern static bool _24e14b276e0c7e30(Number d);

	[Jazor(Op.Inline, "static double.IsNegative(double)", "(Object.is(__arg1, -0) || __arg1 < 0)")]
	public extern static bool _2f6ba4398ec15d8d(Number d);

	[Jazor(Op.Inline, "static double.IsNegativeInfinity(double)", "(__arg1 === -Infinity)")]
	public extern static bool _f0fb1d1302b488d6(Number d);

	// double 最小正规数是 2^-1022 ≈ 2.2250738585072014e-308。
	[Jazor(Op.Inline, "static double.IsNormal(double)", "(isFinite(__arg1) && __arg1 !== 0 && Math.abs(__arg1) >= 2.2250738585072014e-308)")]
	public extern static bool _9b3adc853b9cfe8f(Number d);

	[Jazor(Op.Inline, "static double.IsPositiveInfinity(double)", "(__arg1 === Infinity)")]
	public extern static bool _d15ff5d4064e951a(Number d);

	// double 次正规数是非零有限值，且绝对值小于最小正规数。
	[Jazor(Op.Inline, "static double.IsSubnormal(double)", "(isFinite(__arg1) && __arg1 !== 0 && Math.abs(__arg1) < 2.2250738585072014e-308)")]
	public extern static bool _a48f9d7298aa7e76(Number d);

	// CompareTo 和 Equals
	[Jazor(Op.Import, "double.CompareTo(object)")]
	public static Number _b0d483b6deae2278(Number instance, object? value)
	{
		if (value == null)
			return 1;
		if (TypeOf(value) != "number")
			throw new Error("ArgumentException: Object must be of type Double.");

		return CompareCore(instance, (Number)value);
	}

	[Jazor(Op.Inline, "double.CompareTo(double)", "(isNaN(__arg1) ? (isNaN(__arg2) ? 0 : -1) : (isNaN(__arg2) ? 1 : (__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))))")]
	public extern static Number _7b8150796366d2b1(Number instance, Number value);

	[Jazor(Op.Import, "override double.Equals(object)")]
	public static bool _b5f97a04bba189b0(Number instance, object? obj)
	{
		if (obj == null || TypeOf(obj) != "number")
			return false;

		return AreEqualCore(instance, (Number)obj);
	}

	// 操作符 - 使用 Op.Allowed
	[Jazor(Op.Allowed, "static double.operator ==(double, double)")]
	public extern static bool _a4d750aa912f2bd7(Number left, Number right);

	[Jazor(Op.Allowed, "static double.operator !=(double, double)")]
	public extern static bool _d17fe84520a83d30(Number left, Number right);

	[Jazor(Op.Allowed, "static double.operator <(double, double)")]
	public extern static bool _f33377c7d472de67(Number left, Number right);

	[Jazor(Op.Allowed, "static double.operator >(double, double)")]
	public extern static bool _0ff0091b916b4a34(Number left, Number right);

	[Jazor(Op.Allowed, "static double.operator <=(double, double)")]
	public extern static bool _cda1ab775e265c7b(Number left, Number right);

	[Jazor(Op.Allowed, "static double.operator >=(double, double)")]
	public extern static bool _4f7605355b48150a(Number left, Number right);

	[Jazor(Op.Inline, "double.Equals(double)", "(isNaN(__arg1) ? isNaN(__arg2) : (isNaN(__arg2) ? false : (!(__arg1 < __arg2) && !(__arg1 > __arg2))))")]
	public extern static bool _6c01d37504f73181(Number instance, Number obj);

	[Jazor(Op.Discard, "override double.GetHashCode()")]
	public extern static Number _73dea7106d8085a6(Number instance);

	// ToString
	[Jazor(Op.Alias, "override double.ToString()", "toString")]
	public extern static string _faf4dc1f54bddf75(Number instance);

	[Jazor(Op.Discard, "double.ToString(string)")]
	public extern static string _3fdd3b28b5e148e9(Number instance, string? format);

	[Jazor(Op.Discard, "double.ToString(System.IFormatProvider)")]
	public extern static string _060e7930ebdb6c74(Number instance, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "double.ToString(string, System.IFormatProvider)")]
	public extern static string _3ab59f70a1114579(Number instance, string? format, Intl.NumberFormat? provider);

	// Parse/TryParse
	/// <summary>
	/// C#: double.Parse(s)
	/// JS: Number(s) with validation
	/// </summary>
	[Jazor(Op.Import, "static double.Parse(string)")]
	public static Number _5810f85a3710b88d(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		if (!TryParseCore(s, out var result))
			throw new Error($"FormatException: The input string '{s}' was not in a correct format.");
		return result;
	}

	[Jazor(Op.Discard, "static double.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _41091ebfff87c5a3(string s, object style);

	[Jazor(Op.Discard, "static double.Parse(string, System.IFormatProvider)")]
	public extern static Number _5b091c28760d19a0(string s, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static double.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _e23e5c173e845cc9(string s, object style, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static double.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _1566d690221e91c2(string s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: double.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static double.TryParse(string, out double)")]
	public static Array<object?> _a29d389185c5e37d(string? s, Number result)
	{
		if (!TryParseCore(s, out var value))
			return [false, 0];

		return [true, value];
	}

	[Jazor(Op.Discard, "static double.TryParse(System.ReadOnlySpan<char>, out double)")]
	public extern static Array<object?> _059799e0a3b763c1(string s, Number result);

	[Jazor(Op.Discard, "static double.TryParse(System.ReadOnlySpan<byte>, out double)")]
	public extern static Array<object?> _ec88293b6cb03791(Uint8Array utf8Text, Number result);

	[Jazor(Op.Discard, "static double.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out double)")]
	public extern static Array<object?> _ac0f50fde0490598(string? s, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static double.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out double)")]
	public extern static Array<object?> _632e234f0359bd6f(string s, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Inline, "double.GetTypeCode()", "14")]
	public extern static System.TypeCode _faf3eda13d4c24c6(Number instance);

	// 数学方法
	[Jazor(Op.Import, "static double.IsPow2(double)")]
	public static bool _0f9f49a802919a8f(Number value)
		=> IsPow2Core(value);

	[Jazor(Op.Inline, "static double.Log2(double)", "Math.log2(__arg1)")]
	public extern static Number _3ca26f53faecc630(Number value);

	[Jazor(Op.Inline, "static double.Exp(double)", "Math.exp(__arg1)")]
	public extern static Number _e94626bfb529f1e2(Number x);

	[Jazor(Op.Inline, "static double.ExpM1(double)", "(Math.exp(__arg1) - 1)")]
	public extern static Number _1a8fc1577d8842a1(Number x);

	[Jazor(Op.Inline, "static double.Exp2(double)", "Math.pow(2, __arg1)")]
	public extern static Number _894bcd9f10fe195f(Number x);

	[Jazor(Op.Inline, "static double.Exp2M1(double)", "(Math.pow(2, __arg1) - 1)")]
	public extern static Number _b2c7a69c53b5558f(Number x);

	[Jazor(Op.Inline, "static double.Exp10(double)", "(Math.pow(10, __arg1))")]
	public extern static Number _433ea7f5bfe42847(Number x);

	[Jazor(Op.Inline, "static double.Exp10M1(double)", "(Math.pow(10, __arg1) - 1)")]
	public extern static Number _aece0b0b794624da(Number x);

	[Jazor(Op.Inline, "static double.Ceiling(double)", "Math.ceil(__arg1)")]
	public extern static Number _e435d9759ac9c07d(Number x);

	[Jazor(Op.Discard, "static double.ConvertToInteger<TInteger>(double)")]
	public extern static TInteger _cf8db91150253994<TInteger>(Number value);

	[Jazor(Op.Discard, "static double.ConvertToIntegerNative<TInteger>(double)")]
	public extern static TInteger _869e51717acd1e28<TInteger>(Number value);

	[Jazor(Op.Inline, "static double.Floor(double)", "Math.floor(__arg1)")]
	public extern static Number _52dffd07187dd0c2(Number x);

	[Jazor(Op.Inline, "static double.Round(double)", "Math.round(__arg1)")]
	public extern static Number _0bc6b7459346bc5f(Number x);

	[Jazor(Op.Discard, "static double.Round(double, int)")]
	public extern static Number _b439595e3752c6a9(Number x, Number digits);

	[Jazor(Op.Discard, "static double.Round(double, System.MidpointRounding)")]
	public extern static Number _7aeacc68b27f02f7(Number x, object mode);

	[Jazor(Op.Discard, "static double.Round(double, int, System.MidpointRounding)")]
	public extern static Number _6e429701c9779ef6(Number x, Number digits, object mode);

	[Jazor(Op.Inline, "static double.Truncate(double)", "Math.trunc(__arg1)")]
	public extern static Number _98f3d13b9b717048(Number x);

	[Jazor(Op.Inline, "static double.Atan2(double, double)", "Math.atan2(__arg1, __arg2)")]
	public extern static Number _d606d02df668235c(Number y, Number x);

	[Jazor(Op.Inline, "static double.Atan2Pi(double, double)", "(Math.atan2(__arg1, __arg2) / Math.PI)")]
	public extern static Number _f54e39103ea7d6b5(Number y, Number x);

	[Jazor(Op.Discard, "static double.BitDecrement(double)")]
	public extern static Number _4ce9474a7b3b7534(Number x);

	[Jazor(Op.Discard, "static double.BitIncrement(double)")]
	public extern static Number _a83d47e386f63de0(Number x);

	[Jazor(Op.Inline, "static double.FusedMultiplyAdd(double, double, double)", "(__arg1 * __arg2 + __arg3)")]
	public extern static Number _a7385e0d1e651c3f(Number left, Number right, Number addend);

	[Jazor(Op.Inline, "static double.Ieee754Remainder(double, double)", "(__arg1 - __arg2 * Math.round(__arg1 / __arg2))")]
	public extern static Number _092bc2bc891d33a8(Number left, Number right);

	[Jazor(Op.Inline, "static double.ILogB(double)", "Math.log2(__arg1)")]
	public extern static Number _48628732b1dc8ac9(Number x);

	[Jazor(Op.Inline, "static double.Lerp(double, double, double)", "(__arg1 + (__arg2 - __arg1) * __arg3)")]
	public extern static Number _a5426c98bc8a2df3(Number value1, Number value2, Number amount);

	[Jazor(Op.Inline, "static double.ReciprocalEstimate(double)", "(1 / __arg1)")]
	public extern static Number _a07d02f7af20108d(Number x);

	[Jazor(Op.Inline, "static double.ReciprocalSqrtEstimate(double)", "(1 / Math.sqrt(__arg1))")]
	public extern static Number _093ed023d5ee163e(Number x);

	[Jazor(Op.Inline, "static double.ScaleB(double, int)", "(__arg1 * Math.pow(2, __arg2))")]
	public extern static Number _efc90b780554b82f(Number x, Number n);

	[Jazor(Op.Inline, "static double.Acosh(double)", "Math.acosh(__arg1)")]
	public extern static Number _a0e391e3d9aa5827(Number x);

	[Jazor(Op.Inline, "static double.Asinh(double)", "Math.asinh(__arg1)")]
	public extern static Number _57778d867801a120(Number x);

	[Jazor(Op.Inline, "static double.Atanh(double)", "Math.atanh(__arg1)")]
	public extern static Number _21375f189d937aa8(Number x);

	[Jazor(Op.Inline, "static double.Cosh(double)", "Math.cosh(__arg1)")]
	public extern static Number _e4a259570c5acab6(Number x);

	[Jazor(Op.Inline, "static double.Sinh(double)", "Math.sinh(__arg1)")]
	public extern static Number _dea96f28cdef92ad(Number x);

	[Jazor(Op.Inline, "static double.Tanh(double)", "Math.tanh(__arg1)")]
	public extern static Number _5169c7d89ba27c38(Number x);

	[Jazor(Op.Inline, "static double.Log(double)", "Math.log(__arg1)")]
	public extern static Number _f89aa2d9ce52cc5e(Number x);

	[Jazor(Op.Inline, "static double.Log(double, double)", "(Math.log(__arg1) / Math.log(__arg2))")]
	public extern static Number _2367dc158f1f7ec9(Number x, Number newBase);

	[Jazor(Op.Inline, "static double.LogP1(double)", "Math.log1p(__arg1)")]
	public extern static Number _379f80adec6e897b(Number x);

	[Jazor(Op.Inline, "static double.Log2P1(double)", "(Math.log2(__arg1 + 1))")]
	public extern static Number _0f38233678cfefdc(Number x);

	[Jazor(Op.Inline, "static double.Log10(double)", "Math.log10(__arg1)")]
	public extern static Number _d057b30c2fca7de9(Number x);

	[Jazor(Op.Inline, "static double.Log10P1(double)", "(Math.log10(__arg1 + 1))")]
	public extern static Number _f0b78003a9ab01fb(Number x);

	[Jazor(Op.Inline, "static double.Clamp(double, double, double)", "(Math.max(__arg2, Math.min(__arg1, __arg3)))")]
	public extern static Number _8a90b4c9a1beefd9(Number value, Number min, Number max);

	[Jazor(Op.Discard, "static double.ClampNative(double, double, double)")]
	public extern static Number _ead55aa3a172f045(Number value, Number min, Number max);

	[Jazor(Op.Inline, "static double.CopySign(double, double)", "((__arg2 < 0 || Object.is(__arg2, -0)) ? -Math.abs(__arg1) : Math.abs(__arg1))")]
	public extern static Number _7d753440d9da2ba5(Number value, Number sign);

	[Jazor(Op.Inline, "static double.Max(double, double)", "Math.max(__arg1, __arg2)")]
	public extern static Number _4d275f0cc2087a70(Number x, Number y);

	[Jazor(Op.Discard, "static double.MaxNative(double, double)")]
	public extern static Number _a0dd8cfd308fc2ee(Number x, Number y);

	[Jazor(Op.Inline, "static double.MaxNumber(double, double)", "(isNaN(__arg1) ? __arg2 : (isNaN(__arg2) ? __arg1 : Math.max(__arg1, __arg2)))")]
	public extern static Number _ca88bd0ea64fa29f(Number x, Number y);

	[Jazor(Op.Inline, "static double.Min(double, double)", "Math.min(__arg1, __arg2)")]
	public extern static Number _8a25c3cdacb6ea23(Number x, Number y);

	[Jazor(Op.Discard, "static double.MinNative(double, double)")]
	public extern static Number _2aadcd7ef1e13714(Number x, Number y);

	[Jazor(Op.Inline, "static double.MinNumber(double, double)", "(isNaN(__arg1) ? __arg2 : (isNaN(__arg2) ? __arg1 : Math.min(__arg1, __arg2)))")]
	public extern static Number _d19f0527d6ae110f(Number x, Number y);

	[Jazor(Op.Import, "static double.Sign(double)")]
	public static Number _eee146c74a9bc322(Number value)
		=> SignCore(value);

	[Jazor(Op.Inline, "static double.Abs(double)", "Math.abs(__arg1)")]
	public extern static Number _13256ae561a599a8(Number value);

	[Jazor(Op.Discard, "static double.CreateChecked<TOther>(TOther)")]
	public extern static Number _ddfc88bb430f2c3e<TOther>(object value);

	[Jazor(Op.Discard, "static double.CreateSaturating<TOther>(TOther)")]
	public extern static Number _5bb76ff1642d9cf8<TOther>(object value);

	[Jazor(Op.Discard, "static double.CreateTruncating<TOther>(TOther)")]
	public extern static Number _e3a12f862df0ccea<TOther>(object value);

	[Jazor(Op.Inline, "static double.IsEvenInteger(double)", "(__arg1 % 2 === 0)")]
	public extern static bool _e3c00c1b96ee23bd(Number value);

	[Jazor(Op.Inline, "static double.IsInteger(double)", "(Number.isInteger(__arg1))")]
	public extern static bool _f0cb8da3d3123834(Number value);

	[Jazor(Op.Inline, "static double.IsOddInteger(double)", "(__arg1 % 2 !== 0)")]
	public extern static bool _0f52036842645ea9(Number value);

	[Jazor(Op.Inline, "static double.IsPositive(double)", "(__arg1 > 0 || Object.is(__arg1, 0))")]
	public extern static bool _c1220c050b39d180(Number value);

	[Jazor(Op.Inline, "static double.IsRealNumber(double)", "(!isNaN(__arg1) && __arg1 !== Infinity && __arg1 !== -Infinity)")]
	public extern static bool _0e7439da8bbce1ab(Number value);

	[Jazor(Op.Import, "static double.MaxMagnitude(double, double)")]
	public static Number _b6202851542d164c(Number x, Number y)
		=> MaxMagnitudeCore(x, y);

	[Jazor(Op.Import, "static double.MaxMagnitudeNumber(double, double)")]
	public static Number _7f7b38b043f3f42f(Number x, Number y)
		=> MaxMagnitudeNumberCore(x, y);

	[Jazor(Op.Import, "static double.MinMagnitude(double, double)")]
	public static Number _bb1daa880a2ad14e(Number x, Number y)
		=> MinMagnitudeCore(x, y);

	[Jazor(Op.Import, "static double.MinMagnitudeNumber(double, double)")]
	public static Number _315c6cdfa11efcf2(Number x, Number y)
		=> MinMagnitudeNumberCore(x, y);

	[Jazor(Op.Discard, "static double.MultiplyAddEstimate(double, double, double)")]
	public extern static Number _a3676143141ac38a(Number left, Number right, Number addend);

	[Jazor(Op.Import, "static double.TryParse(string, System.IFormatProvider, out double)")]
	public static Array<object?> _f1644d5121fae09c(string? s, Intl.NumberFormat? provider, Number result)
	{
		if (!TryParseCore(s, out var value))
			return [false, 0];

		return [true, value];
	}

	[Jazor(Op.Inline, "static double.Pow(double, double)", "Math.pow(__arg1, __arg2)")]
	public extern static Number _a9ce690fc0374936(Number x, Number y);

	[Jazor(Op.Inline, "static double.Cbrt(double)", "Math.cbrt(__arg1)")]
	public extern static Number _be2f8c6b23df2f9d(Number x);

	[Jazor(Op.Inline, "static double.Hypot(double, double)", "Math.hypot(__arg1, __arg2)")]
	public extern static Number _7b8e31add532abe8(Number x, Number y);

	[Jazor(Op.Inline, "static double.RootN(double, int)", "Math.pow(__arg1, 1 / __arg2)")]
	public extern static Number _83649fc6ded4d88e(Number x, Number n);

	[Jazor(Op.Inline, "static double.Sqrt(double)", "Math.sqrt(__arg1)")]
	public extern static Number _73df268429011d00(Number x);

	[Jazor(Op.Discard, "static double.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _ffac89005f82f8e5(string s, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static double.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out double)")]
	public extern static Array<object?> _55ffdd4c4ffdc9a8(string s, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Inline, "static double.Acos(double)", "Math.acos(__arg1)")]
	public extern static Number _1c32d7b441f1bec1(Number x);

	[Jazor(Op.Inline, "static double.AcosPi(double)", "(Math.acos(__arg1) / Math.PI)")]
	public extern static Number _4a99593b807868d6(Number x);

	[Jazor(Op.Inline, "static double.Asin(double)", "Math.asin(__arg1)")]
	public extern static Number _517eb387ef38a60b(Number x);

	[Jazor(Op.Inline, "static double.AsinPi(double)", "(Math.asin(__arg1) / Math.PI)")]
	public extern static Number _1a0239dc7bac42d0(Number x);

	[Jazor(Op.Inline, "static double.Atan(double)", "Math.atan(__arg1)")]
	public extern static Number _a6a8f60d8be1baab(Number x);

	[Jazor(Op.Inline, "static double.AtanPi(double)", "(Math.atan(__arg1) / Math.PI)")]
	public extern static Number _fa0c5717daf60a22(Number x);

	[Jazor(Op.Inline, "static double.Cos(double)", "Math.cos(__arg1)")]
	public extern static Number _ab249d49b3cb5f87(Number x);

	[Jazor(Op.Inline, "static double.CosPi(double)", "Math.cos(__arg1 * Math.PI)")]
	public extern static Number _68646d1a3f7e1c4e(Number x);

	[Jazor(Op.Inline, "static double.DegreesToRadians(double)", "(__arg1 * Math.PI / 180)")]
	public extern static Number _b613a401ab60cfa7(Number degrees);

	[Jazor(Op.Inline, "static double.RadiansToDegrees(double)", "(__arg1 * 180 / Math.PI)")]
	public extern static Number _1ed0662536b0a079(Number radians);

	[Jazor(Op.Inline, "static double.Sin(double)", "Math.sin(__arg1)")]
	public extern static Number _82a42c3870a8a263(Number x);

	[Jazor(Op.Import, "static double.SinCos(double)")]
	public static (double Sin, double Cos) _bc56189e3e1f8a22(Number x)
		=> (Sin: Math.SinFn(x), Cos: Math.CosFn(x));

	[Jazor(Op.Import, "static double.SinCosPi(double)")]
	public static (double SinPi, double CosPi) _0f4aeef5d225794d(Number x)
	{
		var angle = x * Math.PI;
		return (SinPi: Math.SinFn(angle), CosPi: Math.CosFn(angle));
	}

	[Jazor(Op.Inline, "static double.SinPi(double)", "Math.sin(__arg1 * Math.PI)")]
	public extern static Number _364c4226f027481d(Number x);

	[Jazor(Op.Inline, "static double.Tan(double)", "Math.tan(__arg1)")]
	public extern static Number _3f5c35650c642d58(Number x);

	[Jazor(Op.Inline, "static double.TanPi(double)", "Math.tan(__arg1 * Math.PI)")]
	public extern static Number _c193db8303daa585(Number x);

	[Jazor(Op.Discard, "static double.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _95cf4052dcf1d6d8(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static double.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out double)")]
	public extern static Array<object?> _654e8bbd8869bbea(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static double.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _cd8bb3b9e099ef63(Uint8Array utf8Text, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static double.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out double)")]
	public extern static Array<object?> _75fcd554c7fa663e(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
