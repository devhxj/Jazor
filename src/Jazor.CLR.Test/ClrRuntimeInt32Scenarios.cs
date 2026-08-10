namespace Jazor.CLR.Test;

internal static class ClrRuntimeInt32Scenarios
{
    private const string ModulePath = "System/Int32Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("int32.compare.less-than", "int.CompareTo(object)", [Number(4), Number(9)], Number(-1)),
        Success("int32.compare.equal", "int.CompareTo(object)", [Number(9), Number(9)], Number(0)),
        Success("int32.compare.greater-than", "int.CompareTo(object)", [Number(12), Number(9)], Number(1)),
        Success("int32.compare.null-is-before-value", "int.CompareTo(object)", [Number(0), Null()], Number(1)),
        Failure("int32.compare.wrong-type", "int.CompareTo(object)", [Number(0), Bool(false)], "ArgumentException"),
        Success("int32.parse.trimmed-leading-plus", "static int.Parse(string)", [Text("  +42 ")], Number(42)),
        Success("int32.parse.minimum", "static int.Parse(string)", [Text("-2147483648")], Number(-2147483648)),
        Success("int32.parse.maximum", "static int.Parse(string)", [Text("2147483647")], Number(2147483647)),
        Success("int32.parse.negative-zero", "static int.Parse(string)", [Text("-0")], Number(0)),
        Failure("int32.parse.null", "static int.Parse(string)", [Null()], "ArgumentNullException"),
        Failure("int32.parse.trailing-text", "static int.Parse(string)", [Text("12px")], "FormatException"),
        Failure("int32.parse.positive-overflow", "static int.Parse(string)", [Text("2147483648")], "OverflowException"),
        Failure("int32.parse.negative-overflow", "static int.Parse(string)", [Text("-2147483649")], "OverflowException"),
        Success("int32.try-parse.valid-negative", "static int.TryParse(string, out int)", [Text(" -17 "), Number(99)], Array(Bool(true), Number(-17))),
        Success("int32.try-parse.null", "static int.TryParse(string, out int)", [Null(), Number(99)], Array(Bool(false), Number(0))),
        Success("int32.try-parse.trailing-text", "static int.TryParse(string, out int)", [Text("17x"), Number(99)], Array(Bool(false), Number(0))),
        Success("int32.try-parse.overflow", "static int.TryParse(string, out int)", [Text("2147483648"), Number(99)], Array(Bool(false), Number(0))),
        Success(
            "int32.div-rem.positive-operands",
            "static int.DivRem(int, int)",
            [Number(17), Number(5)],
            Record(("Quotient", Number(3)), ("Remainder", Number(2)))),
        Success(
            "int32.div-rem.negative-dividend-truncates-to-zero",
            "static int.DivRem(int, int)",
            [Number(-17), Number(5)],
            Record(("Quotient", Number(-3)), ("Remainder", Number(-2)))),
        Failure("int32.div-rem.zero-divisor", "static int.DivRem(int, int)", [Number(17), Number(0)], "DivideByZeroException"),
        Failure("int32.div-rem.minimum-overflow", "static int.DivRem(int, int)", [Number(-2147483648), Number(-1)], "OverflowException"),
        Success("int32.pop-count.zero", "static int.PopCount(int)", [Number(0)], Number(0)),
        Success("int32.pop-count.all-bits-set", "static int.PopCount(int)", [Number(-1)], Number(32)),
        Success("int32.max-magnitude.equal-magnitude-prefers-positive", "static int.MaxMagnitude(int, int)", [Number(-7), Number(7)], Number(7)),
        Success("int32.min-magnitude.equal-magnitude-prefers-negative", "static int.MinMagnitude(int, int)", [Number(-7), Number(7)], Number(-7))
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
