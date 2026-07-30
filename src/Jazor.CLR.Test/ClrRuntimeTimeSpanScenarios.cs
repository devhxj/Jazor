using System.Globalization;
using System.Numerics;

namespace Jazor.CLR.Test;

internal static class ClrRuntimeTimeSpanScenarios
{
    private const string ModulePath = "System/TimeSpanModule.js";
    private static readonly TimeSpan SampleValue = new TimeSpan(2, 3, 4, 5, 6, 7) + TimeSpan.FromTicks(9);
    private static readonly TimeSpan NegativeSampleValue = -SampleValue;

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        SuccessSpan("timespan.constant.zero", "static readonly System.TimeSpan.Zero", [], TimeSpan.Zero),
        SuccessSpan("timespan.constant.maximum", "static readonly System.TimeSpan.MaxValue", [], TimeSpan.MaxValue),
        SuccessSpan("timespan.constant.minimum", "static readonly System.TimeSpan.MinValue", [], TimeSpan.MinValue),

        SuccessSpan("timespan.ctor.default", "System.TimeSpan.TimeSpan()", [], TimeSpan.Zero),
        SuccessSpan("timespan.ctor.ticks-positive", "System.TimeSpan.TimeSpan(long)", [Big(12_345_678)], TimeSpan.FromTicks(12_345_678)),
        SuccessSpan("timespan.ctor.ticks-negative", "System.TimeSpan.TimeSpan(long)", [Big(-12_345_678)], TimeSpan.FromTicks(-12_345_678)),
        SuccessSpan("timespan.ctor.hours-minutes-seconds", "System.TimeSpan.TimeSpan(int, int, int)", [Number(25), Number(61), Number(62)], new TimeSpan(25, 61, 62)),
        SuccessSpan("timespan.ctor.negative-components", "System.TimeSpan.TimeSpan(int, int, int)", [Number(-1), Number(-2), Number(-3)], new TimeSpan(-1, -2, -3)),
        SuccessSpan("timespan.ctor.days-through-seconds", "System.TimeSpan.TimeSpan(int, int, int, int)", [Number(2), Number(3), Number(4), Number(5)], new TimeSpan(2, 3, 4, 5)),
        SuccessSpan("timespan.ctor.days-through-milliseconds", "System.TimeSpan.TimeSpan(int, int, int, int, int)", [Number(2), Number(3), Number(4), Number(5), Number(6)], new TimeSpan(2, 3, 4, 5, 6)),
        SuccessSpan("timespan.ctor.days-through-microseconds", "System.TimeSpan.TimeSpan(int, int, int, int, int, int)", [Number(2), Number(3), Number(4), Number(5), Number(6), Number(7)], new TimeSpan(2, 3, 4, 5, 6, 7)),
        Failure("timespan.ctor.overflow", "System.TimeSpan.TimeSpan(long)", [Big(BigInteger.Parse("9223372036854775808", CultureInfo.InvariantCulture))], "OverflowException"),

