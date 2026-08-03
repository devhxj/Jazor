namespace Jazor.CLR;

/// <summary>
/// 固定位宽 BigInt CLR 映射的共享 runtime 算法。
/// </summary>
/// <remarks>
/// JavaScript BigInt 本身没有位宽。调用方必须显式传入边界、mask 和 sign bit，
/// 才能保持 Int64/Int128 等 CLR 类型在解析和位运算处的固定宽度语义。
/// </remarks>
[ECMAScriptModule("System/Numerics/BigIntIntegerRuntime.js")]
internal static class BigIntIntegerRuntime
{
	private static readonly RegExp DecimalIntegerPattern = new(@"^[+-]?\d+$");

	internal static Number CompareToObject(BigInt instance, object? value, string typeName)
	{
		if (value == null)
			return 1;
		if (TypeOf(value) != "bigint")
			throw new Error($"ArgumentException: Object must be of type {typeName}.");

		var other = (BigInt)value;
		return instance < other ? -1 : (instance > other ? 1 : 0);
	}

	internal static BigInt Parse(string? text, BigInt minValue, BigInt maxValue, string typeName)
	{
		if (text == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		var trimmed = text.Trim();
		if (trimmed.Length == 0 || !DecimalIntegerPattern.Test(trimmed))
			throw new Error($"FormatException: String '{text}' was not recognized as a valid {typeName}.");

		var value = BigIntFn(trimmed);
		if (value < minValue || value > maxValue)
			throw new Error($"OverflowException: Value '{text}' was either too large or too small for a {typeName}.");

		return value;
	}

	internal static Array<object?> TryParse(string? text, BigInt minValue, BigInt maxValue)
	{
		if (text == null)
			return [false, BigInt.Zero];

		var trimmed = text.Trim();
		if (trimmed.Length == 0 || !DecimalIntegerPattern.Test(trimmed))
			return [false, BigInt.Zero];

		var value = BigIntFn(trimmed);
		if (value < minValue || value > maxValue)
			return [false, BigInt.Zero];

		return [true, value];
	}

	internal static BigInt EnsureRange(BigInt value, BigInt minValue, BigInt maxValue)
	{
		if (value < minValue || value > maxValue)
			throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");

		return value;
	}

	internal static Number ToCheckedNumber(BigInt value, BigInt minValue, BigInt maxValue)
		=> NumberFn(EnsureRange(value, minValue, maxValue));

	internal static BigInt FromFloatingChecked(Number value, BigInt minValue, BigInt maxValue)
	{
		if (!DoubleModule.IsFiniteCore(value))
			throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");

		return EnsureRange(BigIntFn(Math.TruncFn(value)), minValue, maxValue);
	}

	internal static BigInt FromFloatingCheckedUInt128(Number value, BigInt maxValue)
	{
		// UInt128's checked floating conversion rejects every negative source, including -0.5.
		if (value < 0)
			throw new Error("OverflowException: Arithmetic operation resulted in an overflow.");

		return FromFloatingChecked(value, BigInt.Zero, maxValue);
	}

	internal static BigInt FromFloatingSaturatingSigned(Number value, BigInt minValue, BigInt maxValue)
	{
		if (IsNaN(value))
			return BigInt.Zero;
		if (!DoubleModule.IsFiniteCore(value))
			return value < 0 ? minValue : maxValue;

		var integer = BigIntFn(Math.TruncFn(value));
		return integer < minValue ? minValue : (integer > maxValue ? maxValue : integer);
	}

	internal static BigInt FromFloatingSaturatingUnsigned(Number value, BigInt maxValue)
	{
		if (IsNaN(value) || value <= 0)
			return BigInt.Zero;
		if (!DoubleModule.IsFiniteCore(value))
			return maxValue;

		var integer = BigIntFn(Math.TruncFn(value));
		return integer > maxValue ? maxValue : integer;
	}

	internal static string ToDecimal(BigInt value, BigInt minValue, BigInt maxValue)
		=> EnsureRange(value, minValue, maxValue).ToString() ?? "";

	internal static BigInt FromDecimal(string value, BigInt minValue, BigInt maxValue)
	{
		var integral = BigIntFn(DecimalModule._be8b149ea0e1d76b(value));
		return EnsureRange(integral, minValue, maxValue);
	}

	internal static (BigInt Quotient, BigInt Remainder) DivRemSigned(BigInt left, BigInt right, BigInt minValue)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");
		if (left == minValue && right == -BigInt.One)
			throw new Error("OverflowException");

		return (Quotient: left / right, Remainder: left % right);
	}

	internal static (BigInt Quotient, BigInt Remainder) DivRemUnsigned(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");

		return (Quotient: left / right, Remainder: left % right);
	}

	internal static BigInt DivideSigned(BigInt left, BigInt right, BigInt minValue)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");
		if (left == minValue && right == -BigInt.One)
			throw new Error("OverflowException");

