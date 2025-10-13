using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("double")]
public static class DoubleModule
{
	//double.MinValue = -1.7976931348623157E+308;

	//double.MaxValue = 1.7976931348623157E+308;

	//double.Epsilon = 5E-324;

	//double.NegativeInfinity = -∞;

	//double.PositiveInfinity = ∞;

	//double.NaN = NaN;

	//double.NegativeZero = -0;

	//double.E = 2.718281828459045;

	//double.Pi = 3.141592653589793;

	//double.Tau = 6.283185307179586;

    [WhiteList("double.Double()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleNew();

    ///<summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if the value is finite (zero, subnormal or normal); <see langword="false" /> otherwise.</returns>    [WhiteList("static double.IsFinite(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsFinite(Number d);

    ///<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.PositiveInfinity" /> or <see cref="F:System.Double.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.IsInfinity(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsInfinity(Number d);

    ///<summary>Returns a value that indicates whether the specified value is not a number (<see cref="F:System.Double.NaN" />).</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.NaN" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.IsNaN(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsNaN(Number d);

    ///<summary>Determines whether the specified value is negative.</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if the value is negative; <see langword="false" /> otherwise.</returns>    [WhiteList("static double.IsNegative(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsNegative(Number d);

    ///<summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.IsNegativeInfinity(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsNegativeInfinity(Number d);

    ///<summary>Determines whether the specified value is normal.</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if the value is normal; <see langword="false" /> otherwise.</returns>    [WhiteList("static double.IsNormal(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsNormal(Number d);

    ///<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.PositiveInfinity" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.IsPositiveInfinity(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsPositiveInfinity(Number d);

    ///<summary>Determines whether the specified value is subnormal.</summary>
    ///<param name="d">A double-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if the value is subnormal; <see langword="false" /> otherwise.</returns>    [WhiteList("static double.IsSubnormal(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsSubnormal(Number d);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
    ///<param name="value">An object to compare, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Double" />.</exception>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> A negative integer</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Double.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or this instance and <paramref name="value" /> are both <see langword="Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" /></description></item><item><term> A positive integer</term><description> This instance is greater than <paramref name="value" />, OR this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Double.NaN" />), OR <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>    [WhiteList("double.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCompareTo(Number instance, Object? value);

    ///<summary>Compares this instance to a specified double-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified double-precision floating-point number.</summary>
    ///<param name="value">A double-precision floating-point number to compare.</param>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Double.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or both this instance and <paramref name="value" /> are not a number (<see cref="F:System.Double.NaN" />), <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Double.NaN" />).</description></item></list></returns>    [WhiteList("double.CompareTo(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCompareTo2(Number instance, Number value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Double" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override double.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleEquals(Number instance, Object? obj);

    ///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> values are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.operator ==(double, double)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool DoubleOpEquality(Number left, Number right);

    ///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> values are not equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.operator !=(double, double)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool DoubleOpInequality(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.operator <(double, double)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool DoubleOpLessThan(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.operator >(double, double)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool DoubleOpGreaterThan(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.operator <=(double, double)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool DoubleOpLessThanOrEqual(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.operator >=(double, double)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool DoubleOpGreaterThanOrEqual(Number left, Number right);

    ///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Double" /> object represent the same value.</summary>
    ///<param name="obj">A <see cref="T:System.Double" /> object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is equal to this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("double.Equals(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleEquals2(Number instance, Number obj);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override double.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleGetHashCode(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
    ///<returns>The string representation of the value of this instance.</returns>    [WhiteList("override double.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DoubleToString(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>    [WhiteList("double.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DoubleToString2(Number instance, string? format);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>    [WhiteList("double.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DoubleToString3(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>    [WhiteList("double.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DoubleToString4(Number instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current double instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("double.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryFormat(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("double.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryFormat2(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its double-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
    ///<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static double.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse(string s);

    ///<summary>Converts the string representation of a number in a specified style to its double-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />. A typical value to specify is a combination of <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static double.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse2(string s, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its double-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
    ///<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static double.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse3(string s, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
    ///<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static double.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse4(string s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent.</summary>
    ///<param name="s">A character span that contains the number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />.  A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static double.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<param name="result">When this method returns, contains the double-precision floating-point number equivalent of the <paramref name="s" /> parameter, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or is not a number in a valid format. It also fails on .NET Framework and .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.TryParse(string, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse(string? s, OutValue<Number> result);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A character span that contains the string representation of the number to convert.</param>
    ///<param name="result">When this method returns, contains the double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="s" /> parameter, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or empty. If <paramref name="s" /> is a valid number less than <see cref="F:System.Double.MinValue">Double.MinValue</see>, <paramref name="result" /> is <see cref="F:System.Double.NegativeInfinity" />. If <paramref name="s" /> is a valid number greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>, <paramref name="result" /> is <see cref="F:System.Double.PositiveInfinity" />. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.TryParse(System.ReadOnlySpan<char>, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse2(Uint32Array s, OutValue<Number> result);

    ///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its double-precision floating-point number equivalent.</summary>
    ///<param name="utf8Text">A read-only UTF-8 character span that contains the number to convert.</param>
    ///<param name="result">When this method returns, contains a double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="utf8Text" /> if the conversion succeeded or zero if the conversion failed. The conversion fails if the <paramref name="utf8Text" /> is <see cref="P:System.ReadOnlySpan`1.Empty" /> or is not in a valid format. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.TryParse(System.ReadOnlySpan<byte>, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse3(Uint8Array utf8Text, OutValue<Number> result);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string containing a number to convert.</param>
    ///<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains a double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or is not in a format compliant with <paramref name="style" />, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumeration constants. It also fails on .NET Framework or .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.SByte.MinValue">SByte.MinValue</see> or greater than <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse4(string? s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Converts a character span containing the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A read-only character span that contains the number to convert.</param>
    ///<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns and if the conversion succeeded, contains a double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="s" />. Contains zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, an empty character span, or not a number in a format compliant with <paramref name="style" />. If <paramref name="s" /> is a valid number less than <see cref="F:System.Double.MinValue">Double.MinValue</see>, <paramref name="result" /> is <see cref="F:System.Double.NegativeInfinity" />. If <paramref name="s" /> is a valid number greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>, <paramref name="result" /> is <see cref="F:System.Double.PositiveInfinity" />. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static double.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Double" />.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.Double" />.</returns>    [WhiteList("double.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode DoubleGetTypeCode(Number instance);

    ///<summary>Determines if a value is a power of two.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.IsPow2(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsPow2(Number value);

    ///<summary>Computes the log2 of a value.</summary>
    ///<param name="value">The value whose log2 is to be computed.</param>
    ///<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static double.Log2(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLog2(Number value);

    ///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">E</code> raised to the power of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Exp(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleExp(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">Ex - 1</code></returns>    [WhiteList("static double.ExpM1(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleExpM1(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">2x</code></returns>    [WhiteList("static double.Exp2(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleExp2(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">2x - 1</code></returns>    [WhiteList("static double.Exp2M1(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleExp2M1(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">10x</code></returns>    [WhiteList("static double.Exp10(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleExp10(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">10x - 1</code></returns>    [WhiteList("static double.Exp10M1(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleExp10M1(Number x);

    ///<summary>Computes the ceiling of a value.</summary>
    ///<param name="x">The value whose ceiling is to be computed.</param>
    ///<returns>The ceiling of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Ceiling(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCeiling(Number x);

    ///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
    ///<param name="value">The value to be converted.</param>
    ///<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static double.ConvertToInteger<TInteger>(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TInteger DoubleConvertToInteger<TInteger>(Number value);

    ///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
    ///<param name="value">The value to be converted.</param>
    ///<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static double.ConvertToIntegerNative<TInteger>(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TInteger DoubleConvertToIntegerNative<TInteger>(Number value);

    ///<summary>Computes the floor of a value.</summary>
    ///<param name="x">The value whose floor is to be computed.</param>
    ///<returns>The floor of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Floor(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleFloor(Number x);

    ///<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
    ///<param name="x">The value to round.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using the default rounding mode.</returns>    [WhiteList("static double.Round(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleRound(Number x);

    ///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
    ///<param name="x">The value to round.</param>
    ///<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using the default rounding mode.</returns>    [WhiteList("static double.Round(double, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleRound2(Number x, Number digits);

    ///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
    ///<param name="x">The value to round.</param>
    ///<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using <code data-dev-comment-type="paramref">mode</code>.</returns>    [WhiteList("static double.Round(double, System.MidpointRounding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleRound3(Number x, System.MidpointRounding mode);

    ///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
    ///<param name="x">The value to round.</param>
    ///<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using <code data-dev-comment-type="paramref">mode</code>.</returns>    [WhiteList("static double.Round(double, int, System.MidpointRounding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleRound4(Number x, Number digits, System.MidpointRounding mode);

    ///<summary>Truncates a value.</summary>
    ///<param name="x">The value to truncate.</param>
    ///<returns>The truncation of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Truncate(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleTruncate(Number x);

    ///<summary>Computes the arc-tangent of the quotient of two values.</summary>
    ///<param name="y">The y-coordinate of a point.</param>
    ///<param name="x">The x-coordinate of a point.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Atan2(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAtan2(Number y, Number x);

    ///<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="y">The y-coordinate of a point.</param>
    ///<param name="x">The x-coordinate of a point.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.Atan2Pi(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAtan2Pi(Number y, Number x);

    ///<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
    ///<param name="x">The value to be bitwise decremented.</param>
    ///<returns>The smallest value that compares less than <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.BitDecrement(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleBitDecrement(Number x);

    ///<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
    ///<param name="x">The value to be bitwise incremented.</param>
    ///<returns>The smallest value that compares greater than <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.BitIncrement(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleBitIncrement(Number x);

    ///<summary>Computes the fused multiply-add of three values.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
    ///<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
    ///<param name="addend">The value that is added to the product of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</param>
    ///<returns>The result of <code data-dev-comment-type="paramref">left</code> times <code data-dev-comment-type="paramref">right</code> plus <code data-dev-comment-type="paramref">addend</code> computed as one ternary operation.</returns>    [WhiteList("static double.FusedMultiplyAdd(double, double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleFusedMultiplyAdd(Number left, Number right, Number addend);

    ///<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
    ///<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
    ///<returns>The remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code> as specified by IEEE 754.</returns>    [WhiteList("static double.Ieee754Remainder(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleIeee754Remainder(Number left, Number right);

    ///<summary>Computes the integer logarithm of a value.</summary>
    ///<param name="x">The value whose integer logarithm is to be computed.</param>
    ///<returns>The integer logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.ILogB(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleILogB(Number x);

    ///<summary>Performs a linear interpolation between two values based on the given weight.</summary>
    ///<param name="value1">The first value, which is intended to be the lower bound.</param>
    ///<param name="value2">The second value, which is intended to be the upper bound.</param>
    ///<param name="amount">A value, intended to be between 0 and 1, that indicates the weight of the interpolation.</param>
    ///<returns>The interpolated value.</returns>    [WhiteList("static double.Lerp(double, double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLerp(Number value1, Number value2, Number amount);

    ///<summary>Computes an estimate of the reciprocal of a value.</summary>
    ///<param name="x">The value whose estimate of the reciprocal is to be computed.</param>
    ///<returns>An estimate of the reciprocal of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.ReciprocalEstimate(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleReciprocalEstimate(Number x);

    ///<summary>Computes an estimate of the reciprocal square root of a value.</summary>
    ///<param name="x">The value whose estimate of the reciprocal square root is to be computed.</param>
    ///<returns>An estimate of the reciprocal square root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.ReciprocalSqrtEstimate(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleReciprocalSqrtEstimate(Number x);

    ///<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
    ///<param name="x">The value which base-radix raised to the power of <code data-dev-comment-type="paramref">n</code> multiplies.</param>
    ///<param name="n">The value to which base-radix is raised before multipliying <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>The product of <code data-dev-comment-type="paramref">x</code> and base-radix raised to the power of <code data-dev-comment-type="paramref">n</code>.</returns>    [WhiteList("static double.ScaleB(double, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleScaleB(Number x, Number n);

    ///<summary>Computes the hyperbolic arc-cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
    ///<returns>The hyperbolic arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Acosh(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAcosh(Number x);

    ///<summary>Computes the hyperbolic arc-sine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
    ///<returns>The hyperbolic arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Asinh(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAsinh(Number x);

    ///<summary>Computes the hyperbolic arc-tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
    ///<returns>The hyperbolic arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Atanh(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAtanh(Number x);

    ///<summary>Computes the hyperbolic cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
    ///<returns>The hyperbolic cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Cosh(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCosh(Number x);

    ///<summary>Computes the hyperbolic sine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
    ///<returns>The hyperbolic sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Sinh(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleSinh(Number x);

    ///<summary>Computes the hyperbolic tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
    ///<returns>The hyperbolic tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Tanh(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleTanh(Number x);

    ///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
    ///<param name="x">The value whose natural logarithm is to be computed.</param>
    ///<returns>The natural logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Log(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLog(Number x);

    ///<summary>Computes the logarithm of a value in the specified base.</summary>
    ///<param name="x">The value whose logarithm is to be computed.</param>
    ///<param name="newBase">The base in which the logarithm is to be computed.</param>
    ///<returns>The base-<code data-dev-comment-type="paramref">newBase</code> logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Log(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLog2(Number x, Number newBase);

    ///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
    ///<param name="x">The value to which one is added before computing the natural logarithm.</param>
    ///<returns>  <code data-dev-comment-type="c">log<sub>e</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>    [WhiteList("static double.LogP1(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLogP1(Number x);

    ///<summary>Computes the base-2 logarithm of a value plus one.</summary>
    ///<param name="x">The value to which one is added before computing the base-2 logarithm.</param>
    ///<returns>  <code data-dev-comment-type="c">log<sub>2</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>    [WhiteList("static double.Log2P1(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLog2P1(Number x);

    ///<summary>Computes the base-10 logarithm of a value.</summary>
    ///<param name="x">The value whose base-10 logarithm is to be computed.</param>
    ///<returns>The base-10 logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Log10(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLog10(Number x);

    ///<summary>Computes the base-10 logarithm of a value plus one.</summary>
    ///<param name="x">The value to which one is added before computing the base-10 logarithm.</param>
    ///<returns>  <code data-dev-comment-type="c">log<sub>10</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>    [WhiteList("static double.Log10P1(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleLog10P1(Number x);

    ///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>    [WhiteList("static double.Clamp(double, double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleClamp(Number value, Number min, Number max);

    [WhiteList("static double.ClampNative(double, double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleClampNative(Number value, Number min, Number max);

    ///<summary>Copies the sign of a value to the sign of another value.</summary>
    ///<param name="x" />
    ///<param name="y" />
    ///<param name="value">The value whose magnitude is used in the result.</param>
    ///<param name="sign">The value whose sign is used in the result.</param>
    ///<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>    [WhiteList("static double.CopySign(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCopySign(Number value, Number sign);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.Max(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMax(Number x, Number y);

    [WhiteList("static double.MaxNative(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMaxNative(Number x, Number y);

    ///<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.MaxNumber(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMaxNumber(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.Min(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMin(Number x, Number y);

    [WhiteList("static double.MinNative(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMinNative(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.MinNumber(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMinNumber(Number x, Number y);

    ///<summary>Computes the sign of a value.</summary>
    ///<param name="value">The value whose sign is to be computed.</param>
    ///<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>    [WhiteList("static double.Sign(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleSign(Number value);

    ///<summary>Computes the absolute of a value.</summary>
    ///<param name="value">The value for which to get its absolute.</param>
    ///<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static double.Abs(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAbs(Number value);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static double.CreateChecked<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCreateChecked<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static double.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static double.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.IsEvenInteger(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsEvenInteger(Number value);

    ///<summary>Determines if a value represents an integral value.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.IsInteger(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsInteger(Number value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.IsOddInteger(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsOddInteger(Number value);

    ///<summary>Determines if a value is positive.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.IsPositive(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsPositive(Number value);

    ///<summary>Determines if a value represents a real number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a real number; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.IsRealNumber(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleIsRealNumber(Number value);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.MaxMagnitude(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMaxMagnitude(Number x, Number y);

    ///<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.MaxMagnitudeNumber(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMaxMagnitudeNumber(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.MinMagnitude(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMinMagnitude(Number x, Number y);

    ///<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.MinMagnitudeNumber(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMinMagnitudeNumber(Number x, Number y);

    ///<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
    ///<param name="left">The value to be multiplied with <code data-dev-comment-type="paramref">right</code>.</param>
    ///<param name="right">The value to be multiplied with <code data-dev-comment-type="paramref">left</code>.</param>
    ///<param name="addend">The value to be added to the result of <code data-dev-comment-type="paramref">left</code> multiplied by <code data-dev-comment-type="paramref">right</code>.</param>
    ///<returns>An estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</returns>    [WhiteList("static double.MultiplyAddEstimate(double, double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleMultiplyAddEstimate(Number left, Number right, Number addend);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.TryParse(string, System.IFormatProvider, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse6(string? s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Computes a value raised to a given power.</summary>
    ///<param name="x">The value which is raised to the power of <code data-dev-comment-type="paramref">x</code>.</param>
    ///<param name="y">The power to which <code data-dev-comment-type="paramref">x</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> raised to the power of <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static double.Pow(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoublePow(Number x, Number y);

    ///<summary>Computes the cube-root of a value.</summary>
    ///<param name="x">The value whose cube-root is to be computed.</param>
    ///<returns>The cube-root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Cbrt(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCbrt(Number x);

    ///<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
    ///<param name="x">The value to square and add to <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to square and add to <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>The square root of <code data-dev-comment-type="paramref">x</code>-squared plus <code data-dev-comment-type="paramref">y</code>-squared.</returns>    [WhiteList("static double.Hypot(double, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleHypot(Number x, Number y);

    ///<summary>Computes the n-th root of a value.</summary>
    ///<param name="x">The value whose <code data-dev-comment-type="paramref">n</code>-th root is to be computed.</param>
    ///<param name="n">The degree of the root to be computed.</param>
    ///<returns>The <code data-dev-comment-type="paramref">n</code>-th root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.RootN(double, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleRootN(Number x, Number n);

    ///<summary>Computes the square-root of a value.</summary>
    ///<param name="x">The value whose square-root is to be computed.</param>
    ///<returns>The square-root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Sqrt(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleSqrt(Number x);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static double.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse7(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Computes the arc-cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose arc-cosine is to be computed.</param>
    ///<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Acos(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAcos(Number x);

    ///<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value whose arc-cosine is to be computed.</param>
    ///<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.AcosPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAcosPi(Number x);

    ///<summary>Computes the arc-sine of a value.</summary>
    ///<param name="x">The value, in radians, whose arc-sine is to be computed.</param>
    ///<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Asin(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAsin(Number x);

    ///<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value whose arc-sine is to be computed.</param>
    ///<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.AsinPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAsinPi(Number x);

    ///<summary>Computes the arc-tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose arc-tangent is to be computed.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Atan(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAtan(Number x);

    ///<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
    ///<param name="x">The value whose arc-tangent is to be computed.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.AtanPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleAtanPi(Number x);

    ///<summary>Computes the cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose cosine is to be computed.</param>
    ///<returns>The cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Cos(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCos(Number x);

    ///<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value, in half-revolutions, whose cosine is to be computed.</param>
    ///<returns>The cosine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.CosPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleCosPi(Number x);

    ///<summary>Converts a given value from degrees to radians.</summary>
    ///<param name="degrees">The value to convert to radians.</param>
    ///<returns>The value of <code data-dev-comment-type="paramref">degrees</code> converted to radians.</returns>    [WhiteList("static double.DegreesToRadians(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleDegreesToRadians(Number degrees);

    ///<summary>Converts a given value from radians to degrees.</summary>
    ///<param name="radians">The value to convert to degrees.</param>
    ///<returns>The value of <code data-dev-comment-type="paramref">radians</code> converted to degrees.</returns>    [WhiteList("static double.RadiansToDegrees(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleRadiansToDegrees(Number radians);

    ///<summary>Computes the sine of a value.</summary>
    ///<param name="x">The value, in radians, whose sine is to be computed.</param>
    ///<returns>The sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Sin(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleSin(Number x);

    ///<summary>Computes the sine and cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
    ///<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.SinCos(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (double Sin, double Cos) DoubleSinCos(Number x);

    ///<summary>Computes the sine and cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
    ///<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.SinCosPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (double SinPi, double CosPi) DoubleSinCosPi(Number x);

    ///<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its sine.</param>
    ///<returns>The sine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.SinPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleSinPi(Number x);

    ///<summary>Computes the tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose tangent is to be computed.</param>
    ///<returns>The tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static double.Tan(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleTan(Number x);

    ///<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its tangent.</param>
    ///<returns>The tangent of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static double.TanPi(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleTanPi(Number x);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static double.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse7(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse8(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static double.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DoubleParse8(Uint8Array utf8Text, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static double.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DoubleTryParse9(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<Number> result);
}
