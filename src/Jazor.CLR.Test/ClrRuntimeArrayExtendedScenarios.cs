namespace Jazor.CLR.Test;

internal static class ClrRuntimeArrayExtendedScenarios
{
    private const string ModulePath = "System/ArrayModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("array.binary-search.object", "static System.Array.BinarySearch(System.Array, object)", [Array(Number(2), Number(4), Number(6)), Number(4)], Number(1)),
        Success("array.binary-search.object-range", "static System.Array.BinarySearch(System.Array, int, int, object)", [Array(Number(1), Number(3), Number(5), Number(7)), Number(1), Number(2), Number(5)], Number(2)),
        Success("array.binary-search.object-null-comparer", "static System.Array.BinarySearch(System.Array, object, System.Collections.IComparer)", [Array(Number(2), Number(4), Number(6)), Number(4), Null()], Number(1)),
        Success("array.binary-search.object-range-null-comparer", "static System.Array.BinarySearch(System.Array, int, int, object, System.Collections.IComparer)", [Array(Number(1), Number(3), Number(5), Number(7)), Number(1), Number(2), Number(5), Null()], Number(2)),
        Success("array.binary-search.generic-null-comparer", "static System.Array.BinarySearch<T>(T[], T, System.Collections.Generic.IComparer<T>)", [Array(Number(2), Number(4), Number(6)), Number(4), Null()], Number(1)),
        Success("array.binary-search.generic-range-null-comparer", "static System.Array.BinarySearch<T>(T[], int, int, T, System.Collections.Generic.IComparer<T>)", [Array(Number(1), Number(3), Number(5), Number(7)), Number(1), Number(2), Number(5), Null()], Number(2)),

        Mutation("array.copy.long-length", "static System.Array.Copy(System.Array, System.Array, long)", [Array(Number(1), Number(2), Number(3)), Array(Number(0), Number(0), Number(0)), BigInt(2)], [Array(Number(1), Number(2), Number(3)), Array(Number(1), Number(2), Number(0)), BigInt(2)]),
        Mutation("array.copy.long-ranges", "static System.Array.Copy(System.Array, long, System.Array, long, long)", [Array(Number(1), Number(2), Number(3)), BigInt(1), Array(Number(0), Number(0), Number(0)), BigInt(0), BigInt(2)], [Array(Number(1), Number(2), Number(3)), BigInt(1), Array(Number(2), Number(3), Number(0)), BigInt(0), BigInt(2)]),

