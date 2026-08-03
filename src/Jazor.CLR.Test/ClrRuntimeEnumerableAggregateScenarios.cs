namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableAggregateScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string AggregateMember = "static System.Linq.Enumerable.Aggregate<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TSource, TSource>)";
    private const string AggregateSeedMember = "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>)";
    private const string AggregateResultMember = "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Func<TAccumulate, TResult>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.aggregate.folds-from-first-source-item", AggregateMember, [Array(Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.AddNumbers)], Number(9)),
        Failure("enumerable.aggregate.rejects-empty-source", AggregateMember, [Array(), Callable(ClrRuntimeCallableKind.AddNumbers)], "InvalidOperationException"),
        Failure("enumerable.aggregate.rejects-null-source", AggregateMember, [Null(), Callable(ClrRuntimeCallableKind.AddNumbers)], "ArgumentNullException"),
        Failure("enumerable.aggregate.rejects-null-function", AggregateMember, [Array(Number(2)), Null()], "ArgumentNullException"),
        Success("enumerable.aggregate.seed-folds-every-source-item", AggregateSeedMember, [Array(Number(2), Number(3), Number(4)), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers)], Number(19)),
        Success("enumerable.aggregate.seed-returns-seed-for-empty-source", AggregateSeedMember, [Array(), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers)], Number(10)),
        Failure("enumerable.aggregate.seed-rejects-null-source", AggregateSeedMember, [Null(), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers)], "ArgumentNullException"),
        Failure("enumerable.aggregate.seed-rejects-null-function", AggregateSeedMember, [Array(Number(2)), Number(10), Null()], "ArgumentNullException"),
        Success("enumerable.aggregate.result-applies-result-selector-after-fold", AggregateResultMember, [Array(Number(2), Number(3)), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(30)),
        Success("enumerable.aggregate.result-applies-result-selector-for-empty-source", AggregateResultMember, [Array(), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers), Callable(ClrRuntimeCallableKind.DoubleNumber)], Number(20)),
        Failure("enumerable.aggregate.result-rejects-null-result-selector", AggregateResultMember, [Array(Number(2)), Number(10), Callable(ClrRuntimeCallableKind.AddNumbers), Null()], "ArgumentNullException")
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

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
