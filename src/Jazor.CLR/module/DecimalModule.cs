namespace Jazor.CLR;

[ECMAScriptModule("System/DecimalModule.js")]
[Jazor(Op.Alias, "decimal","String")]
public static class DecimalModule
{
	private static Number MaxFractionDigits => 28;
	private static BigInt MaxDecimalUnscaled => BigInt_("79228162514264337593543950335");

	private static Array<object?> CreateParts(BigInt unscaled, Number scale)
		=> [unscaled, scale];

	private static BigInt GetUnscaled(Array<object?> parts)
		=> (BigInt)parts[0]!;

	private static Number GetScale(Array<object?> parts)
		=> (Number)parts[1]!;

	private static BigInt Pow10(Number exponent)
	{
		var result = BigInt_(1);
		for (var i = 0; i < exponent; i++)
			result *= BigInt_(10);

		return result;
	}

	private static Number MaxNumber(Number left, Number right)
		=> left >= right ? left : right;

	private static string RepeatZero(Number count)
	{
		var text = "";
		for (var i = 0; i < count; i++)
			text += "0";

		return text;
	}

	private static string StripLeadingZeros(string digits)
	{
		while (digits.Length > 1 && digits[0] == '0')
			digits = digits.Substring(1);

		return digits;
	}

	private static Array<object?> NormalizeParts(BigInt unscaled, Number scale)
	{
		if (unscaled == BigInt.Zero)
			return CreateParts(BigInt.Zero, 0);

		while (scale > 0 && unscaled % BigInt_(10) == BigInt.Zero)
		{
			unscaled /= BigInt_(10);
			scale--;
		}

		if (scale < 0 || scale > MaxFractionDigits)
			throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
		var absolute = unscaled < BigInt.Zero ? -unscaled : unscaled;
		if (absolute > MaxDecimalUnscaled)
			throw new Error("OverflowException: Value was either too large or too small for a Decimal.");

		return CreateParts(unscaled, scale);
	}

	private static Array<object?> ParseDecimal(string value)
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
			if (exponentIndex == 0 || exponentIndex == s.Length - 1)
				throw new Error($"FormatException: String '{value}' was not recognized as a valid Decimal.");

			var exponentText = s.Substring(exponentIndex + 1);
			var exponentValue = Number_(exponentText);
			if (IsNaN(exponentValue) || Math.Floor_(exponentValue) != exponentValue)
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

		var unscaled = BigInt_(digits);
		if (negative && unscaled != BigInt.Zero)
			unscaled = -unscaled;

