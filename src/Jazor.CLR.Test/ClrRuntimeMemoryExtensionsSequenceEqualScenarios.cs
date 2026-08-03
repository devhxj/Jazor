namespace Jazor.CLR.Test;

internal static class ClrRuntimeMemoryExtensionsSequenceEqualScenarios
{
    private const string ModulePath = "System/MemoryExtensionsModule.js";
    private const string Member = "System.ReadOnlySpan<T>.SequenceEqual<T>(System.ReadOnlySpan<T>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("memory-extensions.sequence-equal.matches-aligned-values", [Array(Number(2), Number(7), Number(2)), Array(Number(2), Number(7), Number(2))], Bool(true)),
        Success("memory-extensions.sequence-equal.rejects-different-lengths", [Array(Number(2), Number(7)), Array(Number(2), Number(7), Number(2))], Bool(false)),
        Success("memory-extensions.sequence-equal.uses-default-equality-for-nan-and-signed-zero", [Array(Number(double.NaN), Number(-0.0)), Array(Number(double.NaN), Number(0.0))], Bool(true)),
        Failure("memory-extensions.sequence-equal.rejects-null-first", [Null(), Array()], "ArgumentNullException: first is null"),
        Failure("memory-extensions.sequence-equal.rejects-null-second", [Array(), Null()], "ArgumentNullException: second is null")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, Member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, Member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
