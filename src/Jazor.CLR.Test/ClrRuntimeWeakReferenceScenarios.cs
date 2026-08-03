namespace Jazor.CLR.Test;

internal static class ClrRuntimeWeakReferenceScenarios
{
    private const string ModulePath = "System/WeakReferenceModule.js";
    private const string ConstructorMember = "System.WeakReference.WeakReference(object)";
	private const string TrackingConstructorMember = "System.WeakReference.WeakReference(object, bool)";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "weak-reference.target.get-object",
            "virtual System.WeakReference.Target.get",
            [Invoke(ConstructorMember, Record(("name", Text("release"))))],
            Record(("name", Text("release")))),
        Success(
            "weak-reference.is-alive-object",
            "virtual System.WeakReference.IsAlive.get",
            [Invoke(ConstructorMember, Record(("name", Text("release"))))],
            Bool(true)),
        Success(
            "weak-reference.target.set-replaces-object-target",
            "virtual System.WeakReference.Target.get",
            [Sequence(
                Reference("reference", Invoke(ConstructorMember, Record(("name", Text("draft"))))),
                Invoke("virtual System.WeakReference.Target.set", Reference("reference", Null()), Record(("name", Text("release")))),
                Reference("reference", Null()))],
            Record(("name", Text("release")))),
        Success(
            "weak-reference.target.set-null-clears-liveness",
            "virtual System.WeakReference.IsAlive.get",
            [Sequence(
                Reference("reference", Invoke(ConstructorMember, Text("draft"))),
                Invoke("virtual System.WeakReference.Target.set", Reference("reference", Null()), Null()),
                Reference("reference", Null()))],
            Bool(false)),
        Success(
            "weak-reference.target.get-primitive",
            "virtual System.WeakReference.Target.get",
            [Invoke(ConstructorMember, Number(42))],
			Number(42)),
		Success(
			"weak-reference.constructor-without-resurrection-tracking",
			"virtual System.WeakReference.Target.get",
			[Invoke(TrackingConstructorMember, Record(("name", Text("release"))), Bool(false))],
			Record(("name", Text("release")))),
		Failure(
			"weak-reference.constructor-rejects-resurrection-tracking",
			TrackingConstructorMember,
			[Record(("name", Text("release"))), Bool(true)],
			"NotSupportedException")
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ModulePath, arguments, expected);

	private static ClrRuntimeScenario Failure(
		string id,
		string member,
		IReadOnlyList<ClrRuntimeValue> arguments,
		string error)
		=> new(id, member, ModulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties) => ClrRuntimeValue.Record(properties);
    private static ClrRuntimeValue Reference(string id, ClrRuntimeValue value) => ClrRuntimeValue.Reference(id, value);
    private static ClrRuntimeValue Sequence(params ClrRuntimeValue[] steps) => ClrRuntimeValue.Sequence(steps);
    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments) => ClrRuntimeValue.Invoke(member, arguments);
}
