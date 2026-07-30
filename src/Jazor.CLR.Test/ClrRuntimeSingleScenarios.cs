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

        Success("single.sin-cos.zero", "static float.SinCos(float)", [Number(0)], Record(("sin", Number(0)), ("cos", Number(1)))),
        Success("single.sin-cos-pi.zero", "static float.SinCosPi(float)", [Number(0)], Record(("sinPi", Number(0)), ("cosPi", Number(1))))
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
