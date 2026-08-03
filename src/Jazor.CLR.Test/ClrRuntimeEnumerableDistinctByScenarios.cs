namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableDistinctByScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string DistinctByMember = "static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.distinct-by.preserves-first-source-item-per-key",
            [Array(Number(2), Number(7), Number(4), Number(9)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(Number(2), Number(7))),
        Success(
            "enumerable.distinct-by.accepts-empty-source",
            [Array(), Callable(ClrRuntimeCallableKind.IsEven)],
            Array()),
        Failure(
            "enumerable.distinct-by.rejects-null-source",
            [Null(), Callable(ClrRuntimeCallableKind.IsEven)],
            "ArgumentNullException: source is null"),
        Failure(
            "enumerable.distinct-by.rejects-null-key-selector",
            [Array(Number(2)), Null()],
            "ArgumentNullException: keySelector is null")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, DistinctByMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, DistinctByMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
