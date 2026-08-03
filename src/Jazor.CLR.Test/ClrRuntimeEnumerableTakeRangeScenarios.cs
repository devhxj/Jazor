namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableTakeRangeScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";
    private const string Member = "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Range)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.take-range.middle",
            [Array(Number(1), Number(2), Number(3), Number(4), Number(5)), Range(FromStart(1), FromEnd(1))],
            Array(Number(2), Number(3), Number(4))),
        Success(
            "enumerable.take-range.all",
            [Array(Number(1), Number(2), Number(3)), Invoke("static System.Range.All.get")],
            Array(Number(1), Number(2), Number(3))),
        Failure(
            "enumerable.take-range.rejects-inverted-range",
            [Array(Number(1), Number(2), Number(3)), Range(FromEnd(1), FromStart(1))],
            "ArgumentOutOfRangeException"),
        Failure(
            "enumerable.take-range.rejects-null-source",
            [Null(), Range(FromStart(0), FromEnd(0))],
            "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, Member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, Member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Range(ClrRuntimeValue start, ClrRuntimeValue end)
        => Invoke("System.Range.Range(System.Index, System.Index)", start, end);

    private static ClrRuntimeValue FromStart(double value)
        => Invoke("static System.Index.FromStart(int)", Number(value));

    private static ClrRuntimeValue FromEnd(double value)
        => Invoke("static System.Index.FromEnd(int)", Number(value));

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
}
