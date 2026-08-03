namespace Jazor.CLR;

/// <summary>
/// 将 ReadOnlyDictionary&lt;TKey, TValue&gt; 投影为带只读约束的 JavaScript Map。
/// </summary>
/// <remarks>
/// Map 只负责承载键值，ReadOnlyDictionary 的不可写语义由 carrier/helper 协议表达。
/// TryGetValue 等带 out 参数的成员保留 Import 形式，以便显式表达返回值和回写值协议。
/// </remarks>
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
	/// <summary>
	/// Creates a live read-only proxy over the source dictionary. Source mutations remain visible,
	/// while Map mutators invoked through the view are rejected.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)")]
	public static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> _b22e987e1be225aa(Map<TKey, TValue>? dictionary)
	{
		if (dictionary == null)
			throw new Error("ArgumentNullException: dictionary is null.");

		return (System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>)(object)RuntimeModule.MarkAsReadOnlyDictionaryCarrier(dictionary);
	}

	/// <summary>
	/// C#: ReadOnlyDictionary.Empty
	/// JS: new Map()
	/// </summary>
	[Jazor(Op.Import, "static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get")]
	public static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> _43b396f1b8e0a68f()
		=> (System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>)(object)RuntimeModule.MarkAsReadOnlyDictionaryCarrier(DictionaryT2Module<TKey, TValue>.Create(null));

	/// <summary>
	/// Projects the current dictionary keys into the compiler's enumerable array carrier.
	/// The projection has no mutation entry point, so it preserves the read-only boundary while
	/// allowing ordinary foreach and LINQ consumption without exposing Map internals.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get")]
	public static Array<TKey> _4044dececdd2d744(Map<TKey, TValue> instance)
	{
		EnsureInstance(instance);
		return Array<TKey>.From(instance.Keys());
	}

	/// <summary>
	/// Projects the current dictionary values into the compiler's enumerable array carrier.
	/// See <see cref="_4044dececdd2d744"/> for the projection boundary.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get")]
	public static Array<TValue> _b39da265738457a5(Map<TKey, TValue> instance)
	{
		EnsureInstance(instance);
		return Array<TValue>.From(instance.Values());
	}

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
