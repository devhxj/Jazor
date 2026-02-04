# ArrayModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Array.Length.get</br>
**签名**：_fdebc1c5c62f33cc</br>

**成员**：System.Array.LongLength.get</br>
**签名**：_82dc944f60373152</br>

**成员**：System.Array.Rank.get</br>
**签名**：_6ab1259f55d0dd24</br>

**成员**：System.Array.Initialize()</br>
**签名**：_a93e4c6dc74a4cff</br>
**注释**：

```xml
<summary>Initializes every element of the value-type <see cref="T:System.Array" /> by calling the parameterless constructor of the value type.</summary>
```

**成员**：static System.Array.AsReadOnly<T>(T[])</br>
**签名**：_abd52ebcdb6fefcb</br>
**注释**：

```xml
<summary>Returns a read-only wrapper for the specified array.</summary>
<param name="array">The one-dimensional, zero-based array to wrap in a read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> wrapper.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<returns>A read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> wrapper for the specified array.</returns>
```

**成员**：static System.Array.Resize<T>(ref T[], int)</br>
**签名**：_127013d39cf5bff9</br>
**注释**：

```xml
<summary>Changes the number of elements of a one-dimensional array to the specified new size.</summary>
<param name="array">The one-dimensional, zero-based array to resize, or <see langword="null" /> to create a new array with the specified size.</param>
<param name="newSize">The size of the new array.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="newSize" /> is less than zero.</exception>
```

**成员**：static System.Array.CreateInstance(System.Type, int)</br>
**签名**：_7cf4f1d72cf2dca7</br>
**注释**：

```xml
<summary>Creates a one-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and length, with zero-based indexing.</summary>
<param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
<param name="length">The size of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="elementType" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or- <paramref name="elementType" /> is an open generic type.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length" /> is less than zero.</exception>
<returns>A new one-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length, using zero-based indexing.</returns>
```

**成员**：static System.Array.CreateInstance(System.Type, int, int)</br>
**签名**：_3800bc5f99a65eb7</br>
**注释**：

```xml
<summary>Creates a two-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
<param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
<param name="length1">The size of the first dimension of the <see cref="T:System.Array" /> to create.</param>
<param name="length2">The size of the second dimension of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="elementType" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or- <paramref name="elementType" /> is an open generic type.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length1" /> is less than zero. -or- <paramref name="length2" /> is less than zero.</exception>
<returns>A new two-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
```

**成员**：static System.Array.CreateInstance(System.Type, int, int, int)</br>
**签名**：_946705c3abbbb67c</br>
**注释**：

```xml
<summary>Creates a three-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
<param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
<param name="length1">The size of the first dimension of the <see cref="T:System.Array" /> to create.</param>
<param name="length2">The size of the second dimension of the <see cref="T:System.Array" /> to create.</param>
<param name="length3">The size of the third dimension of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="elementType" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or- <paramref name="elementType" /> is an open generic type.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length1" /> is less than zero. -or- <paramref name="length2" /> is less than zero. -or- <paramref name="length3" /> is less than zero.</exception>
<returns>A new three-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
```

**成员**：static System.Array.CreateInstance(System.Type, params int[])</br>
**签名**：_55c950cf5ea775e9</br>
**注释**：

```xml
<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing. The dimension lengths are specified in an array of 32-bit integers.</summary>
<param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
<param name="lengths">An array of 32-bit integers that represent the size of each dimension of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="elementType" /> is <see langword="null" />. -or- <paramref name="lengths" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />. -or- The <paramref name="lengths" /> array contains less than one element.</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or- <paramref name="elementType" /> is an open generic type.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero.</exception>
<returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
```

**成员**：static System.Array.CreateInstance(System.Type, int[], int[])</br>
**签名**：_81e3451a7be5290d</br>
**注释**：

```xml
<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with the specified lower bounds.</summary>
<param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
<param name="lengths">A one-dimensional array that contains the size of each dimension of the <see cref="T:System.Array" /> to create.</param>
<param name="lowerBounds">A one-dimensional array that contains the lower bound (starting index) of each dimension of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="elementType" /> is <see langword="null" />. -or- <paramref name="lengths" /> is <see langword="null" />. -or- <paramref name="lowerBounds" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />. -or- The <paramref name="lengths" /> array contains less than one element. -or- The <paramref name="lengths" /> and <paramref name="lowerBounds" /> arrays do not contain the same number of elements.</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or- <paramref name="elementType" /> is an open generic type.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero. -or- Any value in <paramref name="lowerBounds" /> is very large, such that the sum of a dimension's lower bound and length is greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length and lower bound for each dimension.</returns>
```

**成员**：static System.Array.CreateInstance(System.Type, params long[])</br>
**签名**：_d1e6f82b64452f99</br>
**注释**：

