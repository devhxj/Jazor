using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.DateOnly")]
public static class DateOnlyModule
{
    [WhiteList("System.DateOnly.DateOnly()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyNew();

    [WhiteList("static System.DateOnly.MinValue.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyGetMinValue(Date instance);

    [WhiteList("static System.DateOnly.MaxValue.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyGetMaxValue(Date instance);

    ///<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified year, month, and day.</summary>
    ///<param name="year">The year (1 through 9999).</param>
    ///<param name="month">The month (1 through 12).</param>
    ///<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>    [WhiteList("System.DateOnly.DateOnly(int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyNew2(Number year, Number month, Number day);

    ///<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified year, month, and day for the specified calendar.</summary>
    ///<param name="year">The year (1 through the number of years in calendar).</param>
    ///<param name="month">The month (1 through the number of months in calendar).</param>
    ///<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
    ///<param name="calendar">The calendar that is used to interpret year, month, and day.<paramref name="month" />.</param>    [WhiteList("System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyNew3(Number year, Number month, Number day, GregorianCalendar calendar);

    ///<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified number of days.</summary>
    ///<param name="dayNumber">The number of days since January 1, 0001 in the Proleptic Gregorian calendar.</param>
    ///<returns>A <see cref="T:System.DateOnly" /> structure instance to the specified number of days.</returns>    [WhiteList("static System.DateOnly.FromDayNumber(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyFromDayNumber(Number dayNumber);

    [WhiteList("System.DateOnly.Year.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyGetYear(Date instance);

    [WhiteList("System.DateOnly.Month.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyGetMonth(Date instance);

    [WhiteList("System.DateOnly.Day.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyGetDay(Date instance);

    [WhiteList("System.DateOnly.DayOfWeek.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.DayOfWeek DateOnlyGetDayOfWeek(Date instance);

    [WhiteList("System.DateOnly.DayOfYear.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyGetDayOfYear(Date instance);

    [WhiteList("System.DateOnly.DayNumber.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyGetDayNumber(Date instance);

    ///<summary>Adds the specified number of days to the value of this instance.</summary>
    ///<param name="value">The number of days to add. To subtract days, specify a negative number.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">          The resulting value is greater than <see cref="F:System.DateOnly.MaxValue">DateOnly.MaxValue</see>.</exception>
    ///<returns>An instance whose value is the sum of the date represented by this instance and the number of days represented by value.</returns>    [WhiteList("System.DateOnly.AddDays(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyAddDays(Date instance, Number value);

    ///<summary>Adds the specified number of months to the value of this instance.</summary>
    ///<param name="value">A number of months. The months parameter can be negative or positive.</param>
    ///<returns>An object whose value is the sum of the date represented by this instance and months.</returns>    [WhiteList("System.DateOnly.AddMonths(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyAddMonths(Date instance, Number value);

    ///<summary>Adds the specified number of years to the value of this instance.</summary>
    ///<param name="value">A number of years. The value parameter can be negative or positive.</param>
    ///<returns>An object whose value is the sum of the date represented by this instance and the number of years represented by value.</returns>    [WhiteList("System.DateOnly.AddYears(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyAddYears(Date instance, Number value);

    ///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are equal.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if left and right represent the same date; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool DateOnlyOpEquality(Date left, Date right);

    ///<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are not equal.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if left and right do not represent the same date; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool DateOnlyOpInequality(Date left, Date right);

    ///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is later than another specified DateTime.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if left is later than right; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.operator >(System.DateOnly, System.DateOnly)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool DateOnlyOpGreaterThan(Date left, Date right);

    ///<summary>Determines whether one specified DateOnly represents a date that is the same as or later than another specified <see cref="T:System.DateOnly" />.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if left is the same as or later than right; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool DateOnlyOpGreaterThanOrEqual(Date left, Date right);

    ///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is earlier than another specified <see cref="T:System.DateOnly" />.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if left is earlier than right; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.operator <(System.DateOnly, System.DateOnly)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool DateOnlyOpLessThan(Date left, Date right);

    ///<summary>Determines whether one specified <see cref="T:System.DateOnly" /> represents a date that is the same as or earlier than another specified <see cref="T:System.DateOnly" />.</summary>
    ///<param name="left">The first object to compare.</param>
    ///<param name="right">The second object to compare.</param>
    ///<returns>  <see langword="true" /> if left is the same as or earlier than right; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool DateOnlyOpLessThanOrEqual(Date left, Date right);

    ///<summary>Deconstructs <see cref="T:System.DateOnly" /> by <see cref="P:System.DateOnly.Year" />, <see cref="P:System.DateOnly.Month" />, and <see cref="P:System.DateOnly.Day" />.</summary>
    ///<param name="year">When this method returns, represents the <see cref="P:System.DateOnly.Year" /> value of this <see cref="T:System.DateOnly" /> instance.</param>
    ///<param name="month">When this method returns, represents the <see cref="P:System.DateOnly.Month" /> value of this <see cref="T:System.DateOnly" /> instance.</param>
    ///<param name="day">When this method returns, represents the <see cref="P:System.DateOnly.Day" /> value of this <see cref="T:System.DateOnly" /> instance.</param>    [WhiteList("System.DateOnly.Deconstruct(out int, out int, out int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DateOnlyDeconstruct(Date instance, OutValue<Number> year, OutValue<Number> month, OutValue<Number> day);

    ///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
    ///<param name="time">The time of the day.</param>
    ///<returns>The <see cref="T:System.DateTime" /> instance composed of the date of the current <see cref="T:System.DateOnly" /> instance and the time specified by the input time.</returns>    [WhiteList("System.DateOnly.ToDateTime(System.TimeOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyToDateTime(Date instance, Number time);

    ///<summary>Returns a <see cref="T:System.DateTime" /> instance with the specified input kind that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
    ///<param name="time">The time of the day.</param>
    ///<param name="kind">One of the enumeration values that indicates whether ticks specifies a local time, Coordinated Universal Time (UTC), or neither.</param>
    ///<returns>The <see cref="T:System.DateTime" /> instance composed of the date of the current <see cref="T:System.DateOnly" /> instance and the time specified by the input time.</returns>    [WhiteList("System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyToDateTime2(Date instance, Number time, System.DateTimeKind kind);

    ///<summary>Returns a <see cref="T:System.DateOnly" /> instance that is set to the date part of the specified <paramref name="dateTime" />.</summary>
    ///<param name="dateTime">The <see cref="T:System.DateTime" /> instance.</param>
    ///<returns>The <see cref="T:System.DateOnly" /> instance composed of the date part of the specified input time <paramref name="dateTime" /> instance.</returns>    [WhiteList("static System.DateOnly.FromDateTime(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyFromDateTime(Date dateTime);

    ///<summary>Compares the value of this instance to a specified <see cref="T:System.DateOnly" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
    ///<param name="value">The object to compare to the current instance.</param>
    ///<returns>Less than zero if this instance is earlier than value. Greater than zero if this instance is later than value. Zero if this instance is the same as value.</returns>    [WhiteList("System.DateOnly.CompareTo(System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyCompareTo(Date instance, Date value);

    ///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
    ///<param name="value">A boxed object to compare, or <see langword="null" />.</param>
    ///<returns>Less than zero if this instance is earlier than value. Greater than zero if this instance is later than value. Zero if this instance is the same as value.</returns>    [WhiteList("System.DateOnly.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyCompareTo2(Date instance, Object? value);

    ///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateOnly" /> instance.</summary>
    ///<param name="value">The object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if the value parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("System.DateOnly.Equals(System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyEquals(Date instance, Date value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="value">The object to compare to this instance.</param>
    ///<returns>  <see langword="true" /> if value is an instance of DateOnly and equals the value of this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.DateOnly.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyEquals2(Date instance, Object? value);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override System.DateOnly.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DateOnlyGetHashCode(Date instance);

    ///<summary>Converts a memory span that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
    ///<param name="s">The memory span that contains the string to parse.</param>
    ///<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by provider and styles.</returns>    [WhiteList("static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParse(Uint32Array s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the characters that represent a date to convert.</param>
    ///<param name="format">A span containing the characters that represent a format specifier that defines the required format of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>    [WhiteList("static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the characters that represent a date to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>    [WhiteList("static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact2(Uint32Array s, string[] formats);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the characters that represent a date to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>    [WhiteList("static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact3(Uint32Array s, string[] formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using the conventions of the current culture.</summary>
    ///<param name="s">The string that contains the string to parse.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />.</returns>    [WhiteList("static System.DateOnly.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParse2(string s);

    ///<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
    ///<param name="s">The string that contains the string to parse.</param>
    ///<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" /> for the parse operation to succeed, and that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by provider and styles.</returns>    [WhiteList("static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParse3(string s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
    ///<param name="s">A string containing the characters that represent a date to convert.</param>
    ///<param name="format">A string that represent a format specifier that defines the required format of <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format.</returns>    [WhiteList("static System.DateOnly.ParseExact(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact4(string s, string format);

    ///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
    ///<param name="s">A string containing the characters that represent a date to convert.</param>
    ///<param name="format">A string containing the characters that represent a format specifier that defines the required format of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.DateOnly" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>    [WhiteList("static System.DateOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact5(string s, string format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A span containing the characters that represent a date to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>    [WhiteList("static System.DateOnly.ParseExact(string, string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact6(string s, string[] formats);

    ///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
    ///<param name="s">A string containing the characters that represent a date to convert.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
    ///<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>    [WhiteList("static System.DateOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParseExact7(string s, string[] formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing the date to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParse(Uint32Array s, OutValue<Date> result);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the characters that represent a date to convert.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParse2(Uint32Array s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Date> result);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing a date to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact(Uint32Array s, Uint32Array format, OutValue<Date> result);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" />equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing a date to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact2(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Date> result);

    ///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The span containing the string to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if<paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact3(Uint32Array s, string?[]? formats, OutValue<Date> result);

    ///<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The span containing the string to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact4(Uint32Array s, string?[]? formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Date> result);

    ///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the characters representing the date to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParse(string, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParse3(string? s, OutValue<Date> result);

    ///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the characters that represent a date to convert.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParse4(string? s, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Date> result);

    ///<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string containing the characters representing a date to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(string, string, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact5(string? s, string? format, OutValue<Date> result);

    ///<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing a date to convert.</param>
    ///<param name="format">The required format of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact6(string? s, string? format, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Date> result);

    ///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The string containing date to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(string, string[], out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact7(string? s, string?[]? formats, OutValue<Date> result);

    ///<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">The string containing the date to parse.</param>
    ///<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.DateOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParseExact8(string? s, string?[]? formats, Intl.NumberFormat? provider, System.Globalization.DateTimeStyles style, OutValue<Date> result);

    ///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent long date string representation.</summary>
    ///<returns>A string that contains the long date string representation of the current <see cref="T:System.DateOnly" /> object.</returns>    [WhiteList("System.DateOnly.ToLongDateString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DateOnlyToLongDateString(Date instance);

    ///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent short date string representation.</summary>
    ///<returns>A string that contains the short date string representation of the current <see cref="T:System.DateOnly" /> object.</returns>    [WhiteList("System.DateOnly.ToShortDateString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DateOnlyToShortDateString(Date instance);

    ///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the formatting conventions of the current culture.            The <see cref="T:System.DateOnly" /> object will be formatted in short form.</summary>
    ///<returns>A string that contains the short date string representation of the current <see cref="T:System.DateOnly" /> object.</returns>    [WhiteList("override System.DateOnly.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DateOnlyToString(Date instance);

    ///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
    ///<param name="format">A standard or custom date format string.</param>
    ///<returns>A string representation of value of the current <see cref="T:System.DateOnly" /> object as specified by format.</returns>    [WhiteList("System.DateOnly.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DateOnlyToString2(Date instance, string? format);

    ///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>A string representation of value of the current <see cref="T:System.DateOnly" /> object as specified by provider.</returns>    [WhiteList("System.DateOnly.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DateOnlyToString3(Date instance, Intl.NumberFormat? provider);

    ///<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
    ///<param name="format">A standard or custom date format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>A string representation of value of the current <see cref="T:System.DateOnly" /> object as specified by format and provider.</returns>    [WhiteList("System.DateOnly.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string DateOnlyToString4(Date instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current <see cref="T:System.DateOnly" /> instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("System.DateOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryFormat(Date instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("System.DateOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryFormat2(Date instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Parses a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static System.DateOnly.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParse4(string s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParse5(string? s, Intl.NumberFormat? provider, OutValue<Date> result);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>    [WhiteList("static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date DateOnlyParse5(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DateOnlyTryParse6(Uint32Array s, Intl.NumberFormat? provider, OutValue<Date> result);
}
