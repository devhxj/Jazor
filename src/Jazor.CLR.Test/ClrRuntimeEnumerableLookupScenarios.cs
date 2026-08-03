namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableLookupScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";
    private const string ToLookupMember = "static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.to-lookup.groups-preserve-first-key-and-source-order",
            ToLookupMember,
            [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(Array(Number(1), Number(3)), Array(Number(2), Number(4)))),
        Success(
            "enumerable.to-lookup.element-selector-projects-group-members",
            "static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven), Callable(ClrRuntimeCallableKind.DoubleNumber)],
            Array(Array(Number(2), Number(6)), Array(Number(4), Number(8)))),
        Success(
            "lookup.count-uses-group-count",
            "System.Linq.ILookup<TKey, TElement>.Count.get",
            [Invoke(ToLookupMember, Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven))],
            Number(2)),
        Success(
            "lookup.contains-uses-clr-key-equality",
            "System.Linq.ILookup<TKey, TElement>.Contains(TKey)",
            [Invoke(ToLookupMember, Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)), Bool(false)],
            Bool(true)),
        Success(
            "lookup.indexer-returns-existing-group",
            "System.Linq.ILookup<TKey, TElement>.this[TKey].get",
            [Invoke(ToLookupMember, Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)), Bool(false)],
            Array(Number(1), Number(3))),
        Success(
            "lookup.indexer-returns-empty-array-for-missing-key",
            "System.Linq.ILookup<TKey, TElement>.this[TKey].get",
            [Invoke(ToLookupMember, Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.DoubleNumber)), Number(8)],
            Array()),
        Failure(
            "enumerable.to-lookup.rejects-null-source",
            ToLookupMember,
            [Null(), Callable(ClrRuntimeCallableKind.IsEven)],
            "ArgumentNullException"),
        Failure(
            "enumerable.to-lookup.rejects-null-key-selector",
            ToLookupMember,
            [Array(Number(1)), Null()],
            "ArgumentNullException"),
        Failure(
            "lookup.count-rejects-null-instance",
            "System.Linq.ILookup<TKey, TElement>.Count.get",
            [Null()],
            "NullReferenceException")
    ];

    private static ClrRuntimeScenario Success(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