```xml
<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing. The dimension lengths are specified in an array of 64-bit integers.</summary>
<param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
<param name="lengths">An array of 64-bit integers that represent the size of each dimension of the <see cref="T:System.Array" /> to create. Each integer in the array must be between zero and <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>, inclusive.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="elementType" /> is <see langword="null" />. -or- <paramref name="lengths" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />. -or- The <paramref name="lengths" /> array contains less than one element.</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or- <paramref name="elementType" /> is an open generic type.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
```

**成员**：static System.Array.CreateInstanceFromArrayType(System.Type, int)</br>
**签名**：_8d8c533adf78f2c2</br>
**注释**：

```xml
<summary>Creates a one-dimensional <see cref="T:System.Array" /> of the specified array type and length, with zero-based indexing.</summary>
<param name="arrayType">The type of the array (not of the array element type).</param>
<param name="length">The size of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="arrayType" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length" /> is negative.</exception>
<exception cref="T:System.ArgumentException">  <para>    <paramref name="arrayType" /> is not an array type.</para>  <para>-or-</para>  <para>    <paramref name="arrayType" /> is not one-dimensional array.</para></exception>
<returns>A new one-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length.</returns>
```

**成员**：static System.Array.CreateInstanceFromArrayType(System.Type, params int[])</br>
**签名**：_11529b7770340ce8</br>
**注释**：

```xml
<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
<param name="arrayType">The type of the array (not of the array element type).</param>
<param name="lengths">The dimension lengths, specified in an array of 32-bit integers.</param>
<exception cref="T:System.ArgumentNullException">  <para>    <paramref name="arrayType" /> is <see langword="null" />.</para>  <para>-or-</para>  <para>    <paramref name="lengths" /> is <see langword="null" />.</para></exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <para>The lengths array is empty.</para>  <para>-or-</para>  <para>    <paramref name="arrayType" /> is not an array type.</para>  <para>-or-</para>  <para>    <paramref name="arrayType" /> rank does not match <paramref name="lengths" /> length.</para></exception>
<returns>A new multidimensional <see cref="T:System.Array" /> of the specified Type with the specified length for each dimension, using zero-based indexing.</returns>
```

**成员**：static System.Array.CreateInstanceFromArrayType(System.Type, int[], int[])</br>
**签名**：_c78b33d4f8633a9b</br>
**注释**：

```xml
<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with the specified lower bounds.</summary>
<param name="arrayType">The type of the array (not of the array element type).</param>
<param name="lengths">The dimension lengths, specified in an array of 32-bit integers.</param>
<param name="lowerBounds">A one-dimensional array that contains the lower bound (starting index) of each dimension of the <see cref="T:System.Array" /> to create.</param>
<exception cref="T:System.ArgumentNullException">  <para>    <paramref name="arrayType" /> is <see langword="null" />.</para>  <para>-or-</para>  <para>    <paramref name="lengths" /> is <see langword="null" />.</para>  <para>-or-</para>  <para>    <paramref name="lowerBounds" /> is <see langword="null" />.</para></exception>
<exception cref="T:System.ArgumentException">  <para>The <paramref name="lengths" /> and <paramref name="lowerBounds" /> arrays do not contain the same number of elements.</para>  <para>-or-</para>  <para>The lengths array is empty.</para>  <para>-or-</para>  <para>    <paramref name="arrayType" /> is not an array type.</para>  <para>-or-</para>  <para>    <paramref name="arrayType" /> rank does not match <paramref name="lengths" /> length.</para></exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero.</exception>
<exception cref="T:System.PlatformNotSupportedException">Native AOT: any value in <paramref name="lowerBounds" /> is different than zero.</exception>
<returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length and lower bound for each dimension.</returns>
```

**成员**：static System.Array.Copy(System.Array, System.Array, long)</br>
**签名**：_7a3d7a78ff429283</br>
**注释**：

```xml
<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 64-bit integer.</summary>
<param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
<param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
<param name="length">A 64-bit integer that represents the number of elements to copy. The integer must be between zero and <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>, inclusive.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="sourceArray" /> is <see langword="null" />. -or- <paramref name="destinationArray" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
<exception cref="T:System.ArrayTypeMismatchException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
<exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />. -or- <paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
```

**成员**：static System.Array.Copy(System.Array, long, System.Array, long, long)</br>
**签名**：_e2bd26f0b897dcdc</br>
**注释**：

```xml
<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 64-bit integers.</summary>
<param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
<param name="sourceIndex">A 64-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
<param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
<param name="destinationIndex">A 64-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
<param name="length">A 64-bit integer that represents the number of elements to copy. The integer must be between zero and <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>, inclusive.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="sourceArray" /> is <see langword="null" />. -or- <paramref name="destinationArray" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
<exception cref="T:System.ArrayTypeMismatchException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
<exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" /> is outside the range of valid indexes for the <paramref name="sourceArray" />. -or- <paramref name="destinationIndex" /> is outside the range of valid indexes for the <paramref name="destinationArray" />. -or- <paramref name="length" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />. -or- <paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
```

**成员**：static System.Array.ConstrainedCopy(System.Array, int, System.Array, int, int)</br>
**签名**：_e83857a6975e2bca</br>
**注释**：

