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

	internal static (BigInt Quotient, BigInt Remainder) DivRem(BigInt left, BigInt right)
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

	internal static BigInt Remainder(BigInt left, BigInt right)
	{
		if (right == BigInt.Zero)
			throw new Error("DivideByZeroException");

		return left % right;
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
