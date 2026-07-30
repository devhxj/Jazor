namespace Jazor.CLR.Test;

internal static class ClrRuntimeArrayScenarios
{
    private const string ModulePath = "System/ArrayModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "array.as-read-only.copies-source",
            "static System.Array.AsReadOnly<T>(T[])",
            [Array(Number(1), Number(2))],
            Array(Number(1), Number(2))),
        Failure(
            "array.as-read-only.null-source",
            "static System.Array.AsReadOnly<T>(T[])",
            [Null()],
            "ArgumentNullException"),
        Success(
            "array.resize.shrinks-and-preserves-prefix",
            "static System.Array.Resize<T>(ref T[], int)",
            [Array(Number(1), Number(2), Number(3)), Number(2)],
            Array(Array(Number(1), Number(2)))),
        Success(
            "array.resize.null-to-empty",
            "static System.Array.Resize<T>(ref T[], int)",
            [Null(), Number(0)],
            Array(Array())),
        Failure(
            "array.resize.negative-size",
            "static System.Array.Resize<T>(ref T[], int)",
            [Array(Number(1)), Number(-1)],
            "ArgumentOutOfRangeException"),
        Mutation(
            "array.copy.int-length.writes-destination-prefix",
            "static System.Array.Copy(System.Array, System.Array, int)",
            [Array(Number(4), Number(5), Number(6)), Array(Number(0), Number(0), Number(9)), Number(2)],
            [Array(Number(4), Number(5), Number(6)), Array(Number(4), Number(5), Number(9)), Number(2)]),
        Mutation(
            "array.copy.int-ranges.writes-destination-offset",
            "static System.Array.Copy(System.Array, int, System.Array, int, int)",
            [Array(Number(4), Number(5), Number(6)), Number(1), Array(Number(0), Number(0), Number(0)), Number(0), Number(2)],
            [Array(Number(4), Number(5), Number(6)), Number(1), Array(Number(5), Number(6), Number(0)), Number(0), Number(2)]),
        Mutation(
            "array.constrained-copy.valid-range",
            "static System.Array.ConstrainedCopy(System.Array, int, System.Array, int, int)",
            [Array(Text("a"), Text("b"), Text("c")), Number(0), Array(Text("x"), Text("y"), Text("z")), Number(1), Number(2)],
            [Array(Text("a"), Text("b"), Text("c")), Number(0), Array(Text("x"), Text("a"), Text("b")), Number(1), Number(2)]),
        Failure(
            "array.copy.length-exceeds-destination",
            "static System.Array.Copy(System.Array, System.Array, int)",
            [Array(Number(1), Number(2)), Array(Number(0)), Number(2)],
            "ArgumentException"),
        Success(
            "array.binary-search.generic-found-middle",
            "static System.Array.BinarySearch<T>(T[], T)",
            [Array(Number(2), Number(4), Number(6), Number(8)), Number(6)],
            Number(2)),
        Success(
            "array.binary-search.generic-missing-complement",
            "static System.Array.BinarySearch<T>(T[], T)",
            [Array(Number(2), Number(4), Number(6), Number(8)), Number(5)],
            Number(-3)),
        Success(
            "array.binary-search.range-empty-complement",
            "static System.Array.BinarySearch<T>(T[], int, int, T)",
            [Array(Number(2), Number(4), Number(6)), Number(1), Number(0), Number(3)],
            Number(-2)),
        Failure(
            "array.binary-search.range-outside-array",
            "static System.Array.BinarySearch<T>(T[], int, int, T)",
            [Array(Number(2), Number(4)), Number(1), Number(2), Number(4)],
            "ArgumentException"),
        Success(
            "array.convert-all.maps-each-value",
            "static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)",
            [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.DoubleNumber)],
            Array(Number(2), Number(4), Number(6))),
        Failure(
            "array.convert-all.null-converter",
            "static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)",
            [Array(Number(1)), Null()],
            "ArgumentNullException"),
        Mutation(
            "array.copy-to.int-index",
            "System.Array.CopyTo(System.Array, int)",
            [Array(Text("a"), Text("b")), Array(Text("x"), Text("x"), Text("x")), Number(1)],
            [Array(Text("a"), Text("b")), Array(Text("x"), Text("a"), Text("b")), Number(1)]),
        Mutation(
            "array.copy-to.long-index",
            "System.Array.CopyTo(System.Array, long)",
            [Array(Number(7), Number(8)), Array(Number(0), Number(0), Number(0)), BigInt(1)],
            [Array(Number(7), Number(8)), Array(Number(0), Number(7), Number(8)), BigInt(1)]),
        Failure(
            "array.copy-to.destination-too-small",
            "System.Array.CopyTo(System.Array, int)",
            [Array(Number(1), Number(2)), Array(Number(0), Number(0)), Number(1)],
            "ArgumentException"),
        Mutation(
            "array.fill.bounded-range",
            "static System.Array.Fill<T>(T[], T, int, int)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Number(9), Number(1), Number(2)],
            [Array(Number(1), Number(9), Number(9), Number(4)), Number(9), Number(1), Number(2)]),
        Failure(
            "array.fill.range-exceeds-array",
            "static System.Array.Fill<T>(T[], T, int, int)",
            [Array(Number(1), Number(2)), Number(9), Number(1), Number(2)],
            "ArgumentException"),
        Success(
            "array.exists.even-value",
            "static System.Array.Exists<T>(T[], System.Predicate<T>)",
            [Array(Number(1), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)],
            Bool(true)),
        Success(
            "array.find.no-positive-value-returns-default",
            "static System.Array.Find<T>(T[], System.Predicate<T>)",
            [Array(Text("a"), Text("b")), Callable(ClrRuntimeCallableKind.IsPositive)],
            Null()),
        Success(
            "array.find-all.even-values",
            "static System.Array.FindAll<T>(T[], System.Predicate<T>)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)],
            Array(Number(2), Number(4))),
        Success(
            "array.find-index.bounded-match",
            "static System.Array.FindIndex<T>(T[], int, int, System.Predicate<T>)",
            [Array(Number(2), Number(3), Number(4), Number(6)), Number(1), Number(2), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(2)),
        Success(
            "array.find-last.last-even-value",
            "static System.Array.FindLast<T>(T[], System.Predicate<T>)",
            [Array(Number(2), Number(4), Number(5)), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(4)),
        Success(
            "array.index-of.generic-first-match",
            "static System.Array.IndexOf<T>(T[], T)",
            [Array(Text("a"), Text("b"), Text("a")), Text("a")],
            Number(0)),
        Success(
            "array.index-of.generic-bounded-no-match",
            "static System.Array.IndexOf<T>(T[], T, int, int)",
            [Array(Text("a"), Text("b"), Text("a")), Text("a"), Number(1), Number(1)],
            Number(-1)),
        Failure(
            "array.index-of.generic-invalid-count",
            "static System.Array.IndexOf<T>(T[], T, int, int)",
            [Array(Text("a"), Text("b")), Text("a"), Number(1), Number(2)],
            "ArgumentOutOfRangeException"),
        Success(
            "array.last-index-of.generic-last-match",
            "static System.Array.LastIndexOf<T>(T[], T)",
            [Array(Text("a"), Text("b"), Text("a")), Text("a")],
            Number(2)),
        Success(
            "array.last-index-of.generic-bounded-match",
            "static System.Array.LastIndexOf<T>(T[], T, int, int)",
            [Array(Text("a"), Text("b"), Text("a"), Text("b")), Text("b"), Number(2), Number(2)],
            Number(1)),
        Failure(
            "array.last-index-of.generic-invalid-start",
            "static System.Array.LastIndexOf<T>(T[], T, int)",
            [Array(Text("a")), Text("a"), Number(1)],
            "ArgumentOutOfRangeException"),
        Mutation(
            "array.reverse.generic-whole-array",
            "static System.Array.Reverse<T>(T[])",
            [Array(Number(1), Number(2), Number(3))],
            [Array(Number(3), Number(2), Number(1))]),
        Mutation(
            "array.reverse.generic-segment",
            "static System.Array.Reverse<T>(T[], int, int)",
            [Array(Number(1), Number(2), Number(3), Number(4)), Number(1), Number(2)],
            [Array(Number(1), Number(3), Number(2), Number(4)), Number(1), Number(2)]),
        Failure(
            "array.reverse.segment-outside-array",
            "static System.Array.Reverse<T>(T[], int, int)",
            [Array(Number(1), Number(2)), Number(1), Number(2)],
            "ArgumentException"),
        Mutation(
            "array.sort.generic-whole-array",
            "static System.Array.Sort<T>(T[])",
            [Array(Number(30), Number(10), Number(20))],
            [Array(Number(10), Number(20), Number(30))]),
        Mutation(
            "array.sort.generic-segment",
            "static System.Array.Sort<T>(T[], int, int)",
            [Array(Number(9), Number(3), Number(1), Number(8)), Number(1), Number(2)],
            [Array(Number(9), Number(1), Number(3), Number(8)), Number(1), Number(2)]),
        Mutation(
            "array.sort.keys-and-items-preserves-pairs",
            "static System.Array.Sort<TKey, TValue>(TKey[], TValue[])",
            [Array(Number(3), Number(1), Number(2)), Array(Text("c"), Text("a"), Text("b"))],
            [Array(Number(1), Number(2), Number(3)), Array(Text("a"), Text("b"), Text("c"))]),
        Mutation(
            "array.sort.keys-and-items.bounded-range-preserves-pairs",
            "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int)",
            [Array(Number(9), Number(3), Number(1), Number(8)), Array(Text("z"), Text("c"), Text("a"), Text("y")), Number(1), Number(2)],
            [Array(Number(9), Number(1), Number(3), Number(8)), Array(Text("z"), Text("a"), Text("c"), Text("y")), Number(1), Number(2)]),
        Mutation(
            "array.sort.keys-with-null-items",
            "static System.Array.Sort<TKey, TValue>(TKey[], TValue[])",
            [Array(Number(3), Number(1), Number(2)), Null()],
            [Array(Number(1), Number(2), Number(3)), Null()]),
        Failure(
            "array.sort.items-shorter-than-keys",
            "static System.Array.Sort<TKey, TValue>(TKey[], TValue[])",
            [Array(Number(3), Number(1), Number(2)), Array(Text("c"), Text("a"))],
            "ArgumentException"),
        Mutation(
            "array.sort.keys-and-items.null-comparer-uses-default",
            "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], System.Collections.Generic.IComparer<TKey>)",
            [Array(Number(2), Number(1)), Array(Text("b"), Text("a")), Null()],
            [Array(Number(1), Number(2)), Array(Text("a"), Text("b")), Null()]),
        Mutation(
            "array.sort.comparison-descending",
            "static System.Array.Sort<T>(T[], System.Comparison<T>)",
            [Array(Number(1), Number(3), Number(2)), Callable(ClrRuntimeCallableKind.CompareDescending)],
            [Array(Number(3), Number(2), Number(1)), Callable(ClrRuntimeCallableKind.CompareDescending)]),
        Success(
            "array.true-for-all.positive-values",
            "static System.Array.TrueForAll<T>(T[], System.Predicate<T>)",
            [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsPositive)],
            Bool(true)),
        Failure(
            "array.sort.generic-null-array",
            "static System.Array.Sort<T>(T[])",
            [Null()],
            "ArgumentNullException")
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
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
