# IListModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.IList.this[int].get</br>
**签名**：_049fed3e1cad6543</br>

**成员**：System.Collections.IList.this[int].set</br>
**签名**：_d1d1f177e5b9f8db</br>

**成员**：System.Collections.IList.Add(object)</br>
**签名**：_436bcdacebfc9159</br>
**注释**：

```xml
<summary>Adds an item to the <see cref="T:System.Collections.IList" />.</summary>
<param name="value">The object to add to the <see cref="T:System.Collections.IList" />.</param>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.     -or-     The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
<returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
```

**成员**：System.Collections.IList.Contains(object)</br>
**签名**：_1162c32e927a9e4a</br>
**注释**：

```xml
<summary>Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.</summary>
<param name="value">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
<returns>  <see langword="true" /> if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Collections.IList.Clear()</br>
**签名**：_00d8476a94b1a75c</br>
**注释**：

```xml
<summary>Removes all items from the <see cref="T:System.Collections.IList" />.</summary>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.</exception>
```

**成员**：System.Collections.IList.IsReadOnly.get</br>
**签名**：_2ce407a9d9be8186</br>

**成员**：System.Collections.IList.IsFixedSize.get</br>
**签名**：_b17a6c1583e0a5af</br>

**成员**：System.Collections.IList.IndexOf(object)</br>
**签名**：_3a9e7f97e5f886b1</br>
**注释**：

```xml
<summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
<param name="value">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
<returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
```

**成员**：System.Collections.IList.Insert(int, object)</br>
**签名**：_9e2711121aad1093</br>
**注释**：

```xml
<summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
<param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
<param name="value">The object to insert into the <see cref="T:System.Collections.IList" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.     -or-     The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
<exception cref="T:System.NullReferenceException">  <paramref name="value" /> is null reference in the <see cref="T:System.Collections.IList" />.</exception>
```

**成员**：System.Collections.IList.Remove(object)</br>
**签名**：_305c8313418aa043</br>
**注释**：

```xml
<summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.</summary>
<param name="value">The object to remove from the <see cref="T:System.Collections.IList" />.</param>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.     -or-     The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
```

**成员**：System.Collections.IList.RemoveAt(int)</br>
**签名**：_72d07d6eb16afece</br>
**注释**：

```xml
<summary>Removes the <see cref="T:System.Collections.IList" /> item at the specified index.</summary>
<param name="index">The zero-based index of the item to remove.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.     -or-     The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
```

