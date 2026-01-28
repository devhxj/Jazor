using System.Collections;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Numerics.BigInteger", "System.Numerics.BigInteger", "System.Numerics.BigInteger")]
public static class BigIntegerModule1
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 32-bit signed integer value.</summary>
    ///<param name="value">A 32-bit signed integer.</param>
    [WhiteList("_ba6e0e86598dc8b2","System.Numerics.BigInteger.BigInteger(int)", "_ba6e0e86598dc8b2")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _ba6e0e86598dc8b2(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using an unsigned 32-bit integer value.</summary>
    ///<param name="value">An unsigned 32-bit integer value.</param>
    [WhiteList("_b7b735a5d507d449","System.Numerics.BigInteger.BigInteger(uint)", "_b7b735a5d507d449")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _b7b735a5d507d449(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a 64-bit signed integer value.</summary>
    ///<param name="value">A 64-bit signed integer.</param>
    [WhiteList("_74973910762e0e86","System.Numerics.BigInteger.BigInteger(long)", "_74973910762e0e86")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _74973910762e0e86(BigInt value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure with an unsigned 64-bit integer value.</summary>
    ///<param name="value">An unsigned 64-bit integer.</param>
    [WhiteList("_0421ba6c202fdc80","System.Numerics.BigInteger.BigInteger(ulong)", "_0421ba6c202fdc80")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _0421ba6c202fdc80(BigInt value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a single-precision floating-point value.</summary>
    ///<param name="value">A single-precision floating-point value.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.NegativeInfinity" />, or <see cref="F:System.Single.PositiveInfinity" />.</exception>
    [WhiteList("_cfd2038efd505e1f","System.Numerics.BigInteger.BigInteger(float)", "_cfd2038efd505e1f")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _cfd2038efd505e1f(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a double-precision floating-point value.</summary>
    ///<param name="value">A double-precision floating-point value.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />.</exception>
    [WhiteList("_38c7caccfd5e120e","System.Numerics.BigInteger.BigInteger(double)", "_38c7caccfd5e120e")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _38c7caccfd5e120e(Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using a <see cref="T:System.Decimal" /> value.</summary>
    ///<param name="value">A decimal number.</param>
    [WhiteList("_f715f85cc5dcfe92","System.Numerics.BigInteger.BigInteger(System.Decimal)", "_f715f85cc5dcfe92")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _f715f85cc5dcfe92(System.Decimal value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Numerics.BigInteger" /> structure using the values in a byte array.</summary>
    ///<param name="value">An array of byte values in little-endian order.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    [WhiteList("_c1e724fa6dbf63eb","System.Numerics.BigInteger.BigInteger(byte[])", "_c1e724fa6dbf63eb")]
	public static BigInt _c1e724fa6dbf63eb(Array<byte> value)
	{
		// ����������
		if (value.Length == 0)
			return BigInt.Zero;

		var buffer = new ArrayBuffer(value.Length);
		var array = new Uint8Array(buffer);
		var view = new DataView(array.Buffer, array.ByteOffset, array.ByteLength);
		var result = BigInt.Zero;
		var i = 0u;

		// ÿ�δ��� 8 �ֽڣ�64λ��
		for (; i + 8 <= value.Length; i += 8)
			result = (result << BigInt(64)) | view.GetBigUint64(i, false);

		// ����ʣ���ֽڣ����7�ֽڣ�
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
	[WhiteList("_9c321a7400e5ff9b","System.Numerics.BigInteger.BigInteger(System.ReadOnlySpan<byte>, bool, bool)", "_9c321a7400e5ff9b")]
    [ECMAScriptLiteral("BigInt(@#{0})")]
	public static BigInt _9c321a7400e5ff9b(Array<byte> value, bool isUnsigned, bool isBigEndian)
	{
		// ��������������
		if (value.Length == 0)
			return BigInt.Zero;

		// ����1�ֽ��������
		if (value.Length == 1)
		{
			if (isUnsigned)
			{
				return BigInt(value[0]);
			}
			else
			{
				// �з��Ŵ�����������λΪ1����Ϊ����
				return (value[0] & 0x80) == 0
					? BigInt(value[0])
					: BigInt(value[0]) - BigInt(0x100);
			}
		}

		// ������׼���ȣ�2/4/8�ֽڣ�
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
				// 3/5/6/7�ֽڳ���ʹ�÷Ǳ�׼����
				_ => ProcessNonStandardLength(value, isUnsigned, isBigEndian)
			};
		}

		// ����8�ֽ����ϵķǱ�׼����
		return ProcessNonStandardLength(value, isUnsigned, isBigEndian);

		// �����Ǳ�׼�����ֽ����飨3-7�ֽڻ�>8�ֽڣ�
		static BigInt ProcessNonStandardLength(Array<byte> bytes, bool isUnsigned, bool isBigEndian)
		{
			// �����ֽ�����Ŀ����Ա����޸�ԭʼ����
			var processedBytes = bytes.Slice(0);

			// �������Ҫ��ת��С�����Ա㴦��
			if (isBigEndian)
			{
				processedBytes.Reverse();
			}

			// �����޷��Ŵ�����
			var result = BuildBigIntFromLEBytes(processedBytes);

			// �����з������Ĳ���ת�������λΪ1��
			if (!isUnsigned && (processedBytes[processedBytes.Length - 1] & 0x80) != 0)
			{
				// ������λ�����ֽ��� * 8��
				var bitWidth = BigInt(processedBytes.Length) * BigInt(8);

				// ����2��bitWidth�η���ƫ������
				var offset = BigInt.One << bitWidth;

				// ת��Ϊ�з��Ų���ֵ
				result -= offset;
			}

			return result;
		}

		// ��С�����ֽ����鹹���޷��Ŵ�����
		static BigInt BuildBigIntFromLEBytes(Array<byte> littleEndianBytes)
		{
			var result = BigInt.Zero;

			// �����λ�ֽڿ�ʼ������С������������һ���ֽ������λ��
			for (var i = littleEndianBytes.Length - 1; i >= 0; i--)
			{
				// ����8λ��ƴ�ӵ�ǰ�ֽ�ֵ
				result = (result << BigInt(8)) | BigInt(littleEndianBytes[i] & 0xFF);
			}

			return result;
		}
	}

	[WhiteList("_77fc63f99954f8da","static System.Numerics.BigInteger.Zero.get", "_77fc63f99954f8da")]
	[ECMAScriptLiteral("0n")]
	public extern static BigInt _77fc63f99954f8da(BigInt instance);

    [WhiteList("_9c5419989e842d00","static System.Numerics.BigInteger.One.get", "_9c5419989e842d00")]
	[ECMAScriptLiteral("1n")]
	public extern static BigInt _9c5419989e842d00(BigInt instance);

    [WhiteList("_01c112900aa52c82","static System.Numerics.BigInteger.MinusOne.get", "_01c112900aa52c82")]
    [ECMAScriptLiteral("-1n")]
	public extern static BigInt _01c112900aa52c82(BigInt instance);

    [WhiteList("_ee8564f940baf789","System.Numerics.BigInteger.IsPowerOfTwo.get", "_ee8564f940baf789")]
	[ECMAScriptLiteral("(@#{0} > 0n && ((@#{0} & (@#{0} - 1n)) == 0n))")]
	public extern static bool _ee8564f940baf789(BigInt instance);

    [WhiteList("_c138b3f4dd057592","System.Numerics.BigInteger.IsZero.get","_c138b3f4dd057592")]
	[ECMAScriptLiteral("@#{0} === 0n")]
	public extern static bool _c138b3f4dd057592(BigInt instance);

    [WhiteList("_2aa0739f87c79906","System.Numerics.BigInteger.IsOne.get","_2aa0739f87c79906")]
	[ECMAScriptLiteral("@#{0} === 1n")]
	public extern static bool _2aa0739f87c79906(BigInt instance);

    [WhiteList("_4a465705ad4dc8ca","System.Numerics.BigInteger.IsEven.get","_4a465705ad4dc8ca")]
	[ECMAScriptLiteral("@#{0} % 2n === 0n")]
	public extern static bool _4a465705ad4dc8ca(BigInt instance);

    [WhiteList("_734290a188c5bc5a","System.Numerics.BigInteger.Sign.get","_734290a188c5bc5a")]
	[ECMAScriptLiteral("(@#{0} === 0n ? 0 : (@#{0} > 0n ? 1 : -1))")]
	public extern static Number _734290a188c5bc5a(BigInt instance);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains the number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> is not in the correct format.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("_155212572c9a3297","static System.Numerics.BigInteger.Parse(string)","_155212572c9a3297")]
	[ECMAScriptLiteral("BigInt(@#{0})")]
	public extern static BigInt _155212572c9a3297(string value);

    ///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> does not comply with the input pattern specified by <see cref="T:System.Globalization.NumberStyles" />.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("_a077721686cadcd9","static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles)","_a077721686cadcd9")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _a077721686cadcd9(string value, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains a number to convert.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> is not in the correct format.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("_d1543aa14ab94729","static System.Numerics.BigInteger.Parse(string, System.IFormatProvider)","_d1543aa14ab94729")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _d1543aa14ab94729(string value, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> does not comply with the input pattern specified by <paramref name="style" />.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("_8adf758c3f22af12","static System.Numerics.BigInteger.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)","_8adf758c3f22af12")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _8adf758c3f22af12(string value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The string representation of a number.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or zero (0) if the conversion fails. The conversion fails if the <paramref name="value" /> parameter is <see langword="null" /> or is not of the correct format. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("_59acea2facdaa757","static System.Numerics.BigInteger.TryParse(string, out System.Numerics.BigInteger)","_59acea2facdaa757")]
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
	///<param name="value">The string representation of a number. The string is interpreted using the style specified by <paramref name="style" />.</param>
	///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="value" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
	///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
	///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or <see cref="P:System.Numerics.BigInteger.Zero" /> if the conversion failed. The conversion fails if the <paramref name="value" /> parameter is <see langword="null" /> or is not in a format that is compliant with <paramref name="style" />. This parameter is passed uninitialized.</param>
	///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
	///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
	[WhiteList("_85cd9c4a9c2dadf4","static System.Numerics.BigInteger.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)","_85cd9c4a9c2dadf4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _85cd9c4a9c2dadf4(string? value, System.Globalization.NumberStyles? style, Intl.NumberFormat? provider, OutValue<BigInt?> result);

    ///<summary>Converts the representation of a number, contained in the specified read-only span of characters, in a specified style to its <see cref="T:System.Numerics.BigInteger" /> equivalent.</summary>
    ///<param name="value">A read-only span of characters that contains the number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="value" />.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> does not comply with the input pattern specified by <paramref name="style" />.</exception>
    ///<returns>A value that is equivalent to the number specified in the <paramref name="value" /> parameter.</returns>
    [WhiteList("_00d39f2029fd4266","static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)","_00d39f2029fd4266")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _00d39f2029fd4266(Uint32Array value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the representation of a number contained in the specified read-only character span, to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The representation of a number as a read-only span of characters.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or zero (0) if the conversion fails. The conversion fails if the <paramref name="value" /> parameter is an empty character span or is not of the correct format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("_ded03bf84977945f","static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)","_ded03bf84977945f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _ded03bf84977945f(Uint32Array value, OutValue<BigInt> result);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Numerics.BigInteger" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="value">The representation of a number as a read-only span of characters.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="value" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="value" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Numerics.BigInteger" /> equivalent to the number that is contained in <paramref name="value" />, or <see cref="P:System.Numerics.BigInteger.Zero" /> if the conversion failed. The conversion fails if the <paramref name="value" /> parameter is an empty character span or is not in a format that is compliant with <paramref name="style" />. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> or <see cref="F:System.Globalization.NumberStyles.HexNumber" /> flag along with another value.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("_d733f0a0a427d970","static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Numerics.BigInteger)","_d733f0a0a427d970")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _d733f0a0a427d970(Uint32Array value, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<BigInt> result);

    ///<summary>Compares two <see cref="T:System.Numerics.BigInteger" /> values and returns an integer that indicates whether the first value is less than, equal to, or greater than the second value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>A signed integer that indicates the relative values of <paramref name="left" /> and <paramref name="right" />, as shown in the following table. <list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="left" /> is less than <paramref name="right" />.</description></item><item><term> Zero</term><description><paramref name="left" /> equals <paramref name="right" />.</description></item><item><term> Greater than zero</term><description><paramref name="left" /> is greater than <paramref name="right" />.</description></item></list></returns>
    [WhiteList("_0a6134f61ab96205","static System.Numerics.BigInteger.Compare(System.Numerics.BigInteger, System.Numerics.BigInteger)","_0a6134f61ab96205")]
	[ECMAScriptLiteral("@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _0a6134f61ab96205(BigInt left, BigInt right);

    ///<summary>Gets the absolute value of a <see cref="T:System.Numerics.BigInteger" /> object.</summary>
    ///<param name="value">A number.</param>
    ///<returns>The absolute value of <paramref name="value" />.</returns>
    [WhiteList("_efd2134803006c44","static System.Numerics.BigInteger.Abs(System.Numerics.BigInteger)","_efd2134803006c44")]
	[ECMAScriptLiteral("@#{0} < 0n ? -@#{0} : @#{0}")]
	public extern static BigInt _efd2134803006c44(BigInt value);

    ///<summary>Adds two <see cref="T:System.Numerics.BigInteger" /> values and returns the result.</summary>
    ///<param name="left">The first value to add.</param>
    ///<param name="right">The second value to add.</param>
    ///<returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("_0034b6a7a416df8e","static System.Numerics.BigInteger.Add(System.Numerics.BigInteger, System.Numerics.BigInteger)","_0034b6a7a416df8e")]
	[ECMAScriptLiteral("@#{0} + @#{1}")]
	public extern static BigInt _0034b6a7a416df8e(BigInt left, BigInt right);

    ///<summary>Subtracts one <see cref="T:System.Numerics.BigInteger" /> value from another and returns the result.</summary>
    ///<param name="left">The value to subtract from (the minuend).</param>
    ///<param name="right">The value to subtract (the subtrahend).</param>
    ///<returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    [WhiteList("_31de7c0189a18bd2","static System.Numerics.BigInteger.Subtract(System.Numerics.BigInteger, System.Numerics.BigInteger)","_31de7c0189a18bd2")]
	[ECMAScriptLiteral("@#{0} - @#{1}")]
	public extern static BigInt _31de7c0189a18bd2(BigInt left, BigInt right);

    ///<summary>Returns the product of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first number to multiply.</param>
    ///<param name="right">The second number to multiply.</param>
    ///<returns>The product of the <paramref name="left" /> and <paramref name="right" /> parameters.</returns>
    [WhiteList("_8c06584cae9fcbe7","static System.Numerics.BigInteger.Multiply(System.Numerics.BigInteger, System.Numerics.BigInteger)","_8c06584cae9fcbe7")]
	[ECMAScriptLiteral("@#{0} * @#{1}")]
	public extern static BigInt _8c06584cae9fcbe7(BigInt left, BigInt right);

    ///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another and returns the result.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The quotient of the division.</returns>
    [WhiteList("_7ff5692b085214c4","static System.Numerics.BigInteger.Divide(System.Numerics.BigInteger, System.Numerics.BigInteger)","_7ff5692b085214c4")]
	[ECMAScriptLiteral("@#{0} / @#{1}")]
	public extern static BigInt _7ff5692b085214c4(BigInt dividend, BigInt divisor);

    ///<summary>Performs integer division on two <see cref="T:System.Numerics.BigInteger" /> values and returns the remainder.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The remainder after dividing <paramref name="dividend" /> by <paramref name="divisor" />.</returns>
    [WhiteList("_00d98488c7edf612","static System.Numerics.BigInteger.Remainder(System.Numerics.BigInteger, System.Numerics.BigInteger)","_00d98488c7edf612")]
	[ECMAScriptLiteral("@#{0} % @#{1}")]
	public extern static BigInt _00d98488c7edf612(BigInt dividend, BigInt divisor);

    ///<summary>Divides one <see cref="T:System.Numerics.BigInteger" /> value by another, returns the result, and returns the remainder in an output parameter.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<param name="remainder">When this method returns, contains a <see cref="T:System.Numerics.BigInteger" /> value that represents the remainder from the division. This parameter is passed uninitialized.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The quotient of the division.</returns>
    [WhiteList("_598611fb2b8a064a","static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger, out System.Numerics.BigInteger)","_598611fb2b8a064a")]
	public static BigInt _598611fb2b8a064a(BigInt dividend, BigInt divisor, OutValue<BigInt> remainder)
	{
		remainder.Value = dividend % divisor;
		return dividend / divisor;
	}

	///<summary>Negates a specified <see cref="T:System.Numerics.BigInteger" /> value.</summary>
	///<param name="value">The value to negate.</param>
	///<returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
	[WhiteList("_d160232d04d4f8fe","static System.Numerics.BigInteger.Negate(System.Numerics.BigInteger)","_d160232d04d4f8fe")]
	[ECMAScriptLiteral("-@#{0}")]
	public extern static BigInt _d160232d04d4f8fe(BigInt value);

    ///<summary>Returns the natural (base <see langword="e" />) logarithm of a specified number.</summary>
    ///<param name="value">The number whose logarithm is to be found.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The natural log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
    ///<returns>The natural (base <see langword="e" />) logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
    [WhiteList("_fb5a811e7a32a324", "static System.Numerics.BigInteger.Log(System.Numerics.BigInteger)","_fb5a811e7a32a324")]
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
    ///<param name="value">A number whose logarithm is to be found.</param>
    ///<param name="baseValue">The base of the logarithm.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The log of <paramref name="value" /> is out of range of the <see cref="T:System.Double" /> data type.</exception>
    ///<returns>The base <paramref name="baseValue" /> logarithm of <paramref name="value" />, as shown in the table in the Remarks section.</returns>
    [WhiteList("_acb5aef300c8db0c","static System.Numerics.BigInteger.Log(System.Numerics.BigInteger, double)","_acb5aef300c8db0c")]
	public static Number _acb5aef300c8db0c(BigInt value, Number baseValue)
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
	[WhiteList("_f276cbd7c3b305ea","static System.Numerics.BigInteger.Log10(System.Numerics.BigInteger)","_f276cbd7c3b305ea")]
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
	///<param name="left">The first value.</param>
	///<param name="right">The second value.</param>
	///<returns>The greatest common divisor of <paramref name="left" /> and <paramref name="right" />.</returns>
	[WhiteList("_7555649a5efc7b79","static System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger, System.Numerics.BigInteger)","_7555649a5efc7b79")]
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
	///<param name="left">The first value to compare.</param>
	///<param name="right">The second value to compare.</param>
	///<returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is larger.</returns>
	[WhiteList("_a038619e95a6c0ff","static System.Numerics.BigInteger.Max(System.Numerics.BigInteger, System.Numerics.BigInteger)","_a038619e95a6c0ff")]
	[ECMAScriptLiteral("@#{0} > @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _a038619e95a6c0ff(BigInt left, BigInt right);

    ///<summary>Returns the smaller of two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>The <paramref name="left" /> or <paramref name="right" /> parameter, whichever is smaller.</returns>
    [WhiteList("_b3b093dd81ed2d15","static System.Numerics.BigInteger.Min(System.Numerics.BigInteger, System.Numerics.BigInteger)","_b3b093dd81ed2d15")]
	[ECMAScriptLiteral("@#{0} < @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _b3b093dd81ed2d15(BigInt left, BigInt right);

    ///<summary>Performs modulus division on a number raised to the power of another number.</summary>
    ///<param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
    ///<param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
    ///<param name="modulus">The number by which to divide <paramref name="value" /> raised to the <paramref name="exponent" /> power.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="modulus" /> is zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="exponent" /> is negative.</exception>
    ///<returns>The remainder after dividing <paramref name="value" />exponent by <paramref name="modulus" />.</returns>
    [WhiteList("_ec6961a106ca5bf3","static System.Numerics.BigInteger.ModPow(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)","_ec6961a106ca5bf3")]
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
	///<param name="value">The number to raise to the <paramref name="exponent" /> power.</param>
	///<param name="exponent">The exponent to raise <paramref name="value" /> by.</param>
	///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="exponent" /> is negative.</exception>
	///<returns>The result of raising <paramref name="value" /> to the <paramref name="exponent" /> power.</returns>
	[WhiteList("_31cf4d89164dee40","static System.Numerics.BigInteger.Pow(System.Numerics.BigInteger, int)","_31cf4d89164dee40")]
    [Obsolete("Not Support in Jazor",true)]
	public static BigInt _31cf4d89164dee40(BigInt value, Number exponent)
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
	[WhiteList("_fe64082374302a77","override System.Numerics.BigInteger.GetHashCode)","_fe64082374302a77")]
	public static Number _fe64082374302a77(BigInt instance)
	{
		var positiveValue = instance < BigInt.Zero ? -instance : instance;
		return Number(positiveValue % BigInt(2147483647));
	}

	///<summary>Returns a value that indicates whether the current instance and a specified object have the same value.</summary>
	///<param name="obj">The object to compare.</param>
	///<returns>  <see langword="true" /> if the <paramref name="obj" /> argument is a <see cref="T:System.Numerics.BigInteger" /> object, and its value is equal to the value of the current <see cref="T:System.Numerics.BigInteger" /> instance; otherwise, <see langword="false" />.</returns>
	[WhiteList("_27c2f0d965e3403d","override System.Numerics.BigInteger.Equals(object)","_27c2f0d965e3403d")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _27c2f0d965e3403d(BigInt instance, Object? obj);

    ///<summary>Returns a value that indicates whether the current instance and a signed 64-bit integer have the same value.</summary>
    ///<param name="other">The signed 64-bit integer value to compare.</param>
    ///<returns>  <see langword="true" /> if the signed 64-bit integer and the current instance have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_21afeec99b7ab2ca","override System.Numerics.BigInteger.Equals(long)","_21afeec99b7ab2ca")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _21afeec99b7ab2ca(BigInt instance, BigInt other);

    ///<summary>Returns a value that indicates whether the current instance and an unsigned 64-bit integer have the same value.</summary>
    ///<param name="other">The unsigned 64-bit integer to compare.</param>
    ///<returns>  <see langword="true" /> if the current instance and the unsigned 64-bit integer have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_134be6ec440e455e","System.Numerics.BigInteger.Equals(ulong)")]
    [ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _134be6ec440e455e(BigInt instance, BigInt other);

    ///<summary>Returns a value that indicates whether the current instance and a specified <see cref="T:System.Numerics.BigInteger" /> object have the same value.</summary>
    ///<param name="other">The object to compare.</param>
    ///<returns>  <see langword="true" /> if this <see cref="T:System.Numerics.BigInteger" /> object and <paramref name="other" /> have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_4d44e94420c56981","System.Numerics.BigInteger.Equals(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _4d44e94420c56981(BigInt instance, BigInt other);

    ///<summary>Compares this instance to a signed 64-bit integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the signed 64-bit integer.</summary>
    ///<param name="other">The signed 64-bit integer to compare.</param>
    ///<returns>A signed integer value that indicates the relationship of this instance to <paramref name="other" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Description</description></listheader><item><term> Less than zero</term><description> The current instance is less than <paramref name="other" />.</description></item><item><term> Zero</term><description> The current instance equals <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> The current instance is greater than <paramref name="other" />.</description></item></list></returns>
    [WhiteList("_77851a1e7ef48cb7","System.Numerics.BigInteger.CompareTo(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _77851a1e7ef48cb7(BigInt instance, BigInt other);

    ///<summary>Compares this instance to an unsigned 64-bit integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the unsigned 64-bit integer.</summary>
    ///<param name="other">The unsigned 64-bit integer to compare.</param>
    ///<returns>A signed integer that indicates the relative value of this instance and <paramref name="other" />, as shown in the following table.          <list type="table"><listheader><term>Return value</term><description>Description</description></listheader><item><term>Less than zero</term><description>The current instance is less than <paramref name="other" />.</description></item><item><term>Zero</term><description>The current instance equals <paramref name="other" />.</description></item><item><term>Greater than zero</term><description>The current instance is greater than <paramref name="other" />.</description></item></list></returns>
    [WhiteList("_64e348c0c7830a5c","System.Numerics.BigInteger.CompareTo(ulong)")]
	[ECMAScriptLiteral("@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _64e348c0c7830a5c(BigInt instance, BigInt other);

    ///<summary>Compares this instance to a second <see cref="T:System.Numerics.BigInteger" /> and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
    ///<param name="other">The object to compare.</param>
    ///<returns>A signed integer value that indicates the relationship of this instance to <paramref name="other" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Description</description></listheader><item><term> Less than zero</term><description> The current instance is less than <paramref name="other" />.</description></item><item><term> Zero</term><description> The current instance equals <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> The current instance is greater than <paramref name="other" />.</description></item></list></returns>
    [WhiteList("_02bf2f34cf157e4d","System.Numerics.BigInteger.CompareTo(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _02bf2f34cf157e4d(BigInt instance, BigInt other);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
    ///<param name="obj">The object to compare.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="obj" /> is not a <see cref="T:System.Numerics.BigInteger" />.</exception>
    ///<returns>A signed integer that indicates the relationship of the current instance to the <paramref name="obj" /> parameter, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Description</description></listheader><item><term> Less than zero</term><description> The current instance is less than <paramref name="obj" />.</description></item><item><term> Zero</term><description> The current instance equals <paramref name="obj" />.</description></item><item><term> Greater than zero</term><description> The current instance is greater than <paramref name="obj" />, or the <paramref name="obj" /> parameter is <see langword="null" />.</description></item></list></returns>
    [WhiteList("_9f7b3705890bed98","System.Numerics.BigInteger.CompareTo(object)")]
	[ECMAScriptLiteral("@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _9f7b3705890bed98(BigInt instance, Object? obj);

    ///<summary>Converts a <see cref="T:System.Numerics.BigInteger" /> value to a byte array.</summary>
    ///<returns>The value of the current <see cref="T:System.Numerics.BigInteger" /> object converted to an array of bytes.</returns>
    [WhiteList("_ca46777d5c8cc9b9","System.Numerics.BigInteger.ToByteArray()")]
	public static byte[] _ca46777d5c8cc9b9(BigInt instance)
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
	[WhiteList("_11ed9d474ccf2419","System.Numerics.BigInteger.ToByteArray(bool, bool)")]
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
	///<param name="destination">The destination span to which the resulting bytes should be written.</param>
	///<param name="bytesWritten">The number of bytes written to <paramref name="destination" />.</param>
	///<param name="isUnsigned">  <see langword="true" /> to use unsigned encoding; otherwise, <see langword="false" />.</param>
	///<param name="isBigEndian">  <see langword="true" /> to write the bytes in a big-endian byte order; otherwise, <see langword="false" />.</param>
	///<exception cref="T:System.OverflowException">  <paramref name="isUnsigned" /> is <see langword="true" /> and <see cref="P:System.Numerics.BigInteger.Sign" /> is negative.</exception>
	///<returns>  <see langword="true" /> if the bytes fit in <paramref name="destination" />; <see langword="false" /> if not all bytes could be written due to lack of space.</returns>
	[WhiteList("_76ae4e496fc976fd","System.Numerics.BigInteger.TryWriteBytes(System.Span<byte>, out int, bool, bool)")]
	public static bool _76ae4e496fc976fd(BigInt instance, Uint8Array destination, OutValue<Number> bytesWritten, bool isUnsigned, bool isBigEndian)
	{
		// 1. ���������ֽ���
		var requiredBytes = 1; // ������Ҫ1�ֽ�
		if (instance != BigInt.Zero)
		{
			var isNegative = !isUnsigned && instance < BigInt.Zero;
			var value = isNegative ? (isUnsigned ? instance : -instance - BigInt.One) : instance;
			var bitLength = 0u;

			// ����λ����
			while (value > BigInt.Zero)
			{
				bitLength++;
				value >>= BigInt.One;
			}

			// ���������ֽ�
			requiredBytes = isUnsigned
				? Maths.Max(1, Maths.Ceil(bitLength / 8))
				: Maths.Max(1, Maths.Ceil((bitLength + 1) / 8));
		}

		// 2. ��黺������С
		if (destination.Length < requiredBytes)
		{
			bytesWritten.Value = 0;
			return false;
		}

		// 3. ת��Ϊ�ֽ�����
		var bytes = new Array<byte>();
		if (instance == BigInt.Zero)
			bytes.Push(0);
		else
		{
			var isNegative = !isUnsigned && instance < BigInt.Zero;
			var value = isNegative ? -instance - BigInt.One : instance;

			// ����ʵ����Ҫ�������ֽ���
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

			// ������������
			if (isNegative)
			{
				for (var i = 0u; i < bytes.Length; i++)
					bytes[i] = (byte)((~bytes[i]) & 0xFF);

				// ȷ������λ��ȷ
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

		// 4. ��������ֽ����Ƿ񳬳�������
		if (bytes.Length > destination.Length)
		{
			bytesWritten.Value = 0;
			return false;
		}

		// 5. д��Ŀ������
		for (var i = 0u; i < bytes.Length; i++)
			destination[i] = bytes[i];

		// 6. ���ʣ���ֽڣ������Ҫ��
		var fillByte = !isUnsigned && instance < BigInt.Zero ? 0xFF : 0;
		for (var i = bytes.Length; i < destination.Length; i++)
			destination[i] = (byte)fillByte;

		bytesWritten.Value = bytes.Length;
		return true;
	}

	///<summary>Gets the number of bytes that will be output by <see cref="M:System.Numerics.BigInteger.ToByteArray(System.Boolean,System.Boolean)" /> and <see cref="M:System.Numerics.BigInteger.TryWriteBytes(System.Span{System.Byte},System.Int32@,System.Boolean,System.Boolean)" />.</summary>
	///<param name="isUnsigned">  <see langword="true" /> to use unsigned encoding; otherwise, <see langword="false" />.</param>
	///<returns>The number of bytes.</returns>
	[WhiteList("_c1393b267008395c","System.Numerics.BigInteger.GetByteCount(bool)")]
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
				? Maths.Max(1, Maths.Ceil((bitLength + 1) / 8))
				: Maths.Max(1, Maths.Ceil((bitLength + 1) / 8));
	}

	///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation.</summary>
	///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value.</returns>
	[WhiteList("_a7388cc0c5bc22ad","override System.Numerics.BigInteger.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _a7388cc0c5bc22ad(BigInt instance);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value in the format specified by the <paramref name="provider" /> parameter.</returns>
    [WhiteList("_fe4c3211e57446e7","System.Numerics.BigInteger.ToString(System.IFormatProvider)")]
	public static string _fe4c3211e57446e7(BigInt instance, Intl.NumberFormat? provider)
	{
		if (provider is null)
			return instance.ToString();

		var isNegative = instance < BigInt.Zero;
		var absValue = isNegative ? -instance : instance;
		var strValue = absValue.ToString();

		try
		{
			// ����ֱ��ʹ��Intl.NumberFormat�������ڰ�ȫ������Χ��
			if (absValue <= BigInt(Number.MAX_SAFE_INTEGER))
			{
				var formatted = provider.Format(Number(absValue));
				return isNegative ? $"-{formatted}" : formatted;
			}

			// ���ڳ��������ֶ�ʵ�ֻ������ػ���ʽ��
			var sample = provider.Format(1000.1);
			var groupChar = sample.Includes("1,000") ? "," :
							 sample.Includes("1.000") ? "." :
							 sample.Includes("1 000") ? " " : ",";

			// �����������ӷ���ָ���
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
	[WhiteList("_1650d30e3e9172f5","System.Numerics.BigInteger.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _1650d30e3e9172f5(BigInt instance, string? format);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Numerics.BigInteger" /> object to its equivalent string representation by using the specified format and culture-specific format information.</summary>
    ///<param name="format">A standard or custom numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is not a valid format string.</exception>
    ///<returns>The string representation of the current <see cref="T:System.Numerics.BigInteger" /> value as specified by the <paramref name="format" /> and <paramref name="provider" /> parameters.</returns>
    [WhiteList("_93b0cfb45a1832e9","System.Numerics.BigInteger.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _93b0cfb45a1832e9(BigInt instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Formats this big integer instance into a span of characters.</summary>
    ///<param name="destination">The span of characters into which this instance will be written.</param>
    ///<param name="charsWritten">When the method returns, contains the length of the span in number of characters.</param>
    ///<param name="format">A read-only span of characters that specifies the format for the formatting operation.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>  <see langword="true" /> if the formatting operation succeeds; <see langword="false" /> otherwise.</returns>
    [WhiteList("_90c190be387330ea","System.Numerics.BigInteger.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _90c190be387330ea(BigInt instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Subtracts a <see cref="T:System.Numerics.BigInteger" /> value from another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The value to subtract from (the minuend).</param>
    ///<param name="right">The value to subtract (the subtrahend).</param>
    ///<returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    [WhiteList("_28554dca4c0c49f8","static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} - {1}")]
	public extern static BigInt _28554dca4c0c49f8(BigInt left, BigInt right);

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned byte value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Byte" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_c1afe3218f0f82f9","static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c1afe3218f0f82f9();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.Char" /> value.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to <see cref="T:System.Char" /> value.</returns>
    [WhiteList("_ac2920ee8216c023","static System.Numerics.BigInteger.explicit operator char(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _ac2920ee8216c023();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Decimal" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Decimal" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_9d2085a2aa8febea","static System.Numerics.BigInteger.explicit operator System.Decimal(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Decimal _9d2085a2aa8febea();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a <see cref="T:System.Double" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Double" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_4a6bc22c1d5cd472","static System.Numerics.BigInteger.explicit operator double(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _4a6bc22c1d5cd472();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.Half" /> value.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to <see cref="T:System.Half" /> value.</returns>
    [WhiteList("_7c41bbf7746a0266","static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Half _7c41bbf7746a0266();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 16-bit signed integer value.</summary>
    ///<param name="value">The value to convert to a 16-bit signed integer.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or is greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_c57fc79b767bf069","static System.Numerics.BigInteger.explicit operator short(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c57fc79b767bf069();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 32-bit signed integer value.</summary>
    ///<param name="value">The value to convert to a 32-bit signed integer.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or is greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_7c261f922cc43235","static System.Numerics.BigInteger.explicit operator int(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _7c261f922cc43235();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a 64-bit signed integer value.</summary>
    ///<param name="value">The value to convert to a 64-bit signed integer.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or is greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_15fe350cf299c580","static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _15fe350cf299c580();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.Int128" /> value.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to <see cref="T:System.Int128" /> value.</returns>
    [WhiteList("_5958070a15559320","static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _5958070a15559320();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.IntPtr" /> value.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to <see cref="T:System.IntPtr" /> value.</returns>
    [WhiteList("_11cea9efbc3d0c62","static System.Numerics.BigInteger.explicit operator nint(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static nint _11cea9efbc3d0c62();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a signed 8-bit value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int16" />.</summary>
    ///<param name="value">The value to convert to a signed 8-bit value.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue">SByte.MinValue</see> or is greater than <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_63d8cc7789144528","static System.Numerics.BigInteger.explicit operator sbyte(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _63d8cc7789144528();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to a single-precision floating-point value.</summary>
    ///<param name="value">The value to convert to a single-precision floating-point value.</param>
    ///<returns>An object that contains the closest possible representation of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_24972b9ed8006ec8","static System.Numerics.BigInteger.explicit operator float(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _24972b9ed8006ec8();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 16-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int32" />.</summary>
    ///<param name="value">The value to convert to an unsigned 16-bit integer.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or is greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_b2311568a6faa3b8","static System.Numerics.BigInteger.explicit operator ushort(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _b2311568a6faa3b8();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 32-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Int64" />.</summary>
    ///<param name="value">The value to convert to an unsigned 32-bit integer.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or is greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_385437ecb9a2b10a","static System.Numerics.BigInteger.explicit operator uint(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _385437ecb9a2b10a();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> object to an unsigned 64-bit integer value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Double" />.</summary>
    ///<param name="value">The value to convert to an unsigned 64-bit integer.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.UInt64.MinValue">UInt64.MinValue</see> or is greater than <see cref="F:System.UInt64.MaxValue">UInt64.MaxValue</see>.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_6043725cddf263dd","static System.Numerics.BigInteger.explicit operator ulong(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _6043725cddf263dd();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.UInt128" /> value.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to <see cref="T:System.UInt128" /> value.</returns>
    [WhiteList("_f8ae8a4213449843","static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _f8ae8a4213449843();

    ///<summary>Explicitly converts a big integer to a <see cref="T:System.UIntPtr" /> value.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to <see cref="T:System.UIntPtr" /> value.</returns>
    [WhiteList("_bbf68528b2eedf55","static System.Numerics.BigInteger.explicit operator nuint(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static nuint _bbf68528b2eedf55();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> object to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_8e505e0ce7efa99c","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _8e505e0ce7efa99c();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Double" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_933b3164355c792a","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _933b3164355c792a();

    ///<summary>Explicitly converts a <see cref="T:System.Half" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_c186238bc3a46d2b","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Half)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _c186238bc3a46d2b();

    ///<summary>Explicitly converts a <see cref="T:System.Numerics.Complex" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_088fa1b2a09829ce","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Numerics.Complex)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _088fa1b2a09829ce();

    ///<summary>Defines an explicit conversion of a <see cref="T:System.Single" /> value to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</exception>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_212b6e60ce4e6836","static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _212b6e60ce4e6836();

    ///<summary>Defines an implicit conversion of an unsigned byte to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_24f94dfe434ed1de","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _24f94dfe434ed1de();

    ///<summary>Implicitly converts a <see cref="T:System.Char" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_6f52f939cef7ebfc","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _6f52f939cef7ebfc();

    ///<summary>Defines an implicit conversion of a signed 16-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_5eb359c063a4b04b","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _5eb359c063a4b04b();

    ///<summary>Defines an implicit conversion of a signed 32-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_84639f9693379307","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _84639f9693379307();

    ///<summary>Defines an implicit conversion of a signed 64-bit integer to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_7de492bb278503c8","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _7de492bb278503c8();

    ///<summary>Implicitly converts a <see cref="T:System.Int128" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_aa5bafc867e9b5eb","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.Int128)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _aa5bafc867e9b5eb();

    ///<summary>Implicitly converts a <see cref="T:System.IntPtr" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_70a902bafd0ce64e","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(nint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _70a902bafd0ce64e();

    ///<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.#ctor(System.Int32)" />.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_ff8ba3cf17ec3f75","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(sbyte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _ff8ba3cf17ec3f75();

    ///<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.op_Implicit(System.Int32)~System.Numerics.BigInteger" />.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_9b2419d65cfa19ab","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ushort)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _9b2419d65cfa19ab();

    ///<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="M:System.Numerics.BigInteger.op_Implicit(System.Int64)~System.Numerics.BigInteger" />.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_cf078fbbc4130e0c","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _cf078fbbc4130e0c();

    ///<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Numerics.BigInteger" /> value. This API is not CLS-compliant. The compliant alternative is <see cref="T:System.Double" />.</summary>
    ///<param name="value">The value to convert to a <see cref="T:System.Numerics.BigInteger" />.</param>
    ///<returns>An object that contains the value of the <paramref name="value" /> parameter.</returns>
    [WhiteList("_9b4a5ecbd0f90bd4","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ulong)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _9b4a5ecbd0f90bd4();

    ///<summary>Implicitly converts a <see cref="T:System.UInt128" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_16f7ae7cb82a7523","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.UInt128)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _16f7ae7cb82a7523();

    ///<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a big integer.</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>  <paramref name="value" /> converted to a big integer.</returns>
    [WhiteList("_b7ee0d78d7054a45","static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(nuint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _b7ee0d78d7054a45();

    ///<summary>Performs a bitwise <see langword="And" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The result of the bitwise <see langword="And" /> operation.</returns>
    [WhiteList("_4a529c0a5388c594","static System.Numerics.BigInteger.operator &(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} & {1}")]
	public extern static BigInt _4a529c0a5388c594(BigInt left, BigInt right);

    ///<summary>Performs a bitwise <see langword="Or" /> operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The result of the bitwise <see langword="Or" /> operation.</returns>
    [WhiteList("_752fd4cd29f4f204","static System.Numerics.BigInteger.operator |(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} | {1}")]
	public extern static BigInt _752fd4cd29f4f204(BigInt left, BigInt right);

    ///<summary>Performs a bitwise exclusive <see langword="Or" /> (<see langword="XOr" />) operation on two <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value.</param>
    ///<param name="right">The second value.</param>
    ///<returns>The result of the bitwise <see langword="Or" /> operation.</returns>
    [WhiteList("_a453418c13f7f875","static System.Numerics.BigInteger.operator ^(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} ^ {1}")]
	public extern static BigInt _a453418c13f7f875(BigInt left, BigInt right);

    ///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the left.</summary>
    ///<param name="value">The value whose bits are to be shifted.</param>
    ///<param name="shift">The number of bits to shift <paramref name="value" /> to the left.</param>
    ///<returns>A value that has been shifted to the left by the specified number of bits.</returns>
    [WhiteList("_a29a9a670145ce5e","static System.Numerics.BigInteger.operator <<(System.Numerics.BigInteger, int)")]
    [ECMAScriptLiteral("{0} << {1}")]
	public extern static BigInt _a29a9a670145ce5e(BigInt value, Number shift);

    ///<summary>Shifts a <see cref="T:System.Numerics.BigInteger" /> value a specified number of bits to the right.</summary>
    ///<param name="value">The value whose bits are to be shifted.</param>
    ///<param name="shift">The number of bits to shift <paramref name="value" /> to the right.</param>
    ///<returns>A value that has been shifted to the right by the specified number of bits.</returns>
    [WhiteList("_c0bed6f115403624","static System.Numerics.BigInteger.operator >>(System.Numerics.BigInteger, int)")]
    [ECMAScriptLiteral("{0} >> {1}")]
	public extern static BigInt _c0bed6f115403624(BigInt value, Number shift);

    ///<summary>Returns the bitwise one's complement of a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="value">An integer value.</param>
    ///<returns>The bitwise one's complement of <paramref name="value" />.</returns>
    [WhiteList("_9182cf8afd9b8590","static System.Numerics.BigInteger.operator ~(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("~{0}")]
	public extern static BigInt _9182cf8afd9b8590(BigInt value);

    ///<summary>Negates a specified BigInteger value.</summary>
    ///<param name="value">The value to negate.</param>
    ///<returns>The result of the <paramref name="value" /> parameter multiplied by negative one (-1).</returns>
    [WhiteList("_03be17d45cbe5034","static System.Numerics.BigInteger.operator -(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("-{0}")]
	public extern static BigInt _03be17d45cbe5034(BigInt value);

    ///<summary>Returns the value of the <see cref="T:System.Numerics.BigInteger" /> operand. (The sign of the operand is unchanged.)</summary>
    ///<param name="value">An integer value.</param>
    ///<returns>The value of the <paramref name="value" /> operand.</returns>
    [WhiteList("_7096f6c4ea9fddaf","static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("+{0}")]
	public extern static BigInt _7096f6c4ea9fddaf(BigInt value);

    ///<summary>Increments a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
    ///<param name="value">The value to increment.</param>
    ///<returns>The value of the <paramref name="value" /> parameter incremented by 1.</returns>
    [WhiteList("_cc35859d07374d52","static System.Numerics.BigInteger.operator ++(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("++{0}")]
	public extern static BigInt _cc35859d07374d52(BigInt value);

    ///<summary>Decrements a <see cref="T:System.Numerics.BigInteger" /> value by 1.</summary>
    ///<param name="value">The value to decrement.</param>
    ///<returns>The value of the <paramref name="value" /> parameter decremented by 1.</returns>
    [WhiteList("_6d2fe51e4158a46f","static System.Numerics.BigInteger.operator --(System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("--{0}")]
	public extern static BigInt _6d2fe51e4158a46f(BigInt value);

    ///<summary>Adds the values of two specified <see cref="T:System.Numerics.BigInteger" /> objects.</summary>
    ///<param name="left">The first value to add.</param>
    ///<param name="right">The second value to add.</param>
    ///<returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("_4edde875924e9396","static System.Numerics.BigInteger.operator +(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} + {1}")]
	public extern static BigInt _4edde875924e9396(BigInt left, BigInt right);

    ///<summary>Multiplies two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="left">The first value to multiply.</param>
    ///<param name="right">The second value to multiply.</param>
    ///<returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
    [WhiteList("_3baa1e316a9a8e5c","static System.Numerics.BigInteger.operator *(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} * {1}")]
	public extern static BigInt _3baa1e316a9a8e5c(BigInt left, BigInt right);

    ///<summary>Divides a specified <see cref="T:System.Numerics.BigInteger" /> value by another specified <see cref="T:System.Numerics.BigInteger" /> value by using integer division.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The integral result of the division.</returns>
    [WhiteList("_e87ac03cab9bfae9","static System.Numerics.BigInteger.operator /(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} / {1}")]
	public extern static BigInt _e87ac03cab9bfae9(BigInt dividend, BigInt divisor);

    ///<summary>Returns the remainder that results from division with two specified <see cref="T:System.Numerics.BigInteger" /> values.</summary>
    ///<param name="dividend">The value to be divided.</param>
    ///<param name="divisor">The value to divide by.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="divisor" /> is 0 (zero).</exception>
    ///<returns>The remainder that results from the division.</returns>
    [WhiteList("_44f6e17ba281115c","static System.Numerics.BigInteger.operator %(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} % {1}")]
	public extern static BigInt _44f6e17ba281115c(BigInt dividend, BigInt divisor);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_c921d6c6bf72edae","static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool _c921d6c6bf72edae(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_4175fbcd1bdcbb81","static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool _4175fbcd1bdcbb81(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_38487ed1f787d018","static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool _38487ed1f787d018(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to another <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_c69d246ab7c4d01a","static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool _c69d246ab7c4d01a(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether the values of two <see cref="T:System.Numerics.BigInteger" /> objects are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_a1bca47181bf0a21","static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool _a1bca47181bf0a21(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether two <see cref="T:System.Numerics.BigInteger" /> objects have different values.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("_fa04bb024b763d8c","static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool _fa04bb024b763d8c(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit signed integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_54b970a90a63bed7","static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, long)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool _54b970a90a63bed7(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit signed integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_c5121fb5bb0459d9","static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, long)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool _c5121fb5bb0459d9(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> is greater than a 64-bit signed integer value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_f633b7dba945231e","static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, long)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool _f633b7dba945231e(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit signed integer value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_05b14b64a3ed932c","static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, long)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool _05b14b64a3ed932c(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a signed long integer value are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_23fab9d29faa7b4b","static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, long)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool _23fab9d29faa7b4b(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit signed integer are not equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("_bee7ae6c7fd4ccab","static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, long)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool _bee7ae6c7fd4ccab(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_9e956828e15a31ac","static System.Numerics.BigInteger.operator <(long, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool _9e956828e15a31ac(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_837c7f79427d7687","static System.Numerics.BigInteger.operator <=(long, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool _837c7f79427d7687(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is greater than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_599b5035513f4697","static System.Numerics.BigInteger.operator >(long, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool _599b5035513f4697(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_b8852469edf6fccb","static System.Numerics.BigInteger.operator >=(long, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool _b8852469edf6fccb(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a signed long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_17b7667af4b23f69","static System.Numerics.BigInteger.operator ==(long, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool _17b7667af4b23f69(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit signed integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("_d3215df5ab1e7b4b","static System.Numerics.BigInteger.operator !=(long, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool _d3215df5ab1e7b4b(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than a 64-bit unsigned integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_1f387e21472ec766","static System.Numerics.BigInteger.operator <(System.Numerics.BigInteger, ulong)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool _1f387e21472ec766(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is less than or equal to a 64-bit unsigned integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_8bf92299327b0564","static System.Numerics.BigInteger.operator <=(System.Numerics.BigInteger, ulong)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool _8bf92299327b0564(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_85a372957af0ef3d","static System.Numerics.BigInteger.operator >(System.Numerics.BigInteger, ulong)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool _85a372957af0ef3d(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than or equal to a 64-bit unsigned integer value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_027db5ec51a792f8","static System.Numerics.BigInteger.operator >=(System.Numerics.BigInteger, ulong)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool _027db5ec51a792f8(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and an unsigned long integer value are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_90393e5796d20760","static System.Numerics.BigInteger.operator ==(System.Numerics.BigInteger, ulong)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool _90393e5796d20760(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value and a 64-bit unsigned integer are not equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("_83ed1eafdd051a37","static System.Numerics.BigInteger.operator !=(System.Numerics.BigInteger, ulong)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool _83ed1eafdd051a37(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer is less than a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_229e97bf2be53319","static System.Numerics.BigInteger.operator <(ulong, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool _229e97bf2be53319(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer is less than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_7f4a3ea98d5e7194","static System.Numerics.BigInteger.operator <=(ulong, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool _7f4a3ea98d5e7194(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a <see cref="T:System.Numerics.BigInteger" /> value is greater than a 64-bit unsigned integer.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_e54a63c735fb2514","static System.Numerics.BigInteger.operator >(ulong, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool _e54a63c735fb2514(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer is greater than or equal to a <see cref="T:System.Numerics.BigInteger" /> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_3a5b1bba5ac45b9c","static System.Numerics.BigInteger.operator >=(ulong, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool _3a5b1bba5ac45b9c(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether an unsigned long integer value and a <see cref="T:System.Numerics.BigInteger" /> value are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, <see langword="false" />.</returns>
    [WhiteList("_a97fbe9f639a835b","static System.Numerics.BigInteger.operator ==(ulong, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool _a97fbe9f639a835b(BigInt left, BigInt right);

    ///<summary>Returns a value that indicates whether a 64-bit unsigned integer and a <see cref="T:System.Numerics.BigInteger" /> value are not equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [WhiteList("_ac5266c1db09af16","static System.Numerics.BigInteger.operator !=(ulong, System.Numerics.BigInteger)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool _ac5266c1db09af16(BigInt left, BigInt right);

    ///<summary>Gets the number of bits required for shortest two's complement representation of the current instance without the sign bit.</summary>
    ///<returns>The minimum non-negative number of bits in two's complement notation without the sign bit.</returns>
    [WhiteList("_41fe76dfb4ee2ab2","System.Numerics.BigInteger.GetBitLength()")]
    [Obsolete("Not Support in Jazor",true)]
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
	///<param name="left">The value that <paramref name="right" /> divides.</param>
	///<param name="right">The value that divides <paramref name="left" />.</param>
	///<returns>The quotient and remainder of <paramref name="left" /> divided-by <paramref name="right" />.</returns>
	[WhiteList("_22a21ffe19479f32","static System.Numerics.BigInteger.DivRem(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	public static (BigInt, BigInt) _22a21ffe19479f32(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new RangeError("Division by zero");

		var quotient = left / right;
		var remainder = left % right;

		return (quotient, remainder);
	}

	///<summary>Computes the number of leading zeros in a value.</summary>
	///<param name="value">The value whose leading zeroes are to be counted.</param>
	///<returns>The number of leading zeros in <paramref name="value" />.</returns>
	[WhiteList("_276680abacb93277","static System.Numerics.BigInteger.LeadingZeroCount(System.Numerics.BigInteger)")]
	public static BigInt _276680abacb93277(BigInt value)
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
	///<returns>The number of set bits in <paramref name="value" />.</returns>
	[WhiteList("_5e476c376aca56ae","static System.Numerics.BigInteger.PopCount(System.Numerics.BigInteger)")]
	public static BigInt _5e476c376aca56ae(BigInt value)
	{
		if (value == BigInt.Zero)
			return BigInt.Zero;

		var count = BigInt.Zero;
		var n = value < BigInt.Zero ? -value - BigInt.One : value;

		// Brian Kernighan�㷨
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
	[WhiteList("_ae7b1dd18af32f04","static System.Numerics.BigInteger.RotateLeft(System.Numerics.BigInteger, int)")]
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
	///<param name="value">The value that's rotated right by <paramref name="rotateAmount" />.</param>
	///<param name="rotateAmount">The amount by which <paramref name="value" /> is rotated right.</param>
	///<returns>The result of rotating <paramref name="value" /> right by <paramref name="rotateAmount" />.</returns>
	[WhiteList("_dc8cc860511e78b3","static System.Numerics.BigInteger.RotateRight(System.Numerics.BigInteger, int)")]
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
	///<param name="value">The value whose trailing zeroes are to be counted.</param>
	///<returns>The number of trailing zeros in <paramref name="value" />.</returns>
	[WhiteList("_696502aae4b6e182","static System.Numerics.BigInteger.TrailingZeroCount(System.Numerics.BigInteger)")]
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
	///<param name="value">The value to be checked.</param>
	///<returns>  <see langword="true" /> if <paramref name="value" /> is a power of two; otherwise, <see langword="false" />.</returns>
	[WhiteList("_c0651d019a4b12b1","static System.Numerics.BigInteger.IsPow2(System.Numerics.BigInteger)")]
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
	///<param name="value">The value whose log2 is to be computed.</param>
	///<returns>The log2 of <paramref name="value" />.</returns>
	[WhiteList("_c29a05a989ec3b33","static System.Numerics.BigInteger.Log2(System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
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
	///<param name="value">The value to clamp.</param>
	///<param name="min">The inclusive minimum to which <paramref name="value" /> should clamp.</param>
	///<param name="max">The inclusive maximum to which <paramref name="value" /> should clamp.</param>
	///<returns>The result of clamping <paramref name="value" /> to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</returns>
	[WhiteList("_8548cc83c4d947f5","static System.Numerics.BigInteger.Clamp(System.Numerics.BigInteger, System.Numerics.BigInteger, System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
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
	///<param name="value">The value whose magnitude is used in the result.</param>
	///<param name="sign">The value whose sign is used in the result.</param>
	///<returns>A value with the magnitude of <paramref name="value" /> and the sign of <paramref name="sign" />.</returns>
	[WhiteList("_aa45b92454e3abaa","static System.Numerics.BigInteger.CopySign(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("(@#{1} < 0n ? -1 : 1)*(@#{0} < 0n ? -@#{0} : @#{0})")]
	public extern static BigInt _aa45b92454e3abaa(BigInt value, BigInt sign);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <typeparamref name="TSelf" />.</param>
    ///<typeparam name="TOther">The type of <paramref name="value" />.</typeparam>
    ///<returns>An instance of <typeparamref name="TSelf" /> created from <paramref name="value" />.</returns>
    [WhiteList("_8cbca5624f4a6cc0","static System.Numerics.BigInteger.CreateChecked<TOther>(TOther)")]
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
	///<param name="value">The value that's used to create the instance of <typeparamref name="TSelf" />.</param>
	///<typeparam name="TOther">The type of <paramref name="value" />.</typeparam>
	///<returns>An instance of <typeparamref name="TSelf" /> created from <paramref name="value" />, saturating if <paramref name="value" /> falls outside the representable range of <typeparamref name="TSelf" />.</returns>
	[WhiteList("_f2847eb63549bd6a","static System.Numerics.BigInteger.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _f2847eb63549bd6a<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <typeparamref name="TSelf" />.</param>
    ///<typeparam name="TOther">The type of <paramref name="value" />.</typeparam>
    ///<returns>An instance of <typeparamref name="TSelf" /> created from <paramref name="value" />, truncating if <paramref name="value" /> falls outside the representable range of <typeparamref name="TSelf" />.</returns>
    [WhiteList("_8457175b141355fe","static System.Numerics.BigInteger.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _8457175b141355fe<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is an even integer; otherwise, <see langword="false" />.</returns>
    [WhiteList("_691c1425b8fac31f","static System.Numerics.BigInteger.IsEvenInteger(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("(@#{0} & 1n) === 0n")]
	public extern static bool _691c1425b8fac31f(BigInt value);

    ///<summary>Determines if a value is negative.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is negative; otherwise, <see langword="false" />.</returns>
    [WhiteList("_8cb55ab054b637db","static System.Numerics.BigInteger.IsNegative(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("@#{0} < 0n")]
	public extern static bool _8cb55ab054b637db(BigInt value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is an odd integer; otherwise, <see langword="false" />.</returns>
    [WhiteList("_8213026f03b857e7","static System.Numerics.BigInteger.IsOddInteger(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("(@#{0} & 1n) === 1n")]
	public extern static bool _8213026f03b857e7(BigInt value);

    ///<summary>Determines if a value is positive.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is positive; otherwise, <see langword="false" />.</returns>
    [WhiteList("_386d048147df6eae","static System.Numerics.BigInteger.IsPositive(System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("@#{0} >= 0n")]
	public extern static bool _386d048147df6eae(BigInt value);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <paramref name="y" />.</param>
    ///<param name="y">The value to compare with <paramref name="x" />.</param>
    ///<returns>  <paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
    [WhiteList("_d305de2c64e85995","static System.Numerics.BigInteger.MaxMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("@#{0} > @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _d305de2c64e85995(BigInt x, BigInt y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <paramref name="y" />.</param>
    ///<param name="y">The value to compare with <paramref name="x" />.</param>
    ///<returns>  <paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
    [WhiteList("_fef56ccd17b22e88","static System.Numerics.BigInteger.MinMagnitude(System.Numerics.BigInteger, System.Numerics.BigInteger)")]
	[ECMAScriptLiteral("@#{0} < @#{1} ? @#{0} : @#{1}")]
	public extern static BigInt _fef56ccd17b22e88(BigInt x, BigInt y);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
    ///<returns>  <see langword="true" /> if  <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
    [WhiteList("_10999a356af78aba","static System.Numerics.BigInteger.TryParse(string, System.IFormatProvider, out System.Numerics.BigInteger)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _10999a356af78aba(string? s, Intl.NumberFormat? provider, OutValue<BigInt?> result);

    ///<summary>Shifts a value right by a given amount.</summary>
    ///<param name="value">The value that is shifted right by <paramref name="shiftAmount" />.</param>
    ///<param name="shiftAmount">The amount by which <paramref name="value" /> is shifted right.</param>
    ///<returns>The result of shifting <paramref name="value" /> right by <paramref name="shiftAmount" />.</returns>
    [WhiteList("_49adf7adfc1228f8","static System.Numerics.BigInteger.operator >>>(System.Numerics.BigInteger, int)")]
	public static BigInt _49adf7adfc1228f8(BigInt value, Number shiftAmount)
	{
		if (shiftAmount < 0)
			throw new RangeError("Shift amount must be non-negative");

		var shift = BigInt(shiftAmount);

		if (value >= BigInt.Zero)
			return value >> shift;

		// ���ڸ�������ת��Ϊ������Ч��ʾ��Ȼ������
		// ʹ�� 2^n �Ĳ�������
		var bits = BigInt(64); // ����64λ�����Ը�����Ҫ������
		var mask = (BigInt.One << bits) - BigInt.One;
		var positiveRepresentation = value & mask;
		return positiveRepresentation >> shift;
	}

	///<summary>Parses a span of characters into a value.</summary>
	///<param name="s">The span of characters to parse.</param>
	///<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
	///<returns>The result of parsing <paramref name="s" />.</returns>
	[WhiteList("_8bbfd46a98ce5419","static System.Numerics.BigInteger.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt _8bbfd46a98ce5419(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" />, or an undefined value on failure.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
    [WhiteList("_163b02803ece1f0c","static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Numerics.BigInteger)", "_163b02803ece1f0c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _163b02803ece1f0c(Uint32Array s, Intl.NumberFormat? provider, OutValue<BigInt> result);
}