```xml
<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index.  Guarantees that all changes are undone if the copy does not succeed completely.</summary>
<param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
<param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
<param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
<param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
<param name="length">A 32-bit integer that represents the number of elements to copy.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="sourceArray" /> is <see langword="null" />. -or- <paramref name="destinationArray" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
<exception cref="T:System.ArrayTypeMismatchException">The <paramref name="sourceArray" /> type is neither the same as nor derived from the <paramref name="destinationArray" /> type.</exception>
<exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />. -or- <paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />. -or- <paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
```

**成员**：static System.Array.Copy(System.Array, System.Array, int)</br>
**签名**：_236e3a8894f7381f</br>
**注释**：

```xml
<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 32-bit integer.</summary>
<param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
<param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
<param name="length">A 32-bit integer that represents the number of elements to copy.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="sourceArray" /> is <see langword="null" />. -or- <paramref name="destinationArray" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
<exception cref="T:System.ArrayTypeMismatchException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
<exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />. -or- <paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
```

**成员**：static System.Array.Copy(System.Array, int, System.Array, int, int)</br>
**签名**：_5afb5659a201668f</br>
**注释**：

```xml
<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 32-bit integers.</summary>
<param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
<param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
<param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
<param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
<param name="length">A 32-bit integer that represents the number of elements to copy.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="sourceArray" /> is <see langword="null" />. -or- <paramref name="destinationArray" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
<exception cref="T:System.ArrayTypeMismatchException">  <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
<exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />. -or- <paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />. -or- <paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
```

**成员**：static System.Array.Clear(System.Array)</br>
**签名**：_96774f9ec153a919</br>
**注释**：

```xml
<summary>Clears the contents of an array.</summary>
<param name="array">The array to clear.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
```

**成员**：static System.Array.Clear(System.Array, int, int)</br>
**签名**：_e6e9140591777519</br>
**注释**：

```xml
<summary>Sets a range of elements in an array to the default value of each element type.</summary>
<param name="array">The array whose elements need to be cleared.</param>
<param name="index">The starting index of the range of elements to clear.</param>
<param name="length">The number of elements to clear.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero. -or- The sum of <paramref name="index" /> and <paramref name="length" /> is greater than the size of <paramref name="array" />.</exception>
```

**成员**：System.Array.GetLength(int)</br>
**签名**：_4a62a6d3092e758c</br>
**注释**：

```xml
<summary>Gets a 32-bit integer that represents the number of elements in the specified dimension of the <see cref="T:System.Array" />.</summary>
<param name="dimension">A zero-based dimension of the <see cref="T:System.Array" /> whose length needs to be determined.</param>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="dimension" /> is less than zero. -or- <paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
<returns>A 32-bit integer that represents the number of elements in the specified dimension.</returns>
```

**成员**：System.Array.GetUpperBound(int)</br>
**签名**：_240013ed6fb455ce</br>
**注释**：

```xml
<summary>Gets the index of the last element of the specified dimension in the array.</summary>
<param name="dimension">A zero-based dimension of the array whose upper bound needs to be determined.</param>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="dimension" /> is less than zero. -or- <paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
<returns>The index of the last element of the specified dimension in the array, or -1 if the specified dimension is empty.</returns>
```

**成员**：System.Array.GetLowerBound(int)</br>
**签名**：_de93a1deaab12d20</br>
**注释**：

```xml
<summary>Gets the index of the first element of the specified dimension in the array.</summary>
<param name="dimension">A zero-based dimension of the array whose starting index needs to be determined.</param>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="dimension" /> is less than zero. -or- <paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
<returns>The index of the first element of the specified dimension in the array.</returns>
```

**成员**：System.Array.GetValue(params int[])</br>
**签名**：_e938260256ca4a08</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 32-bit integers.</summary>
<param name="indices">A one-dimensional array of 32-bit integers that represent the indexes specifying the position of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="indices" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
<exception cref="T:System.IndexOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the multidimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.GetValue(int)</br>
**签名**：_eba14f0435c17445</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 32-bit integer.</summary>
<param name="index">A 32-bit integer that represents the position of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the one-dimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.GetValue(int, int)</br>
**签名**：_c479de104d41183c</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
<param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
<exception cref="T:System.IndexOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the two-dimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.GetValue(int, int, int)</br>
**签名**：_a9dc664f06ce55a4</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
<param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<param name="index3">A 32-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the three-dimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.SetValue(object, int)</br>
**签名**：_1f2a45eb847a2ec4</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 32-bit integer.</summary>
<param name="value">The new value for the specified element.</param>
<param name="index">A 32-bit integer that represents the position of the <see cref="T:System.Array" /> element to set.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.SetValue(object, int, int)</br>
**签名**：_7ca03dfc64fd5640</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
<param name="value">The new value for the specified element.</param>
<param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.IndexOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.SetValue(object, int, int, int)</br>
**签名**：_a8dff91417f83303</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
<param name="value">The new value for the specified element.</param>
<param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<param name="index3">A 32-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.SetValue(object, params int[])</br>
**签名**：_8752076a83fbb3f1</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 32-bit integers.</summary>
<param name="value">The new value for the specified element.</param>
<param name="indices">A one-dimensional array of 32-bit integers that represent the indexes specifying the position of the element to set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="indices" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.IndexOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.GetValue(long)</br>
**签名**：_99c592f7140b4f20</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 64-bit integer.</summary>
<param name="index">A 64-bit integer that represents the position of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the one-dimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.GetValue(long, long)</br>
**签名**：_2bad686c503b1e40</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
<param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the two-dimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.GetValue(long, long, long)</br>
**签名**：_8e8e4b0752cd3155</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
<param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<param name="index3">A 64-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the three-dimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.GetValue(params long[])</br>
**签名**：_6a12948779406121</br>
**注释**：

