namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableDefaultTerminalScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string FirstMember =
        "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)";
    private const string FirstWhereMember =
        "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)";
    private const string LastMember =
        "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)";
    private const string LastWhereMember =
        "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)";
    private const string SingleMember =
        "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)";
    private const string SingleWhereMember =
        "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.first-or-default.empty-uses-fallback", FirstMember, [Array(), Number(-1)], Number(-1)),
        Success("enumerable.first-or-default.stops-at-first-item", FirstMember, [Array(Number(2), Number(7)), Number(-1)], Number(2)),
        Success("enumerable.first-or-default.where-no-match-uses-fallback", FirstWhereMember, [Array(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], Number(-1)),
        Success("enumerable.first-or-default.where-stops-at-first-match", FirstWhereMember, [Array(Number(1), Number(4), Number(6)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], Number(4)),
        Success("enumerable.last-or-default.empty-uses-fallback", LastMember, [Array(), Number(-1)], Number(-1)),
        Success("enumerable.last-or-default.returns-final-item", LastMember, [Array(Number(2), Number(7)), Number(-1)], Number(7)),
        Success("enumerable.last-or-default.where-no-match-uses-fallback", LastWhereMember, [Array(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], Number(-1)),
        Success("enumerable.last-or-default.where-observes-final-match", LastWhereMember, [Array(Number(2), Number(4), Number(7)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], Number(4)),
        Success("enumerable.single-or-default.empty-uses-fallback", SingleMember, [Array(), Number(-1)], Number(-1)),
        Success("enumerable.single-or-default.returns-only-item", SingleMember, [Array(Number(7)), Number(-1)], Number(7)),
        Failure("enumerable.single-or-default.rejects-multiple-items", SingleMember, [Array(Number(2), Number(7)), Number(-1)], "InvalidOperationException: Sequence contains more than one element"),
        Success("enumerable.single-or-default.where-no-match-uses-fallback", SingleWhereMember, [Array(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], Number(-1)),
        Success("enumerable.single-or-default.where-returns-one-match", SingleWhereMember, [Array(Number(1), Number(4), Number(7)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], Number(4)),
        Failure("enumerable.single-or-default.where-rejects-multiple-matches", SingleWhereMember, [Array(Number(2), Number(4), Number(7)), Callable(ClrRuntimeCallableKind.IsEven), Number(-1)], "InvalidOperationException: Sequence contains more than one matching element")
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

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
