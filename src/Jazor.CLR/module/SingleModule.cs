namespace Jazor.CLR;

/// <summary>
/// System.Single (float) 类型模块映射规则
///
/// C# float 与 JavaScript Number 的对应关系：
/// - 都使用 IEEE 754 浮点数
/// - 可以直接映射
///
/// Op 类型选择原则：
/// - Inline: 简单表达式和常量
/// - Alias: JS 原生方法
/// - Import: 需要验证的 Parse/TryParse
/// - Discard: 不常用或平台特定的方法
/// </summary>
[ECMAScriptModule("System/SingleModule.js")]
[Jazor(Op.Alias, "float","Number")]
public static class SingleModule
{
	// 常量 - 使用 Op.Inline
	[Jazor(Op.Inline, "static float.MinValue", "-3.4028235E+38")]
	public extern static Number _minValue();

	[Jazor(Op.Inline, "static float.MaxValue", "3.4028235E+38")]
	public extern static Number _maxValue();

	[Jazor(Op.Inline, "static float.Epsilon", "1E-45")]
	public extern static Number _epsilon();

	[Jazor(Op.Inline, "static float.NegativeInfinity", "-Infinity")]
	public extern static Number _negativeInfinity();

	[Jazor(Op.Inline, "static float.PositiveInfinity", "Infinity")]
	public extern static Number _positiveInfinity();

	[Jazor(Op.Inline, "static float.NaN", "NaN")]
	public extern static Number _nan();

	[Jazor(Op.Inline, "static float.NegativeZero", "-0")]
	public extern static Number _negativeZero();

	[Jazor(Op.Inline, "static float.E", "Math.E")]
	public extern static Number _e();

	[Jazor(Op.Inline, "static float.Pi", "Math.PI")]
	public extern static Number _pi();

	[Jazor(Op.Inline, "static float.Tau", "(Math.PI * 2)")]
	public extern static Number _tau();

	[Jazor(Op.Discard ,"float.Single()")]
	public extern static Number _a6b96ca392da4917();

	/// <summary>
	/// C#: float.IsFinite(f)
	/// JS: isFinite(f)
	/// </summary>
	[Jazor(Op.Alias, "static float.IsFinite(float)", "isFinite")]
	public extern static bool _00118f159d09918d(Number f);

	/// <summary>
	/// C#: float.IsInfinity(f)
	/// JS: !isFinite(f)
	/// </summary>
	[Jazor(Op.Inline, "static float.IsInfinity(float)", "!isFinite(@#{0})")]
	public extern static bool _47887f5e1e35e199(Number f);

	/// <summary>
	/// C#: float.IsNaN(f)
	/// JS: isNaN(f)
	/// </summary>
	[Jazor(Op.Alias, "static float.IsNaN(float)", "isNaN")]
	public extern static bool _8c3d7a2e3b690c9a(Number f);

	/// <summary>
	/// C#: float.IsNegative(f)
	/// JS: Object.is(f, -0) || f < 0
	/// </summary>
	[Jazor(Op.Inline, "static float.IsNegative(float)", "(Object.is(@#{0}, -0) || @#{0} < 0)")]
	public extern static bool _846e9450c3f550b6(Number f);

	/// <summary>
	/// C#: float.IsNegativeInfinity(f)
	/// JS: f === -Infinity
	/// </summary>
	[Jazor(Op.Inline, "static float.IsNegativeInfinity(float)", "(@#{0} === -Infinity)")]
	public extern static bool _8b4a47cad79ef70b(Number f);

	///<summary>Determines whether the specified value is normal.</summary>
	[Jazor(Op.Discard ,"static float.IsNormal(float)")]
	public extern static bool _cbc5abbbccc623b6(Number f);

	/// <summary>
	/// C#: float.IsPositiveInfinity(f)
	/// JS: f === Infinity
	/// </summary>
	[Jazor(Op.Inline, "static float.IsPositiveInfinity(float)", "(@#{0} === Infinity)")]
	public extern static bool _b2b89b81c87952dc(Number f);

	///<summary>Determines whether the specified value is subnormal.</summary>
	[Jazor(Op.Discard ,"static float.IsSubnormal(float)")]
	public extern static bool _8e1067f50ae732cb(Number f);

	/// <summary>
	/// C#: float.CompareTo(object)
	/// JS: instance - (value ?? 0)
	/// </summary>
	[Jazor(Op.Inline, "float.CompareTo(object)", "(@#{0} - (@#{1} ?? 0))")]
	public extern static Number _0b80f2f2f1a3c1a6(Number instance, object? value);

