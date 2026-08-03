namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableAppendPrependScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string AppendMember = "static System.Linq.Enumerable.Append<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)";
    private const string PrependMember = "static System.Linq.Enumerable.Prepend<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.append.materializes-source-before-element", AppendMember, [Array(Number(2), Number(7)), Number(9)], Array(Number(2), Number(7), Number(9))),
        Success("enumerable.append.accepts-empty-source", AppendMember, [Array(), Number(9)], Array(Number(9))),
        Failure("enumerable.append.rejects-null-source", AppendMember, [Null(), Number(9)], "ArgumentNullException: source is null"),
        Success("enumerable.prepend.materializes-element-before-source", PrependMember, [Array(Number(2), Number(7)), Number(1)], Array(Number(1), Number(2), Number(7))),
        Success("enumerable.prepend.accepts-empty-source", PrependMember, [Array(), Number(1)], Array(Number(1))),
        Failure("enumerable.prepend.rejects-null-source", PrependMember, [Null(), Number(1)], "ArgumentNullException: source is null")
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
