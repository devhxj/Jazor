namespace Jazor.CLR.Test;

internal static class ClrRuntimeEnumerableNullableNumericScenarios
{
    private const string ModulePath = "System/Linq/EnumerableModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("enumerable.sum.nullable-int.ignores-null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Number(7), Null(), Number(-2))], Number(5)),
        Success("enumerable.sum.nullable-int.all-null-is-zero", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Null())], Number(0)),
        Failure("enumerable.sum.nullable-int.overflow-after-null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Number(2147483647), Number(1))], "OverflowException"),
        Success("enumerable.sum.nullable-long.ignores-null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), BigInt(7), Null(), BigInt(-2))], BigInt(5)),
        Success("enumerable.sum.nullable-long.all-null-is-zero", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), Null())], BigInt(0)),
        Failure("enumerable.sum.nullable-long.overflow-after-null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), BigInt(long.MaxValue), BigInt(1))], "OverflowException"),
        Success("enumerable.sum.nullable-single.rounds-after-nulls", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float?>)", [Array(Null(), Number(1.5), Number(2.25))], Number(3.75)),
        Success("enumerable.sum.nullable-double.propagates-nan", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double?>)", [Array(Null(), Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.sum.nullable-decimal.ignores-null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Texts("3.25"), Null(), Texts("-1.50"))], Texts("1.75")),
        Success("enumerable.sum.nullable-decimal.all-null-is-zero", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Null(), Null())], Texts("0")),
        Failure("enumerable.sum.nullable-decimal.overflow-after-null", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Null(), Texts("79228162514264337593543950335"), Texts("1"))], "OverflowException"),
        Success("enumerable.average.nullable-int.ignores-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Number(1), Number(2))], Number(1.5)),
        Success("enumerable.average.nullable-int.all-null-is-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)", [Array(Null(), Null())], Null()),
        Success("enumerable.average.nullable-long.ignores-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), BigInt(7), BigInt(-2), Null())], Number(2.5)),
        Success("enumerable.average.nullable-long.all-null-is-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), Null())], Null()),
        Failure("enumerable.average.nullable-long.overflow-after-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)", [Array(Null(), BigInt(long.MaxValue), BigInt(long.MaxValue))], "OverflowException"),
        Success("enumerable.average.nullable-single.ignores-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float?>)", [Array(Null(), Number(1), Number(2))], Number(1.5)),
        Success("enumerable.average.nullable-double.propagates-nan", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double?>)", [Array(Null(), Number(1), Number(double.NaN))], Number(double.NaN)),
        Success("enumerable.average.nullable-decimal.ignores-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Texts("3.25"), Null(), Texts("-1.50"))], Texts("0.875")),
        Success("enumerable.average.nullable-decimal.all-null-is-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Null(), Null())], Null()),
        Failure("enumerable.average.nullable-decimal.overflow-after-null", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)", [Array(Null(), Texts("79228162514264337593543950335"), Texts("79228162514264337593543950335"))], "OverflowException"),
        Failure("enumerable.sum.nullable-rejects-null-source", "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)", [Null()], "ArgumentNullException"),
        Failure("enumerable.average.nullable-rejects-null-source", "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)", [Null()], "ArgumentNullException")
    ];

    private static ClrRuntimeScenario Success(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Texts(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
