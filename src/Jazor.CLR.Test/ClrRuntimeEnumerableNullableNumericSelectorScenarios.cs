namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableNullableNumericSelectorScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.sum.nullable-int-by", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(6)),
        Success("enumerable.sum.nullable-long-by", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], BigInt(3)),
        Success("enumerable.sum.nullable-single-by", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(6)),
        Success("enumerable.sum.nullable-double-by", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(6)),
        Success("enumerable.sum.nullable-decimal-by", "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("3")),
        Success("enumerable.average.nullable-int-by", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(3)),
        Success("enumerable.average.nullable-long-by", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], Number(1.5)),
        Success("enumerable.average.nullable-single-by", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(3)),
        Success("enumerable.average.nullable-double-by", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(3)),
        Success("enumerable.average.nullable-decimal-by", "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("1.5")),
        Success("enumerable.min.nullable-int-by", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(2)),
        Success("enumerable.min.nullable-long-by", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], BigInt(1)),
        Success("enumerable.min.nullable-single-by", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(2)),
        Success("enumerable.min.nullable-double-by", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(2)),
        Success("enumerable.min.nullable-decimal-by", "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("1")),
        Success("enumerable.max.nullable-int-by", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(4)),
        Success("enumerable.max.nullable-long-by", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToBigInt)], BigInt(2)),
        Success("enumerable.max.nullable-single-by", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(4)),
        Success("enumerable.max.nullable-double-by", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(4)),
        Success("enumerable.max.nullable-decimal-by", "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", [Numbers(1, 2), Callable(ClrRuntimeCallableKind.ToDecimalText)], Text("2"))
    ];

    private static ClrRuntimeScenario Success(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Numbers(params double[] values) => Array(values.Select(static value => Number(value)).ToArray());
}
