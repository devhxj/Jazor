namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableDefaultIfEmptyScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string DefaultIfEmptyMember =
        "static System.Linq.Enumerable.DefaultIfEmpty<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.default-if-empty.materializes-explicit-value", [Array(), Number(-1)], Array(Number(-1))),
        Success("enumerable.default-if-empty.preserves-non-empty-source", [Array(Number(2), Number(7)), Number(-1)], Array(Number(2), Number(7))),
        Failure("enumerable.default-if-empty.rejects-null-source", [Null(), Number(-1)], "ArgumentNullException: source is null")
    ];

    private static ClrRuntimeScenario Success(
        string id,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, DefaultIfEmptyMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string expectedError)
        => new(id, DefaultIfEmptyMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