        Success("timespan.property.ticks", "System.TimeSpan.Ticks.get", [Sample()], Big(SampleValue.Ticks)),
        Success("timespan.property.days", "System.TimeSpan.Days.get", [Sample()], Number(SampleValue.Days)),
        Success("timespan.property.hours", "System.TimeSpan.Hours.get", [Sample()], Number(SampleValue.Hours)),
        Success("timespan.property.minutes", "System.TimeSpan.Minutes.get", [Sample()], Number(SampleValue.Minutes)),
        Success("timespan.property.seconds", "System.TimeSpan.Seconds.get", [Sample()], Number(SampleValue.Seconds)),
        Success("timespan.property.milliseconds", "System.TimeSpan.Milliseconds.get", [Sample()], Number(SampleValue.Milliseconds)),
        Success("timespan.property.microseconds", "System.TimeSpan.Microseconds.get", [Sample()], Number(SampleValue.Microseconds)),
        Success("timespan.property.nanoseconds", "System.TimeSpan.Nanoseconds.get", [Sample()], Number(SampleValue.Nanoseconds)),
        Success("timespan.property.negative-days", "System.TimeSpan.Days.get", [NegativeSample()], Number(NegativeSampleValue.Days)),
        Success("timespan.property.negative-hours", "System.TimeSpan.Hours.get", [NegativeSample()], Number(NegativeSampleValue.Hours)),
        Success("timespan.property.negative-microseconds", "System.TimeSpan.Microseconds.get", [NegativeSample()], Number(NegativeSampleValue.Microseconds)),
        Success("timespan.property.negative-nanoseconds", "System.TimeSpan.Nanoseconds.get", [NegativeSample()], Number(NegativeSampleValue.Nanoseconds)),
        Success("timespan.property.total-days", "System.TimeSpan.TotalDays.get", [Sample()], Number(SampleValue.TotalDays)),
        Success("timespan.property.total-hours", "System.TimeSpan.TotalHours.get", [Sample()], Number(SampleValue.TotalHours)),
        Success("timespan.property.total-minutes", "System.TimeSpan.TotalMinutes.get", [Sample()], Number(SampleValue.TotalMinutes)),
        Success("timespan.property.total-seconds", "System.TimeSpan.TotalSeconds.get", [Sample()], Number(SampleValue.TotalSeconds)),
        Success("timespan.property.total-milliseconds", "System.TimeSpan.TotalMilliseconds.get", [Sample()], Number(SampleValue.TotalMilliseconds)),
        Success("timespan.property.total-microseconds", "System.TimeSpan.TotalMicroseconds.get", [Sample()], Number(SampleValue.TotalMicroseconds)),
        Success("timespan.property.total-nanoseconds", "System.TimeSpan.TotalNanoseconds.get", [Sample()], Number(SampleValue.TotalNanoseconds)),

        SuccessSpan("timespan.add.crosses-day", "System.TimeSpan.Add(System.TimeSpan)", [Span(TimeSpan.FromHours(23)), Span(TimeSpan.FromHours(2))], TimeSpan.FromHours(25)),
        Failure("timespan.add.overflow", "System.TimeSpan.Add(System.TimeSpan)", [Span(TimeSpan.MaxValue), Span(TimeSpan.FromTicks(1))], "OverflowException"),
        Success("timespan.compare.less", "static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)", [Span(TimeSpan.FromSeconds(1)), Span(TimeSpan.FromSeconds(2))], Number(-1)),
        Success("timespan.compare.equal", "static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)", [Sample(), Sample()], Number(0)),
        Success("timespan.compare.greater", "static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)", [Span(TimeSpan.FromSeconds(2)), Span(TimeSpan.FromSeconds(1))], Number(1)),
        Success("timespan.compare-to-object.null", "System.TimeSpan.CompareTo(object)", [Sample(), Null()], Number(1)),
        Success("timespan.compare-to-object.timespan", "System.TimeSpan.CompareTo(object)", [Sample(), Span(SampleValue + TimeSpan.FromTicks(1))], Number(-1)),
        Failure("timespan.compare-to-object.wrong-type", "System.TimeSpan.CompareTo(object)", [Sample(), Number(1)], "ArgumentException"),
        Success("timespan.compare-to-typed.greater", "System.TimeSpan.CompareTo(System.TimeSpan)", [Sample(), NegativeSample()], Number(1)),
        SuccessSpan("timespan.duration.negative", "System.TimeSpan.Duration()", [NegativeSample()], SampleValue),
        SuccessSpan("timespan.duration.positive", "System.TimeSpan.Duration()", [Sample()], SampleValue),
        Failure("timespan.duration.minimum-overflow", "System.TimeSpan.Duration()", [Span(TimeSpan.MinValue)], "OverflowException"),
        Success("timespan.equals-object.equal", "override System.TimeSpan.Equals(object)", [Sample(), Sample()], Bool(true)),
        Success("timespan.equals-object.wrong-type", "override System.TimeSpan.Equals(object)", [Sample(), Text(SampleValue.ToString())], Bool(false)),
        Success("timespan.equals-typed.different", "System.TimeSpan.Equals(System.TimeSpan)", [Sample(), NegativeSample()], Bool(false)),
        Success("timespan.equals-static.equal", "static System.TimeSpan.Equals(System.TimeSpan, System.TimeSpan)", [Sample(), Sample()], Bool(true)),
        Success("timespan.hash-code.matches-int64", "override System.TimeSpan.GetHashCode()", [Sample()], Number(SampleValue.GetHashCode())),