	/// <summary>
	/// C#: float.CompareTo(float)
	/// JS: instance - value
	/// </summary>
	[Jazor(Op.Inline, "float.CompareTo(float)", "(@#{0} - @#{1})")]
	public extern static Number _f6880f77edc2efe5(Number instance, Number value);

	///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> values are equal.</summary>
	[Jazor(Op.Allowed ,"static float.operator ==(float, float)")]
	public extern static bool _f3cd888d249dd728(Number left, Number right);

	///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> values are not equal.</summary>
	[Jazor(Op.Allowed ,"static float.operator !=(float, float)")]
	public extern static bool _5778f48a657c2a49(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
	[Jazor(Op.Allowed ,"static float.operator <(float, float)")]
	public extern static bool _9b49d03b9cec1f12(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
	[Jazor(Op.Allowed ,"static float.operator >(float, float)")]
	public extern static bool _f640e4a5ea01dafa(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
	[Jazor(Op.Allowed ,"static float.operator <=(float, float)")]
	public extern static bool _a5c15d0a8486be37(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
	[Jazor(Op.Allowed ,"static float.operator >=(float, float)")]
	public extern static bool _de450491712f7a22(Number left, Number right);

	/// <summary>
	/// C#: float.Equals(object)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override float.Equals(object)", "(@#{0} === @#{1})")]
	public extern static bool _eb69b50c7032a809(Number instance, object? obj);

	/// <summary>
	/// C#: float.Equals(float)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "float.Equals(float)", "(@#{0} === @#{1})")]
	public extern static bool _5c45db76bd764c38(Number instance, Number obj);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override float.GetHashCode()")]
	public extern static Number _96e065ea302b67da(Number instance);

	/// <summary>
	/// C#: float.ToString()
	/// JS: instance.toString()
	/// </summary>
	[Jazor(Op.Alias, "override float.ToString()", "toString")]
	public extern static string _a036f8edeee45300(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"float.ToString(System.IFormatProvider)")]
	public extern static string _7343d8ada7c3d925(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"float.ToString(string)")]
	public extern static string _fe0300c4411a1f62(Number instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"float.ToString(string, System.IFormatProvider)")]
	public extern static string _d0d4042bef295e49(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current float number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"float.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _3f2b511e96922b72(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"float.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _bfce4d32c259361c(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: float.Parse(s)
	/// JS: Number(s) with validation
	/// </summary>
	[Jazor(Op.Import, "static float.Parse(string)")]
	public static Number _d0492a7790d81596(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		var trimmed = s.Trim();
		if (trimmed.Length == 0)
			throw new Error("FormatException: The input string was not in a correct format.");
		var result = Number_(trimmed);
		if (IsNaN(result))
			throw new Error($"FormatException: The input string '{s}' was not in a correct format.");
		return result;
	}

	///<summary>Converts the string representation of a number in a specified style to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static float.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _77fa7745f751ec69(string s, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static float.Parse(string, System.IFormatProvider)")]
	public extern static Number _2aab5ef8cfa9accc(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static float.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _cddcce796b50f037(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static float.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _d9762c1528057110(string s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: float.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static float.TryParse(string, out float)")]
	public static Array<object?> _ced8b209dbd75890(string? s, Number result)
	{
		if (s == null || s.Length == 0)
			return [false, 0];
		try
		{
			var trimmed = s.Trim();
			if (trimmed.Length == 0)
				return [false, 0];
			var val = Number_(trimmed);
			if (IsNaN(val))
				return [false, 0];
			return [true, val];
		}
		catch
		{
			return [false, 0];
		}
	}

	///<summary>Converts the string representation of a number in a character span to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(System.ReadOnlySpan<char>, out float)")]
	public extern static Array<object?> _8f337f9f610204bb(string s, Number result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(System.ReadOnlySpan<byte>, out float)")]
	public extern static Array<object?> _35fa5333706d7ec4(Uint8Array utf8Text, Number result);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out float)")]
	public extern static Array<object?> _6b58aaed45e38509(string? s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out float)")]
	public extern static Array<object?> _3a7ff2c98489b96d(string s, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Single" />.</summary>
	[Jazor(Op.Discard ,"float.GetTypeCode()")]
	public extern static System.TypeCode _e38cf33130abe213(Number instance);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Discard ,"static float.IsPow2(float)")]
	public extern static bool _0dcf89ab5d6bd60c(Number value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Discard ,"static float.Log2(float)")]
	public extern static Number _79aeb4d9a5bd7f76(Number value);

	///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
	[Jazor(Op.Discard ,"static float.Exp(float)")]
	public extern static Number _9feb625727b5f8b7(Number x);

	///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
	[Jazor(Op.Discard ,"static float.ExpM1(float)")]
	public extern static Number _225c97db4c06d542(Number x);

	///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
	[Jazor(Op.Discard ,"static float.Exp2(float)")]
	public extern static Number _850a2368fd9ebd00(Number x);

	///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
	[Jazor(Op.Discard ,"static float.Exp2M1(float)")]
	public extern static Number _bea586f79da8325a(Number x);

	///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
	[Jazor(Op.Discard ,"static float.Exp10(float)")]
	public extern static Number _c4a8e15339b99e72(Number x);

	///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
	[Jazor(Op.Discard ,"static float.Exp10M1(float)")]
	public extern static Number _0c886f93ae8f2c80(Number x);

	///<summary>Computes the ceiling of a value.</summary>
	[Jazor(Op.Discard ,"static float.Ceiling(float)")]
	public extern static Number _b6616ccde8acba3f(Number x);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[Jazor(Op.Discard ,"static float.ConvertToInteger<TInteger>(float)")]
	public extern static TInteger _b860c3e3eb3014d6<TInteger>(Number value);

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[Jazor(Op.Discard ,"static float.ConvertToIntegerNative<TInteger>(float)")]
	public extern static TInteger _59f5214dc916fb61<TInteger>(Number value);

	///<summary>Computes the floor of a value.</summary>
	[Jazor(Op.Discard ,"static float.Floor(float)")]
	public extern static Number _32eec2aa95114e61(Number x);

	///<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[Jazor(Op.Discard ,"static float.Round(float)")]
	public extern static Number _99c8e34b34aa762c(Number x);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[Jazor(Op.Discard ,"static float.Round(float, int)")]
	public extern static Number _a0ef44092a5b0a96(Number x, Number digits);

	///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
	[Jazor(Op.Discard ,"static float.Round(float, System.MidpointRounding)")]
	public extern static Number _34bdf4b36464daa4(Number x, object mode);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[Jazor(Op.Discard ,"static float.Round(float, int, System.MidpointRounding)")]
	public extern static Number _b0f1294dc766b202(Number x, Number digits, object mode);

	///<summary>Truncates a value.</summary>
	[Jazor(Op.Discard ,"static float.Truncate(float)")]
	public extern static Number _60637f5113854841(Number x);

	///<summary>Computes the arc-tangent of the quotient of two values.</summary>
	[Jazor(Op.Discard ,"static float.Atan2(float, float)")]
	public extern static Number _81fb32cf771b3b93(Number y, Number x);

	///<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Discard ,"static float.Atan2Pi(float, float)")]
	public extern static Number _6af9ae0f6ba947de(Number y, Number x);

	///<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
	[Jazor(Op.Discard ,"static float.BitDecrement(float)")]
	public extern static Number _9840b2a560428b4a(Number x);

	///<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
	[Jazor(Op.Discard ,"static float.BitIncrement(float)")]
	public extern static Number _eac91380a48fb7bd(Number x);

	///<summary>Computes the fused multiply-add of three values.</summary>
	[Jazor(Op.Discard ,"static float.FusedMultiplyAdd(float, float, float)")]
	public extern static Number _aff67a0c1864d405(Number left, Number right, Number addend);

	///<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
	[Jazor(Op.Discard ,"static float.Ieee754Remainder(float, float)")]
	public extern static Number _e54bb5d6b1fb386d(Number left, Number right);

	///<summary>Computes the integer logarithm of a value.</summary>
	[Jazor(Op.Discard ,"static float.ILogB(float)")]
	public extern static Number _390f9dfb01584a29(Number x);

	///<summary>Performs a linear interpolation between two values based on the given weight.</summary>
	[Jazor(Op.Discard ,"static float.Lerp(float, float, float)")]
	public extern static Number _9784f111f543c6ac(Number value1, Number value2, Number amount);

	///<summary>Computes an estimate of the reciprocal of a value.</summary>
	[Jazor(Op.Discard ,"static float.ReciprocalEstimate(float)")]
	public extern static Number _9a007a301b9dabab(Number x);

	///<summary>Computes an estimate of the reciprocal square root of a value.</summary>
	[Jazor(Op.Discard ,"static float.ReciprocalSqrtEstimate(float)")]
	public extern static Number _4ede4daffe897997(Number x);

	///<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
	[Jazor(Op.Discard ,"static float.ScaleB(float, int)")]
	public extern static Number _9019f10f92f8729e(Number x, Number n);

	///<summary>Computes the hyperbolic arc-cosine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Acosh(float)")]
	public extern static Number _85424839a031a4b7(Number x);

	///<summary>Computes the hyperbolic arc-sine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Asinh(float)")]
	public extern static Number _e6b2592394f1870f(Number x);

	///<summary>Computes the hyperbolic arc-tangent of a value.</summary>
	[Jazor(Op.Discard ,"static float.Atanh(float)")]
	public extern static Number _3d792e12600731b6(Number x);

	///<summary>Computes the hyperbolic cosine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Cosh(float)")]
	public extern static Number _530f9f361ebd69d6(Number x);

	///<summary>Computes the hyperbolic sine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Sinh(float)")]
	public extern static Number _5ebfd243857a3667(Number x);

	///<summary>Computes the hyperbolic tangent of a value.</summary>
	[Jazor(Op.Discard ,"static float.Tanh(float)")]
	public extern static Number _54702f47ad6c11df(Number x);

	///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
	[Jazor(Op.Discard ,"static float.Log(float)")]
	public extern static Number _0311a212e027ef2d(Number x);

	///<summary>Computes the logarithm of a value in the specified base.</summary>
	[Jazor(Op.Discard ,"static float.Log(float, float)")]
	public extern static Number _2346aa8a14187816(Number x, Number newBase);

	///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
	[Jazor(Op.Discard ,"static float.LogP1(float)")]
	public extern static Number _375f5e807e36cf8a(Number x);

	///<summary>Computes the base-10 logarithm of a value.</summary>
	[Jazor(Op.Discard ,"static float.Log10(float)")]
	public extern static Number _13b3c426479d8061(Number x);

	///<summary>Computes the base-2 logarithm of a value plus one.</summary>
	[Jazor(Op.Discard ,"static float.Log2P1(float)")]
	public extern static Number _320a7a02cb084671(Number x);

	///<summary>Computes the base-10 logarithm of a value plus one.</summary>
	[Jazor(Op.Discard ,"static float.Log10P1(float)")]
	public extern static Number _9025daef4465a5f4(Number x);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static float.Clamp(float, float, float)")]
	public extern static Number _fa04e6b14ed00f24(Number value, Number min, Number max);

	[Jazor(Op.Discard ,"static float.ClampNative(float, float, float)")]
	public extern static Number _e50ccb4182ec0a52(Number value, Number min, Number max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Discard ,"static float.CopySign(float, float)")]
	public extern static Number _959cd3c9f503af65(Number value, Number sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static float.Max(float, float)")]
	public extern static Number _b4d95f21e04b4768(Number x, Number y);

	[Jazor(Op.Discard ,"static float.MaxNative(float, float)")]
	public extern static Number _6f3b48cdfa90d3a2(Number x, Number y);

	///<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Discard ,"static float.MaxNumber(float, float)")]
	public extern static Number _3c8d94a02631a0b0(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static float.Min(float, float)")]
	public extern static Number _f0e565231f96990c(Number x, Number y);

	[Jazor(Op.Discard ,"static float.MinNative(float, float)")]
	public extern static Number _334fae190a459e2d(Number x, Number y);

	///<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Discard ,"static float.MinNumber(float, float)")]
	public extern static Number _6bf468999b5de10e(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static float.Sign(float)")]
	public extern static Number _323a6b94e62b2729(Number value);

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Discard ,"static float.Abs(float)")]
	public extern static Number _a520369f28d7dc89(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static float.CreateChecked<TOther>(TOther)")]
	public extern static Number _687013ac9f43fbe4<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static float.CreateSaturating<TOther>(TOther)")]
	public extern static Number _21f779ed6ef58263<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static float.CreateTruncating<TOther>(TOther)")]
	public extern static Number _098c80c8c595a04e<TOther>(object value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static float.IsEvenInteger(float)")]
	public extern static bool _c74cdf25f3c81cf5(Number value);

	///<summary>Determines if a value represents an integral value.</summary>
	[Jazor(Op.Discard ,"static float.IsInteger(float)")]
	public extern static bool _b330185da27a9f39(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static float.IsOddInteger(float)")]
	public extern static bool _071c1156cfc9bd2f(Number value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Discard ,"static float.IsPositive(float)")]
	public extern static bool _aac0109c854f99d4(Number value);

	///<summary>Determines if a value represents a real number.</summary>
	[Jazor(Op.Discard ,"static float.IsRealNumber(float)")]
	public extern static bool _9966e18806e99046(Number value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static float.MaxMagnitude(float, float)")]
	public extern static Number _7c146ff0a50e958f(Number x, Number y);

	///<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Discard ,"static float.MaxMagnitudeNumber(float, float)")]
	public extern static Number _b7b1d7781578b7e0(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static float.MinMagnitude(float, float)")]
	public extern static Number _e5a7b14f707c69f7(Number x, Number y);

	///<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Discard ,"static float.MinMagnitudeNumber(float, float)")]
	public extern static Number _4a2ec5d010e27cb1(Number x, Number y);

	///<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
	[Jazor(Op.Discard ,"static float.MultiplyAddEstimate(float, float, float)")]
	public extern static Number _0790dc6c4730eb68(Number left, Number right, Number addend);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(string, System.IFormatProvider, out float)")]
	public extern static Array<object?> _c6cd666235929784(string? s, Intl.NumberFormat? provider, Number result);

	///<summary>Computes a value raised to a given power.</summary>
	[Jazor(Op.Discard ,"static float.Pow(float, float)")]
	public extern static Number _9dea84f9daad7225(Number x, Number y);

	///<summary>Computes the cube-root of a value.</summary>
	[Jazor(Op.Discard ,"static float.Cbrt(float)")]
	public extern static Number _51ff1f64e04042ff(Number x);

	///<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
	[Jazor(Op.Discard ,"static float.Hypot(float, float)")]
	public extern static Number _76c7c7ae956d3449(Number x, Number y);

	///<summary>Computes the n-th root of a value.</summary>
	[Jazor(Op.Discard ,"static float.RootN(float, int)")]
	public extern static Number _9a3da74ee8bdf7c6(Number x, Number n);

	///<summary>Computes the square-root of a value.</summary>
	[Jazor(Op.Discard ,"static float.Sqrt(float)")]
	public extern static Number _daecc788f9d305e5(Number x);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static float.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _347eb552b6176fde(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out float)")]
	public extern static Array<object?> _c3b1663d39b1d889(string s, Intl.NumberFormat? provider, Number result);

	///<summary>Computes the arc-cosine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Acos(float)")]
	public extern static Number _fff14793e0685103(Number x);

	///<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Discard ,"static float.AcosPi(float)")]
	public extern static Number _b3cd206da76e2588(Number x);

	///<summary>Computes the arc-sine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Asin(float)")]
	public extern static Number _753afad06a77a6ce(Number x);

	///<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Discard ,"static float.AsinPi(float)")]
	public extern static Number _5f4c7e35877dc08c(Number x);

	///<summary>Computes the arc-tangent of a value.</summary>
	[Jazor(Op.Discard ,"static float.Atan(float)")]
	public extern static Number _d91bd1cce9c18aa3(Number x);

	///<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
	[Jazor(Op.Discard ,"static float.AtanPi(float)")]
	public extern static Number _4ba0e55e748cdc42(Number x);

	///<summary>Computes the cosine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Cos(float)")]
	public extern static Number _aef0ed870d0a4481(Number x);

	///<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Discard ,"static float.CosPi(float)")]
	public extern static Number _8901cace41b16205(Number x);

	///<summary>Converts a given value from degrees to radians.</summary>
	[Jazor(Op.Discard ,"static float.DegreesToRadians(float)")]
	public extern static Number _5973d9c23e108b1b(Number degrees);

	///<summary>Converts a given value from radians to degrees.</summary>
	[Jazor(Op.Discard ,"static float.RadiansToDegrees(float)")]
	public extern static Number _b67d60ab600d4498(Number radians);

	///<summary>Computes the sine of a value.</summary>
	[Jazor(Op.Discard ,"static float.Sin(float)")]
	public extern static Number _28ff5aa7214bc112(Number x);

	///<summary>Computes the sine and cosine of a value.</summary>
	[Jazor(Op.Discard ,"static float.SinCos(float)")]
	public extern static (float Sin, float Cos) _9905e3952bca67bc(Number x);

	///<summary>Computes the sine and cosine of a value.</summary>
	[Jazor(Op.Discard ,"static float.SinCosPi(float)")]
	public extern static (float SinPi, float CosPi) _2c792a5d6ef88cd1(Number x);

	///<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Discard ,"static float.SinPi(float)")]
	public extern static Number _2d3a8b418dbab013(Number x);

	///<summary>Computes the tangent of a value.</summary>
	[Jazor(Op.Discard ,"static float.Tan(float)")]
	public extern static Number _c379df7d9fb9a3bd(Number x);

	///<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Discard ,"static float.TanPi(float)")]
	public extern static Number _7775a2adde710e31(Number x);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static float.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _5d3787482806eeab(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out float)")]
	public extern static Array<object?> _b381be81bd5cd295(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static float.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _3d54467f93f0838e(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static float.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out float)")]
	public extern static Array<object?> _e76b3bd6230a30ba(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
