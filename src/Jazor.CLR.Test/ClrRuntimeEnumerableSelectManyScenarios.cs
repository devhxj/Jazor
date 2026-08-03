namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableSelectManyScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.select-many.collection-selector.preserves-outer-inner-order",
            "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TResult>>)",
            [Array(Number(2), Number(3)), Callable(ClrRuntimeCallableKind.ExpandNumber)],
            Array(Number(2), Number(20), Number(3), Number(30))),
        Success(
            "enumerable.select-many.indexed-collection-selector.uses-source-offset",
            "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, System.Collections.Generic.IEnumerable<TResult>>)",
            [Array(Number(10), Number(20)), Callable(ClrRuntimeCallableKind.ExpandWithIndex)],
            Array(Number(10), Number(21))),
        Success(
            "enumerable.select-many.result-selector.combines-source-and-inner-values",
            "static System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TCollection>>, System.Func<TSource, TCollection, TResult>)",
            [
                Array(Number(1), Number(2)),
                Callable(ClrRuntimeCallableKind.ExpandNumber),
                Callable(ClrRuntimeCallableKind.CombineOuterInner)
            ],
            Array(Number(101), Number(110), Number(202), Number(220)))
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
