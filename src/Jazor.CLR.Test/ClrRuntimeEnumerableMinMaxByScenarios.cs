namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableMinMaxByScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string MinByMember = "static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";
    private const string MaxByMember = "static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.min-by.selects-first-minimum-key", MinByMember, [Array(Number(2), Number(7), Number(4), Number(9)), Callable(ClrRuntimeCallableKind.IsEven)], Number(7)),
        Success("enumerable.max-by.selects-first-maximum-key", MaxByMember, [Array(Number(2), Number(7), Number(4), Number(9)), Callable(ClrRuntimeCallableKind.IsEven)], Number(2)),
        Failure("enumerable.min-by.rejects-empty-source", MinByMember, [Array(), Callable(ClrRuntimeCallableKind.IsEven)], "InvalidOperationException: Sequence contains no elements"),
        Failure("enumerable.max-by.rejects-empty-source", MaxByMember, [Array(), Callable(ClrRuntimeCallableKind.IsEven)], "InvalidOperationException: Sequence contains no elements"),
        Failure("enumerable.min-by.rejects-null-source", MinByMember, [Null(), Callable(ClrRuntimeCallableKind.IsEven)], "ArgumentNullException: source is null"),
        Failure("enumerable.max-by.rejects-null-key-selector", MaxByMember, [Array(Number(2)), Null()], "ArgumentNullException: keySelector is null")
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
