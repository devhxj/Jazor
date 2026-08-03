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
[ECMAScriptModule("System/Collections/Generic/DictionaryT2Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>","Map")]
public static class DictionaryT2Module<TKey, TValue>
{
	// Native Map remains the physical carrier. The private WeakMap supplies CLR key equality
	// without exposing bookkeeping fields or changing iteration, representative keys, or size.
	private static readonly WeakMap<Map<TKey, TValue>, (System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash)> States = new();
	private static readonly WeakMap<Map<TKey, TValue>, Number> Capacities = new();

	private static void EnsureInstance(Map<TKey, TValue> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static Number GetCapacity(Map<TKey, TValue> instance)
	{
		EnsureInstance(instance);
		if (!Capacities.Has(instance))
			Capacities.Set(instance, RuntimeModule.GetHashCollectionCapacity(instance.Size));
		return Capacities.Get(instance)!;
	}

	private static void EnsureEntryCapacity(Map<TKey, TValue> instance, Number requiredCount)
	{
		var capacity = GetCapacity(instance);
		if (requiredCount > capacity)
			Capacities.Set(instance, RuntimeModule.ExpandHashCollectionCapacity(capacity));
	}

	private static Number EnsureCapacityCore(Map<TKey, TValue> instance, Number capacity)
	{
		var requested = RuntimeModule.GetHashCollectionCapacity(capacity);
		var current = GetCapacity(instance);
		if (requested <= current)
			return current;

		Capacities.Set(instance, requested);
		return requested;
	}

	private static Number GetHashCode(
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state,
		TKey key)
		=> state.Comparer == null
			? EqualityComparerT1Module<TKey>.GetHashCodeCore(key)
			: state.Comparer.GetHashCode(key!);

	private static bool Equals(
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state,
		TKey left,
		TKey right)
		=> state.Comparer == null
			? EqualityComparerT1Module<TKey>.EqualsCore(left, right)
			: state.Comparer.Equals(left, right);

	private static Number FindEquivalentIndex(
		Array<TKey> bucket,
		TKey key,
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state)
	{
		for (Number index = 0; index < bucket.Length; index++)
		{
			if (Equals(state, bucket[index], key))
				return index;
		}

		return -1;
	}

	private static Array<TKey> GetOrCreateBucket(
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state,
		Number hashCode)
	{
		if (state.KeysByHash.Has(hashCode))
			return state.KeysByHash.Get(hashCode)!;

		var bucket = new Array<TKey>();
		state.KeysByHash.Set(hashCode, bucket);
		return bucket;
	}

	private static void NativeSet(Map<TKey, TValue> instance, TKey key, TValue value)
	{
		var set = Reflect.Get(Map.Prototype, "set");
		if (set == null)
			throw new Error("MissingMethodException: Map.prototype.set is unavailable.");

		Reflect.Apply(set, instance, [key, value]);
	}

	private static TValue? NativeGet(Map<TKey, TValue> instance, TKey key)
	{
		var get = Reflect.Get(Map.Prototype, "get");
		if (get == null)
			throw new Error("MissingMethodException: Map.prototype.get is unavailable.");

		return (TValue?)Reflect.Apply(get, instance, [key]);
	}

	private static bool NativeDelete(Map<TKey, TValue> instance, TKey key)
	{
		var delete = Reflect.Get(Map.Prototype, "delete");
		if (delete == null)
			throw new Error("MissingMethodException: Map.prototype.delete is unavailable.");

		return (bool)Reflect.Apply(delete, instance, [key])!;
	}

	private static void NativeClear(Map<TKey, TValue> instance)
	{
		var clear = Reflect.Get(Map.Prototype, "clear");
		if (clear == null)
			throw new Error("MissingMethodException: Map.prototype.clear is unavailable.");

		Reflect.Apply(clear, instance, []);
	}

	private static Map<TKey, TValue> SetCore(
		Map<TKey, TValue> instance,
		TKey key,
		TValue value,
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state)
	{
		var hashCode = GetHashCode(state, key);
		var bucket = GetOrCreateBucket(state, hashCode);
		var index = FindEquivalentIndex(bucket, key, state);
		if (index >= 0)
		{
			// Dictionary preserves its original key representative when an equivalent key is assigned.
			NativeSet(instance, bucket[index], value);
			return instance;
		}

		EnsureEntryCapacity(instance, instance.Size + 1);
		bucket.Push(key);
		NativeSet(instance, key, value);
		return instance;
	}

	private static bool HasCore(
		TKey key,
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state)
	{
		var hashCode = GetHashCode(state, key);
		if (!state.KeysByHash.Has(hashCode))
			return false;

		return FindEquivalentIndex(state.KeysByHash.Get(hashCode)!, key, state) >= 0;
	}

	private static TValue? GetCore(
		Map<TKey, TValue> instance,
		TKey key,
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state)
	{
		var hashCode = GetHashCode(state, key);
		if (!state.KeysByHash.Has(hashCode))
			return NativeGet(instance, key);

		var bucket = state.KeysByHash.Get(hashCode)!;
		var index = FindEquivalentIndex(bucket, key, state);
		return index < 0 ? NativeGet(instance, key) : NativeGet(instance, bucket[index]);
	}

	private static bool DeleteCore(
		Map<TKey, TValue> instance,
		TKey key,
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state)
	{
		var hashCode = GetHashCode(state, key);
		if (!state.KeysByHash.Has(hashCode))
			return false;

		var bucket = state.KeysByHash.Get(hashCode)!;
		var index = FindEquivalentIndex(bucket, key, state);
		if (index < 0)
			return false;

		var representative = bucket[index];
		bucket.Splice(index, 1);
		if (bucket.Length == 0)
			state.KeysByHash.Delete(hashCode);

		return NativeDelete(instance, representative);
	}

	private static void ClearCore(
		Map<TKey, TValue> instance,
		(System.Collections.Generic.IEqualityComparer<TKey>? Comparer, Map<Number, Array<TKey>> KeysByHash) state)
	{
		state.KeysByHash.Clear();
		NativeClear(instance);
	}

	internal static Map<TKey, TValue> Create(System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> Create(comparer, capacity: 0);

	private static Map<TKey, TValue> Create(System.Collections.Generic.IEqualityComparer<TKey>? comparer, Number capacity)
	{
		var normalizedCapacity = RuntimeModule.GetHashCollectionCapacity(capacity);
		var instance = new Map<TKey, TValue>();
		Capacities.Set(instance, normalizedCapacity);
		if (comparer is null)
			return instance;

		var state = (Comparer: comparer, KeysByHash: new Map<Number, Array<TKey>>());
		States.Set(instance, state);
		Object.DefineProperty(instance, "set", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, TValue, Map<TKey, TValue>>)(
				(key, value) => SetCore(instance, key, value, state)),
			Enumerable = false,
			Writable = false,
			Configurable = true
		});
		Object.DefineProperty(instance, "get", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, TValue?>)(key => GetCore(instance, key, state)),
			Enumerable = false,
			Writable = false,
			Configurable = true
		});
		Object.DefineProperty(instance, "has", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, bool>)(key => HasCore(key, state)),
			Enumerable = false,
			Writable = false,
			Configurable = true
		});
		Object.DefineProperty(instance, "delete", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, bool>)(key => DeleteCore(instance, key, state)),
			Enumerable = false,
			Writable = false,
			Configurable = true
		});
		Object.DefineProperty(instance, "clear", new ECMAScript.PropertyDescriptor
		{
			Value = (Action)(() => ClearCore(instance, state)),
			Enumerable = false,
			Writable = false,
			Configurable = true
		});
		return instance;
	}

	private static void SetItemCore(Map<TKey, TValue> instance, TKey key, TValue value)
	{
		EnsureInstance(instance);
		if (!instance.Has(key))
			EnsureEntryCapacity(instance, instance.Size + 1);
		instance.Set(key, value);
	}

	internal static Map<TKey, TValue> CreateFromMap(
		Map<TKey, TValue> source,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: dictionary is null");

		var result = Create(comparer, source.Size);
		foreach (var key in source.Keys())
			result.Set(key, source.Get(key)!);
		return result;
	}

	private static Map<TKey, TValue> CreateFromPairs(
		IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>> source,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: collection is null.");

		Number initialCapacity = source is Array<System.Collections.Generic.KeyValuePair<TKey, TValue>> values
			? values.Length
			: 0;
		var result = Create(comparer, initialCapacity);
		// KeyValuePair is emitted as a Map-entry array. Deconstruction keeps this adapter
		// on the compiler's structural protocol instead of depending on a CLR wrapper type.
		foreach (var (key, value) in source)
		{
			if (result.Has(key))
				throw new Error("ArgumentException: An item with the same key has already been added.");

			SetItemCore(result, key, value);
		}

		return result;
	}

	internal static System.Collections.Generic.IEqualityComparer<TKey>? GetComparer(Map<TKey, TValue> instance)
	{
		EnsureInstance(instance);
		return States.Has(instance) ? States.Get(instance)!.Comparer : null;
	}

	/// <summary>
	/// C#: new Dictionary&lt;TKey, TValue&gt;()
	/// JS: new Map()
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()", "createDefault")]
	public static Map<TKey, TValue> CreateDefault()
		=> Create(comparer: null);

	/// <summary>
	/// C#: new Dictionary&lt;TKey, TValue&gt;(capacity)
	/// JS: new Map() (Map 没有容量概念)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)", "createWithCapacity")]
	public static Map<TKey, TValue> CreateWithCapacity(Number capacity)
		=> Create(comparer: null, capacity);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)", "createWithComparer")]
	public static Map<TKey, TValue> CreateWithComparer(System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> Create(comparer);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)", "createWithCapacityAndComparer")]
	public static Map<TKey, TValue> CreateWithCapacityAndComparer(
		Number capacity,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> Create(comparer, capacity);

	/// <summary>
	/// C#: new Dictionary&lt;TKey, TValue&gt;(dictionary)
	/// JS: new Map(dictionary.entries())
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", "createFromDictionary")]
	public static Map<TKey, TValue> CreateFromDictionary(Map<TKey, TValue> dictionary)
		=> CreateFromMap(dictionary, comparer: null);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)", "createFromDictionaryWithComparer")]
	public static Map<TKey, TValue> CreateFromDictionaryWithComparer(
		Map<TKey, TValue> dictionary,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> CreateFromMap(dictionary, comparer);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)", "createFromPairs")]
	public static Map<TKey, TValue> CreateFromPairs(
		IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>> collection)
		=> CreateFromPairs(collection, comparer: null);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)", "createFromPairsWithComparer")]
	public static Map<TKey, TValue> CreateFromPairsWithComparer(
		IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>> collection,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> CreateFromPairs(collection, comparer);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get")]
	public static System.Collections.Generic.IEqualityComparer<TKey> _1a4a1b31526edb7a(Map<TKey, TValue> instance)
		=> GetComparer(instance) ?? (System.Collections.Generic.IEqualityComparer<TKey>)EqualityComparerT1Module<TKey>.GetDefault();

	/// <summary>
	/// C#: dict.Count
	/// JS: map.size
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.Dictionary<TKey, TValue>.Count.get", "size")]
	public extern static Number _8603bbd90bf60fc3(Map<TKey,TValue> instance);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get", "getCapacityMember")]
	public static Number GetCapacityMember(Map<TKey, TValue> instance)
		=> GetCapacity(instance);

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
	/// JS: instance.get(key) (缺失时抛出 KeyNotFoundException)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get")]
	public static TValue _e73dbdff85c46ddc(Map<TKey, TValue> instance, TKey key)
	{
		EnsureInstance(instance);
		if (!instance.Has(key))
			throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
		return instance.Get(key)!;
	}

	/// <summary>
	/// C#: dict[key] = value
	/// JS: map.set(key, value)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set", "setItem")]
	public static void SetItem(Map<TKey,TValue> instance, TKey key, TValue value)
		=> SetItemCore(instance, key, value);

	/// <summary>
	/// C#: dict.Add(key, value)
	/// JS: map.set(key, value) (注意：Map 不会检查重复键)
	/// 当前仍保留 Import：重复键检测和异常语义仍需要稳定运行时逻辑承载，
	/// 不要为了减少 Import 把它压成脆弱模板。
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)")]
	public static void _39d6e632c4c102f9(Map<TKey,TValue> instance, TKey key, TValue value)
	{
		EnsureInstance(instance);

		// .NET Dictionary 在键已存在时会抛出异常
		if (instance.Has(key))
			throw new Error("ArgumentException: An item with the same key has already been added.");
		EnsureEntryCapacity(instance, instance.Size + 1);
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
		EnsureInstance(instance);

		if (instance.Has(key))
		{
			var value = instance.Get(key);
			instance.Delete(key);
			return [true, value];
		}
		return [false, null];
	}

	/// <summary>
	/// C#: dict.TryGetValue(key, out value)
	/// JS: [hasKey, value]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public static Array<object?> _7db4d9112b4ba3c4(Map<TKey,TValue> instance, TKey key)
	{
		EnsureInstance(instance);

		if (instance.Has(key))
			return [true, instance.Get(key)];
		return [false, null];
	}

	/// <summary>
	/// C#: dict.TryAdd(key, value)
	/// JS: 检查并添加
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)")]
	public static bool _61b63b2c7b14f06a(Map<TKey,TValue> instance, TKey key, TValue value)
	{
		EnsureInstance(instance);

		if (instance.Has(key))
			return false;
		EnsureEntryCapacity(instance, instance.Size + 1);
		instance.Set(key, value);
		return true;
	}

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)", "ensureCapacity")]
	public static Number EnsureCapacity(Map<TKey, TValue> instance, Number capacity)
		=> EnsureCapacityCore(instance, capacity);

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()")]
	public static void _44cc5aa04712525c(Map<TKey,TValue> instance)
	{
		var capacity = GetCapacity(instance);
		var trimmed = RuntimeModule.GetHashCollectionCapacity(instance.Size);
		if (trimmed < capacity)
			Capacities.Set(instance, trimmed);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)")]
	public static void _dd7fceb710b10915(Map<TKey,TValue> instance, Number capacity)
	{
		var current = GetCapacity(instance);
		if (capacity < instance.Size)
			throw new Error("ArgumentOutOfRangeException: capacity cannot be less than Count.");
		var trimmed = RuntimeModule.GetHashCollectionCapacity(capacity);
		if (trimmed < current)
			Capacities.Set(instance, trimmed);
	}
}
