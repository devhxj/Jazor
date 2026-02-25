# StringBuilderModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Text.StringBuilder.StringBuilder()</br>
**签名**：_2154365d1f9a2abf</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class.</summary>
```

**成员**：System.Text.StringBuilder.StringBuilder(int)</br>
**签名**：_404c94878c905b27</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified capacity.</summary>
<param name="capacity">The suggested starting size of this instance.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.</exception>
```

**成员**：System.Text.StringBuilder.StringBuilder(string)</br>
**签名**：_c2c8c4778873ccdc</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string.</summary>
<param name="value">The string used to initialize the value of the instance. If <paramref name="value" /> is <see langword="null" />, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />).</param>
```

**成员**：System.Text.StringBuilder.StringBuilder(string, int)</br>
**签名**：_8ddc5378f62c27cc</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string and capacity.</summary>
<param name="value">The string used to initialize the value of the instance. If <paramref name="value" /> is <see langword="null" />, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />).</param>
<param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.</exception>
```

**成员**：System.Text.StringBuilder.StringBuilder(string, int, int, int)</br>
**签名**：_70c61ab8ef3313c3</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class from the specified substring and capacity.</summary>
<param name="value">The string that contains the substring used to initialize the value of this instance. If <paramref name="value" /> is <see langword="null" />, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />).</param>
<param name="startIndex">The position within <paramref name="value" /> where the substring begins.</param>
<param name="length">The number of characters in the substring.</param>
<param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero. -or- <paramref name="startIndex" /> plus <paramref name="length" /> is not a position within <paramref name="value" />.</exception>
```

**成员**：System.Text.StringBuilder.StringBuilder(int, int)</br>
**签名**：_f69cee28dea8bcdc</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class that starts with a specified capacity and can grow to a specified maximum.</summary>
<param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />.</param>
<param name="maxCapacity">The maximum number of characters the current string can contain.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="maxCapacity" /> is less than one, <paramref name="capacity" /> is less than zero, or <paramref name="capacity" /> is greater than <paramref name="maxCapacity" />.</exception>
```

**成员**：System.Text.StringBuilder.Capacity.get</br>
**签名**：_20274b0eadfc0539</br>

**成员**：System.Text.StringBuilder.Capacity.set</br>
**签名**：_d58ab6215b243f4f</br>

**成员**：System.Text.StringBuilder.MaxCapacity.get</br>
**签名**：_32a883f2233e3134</br>

**成员**：System.Text.StringBuilder.EnsureCapacity(int)</br>
**签名**：_e957bcfaa166161c</br>
**注释**：

```xml
<summary>Ensures that the capacity of this instance of <see cref="T:System.Text.StringBuilder" /> is at least the specified value.</summary>
<param name="capacity">The minimum capacity to ensure.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>The new capacity of this instance.</returns>
```

**成员**：override System.Text.StringBuilder.ToString()</br>
**签名**：_010347a06fe9584c</br>
**注释**：

```xml
<summary>Converts the value of this instance to a <see cref="T:System.String" />.</summary>
<returns>A string whose value is the same as this instance.</returns>
```

**成员**：System.Text.StringBuilder.ToString(int, int)</br>
**签名**：_4941946dde4f03f0</br>
**注释**：

```xml
<summary>Converts the value of a substring of this instance to a <see cref="T:System.String" />.</summary>
<param name="startIndex">The starting position of the substring in this instance.</param>
<param name="length">The length of the substring.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. -or- The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the length of the current instance.</exception>
<returns>A string whose value is the same as the specified substring of this instance.</returns>
```

**成员**：System.Text.StringBuilder.Clear()</br>
**签名**：_3b8e77fc2c4d5f63</br>
**注释**：

```xml
<summary>Removes all characters from the current <see cref="T:System.Text.StringBuilder" /> instance.</summary>
<returns>An object whose <see cref="P:System.Text.StringBuilder.Length" /> is 0 (zero).</returns>
```

**成员**：System.Text.StringBuilder.Length.get</br>
**签名**：_76a78d5aa26cb6e0</br>

**成员**：System.Text.StringBuilder.Length.set</br>
**签名**：_085925374c6d3abd</br>

