namespace Jazor.CLR.Test;

internal static class ClrRuntimeReadOnlyCollectionScenarios
{
    private const string ReadOnlySetModulePath = "System/Collections/ObjectModel/ReadOnlySetT1Module.js";
    private const string ReadOnlyCollectionModulePath = "System/Collections/ObjectModel/ReadOnlyCollectionT1Module.js";
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("read-only-set.constructor.snapshots-source", "System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2))], Set(Number(1), Number(2))),
        Success("read-only-set.empty", "static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get", ReadOnlySetModulePath, [], Set()),
        Success("read-only-set.proper-subset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1)), Set(Number(1), Number(2))], Bool(true)),
        Success("read-only-set.proper-superset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Set(Number(1))], Bool(true)),
        Success("read-only-set.subset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1)), Set(Number(1), Number(2))], Bool(true)),
        Success("read-only-set.superset", "System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Set(Number(1))], Bool(true)),
        Success("read-only-set.overlaps", "System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Set(Number(2), Number(3))], Bool(true)),
        Success("read-only-set.set-equals.ignores-order", "System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)", ReadOnlySetModulePath, [Set(Number(1), Number(2)), Array(Number(2), Number(1))], Bool(true)),

        Success("read-only-collection.constructor.snapshots-source", "System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)", ReadOnlyCollectionModulePath, [Array(Text("release"), Text("owner"))], Array(Text("release"), Text("owner"))),
        Failure("read-only-collection.constructor.rejects-null-source", "System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)", ReadOnlyCollectionModulePath, [Null()], "ArgumentNullException"),
        Success("read-only-collection.empty", "static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get", ReadOnlyCollectionModulePath, [], Array()),
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

        Success("enumerable.where.value-predicate", "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", EnumerableModulePath, [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], Array(Number(2))),
        Success("enumerable.where.index-predicate", "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)", EnumerableModulePath, [Array(Number(10), Number(11), Number(12)), Callable(ClrRuntimeCallableKind.IsEvenIndex)], Array(Number(10), Number(12))),
        Success("enumerable.select.value-selector", "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>)", EnumerableModulePath, [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.DoubleNumber)], Array(Number(2), Number(4), Number(6))),
        Success("enumerable.select.index-selector", "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, TResult>)", EnumerableModulePath, [Array(Number(10), Number(11), Number(12)), Callable(ClrRuntimeCallableKind.AddIndex)], Array(Number(10), Number(12), Number(14))),
        Success("enumerable.to-list.materializes-set-order", "static System.Linq.Enumerable.ToList<TSource>(System.Collections.Generic.IEnumerable<TSource>)", EnumerableModulePath, [Set(Text("release"), Text("owner"))], Array(Text("release"), Text("owner"))),
        Success("enumerable.to-array.materializes-source", "static System.Linq.Enumerable.ToArray<TSource>(System.Collections.Generic.IEnumerable<TSource>)", EnumerableModulePath, [Array(Text("release"), Text("owner"))], Array(Text("release"), Text("owner")))
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
}