```xml
<summary>Gets the value at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 64-bit integers.</summary>
<param name="indices">A one-dimensional array of 64-bit integers that represent the indexes specifying the position of the <see cref="T:System.Array" /> element to get.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="indices" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
<returns>The value at the specified position in the multidimensional <see cref="T:System.Array" />.</returns>
```

**成员**：System.Array.SetValue(object, long)</br>
**签名**：_d845170315112950</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 64-bit integer.</summary>
<param name="value">The new value for the specified element.</param>
<param name="index">A 64-bit integer that represents the position of the <see cref="T:System.Array" /> element to set.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.SetValue(object, long, long)</br>
**签名**：_24864536d32c0b93</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
<param name="value">The new value for the specified element.</param>
<param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.SetValue(object, long, long, long)</br>
**签名**：_314db333058e554d</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
<param name="value">The new value for the specified element.</param>
<param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<param name="index3">A 64-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to set.</param>
<exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.SetValue(object, params long[])</br>
**签名**：_e3923681669a96b5</br>
**注释**：

```xml
<summary>Sets a value to the element at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 64-bit integers.</summary>
<param name="value">The new value for the specified element.</param>
<param name="indices">A one-dimensional array of 64-bit integers that represent the indexes specifying the position of the element to set.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="indices" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
<exception cref="T:System.InvalidCastException">  <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
```

**成员**：System.Array.GetLongLength(int)</br>
**签名**：_b529d6e54112cf3e</br>
**注释**：

```xml
<summary>Gets a 64-bit integer that represents the number of elements in the specified dimension of the <see cref="T:System.Array" />.</summary>
<param name="dimension">A zero-based dimension of the <see cref="T:System.Array" /> whose length needs to be determined.</param>
<exception cref="T:System.IndexOutOfRangeException">  <paramref name="dimension" /> is less than zero. -or- <paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
<returns>A 64-bit integer that represents the number of elements in the specified dimension.</returns>
```

**成员**：System.Array.SyncRoot.get</br>
**签名**：_5df324fc2064bf14</br>

**成员**：System.Array.IsReadOnly.get</br>
**签名**：_957efa892fba2b42</br>

**成员**：System.Array.IsFixedSize.get</br>
**签名**：_af3654cc2dd2fa42</br>

**成员**：System.Array.IsSynchronized.get</br>
**签名**：_818cd5ec440253da</br>

**成员**：System.Array.Clone()</br>
**签名**：_7b75e1326e081bb2</br>
**注释**：

```xml
<summary>Creates a shallow copy of the <see cref="T:System.Array" />.</summary>
<returns>A shallow copy of the <see cref="T:System.Array" />.</returns>
```

**成员**：static System.Array.BinarySearch(System.Array, object)</br>
**签名**：_0c9e99640a975a5b</br>
**注释**：

```xml
<summary>Searches an entire one-dimensional sorted array for a specific element, using the <see cref="T:System.IComparable" /> interface implemented by each element of the array and by the specified object.</summary>
<param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to search for.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch(System.Array, int, int, object)</br>
**签名**：_fa538add1f784012</br>
**注释**：

```xml
<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the <see cref="T:System.IComparable" /> interface implemented by each element of the array and by the specified value.</summary>
<param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="index">The starting index of the range to search.</param>
<param name="length">The length of the range to search.</param>
<param name="value">The object to search for.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or- <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch(System.Array, object, System.Collections.IComparer)</br>
**签名**：_c453dd981ecbb5c5</br>
**注释**：

```xml
<summary>Searches an entire one-dimensional sorted array for a value using the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
<param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to search for.</param>
<param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="comparer" /> is <see langword="null" />, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch(System.Array, int, int, object, System.Collections.IComparer)</br>
**签名**：_f1fb5c20cf9ffd4d</br>
**注释**：

```xml
<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
<param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="index">The starting index of the range to search.</param>
<param name="length">The length of the range to search.</param>
<param name="value">The object to search for.</param>
<param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or- <paramref name="comparer" /> is <see langword="null" />, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch<T>(T[], T)</br>
**签名**：_75258b66e0bba01a</br>
**注释**：

