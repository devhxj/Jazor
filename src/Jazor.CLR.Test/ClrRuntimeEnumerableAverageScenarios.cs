namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableAverageScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.average.int", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", [Numbers(7, -2, 4)], Number(3)),
        Success("enumerable.average.int.fraction", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", [Numbers(1, 2)], Number(1.5)),
        Success("enumerable.average.int.max-pair", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", [Numbers(2147483647, 2147483647)], Number(2147483647)),
        Failure("enumerable.average.int.empty", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", [Array()], "InvalidOperationException"),
        Success("enumerable.average.long", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)", [BigInts(7, -2, 4)], Number(3)),
        Failure("enumerable.average.long.empty", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)", [Array()], "InvalidOperationException"),
        Failure("enumerable.average.long.overflow", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)", [BigInts(long.MaxValue, long.MaxValue)], "OverflowException"),
        Success("enumerable.average.single", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)", [Numbers(7, -2, 4)], Number(3)),
        Success("enumerable.average.single.nan", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)", [Array(Number(1), Number(double.NaN))], Number(double.NaN)),
        Failure("enumerable.average.single.empty", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)", [Array()], "InvalidOperationException"),
        Success("enumerable.average.double", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)", [Numbers(7, -2, 4)], Number(3)),
        Success("enumerable.average.double.nan", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)", [Array(Number(1), Number(double.NaN))], Number(double.NaN)),
        Failure("enumerable.average.double.empty", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)", [Array()], "InvalidOperationException"),
        Success("enumerable.average.decimal", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)", [Texts("3.25", "-1.50")], Text("0.875")),
        Failure("enumerable.average.decimal.empty", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)", [Array()], "InvalidOperationException"),
        Failure("enumerable.average.decimal.overflow", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)", [Texts("79228162514264337593543950335", "79228162514264337593543950335")], "OverflowException"),
        Failure("enumerable.average.int.null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", [Null()], "ArgumentNullException"),
        Failure("enumerable.average.decimal.null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)", [Null()], "ArgumentNullException")
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
