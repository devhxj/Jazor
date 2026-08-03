namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableNullableMinMaxScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.min.nullable-int.ignores-null", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Number(7), Number(-2))], Number(-2)),
        Success("enumerable.max.nullable-int.ignores-null", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Number(7), Number(-2))], Number(7)),
        Success("enumerable.min.nullable-int.all-null-is-null", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Null())], Null()),
        Success("enumerable.max.nullable-long.ignores-null", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), BigInt(7), BigInt(-2))], BigInt(7)),
        Success("enumerable.min.nullable-long.all-null-is-null", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), Null())], Null()),
        Success("enumerable.min.nullable-single.propagates-nan", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float?>)", [Array(Null(), Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.max.nullable-single.skips-nan", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float?>)", [Array(Null(), Number(double.NaN), Number(1))], Number(1)),
        Success("enumerable.min.nullable-double.propagates-nan", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double?>)", [Array(Null(), Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.max.nullable-double.skips-nan", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double?>)", [Array(Null(), Number(double.NaN), Number(1))], Number(1)),
        Success("enumerable.min.nullable-decimal.numeric-not-lexical", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Text("10"), Null(), Text("2"))], Text("2")),
        Success("enumerable.max.nullable-decimal.numeric-not-lexical", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Text("10"), Null(), Text("2"))], Text("10")),
        Success("enumerable.max.nullable-decimal.all-null-is-null", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Null(), Null())], Null()),
        Failure("enumerable.min.nullable-rejects-null-source", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)", [Null()], "ArgumentNullException"),
        Failure("enumerable.max.nullable-rejects-null-source", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)", [Null()], "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