**成员**：System.Text.StringBuilder.this[int].get</br>
**签名**：_c59f10eccb1d75d4</br>

**成员**：System.Text.StringBuilder.this[int].set</br>
**签名**：_a970d620cd814959</br>

**成员**：System.Text.StringBuilder.GetChunks()</br>
**签名**：_eb70112718b443d3</br>
**注释**：

```xml
<summary>Returns an object that can be used to iterate through the chunks of characters represented in a <see langword="ReadOnlyMemory&lt;Char&gt;" /> created from this <see cref="T:System.Text.StringBuilder" /> instance.</summary>
<returns>An enumerator for the chunks in the <see langword="ReadOnlyMemory&lt;Char&gt;" />.</returns>
```

**成员**：System.Text.StringBuilder.Append(char, int)</br>
**签名**：_77869f53e4b4cf63</br>
**注释**：

```xml
<summary>Appends a specified number of copies of the string representation of a Unicode character to this instance.</summary>
<param name="value">The character to append.</param>
<param name="repeatCount">The number of times to append <paramref name="value" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="repeatCount" /> is less than zero. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(char[], int, int)</br>
**签名**：_76a6be47564b1442</br>
**注释**：

```xml
<summary>Appends the string representation of a specified subarray of Unicode characters to this instance.</summary>
<param name="value">A character array.</param>
<param name="startIndex">The starting position in <paramref name="value" />.</param>
<param name="charCount">The number of characters to append.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />, and <paramref name="startIndex" /> and <paramref name="charCount" /> are not zero.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="charCount" /> is less than zero. -or- <paramref name="startIndex" /> is less than zero. -or- <paramref name="startIndex" /> + <paramref name="charCount" /> is greater than the length of <paramref name="value" />. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(string)</br>
**签名**：_2879b76db56f25fb</br>
**注释**：

```xml
<summary>Appends a copy of the specified string to this instance.</summary>
<param name="value">The string to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(string, int, int)</br>
**签名**：_643a38ba616afd42</br>
**注释**：

```xml
<summary>Appends a copy of a specified substring to this instance.</summary>
<param name="value">The string that contains the substring to append.</param>
<param name="startIndex">The starting position of the substring within <paramref name="value" />.</param>
<param name="count">The number of characters in <paramref name="value" /> to append.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />, and <paramref name="startIndex" /> and <paramref name="count" /> are not zero.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> less than zero. -or- <paramref name="startIndex" /> less than zero. -or- <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="value" />. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(System.Text.StringBuilder)</br>
**签名**：_390481e4ef6d1b43</br>
**注释**：

```xml
<summary>Appends the string representation of a specified string builder to this instance.</summary>
<param name="value">The string builder to append.</param>
<returns>A reference to this instance after the append operation is completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)</br>
**签名**：_2a75c7a6bec12592</br>
**注释**：

```xml
<summary>Appends a copy of a substring within a specified string builder to this instance.</summary>
<param name="value">The string builder that contains the substring to append.</param>
<param name="startIndex">The starting position of the substring within <paramref name="value" />.</param>
<param name="count">The number of characters in <paramref name="value" /> to append.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendLine()</br>
**签名**：_35fe8bcf463e879b</br>
**注释**：

```xml
<summary>Appends the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendLine(string)</br>
**签名**：_c06aaa44e213e405</br>
**注释**：

```xml
<summary>Appends a copy of the specified string followed by the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
<param name="value">The string to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.CopyTo(int, char[], int, int)</br>
**签名**：_e7c76d547b84e1dd</br>
**注释**：

```xml
<summary>Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="T:System.Char" /> array.</summary>
<param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
<param name="destination">The array where characters will be copied.</param>
<param name="destinationIndex">The starting position in <paramref name="destination" /> where characters will be copied. The index is zero-based.</param>
<param name="count">The number of characters to be copied.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="destination" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" />, is less than zero. -or- <paramref name="sourceIndex" /> is greater than the length of this instance.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="sourceIndex" /> + <paramref name="count" /> is greater than the length of this instance. -or- <paramref name="destinationIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="destination" />.</exception>
```

**成员**：System.Text.StringBuilder.CopyTo(int, System.Span<char>, int)</br>
**签名**：_54205e7ac737a01c</br>
**注释**：

