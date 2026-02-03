# HashSetModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Collections.Generic.HashSet<T>.HashSet()</br>
**签名**：_55c044d94c5b0ca8</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the default equality comparer for the set type.      </summary>
```

**成员**：System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)</br>
**签名**：_3a131c59650baae9</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.      </summary>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.      </param>
```

**成员**：System.Collections.Generic.HashSet<T>.HashSet(int)</br>
**签名**：_304904fb5a22f950</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty, but has reserved space for <paramref name="capacity" /> items and uses the default equality comparer for the set type.      </summary>
<param name="capacity">        The initial size of the <see cref="T:System.Collections.Generic.HashSet`1" />.      </param>
```

**成员**：System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_1bd2e054852d9d5f</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the default equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.      </summary>
<param name="collection">The collection whose elements are copied to the new set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)</br>
**签名**：_fe5bb664d9f9c877</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.      </summary>
<param name="collection">The collection whose elements are copied to the new set.</param>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)</br>
**签名**：_baf729bee477b2e7</br>
**注释**：

```xml
<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, and has sufficient capacity to accommodate <paramref name="capacity" /> elements.      </summary>
<param name="capacity">        The initial size of the <see cref="T:System.Collections.Generic.HashSet`1" />.      </param>
<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null (Nothing in Visual Basic) to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation for the set type.      </param>
```

**成员**：System.Collections.Generic.HashSet<T>.Clear()</br>
**签名**：_56d632bf48c92530</br>
**注释**：

```xml
<summary>        Removes all elements from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
```

**成员**：System.Collections.Generic.HashSet<T>.Contains(T)</br>
**签名**：_32b989c96ea23e8c</br>
**注释**：

```xml
<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element.      </summary>
<param name="item">        The element to locate in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.Remove(T)</br>
**签名**：_cfb963650cb3dabd</br>
**注释**：

```xml
<summary>        Removes the specified element from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
<param name="item">The element to remove.</param>
<returns>  <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="item" /> is not found in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.Count.get</br>
**签名**：_4bec0b4d27073edb</br>

**成员**：System.Collections.Generic.HashSet<T>.Capacity.get</br>
**签名**：_97c019008a0c8260</br>

**成员**：System.Collections.Generic.HashSet<T>.GetAlternateLookup<TAlternate>()</br>
**签名**：_3ed41a9b4870a040</br>
**注释**：

```xml
<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.      </summary>
<typeparam name="TAlternate">The alternate type of instance for performing lookups.</typeparam>
<returns>The created lookup instance.</returns>
```

**成员**：System.Collections.Generic.HashSet<T>.TryGetAlternateLookup<TAlternate>(out System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate>)</br>
**签名**：_859aac4462f2d063</br>
**注释**：

```xml
<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.      </summary>
<param name="lookup">The created lookup instance when the method returns true, or a default instance that should not be used if the method returns false.</param>
<typeparam name="TAlternate">The alternate type of instance for performing lookups.</typeparam>
<returns>  <see langword="true" /> if a lookup could be created; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.GetEnumerator()</br>
**签名**：_68a59c6ba9ebe57d</br>
**注释**：

```xml
<summary>        Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
<returns>        A <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object for the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </returns>
```

**成员**：virtual System.Collections.Generic.HashSet<T>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)</br>
**签名**：_8f2db3c5ff390af9</br>
**注释**：

```xml
<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
<param name="info">        A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<param name="context">        A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="info" /> is <see langword="null" />.      </exception>
```

**成员**：virtual System.Collections.Generic.HashSet<T>.OnDeserialization(object)</br>
**签名**：_26975bd136a2f896</br>
**注释**：

```xml
<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.      </summary>
<param name="sender">The source of the deserialization event.</param>
<exception cref="T:System.Runtime.Serialization.SerializationException">        The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.HashSet`1" /> object is invalid.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.Add(T)</br>
**签名**：_e1d2ba750a2788cb</br>
**注释**：

```xml
<summary>Adds the specified element to a set.</summary>
<param name="item">The element to add to the set.</param>
<returns>  <see langword="true" /> if the element is added to the <see cref="T:System.Collections.Generic.HashSet`1" /> object; <see langword="false" /> if the element is already present.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)</br>
**签名**：_20eb460b32c63404</br>
**注释**：