        Success("array.find-index.whole-array", "static System.Array.FindIndex<T>(T[], System.Predicate<T>)", [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], Number(1)),
        Success("array.find-index.from-start", "static System.Array.FindIndex<T>(T[], int, System.Predicate<T>)", [Array(Number(1), Number(3), Number(4)), Number(1), Callable(ClrRuntimeCallableKind.IsEven)], Number(2)),
        Success("array.find-last-index.whole-array", "static System.Array.FindLastIndex<T>(T[], System.Predicate<T>)", [Array(Number(1), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)], Number(2)),
        Success("array.find-last-index.from-start", "static System.Array.FindLastIndex<T>(T[], int, System.Predicate<T>)", [Array(Number(2), Number(4), Number(5)), Number(1), Callable(ClrRuntimeCallableKind.IsEven)], Number(1)),
        Success("array.find-last-index.bounded", "static System.Array.FindLastIndex<T>(T[], int, int, System.Predicate<T>)", [Array(Number(2), Number(4), Number(6)), Number(2), Number(2), Callable(ClrRuntimeCallableKind.IsEven)], Number(2)),
        Mutation("array.for-each.executes-action", "static System.Array.ForEach<T>(T[], System.Action<T>)", [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.DoubleNumber)], [Array(Number(1), Number(2)), Callable(ClrRuntimeCallableKind.DoubleNumber)]),

        Success("array.index-of.object", "static System.Array.IndexOf(System.Array, object)", [Array(Text("a"), Text("b"), Text("a")), Text("a")], Number(0)),
        Success("array.index-of.object-from-start", "static System.Array.IndexOf(System.Array, object, int)", [Array(Text("a"), Text("b"), Text("a")), Text("a"), Number(1)], Number(2)),
        Success("array.index-of.object-bounded", "static System.Array.IndexOf(System.Array, object, int, int)", [Array(Text("a"), Text("b"), Text("a")), Text("a"), Number(1), Number(1)], Number(-1)),
        Success("array.index-of.generic-from-start", "static System.Array.IndexOf<T>(T[], T, int)", [Array(Text("a"), Text("b"), Text("a")), Text("a"), Number(1)], Number(2)),
        Success("array.last-index-of.object", "static System.Array.LastIndexOf(System.Array, object)", [Array(Text("a"), Text("b"), Text("a")), Text("a")], Number(2)),
        Success("array.last-index-of.object-from-start", "static System.Array.LastIndexOf(System.Array, object, int)", [Array(Text("a"), Text("b"), Text("a")), Text("a"), Number(1)], Number(0)),
        Success("array.last-index-of.object-bounded", "static System.Array.LastIndexOf(System.Array, object, int, int)", [Array(Text("a"), Text("b"), Text("a"), Text("b")), Text("b"), Number(2), Number(2)], Number(1)),
        Mutation("array.reverse.object-range", "static System.Array.Reverse(System.Array, int, int)", [Array(Number(1), Number(2), Number(3), Number(4)), Number(1), Number(2)], [Array(Number(1), Number(3), Number(2), Number(4)), Number(1), Number(2)]),

        Mutation("array.sort.object", "static System.Array.Sort(System.Array)", [Array(Number(3), Number(1), Number(2))], [Array(Number(1), Number(2), Number(3))]),
        Mutation("array.sort.object-keys-items", "static System.Array.Sort(System.Array, System.Array)", [Array(Number(2), Number(1)), Array(Text("b"), Text("a"))], [Array(Number(1), Number(2)), Array(Text("a"), Text("b"))]),
        Mutation("array.sort.object-keys-items-null-comparer", "static System.Array.Sort(System.Array, System.Array, System.Collections.IComparer)", [Array(Number(2), Number(1)), Array(Text("b"), Text("a")), Null()], [Array(Number(1), Number(2)), Array(Text("a"), Text("b")), Null()]),
        Mutation("array.sort.object-keys-items-range", "static System.Array.Sort(System.Array, System.Array, int, int)", [Array(Number(9), Number(3), Number(1), Number(8)), Array(Text("z"), Text("c"), Text("a"), Text("y")), Number(1), Number(2)], [Array(Number(9), Number(1), Number(3), Number(8)), Array(Text("z"), Text("a"), Text("c"), Text("y")), Number(1), Number(2)]),
        Mutation("array.sort.object-keys-items-range-null-comparer", "static System.Array.Sort(System.Array, System.Array, int, int, System.Collections.IComparer)", [Array(Number(9), Number(3), Number(1), Number(8)), Array(Text("z"), Text("c"), Text("a"), Text("y")), Number(1), Number(2), Null()], [Array(Number(9), Number(1), Number(3), Number(8)), Array(Text("z"), Text("a"), Text("c"), Text("y")), Number(1), Number(2), Null()]),
        Mutation("array.sort.object-null-comparer", "static System.Array.Sort(System.Array, System.Collections.IComparer)", [Array(Number(3), Number(1), Number(2)), Null()], [Array(Number(1), Number(2), Number(3)), Null()]),
        Mutation("array.sort.object-range", "static System.Array.Sort(System.Array, int, int)", [Array(Number(9), Number(3), Number(1), Number(8)), Number(1), Number(2)], [Array(Number(9), Number(1), Number(3), Number(8)), Number(1), Number(2)]),
        Mutation("array.sort.object-range-null-comparer", "static System.Array.Sort(System.Array, int, int, System.Collections.IComparer)", [Array(Number(9), Number(3), Number(1), Number(8)), Number(1), Number(2), Null()], [Array(Number(9), Number(1), Number(3), Number(8)), Number(1), Number(2), Null()]),
        Mutation("array.sort.generic-null-comparer", "static System.Array.Sort<T>(T[], System.Collections.Generic.IComparer<T>)", [Array(Number(3), Number(1), Number(2)), Null()], [Array(Number(1), Number(2), Number(3)), Null()]),
        Mutation("array.sort.generic-range-null-comparer", "static System.Array.Sort<T>(T[], int, int, System.Collections.Generic.IComparer<T>)", [Array(Number(9), Number(3), Number(1), Number(8)), Number(1), Number(2), Null()], [Array(Number(9), Number(1), Number(3), Number(8)), Number(1), Number(2), Null()]),
        Mutation("array.sort.generic-keys-items-range-null-comparer", "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int, System.Collections.Generic.IComparer<TKey>)", [Array(Number(9), Number(3), Number(1), Number(8)), Array(Text("z"), Text("c"), Text("a"), Text("y")), Number(1), Number(2), Null()], [Array(Number(9), Number(1), Number(3), Number(8)), Array(Text("z"), Text("a"), Text("c"), Text("y")), Number(1), Number(2), Null()])
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Mutation(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, ModulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
