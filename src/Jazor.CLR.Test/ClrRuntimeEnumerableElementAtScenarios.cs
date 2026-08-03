namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableElementAtScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string ElementAtMember = "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)";
    private const string ElementAtIndexMember = "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.element-at.reads-bound-index", [Array(Number(2), Number(7), Number(9)), Number(1)], Number(7)),
        Success("enumerable.element-at.reads-first-index", [Array(Number(2), Number(7), Number(9)), Number(0)], Number(2)),
        Failure("enumerable.element-at.rejects-null-source", [Null(), Number(0)], "ArgumentNullException: source is null"),
        Failure("enumerable.element-at.rejects-negative-index", [Array(Number(2), Number(7)), Number(-1)], "ArgumentOutOfRangeException: index is less than zero"),
        Failure("enumerable.element-at.rejects-index-after-source", [Array(Number(2), Number(7)), Number(2)], "ArgumentOutOfRangeException: index is out of range."),
        IndexSuccess("enumerable.element-at-index.from-start", [Array(Number(2), Number(7), Number(9)), IndexFromStart(1)], Number(7)),
        IndexSuccess("enumerable.element-at-index.from-end", [Array(Number(2), Number(7), Number(9)), IndexFromEnd(2)], Number(7)),
        IndexSuccess("enumerable.element-at-index.from-end-source-length", [Array(Number(2), Number(7), Number(9)), IndexFromEnd(3)], Number(2)),
        IndexFailure("enumerable.element-at-index.rejects-end", [Array(Number(2), Number(7)), IndexFromEnd(0)], "ArgumentOutOfRangeException: index is out of range."),
        IndexFailure("enumerable.element-at-index.rejects-from-start-after-source", [Array(Number(2), Number(7)), IndexFromStart(2)], "ArgumentOutOfRangeException: index is out of range."),
        IndexFailure("enumerable.element-at-index.rejects-from-end-after-source", [Array(Number(2), Number(7)), IndexFromEnd(3)], "ArgumentOutOfRangeException: index is out of range."),
        IndexFailure("enumerable.element-at-index.rejects-null-source", [Null(), IndexFromEnd(1)], "ArgumentNullException: source is null")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, ElementAtMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, ElementAtMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeScenario IndexSuccess(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, ElementAtIndexMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario IndexFailure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, ElementAtIndexMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue IndexFromStart(double value)
        => ClrRuntimeValue.Invoke("static System.Index.FromStart(int)", [Number(value)]);

    private static ClrRuntimeValue IndexFromEnd(double value)
        => ClrRuntimeValue.Invoke("static System.Index.FromEnd(int)", [Number(value)]);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
