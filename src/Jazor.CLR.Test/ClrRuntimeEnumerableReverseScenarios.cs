namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableReverseScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string ReverseEnumerableMember = "static System.Linq.Enumerable.Reverse<TSource>(System.Collections.Generic.IEnumerable<TSource>)";
    private const string ReverseArrayMember = "static System.Linq.Enumerable.Reverse<TSource>(TSource[])";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        new(
            "enumerable.reverse.materializes-reversed-source-order",
            ReverseEnumerableMember,
            EnumerableModulePath,
            [Array(Number(1), Number(3), Number(1), Number(7))],
            Array(Number(7), Number(1), Number(3), Number(1))),
        new(
            "enumerable.reverse.array-overload-materializes-reversed-source-order",
            ReverseArrayMember,
            EnumerableModulePath,
            [Array(Number(1), Number(3), Number(1), Number(7))],
            Array(Number(7), Number(1), Number(3), Number(1))),
        new(
            "enumerable.reverse.rejects-null-source",
            ReverseEnumerableMember,
            EnumerableModulePath,
            [ClrRuntimeValue.Null()],
            ExpectedValue: null,
            ExpectedErrorContains: "ArgumentNullException")
    ];

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
