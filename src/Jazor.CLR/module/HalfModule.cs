namespace Jazor.CLR;

/// <summary>
/// System.Half 映射为 JavaScript Number；通用浮点判定与 DoubleModule 共用核心。
/// </summary>
/// <remarks>
/// Number 只是 Half 的无损 carrier。所有产生新 Half 值的运算必须经过 Math.f16round，
/// 不能把 binary64 中间结果直接暴露为 Half，否则后续比较和成员调用都会观察到错误精度。
/// </remarks>
[ECMAScriptModule("System/HalfModule.js")]
[Jazor(Op.Alias, "System.Half", "Number")]
public static class HalfModule
{
	private static Number RoundToHalf(Number value)
		=> Math.F16roundFn(value);

	internal static Number FromBigIntCore(BigInt value)
		=> RoundToHalf(NumberFn(value));

	private static Number CheckedToNumberCore(Number value, Number min, Number max)
	{
		if (!DoubleModule.IsFiniteCore(value))
			throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");

		// Checked floating conversions validate the truncated integer, so -0.5 can become unsigned zero.
		var truncated = Math.TruncFn(value);
		if (truncated < min || truncated > max)
			throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");

		return truncated == 0 ? 0 : truncated;
	}

	private static Number UncheckedToInt32Core(Number value)
	{
		if (IsNaN(value))
			return 0;
		if (value >= 2147483647)
			return 2147483647;
		if (value <= -2147483648)
			return -2147483648;

		var truncated = Math.TruncFn(value);
		return truncated == 0 ? 0 : truncated;
	}

	private static Number UncheckedToNarrowCore(Number value, Number width, bool signed)
	{
		var integer = BigIntFn(UncheckedToInt32Core(value));
		return NumberFn(signed ? BigInt.AsIntN(width, integer) : BigInt.AsUintN(width, integer));
	}

	private static Number UncheckedToUnsignedNumberCore(Number value, Number max)
	{
		if (IsNaN(value) || value <= 0)
			return 0;
		if (value >= max)
			return max;

		return Math.TruncFn(value);
	}

	private static Number RoundToEven(Number value)
	{
		if (!DoubleModule.IsFiniteCore(value) || value == 0)
			return value;

		var floor = Math.FloorFn(value);
		var fraction = value - floor;
		var rounded = fraction < 0.5
			? floor
			: (fraction > 0.5 ? floor + 1 : (floor % 2 == 0 ? floor : floor + 1));

		return rounded == 0 && value < 0 ? -0 : rounded;
	}

	private static Number Ieee754RemainderCore(Number left, Number right)
	{
		if (IsNaN(left))
			return left;
		if (IsNaN(right))
			return right;

		var regular = RoundToHalf(left % right);
		if (IsNaN(regular))
			return Number.NaN;
		if (regular == 0)
			return left < 0 || Object.Is(left, -0) ? -0 : 0;

		var alternative = RoundToHalf(regular - Math.AbsFn(right) * (left < 0 ? -1 : 1));
		var regularMagnitude = Math.AbsFn(regular);
		var alternativeMagnitude = Math.AbsFn(alternative);
		if (alternativeMagnitude == regularMagnitude)
		{
			var quotient = RoundToHalf(left / right);
			return Math.AbsFn(RoundToEven(quotient)) > Math.AbsFn(quotient)
				? alternative
				: regular;
		}

		return alternativeMagnitude < regularMagnitude ? alternative : regular;
	}

	private static Number ILogBCore(Number value)
	{
		if (IsNaN(value) || !DoubleModule.IsFiniteCore(value))
			return 2147483647;
		if (value == 0)
			return -2147483648;

		var magnitudeBits = GetHalfBitsCore(value) % 32768d;
		var exponentBits = Math.FloorFn(magnitudeBits / 1024d);
		if (exponentBits != 0)
			return exponentBits - 15;

		return HighestSetBitCore(magnitudeBits % 1024d) - 24;
	}

	private static Number BitIncrementCore(Number value)
		=> OffsetAdjacentCore(value, true);

	private static Number BitDecrementCore(Number value)
		=> OffsetAdjacentCore(value, false);

	private static Number OffsetAdjacentCore(Number value, bool increment)
	{
		if (IsNaN(value))
			return value;
		if (increment && value == Number.POSITIVE_INFINITY)
			return value;
		if (!increment && value == Number.NEGATIVE_INFINITY)
			return value;
		if (value == 0)
			return increment ? 5.960464477539063e-8 : -5.960464477539063e-8;

		var bits = GetHalfBitsCore(value);
		var increaseBits = increment ? value > 0 : value < 0;
		return FromHalfBitsCore(increaseBits ? bits + 1 : bits - 1);
	}

	private static Number GetHalfBitsCore(Number value)
	{
		var negative = value < 0 || Object.Is(value, -0);
		var signBits = negative ? 32768 : 0;
		var magnitude = Math.AbsFn(value);
		if (magnitude == Number.POSITIVE_INFINITY)
			return signBits + 31744;
		if (magnitude == 0)
			return signBits;
		if (magnitude < 0.00006103515625)
			return signBits + Math.FloorFn(magnitude / 5.960464477539063e-8 + 0.5);

		var exponent = -14;
		var scale = 0.00006103515625;
		while (magnitude >= scale * 2)
		{
			scale *= 2;
			exponent++;
		}

		var mantissa = Math.FloorFn((magnitude / scale - 1) * 1024 + 0.5);
		return signBits + (exponent + 15) * 1024 + mantissa;
	}

	private static Number FromHalfBitsCore(Number bits)
	{
		var negative = bits >= 32768;
		var magnitudeBits = bits % 32768d;
		var exponentBits = Math.FloorFn(magnitudeBits / 1024d);
		var mantissa = magnitudeBits % 1024d;
		Number value;
		if (exponentBits == 0)
		{
			value = mantissa * 5.960464477539063e-8;
		}
		else if (exponentBits == 31)
		{
			value = mantissa == 0 ? Number.POSITIVE_INFINITY : Number.NaN;
		}
		else
		{
			value = (1 + mantissa / 1024d) * Math.PowFn(2, exponentBits - 15);
		}

		return negative ? -value : value;
	}

