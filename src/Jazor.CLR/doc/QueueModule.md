# QueueModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.Queue<T>.Queue()</br>
**签名**：_ea05a56d08fbd4f9</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1" /> class that is empty and has the default initial capacity.      </summary>
```

**成员**：System.Collections.Generic.Queue<T>.Queue(int)</br>
**签名**：_7fc2b76467c43db9</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1" /> class that is empty and has the specified initial capacity.      </summary>
<param name="capacity">        The initial number of elements that the <see cref="T:System.Collections.Generic.Queue`1" /> can contain.      </param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.      </exception>
```

**成员**：System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_5eae085d83bbe242</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.      </summary>
<param name="collection">        The collection whose elements are copied to the new <see cref="T:System.Collections.Generic.Queue`1" />.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.Queue<T>.Count.get</br>
**签名**：_874ffef6d586566e</br>

**成员**：System.Collections.Generic.Queue<T>.Capacity.get</br>
**签名**：_5ae268005c3a02f2</br>

**成员**：System.Collections.Generic.Queue<T>.Clear()</br>
**签名**：_c1380aa32ab3b19e</br>
**注释**：

```xml
<summary>Removes all objects from the <see cref="T:System.Collections.Generic.Queue`1" />.      </summary>
```

**成员**：System.Collections.Generic.Queue<T>.CopyTo(T[], int)</br>
**签名**：_2a0d34892866da9f</br>
**注释**：

```xml
<summary>Copies the <see cref="T:System.Collections.Generic.Queue`1" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.      </summary>
<param name="array">        The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Queue`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.      </param>
<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than zero.      </exception>
<exception cref="T:System.ArgumentException">        The number of elements in the source <see cref="T:System.Collections.Generic.Queue`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.      </exception>
```

**成员**：System.Collections.Generic.Queue<T>.Enqueue(T)</br>
**签名**：_8a87022169c02c22</br>
**注释**：

```xml
<summary>Adds an object to the end of the <see cref="T:System.Collections.Generic.Queue`1" />.      </summary>
<param name="item">        The object to add to the <see cref="T:System.Collections.Generic.Queue`1" />. The value can be <see langword="null" /> for reference types.      </param>
```

**成员**：System.Collections.Generic.Queue<T>.GetEnumerator()</br>
**签名**：_7cd7aaeba0a5e133</br>
**注释**：

```xml
<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Queue`1" />.      </summary>
<returns>        An <see cref="T:System.Collections.Generic.Queue`1.Enumerator" /> for the <see cref="T:System.Collections.Generic.Queue`1" />.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.Dequeue()</br>
**签名**：_9828432fec9d535a</br>
**注释**：

```xml
<summary>Removes and returns the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />.      </summary>
<exception cref="T:System.InvalidOperationException">        The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.      </exception>
<returns>        The object that is removed from the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.TryDequeue(out T)</br>
**签名**：_96c6e0d13a99b6ff</br>
**注释**：

```xml
<summary>Removes the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />, and copies it to the <paramref name="result" /> parameter.      </summary>
<param name="result">The removed object.</param>
<returns>  <see langword="true" /> if the object is successfully removed; <see langword="false" /> if the <see cref="T:System.Collections.Generic.Queue`1" /> is empty.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.Peek()</br>
**签名**：_e17f3e583930e78f</br>
**注释**：

```xml
<summary>Returns the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" /> without removing it.      </summary>
<exception cref="T:System.InvalidOperationException">        The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.      </exception>
<returns>        The object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.TryPeek(out T)</br>
**签名**：_35559a67cebb0fd9</br>
**注释**：

```xml
<summary>Returns a value that indicates whether there is an object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />, and if one is present, copies it to the <paramref name="result" /> parameter. The object is not removed from the <see cref="T:System.Collections.Generic.Queue`1" />.      </summary>
<param name="result">        If present, the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />; otherwise, the default value of <typeparamref name="T" />.      </param>
<returns>  <see langword="true" /> if there is an object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1" />; <see langword="false" /> if the <see cref="T:System.Collections.Generic.Queue`1" /> is empty.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.Contains(T)</br>
**签名**：_45549ae297d2d16d</br>
**注释**：

```xml
<summary>Determines whether an element is in the <see cref="T:System.Collections.Generic.Queue`1" />.      </summary>
<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.Queue`1" />. The value can be <see langword="null" /> for reference types.      </param>
<returns>  <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.Queue`1" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.ToArray()</br>
**签名**：_8cda2376e71ddbd2</br>
**注释**：

```xml
<summary>Copies the <see cref="T:System.Collections.Generic.Queue`1" /> elements to a new array.      </summary>
<returns>        A new array containing elements copied from the <see cref="T:System.Collections.Generic.Queue`1" />.      </returns>
```

**成员**：System.Collections.Generic.Queue<T>.TrimExcess()</br>
**签名**：_0fee8ff2db680bf0</br>
**注释**：

```xml
<summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.Queue`1" />, if that number is less than 90 percent of current capacity.      </summary>
```

**成员**：System.Collections.Generic.Queue<T>.TrimExcess(int)</br>
**签名**：_eb46bcf16cd114b9</br>
**注释**：

```xml
<summary>Sets the capacity of a <see cref="T:System.Collections.Generic.Queue`1" /> object to the specified number of entries.      </summary>
<param name="capacity">The new capacity.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
```

**成员**：System.Collections.Generic.Queue<T>.EnsureCapacity(int)</br>
**签名**：_0acf245a52678e55</br>
**注释**：

```xml
<summary>Ensures that the capacity of this queue is at least the specified <paramref name="capacity" />. If the current capacity is less than <paramref name="capacity" />, it is increased to at least the specified <paramref name="capacity" />.      </summary>
<param name="capacity">The minimum capacity to ensure.</param>
<returns>The new capacity of this queue.</returns>
```

