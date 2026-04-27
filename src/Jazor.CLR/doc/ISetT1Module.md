# ISetT1Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.ISet<T>.Add(T)</br>
**签名**：_fa512a510bd763de</br>
**注释**：

```xml
<summary>Adds an element to the current set and returns a value to indicate if the element was successfully added.</summary>
<param name="item">The element to add to the set.</param>
<returns>  <see langword="true" /> if the element is added to the set; <see langword="false" /> if the element is already in the set.</returns>
```

**成员**：System.Collections.Generic.ISet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_d9af20d6b8c5e775</br>
**注释**：

```xml
<summary>Modifies the current set so that it contains all elements that are present in the current set, in the specified collection, or in both.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
```

**成员**：System.Collections.Generic.ISet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_202b815f92a32e5d</br>
**注释**：

```xml
<summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
```

**成员**：System.Collections.Generic.ISet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_ac98ad1e0ac9efb5</br>
**注释**：

```xml
<summary>Removes all elements in the specified collection from the current set.</summary>
<param name="other">The collection of items to remove from the set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
```

**成员**：System.Collections.Generic.ISet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_07907f6b669e590a</br>
**注释**：

```xml
<summary>Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
```

**成员**：System.Collections.Generic.ISet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_bcd9e5c5cd4a65e1</br>
**注释**：

```xml
<summary>Determines whether a set is a subset of a specified collection.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current set is a subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.ISet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_a64ad5f437ed3887</br>
**注释**：

```xml
<summary>Determines whether the current set is a superset of a specified collection.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current set is a superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.ISet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_f7d6687c6a479566</br>
**注释**：

```xml
<summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current set is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.ISet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_bf1a417a69fffcb2</br>
**注释**：

```xml
<summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current set is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.ISet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_45e2e920f151fad2</br>
**注释**：

```xml
<summary>Determines whether the current set overlaps with the specified collection.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current set and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.ISet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_afabf76c0df51242</br>
**注释**：

```xml
<summary>Determines whether the current set and the specified collection contain the same elements.</summary>
<param name="other">The collection to compare to the current set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current set is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
```

