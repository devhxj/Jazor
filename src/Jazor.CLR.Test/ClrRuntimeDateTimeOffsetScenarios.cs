using System.Globalization;
using System.Numerics;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeDateTimeOffsetScenarios
{
    private const string ModulePath = "System/DateTimeOffsetModule.js";
    private static readonly DateTimeOffset SampleValue =
        new DateTimeOffset(2024, 1, 2, 3, 4, 5, 6, 7, TimeSpan.FromHours(5.5)).AddTicks(9);
    private static readonly DateTimeOffset SameInstantValue = SampleValue.ToOffset(TimeSpan.FromHours(-3));
    private static readonly DateTimeOffset FormatRoundTripValue =
        new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(5.5));

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        SuccessOffset("datetime-offset.constant.minimum", "static readonly System.DateTimeOffset.MinValue", [], DateTimeOffset.MinValue),
        SuccessOffset("datetime-offset.constant.maximum", "static readonly System.DateTimeOffset.MaxValue", [], DateTimeOffset.MaxValue),
        SuccessOffset("datetime-offset.constant.unix-epoch", "static readonly System.DateTimeOffset.UnixEpoch", [], DateTimeOffset.UnixEpoch),
        SuccessOffset("datetime-offset.ctor.default", "System.DateTimeOffset.DateTimeOffset()", [], default),
        SuccessOffset("datetime-offset.ctor.ticks-offset", "System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)", [Big(SampleValue.Ticks), Span(SampleValue.Offset)], SampleValue),
        Failure("datetime-offset.ctor.offset-not-whole-minute", "System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)", [Big(SampleValue.Ticks), Span(TimeSpan.FromSeconds(1))], "ArgumentException"),
        Failure("datetime-offset.ctor.offset-over-fourteen-hours", "System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)", [Big(SampleValue.Ticks), Span(TimeSpan.FromHours(14) + TimeSpan.FromMinutes(1))], "ArgumentOutOfRangeException"),
        Failure("datetime-offset.ctor.utc-range-underflow", "System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)", [Big(0), Span(TimeSpan.FromHours(1))], "ArgumentOutOfRangeException"),
        SuccessOffset("datetime-offset.ctor.datetime-utc", "System.DateTimeOffset.DateTimeOffset(System.DateTime)", [DateTimeValue(SampleValue.UtcDateTime)], new DateTimeOffset(SampleValue.UtcDateTime)),
        SuccessOffset("datetime-offset.ctor.datetime-unspecified-offset", "System.DateTimeOffset.DateTimeOffset(System.DateTime, System.TimeSpan)", [DateTimeValue(SampleValue.DateTime), Span(SampleValue.Offset)], SampleValue),
        Failure("datetime-offset.ctor.datetime-utc-nonzero-offset", "System.DateTimeOffset.DateTimeOffset(System.DateTime, System.TimeSpan)", [DateTimeValue(SampleValue.UtcDateTime), Span(TimeSpan.FromHours(1))], "ArgumentException"),
        SuccessOffset("datetime-offset.ctor.date-time-offset", "System.DateTimeOffset.DateTimeOffset(System.DateOnly, System.TimeOnly, System.TimeSpan)", [DateOnlyValue(DateOnly.FromDateTime(SampleValue.DateTime)), TimeOnlyValue(TimeOnly.FromDateTime(SampleValue.DateTime)), Span(SampleValue.Offset)], SampleValue),
        SuccessOffset("datetime-offset.ctor.components-seconds", "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)", [Number(2024), Number(2), Number(29), Number(23), Number(59), Number(58), Span(TimeSpan.FromHours(-4))], new DateTimeOffset(2024, 2, 29, 23, 59, 58, TimeSpan.FromHours(-4))),
        SuccessOffset("datetime-offset.ctor.components-milliseconds", "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.TimeSpan)", [Number(2024), Number(2), Number(29), Number(23), Number(59), Number(58), Number(999), Span(TimeSpan.FromHours(-4))], new DateTimeOffset(2024, 2, 29, 23, 59, 58, 999, TimeSpan.FromHours(-4))),
        SuccessOffset("datetime-offset.ctor.components-microseconds", "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.TimeSpan)", [Number(2024), Number(2), Number(29), Number(23), Number(59), Number(58), Number(999), Number(321), Span(TimeSpan.FromHours(-4))], new DateTimeOffset(2024, 2, 29, 23, 59, 58, 999, 321, TimeSpan.FromHours(-4))),
        Failure("datetime-offset.ctor.invalid-calendar-day", "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)", [Number(2023), Number(2), Number(29), Number(0), Number(0), Number(0), Span(TimeSpan.Zero)], "ArgumentOutOfRangeException"),
        Failure("datetime-offset.ctor.invalid-microsecond", "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.TimeSpan)", [Number(2024), Number(1), Number(1), Number(0), Number(0), Number(0), Number(0), Number(1000), Span(TimeSpan.Zero)], "ArgumentOutOfRangeException"),

        Success("datetime-offset.utc-now.offset-is-zero", "System.DateTimeOffset.Offset.get", [Invoke("static System.DateTimeOffset.UtcNow.get")], SpanText(TimeSpan.Zero)),
        Success("datetime-offset.now.local-offset", "System.DateTimeOffset.Offset.get", [Invoke("static System.DateTimeOffset.Now.get")], SpanText(TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.Now))),
        Success("datetime-offset.property.datetime", "System.DateTimeOffset.DateTime.get", [Sample()], DateTimeText(SampleValue.DateTime)),
        Success("datetime-offset.property.utc-datetime", "System.DateTimeOffset.UtcDateTime.get", [Sample()], DateTimeText(SampleValue.UtcDateTime)),
        Success("datetime-offset.property.local-datetime", "System.DateTimeOffset.LocalDateTime.get", [Sample()], DateTimeText(SampleValue.LocalDateTime)),
        SuccessOffset("datetime-offset.to-offset.same-instant", "System.DateTimeOffset.ToOffset(System.TimeSpan)", [Sample(), Span(SameInstantValue.Offset)], SameInstantValue),
        Failure("datetime-offset.to-offset.invalid", "System.DateTimeOffset.ToOffset(System.TimeSpan)", [Sample(), Span(TimeSpan.FromSeconds(30))], "ArgumentException"),
        Success("datetime-offset.property.date", "System.DateTimeOffset.Date.get", [Sample()], DateTimeText(SampleValue.Date)),
        Success("datetime-offset.property.day", "System.DateTimeOffset.Day.get", [Sample()], Number(SampleValue.Day)),
        Success("datetime-offset.property.day-of-week", "System.DateTimeOffset.DayOfWeek.get", [Sample()], Number((int)SampleValue.DayOfWeek)),
        Success("datetime-offset.property.day-of-year", "System.DateTimeOffset.DayOfYear.get", [Sample()], Number(SampleValue.DayOfYear)),
        Success("datetime-offset.property.hour", "System.DateTimeOffset.Hour.get", [Sample()], Number(SampleValue.Hour)),
        Success("datetime-offset.property.millisecond", "System.DateTimeOffset.Millisecond.get", [Sample()], Number(SampleValue.Millisecond)),
        Success("datetime-offset.property.microsecond", "System.DateTimeOffset.Microsecond.get", [Sample()], Number(SampleValue.Microsecond)),
        Success("datetime-offset.property.nanosecond", "System.DateTimeOffset.Nanosecond.get", [Sample()], Number(SampleValue.Nanosecond)),
        Success("datetime-offset.property.minute", "System.DateTimeOffset.Minute.get", [Sample()], Number(SampleValue.Minute)),
        Success("datetime-offset.property.month", "System.DateTimeOffset.Month.get", [Sample()], Number(SampleValue.Month)),
        Success("datetime-offset.property.offset", "System.DateTimeOffset.Offset.get", [Sample()], SpanText(SampleValue.Offset)),
        Success("datetime-offset.property.second", "System.DateTimeOffset.Second.get", [Sample()], Number(SampleValue.Second)),
        Success("datetime-offset.property.ticks", "System.DateTimeOffset.Ticks.get", [Sample()], Big(SampleValue.Ticks)),
        Success("datetime-offset.property.utc-ticks", "System.DateTimeOffset.UtcTicks.get", [Sample()], Big(SampleValue.UtcTicks)),
        Success("datetime-offset.property.time-of-day", "System.DateTimeOffset.TimeOfDay.get", [Sample()], SpanText(SampleValue.TimeOfDay)),
        Success("datetime-offset.property.year", "System.DateTimeOffset.Year.get", [Sample()], Number(SampleValue.Year)),

        SuccessOffset("datetime-offset.add.timespan-crosses-day", "System.DateTimeOffset.Add(System.TimeSpan)", [Sample(), Span(TimeSpan.FromHours(23))], SampleValue.Add(TimeSpan.FromHours(23))),
        SuccessOffset("datetime-offset.add-days.fraction", "System.DateTimeOffset.AddDays(double)", [Sample(), Number(1.25)], SampleValue.AddDays(1.25)),
        SuccessOffset("datetime-offset.add-hours.negative", "System.DateTimeOffset.AddHours(double)", [Sample(), Number(-1.5)], SampleValue.AddHours(-1.5)),
        SuccessOffset("datetime-offset.add-milliseconds.subtick", "System.DateTimeOffset.AddMilliseconds(double)", [Sample(), Number(0.00015)], SampleValue.AddMilliseconds(0.00015)),
        SuccessOffset("datetime-offset.add-microseconds.fraction", "System.DateTimeOffset.AddMicroseconds(double)", [Sample(), Number(1.25)], SampleValue.AddMicroseconds(1.25)),
        SuccessOffset("datetime-offset.add-minutes.crosses-hour", "System.DateTimeOffset.AddMinutes(double)", [Sample(), Number(61.5)], SampleValue.AddMinutes(61.5)),
        SuccessOffset("datetime-offset.add-months.clamps-leap-day", "System.DateTimeOffset.AddMonths(int)", [Offset(new DateTimeOffset(2024, 1, 31, 12, 0, 0, TimeSpan.FromHours(5.5))), Number(1)], new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.FromHours(5.5))),
        SuccessOffset("datetime-offset.add-months.negative", "System.DateTimeOffset.AddMonths(int)", [Offset(new DateTimeOffset(2024, 3, 31, 12, 0, 0, TimeSpan.FromHours(5.5))), Number(-1)], new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.FromHours(5.5))),
        SuccessOffset("datetime-offset.add-seconds.fraction", "System.DateTimeOffset.AddSeconds(double)", [Sample(), Number(30.25)], SampleValue.AddSeconds(30.25)),
        SuccessOffset("datetime-offset.add-ticks.sub-microsecond", "System.DateTimeOffset.AddTicks(long)", [Sample(), Big(9)], SampleValue.AddTicks(9)),
        SuccessOffset("datetime-offset.add-years.clamps-leap-day", "System.DateTimeOffset.AddYears(int)", [Offset(new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.FromHours(5.5))), Number(1)], new DateTimeOffset(2025, 2, 28, 12, 0, 0, TimeSpan.FromHours(5.5))),
        Failure("datetime-offset.add-days.nan", "System.DateTimeOffset.AddDays(double)", [Sample(), Number(double.NaN)], "ArgumentException"),
        Failure("datetime-offset.add-days.infinity", "System.DateTimeOffset.AddDays(double)", [Sample(), Number(double.PositiveInfinity)], "ArgumentOutOfRangeException"),
        Failure("datetime-offset.add-ticks.overflow", "System.DateTimeOffset.AddTicks(long)", [Offset(DateTimeOffset.MaxValue), Big(1)], "ArgumentOutOfRangeException"),

        Success("datetime-offset.compare.same-instant", "static System.DateTimeOffset.Compare(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), SameInstant()], Number(0)),
        Success("datetime-offset.compare.before", "static System.DateTimeOffset.Compare(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddTicks(1))], Number(-1)),
        Success("datetime-offset.compare-to-typed.after", "System.DateTimeOffset.CompareTo(System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddTicks(-1))], Number(1)),
        Success("datetime-offset.compare-to-object.null", "System.DateTimeOffset.CompareTo(object)", [Sample(), Null()], Number(1)),
        Success("datetime-offset.compare-to-object.same-instant", "System.DateTimeOffset.CompareTo(object)", [Sample(), SameInstant()], Number(0)),
        Failure("datetime-offset.compare-to-object.wrong-type", "System.DateTimeOffset.CompareTo(object)", [Sample(), Text("2024-01-02")], "ArgumentException"),
        Success("datetime-offset.equals-object.same-instant", "override System.DateTimeOffset.Equals(object)", [Sample(), SameInstant()], Bool(true)),
        Success("datetime-offset.equals-object.wrong-type", "override System.DateTimeOffset.Equals(object)", [Sample(), Number(1)], Bool(false)),
        Success("datetime-offset.equals-typed.same-instant", "System.DateTimeOffset.Equals(System.DateTimeOffset)", [Sample(), SameInstant()], Bool(true)),
        Success("datetime-offset.equals-exact.different-offset", "System.DateTimeOffset.EqualsExact(System.DateTimeOffset)", [Sample(), SameInstant()], Bool(false)),
        Success("datetime-offset.equals-static.different-instant", "static System.DateTimeOffset.Equals(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddTicks(1))], Bool(false)),
        Success("datetime-offset.hash-code.utc-instant", "override System.DateTimeOffset.GetHashCode()", [Sample()], Number(SampleValue.GetHashCode())),

        SuccessOffset("datetime-offset.file-time.precise", "static System.DateTimeOffset.FromFileTime(long)", [Big(133_486_382_450_060_079)], DateTimeOffset.FromFileTime(133_486_382_450_060_079)),
        Failure("datetime-offset.file-time.invalid", "static System.DateTimeOffset.FromFileTime(long)", [Big(-1)], "ArgumentOutOfRangeException"),
        SuccessOffset("datetime-offset.unix-seconds.epoch", "static System.DateTimeOffset.FromUnixTimeSeconds(long)", [Big(0)], DateTimeOffset.UnixEpoch),
        SuccessOffset("datetime-offset.unix-seconds.before-epoch", "static System.DateTimeOffset.FromUnixTimeSeconds(long)", [Big(-1)], DateTimeOffset.FromUnixTimeSeconds(-1)),
        Failure("datetime-offset.unix-seconds.out-of-range", "static System.DateTimeOffset.FromUnixTimeSeconds(long)", [Big(253_402_300_800)], "ArgumentOutOfRangeException"),
        SuccessOffset("datetime-offset.unix-milliseconds.precise", "static System.DateTimeOffset.FromUnixTimeMilliseconds(long)", [Big(1_704_067_445_006)], DateTimeOffset.FromUnixTimeMilliseconds(1_704_067_445_006)),
        Failure("datetime-offset.unix-milliseconds.out-of-range", "static System.DateTimeOffset.FromUnixTimeMilliseconds(long)", [Big(-62_135_596_800_001)], "ArgumentOutOfRangeException"),

        SuccessOffset("datetime-offset.parse.explicit-offset", "static System.DateTimeOffset.Parse(string)", [Text("2024-01-02T03:04:05.0060079+05:30")], SampleValue),
        SuccessOffset("datetime-offset.parse.utc", "static System.DateTimeOffset.Parse(string, System.IFormatProvider)", [Text("2024-01-02T03:04:05Z"), Text("en-US")], new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)),
        SuccessOffset("datetime-offset.parse.assume-universal", "static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02T03:04:05"), Text("en-US"), Number((int)DateTimeStyles.AssumeUniversal)], new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)),
        SuccessOffset("datetime-offset.parse.adjust-universal", "static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02T03:04:05+05:30"), Text("en-US"), Number((int)DateTimeStyles.AdjustToUniversal)], new DateTimeOffset(2024, 1, 1, 21, 34, 5, TimeSpan.Zero)),
        SuccessOffset("datetime-offset.parse.span-provider-style", "static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-02-29T23:59:58-04:00"), Text("en-US"), Number((int)DateTimeStyles.None)], new DateTimeOffset(2024, 2, 29, 23, 59, 58, TimeSpan.FromHours(-4))),
        SuccessOffset("datetime-offset.parse.span-provider", "static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", [Text("2024-01-02T03:04:05+05:30"), Text("en-US")], new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(5.5))),
        Failure("datetime-offset.parse.invalid-date", "static System.DateTimeOffset.Parse(string)", [Text("2023-02-29T00:00:00Z")], "FormatException"),
        Failure("datetime-offset.parse.invalid-offset", "static System.DateTimeOffset.Parse(string)", [Text("2024-01-02T03:04:05+15:00")], "FormatException"),
        Failure("datetime-offset.parse.conflicting-styles", "static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02"), Text("en-US"), Number((int)(DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal))], "ArgumentException"),

        Success("datetime-offset.subtract.offsets-by-instant", "System.DateTimeOffset.Subtract(System.DateTimeOffset)", [Sample(), SameInstant()], SpanText(TimeSpan.Zero)),
        SuccessOffset("datetime-offset.subtract.timespan", "System.DateTimeOffset.Subtract(System.TimeSpan)", [Sample(), Span(TimeSpan.FromHours(2))], SampleValue.Subtract(TimeSpan.FromHours(2))),
        Success("datetime-offset.to-file-time.roundtrip", "System.DateTimeOffset.ToFileTime()", [Invoke("static System.DateTimeOffset.FromFileTime(long)", Big(133_486_382_450_060_079))], Big(133_486_382_450_060_079)),
        Success("datetime-offset.to-unix-seconds.floor-before-epoch", "System.DateTimeOffset.ToUnixTimeSeconds()", [Offset(DateTimeOffset.UnixEpoch.AddTicks(-1))], Big(-1)),
        Success("datetime-offset.to-unix-milliseconds.floor-before-epoch", "System.DateTimeOffset.ToUnixTimeMilliseconds()", [Offset(DateTimeOffset.UnixEpoch.AddTicks(-1))], Big(-1)),
        SuccessOffset("datetime-offset.to-local-time", "System.DateTimeOffset.ToLocalTime()", [Sample()], SampleValue.ToLocalTime()),
        SuccessOffset("datetime-offset.format.default-roundtrip", "static System.DateTimeOffset.Parse(string)", [Invoke("override System.DateTimeOffset.ToString()", Offset(FormatRoundTripValue))], FormatRoundTripValue),
        Success("datetime-offset.format.roundtrip", "System.DateTimeOffset.ToString(string)", [Sample(), Text("O")], Text(SampleValue.ToString("O", CultureInfo.InvariantCulture))),
        SuccessOffset("datetime-offset.format.provider-roundtrip", "static System.DateTimeOffset.Parse(string, System.IFormatProvider)", [Invoke("System.DateTimeOffset.ToString(System.IFormatProvider)", Offset(FormatRoundTripValue), Text("en-US")), Text("en-US")], FormatRoundTripValue),
        Success("datetime-offset.format.custom-provider", "System.DateTimeOffset.ToString(string, System.IFormatProvider)", [Sample(), Text("yyyy-MM-dd HH:mm:ss.fffffff zzz"), Text("en-US")], Text(SampleValue.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.GetCultureInfo("en-US")))),
        SuccessOffset("datetime-offset.to-universal-time", "System.DateTimeOffset.ToUniversalTime()", [Sample()], SampleValue.ToUniversalTime()),

        Success("datetime-offset.try-parse.valid", "static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)", [Text("2024-01-02T03:04:05.0060079+05:30"), Offset(default)], Array(Bool(true), OffsetText(SampleValue))),
        Success("datetime-offset.try-parse.invalid", "static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)", [Text("not-a-date"), Sample()], Array(Bool(false), OffsetText(default))),
        Success("datetime-offset.try-parse.null", "static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)", [Null(), Sample()], Array(Bool(false), OffsetText(default))),
        Success("datetime-offset.try-parse.span", "static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, out System.DateTimeOffset)", [Text("2024-01-02T03:04:05Z"), Offset(default)], Array(Bool(true), OffsetText(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)))),
        Success("datetime-offset.try-parse.style", "static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)", [Text("2024-01-02T03:04:05"), Text("en-US"), Number((int)DateTimeStyles.AssumeUniversal), Offset(default)], Array(Bool(true), OffsetText(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)))),
        Success("datetime-offset.try-parse.span-style-invalid-input", "static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)", [Text("invalid"), Text("en-US"), Number((int)DateTimeStyles.None), Sample()], Array(Bool(false), OffsetText(default))),
        Failure("datetime-offset.try-parse.invalid-style", "static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)", [Text("2024-01-02"), Text("en-US"), Number(256), Offset(default)], "ArgumentException"),
        Success("datetime-offset.try-parse.provider", "static System.DateTimeOffset.TryParse(string, System.IFormatProvider, out System.DateTimeOffset)", [Text("2024-01-02T03:04:05+05:30"), Text("en-US"), Offset(default)], Array(Bool(true), OffsetText(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(5.5))))),
        Success("datetime-offset.try-parse.span-provider", "static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTimeOffset)", [Text("2024-01-02T03:04:05Z"), Text("en-US"), Offset(default)], Array(Bool(true), OffsetText(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)))),

        SuccessOffset("datetime-offset.implicit.datetime-utc", "static System.DateTimeOffset.implicit operator System.DateTimeOffset(System.DateTime)", [DateTimeValue(SampleValue.UtcDateTime)], new DateTimeOffset(SampleValue.UtcDateTime)),
        SuccessOffset("datetime-offset.operator.add", "static System.DateTimeOffset.operator +(System.DateTimeOffset, System.TimeSpan)", [Sample(), Span(TimeSpan.FromHours(2))], SampleValue + TimeSpan.FromHours(2)),
        SuccessOffset("datetime-offset.operator.subtract-timespan", "static System.DateTimeOffset.operator -(System.DateTimeOffset, System.TimeSpan)", [Sample(), Span(TimeSpan.FromHours(2))], SampleValue - TimeSpan.FromHours(2)),
        Success("datetime-offset.operator.subtract-offset", "static System.DateTimeOffset.operator -(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddHours(-2))], SpanText(TimeSpan.FromHours(2))),
        Success("datetime-offset.operator.equal", "static System.DateTimeOffset.operator ==(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), SameInstant()], Bool(true)),
        Success("datetime-offset.operator.not-equal", "static System.DateTimeOffset.operator !=(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddTicks(1))], Bool(true)),
        Success("datetime-offset.operator.less-than", "static System.DateTimeOffset.operator <(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddTicks(1))], Bool(true)),
        Success("datetime-offset.operator.less-than-or-equal", "static System.DateTimeOffset.operator <=(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), SameInstant()], Bool(true)),
        Success("datetime-offset.operator.greater-than", "static System.DateTimeOffset.operator >(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), Offset(SampleValue.AddTicks(-1))], Bool(true)),
        Success("datetime-offset.operator.greater-than-or-equal", "static System.DateTimeOffset.operator >=(System.DateTimeOffset, System.DateTimeOffset)", [Sample(), SameInstant()], Bool(true)),
        Success("datetime-offset.deconstruct", "System.DateTimeOffset.Deconstruct(out System.DateOnly, out System.TimeOnly, out System.TimeSpan)", [Sample(), DateOnlyValue(DateOnly.MinValue), TimeOnlyValue(TimeOnly.MinValue), Span(TimeSpan.Zero)], Array(Text("2024-01-02"), Text("03:04:05.0060079"), SpanText(SampleValue.Offset)))
    ];

    private static ClrRuntimeValue Sample() => Offset(SampleValue);
    private static ClrRuntimeValue SameInstant() => Offset(SameInstantValue);

    private static ClrRuntimeValue Offset(DateTimeOffset value)
        => Invoke("System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)", Big(value.Ticks), Span(value.Offset));

    private static ClrRuntimeValue DateTimeValue(DateTime value)
        => Invoke("System.DateTime.DateTime(long, System.DateTimeKind)", Big(value.Ticks), Number((int)value.Kind));

    private static ClrRuntimeValue DateOnlyValue(DateOnly value)
        => Invoke("System.DateOnly.DateOnly(int, int, int)", Number(value.Year), Number(value.Month), Number(value.Day));

    private static ClrRuntimeValue TimeOnlyValue(TimeOnly value)
        => Invoke("System.TimeOnly.TimeOnly(long)", Big(value.Ticks));

    private static ClrRuntimeValue Span(TimeSpan value)
        => Invoke("System.TimeSpan.TimeSpan(long)", Big(value.Ticks));

    private static ClrRuntimeValue OffsetText(DateTimeOffset value)
        => Text(value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture));

    private static ClrRuntimeValue DateTimeText(DateTime value)
        => Text(value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture));

    private static ClrRuntimeValue SpanText(TimeSpan value)
        => Text(value.ToString("c", CultureInfo.InvariantCulture));

    private static ClrRuntimeScenario SuccessOffset(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        DateTimeOffset expected)
        => Success(id, member, arguments, OffsetText(expected));

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
    private static ClrRuntimeValue Big(BigInteger value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
