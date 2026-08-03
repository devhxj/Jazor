namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableCountScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.count.enumerates-all-source-items",
            "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
            [Array(Number(1), Number(2), Number(3))],
            Number(3)),
        Failure(
            "enumerable.count.rejects-null-source",
            "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
            [Null()],
            "ArgumentNullException"),
        Success(
            "enumerable.count.predicate-counts-only-matches",
            "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(2)),
        Failure(
            "enumerable.count.predicate-rejects-null",
            "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)",
            [Array(Number(1)), Null()],
            "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string expectedError)
        => new(id, member, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
