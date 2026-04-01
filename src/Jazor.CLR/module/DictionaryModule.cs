namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; 类型模块映射规则
///
/// C# Dictionary&lt;TKey, TValue&gt; 与 JavaScript Map 的对应关系：
/// - Dictionary 映射为 JavaScript Map
/// - 大多数方法可以直接映射
///
/// Op 类型选择原则：
/// - Alias: JS Map 有同名方法
/// - Inline: 简单表达式
/// - Import: 需要额外逻辑的方法
/// - Discard: Dictionary 特有但 JS Map 不完全支持的功能
/// </summary>
[ECMAScriptModule("System/Collections/Generic/DictionaryModule.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>","Map")]
public static class DictionaryModule<TKey, TValue>
{
	/// <summary>
	/// C#: new Dictionary&lt;TKey, TValue&gt;()
	/// JS: new Map()
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()", "new Map()")]
	public extern static Map<TKey,TValue> _30796a6445def409();

	/// <summary>
	/// C#: new Dictionary&lt;TKey, TValue&gt;(capacity)
	/// JS: new Map() (Map 没有容量概念)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)", "new Map()")]
	public extern static Map<TKey,TValue> _8e497c9f7d546fbb(Number capacity);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)")]
	public extern static Map<TKey,TValue> _03710ff0cda22f26(object comparer);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)")]
	public extern static Map<TKey,TValue> _2bb0c02fab9a88cb(Number capacity, object comparer);

	/// <summary>
	/// C#: new Dictionary&lt;TKey, TValue&gt;(dictionary)
	/// JS: new Map(dictionary.entries())
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", "new Map(__arg1)")]
	public extern static Map<TKey,TValue> _70d1054600376f0b(Map<TKey,TValue> dictionary);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)")]
	public extern static Map<TKey,TValue> _06de6f2da368940d(object dictionary, object comparer);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)")]
	public extern static Map<TKey,TValue> _27d751bfb444b6b6(object collection);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)")]
	public extern static Map<TKey,TValue> _193763263aaa47e4(object collection, object comparer);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get")]
	public extern static System.Collections.Generic.IEqualityComparer<TKey> _1a4a1b31526edb7a(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: dict.Count
	/// JS: map.size
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>.Count.get", "size")]
	public extern static Number _8603bbd90bf60fc3(Map<TKey,TValue> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get")]
	public extern static Number _93c9c28de958b6e8(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: dict.Keys
	/// JS: Array.from(map.keys())
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.Keys.get", "Array.from(__arg1.keys())")]
	public extern static Array<TKey> _4f3806a69cb6b35b(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: dict.Values
	/// JS: Array.from(map.values())
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.Values.get", "Array.from(__arg1.values())")]
	public extern static Array<TValue> _300379ba29761970(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: dict[key]
	/// JS: map.get(key) (缺失时抛出 KeyNotFoundException)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get")]
	public static TValue _e73dbdff85c46ddc(Map<TKey,TValue> instance, TKey key)
	{
		if (!instance.Has(key))
			throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
		return instance.Get(key);
	}

	/// <summary>
	/// C#: dict[key] = value
	/// JS: map.set(key, value)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set", "__arg1.set(__arg2, __arg3)")]
	public extern static void _63d62bee2698301f(Map<TKey,TValue> instance, TKey key, TValue value);

	/// <summary>
	/// C#: dict.Add(key, value)
	/// JS: map.set(key, value) (注意：Map 不会检查重复键)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)")]
	public static void _39d6e632c4c102f9(Map<TKey,TValue> instance, TKey key, TValue value)
	{
		// .NET Dictionary 在键已存在时会抛出异常
		if (instance.Has(key))
			throw new Error("ArgumentException: An item with the same key has already been added.");
		instance.Set(key, value);
	}

	/// <summary>
	/// C#: dict.Clear()
	/// JS: map.clear()
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>.Clear()", "clear")]
	public extern static void _d701e854a5da9c91(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: dict.ContainsKey(key)
	/// JS: map.has(key)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>.ContainsKey(TKey)", "has")]
	public extern static bool _ff0298236b0e309d(Map<TKey,TValue> instance, TKey key);

	/// <summary>
	/// C#: dict.ContainsValue(value)
	/// JS: Array.from(map.values()).includes(value)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.Dictionary<TKey, TValue>.ContainsValue(TValue)", "Array.from(__arg1.values()).includes(__arg2)")]
	public extern static bool _a402110d48f70caf(Map<TKey,TValue> instance, TValue value);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.GetEnumerator()")]
	public extern static object _b8461dd7acf36e26(Map<TKey,TValue> instance);

	[Jazor(Op.Discard, "virtual System.Collections.Generic.Dictionary<TKey, TValue>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _5fc3fe57da5092e1(Map<TKey,TValue> instance, object info, object context);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.GetAlternateLookup<TAlternateKey>()")]
	public extern static object _81045d6b89c31295<TAlternateKey>(Map<TKey,TValue> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.TryGetAlternateLookup<TAlternateKey>(out System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>)")]
	public extern static Array<object?> _e3413e985c488b3f<TAlternateKey>(Map<TKey,TValue> instance, object lookup);

	[Jazor(Op.Discard, "virtual System.Collections.Generic.Dictionary<TKey, TValue>.OnDeserialization(object)")]
	public extern static void _2a84c2ff8bbcd82f(Map<TKey,TValue> instance, object? sender);

	/// <summary>
	/// C#: dict.Remove(key)
	/// JS: map.delete(key)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey)", "delete")]
	public extern static bool _0a910bf18a745786(Map<TKey,TValue> instance, TKey key);

	/// <summary>
	/// C#: dict.Remove(key, out value)
	/// JS: [success, value]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)")]
	public static Array<object?> _d6ac89338dff5e3b(Map<TKey,TValue> instance, TKey key)
	{
		if (instance.Has(key))
		{
			var value = instance.Get(key);
			instance.Delete(key);
			return [true, value];
		}
		return [false, default(TValue)];
	}

	/// <summary>
	/// C#: dict.TryGetValue(key, out value)
	/// JS: [hasKey, value]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public static Array<object?> _7db4d9112b4ba3c4(Map<TKey,TValue> instance, TKey key)
	{
		if (instance.Has(key))
			return [true, instance.Get(key)];
		return [false, default(TValue)];
	}

	/// <summary>
	/// C#: dict.TryAdd(key, value)
	/// JS: 检查并添加
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)")]
	public static bool _61b63b2c7b14f06a(Map<TKey,TValue> instance, TKey key, TValue value)
	{
		if (instance.Has(key))
			return false;
		instance.Set(key, value);
		return true;
	}

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)")]
	public extern static Number _fdba95f6eefaa760(Map<TKey,TValue> instance, Number capacity);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()")]
	public extern static void _44cc5aa04712525c(Map<TKey,TValue> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)")]
	public extern static void _dd7fceb710b10915(Map<TKey,TValue> instance, Number capacity);
}
