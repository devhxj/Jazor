namespace Jazor.CLR;

[ECMAScriptModule("System/Numerics/BigIntegerModule.js")]
[Jazor(Op.Alias, "System.Numerics.BigInteger", "BigInt")]
public static class BigIntegerModule
{
	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 32-bit signed integer value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(int)")]
	public extern static BigInt _ba6e0e86598dc8b2(Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using an unsigned 32-bit integer value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(uint)")]
	public extern static BigInt _b7b735a5d507d449(Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 64-bit signed integer value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(long)")]
	public extern static BigInt _74973910762e0e86(BigInt value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure with an unsigned 64-bit integer value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(ulong)")]
	public extern static BigInt _0421ba6c202fdc80(BigInt value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a single-precision floating-point value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(float)")]
	public extern static BigInt _cfd2038efd505e1f(Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a double-precision floating-point value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(double)")]
	public extern static BigInt _38c7caccfd5e120e(Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.BigInteger(System.Decimal)")]
	public extern static BigInt _f715f85cc5dcfe92(object value);

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a byte array.</summary>
	[Jazor(Op.Import ,"System.Numerics.BigInteger.BigInteger(byte[])")]
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
			result = (result << BigInt_(64)) | view.GetBigUint64(i, false);

		// 处理剩余字节（最多7字节）
		if (i < value.Length)
		{
			var remaining = BigInt.Zero;
			for (; i < value.Length; i++)
				remaining = (remaining << BigInt_(8)) | BigInt_(value[i]);

			result = (result << BigInt_((value.Length - i) * 8u)) | remaining;
		}

		return result;
	}	

	///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a read-only span of bytes, and optionally indicating the signing encoding and the endianness byte order.</summary>
	[Jazor(Op.Import ,"System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)")]
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
				return BigInt_(value[0]);
			}
			else
			{
				// 有符号数：如果最高位为1，则为负数
				return (value[0] & 0x80) == 0
					? BigInt_(value[0])
					: BigInt_(value[0]) - BigInt_(0x100);
			}
		}

		// 处理标准长度（2/4/8字节）
		if (value.Length <= 8)
		{
			var buffer = new ArrayBuffer(value.Length);
			var view = new DataView(buffer);
			value.ForEach((item, index) => view.SetUint8(index, item));

			if (value.Length == 2)
				return isUnsigned
					? BigInt_(view.GetUint16(0, !isBigEndian))
					: BigInt_(view.GetInt16(0, !isBigEndian));

			if (value.Length == 4)
				return isUnsigned
					? BigInt_(view.GetUint32(0, !isBigEndian))
					: BigInt_(view.GetInt32(0, !isBigEndian));

			if (value.Length == 8)
				return isUnsigned
					? view.GetBigUint64(0, !isBigEndian)
					: view.GetBigInt64(0, !isBigEndian);

			// 3/5/6/7字节长度使用非标准处理
			return ProcessNonStandardLength(value, isUnsigned, isBigEndian);
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
				var bitWidth = BigInt_(processedBytes.Length) * BigInt_(8);

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
				result = (result << BigInt_(8)) | BigInt_(littleEndianBytes[i] & 0xFF);
			}

			return result;
		}
	}

	/// <summary>
	/// C#: BigInteger.Zero
	/// JS: 0n
	/// </summary>
	[Jazor(Op.Inline, "static System.Numerics.BigInteger.Zero.get", "0n")]
	public extern static BigInt _77fc63f99954f8da();

	/// <summary>
	/// C#: BigInteger.One
	/// JS: 1n
	/// </summary>
	[Jazor(Op.Inline, "static System.Numerics.BigInteger.One.get", "1n")]
	public extern static BigInt _9c5419989e842d00();

	/// <summary>
	/// C#: BigInteger.MinusOne
	/// JS: -1n
	/// </summary>
	[Jazor(Op.Inline, "static System.Numerics.BigInteger.MinusOne.get", "-1n")]
	public extern static BigInt _01c112900aa52c82();

	/// <summary>
	/// C#: bigint.IsPowerOfTwo
	/// JS: instance > 0n && (instance & (instance - 1n)) === 0n
	/// </summary>
	[Jazor(Op.Inline, "System.Numerics.BigInteger.IsPowerOfTwo.get", "(__arg1 > 0n && (__arg1 & (__arg1 - 1n)) === 0n)")]
	public extern static bool _ee8564f940baf789(BigInt instance);

	/// <summary>
	/// C#: bigint.IsZero
	/// JS: instance === 0n
	/// </summary>
	[Jazor(Op.Inline, "System.Numerics.BigInteger.IsZero.get", "(__arg1 === 0n)")]
	public extern static bool _c138b3f4dd057592(BigInt instance);

	/// <summary>
	/// C#: bigint.IsOne
	/// JS: instance === 1n
	/// </summary>
	[Jazor(Op.Inline, "System.Numerics.BigInteger.IsOne.get", "(__arg1 === 1n)")]
	public extern static bool _2aa0739f87c79906(BigInt instance);

	/// <summary>
	/// C#: bigint.IsEven
	/// JS: instance % 2n === 0n
	/// </summary>
	[Jazor(Op.Inline, "System.Numerics.BigInteger.IsEven.get", "(__arg1 % 2n === 0n)")]
	public extern static bool _4a465705ad4dc8ca(BigInt instance);

	/// <summary>
	/// C#: bigint.Sign
	/// JS: Returns -1, 0, or 1 indicating the sign
	/// </summary>
	[Jazor(Op.Import, "System.Numerics.BigInteger.Sign.get")]
	public static Number _734290a188c5bc5a(BigInt instance)
	{
		if (instance > BigInt.Zero) return 1;
		if (instance < BigInt.Zero) return -1;
		return 0;
	}

	/// <summary>
	/// C#: BigInteger.Parse(value)
	/// JS: BigInt(value.trim()) with validation
	/// </summary>
	[Jazor(Op.Import, "static System.Numerics.BigInteger.Parse(string)")]
	public static BigInt _155212572c9a3297(string? value)
	{
		if (value == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		var trimmed = value.Trim();
		if (trimmed.Length == 0)
			throw new Error("FormatException: The input string was not in a correct format.");

		try
		{
			return BigInt_(trimmed);
		}
		catch
		{
			throw new Error($"FormatException: The input string '{value}' was not in a correct format.");
		}
	}

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles)")]
	public extern static BigInt _a077721686cadcd9(string value, object style);

	///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.Parse(string, System.IFormatProvider)")]
	public extern static BigInt _d1543aa14ab94729(string value, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _8adf758c3f22af12(string value, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import, "static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)")]
	public static Array<object?> _59acea2facdaa757(string? value, BigInt? result)
	{
		if (value == null || value.Length == 0)
			return [false, BigInt.Zero];

		try
		{
			var trimmed = value.Trim();
			if (trimmed.Length == 0)
				return [false, BigInt.Zero];

			return [true, BigInt_(trimmed)];
		}
		catch
		{
			return [false, BigInt.Zero];
		}
	}

	///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)")]
	public extern static Array<object?> _85cd9c4a9c2dadf4(string? value, object style, Intl.NumberFormat? provider, BigInt? result);

	///<summary>Converts the representation of a number, contained in the specified read-only span of characters, in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static BigInt _00d39f2029fd4266(Uint32Array value, object style, Intl.NumberFormat? provider);

	///<summary>Tries to convert the representation of a number contained in the specified read-only character span, to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)")]
	public extern static Array<object?> _ded03bf84977945f(Uint32Array value, BigInt result);

	///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)")]
	public extern static Array<object?> _d733f0a0a427d970(Uint32Array value, object style, Intl.NumberFormat? provider, BigInt result);

	/// <summary>
	/// C#: BigInteger.Compare(left, right)
	/// JS: left < right ? -1 : (left > right ? 1 : 0)
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Compare(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _0a6134f61ab96205(BigInt left, BigInt right);

	/// <summary>
	/// C#: BigInteger.Abs(value)
	/// JS: value < 0n ? -value : value
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Abs(System.Numerics.BigInteger)", "(__arg1 < 0n ? -__arg1 : __arg1)")]
	public extern static BigInt _efd2134803006c44(BigInt value);

	/// <summary>
	/// C#: BigInteger.Add(left, right)
	/// JS: left + right
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Add(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 + __arg2)")]
	public extern static BigInt _0034b6a7a416df8e(BigInt left, BigInt right);

	/// <summary>
	/// C#: BigInteger.Subtract(left, right)
	/// JS: left - right
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Subtract(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 - __arg2)")]
	public extern static BigInt _31de7c0189a18bd2(BigInt left, BigInt right);

	/// <summary>
	/// C#: BigInteger.Multiply(left, right)
	/// JS: left * right
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Multiply(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 * __arg2)")]
	public extern static BigInt _8c06584cae9fcbe7(BigInt left, BigInt right);

	/// <summary>
	/// C#: BigInteger.Divide(dividend, divisor)
	/// JS: dividend / divisor
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Divide(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 / __arg2)")]
	public extern static BigInt _7ff5692b085214c4(BigInt dividend, BigInt divisor);

	/// <summary>
	/// C#: BigInteger.Remainder(dividend, divisor)
	/// JS: dividend % divisor
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Remainder(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 % __arg2)")]
	public extern static BigInt _00d98488c7edf612(BigInt dividend, BigInt divisor);

	///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another, returns the result, and returns the remainder in an output parameter.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)")]
	public static Array<object?> _598611fb2b8a064a(BigInt dividend, BigInt divisor, BigInt remainder)
	{
		if (divisor == BigInt.Zero)
			throw new RangeError("Division by zero");

		var quotient = dividend / divisor;
		var rem = dividend % divisor;
		return [quotient, rem];
	}

	/// <summary>
	/// C#: BigInteger.Negate(value)
	/// JS: -value
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Negate(System.Numerics.BigInteger)", "(-__arg1)")]
	public extern static BigInt _d160232d04d4f8fe(BigInt value);

	///<summary>Returns the natural (base <see langword="e" />) logarithm of a specified number.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)")]
	public static Number _fb5a811e7a32a324(BigInt value)
	{
		if (value <= BigInt.Zero)
			throw new Error("Logarithm is undefined for non-positive numbers");

		var str = value.ToString()!;
		var exponent = str.Length - 1;
		var mantissa = Number_(str.Substring(0, 15));

		return Math.Log(mantissa) + exponent * Math.Log(10);
	}	

	///<summary>Returns the logarithm of a specified number in a specified base.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)")]
	public static Number _acb5aef300c8db0c(BigInt value, Number baseValue)
	{
		if (value <= BigInt.Zero)
			throw new RangeError("Logarithm is undefined for non-positive numbers");

		if (baseValue <= 0 || baseValue == 1)
			throw new RangeError("Base must be positive and not equal to 1");

		if (value == BigInt.One)
			return 0;

		if (baseValue == Math.E)
			return Math.Log(Number_(value));

		if (value <= Number.MAX_SAFE_INTEGER)
			return Math.Log(Number_(value)) / Math.Log_(baseValue);

		var str = value.ToString()!;
		var digitCount = str.Length;
		var significantDigits = str.Substring(0, 15);
		var mantissa = ParseFloat(significantDigits) / Math.Pow(10, significantDigits.Length - 1);
		var exponent = digitCount - 1;
		var lnValue = Math.Log(mantissa) + exponent * Math.LN10;
		var lnBase = Math.Log(baseValue);

		return lnValue / lnBase;
	}

	///<summary>Returns the base 10 logarithm of a specified number.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.Log10(System.Numerics.BigInteger)")]
	public static Number _f276cbd7c3b305ea(BigInt value)
	{
		if (value <= BigInt.Zero)
			throw new RangeError("Logarithm is undefined for non-positive numbers");

		if (value == BigInt.One)
			return 0;

		var str = value.ToString()!;
		return (str.Length <= 15)
			? Math.Log10(Number_(value))
			: Math.Log10(Number_(str.Substring(0, 15))) + (str.Length - 15);
	}	

	///<summary>Finds the greatest common divisor of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
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

	/// <summary>
	/// C#: BigInteger.Max(left, right)
	/// JS: left > right ? left : right
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Max(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 > __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _a038619e95a6c0ff(BigInt left, BigInt right);

	/// <summary>
	/// C#: BigInteger.Min(left, right)
	/// JS: left < right ? left : right
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.Min(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg1 < __arg2 ? __arg1 : __arg2)")]
	public extern static BigInt _b3b093dd81ed2d15(BigInt left, BigInt right);

	///<summary>Performs modulus division on a number raised to the power of another number.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)")]
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
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)")]
	public static BigInt _31cf4d89164dee40(BigInt value, Number exponent)
	{
		if (exponent < 0 || !Number.IsInteger(exponent))
			throw new RangeError("The exponent must be a non-negative integer");

		var result = BigInt.One;
		var current = value;
		Number exp = exponent;

		while (exp > 0)
		{
			if (exp % 2 == 1)
				result *= current;

			current *= current;
			exp = Math.Floor_(exp / 2);
		}

		return result;
	}	

	///<summary>Returns the hash code for the current <see cref="T:System.Numerics.BigInteger" /> object.</summary>
	[Jazor(Op.Discard ,"override System.Numerics.BigInteger.GetHashCode()")]
	public extern static Number _fe64082374302a77(BigInt instance);

	/// <summary>
	/// C#: value.Equals(obj)
	/// JS: value === obj
	/// </summary>
	[Jazor(Op.Inline ,"override System.Numerics.BigInteger.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _27c2f0d965e3403d(BigInt instance, object? obj);

	/// <summary>
	/// C#: value.Equals(other)
	/// JS: value === other
	/// </summary>
	[Jazor(Op.Inline ,"System.Numerics.BigInteger.Equals(long)", "(__arg1 === __arg2)")]
	public extern static bool _21afeec99b7ab2ca(BigInt instance, BigInt other);

	/// <summary>
	/// C#: value.Equals(other)
	/// JS: value === other
	/// </summary>
	[Jazor(Op.Inline ,"System.Numerics.BigInteger.Equals(ulong)", "(__arg1 === __arg2)")]
	public extern static bool _134be6ec440e455e(BigInt instance, BigInt other);

	/// <summary>
	/// C#: value.Equals(other)
	/// JS: value === other
	/// </summary>
	[Jazor(Op.Inline ,"System.Numerics.BigInteger.Equals(System.Numerics.BigInteger)", "(__arg1 === __arg2)")]
	public extern static bool _4d44e94420c56981(BigInt instance, BigInt other);

	/// <summary>
	/// C#: value.CompareTo(other)
	/// JS: value < other ? -1 : (value > other ? 1 : 0)
	/// </summary>
	[Jazor(Op.Inline ,"System.Numerics.BigInteger.CompareTo(long)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _77851a1e7ef48cb7(BigInt instance, BigInt other);

	/// <summary>
	/// C#: value.CompareTo(other)
	/// JS: value < other ? -1 : (value > other ? 1 : 0)
	/// </summary>
	[Jazor(Op.Inline ,"System.Numerics.BigInteger.CompareTo(ulong)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _64e348c0c7830a5c(BigInt instance, BigInt other);

	/// <summary>
	/// C#: value.CompareTo(other)
	/// JS: value < other ? -1 : (value > other ? 1 : 0)
	/// </summary>
	[Jazor(Op.Inline ,"System.Numerics.BigInteger.CompareTo(System.Numerics.BigInteger)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _02bf2f34cf157e4d(BigInt instance, BigInt other);

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
	[Jazor(Op.Import ,"System.Numerics.BigInteger.CompareTo(object)")]
	public static Number _9f7b3705890bed98(BigInt instance, object? obj)
	{
		if (obj == null)
			return 1;
		if (obj is BigInt bigIntValue)
			return instance < bigIntValue ? -1 : (instance > bigIntValue ? 1 : 0);

		throw new Error("ArgumentException: Object must be of type BigInteger.");
	}

	///<summary>Converts a <see cref="T:System.Numerics.BigInteger" /> value to a byte array.</summary>
	[Jazor(Op.Import ,"System.Numerics.BigInteger.ToByteArray()")]
	public static byte[] _ca46777d5c8cc9b9(BigInt instance)
	{
		if (instance == BigInt.Zero)
			return [0];

		var isNegative = instance < BigInt.Zero;
		var value = isNegative ? -instance : instance;
		var bytes = new Array<byte>();

		while (value > BigInt.Zero)
		{
			bytes.Unshift(Number_(value & BigInt_(0xFF)));
			value >>= BigInt_(8);
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
	[Jazor(Op.Import ,"System.Numerics.BigInteger.ToByteArray(bool, bool)")]
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

		var minLength = isNegative ? Math.Ceil_((bitLength + 1) / 8) : Math.Ceil_(bitLength / 8);
		var byteLength = Math.Max_(minLength, 1);

		for (var i = 0; i < byteLength; i++)
		{
			var b = Number_(value & BigInt_(0xFF));
			if (isBigEndian)
				bytes.Unshift(b);
			else
				bytes.Push(b);

			value >>= BigInt_(8);
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
	[Jazor(Op.Import, "System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)")]
	public static Array<object?> _76ae4e496fc976fd(BigInt instance, Uint8Array destination, Number bytesWritten, bool isUnsigned, bool isBigEndian)
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
				? Math.Max_(1, Math.Ceil_(bitLength / 8))
				: Math.Max_(1, Math.Ceil_((bitLength + 1) / 8));
		}

		// 2. 检查缓冲区大小
		if (destination.Length < requiredBytes)
			return [false, 0];

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
				var b = Number_(value & BigInt_(0xFF));
				if (isBigEndian)
					bytes.Unshift(b);
				else
					bytes.Push(b);

				value >>= BigInt_(8);
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
			return [false, 0];

		// 5. 写入目标缓冲区
		for (var i = 0u; i < bytes.Length; i++)
			destination[i] = bytes[i];

		// 6. 填充剩余字节（如果需要）
		var fillByte = !isUnsigned && instance < BigInt.Zero ? 0xFF : 0;
		for (var i = bytes.Length; i < destination.Length; i++)
			destination[i] = (byte)fillByte;

		return [true, bytes.Length];
	}

	///<summary>Gets the number of bytes that will be output by <see cref="M:System.Numerics.BigInteger.ToByteArray(System.Boolean,System.Boolean)" /> and <see cref="M:System.Numerics.BigInteger.TryWriteBytes(System.Span{System.Byte},System.Int32@,System.Boolean,System.Boolean)" />.</summary>
	[Jazor(Op.Import ,"System.Numerics.BigInteger.GetByteCount(bool)")]
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
			return Math.Max_(1, Math.Ceil_(bitLength / 8));
		else
			return isNegative
				? Math.Max_(1, Math.Ceil_((bitLength + 1) / 8))  // 负数需要符号位
				: Math.Max_(1, Math.Ceil_(bitLength / 8));        // 正数不需要符号位
	}	

	/// <summary>
	/// C#: value.ToString()
	/// JS: value.toString()
	/// </summary>
	[Jazor(Op.Alias ,"override System.Numerics.BigInteger.ToString()", "toString")]
	public extern static string _a7388cc0c5bc22ad(BigInt instance);

	///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Import ,"System.Numerics.BigInteger.ToString(System.IFormatProvider)")]
	public static string? _fe4c3211e57446e7(BigInt instance, Intl.NumberFormat? provider)
	{
		if (provider is null)
			return instance.ToString();

		var isNegative = instance < BigInt.Zero;
		var absValue = isNegative ? -instance : instance;
		var strValue = absValue.ToString()!;

		try
		{
			// 对于可以直接使用Intl.NumberFormat的范围（在安全整数范围内）
			if (absValue <= BigInt_(Number.MAX_SAFE_INTEGER))
			{
				var formatted = provider.Format(Number_(absValue));
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
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.ToString(string)")]
	public extern static string _1650d30e3e9172f5(BigInt instance, string? format);

	///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.ToString(string, System.IFormatProvider)")]
	public extern static string _93b0cfb45a1832e9(BigInt instance, string? format, Intl.NumberFormat? provider);

	///<summary>Formats this big integer instance into a span of characters.</summary>
	[Jazor(Op.Discard ,"System.Numerics.BigInteger.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _90c190be387330ea(BigInt instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Subtracts a <see cref="T:System.Numerics.BigInteger" /> value from another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _28554dca4c0c49f8(BigInt left, BigInt right);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned byte value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)")]
	public extern static Number _c1afe3218f0f82f9();

	///<summary>Explicitly converts a big integer to a <see cref="T:System.Char" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator char(System.Numerics.BigInteger)")]
	public extern static Number _ac2920ee8216c023();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Decimal(System.Numerics.BigInteger)")]
	public extern static System.Decimal _9d2085a2aa8febea();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Double" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator double(System.Numerics.BigInteger)")]
	public extern static Number _4a6bc22c1d5cd472();

	///<summary>Explicitly converts a big integer to a <see cref="T:System.Half" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)")]
	public extern static System.Half _7c41bbf7746a0266();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 16-bit signed integer value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator short(System.Numerics.BigInteger)")]
	public extern static Number _c57fc79b767bf069();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 32-bit signed integer value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator int(System.Numerics.BigInteger)")]
	public extern static Number _7c261f922cc43235();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 64-bit signed integer value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)")]
	public extern static BigInt _15fe350cf299c580();

	///<summary>Explicitly converts a big integer to a <see cref="T:System.Int128" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)")]
	public extern static BigInt _5958070a15559320();

	///<summary>Explicitly converts a big integer to a <see cref="T:System.IntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator nint(System.Numerics.BigInteger)")]
	public extern static nint _11cea9efbc3d0c62();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a signed 8-bit value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int16" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator sbyte(System.Numerics.BigInteger)")]
	public extern static Number _63d8cc7789144528();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a single-precision floating-point value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator float(System.Numerics.BigInteger)")]
	public extern static Number _24972b9ed8006ec8();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 16-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int32" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator ushort(System.Numerics.BigInteger)")]
	public extern static Number _b2311568a6faa3b8();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 32-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int64" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator uint(System.Numerics.BigInteger)")]
	public extern static Number _385437ecb9a2b10a();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 64-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Double" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator ulong(System.Numerics.BigInteger)")]
	public extern static BigInt _6043725cddf263dd();

	///<summary>Explicitly converts a big integer to a <see cref="T:System.UInt128" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)")]
	public extern static BigInt _f8ae8a4213449843();

	///<summary>Explicitly converts a big integer to a <see cref="T:System.UIntPtr" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator nuint(System.Numerics.BigInteger)")]
	public extern static nuint _bbf68528b2eedf55();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> object to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Decimal)")]
	public extern static BigInt _8e505e0ce7efa99c();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Double" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)")]
	public extern static BigInt _933b3164355c792a();

	///<summary>Explicitly converts a <see cref="T:System.Half" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Half)")]
	public extern static BigInt _c186238bc3a46d2b();

	///<summary>Explicitly converts a <see cref="T:System.Numerics.Complex" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Numerics.Complex)")]
	public extern static BigInt _088fa1b2a09829ce();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Single" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(float)")]
	public extern static BigInt _212b6e60ce4e6836();

	///<summary>Defines an implicit conversion of an unsigned byte to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(byte)")]
	public extern static BigInt _24f94dfe434ed1de();

	///<summary>Implicitly converts a <see cref="T:System.Char" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(char)")]
	public extern static BigInt _6f52f939cef7ebfc();

	///<summary>Defines an implicit conversion of a signed 16-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(short)")]
	public extern static BigInt _5eb359c063a4b04b();

	///<summary>Defines an implicit conversion of a signed 32-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(int)")]
	public extern static BigInt _84639f9693379307();

	///<summary>Defines an implicit conversion of a signed 64-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(long)")]
	public extern static BigInt _7de492bb278503c8();

	///<summary>Implicitly converts a <see cref="T:System.Int128" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.Int128)")]
	public extern static BigInt _aa5bafc867e9b5eb();

	///<summary>Implicitly converts a <see cref="T:System.IntPtr" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(nint)")]
	public extern static BigInt _70a902bafd0ce64e();

	///<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.#ctor(System.Int32)" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(sbyte)")]
	public extern static BigInt _ff8ba3cf17ec3f75();

	///<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.op_Implicit(System.Int32)~System.Numerics.BigInteger" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ushort)")]
	public extern static BigInt _9b2419d65cfa19ab();

	///<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.op_Implicit(System.Int64)~System.Numerics.BigInteger" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(uint)")]
	public extern static BigInt _cf078fbbc4130e0c();

	///<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Double" />.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ulong)")]
	public extern static BigInt _9b4a5ecbd0f90bd4();

	///<summary>Implicitly converts a <see cref="T:System.UInt128" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.UInt128)")]
	public extern static BigInt _16f7ae7cb82a7523();

	///<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a big integer.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(nuint)")]
	public extern static BigInt _b7ee0d78d7054a45();

	///<summary>Performs a bitwise <see langword="And" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator &(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _4a529c0a5388c594(BigInt left, BigInt right);

	///<summary>Performs a bitwise <see langword="Or" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator |(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _752fd4cd29f4f204(BigInt left, BigInt right);

	///<summary>Performs a bitwise exclusive <see langword="Or" /> (<see langword="XOr" />) operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ^(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _a453418c13f7f875(BigInt left, BigInt right);

	///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the left.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <<(System.Numerics.BigInteger, int)")]
	public extern static BigInt _a29a9a670145ce5e(BigInt value, Number shift);

	///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the right.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >>(System.Numerics.BigInteger, int)")]
	public extern static BigInt _c0bed6f115403624(BigInt value, Number shift);

	///<summary>Returns the bitwise one's complement of a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ~(System.Numerics.BigInteger)")]
	public extern static BigInt _9182cf8afd9b8590(BigInt value);

	///<summary>Negates a specified BigInteger value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger)")]
	public extern static BigInt _03be17d45cbe5034(BigInt value);

	///<summary>Returns the value of the <see cref="T:System.Numerics.BigInteger" /> operand. (The sign of the operand is unchanged.)</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger)")]
	public extern static BigInt _7096f6c4ea9fddaf(BigInt value);

	///<summary>Increments a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ++(System.Numerics.BigInteger)")]
	public extern static BigInt _cc35859d07374d52(BigInt value);

	///<summary>Decrements a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator --(System.Numerics.BigInteger)")]
	public extern static BigInt _6d2fe51e4158a46f(BigInt value);

	///<summary>Adds the values of two specified <see cref="T:System.Numerics.BigInteger" /> objects.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _4edde875924e9396(BigInt left, BigInt right);

	///<summary>Multiplies two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator *(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _3baa1e316a9a8e5c(BigInt left, BigInt right);

	///<summary>Divides a specified <see cref="T:System.Numerics.BigInteger" /> value by another specified <see cref="T:System.Numerics.BigInteger" /> value by using integer division.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator /(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _e87ac03cab9bfae9(BigInt dividend, BigInt divisor);

	///<summary>Returns the remainder that results from division with two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator %(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static BigInt _44f6e17ba281115c(BigInt dividend, BigInt divisor);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static bool _c921d6c6bf72edae(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static bool _4175fbcd1bdcbb81(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static bool _38487ed1f787d018(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static bool _c69d246ab7c4d01a(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether the values of two <see cref="T:System.Numerics.BigInteger" /> objects are equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static bool _a1bca47181bf0a21(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether two <see cref="T:System.Numerics.BigInteger" /> objects have different values.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public extern static bool _fa04bb024b763d8c(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit signed integer.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, long)")]
	public extern static bool _54b970a90a63bed7(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit signed integer.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, long)")]
	public extern static bool _c5121fb5bb0459d9(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> is greater than a 64-bit signed integer value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, long)")]
	public extern static bool _f633b7dba945231e(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit signed integer value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, long)")]
	public extern static bool _05b14b64a3ed932c(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a signed long integer value are equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, long)")]
	public extern static bool _23fab9d29faa7b4b(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit signed integer are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, long)")]
	public extern static bool _bee7ae6c7fd4ccab(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit signed integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <(long, System.Numerics.BigInteger)")]
	public extern static bool _9e956828e15a31ac(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit signed integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <=(long, System.Numerics.BigInteger)")]
	public extern static bool _837c7f79427d7687(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit signed integer is greater than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >(long, System.Numerics.BigInteger)")]
	public extern static bool _599b5035513f4697(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit signed integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >=(long, System.Numerics.BigInteger)")]
	public extern static bool _b8852469edf6fccb(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a signed long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ==(long, System.Numerics.BigInteger)")]
	public extern static bool _17b7667af4b23f69(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit signed integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator !=(long, System.Numerics.BigInteger)")]
	public extern static bool _d3215df5ab1e7b4b(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit unsigned integer.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, ulong)")]
	public extern static bool _1f387e21472ec766(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit unsigned integer.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, ulong)")]
	public extern static bool _8bf92299327b0564(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, ulong)")]
	public extern static bool _85a372957af0ef3d(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit unsigned integer value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, ulong)")]
	public extern static bool _027db5ec51a792f8(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and an unsigned long integer value are equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, ulong)")]
	public extern static bool _90393e5796d20760(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit unsigned integer are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, ulong)")]
	public extern static bool _83ed1eafdd051a37(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit unsigned integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <(ulong, System.Numerics.BigInteger)")]
	public extern static bool _229e97bf2be53319(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit unsigned integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator <=(ulong, System.Numerics.BigInteger)")]
	public extern static bool _7f4a3ea98d5e7194(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >(ulong, System.Numerics.BigInteger)")]
	public extern static bool _e54a63c735fb2514(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit unsigned integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator >=(ulong, System.Numerics.BigInteger)")]
	public extern static bool _3a5b1bba5ac45b9c(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether an unsigned long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator ==(ulong, System.Numerics.BigInteger)")]
	public extern static bool _a97fbe9f639a835b(BigInt left, BigInt right);

	///<summary>Returns a value that indicates whether a 64-bit unsigned integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.Numerics.BigInteger.operator !=(ulong, System.Numerics.BigInteger)")]
	public extern static bool _ac5266c1db09af16(BigInt left, BigInt right);

	///<summary>Gets the number of bits required for shortest two's complement representation of the current instance without the sign bit.</summary>
	[Jazor(Op.Import, "System.Numerics.BigInteger.GetBitLength()")]
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
	[Jazor(Op.Import, "static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public static Array<BigInt> _22a21ffe19479f32(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new RangeError("Division by zero");

		var quotient = left / right;
		var remainder = left % right;

		return [quotient, remainder];
	}

	///<summary>Computes the number of leading zeros in a value.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)")]
	public static BigInt _276680abacb93277(BigInt value)
	{
		if (value == BigInt.Zero)
			return BigInt.Zero;

		// BigInt 是任意精度，没有固定的位宽，因此没有前导零的概念
		// 返回 0 表示值是从第一位开始的
		return BigInt.Zero;
	}	

	///<summary>Computes the number of bits that are set in a value.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)")]
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
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)")]
	public static BigInt _ae7b1dd18af32f04(BigInt value, Number rotateAmount)
	{
		if (value == BigInt.Zero)
			return BigInt.Zero;

		var bitLength = Number_(_41fe76dfb4ee2ab2(value));

		var ra = rotateAmount % bitLength;
		if (ra < 0)
			ra += bitLength;

		if (ra == 0)
			return value;

		var mask = (BigInt.One << BigInt_(ra)) - BigInt.One;
		var rotatedOutBits = (value >> BigInt_(bitLength - ra)) & mask;
		var result = ((value << BigInt_(ra)) | rotatedOutBits) & ((BigInt.One << BigInt_(bitLength)) - BigInt.One);

		return result;
	}	

	///<summary>Rotates a value right by a given amount.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)")]
	public static BigInt _dc8cc860511e78b3(BigInt value, Number rotateAmount)
	{
		if (rotateAmount == 0)
			return value;

		// Handle zero value
		if (value == BigInt.Zero)
			return BigInt.Zero;

		var bitLength = Number_(_41fe76dfb4ee2ab2(value));

		// Handle negative rotateAmount (convert to left rotation)
		if (rotateAmount < 0)
		{
			var absAmount = -rotateAmount;
			absAmount %= bitLength;
			if (absAmount == 0)
				return value;

			return (value << BigInt_(absAmount)) | (value >> BigInt_(bitLength - absAmount));
		}

		// Normalize rotateAmount to be within [0, bitLength)
		var ra = rotateAmount % bitLength;
		if (ra == 0)
			return value;

		// Perform right rotation
		var rightPart = value >> BigInt_(ra);
		var leftPart = value & ((BigInt.One << BigInt_(ra)) - BigInt.One);
		var rotated = (leftPart << BigInt_(bitLength - ra)) | rightPart;

		return rotated;
	}	

	///<summary>Computes the number of trailing zeros in a value.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)")]
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
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)")]
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
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)")]
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
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)")]
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

	/// <summary>
	/// C#: BigInteger.CopySign(value, sign)
	/// JS: BigInt 没有 -0，可直接按符号位切换绝对值。
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.CopySign(System.Numerics.BigInteger, System.Numerics.BigInteger)", "(__arg2 < 0n ? (__arg1 < 0n ? __arg1 : -__arg1) : (__arg1 < 0n ? -__arg1 : __arg1))")]
	public extern static BigInt _aa45b92454e3abaa(BigInt value, BigInt sign);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.CreateChecked<TOther>(TOther)")]
	public static BigInt _8cbca5624f4a6cc0<TOther>(object value)
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
			return BigInt_(n);
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
				return BigInt_(trimmed);
			}
			catch
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
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.CreateSaturating<TOther>(TOther)")]
	public extern static BigInt _f2847eb63549bd6a<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.CreateTruncating<TOther>(TOther)")]
	public extern static BigInt _8457175b141355fe<TOther>(object value);

	/// <summary>
	/// C#: BigInteger.IsEvenInteger(value)
	/// JS: value % 2n === 0n
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.IsEvenInteger(System.Numerics.BigInteger)", "(__arg1 % 2n === 0n)")]
	public extern static bool _691c1425b8fac31f(BigInt value);

	/// <summary>
	/// C#: BigInteger.IsNegative(value)
	/// JS: value < 0n
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.IsNegative(System.Numerics.BigInteger)", "(__arg1 < 0n)")]
	public extern static bool _8cb55ab054b637db(BigInt value);

	/// <summary>
	/// C#: BigInteger.IsOddInteger(value)
	/// JS: value % 2n !== 0n
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.IsOddInteger(System.Numerics.BigInteger)", "(__arg1 % 2n !== 0n)")]
	public extern static bool _8213026f03b857e7(BigInt value);

	/// <summary>
	/// C#: BigInteger.IsPositive(value)
	/// JS: value > 0n
	/// </summary>
	[Jazor(Op.Inline ,"static System.Numerics.BigInteger.IsPositive(System.Numerics.BigInteger)", "(__arg1 > 0n)")]
	public extern static bool _386d048147df6eae(BigInt value);

	/// <summary>
	/// C#: BigInteger.MaxMagnitude(x, y)
	/// JS: 先比较绝对值，绝对值相同再按数值大小决胜
	/// </summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public static BigInt _d305de2c64e85995(BigInt x, BigInt y)
	{
		var absX = x < BigInt.Zero ? -x : x;
		var absY = y < BigInt.Zero ? -y : y;
		if (absX > absY)
			return x;
		if (absX < absY)
			return y;

		return x > y ? x : y;
	}

	/// <summary>
	/// C#: BigInteger.MinMagnitude(x, y)
	/// JS: 先比较绝对值，绝对值相同再按数值大小决胜
	/// </summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public static BigInt _fef56ccd17b22e88(BigInt x, BigInt y)
	{
		var absX = x < BigInt.Zero ? -x : x;
		var absY = y < BigInt.Zero ? -y : y;
		if (absX < absY)
			return x;
		if (absX > absY)
			return y;

		return x < y ? x : y;
	}

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.TryParse(string, System.IFormatProvider, out System.Numerics.BigInteger)")]
	public extern static Array<object?> _10999a356af78aba(string? s, Intl.NumberFormat? provider, BigInt? result);

	///<summary>Shifts a value right by a given amount.</summary>
	[Jazor(Op.Import ,"static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)")]
	public static BigInt _49adf7adfc1228f8(BigInt value, Number shiftAmount)
	{
		if (shiftAmount < 0)
			throw new RangeError("Shift amount must be non-negative");

		var shift = BigInt_(shiftAmount);

		if (value >= BigInt.Zero)
			return value >> shift;

		// BigInt 没有原生的 >>> 运算符（JavaScript 的 >>> 仅适用于 Number）
		// 对于负数，抛出异常说明不支持
		throw new Error("Unsigned right shift (>>>) is not supported for BigInt in JavaScript");
	}	

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static BigInt _8bbfd46a98ce5419(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Numerics.BigInteger)")]
	public extern static Array<object?> _163b02803ece1f0c(string s, Intl.NumberFormat? provider, BigInt result);
}
