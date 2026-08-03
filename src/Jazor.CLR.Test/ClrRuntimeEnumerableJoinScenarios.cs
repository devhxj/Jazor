namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableJoinScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.join.pairs-outer-order-with-inner-group-order",
            "static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>)",
            [
                Array(Number(1), Number(2), Number(3)),
                Array(Number(10), Number(11), Number(12)),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.CombineOuterInner)
            ],
            Array(Number(111), Number(210), Number(212), Number(311))),
        Success(
            "enumerable.group-join-invokes-result-for-every-outer-item",
            "static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>)",
            [
                Array(Number(1), Number(2), Number(3)),
                Array(Number(10), Number(11), Number(12)),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.CombineOuterGroupCount)
            ],
            Array(Number(11), Number(22), Number(31)))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