```xml
<summary>Searches an entire one-dimensional sorted array for a specific element, using the <see cref="T:System.IComparable`1" /> generic interface implemented by each element of the <see cref="T:System.Array" /> and by the specified object.</summary>
<param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">  <typeparamref name="T" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch<T>(T[], T, System.Collections.Generic.IComparer<T>)</br>
**签名**：_87f2af26c36fed01</br>
**注释**：

```xml
<summary>Searches an entire one-dimensional sorted array for a value using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
<param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to search for.</param>
<param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable`1" /> implementation of each element.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="comparer" /> is <see langword="null" />, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and <typeparamref name="T" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch<T>(T[], int, int, T)</br>
**签名**：_60003ac825620c60</br>
**注释**：

```xml
<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the <see cref="T:System.IComparable`1" /> generic interface implemented by each element of the <see cref="T:System.Array" /> and by the specified value.</summary>
<param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="index">The starting index of the range to search.</param>
<param name="length">The length of the range to search.</param>
<param name="value">The object to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or- <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <typeparamref name="T" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.BinarySearch<T>(T[], int, int, T, System.Collections.Generic.IComparer<T>)</br>
**签名**：_42b1da24db771714</br>
**注释**：

```xml
<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
<param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="index">The starting index of the range to search.</param>
<param name="length">The length of the range to search.</param>
<param name="value">The object to search for.</param>
<param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable`1" /> implementation of each element.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or- <paramref name="comparer" /> is <see langword="null" />, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and <typeparamref name="T" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
<returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found; otherwise, a negative number. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, the negative number returned is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than all elements in <paramref name="array" />, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted <paramref name="array" />, the return value can be incorrect and a negative number could be returned, even if <paramref name="value" /> is present in <paramref name="array" />.</returns>
```

**成员**：static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)</br>
**签名**：_a73f4ff0bddcc6f6</br>
**注释**：

```xml
<summary>Converts an array of one type to an array of another type.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to convert to a target type.</param>
<param name="converter">A <see cref="T:System.Converter`2" /> that converts each element from one type to another type.</param>
<typeparam name="TInput">The type of the elements of the source array.</typeparam>
<typeparam name="TOutput">The type of the elements of the target array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="converter" /> is <see langword="null" />.</exception>
<returns>An array of the target type containing the converted elements from the source array.</returns>
```

**成员**：System.Array.CopyTo(System.Array, int)</br>
**签名**：_559d75b1e44b3eb0</br>
**注释**：

```xml
<summary>Copies all the elements of the current one-dimensional array to the specified one-dimensional array starting at the specified destination array index. The index is specified as a 32-bit integer.</summary>
<param name="array">The one-dimensional array that is the destination of the elements copied from the current array.</param>
<param name="index">A 32-bit integer that represents the index in <paramref name="array" /> at which copying begins.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="array" /> is multidimensional. -or- The number of elements in the source array is greater than the available number of elements from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
<exception cref="T:System.ArrayTypeMismatchException">The type of the source <see cref="T:System.Array" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
<exception cref="T:System.RankException">The source array is multidimensional.</exception>
<exception cref="T:System.InvalidCastException">At least one element in the source <see cref="T:System.Array" /> cannot be cast to the type of destination <paramref name="array" />.</exception>
```

**成员**：System.Array.CopyTo(System.Array, long)</br>
**签名**：_02714528e8c676b0</br>
**注释**：

```xml
<summary>Copies all the elements of the current one-dimensional array to the specified one-dimensional array starting at the specified destination array index. The index is specified as a 64-bit integer.</summary>
<param name="array">The one-dimensional array that is the destination of the elements copied from the current array.</param>
<param name="index">A 64-bit integer that represents the index in <paramref name="array" /> at which copying begins.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="array" /> is multidimensional. -or- The number of elements in the source array is greater than the available number of elements from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
<exception cref="T:System.ArrayTypeMismatchException">The type of the source <see cref="T:System.Array" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
<exception cref="T:System.RankException">The source <see cref="T:System.Array" /> is multidimensional.</exception>
<exception cref="T:System.InvalidCastException">At least one element in the source <see cref="T:System.Array" /> cannot be cast to the type of destination <paramref name="array" />.</exception>
```

**成员**：static System.Array.Empty<T>()</br>
**签名**：_b36a1b49fd533b3e</br>
**注释**：

```xml
<summary>Returns an empty array.</summary>
<typeparam name="T">The type of the elements of the array.</typeparam>
<returns>An empty array.</returns>
```

**成员**：static System.Array.Exists<T>(T[], System.Predicate<T>)</br>
**签名**：_3795c9344e3fe39f</br>
**注释**：

```xml
<summary>Determines whether the specified array contains elements that match the conditions defined by the specified predicate.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the elements to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if <paramref name="array" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Array.Fill<T>(T[], T)</br>
**签名**：_65ab99eba8176bda</br>
**注释**：

