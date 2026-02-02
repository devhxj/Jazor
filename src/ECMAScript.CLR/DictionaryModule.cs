using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>", WhiteListOp.Replace, "Map", "System/Collections/DictionaryModule.js")]
public static class DictionaryModule
{
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _30796a6445def409<TKey, TValue>();

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _70d1054600376f0b<TKey, TValue>(object dictionary);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _06de6f2da368940d<TKey, TValue>(object dictionary, object comparer);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _27d751bfb444b6b6<TKey, TValue>(object collection);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _193763263aaa47e4<TKey, TValue>(object collection, object comparer);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _03710ff0cda22f26<TKey, TValue>(object comparer);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _8e497c9f7d546fbb<TKey, TValue>(Number capacity);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _2bb0c02fab9a88cb<TKey, TValue>(Number capacity, object comparer);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get", WhiteListOp.Discard)]
	public extern static System.Collections.Generic.IEqualityComparer<TKey> _1a4a1b31526edb7a<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Count.get", WhiteListOp.Discard)]
	public extern static Number _8603bbd90bf60fc3<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get", WhiteListOp.Discard)]
	public extern static Number _93c9c28de958b6e8<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", WhiteListOp.Discard)]
	public extern static TValue _e73dbdff85c46ddc<TKey, TValue>(Map<TKey, TValue> instance, object key);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set", WhiteListOp.Discard)]
	public extern static void _63d62bee2698301f<TKey, TValue>(Map<TKey, TValue> instance, object key, object value);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Keys.get", WhiteListOp.Discard)]
	public extern static IArray<TKey> _4f3806a69cb6b35b<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Values.get", WhiteListOp.Discard)]
	public extern static IArray<TValue> _300379ba29761970<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)", WhiteListOp.Discard)]
	public extern static void _39d6e632c4c102f9<TKey, TValue>(Map<TKey, TValue> instance, object key, object value);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Clear()", WhiteListOp.Discard)]
	public extern static void _d701e854a5da9c91<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.ContainsKey(TKey)", WhiteListOp.Discard)]
	public extern static bool _ff0298236b0e309d<TKey, TValue>(Map<TKey, TValue> instance, object key);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.ContainsValue(TValue)", WhiteListOp.Discard)]
	public extern static bool _a402110d48f70caf<TKey, TValue>(Map<TKey, TValue> instance, object value);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)", WhiteListOp.Discard)]
	public extern static Number _fdba95f6eefaa760<TKey, TValue>(Map<TKey, TValue> instance, Number capacity);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.GetAlternateLookup<TAlternateKey>()", WhiteListOp.Discard)]
	public extern static Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey> _81045d6b89c31295<TKey, TValue, TAlternateKey>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.GetEnumerator()", WhiteListOp.Discard)]
	public extern static System.Collections.Generic.Dictionary<TKey, TValue>.Enumerator _b8461dd7acf36e26<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("virtual System.Collections.Generic.Dictionary<TKey, TValue>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)", WhiteListOp.Discard)]
	public extern static void _5fc3fe57da5092e1<TKey, TValue>(Map<TKey, TValue> instance, object info, object context);

	[WhiteList("virtual System.Collections.Generic.Dictionary<TKey, TValue>.OnDeserialization(object)", WhiteListOp.Discard)]
	public extern static void _2a84c2ff8bbcd82f<TKey, TValue>(Map<TKey, TValue> instance, Object? sender);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey)", WhiteListOp.Discard)]
	public extern static bool _0a910bf18a745786<TKey, TValue>(Map<TKey, TValue> instance, object key);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)", WhiteListOp.Discard)]
	public extern static bool _d6ac89338dff5e3b<TKey, TValue>(Map<TKey, TValue> instance, object key, OutValue<object> value);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()", WhiteListOp.Discard)]
	public extern static void _44cc5aa04712525c<TKey, TValue>(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)", WhiteListOp.Discard)]
	public extern static void _dd7fceb710b10915<TKey, TValue>(Map<TKey, TValue> instance, Number capacity);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)", WhiteListOp.Discard)]
	public extern static bool _61b63b2c7b14f06a<TKey, TValue>(Map<TKey, TValue> instance, object key, object value);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryGetAlternateLookup<TAlternateKey>(out System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>)", WhiteListOp.Discard)]
	public extern static bool _e3413e985c488b3f<TKey, TValue, TAlternateKey>(Map<TKey, TValue> instance, OutValue<Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>> lookup);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", WhiteListOp.Import)]
	public static bool _7db4d9112b4ba3c4<TKey, TValue>(Map<TKey, TValue> instance, TKey key, OutValue<TValue> value)
	{
		if (instance.Has(key))
		{
			value.Value = instance.Get(key);
			return true;
		}

		return false;
	}
}
