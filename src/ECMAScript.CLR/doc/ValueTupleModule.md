# ValueTupleModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.ValueTuple.ValueTuple()</br>
**签名**：_afe5e7b03405c9fc</br>

**成员**：override System.ValueTuple.Equals(object)</br>
**签名**：_f405bb1d41845d0a</br>
**注释**：

```xml
<summary>Returns a value that indicates whether the current <see cref="T:System.ValueTuple" /> instance is equal to a specified object.</summary>
<param name="obj">The object to compare to the current instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is a <see cref="T:System.ValueTuple" /> instance; otherwise, <see langword="false" />.</returns>
```

**成员**：System.ValueTuple.Equals(System.ValueTuple)</br>
**签名**：_075aabd97b9153e6</br>
**注释**：

```xml
<summary>Determines whether two <see cref="T:System.ValueTuple" /> instances are equal. This method always returns <see langword="true" />.</summary>
<param name="other">The value tuple to compare with the current instance.</param>
<returns>This method always returns <see langword="true" />.</returns>
```

**成员**：System.ValueTuple.CompareTo(System.ValueTuple)</br>
**签名**：_f92b072b1ea77fb3</br>
**注释**：

```xml
<summary>Compares the current <see cref="T:System.ValueTuple" /> instance to a specified <see cref="T:System.ValueTuple" /> instance.</summary>
<param name="other">The object to compare with the current instance.</param>
<exception cref="T:System.ArgumentException">  <paramref name="other" /> is not a <see cref="T:System.ValueTuple" /> instance.</exception>
<returns>This method always returns 0.</returns>
```

**成员**：override System.ValueTuple.GetHashCode()</br>
**签名**：_79b4fb9a3ea0524a</br>
**注释**：

```xml
<summary>Returns the hash code for the current <see cref="T:System.ValueTuple" /> instance.</summary>
<returns>This method always return 0.</returns>
```

**成员**：override System.ValueTuple.ToString()</br>
**签名**：_93b143a10f6cb207</br>
**注释**：

```xml
<summary>Returns the string representation of this <see cref="T:System.ValueTuple" /> instance.</summary>
<returns>This method always returns "()".</returns>
```

**成员**：static System.ValueTuple.Create()</br>
**签名**：_b2020d347b181140</br>
**注释**：

```xml
<summary>Creates a new value tuple with zero components.</summary>
<returns>A new value tuple with no components.</returns>
```

**成员**：static System.ValueTuple.Create<T1>(T1)</br>
**签名**：_c01432b1ceab8949</br>
**注释**：

```xml
<summary>Creates a new value tuple with 1 component (a singleton).</summary>
<param name="item1">The value of the value tuple's only component.</param>
<typeparam name="T1">The type of the value tuple's only component.</typeparam>
<returns>A value tuple with 1 component.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2>(T1, T2)</br>
**签名**：_3c42e78c6d0ddf68</br>
**注释**：

```xml
<summary>Creates a new value tuple with 2 components (a pair).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<returns>A value tuple with 2 components.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2, T3>(T1, T2, T3)</br>
**签名**：_6462161c42aa6ac1</br>
**注释**：

```xml
<summary>Creates a new value tuple with 3 components (a triple).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<param name="item3">The value of the value tuple's third component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<typeparam name="T3">The type of the value tuple's third component.</typeparam>
<returns>A value tuple with 3 components.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2, T3, T4>(T1, T2, T3, T4)</br>
**签名**：_7d9afb217b6c02e6</br>
**注释**：

```xml
<summary>Creates a new value tuple with 4 components (a quadruple).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<param name="item3">The value of the value tuple's third component.</param>
<param name="item4">The value of the value tuple's fourth component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<typeparam name="T3">The type of the value tuple's third component.</typeparam>
<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
<returns>A value tuple with 4 components.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2, T3, T4, T5>(T1, T2, T3, T4, T5)</br>
**签名**：_4c097ae606bc8905</br>
**注释**：

```xml
<summary>Creates a new value tuple with 5 components (a quintuple).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<param name="item3">The value of the value tuple's third component.</param>
<param name="item4">The value of the value tuple's fourth component.</param>
<param name="item5">The value of the value tuple's fifth component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<typeparam name="T3">The type of the value tuple's third component.</typeparam>
<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
<returns>A value tuple with 5 components.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6>(T1, T2, T3, T4, T5, T6)</br>
**签名**：_afec461eabd4d8e5</br>
**注释**：

```xml
<summary>Creates a new value tuple with 6 components (a sexuple).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<param name="item3">The value of the value tuple's third component.</param>
<param name="item4">The value of the value tuple's fourth component.</param>
<param name="item5">The value of the value tuple's fifth component.</param>
<param name="item6">The value of the value tuple's sixth component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<typeparam name="T3">The type of the value tuple's third component.</typeparam>
<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
<typeparam name="T6">The type of the value tuple's sixth component.</typeparam>
<returns>A value tuple with 6 components.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7>(T1, T2, T3, T4, T5, T6, T7)</br>
**签名**：_68093829d7705581</br>
**注释**：

```xml
<summary>Creates a new value tuple with 7 components (a septuple).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<param name="item3">The value of the value tuple's third component.</param>
<param name="item4">The value of the value tuple's fourth component.</param>
<param name="item5">The value of the value tuple's fifth component.</param>
<param name="item6">The value of the value tuple's sixth component.</param>
<param name="item7">The value of the value tuple's seventh component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<typeparam name="T3">The type of the value tuple's third component.</typeparam>
<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
<typeparam name="T6">The type of the value tuple's sixth component.</typeparam>
<typeparam name="T7">The type of the value tuple's seventh component.</typeparam>
<returns>A value tuple with 7 components.</returns>
```

**成员**：static System.ValueTuple.Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1, T2, T3, T4, T5, T6, T7, T8)</br>
**签名**：_8bc5fa3a3cbbcbc7</br>
**注释**：

```xml
<summary>Creates a new value tuple with 8 components (an octuple).</summary>
<param name="item1">The value of the value tuple's first component.</param>
<param name="item2">The value of the value tuple's second component.</param>
<param name="item3">The value of the value tuple's third component.</param>
<param name="item4">The value of the value tuple's fourth component.</param>
<param name="item5">The value of the value tuple's fifth component.</param>
<param name="item6">The value of the value tuple's sixth component.</param>
<param name="item7">The value of the value tuple's seventh component.</param>
<param name="item8">The value of the value tuple's eighth component.</param>
<typeparam name="T1">The type of the value tuple's first component.</typeparam>
<typeparam name="T2">The type of the value tuple's second component.</typeparam>
<typeparam name="T3">The type of the value tuple's third component.</typeparam>
<typeparam name="T4">The type of the value tuple's fourth component.</typeparam>
<typeparam name="T5">The type of the value tuple's fifth component.</typeparam>
<typeparam name="T6">The type of the value tuple's sixth component.</typeparam>
<typeparam name="T7">The type of the value tuple's seventh component.</typeparam>
<typeparam name="T8">The type of the value tuple's eighth component.</typeparam>
<returns>A value tuple with 8 components.</returns>
```

