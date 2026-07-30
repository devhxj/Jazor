namespace Jazor.CLR;

/// <summary>
/// 为 Calendar 抽象契约提供 GregorianCalendar 的共享成员转发映射。
/// </summary>
/// <remarks>
/// JavaScript 没有 CLR Calendar 类型层次；接口/抽象类路径必须与具体 GregorianCalendar
/// 使用同一套 helper，避免通过接口调用时日期范围和 era 语义发生漂移。
/// </remarks>
[ECMAScriptModule("System/Globalization/CalendarModule.js")]
[Jazor(Op.Alias, "System.Globalization.Calendar","Object")]
public static class CalendarModule
{
	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.MinSupportedDateTime.get")]
	public static RuntimeModule.JDateTime _a347363369470161(RuntimeModule.JGregorianCalendar instance)
		=> GregorianCalendarModule._13ca7ecb3e3aade5(instance);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.MaxSupportedDateTime.get")]
	public static RuntimeModule.JDateTime _b980a33ad0b9f3af(RuntimeModule.JGregorianCalendar instance)
		=> GregorianCalendarModule._7ba83b2ccdd567b5(instance);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.AlgorithmType.get")]
	public static System.Globalization.CalendarAlgorithmType _d1844de95d117ad7(RuntimeModule.JGregorianCalendar instance)
		=> GregorianCalendarModule._2c293866a460d9ea(instance);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.AddMonths(System.DateTime, int)")]
	public static RuntimeModule.JDateTime _38064c53fd00bf67(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time, Number months)
		=> GregorianCalendarModule._1c4bd410ce12db05(instance, time, months);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.AddYears(System.DateTime, int)")]
	public static RuntimeModule.JDateTime _f44bb645160b213a(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time, Number years)
		=> GregorianCalendarModule._705c207141cada42(instance, time, years);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetDayOfMonth(System.DateTime)")]
	public static Number _e27ad3e4edf84d31(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> GregorianCalendarModule._5f5d0a874674bdea(instance, time);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetDayOfWeek(System.DateTime)")]
	public static System.DayOfWeek _c2e49bc11fd66c3b(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> GregorianCalendarModule._6cdddcc68587ea95(instance, time);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetDayOfYear(System.DateTime)")]
	public static Number _a3eb905ca5ad54f0(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> GregorianCalendarModule._81e475ed63f62602(instance, time);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetDaysInMonth(int, int, int)")]
	public static Number _40ebcfb884fa4a22(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number era)
		=> GregorianCalendarModule._ce58c7d4d1c36fe3(instance, year, month, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetDaysInYear(int, int)")]
	public static Number _29387c54e19bb53d(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
		=> GregorianCalendarModule._7545c4d66f0f3604(instance, year, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetEra(System.DateTime)")]
	public static Number _a2c4355d29a5f6c6(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> GregorianCalendarModule._21a6ebc60ed3b388(instance, time);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.Eras.get")]
	public static Number[] _d0f2af50bb087f93(RuntimeModule.JGregorianCalendar instance)
		=> GregorianCalendarModule._c01c2927eaf2fefe(instance);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetMonth(System.DateTime)")]
	public static Number _065ff2e2ab2f6ac7(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> GregorianCalendarModule._ce76f400b1aa26d3(instance, time);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetMonthsInYear(int, int)")]
	public static Number _6f44c2660ec3b81a(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
		=> GregorianCalendarModule._5df8d3230f9681b9(instance, year, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetYear(System.DateTime)")]
	public static Number _ee1ec411f7f5548f(RuntimeModule.JGregorianCalendar instance, RuntimeModule.JDateTime time)
		=> GregorianCalendarModule._fd5a2cde6fb4d6f5(instance, time);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.IsLeapDay(int, int, int, int)")]
	public static bool _8b4d0a4f09d12c0d(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number day, Number era)
		=> GregorianCalendarModule._10c29328b0ef4014(instance, year, month, day, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.GetLeapMonth(int, int)")]
	public static Number _c7dad9ef764d87fb(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
		=> GregorianCalendarModule._91a08597c1c93445(instance, year, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.IsLeapMonth(int, int, int)")]
	public static bool _afae0c0c0a5ef049(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number era)
		=> GregorianCalendarModule._9917941c9da950b5(instance, year, month, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.IsLeapYear(int, int)")]
	public static bool _f172f5b74c83a0ae(RuntimeModule.JGregorianCalendar instance, Number year, Number era)
		=> GregorianCalendarModule._4c3723e9b82aa507(instance, year, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.ToDateTime(int, int, int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _9588793f1a4fd85a(RuntimeModule.JGregorianCalendar instance, Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number era)
		=> GregorianCalendarModule._29ccd13d5e5508f8(instance, year, month, day, hour, minute, second, millisecond, era);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.TwoDigitYearMax.get")]
	public static Number _be5d8db1be0d56f4(RuntimeModule.JGregorianCalendar instance)
		=> GregorianCalendarModule._e32c11e11fbe2e3b(instance);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.TwoDigitYearMax.set")]
	public static void _67e5ba9715f0d7bf(RuntimeModule.JGregorianCalendar instance, Number value)
		=> GregorianCalendarModule._9537b0490ec80689(instance, value);

	[Jazor(Op.Import ,"virtual System.Globalization.Calendar.ToFourDigitYear(int)")]
	public static Number _8e7d51754b95ea42(RuntimeModule.JGregorianCalendar instance, Number year)
		=> GregorianCalendarModule._cca1b99b56b6a322(instance, year);
}
