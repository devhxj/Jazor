using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.TimeOnly")]
public static class TimeOnlyModule
{
    [WhiteList("System.TimeOnly.TimeOnly()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyNew();

    [WhiteList("static System.TimeOnly.MinValue.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetMinValue(Number instance);

    [WhiteList("static System.TimeOnly.MaxValue.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetMaxValue(Number instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour and the minute.</summary>
    ///<param name="hour">The hours (0 through 23).</param>
    ///<param name="minute">The minutes (0 through 59).</param>    [WhiteList("System.TimeOnly.TimeOnly(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyNew2(Number hour, Number minute);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, and second.</summary>
    ///<param name="hour">The hours (0 through 23).</param>
    ///<param name="minute">The minutes (0 through 59).</param>
    ///<param name="second">The seconds (0 through 59).</param>    [WhiteList("System.TimeOnly.TimeOnly(int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyNew3(Number hour, Number minute, Number second);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, second, and millisecond.</summary>
    ///<param name="hour">The hours (0 through 23).</param>
    ///<param name="minute">The minutes (0 through 59).</param>
    ///<param name="second">The seconds (0 through 59).</param>
    ///<param name="millisecond">The millisecond (0 through 999).</param>    [WhiteList("System.TimeOnly.TimeOnly(int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyNew4(Number hour, Number minute, Number second, Number millisecond);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, second, millisecond, and microsecond.</summary>
    ///<param name="hour">The hours (0 through 23).</param>
    ///<param name="minute">The minutes (0 through 59).</param>
    ///<param name="second">The seconds (0 through 59).</param>
    ///<param name="millisecond">The millisecond (0 through 999).</param>
    ///<param name="microsecond">The microsecond (0 through 999).</param>    [WhiteList("System.TimeOnly.TimeOnly(int, int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyNew5(Number hour, Number minute, Number second, Number millisecond, Number microsecond);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure using a specified number of ticks.</summary>
    ///<param name="ticks">A time of day expressed in the number of 100-nanosecond units since 00:00:00.0000000.</param>    [WhiteList("System.TimeOnly.TimeOnly(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyNew6(BigInt ticks);

    [WhiteList("System.TimeOnly.Hour.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetHour(Number instance);

    [WhiteList("System.TimeOnly.Minute.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetMinute(Number instance);

    [WhiteList("System.TimeOnly.Second.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetSecond(Number instance);

    [WhiteList("System.TimeOnly.Millisecond.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetMillisecond(Number instance);

    [WhiteList("System.TimeOnly.Microsecond.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetMicrosecond(Number instance);

    [WhiteList("System.TimeOnly.Nanosecond.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetNanosecond(Number instance);

    [WhiteList("System.TimeOnly.Ticks.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeOnlyGetTicks(Number instance);

    ///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the value of the specified time span to the value of this instance.</summary>
    ///<param name="value">A positive or negative time interval.</param>
    ///<returns>An object whose value is the sum of the time represented by this instance and the time interval represented by value.</returns>    [WhiteList("System.TimeOnly.Add(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyAdd(Number instance, BigInt value);

    ///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the value of the specified time span to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
    ///<param name="value">A positive or negative time interval.</param>
    ///<param name="wrappedDays">When this method returns, contains the number of excess days, if any, that resulted from wrapping during this addition operation.</param>
    ///<returns>An object whose value is the sum of the time represented by this instance and the time interval represented by value.</returns>    [WhiteList("System.TimeOnly.Add(System.TimeSpan, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyAdd2(Number instance, BigInt value, OutValue<Number> wrappedDays);

    ///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of hours to the value of this instance.</summary>
    ///<param name="value">A number of whole and fractional hours. The value parameter can be negative or positive.</param>
    ///<returns>An object whose value is the sum of the time represented by this instance and the number of hours represented by value.</returns>    [WhiteList("System.TimeOnly.AddHours(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyAddHours(Number instance, Number value);

    ///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of hours to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
    ///<param name="value">A number of whole and fractional hours. The value parameter can be negative or positive.</param>
    ///<param name="wrappedDays">When this method returns, contains the number of excess days, if any, that resulted from wrapping during this addition operation.</param>
    ///<returns>An object whose value is the sum of the time represented by this instance and the number of hours represented by value.</returns>    [WhiteList("System.TimeOnly.AddHours(double, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyAddHours2(Number instance, Number value, OutValue<Number> wrappedDays);

    ///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of minutes to the value of this instance.</summary>
    ///<param name="value">A number of whole and fractional minutes. The value parameter can be negative or positive.</param>
    ///<returns>An object whose value is the sum of the time represented by this instance and the number of minutes represented by value.</returns>    [WhiteList("System.TimeOnly.AddMinutes(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyAddMinutes(Number instance, Number value);

    ///<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of minutes to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
    ///<param name="value">A number of whole and fractional minutes. The value parameter can be negative or positive.</param>
    ///<param name="wrappedDays">When this method returns, contains the number of excess days, if any, that resulted from wrapping during this addition operation.</param>
    ///<returns>An object whose value is the sum of the time represented by this instance and the number of minutes represented by value.</returns>    [WhiteList("System.TimeOnly.AddMinutes(double, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyAddMinutes2(Number instance, Number value, OutValue<Number> wrappedDays);

    ///<summary>Determines if a time falls within the range provided.            Supports both "normal" ranges such as 10:00-12:00, and ranges that span midnight such as 23:00-01:00.</summary>
    ///<param name="start">The starting time of day, inclusive.</param>
    ///<param name="end">The ending time of day, exclusive.</param>
    ///<returns>  <see langword="true" />, if the time falls within the range, <see langword="false" /> otherwise.</returns>    [WhiteList("System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyIsBetween(Number instance, Number start, Number end);

    ///<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>are equal.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if left and right represent the same time; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.operator ==(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool TimeOnlyOpEquality(Number left, Number right);

    ///<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> are not equal.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if left and right do not represent the same time; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.operator !=(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool TimeOnlyOpInequality(Number left, Number right);

    ///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if left is later than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.operator >(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool TimeOnlyOpGreaterThan(Number left, Number right);

    ///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if left is the same as or later than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.operator >=(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool TimeOnlyOpGreaterThanOrEqual(Number left, Number right);

    ///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if left is earlier than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.operator <(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool TimeOnlyOpLessThan(Number left, Number right);

    ///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if left is the same as or earlier than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.operator <=(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool TimeOnlyOpLessThanOrEqual(Number left, Number right);

    ///<summary>Gives the elapsed time between two points on a circular clock, which will always be a positive value.</summary>
    ///<param name="t1">The first <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="t2">The second <see cref="T:System.TimeOnly" /> instance..</param>
    ///<returns>The elapsed time between <paramref name="t1" /> and <paramref name="t2" />.</returns>    [WhiteList("static System.TimeOnly.operator -(System.TimeOnly, System.TimeOnly)")]
    [ECMAScriptLiteral("{0} - {1}")]
	public extern static BigInt TimeOnlyOpSubtraction(Number t1, Number t2);

    ///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" /> and <see cref="P:System.TimeOnly.Minute" />.</summary>
    ///<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>    [WhiteList("System.TimeOnly.Deconstruct(out int, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void TimeOnlyDeconstruct(Number instance, OutValue<Number> hour, OutValue<Number> minute);

    ///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, and <see cref="P:System.TimeOnly.Second" />.</summary>
    ///<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="second">When this method returns, contains the <see cref="P:System.TimeOnly.Second" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>    [WhiteList("System.TimeOnly.Deconstruct(out int, out int, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void TimeOnlyDeconstruct2(Number instance, OutValue<Number> hour, OutValue<Number> minute, OutValue<Number> second);

    ///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, and <see cref="P:System.TimeOnly.Millisecond" />.</summary>
    ///<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="second">When this method returns, contains the <see cref="P:System.TimeOnly.Second" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="millisecond">When this method returns, contains the <see cref="P:System.TimeOnly.Millisecond" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>    [WhiteList("System.TimeOnly.Deconstruct(out int, out int, out int, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void TimeOnlyDeconstruct3(Number instance, OutValue<Number> hour, OutValue<Number> minute, OutValue<Number> second, OutValue<Number> millisecond);

    ///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, <see cref="P:System.TimeOnly.Millisecond" />, and <see cref="P:System.TimeOnly.Microsecond" />.</summary>
    ///<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="second">When this method returns, contains the <see cref="P:System.TimeOnly.Second" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="millisecond">When this method returns, contains the <see cref="P:System.TimeOnly.Millisecond" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
    ///<param name="microsecond">When this method returns, contains the <see cref="P:System.TimeOnly.Microsecond" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>    [WhiteList("System.TimeOnly.Deconstruct(out int, out int, out int, out int, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void TimeOnlyDeconstruct4(Number instance, OutValue<Number> hour, OutValue<Number> minute, OutValue<Number> second, OutValue<Number> millisecond, OutValue<Number> microsecond);

    ///<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a time span representing the time elapsed since midnight.</summary>
    ///<param name="timeSpan">The time interval measured since midnight. This value has to be positive and not exceeding the time of the day.</param>
    ///<returns>A <see cref="T:System.TimeOnly" /> object representing the time elapsed since midnight using the specified time span value.</returns>    [WhiteList("static System.TimeOnly.FromTimeSpan(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyFromTimeSpan(BigInt timeSpan);

    ///<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a <see cref="T:System.DateTime" /> representing the time of the day in this <see cref="T:System.DateTime" /> object.</summary>
    ///<param name="dateTime">The <see cref="T:System.DateTime" /> object to extract the time of the day from.</param>
    ///<returns>A <see cref="T:System.TimeOnly" /> object representing time of the day specified in the <see cref="T:System.DateTime" /> object.</returns>    [WhiteList("static System.TimeOnly.FromDateTime(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyFromDateTime(Date dateTime);

    ///<summary>Convert the current <see cref="T:System.TimeOnly" /> instance to a <see cref="T:System.TimeSpan" /> object.</summary>
    ///<returns>A <see cref="T:System.TimeSpan" /> object spanning to the time specified in the current <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("System.TimeOnly.ToTimeSpan()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeOnlyToTimeSpan(Number instance);

    ///<summary>Compares the value of this instance to a specified <see cref="T:System.TimeOnly" /> value and indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
    ///<param name="value">The object to compare to the current instance.</param>
    ///<returns>A signed number indicating the relative values of this instance and the value parameter.- Less than zero if this instance is earlier than value.- Zero if this instance is the same as value.- Greater than zero if this instance is later than value.</returns>    [WhiteList("System.TimeOnly.CompareTo(System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyCompareTo(Number instance, Number value);

    ///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.TimeOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
    ///<param name="value">A boxed object to compare, or <see langword="null" />.</param>
    ///<returns>A signed number indicating the relative values of this instance and the value parameter.            Less than zero if this instance is earlier than value.            Zero if this instance is the same as value.            Greater than zero if this instance is later than value.</returns>    [WhiteList("System.TimeOnly.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyCompareTo2(Number instance, Object? value);

    ///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.TimeOnly" /> instance.</summary>
    ///<param name="value">The object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if the value parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("System.TimeOnly.Equals(System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyEquals(Number instance, Number value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="value">The object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if value is an instance of <see cref="T:System.TimeOnly" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.TimeOnly.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyEquals2(Number instance, Object? value);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override System.TimeOnly.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyGetHashCode(Number instance);

    ///<summary>Converts a memory span that contains string representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent by using culture-specific format information and a formatting style.</summary>
    ///<param name="s">The memory span that contains the time to parse.</param>
    ///<param name="provider">The culture-specific format information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <code data-dev-comment-type="paramref">s</code>. A typical value to specify is <xref data-throw-if-not-resolved="true" uid="System.Globalization.DateTimeStyles.None"></xref>.</param>
    ///<returns>A <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> instance.</returns>    [WhiteList("static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParse(Uint32Array s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the time to convert.</param>
    ///<param name="format">The format specifier that defines the required format of <paramref name="s" />.</param>
    ///<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the time to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact2(Uint32Array s, string[] formats);

    ///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the time to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact3(Uint32Array s, string[] formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using the conventions of the current culture.</summary>
    ///<param name="s">The string to parse.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParse2(string s);

    ///<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
    ///<param name="s">The string containing the time to parse.</param>
    ///<param name="provider">The culture-specific format information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in s for the parse operation to succeed, and that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A<see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParse3(string s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
    ///<param name="s">A string containing a time to convert.</param>
    ///<param name="format">A format specifier that defines the required format of <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact4(string s, string format);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
    ///<param name="s">A string containing the time to convert.</param>
    ///<param name="format">The format specifier that defines the required format of <paramref name="s" />.</param>
    ///<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.TimeOnly" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact5(string s, string format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span to a <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the time to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(string, string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact6(string s, string[] formats);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A string containing the time to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
    ///<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>    [WhiteList("static System.TimeOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParseExact7(string s, string[] formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span representation of a time to its TimeOnly equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing the time to convert.</param>
    ///<param name="result">When this method returns, contains the TimeOnly value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or MinValue if the conversion failed. The conversion fails if <paramref name="s" /> is the empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParse(Uint32Array s, OutValue<Number> result);

    ///<summary>Converts the specified span representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the characters that represent a time to convert.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <code data-dev-comment-type="paramref">s</code>. A typical value to specify is <xref data-throw-if-not-resolved="true" uid="System.Globalization.DateTimeStyles.None"></xref>.</param>
    ///<param name="result">When this method returns, contains the <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> value equivalent to the time contained in <code data-dev-comment-type="paramref">s</code>, if the conversion succeeded, or System.TimeOnly.MinValue?text=TimeOnly.MinValue if the conversion failed. The conversion fails if <code data-dev-comment-type="paramref">s</code> is an empty string or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if<code data-dev-comment-type="paramref">s</code> was converted successfully; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParse2(Uint32Array s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Number> result);

    ///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the time to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in <paramref name="format" />. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact(Uint32Array s, Uint32Array format, OutValue<Number> result);

    ///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the time to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact2(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Number> result);

    ///<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The span containing the time to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact3(Uint32Array s, string?[]? formats, OutValue<Number> result);

    ///<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The span containing the time to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed time. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact4(Uint32Array s, string?[]? formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Number> result);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the time to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParse(string, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParse3(string? s, OutValue<Number> result);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the time to convert.</param>
    ///<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParse4(string? s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Number> result);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the time to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(string, string, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact5(string? s, string? format, OutValue<Number> result);

    ///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing a time to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact6(string? s, string? format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Number> result);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The string containing the time to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(string, string[], out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact7(string? s, string?[]? formats, OutValue<Number> result);

    ///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The string containing the time to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParseExact8(string? s, string?[]? formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Number> result);

    ///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent long date string representation.</summary>
    ///<returns>The long time string representation of the current instance.</returns>    [WhiteList("System.TimeOnly.ToLongTimeString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeOnlyToLongTimeString(Number instance);

    ///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation.</summary>
    ///<returns>The short time string representation of the current instance.</returns>    [WhiteList("System.TimeOnly.ToShortTimeString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeOnlyToShortTimeString(Number instance);

    ///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation using the formatting conventions of the current culture.</summary>
    ///<returns>The short time string representation of the current instance.</returns>    [WhiteList("override System.TimeOnly.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeOnlyToString(Number instance);

    ///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
    ///<param name="format">A standard or custom time format string.</param>
    ///<returns>A string representation of the current instance with the specified format and the formatting conventions of the current culture.</returns>    [WhiteList("System.TimeOnly.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeOnlyToString2(Number instance, string? format);

    ///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">The culture-specific formatting information.</param>
    ///<returns>A string representation of the current instance as specified by the provider.</returns>    [WhiteList("System.TimeOnly.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeOnlyToString3(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="format">A standard or custom time format string.</param>
    ///<param name="provider">The culture-specific formatting information.</param>
    ///<returns>A string representation of value of the current instance.</returns>    [WhiteList("System.TimeOnly.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeOnlyToString4(Number instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current TimeOnly instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("System.TimeOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryFormat(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("System.TimeOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryFormat2(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Parses a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static System.TimeOnly.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParse4(string s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">A string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> value equivalent to the time contained in <code data-dev-comment-type="paramref">s</code>, if the conversion succeeded, or <xref data-throw-if-not-resolved="true" uid="System.TimeOnly.MinValue"></xref> if the conversion failed. The conversion fails if <code data-dev-comment-type="paramref">s</code> is the empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was parsed successfully; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.TryParse(string, System.IFormatProvider, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParse5(string? s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">A span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeOnlyParse5(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">A span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> value equivalent to the time contained in <code data-dev-comment-type="paramref">s</code>, if the conversion succeeded, or <xref data-throw-if-not-resolved="true" uid="System.TimeOnly.MinValue"></xref> if the conversion failed. The conversion fails if <code data-dev-comment-type="paramref">s</code> is the empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeOnlyTryParse6(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);
}
