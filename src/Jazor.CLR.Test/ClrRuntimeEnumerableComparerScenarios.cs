namespace Jazor.CLR.Test;

/// <summary>
/// Comparer overloads must exercise the comparer protocol rather than merely return the same
/// result as JavaScript native equality or ordering. The parity comparer creates two equivalence
/// classes with deliberate hash collisions inside each class.
/// </summary>
internal static class ClrRuntimeEnumerableComparerScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string IdentityMember = "static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.contains.comparer-uses-equivalence-class",
            "static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(2)), Number(3), ParityEquality()],
            Bool(true)),
        Success(
            "enumerable.distinct.comparer-preserves-first-representative",
            "static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), ParityEquality()],
            Array(Number(1), Number(2))),
        Success(
            "enumerable.distinct-by.comparer-deduplicates-selected-keys",
            "static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Number(1), Number(2))),
        Success(
            "enumerable.except.comparer-observes-equivalence-class",
            "static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Number(3)), ParityEquality()],
            Array(Number(2))),
        Success(
            "enumerable.except-by.comparer-observes-selected-key-class",
            "static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Number(3)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Number(2))),
        Success(
            "enumerable.intersect.comparer-preserves-first-representative",
            "static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Number(3)), ParityEquality()],
            Array(Number(1))),
        Success(
            "enumerable.intersect-by.comparer-preserves-first-key-representative",
            "static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Number(3)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Number(1))),
        Success(
            "enumerable.union.comparer-preserves-first-class-representatives",
            "static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(2)), Array(Number(3), Number(4)), ParityEquality()],
            Array(Number(1), Number(2))),
        Success(
            "enumerable.union-by.comparer-preserves-first-key-representatives",
            "static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2)), Array(Number(3), Number(4)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Number(1), Number(2))),
        Success(
            "enumerable.sequence-equal.comparer-matches-corresponding-classes",
            "static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(2)), Array(Number(3), Number(4)), ParityEquality()],
            Bool(true)),

        Success(
            "enumerable.group-by.comparer-materializes-equivalence-groups",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Array(Number(1), Number(3)), Array(Number(2), Number(4)))),
        Success(
            "enumerable.group-by-element.comparer-groups-projected-elements",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.DoubleNumber), ParityEquality()],
            Array(Array(Number(2), Number(6)), Array(Number(4), Number(8)))),
        Success(
            "enumerable.group-by-result.comparer-observes-materialized-groups",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.CombineOuterGroupCount), ParityEquality()],
            Array(Number(12), Number(22))),
        Success(
            "enumerable.group-by-element-result.comparer-observes-projected-groups",
            "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.DoubleNumber), Callable(ClrRuntimeCallableKind.CombineOuterGroupCount), ParityEquality()],
            Array(Number(12), Number(22))),
        Success(
            "enumerable.to-lookup.comparer-persists-key-equality",
            "static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Array(Array(Number(1), Number(3)), Array(Number(2), Number(4)))),
        Success(
            "enumerable.to-lookup-element.comparer-persists-projected-groups",
            "static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.DoubleNumber), ParityEquality()],
            Array(Array(Number(2), Number(6)), Array(Number(4), Number(8)))),
        Success(
            "enumerable.join.comparer-finds-inner-equivalence-groups",
            "static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2), Number(3)), Array(Number(11), Number(12), Number(13)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.CombineOuterInner), ParityEquality()],
            Array(Number(111), Number(113), Number(212), Number(311), Number(313))),
        Success(
            "enumerable.group-join.comparer-materializes-each-matching-group",
            "static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2), Number(3)), Array(Number(11), Number(12), Number(13)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.CombineOuterGroupCount), ParityEquality()],
            Array(Number(12), Number(21), Number(32))),

        Success(
            "enumerable.min-by.comparer-selects-custom-minimum",
            "static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2)), Callable(ClrRuntimeCallableKind.Identity), DescendingComparer()],
            Number(3)),
        Success(
            "enumerable.max-by.comparer-selects-custom-maximum",
            "static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2)), Callable(ClrRuntimeCallableKind.Identity), DescendingComparer()],
            Number(1)),
        Success(
            "enumerable.order.comparer-uses-custom-direction",
            "static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IComparer<T>)",
            [Array(Number(1), Number(3), Number(2)), DescendingComparer()],
            Array(Number(3), Number(2), Number(1))),
        Success(
            "enumerable.order-descending.comparer-inverts-custom-direction",
            "static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IComparer<T>)",
            [Array(Number(1), Number(3), Number(2)), DescendingComparer()],
            Array(Number(1), Number(2), Number(3))),
        Success(
            "enumerable.order-by.comparer-uses-selected-key-direction",
            "static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2)), Callable(ClrRuntimeCallableKind.Identity), DescendingComparer()],
            Array(Number(3), Number(2), Number(1))),
        Success(
            "enumerable.order-by-descending.comparer-inverts-selected-key-direction",
            "static System.Linq.Enumerable.OrderByDescending<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)",
            [Array(Number(1), Number(3), Number(2)), Callable(ClrRuntimeCallableKind.Identity), DescendingComparer()],
            Array(Number(1), Number(2), Number(3))),
        Success(
            "enumerable.then-by.comparer-orders-equal-primary-keys",
            "static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)",
            [Invoke(IdentityMember, Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)), Callable(ClrRuntimeCallableKind.Identity), DescendingComparer()],
            Array(Number(3), Number(1), Number(4), Number(2))),
        Success(
            "enumerable.then-by-descending.comparer-inverts-secondary-direction",
            "static System.Linq.Enumerable.ThenByDescending<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)",
            [Invoke(IdentityMember, Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)), Callable(ClrRuntimeCallableKind.Identity), DescendingComparer()],
            Array(Number(1), Number(3), Number(2), Number(4))),

        Success(
            "enumerable.to-hash-set.default-creates-set",
            "static System.Linq.Enumerable.ToHashSet<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
            [Array(Number(1), Number(1), Number(2))],
            Set(Number(1), Number(2))),
        Success(
            "enumerable.to-hash-set.comparer-deduplicates-equivalence-classes",
            "static System.Linq.Enumerable.ToHashSet<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)",
            [Array(Number(1), Number(3), Number(2)), ParityEquality()],
            Set(Number(1), Number(2))),
        Success(
            "enumerable.to-dictionary.default-projects-keys-and-elements",
            "static System.Linq.Enumerable.ToDictionary<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)",
            [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.Identity)],
            Map((Number(1), Number(1)), (Number(2), Number(2)))),
        Success(
            "enumerable.to-dictionary.comparer-decorates-map-key-lookup",
            "static System.Linq.Enumerable.ToDictionary<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.Identity), ParityEquality()],
            Map((Number(1), Number(1)), (Number(2), Number(2)))),
        Success(
            "enumerable.to-dictionary-element.default-projects-value",
            "static System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)",
            [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.DoubleNumber)],
            Map((Number(1), Number(2)), (Number(2), Number(4)))),
        Success(
            "enumerable.to-dictionary-element.comparer-decorates-map-key-lookup",
            "static System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)",
            [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.Identity), Callable(ClrRuntimeCallableKind.DoubleNumber), ParityEquality()],
            Map((Number(1), Number(2)), (Number(2), Number(4))))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeValue ParityEquality() => Record(
        ("equals", Callable(ClrRuntimeCallableKind.SameParity)),
        ("getHashCode", Callable(ClrRuntimeCallableKind.ParityHash)));

    private static ClrRuntimeValue DescendingComparer() => Record(
        ("compare", Callable(ClrRuntimeCallableKind.CompareDescending)));

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Set(params ClrRuntimeValue[] values) => ClrRuntimeValue.Set(values);
    private static ClrRuntimeValue Map(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries) => ClrRuntimeValue.Map(entries);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties) => ClrRuntimeValue.Record(properties);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
