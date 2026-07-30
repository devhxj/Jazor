namespace Jazor.CLR.Test;

internal static class ClrRuntimeMathScenarios
{
    private const string ModulePath = "System/MathModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "math.sin-cos.zero",
            "static System.Math.SinCos(double)",
            [Number(0)],
            Record(("sin", Number(0)), ("cos", Number(1)))),
        Success("math.abs.decimal", "static System.Math.Abs(decimal)", [Text("-123.4500")], Text("123.4500")),
        Success("math.ceiling.decimal-negative", "static System.Math.Ceiling(decimal)", [Text("-1.2")], Text("-1")),
        Success("math.clamp.decimal-above-maximum", "static System.Math.Clamp(decimal, decimal, decimal)", [Text("100.5"), Text("0"), Text("10")], Text("10")),
        Success("math.floor.decimal-negative", "static System.Math.Floor(decimal)", [Text("-1.2")], Text("-2")),
        Success("math.max.decimal-preserves-selected-scale", "static System.Math.Max(decimal, decimal)", [Text("1.20"), Text("1.2")], Text("1.20")),
        Success("math.min.decimal", "static System.Math.Min(decimal, decimal)", [Text("-5.5"), Text("2")], Text("-5.5")),
        Success("math.max-magnitude.equal-prefers-positive", "static System.Math.MaxMagnitude(double, double)", [Number(-7), Number(7)], Number(7)),
        Success("math.max-magnitude.nan-propagates", "static System.Math.MaxMagnitude(double, double)", [Number(double.NaN), Number(7)], Number(double.NaN)),
        Success("math.min-magnitude.equal-prefers-negative", "static System.Math.MinMagnitude(double, double)", [Number(-7), Number(7)], Number(-7)),
        Success("math.round.decimal-midpoint-to-even", "static System.Math.Round(decimal)", [Text("2.5")], Text("2")),
        Success("math.round.decimal-with-scale", "static System.Math.Round(decimal, int)", [Text("1.235"), Number(2)], Text("1.24")),
        Success("math.round.decimal-away-from-zero", "static System.Math.Round(decimal, System.MidpointRounding)", [Text("-2.5"), Number(1)], Text("-3")),
        Success("math.round.decimal-scale-toward-zero", "static System.Math.Round(decimal, int, System.MidpointRounding)", [Text("1.239"), Number(2), Number(2)], Text("1.23")),
        Success("math.sign.decimal-negative", "static System.Math.Sign(decimal)", [Text("-0.001")], Number(-1)),
        Success("math.sign.double-positive-infinity", "static System.Math.Sign(double)", [Number(double.PositiveInfinity)], Number(1)),
        Failure("math.sign.double-nan", "static System.Math.Sign(double)", [Number(double.NaN)], "ArithmeticException"),
        Success("math.sign.float-negative", "static System.Math.Sign(float)", [Number(-1.25)], Number(-1)),
        Failure("math.sign.float-nan", "static System.Math.Sign(float)", [Number(double.NaN)], "ArithmeticException"),
        Success("math.truncate.decimal-negative", "static System.Math.Truncate(decimal)", [Text("-12.99")], Text("-12"))
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

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
