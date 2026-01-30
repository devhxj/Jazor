using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("bool", WhiteListOp.Allowed, "System/BooleanModule.js")]
public static class BooleanModule
{
	[WhiteList("bool.Boolean()", WhiteListOp.Discard)]
	public extern static Boolean _2bd9618624257446();

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override bool.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _80b6c29cc0038969(Boolean instance);

	///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
	[WhiteList("override bool.ToString()", WhiteListOp.ToString)]
	public extern static string _d48c2d39317daf8f(Boolean instance);

	///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
	[WhiteList("bool.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _6e30cb91da447de8(Boolean instance, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current boolean instance into the provided span of characters.</summary>
	[WhiteList("bool.TryFormat(System.Span<char>, out int)", WhiteListOp.Discard)]
	public extern static bool _811623fcb5eec2f4(Boolean instance, Uint32Array destination, OutValue<Number> charsWritten);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override bool.Equals(object)", WhiteListOp.Equals)]
	public extern static bool _97cc6572c33639b7(Boolean instance, Object? obj);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Boolean" /> object.</summary>
	[WhiteList("bool.Equals(bool)", WhiteListOp.Equals)]
	public extern static bool _22566f8453458531(Boolean instance, bool obj);

	///<summary>Compares this instance to a specified object and returns an integer that indicates their relationship to one another.</summary>
	[WhiteList("bool.CompareTo(object)", WhiteListOp.CompareTo)]
	public extern static Number _f877237b160159b0(Boolean instance, Object? obj);

	///<summary>Compares this instance to a specified <see cref="T:System.Boolean" /> object and returns an integer that indicates their relationship to one another.</summary>
	///<param name="value">A <see cref="T:System.Boolean" /> object to compare to this instance.</param>
	///<returns>A signed integer that indicates the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance is <see langword="false" /> and <paramref name="value" /> is <see langword="true" />.</description></item><item><term> Zero</term><description> This instance and <paramref name="value" /> are equal (either both are <see langword="true" /> or both are <see langword="false" />).</description></item><item><term> Greater than zero</term><description> This instance is <see langword="true" /> and <paramref name="value" /> is <see langword="false" />.</description></item></list></returns>
	[WhiteList("bool.CompareTo(bool)", WhiteListOp.CompareTo)]
	public extern static Number _52e94ceda3f9af79(Boolean instance, bool value);

	///<summary>Converts the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[WhiteList("static bool.Parse(string)", WhiteListOp.Import)]
	public static bool _5dbf54319ebc8dfe(string value)
	{
		var str = value.Trim().ToLower();
		if (str == "true")
			return true;
		else if (str == "false")
			return false;
		else
			throw new Error($"FormatException: String '{value}' was not recognized as a valid Boolean.");
	}

	///<summary>Converts the specified span representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[WhiteList("static bool.Parse(System.ReadOnlySpan<char>)", WhiteListOp.Import)]
	public static bool _c3ccfdf8f687d2bf(Uint32Array value)
	{
		// Convert Uint32Array to string
		var str = "";
		for (uint i = 0; i < value.Length; i++)
			str += string.FromCodePoint(value[i]);

		// Trim whitespace (consistent with .NET Boolean.Parse behavior)
		str = str.Trim().ToLower();

		// Case-sensitive comparison with "True" and "False"
		if (str == "true")
			return true;

		else if (str == "false")
			return false;

		// Throw exception for invalid values
		throw new Error("String was not recognized as a valid Boolean.");
	}

	///<summary>Tries to convert the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[WhiteList("static bool.TryParse(string, out bool)", WhiteListOp.Import)]
	public static bool _dada4bbdacd7aa19(string? value, OutValue<bool> result)
	{
		var str = value?.Trim()?.ToLower();
		if (str == "true")
		{
			result.Value = true;
			return true;
		}
		else if (str == "false")
		{
			result.Value = false;
			return true;
		}

		return false;
	}

	///<summary>Tries to convert the specified span representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
	[WhiteList("static bool.TryParse(System.ReadOnlySpan<char>, out bool)", WhiteListOp.Import)]
	public static bool _619c4d1c94319558(Uint32Array value, OutValue<bool> result)
	{
		// Convert Uint32Array to string
		var str = "";
		for (uint i = 0; i < value.Length; i++)
			str += string.FromCodePoint(value[i]);

		// Trim whitespace (consistent with .NET Boolean.Parse behavior)
		str = str.Trim().ToLower();
		if (str == "true")
		{
			result.Value = true;
			return true;
		}
		else if (str == "false")
		{
			result.Value = false;
			return true;
		}

		return false;
	}

	///<summary>Returns the type code for the <see cref="T:System.Boolean" /> value type.</summary>
	[WhiteList("bool.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _eb6a23c2a874fdf1(Boolean instance);
}
