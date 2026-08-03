namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableTerminalScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string FirstMember = "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>)";
    private const string FirstPredicateMember = "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";
    private const string LastMember = "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>)";
    private const string LastPredicateMember = "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";
    private const string SingleMember = "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>)";
    private const string SinglePredicateMember = "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.first.returns-first-source-item", FirstMember, [Array(Number(3), Number(1), Number(4))], Number(3)),
        Failure("enumerable.first.rejects-empty-source", FirstMember, [Array()], "InvalidOperationException"),
        Failure("enumerable.first.rejects-null-source", FirstMember, [Null()], "ArgumentNullException"),
        Success("enumerable.first.predicate-returns-first-match", FirstPredicateMember, [Array(Number(1), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)], Number(2)),
        Failure("enumerable.first.predicate-rejects-no-match", FirstPredicateMember, [Array(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], "InvalidOperationException"),
        Failure("enumerable.first.predicate-rejects-null-predicate", FirstPredicateMember, [Array(Number(1)), Null()], "ArgumentNullException"),
        Success("enumerable.last.returns-last-source-item", LastMember, [Array(Number(3), Number(1), Number(4))], Number(4)),
        Failure("enumerable.last.rejects-empty-source", LastMember, [Array()], "InvalidOperationException"),
        Failure("enumerable.last.rejects-null-source", LastMember, [Null()], "ArgumentNullException"),
        Success("enumerable.last.predicate-returns-last-match", LastPredicateMember, [Array(Number(1), Number(2), Number(4), Number(5)), Callable(ClrRuntimeCallableKind.IsEven)], Number(4)),
        Failure("enumerable.last.predicate-rejects-no-match", LastPredicateMember, [Array(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], "InvalidOperationException"),
        Failure("enumerable.last.predicate-rejects-null-predicate", LastPredicateMember, [Array(Number(1)), Null()], "ArgumentNullException"),
        Success("enumerable.single.returns-only-source-item", SingleMember, [Array(Number(7))], Number(7)),
        Failure("enumerable.single.rejects-empty-source", SingleMember, [Array()], "InvalidOperationException"),
        Failure("enumerable.single.rejects-multiple-source-items", SingleMember, [Array(Number(7), Number(9))], "InvalidOperationException"),
        Failure("enumerable.single.rejects-null-source", SingleMember, [Null()], "ArgumentNullException"),
        Success("enumerable.single.predicate-returns-only-match", SinglePredicateMember, [Array(Number(1), Number(2), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], Number(2)),
        Failure("enumerable.single.predicate-rejects-no-match", SinglePredicateMember, [Array(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)], "InvalidOperationException"),
        Failure("enumerable.single.predicate-rejects-multiple-matches", SinglePredicateMember, [Array(Number(1), Number(2), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)], "InvalidOperationException"),
        Failure("enumerable.single.predicate-rejects-null-source", SinglePredicateMember, [Null(), Callable(ClrRuntimeCallableKind.IsEven)], "ArgumentNullException"),
        Failure("enumerable.single.predicate-rejects-null-predicate", SinglePredicateMember, [Array(Number(1)), Null()], "ArgumentNullException")
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
