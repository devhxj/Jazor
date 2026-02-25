namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>","System/Runtime/CompilerServices/ConditionalWeakTableModule.js")]
public static class ConditionalWeakTableModule<TKey, TValue>
{
	///<summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.ConditionalWeakTable`2" /> class.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.ConditionalWeakTable()")]
	public extern static WeakMap<TKey,TValue> _925d15e28de85fd7();

	///<summary>Gets the value of the specified key.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public extern static Array<object?> _8360443cbe5b1f88(WeakMap<TKey,TValue> instance, object key, out object value);

	///<summary>Adds a key to the table.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)")]
	public extern static void _c013f77a250570ce(WeakMap<TKey,TValue> instance, object key, object value);

	///<summary>Adds a key to the table if it doesn't already exist.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)")]
	public extern static bool _6a785a77d1b78937(WeakMap<TKey,TValue> instance, object key, object value);

	///<summary>Adds the key and value if the key doesn't exist, or updates the existing key's value if it does exist.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.AddOrUpdate(TKey, TValue)")]
	public extern static void _3e5ae776a9edba7b(WeakMap<TKey,TValue> instance, object key, object value);

	///<summary>Removes a key and its value from the table.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)")]
	public extern static bool _0b5841f143b2e9e7(WeakMap<TKey,TValue> instance, object key);

	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)")]
	public extern static Array<object?> _14e40010b1fd2993(WeakMap<TKey,TValue> instance, object key, out object value);

	///<summary>Clears all the key/value pairs.</summary>
	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()")]
	public extern static void _57912eda7fd377bb(WeakMap<TKey,TValue> instance);

	[Jazor(Op.Discard ,"System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)")]
	public extern static TValue _8e3321f2e6fa2499(WeakMap<TKey,TValue> instance, object key, object value);

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
