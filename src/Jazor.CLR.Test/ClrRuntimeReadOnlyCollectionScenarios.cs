namespace Jazor.CLR.Test;

internal static class ClrRuntimeReadOnlyCollectionScenarios
{
    private const string ReadOnlySetModulePath = "System/Collections/ObjectModel/ReadOnlySetT1Module.js";
    private const string ReadOnlyCollectionModulePath = "System/Collections/ObjectModel/ReadOnlyCollectionT1Module.js";
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("read-only-set.empty", "static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get", ReadOnlySetModulePath, [], Set()),
        Success("read-only-set.proper-subset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1)), Set(Number(1), Number(2))], Bool(true)),
        Success("read-only-set.proper-superset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Set(Number(1))], Bool(true)),
        Success("read-only-set.subset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1)), Set(Number(1), Number(2))], Bool(true)),
        Success("read-only-set.superset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Set(Number(1))], Bool(true)),
        Success("read-only-set.overlaps", "System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Set(Number(2), Number(3))], Bool(true)),
        Success("read-only-set.set-equals.ignores-order", "System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Array(Number(2), Number(1))], Bool(true)),

        Success("read-only-collection.empty", "static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get", ReadOnlyCollectionModulePath, [], Array()),
        Success("read-only-collection.create-collection.snapshots-order", "static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)", ReadOnlyCollectionModulePath, [Array(Number(1), Number(2), Number(1))], Array(Number(1), Number(2), Number(1))),
        Success("read-only-collection.create-set.deduplicates", "static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)", ReadOnlyCollectionModulePath, [Array(Number(1), Number(2), Number(1))], Set(Number(1), Number(2))),
        Success("read-only-collection.indexer.get-existing", "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner")), Number(1)], Text("owner")),
        Failure("read-only-collection.indexer.rejects-negative-index", "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get", ReadOnlyCollectionModulePath, [Array(Text("release")), Number(-1)], "ArgumentOutOfRangeException"),
        Failure("read-only-collection.indexer.rejects-fractional-index", "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get", ReadOnlyCollectionModulePath, [Array(Text("release")), Number(0.5)], "ArgumentOutOfRangeException"),
        Failure("read-only-collection.indexer.rejects-null-instance", "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get", ReadOnlyCollectionModulePath, [Null(), Number(0)], "NullReferenceException"),
        Mutation("read-only-collection.copy-to-array", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner")), Array(Null(), Null(), Null())], [Array(Text("release"), Text("owner")), Array(Text("release"), Text("owner"), Null())]),
        Failure("read-only-collection.copy-to-array-rejects-small-target", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner")), Array(Null())], "ArgumentException"),
        Failure("read-only-collection.copy-to-array-rejects-null-target", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])", ReadOnlyCollectionModulePath, [Array(Text("release")), Null()], "ArgumentNullException"),
        Mutation("read-only-collection.copy-to-array-with-index", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner")), Array(Text("before"), Null(), Null(), Text("after")), Number(1)], [Array(Text("release"), Text("owner")), Array(Text("before"), Text("release"), Text("owner"), Text("after")), Number(1)]),
        Failure("read-only-collection.copy-to-array-with-index-rejects-fraction", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)", ReadOnlyCollectionModulePath, [Array(Text("release")), Array(Null(), Null()), Number(0.5)], "ArgumentOutOfRangeException"),
        Failure("read-only-collection.copy-to-array-with-index-rejects-small-tail", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner")), Array(Null(), Null()), Number(1)], "ArgumentException"),
        Mutation("read-only-collection.copy-range", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner"), Text("stage")), Number(1), Array(Text("before"), Null(), Null(), Text("after")), Number(1), Number(2)], [Array(Text("release"), Text("owner"), Text("stage")), Number(1), Array(Text("before"), Text("owner"), Text("stage"), Text("after")), Number(1), Number(2)]),
        Failure("read-only-collection.copy-range-rejects-negative-count", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)", ReadOnlyCollectionModulePath, [Array(Text("release")), Number(0), Array(Null()), Number(0), Number(-1)], "ArgumentOutOfRangeException"),
        Failure("read-only-collection.copy-range-rejects-source-overrun", "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)", ReadOnlyCollectionModulePath, [Array(Text("release")), Number(1), Array(Null()), Number(0), Number(1)], "ArgumentException"),

        Success("enumerable.skip.removes-leading-items", "static System.Linq.Enumerable.Skip<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", EnumerableModulePath, [Array(Number(1), Number(2), Number(3), Number(4)), Number(2)], Array(Number(3), Number(4))),
        Success("enumerable.skip.non-positive-count-preserves-all-items", "static System.Linq.Enumerable.Skip<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", EnumerableModulePath, [Array(Number(1), Number(2)), Number(-1)], Array(Number(1), Number(2))),
        Success("enumerable.take.limits-items", "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", EnumerableModulePath, [Array(Number(1), Number(2), Number(3), Number(4)), Number(2)], Array(Number(1), Number(2))),
        Success("enumerable.take.non-positive-count-is-empty", "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", EnumerableModulePath, [Array(Number(1), Number(2)), Number(0)], Array()),
        Failure("enumerable.take.rejects-null-source", "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", EnumerableModulePath, [Null(), Number(1)], "ArgumentNullException"),
        Success("enumerable.any.empty-source-is-false", "static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>)", EnumerableModulePath, [Array()], Bool(false)),
        Success("enumerable.any.non-empty-source-is-true", "static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>)", EnumerableModulePath, [Array(Number(1))], Bool(true)),
        Success("enumerable.any.predicate-stops-on-match", "static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", EnumerableModulePath, [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], Bool(true)),
        Success("enumerable.all.predicate-matches-every-item", "static System.Linq.Enumerable.All<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", EnumerableModulePath, [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsPositive)], Bool(true)),
        Success("enumerable.all.predicate-stops-on-first-non-match", "static System.Linq.Enumerable.All<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", EnumerableModulePath, [Array(Number(1), Number(-1), Number(3)), Callable(ClrRuntimeCallableKind.IsPositive)], Bool(false)),
        Failure("enumerable.any.predicate-rejects-null", "static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", EnumerableModulePath, [Array(Number(1)), Null()], "ArgumentNullException"),
        Success("enumerable.order-by.stable-boolean-keys", "static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", EnumerableModulePath, [Array(Number(2), Number(3), Number(4), Number(1)), Callable(ClrRuntimeCallableKind.IsEven)], Array(Number(3), Number(1), Number(2), Number(4))),
        Success("enumerable.order-by-descending.stable-boolean-keys", "static System.Linq.Enumerable.OrderByDescending<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", EnumerableModulePath, [Array(Number(2), Number(3), Number(4), Number(1)), Callable(ClrRuntimeCallableKind.IsEven)], Array(Number(2), Number(4), Number(3), Number(1))),
        Success("enumerable.order.uses-default-comparer", "static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>)", EnumerableModulePath, [Array(Number(2), Number(3), Number(4), Number(1))], Array(Number(1), Number(2), Number(3), Number(4))),
        Success("enumerable.order-descending.uses-default-comparer", "static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>)", EnumerableModulePath, [Array(Number(2), Number(3), Number(4), Number(1))], Array(Number(4), Number(3), Number(2), Number(1))),
        Failure("enumerable.order-by.rejects-null-selector", "static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", EnumerableModulePath, [Array(Number(1)), Null()], "ArgumentNullException"),
        Failure("enumerable.order.rejects-null-source", "static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>)", EnumerableModulePath, [Null()], "ArgumentNullException"),
        Success("enumerable.then-by.composes-primary-and-secondary-keys", "static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)", EnumerableModulePath, [Invoke("static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Array(Number(2), Number(3), Number(4), Number(1)), Callable(ClrRuntimeCallableKind.IsEven)), Callable(ClrRuntimeCallableKind.DoubleNumber)], Array(Number(1), Number(3), Number(2), Number(4))),
        Success("enumerable.then-by-descending.composes-primary-and-secondary-keys", "static System.Linq.Enumerable.ThenByDescending<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)", EnumerableModulePath, [Invoke("static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Array(Number(2), Number(1), Number(4), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)), Callable(ClrRuntimeCallableKind.DoubleNumber)], Array(Number(3), Number(1), Number(4), Number(2))),
        Failure("enumerable.then-by.rejects-untracked-ordered-source", "static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)", EnumerableModulePath, [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.DoubleNumber)], "ThenBy requires an ordering produced by Jazor")
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeScenario Mutation(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Set(params ClrRuntimeValue[] values) => ClrRuntimeValue.Set(values);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);
}