	private static Number HighestSetBitCore(Number value)
	{
		var bit = -1;
		while (value > 0)
		{
			value = Math.FloorFn(value / 2);
			bit++;
		}

		return bit;
	}

	private static Number ClampCore(Number value, Number min, Number max)
	{
		if (min > max)
			throw new Error("ArgumentException: 'min' cannot be greater than max.");

		return value < min ? min : (value > max ? max : value);
	}

	private static Number MaxNativeCore(Number left, Number right)
		=> left > right ? left : right;

	private static Number MinNativeCore(Number left, Number right)
		=> left < right ? left : right;

	private static Number ClampNativeCore(Number value, Number min, Number max)
	{
		if (min > max)
			throw new Error("ArgumentException: 'min' cannot be greater than max.");

		return MinNativeCore(MaxNativeCore(value, min), max);
	}

	private static Number RootNCore(Number value, Number degree)
	{
		if (degree == 0)
			return Number.NaN;

		var oddDegree = degree % 2 != 0;
		// -0 is valid for even roots and must become +0; finite negative values and -Infinity are not.
		if (value < 0 && !oddDegree)
			return Number.NaN;

		var magnitude = Math.PowFn(Math.AbsFn(value), 1 / degree);
		var negativeResult = oddDegree && (value < 0 || Object.Is(value, -0));
		return RoundToHalf(negativeResult ? -magnitude : magnitude);
	}

	[Jazor(Op.Inline ,"System.Half.Half()", "0")]
	public extern static Number _e57fa2730afaf850();

	[Jazor(Op.Inline, "static System.Half.Epsilon.get", "5.960464477539063e-8")]
	public extern static Number _990c1ec7efa66459();

	[Jazor(Op.Inline, "static System.Half.PositiveInfinity.get", "Infinity")]
	public extern static Number _1a0e55150cfa2b66();

	[Jazor(Op.Inline, "static System.Half.NegativeInfinity.get", "-Infinity")]
	public extern static Number _cb905a45a190dce2();

	[Jazor(Op.Inline, "static System.Half.NaN.get", "NaN")]
	public extern static Number _cd941d11126794b5();

	[Jazor(Op.Inline, "static System.Half.MinValue.get", "-65504")]
	public extern static Number _98f36b18cc555ccd();