```xml
<summary>Copies the characters from a specified segment of this instance to a destination <see cref="T:System.Char" /> span.</summary>
<param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
<param name="destination">The writable span where characters will be copied.</param>
<param name="count">The number of characters to be copied.</param>
```

**成员**：System.Text.StringBuilder.Insert(int, string, int)</br>
**签名**：_da897479d9bd6139</br>
**注释**：

```xml
<summary>Inserts one or more copies of a specified string into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The string to insert.</param>
<param name="count">The number of times to insert <paramref name="value" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the current length of this instance. -or- <paramref name="count" /> is less than zero.</exception>
<exception cref="T:System.OutOfMemoryException">The current length of this <see cref="T:System.Text.StringBuilder" /> object plus the length of <paramref name="value" /> times <paramref name="count" /> exceeds <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after insertion has completed.</returns>
```

**成员**：System.Text.StringBuilder.Remove(int, int)</br>
**签名**：_152bf60dc35a5bb6</br>
**注释**：

```xml
<summary>Removes the specified range of characters from this instance.</summary>
<param name="startIndex">The zero-based position in this instance where removal begins.</param>
<param name="length">The number of characters to remove.</param>
<exception cref="T:System.ArgumentOutOfRangeException">If <paramref name="startIndex" /> or <paramref name="length" /> is less than zero, or <paramref name="startIndex" /> + <paramref name="length" /> is greater than the length of this instance.</exception>
<returns>A reference to this instance after the excise operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(bool)</br>
**签名**：_dded353c61620d12</br>
**注释**：

```xml
<summary>Appends the string representation of a specified Boolean value to this instance.</summary>
<param name="value">The Boolean value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(char)</br>
**签名**：_a2ce7c5adfc1553c</br>
**注释**：

```xml
<summary>Appends the string representation of a specified <see cref="T:System.Char" /> object to this instance.</summary>
<param name="value">The UTF-16-encoded code unit to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(sbyte)</br>
**签名**：_3ce4c9341fd5777f</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 8-bit signed integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(byte)</br>
**签名**：_d530c416b64aac49</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 8-bit unsigned integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(short)</br>
**签名**：_ea789609ea3aeeb0</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 16-bit signed integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(int)</br>
**签名**：_212b9738d2ea3b2d</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 32-bit signed integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(long)</br>
**签名**：_a20035534ee530dd</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 64-bit signed integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(float)</br>
**签名**：_ec1b541b6a274b24</br>
**注释**：

```xml
<summary>Appends the string representation of a specified single-precision floating-point number to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(double)</br>
**签名**：_817e46ee3d60bf66</br>
**注释**：

```xml
<summary>Appends the string representation of a specified double-precision floating-point number to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(decimal)</br>
**签名**：_f07022820ca3881f</br>
**注释**：

```xml
<summary>Appends the string representation of a specified decimal number to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(ushort)</br>
**签名**：_37e94b64bce60492</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 16-bit unsigned integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(uint)</br>
**签名**：_423a4a09f9fa54c4</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 32-bit unsigned integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(ulong)</br>
**签名**：_f09314f07502e2a3</br>
**注释**：

```xml
<summary>Appends the string representation of a specified 64-bit unsigned integer to this instance.</summary>
<param name="value">The value to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(object)</br>
**签名**：_06379efa8addb10d</br>
**注释**：

```xml
<summary>Appends the string representation of a specified object to this instance.</summary>
<param name="value">The object to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(char[])</br>
**签名**：_4ec74831297581ec</br>
**注释**：

```xml
<summary>Appends the string representation of the Unicode characters in a specified array to this instance.</summary>
<param name="value">The array of characters to append.</param>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)</br>
**签名**：_8c68c811d3d42bcf</br>
**注释**：

```xml
<summary>Appends the string representation of a specified read-only character span to this instance.</summary>
<param name="value">The read-only character span to append.</param>
<returns>A reference to this instance after the append operation is completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(System.ReadOnlyMemory<char>)</br>
**签名**：_19e34431ab825546</br>
**注释**：

