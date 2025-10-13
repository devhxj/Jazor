namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("bool")]
public static class BooleanModule
{
    ///<summary>Represents the Boolean value <see langword="true" /> as a string. This field is read-only.</summary>
    [WhiteList("static readonly bool.TrueString")]
	[ECMAScriptLiteral("'true'", false)]
	public extern static string BooleanTrueString();

    ///<summary>Represents the Boolean value <see langword="false" /> as a string. This field is read-only.</summary>
    [WhiteList("static readonly bool.FalseString")]
	[ECMAScriptLiteral("'false'", false)]
	public extern static bool BooleanFalseString();

    [WhiteList("bool.Boolean()")]
	public extern static bool BooleanNew();

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A hash code for the current <see cref="T:System.Boolean" />.</returns>
    [WhiteList("override bool.GetHashCode()")]
	[ECMAScriptLiteral("{0} ? 1 : 0")]
	public extern static Number BooleanGetHashCode(bool instance);

    ///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
    ///<returns>"True" (the value of the <see cref="F:System.Boolean.TrueString" /> property) if the value of this instance is <see langword="true" />, or "False" (the value of the <see cref="F:System.Boolean.FalseString" /> property) if the value of this instance is <see langword="false" />.</returns>
    [WhiteList("override bool.ToString()")]
	public extern static string BooleanToString(bool instance);

    ///<summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
    ///<param name="provider">(Reserved) An <see cref="T:System.IFormatProvider" /> object.</param>
    ///<returns>  <see cref="F:System.Boolean.TrueString" /> if the value of this instance is <see langword="true" />, or <see cref="F:System.Boolean.FalseString" /> if the value of this instance is <see langword="false" />.</returns>
    [WhiteList("bool.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string BooleanToString2(bool instance, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current boolean instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
    [WhiteList("bool.TryFormat(System.Span<char>, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool BooleanTryFormat(bool instance, Uint32Array destination, OutValue<Number> charsWritten);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.Boolean" /> and has the same value as this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("override bool.Equals(object)")]
	[WhiteList("bool.Equals(bool)")]
	[ECMAScriptLiteral("{0} === {1}")]
	public extern static bool BooleanEquals(bool instance, Object? obj);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates their relationship to one another.</summary>
    ///<param name="obj">An object to compare to this instance, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="obj" /> is not a <see cref="T:System.Boolean" />.</exception>
    ///<returns>A signed integer that indicates the relative order of this instance and <paramref name="obj" />. <list type="table"><listheader><term> Return Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance is <see langword="false" /> and <paramref name="obj" /> is <see langword="true" />.</description></item><item><term> Zero</term><description> This instance and <paramref name="obj" /> are equal (either both are <see langword="true" /> or both are <see langword="false" />).</description></item><item><term> Greater than zero</term><description> This instance is <see langword="true" /> and <paramref name="obj" /> is <see langword="false" />. -or- <paramref name="obj" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("bool.CompareTo(object)")]
	[WhiteList("bool.CompareTo(bool)")]
	[ECMAScriptLiteral("{0} === {1} ? 0 :({0} > {1} ? 1 : -1)")]
	public extern static Number BooleanCompareTo(bool instance, Object? obj);

    ///<summary>Converts the specified string representation of a logical value to its <see cref="T:System.Boolean" /> equivalent.</summary>
    ///<param name="value">A string containing the value to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="value" /> is not equivalent to <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is equivalent to <see cref="F:System.Boolean.TrueString" />; <see langword="false" /> if <paramref name="value" /> is equivalent to <see cref="F:System.Boolean.FalseString" />.</returns>
    [WhiteList("static bool.Parse(string)")]
    public static bool BooleanParse(string value)
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
    [WhiteList("static bool.Parse(System.ReadOnlySpan<char>)")]
    public static bool BooleanParse2(Uint32Array value)
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
    [WhiteList("static bool.TryParse(string, out bool)")]
    public static bool BooleanTryParse(string? value, OutValue<bool> result)
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
    [WhiteList("static bool.TryParse(System.ReadOnlySpan<char>, out bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public static bool BooleanTryParse2(Uint32Array value, OutValue<bool> result)
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
    [WhiteList("bool.GetTypeCode()")]
    [ECMAScriptLiteral("'Boolean'")]
    public extern static TypeCode BooleanGetTypeCode(bool instance);
}
