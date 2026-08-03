namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableConcatScenarios
{
    private const string EnumerableModulePath = "System/Linq/EnumerableModule.js";
    private const string ConcatMember = "static System.Linq.Enumerable.Concat<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.concat.preserves-first-then-second-order", [Array(Number(2), Number(7)), Array(Number(3), Number(9))], Array(Number(2), Number(7), Number(3), Number(9))),
        Success("enumerable.concat.accepts-empty-first", [Array(), Array(Number(3), Number(9))], Array(Number(3), Number(9))),
        Success("enumerable.concat.accepts-empty-second", [Array(Number(2), Number(7)), Array()], Array(Number(2), Number(7))),
        Failure("enumerable.concat.rejects-null-first", [Null(), Array()], "ArgumentNullException: first is null"),
        Failure("enumerable.concat.rejects-null-second", [Array(), Null()], "ArgumentNullException: second is null")
    ];

    private static ClrRuntimeScenario Success(string id, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, ConcatMember, EnumerableModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, ConcatMember, EnumerableModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
