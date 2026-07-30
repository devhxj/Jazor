namespace Jazor.CLR;

/// <summary>
/// System.Guid 与 UUID 字符串的映射。
/// </summary>
[ECMAScriptModule("System/GuidModule.js")]
[Jazor(Op.Alias, "System.Guid", "String")]
public static class GuidModule
{
	private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

	private static int GetHexValue(char c)
	{
		if (c >= '0' && c <= '9')
			return c - '0';
		if (c >= 'a' && c <= 'f')
			return c - 'a' + 10;
		if (c >= 'A' && c <= 'F')
			return c - 'A' + 10;

		throw new Error("FormatException: Guid contained a non-hexadecimal character.");
	}

	private static bool IsHexDigit(char c)
		=> (c >= '0' && c <= '9')
			|| (c >= 'a' && c <= 'f')
			|| (c >= 'A' && c <= 'F');

	private static int ParseHexByte(string text, int start)
		=> (GetHexValue(text[start]) << 4) | GetHexValue(text[start + 1]);

	private static bool TryNormalizeGuid(string input, out string normalized)
	{
		normalized = EmptyGuid;

		var text = input.Trim();
		if (text.Length == 0)
			return false;

		if ((text[0] == '{' && text[text.Length - 1] == '}') || (text[0] == '(' && text[text.Length - 1] == ')'))
			text = text.Substring(1, text.Length - 2);

		if (text.Length == 32)
		{
			for (var i = 0; i < text.Length; i++)
			{
				if (!IsHexDigit(text[i]))
					return false;
			}

			text = text.Substring(0, 8)
				+ "-"
				+ text.Substring(8, 4)
				+ "-"
				+ text.Substring(12, 4)
				+ "-"
				+ text.Substring(16, 4)
				+ "-"
				+ text.Substring(20, 12);
		}
		else if (text.Length != 36)
		{
			return false;
		}

		var lower = "";
		for (var i = 0; i < text.Length; i++)
		{
			if (i == 8 || i == 13 || i == 18 || i == 23)
			{
				if (text[i] != '-')
					return false;

				lower += "-";
				continue;
			}

			if (!IsHexDigit(text[i]))
				return false;

			var c = text[i];
			lower += c >= 'A' && c <= 'F' ? ((char)(c + 32)).ToString() : c.ToString();
		}

		normalized = lower;
		return true;
	}

	[Jazor(Op.Import, "System.Guid.Guid()")]
	public static string _0e58e51018e846d2()
		=> EmptyGuid;

	[Jazor(Op.Import, "System.Guid.Guid(string)")]
	public static string _24e026ca196fe82b(string g)
	{
		if (!TryNormalizeGuid(g, out var normalized))
			throw new Error($"FormatException: Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx). Value was '{g}'.");

		return normalized;
	}

	[Jazor(Op.Import, "static readonly System.Guid.Empty")]
	public static string _b4f8dd2bd0561d7e()
		=> EmptyGuid;

	[Jazor(Op.Inline, "static System.Guid.NewGuid()", "globalThis.crypto.randomUUID()")]
	public extern static string _d707d638b3f6760e();

	[Jazor(Op.Import, "static System.Guid.Parse(string)")]
	public static string _085f2911d59439cb(string input)
		=> _24e026ca196fe82b(input);

	[Jazor(Op.Import, "static System.Guid.Parse(System.ReadOnlySpan<char>)")]
	public static string _352ce05083173561(string input)
		=> _24e026ca196fe82b(input);

	[Jazor(Op.Import, "static System.Guid.TryParse(string, out System.Guid)")]
	public static Array<object?> _a7f2670ff5b9fe61(string? input, string result)
	{
		if (input != null && TryNormalizeGuid(input, out var normalized))
			return [true, normalized];

		return [false, EmptyGuid];
	}

	[Jazor(Op.Import, "static System.Guid.TryParse(System.ReadOnlySpan<char>, out System.Guid)")]
	public static Array<object?> _f886f69cda12fbc3(string input, string result)
		=> _a7f2670ff5b9fe61(input, result);

	[Jazor(Op.Import, "override System.Guid.ToString()")]
	public static string _055f1f857de6de37(string instance)
		=> _24e026ca196fe82b(instance);

	[Jazor(Op.Import, "System.Guid.ToString(string)")]
	public static string _a79f651902f5c771(string instance, string? format)
	{
		var normalized = _24e026ca196fe82b(instance);
		if (format == null || format.Length == 0)
			return normalized;
		if (format.Length != 1)
			throw new Error("FormatException: Format string can be only 'N', 'D', 'B', or 'P'.");

		var specifier = format[0];
		if (specifier >= 'a' && specifier <= 'z')
			specifier = (char)(specifier - 32);

		if (specifier == 'D')
			return normalized;
		if (specifier == 'N')
			return normalized.Replace("-", "");
		if (specifier == 'B')
			return "{" + normalized + "}";
		if (specifier == 'P')
			return "(" + normalized + ")";

		throw new Error("FormatException: Format string can be only 'N', 'D', 'B', or 'P'.");
	}

	[Jazor(Op.Import, "System.Guid.ToString(string, System.IFormatProvider)")]
	public static string _dfe41e7b4ff05614(string instance, string? format, object? provider)
		=> _a79f651902f5c771(instance, format);

	[Jazor(Op.Import, "override System.Guid.Equals(object)")]
	public static bool _7883fdaac79384d5(string instance, object? value)
	{
		var other = value as string;
		if (other == null)
			return false;
		if (!TryNormalizeGuid(other, out var normalizedOther))
			return false;

		return _24e026ca196fe82b(instance) == normalizedOther;
	}

	[Jazor(Op.Import, "System.Guid.Equals(System.Guid)")]
	public static bool _79ee6ab0f29f29dd(string instance, string value)
		=> _24e026ca196fe82b(instance) == _24e026ca196fe82b(value);

	[Jazor(Op.Import, "override System.Guid.GetHashCode()")]
	public static Number _6237dbaa794d5c98(string instance)
	{
		var normalized = _24e026ca196fe82b(instance);
		var b0 = ParseHexByte(normalized, 6);
		var b1 = ParseHexByte(normalized, 4);
		var b2 = ParseHexByte(normalized, 2);
		var b3 = ParseHexByte(normalized, 0);
		var b4 = ParseHexByte(normalized, 11);
		var b5 = ParseHexByte(normalized, 9);
		var b6 = ParseHexByte(normalized, 16);
		var b7 = ParseHexByte(normalized, 14);
		var b8 = ParseHexByte(normalized, 19);
		var b9 = ParseHexByte(normalized, 21);
		var b10 = ParseHexByte(normalized, 24);
		var b11 = ParseHexByte(normalized, 26);
		var b12 = ParseHexByte(normalized, 28);
		var b13 = ParseHexByte(normalized, 30);
		var b14 = ParseHexByte(normalized, 32);
		var b15 = ParseHexByte(normalized, 34);

		var i0 = b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
		var i1 = b4 | (b5 << 8) | (b6 << 16) | (b7 << 24);
		var i2 = b8 | (b9 << 8) | (b10 << 16) | (b11 << 24);
		var i3 = b12 | (b13 << 8) | (b14 << 16) | (b15 << 24);
		var hash = i0 ^ i1 ^ i2 ^ i3;

		return hash | 0;
	}
}
