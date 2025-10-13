using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("float")]
public static class SingleModule
{
	//float.MinValue = -3.4028235E+38;

	//float.MaxValue = 3.4028235E+38;

	//float.Epsilon = 1E-45;

	//float.NegativeInfinity = -∞;

	//float.PositiveInfinity = ∞;

	//float.NaN = NaN;

	//float.NegativeZero = -0;

	//float.E = 2.7182817;

	//float.Pi = 3.1415927;

	//float.Tau = 6.2831855;

    [WhiteList("float.Single()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleNew();

    ///<summary>Determines whether the specified value is finite (zero, subnormal or normal).</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if the specified value is finite (zero, subnormal or normal); otherwise, <see langword="false" />.</returns>    [WhiteList("static float.IsFinite(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsFinite(Number f);

    ///<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to <see cref="F:System.Single.PositiveInfinity" /> or <see cref="F:System.Single.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.IsInfinity(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsInfinity(Number f);

    ///<summary>Returns a value that indicates whether the specified value is not a number (<see cref="F:System.Single.NaN" />).</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to not a number (<see cref="F:System.Single.NaN" />); otherwise, <see langword="false" />.</returns>    [WhiteList("static float.IsNaN(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsNaN(Number f);

    ///<summary>Determines whether the specified value is negative.</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if negative, <see langword="false" /> otherwise.</returns>    [WhiteList("static float.IsNegative(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsNegative(Number f);

    ///<summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to <see cref="F:System.Single.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.IsNegativeInfinity(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsNegativeInfinity(Number f);

    ///<summary>Determines whether the specified value is normal.</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="f" /> is normal; <see langword="false" /> otherwise.</returns>    [WhiteList("static float.IsNormal(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsNormal(Number f);

    ///<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to <see cref="F:System.Single.PositiveInfinity" />; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.IsPositiveInfinity(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsPositiveInfinity(Number f);

    ///<summary>Determines whether the specified value is subnormal.</summary>
    ///<param name="f">A single-precision floating-point number.</param>
    ///<returns>  <see langword="true" /> if <paramref name="f" /> is subnormal; <see langword="false" /> otherwise.</returns>    [WhiteList("static float.IsSubnormal(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsSubnormal(Number f);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
    ///<param name="value">An object to compare, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Single" />.</exception>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Single.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or this instance and value are both not a number (<see cref="F:System.Single.NaN" />), <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, OR this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Single.NaN" />), OR <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>    [WhiteList("float.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCompareTo(Number instance, Object? value);

    ///<summary>Compares this instance to a specified single-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified single-precision floating-point number.</summary>
    ///<param name="value">A single-precision floating-point number to compare.</param>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Single.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or both this instance and <paramref name="value" /> are not a number (<see cref="F:System.Single.NaN" />), <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Single.NaN" />).</description></item></list></returns>    [WhiteList("float.CompareTo(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCompareTo2(Number instance, Number value);

    ///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> values are equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.operator ==(float, float)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool SingleOpEquality(Number left, Number right);

    ///<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> values are not equal.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.operator !=(float, float)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool SingleOpInequality(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.operator <(float, float)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool SingleOpLessThan(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.operator >(float, float)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool SingleOpGreaterThan(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.operator <=(float, float)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool SingleOpLessThanOrEqual(Number left, Number right);

    ///<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
    ///<param name="left">The first value to compare.</param>
    ///<param name="right">The second value to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.operator >=(float, float)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool SingleOpGreaterThanOrEqual(Number left, Number right);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Single" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override float.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleEquals(Number instance, Object? obj);

    ///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Single" /> object represent the same value.</summary>
    ///<param name="obj">An object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is equal to this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("float.Equals(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleEquals2(Number instance, Number obj);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override float.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleGetHashCode(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
    ///<returns>The string representation of the value of this instance.</returns>    [WhiteList("override float.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string SingleToString(Number instance);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>    [WhiteList("float.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string SingleToString2(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>    [WhiteList("float.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string SingleToString3(Number instance, string? format);

    ///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>    [WhiteList("float.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string SingleToString4(Number instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current float number instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the charactes that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("float.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryFormat(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("float.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryFormat2(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its single-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
    ///<returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static float.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse(string s);

    ///<summary>Converts the string representation of a number in a specified style to its single-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not a number in a valid format.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>A single-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static float.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse2(string s, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its single-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
    ///<returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static float.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse3(string s, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
    ///<returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static float.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse4(string s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
    ///<param name="s">A character span that contains the number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />.  A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value.-or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>A single-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>    [WhiteList("static float.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string representing a number to convert.</param>
    ///<param name="result">When this method returns, contains single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or is not a number in a valid format. It also fails on .NET Framework and .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.TryParse(string, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse(string? s, OutValue<Number> result);

    ///<summary>Converts the string representation of a number in a character span to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">&gt;A character span that contains the string representation of the number to convert.</param>
    ///<param name="result">When this method returns, contains the single-precision floating-point number equivalent of the <paramref name="s" /> parameter, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or empty or is not a number in a valid format. If <paramref name="s" /> is a valid number less than <see cref="F:System.Single.MinValue">Single.MinValue</see>, <paramref name="result" /> is <see cref="F:System.Single.NegativeInfinity" />. If <paramref name="s" /> is a valid number greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>, <paramref name="result" /> is <see cref="F:System.Single.PositiveInfinity" />. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.TryParse(System.ReadOnlySpan<char>, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse2(Uint32Array s, OutValue<Number> result);

    ///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its single-precision floating-point number equivalent.</summary>
    ///<param name="utf8Text">A read-only UTF-8 character span that contains the number to convert.</param>
    ///<param name="result">When this method returns, contains a single-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="utf8Text" /> if the conversion succeeded or zero if the conversion failed. The conversion fails if the <paramref name="utf8Text" /> is <see cref="P:System.ReadOnlySpan`1.Empty" /> or is not in a valid format. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.TryParse(System.ReadOnlySpan<byte>, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse3(Uint8Array utf8Text, OutValue<Number> result);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string representing a number to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumeration constants. It also fails on .NET Framework or .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse4(string? s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A read-only character span that contains the number to convert. The span is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumerated constants. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static float.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Single" />.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.Single" />.</returns>    [WhiteList("float.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode SingleGetTypeCode(Number instance);

    ///<summary>Determines if a value is a power of two.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.IsPow2(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsPow2(Number value);

    ///<summary>Computes the log2 of a value.</summary>
    ///<param name="value">The value whose log2 is to be computed.</param>
    ///<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static float.Log2(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLog2(Number value);

    ///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">E</code> raised to the power of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Exp(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleExp(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">Ex - 1</code></returns>    [WhiteList("static float.ExpM1(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleExpM1(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">2x</code></returns>    [WhiteList("static float.Exp2(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleExp2(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">2x - 1</code></returns>    [WhiteList("static float.Exp2M1(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleExp2M1(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">10x</code></returns>    [WhiteList("static float.Exp10(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleExp10(Number x);

    ///<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
    ///<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="c">10x - 1</code></returns>    [WhiteList("static float.Exp10M1(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleExp10M1(Number x);

    ///<summary>Computes the ceiling of a value.</summary>
    ///<param name="x">The value whose ceiling is to be computed.</param>
    ///<returns>The ceiling of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Ceiling(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCeiling(Number x);

    ///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
    ///<param name="value">The value to be converted.</param>
    ///<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static float.ConvertToInteger<TInteger>(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TInteger SingleConvertToInteger<TInteger>(Number value);

    ///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
    ///<param name="value">The value to be converted.</param>
    ///<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static float.ConvertToIntegerNative<TInteger>(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TInteger SingleConvertToIntegerNative<TInteger>(Number value);

    ///<summary>Computes the floor of a value.</summary>
    ///<param name="x">The value whose floor is to be computed.</param>
    ///<returns>The floor of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Floor(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleFloor(Number x);

    ///<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
    ///<param name="x">The value to round.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using the default rounding mode.</returns>    [WhiteList("static float.Round(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleRound(Number x);

    ///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
    ///<param name="x">The value to round.</param>
    ///<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using the default rounding mode.</returns>    [WhiteList("static float.Round(float, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleRound2(Number x, Number digits);

    ///<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
    ///<param name="x">The value to round.</param>
    ///<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using <code data-dev-comment-type="paramref">mode</code>.</returns>    [WhiteList("static float.Round(float, System.MidpointRounding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleRound3(Number x, System.MidpointRounding mode);

    ///<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
    ///<param name="x">The value to round.</param>
    ///<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
    ///<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using <code data-dev-comment-type="paramref">mode</code>.</returns>    [WhiteList("static float.Round(float, int, System.MidpointRounding)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleRound4(Number x, Number digits, System.MidpointRounding mode);

    ///<summary>Truncates a value.</summary>
    ///<param name="x">The value to truncate.</param>
    ///<returns>The truncation of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Truncate(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleTruncate(Number x);

    ///<summary>Computes the arc-tangent of the quotient of two values.</summary>
    ///<param name="y">The y-coordinate of a point.</param>
    ///<param name="x">The x-coordinate of a point.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Atan2(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAtan2(Number y, Number x);

    ///<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="y">The y-coordinate of a point.</param>
    ///<param name="x">The x-coordinate of a point.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.Atan2Pi(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAtan2Pi(Number y, Number x);

    ///<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
    ///<param name="x">The value to be bitwise decremented.</param>
    ///<returns>The smallest value that compares less than <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.BitDecrement(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleBitDecrement(Number x);

    ///<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
    ///<param name="x">The value to be bitwise incremented.</param>
    ///<returns>The smallest value that compares greater than <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.BitIncrement(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleBitIncrement(Number x);

    ///<summary>Computes the fused multiply-add of three values.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
    ///<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
    ///<param name="addend">The value that is added to the product of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</param>
    ///<returns>The result of <code data-dev-comment-type="paramref">left</code> times <code data-dev-comment-type="paramref">right</code> plus <code data-dev-comment-type="paramref">addend</code> computed as one ternary operation.</returns>    [WhiteList("static float.FusedMultiplyAdd(float, float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleFusedMultiplyAdd(Number left, Number right, Number addend);

    ///<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
    ///<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
    ///<returns>The remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code> as specified by IEEE 754.</returns>    [WhiteList("static float.Ieee754Remainder(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleIeee754Remainder(Number left, Number right);

    ///<summary>Computes the integer logarithm of a value.</summary>
    ///<param name="x">The value whose integer logarithm is to be computed.</param>
    ///<returns>The integer logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.ILogB(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleILogB(Number x);

    ///<summary>Performs a linear interpolation between two values based on the given weight.</summary>
    ///<param name="value1">The first value, which is intended to be the lower bound.</param>
    ///<param name="value2">The second value, which is intended to be the upper bound.</param>
    ///<param name="amount">A value, intended to be between 0 and 1, that indicates the weight of the interpolation.</param>
    ///<returns>The interpolated value.</returns>    [WhiteList("static float.Lerp(float, float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLerp(Number value1, Number value2, Number amount);

    ///<summary>Computes an estimate of the reciprocal of a value.</summary>
    ///<param name="x">The value whose estimate of the reciprocal is to be computed.</param>
    ///<returns>An estimate of the reciprocal of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.ReciprocalEstimate(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleReciprocalEstimate(Number x);

    ///<summary>Computes an estimate of the reciprocal square root of a value.</summary>
    ///<param name="x">The value whose estimate of the reciprocal square root is to be computed.</param>
    ///<returns>An estimate of the reciprocal square root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.ReciprocalSqrtEstimate(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleReciprocalSqrtEstimate(Number x);

    ///<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
    ///<param name="x">The value which base-radix raised to the power of <code data-dev-comment-type="paramref">n</code> multiplies.</param>
    ///<param name="n">The value to which base-radix is raised before multipliying <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>The product of <code data-dev-comment-type="paramref">x</code> and base-radix raised to the power of <code data-dev-comment-type="paramref">n</code>.</returns>    [WhiteList("static float.ScaleB(float, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleScaleB(Number x, Number n);

    ///<summary>Computes the hyperbolic arc-cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
    ///<returns>The hyperbolic arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Acosh(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAcosh(Number x);

    ///<summary>Computes the hyperbolic arc-sine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
    ///<returns>The hyperbolic arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Asinh(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAsinh(Number x);

    ///<summary>Computes the hyperbolic arc-tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
    ///<returns>The hyperbolic arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Atanh(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAtanh(Number x);

    ///<summary>Computes the hyperbolic cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
    ///<returns>The hyperbolic cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Cosh(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCosh(Number x);

    ///<summary>Computes the hyperbolic sine of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
    ///<returns>The hyperbolic sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Sinh(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleSinh(Number x);

    ///<summary>Computes the hyperbolic tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
    ///<returns>The hyperbolic tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Tanh(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleTanh(Number x);

    ///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
    ///<param name="x">The value whose natural logarithm is to be computed.</param>
    ///<returns>The natural logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Log(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLog(Number x);

    ///<summary>Computes the logarithm of a value in the specified base.</summary>
    ///<param name="x">The value whose logarithm is to be computed.</param>
    ///<param name="newBase">The base in which the logarithm is to be computed.</param>
    ///<returns>The base-<code data-dev-comment-type="paramref">newBase</code> logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Log(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLog2(Number x, Number newBase);

    ///<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
    ///<param name="x">The value to which one is added before computing the natural logarithm.</param>
    ///<returns>  <code data-dev-comment-type="c">log<sub>e</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>    [WhiteList("static float.LogP1(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLogP1(Number x);

    ///<summary>Computes the base-10 logarithm of a value.</summary>
    ///<param name="x">The value whose base-10 logarithm is to be computed.</param>
    ///<returns>The base-10 logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Log10(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLog10(Number x);

    ///<summary>Computes the base-2 logarithm of a value plus one.</summary>
    ///<param name="x">The value to which one is added before computing the base-2 logarithm.</param>
    ///<returns>  <code data-dev-comment-type="c">log<sub>2</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>    [WhiteList("static float.Log2P1(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLog2P1(Number x);

    ///<summary>Computes the base-10 logarithm of a value plus one.</summary>
    ///<param name="x">The value to which one is added before computing the base-10 logarithm.</param>
    ///<returns>  <code data-dev-comment-type="c">log<sub>10</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>    [WhiteList("static float.Log10P1(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleLog10P1(Number x);

    ///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>    [WhiteList("static float.Clamp(float, float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleClamp(Number value, Number min, Number max);

    [WhiteList("static float.ClampNative(float, float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleClampNative(Number value, Number min, Number max);

    ///<summary>Copies the sign of a value to the sign of another value.</summary>
    ///<param name="x" />
    ///<param name="y" />
    ///<param name="value">The value whose magnitude is used in the result.</param>
    ///<param name="sign">The value whose sign is used in the result.</param>
    ///<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>    [WhiteList("static float.CopySign(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCopySign(Number value, Number sign);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.Max(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMax(Number x, Number y);

    [WhiteList("static float.MaxNative(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMaxNative(Number x, Number y);

    ///<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.MaxNumber(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMaxNumber(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.Min(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMin(Number x, Number y);

    [WhiteList("static float.MinNative(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMinNative(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.MinNumber(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMinNumber(Number x, Number y);

    ///<summary>Computes the sign of a value.</summary>
    ///<param name="value">The value whose sign is to be computed.</param>
    ///<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>    [WhiteList("static float.Sign(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleSign(Number value);

    ///<summary>Computes the absolute of a value.</summary>
    ///<param name="value">The value for which to get its absolute.</param>
    ///<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static float.Abs(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAbs(Number value);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>    [WhiteList("static float.CreateChecked<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCreateChecked<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static float.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>    [WhiteList("static float.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.IsEvenInteger(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsEvenInteger(Number value);

    ///<summary>Determines if a value represents an integral value.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.IsInteger(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsInteger(Number value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.IsOddInteger(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsOddInteger(Number value);

    ///<summary>Determines if a value is positive.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.IsPositive(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsPositive(Number value);

    ///<summary>Determines if a value represents a real number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a real number; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.IsRealNumber(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleIsRealNumber(Number value);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.MaxMagnitude(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMaxMagnitude(Number x, Number y);

    ///<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.MaxMagnitudeNumber(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMaxMagnitudeNumber(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.MinMagnitude(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMinMagnitude(Number x, Number y);

    ///<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.MinMagnitudeNumber(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMinMagnitudeNumber(Number x, Number y);

    ///<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
    ///<param name="left">The value to be multiplied with <code data-dev-comment-type="paramref">right</code>.</param>
    ///<param name="right">The value to be multiplied with <code data-dev-comment-type="paramref">left</code>.</param>
    ///<param name="addend">The value to be added to the result of <code data-dev-comment-type="paramref">left</code> multiplied by <code data-dev-comment-type="paramref">right</code>.</param>
    ///<returns>An estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</returns>    [WhiteList("static float.MultiplyAddEstimate(float, float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleMultiplyAddEstimate(Number left, Number right, Number addend);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.TryParse(string, System.IFormatProvider, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse6(string? s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Computes a value raised to a given power.</summary>
    ///<param name="x">The value which is raised to the power of <code data-dev-comment-type="paramref">x</code>.</param>
    ///<param name="y">The power to which <code data-dev-comment-type="paramref">x</code> is raised.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> raised to the power of <code data-dev-comment-type="paramref">y</code>.</returns>    [WhiteList("static float.Pow(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SinglePow(Number x, Number y);

    ///<summary>Computes the cube-root of a value.</summary>
    ///<param name="x">The value whose cube-root is to be computed.</param>
    ///<returns>The cube-root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Cbrt(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCbrt(Number x);

    ///<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
    ///<param name="x">The value to square and add to <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to square and add to <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>The square root of <code data-dev-comment-type="paramref">x</code>-squared plus <code data-dev-comment-type="paramref">y</code>-squared.</returns>    [WhiteList("static float.Hypot(float, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleHypot(Number x, Number y);

    ///<summary>Computes the n-th root of a value.</summary>
    ///<param name="x">The value whose <code data-dev-comment-type="paramref">n</code>-th root is to be computed.</param>
    ///<param name="n">The degree of the root to be computed.</param>
    ///<returns>The <code data-dev-comment-type="paramref">n</code>-th root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.RootN(float, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleRootN(Number x, Number n);

    ///<summary>Computes the square-root of a value.</summary>
    ///<param name="x">The value whose square-root is to be computed.</param>
    ///<returns>The square-root of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Sqrt(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleSqrt(Number x);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static float.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse7(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Computes the arc-cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose arc-cosine is to be computed.</param>
    ///<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Acos(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAcos(Number x);

    ///<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value whose arc-cosine is to be computed.</param>
    ///<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.AcosPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAcosPi(Number x);

    ///<summary>Computes the arc-sine of a value.</summary>
    ///<param name="x">The value, in radians, whose arc-sine is to be computed.</param>
    ///<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Asin(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAsin(Number x);

    ///<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value whose arc-sine is to be computed.</param>
    ///<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.AsinPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAsinPi(Number x);

    ///<summary>Computes the arc-tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose arc-tangent is to be computed.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Atan(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAtan(Number x);

    ///<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
    ///<param name="x">The value whose arc-tangent is to be computed.</param>
    ///<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.AtanPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleAtanPi(Number x);

    ///<summary>Computes the cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose cosine is to be computed.</param>
    ///<returns>The cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Cos(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCos(Number x);

    ///<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value, in half-revolutions, whose cosine is to be computed.</param>
    ///<returns>The cosine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.CosPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleCosPi(Number x);

    ///<summary>Converts a given value from degrees to radians.</summary>
    ///<param name="degrees">The value to convert to radians.</param>
    ///<returns>The value of <code data-dev-comment-type="paramref">degrees</code> converted to radians.</returns>    [WhiteList("static float.DegreesToRadians(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleDegreesToRadians(Number degrees);

    ///<summary>Converts a given value from radians to degrees.</summary>
    ///<param name="radians">The value to convert to degrees.</param>
    ///<returns>The value of <code data-dev-comment-type="paramref">radians</code> converted to degrees.</returns>    [WhiteList("static float.RadiansToDegrees(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleRadiansToDegrees(Number radians);

    ///<summary>Computes the sine of a value.</summary>
    ///<param name="x">The value, in radians, whose sine is to be computed.</param>
    ///<returns>The sine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Sin(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleSin(Number x);

    ///<summary>Computes the sine and cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
    ///<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.SinCos(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (float Sin, float Cos) SingleSinCos(Number x);

    ///<summary>Computes the sine and cosine of a value.</summary>
    ///<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
    ///<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.SinCosPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (float SinPi, float CosPi) SingleSinCosPi(Number x);

    ///<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its sine.</param>
    ///<returns>The sine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.SinPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleSinPi(Number x);

    ///<summary>Computes the tangent of a value.</summary>
    ///<param name="x">The value, in radians, whose tangent is to be computed.</param>
    ///<returns>The tangent of <code data-dev-comment-type="paramref">x</code>.</returns>    [WhiteList("static float.Tan(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleTan(Number x);

    ///<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
    ///<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its tangent.</param>
    ///<returns>The tangent of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>    [WhiteList("static float.TanPi(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleTanPi(Number x);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static float.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse7(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse8(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>    [WhiteList("static float.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number SingleParse8(Uint8Array utf8Text, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static float.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool SingleTryParse9(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<Number> result);
}
