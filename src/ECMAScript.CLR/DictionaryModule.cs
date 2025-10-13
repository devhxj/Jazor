using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>")]
public static class DictionaryModule
{
    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.      </summary>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew<TKey, TValue>();

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.      </summary>
    ///<param name="capacity">        The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.      </param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew2<TKey, TValue>(Number capacity);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew3<TKey, TValue>(System.Collections.Generic.IEqualityComparer<TKey>? comparer);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
    ///<param name="capacity">        The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.      </param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew4<TKey, TValue>(Number capacity, System.Collections.Generic.IEqualityComparer<TKey>? comparer);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.      </summary>
    ///<param name="dictionary">        The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="dictionary" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="dictionary" /> contains one or more duplicate keys.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew5<TKey, TValue>(System.Collections.Generic.IDictionary<TKey, TValue> dictionary);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
    ///<param name="dictionary">        The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="dictionary" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="dictionary" /> contains one or more duplicate keys.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew6<TKey, TValue>(System.Collections.Generic.IDictionary<TKey, TValue> dictionary, System.Collections.Generic.IEqualityComparer<TKey>? comparer);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" />.      </summary>
    ///<param name="collection">        The <see cref="T:System.Collections.Generic.IEnumerable`1" />  whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="collection" /> contains one or more duplicated keys.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew7<TKey, TValue>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>> collection);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
    ///<param name="collection">        The <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="collection" /> contains one or more duplicated keys.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Map<TKey,TValue> DictionaryNew8<TKey, TValue>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>> collection, System.Collections.Generic.IEqualityComparer<TKey>? comparer);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.IEqualityComparer<TKey> DictionaryGetComparer<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Count.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DictionaryGetCount<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DictionaryGetCapacity<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Keys.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static IArray<TKey> DictionaryGetKeys<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Values.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static IArray<TValue> DictionaryGetValues<TKey, TValue>(Map<TKey,TValue> instance);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue DictionaryThis<TKey, TValue>(Map<TKey,TValue> instance, TKey key);

    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryThis<TKey, TValue>(Map<TKey,TValue> instance, TKey key, TValue value);

    ///<summary>Adds the specified key and value to the dictionary.</summary>
    ///<param name="key">The key of the element to add.</param>
    ///<param name="value">        The value of the element to add. The value can be <see langword="null" /> for reference types.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">        An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryAdd<TKey, TValue>(Map<TKey,TValue> instance, TKey key, TValue value);

    ///<summary>        Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </summary>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Clear()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryClear<TKey, TValue>(Map<TKey,TValue> instance);

    ///<summary>        Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.      </summary>
    ///<param name="key">        The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.ContainsKey(TKey)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryContainsKey<TKey, TValue>(Map<TKey,TValue> instance, TKey key);

    ///<summary>        Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains a specific value.      </summary>
    ///<param name="value">        The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified value; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.ContainsValue(TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryContainsValue<TKey, TValue>(Map<TKey,TValue> instance, TValue value);

    ///<summary>        Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </summary>
    ///<returns>        A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.GetEnumerator()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.Dictionary<TKey, TValue>.Enumerator DictionaryGetEnumerator<TKey, TValue>(Map<TKey,TValue> instance);

    ///<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.      </summary>
    ///<param name="info">        A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.      </param>
    ///<param name="context">        A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="info" /> is <see langword="null" />.      </exception>    [WhiteList("virtual System.Collections.Generic.Dictionary<TKey, TValue>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryGetObjectData<TKey, TValue>(Map<TKey,TValue> instance, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context);

    ///<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.Dictionary`2" /> using a <typeparamref name="TAlternateKey" /> as a key instead of a <typeparamref name="TKey" />.      </summary>
    ///<typeparam name="TAlternateKey">The alternate type of a key for performing lookups.</typeparam>
    ///<exception cref="T:System.InvalidOperationException">        The dictionary's comparer is not compatible with <typeparamref name="TAlternateKey" />.      </exception>
    ///<returns>The created lookup instance.</returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.GetAlternateLookup<TAlternateKey>()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey> DictionaryGetAlternateLookup<TKey, TValue, TAlternateKey>(Map<TKey,TValue> instance);

    ///<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.Dictionary`2" /> using a <typeparamref name="TAlternateKey" /> as a key instead of a <typeparamref name="TKey" />.      </summary>
    ///<param name="lookup">The created lookup instance when the method returns true, or a default instance that should not be used if the method returns false.</param>
    ///<typeparam name="TAlternateKey">The alternate type of a key for performing lookups.</typeparam>
    ///<returns>  <see langword="true" /> if a lookup could be created; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryGetAlternateLookup<TAlternateKey>(out System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryTryGetAlternateLookup<TKey, TValue, TAlternateKey>(Map<TKey,TValue> instance, OutValue<Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>> lookup);

    ///<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.      </summary>
    ///<param name="sender">The source of the deserialization event.</param>
    ///<exception cref="T:System.Runtime.Serialization.SerializationException">        The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.Dictionary`2" /> instance is invalid.      </exception>    [WhiteList("virtual System.Collections.Generic.Dictionary<TKey, TValue>.OnDeserialization(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryOnDeserialization<TKey, TValue>(Map<TKey,TValue> instance, Object? sender);

    ///<summary>        Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </summary>
    ///<param name="key">The key of the element to remove.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryRemove<TKey, TValue>(Map<TKey,TValue> instance, TKey key);

    ///<summary>        Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />, and copies the element to the <paramref name="value" /> parameter.      </summary>
    ///<param name="key">The key of the element to remove.</param>
    ///<param name="value">The removed element.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryRemove2<TKey, TValue>(Map<TKey,TValue> instance, TKey key, OutValue<TValue> value);

    ///<summary>Gets the value associated with the specified key.</summary>
    ///<param name="key">The key of the value to get.</param>
    ///<param name="value">        When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryTryGetValue<TKey, TValue>(Map<TKey,TValue> instance, TKey key, OutValue<TValue> value);

    ///<summary>Attempts to add the specified key and value to the dictionary.</summary>
    ///<param name="key">The key of the element to add.</param>
    ///<param name="value">        The value of the element to add. It can be <see langword="null" />.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the key/value pair was added to the dictionary successfully; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool DictionaryTryAdd<TKey, TValue>(Map<TKey,TValue> instance, TKey key, TValue value);

    ///<summary>Ensures that the dictionary can hold up to a specified number of entries without any further expansion of its backing storage.</summary>
    ///<param name="capacity">The number of entries.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>
    ///<returns>        The current capacity of the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </returns>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number DictionaryEnsureCapacity<TKey, TValue>(Map<TKey,TValue> instance, Number capacity);

    ///<summary>Sets the capacity of this dictionary to what it would be if it had been originally initialized with all its entries.</summary>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryTrimExcess<TKey, TValue>(Map<TKey,TValue> instance);

    ///<summary>Sets the capacity of this dictionary to hold up a specified number of entries without any further expansion of its backing storage.</summary>
    ///<param name="capacity">The new capacity.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than <see cref="P:System.Collections.Generic.Dictionary`2.Count" />.      </exception>    [WhiteList("System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void DictionaryTrimExcess2<TKey, TValue>(Map<TKey,TValue> instance, Number capacity);
}
