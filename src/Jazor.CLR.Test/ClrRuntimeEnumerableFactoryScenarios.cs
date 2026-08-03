namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableFactoryScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string EmptyMember = "static System.Linq.Enumerable.Empty<TResult>()";
    private const string RangeMember = "static System.Linq.Enumerable.Range(int, int)";
    private const string RepeatMember = "static System.Linq.Enumerable.Repeat<TResult>(TResult, int)";
    private const string AsEnumerableMember = "static System.Linq.Enumerable.AsEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)";
    private const string SequenceMember = "static System.Linq.Enumerable.Sequence<T>(T, T, T)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.empty.returns-empty-carrier", EmptyMember, [], Array()),
        Success("enumerable.range.materializes-negative-start", RangeMember, [Number(-2), Number(3)], Array(Number(-2), Number(-1), Number(0))),
        Success("enumerable.range.accepts-zero-count-at-int32-max", RangeMember, [Number(2147483647), Number(0)], Array()),
        Success("enumerable.repeat.preserves-value-count", RepeatMember, [Number(7), Number(3)], Array(Number(7), Number(7), Number(7))),
        Success("enumerable.as-enumerable.preserves-null", AsEnumerableMember, [Null()], Null()),
        Success("enumerable.sequence.preserves-three-source-values", SequenceMember, [Number(7), Number(3), Number(9)], Array(Number(7), Number(3), Number(9))),
        Failure("enumerable.range.rejects-negative-count", RangeMember, [Number(0), Number(-1)], "ArgumentOutOfRangeException"),
        Failure("enumerable.range.rejects-int32-overflow", RangeMember, [Number(2147483647), Number(2)], "ArgumentOutOfRangeException"),
        Failure("enumerable.repeat.rejects-negative-count", RepeatMember, [Number(7), Number(-1)], "ArgumentOutOfRangeException")
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
}
