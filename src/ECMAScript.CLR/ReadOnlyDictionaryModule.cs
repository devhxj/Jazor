using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>")]
public static class ReadOnlyDictionaryModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> class that is a wrapper around the specified dictionary.</summary>
    ///<param name="dictionary">The dictionary to wrap.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="dictionary" /> is <see langword="null" />.</exception>    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> ReadOnlyDictionaryNew<TKey, TValue>(System.Collections.Generic.IDictionary<TKey, TValue> dictionary);

    [WhiteList("static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionaryGetEmpty<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.KeyCollection ReadOnlyDictionaryGetKeys<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ValueCollection ReadOnlyDictionaryGetValues<TKey, TValue>(Map<TKey,TValue> instance);

    ///<summary>Determines whether the dictionary contains an element that has the specified key.</summary>
    ///<param name="key">The key to locate in the dictionary.</param>
    ///<returns>  <see langword="true" /> if the dictionary contains an element that has the specified key; otherwise, <see langword="false" />.</returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlyDictionaryContainsKey<TKey, TValue>(Map<TKey,TValue> instance, TKey key);

    ///<summary>Retrieves the value that is associated with the specified key.</summary>
    ///<param name="key">The key whose value will be retrieved.</param>
    ///<param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if the object that implements <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlyDictionaryTryGetValue<TKey, TValue>(Map<TKey,TValue> instance, TKey key, OutValue<TValue> value);

    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue ReadOnlyDictionaryThis<TKey, TValue>(Map<TKey,TValue> instance, TKey key);

    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Count.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ReadOnlyDictionaryGetCount<TKey, TValue>(Map<TKey,TValue> instance);

    ///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyDictionary`2" />.</summary>
    ///<returns>An enumerator that can be used to iterate through the collection.</returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.GetEnumerator()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> ReadOnlyDictionaryGetEnumerator<TKey, TValue>(Map<TKey,TValue> instance);
}