        SuccessSpan("timespan.from-days.double", "static System.TimeSpan.FromDays(double)", [Number(1.25)], TimeSpan.FromDays(1.25)),
        SuccessSpan("timespan.from-days.double-subtick", "static System.TimeSpan.FromDays(double)", [Number(1d / TimeSpan.TicksPerDay / 2d)], TimeSpan.FromDays(1d / TimeSpan.TicksPerDay / 2d)),
        Failure("timespan.from-days.double-nan", "static System.TimeSpan.FromDays(double)", [Number(double.NaN)], "ArgumentException"),
        Failure("timespan.from-days.double-infinity", "static System.TimeSpan.FromDays(double)", [Number(double.PositiveInfinity)], "OverflowException"),
        SuccessSpan("timespan.from-days.int", "static System.TimeSpan.FromDays(int)", [Number(-3)], TimeSpan.FromDays(-3)),
        SuccessSpan("timespan.from-days.components", "static System.TimeSpan.FromDays(int, int, long, long, long, long)", [Number(2), Number(3), Big(4), Big(5), Big(6), Big(7)], new TimeSpan(2, 3, 4, 5, 6, 7)),
        SuccessSpan("timespan.from-hours.int", "static System.TimeSpan.FromHours(int)", [Number(49)], TimeSpan.FromHours(49)),
        SuccessSpan("timespan.from-hours.components", "static System.TimeSpan.FromHours(int, long, long, long, long)", [Number(25), Big(4), Big(5), Big(6), Big(7)], new TimeSpan(0, 25, 4, 5, 6, 7)),
        SuccessSpan("timespan.from-minutes.long", "static System.TimeSpan.FromMinutes(long)", [Big(-90)], TimeSpan.FromMinutes(-90)),
        SuccessSpan("timespan.from-minutes.components", "static System.TimeSpan.FromMinutes(long, long, long, long)", [Big(61), Big(2), Big(3), Big(4)], new TimeSpan(0, 0, 61, 2, 3, 4)),
        SuccessSpan("timespan.from-seconds.long", "static System.TimeSpan.FromSeconds(long)", [Big(90)], TimeSpan.FromSeconds(90)),
        SuccessSpan("timespan.from-seconds.components", "static System.TimeSpan.FromSeconds(long, long, long)", [Big(-61), Big(-2), Big(-3)], new TimeSpan(0, 0, 0, -61, -2, -3)),
        SuccessSpan("timespan.from-milliseconds.long", "static System.TimeSpan.FromMilliseconds(long)", [Big(1_234)], TimeSpan.FromMilliseconds(1_234)),
        SuccessSpan("timespan.from-milliseconds.components", "static System.TimeSpan.FromMilliseconds(long, long)", [Big(1_234), Big(567)], TimeSpan.FromTicks(12_345_670)),
        SuccessSpan("timespan.from-microseconds.long", "static System.TimeSpan.FromMicroseconds(long)", [Big(-1_234)], TimeSpan.FromMicroseconds(-1_234)),
        SuccessSpan("timespan.from-hours.double", "static System.TimeSpan.FromHours(double)", [Number(-1.5)], TimeSpan.FromHours(-1.5)),
        SuccessSpan("timespan.from-milliseconds.double-half-even-down", "static System.TimeSpan.FromMilliseconds(double)", [Number(0.00005)], TimeSpan.FromMilliseconds(0.00005)),
        SuccessSpan("timespan.from-milliseconds.double-half-even-up", "static System.TimeSpan.FromMilliseconds(double)", [Number(0.00015)], TimeSpan.FromMilliseconds(0.00015)),
        SuccessSpan("timespan.from-microseconds.double", "static System.TimeSpan.FromMicroseconds(double)", [Number(1.25)], TimeSpan.FromMicroseconds(1.25)),
        SuccessSpan("timespan.from-minutes.double", "static System.TimeSpan.FromMinutes(double)", [Number(1.25)], TimeSpan.FromMinutes(1.25)),
        SuccessSpan("timespan.from-seconds.double", "static System.TimeSpan.FromSeconds(double)", [Number(-1.25)], TimeSpan.FromSeconds(-1.25)),
        SuccessSpan("timespan.from-ticks.maximum", "static System.TimeSpan.FromTicks(long)", [Big(long.MaxValue)], TimeSpan.MaxValue),

