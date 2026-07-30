namespace Jazor.CLR.Test;

internal static class ClrRuntimeTailScenarios
{
    private const string BigIntegerModulePath = "System/Numerics/BigIntegerModule.js";
    private const string ListModulePath = "System/Collections/Generic/ListT1Module.js";
    private const string BooleanModulePath = "System/BooleanModule.js";
    private const string GenericCollectionModulePath = "System/Collections/Generic/ICollectionT1Module.js";
    private const string GenericListModulePath = "System/Collections/Generic/IListT1Module.js";
    private const string ReadOnlyListModulePath = "System/Collections/Generic/IReadOnlyListT1Module.js";
    private const string CollectionModulePath = "System/Collections/ICollectionModule.js";
    private const string ListInterfaceModulePath = "System/Collections/IListModule.js";
    private const string ExceptionModulePath = "System/ExceptionModule.js";
    private const string DisposableModulePath = "System/IDisposableModule.js";
    private const string AsyncDisposableModulePath = "System/IAsyncDisposableModule.js";
    private const string ComparableModulePath = "System/IComparableModule.js";
    private const string GenericComparableModulePath = "System/IComparableT1Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("big-integer.to-string.null-provider", "System.Numerics.BigInteger.ToString(System.IFormatProvider)", BigIntegerModulePath, [BigInt(123456789), Null()], Text("123456789")),
        Success("big-integer.create-checked.boolean", "static System.Numerics.BigInteger.CreateChecked<TOther>(TOther)", BigIntegerModulePath, [Bool(true)], BigInt(1)),

        Success("list.find-last-index.predicate", "System.Collections.Generic.List<T>.FindLastIndex(System.Predicate<T>)", ListModulePath, [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)], Number(3)),
        Success("list.find-last-index.starts-at-index", "System.Collections.Generic.List<T>.FindLastIndex(int, System.Predicate<T>)", ListModulePath, [Array(Number(1), Number(2), Number(3), Number(4)), Number(2), Callable(ClrRuntimeCallableKind.IsEven)], Number(1)),
        Success("list.last-index-of.respects-start-index", "System.Collections.Generic.List<T>.LastIndexOf(T, int)", ListModulePath, [Array(Text("release"), Text("owner"), Text("release")), Text("release"), Number(1)], Number(0)),

        Success("boolean.try-parse-span.trimmed-true", "static bool.TryParse(System.ReadOnlySpan<char>, out bool)", BooleanModulePath, [Text(" true "), Bool(false)], Array(Bool(true), Bool(true))),

        Mutation("generic-collection.copy-to-with-index", "System.Collections.Generic.ICollection<T>.CopyTo(T[], int)", GenericCollectionModulePath, [Array(Text("release"), Text("owner")), Array(Text("before"), Null(), Null(), Text("after")), Number(1)], [Array(Text("release"), Text("owner")), Array(Text("before"), Text("release"), Text("owner"), Text("after")), Number(1)]),
        Success("generic-list.indexer.get", "System.Collections.Generic.IList<T>.this[int].get", GenericListModulePath, [Array(Text("release"), Text("owner")), Number(1)], Text("owner")),
        Success("read-only-list.indexer.get", "System.Collections.Generic.IReadOnlyList<T>.this[int].get", ReadOnlyListModulePath, [Array(Text("release"), Text("owner")), Number(0)], Text("release")),
        Failure("read-only-list.indexer.rejects-fractional-index", "System.Collections.Generic.IReadOnlyList<T>.this[int].get", ReadOnlyListModulePath, [Array(Text("release")), Number(0.5)], "ArgumentOutOfRangeException"),
        Failure("read-only-list.indexer.rejects-null-instance", "System.Collections.Generic.IReadOnlyList<T>.this[int].get", ReadOnlyListModulePath, [Null(), Number(0)], "NullReferenceException"),
        Mutation("collection.copy-to-with-index", "System.Collections.ICollection.CopyTo(System.Array, int)", CollectionModulePath, [Array(Text("release"), Text("owner")), Array(Text("before"), Null(), Null(), Text("after")), Number(1)], [Array(Text("release"), Text("owner")), Array(Text("before"), Text("release"), Text("owner"), Text("after")), Number(1)]),
        Success("list-interface.indexer.get", "System.Collections.IList.this[int].get", ListInterfaceModulePath, [Array(Text("release"), Text("owner")), Number(1)], Text("owner")),

        Failure("argument-null-exception.throw-if-null", "static System.ArgumentNullException.ThrowIfNull(object, string)", ExceptionModulePath, [Null(), Text("release")], "release"),
        Mutation("disposable.dispose.invokes-carrier", "System.IDisposable.Dispose()", DisposableModulePath, [Disposable()], [Disposable(1)]),
        Mutation("async-disposable.dispose-async.invokes-carrier", "System.IAsyncDisposable.DisposeAsync()", AsyncDisposableModulePath, [AsyncDisposable()], [AsyncDisposable(1)]),
        Success("comparable.compare-to-object", "System.IComparable.CompareTo(object)", ComparableModulePath, [Number(4), Number(9)], Number(-1)),
        Success("generic-comparable.compare-to", "System.IComparable<T>.CompareTo(T)", GenericComparableModulePath, [Text("release"), Text("stage")], Number(-1))
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
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
    private static ClrRuntimeValue Disposable(int count = 0) => ClrRuntimeValue.Disposable(count);
    private static ClrRuntimeValue AsyncDisposable(int count = 0) => ClrRuntimeValue.AsyncDisposable(count);
}
