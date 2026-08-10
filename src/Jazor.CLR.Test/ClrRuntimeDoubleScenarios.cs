namespace Jazor.CLR.Test;

internal static class ClrRuntimeDoubleScenarios
{
    private const string ModulePath = "System/DoubleModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("double.compare.null-is-before-value", "double.CompareTo(object)", [Number(1), Null()], Number(1)),
        Success("double.compare.nan-before-number", "double.CompareTo(object)", [Number(double.NaN), Number(1)], Number(-1)),
        Success("double.compare.nan-equals-nan", "double.CompareTo(object)", [Number(double.NaN), Number(double.NaN)], Number(0)),
        Failure("double.compare.wrong-type", "double.CompareTo(object)", [Number(1), Text("1")], "ArgumentException"),
        Success("double.equals.nan-values", "override double.Equals(object)", [Number(double.NaN), Number(double.NaN)], Bool(true)),
        Success("double.equals.wrong-type", "override double.Equals(object)", [Number(1), Text("1")], Bool(false)),
        Success("double.parse.decimal-exponent", "static double.Parse(string)", [Text(" -1.25e2 ")], Number(-125)),
        Success("double.parse.positive-infinity", "static double.Parse(string)", [Text("Infinity")], Number(double.PositiveInfinity)),
        Success("double.parse.nan-token", "static double.Parse(string)", [Text("NaN")], Number(double.NaN)),
        Failure("double.parse.javascript-hex-is-invalid", "static double.Parse(string)", [Text("0x10")], "FormatException"),
        Failure("double.parse.null", "static double.Parse(string)", [Null()], "ArgumentNullException"),
        Failure("double.parse.whitespace", "static double.Parse(string)", [Text("  ")], "FormatException"),
        Success(
            "double.try-parse.nan-token",
            "static double.TryParse(string, out double)",
            [Text("NaN"), Number(7)],
            Array(Bool(true), Number(double.NaN))),
        Success(
            "double.try-parse.invalid-text",
            "static double.TryParse(string, out double)",
            [Text("twelve"), Number(7)],
            Array(Bool(false), Number(0))),
        Success(
            "double.try-parse.null",
            "static double.TryParse(string, out double)",
            [Null(), Number(7)],
            Array(Bool(false), Number(0))),
        Success(
            "double.try-parse.span",
            "static double.TryParse(System.ReadOnlySpan<char>, out double)",
            [Text("1.25e2"), Number(7)],
            Array(Bool(true), Number(125))),
        Success(
            "double.try-parse.provider-overload",
            "static double.TryParse(string, System.IFormatProvider, out double)",
            [Text("2.5"), Null(), Number(0)],
            Array(Bool(true), Number(2.5))),
        Success("double.is-pow2.fractional-power", "static double.IsPow2(double)", [Number(0.5)], Bool(true)),
        Success("double.is-pow2.non-power", "static double.IsPow2(double)", [Number(12)], Bool(false)),
        Success("double.is-pow2.infinity", "static double.IsPow2(double)", [Number(double.PositiveInfinity)], Bool(false)),
        Success("double.sign.negative", "static double.Sign(double)", [Number(-3.5)], Number(-1)),
        Success("double.sign.negative-zero", "static double.Sign(double)", [Number(-0.0)], Number(0)),
        Failure("double.sign.nan", "static double.Sign(double)", [Number(double.NaN)], "ArithmeticException"),
        Success("double.max-magnitude.larger-absolute-value", "static double.MaxMagnitude(double, double)", [Number(-9), Number(4)], Number(-9)),
        Success("double.max-magnitude.equal-prefers-positive", "static double.MaxMagnitude(double, double)", [Number(-7), Number(7)], Number(7)),
        Success("double.max-magnitude.nan-propagates", "static double.MaxMagnitude(double, double)", [Number(double.NaN), Number(7)], Number(double.NaN)),
        Success("double.max-magnitude-number.skips-nan", "static double.MaxMagnitudeNumber(double, double)", [Number(double.NaN), Number(7)], Number(7)),
        Success("double.min-magnitude.smaller-absolute-value", "static double.MinMagnitude(double, double)", [Number(-9), Number(4)], Number(4)),
        Success("double.min-magnitude.equal-prefers-negative", "static double.MinMagnitude(double, double)", [Number(-7), Number(7)], Number(-7)),
        Success("double.min-magnitude.negative-zero", "static double.MinMagnitude(double, double)", [Number(-0.0), Number(0.0)], Number(-0.0)),
        Success("double.min-magnitude-number.skips-nan", "static double.MinMagnitudeNumber(double, double)", [Number(4), Number(double.NaN)], Number(4)),
        Success("double.clamp-native.nan-value-selects-min", "static double.ClampNative(double, double, double)", [Number(double.NaN), Number(0), Number(1)], Number(0)),
        Success("double.clamp-native.nan-min-selects-max", "static double.ClampNative(double, double, double)", [Number(0.5), Number(double.NaN), Number(1)], Number(1)),
        Success("double.clamp-native.signed-zero-follows-right-operand", "static double.ClampNative(double, double, double)", [Number(-0.0), Number(0.0), Number(1)], Number(0.0)),
        Failure("double.clamp-native.rejects-inverted-range", "static double.ClampNative(double, double, double)", [Number(1), Number(2), Number(0)], "ArgumentException"),
        Success(
            "double.sin-cos.zero",
            "static double.SinCos(double)",
            [Number(0)],
            Record(("Sin", Number(0)), ("Cos", Number(1)))),
        Success(
            "double.sin-cos-pi.zero",
            "static double.SinCosPi(double)",
            [Number(0)],
            Record(("SinPi", Number(0)), ("CosPi", Number(1)))),
        Success("double.round.positive-even-midpoint", "static double.Round(double)", [Number(2.5)], Number(2)),
        Success("double.round.negative-even-midpoint", "static double.Round(double)", [Number(-2.5)], Number(-2)),
        Success("double.round.decimal-digits", "static double.Round(double, int)", [Number(2.675), Number(2)], Number(double.Round(2.675, 2))),
        Success("double.round.away-from-zero", "static double.Round(double, System.MidpointRounding)", [Number(-2.5), Number(1)], Number(-3)),
        Success("double.round.directed-with-digits", "static double.Round(double, int, System.MidpointRounding)", [Number(-1.234), Number(2), Number(3)], Number(-1.24)),
        Failure("double.round.rejects-digits", "static double.Round(double, int)", [Number(1), Number(16)], "ArgumentOutOfRangeException"),
        Failure("double.round.rejects-mode", "static double.Round(double, System.MidpointRounding)", [Number(1), Number(5)], "ArgumentException"),
        Success("double.bit-increment.zero", "static double.BitIncrement(double)", [Number(0)], Number(double.Epsilon)),
        Success("double.bit-increment.one", "static double.BitIncrement(double)", [Number(1)], Number(double.BitIncrement(1))),
        Success("double.bit-increment.negative-infinity", "static double.BitIncrement(double)", [Number(double.NegativeInfinity)], Number(-double.MaxValue)),
        Success("double.bit-decrement.negative-zero", "static double.BitDecrement(double)", [Number(-0.0)], Number(-double.Epsilon)),
        Success("double.bit-decrement.one", "static double.BitDecrement(double)", [Number(1)], Number(double.BitDecrement(1))),
        Success("double.bit-decrement.positive-infinity", "static double.BitDecrement(double)", [Number(double.PositiveInfinity)], Number(double.MaxValue)),
        Success("double.ieee-remainder.lower-even-quotient", "static double.Ieee754Remainder(double, double)", [Number(5), Number(2)], Number(1)),
        Success("double.ieee-remainder.upper-even-quotient", "static double.Ieee754Remainder(double, double)", [Number(7), Number(2)], Number(-1)),
        Success("double.ieee-remainder.negative-zero", "static double.Ieee754Remainder(double, double)", [Number(-4), Number(2)], Number(-0.0)),
        Success("double.ilogb.maximum", "static double.ILogB(double)", [Number(double.MaxValue)], Number(1023)),
        Success("double.ilogb.minimum-subnormal", "static double.ILogB(double)", [Number(double.Epsilon)], Number(-1074)),
        Success("double.ilogb.zero-sentinel", "static double.ILogB(double)", [Number(0)], Number(int.MinValue)),
        Success("double.ilogb.infinity-sentinel", "static double.ILogB(double)", [Number(double.PositiveInfinity)], Number(int.MaxValue))
    ];

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

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