	[Jazor(Op.Inline, "static System.Half.MaxValue.get", "65504")]
	public extern static Number _952cdc80015e32d8();

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> value.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator <(System.Half, System.Half)")]
	public extern static bool _f5c54d178e728de9(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> value.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator >(System.Half, System.Half)")]
	public extern static bool _442eb72d4033f9b3(Number left, Number right);

	///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> value.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator <=(System.Half, System.Half)")]
	public extern static bool _bd81927abe75c102(Number left, Number right);

	///<summary>Returns a value that indicates whether <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator >=(System.Half, System.Half)")]
	public extern static bool _0ae9759d125b80d0(Number left, Number right);

	///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> values are equal.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator ==(System.Half, System.Half)")]
	public extern static bool _922b69707d651de5(Number left, Number right);

	///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Half"></xref> values are not equal.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator !=(System.Half, System.Half)")]
	public extern static bool _d6f2d8118ad5aac4(Number left, Number right);

	///<summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
	[Jazor(Op.Inline, "static System.Half.IsFinite(System.Half)", "Number.isFinite(__arg1)")]
	public extern static bool _e583de2c2947a17f(Number value);

	///<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
	[Jazor(Op.Inline, "static System.Half.IsInfinity(System.Half)", "(__arg1 === Infinity || __arg1 === -Infinity)")]
	public extern static bool _4b18a113ae7fdbf8(Number value);

	///<summary>Determines whether the specified value is not a number.</summary>
	[Jazor(Op.Inline, "static System.Half.IsNaN(System.Half)", "Number.isNaN(__arg1)")]
	public extern static bool _7f65df9e61664e57(Number value);

	///<summary>Determines whether the specified value is negative.</summary>
	[Jazor(Op.Inline, "static System.Half.IsNegative(System.Half)", "(Object.is(__arg1, -0) || __arg1 < 0)")]
	public extern static bool _e15882dda45e1796(Number value);

	///<summary>Determines whether the specified value is negative infinity.</summary>
	[Jazor(Op.Inline, "static System.Half.IsNegativeInfinity(System.Half)", "(__arg1 === -Infinity)")]
	public extern static bool _bed468560246cfd9(Number value);

	///<summary>Determines whether the specified value is normal.</summary>
	[Jazor(Op.Inline, "static System.Half.IsNormal(System.Half)", "(Number.isFinite(__arg1) && __arg1 !== 0 && Math.abs(__arg1) >= 0.00006103515625)")]
	public extern static bool _87aae9c40daa22fe(Number value);

	///<summary>Determines whether the specified value is positive infinity.</summary>
	[Jazor(Op.Inline, "static System.Half.IsPositiveInfinity(System.Half)", "(__arg1 === Infinity)")]
	public extern static bool _4449149a535d0a52(Number value);

	///<summary>Determines whether the specified value is subnormal.</summary>
	[Jazor(Op.Inline, "static System.Half.IsSubnormal(System.Half)", "(Number.isFinite(__arg1) && __arg1 !== 0 && Math.abs(__arg1) < 0.00006103515625)")]
	public extern static bool _3609a09877199072(Number value);

	///<summary>Converts the string representation of a number to its half-precision floating-point number equivalent.</summary>
	[Jazor(Op.Import, "static System.Half.Parse(string)")]
	public static Number _14d80007aa3543a1(string text)
	{
		if (text == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		if (!DoubleModule.TryParseCore(text, out var value))
			throw new Error($"FormatException: The input string '{text}' was not in a correct format.");

		return RoundToHalf(value);
	}

	///<summary>Converts the string representation of a number in a specified style to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Half.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _eb141686844d686e(string s, global::System.Globalization.NumberStyles style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Import, "static System.Half.Parse(string, System.IFormatProvider)")]
	public static Number _92b036ecc84de08d(string text, Intl.NumberFormat? provider)
		=> _14d80007aa3543a1(text);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Half.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _d2ce9dd0a5bd92a1(string s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Half.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _1ff73be01fb3f0ef(string s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its half-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import, "static System.Half.TryParse(string, out System.Half)")]
	public static Array<object?> _83de0b9fe4433805(string? text, Number result)
	{
		if (!DoubleModule.TryParseCore(text, out var value))
			return [false, 0];

		return [true, RoundToHalf(value)];
	}

	///<summary>Converts the span representation of a number to its half-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static System.Half.TryParse(System.ReadOnlySpan<char>, out System.Half)")]
	public static Array<object?> _f5bea48e2d45cf92(string s, Number result)
		=> _83de0b9fe4433805(s, result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its half-precision floating-point number equivalent.</summary>
	[Jazor(Op.Import, "static System.Half.TryParse(System.ReadOnlySpan<byte>, out System.Half)")]
	public static Array<object?> _8ed5272b36771f32(Uint8Array utf8Text, Number result)
		=> _83de0b9fe4433805(RuntimeModule.TryDecodeUtf8(utf8Text), result);

	///<summary>Converts the string representation of a number to its half-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static System.Half.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Half)")]
	public extern static Array<object?> _3399b4ed1bac682f(string? s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider, Number result);

	///<summary>Converts the span representation of a number to its half-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static System.Half.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Half)")]
	public extern static Array<object?> _d2f9d884842d79ba(string s, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider, Number result);

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	[Jazor(Op.Import, "System.Half.CompareTo(object)")]
	public static Number _8a86be5e4541e5ce(Number instance, object? value)
	{
		if (value == null)
			return 1;
		if (TypeOf(value) != "number")
			throw new Error("ArgumentException: Object must be of type Half.");

		return DoubleModule.CompareCore(instance, (Number)value);
	}

	///<summary>Compares this instance to a specified half-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified half-precision floating-point number.</summary>
	[Jazor(Op.Inline, "System.Half.CompareTo(System.Half)", "(isNaN(__arg1) ? (isNaN(__arg2) ? 0 : -1) : (isNaN(__arg2) ? 1 : (__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))))")]
	public extern static Number _30a60931259ef751(Number instance, Number value);

	///<summary>Returns a value that indicates whether this instance is equal to the specified <paramref name="obj" />.</summary>
	[Jazor(Op.Import, "override System.Half.Equals(object)")]
	public static bool _3a07dad87c237b05(Number instance, object? value)
	{
		if (value == null || TypeOf(value) != "number")
			return false;

		return DoubleModule.AreEqualCore(instance, (Number)value);
	}

	///<summary>Compares this instance for equality with <paramref name="other" />.</summary>
	[Jazor(Op.Inline, "System.Half.Equals(System.Half)", "(isNaN(__arg1) ? isNaN(__arg2) : (isNaN(__arg2) ? false : (!(__arg1 < __arg2) && !(__arg1 > __arg2))))")]
	public extern static bool _0b8445daba1707fb(Number instance, Number value);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import, "override System.Half.GetHashCode()")]
	public static Number _f9dc2d5b5c5cdf31(Number instance)
		=> EqualityComparerT1Module<Number>.GetHashCodeCore(instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[Jazor(Op.Alias, "override System.Half.ToString()", "toString")]
	public extern static string _226244bc21ab60b9(Number instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[Jazor(Op.Discard ,"System.Half.ToString(string)")]
	public extern static string _dbdd0547f7a51c3c(Number instance, string? format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.Half.ToString(System.IFormatProvider)")]
	public extern static string _206be385ee011421(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.Half.ToString(string, System.IFormatProvider)")]
	public extern static string _df4de26c47f332f0(Number instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current <see cref="System.Half" /> instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.Half.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _e6f31bc1ca4a91b6(Number instance, string destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.Half.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _ea46d9b736df7a30(Number instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Explicitly converts a <see cref="T:System.Char" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(char)")]
	public static Number _688015ce7a06d3a3(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.Decimal" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Discard ,"static System.Half.explicit operator System.Half(decimal)")]
	public extern static Number _e01ce2a92bbacdf2(string value);

	///<summary>An explicit operator to convert a <see cref="T:System.Double" /> value to a <see cref="T:System.Half" />.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(double)")]
	public static Number _c15dbcdc3a5121a4(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.Int16" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(short)")]
	public static Number _5235d3bf6d040ead(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.Int32" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(int)")]
	public static Number _83d328837e0849f2(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.Int64" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(long)")]
	public static Number _54cc35a643b3964a(BigInt value)
		=> FromBigIntCore(value);

	///<summary>Explicitly converts a <see cref="T:System.IntPtr" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Discard ,"static System.Half.explicit operator System.Half(nint)")]
	public extern static Number _5ce9b896defd51a4(nint value);

	///<summary>An explicit operator to convert a <see cref="T:System.Single" /> value to a <see cref="T:System.Half" />.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(float)")]
	public static Number _c698784c1b652292(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.UInt16" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(ushort)")]
	public static Number _66978b13cd9c4d2c(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.UInt32" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(uint)")]
	public static Number _5fe8cbd0191a1261(Number value)
		=> RoundToHalf(value);

	///<summary>Explicitly converts a <see cref="T:System.UInt64" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Half(ulong)")]
	public static Number _7cde86a6784147b9(BigInt value)
		=> FromBigIntCore(value);

	///<summary>Explicitly converts a <see cref="T:System.UIntPtr" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Discard ,"static System.Half.explicit operator System.Half(nuint)")]
	public extern static Number _dc71056543f828a2(nuint value);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Byte" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator byte(System.Half)")]
	public static Number _4eda3983a0238fe6(Number value)
		=> UncheckedToNarrowCore(value, 8, false);

	[Jazor(Op.Import, "static System.Half.explicit operator checked byte(System.Half)")]
	public static Number _17127d121cc23462(Number value)
		=> CheckedToNumberCore(value, 0, 255);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Char" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator char(System.Half)")]
	public static Number _a51addf0541517b0(Number value)
		=> UncheckedToNarrowCore(value, 16, false);

