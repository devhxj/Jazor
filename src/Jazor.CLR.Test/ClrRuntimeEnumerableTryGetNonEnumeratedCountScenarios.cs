namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableTryGetNonEnumeratedCountScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string Member = "static System.Linq.Enumerable.TryGetNonEnumeratedCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, out int)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "enumerable.try-get-non-enumerated-count.returns-array-carrier-length",
            [Array(Number(7), Number(9), Number(11)), Number(-1)],
            Array(Boolean(true), Number(3))),
        Success(
            "enumerable.try-get-non-enumerated-count.empty-array-has-known-zero-count",
            [Array(), Number(42)],
            Array(Boolean(true), Number(0))),
        Failure(
            "enumerable.try-get-non-enumerated-count.rejects-null-source",
            [Null(), Number(0)],
            "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(
        string id,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, Member, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string expectedError)
        => new(id, Member, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Boolean(bool value) => ClrRuntimeValue.Boolean(value);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
