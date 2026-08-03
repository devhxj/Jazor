namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableChunkScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string ChunkMember = "static System.Linq.Enumerable.Chunk<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.chunk.preserves-order-and-tail", [Array(Number(2), Number(7), Number(3), Number(9), Number(4)), Number(2)], Array(Array(Number(2), Number(7)), Array(Number(3), Number(9)), Array(Number(4)))),
        Success("enumerable.chunk.accepts-exact-groups", [Array(Number(2), Number(7), Number(3), Number(9)), Number(2)], Array(Array(Number(2), Number(7)), Array(Number(3), Number(9)))),
        Success("enumerable.chunk.accepts-single-item-size", [Array(Number(2), Number(7)), Number(1)], Array(Array(Number(2)), Array(Number(7)))),
        Failure("enumerable.chunk.rejects-null-source", [Null(), Number(2)], "ArgumentNullException: source is null"),
        Failure("enumerable.chunk-rejects-zero-size", [Array(Number(2)), Number(0)], "ArgumentOutOfRangeException: size must be greater than zero")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, ChunkMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, ChunkMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
