namespace Jazor.CLR.Test;

internal static class ClrRuntimeSingleScenarios
{
    private const string ModulePath = "System/SingleModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("single.compare.null-is-before-value", "float.CompareTo(object)", [Number(1), Null()], Number(1)),
        Success("single.compare.nan-before-number", "float.CompareTo(object)", [Number(double.NaN), Number(1)], Number(-1)),
        Failure("single.compare.wrong-type", "float.CompareTo(object)", [Number(1), Text("1")], "ArgumentException"),
        Success("single.equals.nan-values", "override float.Equals(object)", [Number(double.NaN), Number(double.NaN)], Bool(true)),
        Success("single.equals.wrong-type", "override float.Equals(object)", [Number(1), Text("1")], Bool(false)),

        Success("single.parse.decimal-exponent", "static float.Parse(string)", [Text(" -1.25e2 ")], Number(-125)),
        Failure("single.parse.javascript-hex-is-invalid", "static float.Parse(string)", [Text("0x10")], "FormatException"),
        Failure("single.parse.null", "static float.Parse(string)", [Null()], "ArgumentNullException"),
        Success("single.try-parse.nan-token", "static float.TryParse(string, out float)", [Text("NaN"), Number(7)], Array(Bool(true), Number(double.NaN))),
        Success("single.try-parse.invalid-text", "static float.TryParse(string, out float)", [Text("twelve"), Number(7)], Array(Bool(false), Number(0))),
        Success("single.try-parse.span", "static float.TryParse(System.ReadOnlySpan<char>, out float)", [Text("-1.25"), Number(7)], Array(Bool(true), Number(-1.25))),

        Success("single.is-pow2.fractional-power", "static float.IsPow2(float)", [Number(0.5)], Bool(true)),
        Success("single.is-pow2.infinity", "static float.IsPow2(float)", [Number(double.PositiveInfinity)], Bool(false)),
        Success("single.sign.negative", "static float.Sign(float)", [Number(-3.5)], Number(-1)),
        Success("single.sign.negative-zero", "static float.Sign(float)", [Number(-0.0)], Number(0)),
        Failure("single.sign.nan", "static float.Sign(float)", [Number(double.NaN)], "ArithmeticException"),

        Success("single.max-magnitude.equal-prefers-positive", "static float.MaxMagnitude(float, float)", [Number(-7), Number(7)], Number(7)),
        Success("single.max-magnitude.nan-propagates", "static float.MaxMagnitude(float, float)", [Number(double.NaN), Number(7)], Number(double.NaN)),
        Success("single.max-magnitude-number.skips-nan", "static float.MaxMagnitudeNumber(float, float)", [Number(double.NaN), Number(7)], Number(7)),
        Success("single.min-magnitude.equal-prefers-negative", "static float.MinMagnitude(float, float)", [Number(-7), Number(7)], Number(-7)),
        Success("single.min-magnitude.negative-zero", "static float.MinMagnitude(float, float)", [Number(-0.0), Number(0.0)], Number(-0.0)),
        Success("single.min-magnitude-number.skips-nan", "static float.MinMagnitudeNumber(float, float)", [Number(4), Number(double.NaN)], Number(4)),
        Success("single.clamp-native.nan-value-selects-min", "static float.ClampNative(float, float, float)", [Number(double.NaN), Number(0), Number(1)], Number(0)),
        Success("single.clamp-native.nan-min-selects-max", "static float.ClampNative(float, float, float)", [Number(0.5), Number(double.NaN), Number(1)], Number(1)),
        Failure("single.clamp-native.rejects-inverted-range", "static float.ClampNative(float, float, float)", [Number(1), Number(2), Number(0)], "ArgumentException"),

        Success("single.sin-cos.zero", "static float.SinCos(float)", [Number(0)], Record(("Sin", Number(0)), ("Cos", Number(1)))),
        Success("single.sin-cos-pi.zero", "static float.SinCosPi(float)", [Number(0)], Record(("SinPi", Number(0)), ("CosPi", Number(1)))),
        Success("single.round.positive-even-midpoint", "static float.Round(float)", [Number(2.5)], Number(2)),
        Success("single.round.negative-even-midpoint", "static float.Round(float)", [Number(-2.5)], Number(-2)),
        Success("single.round.decimal-digits", "static float.Round(float, int)", [Number(2.675f), Number(2)], Number(float.Round(2.675f, 2))),
        Success("single.round.away-from-zero", "static float.Round(float, System.MidpointRounding)", [Number(-2.5), Number(1)], Number(-3)),
        Success("single.round.directed-with-digits", "static float.Round(float, int, System.MidpointRounding)", [Number(-1.234f), Number(2), Number(3)], Number(float.Round(-1.234f, 2, MidpointRounding.ToNegativeInfinity))),
        Failure("single.round.rejects-digits", "static float.Round(float, int)", [Number(1), Number(7)], "ArgumentOutOfRangeException"),
        Failure("single.round.rejects-mode", "static float.Round(float, System.MidpointRounding)", [Number(1), Number(5)], "ArgumentException"),
        Success("single.bit-increment.zero", "static float.BitIncrement(float)", [Number(0)], Number(float.Epsilon)),
        Success("single.bit-increment.one", "static float.BitIncrement(float)", [Number(1)], Number(float.BitIncrement(1))),
        Success("single.bit-increment.negative-infinity", "static float.BitIncrement(float)", [Number(float.NegativeInfinity)], Number(-float.MaxValue)),
        Success("single.bit-decrement.negative-zero", "static float.BitDecrement(float)", [Number(-0.0)], Number(-float.Epsilon)),
        Success("single.bit-decrement.one", "static float.BitDecrement(float)", [Number(1)], Number(float.BitDecrement(1))),
        Success("single.bit-decrement.positive-infinity", "static float.BitDecrement(float)", [Number(float.PositiveInfinity)], Number(float.MaxValue)),
        Success("single.ieee-remainder.lower-even-quotient", "static float.Ieee754Remainder(float, float)", [Number(5), Number(2)], Number(1)),
        Success("single.ieee-remainder.upper-even-quotient", "static float.Ieee754Remainder(float, float)", [Number(7), Number(2)], Number(-1)),
        Success("single.ieee-remainder.negative-zero", "static float.Ieee754Remainder(float, float)", [Number(-4), Number(2)], Number(-0.0)),
        Success("single.ilogb.maximum", "static float.ILogB(float)", [Number(float.MaxValue)], Number(127)),
        Success("single.ilogb.minimum-subnormal", "static float.ILogB(float)", [Number(float.Epsilon)], Number(-149)),
        Success("single.ilogb.zero-sentinel", "static float.ILogB(float)", [Number(0)], Number(int.MinValue)),
        Success("single.ilogb.infinity-sentinel", "static float.ILogB(float)", [Number(float.PositiveInfinity)], Number(int.MaxValue))
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
