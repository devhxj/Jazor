namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableIndexScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string IndexMember = "static System.Linq.Enumerable.Index<TSource>(System.Collections.Generic.IEnumerable<TSource>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.index.materializes-named-tuples-in-source-order",
            [Array(Number(7), Number(3), Number(9))],
            Array(
                Record(("index", Number(0)), ("item", Number(7))),
                Record(("index", Number(1)), ("item", Number(3))),
                Record(("index", Number(2)), ("item", Number(9))))),
        Success("enumerable.index.empty-source", [Array()], Array()),
        Failure("enumerable.index.rejects-null-source", [Null()], "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, IndexMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, IndexMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
