namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableSkipTakeLastScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string SkipLastMember = "static System.Linq.Enumerable.SkipLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)";
    private const string TakeLastMember = "static System.Linq.Enumerable.TakeLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        SkipSuccess("enumerable.skip-last.preserves-prefix", [Array(Number(2), Number(7), Number(3), Number(9)), Number(2)], Array(Number(2), Number(7))),
        SkipSuccess("enumerable.skip-last.returns-empty-when-count-reaches-source", [Array(Number(2), Number(7)), Number(2)], Array()),
        SkipSuccess("enumerable.skip-last.accepts-zero-count", [Array(Number(2), Number(7)), Number(0)], Array(Number(2), Number(7))),
        SkipSuccess("enumerable.skip-last.accepts-negative-count", [Array(Number(2), Number(7)), Number(-1)], Array(Number(2), Number(7))),
        SkipFailure("enumerable.skip-last.rejects-null-source", [Null(), Number(2)], "ArgumentNullException: source is null"),
        TakeSuccess("enumerable.take-last.preserves-tail", [Array(Number(2), Number(7), Number(3), Number(9)), Number(2)], Array(Number(3), Number(9))),
        TakeSuccess("enumerable.take-last.returns-full-source-when-count-exceeds-length", [Array(Number(2), Number(7)), Number(3)], Array(Number(2), Number(7))),
        TakeSuccess("enumerable.take-last.accepts-zero-count", [Array(Number(2), Number(7)), Number(0)], Array()),
        TakeSuccess("enumerable.take-last.accepts-negative-count", [Array(Number(2), Number(7)), Number(-1)], Array()),
        TakeFailure("enumerable.take-last.rejects-null-source", [Null(), Number(2)], "ArgumentNullException: source is null")
    ];

    private static ClrRuntimeScenario SkipSuccess(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, SkipLastMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario SkipFailure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, SkipLastMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeScenario TakeSuccess(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, TakeLastMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario TakeFailure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, TakeLastMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
