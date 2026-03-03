namespace Jazor.CLR;

[ECMAScriptModule("System/Collections/ObjectModel/ReadOnlyDictionaryModule.js")]
[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>","Map")]
public static class ReadOnlyDictionaryModule<TKey, TValue> where TKey : notnull
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> class that is a wrapper around the specified dictionary.</summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", "@#{0}")]
	public extern static Map<TKey,TValue> _b22e987e1be225aa(object dictionary);

	/// <summary>
	/// C#: ReadOnlyDictionary.Empty
	/// JS: new Map()
	/// </summary>
	[Jazor(Op.Inline, "static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get", "new Map()")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> _43b396f1b8e0a68f();

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.KeyCollection _4044dececdd2d744(Map<TKey,TValue> instance);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ValueCollection _b39da265738457a5(Map<TKey,TValue> instance);

	/// <summary>
	/// C#: instance.ContainsKey(key)
	/// JS: instance.has(key)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey)", "@#{0}.has(@#{1})")]
	public extern static bool _08bd8c3015d3691e(Map<TKey,TValue> instance, object key);

	/// <summary>
	/// C#: instance.TryGetValue(key, out value)
	/// JS: [has, value]
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", "(@#{0}.has(@#{1}) ? [true, @#{0}.get(@#{1})] : [false, null])")]
	public extern static Array<object?> _19af957975f1546f(Map<TKey,TValue> instance, object key, object value);

	/// <summary>
	/// C#: instance[key]
	/// JS: instance.get(key)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get", "@#{0}.get(@#{1})")]
	public extern static TValue _ed4a7913b74bfd87(Map<TKey,TValue> instance, object key);

	/// <summary>
	/// C#: instance.Count
	/// JS: instance.size
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Count.get", "@#{0}.size")]
	public extern static Number _3a7eb79e194b9fae(Map<TKey,TValue> instance);

	///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.GetEnumerator()")]
	public extern static System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> _0d3e962b0af0c46c(Map<TKey,TValue> instance);
}
