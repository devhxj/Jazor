namespace Jazor.CLR;

[ECMAScript]
internal static class SetCarrierRuntime
{
	internal const string ReadOnlyCarrierMarker = "__jazor$readonly_set";
	private const string ReadOnlyMutationMessage = "NotSupportedException: Collection is read-only.";

	internal static bool IsReadOnlyCarrier<T>(Set<T> instance)
		=> instance is not null &&
		   Object.GetOwnPropertyDescriptor(instance, ReadOnlyCarrierMarker) is not null;

	private static Set<T> ThrowReadOnlyAdd<T>(T item)
		=> throw new Error(ReadOnlyMutationMessage);

	private static bool ThrowReadOnlyDelete<T>(T item)
		=> throw new Error(ReadOnlyMutationMessage);

	private static void ThrowReadOnlyClear<T>()
		=> throw new Error(ReadOnlyMutationMessage);

	internal static Set<T> MarkAsReadOnlyCarrier<T>(Set<T> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");

		// Keep the marker API idempotent so repeated wrapping paths remain stable.
		if (IsReadOnlyCarrier(instance))
			return instance;

		Object.DefineProperty(instance, ReadOnlyCarrierMarker, new ECMAScript.PropertyDescriptor
		{
			Value = true,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});

		// Set cannot be made read-only via Object.freeze; override mutators on the carrier.
		Object.DefineProperty(instance, "add", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<T, Set<T>>)ThrowReadOnlyAdd<T>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "delete", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<T, bool>)ThrowReadOnlyDelete<T>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "clear", new ECMAScript.PropertyDescriptor
		{
			Value = (Action)ThrowReadOnlyClear<T>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		return instance;
	}
}
