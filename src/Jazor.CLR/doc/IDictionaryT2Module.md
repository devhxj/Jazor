# IDictionaryT2Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get</br>
**签名**：_371fad9265e864a1</br>

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].set</br>
**签名**：_f3b177bfce76ed5c</br>

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.Keys.get</br>
**签名**：_a83465399c1d170f</br>

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.Values.get</br>
**签名**：_a48c0eb82bacff74</br>

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.ContainsKey(TKey)</br>
**签名**：_71847e6aeb7b11d0</br>
**注释**：

```xml
<summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
<param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)</br>
**签名**：_93efc3872e59b431</br>
**注释**：

```xml
<summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
<param name="key">The object to use as the key of the element to add.</param>
<param name="value">The object to use as the value of the element to add.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</exception>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
```

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)</br>
**签名**：_fc84b7a31e5cdfe4</br>
**注释**：

```xml
<summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
<param name="key">The key of the element to remove.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
<returns>  <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.  This method also returns <see langword="false" /> if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
```

**成员**：System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)</br>
**签名**：_ebaafc4d4a520807</br>
**注释**：

```xml
<summary>Gets the value associated with the specified key.</summary>
<param name="key">The key whose value to get.</param>
<param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
```

