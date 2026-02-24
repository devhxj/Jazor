namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>","System/Collections/ObjectModel/ReadOnlyDictionaryModule.js")]
public static class ReadOnlyDictionaryModule<TKey, TValue> where TKey : notnull
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> class that is a wrapper around the specified dictionary.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)")]
	public extern static Map<TKey,TValue> _b22e987e1be225aa(object dictionary);

	[Jazor(Op.Discard ,"static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> _43b396f1b8e0a68f(Map<TKey,TValue> instance);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.KeyCollection _4044dececdd2d744(Map<TKey,TValue> instance);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get")]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ValueCollection _b39da265738457a5(Map<TKey,TValue> instance);

	///<summary>Determines whether the dictionary contains an element that has the specified key.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey)")]
	public extern static bool _08bd8c3015d3691e(Map<TKey,TValue> instance, object key);

	///<summary>Retrieves the value that is associated with the specified key.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
	public extern static bool _19af957975f1546f(Map<TKey,TValue> instance, object key, Box<object> value);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get")]
	public extern static TValue _ed4a7913b74bfd87(Map<TKey,TValue> instance, object key);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Count.get")]
	public extern static Number _3a7eb79e194b9fae(Map<TKey,TValue> instance);

	///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.GetEnumerator()")]
	public extern static System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> _0d3e962b0af0c46c(Map<TKey,TValue> instance);
}
