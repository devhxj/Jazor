using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("decimal")]
public static class DecimalModule
{
	//decimal.Zero = 0;

	//decimal.One = 1;

	//decimal.MinusOne = -1;

	//decimal.MaxValue = 79228162514264337593543950335;

	//decimal.MinValue = -79228162514264337593543950335;

    [WhiteList("decimal.Decimal()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew();

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 32-bit signed integer.</summary>
    ///<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>    [WhiteList("decimal.Decimal(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew2(Number value);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 32-bit unsigned integer.</summary>
    ///<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>    [WhiteList("decimal.Decimal(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew3(Number value);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 64-bit signed integer.</summary>
    ///<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>    [WhiteList("decimal.Decimal(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew4(BigInt value);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 64-bit unsigned integer.</summary>
    ///<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>    [WhiteList("decimal.Decimal(ulong)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew5(BigInt value);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified single-precision floating-point number.</summary>
    ///<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see> or less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</exception>    [WhiteList("decimal.Decimal(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew6(Number value);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified double-precision floating-point number.</summary>
    ///<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see> or less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />.</exception>    [WhiteList("decimal.Decimal(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew7(Number value);

    ///<summary>Converts the specified 64-bit signed integer, which contains an OLE Automation Currency value, to the equivalent <see cref="T:System.Decimal" /> value.</summary>
    ///<param name="cy">An OLE Automation Currency value.</param>
    ///<returns>A <see cref="T:System.Decimal" /> that contains the equivalent of <paramref name="cy" />.</returns>    [WhiteList("static decimal.FromOACurrency(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalFromOACurrency(BigInt cy);

    ///<summary>Converts the specified <see cref="T:System.Decimal" /> value to the equivalent OLE Automation Currency value, which is contained in a 64-bit signed integer.</summary>
    ///<param name="value">The decimal number to convert.</param>
    ///<returns>A 64-bit signed integer that contains the OLE Automation equivalent of <paramref name="value" />.</returns>    [WhiteList("static decimal.ToOACurrency(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt DecimalToOACurrency(String value);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in a specified array.</summary>
    ///<param name="bits">An array of 32-bit signed integers containing a representation of a decimal value.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="bits" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">The length of the <paramref name="bits" /> is not 4. -or- The representation of the decimal value in <paramref name="bits" /> is not valid.</exception>    [WhiteList("decimal.Decimal(int[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew8(int[] bits);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in the specified span.</summary>
    ///<param name="bits">A span of four <see cref="T:System.Int32" /> values that contains a binary representation of a decimal value.</param>
    ///<exception cref="T:System.ArgumentException">The length of <paramref name="bits" /> is not 4, or the representation of the decimal value in <paramref name="bits" /> is not valid.</exception>    [WhiteList("decimal.Decimal(System.ReadOnlySpan<int>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew9(System.ReadOnlySpan<int> bits);

    ///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> from parameters specifying the instance's constituent parts.</summary>
    ///<param name="lo">The low 32 bits of a 96-bit integer.</param>
    ///<param name="mid">The middle 32 bits of a 96-bit integer.</param>
    ///<param name="hi">The high 32 bits of a 96-bit integer.</param>
    ///<param name="isNegative">  <see langword="true" /> to indicate a negative number; <see langword="false" /> to indicate a positive number.</param>
    ///<param name="scale">A power of 10 ranging from 0 to 28.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="scale" /> is greater than 28.</exception>    [WhiteList("decimal.Decimal(int, int, int, bool, byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNew10(Number lo, Number mid, Number hi, bool isNegative, Number scale);

    [WhiteList("decimal.Scale.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalGetScale(String instance);

    ///<summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The first value to add.</param>
    ///<param name="d2">The second value to add.</param>
    ///<exception cref="T:System.OverflowException">The sum of <paramref name="d1" /> and <paramref name="d2" /> is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The sum of <paramref name="d1" /> and <paramref name="d2" />.</returns>    [WhiteList("static decimal.Add(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalAdd(String d1, String d2);

    ///<summary>Returns the smallest integral value that is greater than or equal to the specified decimal number.</summary>
    ///<param name="d">A decimal number.</param>
    ///<returns>The smallest integral value that is greater than or equal to the <paramref name="d" /> parameter. Note that this method returns a <see cref="T:System.Decimal" /> instead of an integral type.</returns>    [WhiteList("static decimal.Ceiling(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalCeiling(String d);

    ///<summary>Compares two specified <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>A signed number indicating the relative values of <paramref name="d1" /> and <paramref name="d2" />. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="d1" /> is less than <paramref name="d2" />.</description></item><item><term> Zero</term><description><paramref name="d1" /> and <paramref name="d2" /> are equal.</description></item><item><term> Greater than zero</term><description><paramref name="d1" /> is greater than <paramref name="d2" />.</description></item></list></returns>    [WhiteList("static decimal.Compare(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalCompare(String d1, String d2);

    ///<summary>Compares this instance to a specified object and returns a comparison of their relative values.</summary>
    ///<param name="value">The object to compare with this instance, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Decimal" />.</exception>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>    [WhiteList("decimal.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalCompareTo(String instance, Object? value);

    ///<summary>Compares this instance to a specified <see cref="T:System.Decimal" /> object and returns a comparison of their relative values.</summary>
    ///<param name="value">The object to compare with this instance.</param>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>    [WhiteList("decimal.CompareTo(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalCompareTo2(String instance, String value);

    ///<summary>Divides two specified <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The dividend.</param>
    ///<param name="d2">The divisor.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="d2" /> is zero.</exception>
    ///<exception cref="T:System.OverflowException">The return value (that is, the quotient) is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The result of dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>    [WhiteList("static decimal.Divide(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalDivide(String d1, String d2);

    ///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Object" /> represent the same type and value.</summary>
    ///<param name="value">The object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is a <see cref="T:System.Decimal" /> and equal to this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override decimal.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalEquals(String instance, Object? value);

    ///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Decimal" /> object represent the same value.</summary>
    ///<param name="value">An object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is equal to this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("decimal.Equals(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalEquals2(String instance, String value);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override decimal.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalGetHashCode(String instance);

    ///<summary>Returns a value indicating whether two specified instances of <see cref="T:System.Decimal" /> represent the same value.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <see langword="true" /> if <paramref name="d1" /> and <paramref name="d2" /> are equal; otherwise, <see langword="false" />.</returns>    [WhiteList("static decimal.Equals(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalEquals3(String d1, String d2);

    ///<summary>Rounds a specified <see cref="T:System.Decimal" /> number to the closest integer toward negative infinity.</summary>
    ///<param name="d">The value to round.</param>
    ///<returns>If <paramref name="d" /> has a fractional part, the next whole <see cref="T:System.Decimal" /> number toward negative infinity that is less than <paramref name="d" />. -or- If <paramref name="d" /> doesn't have a fractional part, <paramref name="d" /> is returned unchanged. Note that the method returns an integral value of type <see cref="T:System.Decimal" />.</returns>    [WhiteList("static decimal.Floor(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalFloor(String d);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
    ///<returns>A string that represents the value of this instance.</returns>    [WhiteList("override decimal.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DecimalToString(String instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
    ///<param name="format">A standard or custom numeric format string.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>    [WhiteList("decimal.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DecimalToString2(String instance, string? format);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>    [WhiteList("decimal.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DecimalToString3(String instance, Intl.NumberFormat? provider);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>    [WhiteList("decimal.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DecimalToString4(String instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current decimal instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("decimal.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryFormat(String instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("decimal.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryFormat2(String instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
    ///<param name="s">The string representation of the number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The equivalent to the number contained in <paramref name="s" />.</returns>    [WhiteList("static decimal.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse(string s);

    ///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
    ///<param name="s">The string representation of the number to convert.</param>
    ///<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see></exception>
    ///<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" />.</returns>    [WhiteList("static decimal.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse2(string s, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
    ///<param name="s">The string representation of the number to convert.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific parsing information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="provider" />.</returns>    [WhiteList("static decimal.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse3(string s, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
    ///<param name="s">The string representation of the number to convert.</param>
    ///<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> object that supplies culture-specific information about the format of <paramref name="s" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" /> and <paramref name="provider" />.</returns>    [WhiteList("static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse4(string s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
    ///<param name="s">The span containing the characters representing the number to convert.</param>
    ///<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> object that supplies culture-specific information about the format of <paramref name="s" />.</param>
    ///<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" /> and <paramref name="provider" />.</returns>    [WhiteList("static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">The string representation of the number to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not a number in a valid format, or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static decimal.TryParse(string, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse(string? s, OutValue<String> result);

    ///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters representing the number to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse2(Uint32Array s, OutValue<String> result);

    ///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its signed decimal equivalent.</summary>
    ///<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
    ///<param name="result">When this method returns, contains the signed decimal value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse3(Uint8Array utf8Text, OutValue<String> result);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">The string representation of the number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
    ///<param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not a number in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse4(string? s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<String> result);

    ///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters representing the number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
    ///<param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not a number in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<String> result);

    ///<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
    ///<param name="d">The value to convert.</param>
    ///<returns>A 32-bit signed integer array with four elements that contain the binary representation of <paramref name="d" />.</returns>    [WhiteList("static decimal.GetBits(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static int[] DecimalGetBits(String d);

    ///<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
    ///<param name="d">The value to convert.</param>
    ///<param name="destination">The span into which to store the four-integer binary representation.</param>
    ///<exception cref="T:System.ArgumentException">The destination span was not long enough to store the binary representation.</exception>
    ///<returns>  <see langword="4" />, which is the number of integers in the binary representation.</returns>    [WhiteList("static decimal.GetBits(decimal, System.Span<int>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalGetBits2(String d, System.Span<int> destination);

    ///<summary>Tries to convert the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
    ///<param name="d">The value to convert.</param>
    ///<param name="destination">The span into which to store the binary representation.</param>
    ///<param name="valuesWritten">When this method returns, contains the number of integers written to the destination.</param>
    ///<returns>  <see langword="true" /> if the decimal's binary representation was written to the destination; <see langword="false" /> if the destination wasn't long enough.</returns>    [WhiteList("static decimal.TryGetBits(decimal, System.Span<int>, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryGetBits(String d, System.Span<int> destination, OutValue<Number> valuesWritten);

    ///<summary>Computes the remainder after dividing two <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The dividend.</param>
    ///<param name="d2">The divisor.</param>
    ///<exception cref="T:System.DivideByZeroException">  <paramref name="d2" /> is zero.</exception>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The remainder after dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>    [WhiteList("static decimal.Remainder(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalRemainder(String d1, String d2);

    ///<summary>Multiplies two specified <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The multiplicand.</param>
    ///<param name="d2">The multiplier.</param>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The result of multiplying <paramref name="d1" /> and <paramref name="d2" />.</returns>    [WhiteList("static decimal.Multiply(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalMultiply(String d1, String d2);

    ///<summary>Returns the result of multiplying the specified <see cref="T:System.Decimal" /> value by negative one.</summary>
    ///<param name="d">The value to negate.</param>
    ///<returns>A decimal number with the value of <paramref name="d" />, but the opposite sign. -or- Zero, if <paramref name="d" /> is zero.</returns>    [WhiteList("static decimal.Negate(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalNegate(String d);

    ///<summary>Rounds a decimal value to the nearest integer.</summary>
    ///<param name="d">A decimal number to round.</param>
    ///<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> value.</exception>
    ///<returns>The integer that is nearest to the <paramref name="d" /> parameter. If <paramref name="d" /> is halfway between two integers, one of which is even and the other odd, the even number is returned.</returns>    [WhiteList("static decimal.Round(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalRound(String d);

    ///<summary>Rounds a <see cref="T:System.Decimal" /> value to a specified number of decimal places.</summary>
    ///<param name="d">A decimal number to round.</param>
    ///<param name="decimals">A value from 0 to 28 that specifies the number of decimal places to round to.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="decimals" /> is not a value from 0 to 28.</exception>
    ///<returns>The decimal number equivalent to <paramref name="d" /> rounded to <paramref name="decimals" /> decimal places.</returns>    [WhiteList("static decimal.Round(decimal, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalRound2(String d, Number decimals);

    ///<summary>Rounds a decimal value to an integer using the specified rounding strategy.</summary>
    ///<param name="d">A decimal number to round.</param>
    ///<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a <see cref="T:System.MidpointRounding" /> value.</exception>
    ///<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
    ///<returns>The integer that <paramref name="d" /> is rounded to using the <paramref name="mode" /> rounding strategy.</returns>    [WhiteList("static decimal.Round(decimal, System.MidpointRounding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalRound3(String d, System.MidpointRounding mode);

    ///<summary>Rounds a decimal value to the specified precision using the specified rounding strategy.</summary>
    ///<param name="d">A decimal number to round.</param>
    ///<param name="decimals">The number of significant decimal places (precision) in the return value.</param>
    ///<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="decimals" /> is less than 0 or greater than 28.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a <see cref="T:System.MidpointRounding" /> value.</exception>
    ///<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
    ///<returns>The number that <paramref name="d" /> is rounded to using the <paramref name="mode" /> rounding strategy and with a precision of <paramref name="decimals" />. If the precision of <paramref name="d" /> is less than <paramref name="decimals" />, <paramref name="d" /> is returned unchanged.</returns>    [WhiteList("static decimal.Round(decimal, int, System.MidpointRounding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalRound4(String d, Number decimals, System.MidpointRounding mode);

    ///<summary>Subtracts a specified <see cref="T:System.Decimal" /> value from another.</summary>
    ///<param name="d1">The minuend.</param>
    ///<param name="d2">The subtrahend.</param>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The result of subtracting <paramref name="d2" /> from <paramref name="d1" />.</returns>    [WhiteList("static decimal.Subtract(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalSubtract(String d1, String d2);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit unsigned integer.</summary>
    ///<param name="value">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>.</exception>
    ///<returns>An 8-bit unsigned integer equivalent to <paramref name="value" />.</returns>    [WhiteList("static decimal.ToByte(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToByte(String value);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit signed integer.</summary>
    ///<param name="value">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue">SByte.MinValue</see> or greater than <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>.</exception>
    ///<returns>An 8-bit signed integer equivalent to <paramref name="value" />.</returns>    [WhiteList("static decimal.ToSByte(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToSByte(String value);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit signed integer.</summary>
    ///<param name="value">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</exception>
    ///<returns>A 16-bit signed integer equivalent to <paramref name="value" />.</returns>    [WhiteList("static decimal.ToInt16(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToInt16(String value);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent double-precision floating-point number.</summary>
    ///<param name="d">The decimal number to convert.</param>
    ///<returns>A double-precision floating-point number equivalent to <paramref name="d" />.</returns>    [WhiteList("static decimal.ToDouble(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToDouble(String d);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit signed integer.</summary>
    ///<param name="d">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="d" /> is less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
    ///<returns>A 32-bit signed integer equivalent to the value of <paramref name="d" />.</returns>    [WhiteList("static decimal.ToInt32(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToInt32(String d);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit signed integer.</summary>
    ///<param name="d">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="d" /> is less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</exception>
    ///<returns>A 64-bit signed integer equivalent to the value of <paramref name="d" />.</returns>    [WhiteList("static decimal.ToInt64(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt DecimalToInt64(String d);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit unsigned integer.</summary>
    ///<param name="value">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see> or less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see>.</exception>
    ///<returns>A 16-bit unsigned integer equivalent to the value of <paramref name="value" />.</returns>    [WhiteList("static decimal.ToUInt16(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToUInt16(String value);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit unsigned integer.</summary>
    ///<param name="d">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="d" /> is negative or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>.</exception>
    ///<returns>A 32-bit unsigned integer equivalent to the value of <paramref name="d" />.</returns>    [WhiteList("static decimal.ToUInt32(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToUInt32(String d);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit unsigned integer.</summary>
    ///<param name="d">The decimal number to convert.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="d" /> is negative or greater than <see cref="F:System.UInt64.MaxValue">UInt64.MaxValue</see>.</exception>
    ///<returns>A 64-bit unsigned integer equivalent to the value of <paramref name="d" />.</returns>    [WhiteList("static decimal.ToUInt64(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt DecimalToUInt64(String d);

    ///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent single-precision floating-point number.</summary>
    ///<param name="d">The decimal number to convert.</param>
    ///<returns>A single-precision floating-point number equivalent to the value of <paramref name="d" />.</returns>    [WhiteList("static decimal.ToSingle(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalToSingle(String d);

    ///<summary>Returns the integral digits of the specified <see cref="T:System.Decimal" />; any fractional digits are discarded.</summary>
    ///<param name="d">The decimal number to truncate.</param>
    ///<returns>The result of <paramref name="d" /> rounded toward zero, to the nearest whole number.</returns>    [WhiteList("static decimal.Truncate(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalTruncate(String d);

    ///<summary>Returns the value of the <see cref="T:System.Decimal" /> operand (the sign of the operand is unchanged).</summary>
    ///<param name="d">The operand to return.</param>
    ///<returns>The value of the operand, <paramref name="d" />.</returns>    [WhiteList("static decimal.operator +(decimal)")]
    [ECMAScriptLiteral("+{0}")]
	public extern static String DecimalOpUnaryPlus(String d);

    ///<summary>Negates the value of the specified <see cref="T:System.Decimal" /> operand.</summary>
    ///<param name="d">The value to negate.</param>
    ///<returns>The result of <paramref name="d" /> multiplied by negative one (-1).</returns>    [WhiteList("static decimal.operator -(decimal)")]
    [ECMAScriptLiteral("-{0}")]
	public extern static String DecimalOpUnaryNegation(String d);

    ///<summary>Increments the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by 1.</summary>
    ///<param name="d">The value to increment.</param>
    ///<returns>The value of <code data-dev-comment-type="paramref">d</code> incremented by 1.</returns>    [WhiteList("static decimal.operator ++(decimal)")]
    [ECMAScriptLiteral("++{0}")]
	public extern static String DecimalOpIncrement(String d);

    ///<summary>Decrements the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by one.</summary>
    ///<param name="d">The value to decrement.</param>
    ///<returns>The value of <code data-dev-comment-type="paramref">d</code> decremented by 1.</returns>    [WhiteList("static decimal.operator --(decimal)")]
    [ECMAScriptLiteral("--{0}")]
	public extern static String DecimalOpDecrement(String d);

    ///<summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The first value to add.</param>
    ///<param name="d2">The second value to add.</param>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The result of adding <paramref name="d1" /> and <paramref name="d2" />.</returns>    [WhiteList("static decimal.operator +(decimal, decimal)")]
    [ECMAScriptLiteral("{0} + {1}")]
	public extern static String DecimalOpAddition(String d1, String d2);

    ///<summary>Subtracts two specified <see cref="T:System.Decimal" /> values.</summary>
    ///<param name="d1">The minuend.</param>
    ///<param name="d2">The subtrahend.</param>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
    ///<returns>The result of subtracting <paramref name="d2" /> from <paramref name="d1" />.</returns>    [WhiteList("static decimal.operator -(decimal, decimal)")]
    [ECMAScriptLiteral("{0} - {1}")]
	public extern static String DecimalOpSubtraction(String d1, String d2);

    ///<summary>Multiplies two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
    ///<param name="d1">The first value to multiply.</param>
    ///<param name="d2">The second value to multiply.</param>
    ///<returns>The result of multiplying <code data-dev-comment-type="paramref">d1</code> by <code data-dev-comment-type="paramref">d2</code>.</returns>    [WhiteList("static decimal.operator *(decimal, decimal)")]
    [ECMAScriptLiteral("{0} * {1}")]
	public extern static String DecimalOpMultiply(String d1, String d2);

    ///<summary>Divides two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
    ///<param name="d1">The dividend.</param>
    ///<param name="d2">The divisor.</param>
    ///<returns>The result of dividing <code data-dev-comment-type="paramref">d1</code> by <code data-dev-comment-type="paramref">d2</code>.</returns>    [WhiteList("static decimal.operator /(decimal, decimal)")]
    [ECMAScriptLiteral("{0} / {1}")]
	public extern static String DecimalOpDivision(String d1, String d2);

    ///<summary>Returns the remainder resulting from dividing two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
    ///<param name="d1">The dividend.</param>
    ///<param name="d2">The divisor.</param>
    ///<returns>The remainder resulting from dividing <code data-dev-comment-type="paramref">d1</code> by <code data-dev-comment-type="paramref">d2</code>.</returns>    [WhiteList("static decimal.operator %(decimal, decimal)")]
    [ECMAScriptLiteral("{0} % {1}")]
	public extern static String DecimalOpModulus(String d1, String d2);

    ///<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values are equal.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> and <code data-dev-comment-type="paramref">d2</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.operator ==(decimal, decimal)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool DecimalOpEquality(String d1, String d2);

    ///<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> objects have different values.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> and <code data-dev-comment-type="paramref">d2</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.operator !=(decimal, decimal)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool DecimalOpInequality(String d1, String d2);

    ///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is less than <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.operator <(decimal, decimal)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool DecimalOpLessThan(String d1, String d2);

    ///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is less than or equal to <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.operator <=(decimal, decimal)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool DecimalOpLessThanOrEqual(String d1, String d2);

    ///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is greater than <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.operator >(decimal, decimal)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool DecimalOpGreaterThan(String d1, String d2);

    ///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
    ///<param name="d1">The first value to compare.</param>
    ///<param name="d2">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is greater than or equal to <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.operator >=(decimal, decimal)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool DecimalOpGreaterThanOrEqual(String d1, String d2);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Decimal" />.</summary>
    ///<returns>The enumerated constant <see cref="F:System.TypeCode.Decimal" />.</returns>    [WhiteList("decimal.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode DecimalGetTypeCode(String instance);

    ///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
    ///<param name="value">The value to be converted.</param>
    ///<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static decimal.ConvertToInteger<TInteger>(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TInteger DecimalConvertToInteger<TInteger>(String value);

    ///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
    ///<param name="value">The value to be converted.</param>
    ///<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static decimal.ConvertToIntegerNative<TInteger>(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TInteger DecimalConvertToIntegerNative<TInteger>(String value);

    ///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>    [WhiteList("static decimal.Clamp(decimal, decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalClamp(String value, String min, String max);

    ///<summary>Copies the sign of a value to the sign of another value.</summary>
    ///<param name="value">The value whose magnitude is used in the result.</param>
    ///<param name="sign">The value whose sign is used in the result.</param>
    ///<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>    [WhiteList("static decimal.CopySign(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalCopySign(String value, String sign);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static decimal.Max(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalMax(String x, String y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static decimal.Min(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalMin(String x, String y);

    ///<summary>Computes the sign of a value.</summary>
    ///<param name="d">The value whose sign is to be computed.</param>
    ///<returns>A positive value if <code data-dev-comment-type="paramref">d</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">d</code> is zero, and a negative value if <code data-dev-comment-type="paramref">d</code> is negative.</returns>    [WhiteList("static decimal.Sign(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DecimalSign(String d);

    ///<summary>Computes the absolute of a value.</summary>
    ///<param name="value">The value for which to get its absolute.</param>
    ///<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static decimal.Abs(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalAbs(String value);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static decimal.CreateChecked<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalCreateChecked<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static decimal.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalCreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static decimal.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalCreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value is in its canonical representation.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is in its canonical representation; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.IsCanonical(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalIsCanonical(String value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.IsEvenInteger(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalIsEvenInteger(String value);

    ///<summary>Determines if a value represents an integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.IsInteger(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalIsInteger(String value);

    ///<summary>Determines if a value is negative.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.IsNegative(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalIsNegative(String value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.IsOddInteger(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalIsOddInteger(String value);

    ///<summary>Determines if a value is positive.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.IsPositive(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalIsPositive(String value);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static decimal.MaxMagnitude(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalMaxMagnitude(String x, String y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static decimal.MinMagnitude(decimal, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalMinMagnitude(String x, String y);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.TryParse(string, System.IFormatProvider, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse6(string? s, Intl.NumberFormat? provider, OutValue<String> result);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse7(Uint32Array s, Intl.NumberFormat? provider, OutValue<String> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse7(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse8(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<String> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String DecimalParse8(Uint8Array utf8Text, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DecimalTryParse9(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<String> result);
}
