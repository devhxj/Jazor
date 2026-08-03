namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableMinMaxScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.min.int", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int>)", [Numbers(7, -2, 4)], Number(-2)),
        Success("enumerable.max.int", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int>)", [Numbers(7, -2, 4)], Number(7)),
        Success("enumerable.min.long", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long>)", [BigInts(7, -2, 4)], BigInt(-2)),
        Success("enumerable.max.long", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long>)", [BigInts(7, -2, 4)], BigInt(7)),
        Success("enumerable.min.single.nan", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float>)", [Array(Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.max.single.ignores-nan", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float>)", [Array(Number(1), Number(double.NaN))], Number(1)),
        Success("enumerable.min.double.nan", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double>)", [Array(Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.max.double.ignores-nan", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double>)", [Array(Number(1), Number(double.NaN))], Number(1)),
        Success("enumerable.min.decimal", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal>)", [Texts("3.25", "-1.50")], Text("-1.50")),
        Success("enumerable.max.decimal", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)", [Texts("3.25", "-1.50")], Text("3.25")),
        Success("enumerable.min.decimal.numeric-not-lexical", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal>)", [Texts("10", "2")], Text("2")),
        Success("enumerable.max.decimal.numeric-not-lexical", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)", [Texts("10", "2")], Text("10")),
        Failure("enumerable.min.int.empty", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int>)", [Array()], "InvalidOperationException"),
        Failure("enumerable.max.long.empty", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long>)", [Array()], "InvalidOperationException"),
        Failure("enumerable.min.double.null", "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double>)", [Null()], "ArgumentNullException"),
        Failure("enumerable.max.decimal.null", "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)", [Null()], "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, string expectedError)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedError);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);

    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);

    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);

    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);

    private static ClrRuntimeValue Numbers(params double[] values) => Array(values.Select(static value => Number(value)).ToArray());

    private static ClrRuntimeValue BigInts(params long[] values) => Array(values.Select(static value => BigInt(value)).ToArray());

    private static ClrRuntimeValue Texts(params string[] values) => Array(values.Select(static value => Text(value)).ToArray());
}
