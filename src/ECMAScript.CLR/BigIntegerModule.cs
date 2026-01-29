using ECMAScript.Common;
using System.Collections;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Numerics.BigInteger", "System.Numerics.BigInteger",WhiteListOp.Allowed)]
public static class BigIntegerModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 32-bit signed integer value.</summary>
    [WhiteList("_ba6e0e86598dc8b2","System.Numerics.BigInteger.BigInteger(int)",WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _ba6e0e86598dc8b2(Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using an unsigned 32-bit integer value.</summary>
	[WhiteList("_b7b735a5d507d449", "System.Numerics.BigInteger.BigInteger(uint)", WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _b7b735a5d507d449(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 64-bit signed integer value.</summary>
    [WhiteList("_74973910762e0e86","System.Numerics.BigInteger.BigInteger(long)",WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _74973910762e0e86(BigInt value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure with an unsigned 64-bit integer value.</summary>
    [WhiteList("_0421ba6c202fdc80","System.Numerics.BigInteger.BigInteger(ulong)",WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _0421ba6c202fdc80(BigInt value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a single-precision floating-point value.</summary>
    [WhiteList("_cfd2038efd505e1f","System.Numerics.BigInteger.BigInteger(float)",WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _cfd2038efd505e1f(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a double-precision floating-point value.</summary>
    [WhiteList("_38c7caccfd5e120e","System.Numerics.BigInteger.BigInteger(double)",WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _38c7caccfd5e120e(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a <see cref="T:System.Decimal" /> value.</summary>
    [WhiteList("_f715f85cc5dcfe92","System.Numerics.BigInteger.BigInteger(System.Decimal)", WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _f715f85cc5dcfe92(System.Decimal value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a byte array.</summary>
    [WhiteList("_c1e724fa6dbf63eb","System.Numerics.BigInteger.BigInteger(byte[])",WhiteListOp.Allowed)]
	public static BigInt _c1e724fa6dbf63eb(Array<byte> value)
	{
		// 空白数组处理
		if (value.Length == 0)
			return BigInt.Zero;

		var buffer = new ArrayBuffer(value.Length);
		var array = new Uint8Array(buffer);
		var view = new DataView(array.Buffer, array.ByteOffset, array.ByteLength);
		var result = BigInt.Zero;
		var i = 0u;

		// 每次处理 8 字节（64位）
		for (; i + 8 <= value.Length; i += 8)
			result = (result << BigInt(64)) | view.GetBigUint64(i, false);

		// 处理剩余字节（最多7字节）
		if (i < value.Length)
		{
			var remaining = BigInt.Zero;
			for (; i < value.Length; i++)
				remaining = (remaining << BigInt(8)) | BigInt(value[i]);

			result = (result << BigInt((value.Length - i) * 8u)) | remaining;
		}

		return result;
	}

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a read-only span of bytes, and optionally indicating the signing encoding and the endianness byte order.</summary>
	[WhiteList("_9c321a7400e5ff9b","System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)",WhiteListOp.Allowed)]
	public static BigInt _9c321a7400e5ff9b(Array<byte> value, bool isUnsigned, bool isBigEndian)
	{
		// 处理空字节数组
		if (value.Length == 0)
			return BigInt.Zero;

		// 处理1字节特殊情况
		if (value.Length == 1)
		{
			if (isUnsigned)
			{
				return BigInt(value[0]);
			}
			else
			{
				// 有符号数：如果最高位为1，则为负数
				return (value[0] & 0x80) == 0
					? BigInt(value[0])
					: BigInt(value[0]) - BigInt(0x100);
			}
		}

		// 处理标准长度（2/4/8字节）
		if (value.Length <= 8)
		{
			var buffer = new ArrayBuffer(value.Length);
			var view = new DataView(buffer);
			value.ForEach((item, index) => view.SetUint8(index, item));

			return value.Length switch
			{
				2 => isUnsigned ?
					BigInt(view.GetUint16(0, !isBigEndian)) :
					BigInt(view.GetInt16(0, !isBigEndian)),
				4 => isUnsigned ?
					BigInt(view.GetUint32(0, !isBigEndian)) :
					BigInt(view.GetInt32(0, !isBigEndian)),
				8 => isUnsigned ?
					view.GetBigUint64(0, !isBigEndian) :
					view.GetBigInt64(0, !isBigEndian),
				// 3/5/6/7字节长度使用非标准处理
				_ => ProcessNonStandardLength(value, isUnsigned, isBigEndian)
			};
		}

		// 处理超过8字节以上的非标准长度
		return ProcessNonStandardLength(value, isUnsigned, isBigEndian);

		// 处理非标准长度字节数组（3-7字节或>8字节）
		static BigInt ProcessNonStandardLength(Array<byte> bytes, bool isUnsigned, bool isBigEndian)
		{
			// 创建字节数组的副本以避免修改原始数据
			var processedBytes = bytes.Slice(0);

			// 如果需要转换为小端序以便处理
			if (isBigEndian)
			{
				processedBytes.Reverse();
			}

			// 从小端序字节数组构建无符号大整数
			var result = BuildBigIntFromLEBytes(processedBytes);

			// 对于有符号数的处理，转换如果最高位为1
			if (!isUnsigned && (processedBytes[processedBytes.Length - 1] & 0x80) != 0)
			{
				// 计算位宽度 = 字节数 * 8
				var bitWidth = BigInt(processedBytes.Length) * BigInt(8);

				// 计算2的bitWidth次方作为偏移量
				var offset = BigInt.One << bitWidth;

				// 转换为有符号补码值
				result -= offset;
			}

			return result;
		}

		// 从小端序字节数组构建无符号大整数
		static BigInt BuildBigIntFromLEBytes(Array<byte> littleEndianBytes)
		{
			var result = BigInt.Zero;

			// 从最低字节开始，按小端序累加，最后一个字节是最高位
			for (var i = littleEndianBytes.Length - 1; i >= 0; i--)
			{
				// 移位8位拼接当前字节值
				result = (result << BigInt(8)) | BigInt(littleEndianBytes[i] & 0xFF);
			}

			return result;
		}
	}

	[WhiteList("_77fc63f99954f8da","static System.Numerics.BigInteger.Zero.get", WhiteListOp.Literal, "0n")]
	public extern static BigInt _77fc63f99954f8da(BigInt instance);

    [WhiteList("_9c5419989e842d00","static System.Numerics.BigInteger.One.get", WhiteListOp.Literal, "1n")]
	public extern static BigInt _9c5419989e842d00(BigInt instance);

    [WhiteList("_01c112900aa52c82","static System.Numerics.BigInteger.MinusOne.get", WhiteListOp.Literal, "-1n")]
	[ECMAScriptLiteral("-1n")]
	public extern static BigInt _01c112900aa52c82(BigInt instance);

    [WhiteList("_ee8564f940baf789","System.Numerics.BigInteger.IsPowerOfTwo.get", WhiteListOp.Literal, "(@#{0} > 0n && ((@#{0} & (@#{0} - 1n)) == 0n))")]
	public extern static bool _ee8564f940baf789(BigInt instance);

    [WhiteList("_c138b3f4dd057592","System.Numerics.BigInteger.IsZero.get", WhiteListOp.Literal, "@#{0} === 0n")]
	public extern static bool _c138b3f4dd057592(BigInt instance);

    [WhiteList("_2aa0739f87c79906","System.Numerics.BigInteger.IsOne.get", WhiteListOp.Literal, "@#{0} === 1n")]
	public extern static bool _2aa0739f87c79906(BigInt instance);

    [WhiteList("_4a465705ad4dc8ca","System.Numerics.BigInteger.IsEven.get", WhiteListOp.Literal, "@#{0} % 2n === 0n")]
	public extern static bool _4a465705ad4dc8ca(BigInt instance);

    [WhiteList("_734290a188c5bc5a","System.Numerics.BigInteger.Sign.get", WhiteListOp.Literal, "(@#{0} === 0n ? 0 : (@#{0} > 0n ? 1 : -1))")]
	public extern static Number _734290a188c5bc5a(BigInt instance);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    [WhiteList("_155212572c9a3297","static System.Numerics.BigInteger.Parse(string)", WhiteListOp.Literal, "BigInt(@#{0})")]
	public extern static BigInt _155212572c9a3297(string value);

    ///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    [WhiteList("_a077721686cadcd9","static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles)",WhiteListOp.Discard)]
	public extern static BigInt _a077721686cadcd9(string value, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    [WhiteList("_d1543aa14ab94729","static System.Numerics.BigInteger.Parse(string, System.IFormatProvider)",WhiteListOp.Discard)]
	public extern static BigInt _d1543aa14ab94729(string value, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    [WhiteList("_8adf758c3f22af12","static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)",WhiteListOp.Discard)]
	public extern static BigInt _8adf758c3f22af12(string value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    [WhiteList("_59acea2facdaa757","static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public static bool _59acea2facdaa757(string? value, OutValue<BigInt?> result)
	{
		try
		{
			if (value?.Length > 0)
			{
				result.Value = BigInt(value);
				return true;
			}
		}
		catch { }

		return false;
	}

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("_85cd9c4a9c2dadf4","static System.Numerics.BigInteger.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static bool _85cd9c4a9c2dadf4(string? value, System.Globalization.NumberStyles? style, Intl.NumberFormat? provider, OutValue<BigInt?> result);

    ///<summary>Converts the representation of a number, contained in the specified read-only span of characters, in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    [WhiteList("_00d39f2029fd4266","static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)",WhiteListOp.Discard)]
	public extern static BigInt _00d39f2029fd4266(Uint32Array value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the representation of a number contained in the specified read-only character span, to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    [WhiteList("_ded03bf84977945f","static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static bool _ded03bf84977945f(Uint32Array value, OutValue<BigInt> result);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    [WhiteList("_d733f0a0a427d970","static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static bool _d733f0a0a427d970(Uint32Array value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<BigInt> result);

    ///<summary>Compares two <see cref="T:System.Numerics.BigInteger" /> values and returns an integer that indicates whether the first value is less than, equal to, or greater than the second value.</summary>
    [WhiteList("_0a6134f61ab96205","static System.Numerics.BigInteger.Compare(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal,"@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _0a6134f61ab96205(BigInt left, BigInt right);

    ///<summary>Gets the absolute value of a <see cref="T:System.Numerics.BigInteger" /> object.</summary>
    [WhiteList("_efd2134803006c44","static System.Numerics.BigInteger.Abs(System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < 0n ? -@#{0} : @#{0}")]
	public extern static BigInt _efd2134803006c44(BigInt value);

    ///<summary>Adds two <see cref="T:System.Numerics.BigInteger" /> values and returns the result.</summary>
    [WhiteList("_0034b6a7a416df8e","static System.Numerics.BigInteger.Add(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} + @#{1}")]
	public extern static BigInt _0034b6a7a416df8e(BigInt left, BigInt right);

    ///<summary>Subtracts one <see cref="T:System.Numerics.BigInteger" /> value from another and returns the result.</summary>
    [WhiteList("_31de7c0189a18bd2","static System.Numerics.BigInteger.Subtract(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} - @#{1}")]
	public extern static BigInt _31de7c0189a18bd2(BigInt left, BigInt right);

    ///<summary>Returns the product of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_8c06584cae9fcbe7","static System.Numerics.BigInteger.Multiply(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} * @#{1}")]
	public extern static BigInt _8c06584cae9fcbe7(BigInt left, BigInt right);

    ///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another and returns the result.</summary>
    [WhiteList("_7ff5692b085214c4","static System.Numerics.BigInteger.Divide(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Discard)]
	[ECMAScriptLiteral("@#{0} / @#{1}")]
	public extern static BigInt _7ff5692b085214c4(BigInt dividend, BigInt divisor);

    ///<summary>Performs integer division on two <see cref="T:System.Numerics.BigInteger" /> values and returns the remainder.</summary>
    [WhiteList("_00d98488c7edf612","static System.Numerics.BigInteger.Remainder(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} % @#{1}")]
	public extern static BigInt _00d98488c7edf612(BigInt dividend, BigInt divisor);

    ///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another, returns the result, and returns the remainder in an output parameter.</summary>
    [WhiteList("_598611fb2b8a064a","static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _598611fb2b8a064a(BigInt dividend, BigInt divisor, OutValue<BigInt> remainder)
	{
		remainder.Value = dividend % divisor;
		return dividend / divisor;
	}

	///<summary>Negates a specified <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[WhiteList("_d160232d04d4f8fe","static System.Numerics.BigInteger.Negate(System.Numerics.BigInteger)",WhiteListOp.Literal, "-@#{0}")]
	public extern static BigInt _d160232d04d4f8fe(BigInt value);

    ///<summary>Returns the natural (base <see langword="e" />) logarithm of a specified number.</summary>
    [WhiteList("_fb5a811e7a32a324", "static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
    public static Number _fb5a811e7a32a324(BigInt value)
    {
        if (value <= BigInt.Zero)
            throw new Error("Logarithm is undefined for non-positive numbers");

        var str = value.ToString();
        var exponent = str.Length - 1;
        var mantissa = Number(str.Substring(0, 15));

        return Maths.Log(mantissa) + exponent * Maths.Log(10);
    }

    ///<summary>Returns the logarithm of a specified number in a specified base.</summary>
    [WhiteList("_acb5aef300c8db0c","static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)",WhiteListOp.Allowed)]
	public static Number _acb5aef300c8db0c(BigInt value, Number baseValue)
	{
		if (value <= BigInt.Zero)
			throw new RangeError("Logarithm is undefined for non-positive numbers");

		if (baseValue <= 0 || baseValue == 1)
			throw new RangeError("Base must be positive and not equal to 1");

		if (value == BigInt.One)
			return 0;

		if (baseValue == Maths.E)
			return Maths.Log(Number(value));

		if (value <= Number.MAX_SAFE_INTEGER)
			return Maths.Log(Number(value)) / Maths.Log(baseValue);

		var str = value.ToString();
		var digitCount = str.Length;
		var significantDigits = str.Substring(0, 15);
		var mantissa = ParseFloat(significantDigits) / Maths.Pow(10, significantDigits.Length - 1);
		var exponent = digitCount - 1;
		var lnValue = Maths.Log(mantissa) + exponent * Maths.LN10;
		var lnBase = Maths.Log(baseValue);

		return lnValue / lnBase;
	}

	///<summary>Returns the base 10 logarithm of a specified number.</summary>
	[WhiteList("_f276cbd7c3b305ea","static System.Numerics.BigInteger.Log10(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static Number _f276cbd7c3b305ea(BigInt value)
	{
		if (value <= BigInt.Zero)
			throw new RangeError("Logarithm is undefined for non-positive numbers");

		if (value == BigInt.One)
			return 0;

		var str = value.ToString();
		return (str.Length <= 15)
			? Maths.Log10(Number(value))
			: Maths.Log10(Number(str.Substring(0, 15))) + (str.Length - 15);
	}

	///<summary>Finds the greatest common divisor of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[WhiteList("_7555649a5efc7b79","static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _7555649a5efc7b79(BigInt left, BigInt right)
	{
		var a = left < BigInt.Zero ? -left : left;
		var b = right < BigInt.Zero ? -right : right;

		if (a == BigInt.Zero)
			return b;

		if (b == BigInt.Zero)
			return a;

		while (b != BigInt.Zero)
		{
			var temp = b;
			b = a % b;
			a = temp;
		}

		return a;
	}

	///<summary>Returns the larger of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[WhiteList("_a038619e95a6c0ff","static System.Numerics.BigInteger.Max(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} > @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _a038619e95a6c0ff(BigInt left, BigInt right);

    ///<summary>Returns the smaller of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_b3b093dd81ed2d15","static System.Numerics.BigInteger.Min(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _b3b093dd81ed2d15(BigInt left, BigInt right);

    ///<summary>Performs modulus division on a number raised to the power of another number.</summary>
    [WhiteList("_ec6961a106ca5bf3","static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _ec6961a106ca5bf3(BigInt value, BigInt exponent, BigInt modulus)
	{
		if (modulus == BigInt.One)
			return BigInt.Zero;

		var result = BigInt.One;
		var val = value % modulus;
		var exp = exponent;

		while (exp > BigInt.Zero)
		{
			if (exp % BigInt.Two == BigInt.One)
				result = (result * val) % modulus;

			exp >>= BigInt.One;
			val = (val * val) % modulus;
		}

		return result;
	}

	///<summary>Raises a <see cref="T:System.Numerics.BigInteger" /> value to the power of a specified value.</summary>
	[WhiteList("_31cf4d89164dee40","static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)",WhiteListOp.Allowed)]
	public static BigInt _31cf4d89164dee40(BigInt value, Number exponent)
	{
		if (exponent < 0 || !Number.IsInteger(exponent))
			throw new RangeError("The exponent must be a non-negative integer");

		var result = BigInt.One;
		var current = value;
		var exp = exponent;

		while (exp > 0)
		{
			if (exp % 2 == 1)
				result *= current;

			current *= current;
			exp = Maths.Floor(exp / 2);
		}

		return result;
	}

	///<summary>Returns the hash code for the current <see cref="T:System.Numerics.BigInteger" /> object.</summary>
	[WhiteList("_fe64082374302a77","override System.Numerics.BigInteger.GetHashCode)",WhiteListOp.Allowed)]
	public static Number _fe64082374302a77(BigInt instance)
	{
		var positiveValue = instance < BigInt.Zero ? -instance : instance;
		return Number(positiveValue % BigInt(2147483647));
	}

	///<summary>Returns a value that indicates whether the current instance and a specified object have the same value.</summary>
	[WhiteList("_27c2f0d965e3403d","override System.Numerics.BigInteger.Equals(object)",WhiteListOp.Literal, "@#{0} === @#{1}")]
	public extern static bool _27c2f0d965e3403d(BigInt instance, Object? obj);

    ///<summary>Returns a value that indicates whether the current instance and a signed 64-bit integer have the same value.</summary>
    [WhiteList("_21afeec99b7ab2ca","override System.Numerics.BigInteger.Equals(long)",WhiteListOp.Literal, "@#{0} === @#{1}")]
	public extern static bool _21afeec99b7ab2ca(BigInt instance, BigInt other);

    ///<summary>Returns a value that indicates whether the current instance and an unsigned 64-bit integer have the same value.</summary>
    [WhiteList("_134be6ec440e455e","System.Numerics.BigInteger.Equals(ulong)",WhiteListOp.Literal, "@#{0} === @#{1}")]
	public extern static bool _134be6ec440e455e(BigInt instance, BigInt other);

    ///<summary>Returns a value that indicates whether the current instance and a specified <see cref="T:System.Numerics.BigInteger" /> object have the same value.</summary>
    [WhiteList("_4d44e94420c56981","System.Numerics.BigInteger.Equals(System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} === @#{1}")]
	public extern static bool _4d44e94420c56981(BigInt instance, BigInt other);

    ///<summary>Compares this instance to a signed 64-bit integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the signed 64-bit integer.</summary>
    [WhiteList("_77851a1e7ef48cb7","System.Numerics.BigInteger.CompareTo(long)",WhiteListOp.Discard)]
	public extern static Number _77851a1e7ef48cb7(BigInt instance, BigInt other);

    ///<summary>Compares this instance to an unsigned 64-bit integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the unsigned 64-bit integer.</summary>
    [WhiteList("_64e348c0c7830a5c","System.Numerics.BigInteger.CompareTo(ulong)",WhiteListOp.Literal, "@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _64e348c0c7830a5c(BigInt instance, BigInt other);

    ///<summary>Compares this instance to a second <see cref="T:System.Numerics.BigInteger" /> and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
    [WhiteList("_02bf2f34cf157e4d","System.Numerics.BigInteger.CompareTo(System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _02bf2f34cf157e4d(BigInt instance, BigInt other);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
    [WhiteList("_9f7b3705890bed98","System.Numerics.BigInteger.CompareTo(object)",WhiteListOp.Literal, "@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _9f7b3705890bed98(BigInt instance, Object? obj);

    ///<summary>Converts a <see cref="T:System.Numerics.BigInteger" /> value to a byte array.</summary>
    [WhiteList("_ca46777d5c8cc9b9","System.Numerics.BigInteger.ToByteArray()",WhiteListOp.Allowed)]
	public static byte[] _ca46777d5c8cc9b9(BigInt instance)
	{
		if (instance == BigInt.Zero)
			return [0];

		var isNegative = instance < BigInt.Zero;
		var value = isNegative ? -instance : instance;
		var bytes = new Array<byte>();

		while (value > BigInt.Zero)
		{
			bytes.Unshift(Number(value & BigInt(0xFF)));
			value >>= BigInt(8);
		}

		// 处理负数的补码表示
		if (isNegative)
		{
			// 按位取反
			for (var i = 0u; i < bytes.Length; i++)
				bytes[i] = (byte)((~bytes[i]) & 0xFF);

			// 加1（补码）
			var carry = 1u;
			for (var i = (uint)bytes.Length - 1; i >= 0 && carry > 0; i--)
			{
				var sum = bytes[i] + carry;
				bytes[i] = (byte)(sum & 0xFF);
				carry = sum >> 8;
			}

			// 确保符号位为1（最高位为1）
			if ((bytes[0] & 0x80) == 0)
				bytes.Unshift(0xFF);
		}

		return bytes;
	}

	///<summary>Returns the value of this <see cref="T:System.Numerics.BigInteger" /> as a byte array using the fewest number of bytes possible. If the value is zero, returns an array of one byte whose element is 0x00.</summary>
	[WhiteList("_11ed9d474ccf2419","System.Numerics.BigInteger.ToByteArray(bool, bool)",WhiteListOp.Allowed)]
	public static byte[] _11ed9d474ccf2419(BigInt instance, bool isUnsigned, bool isBigEndian)
	{
		if (instance == BigInt.Zero)
			return [0];

		var isNegative = !isUnsigned && instance < BigInt.Zero;
		var value = isNegative ? -instance - BigInt.One : instance;
		var bytes = new Array<byte>();
		var bitLength = 0;
		var temp = value;

		while (temp > BigInt.Zero)
		{
			bitLength++;
			temp >>= BigInt.One;
		}

		var minLength = isNegative ? Maths.Ceil((bitLength + 1) / 8) : Maths.Ceil(bitLength / 8);
		var byteLength = Maths.Max(minLength, 1);

		for (var i = 0; i < byteLength; i++)
		{
			var b = Number(value & BigInt(0xFF));
			if (isBigEndian)
				bytes.Unshift(b);
			else
				bytes.Push(b);

			value >>= BigInt(8);
		}

		if (isNegative)
		{
			for (var i = 0u; i < bytes.Length; i++)
				bytes[i] = (byte)((~bytes[i]) & 0xFF);

			if (isBigEndian && (bytes[0] & 0x80) == 0)
				bytes.Unshift(0xFF);

			else if (!isBigEndian && (bytes[bytes.Length - 1] & 0x80) == 0)
				bytes.Push(0xFF);
		}

		return bytes;
	}

	///<summary>Copies the value of this <see cref="T:System.Numerics.BigInteger" /> as little-endian twos-complement bytes, using the fewest number of bytes possible. If the value is zero, outputs one byte whose element is 0x00.</summary>
	[WhiteList("_76ae4e496fc976fd","System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)",WhiteListOp.Allowed)]
	public static bool _76ae4e496fc976fd(BigInt instance, Uint8Array destination, OutValue<Number> bytesWritten, bool isUnsigned, bool isBigEndian)
	{
		// 1. 计算所需字节数
		var requiredBytes = 1; // 至少需要1字节
		if (instance != BigInt.Zero)
		{
			var isNegative = !isUnsigned && instance < BigInt.Zero;
			var value = isNegative ? (isUnsigned ? instance : -instance - BigInt.One) : instance;
			var bitLength = 0u;

			// 计算位长度
			while (value > BigInt.Zero)
			{
				bitLength++;
				value >>= BigInt.One;
			}

			// 计算所需字节数
			requiredBytes = isUnsigned
				? Maths.Max(1, Maths.Ceil(bitLength / 8))
				: Maths.Max(1, Maths.Ceil((bitLength + 1) / 8));
		}

		// 2. 检查缓冲区大小
		if (destination.Length < requiredBytes)
		{
			bytesWritten.Value = 0;
			return false;
		}

		// 3. 转换为字节数组
		var bytes = new Array<byte>();
		if (instance == BigInt.Zero)
			bytes.Push(0);
		else
		{
			var isNegative = !isUnsigned && instance < BigInt.Zero;
			var value = isNegative ? -instance - BigInt.One : instance;

			// 按实际需要生成字节数
			var byteCount = requiredBytes;
			while (byteCount-- > 0)
			{
				var b = Number(value & BigInt(0xFF));
				if (isBigEndian)
					bytes.Unshift(b);
				else
					bytes.Push(b);

				value >>= BigInt(8);
			}

			// 处理负数的补码
			if (isNegative)
			{
				for (var i = 0u; i < bytes.Length; i++)
					bytes[i] = (byte)((~bytes[i]) & 0xFF);

				// 确保符号位正确
				if (isBigEndian && (bytes[0] & 0x80) == 0)
				{
					bytes.Unshift(0xFF);
					requiredBytes++;
				}
				else if (!isBigEndian && (bytes[bytes.Length - 1] & 0x80) == 0)
				{
					bytes.Push(0xFF);
					requiredBytes++;
				}
			}
		}

		// 4. 检查结果字节数是否超出缓冲区
		if (bytes.Length > destination.Length)
		{
			bytesWritten.Value = 0;
			return false;
		}

		// 5. 写入目标缓冲区
		for (var i = 0u; i < bytes.Length; i++)
			destination[i] = bytes[i];

		// 6. 填充剩余字节（如果需要）
		var fillByte = !isUnsigned && instance < BigInt.Zero ? 0xFF : 0;
		for (var i = bytes.Length; i < destination.Length; i++)
			destination[i] = (byte)fillByte;

		bytesWritten.Value = bytes.Length;
		return true;
	}

	///<summary>Gets the number of bytes that will be output by <see cref="M:System.Numerics.BigInteger.ToByteArray(System.Boolean,System.Boolean)" /> and <see cref="M:System.Numerics.BigInteger.TryWriteBytes(System.Span{System.Byte},System.Int32@,System.Boolean,System.Boolean)" />.</summary>
	[WhiteList("_c1393b267008395c","System.Numerics.BigInteger.GetByteCount(bool)",WhiteListOp.Allowed)]
	public static Number _c1393b267008395c(BigInt instance, bool isUnsigned)
	{
		if (instance == BigInt.Zero)
			return 1;

		var isNegative = !isUnsigned && instance < BigInt.Zero;
		var value = isNegative ? -instance : instance;
		var bitLength = 0;

		while (value > BigInt.Zero)
		{
			bitLength++;
			value >>= BigInt.One;
		}

		if (isUnsigned)
			return Maths.Max(1, Maths.Ceil(bitLength / 8));
		else
			return isNegative
				? Maths.Max(1, Maths.Ceil((bitLength + 1) / 8))  // 负数需要符号位
				: Maths.Max(1, Maths.Ceil(bitLength / 8));        // 正数不需要符号位
	}

	///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation.</summary>
	[WhiteList("_a7388cc0c5bc22ad","override System.Numerics.BigInteger.ToString()",WhiteListOp.Allowed)]
	public extern static string _a7388cc0c5bc22ad(BigInt instance);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified culture-specific formatting information.</summary>
    [WhiteList("_fe4c3211e57446e7","System.Numerics.BigInteger.ToString(System.IFormatProvider)",WhiteListOp.Allowed)]
	public static string? _fe4c3211e57446e7(BigInt instance, Intl.NumberFormat? provider)
	{
		if (provider is null)
			return instance.ToString();

		var isNegative = instance < BigInt.Zero;
		var absValue = isNegative ? -instance : instance;
		var strValue = absValue.ToString();

		try
		{
			// 对于可以直接使用Intl.NumberFormat的范围（在安全整数范围内）
			if (absValue <= BigInt(Number.MAX_SAFE_INTEGER))
			{
				var formatted = provider.Format(Number(absValue));
				return isNegative ? $"-{formatted}" : formatted;
			}

			// 对于长数字手动实现本地化分组格式
			var sample = provider.Format(1000.1);
			var groupChar = sample.Includes("1,000") ? "," :
							 sample.Includes("1.000") ? "." :
							 sample.Includes("1 000") ? " " : ",";

			// 从右到左添加分组分隔符
			var result = "";
			var i = strValue.Length;
			var groupCount = 0;

			while (i > 0)
			{
				if (groupCount > 0 && groupCount % 3 == 0)
				{
					result = groupChar + result;
				}
				result = strValue[--i] + result;
				groupCount++;
			}

			return isNegative ? $"-{result}" : result;

		}
		catch
		{
			return instance.ToString();
		}
	}

	///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format.</summary>
	[WhiteList("_1650d30e3e9172f5","System.Numerics.BigInteger.ToString(string)",WhiteListOp.Discard)]
	public extern static string _1650d30e3e9172f5(BigInt instance, string? format);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format and culture-specific format information.</summary>
    [WhiteList("_93b0cfb45a1832e9","System.Numerics.BigInteger.ToString(string, System.IFormatProvider)",WhiteListOp.Discard)]
	public extern static string _93b0cfb45a1832e9(BigInt instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Formats this big integer instance into a span of characters.</summary>
    [WhiteList("_90c190be387330ea","System.Numerics.BigInteger.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)",WhiteListOp.Discard)]
	public extern static bool _90c190be387330ea(BigInt instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Subtracts a <see cref="T:System.Numerics.BigInteger" /> value from another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_28554dca4c0c49f8","static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} - @#{1}")]
	public extern static BigInt _28554dca4c0c49f8(BigInt left, BigInt right);

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned byte value.</summary>
    [WhiteList("_c1afe3218f0f82f9","static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _c1afe3218f0f82f9();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.Char" /> value.</summary>
    [WhiteList("_ac2920ee8216c023","static System.Numerics.BigInteger.explicit operator char(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _ac2920ee8216c023();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Decimal" /> value.</summary>
    [WhiteList("_9d2085a2aa8febea","static System.Numerics.BigInteger.explicit operator System.Decimal(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static System.Decimal _9d2085a2aa8febea();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Double" /> value.</summary>
    [WhiteList("_4a6bc22c1d5cd472","static System.Numerics.BigInteger.explicit operator double(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _4a6bc22c1d5cd472();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.Half" /> value.</summary>
    [WhiteList("_7c41bbf7746a0266","static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static System.Half _7c41bbf7746a0266();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 16-bit signed integer value.</summary>
    [WhiteList("_c57fc79b767bf069","static System.Numerics.BigInteger.explicit operator short(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _c57fc79b767bf069();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 32-bit signed integer value.</summary>
    [WhiteList("_7c261f922cc43235","static System.Numerics.BigInteger.explicit operator int(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _7c261f922cc43235();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 64-bit signed integer value.</summary>
    [WhiteList("_15fe350cf299c580","static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static BigInt _15fe350cf299c580();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.Int128" /> value.</summary>
    [WhiteList("_5958070a15559320","static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static BigInt _5958070a15559320();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.IntPtr" /> value.</summary>
    [WhiteList("_11cea9efbc3d0c62","static System.Numerics.BigInteger.explicit operator nint(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static nint _11cea9efbc3d0c62();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a signed 8-bit value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int16" />.</summary>
    [WhiteList("_63d8cc7789144528","static System.Numerics.BigInteger.explicit operator sbyte(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _63d8cc7789144528();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a single-precision floating-point value.</summary>
    [WhiteList("_24972b9ed8006ec8","static System.Numerics.BigInteger.explicit operator float(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _24972b9ed8006ec8();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 16-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int32" />.</summary>
    [WhiteList("_b2311568a6faa3b8","static System.Numerics.BigInteger.explicit operator ushort(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _b2311568a6faa3b8();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 32-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int64" />.</summary>
    [WhiteList("_385437ecb9a2b10a","static System.Numerics.BigInteger.explicit operator uint(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static Number _385437ecb9a2b10a();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 64-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Double" />.</summary>
    [WhiteList("_6043725cddf263dd","static System.Numerics.BigInteger.explicit operator ulong(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static BigInt _6043725cddf263dd();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.UInt128" /> value.</summary>
    [WhiteList("_f8ae8a4213449843","static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static BigInt _f8ae8a4213449843();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.UIntPtr" /> value.</summary>
    [WhiteList("_bbf68528b2eedf55","static System.Numerics.BigInteger.explicit operator nuint(System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static nuint _bbf68528b2eedf55();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> object to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_8e505e0ce7efa99c","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Decimal)",WhiteListOp.Discard)]
	public extern static BigInt _8e505e0ce7efa99c();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Double" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_933b3164355c792a","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)",WhiteListOp.Discard)]
	public extern static BigInt _933b3164355c792a();

    ///<summary>Explicitly converts a <see cref="T:System.Half" /> value to a big integer.</summary>
    [WhiteList("_c186238bc3a46d2b","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Half)",WhiteListOp.Discard)]
	public extern static BigInt _c186238bc3a46d2b();

    ///<summary>Explicitly converts a <see cref="T:System.Numerics.Complex" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_088fa1b2a09829ce","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Numerics.Complex)",WhiteListOp.Discard)]
	public extern static BigInt _088fa1b2a09829ce();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Single" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_212b6e60ce4e6836","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(float)",WhiteListOp.Discard)]
	public extern static BigInt _212b6e60ce4e6836();

    ///<summary>Defines an implicit conversion of an unsigned byte to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_24f94dfe434ed1de","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(byte)",WhiteListOp.Discard)]
	public extern static BigInt _24f94dfe434ed1de();

    ///<summary>Implicitly converts a <see cref="T:System.Char" /> value to a big integer.</summary>
    [WhiteList("_6f52f939cef7ebfc","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(char)",WhiteListOp.Discard)]
	public extern static BigInt _6f52f939cef7ebfc();

    ///<summary>Defines an implicit conversion of a signed 16-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_5eb359c063a4b04b","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(short)",WhiteListOp.Discard)]
	public extern static BigInt _5eb359c063a4b04b();

    ///<summary>Defines an implicit conversion of a signed 32-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_84639f9693379307","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(int)",WhiteListOp.Discard)]
	public extern static BigInt _84639f9693379307();

    ///<summary>Defines an implicit conversion of a signed 64-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_7de492bb278503c8","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(long)",WhiteListOp.Discard)]
	public extern static BigInt _7de492bb278503c8();

    ///<summary>Implicitly converts a <see cref="T:System.Int128" /> value to a big integer.</summary>
    [WhiteList("_aa5bafc867e9b5eb","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.Int128)",WhiteListOp.Discard)]
	public extern static BigInt _aa5bafc867e9b5eb();

    ///<summary>Implicitly converts a <see cref="T:System.IntPtr" /> value to a big integer.</summary>
    [WhiteList("_70a902bafd0ce64e","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(nint)",WhiteListOp.Discard)]
	public extern static BigInt _70a902bafd0ce64e();

    ///<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.#ctor(System.Int32)" />.</summary>
    [WhiteList("_ff8ba3cf17ec3f75","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(sbyte)",WhiteListOp.Discard)]
	public extern static BigInt _ff8ba3cf17ec3f75();

    ///<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.op_Implicit(System.Int32)~System.Numerics.BigInteger" />.</summary>
    [WhiteList("_9b2419d65cfa19ab","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ushort)",WhiteListOp.Discard)]
	public extern static BigInt _9b2419d65cfa19ab();

    ///<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.op_Implicit(System.Int64)~System.Numerics.BigInteger" />.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_cf078fbbc4130e0c","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(uint)",WhiteListOp.Discard)]
	public extern static BigInt _cf078fbbc4130e0c();

    ///<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Double" />.</summary>
    [WhiteList("_9b4a5ecbd0f90bd4","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ulong)",WhiteListOp.Discard)]
	public extern static BigInt _9b4a5ecbd0f90bd4();

    ///<summary>Implicitly converts a <see cref="T:System.UInt128" /> value to a big integer.</summary>
    [WhiteList("_16f7ae7cb82a7523","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.UInt128)",WhiteListOp.Discard)]
	public extern static BigInt _16f7ae7cb82a7523();

    ///<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a big integer.</summary>
    [WhiteList("_b7ee0d78d7054a45","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(nuint)",WhiteListOp.Discard)]
	public extern static BigInt _b7ee0d78d7054a45();

    ///<summary>Performs a bitwise <see langword="And" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_4a529c0a5388c594","static System.Numerics.BigInteger.operator &(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} & @#{1}")]
	public extern static BigInt _4a529c0a5388c594(BigInt left, BigInt right);

    ///<summary>Performs a bitwise <see langword="Or" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_752fd4cd29f4f204","static System.Numerics.BigInteger.operator |(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} | @#{1}")]
	public extern static BigInt _752fd4cd29f4f204(BigInt left, BigInt right);

    ///<summary>Performs a bitwise exclusive <see langword="Or" /> (<see langword="XOr" />) operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_a453418c13f7f875","static System.Numerics.BigInteger.operator ^(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} ^ @#{1}")]
	public extern static BigInt _a453418c13f7f875(BigInt left, BigInt right);

    ///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the left.</summary>
    [WhiteList("_a29a9a670145ce5e","static System.Numerics.BigInteger.operator <<(System.Numerics.BigInteger, int)",WhiteListOp.Literal, "@#{0} << @#{1}")]
	public extern static BigInt _a29a9a670145ce5e(BigInt value, Number shift);

    ///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the right.</summary>
    [WhiteList("_c0bed6f115403624","static System.Numerics.BigInteger.operator >>(System.Numerics.BigInteger, int)",WhiteListOp.Literal, "@#{0} >> @#{1}")]
	public extern static BigInt _c0bed6f115403624(BigInt value, Number shift);

    ///<summary>Returns the bitwise one's complement of a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_9182cf8afd9b8590","static System.Numerics.BigInteger.operator ~(System.Numerics.BigInteger)",WhiteListOp.Literal, "~@#{0}")]
	public extern static BigInt _9182cf8afd9b8590(BigInt value);

    ///<summary>Negates a specified BigInteger value.</summary>
    [WhiteList("_03be17d45cbe5034","static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger)",WhiteListOp.Literal, "-@#{0}")]
	public extern static BigInt _03be17d45cbe5034(BigInt value);

    ///<summary>Returns the value of the <see cref="T:System.Numerics.BigInteger" /> operand. (The sign of the operand is unchanged.)</summary>
    [WhiteList("_7096f6c4ea9fddaf","static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger)",WhiteListOp.Literal, "+@#{0}")]
	public extern static BigInt _7096f6c4ea9fddaf(BigInt value);

    ///<summary>Increments a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
    [WhiteList("_cc35859d07374d52","static System.Numerics.BigInteger.operator ++(System.Numerics.BigInteger)",WhiteListOp.Literal, "++@#{0}")]
	public extern static BigInt _cc35859d07374d52(BigInt value);

    ///<summary>Decrements a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
    [WhiteList("_6d2fe51e4158a46f","static System.Numerics.BigInteger.operator --(System.Numerics.BigInteger)",WhiteListOp.Literal, "--@#{0}")]
	public extern static BigInt _6d2fe51e4158a46f(BigInt value);

    ///<summary>Adds the values of two specified <see cref="T:System.Numerics.BigInteger" /> objects.</summary>
    [WhiteList("_4edde875924e9396","static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} + @#{1}")]
	public extern static BigInt _4edde875924e9396(BigInt left, BigInt right);

    ///<summary>Multiplies two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_3baa1e316a9a8e5c","static System.Numerics.BigInteger.operator *(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} * @#{1}")]
	public extern static BigInt _3baa1e316a9a8e5c(BigInt left, BigInt right);

    ///<summary>Divides a specified <see cref="T:System.Numerics.BigInteger" /> value by another specified <see cref="T:System.Numerics.BigInteger" /> value by using integer division.</summary>
    [WhiteList("_e87ac03cab9bfae9","static System.Numerics.BigInteger.operator /(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} / @#{1}")]
	public extern static BigInt _e87ac03cab9bfae9(BigInt dividend, BigInt divisor);

    ///<summary>Returns the remainder that results from division with two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    [WhiteList("_44f6e17ba281115c","static System.Numerics.BigInteger.operator %(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} % @#{1}")]
	public extern static BigInt _44f6e17ba281115c(BigInt dividend, BigInt divisor);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_c921d6c6bf72edae","static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < @#{1}")]
	public extern static bool _c921d6c6bf72edae(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_4175fbcd1bdcbb81","static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} <= @#{1}")]
	public extern static bool _4175fbcd1bdcbb81(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_38487ed1f787d018","static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} > @#{1}")]
	public extern static bool _38487ed1f787d018(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_c69d246ab7c4d01a","static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} >= @#{1}")]
	public extern static bool _c69d246ab7c4d01a(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether the values of two <see cref="T:System.Numerics.BigInteger" /> objects are equal.</summary>
    [WhiteList("_a1bca47181bf0a21","static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} == @#{1}")]
	public extern static bool _a1bca47181bf0a21(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether two <see cref="T:System.Numerics.BigInteger" /> objects have different values.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("_fa04bb024b763d8c","static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} != @#{1}")]
	public extern static bool _fa04bb024b763d8c(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit signed integer.</summary>
    [WhiteList("_54b970a90a63bed7","static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, long)",WhiteListOp.Literal, "@#{0} < @#{1}")]
	public extern static bool _54b970a90a63bed7(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit signed integer.</summary>
    [WhiteList("_c5121fb5bb0459d9","static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, long)",WhiteListOp.Literal, "@#{0} <= @#{1}")]
	public extern static bool _c5121fb5bb0459d9(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> is greater than a 64-bit signed integer value.</summary>
    [WhiteList("_f633b7dba945231e","static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, long)",WhiteListOp.Literal, "@#{0} > @#{1}")]
	public extern static bool _f633b7dba945231e(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit signed integer value.</summary>
    [WhiteList("_05b14b64a3ed932c","static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, long)",WhiteListOp.Literal, "@#{0} >= @#{1}")]
	public extern static bool _05b14b64a3ed932c(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a signed long integer value are equal.</summary>
    [WhiteList("_23fab9d29faa7b4b","static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, long)",WhiteListOp.Literal, "@#{0} == @#{1}")]
	public extern static bool _23fab9d29faa7b4b(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit signed integer are not equal.</summary>
    [WhiteList("_bee7ae6c7fd4ccab","static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, long)",WhiteListOp.Literal, "@#{0} != @#{1}")]
	public extern static bool _bee7ae6c7fd4ccab(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_9e956828e15a31ac","static System.Numerics.BigInteger.operator <(long, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < @#{1}")]
	public extern static bool _9e956828e15a31ac(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_837c7f79427d7687","static System.Numerics.BigInteger.operator <=(long, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} <= @#{1}")]
	public extern static bool _837c7f79427d7687(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is greater than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_599b5035513f4697","static System.Numerics.BigInteger.operator >(long, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} > @#{1}")]
	public extern static bool _599b5035513f4697(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_b8852469edf6fccb","static System.Numerics.BigInteger.operator >=(long, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} >= @#{1}")]
	public extern static bool _b8852469edf6fccb(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a signed long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
    [WhiteList("_17b7667af4b23f69","static System.Numerics.BigInteger.operator ==(long, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} == @#{1}")]
	public extern static bool _17b7667af4b23f69(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
    [WhiteList("_d3215df5ab1e7b4b","static System.Numerics.BigInteger.operator !=(long, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} != @#{1}")]
	public extern static bool _d3215df5ab1e7b4b(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit unsigned integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_1f387e21472ec766","static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, ulong)",WhiteListOp.Literal, "@#{0} < @#{1}")]
	public extern static bool _1f387e21472ec766(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit unsigned integer.</summary>
    [WhiteList("_8bf92299327b0564","static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, ulong)",WhiteListOp.Literal, "@#{0} <= @#{1}")]
	public extern static bool _8bf92299327b0564(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
    [WhiteList("_85a372957af0ef3d","static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, ulong)",WhiteListOp.Literal, "@#{0} > @#{1}")]
	public extern static bool _85a372957af0ef3d(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit unsigned integer value.</summary>
    [WhiteList("_027db5ec51a792f8","static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, ulong)",WhiteListOp.Literal, "@#{0} >= @#{1}")]
	public extern static bool _027db5ec51a792f8(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and an unsigned long integer value are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_90393e5796d20760","static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, ulong)",WhiteListOp.Literal, "@#{0} == @#{1}")]
	public extern static bool _90393e5796d20760(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit unsigned integer are not equal.</summary>
    [WhiteList("_83ed1eafdd051a37","static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, ulong)",WhiteListOp.Literal, "@#{0} != @#{1}")]
	public extern static bool _83ed1eafdd051a37(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_229e97bf2be53319","static System.Numerics.BigInteger.operator <(ulong, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < @#{1}")]
	public extern static bool _229e97bf2be53319(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_7f4a3ea98d5e7194","static System.Numerics.BigInteger.operator <=(ulong, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} <= @#{1}")]
	public extern static bool _7f4a3ea98d5e7194(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
    [WhiteList("_e54a63c735fb2514","static System.Numerics.BigInteger.operator >(ulong, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} > @#{1}")]
	public extern static bool _e54a63c735fb2514(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    [WhiteList("_3a5b1bba5ac45b9c","static System.Numerics.BigInteger.operator >=(ulong, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} >= @#{1}")]
	public extern static bool _3a5b1bba5ac45b9c(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether an unsigned long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
    [WhiteList("_a97fbe9f639a835b","static System.Numerics.BigInteger.operator ==(ulong, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} == @#{1}")]
	public extern static bool _a97fbe9f639a835b(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
    [WhiteList("_ac5266c1db09af16","static System.Numerics.BigInteger.operator !=(ulong, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} != @#{1}")]
	public extern static bool _ac5266c1db09af16(BigInt left, BigInt right);

    ///<summary>Gets the number of bits required for shortest two's complement representation of the current instance without the sign bit.</summary>
    [WhiteList("_41fe76dfb4ee2ab2","System.Numerics.BigInteger.GetBitLength()",WhiteListOp.Allowed)]
	public static BigInt _41fe76dfb4ee2ab2(BigInt instance)
	{
		if (instance == BigInt.Zero)
			return BigInt.Zero;

		var isNegative = instance < BigInt.Zero;
		var value = isNegative ? -instance - BigInt.One : instance;
		var bitLength = BigInt.Zero;

		while (value > BigInt.Zero)
		{
			bitLength += BigInt.One;
			value >>= BigInt.One;
		}

		return bitLength;
	}

	///<summary>Computes the quotient and remainder of two values.</summary>
	[WhiteList("_22a21ffe19479f32","static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static (BigInt, BigInt) _22a21ffe19479f32(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new RangeError("Division by zero");

		var quotient = left / right;
		var remainder = left % right;

		return (quotient, remainder);
	}

	///<summary>Computes the number of leading zeros in a value.</summary>
	[WhiteList("_276680abacb93277","static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _276680abacb93277(BigInt value)
	{
		if (value == BigInt.Zero)
			return BigInt.Zero;

		// BigInt 是任意精度，没有固定的位宽，因此没有前导零的概念
		// 返回 0 表示值是从第一位开始的
		return BigInt.Zero;
	}

	///<summary>Computes the number of bits that are set in a value.</summary>
	[WhiteList("_5e476c376aca56ae","static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _5e476c376aca56ae(BigInt value)
	{
		if (value == BigInt.Zero)
			return BigInt.Zero;

		var count = BigInt.Zero;
		var n = value < BigInt.Zero ? -value - BigInt.One : value;

		// Brian Kernighan算法
		while (n > BigInt.Zero)
		{
			n &= n - BigInt.One;
			count += BigInt.One;
		}

		return count;
	}

	///<summary>Rotates a value left by a given amount.</summary>
	///<param name="value">The value that's rotated left by <paramref name="rotateAmount" />.</param>
	///<param name="rotateAmount">The amount by which <paramref name="value" /> is rotated left.</param>
	///<returns>The result of rotating <paramref name="value" /> left by <paramref name="rotateAmount" />.</returns>
	[WhiteList("_ae7b1dd18af32f04","static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)",WhiteListOp.Allowed)]
	public static BigInt _ae7b1dd18af32f04(BigInt value, Number rotateAmount)
	{
		if (value == BigInt.Zero)
			return BigInt.Zero;

		var bitLength = value.ToString(2).Length;

		var ra = rotateAmount % bitLength;
		if (ra < 0)
			ra += bitLength;

		if (ra == 0)
			return value;

		var mask = (BigInt.One << BigInt(ra)) - BigInt.One;
		var rotatedOutBits = (value >> BigInt(bitLength - ra)) & mask;
		var result = ((value << BigInt(ra)) | rotatedOutBits) & ((BigInt.One << BigInt(bitLength)) - BigInt.One);

		return result;
	}

	///<summary>Rotates a value right by a given amount.</summary>
	[WhiteList("_dc8cc860511e78b3","static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)",WhiteListOp.Allowed)]
	public static BigInt _dc8cc860511e78b3(BigInt value, Number rotateAmount)
	{
		if (rotateAmount == 0)
			return value;

		// Handle zero value
		if (value == BigInt.Zero)
			return BigInt.Zero;


		// Calculate the number of bits
		var temp = value;
		var bitLength = 0;
		while (temp > BigInt.Zero)
		{
			bitLength++;
			temp >>= BigInt.One;
		}

		// Handle negative rotateAmount (convert to left rotation)
		if (rotateAmount < 0)
		{
			var absAmount = -rotateAmount;
			absAmount %= bitLength;
			if (absAmount == 0)
				return value;

			return (value << BigInt(absAmount)) | (value >> BigInt(bitLength - absAmount));
		}

		// Normalize rotateAmount to be within [0, bitLength)
		var ra = rotateAmount % bitLength;
		if (ra == 0)
			return value;

		// Perform right rotation
		var rightPart = value >> BigInt(ra);
		var leftPart = value & ((BigInt.One << BigInt(ra)) - BigInt.One);
		var rotated = (leftPart << BigInt(bitLength - ra)) | rightPart;

		return rotated;
	}

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[WhiteList("_696502aae4b6e182","static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _696502aae4b6e182(BigInt value)
	{
		// Handle zero value
		if (value == BigInt.Zero)
			return BigInt.Zero;

		var count = BigInt.Zero;
		var temp = value;

		// Count trailing zeros by repeatedly shifting right until the least significant bit is 1
		while ((temp & BigInt.One) == BigInt.Zero)
		{
			count++;
			temp >>= BigInt.One;
		}

		return count;
	}

	///<summary>Determines if a value is a power of two.</summary>
	[WhiteList("_c0651d019a4b12b1","static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static bool _c0651d019a4b12b1(BigInt value)
	{
		// Negative numbers and zero are not powers of two
		if (value <= BigInt.Zero)
			return false;

		// A number is a power of two if it has exactly one bit set
		// Check using: (value & (value - 1n)) === 0n
		var minusOne = value - BigInt.One;
		var result = (value & minusOne) == BigInt.Zero;

		return result;
	}

	///<summary>Computes the log2 of a value.</summary>
	[WhiteList("_c29a05a989ec3b33","static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)",WhiteListOp.Allowed)]
	public static BigInt _c29a05a989ec3b33(BigInt value)
	{
		// Logarithm is undefined for non-positive numbers
		if (value <= BigInt.Zero)
			throw new RangeError("value must be positive");

		var result = BigInt.Zero;
		var temp = value;

		// Count how many times we can divide by 2 until we reach 1
		while (temp > BigInt.One)
		{
			result++;
			temp >>= BigInt.One;
		}

		return result;
	}

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[WhiteList("_8548cc83c4d947f5","static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public static BigInt _8548cc83c4d947f5(BigInt value, BigInt min, BigInt max)
	{
		// Validate that min <= max
		if (min > max)
			throw new RangeError("min must be less than or equal to max");

		var result = value;

		// Clamp to minimum
		if (result < min)
			result = min;

		// Clamp to maximum
		if (result > max)
			result = max;

		return result;
	}

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[WhiteList("_aa45b92454e3abaa","static System.Numerics.BigInteger.CopySign(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "(@#{1} < 0n ? -1 : 1)*(@#{0} < 0n ? -@#{0} : @#{0})")]
	public extern static BigInt _aa45b92454e3abaa(BigInt value, BigInt sign);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    [WhiteList("_8cbca5624f4a6cc0","static System.Numerics.BigInteger.CreateChecked<TOther>(TOther)",WhiteListOp.Allowed)]
	public static BigInt _8cbca5624f4a6cc0<TOther>(TOther value)
	{
		// Handle BigInt input directly
		if (value is BigInt b)
			return b;

		// Handle number input
		if (value is Number n)
		{
			// Check for non-integer numbers
			if (!Number.IsInteger(n))
				throw new RangeError("Value must be an integer");

			// Check for safe integer range
			if (n < Number.MIN_SAFE_INTEGER || n > Number.MAX_SAFE_INTEGER)
				throw new RangeError("Value is outside safe integer range");

			// Convert to BigInt
			return BigInt(n);
		}

		// Handle string input
		if (value is string s)
		{
			// Trim whitespace
			var trimmed = s.Trim();

			// Validate integer format (optional sign + digits)
			if (!RegExp(@"^-?\d+$").Test(trimmed))
				throw new RangeError("String must represent a valid integer");

			// Convert to BigInt
			try
			{
				return BigInt(trimmed);
			}
			catch (Error e)
			{
				throw new RangeError("Invalid integer string");
			}
		}

		// Handle boolean input
		if (value is bool bl)
			return bl ? BigInt.One : BigInt.Zero;


		// Handle null/undefined
		if (value is null)
			throw new RangeError("Value cannot be null or undefined");

		// Handle other types (objects, symbols, etc.)
		throw new RangeError("Unsupported type for conversion to BigInt");
	}

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[WhiteList("_f2847eb63549bd6a","static System.Numerics.BigInteger.CreateSaturating<TOther>(TOther)",WhiteListOp.Discard)]
	public extern static BigInt _f2847eb63549bd6a<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    [WhiteList("_8457175b141355fe","static System.Numerics.BigInteger.CreateTruncating<TOther>(TOther)",WhiteListOp.Discard)]
	public extern static BigInt _8457175b141355fe<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    [WhiteList("_691c1425b8fac31f","static System.Numerics.BigInteger.IsEvenInteger(System.Numerics.BigInteger)",WhiteListOp.Literal, "(@#{0} & 1n) === 0n")]
	public extern static bool _691c1425b8fac31f(BigInt value);

    ///<summary>Determines if a value is negative.</summary>
    [WhiteList("_8cb55ab054b637db","static System.Numerics.BigInteger.IsNegative(System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < 0n")]
	public extern static bool _8cb55ab054b637db(BigInt value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    [WhiteList("_8213026f03b857e7","static System.Numerics.BigInteger.IsOddInteger(System.Numerics.BigInteger)",WhiteListOp.Literal, "(@#{0} & 1n) === 1n")]
	public extern static bool _8213026f03b857e7(BigInt value);

    ///<summary>Determines if a value is positive.</summary>
    [WhiteList("_386d048147df6eae","static System.Numerics.BigInteger.IsPositive(System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} >= 0n")]
	public extern static bool _386d048147df6eae(BigInt value);

    ///<summary>Compares two values to compute which is greater.</summary>
    [WhiteList("_d305de2c64e85995","static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} > @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _d305de2c64e85995(BigInt x, BigInt y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    [WhiteList("_fef56ccd17b22e88","static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)",WhiteListOp.Literal, "@#{0} < @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _fef56ccd17b22e88(BigInt x, BigInt y);

    ///<summary>Tries to parse a string into a value.</summary>
    [WhiteList("_10999a356af78aba","static System.Numerics.BigInteger.TryParse(string, System.IFormatProvider, out System.Numerics.BigInteger)",WhiteListOp.Discard)]
	public extern static bool _10999a356af78aba(string? s, Intl.NumberFormat? provider, OutValue<BigInt?> result);

    ///<summary>Shifts a value right by a given amount.</summary>
    [WhiteList("_49adf7adfc1228f8","static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)",WhiteListOp.Allowed)]
	public static BigInt _49adf7adfc1228f8(BigInt value, Number shiftAmount)
	{
		if (shiftAmount < 0)
			throw new RangeError("Shift amount must be non-negative");

		var shift = BigInt(shiftAmount);

		if (value >= BigInt.Zero)
			return value >> shift;

		// BigInt 没有原生的 >>> 运算符（JavaScript 的 >>> 仅适用于 Number）
		// 对于负数，抛出异常说明不支持
		throw new Error("Unsigned right shift (>>>) is not supported for BigInt in JavaScript");
	}

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("_8bbfd46a98ce5419","static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)",WhiteListOp.Discard)]
	public extern static BigInt _8bbfd46a98ce5419(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    [WhiteList("_163b02803ece1f0c","static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Numerics.BigInteger)", WhiteListOp.Discard)]
	public extern static bool _163b02803ece1f0c(Uint32Array s, Intl.NumberFormat? provider, OutValue<BigInt> result);
}
