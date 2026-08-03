namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableSetScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.distinct.preserves-first-clr-equivalent-value",
            "static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
            [Array(Number(double.NaN), Number(-0d), Number(0d), Number(double.NaN), Number(1d))],
            Array(Number(double.NaN), Number(-0d), Number(1d))),
        Success(
            "enumerable.union.preserves-first-then-second-unique-order",
            "static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)",
            [Array(Number(1d), Number(2d), Number(1d)), Array(Number(2d), Number(3d), Number(1d))],
            Array(Number(1d), Number(2d), Number(3d))),
        Success(
            "enumerable.except.filters-second-members-and-deduplicates-first",
            "static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)",
            [Array(Number(1d), Number(2d), Number(2d), Number(3d)), Array(Number(2d), Number(4d))],
            Array(Number(1d), Number(3d))),
        Success(
            "enumerable.intersect.preserves-first-order-and-distinctness",
            "static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)",
            [Array(Number(3d), Number(1d), Number(2d), Number(3d), Number(2d)), Array(Number(2d), Number(3d), Number(3d))],
            Array(Number(3d), Number(2d))),
        Success(
            "enumerable.contains.uses-default-clr-equality",
            "static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)",
            [Array(Number(1d), Number(double.NaN), Number(3d)), Number(double.NaN)],
            Boolean(true)),
        Success(
            "memory-extensions.contains.read-only-span-array-carrier-uses-default-clr-equality",
            "static System.MemoryExtensions.Contains<T>(System.ReadOnlySpan<T>, T)",
            "System/MemoryExtensionsModule.js",
            [Array(Number(1d), Number(double.NaN), Number(3d)), Number(double.NaN)],
            Boolean(true))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, EnumerableModulePath, arguments, expected);

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
}
