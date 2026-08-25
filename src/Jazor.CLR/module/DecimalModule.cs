namespace Jazor.CLR;

/// <summary>
/// 为 System.Decimal 提供基于字符串和 BigInt 的精确十进制定点运算映射。
/// </summary>
/// <remarks>
/// Decimal 的精度和舍入规则不能直接交给 JavaScript Number；本模块保留 unscaled value 和
/// scale，并在解析、格式化、转换和算术运算处显式处理范围。新增成员时不要直接 Alias 为 Number。
/// </remarks>
[ECMAScriptModule("System/DecimalModule.js")]
[Jazor(Op.Alias, "decimal","String")]
public static class DecimalModule
{
	private static Number MaxFractionDigits => 28;
	private static BigInt MaxDecimalUnscaled => BigIntValue("79228162514264337593543950335");
	private static BigInt Int64MinValue => BigIntValue("-9223372036854775808");
	private static BigInt Int64MaxValue => BigIntValue("9223372036854775807");
	private static BigInt UInt64MaxValue => BigIntValue("18446744073709551615");
	private static BigInt Int32MinValue => BigIntValue("-2147483648");
	private static BigInt Int32MaxValue => BigIntValue("2147483647");
	private static BigInt UInt32MaxValue => BigIntValue("4294967295");
	private static BigInt Int16MinValue => BigIntValue("-32768");
	private static BigInt Int16MaxValue => BigIntValue("32767");
	private static BigInt UInt16MaxValue => BigIntValue("65535");
	private static BigInt SByteMinValue => BigIntValue("-128");
	private static BigInt SByteMaxValue => BigIntValue("127");
	private static BigInt ByteMaxValue => BigIntValue("255");
	private static Number AllowLeadingWhiteStyle => NumberValue((int)System.Globalization.NumberStyles.AllowLeadingWhite);
	private static Number AllowTrailingWhiteStyle => NumberValue((int)System.Globalization.NumberStyles.AllowTrailingWhite);
	private static Number AllowLeadingSignStyle => NumberValue((int)System.Globalization.NumberStyles.AllowLeadingSign);
	private static Number AllowTrailingSignStyle => NumberValue((int)System.Globalization.NumberStyles.AllowTrailingSign);
	private static Number AllowParenthesesStyle => NumberValue((int)System.Globalization.NumberStyles.AllowParentheses);
	private static Number AllowDecimalPointStyle => NumberValue((int)System.Globalization.NumberStyles.AllowDecimalPoint);
	private static Number AllowThousandsStyle => NumberValue((int)System.Globalization.NumberStyles.AllowThousands);
	private static Number AllowExponentStyle => NumberValue((int)System.Globalization.NumberStyles.AllowExponent);
	private static Number AllowCurrencySymbolStyle => NumberValue((int)System.Globalization.NumberStyles.AllowCurrencySymbol);
	private static Number AllowHexSpecifierStyle => NumberValue((int)System.Globalization.NumberStyles.AllowHexSpecifier);
	private static Number AllowBinarySpecifierStyle => NumberValue((int)System.Globalization.NumberStyles.AllowBinarySpecifier);
	private static Number NumberStyleNumber => NumberValue((int)System.Globalization.NumberStyles.Number);
	private static Number DefaultFixedPrecision => 2;
	private static Number DefaultNumberPrecision => 2;

	private static Array<object?> CreateParts(BigInt unscaled, Number scale)
		=> [unscaled, scale];

	private static BigInt GetUnscaled(Array<object?> parts)
		=> (BigInt)parts[0]!;

	private static Number GetScale(Array<object?> parts)
		=> (Number)parts[1]!;

	private static Array<object?> CreateNumberSymbols(
		string groupSeparator,
		string decimalSeparator,
		Number primaryGroupSize,
		Number secondaryGroupSize)
		=> [groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize];

	private static string GetGroupSeparator(Array<object?> symbols)
		=> (string)symbols[0]!;

	private static string GetDecimalSeparator(Array<object?> symbols)
		=> (string)symbols[1]!;

	private static Number GetPrimaryGroupSize(Array<object?> symbols)
		=> (Number)symbols[2]!;

	private static Number GetSecondaryGroupSize(Array<object?> symbols)
		=> (Number)symbols[3]!;

	private static bool HasStyle(Number style, Number flag)
	{
		var integerStyle = (int)style;
		var integerFlag = (int)flag;
		return (integerStyle & integerFlag) == integerFlag;
	}

	// NumberStyles 在当前 lowering 中会擦除为数值字面量。
	private static Number GetNumberStylesValue(object style)
	{
		if (style is Number numberStyle)
			return numberStyle | 0;

		throw new Error("ArgumentException: Invalid NumberStyles value.");
	}

	private static void ValidateDecimalNumberStyles(Number style)
	{
		if (Math.FloorFunc(style) != style || style < 0)
			throw new Error("ArgumentException: An undefined NumberStyles value is not supported.");
		if (HasStyle(style, AllowHexSpecifierStyle) || HasStyle(style, AllowBinarySpecifierStyle))
			throw new Error("ArgumentException: The number style AllowHexSpecifier or AllowBinarySpecifier is not supported on floating point data types.");
	}

	private static Array<object?> GetNumberSymbols(object? provider)
	{
		if (provider is string locale)
		{
			if (locale.Length == 0)
				return CreateNumberSymbols(",", ".", 3, 3);

			return GetNumberSymbols(new Intl.NumberFormat(locale));
		}

		if (provider is Intl.NumberFormat numberFormat)
			return GetNumberSymbols(numberFormat);

		return GetNumberSymbols(new Intl.NumberFormat());
	}

