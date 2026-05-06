namespace Jazor.CLR;

[ECMAScriptModule("System/Collections/ObjectModel/ReadOnlyDictionaryT2Module.js")]
[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>","Map")]
public static class ReadOnlyDictionaryT2Module<TKey, TValue> where TKey : notnull
{
	private static void EnsureInstance(Map<TKey, TValue> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	// Keep TryGetValue in an import so the out-value contract is expressed directly in Jazor
	// rather than hidden in a conditional JS expression.
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> class that is a wrapper around the specified dictionary.</summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)")]
	public static Map<TKey, TValue> _b22e987e1be225aa(object dictionary)
	{
		if (dictionary == null)
			throw new Error("ArgumentNullException: dictionary is null");

		var source = (Map<TKey, TValue>)dictionary;
		var snapshot = new Map<TKey, TValue>(source.Entries());
		return RuntimeModule.MarkAsReadOnlyDictionaryCarrier(snapshot);
	}

	/// <summary>
	/// C#: ReadOnlyDictionary.Empty
	/// JS: new Map()
	/// </summary>
	[Jazor(Op.Import, "static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get")]
	public static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> _43b396f1b8e0a68f()
		=> (System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>)(object)RuntimeModule.MarkAsReadOnlyDictionaryCarrier(new Map<TKey, TValue>());

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.KeyCollection _4044dececdd2d744(Map<TKey,TValue> instance);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ValueCollection _b39da265738457a5(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: instance.ContainsKey(key)
	/// JS: instance.has(key)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey)", "__arg1.has(__arg2)")]
	public extern static bool _08bd8c3015d3691e(Map<TKey,TValue> instance, object key);

	/// <summary>
	/// C#: instance.TryGetValue(key, out value)
	/// JS: [has, value]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public static Array<object?> _19af957975f1546f(Map<TKey,TValue> instance, object key, object value)
	{
		EnsureInstance(instance);

		var typedKey = (TKey)key;
		if (!instance.Has(typedKey))
			return [false, null];

		return [true, instance.Get(typedKey)];
	}

	/// <summary>
	/// C#: instance[key]
	/// JS: instance.get(key) (缺失时抛出 KeyNotFoundException)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get")]
	public static TValue _ed4a7913b74bfd87(Map<TKey,TValue> instance, object key)
	{
		EnsureInstance(instance);

		var typedKey = (TKey)key;
		if (!instance.Has(typedKey))
			throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
		return instance.Get(typedKey)!;
	}

	/// <summary>
	/// C#: instance.Count
	/// JS: instance.size
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Count.get", "__arg1.size")]
	public extern static Number _3a7eb79e194b9fae(Map<TKey,TValue> instance);

	///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.GetEnumerator()")]
	public extern static System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> _0d3e962b0af0c46c(Map<TKey,TValue> instance);
}
