using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("char")]
public static class CharModule
{
	//char.MaxValue = ;

	//char.MinValue = ;

    [WhiteList("char.Char()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharNew();

    ///<summary>Returns <see langword="true" /> if <paramref name="c" /> is an ASCII character ([ U+0000..U+007F ]).</summary>
    ///<param name="c">The character to analyze.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII character; <see langword="false" /> otherwise.</returns>
    [WhiteList("static char.IsAscii(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAscii(Number c);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>
    [WhiteList("override char.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharGetHashCode(Number instance);

    ///<summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare with this instance or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Char" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("override char.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharEquals(Number instance, Object? obj);

    ///<summary>Returns a value that indicates whether this instance is equal to the specified <see cref="T:System.Char" /> object.</summary>
    ///<param name="obj">An object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="obj" /> parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("char.Equals(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharEquals2(Number instance, Number obj);

    ///<summary>Compares this instance to a specified object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
    ///<param name="value">An object to compare this instance to, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Char" /> object.</exception>
    ///<returns>A signed number indicating the position of this instance in the sort order in relation to the <paramref name="value" /> parameter. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="value" />. -or- <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("char.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharCompareTo(Number instance, Object? value);

    ///<summary>Compares this instance to a specified <see cref="T:System.Char" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Char" /> object.</summary>
    ///<param name="value">A <see cref="T:System.Char" /> object to compare.</param>
    ///<returns>A signed number indicating the position of this instance in the sort order in relation to the <paramref name="value" /> parameter. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="value" />.</description></item></list></returns>
    [WhiteList("char.CompareTo(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharCompareTo2(Number instance, Number value);

    ///<summary>Converts the value of this instance to its equivalent string representation.</summary>
    ///<returns>The string representation of the value of this instance.</returns>
    [WhiteList("override char.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CharToString(Number instance);

    ///<summary>Converts the value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">(Reserved) An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
    [WhiteList("char.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CharToString2(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the specified Unicode character to its equivalent string representation.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<returns>The string representation of the value of <paramref name="c" />.</returns>
    [WhiteList("static char.ToString(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CharToString3(Number c);

    ///<summary>Converts the value of the specified string to its equivalent Unicode character.</summary>
    ///<param name="s">A string that contains a single character, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The length of <paramref name="s" /> is not 1.</exception>
    ///<returns>A Unicode character equivalent to the sole character in <paramref name="s" />.</returns>
    [WhiteList("static char.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharParse(string s);

    ///<summary>Converts the value of the specified string to its equivalent Unicode character. A return code indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string that contains a single character, or <see langword="null" />.</param>
    ///<param name="result">When this method returns, contains a Unicode character equivalent to the sole character in <paramref name="s" />, if the conversion succeeded, or an undefined value if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or the length of <paramref name="s" /> is not 1. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.TryParse(string, out char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharTryParse(string? s, OutValue<Number> result);

    ///<summary>Indicates whether a character is categorized as an ASCII letter.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiLetter(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiLetter(Number c);

    ///<summary>Indicates whether a character is categorized as a lowercase ASCII letter.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a lowercase ASCII letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiLetterLower(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiLetterLower(Number c);

    ///<summary>Indicates whether a character is categorized as an uppercase ASCII letter.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is an uppercase ASCII letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiLetterUpper(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiLetterUpper(Number c);

    ///<summary>Indicates whether a character is categorized as an ASCII digit.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiDigit(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiDigit(Number c);

    ///<summary>Indicates whether a character is categorized as an ASCII letter or digit.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII letter or digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiLetterOrDigit(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiLetterOrDigit(Number c);

    ///<summary>Indicates whether a character is categorized as an ASCII hexademical digit.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a hexademical digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiHexDigit(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiHexDigit(Number c);

    ///<summary>Indicates whether a character is categorized as an ASCII upper-case hexademical digit.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a hexademical digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiHexDigitUpper(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiHexDigitUpper(Number c);

    ///<summary>Indicates whether a character is categorized as an ASCII lower-case hexademical digit.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a lower-case hexademical digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsAsciiHexDigitLower(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsAsciiHexDigitLower(Number c);

    ///<summary>Indicates whether the specified Unicode character is categorized as a decimal digit.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a decimal digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsDigit(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsDigit(Number c);

    ///<summary>Indicates whether a character is within the specified inclusive range.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<param name="minInclusive">The lower bound, inclusive.</param>
    ///<param name="maxInclusive">The upper bound, inclusive.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is within the specified range; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsBetween(char, char, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsBetween(Number c, Number minInclusive, Number maxInclusive);

    ///<summary>Indicates whether the specified Unicode character is categorized as a Unicode letter.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLetter(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLetter(Number c);

    ///<summary>Indicates whether the specified Unicode character is categorized as white space.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is white space; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsWhiteSpace(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsWhiteSpace(Number c);

    ///<summary>Indicates whether the specified Unicode character is categorized as an uppercase letter.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is an uppercase letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsUpper(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsUpper(Number c);

    ///<summary>Indicates whether the specified Unicode character is categorized as a lowercase letter.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a lowercase letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLower(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLower(Number c);

    ///<summary>Indicates whether the specified Unicode character is categorized as a punctuation mark.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a punctuation mark; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsPunctuation(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsPunctuation(Number c);

    ///<summary>Indicates whether the specified Unicode character is categorized as a letter or a decimal digit.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a letter or a decimal digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLetterOrDigit(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLetterOrDigit(Number c);

    ///<summary>Converts the value of a specified Unicode character to its uppercase equivalent using specified culture-specific formatting information.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<param name="culture">An object that supplies culture-specific casing rules.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="culture" /> is <see langword="null" />.</exception>
    ///<returns>The uppercase equivalent of <paramref name="c" />, modified according to <paramref name="culture" />, or the unchanged value of <paramref name="c" /> if <paramref name="c" /> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
    [WhiteList("static char.ToUpper(char, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharToUpper(Number c, String culture);

    ///<summary>Converts the value of a Unicode character to its uppercase equivalent.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<returns>The uppercase equivalent of <paramref name="c" />, or the unchanged value of <paramref name="c" /> if <paramref name="c" /> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
    [WhiteList("static char.ToUpper(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharToUpper2(Number c);

    ///<summary>Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant culture.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<returns>The uppercase equivalent of the <paramref name="c" /> parameter, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already uppercase or not alphabetic.</returns>
    [WhiteList("static char.ToUpperInvariant(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharToUpperInvariant(Number c);

    ///<summary>Converts the value of a specified Unicode character to its lowercase equivalent using specified culture-specific formatting information.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<param name="culture">An object that supplies culture-specific casing rules.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="culture" /> is <see langword="null" />.</exception>
    ///<returns>The lowercase equivalent of <paramref name="c" />, modified according to <paramref name="culture" />, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
    [WhiteList("static char.ToLower(char, System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharToLower(Number c, String culture);

    ///<summary>Converts the value of a Unicode character to its lowercase equivalent.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<returns>The lowercase equivalent of <paramref name="c" />, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
    [WhiteList("static char.ToLower(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharToLower2(Number c);

    ///<summary>Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant culture.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<returns>The lowercase equivalent of the <paramref name="c" /> parameter, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
    [WhiteList("static char.ToLowerInvariant(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharToLowerInvariant(Number c);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Char" />.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.Char" />.</returns>
    [WhiteList("char.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode CharGetTypeCode(Number instance);

    ///<summary>Indicates whether the specified Unicode character is categorized as a control character.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a control character; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsControl(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsControl(Number c);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a control character.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a control character; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsControl(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsControl2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a decimal digit.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a decimal digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsDigit(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsDigit2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a Unicode letter.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLetter(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLetter2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a letter or a decimal digit.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a letter or a decimal digit; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLetterOrDigit(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLetterOrDigit2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a lowercase letter.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a lowercase letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLower(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLower2(string s, Number index);

    ///<summary>Indicates whether the specified Unicode character is categorized as a number.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a number; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsNumber(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsNumber(Number c);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a number.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a number; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsNumber(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsNumber2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a punctuation mark.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a punctuation mark; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsPunctuation(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsPunctuation2(string s, Number index);

    ///<summary>Indicates whether the specified Unicode character is categorized as a separator character.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a separator character; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSeparator(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSeparator(Number c);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a separator character.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a separator character; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSeparator(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSeparator2(string s, Number index);

    ///<summary>Indicates whether the specified character has a surrogate code unit.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is either a high surrogate or a low surrogate; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSurrogate(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSurrogate(Number c);

    ///<summary>Indicates whether the character at the specified position in a specified string has a surrogate code unit.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a either a high surrogate or a low surrogate; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSurrogate(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSurrogate2(string s, Number index);

    ///<summary>Indicates whether the specified Unicode character is categorized as a symbol character.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if <paramref name="c" /> is a symbol character; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSymbol(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSymbol(Number c);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as a symbol character.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a symbol character; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSymbol(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSymbol2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as an uppercase letter.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is an uppercase letter; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsUpper(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsUpper2(string s, Number index);

    ///<summary>Indicates whether the character at the specified position in a specified string is categorized as white space.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is white space; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsWhiteSpace(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsWhiteSpace2(string s, Number index);

    ///<summary>Categorizes a specified Unicode character into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
    ///<param name="c">The Unicode character to categorize.</param>
    ///<returns>A <see cref="T:System.Globalization.UnicodeCategory" /> value that identifies the group that contains <paramref name="c" />.</returns>
    [WhiteList("static char.GetUnicodeCategory(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.UnicodeCategory CharGetUnicodeCategory(Number c);

    ///<summary>Categorizes the character at the specified position in a specified string into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
    ///<param name="s">A <see cref="T:System.String" />.</param>
    ///<param name="index">The character position in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>A <see cref="T:System.Globalization.UnicodeCategory" /> enumerated constant that identifies the group that contains the character at position <paramref name="index" /> in <paramref name="s" />.</returns>
    [WhiteList("static char.GetUnicodeCategory(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.UnicodeCategory CharGetUnicodeCategory2(string s, Number index);

    ///<summary>Converts the specified numeric Unicode character to a double-precision floating point number.</summary>
    ///<param name="c">The Unicode character to convert.</param>
    ///<returns>The numeric value of <paramref name="c" /> if that character represents a number; otherwise, -1.0.</returns>
    [WhiteList("static char.GetNumericValue(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharGetNumericValue(Number c);

    ///<summary>Converts the numeric Unicode character at the specified position in a specified string to a double-precision floating point number.</summary>
    ///<param name="s">A <see cref="T:System.String" />.</param>
    ///<param name="index">The character position in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
    ///<returns>The numeric value of the character at position <paramref name="index" /> in <paramref name="s" /> if that character represents a number; otherwise, -1.</returns>
    [WhiteList("static char.GetNumericValue(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharGetNumericValue2(string s, Number index);

    ///<summary>Indicates whether the specified <see cref="T:System.Char" /> object is a high surrogate.</summary>
    ///<param name="c">The Unicode character to evaluate.</param>
    ///<returns>  <see langword="true" /> if the numeric value of the <paramref name="c" /> parameter ranges from U+D800 through U+DBFF; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsHighSurrogate(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsHighSurrogate(Number c);

    ///<summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a high surrogate.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the numeric value of the specified character in the <paramref name="s" /> parameter ranges from U+D800 through U+DBFF; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsHighSurrogate(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsHighSurrogate2(string s, Number index);

    ///<summary>Indicates whether the specified <see cref="T:System.Char" /> object is a low surrogate.</summary>
    ///<param name="c">The character to evaluate.</param>
    ///<returns>  <see langword="true" /> if the numeric value of the <paramref name="c" /> parameter ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLowSurrogate(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLowSurrogate(Number c);

    ///<summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a low surrogate.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the numeric value of the specified character in the <paramref name="s" /> parameter ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsLowSurrogate(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsLowSurrogate2(string s, Number index);

    ///<summary>Indicates whether two adjacent <see cref="T:System.Char" /> objects at a specified position in a string form a surrogate pair.</summary>
    ///<param name="s">A string.</param>
    ///<param name="index">The starting position of the pair of characters to evaluate within <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
    ///<returns>  <see langword="true" /> if the <paramref name="s" /> parameter includes adjacent characters at positions <paramref name="index" /> and <paramref name="index" /> + 1, and the numeric value of the character at position <paramref name="index" /> ranges from U+D800 through U+DBFF, and the numeric value of the character at position <paramref name="index" />+1 ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSurrogatePair(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSurrogatePair(string s, Number index);

    ///<summary>Indicates whether the two specified <see cref="T:System.Char" /> objects form a surrogate pair.</summary>
    ///<param name="highSurrogate">The character to evaluate as the high surrogate of a surrogate pair.</param>
    ///<param name="lowSurrogate">The character to evaluate as the low surrogate of a surrogate pair.</param>
    ///<returns>  <see langword="true" /> if the numeric value of the <paramref name="highSurrogate" /> parameter ranges from U+D800 through U+DBFF, and the numeric value of the <paramref name="lowSurrogate" /> parameter ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
    [WhiteList("static char.IsSurrogatePair(char, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CharIsSurrogatePair2(Number highSurrogate, Number lowSurrogate);

    ///<summary>Converts the specified Unicode code point into a UTF-16 encoded string.</summary>
    ///<param name="utf32">A 21-bit Unicode code point.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="utf32" /> is not a valid 21-bit Unicode code point ranging from U+0 through U+10FFFF, excluding the surrogate pair range from U+D800 through U+DFFF.</exception>
    ///<returns>A string consisting of one <see cref="T:System.Char" /> object or a surrogate pair of <see cref="T:System.Char" /> objects equivalent to the code point specified by the <paramref name="utf32" /> parameter.</returns>
    [WhiteList("static char.ConvertFromUtf32(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CharConvertFromUtf32(Number utf32);

    ///<summary>Converts the value of a UTF-16 encoded surrogate pair into a Unicode code point.</summary>
    ///<param name="highSurrogate">A high surrogate code unit (that is, a code unit ranging from U+D800 through U+DBFF).</param>
    ///<param name="lowSurrogate">A low surrogate code unit (that is, a code unit ranging from U+DC00 through U+DFFF).</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="highSurrogate" /> is not in the range U+D800 through U+DBFF, or <paramref name="lowSurrogate" /> is not in the range U+DC00 through U+DFFF.</exception>
    ///<returns>The 21-bit Unicode code point represented by the <paramref name="highSurrogate" /> and <paramref name="lowSurrogate" /> parameters.</returns>
    [WhiteList("static char.ConvertToUtf32(char, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharConvertToUtf32(Number highSurrogate, Number lowSurrogate);

    ///<summary>Converts the value of a UTF-16 encoded character or surrogate pair at a specified position in a string into a Unicode code point.</summary>
    ///<param name="s">A string that contains a character or surrogate pair.</param>
    ///<param name="index">The index position of the character or surrogate pair in <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
    ///<exception cref="T:System.ArgumentException">The specified index position contains a surrogate pair, and either the first character in the pair is not a valid high surrogate or the second character in the pair is not a valid low surrogate.</exception>
    ///<returns>The 21-bit Unicode code point represented by the character or surrogate pair at the position in the <paramref name="s" /> parameter specified by the <paramref name="index" /> parameter.</returns>
    [WhiteList("static char.ConvertToUtf32(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CharConvertToUtf322(string s, Number index);
}
