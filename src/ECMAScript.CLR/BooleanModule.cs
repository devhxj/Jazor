using System.Collections;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("bool", "bool", "bool")]
public static class BooleanModule
{
    [WhiteList("_2bd9618624257446", "bool.Boolean()", "_2bd9618624257446")]
	public extern static Boolean _2bd9618624257446();

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A hash code for the current <see cref="T:System.Boolean" />.</returns>
    [WhiteList("_80b6c29cc0038969", "override bool.GetHashCode()", "_80b6c29cc0038969")]
	[ECMAScriptLiteral("@#{0} ? 1 : 0")]
	public extern static Number _80b6c29cc0038969(Boolean instance);

    ///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
    ///<returns>"True" (the value of the <see cref="F:System.Boolean.TrueString" /> property) if the value of this instance is <see langword="true" />, or "False" (the value of the <see cref="F:System.Boolean.FalseString" /> property) if the value of this instance is <see langword="false" />.</returns>
    [WhiteList("_d48c2d39317daf8f", "override bool.ToString()", "_d48c2d39317daf8f")]
	public extern static string _d48c2d39317daf8f(Boolean instance);

    ///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
    ///<param name="provider">(Reserved) An <see cref="T:System.IFormatProvider" /> object.</param>
    ///<returns>  <see cref="F:System.Boolean.TrueString" /> if the value of this instance is <see langword="true" />, or <see cref="F:System.Boolean.FalseString" /> if the value of this instance is <see langword="false" />.</returns>
    [WhiteList("_6e30cb91da447de8", "bool.ToString(System.IFormatProvider)", "_6e30cb91da447de8")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _6e30cb91da447de8(Boolean instance, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current boolean instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
    [WhiteList("_811623fcb5eec2f4", "bool.TryFormat(System.Span<char>, out int)", "_811623fcb5eec2f4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _811623fcb5eec2f4(Boolean instance, Uint32Array destination, OutValue<Number> charsWritten);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.Boolean" /> and has the same value as this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("_97cc6572c33639b7", "override bool.Equals(object)", "_97cc6572c33639b7")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _97cc6572c33639b7(Boolean instance, Object? obj);

    ///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Boolean" /> object.</summary>
    ///<param name="obj">A <see cref="T:System.Boolean" /> value to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> has the same value as this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("_22566f8453458531", "bool.Equals(bool)", "_22566f8453458531")]
	[ECMAScriptLiteral("@#{0} === @#{1}")]
	public extern static bool _22566f8453458531(Boolean instance, bool obj);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates their relationship to one another.</summary>
    ///<param name="obj">An object to compare to this instance, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="obj" /> is not a <see cref="T:System.Boolean" />.</exception>
    ///<returns>A signed integer that indicates the relative order of this instance and <paramref name="obj" />. <list type="table"><listheader><term> Return Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance is <see langword="false" /> and <paramref name="obj" /> is <see langword="true" />.</description></item><item><term> Zero</term><description> This instance and <paramref name="obj" /> are equal (either both are <see langword="true" /> or both are <see langword="false" />).</description></item><item><term> Greater than zero</term><description> This instance is <see langword="true" /> and <paramref name="obj" /> is <see langword="false" />. -or- <paramref name="obj" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("_f877237b160159b0", "bool.CompareTo(object)", "_f877237b160159b0")]
	[ECMAScriptLiteral("@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _f877237b160159b0(Boolean instance, Object? obj);

    ///<summary>Compares this instance to a specified <see cref="T:System.Boolean" /> object and returns an integer that indicates their relationship to one another.</summary>
    ///<param name="value">A <see cref="T:System.Boolean" /> object to compare to this instance.</param>
    ///<returns>A signed integer that indicates the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance is <see langword="false" /> and <paramref name="value" /> is <see langword="true" />.</description></item><item><term> Zero</term><description> This instance and <paramref name="value" /> are equal (either both are <see langword="true" /> or both are <see langword="false" />).</description></item><item><term> Greater than zero</term><description> This instance is <see langword="true" /> and <paramref name="value" /> is <see langword="false" />.</description></item></list></returns>
    [WhiteList("_52e94ceda3f9af79", "bool.CompareTo(bool)", "_52e94ceda3f9af79")]
    [ECMAScriptLiteral("@#{0} === @#{1} ? 0 :(@#{0} > @#{1} ? 1 : -1)")]
	public extern static Number _52e94ceda3f9af79(Boolean instance, bool value);

    ///<summary>Converts the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
    ///<param name="value">A string containing the value to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> is not equivalent to <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is equivalent to <see cref="F:System.Boolean.TrueString" />; <see langword="false" /> if <paramref name="value" /> is equivalent to <see cref="F:System.Boolean.FalseString" />.</returns>
    [WhiteList("_5dbf54319ebc8dfe", "static bool.Parse(string)", "_5dbf54319ebc8dfe")]
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
	///<param name="value">A span containing the characters representing the value to convert.</param>
	///<returns>  <see langword="true" /> if <paramref name="value" /> is equivalent to <see cref="F:System.Boolean.TrueString" />; <see langword="false" /> if <paramref name="value" /> is equivalent to <see cref="F:System.Boolean.FalseString" />.</returns>
	[WhiteList("_c3ccfdf8f687d2bf", "static bool.Parse(System.ReadOnlySpan<char>)", "_c3ccfdf8f687d2bf")]
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
	///<param name="value">A string containing the value to convert.</param>
	///<param name="result">When this method returns, if the conversion succeeded, contains <see langword="true" /> if <paramref name="value" /> is equal to <see cref="F:System.Boolean.TrueString" /> or <see langword="false" /> if <paramref name="value" /> is equal to <see cref="F:System.Boolean.FalseString" />. If the conversion failed, contains <see langword="false" />. The conversion fails if <paramref name="value" /> is <see langword="null" /> or is not equal to the value of either the <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" /> field.</param>
	///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
	[WhiteList("_dada4bbdacd7aa19", "static bool.TryParse(string, out bool)", "_dada4bbdacd7aa19")]
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
	///<param name="value">A span containing the characters representing the value to convert.</param>
	///<param name="result">When this method returns, if the conversion succeeded, contains <see langword="true" /> if <paramref name="value" /> is equal to <see cref="F:System.Boolean.TrueString" /> or <see langword="false" /> if <paramref name="value" /> is equal to <see cref="F:System.Boolean.FalseString" />. If the conversion failed, contains <see langword="false" />. The conversion fails if <paramref name="value" /> is <see langword="null" /> or is not equal to the value of either the <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" /> field.</param>
	///<returns>  <see langword="true" /> if <paramref name="value" /> was converted successfully; otherwise, <see langword="false" />.</returns>
	[WhiteList("_619c4d1c94319558", "static bool.TryParse(System.ReadOnlySpan<char>, out bool)", "_619c4d1c94319558")]
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
	///<returns>The enumerated constant <see cref="F:System.TypeCode.Boolean" />.</returns>
	[WhiteList("_eb6a23c2a874fdf1", "bool.GetTypeCode()", "_eb6a23c2a874fdf1")]
	[ECMAScriptLiteral("'Boolean'")]
	public extern static System.TypeCode _eb6a23c2a874fdf1(Boolean instance);
}
