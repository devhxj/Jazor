using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>", WhiteListOp.Allowed, null, "System/Collections/Generic/DictionaryModule.js")]
public static class DictionaryModule<TKey, TValue> where TKey : notnull
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _30796a6445def409();

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _8e497c9f7d546fbb(Number capacity);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _03710ff0cda22f26(object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _2bb0c02fab9a88cb(Number capacity, object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _70d1054600376f0b(object dictionary);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _06de6f2da368940d(object dictionary, object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _27d751bfb444b6b6(object collection);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)", WhiteListOp.Discard)]
	public extern static Map<TKey, TValue> _193763263aaa47e4(object collection, object comparer);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get", WhiteListOp.Discard)]
	public extern static System.Collections.Generic.IEqualityComparer<TKey> _1a4a1b31526edb7a(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Count.get", WhiteListOp.Discard)]
	public extern static Number _8603bbd90bf60fc3(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get", WhiteListOp.Discard)]
	public extern static Number _93c9c28de958b6e8(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Keys.get", WhiteListOp.Discard)]
	public extern static IArray<TKey> _4f3806a69cb6b35b(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Values.get", WhiteListOp.Discard)]
	public extern static IArray<TValue> _300379ba29761970(Map<TKey, TValue> instance);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", WhiteListOp.Discard)]
	public extern static TValue _e73dbdff85c46ddc(Map<TKey, TValue> instance, object key);

	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set", WhiteListOp.Discard)]
	public extern static void _63d62bee2698301f(Map<TKey, TValue> instance, object key, object value);

	///<summary>Adds the specified key and value to the dictionary.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)", WhiteListOp.Discard)]
	public extern static void _39d6e632c4c102f9(Map<TKey, TValue> instance, object key, object value);

	///<summary>Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Clear()", WhiteListOp.Discard)]
	public extern static void _d701e854a5da9c91(Map<TKey, TValue> instance);

	///<summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.ContainsKey(TKey)", WhiteListOp.Discard)]
	public extern static bool _ff0298236b0e309d(Map<TKey, TValue> instance, object key);

	///<summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains a specific value.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.ContainsValue(TValue)", WhiteListOp.Discard)]
	public extern static bool _a402110d48f70caf(Map<TKey, TValue> instance, object value);

	///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.GetEnumerator()", WhiteListOp.Discard)]
	public extern static Dictionary<TKey, TValue>.Enumerator _b8461dd7acf36e26(Map<TKey, TValue> instance);

	///<summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</summary>
	[WhiteList("virtual System.Collections.Generic.Dictionary<TKey, TValue>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)", WhiteListOp.Discard)]
	public extern static void _5fc3fe57da5092e1(Map<TKey, TValue> instance, object info, object context);

	///<summary>Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.Dictionary`2" /> using a <typeparamref name="TAlternateKey" /> as a key instead of a <typeparamref name="TKey" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.GetAlternateLookup<TAlternateKey>()", WhiteListOp.Discard)]
	public extern static Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey> _81045d6b89c31295<TAlternateKey>(Map<TKey, TValue> instance) where TAlternateKey : notnull;

	///<summary>Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.Dictionary`2" /> using a <typeparamref name="TAlternateKey" /> as a key instead of a <typeparamref name="TKey" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryGetAlternateLookup<TAlternateKey>(out System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>)", WhiteListOp.Discard)]
	public extern static bool _e3413e985c488b3f<TAlternateKey>(Map<TKey, TValue> instance, Box<Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>> lookup) where TAlternateKey : notnull;

	///<summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	[WhiteList("virtual System.Collections.Generic.Dictionary<TKey, TValue>.OnDeserialization(object)", WhiteListOp.Discard)]
	public extern static void _2a84c2ff8bbcd82f(Map<TKey, TValue> instance, Object? sender);

	///<summary>Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey)", WhiteListOp.Discard)]
	public extern static bool _0a910bf18a745786(Map<TKey, TValue> instance, object key);

	///<summary>Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />, and copies the element to the <paramref name="value" /> parameter.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)", WhiteListOp.Discard)]
	public extern static bool _d6ac89338dff5e3b(Map<TKey, TValue> instance, object key, Box<object> value);

	///<summary>Gets the value associated with the specified key.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", WhiteListOp.Discard)]
	public extern static bool _7db4d9112b4ba3c4(Map<TKey, TValue> instance, object key, Box<object> value);

	///<summary>Attempts to add the specified key and value to the dictionary.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)", WhiteListOp.Discard)]
	public extern static bool _61b63b2c7b14f06a(Map<TKey, TValue> instance, object key, object value);

	///<summary>Ensures that the dictionary can hold up to a specified number of entries without any further expansion of its backing storage.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)", WhiteListOp.Discard)]
	public extern static Number _fdba95f6eefaa760(Map<TKey, TValue> instance, Number capacity);

	///<summary>Sets the capacity of this dictionary to what it would be if it had been originally initialized with all its entries.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()", WhiteListOp.Discard)]
	public extern static void _44cc5aa04712525c(Map<TKey, TValue> instance);

	///<summary>Sets the capacity of this dictionary to hold up a specified number of entries without any further expansion of its backing storage.</summary>
	[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)", WhiteListOp.Discard)]
	public extern static void _dd7fceb710b10915(Map<TKey, TValue> instance, Number capacity);
}