		return NormalizeParts(unscaled, scale);
	}

	private static string FormatDecimal(BigInt unscaled, Number scale)
	{
		var normalized = NormalizeParts(unscaled, scale);
		unscaled = GetUnscaled(normalized);
		scale = GetScale(normalized);
		if (unscaled == BigInt.Zero)
			return "0";

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

	private static string NormalizeDecimal(string value)
	{
		var parts = ParseDecimal(value);
		return FormatDecimal(GetUnscaled(parts), GetScale(parts));
	}

	private static string CreateDecimalFromNumber(Number value)
	{
		if (!DoubleModule._aed2927097617729(value))
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
		var comparison = absoluteRemainder * BigInt_(2) - absoluteDenominator;
		if (comparison < BigInt.Zero)
			return quotient;

		var step = numerator < BigInt.Zero ? -BigInt_(1) : BigInt_(1);
		if (comparison > BigInt.Zero)
			return quotient + step;

		return quotient % BigInt_(2) == BigInt.Zero ? quotient : quotient + step;
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

		return FormatDecimal(DivideAndRound(numerator, denominator), MaxFractionDigits);
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
			quotient -= BigInt_(1);

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
			quotient += BigInt_(1);

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
		=> GetScale(ParseDecimal(value)) == 0;

	private static Number GetStringHashCode(string text)
	{
		var hash = 0;
		for (var i = 0; i < text.Length; i++)
			hash = ((hash << 5) - hash) + text[i];

		return hash | 0;
	}

	//decimal.Zero = 0;

	//decimal.One = 1;

	//decimal.MinusOne = -1;

	//decimal.MaxValue = 79228162514264337593543950335;

	//decimal.MinValue = -79228162514264337593543950335;

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
	[Jazor(Op.Discard ,"static decimal.FromOACurrency(long)")]
	public extern static string _6cd0f8dfbedd7209(BigInt cy);

	///<summary>Converts the specified <see cref="T:System.Decimal" /> value to the equivalent OLE Automation Currency value, which is contained in a 64-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToOACurrency(decimal)")]
	public extern static BigInt _5d257b5cc33cdaeb(string value);

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

		var other = value as string;
		if (other == null)
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
		var other = value as string;
		return other != null && CompareDecimal(instance, other) == 0;
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
		=> GetStringHashCode(NormalizeDecimal(instance));

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
		=> NormalizeDecimal(instance);

	/// <summary>
	/// C#: instance.ToString(format)
	/// JS: Number(instance).toFixed(Number(format.replace(/[^0-9]/g, '')) || 0)
	/// </summary>
	[Jazor(Op.Import, "decimal.ToString(string)")]
	public static string _af32d07083f1da07(string instance, string? format)
	{
		return NormalizeDecimal(instance);
	}

	/// <summary>
	/// C#: instance.ToString(provider)
	/// JS: instance
	/// </summary>
	[Jazor(Op.Import, "decimal.ToString(System.IFormatProvider)")]
	public static string _6234ba988b3e006d(string instance, Intl.NumberFormat? provider)
		=> NormalizeDecimal(instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Import ,"decimal.ToString(string, System.IFormatProvider)")]
	public static string _b1e6a06111674f0c(string instance, string? format, Intl.NumberFormat? provider)
		=> NormalizeDecimal(instance);

	///<summary>Tries to format the value of the current decimal instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"decimal.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _919259e7087cfd17(string instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"decimal.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _c5d11df37776e790(string instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string)")]
	public static string _91a2436283a24315(string s)
		=> NormalizeDecimal(s);

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string, System.Globalization.NumberStyles)")]
	public static string _79a0e8ede29256cc(string s, object style)
		=> NormalizeDecimal(s);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string, System.IFormatProvider)")]
	public static string _01be2a34fe2cda4e(string s, Intl.NumberFormat? provider)
		=> NormalizeDecimal(s);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public static string _f525a420b2d600ec(string s, object style, Intl.NumberFormat? provider)
		=> NormalizeDecimal(s);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public static string _8e0c949ee2411c7f(string s, object style, Intl.NumberFormat? provider)
		=> NormalizeDecimal(s);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(string, out decimal)")]
	public static Array<object?> _e96278809bb50e35(string? s, string result)
	{
		if (s == null || s.Length == 0)
			return [false, "0"];

		try
		{
			return [true, NormalizeDecimal(s)];
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
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)")]
	public extern static Array<object?> _0111d7c27998205b(Uint8Array utf8Text, string result);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public static Array<object?> _b4ecd2424c9a371e(string? s, object style, Intl.NumberFormat? provider, string result)
		=> _e96278809bb50e35(s, result);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public static Array<object?> _ed6b24306e2ef5cd(string s, object style, Intl.NumberFormat? provider, string result)
		=> _e96278809bb50e35(s, result);

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
	[Jazor(Op.Discard ,"static decimal.Round(decimal)")]
	public extern static string _4a816369b59f1ca3(string d);

	///<summary>Rounds a <see cref="T:System.Decimal" /> value to a specified number of decimal places.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal, int)")]
	public extern static string _bc3a974d51c694ab(string d, Number decimals);

	///<summary>Rounds a decimal value to an integer using the specified rounding strategy.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal, System.MidpointRounding)")]
	public extern static string _a334f7e82122cfc2(string d, object mode);

	///<summary>Rounds a decimal value to the specified precision using the specified rounding strategy.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal, int, System.MidpointRounding)")]
	public extern static string _09ee3a4652dbe73c(string d, Number decimals, object mode);

	///<summary>Subtracts a specified <see cref="T:System.Decimal" /> value from another.</summary>
	[Jazor(Op.Import ,"static decimal.Subtract(decimal, decimal)")]
	public static string _3e80f2d9cf753d05(string d1, string d2)
		=> SubtractDecimal(d1, d2);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToByte(decimal)")]
	public extern static Number _d2aabede7e0207c1(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToSByte(decimal)")]
	public extern static Number _175bf5ee849fcf8f(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToInt16(decimal)")]
	public extern static Number _5df8c6a064c50c5f(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent double-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.ToDouble(decimal)")]
	public extern static Number _cfbbd251b43c99f4(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToInt32(decimal)")]
	public extern static Number _ad71e0d1a8679244(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToInt64(decimal)")]
	public extern static BigInt _7a077e2e1baba462(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToUInt16(decimal)")]
	public extern static Number _21bc553743dd324b(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToUInt32(decimal)")]
	public extern static Number _c975b2e5b2f4c009(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToUInt64(decimal)")]
	public extern static BigInt _9b15def492d41a4a(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent single-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.ToSingle(decimal)")]
	public extern static Number _1450e4ab34b1a945(string d);

	///<summary>Returns the integral digits of the specified <see cref="T:System.Decimal" />; any fractional digits are discarded.</summary>
	[Jazor(Op.Import ,"static decimal.Truncate(decimal)")]
	public static string _be8b149ea0e1d76b(string d)
		=> TruncateDecimal(d);

	///<summary>Defines an implicit conversion of an 8-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(byte)")]
	public extern static string _c605c67b2cd1973c();

	///<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(sbyte)")]
	public extern static string _e8d5240b7aa52784();

	///<summary>Defines an implicit conversion of a 16-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(short)")]
	public extern static string _8635fe57a74e1249();

	///<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(ushort)")]
	public extern static string _7c3cfa0de18bd43c();

	///<summary>Defines an implicit conversion of a Unicode character to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(char)")]
	public extern static string _d4af042bf014fd51();

	///<summary>Defines an implicit conversion of a 32-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(int)")]
	public extern static string _f5a5d600ccd38777();

	///<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(uint)")]
	public extern static string _d8b659cd861d2409();

	///<summary>Defines an implicit conversion of a 64-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(long)")]
	public extern static string _23103e069358ca06();

	///<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(ulong)")]
	public extern static string _7ab8c627f74cb718();

	///<summary>Defines an explicit conversion of a single-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator decimal(float)")]
	public extern static string _f456cac2ae523add();

	///<summary>Defines an explicit conversion of a double-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator decimal(double)")]
	public extern static string _8f3a66f6dc828dff();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator byte(decimal)")]
	public extern static Number _a8bfc1feb93c39cb();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit signed integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator sbyte(decimal)")]
	public extern static Number _824c1dbd3e6691ba();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a Unicode character.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator char(decimal)")]
	public extern static Number _e2c93b47df7960a8();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator short(decimal)")]
	public extern static Number _8f4ca64a21fb08cc();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator ushort(decimal)")]
	public extern static Number _3e209c4283c6e05e();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator int(decimal)")]
	public extern static Number _bc03e302b86b6800();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator uint(decimal)")]
	public extern static Number _dea1c1c9c8f2b495();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator long(decimal)")]
	public extern static BigInt _df6860f57d568704();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator ulong(decimal)")]
	public extern static BigInt _047386be34a2d276();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a single-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator float(decimal)")]
	public extern static Number _2de5f5a183f9455b();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a double-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator double(decimal)")]
	public extern static Number _2db2eb304fe215ee();

	///<summary>Returns the value of the <see cref="T:System.Decimal" /> operand (the sign of the operand is unchanged).</summary>
	[Jazor(Op.Import ,"static decimal.operator +(decimal)")]
	public static string _53fb6447e19a3943(string d)
		=> NormalizeDecimal(d);

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
	[Jazor(Op.Discard ,"decimal.GetTypeCode()")]
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
		if (CompareDecimal(value, min) < 0)
			return NormalizeDecimal(min);
		if (CompareDecimal(value, max) > 0)
			return NormalizeDecimal(max);
		return NormalizeDecimal(value);
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
		=> CompareDecimal(x, y) >= 0 ? NormalizeDecimal(x) : NormalizeDecimal(y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Import ,"static decimal.Min(decimal, decimal)")]
	public static string _ceb21f954af742e7(string x, string y)
		=> CompareDecimal(x, y) <= 0 ? NormalizeDecimal(x) : NormalizeDecimal(y);

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
	[Jazor(Op.Discard ,"static decimal.IsCanonical(decimal)")]
	public extern static bool _b80d517d733633a6(string value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Import ,"static decimal.IsEvenInteger(decimal)")]
	public static bool _9d28fa751d24ce2e(string value)
	{
		var parts = ParseDecimal(value);
		return GetScale(parts) == 0 && GetUnscaled(parts) % BigInt_(2) == BigInt.Zero;
	}

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
	{
		var parts = ParseDecimal(value);
		return GetScale(parts) == 0 && GetUnscaled(parts) % BigInt_(2) != BigInt.Zero;
	}

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Import ,"static decimal.IsPositive(decimal)")]
	public static bool _03c325899b0e33f0(string value)
		=> SignDecimal(value) >= 0;

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static decimal.MaxMagnitude(decimal, decimal)")]
	public extern static string _becce0ac49342bb2(string x, string y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static decimal.MinMagnitude(decimal, decimal)")]
	public extern static string _5df17b0a512de878(string x, string y);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(string, System.IFormatProvider, out decimal)")]
	public static Array<object?> _a3ffdb214a9c82a0(string? s, Intl.NumberFormat? provider, string result)
		=> _e96278809bb50e35(s, result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static string _c644fa2b15360347(string s, Intl.NumberFormat? provider)
		=> NormalizeDecimal(s);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)")]
	public static Array<object?> _7ac8df441c1485cf(string s, Intl.NumberFormat? provider, string result)
		=> _e96278809bb50e35(s, result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static string _e81acb76373d457e(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _acbda6e104ca3de4(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, string result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static string _d3d821054d142668(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _8122c647766e18ff(Uint8Array utf8Text, Intl.NumberFormat? provider, string result);
}