        SuccessSpan("timespan.negate.positive", "System.TimeSpan.Negate()", [Sample()], -SampleValue),
        Failure("timespan.negate.minimum-overflow", "System.TimeSpan.Negate()", [Span(TimeSpan.MinValue)], "OverflowException"),
        SuccessSpan("timespan.subtract.crosses-zero", "System.TimeSpan.Subtract(System.TimeSpan)", [Span(TimeSpan.FromHours(1)), Span(TimeSpan.FromHours(2))], TimeSpan.FromHours(-1)),
        Failure("timespan.subtract.overflow", "System.TimeSpan.Subtract(System.TimeSpan)", [Span(TimeSpan.MinValue), Span(TimeSpan.FromTicks(1))], "OverflowException"),
        SuccessSpan("timespan.multiply.positive", "System.TimeSpan.Multiply(double)", [Span(TimeSpan.FromTicks(3)), Number(0.5)], TimeSpan.FromTicks(2)),
        SuccessSpan("timespan.multiply.negative", "System.TimeSpan.Multiply(double)", [Span(TimeSpan.FromTicks(3)), Number(-0.5)], TimeSpan.FromTicks(-2)),
        SuccessSpan("timespan.multiply.half-even-down", "System.TimeSpan.Multiply(double)", [Span(TimeSpan.FromTicks(1)), Number(0.5)], TimeSpan.Zero),
        Failure("timespan.multiply.nan", "System.TimeSpan.Multiply(double)", [Sample(), Number(double.NaN)], "ArgumentException"),
        Failure("timespan.multiply.infinity", "System.TimeSpan.Multiply(double)", [Sample(), Number(double.PositiveInfinity)], "OverflowException"),
        SuccessSpan("timespan.divide.double-half-even", "System.TimeSpan.Divide(double)", [Span(TimeSpan.FromTicks(3)), Number(2)], TimeSpan.FromTicks(2)),
        SuccessSpan("timespan.divide.double-negative", "System.TimeSpan.Divide(double)", [Span(TimeSpan.FromTicks(3)), Number(-2)], TimeSpan.FromTicks(-2)),
        Failure("timespan.divide.double-zero", "System.TimeSpan.Divide(double)", [Sample(), Number(0)], "OverflowException"),
        Failure("timespan.divide.double-nan", "System.TimeSpan.Divide(double)", [Sample(), Number(double.NaN)], "ArgumentException"),
        Success("timespan.divide.timespan-ratio", "System.TimeSpan.Divide(System.TimeSpan)", [Span(TimeSpan.FromMinutes(90)), Span(TimeSpan.FromMinutes(60))], Number(1.5)),
        Success("timespan.divide.timespan-zero", "System.TimeSpan.Divide(System.TimeSpan)", [Span(TimeSpan.FromTicks(1)), Span(TimeSpan.Zero)], Number(double.PositiveInfinity)),

