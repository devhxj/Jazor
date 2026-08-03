namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableLongCountScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string LongCountMember = "static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>)";
    private const string LongCountWhereMember = "static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.long-count.uses-bigint-carrier", LongCountMember, [Array(Number(1), Number(2), Number(3))], BigInt(3)),
        Success("enumerable.long-count.predicate-counts-only-matches", LongCountWhereMember, [Array(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)], BigInt(2)),
        Failure("enumerable.long-count.rejects-null-source", LongCountMember, [Null()], "ArgumentNullException"),
        Failure("enumerable.long-count.predicate-rejects-null", LongCountWhereMember, [Array(Number(1)), Null()], "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string expectedError)
        => new(id, member, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