```xml
<summary>Assigns the given <paramref name="value" /> of type <typeparamref name="T" /> to each element of the specified <paramref name="array" />.</summary>
<param name="array">The array to be filled.</param>
<param name="value">The value to assign to each array element.</param>
<typeparam name="T">The type of the elements in the array.</typeparam>
```

**成员**：static System.Array.Fill<T>(T[], T, int, int)</br>
**签名**：_8edf171ab37f3a05</br>
**注释**：

```xml
<summary>Assigns the given <paramref name="value" /> of type <typeparamref name="T" /> to the elements of the specified <paramref name="array" /> which are          within the range of <paramref name="startIndex" /> (inclusive) and the next <paramref name="count" /> number of indices.</summary>
<param name="array">The <see cref="T:System.Array" /> to be filled.</param>
<param name="value">The new value for the elements in the specified range.</param>
<param name="startIndex">A 32-bit integer that represents the index in the <see cref="T:System.Array" /> at which filling begins.</param>
<param name="count">The number of elements to copy.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
```

**成员**：static System.Array.Find<T>(T[], System.Predicate<T>)</br>
**签名**：_1dfc77048ccf0234</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based array to search.</param>
<param name="match">The predicate that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <typeparamref name="T" />.</returns>
```

**成员**：static System.Array.FindAll<T>(T[], System.Predicate<T>)</br>
**签名**：_b373eb093e6c7b63</br>
**注释**：

```xml
<summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the elements to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>An <see cref="T:System.Array" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Array" />.</returns>
```

**成员**：static System.Array.FindIndex<T>(T[], System.Predicate<T>)</br>
**签名**：_64f5a7fd5c436edb</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.FindIndex<T>(T[], int, System.Predicate<T>)</br>
**签名**：_42e008ba24b77e94</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="startIndex">The zero-based starting index of the search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.FindIndex<T>(T[], int, int, System.Predicate<T>)</br>
**签名**：_fdfc005bdc859fff</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="startIndex">The zero-based starting index of the search.</param>
<param name="count">The number of elements in the section to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />. -or- <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
<returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.FindLast<T>(T[], System.Predicate<T>)</br>
**签名**：_2786abe2cff245fa</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <typeparamref name="T" />.</returns>
```

**成员**：static System.Array.FindLastIndex<T>(T[], System.Predicate<T>)</br>
**签名**：_ea3118f38aa5f363</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.FindLastIndex<T>(T[], int, System.Predicate<T>)</br>
**签名**：_56359f972a00ab73</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="startIndex">The zero-based starting index of the backward search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.FindLastIndex<T>(T[], int, int, System.Predicate<T>)</br>
**签名**：_6b63489e941ef0f0</br>
**注释**：

```xml
<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="startIndex">The zero-based starting index of the backward search.</param>
<param name="count">The number of elements in the section to search.</param>
<param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />. -or- <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
<returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.ForEach<T>(T[], System.Action<T>)</br>
**签名**：_ad1c39ab55fe27b9</br>
**注释**：

```xml
<summary>Performs the specified action on each element of the specified array.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> on whose elements the action is to be performed.</param>
<param name="action">The <see cref="T:System.Action`1" /> to perform on each element of <paramref name="array" />.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="action" /> is <see langword="null" />.</exception>
```

**成员**：static System.Array.IndexOf(System.Array, object)</br>
**签名**：_cde8d7a78af8dc9a</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
<param name="array">The one-dimensional array to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<returns>The index of the first occurrence of <paramref name="value" /> in <paramref name="array" />, if found; otherwise, the lower bound of the array minus 1.</returns>
```

**成员**：static System.Array.IndexOf(System.Array, object, int)</br>
**签名**：_2151f4cd0a63b0a2</br>
**注释**：

```xml
<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
<param name="array">The one-dimensional array to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<returns>The index of the first occurrence of <paramref name="value" />, if it's found, within the range of elements in <paramref name="array" /> that extends from <paramref name="startIndex" /> to the last element; otherwise, the lower bound of the array minus 1.</returns>
```

**成员**：static System.Array.IndexOf(System.Array, object, int, int)</br>
**签名**：_c419efc216312a6a</br>
**注释**：

```xml
<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of ifs first occurrence. The range extends from a specified index for a specified number of elements.</summary>
<param name="array">The one-dimensional array to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
<param name="count">The number of elements to search.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />. -or- <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<returns>The index of the first occurrence of <paramref name="value" />, if it's found in the <paramref name="array" /> from index <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> - 1; otherwise, the lower bound of the array minus 1.</returns>
```

**成员**：static System.Array.IndexOf<T>(T[], T)</br>
**签名**：_34e8668cac3c06fa</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
<param name="array">The one-dimensional, zero-based array to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<returns>The zero-based index of the first occurrence of <paramref name="value" /> in the entire <paramref name="array" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.IndexOf<T>(T[], T, int)</br>
**签名**：_d7a4d17a98a17e7e</br>
**注释**：

