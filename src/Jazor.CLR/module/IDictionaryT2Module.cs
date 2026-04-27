namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IDictionary&lt;TKey, TValue&gt; 类型模块映射规则
///
/// IDictionary&lt;TKey, TValue&gt; 是泛型字典接口，映射到 JavaScript Map。
///
/// Op 类型选择原则：
/// - Alias/Inline/Import: 只开放不依赖具体可变 carrier 的查询语义
/// - Discard: 可变成员与 ReadOnlyDictionary 共享同一 runtime alias，接口层不能静默假设可写
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IDictionaryT2Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IDictionary<TKey, TValue>", "Map")]
public static class IDictionaryT2Module<TKey, TValue>
{
	/// <summary>
	/// C#: dict[key]
	/// JS: ((__m, __k) => { if (__m.has(__k)) return __m.get(__k); throw KeyNotFoundException; })(map, key)
	/// 用参数化 IIFE 保证 receiver / key 只求值一次，避免内联模板重复求值导致副作用次数错误。
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get", "((__m, __k) => { if (__m.has(__k)) return __m.get(__k); throw new Error('KeyNotFoundException: The given key was not present in the dictionary.'); })(__arg1, __arg2)")]
	public extern static TValue _371fad9265e864a1(Map<TKey, TValue> instance, TKey key);

	/// <summary>
	/// C#: dict[key] = value
	/// JS: map.set(key, value)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].set")]
	public static void _f3b177bfce76ed5c(Map<TKey, TValue> instance, TKey key, TValue value)
	{
		// ReadOnlyDictionary 运行时 carrier 共享 Map；接口写入口必须守住只读边界。
		if (DictionaryCarrierRuntime.IsReadOnlyCarrier(instance))
			throw new Error("NotSupportedException: Collection is read-only.");
		instance.Set(key, value);
	}

	/// <summary>
	/// C#: dict.Keys
	/// JS: Array.from(map.keys())
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.IDictionary<TKey, TValue>.Keys.get", "Array.from(__arg1.keys())")]
	public extern static Array<TKey> _a83465399c1d170f(Map<TKey, TValue> instance);

	/// <summary>
	/// C#: dict.Values
	/// JS: Array.from(map.values())
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.IDictionary<TKey, TValue>.Values.get", "Array.from(__arg1.values())")]
	public extern static Array<TValue> _a48c0eb82bacff74(Map<TKey, TValue> instance);

	/// <summary>
	/// C#: dict.ContainsKey(key)
	/// JS: map.has(key)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.IDictionary<TKey, TValue>.ContainsKey(TKey)", "has")]
	public extern static bool _71847e6aeb7b11d0(Map<TKey, TValue> instance, TKey key);

	/// <summary>
	/// C#: dict.Add(key, value)
	/// JS: map.set(key, value)
	/// 注意：IDictionary.Add 在键已存在时抛出异常
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)")]
	public extern static void _93efc3872e59b431(Map<TKey, TValue> instance, TKey key, TValue value);

	/// <summary>
	/// C#: dict.Remove(key)
	/// JS: map.delete(key)
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)")]
	public extern static bool _fc84b7a31e5cdfe4(Map<TKey, TValue> instance, TKey key);

	/// <summary>
	/// C#: dict.TryGetValue(key, out value)
	/// JS: [hasKey, value]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public static Array<object?> _ebaafc4d4a520807(Map<TKey, TValue> instance, TKey key)
	{
		if (instance.Has(key))
			return [true, instance.Get(key)];
		return [false, default(TValue)];
	}
}
