namespace Jazor.CLR.Test;

internal static class ClrRuntimeComparerScenarios
{
    private const string EqualityComparerModulePath = "System/Collections/Generic/EqualityComparerT1Module.js";
    private const string GenericEqualityComparerModulePath = "System/Collections/Generic/IEqualityComparerT1Module.js";
    private const string NonGenericEqualityComparerModulePath = "System/Collections/IEqualityComparerModule.js";
    private const string ComparerModulePath = "System/Collections/Generic/ComparerT1Module.js";
    private const string GenericComparerModulePath = "System/Collections/Generic/IComparerT1Module.js";
    private const string NonGenericComparerModulePath = "System/Collections/IComparerModule.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("equality-comparer.equals.nan-values", "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)", EqualityComparerModulePath, [Record(), Number(double.NaN), Number(double.NaN)], Bool(true)),
        Success("equality-comparer.hash-code.string", "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", EqualityComparerModulePath, [Record(), Text("A")], Number(592)),

        Success("generic-equality-comparer.equals.signed-zero", "System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)", GenericEqualityComparerModulePath, [Record(), Number(-0.0), Number(0.0)], Bool(true)),
        Success("generic-equality-comparer.hash-code.null", "System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)", GenericEqualityComparerModulePath, [Record(), Null()], Number(0)),

        Success("non-generic-equality-comparer.equals.equal-strings", "System.Collections.IEqualityComparer.Equals(object, object)", NonGenericEqualityComparerModulePath, [Record(), Text("release"), Text("release")], Bool(true)),
        Success("non-generic-equality-comparer.hash-code.bigint", "System.Collections.IEqualityComparer.GetHashCode(object)", NonGenericEqualityComparerModulePath, [Record(), BigInt(17)], Number(17911)),

        Success("comparer.compare.nan-is-after-number", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [Record(), Number(double.NaN), Number(2)], Number(1)),
        Success("generic-comparer.compare-strings", "System.Collections.Generic.IComparer<T>.Compare(T, T)", GenericComparerModulePath, [Record(), Text("release"), Text("stage")], Number(-1)),
        Success("non-generic-comparer.compare-null-before-value", "System.Collections.IComparer.Compare(object, object)", NonGenericComparerModulePath, [Record(), Null(), Text("release")], Number(-1))
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Record() => ClrRuntimeValue.Record();
}
