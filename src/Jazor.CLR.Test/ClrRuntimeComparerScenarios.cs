namespace Jazor.CLR.Test;

internal static class ClrRuntimeComparerScenarios
{
    private const string EqualityComparerModulePath = "System/Collections/Generic/EqualityComparerT1Module.js";
    private const string GenericEqualityComparerModulePath = "System/Collections/Generic/IEqualityComparerT1Module.js";
    private const string NonGenericEqualityComparerModulePath = "System/Collections/IEqualityComparerModule.js";
    private const string ComparerModulePath = "System/Collections/Generic/ComparerT1Module.js";
    private const string GenericComparerModulePath = "System/Collections/Generic/IComparerT1Module.js";
    private const string NonGenericComparerModulePath = "System/Collections/IComparerModule.js";
    private const string EqualityDefaultMember = "static System.Collections.Generic.EqualityComparer<T>.Default.get";
    private const string ComparerDefaultMember = "static System.Collections.Generic.Comparer<T>.Default.get";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("equality-comparer.equals.nan-values", "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)", EqualityComparerModulePath, [DefaultEquality(), Number(double.NaN), Number(double.NaN)], Bool(true)),
        Success("equality-comparer.equals.distinct-numbers", "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)", EqualityComparerModulePath, [DefaultEquality(), Number(3), Number(4)], Bool(false)),
        Success("equality-comparer.equals.distinct-records", "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)", EqualityComparerModulePath, [DefaultEquality(), Record(("id", Number(1))), Record(("id", Number(1)))], Bool(false)),
        Failure("equality-comparer.equals.rejects-null-receiver", "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)", EqualityComparerModulePath, [Null(), Number(3), Number(3)], "NullReferenceException"),
        Success("equality-comparer.hash-code.string", "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", EqualityComparerModulePath, [DefaultEquality(), Text("A")], Number(592)),
        Success("equality-comparer.hash-code.boolean", "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", EqualityComparerModulePath, [DefaultEquality(), Bool(true)], Number(1)),
        Success("equality-comparer.hash-code.integral-number", "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", EqualityComparerModulePath, [DefaultEquality(), Number(42)], Number(42)),
        Success("equality-comparer.hash-code.fractional-number", "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", EqualityComparerModulePath, [DefaultEquality(), Number(1.5)], Number(555015)),
        Success("equality-comparer.hash-code.nan", "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", EqualityComparerModulePath, [DefaultEquality(), Number(double.NaN)], Number(0)),

        Success("generic-equality-comparer.equals.signed-zero", "System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)", GenericEqualityComparerModulePath, [DefaultEquality(), Number(-0.0), Number(0.0)], Bool(true)),
        Failure("generic-equality-comparer.equals.rejects-null-receiver", "System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)", GenericEqualityComparerModulePath, [Null(), Number(3), Number(3)], "NullReferenceException"),
        Success("generic-equality-comparer.hash-code.null", "System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)", GenericEqualityComparerModulePath, [DefaultEquality(), Null()], Number(0)),

        Success("non-generic-equality-comparer.equals.equal-strings", "System.Collections.IEqualityComparer.Equals(object, object)", NonGenericEqualityComparerModulePath, [DefaultEquality(), Text("release"), Text("release")], Bool(true)),
        Success("non-generic-equality-comparer.equals.distinct-strings", "System.Collections.IEqualityComparer.Equals(object, object)", NonGenericEqualityComparerModulePath, [DefaultEquality(), Text("release"), Text("owner")], Bool(false)),
        Success("non-generic-equality-comparer.hash-code.bigint", "System.Collections.IEqualityComparer.GetHashCode(object)", NonGenericEqualityComparerModulePath, [DefaultEquality(), BigInt(17)], Number(17911)),

        Success("comparer.compare.nan-is-before-number", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [DefaultComparer(), Number(double.NaN), Number(2)], Number(-1)),
        Success("comparer.compare.number-is-after-nan", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [DefaultComparer(), Number(2), Number(double.NaN)], Number(1)),
        Success("comparer.compare.boolean-order", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [DefaultComparer(), Bool(false), Bool(true)], Number(-1)),
        Success("comparer.compare-bigint-order", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [DefaultComparer(), BigInt(19), BigInt(17)], Number(1)),
        Failure("comparer.compare.rejects-unrelated-records", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [DefaultComparer(), Record(("id", Number(1))), Record(("id", Number(2)))], "ArgumentException"),
        Failure("comparer.compare.rejects-null-receiver", "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", ComparerModulePath, [Null(), Number(3), Number(3)], "NullReferenceException"),
        Success("generic-comparer.compare-strings", "System.Collections.Generic.IComparer<T>.Compare(T, T)", GenericComparerModulePath, [DefaultComparer(), Text("release"), Text("stage")], Number(-1)),
        Failure("generic-comparer.compare.rejects-null-receiver", "System.Collections.Generic.IComparer<T>.Compare(T, T)", GenericComparerModulePath, [Null(), Text("release"), Text("stage")], "NullReferenceException"),
        Success("non-generic-comparer.compare-null-before-value", "System.Collections.IComparer.Compare(object, object)", NonGenericComparerModulePath, [DefaultComparer(), Null(), Text("release")], Number(-1))
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue DefaultEquality() => ClrRuntimeValue.Invoke(EqualityDefaultMember);
    private static ClrRuntimeValue DefaultComparer() => ClrRuntimeValue.Invoke(ComparerDefaultMember);
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue BigInt(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
}
