# StackModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.Stack<T>.Stack()</br>
**签名**：_7d15fcc03d17599b</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Stack`1" /> class that is empty and has the default initial capacity.      </summary>
```

**成员**：System.Collections.Generic.Stack<T>.Stack(int)</br>
**签名**：_f4ca5eb8de25d4a3</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Stack`1" /> class that is empty and has the specified initial capacity or the default initial capacity, whichever is greater.      </summary>
<param name="capacity">        The initial number of elements that the <see cref="T:System.Collections.Generic.Stack`1" /> can contain.      </param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.      </exception>
```

**成员**：System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_60d564060ac5fb0f</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Stack`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.      </summary>
<param name="collection">The collection to copy elements from.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.Stack<T>.Count.get</br>
**签名**：_ec97cc120d8d804b</br>

**成员**：System.Collections.Generic.Stack<T>.Capacity.get</br>
**签名**：_621ea9b1c6bf97e6</br>

**成员**：System.Collections.Generic.Stack<T>.Clear()</br>
**签名**：_431a6c983678bc4d</br>
**注释**：

```xml
<summary>Removes all objects from the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
```

**成员**：System.Collections.Generic.Stack<T>.Contains(T)</br>
**签名**：_f8679c85a69f0514</br>
**注释**：

```xml
<summary>Determines whether an element is in the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.Stack`1" />. The value can be <see langword="null" /> for reference types.      </param>
<returns>  <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.Stack`1" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.Stack<T>.CopyTo(T[], int)</br>
**签名**：_effd13f163a27fa6</br>
**注释**：

```xml
<summary>Copies the <see cref="T:System.Collections.Generic.Stack`1" /> to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.      </summary>
<param name="array">        The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Stack`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.      </param>
<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than zero.      </exception>
<exception cref="T:System.ArgumentException">        The number of elements in the source <see cref="T:System.Collections.Generic.Stack`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.      </exception>
```

**成员**：System.Collections.Generic.Stack<T>.GetEnumerator()</br>
**签名**：_d3b630e4cd3c8825</br>
**注释**：

```xml
<summary>Returns an enumerator for the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
<returns>        An <see cref="T:System.Collections.Generic.Stack`1.Enumerator" /> for the <see cref="T:System.Collections.Generic.Stack`1" />.      </returns>
```

**成员**：System.Collections.Generic.Stack<T>.TrimExcess()</br>
**签名**：_7fb66dbd93352570</br>
**注释**：

```xml
<summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.Stack`1" />, if that number is less than 90 percent of current capacity.      </summary>
```

**成员**：System.Collections.Generic.Stack<T>.TrimExcess(int)</br>
**签名**：_b2c5ca3174fe4db9</br>
**注释**：

```xml
<summary>Sets the capacity of a <see cref="T:System.Collections.Generic.Stack`1" /> object to a specified number of entries.      </summary>
<param name="capacity">The new capacity.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Passed capacity is lower than 0 or entries count.</exception>
```

**成员**：System.Collections.Generic.Stack<T>.Peek()</br>
**签名**：_c406861f59a5ccaf</br>
**注释**：

```xml
<summary>Returns the object at the top of the <see cref="T:System.Collections.Generic.Stack`1" /> without removing it.      </summary>
<exception cref="T:System.InvalidOperationException">        The <see cref="T:System.Collections.Generic.Stack`1" /> is empty.      </exception>
<returns>        The object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />.      </returns>
```

**成员**：System.Collections.Generic.Stack<T>.TryPeek(out T)</br>
**签名**：_fa141b6d3bc0d25a</br>
**注释**：

```xml
<summary>Returns a value that indicates whether there is an object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />, and if one is present, copies it to the <paramref name="result" /> parameter. The object is not removed from the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
<param name="result">        If present, the object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />; otherwise, the default value of <typeparamref name="T" />.      </param>
<returns>  <see langword="true" /> if there is an object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />; <see langword="false" /> if the <see cref="T:System.Collections.Generic.Stack`1" /> is empty.      </returns>
```

**成员**：System.Collections.Generic.Stack<T>.Pop()</br>
**签名**：_26474a0aeb01f889</br>
**注释**：

```xml
<summary>Removes and returns the object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
<exception cref="T:System.InvalidOperationException">        The <see cref="T:System.Collections.Generic.Stack`1" /> is empty.      </exception>
<returns>        The object removed from the top of the <see cref="T:System.Collections.Generic.Stack`1" />.      </returns>
```

**成员**：System.Collections.Generic.Stack<T>.TryPop(out T)</br>
**签名**：_247c56433f8b7216</br>
**注释**：

```xml
<summary>Returns a value that indicates whether there is an object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />, and if one is present, copies it to the <paramref name="result" /> parameter, and removes it from the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
<param name="result">        If present, the object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />; otherwise, the default value of <typeparamref name="T" />.      </param>
<returns>  <see langword="true" /> if there is an object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />; <see langword="false" /> if the <see cref="T:System.Collections.Generic.Stack`1" /> is empty.      </returns>
```

**成员**：System.Collections.Generic.Stack<T>.Push(T)</br>
**签名**：_c18157d266fca530</br>
**注释**：

```xml
<summary>Inserts an object at the top of the <see cref="T:System.Collections.Generic.Stack`1" />.      </summary>
<param name="item">        The object to push onto the <see cref="T:System.Collections.Generic.Stack`1" />. The value can be <see langword="null" /> for reference types.      </param>
```

**成员**：System.Collections.Generic.Stack<T>.EnsureCapacity(int)</br>
**签名**：_79a574dc1135fb9a</br>
**注释**：

```xml
<summary>Ensures that the capacity of this Stack is at least the specified <paramref name="capacity" />. If the current capacity is less than <paramref name="capacity" />, it is increased to at least the specified <paramref name="capacity" />.      </summary>
<param name="capacity">The minimum capacity to ensure.</param>
<returns>The new capacity of this stack.</returns>
```

**成员**：System.Collections.Generic.Stack<T>.ToArray()</br>
**签名**：_e40d0cf595a7fe44</br>
**注释**：

```xml
<summary>Copies the <see cref="T:System.Collections.Generic.Stack`1" /> to a new array.      </summary>
<returns>        A new array containing copies of the elements of the <see cref="T:System.Collections.Generic.Stack`1" />.      </returns>
```

