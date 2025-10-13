using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("short")]
public static class Int16Module
{
	//short.MaxValue = 32767;

	//short.MinValue = -32768;

    [WhiteList("short.Int16()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16New();

    ///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the object.</summary>
    ///<param name="value">An object to compare, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not an <see cref="T:System.Int16" />.</exception>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>    [WhiteList("short.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16CompareTo(Number instance, Object? value);

    ///<summary>Compares this instance to a specified 16-bit signed integer and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified 16-bit signed integer.</summary>
    ///<param name="value">An integer to compare.</param>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>    [WhiteList("short.CompareTo(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16CompareTo2(Number instance, Number value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Int16" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override short.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16Equals(Number instance, Object? obj);

    ///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int16" /> value.</summary>
    ///<param name="obj">An <see cref="T:System.Int16" /> value to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> has the same value as this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("short.Equals(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16Equals2(Number instance, Number obj);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override short.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16GetHashCode(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
    ///<returns>The string representation of the value of this instance, consisting of a minus sign if the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.</returns>    [WhiteList("override short.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string Int16ToString(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>    [WhiteList("short.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string Int16ToString2(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>    [WhiteList("short.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string Int16ToString3(Number instance, string? format);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific formatting information.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>    [WhiteList("short.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string Int16ToString4(Number instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current short number instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("short.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryFormat(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("short.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryFormat2(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its 16-bit signed integer equivalent.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</exception>
    ///<returns>A 16-bit signed integer equivalent to the number contained in <paramref name="s" />.</returns>    [WhiteList("static short.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse(string s);

    ///<summary>Converts the string representation of a number in a specified style to its 16-bit signed integer equivalent.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>. -or- <paramref name="s" /> includes non-zero fractional digits.</exception>
    ///<returns>A 16-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static short.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse2(string s, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its 16-bit signed integer equivalent.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</exception>
    ///<returns>A 16-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static short.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse3(string s, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>. -or- <paramref name="s" /> includes non-zero fractional digits.</exception>
    ///<returns>A 16-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static short.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse4(string s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent.</summary>
    ///<param name="s">A span containing the characters representing the number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<returns>A 16-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>    [WhiteList("static short.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<param name="result">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static short.TryParse(string, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse(string? s, OutValue<Number> result);

    ///<summary>Converts the span representation of a number in a culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters that represent the number to convert.</param>
    ///<param name="result">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static short.TryParse(System.ReadOnlySpan<char>, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse2(Uint32Array s, OutValue<Number> result);

    ///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 16-bit signed integer equivalent.</summary>
    ///<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
    ///<param name="result">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static short.TryParse(System.ReadOnlySpan<byte>, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse3(Uint8Array utf8Text, OutValue<Number> result);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string containing a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static short.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse4(string? s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters representing the number to convert. The span is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the 16-bit signed integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static short.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int16" />.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.Int16" />.</returns>    [WhiteList("short.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode Int16GetTypeCode(Number instance);

    ///<summary>Computes the quotient and remainder of two values.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
    ///<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
    ///<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>    [WhiteList("static short.DivRem(short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (short Quotient, short Remainder) Int16DivRem(Number left, Number right);

    ///<summary>Computes the number of leading zeros in a value.</summary>
    ///<param name="value">The value whose leading zeroes are to be counted.</param>
    ///<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static short.LeadingZeroCount(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16LeadingZeroCount(Number value);

    ///<summary>Computes the number of bits that are set in a value.</summary>
    ///<param name="value">The value whose set bits are to be counted.</param>
    ///<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static short.PopCount(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16PopCount(Number value);

    ///<summary>Rotates a value left by a given amount.</summary>
    ///<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>    [WhiteList("static short.RotateLeft(short, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16RotateLeft(Number value, Number rotateAmount);

    ///<summary>Rotates a value right by a given amount.</summary>
    ///<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>    [WhiteList("static short.RotateRight(short, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16RotateRight(Number value, Number rotateAmount);

    ///<summary>Computes the number of trailing zeros in a value.</summary>
    ///<param name="value">The value whose trailing zeroes are to be counted.</param>
    ///<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static short.TrailingZeroCount(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16TrailingZeroCount(Number value);

    ///<summary>Determines if a value is a power of two.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.IsPow2(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16IsPow2(Number value);

    ///<summary>Computes the log2 of a value.</summary>
    ///<param name="value">The value whose log2 is to be computed.</param>
    ///<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static short.Log2(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Log2(Number value);

    ///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>    [WhiteList("static short.Clamp(short, short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Clamp(Number value, Number min, Number max);

    ///<summary>Copies the sign of a value to the sign of another value.</summary>
    ///<param name="value">The value whose magnitude is used in the result.</param>
    ///<param name="sign">The value whose sign is used in the result.</param>
    ///<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>    [WhiteList("static short.CopySign(short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16CopySign(Number value, Number sign);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static short.Max(short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Max(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static short.Min(short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Min(Number x, Number y);

    ///<summary>Computes the sign of a value.</summary>
    ///<param name="value">The value whose sign is to be computed.</param>
    ///<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>    [WhiteList("static short.Sign(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Sign(Number value);

    ///<summary>Computes the absolute of a value.</summary>
    ///<param name="value">The value for which to get its absolute.</param>
    ///<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static short.Abs(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Abs(Number value);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static short.CreateChecked<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16CreateChecked<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static short.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16CreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static short.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16CreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.IsEvenInteger(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16IsEvenInteger(Number value);

    ///<summary>Determines if a value is negative.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.IsNegative(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16IsNegative(Number value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.IsOddInteger(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16IsOddInteger(Number value);

    ///<summary>Determines if a value is positive.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.IsPositive(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16IsPositive(Number value);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static short.MaxMagnitude(short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16MaxMagnitude(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static short.MinMagnitude(short, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16MinMagnitude(Number x, Number y);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.TryParse(string, System.IFormatProvider, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse6(string? s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static short.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse7(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static short.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse7(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse8(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static short.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number Int16Parse8(Uint8Array utf8Text, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static short.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool Int16TryParse9(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<Number> result);
}
