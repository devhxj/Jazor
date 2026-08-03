# IndexModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Index.Index()</br>
**签名**：_94a150c0b38bdd79</br>

**成员**：System.Index.Index(int, bool)</br>
**签名**：_f406c4c734b11d38</br>
**注释**：

```xml
<summary>Initializes a new <see cref="T:System.Index" /> with a specified index position and a value that indicates if the index is from the beginning or the end of a collection.</summary>
<param name="value">The index value. It has to be greater then or equal to zero.</param>
<param name="fromEnd">  <see langword="true" /> to index from the end of the collection, or <see langword="false" /> to index from the beginning of the collection.</param>
```

**成员**：static System.Index.Start.get</br>
**签名**：_c6ec2b575aff2e24</br>

**成员**：static System.Index.End.get</br>
**签名**：_0ba7c760bb17a58f</br>

**成员**：static System.Index.FromStart(int)</br>
**签名**：_1b0e1c2ab6c4cd39</br>
**注释**：

```xml
<summary>Creates an <see cref="T:System.Index" /> from the specified index at the start of a collection.</summary>
<param name="value">The index position from the start of a collection.</param>
<returns>The index value.</returns>
```

**成员**：static System.Index.FromEnd(int)</br>
**签名**：_ce8b9229a41c8545</br>
**注释**：

```xml
<summary>Creates an <see cref="T:System.Index" /> from the end of a collection at a specified index position.</summary>
<param name="value">The index value from the end of a collection.</param>
<returns>The index value.</returns>
```

**成员**：System.Index.Value.get</br>
**签名**：_71953783d6b61ae1</br>

**成员**：System.Index.IsFromEnd.get</br>
**签名**：_b141712b3756cf57</br>

**成员**：System.Index.GetOffset(int)</br>
**签名**：_9b817e75f3f8f58f</br>
**注释**：

```xml
<summary>Calculates the offset from the start of the collection using the specified collection length.</summary>
<param name="length">The length of the collection that the Index will be used with. Must be a positive value.</param>
<returns>The offset.</returns>
```

**成员**：override System.Index.Equals(object)</br>
**签名**：_2910b3afb47ad8b1</br>
**注释**：

```xml
<summary>Indicates whether the current Index object is equal to a specified object.</summary>
<param name="value">An object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="value" /> is of type <see cref="T:System.Index" /> and is equal to the current instance; <see langword="false" /> otherwise.</returns>
```

**成员**：System.Index.Equals(System.Index)</br>
**签名**：_83db7aa629254762</br>
**注释**：

```xml
<summary>Returns a value that indicates whether the current object is equal to another <see cref="T:System.Index" /> object.</summary>
<param name="other">The object to compare with this instance.</param>
<returns>  <see langword="true" /> if the current Index object is equal to <paramref name="other" />; <see langword="false" /> otherwise.</returns>
```

**成员**：override System.Index.GetHashCode()</br>
**签名**：_1c7f7405a620c971</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>The hash code.</returns>
```

**成员**：static System.Index.implicit operator System.Index(int)</br>
**签名**：_1e1b56e4e760a5d5</br>
**注释**：

```xml
<summary>Converts an integer number to an <see cref="T:System.Index" />.</summary>
<param name="value">The integer to convert.</param>
<returns>An index representing the integer.</returns>
```

**成员**：override System.Index.ToString()</br>
**签名**：_0fb768c390456f95</br>
**注释**：

```xml
<summary>Returns the string representation of the current <see cref="T:System.Index" /> instance.</summary>
<returns>The string representation of the <see cref="T:System.Index" />.</returns>
```
