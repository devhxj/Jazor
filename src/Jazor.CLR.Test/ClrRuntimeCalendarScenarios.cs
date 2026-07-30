using System.Globalization;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeCalendarScenarios
{
    private const string GregorianModulePath = "System/Globalization/GregorianCalendarModule.js";
    private const string CalendarModulePath = "System/Globalization/CalendarModule.js";
    private static readonly DateTime SampleDate = new(2024, 2, 29, 3, 4, 5, 6, DateTimeKind.Unspecified);

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        GregorianDate("calendar.gregorian.minimum", "override System.Globalization.GregorianCalendar.MinSupportedDateTime.get", [Gregorian()], DateTime.MinValue),
        GregorianDate("calendar.gregorian.maximum", "override System.Globalization.GregorianCalendar.MaxSupportedDateTime.get", [Gregorian()], DateTime.MaxValue),
        Gregorian("calendar.gregorian.algorithm", "override System.Globalization.GregorianCalendar.AlgorithmType.get", [Gregorian()], Number((int)CalendarAlgorithmType.SolarCalendar)),
        Gregorian("calendar.gregorian.ctor.default-type", "virtual System.Globalization.GregorianCalendar.CalendarType.get", [Invoke("System.Globalization.GregorianCalendar.GregorianCalendar()")], Number((int)GregorianCalendarTypes.Localized)),
        Gregorian("calendar.gregorian.ctor.english-type", "virtual System.Globalization.GregorianCalendar.CalendarType.get", [Invoke("System.Globalization.GregorianCalendar.GregorianCalendar(System.Globalization.GregorianCalendarTypes)", Number((int)GregorianCalendarTypes.USEnglish))], Number((int)GregorianCalendarTypes.USEnglish)),
        Gregorian("calendar.gregorian.type-get", "virtual System.Globalization.GregorianCalendar.CalendarType.get", [Gregorian()], Number((int)GregorianCalendarTypes.Localized)),
        Gregorian("calendar.gregorian.type-set", "virtual System.Globalization.GregorianCalendar.CalendarType.set", [Gregorian(), Number((int)GregorianCalendarTypes.USEnglish)], Undefined()),
        GregorianFailure("calendar.gregorian.type-invalid", "System.Globalization.GregorianCalendar.GregorianCalendar(System.Globalization.GregorianCalendarTypes)", [Number(3)], "ArgumentOutOfRangeException"),
        GregorianDate("calendar.gregorian.add-months-clamps", "override System.Globalization.GregorianCalendar.AddMonths(System.DateTime, int)", [Gregorian(), DateTimeValue(new DateTime(2024, 1, 31, 3, 4, 5, 6)), Number(1)], new DateTime(2024, 2, 29, 3, 4, 5, 6)),
        GregorianDate("calendar.gregorian.add-years-clamps", "override System.Globalization.GregorianCalendar.AddYears(System.DateTime, int)", [Gregorian(), DateTimeValue(SampleDate), Number(1)], new DateTime(2025, 2, 28, 3, 4, 5, 6)),
        Gregorian("calendar.gregorian.day-of-month", "override System.Globalization.GregorianCalendar.GetDayOfMonth(System.DateTime)", [Gregorian(), SampleDateValue()], Number(29)),
        Gregorian("calendar.gregorian.day-of-week", "override System.Globalization.GregorianCalendar.GetDayOfWeek(System.DateTime)", [Gregorian(), SampleDateValue()], Number((int)DayOfWeek.Thursday)),
        Gregorian("calendar.gregorian.day-of-year", "override System.Globalization.GregorianCalendar.GetDayOfYear(System.DateTime)", [Gregorian(), SampleDateValue()], Number(60)),
        Gregorian("calendar.gregorian.days-in-month", "override System.Globalization.GregorianCalendar.GetDaysInMonth(int, int, int)", [Gregorian(), Number(2024), Number(2), Era()], Number(29)),
        Gregorian("calendar.gregorian.days-in-year", "override System.Globalization.GregorianCalendar.GetDaysInYear(int, int)", [Gregorian(), Number(2024), Era()], Number(366)),
        Gregorian("calendar.gregorian.era", "override System.Globalization.GregorianCalendar.GetEra(System.DateTime)", [Gregorian(), SampleDateValue()], Number(1)),
        Gregorian("calendar.gregorian.eras", "override System.Globalization.GregorianCalendar.Eras.get", [Gregorian()], Array(Number(1))),
        Gregorian("calendar.gregorian.month", "override System.Globalization.GregorianCalendar.GetMonth(System.DateTime)", [Gregorian(), SampleDateValue()], Number(2)),
        Gregorian("calendar.gregorian.months-in-year", "override System.Globalization.GregorianCalendar.GetMonthsInYear(int, int)", [Gregorian(), Number(2024), Era()], Number(12)),
        Gregorian("calendar.gregorian.year", "override System.Globalization.GregorianCalendar.GetYear(System.DateTime)", [Gregorian(), SampleDateValue()], Number(2024)),
        Gregorian("calendar.gregorian.is-leap-day", "override System.Globalization.GregorianCalendar.IsLeapDay(int, int, int, int)", [Gregorian(), Number(2024), Number(2), Number(29), Era()], Bool(true)),
        Gregorian("calendar.gregorian.leap-month", "override System.Globalization.GregorianCalendar.GetLeapMonth(int, int)", [Gregorian(), Number(2024), Era()], Number(0)),
        Gregorian("calendar.gregorian.is-leap-month", "override System.Globalization.GregorianCalendar.IsLeapMonth(int, int, int)", [Gregorian(), Number(2024), Number(2), Era()], Bool(false)),
        Gregorian("calendar.gregorian.is-leap-year", "override System.Globalization.GregorianCalendar.IsLeapYear(int, int)", [Gregorian(), Number(2000), Era()], Bool(true)),
        Gregorian("calendar.gregorian.is-not-leap-century", "override System.Globalization.GregorianCalendar.IsLeapYear(int, int)", [Gregorian(), Number(2100), Era()], Bool(false)),
        GregorianDate("calendar.gregorian.to-datetime", "override System.Globalization.GregorianCalendar.ToDateTime(int, int, int, int, int, int, int, int)", [Gregorian(), Number(2024), Number(2), Number(29), Number(3), Number(4), Number(5), Number(6), Era()], SampleDate),
        Gregorian("calendar.gregorian.two-digit-max-get", "override System.Globalization.GregorianCalendar.TwoDigitYearMax.get", [Gregorian()], Number(2049)),
        Gregorian("calendar.gregorian.two-digit-max-set", "override System.Globalization.GregorianCalendar.TwoDigitYearMax.set", [Gregorian(), Number(2099)], Undefined()),
        Gregorian("calendar.gregorian.four-digit-before-pivot", "override System.Globalization.GregorianCalendar.ToFourDigitYear(int)", [Gregorian(), Number(49)], Number(2049)),
        Gregorian("calendar.gregorian.four-digit-after-pivot", "override System.Globalization.GregorianCalendar.ToFourDigitYear(int)", [Gregorian(), Number(50)], Number(1950)),
        GregorianFailure("calendar.gregorian.invalid-era", "override System.Globalization.GregorianCalendar.GetDaysInYear(int, int)", [Gregorian(), Number(2024), Number(2)], "ArgumentOutOfRangeException"),
        GregorianFailure("calendar.gregorian.two-digit-max-invalid", "override System.Globalization.GregorianCalendar.TwoDigitYearMax.set", [Gregorian(), Number(98)], "ArgumentOutOfRangeException"),

        CalendarDate("calendar.base.minimum", "virtual System.Globalization.Calendar.MinSupportedDateTime.get", [Gregorian()], DateTime.MinValue),
        CalendarDate("calendar.base.maximum", "virtual System.Globalization.Calendar.MaxSupportedDateTime.get", [Gregorian()], DateTime.MaxValue),
        Calendar("calendar.base.algorithm", "virtual System.Globalization.Calendar.AlgorithmType.get", [Gregorian()], Number((int)CalendarAlgorithmType.SolarCalendar)),
        CalendarDate("calendar.base.add-months", "virtual System.Globalization.Calendar.AddMonths(System.DateTime, int)", [Gregorian(), DateTimeValue(new DateTime(2024, 1, 31)), Number(1)], new DateTime(2024, 2, 29)),
        CalendarDate("calendar.base.add-years", "virtual System.Globalization.Calendar.AddYears(System.DateTime, int)", [Gregorian(), SampleDateValue(), Number(1)], new DateTime(2025, 2, 28, 3, 4, 5, 6)),
        Calendar("calendar.base.day-of-month", "virtual System.Globalization.Calendar.GetDayOfMonth(System.DateTime)", [Gregorian(), SampleDateValue()], Number(29)),
        Calendar("calendar.base.day-of-week", "virtual System.Globalization.Calendar.GetDayOfWeek(System.DateTime)", [Gregorian(), SampleDateValue()], Number((int)DayOfWeek.Thursday)),
        Calendar("calendar.base.day-of-year", "virtual System.Globalization.Calendar.GetDayOfYear(System.DateTime)", [Gregorian(), SampleDateValue()], Number(60)),
        Calendar("calendar.base.days-in-month", "virtual System.Globalization.Calendar.GetDaysInMonth(int, int, int)", [Gregorian(), Number(2024), Number(2), Era()], Number(29)),
        Calendar("calendar.base.days-in-year", "virtual System.Globalization.Calendar.GetDaysInYear(int, int)", [Gregorian(), Number(2024), Era()], Number(366)),
        Calendar("calendar.base.era", "virtual System.Globalization.Calendar.GetEra(System.DateTime)", [Gregorian(), SampleDateValue()], Number(1)),
        Calendar("calendar.base.eras", "virtual System.Globalization.Calendar.Eras.get", [Gregorian()], Array(Number(1))),
        Calendar("calendar.base.month", "virtual System.Globalization.Calendar.GetMonth(System.DateTime)", [Gregorian(), SampleDateValue()], Number(2)),
        Calendar("calendar.base.months-in-year", "virtual System.Globalization.Calendar.GetMonthsInYear(int, int)", [Gregorian(), Number(2024), Era()], Number(12)),
        Calendar("calendar.base.year", "virtual System.Globalization.Calendar.GetYear(System.DateTime)", [Gregorian(), SampleDateValue()], Number(2024)),
        Calendar("calendar.base.is-leap-day", "virtual System.Globalization.Calendar.IsLeapDay(int, int, int, int)", [Gregorian(), Number(2024), Number(2), Number(29), Era()], Bool(true)),
        Calendar("calendar.base.leap-month", "virtual System.Globalization.Calendar.GetLeapMonth(int, int)", [Gregorian(), Number(2024), Era()], Number(0)),
        Calendar("calendar.base.is-leap-month", "virtual System.Globalization.Calendar.IsLeapMonth(int, int, int)", [Gregorian(), Number(2024), Number(2), Era()], Bool(false)),
        Calendar("calendar.base.is-leap-year", "virtual System.Globalization.Calendar.IsLeapYear(int, int)", [Gregorian(), Number(2024), Era()], Bool(true)),
        CalendarDate("calendar.base.to-datetime", "virtual System.Globalization.Calendar.ToDateTime(int, int, int, int, int, int, int, int)", [Gregorian(), Number(2024), Number(2), Number(29), Number(3), Number(4), Number(5), Number(6), Era()], SampleDate),
        Calendar("calendar.base.two-digit-max-get", "virtual System.Globalization.Calendar.TwoDigitYearMax.get", [Gregorian()], Number(2049)),
        Calendar("calendar.base.two-digit-max-set", "virtual System.Globalization.Calendar.TwoDigitYearMax.set", [Gregorian(), Number(2099)], Undefined()),
        Calendar("calendar.base.four-digit-year", "virtual System.Globalization.Calendar.ToFourDigitYear(int)", [Gregorian(), Number(50)], Number(1950))
    ];

    private static ClrRuntimeValue Gregorian()
        => Invoke("System.Globalization.GregorianCalendar.GregorianCalendar()");

    private static ClrRuntimeValue Era() => Number(1);
    private static ClrRuntimeValue SampleDateValue() => DateTimeValue(SampleDate);

    private static ClrRuntimeValue DateTimeValue(DateTime value)
        => Invoke("System.DateTime.DateTime(long, System.DateTimeKind)", Big(value.Ticks), Number((int)value.Kind));

    private static ClrRuntimeValue DateTimeText(DateTime value)
        => Text(value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture));

    private static ClrRuntimeScenario Gregorian(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, GregorianModulePath, arguments, expected);

    private static ClrRuntimeScenario GregorianDate(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, DateTime expected)
        => Gregorian(id, member, arguments, DateTimeText(expected));

    private static ClrRuntimeScenario GregorianFailure(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, GregorianModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeScenario Calendar(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, CalendarModulePath, arguments, expected);

    private static ClrRuntimeScenario CalendarDate(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, DateTime expected)
        => Calendar(id, member, arguments, DateTimeText(expected));

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Undefined() => ClrRuntimeValue.Undefined();
}
