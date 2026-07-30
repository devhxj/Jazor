namespace Jazor.CLR;

/// <summary>
/// 实现 GregorianCalendar 的日期分解、加减和范围校验语义。
/// </summary>
/// <remarks>
/// Calendar API 使用 era、两位年份和特定文化规则，不能直接等同于 JavaScript Date 的本地
/// 方法。该模块与 RuntimeModule.JDateTime 协作，并通过显式校验维持 .NET 日期范围。
/// </remarks>
[ECMAScriptModule("System/Globalization/GregorianCalendarModule.js")]
[Jazor(Op.Alias, "System.Globalization.GregorianCalendar","Object")]
public static class GregorianCalendarModule
{
	private static Number CurrentEra => 0;
	private static Number DefaultTwoDigitYearMax => 2049;

	private static Number LocalizedCalendarType => 1;
	private static Number USEnglishCalendarType => 2;
	private static Number MiddleEastFrenchCalendarType => 9;
	private static Number ArabicCalendarType => 10;
	private static Number TransliteratedEnglishCalendarType => 11;
	private static Number TransliteratedFrenchCalendarType => 12;

	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
			throw new Error(message);
	}

	private static Number GetCalendarTypeValue(object type)
	{
		if (type is Number numberType)
			return numberType;
		if (type is System.Globalization.GregorianCalendarTypes enumType)
			return NumberFn((int)enumType);

		throw new Error("ArgumentException: Invalid GregorianCalendarTypes value.");
	}

	private static void ValidateCalendarType(object type)
	{
		var value = GetCalendarTypeValue(type);
		EnsureWholeNumber(value, "ArgumentOutOfRangeException: GregorianCalendarTypes value was not valid.");
		if (value != LocalizedCalendarType
			&& value != USEnglishCalendarType
			&& value != MiddleEastFrenchCalendarType
			&& value != ArabicCalendarType
			&& value != TransliteratedEnglishCalendarType
			&& value != TransliteratedFrenchCalendarType)
			throw new Error("ArgumentOutOfRangeException: GregorianCalendarTypes value was not valid.");
	}

	private static void ValidateEra(Number era)
	{
		EnsureWholeNumber(era, "ArgumentOutOfRangeException: Era must be a whole number.");
		if (era != CurrentEra && era != 1)
			throw new Error("ArgumentOutOfRangeException: Era value was not valid.");
	}

	private static void ValidateYear(Number year)
	{
		EnsureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number.");
		if (year < 1 || year > 9999)
			throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
	}

	private static void ValidateMonth(Number year, Number month)
	{
		ValidateYear(year);
		EnsureWholeNumber(month, "ArgumentOutOfRangeException: Month must be a whole number.");
		if (month < 1 || month > 12)
			throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
	}

	private static void ValidateDate(Number year, Number month, Number day)
	{
		ValidateMonth(year, month);
		EnsureWholeNumber(day, "ArgumentOutOfRangeException: Day must be a whole number.");
		if (day < 1 || day > RuntimeModule.GetDaysInMonth(year, month))
			throw new Error("ArgumentOutOfRangeException: Day is out of range for the specified month and year.");
	}

	/// <summary>
	/// C#: GregorianCalendar.ADEra
	/// JS: 1
	/// </summary>
	[Jazor(Op.Inline, "static readonly System.Globalization.GregorianCalendar.ADEra", "1")]
	public extern static Number _fa491b52106d378d();

	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.MinSupportedDateTime.get")]
	public static RuntimeModule.JDateTime _13ca7ecb3e3aade5(RuntimeModule.JGregorianCalendar instance)
		=> DateTimeModule._fad0c74e1c9df5bb();

	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.MaxSupportedDateTime.get")]
	public static RuntimeModule.JDateTime _7ba83b2ccdd567b5(RuntimeModule.JGregorianCalendar instance)
		=> DateTimeModule._eb38dc04224730ea();

	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.AlgorithmType.get")]
	public static System.Globalization.CalendarAlgorithmType _2c293866a460d9ea(RuntimeModule.JGregorianCalendar instance)
		=> System.Globalization.CalendarAlgorithmType.SolarCalendar;

	/// <summary>
	/// C#: new GregorianCalendar()
	/// JS: new JGregorianCalendar(Localized, 2029)
	/// </summary>
	[Jazor(Op.Import, "System.Globalization.GregorianCalendar.GregorianCalendar()")]
	public static RuntimeModule.JGregorianCalendar _23b9e8d671b5210e()
		=> new(LocalizedCalendarType, DefaultTwoDigitYearMax);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the specified <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
	[Jazor(Op.Import, "System.Globalization.GregorianCalendar.GregorianCalendar(System.Globalization.GregorianCalendarTypes)")]
	public static RuntimeModule.JGregorianCalendar _c043a86ee7a70c81(object type)
	{
		ValidateCalendarType(type);
		return new(GetCalendarTypeValue(type), DefaultTwoDigitYearMax);
	}

	[Jazor(Op.Import ,"virtual System.Globalization.GregorianCalendar.CalendarType.get")]
	public static System.Globalization.GregorianCalendarTypes _33a82cf70a73ecdd(RuntimeModule.JGregorianCalendar instance)
		=> (System.Globalization.GregorianCalendarTypes)(int)instance.CalendarType;

	[Jazor(Op.Import ,"virtual System.Globalization.GregorianCalendar.CalendarType.set")]
	public static void _ab29134350e86147(RuntimeModule.JGregorianCalendar instance, object value)
	{
		ValidateCalendarType(value);
		instance.CalendarType = GetCalendarTypeValue(value);
	}

	///<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.AddMonths(System.DateTime, int)")]
	public static RuntimeModule.JDateTime _1c4bd410ce12db05(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time, Number months)
		=> DateTimeModule._aae197b95f9024a4(time, months);

	///<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.AddYears(System.DateTime, int)")]
	public static RuntimeModule.JDateTime _705c207141cada42(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time, Number years)
		=> DateTimeModule._3353d31b02f2bed8(time, years);

	/// <summary>
	/// C#: calendar.GetDayOfMonth(time)
	/// JS: time.getDate()
	/// </summary>
	[Jazor(Op.Import, "override System.Globalization.GregorianCalendar.GetDayOfMonth(System.DateTime)")]
	public static Number _5f5d0a874674bdea(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> time.Date.GetDate();

	///<summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetDayOfWeek(System.DateTime)")]
	public static System.DayOfWeek _6cdddcc68587ea95(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> (System.DayOfWeek)(int)time.Date.GetDay();

	///<summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetDayOfYear(System.DateTime)")]
	public static Number _81e475ed63f62602(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
	{
		var year = time.Date.GetFullYear();
		var start = Date.UTC(year, 0, 0);
		var current = Date.UTC(year, time.Date.GetMonth(), time.Date.GetDate());
		return Math.FloorFn((current - start) / 86400000);
	}

	///<summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetDaysInMonth(int, int, int)")]
	public static Number _ce58c7d4d1c36fe3(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number era)
	{
		ValidateEra(era);
		return RuntimeModule.GetDaysInMonth(year, month);
	}

	///<summary>Returns the number of days in the specified year in the specified era.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetDaysInYear(int, int)")]
	public static Number _7545c4d66f0f3604(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
	{
		ValidateEra(era);
		ValidateYear(year);
		return _4c3723e9b82aa507(instance, year, era) ? 366 : 365;
	}

	///<summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetEra(System.DateTime)")]
	public static Number _21a6ebc60ed3b388(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> 1;

	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.Eras.get")]
	public static Number[] _c01c2927eaf2fefe(RuntimeModule.JGregorianCalendar instance)
		=> [1];

	/// <summary>
	/// C#: calendar.GetMonth(time)
	/// JS: (time.getMonth() + 1)
	/// </summary>
	[Jazor(Op.Import, "override System.Globalization.GregorianCalendar.GetMonth(System.DateTime)")]
	public static Number _ce76f400b1aa26d3(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> time.Date.GetMonth() + 1;

	///<summary>Returns the number of months in the specified year in the specified era.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetMonthsInYear(int, int)")]
	public static Number _5df8d3230f9681b9(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
	{
		ValidateEra(era);
		ValidateYear(year);
		return 12;
	}

	/// <summary>
	/// C#: calendar.GetYear(time)
	/// JS: time.getFullYear()
	/// </summary>
	[Jazor(Op.Import, "override System.Globalization.GregorianCalendar.GetYear(System.DateTime)")]
	public static Number _fd5a2cde6fb4d6f5(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> time.Date.GetFullYear();

	///<summary>Determines whether the specified date in the specified era is a leap day.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.IsLeapDay(int, int, int, int)")]
	public static bool _10c29328b0ef4014(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number day, Number era)
	{
		ValidateEra(era);
		ValidateDate(year, month, day);
		return month == 2 && day == 29 && _4c3723e9b82aa507(instance, year, era);
	}

	///<summary>Calculates the leap month for a specified year and era.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.GetLeapMonth(int, int)")]
	public static Number _91a08597c1c93445(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
	{
		ValidateEra(era);
		ValidateYear(year);
		return 0;
	}

	///<summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.IsLeapMonth(int, int, int)")]
	public static bool _9917941c9da950b5(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number era)
	{
		ValidateEra(era);
		ValidateMonth(year, month);
		return false;
	}

	/// <summary>
	/// C#: calendar.IsLeapYear(year, era)
	/// JS: year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0)
	/// </summary>
	[Jazor(Op.Import, "override System.Globalization.GregorianCalendar.IsLeapYear(int, int)")]
	public static bool _4c3723e9b82aa507(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
	{
		ValidateEra(era);
		ValidateYear(year);
		return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
	}

	///<summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.ToDateTime(int, int, int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _29ccd13d5e5508f8(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number era)
	{
		ValidateEra(era);
		return new(RuntimeModule.CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), 0);
	}

	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.TwoDigitYearMax.get")]
	public static Number _e32c11e11fbe2e3b(RuntimeModule.JGregorianCalendar instance)
		=> instance.TwoDigitYearMax;

	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.TwoDigitYearMax.set")]
	public static void _9537b0490ec80689(RuntimeModule.JGregorianCalendar instance, Number value)
	{
		EnsureWholeNumber(value, "ArgumentOutOfRangeException: TwoDigitYearMax must be a whole number.");
		if (value < 99 || value > 9999)
			throw new Error("ArgumentOutOfRangeException: TwoDigitYearMax must be between 99 and 9999.");

		instance.TwoDigitYearMax = value;
	}

	///<summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.GregorianCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
	[Jazor(Op.Import ,"override System.Globalization.GregorianCalendar.ToFourDigitYear(int)")]
	public static Number _cca1b99b56b6a322(RuntimeModule.JGregorianCalendar instance, Number year)
	{
		EnsureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number.");
		if (year < 0 || year > 9999)
			throw new Error("ArgumentOutOfRangeException: Year must be between 0 and 9999.");
		if (year >= 100)
			return year;

		var century = Math.FloorFn(instance.TwoDigitYearMax / 100) * 100;
		var pivot = instance.TwoDigitYearMax % 100;
		return year <= pivot ? century + year : century - 100 + year;
	}
}
