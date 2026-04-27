# ICollectionT1Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.ICollection<T>.Count.get</br>
**签名**：_c325d97a583f4b86</br>

**成员**：System.Collections.Generic.ICollection<T>.IsReadOnly.get</br>
**签名**：_1257c5832793c86d</br>

**成员**：System.Collections.Generic.ICollection<T>.Add(T)</br>
**签名**：_c0023f4a7a67220a</br>
**注释**：

```xml
<summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
<param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
```

**成员**：System.Collections.Generic.ICollection<T>.Clear()</br>
**签名**：_d067c092ac624f6a</br>
**注释**：

```xml
<summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
```

**成员**：System.Collections.Generic.ICollection<T>.Contains(T)</br>
**签名**：_f4e19820d0dc17ec</br>
**注释**：

```xml
<summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
<returns>  <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.Generic.ICollection<T>.CopyTo(T[], int)</br>
**签名**：_03c4a0ae3554065f</br>
**注释**：

```xml
<summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
<param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than 0.</exception>
<exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
```

**成员**：System.Collections.Generic.ICollection<T>.Remove(T)</br>
**签名**：_0a859d3497130ea7</br>
**注释**：

```xml
<summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
<param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
<returns>  <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
```

