using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("uint")]
public static class UInt32Module
{
	//uint.MaxValue = 4294967295;

	//uint.MinValue = 0;

    [WhiteList("uint.UInt32()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32New();

    ///<summary>Produces the full product of two unsigned 32-bit numbers.</summary>
    ///<param name="left">The first number to multiply.</param>
    ///<param name="right">The second number to multiply.</param>
    ///<returns>The number containing the product of the specified numbers.</returns>    [WhiteList("static uint.BigMul(uint, uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt UInt32BigMul(Number left, Number right);

    ///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
    ///<param name="value">An object to compare, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.UInt32" />.</exception>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>    [WhiteList("uint.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32CompareTo(Number instance, Object? value);

    ///<summary>Compares this instance to a specified 32-bit unsigned integer and returns an indication of their relative values.</summary>
    ///<param name="value">An unsigned integer to compare.</param>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>    [WhiteList("uint.CompareTo(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32CompareTo2(Number instance, Number value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.UInt32" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override uint.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32Equals(Number instance, Object? obj);

    ///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.UInt32" />.</summary>
    ///<param name="obj">A value to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> has the same value as this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("uint.Equals(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32Equals2(Number instance, Number obj);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override uint.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32GetHashCode(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
    ///<returns>The string representation of the value of this instance, consisting of a sequence of digits ranging from 0 to 9, without a sign or leading zeroes.</returns>    [WhiteList("override uint.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string UInt32ToString(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance, which consists of a sequence of digits ranging from 0 to 9, without a sign or leading zeros.</returns>    [WhiteList("uint.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string UInt32ToString2(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<exception cref="T:System.FormatException">The <paramref name="format" /> parameter is invalid.</exception>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>    [WhiteList("uint.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string UInt32ToString3(Number instance, string? format);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about this instance.</param>
    ///<exception cref="T:System.FormatException">The <paramref name="format" /> parameter is invalid.</exception>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>    [WhiteList("uint.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string UInt32ToString4(Number instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current unsigned integer number instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("uint.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryFormat(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("uint.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryFormat2(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its 32-bit unsigned integer equivalent.</summary>
    ///<param name="s">A string representing the number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The <paramref name="s" /> parameter is not of the correct format.</exception>
    ///<exception cref="T:System.OverflowException">The <paramref name="s" /> parameter represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>.</exception>
    ///<returns>A 32-bit unsigned integer equivalent to the number contained in <paramref name="s" />.</returns>    [WhiteList("static uint.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse(string s);

    ///<summary>Converts the string representation of a number in a specified style to its 32-bit unsigned integer equivalent.</summary>
    ///<param name="s">A string representing the number to convert. The string is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
    ///<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
    ///<returns>A 32-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static uint.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse2(string s, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its 32-bit unsigned integer equivalent.</summary>
    ///<param name="s">A string that represents the number to convert.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct style.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>.</exception>
    ///<returns>A 32-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static uint.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse3(string s, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent.</summary>
    ///<param name="s">A string representing the number to convert. The string is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
    ///<returns>A 32-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static uint.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse4(string s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent.</summary>
    ///<param name="s">A span containing the characters that represent the number to convert. The span is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<returns>A 32-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static uint.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the string representation of a number to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string that represents the number to convert.</param>
    ///<param name="result">When this method returns, contains the 32-bit unsigned integer value that is equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static uint.TryParse(string, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse(string? s, OutValue<Number> result);

    ///<summary>Tries to convert the span representation of a number to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters that represent the number to convert.</param>
    ///<param name="result">When this method returns, contains the 32-bit unsigned integer value that is equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static uint.TryParse(System.ReadOnlySpan<char>, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse2(Uint32Array s, OutValue<Number> result);

    ///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 32-bit unsigned integer equivalent.</summary>
    ///<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
    ///<param name="result">When this method returns, contains the 32-bit unsigned integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static uint.TryParse(System.ReadOnlySpan<byte>, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse3(Uint8Array utf8Text, OutValue<Number> result);

    ///<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string that represents the number to convert. The string is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the 32-bit unsigned integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static uint.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse4(string? s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters that represent the number to convert. The span is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the 32-bit unsigned integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number that is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static uint.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt32" />.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.UInt32" />.</returns>    [WhiteList("uint.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode UInt32GetTypeCode(Number instance);

    ///<summary>Computes the quotient and remainder of two values.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
    ///<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
    ///<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>    [WhiteList("static uint.DivRem(uint, uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (uint Quotient, uint Remainder) UInt32DivRem(Number left, Number right);

    ///<summary>Computes the number of leading zeros in a value.</summary>
    ///<param name="value">The value whose leading zeroes are to be counted.</param>
    ///<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static uint.LeadingZeroCount(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32LeadingZeroCount(Number value);

    ///<summary>Computes the number of bits that are set in a value.</summary>
    ///<param name="value">The value whose set bits are to be counted.</param>
    ///<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static uint.PopCount(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32PopCount(Number value);

    ///<summary>Rotates a value left by a given amount.</summary>
    ///<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>    [WhiteList("static uint.RotateLeft(uint, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32RotateLeft(Number value, Number rotateAmount);

    ///<summary>Rotates a value right by a given amount.</summary>
    ///<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>    [WhiteList("static uint.RotateRight(uint, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32RotateRight(Number value, Number rotateAmount);

    ///<summary>Computes the number of trailing zeros in a value.</summary>
    ///<param name="value">The value whose trailing zeroes are to be counted.</param>
    ///<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static uint.TrailingZeroCount(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32TrailingZeroCount(Number value);

    ///<summary>Determines if a value is a power of two.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.IsPow2(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32IsPow2(Number value);

    ///<summary>Computes the log2 of a value.</summary>
    ///<param name="value">The value whose log2 is to be computed.</param>
    ///<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static uint.Log2(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Log2(Number value);

    ///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>    [WhiteList("static uint.Clamp(uint, uint, uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Clamp(Number value, Number min, Number max);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static uint.Max(uint, uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Max(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static uint.Min(uint, uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Min(Number x, Number y);

    ///<summary>Computes the sign of a value.</summary>
    ///<param name="value">The value whose sign is to be computed.</param>
    ///<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>    [WhiteList("static uint.Sign(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Sign(Number value);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static uint.CreateChecked<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32CreateChecked<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static uint.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32CreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static uint.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32CreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.IsEvenInteger(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32IsEvenInteger(Number value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.IsOddInteger(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32IsOddInteger(Number value);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.TryParse(string, System.IFormatProvider, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse6(string? s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static uint.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse7(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static uint.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse7(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse8(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static uint.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number UInt32Parse8(Uint8Array utf8Text, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static uint.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool UInt32TryParse9(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<Number> result);
}
