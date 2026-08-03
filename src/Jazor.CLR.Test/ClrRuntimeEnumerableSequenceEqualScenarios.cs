namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableSequenceEqualScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string SequenceEqualMember = "static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.sequence-equal.matches-equal-values", [Array(Number(3), Number(1), Number(4)), Array(Number(3), Number(1), Number(4))], Bool(true)),
        Success("enumerable.sequence-equal.rejects-different-lengths", [Array(Number(3), Number(1)), Array(Number(3), Number(1), Number(4))], Bool(false)),
        Success("enumerable.sequence-equal.rejects-first-mismatch", [Array(Number(3), Number(1), Number(4)), Array(Number(3), Number(2), Number(4))], Bool(false)),
        Success("enumerable.sequence-equal.uses-default-equality-for-nan-and-signed-zero", [Array(Number(double.NaN), Number(-0.0)), Array(Number(double.NaN), Number(0.0))], Bool(true)),
        Failure("enumerable.sequence-equal.rejects-null-first", [Null(), Array()], "ArgumentNullException: first is null"),
        Failure("enumerable.sequence-equal.rejects-null-second", [Array(), Null()], "ArgumentNullException: second is null")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, SequenceEqualMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, SequenceEqualMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
