using System.Globalization;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeDateOnlyScenarios
{
    private const string ModulePath = "System/DateOnlyModule.js";
    private static readonly DateOnly SampleValue = new(2024, 2, 29);

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        SuccessDate("date-only.constant.minimum", "static System.DateOnly.MinValue.get", [], DateOnly.MinValue),
        SuccessDate("date-only.constant.maximum", "static System.DateOnly.MaxValue.get", [], DateOnly.MaxValue),
        SuccessDate("date-only.ctor.default", "System.DateOnly.DateOnly()", [], DateOnly.MinValue),
        SuccessDate("date-only.ctor.ymd", "System.DateOnly.DateOnly(int, int, int)", [Number(2024), Number(2), Number(29)], SampleValue),
        SuccessDate("date-only.ctor.ymd-calendar", "System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)", [Number(2024), Number(2), Number(29), Null()], SampleValue),
        Failure("date-only.ctor.invalid-day", "System.DateOnly.DateOnly(int, int, int)", [Number(2023), Number(2), Number(29)], "ArgumentOutOfRangeException"),
        SuccessDate("date-only.from-day-number.zero", "static System.DateOnly.FromDayNumber(int)", [Number(0)], DateOnly.MinValue),
        SuccessDate("date-only.from-day-number.leap-day", "static System.DateOnly.FromDayNumber(int)", [Number(SampleValue.DayNumber)], SampleValue),
        SuccessDate("date-only.from-day-number.maximum", "static System.DateOnly.FromDayNumber(int)", [Number(DateOnly.MaxValue.DayNumber)], DateOnly.MaxValue),
        Failure("date-only.from-day-number.negative", "static System.DateOnly.FromDayNumber(int)", [Number(-1)], "ArgumentOutOfRangeException"),
        Failure("date-only.from-day-number.out-of-range", "static System.DateOnly.FromDayNumber(int)", [Number(DateOnly.MaxValue.DayNumber + 1)], "ArgumentOutOfRangeException"),

        Success("date-only.property.year", "System.DateOnly.Year.get", [Sample()], Number(SampleValue.Year)),
        Success("date-only.property.month", "System.DateOnly.Month.get", [Sample()], Number(SampleValue.Month)),
        Success("date-only.property.day", "System.DateOnly.Day.get", [Sample()], Number(SampleValue.Day)),
        Success("date-only.property.day-of-week", "System.DateOnly.DayOfWeek.get", [Sample()], Number((int)SampleValue.DayOfWeek)),
        Success("date-only.property.day-of-year", "System.DateOnly.DayOfYear.get", [Sample()], Number(SampleValue.DayOfYear)),
        Success("date-only.property.day-number", "System.DateOnly.DayNumber.get", [Sample()], Number(SampleValue.DayNumber)),

        SuccessDate("date-only.add-days.crosses-month", "System.DateOnly.AddDays(int)", [Date(new DateOnly(2024, 1, 31)), Number(1)], new DateOnly(2024, 2, 1)),
        SuccessDate("date-only.add-days.leap-day", "System.DateOnly.AddDays(int)", [Date(new DateOnly(2024, 2, 28)), Number(1)], SampleValue),
        SuccessDate("date-only.add-days.negative", "System.DateOnly.AddDays(int)", [Date(new DateOnly(2024, 3, 1)), Number(-1)], SampleValue),
        SuccessDate("date-only.add-months.clamps", "System.DateOnly.AddMonths(int)", [Date(new DateOnly(2024, 1, 31)), Number(1)], SampleValue),
        SuccessDate("date-only.add-months.negative", "System.DateOnly.AddMonths(int)", [Date(new DateOnly(2024, 3, 31)), Number(-1)], SampleValue),
        SuccessDate("date-only.add-years.clamps-leap-day", "System.DateOnly.AddYears(int)", [Sample(), Number(1)], new DateOnly(2025, 2, 28)),
        Failure("date-only.add-days.fractional", "System.DateOnly.AddDays(int)", [Sample(), Number(1.5)], "ArgumentOutOfRangeException"),
        Failure("date-only.add-years.out-of-range", "System.DateOnly.AddYears(int)", [DateOnlyValue(DateOnly.MaxValue), Number(1)], "ArgumentOutOfRangeException"),

        Success("date-only.operator.equal", "static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)", [Sample(), Sample()], Bool(true)),
        Success("date-only.operator.not-equal", "static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)", [Sample(), Date(new DateOnly(2024, 3, 1))], Bool(true)),
        Success("date-only.operator.greater", "static System.DateOnly.operator >(System.DateOnly, System.DateOnly)", [Date(new DateOnly(2024, 3, 1)), Sample()], Bool(true)),
        Success("date-only.operator.greater-or-equal", "static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)", [Sample(), Sample()], Bool(true)),
        Success("date-only.operator.less", "static System.DateOnly.operator <(System.DateOnly, System.DateOnly)", [Sample(), Date(new DateOnly(2024, 3, 1))], Bool(true)),
        Success("date-only.operator.less-or-equal", "static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)", [Sample(), Sample()], Bool(true)),
        Success("date-only.deconstruct", "System.DateOnly.Deconstruct(out int, out int, out int)", [Sample(), Number(0), Number(0), Number(0)], Array(Number(2024), Number(2), Number(29))),
        Success("date-only.to-datetime.unspecified", "System.DateOnly.ToDateTime(System.TimeOnly)", [Sample(), TimeOnlyValue(new TimeOnly(3, 4, 5, 6, 7).Add(TimeSpan.FromTicks(9)))], DateTimeText(new DateTime(2024, 2, 29, 3, 4, 5, 6).AddTicks(79))),
        Success("date-only.to-datetime.utc", "System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)", [Sample(), TimeOnlyValue(new TimeOnly(3, 4, 5)), Number((int)DateTimeKind.Utc)], DateTimeText(new DateTime(2024, 2, 29, 3, 4, 5, DateTimeKind.Utc))),
        Failure("date-only.to-datetime.invalid-kind", "System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)", [Sample(), TimeOnlyValue(TimeOnly.MinValue), Number(3)], "ArgumentException"),
        SuccessDate("date-only.from-datetime", "static System.DateOnly.FromDateTime(System.DateTime)", [DateTimeValue(new DateTime(2024, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc))], SampleValue),
        Success("date-only.compare-to-typed.less", "System.DateOnly.CompareTo(System.DateOnly)", [Sample(), Date(new DateOnly(2024, 3, 1))], Number(-1)),
        Success("date-only.compare-to-object.null", "System.DateOnly.CompareTo(object)", [Sample(), Null()], Number(1)),
        Success("date-only.compare-to-object.equal", "System.DateOnly.CompareTo(object)", [Sample(), Sample()], Number(0)),
        Failure("date-only.compare-to-object-wrong-type", "System.DateOnly.CompareTo(object)", [Sample(), Text("2024-02-29")], "ArgumentException"),
        Success("date-only.equals-typed", "System.DateOnly.Equals(System.DateOnly)", [Sample(), Sample()], Bool(true)),
        Success("date-only.equals-object-wrong-type", "override System.DateOnly.Equals(object)", [Sample(), Number(1)], Bool(false)),
        Success("date-only.hash-code", "override System.DateOnly.GetHashCode()", [Sample()], Number(SampleValue.GetHashCode())),

        SuccessDate("date-only.parse.iso", "static System.DateOnly.Parse(string)", [Text("2024-02-29")], SampleValue),
        SuccessDate("date-only.parse.whitespace", "static System.DateOnly.Parse(string)", [Text("  2024-02-29  ")], SampleValue),
        SuccessDate("date-only.parse.provider", "static System.DateOnly.Parse(string, System.IFormatProvider)", [Text("2024-02-29"), Text("en-US")], SampleValue),
        SuccessDate("date-only.parse.span-provider", "static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", [Text("2024-02-29"), Text("en-US")], SampleValue),
        SuccessDate("date-only.parse.style", "static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text(" 2024-02-29 "), Text("en-US"), Number((int)DateTimeStyles.AllowWhiteSpaces)], SampleValue),
        SuccessDate("date-only.parse.span-style", "static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-02-29"), Text("en-US"), Number((int)DateTimeStyles.None)], SampleValue),
        Failure("date-only.parse.invalid-leap-day", "static System.DateOnly.Parse(string)", [Text("2023-02-29")], "FormatException"),
        Failure("date-only.parse.invalid-style", "static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-02-29"), Text("en-US"), Number((int)DateTimeStyles.AssumeUniversal)], "ArgumentException"),
        Success("date-only.try-parse.valid", "static System.DateOnly.TryParse(string, out System.DateOnly)", [Text("2024-02-29"), DateOnlyValue(DateOnly.MinValue)], Array(Bool(true), DateText(SampleValue))),
        Success("date-only.try-parse.invalid", "static System.DateOnly.TryParse(string, out System.DateOnly)", [Text("not-a-date"), Sample()], Array(Bool(false), DateText(DateOnly.MinValue))),
        Success("date-only.try-parse.null", "static System.DateOnly.TryParse(string, out System.DateOnly)", [Null(), Sample()], Array(Bool(false), DateText(DateOnly.MinValue))),
        Success("date-only.try-parse.span", "static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)", [Text("2024-02-29"), DateOnlyValue(DateOnly.MinValue)], Array(Bool(true), DateText(SampleValue))),
        Success("date-only.try-parse.style", "static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", [Text("2024-02-29"), Text("en-US"), Number((int)DateTimeStyles.AllowWhiteSpaces), DateOnlyValue(DateOnly.MinValue)], Array(Bool(true), DateText(SampleValue))),
        Success("date-only.try-parse.span-style-invalid", "static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)", [Text("2024-02-29"), Text("en-US"), Number((int)DateTimeStyles.AssumeUniversal), DateOnlyValue(DateOnly.MinValue)], Array(Bool(false), DateText(DateOnly.MinValue))),
        Success("date-only.try-parse.provider", "static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)", [Text("2024-02-29"), Text("en-US"), DateOnlyValue(DateOnly.MinValue)], Array(Bool(true), DateText(SampleValue))),
        Success("date-only.try-parse.span-provider", "static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)", [Text("2024-02-29"), Text("en-US"), DateOnlyValue(DateOnly.MinValue)], Array(Bool(true), DateText(SampleValue))),

        SuccessDate("date-only.format.long-roundtrip", "static System.DateOnly.Parse(string)", [Invoke("System.DateOnly.ToLongDateString()", Sample())], SampleValue),
        SuccessDate("date-only.format.short-roundtrip", "static System.DateOnly.Parse(string)", [Invoke("System.DateOnly.ToShortDateString()", Sample())], SampleValue),
        Success("date-only.format.roundtrip", "System.DateOnly.ToString(string)", [Sample(), Text("O")], DateText(SampleValue)),
        SuccessDate("date-only.format.provider-roundtrip", "static System.DateOnly.Parse(string, System.IFormatProvider)", [Invoke("System.DateOnly.ToString(System.IFormatProvider)", Sample(), Text("en-US")), Text("en-US")], SampleValue),
        SuccessDate("date-only.format.custom-provider-roundtrip", "static System.DateOnly.Parse(string, System.IFormatProvider)", [Invoke("System.DateOnly.ToString(string, System.IFormatProvider)", Sample(), Text("O"), Text("en-US")), Text("en-US")], SampleValue)
    ];

    private static ClrRuntimeValue Sample() => Date(SampleValue);
    private static ClrRuntimeValue Date(DateOnly value) => Invoke("System.DateOnly.DateOnly(int, int, int)", Number(value.Year), Number(value.Month), Number(value.Day));
    private static ClrRuntimeValue DateOnlyValue(DateOnly value) => Date(value);
    private static ClrRuntimeValue TimeOnlyValue(TimeOnly value) => Invoke("System.TimeOnly.TimeOnly(long)", Big(value.Ticks));

    private static ClrRuntimeValue DateTimeValue(DateTime value)
        => Invoke("System.DateTime.DateTime(long, System.DateTimeKind)", Big(value.Ticks), Number((int)value.Kind));

    private static ClrRuntimeValue DateText(DateOnly value)
        => Text(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

    private static ClrRuntimeValue DateTimeText(DateTime value)
        => Text(value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture));

    private static ClrRuntimeScenario SuccessDate(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        DateOnly expected)
        => Success(id, member, arguments, DateText(expected));

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
