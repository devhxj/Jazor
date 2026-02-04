using ECMAScript.Common;
using System.Collections.ObjectModel;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>", WhiteListOp.Allowed, null,"System/Collections/ObjectModel/ReadOnlyDictionary.js")]
public static class ReadOnlyDictionaryModule<TKey, TValue> where TKey : notnull
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> class that is a wrapper around the specified dictionary.</summary>
	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", WhiteListOp.Discard)]
	public extern static Map<TKey,TValue> _b22e987e1be225aa(object dictionary);

	[WhiteList("static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get", WhiteListOp.Discard)]
	public extern static ReadOnlyDictionary<TKey, TValue> _43b396f1b8e0a68f(Map<TKey,TValue> instance);

	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get", WhiteListOp.Discard)]
	public extern static ReadOnlyDictionary<TKey, TValue>.KeyCollection _4044dececdd2d744(Map<TKey,TValue> instance);

	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get", WhiteListOp.Discard)]
	public extern static ReadOnlyDictionary<TKey, TValue>.ValueCollection _b39da265738457a5(Map<TKey,TValue> instance);

	///<summary>Determines whether the dictionary contains an element that has the specified key.</summary>
	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey)", WhiteListOp.Discard)]
	public extern static bool _08bd8c3015d3691e(Map<TKey,TValue> instance, object key);

	///<summary>Retrieves the value that is associated with the specified key.</summary>
	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", WhiteListOp.Discard)]
	public extern static bool _19af957975f1546f(Map<TKey,TValue> instance, object key, Box<object> value);

	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get", WhiteListOp.Discard)]
	public extern static TValue _ed4a7913b74bfd87(Map<TKey,TValue> instance, object key);

	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Count.get", WhiteListOp.Discard)]
	public extern static Number _3a7eb79e194b9fae(Map<TKey,TValue> instance);

	///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</summary>
	[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.GetEnumerator()", WhiteListOp.Discard)]
	public extern static IEnumerator<KeyValuePair<TKey, TValue>> _0d3e962b0af0c46c(Map<TKey,TValue> instance);
}
