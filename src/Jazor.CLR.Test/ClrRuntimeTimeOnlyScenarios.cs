using System.Globalization;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeTimeOnlyScenarios
{
    private const string ModulePath = "System/TimeOnlyModule.js";
    private static readonly TimeOnly SampleValue = new TimeOnly(3, 4, 5, 6, 7).Add(TimeSpan.FromTicks(9));

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        SuccessTime("time-only.constant.minimum", "static System.TimeOnly.MinValue.get", [], TimeOnly.MinValue),
        SuccessTime("time-only.constant.maximum", "static System.TimeOnly.MaxValue.get", [], TimeOnly.MaxValue),
        SuccessTime("time-only.ctor.default", "System.TimeOnly.TimeOnly()", [], TimeOnly.MinValue),
        SuccessTime("time-only.ctor.hour-minute", "System.TimeOnly.TimeOnly(int, int)", [Number(23), Number(59)], new TimeOnly(23, 59)),
        SuccessTime("time-only.ctor.seconds", "System.TimeOnly.TimeOnly(int, int, int)", [Number(3), Number(4), Number(5)], new TimeOnly(3, 4, 5)),
        SuccessTime("time-only.ctor.milliseconds", "System.TimeOnly.TimeOnly(int, int, int, int)", [Number(3), Number(4), Number(5), Number(6)], new TimeOnly(3, 4, 5, 6)),
        SuccessTime("time-only.ctor.microseconds", "System.TimeOnly.TimeOnly(int, int, int, int, int)", [Number(3), Number(4), Number(5), Number(6), Number(7)], new TimeOnly(3, 4, 5, 6, 7)),
        SuccessTime("time-only.ctor.ticks", "System.TimeOnly.TimeOnly(long)", [Big(SampleValue.Ticks)], SampleValue),
        Failure("time-only.ctor.hour-out-of-range", "System.TimeOnly.TimeOnly(int, int)", [Number(24), Number(0)], "ArgumentOutOfRangeException"),
        Failure("time-only.ctor.ticks-negative", "System.TimeOnly.TimeOnly(long)", [Big(-1)], "ArgumentOutOfRangeException"),
        Failure("time-only.ctor.ticks-day", "System.TimeOnly.TimeOnly(long)", [Big(TimeSpan.TicksPerDay)], "ArgumentOutOfRangeException"),

        Success("time-only.property.hour", "System.TimeOnly.Hour.get", [Sample()], Number(SampleValue.Hour)),
        Success("time-only.property.minute", "System.TimeOnly.Minute.get", [Sample()], Number(SampleValue.Minute)),
        Success("time-only.property.second", "System.TimeOnly.Second.get", [Sample()], Number(SampleValue.Second)),
        Success("time-only.property.millisecond", "System.TimeOnly.Millisecond.get", [Sample()], Number(SampleValue.Millisecond)),
        Success("time-only.property.microsecond", "System.TimeOnly.Microsecond.get", [Sample()], Number(SampleValue.Microsecond)),
        Success("time-only.property.nanosecond", "System.TimeOnly.Nanosecond.get", [Sample()], Number(SampleValue.Nanosecond)),
        Success("time-only.property.ticks", "System.TimeOnly.Ticks.get", [Sample()], Big(SampleValue.Ticks)),

        SuccessTime("time-only.add.timespan-wraps-forward", "System.TimeOnly.Add(System.TimeSpan)", [Time(new TimeOnly(23, 30)), Span(TimeSpan.FromHours(2))], new TimeOnly(1, 30)),
        SuccessTime("time-only.add.timespan-wraps-backward", "System.TimeOnly.Add(System.TimeSpan)", [Time(new TimeOnly(0, 30)), Span(TimeSpan.FromHours(-2))], new TimeOnly(22, 30)),
        Success("time-only.add.timespan-out-forward", "System.TimeOnly.Add(System.TimeSpan, out int)", [Time(new TimeOnly(23, 30)), Span(TimeSpan.FromHours(49)), Number(0)], Array(TimeText(new TimeOnly(0, 30)), Number(3))),
        Success("time-only.add.timespan-out-backward", "System.TimeOnly.Add(System.TimeSpan, out int)", [Time(new TimeOnly(0, 30)), Span(TimeSpan.FromHours(-49))], Array(TimeText(new TimeOnly(23, 30)), Number(-3))),
        SuccessTime("time-only.add-hours.fraction", "System.TimeOnly.AddHours(double)", [Sample(), Number(1.5)], SampleValue.AddHours(1.5)),
        Success("time-only.add-hours.out", "System.TimeOnly.AddHours(double, out int)", [Time(new TimeOnly(23, 30)), Number(25.5), Number(0)], Array(TimeText(new TimeOnly(1, 0)), Number(2))),
        SuccessTime("time-only.add-minutes.fraction", "System.TimeOnly.AddMinutes(double)", [Sample(), Number(-4.5)], SampleValue.AddMinutes(-4.5)),
        Success("time-only.add-minutes.out", "System.TimeOnly.AddMinutes(double, out int)", [Time(new TimeOnly(0, 30)), Number(-90), Number(0)], Array(TimeText(new TimeOnly(23, 0)), Number(-1))),
        Failure("time-only.add-hours.nan", "System.TimeOnly.AddHours(double)", [Sample(), Number(double.NaN)], "ArgumentException"),
        Failure("time-only.add-minutes.infinity", "System.TimeOnly.AddMinutes(double)", [Sample(), Number(double.PositiveInfinity)], "ArgumentOutOfRangeException"),
        Success("time-only.is-between.simple-true", "System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)", [Time(new TimeOnly(12, 0)), Time(new TimeOnly(9, 0)), Time(new TimeOnly(17, 0))], Bool(true)),
        Success("time-only.is-between.simple-end-exclusive", "System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)", [Time(new TimeOnly(17, 0)), Time(new TimeOnly(9, 0)), Time(new TimeOnly(17, 0))], Bool(false)),
        Success("time-only.is-between-midnight-true", "System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)", [Time(new TimeOnly(1, 0)), Time(new TimeOnly(22, 0)), Time(new TimeOnly(2, 0))], Bool(true)),
        Success("time-only.is-between-midnight-false", "System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)", [Time(new TimeOnly(12, 0)), Time(new TimeOnly(22, 0)), Time(new TimeOnly(2, 0))], Bool(false)),

        Success("time-only.operator.equal", "static System.TimeOnly.operator ==(System.TimeOnly, System.TimeOnly)", [Sample(), Sample()], Bool(true)),
        Success("time-only.operator.not-equal", "static System.TimeOnly.operator !=(System.TimeOnly, System.TimeOnly)", [Sample(), Time(SampleValue.Add(TimeSpan.FromTicks(1)))], Bool(true)),
        Success("time-only.operator.greater", "static System.TimeOnly.operator >(System.TimeOnly, System.TimeOnly)", [Sample(), Time(new TimeOnly(1, 0))], Bool(true)),
        Success("time-only.operator.greater-or-equal", "static System.TimeOnly.operator >=(System.TimeOnly, System.TimeOnly)", [Sample(), Sample()], Bool(true)),
        Success("time-only.operator.less", "static System.TimeOnly.operator <(System.TimeOnly, System.TimeOnly)", [Time(new TimeOnly(1, 0)), Sample()], Bool(true)),
        Success("time-only.operator.less-or-equal", "static System.TimeOnly.operator <=(System.TimeOnly, System.TimeOnly)", [Sample(), Sample()], Bool(true)),
        Success("time-only.operator.subtract-circular", "static System.TimeOnly.operator -(System.TimeOnly, System.TimeOnly)", [Time(new TimeOnly(1, 0)), Time(new TimeOnly(23, 0))], SpanText(TimeSpan.FromHours(2))),

        Success("time-only.deconstruct.hour-minute", "System.TimeOnly.Deconstruct(out int, out int)", [Sample(), Number(0), Number(0)], Array(Number(3), Number(4))),
        Success("time-only.deconstruct.seconds", "System.TimeOnly.Deconstruct(out int, out int, out int)", [Sample(), Number(0), Number(0), Number(0)], Array(Number(3), Number(4), Number(5))),
        Success("time-only.deconstruct.milliseconds", "System.TimeOnly.Deconstruct(out int, out int, out int, out int)", [Sample(), Number(0), Number(0), Number(0), Number(0)], Array(Number(3), Number(4), Number(5), Number(6))),
        Success("time-only.deconstruct.microseconds", "System.TimeOnly.Deconstruct(out int, out int, out int, out int, out int)", [Sample(), Number(0), Number(0), Number(0), Number(0), Number(0)], Array(Number(3), Number(4), Number(5), Number(6), Number(7))),
        SuccessTime("time-only.from-timespan", "static System.TimeOnly.FromTimeSpan(System.TimeSpan)", [Span(SampleValue.ToTimeSpan())], SampleValue),
        Failure("time-only.from-timespan-out-of-range", "static System.TimeOnly.FromTimeSpan(System.TimeSpan)", [Span(TimeSpan.FromDays(1))], "ArgumentOutOfRangeException"),
        SuccessTime("time-only.from-datetime", "static System.TimeOnly.FromDateTime(System.DateTime)", [DateTimeValue(new DateTime(2024, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc).AddTicks(79))], new TimeOnly(3, 4, 5, 6, 7).Add(TimeSpan.FromTicks(9))),
        Success("time-only.to-timespan", "System.TimeOnly.ToTimeSpan()", [Sample()], SpanText(SampleValue.ToTimeSpan())),
        Success("time-only.compare-to-typed.less", "System.TimeOnly.CompareTo(System.TimeOnly)", [Time(new TimeOnly(1, 0)), Sample()], Number(-1)),
        Success("time-only.compare-to-object.null", "System.TimeOnly.CompareTo(object)", [Sample(), Null()], Number(1)),
        Success("time-only.compare-to-object.equal", "System.TimeOnly.CompareTo(object)", [Sample(), Sample()], Number(0)),
        Failure("time-only.compare-to-object-wrong-type", "System.TimeOnly.CompareTo(object)", [Sample(), Text("03:04")], "ArgumentException"),
        Success("time-only.equals-typed", "System.TimeOnly.Equals(System.TimeOnly)", [Sample(), Sample()], Bool(true)),
        Success("time-only.equals-object-wrong-type", "override System.TimeOnly.Equals(object)", [Sample(), Number(1)], Bool(false)),
        Success("time-only.hash-code", "override System.TimeOnly.GetHashCode()", [Sample()], Number(SampleValue.GetHashCode())),

        SuccessTime("time-only.parse.full-precision", "static System.TimeOnly.Parse(string)", [Text("03:04:05.0060079")], SampleValue),
        SuccessTime("time-only.parse.whitespace", "static System.TimeOnly.Parse(string)", [Text("  03:04  ")], new TimeOnly(3, 4)),
        SuccessTime("time-only.parse.provider", "static System.TimeOnly.Parse(string, System.IFormatProvider)", [Text("03:04:05"), Text("en-US")], new TimeOnly(3, 4, 5)),
        SuccessTime("time-only.parse.span-provider", "static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", [Text("03:04:05"), Text("en-US")], new TimeOnly(3, 4, 5)),
        SuccessTime("time-only.parse.style", "static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text(" 03:04:05 "), Text("en-US"), Number((int)DateTimeStyles.AllowWhiteSpaces)], new TimeOnly(3, 4, 5)),
        SuccessTime("time-only.parse.span-style", "static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("03:04:05"), Text("en-US"), Number((int)DateTimeStyles.None)], new TimeOnly(3, 4, 5)),
        Failure("time-only.parse.invalid-hour", "static System.TimeOnly.Parse(string)", [Text("24:00")], "FormatException"),
        Failure("time-only.parse.invalid-fraction", "static System.TimeOnly.Parse(string)", [Text("03:04:05.12345678")], "FormatException"),
        Failure("time-only.parse.invalid-style", "static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("03:04"), Text("en-US"), Number((int)DateTimeStyles.AssumeUniversal)], "ArgumentException"),
        Success("time-only.try-parse.valid", "static System.TimeOnly.TryParse(string, out System.TimeOnly)", [Text("03:04:05.0060079"), Time(TimeOnly.MinValue)], Array(Bool(true), TimeText(SampleValue))),
        Success("time-only.try-parse.invalid", "static System.TimeOnly.TryParse(string, out System.TimeOnly)", [Text("not-a-time"), Sample()], Array(Bool(false), TimeText(TimeOnly.MinValue))),
        Success("time-only.try-parse.null", "static System.TimeOnly.TryParse(string, out System.TimeOnly)", [Null(), Sample()], Array(Bool(false), TimeText(TimeOnly.MinValue))),
        Success("time-only.try-parse.span", "static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, out System.TimeOnly)", [Text("03:04"), Time(TimeOnly.MinValue)], Array(Bool(true), TimeText(new TimeOnly(3, 4)))),
        Success("time-only.try-parse.style", "static System.TimeOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)", [Text("03:04"), Text("en-US"), Number((int)DateTimeStyles.AllowWhiteSpaces), Time(TimeOnly.MinValue)], Array(Bool(true), TimeText(new TimeOnly(3, 4)))),
        Success("time-only.try-parse.span-style-invalid", "static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)", [Text("03:04"), Text("en-US"), Number((int)DateTimeStyles.AssumeUniversal), Time(TimeOnly.MinValue)], Array(Bool(false), TimeText(TimeOnly.MinValue))),
        Success("time-only.try-parse.provider", "static System.TimeOnly.TryParse(string, System.IFormatProvider, out System.TimeOnly)", [Text("03:04"), Text("en-US"), Time(TimeOnly.MinValue)], Array(Bool(true), TimeText(new TimeOnly(3, 4)))),
        Success("time-only.try-parse.span-provider", "static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeOnly)", [Text("03:04"), Text("en-US"), Time(TimeOnly.MinValue)], Array(Bool(true), TimeText(new TimeOnly(3, 4)))),

        Success("time-only.format.long-roundtrip", "static System.TimeOnly.Parse(string)", [Invoke("System.TimeOnly.ToLongTimeString()", Time(new TimeOnly(3, 4, 5)))], TimeText(new TimeOnly(3, 4, 5))),
        Success("time-only.format.short-roundtrip", "static System.TimeOnly.Parse(string)", [Invoke("System.TimeOnly.ToShortTimeString()", Time(new TimeOnly(3, 4)))], TimeText(new TimeOnly(3, 4))),
        Success("time-only.format.roundtrip", "System.TimeOnly.ToString(string)", [Sample(), Text("O")], TimeText(SampleValue)),
        Success("time-only.format.provider-roundtrip", "static System.TimeOnly.Parse(string, System.IFormatProvider)", [Invoke("System.TimeOnly.ToString(System.IFormatProvider)", Sample(), Text("en-US")), Text("en-US")], TimeText(SampleValue)),
        Success("time-only.format.custom-provider-roundtrip", "static System.TimeOnly.Parse(string, System.IFormatProvider)", [Invoke("System.TimeOnly.ToString(string, System.IFormatProvider)", Sample(), Text("O"), Text("en-US")), Text("en-US")], TimeText(SampleValue))
    ];

    private static ClrRuntimeValue Sample() => Time(SampleValue);
    private static ClrRuntimeValue Time(TimeOnly value) => Invoke("System.TimeOnly.TimeOnly(long)", Big(value.Ticks));
    private static ClrRuntimeValue Span(TimeSpan value) => Invoke("System.TimeSpan.TimeSpan(long)", Big(value.Ticks));

    private static ClrRuntimeValue DateTimeValue(DateTime value)
        => Invoke("System.DateTime.DateTime(long, System.DateTimeKind)", Big(value.Ticks), Number((int)value.Kind));

    private static ClrRuntimeValue TimeText(TimeOnly value)
        => Text(value.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture));

    private static ClrRuntimeValue SpanText(TimeSpan value)
        => Text(value.ToString("c", CultureInfo.InvariantCulture));

    private static ClrRuntimeScenario SuccessTime(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        TimeOnly expected)
        => Success(id, member, arguments, TimeText(expected));

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
