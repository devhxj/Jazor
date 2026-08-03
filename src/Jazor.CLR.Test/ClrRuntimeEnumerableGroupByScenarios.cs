namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableGroupByScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string GroupingModulePath = "System/Linq/GroupingT2Module.js";
    private const string GroupByMember =
        "static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.group-by.preserves-first-key-and-source-order",
            GroupByMember,
            EnumerableModulePath,
            [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(
                Array(Number(1), Number(3)),
                Array(Number(2), Number(4)))),
        Success(
            "enumerable.group-by.element-selector-projects-within-each-group",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)",
            EnumerableModulePath,
            [
                Array(Number(1), Number(2), Number(3), Number(4)),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.DoubleNumber)
            ],
            Array(
                Array(Number(2), Number(6)),
                Array(Number(4), Number(8)))),
        Success(
            "enumerable.group-by.result-selector-projects-materialized-groups",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>)",
            EnumerableModulePath,
            [
                Array(Number(1), Number(2), Number(3), Number(4)),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.GroupKeyAndSum)
            ],
            Array(Number(4), Number(106))),
        Success(
            "enumerable.group-by.element-result-selector-projects-materialized-groups",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>)",
            EnumerableModulePath,
            [
                Array(Number(1), Number(2), Number(3), Number(4)),
                Callable(ClrRuntimeCallableKind.IsEven),
                Callable(ClrRuntimeCallableKind.DoubleNumber),
                Callable(ClrRuntimeCallableKind.GroupKeyAndSum)
            ],
            Array(Number(8), Number(112))),
        Success(
            "enumerable.group-by.grouping-key-reads-private-carrier-metadata",
            "System.Linq.IGrouping<TKey, TElement>.Key.get",
            GroupingModulePath,
            [
                ArrayElement(
                    Invoke(
                        GroupByMember,
                        Array(Number(1), Number(2), Number(3)),
                        Callable(ClrRuntimeCallableKind.IsEven)),
                    0)
            ],
            Boolean(false))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeValue Boolean(bool value) => ClrRuntimeValue.Boolean(value);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue ArrayElement(ClrRuntimeValue array, int index) => ClrRuntimeValue.ArrayElement(array, index);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments) => ClrRuntimeValue.Invoke(member, arguments);
}
