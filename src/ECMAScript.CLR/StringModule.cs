namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("string")]
public static class StringModule
{
    ///<summary>Represents the empty string. This field is read-only.</summary>
    [WhiteList("static readonly string.Empty")]
	[ECMAScriptLiteral("''")]
	public extern static string StringEmpty();

    ///<summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
    ///<param name="str">A string to search for in the intern pool.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="str" /> is <see langword="null" />.</exception>
    ///<returns>The system's reference to <paramref name="str" />, if it is interned; otherwise, a new reference to a string with the value of <paramref name="str" />.</returns>
    [WhiteList("static string.Intern(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringIntern(string str);

    ///<summary>Retrieves a reference to a specified <see cref="T:System.String" />.</summary>
    ///<param name="str">The string to search for in the intern pool.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="str" /> is <see langword="null" />.</exception>
    ///<returns>A reference to <paramref name="str" /> if it is in the common language runtime intern pool; otherwise, <see langword="null" />.</returns>
    [WhiteList("static string.IsInterned(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string? StringIsInterned(string str);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, string, bool)")]
    [WhiteList("static string.Compare(string, string)")]
    public static Number StringCompare(string? strA, string? strB, bool ignoreCase = false)
    {
		// 处理 null 或 undefined 值
        if (IsNullOrUndefined(strA))
            if (IsNullOrUndefined(strB))
                return 0;
            else
                return -1;

		if (IsNullOrUndefined(strB)) 
            return 1;

		// 处理空字符串
        if (object.Is(strA, ""))
            if (object.Is(strB, ""))
                return 0;
            else
                return -1;

		if (Is(strB, "")) 
            return 1;

        // 使用 localeCompare 进行比较
        return strA!.LocaleCompare(strB!, string.Undefined, new
	    {
		    sensitivity = ignoreCase ? "base" : "case",
		    numeric = true,
		    ignorePunctuation = false
	    });
    }

    ///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<exception cref="T:System.NotSupportedException">  <see cref="T:System.StringComparison" /> is not supported.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> is in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, string, System.StringComparison)")]
    public static Number StringCompare2(string? strA, string? strB, System.StringComparison comparisonType)
    {
		if (strA is null && strB is null)
			return 0;
		else if (strA is null)
			return -1;
		else if (strB is null)
			return 1;

		switch (comparisonType)
		{
			case StringComparison.CurrentCulture:
			case StringComparison.InvariantCulture:
				return strA.LocaleCompare(strB);

			case StringComparison.CurrentCultureIgnoreCase:
			case StringComparison.InvariantCultureIgnoreCase:
                return strA.LocaleCompare(strB, null, new { sensitivity = "accent" });

			case StringComparison.Ordinal:
				if (strA < strB) return -1;
				if (strA > strB) return 1;
				return 0;

			case StringComparison.OrdinalIgnoreCase:
				var aLower = strA.ToLower();
				var bLower = strB.ToLower();
				if (aLower < bLower) return -1;
				if (aLower > bLower) return 1;
				return 0;

			default:
				throw new Error("NotSupportedException: StringComparison is not supported.");
		}
	}

    ///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two strings to each other in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="culture">The culture that supplies culture-specific comparison information. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<param name="options">Options to use when performing the comparison (such as ignoring case or symbols).</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between <paramref name="strA" /> and <paramref name="strB" />, as shown in the following table<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, string, System.Globalization.CultureInfo, System.Globalization.CompareOptions)")]
    public static Number StringCompare4(string? strA, string? strB, string? culture, CompareOptions options)
    {
        var a = IsNullOrUndefined(strA) ? "" : strA!;
	    var b = IsNullOrUndefined(strB) ? "" : strB!;

        if ((options & CompareOptions.Ordinal) == CompareOptions.Ordinal)
            return a.LocaleCompare(b, null, new { sensitivity = "base" });

        if ((options & CompareOptions.OrdinalIgnoreCase) == CompareOptions.OrdinalIgnoreCase)
            return a.LocaleCompare(b, null, new { sensitivity = "accent" });

		var locale = OrUndefined(culture);

		var compareOptions = new {
            sensitivity = "variant", 
            ignorePunctuation = false,
            numeric = false,
            caseFirst = "false"
        };

        if ((options & CompareOptions.IgnoreCase) == CompareOptions.IgnoreCase)
            compareOptions = compareOptions with { sensitivity = "accent" };
        
        if ((options & CompareOptions.IgnoreNonSpace) == CompareOptions.IgnoreNonSpace)
	        compareOptions = compareOptions with { sensitivity = "base" };
        
        if ((options & CompareOptions.IgnoreSymbols) == CompareOptions.IgnoreSymbols)
	        compareOptions = compareOptions with { ignorePunctuation = true };
        
        if ((options & CompareOptions.IgnoreWidth) == CompareOptions.IgnoreWidth)
        {

        }

        if ((options & CompareOptions.StringSort) == CompareOptions.StringSort)
        {

        }

        return a.LocaleCompare(b, culture, compareOptions);
    }

    ///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">An object that supplies culture-specific comparison information. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, string, bool, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompare5(string? strA, string? strB, bool ignoreCase, String? culture);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, int, string, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompare6(string? strA, Number indexA, string? strB, Number indexB, Number length);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, int, string, int, int, bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompare7(string? strA, Number indexA, string? strB, Number indexB, Number length, bool ignoreCase);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">An object that supplies culture-specific comparison information. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="strA" /> or <paramref name="strB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<returns>An integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, int, string, int, int, bool, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompare8(string? strA, Number indexA, string? strB, Number indexB, Number length, bool ignoreCase, String? culture);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two substrings to each other in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The starting position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The starting position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<param name="culture">An object that supplies culture-specific comparison information. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<param name="options">Options to use when performing the comparison (such as ignoring case or symbols).</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" /><see langword=".Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" /><see langword=".Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="strA" /> or <paramref name="strB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<returns>An integer that indicates the lexical relationship between the two substrings, as shown in the following table.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, int, string, int, int, System.Globalization.CultureInfo, System.Globalization.CompareOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompare9(string? strA, Number indexA, string? strB, Number indexB, Number length, String? culture, System.Globalization.CompareOptions options);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or the <paramref name="length" /> parameter is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("static string.Compare(string, int, string, int, int, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompare10(string? strA, Number indexA, string? strB, Number indexB, Number length, System.StringComparison comparisonType);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<returns>An integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> is less than <paramref name="strB" />.</description></item><item><term> Zero</term><description><paramref name="strA" /> and <paramref name="strB" /> are equal.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> is greater than <paramref name="strB" />.</description></item></list></returns>
    [WhiteList("static string.CompareOrdinal(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompareOrdinal(string? strA, string? strB);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The starting index of the substring in <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The starting index of the substring in <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="strA" /> is not <see langword="null" /> and <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="strB" /> is not <see langword="null" /> and <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />.</description></item><item><term> Zero</term><description> The substrings are equal, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />.</description></item></list></returns>
    [WhiteList("static string.CompareOrdinal(string, int, string, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompareOrdinal2(string? strA, Number indexA, string? strB, Number indexB, Number length);

    ///<summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
    ///<param name="value">An object that evaluates to a <see cref="T:System.String" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.String" />.</exception>
    ///<returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="value" /> parameter.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="value" />.-or-<paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("string.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompareTo(String instance, Object? value);

    ///<summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified string.</summary>
    ///<param name="strB">The string to compare with this instance.</param>
    ///<returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="strB" /> parameter.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="strB" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="strB" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="strB" />.-or-<paramref name="strB" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("string.CompareTo(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringCompareTo2(String instance, string? strB);

    ///<summary>Determines whether the end of this string instance matches the specified string.</summary>
    ///<param name="value">The string to compare to the substring at the end of this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the end of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.EndsWith(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEndsWith(String instance, string value);

    ///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
    ///<param name="value">The string to compare to the substring at the end of this instance.</param>
    ///<param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter matches the end of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.EndsWith(string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEndsWith2(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
    ///<param name="value">The string to compare to the substring at the end of this instance.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">Cultural information that determines how this instance and <paramref name="value" /> are compared. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter matches the end of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.EndsWith(string, bool, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEndsWith3(String instance, string value, bool ignoreCase, String? culture);

    ///<summary>Determines whether the end of this string instance matches the specified character.</summary>
    ///<param name="value">The character to compare to the character at the end of this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the end of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.EndsWith(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEndsWith4(String instance, Number value);

    ///<summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
    ///<param name="obj">The string to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.String" /> and its value is the same as this instance; otherwise, <see langword="false" />.  If <paramref name="obj" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
    [WhiteList("override string.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEquals(String instance, Object? obj);

    ///<summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
    ///<param name="value">The string to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if the value of the <paramref name="value" /> parameter is the same as the value of this instance; otherwise, <see langword="false" />. If <paramref name="value" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
    [WhiteList("string.Equals(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEquals2(String instance, string? value);

    ///<summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
    ///<param name="value">The string to compare to this instance.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies how the strings will be compared.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if the value of the <paramref name="value" /> parameter is the same as this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.Equals(string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEquals3(String instance, string? value, System.StringComparison comparisonType);

    ///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, <see langword="false" />. If both <paramref name="a" /> and <paramref name="b" /> are <see langword="null" />, the method returns <see langword="true" />.</returns>
    [WhiteList("static string.Equals(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEquals4(string? a, string? b);

    ///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if the value of the <paramref name="a" /> parameter is equal to the value of the <paramref name="b" /> parameter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static string.Equals(string, string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringEquals5(string? a, string? b, System.StringComparison comparisonType);

    ///<summary>Determines whether two specified strings have the same value.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("static string.operator ==(string, string)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool StringOpEquality(string? a, string? b);

    ///<summary>Determines whether two specified strings have different values.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if the value of <paramref name="a" /> is different from the value of <paramref name="b" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("static string.operator !=(string, string)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool StringOpInequality(string? a, string? b);

    ///<summary>Returns the hash code for this string.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("override string.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringGetHashCode(String instance);

    ///<summary>Returns the hash code for this string using the specified rules.</summary>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("string.GetHashCode(System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringGetHashCode2(String instance, System.StringComparison comparisonType);

    ///<summary>Returns the hash code for the provided read-only character span.</summary>
    ///<param name="value">A read-only character span.</param>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("static string.GetHashCode(System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringGetHashCode3(Uint32Array value);

    ///<summary>Returns the hash code for the provided read-only character span using the specified rules.</summary>
    ///<param name="value">A read-only character span.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringGetHashCode4(Uint32Array value, System.StringComparison comparisonType);

    ///<summary>Determines whether the beginning of this string instance matches the specified string.</summary>
    ///<param name="value">The string to compare.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the beginning of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.StartsWith(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringStartsWith(String instance, string value);

    ///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
    ///<param name="value">The string to compare.</param>
    ///<param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if this instance begins with <paramref name="value" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.StartsWith(string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringStartsWith2(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
    ///<param name="value">The string to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">Cultural information that determines how this string and <paramref name="value" /> are compared. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter matches the beginning of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.StartsWith(string, bool, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringStartsWith3(String instance, string value, bool ignoreCase, String? culture);

    ///<summary>Determines whether this string instance starts with the specified character.</summary>
    ///<param name="value">The character to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the beginning of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.StartsWith(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringStartsWith4(String instance, Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified character array.</summary>
    ///<param name="value">An array of Unicode characters.</param>
    [WhiteList("string.String(char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew(char[]? value);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
    ///<param name="value">An array of Unicode characters.</param>
    ///<param name="startIndex">The starting position within <paramref name="value" />.</param>
    ///<param name="length">The number of characters within <paramref name="value" /> to use.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the number of elements in <paramref name="value" />.</exception>
    [WhiteList("string.String(char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew2(char[] value, Number startIndex, Number length);

    /*
    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of Unicode characters.</summary>
    ///<param name="value">A pointer to a null-terminated array of Unicode characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current process does not have read access to all the addressed characters.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> specifies an array that contains an invalid Unicode character, or <paramref name="value" /> specifies an address less than 64000.</exception>
    [WhiteList("string.String(char*)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew3(char* value);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of Unicode characters, a starting character position within that array, and a length.</summary>
    ///<param name="value">A pointer to an array of Unicode characters.</param>
    ///<param name="startIndex">The starting position within <paramref name="value" />.</param>
    ///<param name="length">The number of characters within <paramref name="value" /> to use.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero, <paramref name="value" /> + <paramref name="startIndex" /> cause a pointer overflow, or the current process does not have read access to all the addressed characters.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> specifies an array that contains an invalid Unicode character, or <paramref name="value" /> + <paramref name="startIndex" /> specifies an address less than 64000.</exception>
    [WhiteList("string.String(char*, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew4(char* value, Number startIndex, Number length);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a pointer to an array of 8-bit signed integers.</summary>
    ///<param name="value">A pointer to a null-terminated array of 8-bit signed integers. The integers are interpreted using the current system code page encoding on Windows (referred to as CP_ACP) and as UTF-8 encoding on non-Windows.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded in ANSI.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the new string to initialize, which is determined by the null termination character of <paramref name="value" />, is too large to allocate.</exception>
    ///<exception cref="T:System.AccessViolationException">  <paramref name="value" /> specifies an invalid address.</exception>
    [WhiteList("string.String(sbyte*)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew5(sbyte* value);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of 8-bit signed integers, a starting position within that array, and a length.</summary>
    ///<param name="value">A pointer to an array of 8-bit signed integers. The integers are interpreted using the current system code page encoding on Windows (referred to as CP_ACP) and as UTF-8 encoding on non-Windows.</param>
    ///<param name="startIndex">The starting position within <paramref name="value" />.</param>
    ///<param name="length">The number of characters within <paramref name="value" /> to use.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is too large for the current platform; that is, the address calculation overflowed.-or-The length of the new string to initialize is too large to allocate.</exception>
    ///<exception cref="T:System.ArgumentException">The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is less than 64K.-or-A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded in ANSI.</exception>
    ///<exception cref="T:System.AccessViolationException">  <paramref name="value" />, <paramref name="startIndex" />, and <paramref name="length" /> collectively specify an invalid address.</exception>
    [WhiteList("string.String(sbyte*, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew6(sbyte* value, Number startIndex, Number length);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of 8-bit signed integers, a starting position within that array, a length, and an <see cref="T:System.Text.Encoding" /> object.</summary>
    ///<param name="value">A pointer to an array of 8-bit signed integers.</param>
    ///<param name="startIndex">The starting position within <paramref name="value" />.</param>
    ///<param name="length">The number of characters within <paramref name="value" /> to use.</param>
    ///<param name="enc">An object that specifies how the array referenced by <paramref name="value" /> is encoded. If <paramref name="enc" /> is <see langword="null" />, ANSI encoding is assumed.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is too large for the current platform; that is, the address calculation overflowed.-or-The length of the new string to initialize is too large to allocate.</exception>
    ///<exception cref="T:System.ArgumentException">The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is less than 64K.-or-A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded as specified by <paramref name="enc" />.</exception>
    ///<exception cref="T:System.AccessViolationException">  <paramref name="value" />, <paramref name="startIndex" />, and <paramref name="length" /> collectively specify an invalid address.</exception>
    [WhiteList("string.String(sbyte*, int, int, System.Text.Encoding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew7(sbyte* value, Number startIndex, Number length, System.Text.Encoding enc);
    */

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
    ///<param name="c">A Unicode character.</param>
    ///<param name="count">The number of times <paramref name="c" /> occurs.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is less than zero.</exception>
    [WhiteList("string.String(char, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew8(Number c, Number count);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified read-only span.</summary>
    ///<param name="value">A read-only span of Unicode characters.</param>
    [WhiteList("string.String(System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String StringNew9(Uint32Array value);

    ///<summary>Creates a new string with a specific length and initializes it after creation by using the specified callback.</summary>
    ///<param name="length">The length of the string to create.</param>
    ///<param name="state">The element to pass to <paramref name="action" />.</param>
    ///<param name="action">A callback to initialize the string.</param>
    ///<typeparam name="TState">The type of the element to pass to <paramref name="action" />.</typeparam>
    ///<returns>The created string.</returns>
    [WhiteList("static string.Create<TState>(int, TState, System.Buffers.SpanAction<char, TState>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringCreate<TState>(Number length, TState state, System.Buffers.SpanAction<char, TState> action);

    /*
    ///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="handler">The interpolated string, passed by reference.</param>
    ///<returns>The string that results for formatting the interpolated string using the specified format provider.</returns>
    [WhiteList("static string.Create(System.IFormatProvider, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringCreate2(Intl.NumberFormat? provider, RefValue<System.Runtime.CompilerServices.DefaultInterpolatedStringHandler> handler);

    ///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="initialBuffer">The initial buffer that may be used as temporary space as part of the formatting operation. The contents of this buffer may be overwritten.</param>
    ///<param name="handler">The interpolated string, passed by reference.</param>
    ///<returns>The string that results for formatting the interpolated string using the specified format provider.</returns>
    [WhiteList("static string.Create(System.IFormatProvider, System.Span<char>, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringCreate3(Intl.NumberFormat? provider, Uint32Array initialBuffer, RefValue<System.Runtime.CompilerServices.DefaultInterpolatedStringHandler> handler);
    */

    ///<summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
    ///<returns>This instance of <see cref="T:System.String" />.</returns>
    [WhiteList("string.Clone()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Object StringClone(String instance);

    ///<summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
    ///<param name="str">The string to copy.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="str" /> is <see langword="null" />.</exception>
    ///<returns>A new string with the same value as <paramref name="str" />.</returns>
    [WhiteList("static string.Copy(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringCopy(string str);

    ///<summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
    ///<param name="sourceIndex">The index of the first character in this instance to copy.</param>
    ///<param name="destination">An array of Unicode characters to which characters in this instance are copied.</param>
    ///<param name="destinationIndex">The index in <paramref name="destination" /> at which the copy operation begins.</param>
    ///<param name="count">The number of characters in this instance to copy to <paramref name="destination" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="destination" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" /> is negative-or-<paramref name="sourceIndex" /> does not identify a position in the current instance.-or-<paramref name="destinationIndex" /> does not identify a valid index in the <paramref name="destination" /> array.-or-<paramref name="count" /> is greater than the length of the substring from <paramref name="sourceIndex" /> to the end of this instance-or-<paramref name="count" /> is greater than the length of the subarray from <paramref name="destinationIndex" /> to the end of the <paramref name="destination" /> array.</exception>
    [WhiteList("string.CopyTo(int, char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringCopyTo(String instance, Number sourceIndex, char[] destination, Number destinationIndex, Number count);

    ///<summary>Copies the contents of this string into the destination span.</summary>
    ///<param name="destination">The span into which to copy this string's contents.</param>
    ///<exception cref="T:System.ArgumentException">The destination span is shorter than the source string.</exception>
    [WhiteList("string.CopyTo(System.Span<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringCopyTo2(String instance, Uint32Array destination);

    ///<summary>Copies the contents of this string into the destination span.</summary>
    ///<param name="destination">The span into which to copy this string's contents.</param>
    ///<returns>  <see langword="true" /> if the data was copied; <see langword="false" /> if the destination was too short to fit the contents of the string.</returns>
    [WhiteList("string.TryCopyTo(System.Span<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringTryCopyTo(String instance, Uint32Array destination);

    ///<summary>Copies the characters in this instance to a Unicode character array.</summary>
    ///<returns>A Unicode character array whose elements are the individual characters of this instance. If this instance is an empty string, the returned array is empty and has a zero length.</returns>
    [WhiteList("string.ToCharArray()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static char[] StringToCharArray(String instance);

    ///<summary>Copies the characters in a specified substring in this instance to a Unicode character array.</summary>
    ///<param name="startIndex">The starting position of a substring in this instance.</param>
    ///<param name="length">The length of the substring in this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-<paramref name="startIndex" /> plus <paramref name="length" /> is greater than the length of this instance.</exception>
    ///<returns>A Unicode character array whose elements are the <paramref name="length" /> number of characters in this instance starting from character position <paramref name="startIndex" />.</returns>
    [WhiteList("string.ToCharArray(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static char[] StringToCharArray2(String instance, Number startIndex, Number length);

    ///<summary>Indicates whether the specified string is <see langword="null" /> or an empty string ("").</summary>
    ///<param name="value">The string to test.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter is <see langword="null" /> or an empty string (""); otherwise, <see langword="false" />.</returns>
    [WhiteList("static string.IsNullOrEmpty(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringIsNullOrEmpty(string? value);

    ///<summary>Indicates whether a specified string is <see langword="null" />, empty, or consists only of white-space characters.</summary>
    ///<param name="value">The string to test.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, or if <paramref name="value" /> consists exclusively of white-space characters.</returns>
    [WhiteList("static string.IsNullOrWhiteSpace(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringIsNullOrWhiteSpace(string? value);

    ///<summary>Returns a reference to the element of the string at index zero.This method is intended to support .NET compilers and is not intended to be called by user code.</summary>
    ///<exception cref="T:System.NullReferenceException">The string is null.</exception>
    ///<returns>A reference to the first character in the string, or a reference to the string's null terminator if the string is empty.</returns>
    [WhiteList("string.GetPinnableReference()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringGetPinnableReference(String instance);

    ///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
    ///<returns>The current string.</returns>
    [WhiteList("override string.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToString(String instance);

    ///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
    ///<param name="provider">(Reserved) An object that supplies culture-specific formatting information.</param>
    ///<returns>The current string.</returns>
    [WhiteList("string.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToString2(String instance, Intl.NumberFormat? provider);

    ///<summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
    ///<returns>An enumerator object.</returns>
    [WhiteList("string.GetEnumerator()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.CharEnumerator StringGetEnumerator(String instance);

    ///<summary>Returns an enumeration of <see cref="T:System.Text.Rune" /> from this string.</summary>
    ///<returns>A string rune enumerator.</returns>
    [WhiteList("string.EnumerateRunes()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringRuneEnumerator StringEnumerateRunes(String instance);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for the <see cref="T:System.String" /> class.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.String" />.</returns>
    [WhiteList("string.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode StringGetTypeCode(String instance);

    ///<summary>Indicates whether this string is in Unicode normalization form C.</summary>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>  <see langword="true" /> if this string is in normalization form C; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.IsNormalized()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringIsNormalized(String instance);

    ///<summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
    ///<param name="normalizationForm">A Unicode normalization form.</param>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>  <see langword="true" /> if this string is in the normalization form specified by the <paramref name="normalizationForm" /> parameter; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.IsNormalized(System.Text.NormalizationForm)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringIsNormalized2(String instance, System.Text.NormalizationForm normalizationForm);

    ///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>A new, normalized string whose textual value is the same as this string, but whose binary representation is in normalization form C.</returns>
    [WhiteList("string.Normalize()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringNormalize(String instance);

    ///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
    ///<param name="normalizationForm">A Unicode normalization form.</param>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>A new string whose textual value is the same as this string, but whose binary representation is in the normalization form specified by the <paramref name="normalizationForm" /> parameter.</returns>
    [WhiteList("string.Normalize(System.Text.NormalizationForm)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringNormalize2(String instance, System.Text.NormalizationForm normalizationForm);

    [WhiteList("string.this[int].get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringThis(String instance, Number index);

    [WhiteList("string.Length.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringGetLength(String instance);

    ///<summary>Creates the string  representation of a specified object.</summary>
    ///<param name="arg0">The object to represent, or <see langword="null" />.</param>
    ///<returns>The string representation of the value of <paramref name="arg0" />, or <see cref="F:System.String.Empty" /> if <paramref name="arg0" /> is <see langword="null" />.</returns>
    [WhiteList("static string.Concat(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat(Object? arg0);

    ///<summary>Concatenates the string representations of two specified objects.</summary>
    ///<param name="arg0">The first object to concatenate.</param>
    ///<param name="arg1">The second object to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
    [WhiteList("static string.Concat(object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat2(Object? arg0, Object? arg1);

    ///<summary>Concatenates the string representations of three specified objects.</summary>
    ///<param name="arg0">The first object to concatenate.</param>
    ///<param name="arg1">The second object to concatenate.</param>
    ///<param name="arg2">The third object to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
    [WhiteList("static string.Concat(object, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat3(Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
    ///<param name="args">An object array that contains the elements to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>The concatenated string representations of the values of the elements in <paramref name="args" />.</returns>
    [WhiteList("static string.Concat(params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat4(params object?[] args);

    ///<summary>Concatenates the string representations of the elements in a specified span of objects.</summary>
    ///<param name="args">A span of objects that contains the elements to concatenate.</param>
    ///<returns>The concatenated string representations of the values of the elements in <paramref name="args" />.</returns>
    [WhiteList("static string.Concat(params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat5(params System.ReadOnlySpan<object?> args);

    ///<summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
    ///<param name="values">A collection object that implements the <see cref="T:System.Collections.Generic.IEnumerable`1" /> interface.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<returns>The concatenated members in <paramref name="values" />.</returns>
    [WhiteList("static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat6<T>(IEnumerable<T> values);

    ///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
    ///<param name="values">A collection object that implements <see cref="T:System.Collections.Generic.IEnumerable`1" /> and whose generic type argument is <see cref="T:System.String" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<returns>The concatenated strings in <paramref name="values" />, or <see cref="F:System.String.Empty" /> if <paramref name="values" /> is an empty <see langword="IEnumerable(Of String)" />.</returns>
    [WhiteList("static string.Concat(System.Collections.Generic.IEnumerable<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat7(System.Collections.Generic.IEnumerable<string?> values);

    ///<summary>Concatenates two specified instances of <see cref="T:System.String" />.</summary>
    ///<param name="str0">The first string to concatenate.</param>
    ///<param name="str1">The second string to concatenate.</param>
    ///<returns>The concatenation of <paramref name="str0" /> and <paramref name="str1" />.</returns>
    [WhiteList("static string.Concat(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat8(string? str0, string? str1);

    ///<summary>Concatenates three specified instances of <see cref="T:System.String" />.</summary>
    ///<param name="str0">The first string to concatenate.</param>
    ///<param name="str1">The second string to concatenate.</param>
    ///<param name="str2">The third string to concatenate.</param>
    ///<returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, and <paramref name="str2" />.</returns>
    [WhiteList("static string.Concat(string, string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat9(string? str0, string? str1, string? str2);

    ///<summary>Concatenates four specified instances of <see cref="T:System.String" />.</summary>
    ///<param name="str0">The first string to concatenate.</param>
    ///<param name="str1">The second string to concatenate.</param>
    ///<param name="str2">The third string to concatenate.</param>
    ///<param name="str3">The fourth string to concatenate.</param>
    ///<returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, <paramref name="str2" />, and <paramref name="str3" />.</returns>
    [WhiteList("static string.Concat(string, string, string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat10(string? str0, string? str1, string? str2, string? str3);

    ///<summary>Concatenates the string representations of two specified read-only character spans.</summary>
    ///<param name="str0">The first read-only character span to concatenate.</param>
    ///<param name="str1">The second read-only character span to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="str0" /> and <paramref name="str1" />.</returns>
    [WhiteList("static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat11(Uint32Array str0, Uint32Array str1);

    ///<summary>Concatenates the string representations of three specified read-only character spans.</summary>
    ///<param name="str0">The first read-only character span to concatenate.</param>
    ///<param name="str1">The second read-only character span to concatenate.</param>
    ///<param name="str2">The third read-only character span to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="str0" />, <paramref name="str1" /> and <paramref name="str2" />.</returns>
    [WhiteList("static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat12(Uint32Array str0, Uint32Array str1, Uint32Array str2);

    ///<summary>Concatenates the string representations of four specified read-only character spans.</summary>
    ///<param name="str0">The first read-only character span to concatenate.</param>
    ///<param name="str1">The second read-only character span to concatenate.</param>
    ///<param name="str2">The third read-only character span to concatenate.</param>
    ///<param name="str3">The fourth read-only character span to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="str0" />, <paramref name="str1" />, <paramref name="str2" /> and <paramref name="str3" />.</returns>
    [WhiteList("static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat13(Uint32Array str0, Uint32Array str1, Uint32Array str2, Uint32Array str3);

    ///<summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
    ///<param name="values">An array of string instances.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>The concatenated elements of <paramref name="values" />.</returns>
    [WhiteList("static string.Concat(params string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat14(params string?[] values);

    ///<summary>Concatenates the elements of a specified span of <see cref="T:System.String" />.</summary>
    ///<param name="values">A span of <see cref="T:System.String" /> instances.</param>
    ///<returns>The concatenated elements of <paramref name="values" />.</returns>
    [WhiteList("static string.Concat(params System.ReadOnlySpan<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringConcat15(params System.ReadOnlySpan<string?> values);

    ///<summary>Replaces one or more format items in a string with the string representation of a specified object.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The format item in <paramref name="format" /> is invalid.-or-The index of a format item is not zero.</exception>
    ///<returns>A copy of <paramref name="format" /> in which any format items are replaced by the string representation of <paramref name="arg0" />.</returns>
    [WhiteList("static string.Format(string, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat(string format, Object? arg0);

    ///<summary>Replaces the format items in a string with the string representation of two specified objects.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is not zero or one.</exception>
    ///<returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
    [WhiteList("static string.Format(string, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat2(string format, Object? arg0, Object? arg1);

    ///<summary>Replaces the format items in a string with the string representation of three specified objects.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than two.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
    [WhiteList("static string.Format(string, object, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat3(string format, Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object array that contains zero or more objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("static string.Format(string, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat4(string format, params object?[] args);

    ///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified span.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object span that contains zero or more objects to format.</param>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("static string.Format(string, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat5(string format, params System.ReadOnlySpan<object?> args);

    ///<summary>Replaces the format item or items in a specified string with the string representation of the corresponding object. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is not zero.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format item or items have been replaced by the string representation of <paramref name="arg0" />.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, string, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat6(Intl.NumberFormat? provider, string format, Object? arg0);

    ///<summary>Replaces the format items in a string with the string representation of two specified objects. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is not zero or one.</exception>
    ///<returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, string, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat7(Intl.NumberFormat? provider, string format, Object? arg0, Object? arg1);

    ///<summary>Replaces the format items in a string with the string representation of three specified objects. An parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than two.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, string, object, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat8(Intl.NumberFormat? provider, string format, Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified array. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object array that contains zero or more objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, string, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat9(Intl.NumberFormat? provider, string format, params object?[] args);

    ///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified span. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object span that contains zero or more objects to format.</param>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, string, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat10(Intl.NumberFormat? provider, string format, params System.ReadOnlySpan<object?> args);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("static string.Format<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat11<TArg0>(Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<typeparam name="TArg1">The type of the second object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("static string.Format<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat12<TArg0, TArg1>(Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0, TArg1 arg1);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<typeparam name="TArg1">The type of the second object to format.</typeparam>
    ///<typeparam name="TArg2">The type of the third object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("static string.Format<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat13<TArg0, TArg1, TArg2>(Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="args">An array of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat14(Intl.NumberFormat? provider, System.Text.CompositeFormat format, params object?[] args);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="args">A span of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringFormat15(Intl.NumberFormat? provider, System.Text.CompositeFormat format, params System.ReadOnlySpan<object?> args);

    ///<summary>Returns a new string in which a specified string is inserted at a specified index position in this instance.</summary>
    ///<param name="startIndex">The zero-based index position of the insertion.</param>
    ///<param name="value">The string to insert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is negative or greater than the length of this instance.</exception>
    ///<returns>A new string that is equivalent to this instance, but with <paramref name="value" /> inserted at position <paramref name="startIndex" />.</returns>
    [WhiteList("string.Insert(int, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringInsert(String instance, Number startIndex, string value);

    ///<summary>Concatenates an array of strings, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">An array of strings to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="value" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("static string.Join(char, params string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin(Number separator, params string?[] value);

    ///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">A span that contains the elements to concatenate.</param>
    ///<returns>A string that consists of the elements of <paramref name="value" /> delimited by the <paramref name="separator" /> string. -or- <see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("static string.Join(char, params System.ReadOnlySpan<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin2(Number separator, params System.ReadOnlySpan<string?> value);

    ///<summary>Concatenates all the elements of a string array, using the specified separator between each element.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">An array that contains the elements to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements in <paramref name="value" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("static string.Join(string, params string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin3(string? separator, params string?[] value);

    ///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">A span that contains the elements to concatenate.</param>
    ///<returns>A string that consists of the elements of <paramref name="value" /> delimited by the <paramref name="separator" /> string. -or- <see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("static string.Join(string, params System.ReadOnlySpan<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin4(string? separator, params System.ReadOnlySpan<string?> value);

    ///<summary>Concatenates an array of strings, using the specified separator between each member, starting with the element in <paramref name="value" /> located at the <paramref name="startIndex" /> position, and concatenating up to <paramref name="count" /> elements.</summary>
    ///<param name="separator">Concatenates an array of strings, using the specified separator between each member, starting with the element located at the specified index and including a specified number of elements.</param>
    ///<param name="value">An array of strings to concatenate.</param>
    ///<param name="startIndex">The first item in <paramref name="value" /> to concatenate.</param>
    ///<param name="count">The number of elements from <paramref name="value" /> to concatenate, starting with the element in the <paramref name="startIndex" /> position.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="count" /> are negative.-or-<paramref name="startIndex" /> is greater than the length of <paramref name="value" />  - <paramref name="count" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of <paramref name="count" /> elements of <paramref name="value" /> starting at <paramref name="startIndex" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="count" /> is zero.</returns>
    [WhiteList("static string.Join(char, string[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin5(Number separator, string?[] value, Number startIndex, Number count);

    ///<summary>Concatenates the specified elements of a string array, using the specified separator between each element.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">An array that contains the elements to concatenate.</param>
    ///<param name="startIndex">The first element in <paramref name="value" /> to use.</param>
    ///<param name="count">The number of elements of <paramref name="value" /> to use.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="count" /> is less than 0.-or-<paramref name="startIndex" /> plus <paramref name="count" /> is greater than the number of elements in <paramref name="value" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>A string that consists of <paramref name="count" /> elements of <paramref name="value" /> starting at <paramref name="startIndex" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="count" /> is zero.</returns>
    [WhiteList("static string.Join(string, string[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin6(string? separator, string?[] value, Number startIndex, Number count);

    ///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator.<paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the strings to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("static string.Join(string, System.Collections.Generic.IEnumerable<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin7(string? separator, System.Collections.Generic.IEnumerable<string?> values);

    ///<summary>Concatenates the string representations of an array of objects, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array of objects whose string representations will be concatenated.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("static string.Join(char, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin8(Number separator, params object?[] values);

    ///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if value has more than one element.</param>
    ///<param name="values">A span of objects whose string representations will be concatenated.</param>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> character. -or- <see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("static string.Join(char, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin9(Number separator, params System.ReadOnlySpan<object?> values);

    ///<summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array that contains the elements to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.-or-.NET Framework only: <see cref="F:System.String.Empty" /> if the first element of <paramref name="values" /> is <see langword="null" />.</returns>
    [WhiteList("static string.Join(string, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin10(string? separator, params object?[] values);

    ///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A span of objects whose string representations will be concatenated.</param>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string. -or- <see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("static string.Join(string, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin11(string? separator, params System.ReadOnlySpan<object?> values);

    ///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the objects to concatenate.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has no elements.</returns>
    [WhiteList("static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin12<T>(Number separator, IEnumerable<T> values);

    ///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the objects to concatenate.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has no elements.</returns>
    [WhiteList("static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringJoin13<T>(string? separator, IEnumerable<T> values);

    ///<summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("string.PadLeft(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringPadLeft(String instance, Number totalWidth);

    ///<summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<param name="paddingChar">A Unicode padding character.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("string.PadLeft(int, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringPadLeft2(String instance, Number totalWidth, Number paddingChar);

    ///<summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("string.PadRight(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringPadRight(String instance, Number totalWidth);

    ///<summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<param name="paddingChar">A Unicode padding character.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("string.PadRight(int, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringPadRight2(String instance, Number totalWidth, Number paddingChar);

    ///<summary>Returns a new string in which a specified number of characters in the current instance beginning at a specified position have been deleted.</summary>
    ///<param name="startIndex">The zero-based position to begin deleting characters.</param>
    ///<param name="count">The number of characters to delete.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> plus <paramref name="count" /> specify a position outside this instance.</exception>
    ///<returns>A new string that is equivalent to this instance except for the removed characters.</returns>
    [WhiteList("string.Remove(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringRemove(String instance, Number startIndex, Number count);

    ///<summary>Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.</summary>
    ///<param name="startIndex">The zero-based position to begin deleting characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than zero.-or-<paramref name="startIndex" /> specifies a position that is not within this string.</exception>
    ///<returns>A new string that is equivalent to this string except for the removed characters.</returns>
    [WhiteList("string.Remove(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringRemove2(String instance, Number startIndex);

    ///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided culture and case sensitivity.</summary>
    ///<param name="oldValue">The string to be replaced.</param>
    ///<param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore casing when comparing; <see langword="false" /> otherwise.</param>
    ///<param name="culture">The culture to use when comparing. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="oldValue" /> is the empty string ("").</exception>
    ///<returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Replace(string, string, bool, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringReplace(String instance, string oldValue, string? newValue, bool ignoreCase, String? culture);

    ///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.</summary>
    ///<param name="oldValue">The string to be replaced.</param>
    ///<param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
    ///<param name="comparisonType">One of the enumeration values that determines how <paramref name="oldValue" /> is searched within this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="oldValue" /> is the empty string ("").</exception>
    ///<returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Replace(string, string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringReplace2(String instance, string oldValue, string? newValue, System.StringComparison comparisonType);

    ///<summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
    ///<param name="oldChar">The Unicode character to be replaced.</param>
    ///<param name="newChar">The Unicode character to replace all occurrences of <paramref name="oldChar" />.</param>
    ///<returns>A string that is equivalent to this instance except that all instances of <paramref name="oldChar" /> are replaced with <paramref name="newChar" />. If <paramref name="oldChar" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Replace(char, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringReplace3(String instance, Number oldChar, Number newChar);

    ///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.</summary>
    ///<param name="oldValue">The string to be replaced.</param>
    ///<param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="oldValue" /> is the empty string ("").</exception>
    ///<returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Replace(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringReplace4(String instance, string oldValue, string? newValue);

    ///<summary>Replaces all newline sequences in the current string with <see cref="P:System.Environment.NewLine" />.</summary>
    ///<returns>A string whose contents match the current string, but with all newline sequences replaced with <see cref="P:System.Environment.NewLine" />.</returns>
    [WhiteList("string.ReplaceLineEndings()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringReplaceLineEndings(String instance);

    ///<summary>Replaces all newline sequences in the current string with <paramref name="replacementText" />.</summary>
    ///<param name="replacementText">The text to use as replacement.</param>
    ///<returns>A string whose contents match the current string, but with all newline sequences replaced with <paramref name="replacementText" />.</returns>
    [WhiteList("string.ReplaceLineEndings(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringReplaceLineEndings2(String instance, string replacementText);

    ///<summary>Splits a string into substrings based on a specified delimiting character and, optionally, options.</summary>
    ///<param name="separator">A character that delimits the substrings in this string.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("string.Split(char, System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit(String instance, Number separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on a specified delimiting character and, optionally, options.        Splits a string into a maximum number of substrings based on the provided character separator, optionally omitting empty substrings from the result.</summary>
    ///<param name="separator">A character that delimits the substrings in this instance.</param>
    ///<param name="count">The maximum number of elements expected in the array.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array that contains at most <paramref name="count" /> substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("string.Split(char, int, System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit2(String instance, Number separator, Number count, System.StringSplitOptions options);

    ///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
    ///<param name="separator">An array of delimiting characters, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("string.Split(params char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit3(String instance, params char[]? separator);

    ///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
    ///<param name="separator">A span of delimiting characters, or an empty span that contains no delimiters.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by one or more characters in <paramref name="separator" />.</returns>
    [WhiteList("string.Split(params System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit4(String instance, params Array<uint> separator);

    ///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters.</summary>
    ///<param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="count">The maximum number of substrings to return.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.</exception>
    ///<returns>An array whose elements contain the substrings in this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("string.Split(char[], int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit5(String instance, char[]? separator, Number count);

    ///<summary>Splits a string into substrings based on specified delimiting characters and options.</summary>
    ///<param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("string.Split(char[], System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit6(String instance, char[]? separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters and, optionally, options.</summary>
    ///<param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="count">The maximum number of substrings to return.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array that contains the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("string.Split(char[], int, System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit7(String instance, char[]? separator, Number count, System.StringSplitOptions options);

    ///<summary>Splits a string into substrings that are based on the provided string separator.</summary>
    ///<param name="separator">A string that delimits the substrings in this string.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("string.Split(string, System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit8(String instance, string? separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on a specified delimiting string and, optionally, options.</summary>
    ///<param name="separator">A string that delimits the substrings in this instance.</param>
    ///<param name="count">The maximum number of elements expected in the array.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array that contains at most <paramref name="count" /> substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("string.Split(string, int, System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit9(String instance, string? separator, Number count, System.StringSplitOptions options);

    ///<summary>Splits a string into substrings based on a specified delimiting string and, optionally, options.</summary>
    ///<param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("string.Split(string[], System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit10(String instance, string[]? separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on specified delimiting strings and, optionally, options.</summary>
    ///<param name="separator">The strings that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="count">The maximum number of substrings to return.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("string.Split(string[], int, System.StringSplitOptions)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] StringSplit11(String instance, string[]? separator, Number count, System.StringSplitOptions options);

    ///<summary>Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.</summary>
    ///<param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than zero or greater than the length of this instance.</exception>
    ///<returns>A string that is equivalent to the substring that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance.</returns>
    [WhiteList("string.Substring(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringSubstring(String instance, Number startIndex);

    ///<summary>Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.</summary>
    ///<param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
    ///<param name="length">The number of characters in the substring.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> plus <paramref name="length" /> indicates a position not within this instance.-or-<paramref name="startIndex" /> or <paramref name="length" /> is less than zero.</exception>
    ///<returns>A string that is equivalent to the substring of length <paramref name="length" /> that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance and <paramref name="length" /> is zero.</returns>
    [WhiteList("string.Substring(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringSubstring2(String instance, Number startIndex, Number length);

    ///<summary>Returns a copy of this string converted to lowercase.</summary>
    ///<returns>A string in lowercase.</returns>
    [WhiteList("string.ToLower()")]
	[ECMAScriptLiteral("1n", false)]
	public extern static string StringToLower(String instance);

    ///<summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
    ///<param name="culture">An object that supplies culture-specific casing rules. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<returns>The lowercase equivalent of the current string.</returns>
    [WhiteList("string.ToLower(System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToLower2(String instance, String? culture);

    ///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to lowercase using the casing rules of the invariant culture.</summary>
    ///<returns>The lowercase equivalent of the current string.</returns>
    [WhiteList("string.ToLowerInvariant()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToLowerInvariant(String instance);

    ///<summary>Returns a copy of this string converted to uppercase.</summary>
    ///<returns>The uppercase equivalent of the current string.</returns>
    [WhiteList("string.ToUpper()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToUpper(String instance);

    ///<summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
    ///<param name="culture">An object that supplies culture-specific casing rules. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<returns>The uppercase equivalent of the current string.</returns>
    [WhiteList("string.ToUpper(System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToUpper2(String instance, String? culture);

    ///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to uppercase using the casing rules of the invariant culture.</summary>
    ///<returns>The uppercase equivalent of the current string.</returns>
    [WhiteList("string.ToUpperInvariant()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringToUpperInvariant(String instance);

    ///<summary>Removes all leading and trailing white-space characters from the current string.</summary>
    ///<returns>The string that remains after all white-space characters are removed from the start and end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Trim()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrim(String instance);

    ///<summary>Removes all leading and trailing instances of a character from the current string.</summary>
    ///<param name="trimChar">A Unicode character to remove.</param>
    ///<returns>The string that remains after all instances of the <paramref name="trimChar" /> character are removed from the start and end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Trim(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrim2(String instance, Number trimChar);

    ///<summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current string.</summary>
    ///<param name="trimChars">An array of Unicode characters to remove, or <see langword="null" />.</param>
    ///<returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the start and end of the current string. If <paramref name="trimChars" /> is <see langword="null" /> or an empty array, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Trim(params char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrim3(String instance, params char[]? trimChars);

    ///<summary>Removes all leading and trailing occurrences of a set of characters specified in a span from the current string.</summary>
    ///<param name="trimChars">A span of Unicode characters to remove.</param>
    ///<returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the start and end of the current string. If <paramref name="trimChars" /> is empty, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.Trim(params System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrim4(String instance, params Array<UInt32> trimChars);

    ///<summary>Removes all the leading white-space characters from the current string.</summary>
    ///<returns>The string that remains after all white-space characters are removed from the start of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimStart()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimStart(String instance);

    ///<summary>Removes all the leading occurrences of a specified character from the current string.</summary>
    ///<param name="trimChar">The Unicode character to remove.</param>
    ///<returns>The string that remains after all occurrences of the <paramref name="trimChar" /> character are removed from the start of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimStart(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimStart2(String instance, Number trimChar);

    ///<summary>Removes all the leading occurrences of a set of characters specified in an array from the current string.</summary>
    ///<param name="trimChars">An array of Unicode characters to remove, or <see langword="null" />.</param>
    ///<returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the start of the current string. If <paramref name="trimChars" /> is <see langword="null" /> or an empty array, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimStart(params char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimStart3(String instance, params char[]? trimChars);

    ///<summary>Removes all the leading occurrences of a set of characters specified in a span from the current string.</summary>
    ///<param name="trimChars">A span of Unicode characters to remove.</param>
    ///<returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the start of the current string. If <paramref name="trimChars" /> is empty, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimStart(params System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimStart4(String instance, params Array<UInt32> trimChars);

    ///<summary>Removes all the trailing white-space characters from the current string.</summary>
    ///<returns>The string that remains after all white-space characters are removed from the end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimEnd()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimEnd(String instance);

    ///<summary>Removes all the trailing occurrences of a character from the current string.</summary>
    ///<param name="trimChar">A Unicode character to remove.</param>
    ///<returns>The string that remains after all occurrences of the <paramref name="trimChar" /> character are removed from the end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimEnd(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimEnd2(String instance, Number trimChar);

    ///<summary>Removes all the trailing occurrences of a set of characters specified in an array from the current string.</summary>
    ///<param name="trimChars">An array of Unicode characters to remove, or <see langword="null" />.</param>
    ///<returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the end of the current string. If <paramref name="trimChars" /> is <see langword="null" /> or an empty array, Unicode white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimEnd(params char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimEnd3(String instance, params char[]? trimChars);

    ///<summary>Removes all the trailing occurrences of a set of characters specified in a span from the current string.</summary>
    ///<param name="trimChars">A span of Unicode characters to remove.</param>
    ///<returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the end of the current string. If <paramref name="trimChars" /> is empty, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("string.TrimEnd(params System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringTrimEnd4(String instance, params Array<UInt32> trimChars);

    ///<summary>Returns a value indicating whether a specified substring occurs within this string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.</returns>
    [WhiteList("string.Contains(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringContains(String instance, string value);

    ///<summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.</returns>
    [WhiteList("string.Contains(string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringContains2(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Returns a value indicating whether a specified character occurs within this string.</summary>
    ///<param name="value">The character to seek.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.Contains(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringContains3(String instance, Number value);

    ///<summary>Returns a value indicating whether a specified character occurs within this string, using the specified comparison rules.</summary>
    ///<param name="value">The character to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("string.Contains(char, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringContains4(String instance, Number value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string.</summary>
    ///<param name="value">A Unicode character to seek.</param>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
    [WhiteList("string.IndexOf(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf(String instance, Number value);

    ///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. The search starts at a specified character position.</summary>
    ///<param name="value">A Unicode character to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of the string.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the string if that character is found, or -1 if it is not.</returns>
    [WhiteList("string.IndexOf(char, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf2(String instance, Number value, Number startIndex);

    ///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. A parameter specifies the type of search to use for the specified character.</summary>
    ///<param name="value">The character to seek.</param>
    ///<param name="comparisonType">An enumeration value that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based index of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
    [WhiteList("string.IndexOf(char, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf3(String instance, Number value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
    ///<param name="value">A Unicode character to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the string if that character is found, or -1 if it is not.</returns>
    [WhiteList("string.IndexOf(char, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf4(String instance, Number value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("string.IndexOfAny(char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOfAny(String instance, char[] anyOf);

    ///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the number of characters in this instance.</exception>
    ///<returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("string.IndexOfAny(char[], int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOfAny2(String instance, char[] anyOf, Number startIndex);

    ///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="count" /> + <paramref name="startIndex" /> is greater than the number of characters in this instance.</exception>
    ///<returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("string.IndexOfAny(char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOfAny3(String instance, char[] anyOf, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance.</summary>
    ///<param name="value">The string to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
    [WhiteList("string.IndexOf(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf5(String instance, string value);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("string.IndexOf(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf6(String instance, string value, Number startIndex);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("string.IndexOf(string, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf7(String instance, string value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
    [WhiteList("string.IndexOf(string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf8(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based index position of the <paramref name="value" /> parameter from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("string.IndexOf(string, int, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf9(String instance, string value, Number startIndex, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the length of this instance.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based index position of the <paramref name="value" /> parameter from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("string.IndexOf(string, int, int, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringIndexOf10(String instance, string value, Number startIndex, Number count, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.</summary>
    ///<param name="value">The Unicode character to seek.</param>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
    [WhiteList("string.LastIndexOf(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf(String instance, Number value);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
    ///<param name="value">The Unicode character to seek.</param>
    ///<param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOf(char, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf2(String instance, Number value, Number startIndex);

    ///<summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
    ///<param name="value">The Unicode character to seek.</param>
    ///<param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" /> + 1 is less than zero.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOf(char, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf3(String instance, Number value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("string.LastIndexOfAny(char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOfAny(String instance, char[] anyOf);

    ///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> specifies a position that is not within this instance.</exception>
    ///<returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOfAny(char[], int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOfAny2(String instance, char[] anyOf, Number startIndex);

    ///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> minus <paramref name="count" /> + 1 is less than zero.</exception>
    ///<returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOfAny(char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOfAny3(String instance, char[] anyOf, Number startIndex, Number count);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance.</summary>
    ///<param name="value">The string to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>The zero-based starting index position of <paramref name="value" /> if that string is found, or -1 if it is not.</returns>
    [WhiteList("string.LastIndexOf(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf4(String instance, string value);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance.-or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than -1 or greater than zero.</exception>
    ///<returns>The zero-based starting index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOf(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf5(String instance, string value, Number startIndex);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" />+ 1 specifies a position that is not within this instance.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="startIndex" /> is less than -1 or greater than zero.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="count" /> is greater than 1.</exception>
    ///<returns>The zero-based starting index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOf(string, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf6(String instance, string value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based starting index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not.</returns>
    [WhiteList("string.LastIndexOf(string, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf7(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance.-or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than -1 or greater than zero.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based starting index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOf(string, int, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf8(String instance, string value, Number startIndex, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> + 1 - <paramref name="count" /> specifies a position that is not within this instance.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="startIndex" /> is less than -1 or greater than zero.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="count" /> is greater than 1.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based starting index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("string.LastIndexOf(string, int, int, System.StringComparison)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringLastIndexOf9(String instance, string value, Number startIndex, Number count, System.StringComparison comparisonType);
}
