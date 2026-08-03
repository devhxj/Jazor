namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableNumericSelectorScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.sum-by.int", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(6)),
        Success("enumerable.sum-by.long", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], BigInt(3)),
        Success("enumerable.sum-by.single", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(6)),
        Success("enumerable.sum-by.double", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(6)),
        Success("enumerable.sum-by.decimal", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("3")),
        Failure("enumerable.sum-by.int.null-selector", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1), Null()], "ArgumentNullException"),
        Success("enumerable.average-by.int", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(3)),
        Success("enumerable.average-by.long", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], Number(1.5)),
        Success("enumerable.average-by.single", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(3)),
        Success("enumerable.average-by.double", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(3)),
        Success("enumerable.average-by.decimal", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("1.5")),
        Failure("enumerable.average-by.int.null-selector", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1), Null()], "ArgumentNullException"),
        Success("enumerable.min-by.int", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(2)),
        Success("enumerable.min-by.long", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], BigInt(1)),
        Success("enumerable.min-by.single", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(2)),
        Success("enumerable.min-by.double", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(2)),
        Success("enumerable.min-by.decimal", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", [Array(Text("10"), Text("2")), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("2")),
        Success("enumerable.max-by.int", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(4)),
        Success("enumerable.max-by.long", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], BigInt(2)),
        Success("enumerable.max-by.single", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(4)),
        Success("enumerable.max-by.double", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(4)),
        Success("enumerable.max-by.decimal", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", [Array(Text("10"), Text("2")), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("10")),
        Failure("enumerable.min-by.int.null-selector", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", [Numbers(1), Null()], "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Numbers(params double[] values) => Array(values.Select(static value => Number(value)).ToArray());

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
