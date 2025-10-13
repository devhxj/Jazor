namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Numerics.BigInteger")]
public static class BigIntegerModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 32-bit signed integer value.</summary>
    ///<param name="value">A 32-bit signed integer.</param>
    [WhiteList("System.Numerics.BigInteger.BigInteger(int)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(uint)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(long)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(ulong)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(float)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(double)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(System.Decimal)")]
	[WhiteList("System.Numerics.BigInteger.BigInteger(byte[])")]
	[ECMAScriptLiteral("BigInt({0})")]
    public extern static BigInt BigIntegerNew(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a byte array.</summary>
    ///<param name="value">An array of byte values in little-endian order.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    [WhiteList("System.Numerics.BigInteger.BigInteger(byte[])")]
    public static BigInt BigIntegerNew8(Array<byte> value)
    {
        // 处理空数组
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
    ///<param name="value">A read-only span of bytes representing the big integer.</param>
    ///<param name="isUnsigned">  <see langword="true" /> to indicate <paramref name="value" /> uses unsigned encoding; otherwise, <see langword="false" /> (the default value).</param>
    ///<param name="isBigEndian">  <see langword="true" /> to indicate <paramref name="value" /> is in big-endian byte order; otherwise, <see langword="false" />  (the default value).</param>
    [WhiteList("System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)")]
    public static BigInt BigIntegerNew9(Array<byte> value, bool isUnsigned, bool isBigEndian)
    {
		// 处理空数组的情况
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
				// 有符号处理：如果最高位为1，则为负数
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

		// 处理8字节以上的非标准长度
		return ProcessNonStandardLength(value, isUnsigned, isBigEndian);

		// 处理非标准长度字节数组（3-7字节或>8字节）
		static BigInt ProcessNonStandardLength(Array<byte> bytes, bool isUnsigned, bool isBigEndian)
		{
			// 创建字节数组的拷贝以避免修改原始数据
			var processedBytes = bytes.Slice(0);

			// 大端序需要反转成小端序以便处理
			if (isBigEndian)
			{
				processedBytes.Reverse();
			}

			// 构建无符号大整数
			var result = BuildBigIntFromLEBytes(processedBytes);

			// 处理有符号数的补码转换（最高位为1）
			if (!isUnsigned && (processedBytes[processedBytes.Length - 1] & 0x80) != 0)
			{
				// 计算总位数（字节数 * 8）
				var bitWidth = BigInt(processedBytes.Length) * BigInt(8);

				// 计算2的bitWidth次方（偏移量）
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

			// 从最高位字节开始处理（小端序数组的最后一个字节是最高位）
			for (var i = littleEndianBytes.Length - 1; i >= 0; i--)
			{
				// 左移8位后拼接当前字节值
				result = (result << BigInt(8)) | BigInt(littleEndianBytes[i] & 0xFF);
			}

			return result;
		}
	}

    [WhiteList("static System.Numerics.BigInteger.Zero.get")]
    [ECMAScriptLiteral("0n",false)]
    public extern static BigInt BigIntegerGetZero(BigInt instance);

    [WhiteList("static System.Numerics.BigInteger.One.get")]
    [ECMAScriptLiteral("1n", false)]
    public extern static BigInt BigIntegerGetOne(BigInt instance);

    [WhiteList("static System.Numerics.BigInteger.MinusOne.get")]
    [ECMAScriptLiteral("-1n", false)]
    public extern static BigInt BigIntegerGetMinusOne(BigInt instance);

    [WhiteList("System.Numerics.BigInteger.IsPowerOfTwo.get")]
    [ECMAScriptLiteral("({0} > 0n && (({0} & ({0} - 1n)) == 0n))")]
    public extern static bool BigIntegerGetIsPowerOfTwo(BigInt instance);

    [WhiteList("System.Numerics.BigInteger.IsZero.get")]
    [ECMAScriptLiteral("{0} === 0n")]
    public extern static bool BigIntegerGetIsZero(BigInt instance);

    [WhiteList("System.Numerics.BigInteger.IsOne.get")]
	[ECMAScriptLiteral("{0} === 1n")]
	public extern static bool BigIntegerGetIsOne(BigInt instance);

    [WhiteList("System.Numerics.BigInteger.IsEven.get")]
    [ECMAScriptLiteral("{0} % 2n === 0n")]
    public extern static bool BigIntegerGetIsEven(BigInt instance);

    [WhiteList("System.Numerics.BigInteger.Sign.get")]
    [ECMAScriptLiteral("({0} == 0n ? 0 : ({0} > 0n ? 1 : -1))")]
    public extern static Number BigIntegerGetSign(BigInt instance);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains the number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> is not in the correct format.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("static System.Numerics.BigInteger.Parse(string)")]
    [ECMAScriptLiteral("BigInt({0})")]
    public extern static BigInt BigIntegerParse(string value);

    ///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> does not comply with the input pattern specified by <see cref="T:System.Globalization.NumberStyles" />.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerParse2(string value, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains a number to convert.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> is not in the correct format.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("static System.Numerics.BigInteger.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerParse3(string value, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> does not comply with the input pattern specified by <paramref name="style" />.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerParse4(string value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The string representation of a number.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or zero (0) if the conversion fails. The conversion fails if the <paramref name="value" /> parameter is <see langword="null" /> or is not of the correct format. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)")]
    public static bool BigIntegerTryParse(string? value, OutValue<BigInt?> result)
    {
        try
        {
            if (value.Length > 0)
            {
                result.Value = BigInt(value);
                return true;
            }
        }
        catch { }

        return false;
    }

    ///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The string representation of a number. The string is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="value" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or <see cref="P:System.Numerics.BigInteger.Zero" /> if the conversion failed. The conversion fails if the <paramref name="value" /> parameter is <see langword="null" /> or is not in a format that is compliant with <paramref name="style" />. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static bool BigIntegerTryParse2(string? value, System.Globalization.NumberStyles? style, Intl.NumberFormat? provider, OutValue<BigInt?> result);

    ///<summary>Converts the representation of a number, contained in the specified read-only span of characters, in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A read-only span of characters that contains the number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> does not comply with the input pattern specified by <paramref name="style" />.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerParse5(Uint32Array value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the representation of a number contained in the specified read-only character span, to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The representation of a number as a read-only span of characters.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or zero (0) if the conversion fails. The conversion fails if the <paramref name="value" /> parameter is an empty character span or is not of the correct format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static bool BigIntegerTryParse3(Uint32Array value, OutValue<BigInt> result);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The representation of a number as a read-only span of characters.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="value" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or <see cref="P:System.Numerics.BigInteger.Zero" /> if the conversion failed. The conversion fails if the <paramref name="value" /> parameter is an empty character span or is not in a format that is compliant with <paramref name="style" />. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static bool BigIntegerTryParse4(Uint32Array value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<BigInt> result);

    ///<summary>Compares two <see cref="T:System.Numerics.BigInteger" /> values and returns an integer that indicates whether the first value is less than, equal to, or greater than the second value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>A signed integer that indicates the relative values of <paramref name="left" /> and <paramref name="right" />, as shown in the following table. <list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="left" /> is less than <paramref name="right" />.</description></item><item><term> Zero</term><description><paramref name="left" /> equals <paramref name="right" />.</description></item><item><term> Greater than zero</term><description><paramref name="left" /> is greater than <paramref name="right" />.</description></item></list></returns>
    [WhiteList("static System.Numerics.BigInteger.Compare(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("System.Numerics.BigInteger.CompareTo(long)")]
	[WhiteList("System.Numerics.BigInteger.CompareTo(ulong)")]
	[WhiteList("System.Numerics.BigInteger.CompareTo(System.Numerics.BigInteger)")]
	[WhiteList("System.Numerics.BigInteger.CompareTo(object)")]
	[ECMAScriptLiteral("{0} === {1} ? 0 :({0} > {1} ? 1 : -1)")]
    public extern static Number BigIntegerCompare(BigInt left, BigInt right);

    ///<summary>Gets the absolute value of a <see cref="T:System.Numerics.BigInteger" /> object.</summary>
    ///<param name="value">A number.</param>
    ///<returns>The absolute value of <paramref name="value" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.Abs(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} < 0n ? -{0} : {0}")]
    public extern static BigInt BigIntegerAbs(BigInt value);

    ///<summary>Adds two <see cref="T:System.Numerics.BigInteger" /> values and returns the result.</summary>
    ///<param name="left">The first value to add.</param>
    ///<param name="right">The second value to add.</param>
    ///<returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.Add(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} + {1}")]
    public extern static BigInt BigIntegerAdd(BigInt left, BigInt right);

    ///<summary>Subtracts one <see cref="T:System.Numerics.BigInteger" /> value from another and returns the result.</summary>
    ///<param name="left">The value to subtract from (the minuend).</param>
    ///<param name="right">The value to subtract (the subtrahend).</param>
    ///<returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.Subtract(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} - {1}")]
    public extern static BigInt BigIntegerSubtract(BigInt left, BigInt right);

    ///<summary>Returns the product of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first number to multiply.</param>
    ///<param name="right">The second number to multiply.</param>
    ///<returns>The product of the <paramref name="left" /> and <paramref name="right" /> parameters.</returns>
    [WhiteList("static System.Numerics.BigInteger.Multiply(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} * {1}")]
    public extern static BigInt BigIntegerMultiply(BigInt left, BigInt right);

    ///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another and returns the result.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The quotient of the division.</returns>
    [WhiteList("static System.Numerics.BigInteger.Divide(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} / {1}")]
    public extern static BigInt BigIntegerDivide(BigInt dividend, BigInt divisor);

    ///<summary>Performs integer division on two <see cref="T:System.Numerics.BigInteger" /> values and returns the remainder.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The remainder after dividing <paramref name="dividend" /> by <paramref name="divisor" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.Remainder(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} % {1}")]
	public extern static BigInt BigIntegerRemainder(BigInt dividend, BigInt divisor);

    ///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another, returns the result, and returns the remainder in an output parameter.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<param name="remainder">When this method returns, contains a <see cref="T:System.Numerics.BigInteger" /> value that represents the remainder from the division. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The quotient of the division.</returns>
    [WhiteList("static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)")]
    public static BigInt BigIntegerDivRem(BigInt dividend, BigInt divisor, OutValue<BigInt> remainder)
    {
		remainder.Value = dividend % divisor;
        return dividend / divisor;
	}

    ///<summary>Negates a specified <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to negate.</param>
    ///<returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
    [WhiteList("static System.Numerics.BigInteger.Negate(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("-{0}")]
	public extern static BigInt BigIntegerNegate(BigInt value);

    ///<summary>Returns the natural (base <see langword="e" />) logarithm of a specified number.</summary>
    ///<param name="value">The number whose logarithm is to be found.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The natural log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
    ///<returns>The natural (base <see langword="e" />) logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
    [WhiteList("static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)")]
    public static Number BigIntegerLog(BigInt value)
    {
		if (value <= BigInt.Zero) 
			throw new Error("Logarithm is undefined for non-positive numbers");
		
		var str = value.ToString();
		var exponent = str.Length - 1;
		var mantissa = Number(str.Substring(0, 15));

		return Maths.Log(mantissa) + exponent * Maths.Log(10);
	}

    ///<summary>Returns the logarithm of a specified number in a specified base.</summary>
    ///<param name="value">A number whose logarithm is to be found.</param>
    ///<param name="baseValue">The base of the logarithm.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
    ///<returns>The base <paramref name="baseValue" /> logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
    [WhiteList("static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)")]
    public static Number BigIntegerLog2(BigInt value, Number baseValue)
    {
		if (value <= BigInt.Zero) 
            throw new RangeError("Logarithm is undefined for non-positive numbers");

		if (baseValue <= 0 || baseValue == 1) 
            throw new RangeError("Base must be positive and not equal to 1");

		if (value == BigInt.One) 
            return 0;

		if (baseValue == Maths.E) 
            Maths.Log(Number(value));

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
    ///<param name="value">A number whose logarithm is to be found.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The base 10 log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
    ///<returns>The base 10 logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
    [WhiteList("static System.Numerics.BigInteger.Log10(System.Numerics.BigInteger)")]
    public static Number BigIntegerLog10(BigInt value)
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
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The greatest common divisor of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    public static BigInt BigIntegerGreatestCommonDivisor(BigInt left, BigInt right)
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
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is larger.</returns>
    [WhiteList("static System.Numerics.BigInteger.Max(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} > {1} ? {0} : {1}")]
	public extern static BigInt BigIntegerMax(BigInt left, BigInt right);

    ///<summary>Returns the smaller of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is smaller.</returns>
    [WhiteList("static System.Numerics.BigInteger.Min(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} < {1} ? {0} : {1}")]
	public extern static BigInt BigIntegerMin(BigInt left, BigInt right);

    ///<summary>Performs modulus division on a number raised to the power of another number.</summary>
    ///<param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
    ///<param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
    ///<param name="modulus">The number by which to divide <paramref name="value" /> raised to the <paramref name="exponent" /> power.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="modulus" /> is zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="exponent" /> is negative.</exception>
    ///<returns>The remainder after dividing <paramref name="value" />exponent by <paramref name="modulus" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    public static BigInt BigIntegerModPow(BigInt value, BigInt exponent, BigInt modulus)
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
    ///<param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
    ///<param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="exponent" /> is negative.</exception>
    ///<returns>The result of raising <paramref name="value" /> to the <paramref name="exponent" /> power.</returns>
    [WhiteList("static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)")]
    public static BigInt BigIntegerPow(BigInt value, Number exponent)
    {
        if (value <= BigInt.Zero)
            throw new RangeError("The index must be a non negative integer");

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
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("override System.Numerics.BigInteger.GetHashCode()")]
    public static Number BigIntegerGetHashCode(BigInt instance)
    {
		var positiveValue = instance < BigInt.Zero ? -instance : instance;
		return Number(positiveValue % BigInt(2147483647));
	}

    ///<summary>Returns a value that indicates whether the current instance and a specified object have the same value.</summary>
    ///<param name="obj">The object to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="obj" /> argument is a <see cref="T:System.Numerics.BigInteger" /> object, and its value is equal to the value of the current <see cref="T:System.Numerics.BigInteger" /> instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("override System.Numerics.BigInteger.Equals(object)")]
	[WhiteList("System.Numerics.BigInteger.Equals(long)")]
	[WhiteList("System.Numerics.BigInteger.Equals(ulong)")]
	[WhiteList("System.Numerics.BigInteger.Equals(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} === {1}")]
	public extern static bool BigIntegerEquals(BigInt instance, Object? obj);

    ///<summary>Converts a <see cref="T:System.Numerics.BigInteger" /> value to a byte array.</summary>
    ///<returns>The value of the current <see cref="T:System.Numerics.BigInteger" /> object converted to an array of bytes.</returns>
    [WhiteList("System.Numerics.BigInteger.ToByteArray()")]
    public static Array<byte> BigIntegerToByteArray(BigInt instance)
    {
		if (instance == BigInt.Zero) 
            return [0];

		var value = instance;
        var bytes = new Array<byte>();

		while (value > BigInt.Zero) 
        {
			bytes.Unshift(Number(value & BigInt(0xFF)));
			value >>= BigInt(8);
		}

		return bytes;
	}

    ///<summary>Returns the value of this <see cref="T:System.Numerics.BigInteger" /> as a byte array using the fewest number of bytes possible. If the value is zero, returns an array of one byte whose element is 0x00.</summary>
    ///<param name="isUnsigned">  <see langword="true" /> to use unsigned encoding; otherwise, <see langword="false" />.</param>
    ///<param name="isBigEndian">  <see langword="true" /> to write the bytes in a big-endian byte order; otherwise, <see langword="false" />.</param>
    ///<exception cref="T:System.OverflowException">If <paramref name="isUnsigned" /> is <see langword="true" /> and <see cref="P:System.Numerics.BigInteger.Sign" /> is negative.</exception>
    ///<returns>The value of the current <see cref="T:System.Numerics.BigInteger" /> object converted to an array of bytes.</returns>
    [WhiteList("System.Numerics.BigInteger.ToByteArray(bool, bool)")]
    public static Array<byte> BigIntegerToByteArray2(BigInt instance, bool isUnsigned, bool isBigEndian)
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
    ///<param name="destination">The destination span to which the resulting bytes should be written.</param>
    ///<param name="bytesWritten">The number of bytes written to <paramref name="destination" />.</param>
    ///<param name="isUnsigned">  <see langword="true" /> to use unsigned encoding; otherwise, <see langword="false" />.</param>
    ///<param name="isBigEndian">  <see langword="true" /> to write the bytes in a big-endian byte order; otherwise, <see langword="false" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="isUnsigned" /> is <see langword="true" /> and <see cref="P:System.Numerics.BigInteger.Sign" /> is negative.</exception>
    ///<returns>  <see langword="true" /> if the bytes fit in <paramref name="destination" />; <see langword="false" /> if not all bytes could be written due to lack of space.</returns>
    [WhiteList("System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)")]
    public static bool BigIntegerTryWriteBytes(BigInt instance, Uint8Array destination, OutValue<Number> bytesWritten, bool isUnsigned, bool isBigEndian)
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

            // 计算所需字节
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

            // 计算实际需要处理的字节数
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

            // 处理负数补码
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

        // 4. 检查最终字节数是否超出缓冲区
        if (bytes.Length > destination.Length)
        {
            bytesWritten.Value = 0;
            return false;
        }

        // 5. 写入目标数组
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
    ///<param name="isUnsigned">  <see langword="true" /> to use unsigned encoding; otherwise, <see langword="false" />.</param>
    ///<returns>The number of bytes.</returns>
    [WhiteList("System.Numerics.BigInteger.GetByteCount(bool)")]
    public static Number BigIntegerGetByteCount(BigInt instance, bool isUnsigned)
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
                ? Maths.Max(1, Maths.Ceil((bitLength + 1) / 8))
                : Maths.Max(1, Maths.Ceil((bitLength + 1) / 8));
    }

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation.</summary>
    ///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value.</returns>
    [WhiteList("override System.Numerics.BigInteger.ToString()")]
    public extern static string BigIntegerToString(BigInt instance);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value in the format specified by the <paramref name="provider" /> parameter.</returns>
    [WhiteList("System.Numerics.BigInteger.ToString(System.IFormatProvider)")]
    public static string BigIntegerToString2(BigInt instance, Intl.NumberFormat? provider)
    {
		if (provider is null) 
            return instance.ToString();

		var isNegative = instance < BigInt.Zero;
		var absValue = isNegative ? -instance : instance;
		var strValue = absValue.ToString();

		try
		{
            // 尝试直接使用Intl.NumberFormat（适用于安全整数范围）
            if (absValue <= BigInt(Number.MAX_SAFE_INTEGER))
            {
                var formatted = provider.Format(Number(absValue));
                return isNegative ? $"-{formatted}" : formatted;
            }

			// 对于超大数，手动实现基本本地化格式化
			var sample = provider.Format(1000.1);
            var groupChar = sample.Includes("1,000") ? "," :
                             sample.Includes("1.000") ? "." :
                             sample.Includes("1 000") ? " " : ",";

			// 从右向左添加分组分隔符
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
    ///<param name="format">A standard or custom numeric format string.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is not a valid format string.</exception>
    ///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value in the format specified by the <paramref name="format" /> parameter.</returns>
    [WhiteList("System.Numerics.BigInteger.ToString(string)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static string BigIntegerToString3(BigInt instance, string? format);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format and culture-specific format information.</summary>
    ///<param name="format">A standard or custom numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is not a valid format string.</exception>
    ///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value as specified by the <paramref name="format" /> and <paramref name="provider" /> parameters.</returns>
    [WhiteList("System.Numerics.BigInteger.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static string BigIntegerToString4(BigInt instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Formats this big integer instance into a span of characters.</summary>
    ///<param name="destination">The span of characters into which this instance will be written.</param>
    ///<param name="charsWritten">When the method returns, contains the length of the span in number of characters.</param>
    ///<param name="format">A read-only span of characters that specifies the format for the formatting operation.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
    ///<returns>  <see langword="true" /> if the formatting operation succeeds; <see langword="false" /> otherwise.</returns>
    [WhiteList("System.Numerics.BigInteger.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static bool BigIntegerTryFormat(BigInt instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Subtracts a <see cref="T:System.Numerics.BigInteger" /> value from another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The value to subtract from (the minuend).</param>
    ///<param name="right">The value to subtract (the subtrahend).</param>
    ///<returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} - {1}")]
	public extern static BigInt BigIntegerOpSubtraction(BigInt left, BigInt right);

    ///<summary>Performs a bitwise <see langword="And" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The result of the bitwise <see langword="And" /> operation.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator &(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} & {1}")]
	public extern static BigInt BigIntegerOpBitwiseAnd(BigInt left, BigInt right);

    ///<summary>Performs a bitwise <see langword="Or" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The result of the bitwise <see langword="Or" /> operation.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator |(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} | {1}")]
	public extern static BigInt BigIntegerOpBitwiseOr(BigInt left, BigInt right);

    ///<summary>Performs a bitwise exclusive <see langword="Or" /> (<see langword="XOr" />) operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The result of the bitwise <see langword="Or" /> operation.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator ^(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} ^ {1}")]
	public extern static BigInt BigIntegerOpExclusiveOr(BigInt left, BigInt right);

    ///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the left.</summary>
    ///<param name="value">The value whose bits are to be shifted.</param>
    ///<param name="shift">The number of bits to shift <paramref name="value" /> to the left.</param>
    ///<returns>A value that has been shifted to the left by the specified number of bits.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator <<(System.Numerics.BigInteger, int)")]
	[ECMAScriptLiteral("{0} << {1}")]
	public extern static BigInt BigIntegerOpLeftShift(BigInt value, Number shift);

    ///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the right.</summary>
    ///<param name="value">The value whose bits are to be shifted.</param>
    ///<param name="shift">The number of bits to shift <paramref name="value" /> to the right.</param>
    ///<returns>A value that has been shifted to the right by the specified number of bits.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator >>(System.Numerics.BigInteger, int)")]
	[ECMAScriptLiteral("{0} >> {1}")]
	public extern static BigInt BigIntegerOpRightShift(BigInt value, Number shift);

    ///<summary>Returns the bitwise one's complement of a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">An integer value.</param>
    ///<returns>The bitwise one's complement of <paramref name="value" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator ~(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("~{0}")]
	public extern static BigInt BigIntegerOpOnesComplement(BigInt value);

    ///<summary>Negates a specified BigInteger value.</summary>
    ///<param name="value">The value to negate.</param>
    ///<returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
    [WhiteList("static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("-{0}")]
	public extern static BigInt BigIntegerOpUnaryNegation(BigInt value);

    ///<summary>Returns the value of the <see cref="T:System.Numerics.BigInteger" /> operand. (The sign of the operand is unchanged.)</summary>
    ///<param name="value">An integer value.</param>
    ///<returns>The value of the <paramref name="value" /> operand.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("+{0}")]
	public extern static BigInt BigIntegerOpUnaryPlus(BigInt value);

    ///<summary>Increments a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
    ///<param name="value">The value to increment.</param>
    ///<returns>The value of the <paramref name="value" /> parameter incremented by 1.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator ++(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("++{0}")]
    public extern static BigInt BigIntegerOpIncrement(BigInt value);

    ///<summary>Decrements a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
    ///<param name="value">The value to decrement.</param>
    ///<returns>The value of the <paramref name="value" /> parameter decremented by 1.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator --(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("--{0}")]
    public extern static BigInt BigIntegerOpDecrement(BigInt value);

    ///<summary>Adds the values of two specified <see cref="T:System.Numerics.BigInteger" /> objects.</summary>
    ///<param name="left">The first value to add.</param>
    ///<param name="right">The second value to add.</param>
    ///<returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} + {1}")]
    public extern static BigInt BigIntegerOpAddition(BigInt left, BigInt right);

    ///<summary>Multiplies two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value to multiply.</param>
    ///<param name="right">The second value to multiply.</param>
    ///<returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator *(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} * {1}")]
    public extern static BigInt BigIntegerOpMultiply(BigInt left, BigInt right);

    ///<summary>Divides a specified <see cref="T:System.Numerics.BigInteger" /> value by another specified <see cref="T:System.Numerics.BigInteger" /> value by using integer division.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The integral result of the division.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator /(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} / {1}")]
    public extern static BigInt BigIntegerOpDivision(BigInt dividend, BigInt divisor);

    ///<summary>Returns the remainder that results from division with two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The remainder that results from the division.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator %(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} % {1}")]
    public extern static BigInt BigIntegerOpModulus(BigInt dividend, BigInt divisor);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, long)")]
	[WhiteList("static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, ulong)")]
	[WhiteList("static System.Numerics.BigInteger.operator <(long, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator <(ulong, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} < {1}")]
    public extern static bool BigIntegerOpLessThan(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, long)")]
	[WhiteList("static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, ulong)")]
	[WhiteList("static System.Numerics.BigInteger.operator <=(long, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator <=(ulong, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} <= {1}")]
    public extern static bool BigIntegerOpLessThanOrEqual(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, long)")]
	[WhiteList("static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, ulong)")]
	[WhiteList("static System.Numerics.BigInteger.operator >(long, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator >(ulong, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} > {1}")]
    public extern static bool BigIntegerOpGreaterThan(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, long)")]
	[WhiteList("static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, ulong)")]
	[WhiteList("static System.Numerics.BigInteger.operator >=(long, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator >=(ulong, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} >= {1}")]
    public extern static bool BigIntegerOpGreaterThanOrEqual(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether the values of two <see cref="T:System.Numerics.BigInteger" /> objects are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, long)")]
	[WhiteList("static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, ulong)")]
	[WhiteList("static System.Numerics.BigInteger.operator ==(long, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator ==(ulong, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} == {1}")]
    public extern static bool BigIntegerOpEquality(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether two <see cref="T:System.Numerics.BigInteger" /> objects have different values.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, long)")]
	[WhiteList("static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, ulong)")]
	[WhiteList("static System.Numerics.BigInteger.operator !=(long, System.Numerics.BigInteger)")]
	[WhiteList("static System.Numerics.BigInteger.operator !=(ulong, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("{0} != {1}")]
    public extern static bool BigIntegerOpInequality(BigInt left, BigInt right);

    ///<summary>Gets the number of bits required for shortest two's complement representation of the current instance without the sign bit.</summary>
    ///<returns>The minimum non-negative number of bits in two's complement notation without the sign bit.</returns>
    [WhiteList("System.Numerics.BigInteger.GetBitLength()")]
    public static BigInt BigIntegerGetBitLength(BigInt instance)
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
    ///<param name="left">The value that <code data-dev-comment-type="paramref">right</code> divides.</param>
    ///<param name="right">The value that divides <code data-dev-comment-type="paramref">left</code>.</param>
    ///<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public static (BigInt, BigInt) BigIntegerDivRem2(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero) 
			throw new RangeError("Division by zero");
		
		var quotient = left / right;
		var remainder = left % right;

		return (quotient, remainder);
	}

    ///<summary>Computes the number of leading zeros in a value.</summary>
    ///<param name="value">The value whose leading zeroes are to be counted.</param>
    ///<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)")]
	public static BigInt BigIntegerLeadingZeroCount(BigInt value)
	{
		if (value == BigInt.Zero) 
			return BigInt(64);
		
		var binaryStr = value.ToString(2);
		var bitLength = BigInt(binaryStr.Length);
		var leadingZeros = BigInt(64) - bitLength;
		return leadingZeros > BigInt.Zero ? leadingZeros : BigInt.Zero;
	}

    ///<summary>Computes the number of bits that are set in a value.</summary>
    ///<param name="value">The value whose set bits are to be counted.</param>
    ///<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)")]
    public static BigInt BigIntegerPopCount(BigInt value)
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
    ///<param name="value">The value that's rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)")]
	public static BigInt BigIntegerRotateLeft(BigInt value, Number rotateAmount)
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
    ///<param name="value">The value that's rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)")]
    public static BigInt BigIntegerRotateRight(BigInt value, Number rotateAmount)
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

            return (value << BigInt(absAmount)) |  (value >> BigInt(bitLength - absAmount));
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
    ///<param name="value">The value whose trailing zeroes are to be counted.</param>
    ///<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)")]
	public static BigInt BigIntegerTrailingZeroCount(BigInt value)
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
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)")]
    public static bool BigIntegerIsPow2(BigInt value)
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
    ///<param name="value">The value whose log2 is to be computed.</param>
    ///<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)")]
    public static BigInt BigIntegerLog2(BigInt value)
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
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public static BigInt BigIntegerClamp(BigInt value, BigInt min, BigInt max)
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
    ///<param name="value">The value whose magnitude is used in the result.</param>
    ///<param name="sign">The value whose sign is used in the result.</param>
    ///<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.CopySign(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("({1} < 0n ? -1 : 1)*({0} < 0n ? -{0} : {0})")]
    public extern static BigInt BigIntegerCopySign(BigInt value, BigInt sign);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.CreateChecked<TOther>(TOther)")]
	public static BigInt BigIntegerCreateChecked<TOther>(TOther value)
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
			if (RegExp(@"!/ ^-?\d +$/").Test(trimmed)) 
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
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerCreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerCreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.IsEvenInteger(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("({0} & 1n) === 0n")]
	public extern static bool BigIntegerIsEvenInteger(BigInt value);

    ///<summary>Determines if a value is negative.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.IsNegative(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} < 0n")]
    public extern static bool BigIntegerIsNegative(BigInt value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.IsOddInteger(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("({0} & 1n) === 1n")]
    public extern static bool BigIntegerIsOddInteger(BigInt value);

    ///<summary>Determines if a value is positive.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.IsPositive(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} >= 0n")]
    public extern static bool BigIntegerIsPositive(BigInt value);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} > {1} ? {0} : {1}")]
    public extern static BigInt BigIntegerMaxMagnitude(BigInt x, BigInt y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} < {1} ? {0} : {1}")]
    public extern static BigInt BigIntegerMinMagnitude(BigInt x, BigInt y);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if  <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.TryParse(string, System.IFormatProvider, out System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static bool BigIntegerTryParse5(string? s, Intl.NumberFormat? provider, OutValue<BigInt?> result);

    ///<summary>Shifts a value right by a given amount.</summary>
    ///<param name="value">The value that is shifted right by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
    ///<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted right.</param>
    ///<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)")]
    public static BigInt BigIntegerOpUnsignedRightShift(BigInt value, Number shiftAmount)
    {
        if (shiftAmount < 0)
            throw new RangeError("Shift amount must be non-negative");

        var shift = BigInt(shiftAmount);

        if (value >= BigInt.Zero)
            return value >> shift;

        // 对于负数：先转换为正数等效表示，然后右移
        // 使用 2^n 的补数概念
        var bits = BigInt(64); // 假设64位（可以根据需要调整）
        var mask = (BigInt.One << bits) - BigInt.One;
        var positiveRepresentation = value & mask;
        return positiveRepresentation >> shift;
    }

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static BigInt BigIntegerParse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor", true)]
    public extern static bool BigIntegerTryParse6(Uint32Array s, Intl.NumberFormat? provider, OutValue<BigInt> result);
}