```xml
<summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
<param name="equalValue">The value to search for.</param>
<param name="actualValue">The value from the set that the search found, or the default value of T when the search yielded no match.</param>
<returns>A value indicating whether the search was successful.</returns>
```

**成员**：System.Collections.Generic.HashSet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_b2bd5d22aadd44a8</br>
**注释**：

```xml
<summary>        Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain all elements that are present in itself, the specified collection, or both.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_3a6a072035334578</br>
**注释**：

```xml
<summary>        Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present in that object and in the specified collection.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_373e2e9ed1fb3f5b</br>
**注释**：

```xml
<summary>        Removes all elements in the specified collection from the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
<param name="other">        The collection of items to remove from the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_a22fe44dc0ae9ad2</br>
**注释**：

```xml
<summary>        Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present either in that object or in the specified collection, but not both.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_23c8bcfc6b71d2b1</br>
**注释**：

```xml
<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of the specified collection.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_fb8566ae66aa9591</br>
**注释**：

```xml
<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of the specified collection.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_3be7fbb1d68799fb</br>
**注释**：

```xml
<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of the specified collection.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_cc0cc2d0f5be70db</br>
**注释**：

```xml
<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of the specified collection.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_84709aa8ff70a52a</br>
**注释**：

```xml
<summary>        Determines whether the current <see cref="T:System.Collections.Generic.HashSet`1" /> object and a specified collection share common elements.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_55425d259e5f54ea</br>
**注释**：

```xml
<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object and the specified collection contain the same elements.      </summary>
<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is equal to <paramref name="other" />; otherwise, <see langword="false" />.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.CopyTo(T[])</br>
**签名**：_614185e6ff9ff9fd</br>
**注释**：

```xml
<summary>        Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.      </summary>
<param name="array">        The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.CopyTo(T[], int)</br>
**签名**：_9ac2dfb153a1d53c</br>
**注释**：

```xml
<summary>        Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.      </summary>
<param name="array">        The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.      </param>
<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than 0.      </exception>
<exception cref="T:System.ArgumentException">  <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)</br>
**签名**：_622a881b75871c97</br>
**注释**：

```xml
<summary>        Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.      </summary>
<param name="array">        The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.      </param>
<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
<param name="count">        The number of elements to copy to <paramref name="array" />.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
<exception cref="T:System.ArgumentException">  <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.        -or-        <paramref name="count" /> is greater than the available space from the <paramref name="index" /> to the end of the destination <paramref name="array" />.      </exception>
```

**成员**：System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)</br>
**签名**：_112079825eb01119</br>
**注释**：

```xml
<summary>        Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.      </summary>
<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.      </param>
<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
<returns>        The number of elements that were removed from the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.      </returns>
```

**成员**：System.Collections.Generic.HashSet<T>.Comparer.get</br>
**签名**：_0c0d81e2205a9cb9</br>

**成员**：System.Collections.Generic.HashSet<T>.EnsureCapacity(int)</br>
**签名**：_b53dcd5d4f0c57d7</br>
**注释**：

```xml
<summary>Ensures that this hash set can hold the specified number of elements without any further expansion of its backing storage.</summary>
<param name="capacity">The minimum capacity to ensure.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.      </exception>
<returns>The new capacity of this instance.</returns>
```

**成员**：System.Collections.Generic.HashSet<T>.TrimExcess()</br>
**签名**：_09f9b6aba126decb</br>
**注释**：

```xml
<summary>        Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.      </summary>
```

**成员**：System.Collections.Generic.HashSet<T>.TrimExcess(int)</br>
**签名**：_e4dd8faf507013ad</br>
**注释**：

```xml
<summary>        Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the specified number of entries, rounded up to a nearby, implementation-specific value.      </summary>
<param name="capacity">The new capacity.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The specified capacity is lower than the count of entries.</exception>
```

**成员**：static System.Collections.Generic.HashSet<T>.CreateSetComparer()</br>
**签名**：_2d028c1bc3e2f479</br>
**注释**：

```xml
<summary>        Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
<returns>        An <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for deep equality testing of the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </returns>
```

