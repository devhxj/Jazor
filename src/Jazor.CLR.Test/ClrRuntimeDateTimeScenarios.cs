using System.Numerics;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeDateTimeScenarios
{
    private const string ModulePath = "System/DateTimeModule.js";
    private const string MinText = "0001-01-01T00:00:00.0000000";
    private const string MaxText = "9999-12-31T23:59:59.9999999";
    private const string PreciseText = "2024-01-02T03:04:05.0060079";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("datetime.constant.min-value", "static readonly System.DateTime.MinValue", [], Text(MinText)),
        Success("datetime.constant.max-value", "static readonly System.DateTime.MaxValue", [], Text(MaxText)),
        Success("datetime.constant.unix-epoch", "static readonly System.DateTime.UnixEpoch", [], Text("1970-01-01T00:00:00.0000000")),
        Success("datetime.constant.unix-epoch-kind", "System.DateTime.Kind.get", [Invoke("static readonly System.DateTime.UnixEpoch")], Number(1)),
        Success("datetime.ctor.default", "System.DateTime.DateTime()", [], Text(MinText)),
        Success("datetime.ctor.ticks", "System.DateTime.DateTime(long)", [Big("638397614450060079")], Text(PreciseText)),
        Success("datetime.ctor.ticks-kind", "System.DateTime.Kind.get", [Invoke("System.DateTime.DateTime(long, System.DateTimeKind)", Big("621355968000000000"), Number(1))], Number(1)),
        Failure("datetime.ctor.ticks-negative", "System.DateTime.DateTime(long)", [Big(-1)], "ArgumentOutOfRangeException"),
        Failure("datetime.ctor.ticks-above-maximum", "System.DateTime.DateTime(long)", [Big("3155378976000000000")], "ArgumentOutOfRangeException"),
        Success("datetime.ctor.ymd-leap-day", "System.DateTime.DateTime(int, int, int)", [Number(2024), Number(2), Number(29)], Text("2024-02-29T00:00:00.0000000")),
        Failure("datetime.ctor.ymd-invalid-day", "System.DateTime.DateTime(int, int, int)", [Number(2023), Number(2), Number(29)], "ArgumentOutOfRangeException"),
        Failure("datetime.ctor.ymd-fractional-year", "System.DateTime.DateTime(int, int, int)", [Number(2024.5), Number(1), Number(1)], "ArgumentOutOfRangeException"),
        Success("datetime.ctor.components", "System.DateTime.DateTime(int, int, int, int, int, int)", [Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5)], Text("2024-01-02T03:04:05.0000000")),
        Success("datetime.ctor.components-kind", "System.DateTime.Kind.get", [Date(2024, 1, 2, 3, 4, 5, 6, 2)], Number(2)),
        Failure("datetime.ctor.components-invalid-hour", "System.DateTime.DateTime(int, int, int, int, int, int)", [Number(2024), Number(1), Number(2), Number(24), Number(0), Number(0)], "ArgumentOutOfRangeException"),
        Failure("datetime.ctor.components-invalid-kind", "System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)", [Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5), Number(3)], "ArgumentException"),
        Success("datetime.ctor.microsecond", "System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)", [Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5), Number(6), Number(7), Number(1)], Text("2024-01-02T03:04:05.0060070")),
        Failure("datetime.ctor.microsecond-out-of-range", "System.DateTime.DateTime(int, int, int, int, int, int, int, int)", [Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5), Number(6), Number(1000)], "ArgumentOutOfRangeException"),
        Success("datetime.ctor.components-millisecond", "System.DateTime.DateTime(int, int, int, int, int, int, int)", [Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5), Number(6)], Text("2024-01-02T03:04:05.0060000")),
        Success("datetime.ctor.date-only-time-only-unspecified", "System.DateTime.DateTime(System.DateOnly, System.TimeOnly)", [DateOnly(2024, 2, 29), TimeOnly(23, 59, 58, 999, 999)], Text("2024-02-29T23:59:58.9999990")),
        Success("datetime.ctor.date-only-time-only", "System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)", [DateOnly(2024, 2, 29), TimeOnly(23, 59, 58, 999, 999), Number(1)], Text("2024-02-29T23:59:58.9999990")),

        Success("datetime.property.date", "System.DateTime.Date.get", [PreciseUtc()], Text("2024-01-02T00:00:00.0000000")),
        Success("datetime.property.day", "System.DateTime.Day.get", [PreciseUtc()], Number(2)),
        Success("datetime.property.day-of-week", "System.DateTime.DayOfWeek.get", [PreciseUtc()], Number(2)),
        Success("datetime.property.day-of-year", "System.DateTime.DayOfYear.get", [PreciseUtc()], Number(2)),
        Success("datetime.property.hour", "System.DateTime.Hour.get", [PreciseUtc()], Number(3)),
        Success("datetime.property.kind", "System.DateTime.Kind.get", [PreciseUtc()], Number(1)),
        Success("datetime.property.millisecond", "System.DateTime.Millisecond.get", [PreciseUtc()], Number(6)),
        Success("datetime.property.microsecond", "System.DateTime.Microsecond.get", [PreciseUtc()], Number(7)),
        Success("datetime.property.nanosecond", "System.DateTime.Nanosecond.get", [PreciseUtc()], Number(900)),
        Success("datetime.property.minute", "System.DateTime.Minute.get", [PreciseUtc()], Number(4)),
        Success("datetime.property.month", "System.DateTime.Month.get", [PreciseUtc()], Number(1)),
        Success("datetime.property.second", "System.DateTime.Second.get", [PreciseUtc()], Number(5)),
        Success("datetime.property.ticks", "System.DateTime.Ticks.get", [PreciseUtc()], Big("638397614450060079")),
        Success("datetime.property.time-of-day", "System.DateTime.TimeOfDay.get", [PreciseUtc()], Text("03:04:05.0060079")),
        Success("datetime.property.year", "System.DateTime.Year.get", [PreciseUtc()], Number(2024)),

        Success("datetime.add.timespan", "System.DateTime.Add(System.TimeSpan)", [Date(2024, 1, 31, 23, 30, 0, 0, 1), TimeSpan(Big("54000000000"))], Text("2024-02-01T01:00:00.0000000")),
        Success("datetime.add-days.half", "System.DateTime.AddDays(double)", [Date(2024, 1, 31, 23, 30, 0, 0, 1), Number(0.5)], Text("2024-02-01T11:30:00.0000000")),
        Success("datetime.add-days.tick-precision", "System.DateTime.AddDays(double)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(0.000000001)], Text("2024-01-01T00:00:00.0000864")),
        Success("datetime.add-hours.tick-precision", "System.DateTime.AddHours(double)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(0.000000001)], Text("2024-01-01T00:00:00.0000036")),
        Success("datetime.add-milliseconds.tick-precision", "System.DateTime.AddMilliseconds(double)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(0.0006)], Text("2024-01-01T00:00:00.0000005")),
        Success("datetime.add-microseconds.tick-precision", "System.DateTime.AddMicroseconds(double)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(0.55)], Text("2024-01-01T00:00:00.0000005")),
        Success("datetime.add-minutes.crosses-midnight", "System.DateTime.AddMinutes(double)", [Date(2024, 1, 31, 23, 30, 0, 0, 1), Number(31)], Text("2024-02-01T00:01:00.0000000")),
        Success("datetime.add-seconds.fraction", "System.DateTime.AddSeconds(double)", [Date(2024, 1, 31, 23, 30, 0, 0, 1), Number(30.25)], Text("2024-01-31T23:30:30.2500000")),
        Success("datetime.add-ticks.sub-microsecond", "System.DateTime.AddTicks(long)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Big(9)], Text("2024-01-01T00:00:00.0000009")),
        Success("datetime.add-months.clamps-leap-day", "System.DateTime.AddMonths(int)", [Date(2024, 1, 31, 12, 0, 0, 0, 0), Number(1)], Text("2024-02-29T12:00:00.0000000")),
        Success("datetime.add-months.negative", "System.DateTime.AddMonths(int)", [Date(2024, 3, 31, 12, 0, 0, 0, 0), Number(-1)], Text("2024-02-29T12:00:00.0000000")),
        Success("datetime.add-years.clamps-leap-day", "System.DateTime.AddYears(int)", [Date(2024, 2, 29, 12, 0, 0, 0, 0), Number(1)], Text("2025-02-28T12:00:00.0000000")),
        Failure("datetime.add-days.nan", "System.DateTime.AddDays(double)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(double.NaN)], "ArgumentException"),
        Failure("datetime.add-days.infinity", "System.DateTime.AddDays(double)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(double.PositiveInfinity)], "ArgumentOutOfRangeException"),
        Failure("datetime.add-months.fractional", "System.DateTime.AddMonths(int)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Number(1.5)], "ArgumentOutOfRangeException"),
        Failure("datetime.add-ticks.overflow", "System.DateTime.AddTicks(long)", [Invoke("static readonly System.DateTime.MaxValue"), Big(1)], "ArgumentOutOfRangeException"),

        Success("datetime.compare.before", "static System.DateTime.Compare(System.DateTime, System.DateTime)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Date(2024, 1, 2, 0, 0, 0, 0, 0)], Number(-1)),
        Success("datetime.compare.ignores-kind", "static System.DateTime.Compare(System.DateTime, System.DateTime)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Date(2024, 1, 1, 0, 0, 0, 0, 1)], Number(0)),
        Success("datetime.compare-to-object.null", "System.DateTime.CompareTo(object)", [PreciseUtc(), Null()], Number(1)),
        Success("datetime.compare-to-object.datetime", "System.DateTime.CompareTo(object)", [PreciseUtc(), Date(2024, 1, 3, 0, 0, 0, 0, 0)], Number(-1)),
        Failure("datetime.compare-to-object.wrong-type", "System.DateTime.CompareTo(object)", [PreciseUtc(), Number(1)], "ArgumentException"),
        Success("datetime.compare-to.datetime", "System.DateTime.CompareTo(System.DateTime)", [PreciseUtc(), Date(2024, 1, 3, 0, 0, 0, 0, 0)], Number(-1)),
        Success("datetime.equals-object.same-ticks", "override System.DateTime.Equals(object)", [PreciseUtc(), Invoke("static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)", PreciseUtc(), Number(0))], Bool(true)),
        Success("datetime.equals-object.wrong-type", "override System.DateTime.Equals(object)", [PreciseUtc(), Number(1)], Bool(false)),
        Success("datetime.equals.datetime-same-ticks", "System.DateTime.Equals(System.DateTime)", [PreciseUtc(), Invoke("static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)", PreciseUtc(), Number(0))], Bool(true)),
        Success("datetime.equals-static.different", "static System.DateTime.Equals(System.DateTime, System.DateTime)", [PreciseUtc(), Date(2024, 1, 3, 0, 0, 0, 0, 1)], Bool(false)),
        Success("datetime.operator.equal", "static System.DateTime.operator ==(System.DateTime, System.DateTime)", [PreciseUtc(), Invoke("static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)", PreciseUtc(), Number(2))], Bool(true)),
        Success("datetime.operator.not-equal", "static System.DateTime.operator !=(System.DateTime, System.DateTime)", [PreciseUtc(), Date(2024, 1, 3, 0, 0, 0, 0, 1)], Bool(true)),
        Success("datetime.operator.less-than", "static System.DateTime.operator <(System.DateTime, System.DateTime)", [Date(2024, 1, 1, 0, 0, 0, 0, 0), Date(2024, 1, 2, 0, 0, 0, 0, 0)], Bool(true)),
        Success("datetime.operator.less-than-or-equal", "static System.DateTime.operator <=(System.DateTime, System.DateTime)", [PreciseUtc(), PreciseUtc()], Bool(true)),
        Success("datetime.operator.greater-than", "static System.DateTime.operator >(System.DateTime, System.DateTime)", [Date(2024, 1, 2, 0, 0, 0, 0, 0), Date(2024, 1, 1, 0, 0, 0, 0, 0)], Bool(true)),
        Success("datetime.operator.greater-than-or-equal", "static System.DateTime.operator >=(System.DateTime, System.DateTime)", [PreciseUtc(), PreciseUtc()], Bool(true)),

        Success("datetime.days-in-month.leap-february", "static System.DateTime.DaysInMonth(int, int)", [Number(2024), Number(2)], Number(29)),
        Success("datetime.days-in-month.common-february", "static System.DateTime.DaysInMonth(int, int)", [Number(2100), Number(2)], Number(28)),
        Failure("datetime.days-in-month.invalid-month", "static System.DateTime.DaysInMonth(int, int)", [Number(2024), Number(13)], "ArgumentOutOfRangeException"),
        Success("datetime.is-leap-year.century", "static System.DateTime.IsLeapYear(int)", [Number(2000)], Bool(true)),
        Success("datetime.is-leap-year.non-leap-century", "static System.DateTime.IsLeapYear(int)", [Number(2100)], Bool(false)),
        Failure("datetime.is-leap-year.invalid", "static System.DateTime.IsLeapYear(int)", [Number(0)], "ArgumentOutOfRangeException"),

        Success("datetime.specify-kind.preserves-value", "static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)", [PreciseUtc(), Number(0)], Text(PreciseText)),
        Success("datetime.specify-kind.changes-kind", "System.DateTime.Kind.get", [Invoke("static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)", PreciseUtc(), Number(2))], Number(2)),
        Success("datetime.binary.utc-roundtrip", "static System.DateTime.FromBinary(long)", [Invoke("System.DateTime.ToBinary()", PreciseUtc())], Text(PreciseText)),
        Success("datetime.binary.utc-roundtrip-kind", "System.DateTime.Kind.get", [Invoke("static System.DateTime.FromBinary(long)", Invoke("System.DateTime.ToBinary()", PreciseUtc()))], Number(1)),
        Success("datetime.file-time.utc-epoch", "static System.DateTime.FromFileTimeUtc(long)", [Big(0)], Text("1601-01-01T00:00:00.0000000")),
        Success("datetime.file-time.utc-roundtrip", "System.DateTime.ToFileTimeUtc()", [Invoke("static System.DateTime.FromFileTimeUtc(long)", Big("133486382450060079"))], Big("133486382450060079")),
        Success("datetime.file-time.local-roundtrip", "System.DateTime.ToFileTime()", [Invoke("static System.DateTime.FromFileTime(long)", Big("133486382450060079"))], Big("133486382450060079")),
        Failure("datetime.file-time.negative", "static System.DateTime.FromFileTimeUtc(long)", [Big(-1)], "ArgumentOutOfRangeException"),
        Success("datetime.oa-date.zero", "static System.DateTime.FromOADate(double)", [Number(0)], Text("1899-12-30T00:00:00.0000000")),
        Success("datetime.oa-date.roundtrip", "System.DateTime.ToOADate()", [Invoke("static System.DateTime.FromOADate(double)", Number(45293.5))], Number(45293.5)),
        Success("datetime.daylight-saving.utc", "System.DateTime.IsDaylightSavingTime()", [PreciseUtc()], Bool(false)),
        Success("datetime.to-local-time.kind", "System.DateTime.Kind.get", [Invoke("System.DateTime.ToLocalTime()", PreciseUtc())], Number(2)),
        Success("datetime.to-universal.utc-no-op", "System.DateTime.ToUniversalTime()", [PreciseUtc()], Text(PreciseText)),

        Success("datetime.subtract.datetime", "System.DateTime.Subtract(System.DateTime)", [Date(2024, 1, 3, 2, 3, 4, 0, 0), Invoke("System.DateTime.AddTicks(long)", Date(2024, 1, 2, 0, 0, 0, 0, 0), Big(-5))], Text("1.02:03:04.0000005")),
        Success("datetime.subtract.timespan", "System.DateTime.Subtract(System.TimeSpan)", [Date(2024, 1, 2, 0, 0, 0, 0, 1), TimeSpan(Big(1))], Text("2024-01-01T23:59:59.9999999")),
        Success("datetime.operator.add-timespan", "static System.DateTime.operator +(System.DateTime, System.TimeSpan)", [Date(2024, 1, 1, 0, 0, 0, 0, 1), TimeSpan(Big(1))], Text("2024-01-01T00:00:00.0000001")),
        Success("datetime.operator.subtract-timespan", "static System.DateTime.operator -(System.DateTime, System.TimeSpan)", [Date(2024, 1, 1, 0, 0, 0, 0, 1), TimeSpan(Big(1))], Text("2023-12-31T23:59:59.9999999")),
        Success("datetime.operator.subtract-datetime", "static System.DateTime.operator -(System.DateTime, System.DateTime)", [Date(2024, 1, 2, 0, 0, 0, 0, 0), Date(2024, 1, 1, 0, 0, 0, 0, 0)], Text("1.00:00:00")),
        Success("datetime.deconstruct.date-time", "System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)", [PreciseUtc(), DateOnly(1, 1, 1), TimeOnly(0, 0, 0, 0, 0)], Array(Text("2024-01-02"), Text("03:04:05.0060079"))),
        Success("datetime.deconstruct.components", "System.DateTime.Deconstruct(out int, out int, out int)", [PreciseUtc(), Number(0), Number(0), Number(0)], Array(Number(2024), Number(1), Number(2))),

        Success("datetime.format.invariant-default", "System.DateTime.ToString(System.IFormatProvider)", [PreciseUtc(), Text("")], Text("01/02/2024 03:04:05")),
        Success("datetime.format.roundtrip-utc", "System.DateTime.ToString(string, System.IFormatProvider)", [PreciseUtc(), Text("O"), Text("")], Text("2024-01-02T03:04:05.0060079Z")),
        Success("datetime.format.sortable", "System.DateTime.ToString(string, System.IFormatProvider)", [PreciseUtc(), Text("s"), Text("")], Text("2024-01-02T03:04:05")),
        Success("datetime.format.rfc1123", "System.DateTime.ToString(string, System.IFormatProvider)", [PreciseUtc(), Text("R"), Text("")], Text("Tue, 02 Jan 2024 03:04:05 GMT")),
        Success("datetime.format.custom-fraction-kind", "System.DateTime.ToString(string, System.IFormatProvider)", [PreciseUtc(), Text("yyyy-MM-dd HH:mm:ss.fffffff K"), Text("")], Text("2024-01-02 03:04:05.0060079 Z")),
        Success("datetime.format.custom-trimmed-fraction", "System.DateTime.ToString(string, System.IFormatProvider)", [Invoke("System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)", Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5), Number(6), Number(700), Number(0)), Text("HH:mm:ss.FFFFFFF"), Text("")], Text("03:04:05.0067")),
        Success("datetime.format.german-date-separator", "System.DateTime.ToString(string, System.IFormatProvider)", [PreciseUtc(), Text("dd/MM/yyyy"), Text("de-DE")], Text("02.01.2024")),
        Failure("datetime.format.invalid-standard", "System.DateTime.ToString(string, System.IFormatProvider)", [PreciseUtc(), Text("X"), Text("")], "FormatException"),
        Success("datetime.format.roundtrip-without-provider", "System.DateTime.ToString(string)", [PreciseUtc(), Text("O")], Text("2024-01-02T03:04:05.0060079Z")),
        Success("datetime.format.default-roundtrip", "static System.DateTime.Parse(string)", [Invoke("override System.DateTime.ToString()", Date(2024, 1, 2, 3, 4, 5, 0, 0))], Text("2024-01-02T03:04:05.0000000")),
        Success("datetime.format.long-date-roundtrip", "static System.DateTime.Parse(string)", [Invoke("System.DateTime.ToLongDateString()", Date(2024, 1, 2, 0, 0, 0, 0, 0))], Text("2024-01-02T00:00:00.0000000")),
        Success("datetime.format.short-date-roundtrip", "static System.DateTime.Parse(string)", [Invoke("System.DateTime.ToShortDateString()", Date(2024, 1, 2, 0, 0, 0, 0, 0))], Text("2024-01-02T00:00:00.0000000")),
        Success("datetime.format.long-time-hour", "System.DateTime.Hour.get", [Invoke("static System.DateTime.Parse(string)", Invoke("System.DateTime.ToLongTimeString()", Date(2024, 1, 2, 3, 4, 5, 0, 0)))], Number(3)),
        Success("datetime.format.short-time-hour", "System.DateTime.Hour.get", [Invoke("static System.DateTime.Parse(string)", Invoke("System.DateTime.ToShortTimeString()", Date(2024, 1, 2, 3, 4, 5, 0, 0)))], Number(3)),
        Success("datetime.hash-code.precise-value", "override System.DateTime.GetHashCode()", [PreciseUtc()], Number(1914478608)),

        Success("datetime.parse.iso-unspecified", "static System.DateTime.Parse(string)", [Text("2024-01-02T03:04:05.0060079")], Text(PreciseText)),
        Success("datetime.parse.iso-whitespace", "static System.DateTime.Parse(string)", [Text("  2024-02-29 23:59:58  ")], Text("2024-02-29T23:59:58.0000000")),
        Failure("datetime.parse.empty", "static System.DateTime.Parse(string)", [Text("   ")], "FormatException"),
        Failure("datetime.parse.invalid-date", "static System.DateTime.Parse(string)", [Text("2023-02-29")], "FormatException"),
        Failure("datetime.parse.null", "static System.DateTime.Parse(string)", [Null()], "ArgumentNullException"),
        Success("datetime.parse.roundtrip-z-kind", "System.DateTime.Kind.get", [Invoke("static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", Text("2024-01-02T03:04:05Z"), Text(""), Number(128))], Number(1)),
        Success("datetime.parse.adjust-offset-to-utc", "static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02T03:04:05+02:00"), Text(""), Number(16)], Text("2024-01-02T01:04:05.0000000")),
        Success("datetime.parse.assume-universal-adjust", "static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02T03:04:05"), Text(""), Number(80)], Text("2024-01-02T03:04:05.0000000")),
        Success("datetime.parse.no-current-date-default", "static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("12:34:56.0000007"), Text(""), Number(8)], Text("0001-01-01T12:34:56.0000007")),
        Failure("datetime.parse.conflicting-assumptions", "static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02"), Text(""), Number(96)], "ArgumentException"),
        Failure("datetime.parse.roundtrip-conflict", "static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02"), Text(""), Number(144)], "ArgumentException"),
        Failure("datetime.parse.undefined-style", "static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02"), Text(""), Number(256)], "ArgumentException"),
        Success("datetime.parse.provider", "static System.DateTime.Parse(string, System.IFormatProvider)", [Text("2024-01-02T03:04:05.0060079"), Text("")], Text(PreciseText)),
        Success("datetime.parse.span-provider", "static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", [Text("2024-01-02T03:04:05.0060079"), Text("")], Text(PreciseText)),
        Success("datetime.parse.span-provider-styles", "static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", [Text("2024-01-02T03:04:05.0060079"), Text(""), Number(0)], Text(PreciseText)),
        Success("datetime.try-parse.valid", "static System.DateTime.TryParse(string, out System.DateTime)", [Text("2024-01-02T03:04:05.0060079"), Invoke("System.DateTime.DateTime()")], Array(Bool(true), Text(PreciseText))),
        Success("datetime.try-parse.invalid", "static System.DateTime.TryParse(string, out System.DateTime)", [Text("not-a-date"), PreciseUtc()], Array(Bool(false), Text(MinText))),
        Success("datetime.try-parse.null", "static System.DateTime.TryParse(string, out System.DateTime)", [Null(), PreciseUtc()], Array(Bool(false), Text(MinText))),
        Success("datetime.try-parse.provider", "static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)", [Text("2024-01-02T03:04:05.0060079"), Text(""), Invoke("System.DateTime.DateTime()")], Array(Bool(true), Text(PreciseText))),
        Success("datetime.try-parse.span", "static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)", [Text("2024-01-02T03:04:05.0060079"), Invoke("System.DateTime.DateTime()")], Array(Bool(true), Text(PreciseText))),
        Success("datetime.try-parse.span-provider", "static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)", [Text("2024-01-02T03:04:05.0060079"), Text(""), Invoke("System.DateTime.DateTime()")], Array(Bool(true), Text(PreciseText))),
        Success("datetime.try-parse.span-provider-styles", "static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", [Text("2024-01-02T03:04:05.0060079"), Text(""), Number(0), Invoke("System.DateTime.DateTime()")], Array(Bool(true), Text(PreciseText))),
        Failure("datetime.try-parse.invalid-style", "static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", [Text("2024-01-02"), Text(""), Number(256), Invoke("System.DateTime.DateTime()")], "ArgumentException"),

        Success("datetime.current.now-kind", "System.DateTime.Kind.get", [Invoke("static System.DateTime.Now.get")], Number(2)),
        Success("datetime.current.today-kind", "System.DateTime.Kind.get", [Invoke("static System.DateTime.Today.get")], Number(2)),
        Success("datetime.current.utc-now-kind", "System.DateTime.Kind.get", [Invoke("static System.DateTime.UtcNow.get")], Number(1))
    ];

    private static ClrRuntimeValue Date(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int millisecond,
        int kind)
        => Invoke(
            "System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)",
            Number(year), Number(month), Number(day), Number(hour), Number(minute), Number(second), Number(millisecond), Number(kind));

    private static ClrRuntimeValue PreciseUtc()
        => Invoke(
            "System.DateTime.AddTicks(long)",
            Invoke(
                "System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)",
                Number(2024), Number(1), Number(2), Number(3), Number(4), Number(5), Number(6), Number(7), Number(1)),
            Big(9));

    private static ClrRuntimeValue DateOnly(int year, int month, int day)
        => Invoke("System.DateOnly.DateOnly(int, int, int)", Number(year), Number(month), Number(day));

    private static ClrRuntimeValue TimeOnly(int hour, int minute, int second, int millisecond, int microsecond)
        => Invoke(
            "System.TimeOnly.TimeOnly(int, int, int, int, int)",
            Number(hour), Number(minute), Number(second), Number(millisecond), Number(microsecond));

    private static ClrRuntimeValue TimeSpan(ClrRuntimeValue ticks)
        => Invoke("System.TimeSpan.TimeSpan(long)", ticks);

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
    private static ClrRuntimeValue Big(string value) => ClrRuntimeValue.BigInt(BigInteger.Parse(value));
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
