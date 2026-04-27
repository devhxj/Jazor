namespace Jazor.CLR;

internal static class DictionaryCarrierRuntime
{
	internal const string ReadOnlyCarrierMarker = "__jazor$readonly_dictionary";

	internal static bool IsReadOnlyCarrier<TKey, TValue>(Map<TKey, TValue> instance)
		=> Object.GetOwnPropertyDescriptor(instance, ReadOnlyCarrierMarker) is not null;

	internal static Map<TKey, TValue> MarkAsReadOnlyCarrier<TKey, TValue>(Map<TKey, TValue> instance)
	{
		Object.DefineProperty(instance, ReadOnlyCarrierMarker, new ECMAScript.PropertyDescriptor
		{
			Value = true,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		return instance;
	}
}
