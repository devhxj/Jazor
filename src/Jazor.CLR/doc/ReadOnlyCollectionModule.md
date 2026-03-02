# ReadOnlyCollectionModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)</br>
**签名**：_d4e5f6a7b8c9d0e1</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> class that is a read-only wrapper around the specified list.</summary>
<param name="list">The list to wrap.</param>
<exception cref="T:System.ArgumentNullException"><paramref name="list" /> is <see langword="null" />.</exception>
```

**成员**：static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get</br>
**签名**：_e5f6a7b8c9d0e1f2</br>

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.Count.get</br>
**签名**：_f6a7b8c9d0e1f2a3</br>
**注释**：

```xml
<summary>Gets the number of elements contained in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</summary>
<returns>The number of elements contained in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</returns>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.Contains(T)</br>
**签名**：_a7b8c9d0e1f2a3b4</br>
**注释**：

```xml
<summary>Determines whether an element is in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</summary>
<param name="value">The object to locate in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />. The value can be <see langword="null" /> for reference types.</param>
<returns><see langword="true" /> if <paramref name="value" /> is found in the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get</br>
**签名**：_b8c9d0e1f2a3b4c5</br>
**注释**：

```xml
<summary>Gets the element at the specified index.</summary>
<param name="index">The zero-based index of the element to get.</param>
<returns>The element at the specified index.</returns>
<exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.ReadOnlyCollection`1.Count" />.</exception>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.IndexOf(T)</br>
**签名**：_c9d0e1f2a3b4c5d6</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</summary>
<param name="value">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />. The value can be <see langword="null" /> for reference types.</param>
<returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />, if found; otherwise, -1.</returns>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])</br>
**签名**：_d0e1f2a3b4c5d6e7</br>
**注释**：

```xml
<summary>Copies the entire <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the beginning of the target array.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
<exception cref="T:System.ArgumentNullException"><paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> is greater than the number of elements that the destination <see cref="T:System.Array" /> can contain.</exception>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)</br>
**签名**：_e1f2a3b4c5d6e7f8</br>
**注释**：

```xml
<summary>Copies the entire <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
<param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
<exception cref="T:System.ArgumentNullException"><paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <see cref="T:System.Array" />.</exception>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)</br>
**签名**：_f2a3b4c5d6e7f8a9</br>
**注释**：

```xml
<summary>Copies a range of elements from the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
<param name="index">The zero-based index in the source <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> at which copying begins.</param>
<param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
<param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
<param name="count">The number of elements to copy.</param>
<exception cref="T:System.ArgumentNullException"><paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.-or-<paramref name="arrayIndex" /> is less than zero.-or-<paramref name="count" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException"><paramref name="index" /> is equal to or greater than the <see cref="P:System.Collections.ObjectModel.ReadOnlyCollection`1.Count" /> of the source <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.-or-The number of elements from <paramref name="index" /> to the end of the source <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <see cref="T:System.Array" />.</exception>
```

**成员**：System.Collections.ObjectModel.ReadOnlyCollection<T>.GetEnumerator()</br>
**签名**：_a3b4c5d6e7f8a9b0</br>
**注释**：

```xml
<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</summary>
<returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> for the <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</returns>
```

**成员**：static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)</br>
**签名**：_a0cccd63a3a3eee1</br>

**成员**：static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)</br>
**签名**：_b80678a096dde585</br>

