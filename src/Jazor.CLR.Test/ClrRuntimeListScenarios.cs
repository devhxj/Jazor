namespace Jazor.CLR.Test;

internal static class ClrRuntimeListScenarios
{
    private const string ModulePath = "System/Collections/Generic/ListT1Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "list.indexer.get.valid-index",
            "System.Collections.Generic.List<T>.this[int].get",
            [Array(Text("a"), Text("b")), Number(1)],
            Text("b")),
        Failure(
            "list.indexer.get.negative-index",
            "System.Collections.Generic.List<T>.this[int].get",
            [Array(Text("a")), Number(-1)],
            "ArgumentOutOfRangeException"),
        Failure(
            "list.indexer.get.fractional-index",
            "System.Collections.Generic.List<T>.this[int].get",
            [Array(Text("a")), Number(0.5)],
            "ArgumentOutOfRangeException"),
        Mutation(
            "list.indexer.set.replaces-item",
            "System.Collections.Generic.List<T>.this[int].set",
            [Array(Text("a"), Text("b")), Number(0), Text("x")],
            [Array(Text("x"), Text("b")), Number(0), Text("x")]),
        Failure(
            "list.indexer.set.index-equals-count",
            "System.Collections.Generic.List<T>.this[int].set",
            [Array(Text("a")), Number(1), Text("x")],
            "ArgumentOutOfRangeException"),
        Mutation(
            "list.add-range.appends-sequence",
            "System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)",
            [Array(Number(1), Number(2)), Array(Number(3), Number(4))],
            [Array(Number(1), Number(2), Number(3), Number(4)), Array(Number(3), Number(4))]),
        Failure(
            "list.add-range.null-sequence",
            "System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)",
            [Array(Number(1)), Null()],
            "ArgumentNullException"),
        Success(
            "list.binary-search.default-finds-sorted-item",
            "System.Collections.Generic.List<T>.BinarySearch(T)",
            [Array(Number(1), Number(3), Number(5)), Number(3)],
            Number(1)),
        Success(
            "list.binary-search.comparer-uses-custom-order",
            "System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)",
            [Array(Number(5), Number(3), Number(1)), Number(3), DescendingComparer()],
            Number(1)),
        Success(
            "list.binary-search.range-honors-custom-order",
            "System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)",
            [Array(Number(9), Number(5), Number(3), Number(1), Number(8)), Number(1), Number(3), Number(3), DescendingComparer()],
            Number(2)),
        Success(
            "list.convert-all.projects-every-item",
            "System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)",
            [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.DoubleNumber)],
            Array(Number(2), Number(4), Number(6))),
        Failure(
            "list.convert-all.rejects-null-converter",
            "System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)",
            [Array(Number(1)), Null()],
            "ArgumentNullException"),
        Mutation(
            "list.copy-to.whole-list",
            "System.Collections.Generic.List<T>.CopyTo(T[])",
            [Array(Number(1), Number(2)), Array(Number(0), Number(0), Number(9))],
            [Array(Number(1), Number(2)), Array(Number(1), Number(2), Number(9))]),
        Failure(
            "list.copy-to.whole-list.insufficient-capacity",
            "System.Collections.Generic.List<T>.CopyTo(T[])",
            [Array(Number(1), Number(2)), Array(Number(0))],
            "ArgumentException"),
        Mutation(
            "list.copy-to.range-and-offset",
            "System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)",
            [Array(Number(1), Number(2), Number(3)), Number(1), Array(Number(0), Number(0), Number(0)), Number(0), Number(2)],
            [Array(Number(1), Number(2), Number(3)), Number(1), Array(Number(2), Number(3), Number(0)), Number(0), Number(2)]),
        Failure(
            "list.copy-to.range-exceeds-source",
            "System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)",
            [Array(Number(1), Number(2)), Number(1), Array(Number(0), Number(0)), Number(0), Number(2)],
            "ArgumentException"),
        Mutation(
            "list.copy-to.destination-offset",
            "System.Collections.Generic.List<T>.CopyTo(T[], int)",
            [Array(Text("a"), Text("b")), Array(Text("x"), Text("x"), Text("x")), Number(1)],
            [Array(Text("a"), Text("b")), Array(Text("x"), Text("a"), Text("b")), Number(1)]),
        Success(
            "list.find-index.from-start-index",
            "System.Collections.Generic.List<T>.FindIndex(int, System.Predicate<T>)",
            [Array(Number(2), Number(4), Number(5), Number(6)), Number(2), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(3)),
        Success(
            "list.find-index.bounded-no-match",
            "System.Collections.Generic.List<T>.FindIndex(int, int, System.Predicate<T>)",
            [Array(Number(1), Number(2), Number(4)), Number(0), Number(1), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(-1)),
        Failure(
            "list.find-index.null-predicate",
            "System.Collections.Generic.List<T>.FindIndex(int, System.Predicate<T>)",
            [Array(Number(1)), Number(0), Null()],
            "ArgumentNullException"),
        Success(
            "list.find-last.last-matching-value",
            "System.Collections.Generic.List<T>.FindLast(System.Predicate<T>)",
            [Array(Number(2), Number(3), Number(4), Number(5)), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(4)),
        Success(
            "list.find-last.no-match-returns-default",
            "System.Collections.Generic.List<T>.FindLast(System.Predicate<T>)",
            [Array(Number(-2), Number(-1)), Callable(ClrRuntimeCallableKind.IsPositive)],
            Null()),
        Success(
            "list.find-last-index.bounded-match",
            "System.Collections.Generic.List<T>.FindLastIndex(int, int, System.Predicate<T>)",
            [Array(Number(2), Number(3), Number(4), Number(6)), Number(2), Number(2), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(2)),
        Failure(
            "list.find-last-index.invalid-count",
            "System.Collections.Generic.List<T>.FindLastIndex(int, int, System.Predicate<T>)",
            [Array(Number(2), Number(4)), Number(1), Number(3), Callable(ClrRuntimeCallableKind.IsEven)],
            "ArgumentOutOfRangeException"),
        Success(
            "list.get-range.middle-slice",
            "System.Collections.Generic.List<T>.GetRange(int, int)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Number(1), Number(2)],
            Array(Number(2), Number(3))),
        Failure(
            "list.get-range.outside-list",
            "System.Collections.Generic.List<T>.GetRange(int, int)",
            [Array(Number(1), Number(2)), Number(1), Number(2)],
            "ArgumentException"),
        Success(
            "list.index-of.from-index-finds-nan",
            "System.Collections.Generic.List<T>.IndexOf(T, int)",
            [Array(Number(1), Number(double.NaN), Number(3)), Number(double.NaN), Number(1)],
            Number(1)),
        Success(
            "list.index-of.bounded-range",
            "System.Collections.Generic.List<T>.IndexOf(T, int, int)",
            [Array(Text("a"), Text("b"), Text("a")), Text("a"), Number(1), Number(2)],
            Number(2)),
        Failure(
            "list.index-of.invalid-range",
            "System.Collections.Generic.List<T>.IndexOf(T, int, int)",
            [Array(Text("a"), Text("b")), Text("a"), Number(1), Number(2)],
            "ArgumentOutOfRangeException"),
        Mutation(
            "list.insert.middle",
            "System.Collections.Generic.List<T>.Insert(int, T)",
            [Array(Text("a"), Text("c")), Number(1), Text("b")],
            [Array(Text("a"), Text("b"), Text("c")), Number(1), Text("b")]),
        Mutation(
            "list.insert.at-end",
            "System.Collections.Generic.List<T>.Insert(int, T)",
            [Array(Number(1)), Number(1), Number(2)],
            [Array(Number(1), Number(2)), Number(1), Number(2)]),
        Failure(
            "list.insert.fractional-index",
            "System.Collections.Generic.List<T>.Insert(int, T)",
            [Array(Number(1)), Number(0.5), Number(2)],
            "ArgumentOutOfRangeException"),
        Mutation(
            "list.insert-range.middle",
            "System.Collections.Generic.List<T>.InsertRange(int, System.Collections.Generic.IEnumerable<T>)",
            [Array(Number(1), Number(4)), Number(1), Array(Number(2), Number(3))],
            [Array(Number(1), Number(2), Number(3), Number(4)), Number(1), Array(Number(2), Number(3))]),
        Success(
            "list.last-index-of.bounded-range",
            "System.Collections.Generic.List<T>.LastIndexOf(T, int, int)",
            [Array(Text("a"), Text("b"), Text("a"), Text("b")), Text("b"), Number(2), Number(3)],
            Number(1)),
        SuccessMutation(
            "list.remove.first-match",
            "System.Collections.Generic.List<T>.Remove(T)",
            [Array(Text("a"), Text("b"), Text("a")), Text("a")],
            Bool(true),
            [Array(Text("b"), Text("a")), Text("a")]),
        SuccessMutation(
            "list.remove.missing-value",
            "System.Collections.Generic.List<T>.Remove(T)",
            [Array(Text("a"), Text("b")), Text("x")],
            Bool(false),
            [Array(Text("a"), Text("b")), Text("x")]),
        SuccessMutation(
            "list.remove-all.even-values",
            "System.Collections.Generic.List<T>.RemoveAll(System.Predicate<T>)",
            [Array(Number(1), Number(2), Number(4), Number(5)), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(2),
            [Array(Number(1), Number(5)), Callable(ClrRuntimeCallableKind.IsEven)]),
        Mutation(
            "list.remove-at.middle",
            "System.Collections.Generic.List<T>.RemoveAt(int)",
            [Array(Number(1), Number(2), Number(3)), Number(1)],
            [Array(Number(1), Number(3)), Number(1)]),
        Failure(
            "list.remove-at.index-equals-count",
            "System.Collections.Generic.List<T>.RemoveAt(int)",
            [Array(Number(1)), Number(1)],
            "ArgumentOutOfRangeException"),
        Mutation(
            "list.remove-range.middle",
            "System.Collections.Generic.List<T>.RemoveRange(int, int)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Number(1), Number(2)],
            [Array(Number(1), Number(4)), Number(1), Number(2)]),
        Mutation(
            "list.reverse.middle-range",
            "System.Collections.Generic.List<T>.Reverse(int, int)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Number(1), Number(2)],
            [Array(Number(1), Number(3), Number(2), Number(4)), Number(1), Number(2)]),
        Mutation(
            "list.sort.default-comparer",
            "System.Collections.Generic.List<T>.Sort()",
            [Array(Number(30), Number(10), Number(20))],
            [Array(Number(10), Number(20), Number(30))]),
        Mutation(
            "list.trim-excess.preserves-content",
            "System.Collections.Generic.List<T>.TrimExcess()",
            [Array(Number(1), Number(2))],
            [Array(Number(1), Number(2))]),
        Mutation(
            "list.sort.null-comparer-uses-default",
            "System.Collections.Generic.List<T>.Sort(System.Collections.Generic.IComparer<T>)",
            [Array(Text("c"), Text("a"), Text("b")), Null()],
            [Array(Text("a"), Text("b"), Text("c")), Null()]),
        Mutation(
            "list.sort.bounded-range",
            "System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)",
            [Array(Number(9), Number(3), Number(1), Number(8)), Number(1), Number(2), Null()],
            [Array(Number(9), Number(1), Number(3), Number(8)), Number(1), Number(2), Null()]),
        Failure(
            "list.sort.invalid-range",
            "System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)",
            [Array(Number(1), Number(2)), Number(1), Number(2), Null()],
            "ArgumentException")
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario SuccessMutation(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, ModulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Mutation(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(
            id,
            member,
            ModulePath,
            arguments,
            ClrRuntimeValue.Undefined(),
            ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue DescendingComparer() => ClrRuntimeValue.Record(
        ("compare", Callable(ClrRuntimeCallableKind.CompareDescending)));
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