```xml
<summary>Searches for the specified object in a range of elements of a one dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
<param name="array">The one-dimensional, zero-based array to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty array.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from <paramref name="startIndex" /> to the last element, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.IndexOf<T>(T[], T, int, int)</br>
**签名**：_e3d80b27a67e8a0d</br>
**注释**：

```xml
<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index for a specified number of elements.</summary>
<param name="array">The one-dimensional, zero-based array to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty array.</param>
<param name="count">The number of elements in the section to search.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />. -or- <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
<returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified in <paramref name="count" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.LastIndexOf(System.Array, object)</br>
**签名**：_85801a2dbc247f17</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of the last occurrence within the entire one-dimensional <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<returns>The index of the last occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, the lower bound of the array minus 1.</returns>
```

**成员**：static System.Array.LastIndexOf(System.Array, object, int)</br>
**签名**：_6b23455f7b2f95ff</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The starting index of the backward search.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<returns>The index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from the first element to <paramref name="startIndex" />, if found; otherwise, the lower bound of the array minus 1.</returns>
```

**成员**：static System.Array.LastIndexOf(System.Array, object, int, int)</br>
**签名**：_7f5af90fd2a084fe</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The starting index of the backward search.</param>
<param name="count">The number of elements in the section to search.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />. -or- <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<returns>The index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that contains the number of elements specified in <paramref name="count" /> and ends at <paramref name="startIndex" />, if found; otherwise, the lower bound of the array minus 1.</returns>
```

**成员**：static System.Array.LastIndexOf<T>(T[], T)</br>
**签名**：_198d0f4fcb1c0679</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<returns>The zero-based index of the last occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.LastIndexOf<T>(T[], T, int)</br>
**签名**：_5c2c6aa99d0e0549</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The zero-based starting index of the backward search.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
<returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from the first element to <paramref name="startIndex" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.LastIndexOf<T>(T[], T, int, int)</br>
**签名**：_b5bf131d8947c855</br>
**注释**：

```xml
<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
<param name="value">The object to locate in <paramref name="array" />.</param>
<param name="startIndex">The zero-based starting index of the backward search.</param>
<param name="count">The number of elements in the section to search.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />. -or- <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
<returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that contains the number of elements specified in <paramref name="count" /> and ends at <paramref name="startIndex" />, if found; otherwise, -1.</returns>
```

**成员**：static System.Array.Reverse(System.Array)</br>
**签名**：_c02ce18f02385f3d</br>
**注释**：

```xml
<summary>Reverses the sequence of the elements in the entire one-dimensional <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to reverse.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
```

**成员**：static System.Array.Reverse(System.Array, int, int)</br>
**签名**：_36c04f95b4ffdfd5</br>
**注释**：

```xml
<summary>Reverses the sequence of a subset of the elements in the one-dimensional <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to reverse.</param>
<param name="index">The starting index of the section to reverse.</param>
<param name="length">The number of elements in the section to reverse.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
```

**成员**：static System.Array.Reverse<T>(T[])</br>
**签名**：_e2b02681782c394b</br>
**注释**：

```xml
<summary>Reverses the sequence of the elements in the one-dimensional generic array.</summary>
<param name="array">The one-dimensional array of elements to reverse.</param>
<typeparam name="T">The type of the elements in <paramref name="array" />.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
```

**成员**：static System.Array.Reverse<T>(T[], int, int)</br>
**签名**：_5b0cbdf276c63339</br>
**注释**：

```xml
<summary>Reverses the sequence of a subset of the elements in the one-dimensional generic array.</summary>
<param name="array">The one-dimensional array of elements to reverse.</param>
<param name="index">The starting index of the section to reverse.</param>
<param name="length">The number of elements in the section to reverse.</param>
<typeparam name="T">The type of the elements in <paramref name="array" />.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
```

**成员**：static System.Array.Sort(System.Array)</br>
**签名**：_07ee8311aaf13b6b</br>
**注释**：

```xml
<summary>Sorts the elements in an entire one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort(System.Array, System.Array)</br>
**签名**：_4df21ca760120c59</br>
**注释**：

```xml
<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
<param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <see langword="null" /> to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional. -or- The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort(System.Array, int, int)</br>
**签名**：_4e10132b81a43421</br>
**注释**：

```xml
<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort(System.Array, System.Array, int, int)</br>
**签名**：_12789d2affa27035</br>
**注释**：

```xml
<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
<param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <see langword="null" /> to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional. -or- The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="keys" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or- <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <paramref name="items" /> is not <see langword="null" />, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort(System.Array, System.Collections.IComparer)</br>
**签名**：_093c373956602c04</br>
**注释**：

```xml
<summary>Sorts the elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
<param name="array">The one-dimensional array to sort.</param>
<param name="comparer">The implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
<exception cref="T:System.ArgumentException">The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
```

**成员**：static System.Array.Sort(System.Array, System.Array, System.Collections.IComparer)</br>
**签名**：_122404a1fc2867ba</br>
**注释**：

```xml
<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
<param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <see langword="null" /> to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
<param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional. -or- The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or- The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort(System.Array, int, int, System.Collections.IComparer)</br>
**签名**：_b2141b8c013bc1b0</br>
**注释**：

