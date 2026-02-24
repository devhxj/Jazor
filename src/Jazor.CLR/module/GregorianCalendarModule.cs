namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Globalization.GregorianCalendar","System/Globalization/GregorianCalendarModule.js")]
public static class GregorianCalendarModule
{
	//System.Globalization.GregorianCalendar.ADEra = 1;

	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.MinSupportedDateTime.get")]
	public extern static Date _13ca7ecb3e3aade5(System.Globalization.GregorianCalendar instance);

	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.MaxSupportedDateTime.get")]
	public extern static Date _7ba83b2ccdd567b5(System.Globalization.GregorianCalendar instance);

	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.AlgorithmType.get")]
	public extern static System.Globalization.CalendarAlgorithmType _2c293866a460d9ea(System.Globalization.GregorianCalendar instance);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the default <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
	[Jazor(Op.Discard ,"System.Globalization.GregorianCalendar.GregorianCalendar()")]
	public extern static System.Globalization.GregorianCalendar _23b9e8d671b5210e();

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the specified <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
	[Jazor(Op.Discard ,"System.Globalization.GregorianCalendar.GregorianCalendar(System.Globalization.GregorianCalendarTypes)")]
	public extern static System.Globalization.GregorianCalendar _c043a86ee7a70c81(object type);

	[Jazor(Op.Discard ,"virtual System.Globalization.GregorianCalendar.CalendarType.get")]
	public extern static System.Globalization.GregorianCalendarTypes _33a82cf70a73ecdd(System.Globalization.GregorianCalendar instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.GregorianCalendar.CalendarType.set")]
	public extern static void _ab29134350e86147(System.Globalization.GregorianCalendar instance, object value);

	///<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.AddMonths(System.DateTime, int)")]
	public extern static Date _1c4bd410ce12db05(System.Globalization.GregorianCalendar instance, Date time, Number months);

	///<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.AddYears(System.DateTime, int)")]
	public extern static Date _705c207141cada42(System.Globalization.GregorianCalendar instance, Date time, Number years);

	///<summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetDayOfMonth(System.DateTime)")]
	public extern static Number _5f5d0a874674bdea(System.Globalization.GregorianCalendar instance, Date time);

	///<summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetDayOfWeek(System.DateTime)")]
	public extern static System.DayOfWeek _6cdddcc68587ea95(System.Globalization.GregorianCalendar instance, Date time);

	///<summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetDayOfYear(System.DateTime)")]
	public extern static Number _81e475ed63f62602(System.Globalization.GregorianCalendar instance, Date time);

	///<summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetDaysInMonth(int, int, int)")]
	public extern static Number _ce58c7d4d1c36fe3(System.Globalization.GregorianCalendar instance, Number year, Number month, Number era);

	///<summary>Returns the number of days in the specified year in the specified era.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetDaysInYear(int, int)")]
	public extern static Number _7545c4d66f0f3604(System.Globalization.GregorianCalendar instance, Number year, Number era);

	///<summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetEra(System.DateTime)")]
	public extern static Number _21a6ebc60ed3b388(System.Globalization.GregorianCalendar instance, Date time);

	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.Eras.get")]
	public extern static int[] _c01c2927eaf2fefe(System.Globalization.GregorianCalendar instance);

	///<summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetMonth(System.DateTime)")]
	public extern static Number _ce76f400b1aa26d3(System.Globalization.GregorianCalendar instance, Date time);

	///<summary>Returns the number of months in the specified year in the specified era.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetMonthsInYear(int, int)")]
	public extern static Number _5df8d3230f9681b9(System.Globalization.GregorianCalendar instance, Number year, Number era);

	///<summary>Returns the year in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetYear(System.DateTime)")]
	public extern static Number _fd5a2cde6fb4d6f5(System.Globalization.GregorianCalendar instance, Date time);

	///<summary>Determines whether the specified date in the specified era is a leap day.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.IsLeapDay(int, int, int, int)")]
	public extern static bool _10c29328b0ef4014(System.Globalization.GregorianCalendar instance, Number year, Number month, Number day, Number era);

	///<summary>Calculates the leap month for a specified year and era.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.GetLeapMonth(int, int)")]
	public extern static Number _91a08597c1c93445(System.Globalization.GregorianCalendar instance, Number year, Number era);

	///<summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.IsLeapMonth(int, int, int)")]
	public extern static bool _9917941c9da950b5(System.Globalization.GregorianCalendar instance, Number year, Number month, Number era);

	///<summary>Determines whether the specified year in the specified era is a leap year.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.IsLeapYear(int, int)")]
	public extern static bool _4c3723e9b82aa507(System.Globalization.GregorianCalendar instance, Number year, Number era);

	///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.ToDateTime(int, int, int, int, int, int, int, int)")]
	public extern static Date _29ccd13d5e5508f8(System.Globalization.GregorianCalendar instance, Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number era);

	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.TwoDigitYearMax.get")]
	public extern static Number _e32c11e11fbe2e3b(System.Globalization.GregorianCalendar instance);

	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.TwoDigitYearMax.set")]
	public extern static void _9537b0490ec80689(System.Globalization.GregorianCalendar instance, Number value);

	///<summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.GregorianCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.GregorianCalendar.ToFourDigitYear(int)")]
	public extern static Number _cca1b99b56b6a322(System.Globalization.GregorianCalendar instance, Number year);
}
