using System.Collections;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("string","string","string")]
public static class StringModule
{
    ///<summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
    ///<param name="str">A string to search for in the intern pool.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="str" /> is <see langword="null" />.</exception>
    ///<returns>The system's reference to <paramref name="str" />, if it is interned; otherwise, a new reference to a string with the value of <paramref name="str" />.</returns>
    [WhiteList("_1234444e218b96c3","static string.Intern(string)","_1234444e218b96c3")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _1234444e218b96c3(string str);

    ///<summary>Retrieves a reference to a specified <see cref="T:System.String" />.</summary>
    ///<param name="str">The string to search for in the intern pool.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="str" /> is <see langword="null" />.</exception>
    ///<returns>A reference to <paramref name="str" /> if it is in the common language runtime intern pool; otherwise, <see langword="null" />.</returns>
    [WhiteList("_0af8a50f6d6b3e26","static string.IsInterned(string)","_0af8a50f6d6b3e26")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string? _0af8a50f6d6b3e26(string str);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_e16eea9fe3891a62","static string.Compare(string, string)","_e16eea9fe3891a62")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _e16eea9fe3891a62(string? strA, string? strB);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_20874c0b43640318","static string.Compare(string, string, bool)","_20874c0b43640318")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _20874c0b43640318(string? strA, string? strB, bool ignoreCase);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<exception cref="T:System.NotSupportedException">  <see cref="T:System.StringComparison" /> is not supported.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> is in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_9d940114ace1198f","static string.Compare(string, string, System.StringComparison)","_9d940114ace1198f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _9d940114ace1198f(string? strA, string? strB, System.StringComparison comparisonType);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two strings to each other in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="culture">The culture that supplies culture-specific comparison information. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<param name="options">Options to use when performing the comparison (such as ignoring case or symbols).</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between <paramref name="strA" /> and <paramref name="strB" />, as shown in the following table<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_3df4c7373f0b47b6","static string.Compare(string, string, System.Globalization.CultureInfo, System.Globalization.CompareOptions)","_3df4c7373f0b47b6")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _3df4c7373f0b47b6(string? strA, string? strB, String? culture, System.Globalization.CompareOptions options);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">An object that supplies culture-specific comparison information. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> precedes <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description><paramref name="strA" /> occurs in the same position as <paramref name="strB" /> in the sort order.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> follows <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_7349ec2403e9750d","static string.Compare(string, string, bool, System.Globalization.CultureInfo)","_7349ec2403e9750d")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _7349ec2403e9750d(string? strA, string? strB, bool ignoreCase, String? culture);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_27da56ab23a965a9","static string.Compare(string, int, string, int, int)","_27da56ab23a965a9")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _27da56ab23a965a9(string? strA, Number indexA, string? strB, Number indexB, Number length);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The position of the substring within <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The position of the substring within <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is <see langword="null" />, and <paramref name="length" /> is greater than zero.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> precedes the substring in <paramref name="strB" /> in the sort order.</description></item><item><term> Zero</term><description> The substrings occur in the same position in the sort order, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> follows the substring in <paramref name="strB" /> in the sort order.</description></item></list></returns>
    [WhiteList("_ae9588dc995de641","static string.Compare(string, int, string, int, int, bool)","_ae9588dc995de641")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _ae9588dc995de641(string? strA, Number indexA, string? strB, Number indexB, Number length, bool ignoreCase);

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
    [WhiteList("_e926c87c90eaf4a5","static string.Compare(string, int, string, int, int, bool, System.Globalization.CultureInfo)","_e926c87c90eaf4a5")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _e926c87c90eaf4a5(string? strA, Number indexA, string? strB, Number indexB, Number length, bool ignoreCase, String? culture);

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
    [WhiteList("_6de73d4e145d51a4","static string.Compare(string, int, string, int, int, System.Globalization.CultureInfo, System.Globalization.CompareOptions)","_6de73d4e145d51a4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _6de73d4e145d51a4(string? strA, Number indexA, string? strB, Number indexB, Number length, String? culture, System.Globalization.CompareOptions options);

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
    [WhiteList("_d78fb9d76fca75e4","static string.Compare(string, int, string, int, int, System.StringComparison)","_d78fb9d76fca75e4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _d78fb9d76fca75e4(string? strA, Number indexA, string? strB, Number indexB, Number length, System.StringComparison comparisonType);

    ///<summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
    ///<param name="strA">The first string to compare.</param>
    ///<param name="strB">The second string to compare.</param>
    ///<returns>An integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="strA" /> is less than <paramref name="strB" />.</description></item><item><term> Zero</term><description><paramref name="strA" /> and <paramref name="strB" /> are equal.</description></item><item><term> Greater than zero</term><description><paramref name="strA" /> is greater than <paramref name="strB" />.</description></item></list></returns>
    [WhiteList("_a55d307de6e31c7b","static string.CompareOrdinal(string, string)","_a55d307de6e31c7b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _a55d307de6e31c7b(string? strA, string? strB);

    ///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring.</summary>
    ///<param name="strA">The first string to use in the comparison.</param>
    ///<param name="indexA">The starting index of the substring in <paramref name="strA" />.</param>
    ///<param name="strB">The second string to use in the comparison.</param>
    ///<param name="indexB">The starting index of the substring in <paramref name="strB" />.</param>
    ///<param name="length">The maximum number of characters in the substrings to compare.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="strA" /> is not <see langword="null" /> and <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or-<paramref name="strB" /> is not <see langword="null" /> and <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.</exception>
    ///<returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />.</description></item><item><term> Zero</term><description> The substrings are equal, or <paramref name="length" /> is zero.</description></item><item><term> Greater than zero</term><description> The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />.</description></item></list></returns>
    [WhiteList("_dc789454b6ef6bcb","static string.CompareOrdinal(string, int, string, int, int)","_dc789454b6ef6bcb")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _dc789454b6ef6bcb(string? strA, Number indexA, string? strB, Number indexB, Number length);

    ///<summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
    ///<param name="value">An object that evaluates to a <see cref="T:System.String" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.String" />.</exception>
    ///<returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="value" /> parameter.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="value" />.-or-<paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("_629b0613344d82e7","string.CompareTo(object)","_629b0613344d82e7")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _629b0613344d82e7(String instance, Object? value);

    ///<summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified string.</summary>
    ///<param name="strB">The string to compare with this instance.</param>
    ///<returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="strB" /> parameter.<list type="table"><listheader><term> Value</term><description> Condition</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="strB" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="strB" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="strB" />.-or-<paramref name="strB" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("_380e7c7649d703f0","string.CompareTo(string)","_380e7c7649d703f0")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _380e7c7649d703f0(String instance, string? strB);

    ///<summary>Determines whether the end of this string instance matches the specified string.</summary>
    ///<param name="value">The string to compare to the substring at the end of this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the end of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("_33de316681320ec7","string.EndsWith(string)","_33de316681320ec7")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _33de316681320ec7(String instance, string value);

    ///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
    ///<param name="value">The string to compare to the substring at the end of this instance.</param>
    ///<param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter matches the end of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_946b7129a48c8114","string.EndsWith(string, System.StringComparison)","_946b7129a48c8114")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _946b7129a48c8114(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
    ///<param name="value">The string to compare to the substring at the end of this instance.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">Cultural information that determines how this instance and <paramref name="value" /> are compared. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter matches the end of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_679207cac049d3c6","string.EndsWith(string, bool, System.Globalization.CultureInfo)","_679207cac049d3c6")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _679207cac049d3c6(String instance, string value, bool ignoreCase, String? culture);

    ///<summary>Determines whether the end of this string instance matches the specified character.</summary>
    ///<param name="value">The character to compare to the character at the end of this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the end of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("_7619ce4eda48c8e8","string.EndsWith(char)","_7619ce4eda48c8e8")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _7619ce4eda48c8e8(String instance, Number value);

    ///<summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
    ///<param name="obj">The string to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.String" /> and its value is the same as this instance; otherwise, <see langword="false" />.  If <paramref name="obj" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
    [WhiteList("_def18c2802a57249","override string.Equals(object)","_def18c2802a57249")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _def18c2802a57249(String instance, Object? obj);

    ///<summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
    ///<param name="value">The string to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if the value of the <paramref name="value" /> parameter is the same as the value of this instance; otherwise, <see langword="false" />. If <paramref name="value" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
    [WhiteList("_6ee9bc86e4384225","string.Equals(string)","_6ee9bc86e4384225")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _6ee9bc86e4384225(String instance, string? value);

    ///<summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
    ///<param name="value">The string to compare to this instance.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies how the strings will be compared.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if the value of the <paramref name="value" /> parameter is the same as this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_f8e1e01e8c17e8bb","string.Equals(string, System.StringComparison)","_f8e1e01e8c17e8bb")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _f8e1e01e8c17e8bb(String instance, string? value, System.StringComparison comparisonType);

    ///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, <see langword="false" />. If both <paramref name="a" /> and <paramref name="b" /> are <see langword="null" />, the method returns <see langword="true" />.</returns>
    [WhiteList("_e6b1648151c863d5","static string.Equals(string, string)","_e6b1648151c863d5")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _e6b1648151c863d5(string? a, string? b);

    ///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if the value of the <paramref name="a" /> parameter is equal to the value of the <paramref name="b" /> parameter; otherwise, <see langword="false" />.</returns>
    [WhiteList("_b7c36408f0f172e9","static string.Equals(string, string, System.StringComparison)","_b7c36408f0f172e9")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _b7c36408f0f172e9(string? a, string? b, System.StringComparison comparisonType);

    ///<summary>Determines whether two specified strings have the same value.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_ee27dec45b308755","static string.operator ==(string, string)","_ee27dec45b308755")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool _ee27dec45b308755(string? a, string? b);

    ///<summary>Determines whether two specified strings have different values.</summary>
    ///<param name="a">The first string to compare, or <see langword="null" />.</param>
    ///<param name="b">The second string to compare, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if the value of <paramref name="a" /> is different from the value of <paramref name="b" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_1573803c425863d3","static string.operator !=(string, string)","_1573803c425863d3")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool _1573803c425863d3(string? a, string? b);

    ///<summary>Returns the hash code for this string.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("_bccdd3f386a6fbbc","override string.GetHashCode()","_bccdd3f386a6fbbc")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _bccdd3f386a6fbbc(String instance);

    ///<summary>Returns the hash code for this string using the specified rules.</summary>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("_04edfc3090710ca7","string.GetHashCode(System.StringComparison)","_04edfc3090710ca7")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _04edfc3090710ca7(String instance, System.StringComparison comparisonType);

    ///<summary>Returns the hash code for the provided read-only character span.</summary>
    ///<param name="value">A read-only character span.</param>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("_4598a18be32f839d","static string.GetHashCode(System.ReadOnlySpan<char>)","_4598a18be32f839d")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _4598a18be32f839d(Uint32Array value);

    ///<summary>Returns the hash code for the provided read-only character span using the specified rules.</summary>
    ///<param name="value">A read-only character span.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("_d123047f69d911f5","static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)","_d123047f69d911f5")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _d123047f69d911f5(Uint32Array value, System.StringComparison comparisonType);

    ///<summary>Determines whether the beginning of this string instance matches the specified string.</summary>
    ///<param name="value">The string to compare.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the beginning of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_1cda198f8257d023","string.StartsWith(string)","_1cda198f8257d023")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _1cda198f8257d023(String instance, string value);

    ///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
    ///<param name="value">The string to compare.</param>
    ///<param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>  <see langword="true" /> if this instance begins with <paramref name="value" />; otherwise, <see langword="false" />.</returns>
    [WhiteList("_0333a0fd5f67d8a0","string.StartsWith(string, System.StringComparison)","_0333a0fd5f67d8a0")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _0333a0fd5f67d8a0(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
    ///<param name="value">The string to compare.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore case during the comparison; otherwise, <see langword="false" />.</param>
    ///<param name="culture">Cultural information that determines how this string and <paramref name="value" /> are compared. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter matches the beginning of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_16d66a076936ebd2","string.StartsWith(string, bool, System.Globalization.CultureInfo)","_16d66a076936ebd2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _16d66a076936ebd2(String instance, string value, bool ignoreCase, String? culture);

    ///<summary>Determines whether this string instance starts with the specified character.</summary>
    ///<param name="value">The character to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> matches the beginning of this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_ef46304ffa6d6ccf","string.StartsWith(char)","_ef46304ffa6d6ccf")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _ef46304ffa6d6ccf(String instance, Number value);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified character array.</summary>
    ///<param name="value">An array of Unicode characters.</param>
    [WhiteList("_6651b0a853e8e991","string.String(char[])","_6651b0a853e8e991")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String _6651b0a853e8e991(char[]? value);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
    ///<param name="value">An array of Unicode characters.</param>
    ///<param name="startIndex">The starting position within <paramref name="value" />.</param>
    ///<param name="length">The number of characters within <paramref name="value" /> to use.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the number of elements in <paramref name="value" />.</exception>
    [WhiteList("_ddce1a944159fc8b","string.String(char[], int, int)","_ddce1a944159fc8b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String _ddce1a944159fc8b(char[] value, Number startIndex, Number length);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
    ///<param name="c">A Unicode character.</param>
    ///<param name="count">The number of times <paramref name="c" /> occurs.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is less than zero.</exception>
    [WhiteList("_0ce0d88e18c041c8","string.String(char, int)","_0ce0d88e18c041c8")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String _0ce0d88e18c041c8(Number c, Number count);

    ///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified read-only span.</summary>
    ///<param name="value">A read-only span of Unicode characters.</param>
    [WhiteList("_009fee2e166a416d","string.String(System.ReadOnlySpan<char>)","_009fee2e166a416d")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String _009fee2e166a416d(Uint32Array value);

    ///<summary>Creates a new string with a specific length and initializes it after creation by using the specified callback.</summary>
    ///<param name="length">The length of the string to create.</param>
    ///<param name="state">The element to pass to <paramref name="action" />.</param>
    ///<param name="action">A callback to initialize the string.</param>
    ///<typeparam name="TState">The type of the element to pass to <paramref name="action" />.</typeparam>
    ///<returns>The created string.</returns>
    [WhiteList("_dcfb926861070414","static string.Create<TState>(int, TState, System.Buffers.SpanAction<char, TState>)","_dcfb926861070414")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _dcfb926861070414<TState>(Number length, TState state, System.Buffers.SpanAction<char, TState> action);

    ///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="handler">The interpolated string, passed by reference.</param>
    ///<returns>The string that results for formatting the interpolated string using the specified format provider.</returns>
    [WhiteList("_af610a42747a747c","static string.Create(System.IFormatProvider, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)","_af610a42747a747c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _af610a42747a747c(Intl.NumberFormat? provider, RefValue<object> handler);

    ///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="initialBuffer">The initial buffer that may be used as temporary space as part of the formatting operation. The contents of this buffer may be overwritten.</param>
    ///<param name="handler">The interpolated string, passed by reference.</param>
    ///<returns>The string that results for formatting the interpolated string using the specified format provider.</returns>
    [WhiteList("_1978314137f5a599","static string.Create(System.IFormatProvider, System.Span<char>, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)","_1978314137f5a599")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _1978314137f5a599(Intl.NumberFormat? provider, Uint32Array initialBuffer, RefValue<object> handler);

    ///<summary>Defines an implicit conversion of a given string to a read-only span of characters.</summary>
    ///<param name="value">A string to implicitly convert.</param>
    ///<returns>A new read-only span of characters representing the string.</returns>
    [WhiteList("_5ff800b094791eb0","static string.implicit operator System.ReadOnlySpan<char>(string)","_5ff800b094791eb0")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Uint32Array _5ff800b094791eb0();

    ///<summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
    ///<returns>This instance of <see cref="T:System.String" />.</returns>
    [WhiteList("_488d7e5ec582c6fb","string.Clone()","_488d7e5ec582c6fb")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Object _488d7e5ec582c6fb(String instance);

    ///<summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
    ///<param name="str">The string to copy.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="str" /> is <see langword="null" />.</exception>
    ///<returns>A new string with the same value as <paramref name="str" />.</returns>
    [WhiteList("_0dc0a16fd99401f8","static string.Copy(string)","_0dc0a16fd99401f8")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _0dc0a16fd99401f8(string str);

    ///<summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
    ///<param name="sourceIndex">The index of the first character in this instance to copy.</param>
    ///<param name="destination">An array of Unicode characters to which characters in this instance are copied.</param>
    ///<param name="destinationIndex">The index in <paramref name="destination" /> at which the copy operation begins.</param>
    ///<param name="count">The number of characters in this instance to copy to <paramref name="destination" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="destination" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" /> is negative-or-<paramref name="sourceIndex" /> does not identify a position in the current instance.-or-<paramref name="destinationIndex" /> does not identify a valid index in the <paramref name="destination" /> array.-or-<paramref name="count" /> is greater than the length of the substring from <paramref name="sourceIndex" /> to the end of this instance-or-<paramref name="count" /> is greater than the length of the subarray from <paramref name="destinationIndex" /> to the end of the <paramref name="destination" /> array.</exception>
    [WhiteList("_45bb6097c28a2f1e","string.CopyTo(int, char[], int, int)","_45bb6097c28a2f1e")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void _45bb6097c28a2f1e(String instance, Number sourceIndex, char[] destination, Number destinationIndex, Number count);

    ///<summary>Copies the contents of this string into the destination span.</summary>
    ///<param name="destination">The span into which to copy this string's contents.</param>
    ///<exception cref="T:System.ArgumentException">The destination span is shorter than the source string.</exception>
    [WhiteList("_2b86529e4a090aee","string.CopyTo(System.Span<char>)","_2b86529e4a090aee")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void _2b86529e4a090aee(String instance, Uint32Array destination);

    ///<summary>Copies the contents of this string into the destination span.</summary>
    ///<param name="destination">The span into which to copy this string's contents.</param>
    ///<returns>  <see langword="true" /> if the data was copied; <see langword="false" /> if the destination was too short to fit the contents of the string.</returns>
    [WhiteList("_b0ab2eeef447828c","string.TryCopyTo(System.Span<char>)","_b0ab2eeef447828c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _b0ab2eeef447828c(String instance, Uint32Array destination);

    ///<summary>Copies the characters in this instance to a Unicode character array.</summary>
    ///<returns>A Unicode character array whose elements are the individual characters of this instance. If this instance is an empty string, the returned array is empty and has a zero length.</returns>
    [WhiteList("_7b8eb7b3d52c463d","string.ToCharArray()","_7b8eb7b3d52c463d")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static char[] _7b8eb7b3d52c463d(String instance);

    ///<summary>Copies the characters in a specified substring in this instance to a Unicode character array.</summary>
    ///<param name="startIndex">The starting position of a substring in this instance.</param>
    ///<param name="length">The length of the substring in this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-<paramref name="startIndex" /> plus <paramref name="length" /> is greater than the length of this instance.</exception>
    ///<returns>A Unicode character array whose elements are the <paramref name="length" /> number of characters in this instance starting from character position <paramref name="startIndex" />.</returns>
    [WhiteList("_53042938adf57f41","string.ToCharArray(int, int)","_53042938adf57f41")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static char[] _53042938adf57f41(String instance, Number startIndex, Number length);

    ///<summary>Indicates whether the specified string is <see langword="null" /> or an empty string ("").</summary>
    ///<param name="value">The string to test.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter is <see langword="null" /> or an empty string (""); otherwise, <see langword="false" />.</returns>
    [WhiteList("_f6e1cc63ac93e98f","static string.IsNullOrEmpty(string)","_f6e1cc63ac93e98f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _f6e1cc63ac93e98f(string? value);

    ///<summary>Indicates whether a specified string is <see langword="null" />, empty, or consists only of white-space characters.</summary>
    ///<param name="value">The string to test.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, or if <paramref name="value" /> consists exclusively of white-space characters.</returns>
    [WhiteList("_257a1a64b4d0f7d2","static string.IsNullOrWhiteSpace(string)","_257a1a64b4d0f7d2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _257a1a64b4d0f7d2(string? value);

    ///<summary>Returns a reference to the element of the string at index zero.This method is intended to support .NET compilers and is not intended to be called by user code.</summary>
    ///<exception cref="T:System.NullReferenceException">The string is null.</exception>
    ///<returns>A reference to the first character in the string, or a reference to the string's null terminator if the string is empty.</returns>
    [WhiteList("_519728f02e3ba627","string.GetPinnableReference()","_519728f02e3ba627")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _519728f02e3ba627(String instance);

    ///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
    ///<returns>The current string.</returns>
    [WhiteList("_3158320a4854cc16","override string.ToString()","_3158320a4854cc16")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _3158320a4854cc16(String instance);

    ///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
    ///<param name="provider">(Reserved) An object that supplies culture-specific formatting information.</param>
    ///<returns>The current string.</returns>
    [WhiteList("_555baf594c383de9","string.ToString(System.IFormatProvider)","_555baf594c383de9")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _555baf594c383de9(String instance, Intl.NumberFormat? provider);

    ///<summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
    ///<returns>An enumerator object.</returns>
    [WhiteList("_b5d8c191b0b746ca","string.GetEnumerator()","_b5d8c191b0b746ca")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.CharEnumerator _b5d8c191b0b746ca(String instance);

    ///<summary>Returns an enumeration of <see cref="T:System.Text.Rune" /> from this string.</summary>
    ///<returns>A string rune enumerator.</returns>
    [WhiteList("_1e33e6a38a2179d0","string.EnumerateRunes()","_1e33e6a38a2179d0")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringRuneEnumerator _1e33e6a38a2179d0(String instance);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for the <see cref="T:System.String" /> class.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.String" />.</returns>
    [WhiteList("_b4f593c93e2f2c61","string.GetTypeCode()","_b4f593c93e2f2c61")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode _b4f593c93e2f2c61(String instance);

    ///<summary>Indicates whether this string is in Unicode normalization form C.</summary>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>  <see langword="true" /> if this string is in normalization form C; otherwise, <see langword="false" />.</returns>
    [WhiteList("_f645a0207f41fd4a","string.IsNormalized()","_f645a0207f41fd4a")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _f645a0207f41fd4a(String instance);

    ///<summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
    ///<param name="normalizationForm">A Unicode normalization form.</param>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>  <see langword="true" /> if this string is in the normalization form specified by the <paramref name="normalizationForm" /> parameter; otherwise, <see langword="false" />.</returns>
    [WhiteList("_30d0ce62702ae938","string.IsNormalized(System.Text.NormalizationForm)","_30d0ce62702ae938")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _30d0ce62702ae938(String instance, System.Text.NormalizationForm normalizationForm);

    ///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>A new, normalized string whose textual value is the same as this string, but whose binary representation is in normalization form C.</returns>
    [WhiteList("_967ef647d59f3e39","string.Normalize()","_967ef647d59f3e39")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _967ef647d59f3e39(String instance);

    ///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
    ///<param name="normalizationForm">A Unicode normalization form.</param>
    ///<exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
    ///<returns>A new string whose textual value is the same as this string, but whose binary representation is in the normalization form specified by the <paramref name="normalizationForm" /> parameter.</returns>
    [WhiteList("_59b116010f03241b","string.Normalize(System.Text.NormalizationForm)","_59b116010f03241b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _59b116010f03241b(String instance, System.Text.NormalizationForm normalizationForm);

    [WhiteList("_5ad63706a889c294","string.this[int].get","_5ad63706a889c294")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _5ad63706a889c294(String instance, Number index);

    [WhiteList("_1b0d64005dc28838","string.Length.get","_1b0d64005dc28838")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _1b0d64005dc28838(String instance);

    ///<summary>Creates the string  representation of a specified object.</summary>
    ///<param name="arg0">The object to represent, or <see langword="null" />.</param>
    ///<returns>The string representation of the value of <paramref name="arg0" />, or <see cref="F:System.String.Empty" /> if <paramref name="arg0" /> is <see langword="null" />.</returns>
    [WhiteList("_db938b9c2eb90d32","static string.Concat(object)","_db938b9c2eb90d32")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _db938b9c2eb90d32(Object? arg0);

    ///<summary>Concatenates the string representations of two specified objects.</summary>
    ///<param name="arg0">The first object to concatenate.</param>
    ///<param name="arg1">The second object to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
    [WhiteList("_d330ca25546acf36","static string.Concat(object, object)","_d330ca25546acf36")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _d330ca25546acf36(Object? arg0, Object? arg1);

    ///<summary>Concatenates the string representations of three specified objects.</summary>
    ///<param name="arg0">The first object to concatenate.</param>
    ///<param name="arg1">The second object to concatenate.</param>
    ///<param name="arg2">The third object to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
    [WhiteList("_dab9155adbef8f67","static string.Concat(object, object, object)","_dab9155adbef8f67")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _dab9155adbef8f67(Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
    ///<param name="args">An object array that contains the elements to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>The concatenated string representations of the values of the elements in <paramref name="args" />.</returns>
    [WhiteList("_e102498b82e5b869","static string.Concat(params object[])","_e102498b82e5b869")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _e102498b82e5b869( object?[] args);

    ///<summary>Concatenates the string representations of the elements in a specified span of objects.</summary>
    ///<param name="args">A span of objects that contains the elements to concatenate.</param>
    ///<returns>The concatenated string representations of the values of the elements in <paramref name="args" />.</returns>
    [WhiteList("_2d6a291b64a11ba3","static string.Concat(params System.ReadOnlySpan<object>)","_2d6a291b64a11ba3")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _2d6a291b64a11ba3( System.ReadOnlySpan<object?> args);

    ///<summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
    ///<param name="values">A collection object that implements the <see cref="T:System.Collections.Generic.IEnumerable`1" /> interface.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<returns>The concatenated members in <paramref name="values" />.</returns>
    [WhiteList("_68574aee669f440f","static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)","_68574aee669f440f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _68574aee669f440f<T>(IEnumerable<T> values);

    ///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
    ///<param name="values">A collection object that implements <see cref="T:System.Collections.Generic.IEnumerable`1" /> and whose generic type argument is <see cref="T:System.String" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<returns>The concatenated strings in <paramref name="values" />, or <see cref="F:System.String.Empty" /> if <paramref name="values" /> is an empty <see langword="IEnumerable(Of String)" />.</returns>
    [WhiteList("_a2a66aa54427416c","static string.Concat(System.Collections.Generic.IEnumerable<string>)","_a2a66aa54427416c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _a2a66aa54427416c(System.Collections.Generic.IEnumerable<string?> values);

    ///<summary>Concatenates two specified instances of <see cref="T:System.String" />.</summary>
    ///<param name="str0">The first string to concatenate.</param>
    ///<param name="str1">The second string to concatenate.</param>
    ///<returns>The concatenation of <paramref name="str0" /> and <paramref name="str1" />.</returns>
    [WhiteList("_021d71ef80d7918e","static string.Concat(string, string)","_021d71ef80d7918e")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _021d71ef80d7918e(string? str0, string? str1);

    ///<summary>Concatenates three specified instances of <see cref="T:System.String" />.</summary>
    ///<param name="str0">The first string to concatenate.</param>
    ///<param name="str1">The second string to concatenate.</param>
    ///<param name="str2">The third string to concatenate.</param>
    ///<returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, and <paramref name="str2" />.</returns>
    [WhiteList("_ccc7897cb6f89406","static string.Concat(string, string, string)","_ccc7897cb6f89406")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _ccc7897cb6f89406(string? str0, string? str1, string? str2);

    ///<summary>Concatenates four specified instances of <see cref="T:System.String" />.</summary>
    ///<param name="str0">The first string to concatenate.</param>
    ///<param name="str1">The second string to concatenate.</param>
    ///<param name="str2">The third string to concatenate.</param>
    ///<param name="str3">The fourth string to concatenate.</param>
    ///<returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, <paramref name="str2" />, and <paramref name="str3" />.</returns>
    [WhiteList("_abe4ba2b38df2f54","static string.Concat(string, string, string, string)","_abe4ba2b38df2f54")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _abe4ba2b38df2f54(string? str0, string? str1, string? str2, string? str3);

    ///<summary>Concatenates the string representations of two specified read-only character spans.</summary>
    ///<param name="str0">The first read-only character span to concatenate.</param>
    ///<param name="str1">The second read-only character span to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="str0" /> and <paramref name="str1" />.</returns>
    [WhiteList("_a6102c27abe1ff18","static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)","_a6102c27abe1ff18")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _a6102c27abe1ff18(Uint32Array str0, Uint32Array str1);

    ///<summary>Concatenates the string representations of three specified read-only character spans.</summary>
    ///<param name="str0">The first read-only character span to concatenate.</param>
    ///<param name="str1">The second read-only character span to concatenate.</param>
    ///<param name="str2">The third read-only character span to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="str0" />, <paramref name="str1" /> and <paramref name="str2" />.</returns>
    [WhiteList("_7de0cfb062a343ee","static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)","_7de0cfb062a343ee")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _7de0cfb062a343ee(Uint32Array str0, Uint32Array str1, Uint32Array str2);

    ///<summary>Concatenates the string representations of four specified read-only character spans.</summary>
    ///<param name="str0">The first read-only character span to concatenate.</param>
    ///<param name="str1">The second read-only character span to concatenate.</param>
    ///<param name="str2">The third read-only character span to concatenate.</param>
    ///<param name="str3">The fourth read-only character span to concatenate.</param>
    ///<returns>The concatenated string representations of the values of <paramref name="str0" />, <paramref name="str1" />, <paramref name="str2" /> and <paramref name="str3" />.</returns>
    [WhiteList("_5177ae056c5ca775","static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)","_5177ae056c5ca775")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _5177ae056c5ca775(Uint32Array str0, Uint32Array str1, Uint32Array str2, Uint32Array str3);

    ///<summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
    ///<param name="values">An array of string instances.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>The concatenated elements of <paramref name="values" />.</returns>
    [WhiteList("_0f681227152a171b","static string.Concat(params string[])","_0f681227152a171b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _0f681227152a171b( string?[] values);

    ///<summary>Concatenates the elements of a specified span of <see cref="T:System.String" />.</summary>
    ///<param name="values">A span of <see cref="T:System.String" /> instances.</param>
    ///<returns>The concatenated elements of <paramref name="values" />.</returns>
    [WhiteList("_22098d7fa5ce7a81","static string.Concat(params System.ReadOnlySpan<string>)","_22098d7fa5ce7a81")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _22098d7fa5ce7a81( System.ReadOnlySpan<string?> values);

    ///<summary>Replaces one or more format items in a string with the string representation of a specified object.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The format item in <paramref name="format" /> is invalid.-or-The index of a format item is not zero.</exception>
    ///<returns>A copy of <paramref name="format" /> in which any format items are replaced by the string representation of <paramref name="arg0" />.</returns>
    [WhiteList("_980dff69bc3b8afa","static string.Format(string, object)","_980dff69bc3b8afa")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _980dff69bc3b8afa(string format, Object? arg0);

    ///<summary>Replaces the format items in a string with the string representation of two specified objects.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is not zero or one.</exception>
    ///<returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
    [WhiteList("_8606f3cc36d1f8ed","static string.Format(string, object, object)","_8606f3cc36d1f8ed")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _8606f3cc36d1f8ed(string format, Object? arg0, Object? arg1);

    ///<summary>Replaces the format items in a string with the string representation of three specified objects.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than two.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
    [WhiteList("_cda0978188193522","static string.Format(string, object, object, object)","_cda0978188193522")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _cda0978188193522(string format, Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object array that contains zero or more objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("_99b8bed2ce27774c","static string.Format(string, params object[])","_99b8bed2ce27774c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _99b8bed2ce27774c(string format,  object?[] args);

    ///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified span.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object span that contains zero or more objects to format.</param>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("_38dfe358e33e2c5d","static string.Format(string, params System.ReadOnlySpan<object>)","_38dfe358e33e2c5d")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _38dfe358e33e2c5d(string format,  System.ReadOnlySpan<object?> args);

    ///<summary>Replaces the format item or items in a specified string with the string representation of the corresponding object. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is not zero.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format item or items have been replaced by the string representation of <paramref name="arg0" />.</returns>
    [WhiteList("_03246c01949cf478","static string.Format(System.IFormatProvider, string, object)","_03246c01949cf478")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _03246c01949cf478(Intl.NumberFormat? provider, string format, Object? arg0);

    ///<summary>Replaces the format items in a string with the string representation of two specified objects. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is not zero or one.</exception>
    ///<returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
    [WhiteList("_661214177662ec13","static string.Format(System.IFormatProvider, string, object, object)","_661214177662ec13")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _661214177662ec13(Intl.NumberFormat? provider, string format, Object? arg0, Object? arg1);

    ///<summary>Replaces the format items in a string with the string representation of three specified objects. An parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than two.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
    [WhiteList("_915cdc23ed4c4425","static string.Format(System.IFormatProvider, string, object, object, object)","_915cdc23ed4c4425")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _915cdc23ed4c4425(Intl.NumberFormat? provider, string format, Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified array. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object array that contains zero or more objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array.</exception>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("_2b199e5bf9c94fc2","static string.Format(System.IFormatProvider, string, params object[])","_2b199e5bf9c94fc2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _2b199e5bf9c94fc2(Intl.NumberFormat? provider, string format,  object?[] args);

    ///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified span. A parameter supplies culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An object span that contains zero or more objects to format.</param>
    ///<returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
    [WhiteList("_8a09a1f92212621f","static string.Format(System.IFormatProvider, string, params System.ReadOnlySpan<object>)","_8a09a1f92212621f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _8a09a1f92212621f(Intl.NumberFormat? provider, string format,  System.ReadOnlySpan<object?> args);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("_2fd17baa6bc57571","static string.Format<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)","_2fd17baa6bc57571")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _2fd17baa6bc57571<TArg0>(Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0);

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
    [WhiteList("_879b6befd667cd5c","static string.Format<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)","_879b6befd667cd5c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _879b6befd667cd5c<TArg0, TArg1>(Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0, TArg1 arg1);

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
    [WhiteList("_850c49e163cd3ed0","static string.Format<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)","_850c49e163cd3ed0")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _850c49e163cd3ed0<TArg0, TArg1, TArg2>(Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="args">An array of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("_1183035ecb38f2a4","static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params object[])","_1183035ecb38f2a4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _1183035ecb38f2a4(Intl.NumberFormat? provider, System.Text.CompositeFormat format,  object?[] args);

    ///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="args">A span of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>The formatted string.</returns>
    [WhiteList("_e4458a04839fcdc5","static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)","_e4458a04839fcdc5")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _e4458a04839fcdc5(Intl.NumberFormat? provider, System.Text.CompositeFormat format,  System.ReadOnlySpan<object?> args);

    ///<summary>Returns a new string in which a specified string is inserted at a specified index position in this instance.</summary>
    ///<param name="startIndex">The zero-based index position of the insertion.</param>
    ///<param name="value">The string to insert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is negative or greater than the length of this instance.</exception>
    ///<returns>A new string that is equivalent to this instance, but with <paramref name="value" /> inserted at position <paramref name="startIndex" />.</returns>
    [WhiteList("_91223088dad76801","string.Insert(int, string)","_91223088dad76801")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _91223088dad76801(String instance, Number startIndex, string value);

    ///<summary>Concatenates an array of strings, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">An array of strings to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="value" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("_14ec7ebbb72b7d13","static string.Join(char, params string[])","_14ec7ebbb72b7d13")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _14ec7ebbb72b7d13(Number separator,  string?[] value);

    ///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">A span that contains the elements to concatenate.</param>
    ///<returns>A string that consists of the elements of <paramref name="value" /> delimited by the <paramref name="separator" /> string. -or- <see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("_9f939553178c2ca6","static string.Join(char, params System.ReadOnlySpan<string>)","_9f939553178c2ca6")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _9f939553178c2ca6(Number separator,  System.ReadOnlySpan<string?> value);

    ///<summary>Concatenates all the elements of a string array, using the specified separator between each element.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">An array that contains the elements to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements in <paramref name="value" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("_f269cd27a4bbd549","static string.Join(string, params string[])","_f269cd27a4bbd549")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _f269cd27a4bbd549(string? separator,  string?[] value);

    ///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">A span that contains the elements to concatenate.</param>
    ///<returns>A string that consists of the elements of <paramref name="value" /> delimited by the <paramref name="separator" /> string. -or- <see cref="F:System.String.Empty" /> if <paramref name="value" /> has zero elements.</returns>
    [WhiteList("_224682d778b9facf","static string.Join(string, params System.ReadOnlySpan<string>)","_224682d778b9facf")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _224682d778b9facf(string? separator,  System.ReadOnlySpan<string?> value);

    ///<summary>Concatenates an array of strings, using the specified separator between each member, starting with the element in <paramref name="value" /> located at the <paramref name="startIndex" /> position, and concatenating up to <paramref name="count" /> elements.</summary>
    ///<param name="separator">Concatenates an array of strings, using the specified separator between each member, starting with the element located at the specified index and including a specified number of elements.</param>
    ///<param name="value">An array of strings to concatenate.</param>
    ///<param name="startIndex">The first item in <paramref name="value" /> to concatenate.</param>
    ///<param name="count">The number of elements from <paramref name="value" /> to concatenate, starting with the element in the <paramref name="startIndex" /> position.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="count" /> are negative.-or-<paramref name="startIndex" /> is greater than the length of <paramref name="value" />  - <paramref name="count" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of <paramref name="count" /> elements of <paramref name="value" /> starting at <paramref name="startIndex" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="count" /> is zero.</returns>
    [WhiteList("_f461a3c632706317","static string.Join(char, string[], int, int)","_f461a3c632706317")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _f461a3c632706317(Number separator, string?[] value, Number startIndex, Number count);

    ///<summary>Concatenates the specified elements of a string array, using the specified separator between each element.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
    ///<param name="value">An array that contains the elements to concatenate.</param>
    ///<param name="startIndex">The first element in <paramref name="value" /> to use.</param>
    ///<param name="count">The number of elements of <paramref name="value" /> to use.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="count" /> is less than 0.-or-<paramref name="startIndex" /> plus <paramref name="count" /> is greater than the number of elements in <paramref name="value" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>A string that consists of <paramref name="count" /> elements of <paramref name="value" /> starting at <paramref name="startIndex" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="count" /> is zero.</returns>
    [WhiteList("_f1ad756b7baec84b","static string.Join(string, string[], int, int)","_f1ad756b7baec84b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _f1ad756b7baec84b(string? separator, string?[] value, Number startIndex, Number count);

    ///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator.<paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the strings to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("_d8814705c8078096","static string.Join(string, System.Collections.Generic.IEnumerable<string>)","_d8814705c8078096")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _d8814705c8078096(string? separator, System.Collections.Generic.IEnumerable<string?> values);

    ///<summary>Concatenates the string representations of an array of objects, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array of objects whose string representations will be concatenated.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("_5ac0762c6816a423","static string.Join(char, params object[])","_5ac0762c6816a423")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _5ac0762c6816a423(Number separator,  object?[] values);

    ///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if value has more than one element.</param>
    ///<param name="values">A span of objects whose string representations will be concatenated.</param>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> character. -or- <see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("_477a1f45d63f93c2","static string.Join(char, params System.ReadOnlySpan<object>)","_477a1f45d63f93c2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _477a1f45d63f93c2(Number separator,  System.ReadOnlySpan<object?> values);

    ///<summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array that contains the elements to concatenate.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.-or-.NET Framework only: <see cref="F:System.String.Empty" /> if the first element of <paramref name="values" /> is <see langword="null" />.</returns>
    [WhiteList("_c69ae51b8f3b72f0","static string.Join(string, params object[])","_c69ae51b8f3b72f0")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _c69ae51b8f3b72f0(string? separator,  object?[] values);

    ///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A span of objects whose string representations will be concatenated.</param>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string. -or- <see cref="F:System.String.Empty" /> if <paramref name="values" /> has zero elements.</returns>
    [WhiteList("_f8903c473c9e5f05","static string.Join(string, params System.ReadOnlySpan<object>)","_f8903c473c9e5f05")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _f8903c473c9e5f05(string? separator,  System.ReadOnlySpan<object?> values);

    ///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the objects to concatenate.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> character.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has no elements.</returns>
    [WhiteList("_1c599eccbbc8f2b8","static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)","_1c599eccbbc8f2b8")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _1c599eccbbc8f2b8<T>(Number separator, IEnumerable<T> values);

    ///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the objects to concatenate.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="values" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>).</exception>
    ///<returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string.-or-<see cref="F:System.String.Empty" /> if <paramref name="values" /> has no elements.</returns>
    [WhiteList("_c78854b22e947a4f","static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)","_c78854b22e947a4f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _c78854b22e947a4f<T>(string? separator, IEnumerable<T> values);

    ///<summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("_26620c4bafb4f435","string.PadLeft(int)","_26620c4bafb4f435")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _26620c4bafb4f435(String instance, Number totalWidth);

    ///<summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<param name="paddingChar">A Unicode padding character.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("_7894e0294f780eb5","string.PadLeft(int, char)","_7894e0294f780eb5")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _7894e0294f780eb5(String instance, Number totalWidth, Number paddingChar);

    ///<summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("_0e8f0a28fc1de8c2","string.PadRight(int)","_0e8f0a28fc1de8c2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _0e8f0a28fc1de8c2(String instance, Number totalWidth);

    ///<summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
    ///<param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
    ///<param name="paddingChar">A Unicode padding character.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="totalWidth" /> is less than zero.</exception>
    ///<returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
    [WhiteList("_685227781124d327","string.PadRight(int, char)","_685227781124d327")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _685227781124d327(String instance, Number totalWidth, Number paddingChar);

    ///<summary>Returns a new string in which a specified number of characters in the current instance beginning at a specified position have been deleted.</summary>
    ///<param name="startIndex">The zero-based position to begin deleting characters.</param>
    ///<param name="count">The number of characters to delete.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> plus <paramref name="count" /> specify a position outside this instance.</exception>
    ///<returns>A new string that is equivalent to this instance except for the removed characters.</returns>
    [WhiteList("_ac075983805231a6","string.Remove(int, int)","_ac075983805231a6")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _ac075983805231a6(String instance, Number startIndex, Number count);

    ///<summary>Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.</summary>
    ///<param name="startIndex">The zero-based position to begin deleting characters.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than zero.-or-<paramref name="startIndex" /> specifies a position that is not within this string.</exception>
    ///<returns>A new string that is equivalent to this string except for the removed characters.</returns>
    [WhiteList("_d258363cef56cdfb","string.Remove(int)","_d258363cef56cdfb")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _d258363cef56cdfb(String instance, Number startIndex);

    ///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided culture and case sensitivity.</summary>
    ///<param name="oldValue">The string to be replaced.</param>
    ///<param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
    ///<param name="ignoreCase">  <see langword="true" /> to ignore casing when comparing; <see langword="false" /> otherwise.</param>
    ///<param name="culture">The culture to use when comparing. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="oldValue" /> is the empty string ("").</exception>
    ///<returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_80ebf2c83f8072e2","string.Replace(string, string, bool, System.Globalization.CultureInfo)","_80ebf2c83f8072e2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _80ebf2c83f8072e2(String instance, string oldValue, string? newValue, bool ignoreCase, String? culture);

    ///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.</summary>
    ///<param name="oldValue">The string to be replaced.</param>
    ///<param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
    ///<param name="comparisonType">One of the enumeration values that determines how <paramref name="oldValue" /> is searched within this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="oldValue" /> is the empty string ("").</exception>
    ///<returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_8a7510653022a974","string.Replace(string, string, System.StringComparison)","_8a7510653022a974")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _8a7510653022a974(String instance, string oldValue, string? newValue, System.StringComparison comparisonType);

    ///<summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
    ///<param name="oldChar">The Unicode character to be replaced.</param>
    ///<param name="newChar">The Unicode character to replace all occurrences of <paramref name="oldChar" />.</param>
    ///<returns>A string that is equivalent to this instance except that all instances of <paramref name="oldChar" /> are replaced with <paramref name="newChar" />. If <paramref name="oldChar" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_7d7cb13bbbbb83c8","string.Replace(char, char)","_7d7cb13bbbbb83c8")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _7d7cb13bbbbb83c8(String instance, Number oldChar, Number newChar);

    ///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.</summary>
    ///<param name="oldValue">The string to be replaced.</param>
    ///<param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="oldValue" /> is the empty string ("").</exception>
    ///<returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_78a0e353c29afbc9","string.Replace(string, string)","_78a0e353c29afbc9")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _78a0e353c29afbc9(String instance, string oldValue, string? newValue);

    ///<summary>Replaces all newline sequences in the current string with <see cref="P:System.Environment.NewLine" />.</summary>
    ///<returns>A string whose contents match the current string, but with all newline sequences replaced with <see cref="P:System.Environment.NewLine" />.</returns>
    [WhiteList("_3720e4de26fa4c1b","string.ReplaceLineEndings()","_3720e4de26fa4c1b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _3720e4de26fa4c1b(String instance);

    ///<summary>Replaces all newline sequences in the current string with <paramref name="replacementText" />.</summary>
    ///<param name="replacementText">The text to use as replacement.</param>
    ///<returns>A string whose contents match the current string, but with all newline sequences replaced with <paramref name="replacementText" />.</returns>
    [WhiteList("_35041c0250b36108","string.ReplaceLineEndings(string)","_35041c0250b36108")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _35041c0250b36108(String instance, string replacementText);

    ///<summary>Splits a string into substrings based on a specified delimiting character and, optionally, options.</summary>
    ///<param name="separator">A character that delimits the substrings in this string.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("_d8080c573d45b4b4","string.Split(char, System.StringSplitOptions)","_d8080c573d45b4b4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _d8080c573d45b4b4(String instance, Number separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on a specified delimiting character and, optionally, options.        Splits a string into a maximum number of substrings based on the provided character separator, optionally omitting empty substrings from the result.</summary>
    ///<param name="separator">A character that delimits the substrings in this instance.</param>
    ///<param name="count">The maximum number of elements expected in the array.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array that contains at most <paramref name="count" /> substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("_aaa73a4811837ec7","string.Split(char, int, System.StringSplitOptions)","_aaa73a4811837ec7")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _aaa73a4811837ec7(String instance, Number separator, Number count, System.StringSplitOptions options);

    ///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
    ///<param name="separator">An array of delimiting characters, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("_62c8810ea13dba45","string.Split(params char[])","_62c8810ea13dba45")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _62c8810ea13dba45(String instance,  char[]? separator);

    ///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
    ///<param name="separator">A span of delimiting characters, or an empty span that contains no delimiters.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by one or more characters in <paramref name="separator" />.</returns>
    [WhiteList("_5417a93b3075813a","string.Split(params System.ReadOnlySpan<char>)","_5417a93b3075813a")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _5417a93b3075813a(String instance,  Uint32Array separator);

    ///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters.</summary>
    ///<param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="count">The maximum number of substrings to return.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.</exception>
    ///<returns>An array whose elements contain the substrings in this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("_d03d120228c0c4ed","string.Split(char[], int)","_d03d120228c0c4ed")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _d03d120228c0c4ed(String instance, char[]? separator, Number count);

    ///<summary>Splits a string into substrings based on specified delimiting characters and options.</summary>
    ///<param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("_25c1f15b0ed2cb6e","string.Split(char[], System.StringSplitOptions)","_25c1f15b0ed2cb6e")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _25c1f15b0ed2cb6e(String instance, char[]? separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters and, optionally, options.</summary>
    ///<param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="count">The maximum number of substrings to return.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array that contains the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("_c8e5ceed33c6c638","string.Split(char[], int, System.StringSplitOptions)","_c8e5ceed33c6c638")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _c8e5ceed33c6c638(String instance, char[]? separator, Number count, System.StringSplitOptions options);

    ///<summary>Splits a string into substrings that are based on the provided string separator.</summary>
    ///<param name="separator">A string that delimits the substrings in this string.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array whose elements contain the substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("_189761f781df8770","string.Split(string, System.StringSplitOptions)","_189761f781df8770")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _189761f781df8770(String instance, string? separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on a specified delimiting string and, optionally, options.</summary>
    ///<param name="separator">A string that delimits the substrings in this instance.</param>
    ///<param name="count">The maximum number of elements expected in the array.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<returns>An array that contains at most <paramref name="count" /> substrings from this instance that are delimited by <paramref name="separator" />.</returns>
    [WhiteList("_96eb0a23afa7fdfb","string.Split(string, int, System.StringSplitOptions)","_96eb0a23afa7fdfb")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _96eb0a23afa7fdfb(String instance, string? separator, Number count, System.StringSplitOptions options);

    ///<summary>Splits a string into substrings based on a specified delimiting string and, optionally, options.</summary>
    ///<param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("_fff99c96206a241e","string.Split(string[], System.StringSplitOptions)","_fff99c96206a241e")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _fff99c96206a241e(String instance, string[]? separator, System.StringSplitOptions options);

    ///<summary>Splits a string into a maximum number of substrings based on specified delimiting strings and, optionally, options.</summary>
    ///<param name="separator">The strings that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null" />.</param>
    ///<param name="count">The maximum number of substrings to return.</param>
    ///<param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
    ///<returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
    [WhiteList("_f3c7edcc7cc89a4a","string.Split(string[], int, System.StringSplitOptions)","_f3c7edcc7cc89a4a")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string[] _f3c7edcc7cc89a4a(String instance, string[]? separator, Number count, System.StringSplitOptions options);

    ///<summary>Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.</summary>
    ///<param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than zero or greater than the length of this instance.</exception>
    ///<returns>A string that is equivalent to the substring that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance.</returns>
    [WhiteList("_6b947e3ae92ce851","string.Substring(int)","_6b947e3ae92ce851")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _6b947e3ae92ce851(String instance, Number startIndex);

    ///<summary>Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.</summary>
    ///<param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
    ///<param name="length">The number of characters in the substring.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> plus <paramref name="length" /> indicates a position not within this instance.-or-<paramref name="startIndex" /> or <paramref name="length" /> is less than zero.</exception>
    ///<returns>A string that is equivalent to the substring of length <paramref name="length" /> that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance and <paramref name="length" /> is zero.</returns>
    [WhiteList("_ac659b5819c0360c","string.Substring(int, int)","_ac659b5819c0360c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _ac659b5819c0360c(String instance, Number startIndex, Number length);

    ///<summary>Returns a copy of this string converted to lowercase.</summary>
    ///<returns>A string in lowercase.</returns>
    [WhiteList("_482205d85705de41","string.ToLower()","_482205d85705de41")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _482205d85705de41(String instance);

    ///<summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
    ///<param name="culture">An object that supplies culture-specific casing rules. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<returns>The lowercase equivalent of the current string.</returns>
    [WhiteList("_8e06da9945efff04","string.ToLower(System.Globalization.CultureInfo)","_8e06da9945efff04")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _8e06da9945efff04(String instance, String? culture);

    ///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to lowercase using the casing rules of the invariant culture.</summary>
    ///<returns>The lowercase equivalent of the current string.</returns>
    [WhiteList("_3ff043d0307f4917","string.ToLowerInvariant()","_3ff043d0307f4917")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _3ff043d0307f4917(String instance);

    ///<summary>Returns a copy of this string converted to uppercase.</summary>
    ///<returns>The uppercase equivalent of the current string.</returns>
    [WhiteList("_4b84099d877364bd","string.ToUpper()","_4b84099d877364bd")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _4b84099d877364bd(String instance);

    ///<summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
    ///<param name="culture">An object that supplies culture-specific casing rules. If <paramref name="culture" /> is <see langword="null" />, the current culture is used.</param>
    ///<returns>The uppercase equivalent of the current string.</returns>
    [WhiteList("_9369d4b370002404","string.ToUpper(System.Globalization.CultureInfo)","_9369d4b370002404")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _9369d4b370002404(String instance, String? culture);

    ///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to uppercase using the casing rules of the invariant culture.</summary>
    ///<returns>The uppercase equivalent of the current string.</returns>
    [WhiteList("_3dc9c0782170eb46","string.ToUpperInvariant()","_3dc9c0782170eb46")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _3dc9c0782170eb46(String instance);

    ///<summary>Removes all leading and trailing white-space characters from the current string.</summary>
    ///<returns>The string that remains after all white-space characters are removed from the start and end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_eb98ee79e16b7ad4","string.Trim()","_eb98ee79e16b7ad4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _eb98ee79e16b7ad4(String instance);

    ///<summary>Removes all leading and trailing instances of a character from the current string.</summary>
    ///<param name="trimChar">A Unicode character to remove.</param>
    ///<returns>The string that remains after all instances of the <paramref name="trimChar" /> character are removed from the start and end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_5d7e005b9dcb67de","string.Trim(char)","_5d7e005b9dcb67de")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _5d7e005b9dcb67de(String instance, Number trimChar);

    ///<summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current string.</summary>
    ///<param name="trimChars">An array of Unicode characters to remove, or <see langword="null" />.</param>
    ///<returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the start and end of the current string. If <paramref name="trimChars" /> is <see langword="null" /> or an empty array, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_c6c444b4e71e14f7","string.Trim(params char[])","_c6c444b4e71e14f7")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _c6c444b4e71e14f7(String instance,  char[]? trimChars);

    ///<summary>Removes all leading and trailing occurrences of a set of characters specified in a span from the current string.</summary>
    ///<param name="trimChars">A span of Unicode characters to remove.</param>
    ///<returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the start and end of the current string. If <paramref name="trimChars" /> is empty, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_0e8e4169883e5222","string.Trim(params System.ReadOnlySpan<char>)","_0e8e4169883e5222")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _0e8e4169883e5222(String instance,  Uint32Array trimChars);

    ///<summary>Removes all the leading white-space characters from the current string.</summary>
    ///<returns>The string that remains after all white-space characters are removed from the start of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_1ca7f6e7edd1e070","string.TrimStart()","_1ca7f6e7edd1e070")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _1ca7f6e7edd1e070(String instance);

    ///<summary>Removes all the leading occurrences of a specified character from the current string.</summary>
    ///<param name="trimChar">The Unicode character to remove.</param>
    ///<returns>The string that remains after all occurrences of the <paramref name="trimChar" /> character are removed from the start of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_561fe737e62cf332","string.TrimStart(char)","_561fe737e62cf332")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _561fe737e62cf332(String instance, Number trimChar);

    ///<summary>Removes all the leading occurrences of a set of characters specified in an array from the current string.</summary>
    ///<param name="trimChars">An array of Unicode characters to remove, or <see langword="null" />.</param>
    ///<returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the start of the current string. If <paramref name="trimChars" /> is <see langword="null" /> or an empty array, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_98731360726c6976","string.TrimStart(params char[])","_98731360726c6976")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _98731360726c6976(String instance,  char[]? trimChars);

    ///<summary>Removes all the leading occurrences of a set of characters specified in a span from the current string.</summary>
    ///<param name="trimChars">A span of Unicode characters to remove.</param>
    ///<returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the start of the current string. If <paramref name="trimChars" /> is empty, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_f0473806a2e03bb6","string.TrimStart(params System.ReadOnlySpan<char>)","_f0473806a2e03bb6")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _f0473806a2e03bb6(String instance,  Uint32Array trimChars);

    ///<summary>Removes all the trailing white-space characters from the current string.</summary>
    ///<returns>The string that remains after all white-space characters are removed from the end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_760bdb666072200b","string.TrimEnd()","_760bdb666072200b")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _760bdb666072200b(String instance);

    ///<summary>Removes all the trailing occurrences of a character from the current string.</summary>
    ///<param name="trimChar">A Unicode character to remove.</param>
    ///<returns>The string that remains after all occurrences of the <paramref name="trimChar" /> character are removed from the end of the current string. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_eb362a090d734099","string.TrimEnd(char)","_eb362a090d734099")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _eb362a090d734099(String instance, Number trimChar);

    ///<summary>Removes all the trailing occurrences of a set of characters specified in an array from the current string.</summary>
    ///<param name="trimChars">An array of Unicode characters to remove, or <see langword="null" />.</param>
    ///<returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the end of the current string. If <paramref name="trimChars" /> is <see langword="null" /> or an empty array, Unicode white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_a62862c1fbaa21c3","string.TrimEnd(params char[])","_a62862c1fbaa21c3")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _a62862c1fbaa21c3(String instance,  char[]? trimChars);

    ///<summary>Removes all the trailing occurrences of a set of characters specified in a span from the current string.</summary>
    ///<param name="trimChars">A span of Unicode characters to remove.</param>
    ///<returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the end of the current string. If <paramref name="trimChars" /> is empty, white-space characters are removed instead. If no characters can be trimmed from the current instance, the method returns the current instance unchanged.</returns>
    [WhiteList("_4f8d256566de4b17","string.TrimEnd(params System.ReadOnlySpan<char>)","_4f8d256566de4b17")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string _4f8d256566de4b17(String instance,  Uint32Array trimChars);

    ///<summary>Returns a value indicating whether a specified substring occurs within this string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.</returns>
    [WhiteList("_c42ed9bafadfb16c","string.Contains(string)","_c42ed9bafadfb16c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _c42ed9bafadfb16c(String instance, string value);

    ///<summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.</returns>
    [WhiteList("_d52d7114d5c1b839","string.Contains(string, System.StringComparison)","_d52d7114d5c1b839")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _d52d7114d5c1b839(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Returns a value indicating whether a specified character occurs within this string.</summary>
    ///<param name="value">The character to seek.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_5de05262ccc56b2e","string.Contains(char)","_5de05262ccc56b2e")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _5de05262ccc56b2e(String instance, Number value);

    ///<summary>Returns a value indicating whether a specified character occurs within this string, using the specified comparison rules.</summary>
    ///<param name="value">The character to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string; otherwise, <see langword="false" />.</returns>
    [WhiteList("_16d4b2b4de019fb2","string.Contains(char, System.StringComparison)","_16d4b2b4de019fb2")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool _16d4b2b4de019fb2(String instance, Number value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string.</summary>
    ///<param name="value">A Unicode character to seek.</param>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
    [WhiteList("_9c8b4ffa28964fba","string.IndexOf(char)","_9c8b4ffa28964fba")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _9c8b4ffa28964fba(String instance, Number value);

    ///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. The search starts at a specified character position.</summary>
    ///<param name="value">A Unicode character to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of the string.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the string if that character is found, or -1 if it is not.</returns>
    [WhiteList("_c98394955f62f130","string.IndexOf(char, int)","_c98394955f62f130")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c98394955f62f130(String instance, Number value, Number startIndex);

    ///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. A parameter specifies the type of search to use for the specified character.</summary>
    ///<param name="value">The character to seek.</param>
    ///<param name="comparisonType">An enumeration value that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based index of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
    [WhiteList("_5331447e2c855a66","string.IndexOf(char, System.StringComparison)","_5331447e2c855a66")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _5331447e2c855a66(String instance, Number value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
    ///<param name="value">A Unicode character to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the string if that character is found, or -1 if it is not.</returns>
    [WhiteList("_d2873e605fbed764","string.IndexOf(char, int, int)","_d2873e605fbed764")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _d2873e605fbed764(String instance, Number value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("_69b749a1c6cbae78","string.IndexOfAny(char[])","_69b749a1c6cbae78")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _69b749a1c6cbae78(String instance, char[] anyOf);

    ///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the number of characters in this instance.</exception>
    ///<returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("_63633a5f3b85c5a9","string.IndexOfAny(char[], int)","_63633a5f3b85c5a9")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _63633a5f3b85c5a9(String instance, char[] anyOf, Number startIndex);

    ///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="count" /> + <paramref name="startIndex" /> is greater than the number of characters in this instance.</exception>
    ///<returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("_cb863079aae72451","string.IndexOfAny(char[], int, int)","_cb863079aae72451")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _cb863079aae72451(String instance, char[] anyOf, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance.</summary>
    ///<param name="value">The string to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
    [WhiteList("_6fd03b0f0c2de338","string.IndexOf(string)","_6fd03b0f0c2de338")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _6fd03b0f0c2de338(String instance, string value);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("_8c391718b5fbe536","string.IndexOf(string, int)","_8c391718b5fbe536")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _8c391718b5fbe536(String instance, string value, Number startIndex);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("_ff549d811898fb56","string.IndexOf(string, int, int)","_ff549d811898fb56")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _ff549d811898fb56(String instance, string value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
    [WhiteList("_3ae4900da2b07b27","string.IndexOf(string, System.StringComparison)","_3ae4900da2b07b27")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _3ae4900da2b07b27(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based index position of the <paramref name="value" /> parameter from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("_2fabe2b831abe71e","string.IndexOf(string, int, System.StringComparison)","_2fabe2b831abe71e")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _2fabe2b831abe71e(String instance, string value, Number startIndex, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-<paramref name="startIndex" /> is greater than the length of this instance.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based index position of the <paramref name="value" /> parameter from the start of the current instance if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
    [WhiteList("_ab22561fc42166db","string.IndexOf(string, int, int, System.StringComparison)","_ab22561fc42166db")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _ab22561fc42166db(String instance, string value, Number startIndex, Number count, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.</summary>
    ///<param name="value">The Unicode character to seek.</param>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
    [WhiteList("_da9a8971cb787f7f","string.LastIndexOf(char)","_da9a8971cb787f7f")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _da9a8971cb787f7f(String instance, Number value);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
    ///<param name="value">The Unicode character to seek.</param>
    ///<param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_b21118cfc4c55581","string.LastIndexOf(char, int)","_b21118cfc4c55581")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _b21118cfc4c55581(String instance, Number value, Number startIndex);

    ///<summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
    ///<param name="value">The Unicode character to seek.</param>
    ///<param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" /> + 1 is less than zero.</exception>
    ///<returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_dbdd57f8d259ce66","string.LastIndexOf(char, int, int)","_dbdd57f8d259ce66")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _dbdd57f8d259ce66(String instance, Number value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
    [WhiteList("_c0212f4213a99019","string.LastIndexOfAny(char[])","_c0212f4213a99019")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c0212f4213a99019(String instance, char[] anyOf);

    ///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> specifies a position that is not within this instance.</exception>
    ///<returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_c401e64318e768c4","string.LastIndexOfAny(char[], int)","_c401e64318e768c4")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c401e64318e768c4(String instance, char[] anyOf, Number startIndex);

    ///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
    ///<param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="anyOf" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> minus <paramref name="count" /> + 1 is less than zero.</exception>
    ///<returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_3c17fcef5615e7a3","string.LastIndexOfAny(char[], int, int)","_3c17fcef5615e7a3")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _3c17fcef5615e7a3(String instance, char[] anyOf, Number startIndex, Number count);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance.</summary>
    ///<param name="value">The string to seek.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<returns>The zero-based starting index position of <paramref name="value" /> if that string is found, or -1 if it is not.</returns>
    [WhiteList("_ed4ccee87d9df9fc","string.LastIndexOf(string)","_ed4ccee87d9df9fc")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _ed4ccee87d9df9fc(String instance, string value);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance.-or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than -1 or greater than zero.</exception>
    ///<returns>The zero-based starting index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_404d5ed27b7e190a","string.LastIndexOf(string, int)","_404d5ed27b7e190a")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _404d5ed27b7e190a(String instance, string value, Number startIndex);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" />+ 1 specifies a position that is not within this instance.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="startIndex" /> is less than -1 or greater than zero.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="count" /> is greater than 1.</exception>
    ///<returns>The zero-based starting index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_c4ee024d06ee238c","string.LastIndexOf(string, int, int)","_c4ee024d06ee238c")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c4ee024d06ee238c(String instance, string value, Number startIndex, Number count);

    ///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based starting index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not.</returns>
    [WhiteList("_78449c135e18c4bc","string.LastIndexOf(string, System.StringComparison)","_78449c135e18c4bc")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _78449c135e18c4bc(String instance, string value, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance.-or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than -1 or greater than zero.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based starting index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_359dbce44ce4a4da","string.LastIndexOf(string, int, System.StringComparison)","_359dbce44ce4a4da")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _359dbce44ce4a4da(String instance, string value, Number startIndex, System.StringComparison comparisonType);

    ///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
    ///<param name="value">The string to seek.</param>
    ///<param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
    ///<param name="count">The number of character positions to examine.</param>
    ///<param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> + 1 - <paramref name="count" /> specifies a position that is not within this instance.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="startIndex" /> is less than -1 or greater than zero.-or-The current instance equals <see cref="F:System.String.Empty" /> and <paramref name="count" /> is greater than 1.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
    ///<returns>The zero-based starting index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
    [WhiteList("_c911a06f021bd138","string.LastIndexOf(string, int, int, System.StringComparison)","_c911a06f021bd138")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number _c911a06f021bd138(String instance, string value, Number startIndex, Number count, System.StringComparison comparisonType);
}