        SuccessSpan("timespan.parse.full-precision", "static System.TimeSpan.Parse(string)", [Text("2.03:04:05.0060079")], new TimeSpan(2, 3, 4, 5, 6, 7) + TimeSpan.FromTicks(9)),
        SuccessSpan("timespan.parse.negative", "static System.TimeSpan.Parse(string)", [Text("-01:02:03.0040056")], -(new TimeSpan(0, 1, 2, 3, 4, 5) + TimeSpan.FromTicks(6))),
        SuccessSpan("timespan.parse.whitespace", "static System.TimeSpan.Parse(string)", [Text("  01:02:03  ")], new TimeSpan(1, 2, 3)),
        SuccessSpan("timespan.parse.provider", "static System.TimeSpan.Parse(string, System.IFormatProvider)", [Text("1.02:03:04.0050000"), Null()], new TimeSpan(1, 2, 3, 4, 5)),
        SuccessSpan("timespan.parse.span-provider", "static System.TimeSpan.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", [Text("00:00:00.0000001"), Null()], TimeSpan.FromTicks(1)),
        Failure("timespan.parse.empty", "static System.TimeSpan.Parse(string)", [Text("   ")], "FormatException"),
        Failure("timespan.parse.missing-colon", "static System.TimeSpan.Parse(string)", [Text("123")], "FormatException"),
        Failure("timespan.parse.invalid-digit", "static System.TimeSpan.Parse(string)", [Text("01:ab:03")], "FormatException"),
        Failure("timespan.parse.day-hour-out-of-range", "static System.TimeSpan.Parse(string)", [Text("1.24:00:00")], "FormatException"),
        Failure("timespan.parse.minute-out-of-range", "static System.TimeSpan.Parse(string)", [Text("01:60:00")], "FormatException"),
        Failure("timespan.parse.fraction-too-long", "static System.TimeSpan.Parse(string)", [Text("00:00:00.12345678")], "FormatException"),
        Failure("timespan.parse.overflow", "static System.TimeSpan.Parse(string)", [Text("10675200.00:00:00")], "OverflowException"),
        Success("timespan.try-parse.valid", "static System.TimeSpan.TryParse(string, out System.TimeSpan)", [Text("1.02:03:04.0050067"), Span(TimeSpan.Zero)], Array(Bool(true), SpanText(new TimeSpan(1, 2, 3, 4, 5, 6) + TimeSpan.FromTicks(7)))),
        Success("timespan.try-parse.invalid", "static System.TimeSpan.TryParse(string, out System.TimeSpan)", [Text("not-a-timespan"), Sample()], Array(Bool(false), SpanText(TimeSpan.Zero))),
        Success("timespan.try-parse.null", "static System.TimeSpan.TryParse(string, out System.TimeSpan)", [Null(), Sample()], Array(Bool(false), SpanText(TimeSpan.Zero))),
        Success("timespan.try-parse.span", "static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, out System.TimeSpan)", [Text("00:00:01"), Span(TimeSpan.Zero)], Array(Bool(true), SpanText(TimeSpan.FromSeconds(1)))),
        Success("timespan.try-parse.provider", "static System.TimeSpan.TryParse(string, System.IFormatProvider, out System.TimeSpan)", [Text("00:01:00"), Null(), Span(TimeSpan.Zero)], Array(Bool(true), SpanText(TimeSpan.FromMinutes(1)))),
        Success("timespan.try-parse.span-provider", "static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)", [Text("01:00:00"), Null(), Span(TimeSpan.Zero)], Array(Bool(true), SpanText(TimeSpan.FromHours(1)))),

        Success("timespan.format.constant", "System.TimeSpan.ToString(string)", [Sample(), Text("c")], SpanText(SampleValue)),
        Success("timespan.format.general-short", "System.TimeSpan.ToString(string)", [Sample(), Text("g")], Text(SampleValue.ToString("g", CultureInfo.InvariantCulture))),
        Success("timespan.format.general-long-provider", "System.TimeSpan.ToString(string, System.IFormatProvider)", [Sample(), Text("G"), Null()], Text(SampleValue.ToString("G", CultureInfo.InvariantCulture))),
        Success("timespan.format.null-default", "System.TimeSpan.ToString(string, System.IFormatProvider)", [NegativeSample(), Null(), Null()], SpanText(NegativeSampleValue)),

