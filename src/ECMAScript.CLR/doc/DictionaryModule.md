# DictionaryModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()</br>
**签名**：_30796a6445def409</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.      </summary>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)</br>
**签名**：_8e497c9f7d546fbb</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.      </summary>
<param name="capacity">        The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.      </param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)</br>
**签名**：_03710ff0cda22f26</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)</br>
**签名**：_2bb0c02fab9a88cb</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
<param name="capacity">        The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.      </param>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)</br>
**签名**：_70d1054600376f0b</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.      </summary>
<param name="dictionary">        The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="dictionary" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentException">  <paramref name="dictionary" /> contains one or more duplicate keys.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)</br>
**签名**：_06de6f2da368940d</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
<param name="dictionary">        The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="dictionary" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentException">  <paramref name="dictionary" /> contains one or more duplicate keys.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)</br>
**签名**：_27d751bfb444b6b6</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" />.      </summary>
<param name="collection">        The <see cref="T:System.Collections.Generic.IEnumerable`1" />  whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentException">  <paramref name="collection" /> contains one or more duplicated keys.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)</br>
**签名**：_193763263aaa47e4</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IEnumerable`1" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.      </summary>
<param name="collection">        The <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentException">  <paramref name="collection" /> contains one or more duplicated keys.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get</br>
**签名**：_1a4a1b31526edb7a</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Count.get</br>
**签名**：_8603bbd90bf60fc3</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get</br>
**签名**：_93c9c28de958b6e8</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Keys.get</br>
**签名**：_4f3806a69cb6b35b</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Values.get</br>
**签名**：_300379ba29761970</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get</br>
**签名**：_e73dbdff85c46ddc</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set</br>
**签名**：_63d62bee2698301f</br>

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)</br>
**签名**：_39d6e632c4c102f9</br>
**注释**：

```xml
<summary>Adds the specified key and value to the dictionary.</summary>
<param name="key">The key of the element to add.</param>
<param name="value">        The value of the element to add. The value can be <see langword="null" /> for reference types.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentException">        An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Clear()</br>
**签名**：_d701e854a5da9c91</br>
**注释**：

```xml
<summary>        Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </summary>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.ContainsKey(TKey)</br>
**签名**：_ff0298236b0e309d</br>
**注释**：

```xml
<summary>        Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.      </summary>
<param name="key">        The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.ContainsValue(TValue)</br>
**签名**：_a402110d48f70caf</br>
**注释**：

```xml
<summary>        Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains a specific value.      </summary>
<param name="value">        The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />. The value can be <see langword="null" /> for reference types.      </param>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified value; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.GetEnumerator()</br>
**签名**：_b8461dd7acf36e26</br>
**注释**：

```xml
<summary>        Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </summary>
<returns>        A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </returns>
```

**成员**：virtual System.Collections.Generic.Dictionary<TKey, TValue>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)</br>
**签名**：_5fc3fe57da5092e1</br>
**注释**：

```xml
<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.      </summary>
<param name="info">        A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.      </param>
<param name="context">        A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="info" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.GetAlternateLookup<TAlternateKey>()</br>
**签名**：_81045d6b89c31295</br>
**注释**：

```xml
<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.Dictionary`2" /> using a <typeparamref name="TAlternateKey" /> as a key instead of a <typeparamref name="TKey" />.      </summary>
<typeparam name="TAlternateKey">The alternate type of a key for performing lookups.</typeparam>
<exception cref="T:System.InvalidOperationException">        The dictionary's comparer is not compatible with <typeparamref name="TAlternateKey" />.      </exception>
<returns>The created lookup instance.</returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.TryGetAlternateLookup<TAlternateKey>(out System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>)</br>
**签名**：_e3413e985c488b3f</br>
**注释**：

```xml
<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.Dictionary`2" /> using a <typeparamref name="TAlternateKey" /> as a key instead of a <typeparamref name="TKey" />.      </summary>
<param name="lookup">The created lookup instance when the method returns true, or a default instance that should not be used if the method returns false.</param>
<typeparam name="TAlternateKey">The alternate type of a key for performing lookups.</typeparam>
<returns>  <see langword="true" /> if a lookup could be created; otherwise, <see langword="false" />.      </returns>
```

**成员**：virtual System.Collections.Generic.Dictionary<TKey, TValue>.OnDeserialization(object)</br>
**签名**：_2a84c2ff8bbcd82f</br>
**注释**：

```xml
<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.      </summary>
<param name="sender">The source of the deserialization event.</param>
<exception cref="T:System.Runtime.Serialization.SerializationException">        The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.Dictionary`2" /> instance is invalid.      </exception>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey)</br>
**签名**：_0a910bf18a745786</br>
**注释**：

```xml
<summary>        Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </summary>
<param name="key">The key of the element to remove.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)</br>
**签名**：_d6ac89338dff5e3b</br>
**注释**：

```xml
<summary>        Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />, and copies the element to the <paramref name="value" /> parameter.      </summary>
<param name="key">The key of the element to remove.</param>
<param name="value">The removed element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)</br>
**签名**：_7db4d9112b4ba3c4</br>
**注释**：

```xml
<summary>Gets the value associated with the specified key.</summary>
<param name="key">The key of the value to get.</param>
<param name="value">        When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)</br>
**签名**：_61b63b2c7b14f06a</br>
**注释**：

```xml
<summary>Attempts to add the specified key and value to the dictionary.</summary>
<param name="key">The key of the element to add.</param>
<param name="value">        The value of the element to add. It can be <see langword="null" />.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the key/value pair was added to the dictionary successfully; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)</br>
**签名**：_fdba95f6eefaa760</br>
**注释**：

```xml
<summary>Ensures that the dictionary can hold up to a specified number of entries without any further expansion of its backing storage.</summary>
<param name="capacity">The number of entries.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>
<returns>        The current capacity of the <see cref="T:System.Collections.Generic.Dictionary`2" />.      </returns>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()</br>
**签名**：_44cc5aa04712525c</br>
**注释**：

```xml
<summary>Sets the capacity of this dictionary to what it would be if it had been originally initialized with all its entries.</summary>
```

**成员**：System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)</br>
**签名**：_dd7fceb710b10915</br>
**注释**：

```xml
<summary>Sets the capacity of this dictionary to hold up a specified number of entries without any further expansion of its backing storage.</summary>
<param name="capacity">The new capacity.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than <see cref="P:System.Collections.Generic.Dictionary`2.Count" />.      </exception>
```

