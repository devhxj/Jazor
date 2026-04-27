# ConditionalWeakTableT2Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.ConditionalWeakTable()</br>
**签名**：_925d15e28de85fd7</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.ConditionalWeakTable`2" /> class.</summary>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)</br>
**签名**：_8360443cbe5b1f88</br>
**注释**：

```xml
<summary>Gets the value of the specified key.</summary>
<param name="key">The key that represents an object with an attached property.</param>
<param name="value">When this method returns, contains the attached property value. If <paramref name="key" /> is not found, <paramref name="value" /> contains the default value.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if <paramref name="key" /> is found; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)</br>
**签名**：_c013f77a250570ce</br>
**注释**：

```xml
<summary>Adds a key to the table.</summary>
<param name="key">The key to add. <paramref name="key" /> represents the object to which the property is attached.</param>
<param name="value">The key's property value.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="key" /> already exists.</exception>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)</br>
**签名**：_6a785a77d1b78937</br>
**注释**：

```xml
<summary>Adds a key to the table if it doesn't already exist.</summary>
<param name="key">The key to add.</param>
<param name="value">The key's property value.</param>
<returns>  <see langword="true" /> if the key/value pair was added; <see langword="false" /> if the table already contained the key.</returns>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.AddOrUpdate(TKey, TValue)</br>
**签名**：_3e5ae776a9edba7b</br>
**注释**：

```xml
<summary>Adds the key and value if the key doesn't exist, or updates the existing key's value if it does exist.</summary>
<param name="key">The key to add or update. May not be <see langword="null" />.</param>
<param name="value">The value to associate with <paramref name="key" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)</br>
**签名**：_0b5841f143b2e9e7</br>
**注释**：

```xml
<summary>Removes a key and its value from the table.</summary>
<param name="key">The key to remove.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the key is found and removed; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)</br>
**签名**：_14e40010b1fd2993</br>

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()</br>
**签名**：_57912eda7fd377bb</br>
**注释**：

```xml
<summary>Clears all the key/value pairs.</summary>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)</br>
**签名**：_8e3321f2e6fa2499</br>

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, System.Func<TKey, TValue>)</br>
**签名**：_ed09a626bf4f3ea8</br>

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)</br>
**签名**：_eaeddd47f4a65d81</br>

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetValue(TKey, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback)</br>
**签名**：_43edc29b01c6a1f0</br>
**注释**：

```xml
<summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes a callback method to create a value that is bound to the specified key.</summary>
<param name="key">The key to search for. <paramref name="key" /> represents the object to which the property is attached.</param>
<param name="createValueCallback">A delegate to a method that can create a value for the given <paramref name="key" />. It has a single parameter of type <c>TKey</c>, and returns a value of type <c>TValue</c>.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> or <paramref name="createValueCallback" /> is <see langword="null" />.</exception>
<returns>The value attached to <paramref name="key" />, if <paramref name="key" /> already exists in the table; otherwise, the new value returned by the <paramref name="createValueCallback" /> delegate.</returns>
```

**成员**：System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrCreateValue(TKey)</br>
**签名**：_8e97651a27c54464</br>
**注释**：

```xml
<summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes the parameterless constructor of the class that represents the table's value to create a value that is bound to the specified key.</summary>
<param name="key">The key to search for. <paramref name="key" /> represents the object to which the property is attached.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
<exception cref="T:System.MissingMethodException">The class that represents the table's value does not define a parameterless constructor.Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MissingMemberException" />, instead.</exception>
<returns>The value that corresponds to <paramref name="key" />, if <paramref name="key" /> already exists in the table; otherwise, a new value created by the parameterless constructor of the class defined by the <paramref name="TValue" /> generic type parameter.</returns>
```