```xml
<summary>Appends the string representation of a specified read-only character memory region to this instance.</summary>
<param name="value">The read-only character memory region to append.</param>
<returns>A reference to this instance after the append operation is completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)</br>
**签名**：_b753ce137296837a</br>
**注释**：

```xml
<summary>Appends the specified interpolated string to this instance.</summary>
<param name="handler">The interpolated string to append.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Append(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)</br>
**签名**：_c38a3237ddfa0a19</br>
**注释**：

```xml
<summary>Appends the specified interpolated string to this instance using the specified format.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="handler">The interpolated string to append.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendLine(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)</br>
**签名**：_c52ed5039c53253f</br>
**注释**：

```xml
<summary>Appends the specified interpolated string followed by the default line terminator to the end of the current StringBuilder object.</summary>
<param name="handler">The interpolated string to append.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendLine(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)</br>
**签名**：_0192e43c680249a7</br>
**注释**：

```xml
<summary>Appends the specified interpolated string using the specified format, followed by the default line terminator, to the end of the current StringBuilder object.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="handler">The interpolated string to append.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(string, params object[])</br>
**签名**：_8bc8cc43c6d93195</br>
**注释**：

```xml
<summary>Concatenates the string representations of the elements in the provided array of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<object>)</br>
**签名**：_f4377679fddd51ad</br>
**注释**：

```xml
<summary>Concatenates the string representations of the elements in the provided span of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin<T>(string, System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_8d04089684a00c7b</br>
**注释**：

```xml
<summary>Concatenates and appends the members of a collection, using the specified separator between each member.</summary>
<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the concatenated and appended strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(string, params string[])</br>
**签名**：_6ceea7a4bfd233b6</br>
**注释**：

```xml
<summary>Concatenates the strings of the provided array, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)</br>
**签名**：_035c615b56218700</br>
**注释**：

```xml
<summary>Concatenates the strings of the provided span, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(char, params object[])</br>
**签名**：_a5aab658026ac255</br>
**注释**：

```xml
<summary>Concatenates the string representations of the elements in the provided array of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<object>)</br>
**签名**：_f9ca702aaa0e6322</br>
**注释**：

```xml
<summary>Concatenates the string representations of the elements in the provided span of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin<T>(char, System.Collections.Generic.IEnumerable<T>)</br>
**签名**：_3510fcab582042e0</br>
**注释**：

```xml
<summary>Concatenates and appends the members of a collection, using the specified char separator between each member.</summary>
<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the concatenated and appended strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(char, params string[])</br>
**签名**：_02a3ec9f0e91877f</br>
**注释**：

```xml
<summary>Concatenates the strings of the provided array, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)</br>
**签名**：_08c4f86d45c8b851</br>
**注释**：

```xml
<summary>Concatenates the strings of the provided span, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, string)</br>
**签名**：_40a305d0112c40d9</br>
**注释**：

```xml
<summary>Inserts a string into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The string to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the current length of this instance. -or- The current length of this <see cref="T:System.Text.StringBuilder" /> object plus the length of <paramref name="value" /> exceeds <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, bool)</br>
**签名**：_2e7808d3cd4780e8</br>
**注释**：

```xml
<summary>Inserts the string representation of a Boolean value into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, sbyte)</br>
**签名**：_5d866e86d8040d7d</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, byte)</br>
**签名**：_a90cbae6c991fb88</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, short)</br>
**签名**：_bf04d5cd34dd9bba</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, char)</br>
**签名**：_d09b2a26b288fbd7</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified Unicode character into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, char[])</br>
**签名**：_a4c62411da366ab0</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The character array to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, char[], int, int)</br>
**签名**：_f5ea58b7b0201715</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified subarray of Unicode characters into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">A character array.</param>
<param name="startIndex">The starting index within <paramref name="value" />.</param>
<param name="charCount">The number of characters to insert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />, and <paramref name="startIndex" /> and <paramref name="charCount" /> are not zero.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" />, <paramref name="startIndex" />, or <paramref name="charCount" /> is less than zero. -or- <paramref name="index" /> is greater than the length of this instance. -or- <paramref name="startIndex" /> plus <paramref name="charCount" /> is not a position within <paramref name="value" />. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, int)</br>
**签名**：_762de3335798fa24</br>
**注释**：

```xml
<summary>Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, long)</br>
**签名**：_057e461451fbc2f6</br>
**注释**：

