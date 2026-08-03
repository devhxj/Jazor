namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableSetByScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string UnionByMember = "static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";
    private const string ExceptByMember = "static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)";
    private const string IntersectByMember = "static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.union-by.preserves-first-key-representatives",
            UnionByMember,
            [Array(Number(1), Number(3), Number(2)), Array(Number(4), Number(5)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(Number(1), Number(2))),
        Success(
            "enumerable.except-by.filters-existing-keys-and-deduplicates-source",
            ExceptByMember,
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Boolean(false)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(Number(2))),
        Success(
            "enumerable.intersect-by.preserves-first-order-per-key",
            IntersectByMember,
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Boolean(true)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(Number(2))),
        Failure(
            "enumerable.union-by.rejects-null-first",
            UnionByMember,
            [Null(), Array(Number(1)), Callable(ClrRuntimeCallableKind.IsEven)],
            "ArgumentNullException: first is null"),
        Failure(
            "enumerable.except-by.rejects-null-key-sequence",
            ExceptByMember,
            [Array(Number(1)), Null(), Callable(ClrRuntimeCallableKind.IsEven)],
            "ArgumentNullException: second is null"),
        Failure(
            "enumerable.intersect-by.rejects-null-key-selector",
            IntersectByMember,
            [Array(Number(1)), Array(Number(1)), Null()],
            "ArgumentNullException: keySelector is null")
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

    private static ClrRuntimeValue Boolean(bool value) => ClrRuntimeValue.Boolean(value);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
