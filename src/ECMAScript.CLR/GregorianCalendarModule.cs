using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Globalization.GregorianCalendar")]
public static class GregorianCalendarModule
{
	//System.Globalization.GregorianCalendar.ADEra = 1;

    [WhiteList("override System.Globalization.GregorianCalendar.MinSupportedDateTime.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date GregorianCalendarGetMinSupportedDateTime(System.Globalization.GregorianCalendar instance);

    [WhiteList("override System.Globalization.GregorianCalendar.MaxSupportedDateTime.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date GregorianCalendarGetMaxSupportedDateTime(System.Globalization.GregorianCalendar instance);

    [WhiteList("override System.Globalization.GregorianCalendar.AlgorithmType.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.CalendarAlgorithmType GregorianCalendarGetAlgorithmType(System.Globalization.GregorianCalendar instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the default <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>    [WhiteList("System.Globalization.GregorianCalendar.GregorianCalendar()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.GregorianCalendar GregorianCalendarNew();

    ///<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the specified <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
    ///<param name="type">The <see cref="T:System.Globalization.GregorianCalendarTypes" /> value that denotes which language version of the calendar to create.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="type" /> is not a member of the <see cref="T:System.Globalization.GregorianCalendarTypes" /> enumeration.</exception>    [WhiteList("System.Globalization.GregorianCalendar.GregorianCalendar(System.Globalization.GregorianCalendarTypes)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.GregorianCalendar GregorianCalendarNew2(System.Globalization.GregorianCalendarTypes type);

    [WhiteList("virtual System.Globalization.GregorianCalendar.CalendarType.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.GregorianCalendarTypes GregorianCalendarGetCalendarType(System.Globalization.GregorianCalendar instance);

    [WhiteList("virtual System.Globalization.GregorianCalendar.CalendarType.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void GregorianCalendarSetCalendarType(System.Globalization.GregorianCalendar instance, System.Globalization.GregorianCalendarTypes value);

    ///<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to which to add months.</param>
    ///<param name="months">The number of months to add.</param>
    ///<exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="months" /> is less than -120000.     -or-     <paramref name="months" /> is greater than 120000.</exception>
    ///<returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of months to the specified <see cref="T:System.DateTime" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.AddMonths(System.DateTime, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date GregorianCalendarAddMonths(System.Globalization.GregorianCalendar instance, Date time, Number months);

    ///<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to which to add years.</param>
    ///<param name="years">The number of years to add.</param>
    ///<exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range.</exception>
    ///<returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.AddYears(System.DateTime, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date GregorianCalendarAddYears(System.Globalization.GregorianCalendar instance, Date time, Number years);

    ///<summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
    ///<returns>An integer from 1 to 31 that represents the day of the month in <paramref name="time" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetDayOfMonth(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetDayOfMonth(System.Globalization.GregorianCalendar instance, Date time);

    ///<summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
    ///<returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in <paramref name="time" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetDayOfWeek(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.DayOfWeek GregorianCalendarGetDayOfWeek(System.Globalization.GregorianCalendar instance, Date time);

    ///<summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
    ///<returns>An integer from 1 to 366 that represents the day of the year in <paramref name="time" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetDayOfYear(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetDayOfYear(System.Globalization.GregorianCalendar instance, Date time);

    ///<summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="month">An integer from 1 to 12 that represents the month.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.</exception>
    ///<returns>The number of days in the specified month in the specified year in the specified era.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetDaysInMonth(int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetDaysInMonth(System.Globalization.GregorianCalendar instance, Number year, Number month, Number era);

    ///<summary>Returns the number of days in the specified year in the specified era.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.</exception>
    ///<returns>The number of days in the specified year in the specified era.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetDaysInYear(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetDaysInYear(System.Globalization.GregorianCalendar instance, Number year, Number era);

    ///<summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
    ///<returns>An integer that represents the era in <paramref name="time" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetEra(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetEra(System.Globalization.GregorianCalendar instance, Date time);

    [WhiteList("override System.Globalization.GregorianCalendar.Eras.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static int[] GregorianCalendarGetEras(System.Globalization.GregorianCalendar instance);

    ///<summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
    ///<returns>An integer from 1 to 12 that represents the month in <paramref name="time" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetMonth(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetMonth(System.Globalization.GregorianCalendar instance, Date time);

    ///<summary>Returns the number of months in the specified year in the specified era.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.</exception>
    ///<returns>The number of months in the specified year in the specified era.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetMonthsInYear(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetMonthsInYear(System.Globalization.GregorianCalendar instance, Number year, Number era);

    ///<summary>Returns the year in the specified <see cref="T:System.DateTime" />.</summary>
    ///<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
    ///<returns>An integer that represents the year in <paramref name="time" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetYear(System.DateTime)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetYear(System.Globalization.GregorianCalendar instance, Date time);

    ///<summary>Determines whether the specified date in the specified era is a leap day.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="month">An integer from 1 to 12 that represents the month.</param>
    ///<param name="day">An integer from 1 to 31 that represents the day.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.     -or-     <paramref name="day" /> is outside the range supported by the calendar.</exception>
    ///<returns>  <see langword="true" /> if the specified day is a leap day; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.IsLeapDay(int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool GregorianCalendarIsLeapDay(System.Globalization.GregorianCalendar instance, Number year, Number month, Number day, Number era);

    ///<summary>Calculates the leap month for a specified year and era.</summary>
    ///<param name="year">A year.</param>
    ///<param name="era">An era. Specify either <see cref="F:System.Globalization.GregorianCalendar.ADEra" /> or <see langword="GregorianCalendar.Eras[Calendar.CurrentEra]" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than the Gregorian calendar year 1 or greater than the Gregorian calendar year 9999.     -or-     <paramref name="era" /> is not <see cref="F:System.Globalization.GregorianCalendar.ADEra" /> or <see langword="GregorianCalendar.Eras[Calendar.CurrentEra]" />.</exception>
    ///<returns>Always 0 because the Gregorian calendar does not recognize leap months.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.GetLeapMonth(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetLeapMonth(System.Globalization.GregorianCalendar instance, Number year, Number era);

    ///<summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="month">An integer from 1 to 12 that represents the month.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.</exception>
    ///<returns>This method always returns <see langword="false" />, unless overridden by a derived class.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.IsLeapMonth(int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool GregorianCalendarIsLeapMonth(System.Globalization.GregorianCalendar instance, Number year, Number month, Number era);

    ///<summary>Determines whether the specified year in the specified era is a leap year.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.</exception>
    ///<returns>  <see langword="true" /> if the specified year is a leap year; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.IsLeapYear(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool GregorianCalendarIsLeapYear(System.Globalization.GregorianCalendar instance, Number year, Number era);

    ///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
    ///<param name="year">An integer that represents the year.</param>
    ///<param name="month">An integer from 1 to 12 that represents the month.</param>
    ///<param name="day">An integer from 1 to 31 that represents the day.</param>
    ///<param name="hour">An integer from 0 to 23 that represents the hour.</param>
    ///<param name="minute">An integer from 0 to 59 that represents the minute.</param>
    ///<param name="second">An integer from 0 to 59 that represents the second.</param>
    ///<param name="millisecond">An integer from 0 to 999 that represents the millisecond.</param>
    ///<param name="era">An integer that represents the era.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.     -or-     <paramref name="day" /> is outside the range supported by the calendar.     -or-     <paramref name="hour" /> is less than zero or greater than 23.     -or-     <paramref name="minute" /> is less than zero or greater than 59.     -or-     <paramref name="second" /> is less than zero or greater than 59.     -or-     <paramref name="millisecond" /> is less than zero or greater than 999.</exception>
    ///<returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.ToDateTime(int, int, int, int, int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Date GregorianCalendarToDateTime(System.Globalization.GregorianCalendar instance, Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number era);

    [WhiteList("override System.Globalization.GregorianCalendar.TwoDigitYearMax.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarGetTwoDigitYearMax(System.Globalization.GregorianCalendar instance);

    [WhiteList("override System.Globalization.GregorianCalendar.TwoDigitYearMax.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void GregorianCalendarSetTwoDigitYearMax(System.Globalization.GregorianCalendar instance, Number value);

    ///<summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.GregorianCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
    ///<param name="year">A two-digit or four-digit integer that represents the year to convert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is outside the range supported by the calendar.</exception>
    ///<returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>    [WhiteList("override System.Globalization.GregorianCalendar.ToFourDigitYear(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number GregorianCalendarToFourDigitYear(System.Globalization.GregorianCalendar instance, Number year);
}
