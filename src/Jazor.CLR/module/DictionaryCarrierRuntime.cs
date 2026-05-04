namespace Jazor.CLR;

[ECMAScript]
internal static class DictionaryCarrierRuntime
{
	internal const string ReadOnlyCarrierMarker = "__jazor$readonly_dictionary";
	private const string ReadOnlyMutationMessage = "NotSupportedException: Collection is read-only.";

	internal static bool IsReadOnlyCarrier<TKey, TValue>(Map<TKey, TValue> instance)
		=> instance is not null &&
		   Object.GetOwnPropertyDescriptor(instance, ReadOnlyCarrierMarker) is not null;

	private static Map<TKey, TValue> ThrowReadOnlySet<TKey, TValue>(TKey key, TValue value)
		=> throw new Error(ReadOnlyMutationMessage);

	private static bool ThrowReadOnlyDelete<TKey, TValue>(TKey key)
		=> throw new Error(ReadOnlyMutationMessage);

	private static void ThrowReadOnlyClear<TKey, TValue>()
		=> throw new Error(ReadOnlyMutationMessage);

	internal static Map<TKey, TValue> MarkAsReadOnlyCarrier<TKey, TValue>(Map<TKey, TValue> instance)
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

		// Map cannot be made read-only via Object.freeze; override mutators on the carrier.
		Object.DefineProperty(instance, "set", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, TValue, Map<TKey, TValue>>)ThrowReadOnlySet<TKey, TValue>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "delete", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, bool>)ThrowReadOnlyDelete<TKey, TValue>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "clear", new ECMAScript.PropertyDescriptor
		{
			Value = (Action)ThrowReadOnlyClear<TKey, TValue>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		return instance;
	}
}