```xml
<summary>Inserts the string representation of a 64-bit signed integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, float)</br>
**签名**：_5fa422ae348735cc</br>
**注释**：

```xml
<summary>Inserts the string representation of a single-precision floating point number into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, double)</br>
**签名**：_7e09aba586586854</br>
**注释**：

```xml
<summary>Inserts the string representation of a double-precision floating-point number into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, decimal)</br>
**签名**：_7244d40cd7bdaa7a</br>
**注释**：

```xml
<summary>Inserts the string representation of a decimal number into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, ushort)</br>
**签名**：_62b03548ac3a7f3c</br>
**注释**：

```xml
<summary>Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, uint)</br>
**签名**：_865132ea357402b6</br>
**注释**：

```xml
<summary>Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, ulong)</br>
**签名**：_e98da0d88b51734a</br>
**注释**：

```xml
<summary>Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The value to insert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, object)</br>
**签名**：_463fe06f693b73f1</br>
**注释**：

```xml
<summary>Inserts the string representation of an object into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The object to insert, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)</br>
**签名**：_ed1b69fd4bc25279</br>
**注释**：

```xml
<summary>Inserts the sequence of characters into this instance at the specified character position.</summary>
<param name="index">The position in this instance where insertion begins.</param>
<param name="value">The character span to insert.</param>
<returns>A reference to this instance after the insert operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(string, object)</br>
**签名**：_77a7606b3d9eca3e</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument.</summary>
<param name="format">A composite format string.</param>
<param name="arg0">An object to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 1.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of <paramref name="arg0" />.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(string, object, object)</br>
**签名**：_e3954878ec607794</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments.</summary>
<param name="format">A composite format string.</param>
<param name="arg0">The first object to format.</param>
<param name="arg1">The second object to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 2.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(string, object, object, object)</br>
**签名**：_5ba4a5dce6c59d24</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments.</summary>
<param name="format">A composite format string.</param>
<param name="arg0">The first object to format.</param>
<param name="arg1">The second object to format.</param>
<param name="arg2">The third object to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 3.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(string, params object[])</br>
**签名**：_6fc54e5431a32faa</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.</summary>
<param name="format">A composite format string.</param>
<param name="args">An array of objects to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(string, params System.ReadOnlySpan<object>)</br>
**签名**：_79714193eef28be4</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter span.</summary>
<param name="format">A composite format string.</param>
<param name="args">A span of objects to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> span.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object)</br>
**签名**：_d2a6136c3496706f</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A composite format string.</param>
<param name="arg0">The object to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to one (1).</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> in which any format specification is replaced by the string representation of <paramref name="arg0" />.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object, object)</br>
**签名**：_46fad2ab5d282d81</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A composite format string.</param>
<param name="arg0">The first object to format.</param>
<param name="arg1">The second object to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 2 (two).</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object, object, object)</br>
**签名**：_1b411bcc9ec45bf7</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A composite format string.</param>
<param name="arg0">The first object to format.</param>
<param name="arg1">The second object to format.</param>
<param name="arg2">The third object to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 3 (three).</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params object[])</br>
**签名**：_7b93ea5668c90df3</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A composite format string.</param>
<param name="args">An array of objects to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params System.ReadOnlySpan<object>)</br>
**签名**：_99e92b2a2bb0066c</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter span using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A composite format string.</param>
<param name="args">A span of objects to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> span.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)</br>
**签名**：_c50a53c322d59bfc</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
<param name="arg0">The first object to format.</param>
<typeparam name="TArg0">The type of the first object to format.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)</br>
**签名**：_529a8de0ce89f30f</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
<param name="arg0">The first object to format.</param>
<param name="arg1">The second object to format.</param>
<typeparam name="TArg0">The type of the first object to format.</typeparam>
<typeparam name="TArg1">The type of the second object to format.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)</br>
**签名**：_e637f9f49752d183</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
<param name="arg0">The first object to format.</param>
<param name="arg1">The second object to format.</param>
<param name="arg2">The third object to format.</param>
<typeparam name="TArg0">The type of the first object to format.</typeparam>
<typeparam name="TArg1">The type of the second object to format.</typeparam>
<typeparam name="TArg2">The type of the third object to format.</typeparam>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, System.Text.CompositeFormat, params object[])</br>
**签名**：_353eb0f30e59595f</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
<param name="args">An array of objects to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or  <paramref name="args" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.AppendFormat(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)</br>
**签名**：_c17e25151f610256</br>
**注释**：

```xml
<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
<param name="args">A span of objects to format.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
<returns>A reference to this instance after the append operation has completed.</returns>
```

**成员**：System.Text.StringBuilder.Replace(string, string)</br>
**签名**：_e11a2e954631c69a</br>
**注释**：

```xml
<summary>Replaces all occurrences of a specified string in this instance with another specified string.</summary>
<param name="oldValue">The string to replace.</param>
<param name="newValue">The string that replaces <paramref name="oldValue" />, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The length of <paramref name="oldValue" /> is zero.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance with all instances of <paramref name="oldValue" /> replaced by <paramref name="newValue" />.</returns>
```

**成员**：System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)</br>
**签名**：_c7be232bff90ab62</br>
**注释**：

```xml
<summary>Replaces all instances of one read-only character span with another in this builder.</summary>
<param name="oldValue">The read-only character span to replace.</param>
<param name="newValue">The read-only character span to replace <paramref name="oldValue" /> with.</param>
<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>
```

**成员**：System.Text.StringBuilder.Equals(System.Text.StringBuilder)</br>
**签名**：_843038bb92e97c63</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="sb">An object to compare with this instance, or <see langword="null" />.</param>
<returns>  <see langword="true" /> if this instance and <paramref name="sb" /> have equal string, <see cref="P:System.Text.StringBuilder.Capacity" />, and <see cref="P:System.Text.StringBuilder.MaxCapacity" /> values; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)</br>
**签名**：_251b340a59afa04d</br>
**注释**：

```xml
<summary>Returns a value indicating whether the characters in this instance are equal to the characters in a specified read-only character span.</summary>
<param name="span">The character span to compare with the current instance.</param>
<returns>  <see langword="true" /> if the characters in this instance and <paramref name="span" /> are the same; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Text.StringBuilder.Replace(string, string, int, int)</br>
**签名**：_34859fdec187084f</br>
**注释**：

