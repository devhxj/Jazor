namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableSumScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.sum.int", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", [Numbers(7, -2, 4)], Number(9)),
        Success("enumerable.sum.int.empty", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", [Array()], Number(0)),
        Failure("enumerable.sum.int.positive-overflow", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", [Numbers(2147483647, 1)], "OverflowException"),
        Failure("enumerable.sum.int.negative-overflow", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", [Numbers(-2147483648, -1)], "OverflowException"),
        Success("enumerable.sum.long", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)", [BigInts(7, -2, 4)], BigInt(9)),
        Success("enumerable.sum.long.empty", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)", [Array()], BigInt(0)),
        Failure("enumerable.sum.long.overflow", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)", [BigInts(long.MaxValue, 1)], "OverflowException"),
        Success("enumerable.sum.single.empty", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)", [Array()], Number(0)),
        Success("enumerable.sum.single.nan", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)", [Array(Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.sum.double", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)", [Numbers(7, -2, 4)], Number(9)),
        Success("enumerable.sum.double.nan", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)", [Array(Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.sum.decimal.empty", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)", [Array()], Text("0")),
        Success("enumerable.sum.decimal", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)", [Texts("3.25", "-1.50")], Text("1.75")),
        Failure("enumerable.sum.decimal.overflow", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)", [Texts("79228162514264337593543950335", "1")], "OverflowException"),
        Failure("enumerable.sum.int.null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", [Null()], "ArgumentNullException"),
        Failure("enumerable.sum.decimal.null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)", [Null()], "ArgumentNullException")
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
