namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableAggregateByScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.count-by.comparer-preserves-first-representative-and-order",
            "static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4), Number(5)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Array(Number(1), Number(3)), Array(Number(2), Number(2)))),
        Failure(
            "enumerable.count-by.rejects-null-key-selector",
            "static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1)), Null(), Null()],
            "ArgumentNullException: keySelector is null"),
        Success(
            "enumerable.aggregate-by.fixed-seed.comparer-preserves-first-representative-and-order",
            "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers), ParityEquality()],
            Array(Array(Number(1), Number(14)), Array(Number(2), Number(16)))),
        Failure(
            "enumerable.aggregate-by.fixed-seed.rejects-null-accumulator",
            "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1)), Callable(ClrRuntimeCallableKind.Identity), Number(0), Null(), Null()],
            "ArgumentNullException: func is null"),
        Success(
            "enumerable.aggregate-by.key-seed.comparer-initializes-per-first-key",
            "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.DoubleNumber), Callable(ClrRuntimeCallableKind.AddNumbers), ParityEquality()],
            Array(Array(Number(1), Number(6)), Array(Number(2), Number(10)))),
        Failure(
            "enumerable.aggregate-by.key-seed.rejects-null-seed-selector",
            "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1)), Callable(ClrRuntimeCallableKind.Identity), Null(), Callable(ClrRuntimeCallableKind.AddNumbers), Null()],
            "ArgumentNullException: seedSelector is null")
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

    private static ClrRuntimeValue ParityEquality() => Record(
        ("equals", Callable(ClrRuntimeCallableKind.SameParity)),
        ("getHashCode", Callable(ClrRuntimeCallableKind.ParityHash)));

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties) => ClrRuntimeValue.Record(properties);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
