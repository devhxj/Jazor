namespace Jazor.CLR;

[ECMAScriptModule("System/Runtime/CompilerServices/ConditionalWeakTableModule.js")]
[Jazor(Op.Alias, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>","WeakMap")]
public static class ConditionalWeakTableModule<TKey, TValue> where TKey : class
{
	/// <summary>
	/// C#: new ConditionalWeakTable()
	/// JS: new WeakMap()
	/// </summary>
	[Jazor(Op.Inline, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.ConditionalWeakTable()", "new WeakMap()")]
	public extern static WeakMap<TKey,TValue> _925d15e28de85fd7();

	/// <summary>
	/// C#: instance.TryGetValue(key, out value)
	/// JS: [has, value]
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public static Array<object?> _8360443cbe5b1f88(WeakMap<TKey,TValue> instance, object key, object value)
	{
		var typedKey = (TKey)key;
		if (!instance.Has(typedKey))
			return [false, null];

		return [true, instance.Get(typedKey)];
	}

	/// <summary>
	/// C#: instance.Add(key, value)
	/// JS: instance.set(key, value) (如果 key 已存在则抛异常)
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)")]
	public static void _c013f77a250570ce(WeakMap<TKey,TValue> instance, object key, object value)
	{
		var typedKey = (TKey)key;
		var typedValue = (TValue)value;
		if (instance.Has(typedKey))
			throw new Error("ArgumentException: An item with the same key has already been added.");
		instance.Set(typedKey, typedValue);
	}

	/// <summary>
	/// C#: instance.TryAdd(key, value)
	/// JS: !instance.has(key) && instance.set(key, value)
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)")]
	public static bool _6a785a77d1b78937(WeakMap<TKey,TValue> instance, object key, object value)
	{
		var typedKey = (TKey)key;
		if (instance.Has(typedKey))
			return false;

		instance.Set(typedKey, (TValue)value);
		return true;
	}

	/// <summary>
	/// C#: instance.AddOrUpdate(key, value)
	/// JS: instance.set(key, value)
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.AddOrUpdate(TKey, TValue)")]
	public static void _3e5ae776a9edba7b(WeakMap<TKey,TValue> instance, object key, object value)
		=> instance.Set((TKey)key, (TValue)value);

	/// <summary>
	/// C#: instance.Remove(key)
	/// JS: instance.delete(key)
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)")]
	public static bool _0b5841f143b2e9e7(WeakMap<TKey,TValue> instance, object key)
		=> instance.Delete((TKey)key);

	/// <summary>
	/// C#: instance.Remove(key, out value)
	/// JS: [deleted, value]
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)")]
	public static Array<object?> _14e40010b1fd2993(WeakMap<TKey,TValue> instance, object key, object value)
	{
		var typedKey = (TKey)key;
		if (!instance.Has(typedKey))
			return [false, null];

		var currentValue = instance.Get(typedKey);
		instance.Delete(typedKey);
		return [true, currentValue];
	}

	/// <summary>
	/// C#: instance.Clear()
	/// JS: instance = new WeakMap() (reassignment)
	/// Note: WeakMap doesn't have clear(), so we return a new instance
	/// </summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()")]
	public extern static void _57912eda7fd377bb(WeakMap<TKey,TValue> instance);

	/// <summary>
	/// C#: instance.GetOrAdd(key, value)
	/// JS: 如果 key 已存在则返回旧值，否则写入并返回新值
	/// </summary>
	[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)")]
	public static TValue _8e3321f2e6fa2499(WeakMap<TKey,TValue> instance, object key, object value)
	{
		var typedKey = (TKey)key;
		if (instance.Has(typedKey))
			return instance.Get(typedKey)!;

		var typedValue = (TValue)value;
		instance.Set(typedKey, typedValue);
		return typedValue;
	}

	/// <summary>
	/// C#: instance.GetOrAdd(key, valueFactory)
	/// JS: instance.get(key) ?? (instance.set(key, valueFactory(key)), valueFactory(key))
	/// </summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, System.Func<TKey, TValue>)")]
	public extern static TValue _ed09a626bf4f3ea8(WeakMap<TKey,TValue> instance, object key, object valueFactory);

	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)")]
	public extern static TValue _eaeddd47f4a65d81<TArg>(WeakMap<TKey,TValue> instance, object key, object valueFactory, object factoryArgument);

	///<summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes a callback method to create a value that is bound to the specified key.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetValue(TKey, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback)")]
	public extern static TValue _43edc29b01c6a1f0(WeakMap<TKey,TValue> instance, object key, object createValueCallback);

	///<summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes the parameterless constructor of the class that represents the table's value to create a value that is bound to the specified key.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrCreateValue(TKey)")]
	public extern static TValue _8e97651a27c54464(WeakMap<TKey,TValue> instance, object key);
}
