# RangeModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Range.Range()</br>
**签名**：_d5659647559c2c27</br>

**成员**：System.Range.Start.get</br>
**签名**：_ff879b9ef9597efb</br>

**成员**：System.Range.End.get</br>
**签名**：_0be235222ad447c5</br>

**成员**：System.Range.Range(System.Index, System.Index)</br>
**签名**：_fc3dfc5dbaa397eb</br>
**注释**：

```xml
<summary>Instantiates a new <see cref="T:System.Range" /> instance with the specified starting and ending indexes.</summary>
<param name="start">The inclusive start index of the range.</param>
<param name="end">The exclusive end index of the range.</param>
```

**成员**：override System.Range.Equals(object)</br>
**签名**：_31b6c9a4877f04c4</br>
**注释**：

```xml
<summary>Returns a value that indicates whether the current instance is equal to a specified object.</summary>
<param name="value">An object to compare with this Range object.</param>
<returns>  <see langword="true" /> if <paramref name="value" /> is of type <see cref="T:System.Range" /> and is equal to the current instance; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Range.Equals(System.Range)</br>
**签名**：_f858c453f3829489</br>
**注释**：

```xml
<summary>Returns a value that indicates whether the current instance is equal to another <see cref="T:System.Range" /> object.</summary>
<param name="other">A Range object to compare with this Range object.</param>
<returns>  <see langword="true" /> if the current instance is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.Range.GetHashCode()</br>
**签名**：_7fc0f3cc7ec542d3</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>The hash code.</returns>
```

**成员**：override System.Range.ToString()</br>
**签名**：_1c286146a6526629</br>
**注释**：

```xml
<summary>Returns the string representation of the current <see cref="T:System.Range" /> object.</summary>
<returns>The string representation of the range.</returns>
```

**成员**：static System.Range.StartAt(System.Index)</br>
**签名**：_2cc8d1f98d9f4b16</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.Range" /> instance starting from a specified start index to the end of the collection.</summary>
<param name="start">The position of the first element from which the Range will be created.</param>
<returns>A range from <paramref name="start" /> to the end of the collection.</returns>
```

**成员**：static System.Range.EndAt(System.Index)</br>
**签名**：_1df4ded30f6797b5</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Range" /> object starting from the first element in the collection to a specified end index.</summary>
<param name="end">The position of the last element up to which the <see cref="T:System.Range" /> object will be created.</param>
<returns>A range that starts from the first element to <paramref name="end" />.</returns>
```

**成员**：static System.Range.All.get</br>
**签名**：_9fb8edf805e88967</br>

**成员**：System.Range.GetOffsetAndLength(int)</br>
**签名**：_1c7a1e658ed790ff</br>
**注释**：

```xml
<summary>Calculates the start offset and length of the range object using a collection length.</summary>
<param name="length">A positive integer that represents the length of the collection that the range will be used with.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="length" /> is outside the bounds of the current range.</exception>
<returns>The start offset and length of the range.</returns>
```