	[Jazor(Op.Import, "static System.Half.explicit operator checked char(System.Half)")]
	public static Number _0ce814bef1ddcd6b(Number value)
		=> CheckedToNumberCore(value, 0, 65535);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Half.explicit operator decimal(System.Half)")]
	public extern static string _e7a6ea38b3750a1b(Number value);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Int16" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator short(System.Half)")]
	public static Number _f3478913297420e6(Number value)
		=> UncheckedToNarrowCore(value, 16, true);

	[Jazor(Op.Import, "static System.Half.explicit operator checked short(System.Half)")]
	public static Number _a97f96a06c928768(Number value)
		=> CheckedToNumberCore(value, -32768, 32767);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Int32" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator int(System.Half)")]
	public static Number _b72c1f59dbe70e00(Number value)
		=> UncheckedToInt32Core(value);

	[Jazor(Op.Import, "static System.Half.explicit operator checked int(System.Half)")]
	public static Number _70697b238a197bc2(Number value)
		=> CheckedToNumberCore(value, -2147483648, 2147483647);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Int64" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator long(System.Half)")]
	public static BigInt _1d590a5b31b1ced4(Number value)
		=> BigIntIntegerRuntime.FromFloatingSaturatingSigned(
			value,
			BigIntFn("-9223372036854775808"),
			BigIntFn("9223372036854775807"));

	[Jazor(Op.Import, "static System.Half.explicit operator checked long(System.Half)")]
	public static BigInt _b245ca9db3ecb868(Number value)
		=> BigIntIntegerRuntime.FromFloatingChecked(
			value,
			BigIntFn("-9223372036854775808"),
			BigIntFn("9223372036854775807"));

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.Int128" />.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.Int128(System.Half)")]
	public static BigInt _24b890794cafdd5b(Number value)
		=> BigIntIntegerRuntime.FromFloatingSaturatingSigned(
			value,
			BigIntFn("-170141183460469231731687303715884105728"),
			BigIntFn("170141183460469231731687303715884105727"));

	[Jazor(Op.Import, "static System.Half.explicit operator checked System.Int128(System.Half)")]
	public static BigInt _ad10a10a383b6b8c(Number value)
		=> BigIntIntegerRuntime.FromFloatingChecked(
			value,
			BigIntFn("-170141183460469231731687303715884105728"),
			BigIntFn("170141183460469231731687303715884105727"));

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.IntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Half.explicit operator nint(System.Half)")]
	public extern static nint _5701bcbe09e64082(Number value);

	[Jazor(Op.Discard ,"static System.Half.explicit operator checked nint(System.Half)")]
	public extern static nint _408c2eab0d0d5948(Number value);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.SByte" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator sbyte(System.Half)")]
	public static Number _0c7451f23f55d772(Number value)
		=> UncheckedToNarrowCore(value, 8, true);

	[Jazor(Op.Import, "static System.Half.explicit operator checked sbyte(System.Half)")]
	public static Number _d68498a3229ff278(Number value)
		=> CheckedToNumberCore(value, -128, 127);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.UInt16" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator ushort(System.Half)")]
	public static Number _5506dadf5b952671(Number value)
		=> UncheckedToNarrowCore(value, 16, false);

	[Jazor(Op.Import, "static System.Half.explicit operator checked ushort(System.Half)")]
	public static Number _d7ccb4b5709ce4ea(Number value)
		=> CheckedToNumberCore(value, 0, 65535);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.UInt32" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator uint(System.Half)")]
	public static Number _6d14496c702de03c(Number value)
		=> UncheckedToUnsignedNumberCore(value, 4294967295);

	[Jazor(Op.Import, "static System.Half.explicit operator checked uint(System.Half)")]
	public static Number _8e635ebf316e6be7(Number value)
		=> CheckedToNumberCore(value, 0, 4294967295);

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.UInt64" /> value.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator ulong(System.Half)")]
	public static BigInt _368654d3a116fc21(Number value)
		=> BigIntIntegerRuntime.FromFloatingSaturatingUnsigned(value, BigIntFn("18446744073709551615"));

	[Jazor(Op.Import, "static System.Half.explicit operator checked ulong(System.Half)")]
	public static BigInt _8d52fe89e6ca9452(Number value)
		=> BigIntIntegerRuntime.FromFloatingChecked(value, BigInt.Zero, BigIntFn("18446744073709551615"));

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.UInt128" />.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator System.UInt128(System.Half)")]
	public static BigInt _de1cee73a929bf8e(Number value)
		=> BigIntIntegerRuntime.FromFloatingSaturatingUnsigned(
			value,
			BigIntFn("340282366920938463463374607431768211455"));

	[Jazor(Op.Import, "static System.Half.explicit operator checked System.UInt128(System.Half)")]
	public static BigInt _bd3cc1c48165dbab(Number value)
		=> BigIntIntegerRuntime.FromFloatingCheckedUInt128(
			value,
			BigIntFn("340282366920938463463374607431768211455"));

	///<summary>Explicitly converts a half-precision floating-point value to its nearest representable <see cref="T:System.UIntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Half.explicit operator nuint(System.Half)")]
	public extern static nuint _a3f18a071db79160(Number value);

	[Jazor(Op.Discard ,"static System.Half.explicit operator checked nuint(System.Half)")]
	public extern static nuint _bb3b3896ffce705a(Number value);

	///<summary>Implicitly converts a <see cref="T:System.Byte" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.implicit operator System.Half(byte)")]
	public static Number _b5ec2ce7adbc5cd7(Number value)
		=> RoundToHalf(value);

	///<summary>Implicitly converts a <see cref="T:System.SByte" /> value to its nearest representable half-precision floating-point value.</summary>
	[Jazor(Op.Import, "static System.Half.implicit operator System.Half(sbyte)")]
	public static Number _e9ab5db75451afaa(Number value)
		=> RoundToHalf(value);

	///<summary>An explicit operator to convert a <see cref="T:System.Half" /> value to a <see cref="T:System.Double" />.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator double(System.Half)")]
	public static Number _0cce99536d7741bb(Number value)
		=> value;

	///<summary>An explicit operator to convert a <see cref="T:System.Half" /> value to a <see cref="T:System.Single" />.</summary>
	[Jazor(Op.Import, "static System.Half.explicit operator float(System.Half)")]
	public static Number _e5c3410a6fc7ae9a(Number value)
		=> value;

	///<summary>Adds two values together to compute their sum.</summary>
	[Jazor(Op.Inline, "static System.Half.operator +(System.Half, System.Half)", "Math.f16round(__arg1 + __arg2)")]
	public extern static Number _15e83b166d64a525(Number left, Number right);

	///<summary>Determines if a value is a power of two.</summary>
	[Jazor(Op.Import, "static System.Half.IsPow2(System.Half)")]
	public static bool _8b5f0cb98ef4522c(Number value)
		=> DoubleModule.IsPow2Core(value);

	///<summary>Computes the log2 of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Log2(System.Half)", "Math.f16round(Math.log2(__arg1))")]
	public extern static Number _50e42d91eadd858c(Number value);

	///<summary>Decrements a value.</summary>
	[Jazor(Op.Inline, "static System.Half.operator --(System.Half)", "Math.f16round(__arg1 - 1)")]
	public extern static Number _21dce893594e6a88(Number value);

	///<summary>Divides two values together to compute their quotient.</summary>
	[Jazor(Op.Inline, "static System.Half.operator /(System.Half, System.Half)", "Math.f16round(__arg1 / __arg2)")]
	public extern static Number _31bd8106aec1d697(Number left, Number right);

	///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
	[Jazor(Op.Inline, "static System.Half.Exp(System.Half)", "Math.f16round(Math.exp(__arg1))")]
	public extern static Number _a7fa6fea71e1af01(Number value);

	///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
	[Jazor(Op.Inline, "static System.Half.ExpM1(System.Half)", "Math.f16round(Math.exp(__arg1) - 1)")]
	public extern static Number _07444142f58d0e8b(Number value);

	///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
	[Jazor(Op.Inline, "static System.Half.Exp2(System.Half)", "Math.f16round(Math.pow(2, __arg1))")]
	public extern static Number _e8d9bbc26b41707d(Number value);

	///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
	[Jazor(Op.Inline, "static System.Half.Exp2M1(System.Half)", "Math.f16round(Math.pow(2, __arg1) - 1)")]
	public extern static Number _538126e3a652c9d4(Number value);

	///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
	[Jazor(Op.Inline, "static System.Half.Exp10(System.Half)", "Math.f16round(Math.pow(10, __arg1))")]
	public extern static Number _530bc0e2964110b9(Number value);

	///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
	[Jazor(Op.Inline, "static System.Half.Exp10M1(System.Half)", "Math.f16round(Math.pow(10, __arg1) - 1)")]
	public extern static Number _eb941b49b9a77bf8(Number value);

	///<summary>Computes the ceiling of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Ceiling(System.Half)", "Math.ceil(__arg1)")]
	public extern static Number _5f9505b255a08188(Number value);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[Jazor(Op.Discard ,"static System.Half.ConvertToInteger<TInteger>(System.Half)")]
	public extern static TInteger _b788b9aa0fe2ff4c<TInteger>(Number value)		where TInteger : global::System.Numerics.IBinaryInteger<TInteger>;

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[Jazor(Op.Discard ,"static System.Half.ConvertToIntegerNative<TInteger>(System.Half)")]
	public extern static TInteger _e9ab341ff9fe6331<TInteger>(Number value)		where TInteger : global::System.Numerics.IBinaryInteger<TInteger>;

	///<summary>Computes the floor of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Floor(System.Half)", "Math.floor(__arg1)")]
	public extern static Number _d6fe3bcb6907fd7d(Number value);

	///<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[Jazor(Op.Import, "static System.Half.Round(System.Half)")]
	public static Number _8654f1427404f736(Number value)
		=> RoundToEven(value);

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[Jazor(Op.Import, "static System.Half.Round(System.Half, int)")]
	public static Number _a977225c7ea195c2(Number x, Number digits)
		=> RoundToHalf(SingleModule.RoundCore(x, digits, 0));

	///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
	[Jazor(Op.Import, "static System.Half.Round(System.Half, System.MidpointRounding)")]
	public static Number _a3bd625b8647d19e(Number x, Number mode)
		=> RoundToHalf(SingleModule.RoundCore(x, 0, mode));

	///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
	[Jazor(Op.Import, "static System.Half.Round(System.Half, int, System.MidpointRounding)")]
	public static Number _df8d144bad4e8a0b(Number x, Number digits, Number mode)
		=> RoundToHalf(SingleModule.RoundCore(x, digits, mode));

	///<summary>Truncates a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Truncate(System.Half)", "Math.trunc(__arg1)")]
	public extern static Number _39b223ef3f993244(Number value);

	[Jazor(Op.Inline, "static System.Half.E.get", "2.71875")]
	public extern static Number _20b56e33a189ad96();

	[Jazor(Op.Inline, "static System.Half.Pi.get", "3.140625")]
	public extern static Number _ab1c9fafbf00056d();

	[Jazor(Op.Inline, "static System.Half.Tau.get", "6.28125")]
	public extern static Number _583a8bc34e12207b();

	[Jazor(Op.Inline, "static System.Half.NegativeZero.get", "-0")]
	public extern static Number _0ad265f6bb77407b();

	///<summary>Computes the arc-tangent of the quotient of two values.</summary>
	[Jazor(Op.Inline, "static System.Half.Atan2(System.Half, System.Half)", "Math.f16round(Math.atan2(__arg1, __arg2))")]
	public extern static Number _bf44d164259c15da(Number y, Number x);

	///<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.Atan2Pi(System.Half, System.Half)", "Math.f16round(Math.atan2(__arg1, __arg2) / Math.PI)")]
	public extern static Number _8ad72e09d77426ab(Number y, Number x);

	///<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
	[Jazor(Op.Import, "static System.Half.BitDecrement(System.Half)")]
	public static Number _c976c1d81370babf(Number x)
		=> BitDecrementCore(x);

	///<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
	[Jazor(Op.Import, "static System.Half.BitIncrement(System.Half)")]
	public static Number _3bbda0fdee7bad1d(Number x)
		=> BitIncrementCore(x);

	///<summary>Computes the fused multiply-add of three values.</summary>
	[Jazor(Op.Inline, "static System.Half.FusedMultiplyAdd(System.Half, System.Half, System.Half)", "Math.f16round(__arg1 * __arg2 + __arg3)")]
	public extern static Number _92059353d5f47f52(Number left, Number right, Number addend);

	///<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
	[Jazor(Op.Import, "static System.Half.Ieee754Remainder(System.Half, System.Half)")]
	public static Number _18006f6446bcf954(Number left, Number right)
		=> Ieee754RemainderCore(left, right);

	///<summary>Computes the integer logarithm of a value.</summary>
	[Jazor(Op.Import, "static System.Half.ILogB(System.Half)")]
	public static Number _32ebc25218ce32e0(Number value)
		=> ILogBCore(value);

	///<summary>Performs a linear interpolation between two values based on the given weight.</summary>
	[Jazor(Op.Inline, "static System.Half.Lerp(System.Half, System.Half, System.Half)", "Math.f16round(__arg1 + (__arg2 - __arg1) * __arg3)")]
	public extern static Number _281b31dd3063e35c(Number value1, Number value2, Number amount);

	///<summary>Computes an estimate of the reciprocal of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.ReciprocalEstimate(System.Half)", "Math.f16round(1 / __arg1)")]
	public extern static Number _3c5166982a7e4c6b(Number value);

	///<summary>Computes an estimate of the reciprocal square root of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.ReciprocalSqrtEstimate(System.Half)", "Math.f16round(1 / Math.sqrt(__arg1))")]
	public extern static Number _9701aebe42f29e01(Number value);

	///<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
	[Jazor(Op.Inline, "static System.Half.ScaleB(System.Half, int)", "Math.f16round(__arg1 * Math.pow(2, __arg2))")]
	public extern static Number _1c84d26a02360b31(Number value, Number exponent);

	///<summary>Computes the hyperbolic arc-cosine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Acosh(System.Half)", "Math.f16round(Math.acosh(__arg1))")]
	public extern static Number _3ffeec9c67d15db1(Number value);

	///<summary>Computes the hyperbolic arc-sine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Asinh(System.Half)", "Math.f16round(Math.asinh(__arg1))")]
	public extern static Number _99ed70dc8a7f8541(Number value);

	///<summary>Computes the hyperbolic arc-tangent of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Atanh(System.Half)", "Math.f16round(Math.atanh(__arg1))")]
	public extern static Number _91d301b6cc6ed854(Number value);

	///<summary>Computes the hyperbolic cosine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Cosh(System.Half)", "Math.f16round(Math.cosh(__arg1))")]
	public extern static Number _89ad7d31e186ff0b(Number value);

	///<summary>Computes the hyperbolic sine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Sinh(System.Half)", "Math.f16round(Math.sinh(__arg1))")]
	public extern static Number _9983bab097d002c4(Number value);

	///<summary>Computes the hyperbolic tangent of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Tanh(System.Half)", "Math.f16round(Math.tanh(__arg1))")]
	public extern static Number _9b05dfb4dacbab50(Number value);

	///<summary>Increments a value.</summary>
	[Jazor(Op.Inline, "static System.Half.operator ++(System.Half)", "Math.f16round(__arg1 + 1)")]
	public extern static Number _8a130a46c2e685e4(Number value);

	///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Log(System.Half)", "Math.f16round(Math.log(__arg1))")]
	public extern static Number _1d346e947c93796a(Number value);

	///<summary>Computes the logarithm of a value in the specified base.</summary>
	[Jazor(Op.Inline, "static System.Half.Log(System.Half, System.Half)", "Math.f16round(Math.log(__arg1) / Math.log(__arg2))")]
	public extern static Number _e9543dba526157bb(Number value, Number newBase);

	///<summary>Computes the base-10 logarithm of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Log10(System.Half)", "Math.f16round(Math.log10(__arg1))")]
	public extern static Number _bf18fa93c9143ce7(Number value);

	///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
	[Jazor(Op.Inline, "static System.Half.LogP1(System.Half)", "Math.f16round(Math.log1p(__arg1))")]
	public extern static Number _c7bfbccbaa6f0096(Number value);

	///<summary>Computes the base-2 logarithm of a value plus one.</summary>
	[Jazor(Op.Inline, "static System.Half.Log2P1(System.Half)", "Math.f16round(Math.log2(__arg1 + 1))")]
	public extern static Number _307d0b18233ab939(Number value);

	///<summary>Computes the base-10 logarithm of a value plus one.</summary>
	[Jazor(Op.Inline, "static System.Half.Log10P1(System.Half)", "Math.f16round(Math.log10(__arg1 + 1))")]
	public extern static Number _6ef95ef69ba65637(Number value);

	///<summary>Divides two values together to compute their modulus or remainder.</summary>
	[Jazor(Op.Inline, "static System.Half.operator %(System.Half, System.Half)", "Math.f16round(__arg1 % __arg2)")]
	public extern static Number _ef99ab16caf9d04c(Number left, Number right);

	[Jazor(Op.Inline, "static System.Half.MultiplicativeIdentity.get", "1")]
	public extern static Number _9135806cbc71006f();

	///<summary>Multiplies two values together to compute their product.</summary>
	[Jazor(Op.Inline, "static System.Half.operator *(System.Half, System.Half)", "Math.f16round(__arg1 * __arg2)")]
	public extern static Number _d5faf45c7aa80143(Number left, Number right);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Import, "static System.Half.Clamp(System.Half, System.Half, System.Half)")]
	public static Number _6335905a4e3a886f(Number value, Number min, Number max)
		=> ClampCore(value, min, max);

	[Jazor(Op.Import ,"static System.Half.ClampNative(System.Half, System.Half, System.Half)")]
	public static Number _de3198267b6b5ced(Number value, Number min, Number max)
		=> ClampNativeCore(value, min, max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Inline, "static System.Half.CopySign(System.Half, System.Half)", "((__arg2 < 0 || Object.is(__arg2, -0)) ? -Math.abs(__arg1) : Math.abs(__arg1))")]
	public extern static Number _61a2b73d875185f7(Number value, Number sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Inline, "static System.Half.Max(System.Half, System.Half)", "Math.max(__arg1, __arg2)")]
	public extern static Number _b0445d5f12e95123(Number x, Number y);

	[Jazor(Op.Inline ,"static System.Half.MaxNative(System.Half, System.Half)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static Number _1dc3ea05a326229a(Number x, Number y);

	///<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.MaxNumber(System.Half, System.Half)", "(isNaN(__arg1) ? __arg2 : (isNaN(__arg2) ? __arg1 : Math.max(__arg1, __arg2)))")]
	public extern static Number _39cd7d1bd849cab4(Number x, Number y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Inline, "static System.Half.Min(System.Half, System.Half)", "Math.min(__arg1, __arg2)")]
	public extern static Number _1e2274498f8fa191(Number x, Number y);

	[Jazor(Op.Inline ,"static System.Half.MinNative(System.Half, System.Half)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static Number _7eb1110bffc904c0(Number x, Number y);

	///<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.MinNumber(System.Half, System.Half)", "(isNaN(__arg1) ? __arg2 : (isNaN(__arg2) ? __arg1 : Math.min(__arg1, __arg2)))")]
	public extern static Number _8661672e63915b33(Number x, Number y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Import, "static System.Half.Sign(System.Half)")]
	public static Number _86dc947c6d8aa31a(Number value)
		=> DoubleModule.SignCore(value);

	[Jazor(Op.Inline, "static System.Half.One.get", "1")]
	public extern static Number _9b62844bcac15446();

	[Jazor(Op.Inline, "static System.Half.Zero.get", "0")]
	public extern static Number _60dfaaeb5c0ca886();

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Abs(System.Half)", "Math.abs(__arg1)")]
	public extern static Number _06cb29794ed1d995(Number value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Half.CreateChecked<TOther>(TOther)")]
	public extern static Number _d43cfd4432bc935c<TOther>(TOther value)
		where TOther : global::System.Numerics.INumberBase<TOther>;

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Half.CreateSaturating<TOther>(TOther)")]
	public extern static Number _9503b27b5a950674<TOther>(TOther value)
		where TOther : global::System.Numerics.INumberBase<TOther>;

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Half.CreateTruncating<TOther>(TOther)")]
	public extern static Number _9b43d0ce280d7090<TOther>(TOther value)
		where TOther : global::System.Numerics.INumberBase<TOther>;

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Inline, "static System.Half.IsEvenInteger(System.Half)", "(__arg1 % 2 === 0)")]
	public extern static bool _c3394f7d290c0616(Number value);

	///<summary>Determines if a value represents an integral value.</summary>
	[Jazor(Op.Inline, "static System.Half.IsInteger(System.Half)", "Number.isInteger(__arg1)")]
	public extern static bool _e66f4bcfa8cccdb0(Number value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Inline, "static System.Half.IsOddInteger(System.Half)", "(Number.isInteger(__arg1) && __arg1 % 2 !== 0)")]
	public extern static bool _3ddd2a35067d173b(Number value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Inline, "static System.Half.IsPositive(System.Half)", "(__arg1 > 0 || Object.is(__arg1, 0))")]
	public extern static bool _51ea5ee44c080342(Number value);

	///<summary>Determines if a value represents a real number.</summary>
	[Jazor(Op.Inline, "static System.Half.IsRealNumber(System.Half)", "Number.isFinite(__arg1)")]
	public extern static bool _ec68050bacf76cd4(Number value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Import, "static System.Half.MaxMagnitude(System.Half, System.Half)")]
	public static Number _62245f7092999e63(Number x, Number y)
		=> DoubleModule.MaxMagnitudeCore(x, y);

	///<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Import, "static System.Half.MaxMagnitudeNumber(System.Half, System.Half)")]
	public static Number _52991fa82e7974ee(Number x, Number y)
		=> DoubleModule.MaxMagnitudeNumberCore(x, y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Import, "static System.Half.MinMagnitude(System.Half, System.Half)")]
	public static Number _ceb58186d4c7edf0(Number x, Number y)
		=> DoubleModule.MinMagnitudeCore(x, y);

	///<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
	[Jazor(Op.Import, "static System.Half.MinMagnitudeNumber(System.Half, System.Half)")]
	public static Number _d6bec8db0dff7ab7(Number x, Number y)
		=> DoubleModule.MinMagnitudeNumberCore(x, y);

	///<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
	[Jazor(Op.Inline ,"static System.Half.MultiplyAddEstimate(System.Half, System.Half, System.Half)", "Math.f16round(__arg1 * __arg2 + __arg3)")]
	public extern static Number _ac2e24469a20d68a(Number left, Number right, Number addend);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import, "static System.Half.TryParse(string, System.IFormatProvider, out System.Half)")]
	public static Array<object?> _53367d1aaf68b5df(string? text, Intl.NumberFormat? provider, Number result)
		=> _83de0b9fe4433805(text, result);

	///<summary>Computes a value raised to a given power.</summary>
	[Jazor(Op.Inline, "static System.Half.Pow(System.Half, System.Half)", "Math.f16round(Math.pow(__arg1, __arg2))")]
	public extern static Number _a9f3147e8b57f5de(Number x, Number y);

	///<summary>Computes the cube-root of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Cbrt(System.Half)", "Math.f16round(Math.cbrt(__arg1))")]
	public extern static Number _1d0363fd6fc39e64(Number value);

	///<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
	[Jazor(Op.Inline, "static System.Half.Hypot(System.Half, System.Half)", "Math.f16round(Math.hypot(__arg1, __arg2))")]
	public extern static Number _94164ba89e051cf0(Number x, Number y);

	///<summary>Computes the n-th root of a value.</summary>
	[Jazor(Op.Import, "static System.Half.RootN(System.Half, int)")]
	public static Number _7d0e51fe4ac37ce8(Number value, Number root)
		=> RootNCore(value, root);

	///<summary>Computes the square-root of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Sqrt(System.Half)", "Math.f16round(Math.sqrt(__arg1))")]
	public extern static Number _4608c310544c0ac0(Number value);

	[Jazor(Op.Inline, "static System.Half.NegativeOne.get", "-1")]
	public extern static Number _21698e11ee94a560();

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Half.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _dcb3ed63566de2c5(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Half.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Half)")]
	public extern static Array<object?> _6fa9b6d3f9d71caa(string s, Intl.NumberFormat? provider, Number result);

	///<summary>Subtracts two values to compute their difference.</summary>
	[Jazor(Op.Inline, "static System.Half.operator -(System.Half, System.Half)", "Math.f16round(__arg1 - __arg2)")]
	public extern static Number _4de7f1e76d25cbdb(Number left, Number right);

	///<summary>Computes the arc-cosine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Acos(System.Half)", "Math.f16round(Math.acos(__arg1))")]
	public extern static Number _6c9a2c09639714a9(Number value);

	///<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.AcosPi(System.Half)", "Math.f16round(Math.acos(__arg1) / Math.PI)")]
	public extern static Number _60947bcda33f22db(Number value);

	///<summary>Computes the arc-sine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Asin(System.Half)", "Math.f16round(Math.asin(__arg1))")]
	public extern static Number _0cc0e377c100101a(Number value);

	///<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.AsinPi(System.Half)", "Math.f16round(Math.asin(__arg1) / Math.PI)")]
	public extern static Number _a8e63edad634e02a(Number value);

	///<summary>Computes the arc-tangent of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Atan(System.Half)", "Math.f16round(Math.atan(__arg1))")]
	public extern static Number _ebe5ac26d95fa5ae(Number value);

	///<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
	[Jazor(Op.Inline, "static System.Half.AtanPi(System.Half)", "Math.f16round(Math.atan(__arg1) / Math.PI)")]
	public extern static Number _1be733fe261ea3a9(Number value);

	///<summary>Computes the cosine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Cos(System.Half)", "Math.f16round(Math.cos(__arg1))")]
	public extern static Number _5f075fcf895d9a14(Number value);

	///<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.CosPi(System.Half)", "Math.f16round(Math.cos(__arg1 * Math.PI))")]
	public extern static Number _16198dd6405a834b(Number value);

	///<summary>Converts a given value from degrees to radians.</summary>
	[Jazor(Op.Inline, "static System.Half.DegreesToRadians(System.Half)", "Math.f16round(__arg1 * Math.PI / 180)")]
	public extern static Number _0aaa0b1877e6ba97(Number value);

	///<summary>Converts a given value from radians to degrees.</summary>
	[Jazor(Op.Inline, "static System.Half.RadiansToDegrees(System.Half)", "Math.f16round(__arg1 * 180 / Math.PI)")]
	public extern static Number _172656f071e3da5f(Number value);

	///<summary>Computes the sine of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Sin(System.Half)", "Math.f16round(Math.sin(__arg1))")]
	public extern static Number _cb4eed588629026f(Number value);

	///<summary>Computes the sine and cosine of a value.</summary>
	[Jazor(Op.Import, "static System.Half.SinCos(System.Half)")]
	public static (Number Sin, Number Cos) _7bdc16d36920d5d9(Number value)
		=> (Sin: RoundToHalf(Math.SinFn(value)), Cos: RoundToHalf(Math.CosFn(value)));

	///<summary>Computes the sine and cosine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Import, "static System.Half.SinCosPi(System.Half)")]
	public static (Number SinPi, Number CosPi) _a1628326328dadd0(Number value)
	{
		var angle = value * Math.PI;
		return (RoundToHalf(Math.SinFn(angle)), RoundToHalf(Math.CosFn(angle)));
	}

	///<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.SinPi(System.Half)", "Math.f16round(Math.sin(__arg1 * Math.PI))")]
	public extern static Number _b4ba144dc45ba16f(Number value);

	///<summary>Computes the tangent of a value.</summary>
	[Jazor(Op.Inline, "static System.Half.Tan(System.Half)", "Math.f16round(Math.tan(__arg1))")]
	public extern static Number _6b99822ac69f068e(Number value);

	///<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
	[Jazor(Op.Inline, "static System.Half.TanPi(System.Half)", "Math.f16round(Math.tan(__arg1 * Math.PI))")]
	public extern static Number _74069ea6e6a4facb(Number value);

	///<summary>Computes the unary negation of a value.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator -(System.Half)")]
	public extern static Number _95bff0eef3c67977(Number value);

	///<summary>Computes the unary plus of a value.</summary>
	[Jazor(Op.Allowed, "static System.Half.operator +(System.Half)")]
	public extern static Number _a269cf8c5b3b5b68(Number value);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Half.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _2f8671997d0c4a70(Uint8Array utf8Text, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Half.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Half)")]
	public extern static Array<object?> _5cc398e88720c483(Uint8Array utf8Text, global::System.Globalization.NumberStyles style, Intl.NumberFormat? provider, Number result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Half.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _4643374acc136002(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Half.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out System.Half)")]
	public extern static Array<object?> _6aeaf87eeee0e50f(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