```xml
<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
<param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">  <paramref name="array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or- The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort(System.Array, System.Array, int, int, System.Collections.IComparer)</br>
**签名**：_a95c3f83e8cd4623</br>
**注释**：

```xml
<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
<param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <see langword="null" /> to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements. -or- <see langword="null" /> to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional. -or- The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="keys" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />. -or- <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or- <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <paramref name="items" /> is not <see langword="null" />, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />. -or- The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
```

**成员**：static System.Array.Sort<T>(T[])</br>
**签名**：_382add2bad872f67</br>
**注释**：

```xml
<summary>Sorts the elements in an entire <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<TKey, TValue>(TKey[], TValue[])</br>
**签名**：_1a3ebd994898c67c</br>
**注释**：

```xml
<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
<param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or <see langword="null" /> to sort only <paramref name="keys" />.</param>
<typeparam name="TKey">The type of the elements of the key array.</typeparam>
<typeparam name="TValue">The type of the elements of the items array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />. -or- <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<T>(T[], int, int)</br>
**签名**：_80e6f8922ae8703c</br>
**注释**：

```xml
<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int)</br>
**签名**：_9b803c8e781cf3c0</br>
**注释**：

```xml
<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
<param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or <see langword="null" /> to sort only <paramref name="keys" />.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<typeparam name="TKey">The type of the elements of the key array.</typeparam>
<typeparam name="TValue">The type of the elements of the items array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="keys" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />. -or- <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or- <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <paramref name="items" /> is not <see langword="null" />, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />.</exception>
<exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<T>(T[], System.Collections.Generic.IComparer<T>)</br>
**签名**：_92474aed4e4823f3</br>
**注释**：

```xml
<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
<param name="array">The one-dimensional, zero-base <see cref="T:System.Array" /> to sort.</param>
<param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or <see langword="null" /> to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
<exception cref="T:System.ArgumentException">The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
```

**成员**：static System.Array.Sort<TKey, TValue>(TKey[], TValue[], System.Collections.Generic.IComparer<TKey>)</br>
**签名**：_dfd5fefaaa03a228</br>
**注释**：

```xml
<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
<param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or <see langword="null" /> to sort only <paramref name="keys" />.</param>
<param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or <see langword="null" /> to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
<typeparam name="TKey">The type of the elements of the key array.</typeparam>
<typeparam name="TValue">The type of the elements of the items array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />. -or- <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or- The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<T>(T[], int, int, System.Collections.Generic.IComparer<T>)</br>
**签名**：_55dbc52295bd7984</br>
**注释**：

```xml
<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or <see langword="null" /> to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="array" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or- The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int, System.Collections.Generic.IComparer<TKey>)</br>
**签名**：_f3e7263659ac2e30</br>
**注释**：

```xml
<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
<param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort.</param>
<param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or <see langword="null" /> to sort only <paramref name="keys" />.</param>
<param name="index">The starting index of the range to sort.</param>
<param name="length">The number of elements in the range to sort.</param>
<param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or <see langword="null" /> to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
<typeparam name="TKey">The type of the elements of the key array.</typeparam>
<typeparam name="TValue">The type of the elements of the items array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="keys" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than the lower bound of <paramref name="keys" />. -or- <paramref name="length" /> is less than zero.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="items" /> is not <see langword="null" />, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />. -or- <paramref name="items" /> is not <see langword="null" />, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or- <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />. -or- <paramref name="items" /> is not <see langword="null" />, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />. -or- The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
```

**成员**：static System.Array.Sort<T>(T[], System.Comparison<T>)</br>
**签名**：_c8fcae59a3aca6f6</br>
**注释**：

```xml
<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort.</param>
<param name="comparison">The <see cref="T:System.Comparison`1" /> to use when comparing elements.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="comparison" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The implementation of <paramref name="comparison" /> caused an error during the sort. For example, <paramref name="comparison" /> might not return 0 when comparing an item with itself.</exception>
```

**成员**：static System.Array.TrueForAll<T>(T[], System.Predicate<T>)</br>
**签名**：_7deb21b3fbe579c9</br>
**注释**：

```xml
<summary>Determines whether every element in the array matches the conditions defined by the specified predicate.</summary>
<param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to check against the conditions.</param>
<param name="match">The predicate that defines the conditions to check against the elements.</param>
<typeparam name="T">The type of the elements of the array.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />. -or- <paramref name="match" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if every element in <paramref name="array" /> matches the conditions defined by the specified predicate; otherwise, <see langword="false" />. If there are no elements in the array, the return value is <see langword="true" />.</returns>
```

**成员**：static System.Array.MaxLength.get</br>
**签名**：_a7a42b1fbdbc7628</br>

**成员**：System.Array.GetEnumerator()</br>
**签名**：_1e9012cd200b3827</br>
**注释**：

```xml
<summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</summary>
<returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</returns>
```

