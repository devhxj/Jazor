namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableWhileScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string SkipWhileMember = "static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";
    private const string SkipWhileAtMember = "static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)";
    private const string TakeWhileMember = "static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";
    private const string TakeWhileAtMember = "static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.skip-while.stops-calling-predicate-after-first-false", SkipWhileMember, [Array(Number(2), Number(4), Number(1), Number(6)), Callable(ClrRuntimeCallableKind.IsEven)], Array(Number(1), Number(6))),
        Success("enumerable.skip-while-index.uses-source-index-until-first-false", SkipWhileAtMember, [Array(Number(10), Number(20), Number(30), Number(40)), Callable(ClrRuntimeCallableKind.IsEvenIndex)], Array(Number(20), Number(30), Number(40))),
        Success("enumerable.take-while.stops-at-first-false", TakeWhileMember, [Array(Number(2), Number(4), Number(1), Number(6)), Callable(ClrRuntimeCallableKind.IsEven)], Array(Number(2), Number(4))),
        Success("enumerable.take-while-index.uses-source-index-through-failing-item", TakeWhileAtMember, [Array(Number(10), Number(20), Number(30)), Callable(ClrRuntimeCallableKind.IsEvenIndex)], Array(Number(10))),
        Failure("enumerable.skip-while.rejects-null-source", SkipWhileMember, [Null(), Callable(ClrRuntimeCallableKind.IsEven)], "ArgumentNullException: source is null"),
        Failure("enumerable.skip-while-index.rejects-null-predicate", SkipWhileAtMember, [Array(Number(1)), Null()], "ArgumentNullException: predicate is null"),
        Failure("enumerable.take-while.rejects-null-source", TakeWhileMember, [Null(), Callable(ClrRuntimeCallableKind.IsEven)], "ArgumentNullException: source is null"),
        Failure("enumerable.take-while-index.rejects-null-predicate", TakeWhileAtMember, [Array(Number(1)), Null()], "ArgumentNullException: predicate is null")
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
