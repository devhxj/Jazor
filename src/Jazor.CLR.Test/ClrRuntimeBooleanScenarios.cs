namespace Jazor.CLR.Test;

internal static class ClrRuntimeBooleanScenarios
{
    private const string ModulePath = "System/BooleanModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("boolean.compare.false-before-true", "bool.CompareTo(object)", [Bool(false), Bool(true)], Number(-1)),
        Success("boolean.compare.true-after-false", "bool.CompareTo(object)", [Bool(true), Bool(false)], Number(1)),
        Success("boolean.compare.equal-values", "bool.CompareTo(object)", [Bool(false), Bool(false)], Number(0)),
        Success("boolean.compare.null-is-before-value", "bool.CompareTo(object)", [Bool(true), Null()], Number(1)),
        Failure("boolean.compare.wrong-type", "bool.CompareTo(object)", [Bool(true), Text("true")], "ArgumentException"),
        Success("boolean.parse.trimmed-mixed-case-true", "static bool.Parse(string)", [Text("  TrUe\t")], Bool(true)),
        Success("boolean.parse.false", "static bool.Parse(string)", [Text("false")], Bool(false)),
        Failure("boolean.parse.null", "static bool.Parse(string)", [Null()], "ArgumentNullException"),
        Failure("boolean.parse.invalid-token", "static bool.Parse(string)", [Text("yes")], "FormatException"),
        Success("boolean.parse.span-true", "static bool.Parse(System.ReadOnlySpan<char>)", [Text("TRUE")], Bool(true)),
        Success("boolean.try-parse.true", "static bool.TryParse(string, out bool)", [Text(" true "), Bool(false)], Array(Bool(true), Bool(true))),
        Success("boolean.try-parse.false", "static bool.TryParse(string, out bool)", [Text("FALSE"), Bool(true)], Array(Bool(true), Bool(false))),
        Success("boolean.try-parse.invalid-token", "static bool.TryParse(string, out bool)", [Text("1"), Bool(true)], Array(Bool(false), Bool(false))),
        Success("boolean.try-parse.null", "static bool.TryParse(string, out bool)", [Null(), Bool(true)], Array(Bool(false), Bool(false)))
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
}