		return left / right;
	}

	internal static BigInt DivideUnsigned(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");

		return left / right;
	}

	internal static BigInt RemainderSigned(BigInt left, BigInt right, BigInt minValue)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");
		// C# integral remainder shares division's MinValue / -1 overflow rule.
		if (left == minValue && right == -BigInt.One)
			throw new Error("OverflowException");

		return left % right;
	}

	internal static BigInt RemainderUnsigned(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");

		return left % right;
	}

	internal static BigInt AbsSigned(BigInt value, BigInt minValue)
	{
		if (value == minValue)
			throw new Error("OverflowException");

		return value < BigInt.Zero ? -value : value;
	}

	internal static BigInt CopySignSigned(BigInt value, BigInt sign, BigInt minValue)
	{
		if (value == minValue)
		{
			if (sign >= BigInt.Zero)
				throw new Error("OverflowException");

			return minValue;
		}

		var magnitude = value < BigInt.Zero ? -value : value;
		return sign < BigInt.Zero ? -magnitude : magnitude;
	}

	internal static BigInt Clamp(BigInt value, BigInt min, BigInt max)
	{
		if (min > max)
			throw new Error("ArgumentException: 'min' cannot be greater than max.");

		return value < min ? min : (value > max ? max : value);
	}

	internal static BigInt LeadingZeroCount(BigInt value, Number bitWidth, BigInt mask)
	{
		var normalized = value & mask;
		if (normalized == BigInt.Zero)
			return BigIntFn(bitWidth);

		var significantBits = BigInt.Zero;
		while (normalized > BigInt.Zero)
		{
			normalized = normalized >> BigInt.One;
			significantBits = significantBits + BigInt.One;
		}

		return BigIntFn(bitWidth) - significantBits;
	}

	internal static BigInt PopCount(BigInt value, BigInt mask)
	{
		var count = BigInt.Zero;
		var normalized = value & mask;
		while (normalized > BigInt.Zero)
		{
			count = count + (normalized & BigInt.One);
			normalized = normalized >> BigInt.One;
		}

		return count;
	}

	internal static BigInt RotateLeft(
		BigInt value,
		Number rotateAmount,
		Number bitWidth,
		BigInt mask,
		BigInt modulus,
		BigInt signBit,
		bool signed)
	{
		var amount = NormalizeRotateAmount(rotateAmount, bitWidth);
		var shift = BigIntFn(amount);
		var normalized = value & mask;
		if (shift != BigInt.Zero)
		{
			var width = BigIntFn(bitWidth);
			normalized = ((normalized << shift) | (normalized >> (width - shift))) & mask;
		}

		return RestoreSignedValue(normalized, modulus, signBit, signed);
	}

	internal static BigInt RotateRight(
		BigInt value,
		Number rotateAmount,
		Number bitWidth,
		BigInt mask,
		BigInt modulus,
		BigInt signBit,
		bool signed)
	{
		var amount = NormalizeRotateAmount(rotateAmount, bitWidth);
		var shift = BigIntFn(amount);
		var normalized = value & mask;
		if (shift != BigInt.Zero)
		{
			var width = BigIntFn(bitWidth);
			normalized = ((normalized >> shift) | (normalized << (width - shift))) & mask;
		}

		return RestoreSignedValue(normalized, modulus, signBit, signed);
	}

	internal static BigInt TrailingZeroCount(BigInt value, Number bitWidth, BigInt mask)
	{
		var normalized = value & mask;
		if (normalized == BigInt.Zero)
			return BigIntFn(bitWidth);

		var count = BigInt.Zero;
		while ((normalized & BigInt.One) == BigInt.Zero)
		{
			normalized = normalized >> BigInt.One;
			count = count + BigInt.One;
		}

		return count;
	}

	internal static BigInt Log2Signed(BigInt value)
	{
		if (value < BigInt.Zero)
			throw new Error("ArgumentOutOfRangeException: value must be non-negative.");

		if (value == BigInt.Zero)
			return BigInt.Zero;

		var result = -BigInt.One;
		while (value > BigInt.Zero)
		{
			value = value >> BigInt.One;
			result = result + BigInt.One;
		}

		return result;
	}

	internal static BigInt Log10(BigInt value)
	{
		if (value < BigInt.Zero)
			throw new Error("ArgumentOutOfRangeException: value must be non-negative.");

		return value == BigInt.Zero
			? BigInt.Zero
			: BigIntFn(value.ToString().Length - 1);
	}

	internal static Array<BigInt> BigMulSigned(BigInt left, BigInt right, Number bitWidth)
	{
		var product = left * right;
		var shift = BigIntFn(bitWidth);
		return [
			BigInt.AsIntN(bitWidth, product >> shift),
			BigInt.AsIntN(bitWidth, product)
		];
	}

	internal static Array<BigInt> BigMulUnsigned(BigInt left, BigInt right, Number bitWidth)
	{
		var product = left * right;
		var shift = BigIntFn(bitWidth);
		return [
			BigInt.AsUintN(bitWidth, product >> shift),
			BigInt.AsUintN(bitWidth, product)
		];
	}

	internal static BigInt MaxMagnitude(BigInt x, BigInt y)
	{
		var absX = x < BigInt.Zero ? -x : x;
		var absY = y < BigInt.Zero ? -y : y;
		if (absX > absY)
			return x;
		if (absX < absY)
			return y;

		return x > y ? x : y;
	}

	internal static BigInt MinMagnitude(BigInt x, BigInt y)
	{
		var absX = x < BigInt.Zero ? -x : x;
		var absY = y < BigInt.Zero ? -y : y;
		if (absX < absY)
			return x;
		if (absX > absY)
			return y;

		return x < y ? x : y;
	}

	private static Number NormalizeRotateAmount(Number rotateAmount, Number bitWidth)
	{
		var amount = rotateAmount % bitWidth;
		return amount < 0 ? amount + bitWidth : amount;
	}

	private static BigInt RestoreSignedValue(
		BigInt normalized,
		BigInt modulus,
		BigInt signBit,
		bool signed)
		=> signed && normalized >= signBit ? normalized - modulus : normalized;
}