```xml
<summary>Replaces, within a substring of this instance, all occurrences of a specified string with another specified string.</summary>
<param name="oldValue">The string to replace.</param>
<param name="newValue">The string that replaces <paramref name="oldValue" />, or <see langword="null" />.</param>
<param name="startIndex">The position in this instance where the substring begins.</param>
<param name="count">The length of the substring.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The length of <paramref name="oldValue" /> is zero.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> plus <paramref name="count" /> indicates a character position not within this instance. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
<returns>A reference to this instance with all instances of <paramref name="oldValue" /> replaced by <paramref name="newValue" /> in the range from <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> - 1.</returns>
```

**成员**：System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)</br>
**签名**：_5681048ad18a4b3f</br>
**注释**：

```xml
<summary>Replaces all instances of one read-only character span with another in part of this builder.</summary>
<param name="oldValue">The read-only character span to replace.</param>
<param name="newValue">The read-only character span to replace <paramref name="oldValue" /> with.</param>
<param name="startIndex">The index to start in this builder.</param>
<param name="count">The number of characters to read in this builder.</param>
<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>
```

**成员**：System.Text.StringBuilder.Replace(char, char)</br>
**签名**：_618d386adc69ad32</br>
**注释**：

```xml
<summary>Replaces all occurrences of a specified character in this instance with another specified character.</summary>
<param name="oldChar">The character to replace.</param>
<param name="newChar">The character that replaces <paramref name="oldChar" />.</param>
<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>
```

**成员**：System.Text.StringBuilder.Replace(char, char, int, int)</br>
**签名**：_b1fd321da487f718</br>
**注释**：

```xml
<summary>Replaces, within a substring of this instance, all occurrences of a specified character with another specified character.</summary>
<param name="oldChar">The character to replace.</param>
<param name="newChar">The character that replaces <paramref name="oldChar" />.</param>
<param name="startIndex">The position in this instance where the substring begins.</param>
<param name="count">The length of the substring.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of the value of this instance. -or- <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.</exception>
<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" /> in the range from <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> -1.</returns>
```