	private static Array<object?> GetNumberSymbols(Intl.NumberFormat numberFormat)
	{
		var groupSeparator = ",";
		var decimalSeparator = ".";
		var integerParts = new Array<Number>();
		var parts = numberFormat.FormatToParts(123456789.1);
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i]!;
			if (part.Type == "group")
				groupSeparator = part.Value;
			else if (part.Type == "decimal")
				decimalSeparator = part.Value;
			else if (part.Type == "integer")
				integerParts.Push(part.Value.Length);
		}

		var primaryGroupSize = 3;
		var secondaryGroupSize = 3;
		if (integerParts.Length > 0)
		{
			primaryGroupSize = integerParts[integerParts.Length - 1]!;
			secondaryGroupSize = integerParts.Length > 1
				? integerParts[integerParts.Length - 2]!
				: primaryGroupSize;
		}

		return CreateNumberSymbols(groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize);
	}

	private static BigInt Pow10(Number exponent)
	{
		var result = BigIntValue(1);
		for (var i = 0; i < exponent; i++)
			result *= BigIntValue(10);

		return result;
	}

	private static Number MaxNumber(Number left, Number right)
		=> left >= right ? left : right;

	private static string RepeatZero(Number count)
	{
		var parts = new Array<string>();
		for (var i = 0; i < count; i++)
			parts.Push("0");

		return parts.Join("");
	}

	private static string StripLeadingZeros(string digits)
	{
		while (digits.Length > 1 && digits[0] == '0')
			digits = digits.Substring(1);

		return digits;
	}

	private static Array<object?> PreserveParts(BigInt unscaled, Number scale)
	{
		var absolute = unscaled < BigInt.Zero ? -unscaled : unscaled;
		while (scale > 0
			&& (scale > MaxFractionDigits || absolute > MaxDecimalUnscaled)
			&& unscaled % BigIntValue(10) == BigInt.Zero)
		{
			unscaled /= BigIntValue(10);
			scale--;
			absolute = unscaled < BigInt.Zero ? -unscaled : unscaled;
		}

		if (scale < 0 || scale > MaxFractionDigits)
			throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
		if (absolute > MaxDecimalUnscaled)
			throw new Error("OverflowException: Value was either too large or too small for a Decimal.");

		return CreateParts(unscaled, scale);
	}

	private static Array<object?> NormalizeParts(BigInt unscaled, Number scale)
	{
		var parts = PreserveParts(unscaled, scale);
		unscaled = GetUnscaled(parts);
		scale = GetScale(parts);
		while (scale > 0 && unscaled % BigIntValue(10) == BigInt.Zero)
		{
			unscaled /= BigIntValue(10);
			scale--;
		}

		return CreateParts(unscaled, scale);
	}

	private static Array<object?> ParseDecimal(string value, bool allowExponent = true)
	{
		var s = value.Trim();
		if (s.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid Decimal.");

		var negative = false;
		if (s[0] == '+' || s[0] == '-')
		{
			negative = s[0] == '-';
			s = s.Substring(1);
			if (s.Length == 0)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");
		}

		var exponent = 0;
		var exponentIndex = s.IndexOf('e');
		if (exponentIndex < 0)
			exponentIndex = s.IndexOf('E');
		if (exponentIndex >= 0)
		{
			if (!allowExponent)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");
			if (exponentIndex == 0 || exponentIndex == s.Length - 1)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

			var exponentText = s.Substring(exponentIndex + 1);
			var exponentValue = NumberValue(exponentText);
			// Reject non-finite/oversized exponents before RepeatZero can turn attacker input
			// into an unbounded string-building loop.
			if (IsNaN(exponentValue)
				|| !DoubleModule.IsFiniteCore(exponentValue)
				|| Math.FloorFunc(exponentValue) != exponentValue
				|| exponentValue < -100
				|| exponentValue > 100)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

			exponent = exponentValue;
			s = s.Substring(0, exponentIndex);
			if (s.Length == 0)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");
		}

		var dotIndex = s.IndexOf('.');
		if (dotIndex >= 0 && s.IndexOf('.', dotIndex + 1) >= 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		var integerDigits = dotIndex >= 0 ? s.Substring(0, dotIndex) : s;
		var fractionDigits = dotIndex >= 0 ? s.Substring(dotIndex + 1) : "";
		if (integerDigits.Length == 0 && fractionDigits.Length == 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");
		if (integerDigits.Length == 0)
			integerDigits = "0";

		var digits = integerDigits + fractionDigits;
		if (digits.Length == 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		for (var i = 0; i < digits.Length; i++)
		{
			var c = digits[i];
			if (c < '0' || c > '9')
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");
		}

		digits = StripLeadingZeros(digits);
		var scale = fractionDigits.Length - exponent;
		if (scale < 0)
		{
			digits += RepeatZero(-scale);
			scale = 0;
		}

		var unscaled = BigIntValue(digits);
		if (negative && unscaled != BigInt.Zero)
			unscaled = -unscaled;

		return PreserveParts(unscaled, scale);
	}

	private static string NormalizeDecimal(string value)
	{
		var parts = ParseDecimal(value);
		return FormatNormalizedDecimal(GetUnscaled(parts), GetScale(parts));
	}

	private static string RemoveAllOccurrences(string value, string token)
		=> token.Length == 0 ? value : value.Replace(token, "");

	private static string ReplaceDecimalSeparator(string value, string decimalSeparator)
	{
		if (decimalSeparator.Length == 0 || decimalSeparator == ".")
			return value;

		return value.Replace(decimalSeparator, ".");
	}

	private static string NormalizeExternalDecimalText(string value, Number style, object? provider)
	{
		ValidateDecimalNumberStyles(style);

		var text = value;
		var allowLeadingWhite = HasStyle(style, AllowLeadingWhiteStyle);
		var allowTrailingWhite = HasStyle(style, AllowTrailingWhiteStyle);
		if (allowLeadingWhite && allowTrailingWhite)
			text = text.Trim();
		else if (allowLeadingWhite)
			text = text.TrimStart();
		else if (allowTrailingWhite)
			text = text.TrimEnd();

		if (text.Length == 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		var negative = false;
		if (HasStyle(style, AllowParenthesesStyle) && text.Length >= 2 && text[0] == '(' && text[text.Length - 1] == ')')
		{
			negative = true;
			text = text.Substring(1, text.Length - 2);
			if (allowLeadingWhite && allowTrailingWhite)
				text = text.Trim();
			else if (allowLeadingWhite)
				text = text.TrimStart();
			else if (allowTrailingWhite)
				text = text.TrimEnd();
		}

		if (text.Length == 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		if (text[0] == '+' || text[0] == '-')
		{
			if (!HasStyle(style, AllowLeadingSignStyle))
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

			negative = text[0] == '-' ? !negative : negative;
			text = text.Substring(1);
		}

		if (text.Length == 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		if (text[text.Length - 1] == '+' || text[text.Length - 1] == '-')
		{
			if (!HasStyle(style, AllowTrailingSignStyle))
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

			negative = text[text.Length - 1] == '-' ? !negative : negative;
			text = text.Substring(0, text.Length - 1);
		}

		if (text.Length == 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		var symbols = GetNumberSymbols(provider);
		var groupSeparator = GetGroupSeparator(symbols);
		var decimalSeparator = GetDecimalSeparator(symbols);
		if (groupSeparator.Length != 0)
		{
			if (HasStyle(style, AllowThousandsStyle))
				text = RemoveAllOccurrences(text, groupSeparator);
			else if (text.IndexOf(groupSeparator) >= 0)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");
		}

		if (decimalSeparator != ".")
		{
			if (text.IndexOf(decimalSeparator) >= 0)
			{
				if (!HasStyle(style, AllowDecimalPointStyle))
					throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

				text = ReplaceDecimalSeparator(text, decimalSeparator);
			}
		}

		if (!HasStyle(style, AllowDecimalPointStyle) && text.IndexOf('.') >= 0)
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

		if (negative)
			text = "-" + text;

		return text;
	}

	private static string ParseDecimalExternal(string? value, Number style, object? provider)
	{
		if (value == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		var normalized = NormalizeExternalDecimalText(value, style, provider);
		var parts = ParseDecimal(normalized, HasStyle(style, AllowExponentStyle));
		return FormatDecimal(GetUnscaled(parts), GetScale(parts));
	}

	private static string FormatDecimal(BigInt unscaled, Number scale)
	{
		var preserved = PreserveParts(unscaled, scale);
		unscaled = GetUnscaled(preserved);
		scale = GetScale(preserved);

		var negative = unscaled < BigInt.Zero;
		var absolute = negative ? -unscaled : unscaled;
		var digits = absolute.ToString()!;
		if (scale == 0)
			return negative ? "-" + digits : digits;

		if (digits.Length <= scale)
			digits = RepeatZero(scale - digits.Length + 1) + digits;

		var split = digits.Length - scale;
		var text = digits.Substring(0, split) + "." + digits.Substring(split);
		return negative ? "-" + text : text;
	}

	private static string FormatNormalizedDecimal(BigInt unscaled, Number scale)
	{
		var normalized = NormalizeParts(unscaled, scale);
		return FormatDecimal(GetUnscaled(normalized), GetScale(normalized));
	}

	private static string PreserveDecimal(string value)
	{
		var parts = ParseDecimal(value);
		return FormatDecimal(GetUnscaled(parts), GetScale(parts));
	}

	private static string FormatDecimalToScale(string value, Number scale)
	{
		var parts = ParseDecimal(value);
		var unscaled = GetUnscaled(parts);
		var sourceScale = GetScale(parts);
		if (sourceScale < scale)
			unscaled *= Pow10(scale - sourceScale);
		else if (sourceScale > scale)
			unscaled /= Pow10(sourceScale - scale);

		var negative = unscaled < BigInt.Zero;
		var absolute = negative ? -unscaled : unscaled;
		var digits = absolute.ToString()!;
		if (scale == 0)
			return negative ? "-" + digits : digits;

		if (digits.Length <= scale)
			digits = RepeatZero(scale - digits.Length + 1) + digits;

		var split = digits.Length - scale;
		var text = digits.Substring(0, split) + "." + digits.Substring(split);
		return negative ? "-" + text : text;
	}

	private static string InsertGroupSeparators(
		string integerDigits,
		string separator,
		Number primaryGroupSize,
		Number secondaryGroupSize)
	{
		if (separator.Length == 0 || integerDigits.Length <= primaryGroupSize)
			return integerDigits;

		var groups = new Array<string>();
		var end = integerDigits.Length;
		var size = primaryGroupSize;
		while (end > size)
		{
			groups.Push(integerDigits.Substring(end - size, size));
			end -= size;
			size = secondaryGroupSize;
		}

		var result = integerDigits.Substring(0, end);
		for (var i = groups.Length - 1; i >= 0; i--)
			result += separator + groups[i]!;

		return result;
	}

	private static string ApplyNumberSeparators(string value, object? provider)
	{
		var symbols = GetNumberSymbols(provider);
		var groupSeparator = GetGroupSeparator(symbols);
		var decimalSeparator = GetDecimalSeparator(symbols);
		var primaryGroupSize = GetPrimaryGroupSize(symbols);
		var secondaryGroupSize = GetSecondaryGroupSize(symbols);
		var sign = "";
		var digits = value;
		if (digits[0] == '-')
		{
			sign = "-";
			digits = digits.Substring(1);
		}

		var dotIndex = digits.IndexOf('.');
		var integerDigits = dotIndex >= 0 ? digits.Substring(0, dotIndex) : digits;
		var fractionDigits = dotIndex >= 0 ? digits.Substring(dotIndex + 1) : "";
		var groupedInteger = InsertGroupSeparators(integerDigits, groupSeparator, primaryGroupSize, secondaryGroupSize);
		if (fractionDigits.Length == 0)
			return sign + groupedInteger;

		return sign + groupedInteger + decimalSeparator + fractionDigits;
	}

	private static Number ParsePrecision(string format, Number defaultValue)
	{
		if (format.Length == 1)
			return defaultValue;

		var precisionText = format.Substring(1);
		for (var i = 0; i < precisionText.Length; i++)
		{
			var c = precisionText[i];
			if (c < '0' || c > '9')
				throw new Error("FormatException: Format specifier was invalid.");
		}

		return NumberValue(precisionText);
	}

	private static bool IsSimpleCustomDecimalFormat(string format)
	{
		for (var i = 0; i < format.Length; i++)
		{
			var c = format[i];
			if (c != '0' && c != '#' && c != '.' && c != ',')
				return false;
		}

		return true;
	}

	private static string FormatDecimalWithFormat(string value, string? format, object? provider)
	{
		if (format == null || format.Length == 0)
			return PreserveDecimal(value);

		var specifier = format[0];
		if ((specifier == 'G' || specifier == 'g') && format.Length == 1)
			return PreserveDecimal(value);
		if (specifier == 'F' || specifier == 'f')
		{
			var precision = ParsePrecision(format, DefaultFixedPrecision);
			return FormatDecimalToScale(RoundDecimal(value, precision), precision);
		}
		if (specifier == 'N' || specifier == 'n')
		{
			var precision = ParsePrecision(format, DefaultNumberPrecision);
			return ApplyNumberSeparators(
				FormatDecimalToScale(RoundDecimal(value, precision), precision),
				provider);
		}
		if (IsSimpleCustomDecimalFormat(format))
		{
			var dotIndex = format.IndexOf('.');
			var scale = dotIndex < 0 ? 0 : format.Length - dotIndex - 1;
			var formatted = FormatDecimalToScale(RoundDecimal(value, scale), scale);
			return format.IndexOf(',') >= 0 ? ApplyNumberSeparators(formatted, provider) : formatted;
		}

		throw new Error("FormatException: Format specifier was invalid.");
	}

	private static string CreateDecimalFromNumber(Number value)
	{
		if (!DoubleModule.IsFiniteCore(value))
			throw new Error("OverflowException: Value was either too large or too small for a Decimal.");

		return NormalizeDecimal(value.ToString());
	}

	private static BigInt AlignUnscaled(Array<object?> value, Number targetScale)
	{
		var scale = GetScale(value);
		if (targetScale <= scale)
			return GetUnscaled(value);

		return GetUnscaled(value) * Pow10(targetScale - scale);
	}

	private static BigInt TruncateToIntegralValue(string value)
	{
		var parts = ParseDecimal(value);
		var scale = GetScale(parts);
		if (scale == 0)
			return GetUnscaled(parts);

		return GetUnscaled(parts) / Pow10(scale);
	}

	private static Number ToCheckedNumber(string value, BigInt min, BigInt max, string typeName)
	{
		var integral = TruncateToIntegralValue(value);
		if (integral < min || integral > max)
			throw new Error($"OverflowException: Value was either too large or too small for a {typeName}.");

		return NumberValue(integral);
	}

	private static BigInt ToCheckedBigInt(string value, BigInt min, BigInt max, string typeName)
	{
		var integral = TruncateToIntegralValue(value);
		if (integral < min || integral > max)
			throw new Error($"OverflowException: Value was either too large or too small for a {typeName}.");

		return integral;
	}

	private static string FromOACurrencyCore(BigInt currency)
	{
		var scale = 4;
		while (currency != BigInt.Zero && scale > 0 && currency % BigIntValue(10) == BigInt.Zero)
		{
			currency /= BigIntValue(10);
			scale--;
		}

		// CLR keeps four fractional digits for zero, but removes redundant trailing zeros
		// from every non-zero OA Currency value.
		return FormatDecimal(currency, scale);
	}

	private static BigInt ToOACurrencyCore(string value)
	{
		var parts = ParseDecimal(value);
		var unscaled = GetUnscaled(parts);
		var scale = GetScale(parts);
		BigInt currency;
		if (scale <= 4)
			currency = unscaled * Pow10(4 - scale);
		else
			currency = DivideAndRound(unscaled, Pow10(scale - 4));

		if (currency < Int64MinValue || currency > Int64MaxValue)
			throw new Error("OverflowException: Value was either too large or too small for a Currency.");

		return currency;
	}

	// MidpointRounding 在当前 lowering 中会擦除为数值字面量。
	private static Number GetMidpointRoundingValue(object mode)
	{
		if (mode is Number numberMode)
			return numberMode;

		throw new Error("ArgumentException: Invalid MidpointRounding value.");
	}

	private static string RoundDecimal(string value, Number decimals, object? mode = null)
	{
		if (Math.FloorFunc(decimals) != decimals || decimals < 0 || decimals > MaxFractionDigits)
			throw new Error("ArgumentOutOfRangeException: Decimal digits must be between 0 and 28.");

		var modeValue = mode == null ? NumberValue(0) : GetMidpointRoundingValue(mode);
		if (modeValue < 0 || modeValue > 4 || Math.FloorFunc(modeValue) != modeValue)
			throw new Error("ArgumentException: Invalid MidpointRounding value.");

		var parts = ParseDecimal(value);
		var scale = GetScale(parts);
		var unscaled = GetUnscaled(parts);
		if (scale <= decimals)
			return FormatDecimal(unscaled, scale);

		var trimScale = scale - decimals;
		var divisor = Pow10(trimScale);
		var quotient = unscaled / divisor;
		var remainder = unscaled % divisor;
		if (remainder == BigInt.Zero)
			return FormatDecimal(quotient, decimals);

		var negative = unscaled < BigInt.Zero;
		if (modeValue == 2)
			return FormatDecimal(quotient, decimals);
		if (modeValue == 3)
			return FormatDecimal(negative ? quotient - BigIntValue(1) : quotient, decimals);
		if (modeValue == 4)
			return FormatDecimal(negative ? quotient : quotient + BigIntValue(1), decimals);

		var absoluteRemainder = negative ? -remainder : remainder;
		var comparison = absoluteRemainder * BigIntValue(2) - divisor;
		if (comparison < BigInt.Zero)
			return FormatDecimal(quotient, decimals);

		var step = negative ? -BigIntValue(1) : BigIntValue(1);
		if (comparison > BigInt.Zero || modeValue == 1)
			return FormatDecimal(quotient + step, decimals);

		return quotient % BigIntValue(2) == BigInt.Zero
			? FormatDecimal(quotient, decimals)
			: FormatDecimal(quotient + step, decimals);
	}

	private static Number CompareDecimal(string left, string right)
	{
		var a = ParseDecimal(left);
		var b = ParseDecimal(right);
		var targetScale = MaxNumber(GetScale(a), GetScale(b));
		var leftValue = AlignUnscaled(a, targetScale);
		var rightValue = AlignUnscaled(b, targetScale);
		if (leftValue < rightValue)
			return -1;
		if (leftValue > rightValue)
			return 1;
		return 0;
	}

	private static string AddDecimal(string left, string right)
	{
		var a = ParseDecimal(left);
		var b = ParseDecimal(right);
		var targetScale = MaxNumber(GetScale(a), GetScale(b));
		return FormatDecimal(AlignUnscaled(a, targetScale) + AlignUnscaled(b, targetScale), targetScale);
	}

	private static string SubtractDecimal(string left, string right)
		=> AddDecimal(left, NegateDecimal(right));

	private static string NegateDecimal(string value)
	{
		var parts = ParseDecimal(value);
		return FormatDecimal(-GetUnscaled(parts), GetScale(parts));
	}

	private static string MultiplyDecimal(string left, string right)
	{
		var a = ParseDecimal(left);
		var b = ParseDecimal(right);
		return FormatDecimal(GetUnscaled(a) * GetUnscaled(b), GetScale(a) + GetScale(b));
	}

	private static BigInt DivideAndRound(BigInt numerator, BigInt denominator)
	{
		var quotient = numerator / denominator;
		var remainder = numerator % denominator;
		if (remainder == BigInt.Zero)
			return quotient;

		var absoluteRemainder = remainder < BigInt.Zero ? -remainder : remainder;
		var absoluteDenominator = denominator < BigInt.Zero ? -denominator : denominator;
		var comparison = absoluteRemainder * BigIntValue(2) - absoluteDenominator;
		if (comparison < BigInt.Zero)
			return quotient;

		var step = numerator < BigInt.Zero ? -BigIntValue(1) : BigIntValue(1);
		if (comparison > BigInt.Zero)
			return quotient + step;

		return quotient % BigIntValue(2) == BigInt.Zero ? quotient : quotient + step;
	}

	private static string DivideDecimal(string left, string right)
	{
		var a = ParseDecimal(left);
		var b = ParseDecimal(right);
		if (GetUnscaled(b) == BigInt.Zero)
			throw new Error("DivideByZeroException: Attempted to divide by zero.");

		var scaleDelta = MaxFractionDigits + GetScale(b) - GetScale(a);
		var numerator = GetUnscaled(a);
		var denominator = GetUnscaled(b);
		if (scaleDelta >= 0)
			numerator *= Pow10(scaleDelta);
		else
			denominator *= Pow10(-scaleDelta);

		return FormatNormalizedDecimal(DivideAndRound(numerator, denominator), MaxFractionDigits);
	}

	private static string RemainderDecimal(string left, string right)
	{
		var a = ParseDecimal(left);
		var b = ParseDecimal(right);
		if (GetUnscaled(b) == BigInt.Zero)
			throw new Error("DivideByZeroException: Attempted to divide by zero.");

		var targetScale = MaxNumber(GetScale(a), GetScale(b));
		return FormatDecimal(AlignUnscaled(a, targetScale) % AlignUnscaled(b, targetScale), targetScale);
	}

	private static string FloorDecimal(string value)
	{
		var parts = ParseDecimal(value);
		var scale = GetScale(parts);
		var unscaled = GetUnscaled(parts);
		if (scale == 0)
			return FormatDecimal(unscaled, 0);

		var divisor = Pow10(scale);
		var quotient = unscaled / divisor;
		var remainder = unscaled % divisor;
		if (remainder != BigInt.Zero && unscaled < BigInt.Zero)
			quotient -= BigIntValue(1);

		return FormatDecimal(quotient, 0);
	}

	private static string CeilingDecimal(string value)
	{
		var parts = ParseDecimal(value);
		var scale = GetScale(parts);
		var unscaled = GetUnscaled(parts);
		if (scale == 0)
			return FormatDecimal(unscaled, 0);

		var divisor = Pow10(scale);
		var quotient = unscaled / divisor;
		var remainder = unscaled % divisor;
		if (remainder != BigInt.Zero && unscaled > BigInt.Zero)
			quotient += BigIntValue(1);

		return FormatDecimal(quotient, 0);
	}

	private static string TruncateDecimal(string value)
	{
		var parts = ParseDecimal(value);
		var scale = GetScale(parts);
		var unscaled = GetUnscaled(parts);
		if (scale == 0)
			return FormatDecimal(unscaled, 0);

		return FormatDecimal(unscaled / Pow10(scale), 0);
	}

	private static string AbsDecimal(string value)
	{
		var parts = ParseDecimal(value);
		var unscaled = GetUnscaled(parts);
		return FormatDecimal(unscaled < BigInt.Zero ? -unscaled : unscaled, GetScale(parts));
	}

	private static Number SignDecimal(string value)
	{
		var parts = ParseDecimal(value);
		var unscaled = GetUnscaled(parts);
		if (unscaled < BigInt.Zero)
			return -1;
		if (unscaled > BigInt.Zero)
			return 1;
		return 0;
	}

	private static bool IsIntegerDecimal(string value)
	{
		var parts = ParseDecimal(value);
		var scale = GetScale(parts);
		return scale == 0 || GetUnscaled(parts) % Pow10(scale) == BigInt.Zero;
	}

	//decimal.Zero = 0;

	//decimal.One = 1;

	//decimal.MinusOne = -1;

	//decimal.MaxValue = 79228162514264337593543950335;

	//decimal.MinValue = -79228162514264337593543950335;

	[Jazor(Op.Import, "static readonly decimal.Zero")]
	public static string _5faf9ddf65d02495()
		=> "0";

	[Jazor(Op.Import, "static readonly decimal.One")]
	public static string _3db06a98834e6ef8()
		=> "1";

	[Jazor(Op.Import, "static readonly decimal.MinusOne")]
	public static string _9311127a9ca2b91d()
		=> "-1";

	[Jazor(Op.Import, "static readonly decimal.MaxValue")]
	public static string _6a4e5f697d4fc607()
		=> "79228162514264337593543950335";

	[Jazor(Op.Import, "static readonly decimal.MinValue")]
	public static string _cc6392a7d6df1e14()
		=> "-79228162514264337593543950335";

	/// <summary>
	/// C#: new decimal()
	/// JS: "0" (decimal as string for precision)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal()", "'0'")]
	public extern static string _a7246904c5449b5f();

	/// <summary>
	/// C#: new decimal(int)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(int)", "String(__arg1)")]
	public extern static string _9c4dd6829012e347(Number value);

	/// <summary>
	/// C#: new decimal(uint)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(uint)", "String(__arg1)")]
	public extern static string _73a058b17ed5de01(Number value);

	/// <summary>
	/// C#: new decimal(long)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(long)", "String(__arg1)")]
	public extern static string _188ee93a8a80b7f4(BigInt value);

	/// <summary>
	/// C#: new decimal(ulong)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(ulong)", "String(__arg1)")]
	public extern static string _9a3a0f6f89e1e594(BigInt value);

	/// <summary>
	/// C#: new decimal(float)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Import, "decimal.Decimal(float)")]
	public static string _2f7f0d9035a4bbf6(Number value)
		=> CreateDecimalFromNumber(value);

	/// <summary>
	/// C#: new decimal(double)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Import, "decimal.Decimal(double)")]
	public static string _cb7c7a937d3b8460(Number value)
		=> CreateDecimalFromNumber(value);

	///<summary>Converts the specified 64-bit signed integer, which contains an OLE Automation Currency value, to the equivalent <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Import ,"static decimal.FromOACurrency(long)")]
	public static string _6cd0f8dfbedd7209(BigInt cy)
		=> FromOACurrencyCore(cy);

	///<summary>Converts the specified <see cref="T:System.Decimal" /> value to the equivalent OLE Automation Currency value, which is contained in a 64-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToOACurrency(decimal)")]
	public static BigInt _5d257b5cc33cdaeb(string value)
		=> ToOACurrencyCore(value);

	///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in a specified array.</summary>
	[Jazor(Op.Discard ,"decimal.Decimal(int[])")]
	public extern static string _1189e4d3b4884066(object bits);

	///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in the specified span.</summary>
	[Jazor(Op.Discard ,"decimal.Decimal(System.ReadOnlySpan<int>)")]
	public extern static string _e195522f8f6783c0(object bits);

	///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> from parameters specifying the instance's constituent parts.</summary>
	[Jazor(Op.Discard ,"decimal.Decimal(int, int, int, bool, byte)")]
	public extern static string _030063a806322293(Number lo, Number mid, Number hi, bool isNegative, Number scale);

	[Jazor(Op.Import ,"decimal.Scale.get")]
	public static Number _db7e7c8def75fee8(string instance)
		=> GetScale(ParseDecimal(instance));

	/// <summary>
	/// C#: decimal.Add(d1, d2)
	/// JS: String(Number(d1) + Number(d2))
	/// </summary>
	[Jazor(Op.Import, "static decimal.Add(decimal, decimal)")]
	public static string _f73258f14e05c790(string d1, string d2)
		=> AddDecimal(d1, d2);

	/// <summary>
	/// C#: decimal.Ceiling(d)
	/// JS: String(Math.ceil(Number(d)))
	/// </summary>
	[Jazor(Op.Import, "static decimal.Ceiling(decimal)")]
	public static string _84028a6e79626057(string d)
		=> CeilingDecimal(d);

	/// <summary>
	/// C#: decimal.Compare(d1, d2)
	/// JS: Number(d1) < Number(d2) ? -1 : (Number(d1) > Number(d2) ? 1 : 0)
	/// </summary>
	[Jazor(Op.Import, "static decimal.Compare(decimal, decimal)")]
	public static Number _c11e0aef6b5ccf1e(string d1, string d2)
		=> CompareDecimal(d1, d2);

	///<summary>Compares this instance to a specified object and returns a comparison of their relative values.</summary>
	[Jazor(Op.Import ,"decimal.CompareTo(object)")]
	public static Number _ff0e77ab6566e092(string instance, object? value)
	{
		if (value == null)
			return 1;

		if (value is not string other)
			throw new Error("ArgumentException: Object must be of type Decimal.");

		return _ca8a78810233056c(instance, other);
	}

	/// <summary>
	/// C#: instance.CompareTo(value)
	/// JS: Number(instance) < Number(value) ? -1 : (Number(instance) > Number(value) ? 1 : 0)
	/// </summary>
	[Jazor(Op.Import, "decimal.CompareTo(decimal)")]
	public static Number _ca8a78810233056c(string instance, string value)
		=> CompareDecimal(instance, value);

	/// <summary>
	/// C#: decimal.Divide(d1, d2)
	/// JS: String(Number(d1) / Number(d2))
	/// </summary>
	[Jazor(Op.Import, "static decimal.Divide(decimal, decimal)")]
	public static string _f5c1c0a2a040b000(string d1, string d2)
		=> DivideDecimal(d1, d2);

	/// <summary>
	/// C#: instance.Equals(value)
	/// JS: Number(instance) === Number(value)
	/// </summary>
	[Jazor(Op.Import, "override decimal.Equals(object)")]
	public static bool _8abe47785e51f122(string instance, object? value)
	{
		return value is string other && CompareDecimal(instance, other) == 0;
	}

	/// <summary>
	/// C#: instance.Equals(value)
	/// JS: Number(instance) === Number(value)
	/// </summary>
	[Jazor(Op.Import, "decimal.Equals(decimal)")]
	public static bool _3dfd87d9d2f35e11(string instance, string value)
		=> CompareDecimal(instance, value) == 0;

	/// <summary>
	/// C#: instance.GetHashCode()
	/// JS: Number(instance) | 0 (convert to int32)
	/// </summary>
	[Jazor(Op.Import, "override decimal.GetHashCode()")]
	public static Number _f58659c33299d2b1(string instance)
		=> RuntimeModule.GetStringHashCode(NormalizeDecimal(instance), 0);

	/// <summary>
	/// C#: decimal.Equals(d1, d2)
	/// JS: Number(d1) === Number(d2)
	/// </summary>
	[Jazor(Op.Import, "static decimal.Equals(decimal, decimal)")]
	public static bool _b25c4446c28ed255(string d1, string d2)
		=> CompareDecimal(d1, d2) == 0;

	/// <summary>
	/// C#: decimal.Floor(d)
	/// JS: String(Math.floor(Number(d)))
	/// </summary>
	[Jazor(Op.Import, "static decimal.Floor(decimal)")]
	public static string _518facaaeeb29ead(string d)
		=> FloorDecimal(d);

	/// <summary>
	/// C#: instance.ToString()
	/// JS: instance
	/// </summary>
	[Jazor(Op.Import, "override decimal.ToString()")]
	public static string _65a0e4fe8ccdd829(string instance)
		=> PreserveDecimal(instance);

	/// <summary>
	/// C#: instance.ToString(format)
	/// JS: Number(instance).toFixed(Number(format.replace(/[^0-9]/g, '')) || 0)
	/// </summary>
	[Jazor(Op.Import, "decimal.ToString(string)")]
	public static string _af32d07083f1da07(string instance, string? format)
		=> FormatDecimalWithFormat(instance, format, null);

	/// <summary>
	/// C#: instance.ToString(provider)
	/// JS: instance
	/// </summary>
	[Jazor(Op.Import, "decimal.ToString(System.IFormatProvider)")]
	public static string _6234ba988b3e006d(string instance, Intl.NumberFormat? provider)
		=> FormatDecimalWithFormat(instance, null, provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Import ,"decimal.ToString(string, System.IFormatProvider)")]
	public static string _b1e6a06111674f0c(string instance, string? format, Intl.NumberFormat? provider)
		=> FormatDecimalWithFormat(instance, format, provider);

	///<summary>Tries to format the value of the current decimal instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"decimal.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _919259e7087cfd17(string instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"decimal.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _c5d11df37776e790(string instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string)")]
	public static string _91a2436283a24315(string s)
		=> ParseDecimalExternal(s, NumberStyleNumber, null);

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string, System.Globalization.NumberStyles)")]
	public static string _79a0e8ede29256cc(string s, object style)
		=> ParseDecimalExternal(s, GetNumberStylesValue(style), null);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string, System.IFormatProvider)")]
	public static string _01be2a34fe2cda4e(string s, Intl.NumberFormat? provider)
		=> ParseDecimalExternal(s, NumberStyleNumber, provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public static string _f525a420b2d600ec(string s, object style, Intl.NumberFormat? provider)
		=> ParseDecimalExternal(s, GetNumberStylesValue(style), provider);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public static string _8e0c949ee2411c7f(string s, object style, Intl.NumberFormat? provider)
		=> ParseDecimalExternal(s, GetNumberStylesValue(style), provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(string, out decimal)")]
	public static Array<object?> _e96278809bb50e35(string? s, string result)
	{
		if (s == null || s.Length == 0)
			return [false, "0"];

		try
		{
			return [true, ParseDecimalExternal(s, NumberStyleNumber, null)];
		}
		catch
		{
			return [false, "0"];
		}
	}

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)")]
	public static Array<object?> _5f6432cf52162431(string s, string result)
		=> _e96278809bb50e35(s, result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its signed decimal equivalent.</summary>
	[Jazor(Op.Import, "static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)")]
	public static Array<object?> _0111d7c27998205b(Uint8Array utf8Text, string result)
		=> _e96278809bb50e35(RuntimeModule.TryDecodeUtf8(utf8Text), result);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public static Array<object?> _b4ecd2424c9a371e(string? s, object style, Intl.NumberFormat? provider, string result)
	{
		var styleValue = GetNumberStylesValue(style);
		ValidateDecimalNumberStyles(styleValue);
		if (s == null || s.Length == 0)
			return [false, "0"];

		try
		{
			return [true, ParseDecimalExternal(s, styleValue, provider)];
		}
		catch
		{
			return [false, "0"];
		}
	}

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public static Array<object?> _ed6b24306e2ef5cd(string s, object style, Intl.NumberFormat? provider, string result)
		=> _b4ecd2424c9a371e(s, style, provider, result);

	///<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	[Jazor(Op.Discard ,"static decimal.GetBits(decimal)")]
	public extern static int[] _e0536acf9668ef57(string d);

	///<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	[Jazor(Op.Discard ,"static decimal.GetBits(decimal, System.Span<int>)")]
	public extern static Number _9d53437d519e15cb(string d, object destination);

	///<summary>Tries to convert the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	[Jazor(Op.Discard ,"static decimal.TryGetBits(decimal, System.Span<int>, out int)")]
	public extern static Array<object?> _db7a1f9648d8e6eb(string d, object destination, Number valuesWritten);

	///<summary>Computes the remainder after dividing two <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Import ,"static decimal.Remainder(decimal, decimal)")]
	public static string _700359e0de148ee3(string d1, string d2)
		=> RemainderDecimal(d1, d2);

	///<summary>Multiplies two specified <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Import ,"static decimal.Multiply(decimal, decimal)")]
	public static string _d5be5da3d4effe96(string d1, string d2)
		=> MultiplyDecimal(d1, d2);

	///<summary>Returns the result of multiplying the specified <see cref="T:System.Decimal" /> value by negative one.</summary>
	[Jazor(Op.Import ,"static decimal.Negate(decimal)")]
	public static string _26945a698afa2a91(string d)
		=> NegateDecimal(d);

	///<summary>Rounds a decimal value to the nearest integer.</summary>
	[Jazor(Op.Import ,"static decimal.Round(decimal)")]
	public static string _4a816369b59f1ca3(string d)
		=> RoundDecimal(d, 0);

	///<summary>Rounds a <see cref="T:System.Decimal" /> value to a specified number of decimal places.</summary>
	[Jazor(Op.Import ,"static decimal.Round(decimal, int)")]
	public static string _bc3a974d51c694ab(string d, Number decimals)
		=> RoundDecimal(d, decimals);

	///<summary>Rounds a decimal value to an integer using the specified rounding strategy.</summary>
	[Jazor(Op.Import ,"static decimal.Round(decimal, System.MidpointRounding)")]
	public static string _a334f7e82122cfc2(string d, object mode)
		=> RoundDecimal(d, 0, mode);

	///<summary>Rounds a decimal value to the specified precision using the specified rounding strategy.</summary>
	[Jazor(Op.Import ,"static decimal.Round(decimal, int, System.MidpointRounding)")]
	public static string _09ee3a4652dbe73c(string d, Number decimals, object mode)
		=> RoundDecimal(d, decimals, mode);

	///<summary>Subtracts a specified <see cref="T:System.Decimal" /> value from another.</summary>
	[Jazor(Op.Import ,"static decimal.Subtract(decimal, decimal)")]
	public static string _3e80f2d9cf753d05(string d1, string d2)
		=> SubtractDecimal(d1, d2);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit unsigned integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToByte(decimal)")]
	public static Number _d2aabede7e0207c1(string value)
		=> ToCheckedNumber(value, BigInt.Zero, ByteMaxValue, "Byte");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToSByte(decimal)")]
	public static Number _175bf5ee849fcf8f(string value)
		=> ToCheckedNumber(value, SByteMinValue, SByteMaxValue, "SByte");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToInt16(decimal)")]
	public static Number _5df8c6a064c50c5f(string value)
		=> ToCheckedNumber(value, Int16MinValue, Int16MaxValue, "Int16");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent double-precision floating-point number.</summary>
	[Jazor(Op.Import ,"static decimal.ToDouble(decimal)")]
	public static Number _cfbbd251b43c99f4(string d)
		=> NumberValue(NormalizeDecimal(d));

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToInt32(decimal)")]
	public static Number _ad71e0d1a8679244(string d)
		=> ToCheckedNumber(d, Int32MinValue, Int32MaxValue, "Int32");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToInt64(decimal)")]
	public static BigInt _7a077e2e1baba462(string d)
		=> ToCheckedBigInt(d, Int64MinValue, Int64MaxValue, "Int64");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit unsigned integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToUInt16(decimal)")]
	public static Number _21bc553743dd324b(string value)
		=> ToCheckedNumber(value, BigInt.Zero, UInt16MaxValue, "UInt16");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit unsigned integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToUInt32(decimal)")]
	public static Number _c975b2e5b2f4c009(string d)
		=> ToCheckedNumber(d, BigInt.Zero, UInt32MaxValue, "UInt32");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit unsigned integer.</summary>
	[Jazor(Op.Import ,"static decimal.ToUInt64(decimal)")]
	public static BigInt _9b15def492d41a4a(string d)
		=> ToCheckedBigInt(d, BigInt.Zero, UInt64MaxValue, "UInt64");

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent single-precision floating-point number.</summary>
	[Jazor(Op.Import ,"static decimal.ToSingle(decimal)")]
	public static Number _1450e4ab34b1a945(string d)
		=> NumberValue(NormalizeDecimal(d));

	///<summary>Returns the integral digits of the specified <see cref="T:System.Decimal" />; any fractional digits are discarded.</summary>
	[Jazor(Op.Import ,"static decimal.Truncate(decimal)")]
	public static string _be8b149ea0e1d76b(string d)
		=> TruncateDecimal(d);

	///<summary>Defines an implicit conversion of an 8-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(byte)")]
	public static string _c605c67b2cd1973c(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(sbyte)")]
	public static string _e8d5240b7aa52784(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a 16-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(short)")]
	public static string _8635fe57a74e1249(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(ushort)")]
	public static string _7c3cfa0de18bd43c(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a Unicode character to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(char)")]
	public static string _d4af042bf014fd51(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a 32-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(int)")]
	public static string _f5a5d600ccd38777(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(uint)")]
	public static string _d8b659cd861d2409(Number value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a 64-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(long)")]
	public static string _23103e069358ca06(BigInt value)
		=> value.ToString() ?? "";

	///<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.implicit operator decimal(ulong)")]
	public static string _7ab8c627f74cb718(BigInt value)
		=> value.ToString() ?? "";

	///<summary>Defines an explicit conversion of a single-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator decimal(float)")]
	public static string _f456cac2ae523add(Number value)
		=> CreateDecimalFromNumber(value);

	///<summary>Defines an explicit conversion of a double-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator decimal(double)")]
	public static string _8f3a66f6dc828dff(Number value)
		=> CreateDecimalFromNumber(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit unsigned integer.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator byte(decimal)")]
	public static Number _a8bfc1feb93c39cb(string value)
		=> _d2aabede7e0207c1(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit signed integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator sbyte(decimal)")]
	public static Number _824c1dbd3e6691ba(string value)
		=> _175bf5ee849fcf8f(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a Unicode character.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator char(decimal)")]
	public static Number _e2c93b47df7960a8(string value)
		=> ToCheckedNumber(value, BigInt.Zero, UInt16MaxValue, "Char");

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator short(decimal)")]
	public static Number _8f4ca64a21fb08cc(string value)
		=> _5df8c6a064c50c5f(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator ushort(decimal)")]
	public static Number _3e209c4283c6e05e(string value)
		=> _21bc553743dd324b(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator int(decimal)")]
	public static Number _bc03e302b86b6800(string value)
		=> _ad71e0d1a8679244(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator uint(decimal)")]
	public static Number _dea1c1c9c8f2b495(string value)
		=> _c975b2e5b2f4c009(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit signed integer.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator long(decimal)")]
	public static BigInt _df6860f57d568704(string value)
		=> _7a077e2e1baba462(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator ulong(decimal)")]
	public static BigInt _047386be34a2d276(string value)
		=> _9b15def492d41a4a(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a single-precision floating-point number.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator float(decimal)")]
	public static Number _2de5f5a183f9455b(string value)
		=> _1450e4ab34b1a945(value);

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a double-precision floating-point number.</summary>
	[Jazor(Op.Import ,"static decimal.explicit operator double(decimal)")]
	public static Number _2db2eb304fe215ee(string value)
		=> _cfbbd251b43c99f4(value);

	///<summary>Returns the value of the <see cref="T:System.Decimal" /> operand (the sign of the operand is unchanged).</summary>
	[Jazor(Op.Import ,"static decimal.operator +(decimal)")]
	public static string _53fb6447e19a3943(string d)
		=> PreserveDecimal(d);

	///<summary>Negates the value of the specified <see cref="T:System.Decimal" /> operand.</summary>
	[Jazor(Op.Import ,"static decimal.operator -(decimal)")]
	public static string _ec128cb5140788f6(string d)
		=> NegateDecimal(d);

	///<summary>Increments the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by 1.</summary>
	[Jazor(Op.Import ,"static decimal.operator ++(decimal)")]
	public static string _20e1c565f1757f95(string d)
		=> AddDecimal(d, "1");

	///<summary>Decrements the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by one.</summary>
	[Jazor(Op.Import ,"static decimal.operator --(decimal)")]
	public static string _92103936e252998e(string d)
		=> SubtractDecimal(d, "1");

	///<summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Import ,"static decimal.operator +(decimal, decimal)")]
	public static string _6916013808c205d4(string d1, string d2)
		=> AddDecimal(d1, d2);

	///<summary>Subtracts two specified <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Import ,"static decimal.operator -(decimal, decimal)")]
	public static string _7b8c963ebbb0237b(string d1, string d2)
		=> SubtractDecimal(d1, d2);

	///<summary>Multiplies two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
	[Jazor(Op.Import ,"static decimal.operator *(decimal, decimal)")]
	public static string _5794746a3d1c5c7d(string d1, string d2)
		=> MultiplyDecimal(d1, d2);

	///<summary>Divides two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
	[Jazor(Op.Import ,"static decimal.operator /(decimal, decimal)")]
	public static string _18540fea4c4d81f3(string d1, string d2)
		=> DivideDecimal(d1, d2);

	///<summary>Returns the remainder resulting from dividing two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
	[Jazor(Op.Import ,"static decimal.operator %(decimal, decimal)")]
	public static string _cf5ffdcf799ce372(string d1, string d2)
		=> RemainderDecimal(d1, d2);

	///<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values are equal.</summary>
	[Jazor(Op.Import ,"static decimal.operator ==(decimal, decimal)")]
	public static bool _9831be72bebc3a57(string d1, string d2)
		=> CompareDecimal(d1, d2) == 0;

	///<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> objects have different values.</summary>
	[Jazor(Op.Import ,"static decimal.operator !=(decimal, decimal)")]
	public static bool _6e351e0d21e0ccd9(string d1, string d2)
		=> CompareDecimal(d1, d2) != 0;

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Import ,"static decimal.operator <(decimal, decimal)")]
	public static bool _9e3b1978bc32f62a(string d1, string d2)
		=> CompareDecimal(d1, d2) < 0;

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Import ,"static decimal.operator <=(decimal, decimal)")]
	public static bool _01544ed3b8bf9a49(string d1, string d2)
		=> CompareDecimal(d1, d2) <= 0;

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Import ,"static decimal.operator >(decimal, decimal)")]
	public static bool _bb8c4bd3620de56b(string d1, string d2)
		=> CompareDecimal(d1, d2) > 0;

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Import ,"static decimal.operator >=(decimal, decimal)")]
	public static bool _325daf3875076acb(string d1, string d2)
		=> CompareDecimal(d1, d2) >= 0;

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Inline ,"decimal.GetTypeCode()", "15")]
	public extern static System.TypeCode _323e061741a92593(string instance);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[Jazor(Op.Discard ,"static decimal.ConvertToInteger<TInteger>(decimal)")]
	public extern static TInteger _3c8005c9c5a1e322<TInteger>(string value);

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[Jazor(Op.Discard ,"static decimal.ConvertToIntegerNative<TInteger>(decimal)")]
	public extern static TInteger _c3fce0dbb13c48ea<TInteger>(string value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Import ,"static decimal.Clamp(decimal, decimal, decimal)")]
	public static string _e886400fbfdbdaaa(string value, string min, string max)
	{
		if (CompareDecimal(min, max) > 0)
			throw new Error("ArgumentException: min must be less than or equal to max.");
		if (CompareDecimal(value, min) < 0)
			return PreserveDecimal(min);
		if (CompareDecimal(value, max) > 0)
			return PreserveDecimal(max);
		return PreserveDecimal(value);
	}

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Import ,"static decimal.CopySign(decimal, decimal)")]
	public static string _30df447725c40575(string value, string sign)
	{
		var absolute = AbsDecimal(value);
		return SignDecimal(sign) < 0 ? NegateDecimal(absolute) : absolute;
	}

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Import ,"static decimal.Max(decimal, decimal)")]
	public static string _872018e11335480a(string x, string y)
		=> CompareDecimal(x, y) >= 0 ? PreserveDecimal(x) : PreserveDecimal(y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Import ,"static decimal.Min(decimal, decimal)")]
	public static string _ceb21f954af742e7(string x, string y)
		=> CompareDecimal(x, y) < 0 ? PreserveDecimal(x) : PreserveDecimal(y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Import ,"static decimal.Sign(decimal)")]
	public static Number _ed803cf9c8c052f1(string d)
		=> SignDecimal(d);

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Import ,"static decimal.Abs(decimal)")]
	public static string _e85678b4de2283e8(string value)
		=> AbsDecimal(value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static decimal.CreateChecked<TOther>(TOther)")]
	public extern static string _1db5e716e3d6b295<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static decimal.CreateSaturating<TOther>(TOther)")]
	public extern static string _0263284f14d9d42b<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static decimal.CreateTruncating<TOther>(TOther)")]
	public extern static string _5c966a3c7ee1bf4c<TOther>(object value);

	///<summary>Determines if a value is in its canonical representation.</summary>
	[Jazor(Op.Import ,"static decimal.IsCanonical(decimal)")]
	public static bool _b80d517d733633a6(string value)
	{
		try
		{
			return value == NormalizeDecimal(value);
		}
		catch
		{
			return false;
		}
	}

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Import ,"static decimal.IsEvenInteger(decimal)")]
	public static bool _9d28fa751d24ce2e(string value)
		=> IsIntegerDecimal(value) && TruncateToIntegralValue(value) % BigIntValue(2) == BigInt.Zero;

	///<summary>Determines if a value represents an integral number.</summary>
	[Jazor(Op.Import ,"static decimal.IsInteger(decimal)")]
	public static bool _e79590278b446432(string value)
		=> IsIntegerDecimal(value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Import ,"static decimal.IsNegative(decimal)")]
	public static bool _1ad42f1c78dbe014(string value)
		=> SignDecimal(value) < 0;

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Import ,"static decimal.IsOddInteger(decimal)")]
	public static bool _38587400d9c44cb5(string value)
		=> IsIntegerDecimal(value) && TruncateToIntegralValue(value) % BigIntValue(2) != BigInt.Zero;

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Import ,"static decimal.IsPositive(decimal)")]
	public static bool _03c325899b0e33f0(string value)
		=> SignDecimal(value) >= 0;

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Import ,"static decimal.MaxMagnitude(decimal, decimal)")]
	public static string _becce0ac49342bb2(string x, string y)
	{
		var ax = AbsDecimal(x);
		var ay = AbsDecimal(y);
		var comparison = CompareDecimal(ax, ay);
		if (comparison > 0)
			return PreserveDecimal(x);
		if (comparison < 0)
			return PreserveDecimal(y);
		return CompareDecimal(x, y) >= 0 ? PreserveDecimal(x) : PreserveDecimal(y);
	}

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Import ,"static decimal.MinMagnitude(decimal, decimal)")]
	public static string _5df17b0a512de878(string x, string y)
	{
		var ax = AbsDecimal(x);
		var ay = AbsDecimal(y);
		var comparison = CompareDecimal(ax, ay);
		if (comparison < 0)
			return PreserveDecimal(x);
		if (comparison > 0)
			return PreserveDecimal(y);
		return CompareDecimal(x, y) <= 0 ? PreserveDecimal(x) : PreserveDecimal(y);
	}

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(string, System.IFormatProvider, out decimal)")]
	public static Array<object?> _a3ffdb214a9c82a0(string? s, Intl.NumberFormat? provider, string result)
	{
		if (s == null || s.Length == 0)
			return [false, "0"];

		try
		{
			return [true, ParseDecimalExternal(s, NumberStyleNumber, provider)];
		}
		catch
		{
			return [false, "0"];
		}
	}

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static string _c644fa2b15360347(string s, Intl.NumberFormat? provider)
		=> ParseDecimalExternal(s, NumberStyleNumber, provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)")]
	public static Array<object?> _7ac8df441c1485cf(string s, Intl.NumberFormat? provider, string result)
		=> _a3ffdb214a9c82a0(s, provider, result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Import, "static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public static string _e81acb76373d457e(Uint8Array utf8Text, object style, Intl.NumberFormat? provider)
		=> _f525a420b2d600ec(RuntimeModule.DecodeUtf8OrThrowFormat(utf8Text), style, provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Import, "static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public static Array<object?> _acbda6e104ca3de4(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, string result)
		=> _b4ecd2424c9a371e(RuntimeModule.TryDecodeUtf8(utf8Text), style, provider, result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Import, "static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public static string _d3d821054d142668(Uint8Array utf8Text, Intl.NumberFormat? provider)
		=> _01be2a34fe2cda4e(RuntimeModule.DecodeUtf8OrThrowFormat(utf8Text), provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Import, "static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)")]
	public static Array<object?> _8122c647766e18ff(Uint8Array utf8Text, Intl.NumberFormat? provider, string result)
		=> _a3ffdb214a9c82a0(RuntimeModule.TryDecodeUtf8(utf8Text), provider, result);
}