        SuccessSpan("timespan.operator.unary-minus", "static System.TimeSpan.operator -(System.TimeSpan)", [Sample()], -SampleValue),
        Failure("timespan.operator.unary-minus-minimum", "static System.TimeSpan.operator -(System.TimeSpan)", [Span(TimeSpan.MinValue)], "OverflowException"),
        SuccessSpan("timespan.operator.subtract", "static System.TimeSpan.operator -(System.TimeSpan, System.TimeSpan)", [Span(TimeSpan.FromHours(1)), Span(TimeSpan.FromMinutes(90))], TimeSpan.FromMinutes(-30)),
        SuccessSpan("timespan.operator.unary-plus", "static System.TimeSpan.operator +(System.TimeSpan)", [NegativeSample()], NegativeSampleValue),
        SuccessSpan("timespan.operator.add", "static System.TimeSpan.operator +(System.TimeSpan, System.TimeSpan)", [Span(TimeSpan.FromHours(23)), Span(TimeSpan.FromHours(2))], TimeSpan.FromHours(25)),
        SuccessSpan("timespan.operator.multiply-right", "static System.TimeSpan.operator *(System.TimeSpan, double)", [Span(TimeSpan.FromTicks(5)), Number(0.5)], TimeSpan.FromTicks(2)),
        SuccessSpan("timespan.operator.multiply-left", "static System.TimeSpan.operator *(double, System.TimeSpan)", [Number(0.5), Span(TimeSpan.FromTicks(7))], TimeSpan.FromTicks(4)),
        SuccessSpan("timespan.operator.divide-double", "static System.TimeSpan.operator /(System.TimeSpan, double)", [Span(TimeSpan.FromTicks(7)), Number(2)], TimeSpan.FromTicks(4)),
        Success("timespan.operator.divide-timespan", "static System.TimeSpan.operator /(System.TimeSpan, System.TimeSpan)", [Span(TimeSpan.FromHours(2)), Span(TimeSpan.FromMinutes(30))], Number(4)),
        Success("timespan.operator.equal", "static System.TimeSpan.operator ==(System.TimeSpan, System.TimeSpan)", [Sample(), Sample()], Bool(true)),
        Success("timespan.operator.not-equal", "static System.TimeSpan.operator !=(System.TimeSpan, System.TimeSpan)", [Sample(), NegativeSample()], Bool(true)),
        Success("timespan.operator.less-than", "static System.TimeSpan.operator <(System.TimeSpan, System.TimeSpan)", [NegativeSample(), Sample()], Bool(true)),
        Success("timespan.operator.less-than-or-equal", "static System.TimeSpan.operator <=(System.TimeSpan, System.TimeSpan)", [Sample(), Sample()], Bool(true)),
        Success("timespan.operator.greater-than", "static System.TimeSpan.operator >(System.TimeSpan, System.TimeSpan)", [Sample(), NegativeSample()], Bool(true)),
        Success("timespan.operator.greater-than-or-equal", "static System.TimeSpan.operator >=(System.TimeSpan, System.TimeSpan)", [Sample(), Sample()], Bool(true))
    ];

    private static ClrRuntimeValue Sample() => Span(SampleValue);
    private static ClrRuntimeValue NegativeSample() => Span(NegativeSampleValue);
    private static ClrRuntimeValue Span(TimeSpan value) => Span(value.Ticks);
    private static ClrRuntimeValue Span(long ticks) => Invoke("System.TimeSpan.TimeSpan(long)", Big(ticks));
    private static ClrRuntimeValue SpanText(TimeSpan value) => Text(value.ToString("c", CultureInfo.InvariantCulture));

    private static ClrRuntimeScenario SuccessSpan(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        TimeSpan expected)
        => Success(id, member, arguments, SpanText(expected));

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
