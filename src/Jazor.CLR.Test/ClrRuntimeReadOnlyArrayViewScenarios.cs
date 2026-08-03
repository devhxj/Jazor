namespace Jazor.CLR.Test;

internal static class ClrRuntimeReadOnlyArrayViewScenarios
{
	private const string ArrayModulePath = "System/ArrayModule.js";
	private const string ListModulePath = "System/Collections/Generic/ListT1Module.js";
	private const string ReadOnlyCollectionModulePath = "System/Collections/ObjectModel/ReadOnlyCollectionT1Module.js";
	private const string ListAddMember = "System.Collections.Generic.List<T>.Add(T)";
	private const string ReadOnlyIndexerMember = "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get";

	public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
	[
		LiveView(
			"array.as-read-only.live-view",
			"static System.Array.AsReadOnly<T>(T[])",
			ArrayModulePath),
		LiveView(
			"list.as-read-only.live-view",
			"System.Collections.Generic.List<T>.AsReadOnly()",
			ListModulePath),
		LiveView(
			"read-only-collection.constructor.live-view",
			"System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)",
			ReadOnlyCollectionModulePath),
		Failure(
			"list.as-read-only.rejects-view-mutation",
			ListAddMember,
			ListModulePath,
			[Invoke("System.Collections.Generic.List<T>.AsReadOnly()", Array(Number(1))), Number(2)],
			"NotSupportedException: Collection is read-only."),
		Failure(
			"array.as-read-only.rejects-null",
			"static System.Array.AsReadOnly<T>(T[])",
			ArrayModulePath,
			[Null()],
			"ArgumentNullException"),
		Failure(
			"read-only-collection.constructor.rejects-null",
			"System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)",
			ReadOnlyCollectionModulePath,
			[Null()],
			"ArgumentNullException")
	];

	private static ClrRuntimeScenario LiveView(string id, string factoryMember, string modulePath)
	{
		var source = Reference(id + ".source", Array(Number(1)));
		var view = Reference(id + ".view", Invoke(factoryMember, source));
		return Success(
			id,
			ReadOnlyIndexerMember,
			ReadOnlyCollectionModulePath,
			[
				Sequence(
					source,
					view,
					Invoke(ListAddMember, source, Number(2)),
					view),
				Number(1)
			],
			Number(2));
	}

	private static ClrRuntimeScenario Success(
		string id,
		string member,
		string modulePath,
		IReadOnlyList<ClrRuntimeValue> arguments,
		ClrRuntimeValue expected)
		=> new(id, member, modulePath, arguments, expected);

	private static ClrRuntimeScenario Failure(
		string id,
		string member,
		string modulePath,
		IReadOnlyList<ClrRuntimeValue> arguments,
		string expectedError)
		=> new(id, member, modulePath, arguments, null, expectedError);

	private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
	private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
	private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
	private static ClrRuntimeValue Reference(string id, ClrRuntimeValue value) => ClrRuntimeValue.Reference(id, value);
	private static ClrRuntimeValue Sequence(params ClrRuntimeValue[] steps) => ClrRuntimeValue.Sequence(steps);
	private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments) => ClrRuntimeValue.Invoke(member, arguments);
}
