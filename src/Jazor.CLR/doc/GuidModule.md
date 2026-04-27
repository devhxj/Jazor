# GuidModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：static readonly System.Guid.Empty</br>
**签名**：_124e041f3a0a52ac</br>
**注释**：

```xml
<summary>A read-only instance of the <see cref="T:System.Guid" /> structure whose value is all zeros.</summary>
```

**成员**：System.Guid.Guid()</br>
**签名**：_7b413c883ba7c148</br>

**成员**：static System.Guid.AllBitsSet.get</br>
**签名**：_8c455e8bff588fd2</br>

**成员**：System.Guid.Guid(byte[])</br>
**签名**：_06c4a482c0dec5ad</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified array of bytes.</summary>
<param name="b">A 16-element byte array containing values with which to initialize the GUID.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="b" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="b" /> is not 16 bytes long.</exception>
```

**成员**：System.Guid.Guid(System.ReadOnlySpan<byte>)</br>
**签名**：_e06e1a852ba90710</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the value represented by the specified read-only span of bytes.</summary>
<param name="b">A read-only span containing the bytes representing the GUID. The span must be exactly 16 bytes long.</param>
<exception cref="T:System.ArgumentException">The span must be exactly 16 bytes long.</exception>
```

**成员**：System.Guid.Guid(System.ReadOnlySpan<byte>, bool)</br>
**签名**：_c622cc26a27027e6</br>
**注释**：

```xml
<param name="b" />
<param name="bigEndian" />
```

**成员**：System.Guid.Guid(uint, ushort, ushort, byte, byte, byte, byte, byte, byte, byte, byte)</br>
**签名**：_f7af165b32607d21</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified unsigned integers and bytes.</summary>
<param name="a">The first 4 bytes of the GUID.</param>
<param name="b">The next 2 bytes of the GUID.</param>
<param name="c">The next 2 bytes of the GUID.</param>
<param name="d">The next byte of the GUID.</param>
<param name="e">The next byte of the GUID.</param>
<param name="f">The next byte of the GUID.</param>
<param name="g">The next byte of the GUID.</param>
<param name="h">The next byte of the GUID.</param>
<param name="i">The next byte of the GUID.</param>
<param name="j">The next byte of the GUID.</param>
<param name="k">The next byte of the GUID.</param>
```

**成员**：System.Guid.Guid(int, short, short, byte[])</br>
**签名**：_1d7aed4040ea426e</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified integers and byte array.</summary>
<param name="a">The first 4 bytes of the GUID.</param>
<param name="b">The next 2 bytes of the GUID.</param>
<param name="c">The next 2 bytes of the GUID.</param>
<param name="d">The remaining 8 bytes of the GUID.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="d" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="d" /> is not 8 bytes long.</exception>
```

**成员**：System.Guid.Guid(int, short, short, byte, byte, byte, byte, byte, byte, byte, byte)</br>
**签名**：_601a415444f0a9cf</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the specified integers and bytes.</summary>
<param name="a">The first 4 bytes of the GUID.</param>
<param name="b">The next 2 bytes of the GUID.</param>
<param name="c">The next 2 bytes of the GUID.</param>
<param name="d">The next byte of the GUID.</param>
<param name="e">The next byte of the GUID.</param>
<param name="f">The next byte of the GUID.</param>
<param name="g">The next byte of the GUID.</param>
<param name="h">The next byte of the GUID.</param>
<param name="i">The next byte of the GUID.</param>
<param name="j">The next byte of the GUID.</param>
<param name="k">The next byte of the GUID.</param>
```

**成员**：System.Guid.Guid(string)</br>
**签名**：_df634cc7a499970c</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure by using the value represented by the specified string.</summary>
<param name="g">A string that contains a GUID in one of the following formats ("d" represents a hexadecimal digit whose case is ignored): 32 contiguous hexadecimal digits: dddddddddddddddddddddddddddddddd -or- Groups of 8, 4, 4, 4, and 12 hexadecimal digits with hyphens between the groups. The entire GUID can optionally be enclosed in matching braces or parentheses: dddddddd-dddd-dddd-dddd-dddddddddddd -or- {dddddddd-dddd-dddd-dddd-dddddddddddd} -or- (dddddddd-dddd-dddd-dddd-dddddddddddd) -or- Groups of 8, 4, and 4 hexadecimal digits, and a subset of eight groups of 2 hexadecimal digits, with each group prefixed by "0x" or "0X", and separated by commas. The entire GUID, as well as the subset, is enclosed in matching braces: {0xdddddddd, 0xdddd, 0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}} All braces, commas, and "0x" prefixes are required. All embedded spaces are ignored. All leading zeros in a group are ignored. The hexadecimal digits shown in a group are the maximum number of meaningful hexadecimal digits that can appear in that group. You can specify from 1 to the number of hexadecimal digits shown for a group. The specified digits are assumed to be the low-order digits of the group.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="g" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The format of <paramref name="g" /> is invalid.</exception>
<exception cref="T:System.OverflowException">The format of <paramref name="g" /> is invalid.</exception>
```

**成员**：System.Guid.Variant.get</br>
**签名**：_6012fbf29b35f86c</br>

**成员**：System.Guid.Version.get</br>
**签名**：_5fd0fdd3971f7fd9</br>

**成员**：static System.Guid.CreateVersion7()</br>
**签名**：_c3c0c5285a834b43</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.Guid" /> according to RFC 9562, following the Version 7 format.</summary>
<returns>A new <see cref="T:System.Guid" /> according to RFC 9562, following the Version 7 format.</returns>
```

**成员**：static System.Guid.CreateVersion7(System.DateTimeOffset)</br>
**签名**：_ef07efb99d5cd0bd</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.Guid" /> according to RFC 9562, following the Version 7 format.</summary>
<param name="timestamp">The date-time offset used to determine the Unix Epoch timestamp.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="timestamp" /> represents an offset prior to <see cref="F:System.DateTimeOffset.UnixEpoch" />.</exception>
<returns>A new <see cref="T:System.Guid" /> according to RFC 9562, following the Version 7 format.</returns>
```

**成员**：static System.Guid.Parse(string)</br>
**签名**：_8167ca2e2e1c1ea8</br>
**注释**：

```xml
<summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure.</summary>
<param name="input">The string to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is not in a recognized format.</exception>
<returns>A structure that contains the value that was parsed.</returns>
```

**成员**：static System.Guid.Parse(System.ReadOnlySpan<char>)</br>
**签名**：_3afccc60a10c1f49</br>
**注释**：

```xml
<summary>Converts a read-only character span that represents a GUID to the equivalent <see cref="T:System.Guid" /> structure.</summary>
<param name="input">A read-only span containing the bytes representing a GUID.</param>
<exception cref="T:System.FormatException">  <paramref name="input" /> is not in a recognized format.-or-After trimming, the length of the read-only character span is 0.</exception>
<returns>A structure that contains the value that was parsed.</returns>
```

**成员**：static System.Guid.Parse(System.ReadOnlySpan<byte>)</br>
**签名**：_be9899e771b9d9e4</br>

**成员**：static System.Guid.TryParse(string, out System.Guid)</br>
**签名**：_808065f1c1b0972d</br>
**注释**：

```xml
<summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure.</summary>
<param name="input">A string containing the GUID to convert.</param>
<param name="result">When this method returns, contains the parsed value. If the method returns <see langword="true" />, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns <see langword="false" />, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />.</param>
<returns>  <see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Guid.TryParse(System.ReadOnlySpan<char>, out System.Guid)</br>
**签名**：_935105ee1ee12d8f</br>
**注释**：

```xml
<summary>Converts the specified read-only span of characters containing the representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure.</summary>
<param name="input">A span containing the characters representing the GUID to convert.</param>
<param name="result">When this method returns, contains the parsed value. If the method returns <see langword="true" />, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns <see langword="false" />, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />.</param>
<returns>  <see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Guid.TryParse(System.ReadOnlySpan<byte>, out System.Guid)</br>
**签名**：_3d2c84584aa615b8</br>

**成员**：static System.Guid.ParseExact(string, string)</br>
**签名**：_99abba4254045f07</br>
**注释**：

```xml
<summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
<param name="input">The GUID to convert.</param>
<param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> or <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is not in the format specified by <paramref name="format" />.</exception>
<returns>A structure that contains the value that was parsed.</returns>
```

**成员**：static System.Guid.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)</br>
**签名**：_f3a5f0ba435c534c</br>
**注释**：

```xml
<summary>Converts the character span representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
<param name="input">A read-only span containing the characters representing the GUID to convert.</param>
<param name="format">A read-only span of characters representing one of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
<returns>A structure that contains the value that was parsed.</returns>
```

**成员**：static System.Guid.TryParseExact(string, string, out System.Guid)</br>
**签名**：_1a6c8f36c7ff3077</br>
**注释**：

```xml
<summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
<param name="input">The GUID to convert.</param>
<param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
<param name="result">When this method returns, contains the parsed value. If the method returns <see langword="true" />, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns <see langword="false" />, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />.</param>
<returns>  <see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Guid.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.Guid)</br>
**签名**：_9caff56d96a9ed90</br>
**注释**：

```xml
<summary>Converts span of characters representing the GUID to the equivalent <see cref="T:System.Guid" /> structure, provided that the string is in the specified format.</summary>
<param name="input">A read-only span containing the characters representing the GUID to convert.</param>
<param name="format">A read-only span containing a character representing one of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />: "N", "D", "B", "P", or "X".</param>
<param name="result">When this method returns, contains the parsed value. If the method returns <see langword="true" />, <paramref name="result" /> contains a valid <see cref="T:System.Guid" />. If the method returns <see langword="false" />, <paramref name="result" /> equals <see cref="F:System.Guid.Empty" />.</param>
<returns>  <see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Guid.ToByteArray()</br>
**签名**：_a7a0b7e0f08982ee</br>
**注释**：

```xml
<summary>Returns a 16-element byte array that contains the value of this instance.</summary>
<returns>A 16-element byte array.</returns>
```

**成员**：System.Guid.ToByteArray(bool)</br>
**签名**：_8f9adaf6ffd8f11c</br>
**注释**：

```xml
<param name="bigEndian" />
```

**成员**：System.Guid.TryWriteBytes(System.Span<byte>)</br>
**签名**：_1d11fe59d498e4db</br>
**注释**：

```xml
<summary>Tries to write the current GUID instance into a span of bytes.</summary>
<param name="destination">When this method returns, the GUID as a span of bytes.</param>
<returns>  <see langword="true" /> if the GUID is successfully written to the specified span; <see langword="false" /> otherwise.</returns>
```

**成员**：System.Guid.TryWriteBytes(System.Span<byte>, bool, out int)</br>
**签名**：_f4726c5c8c855276</br>
**注释**：

```xml
<param name="destination" />
<param name="bigEndian" />
<param name="bytesWritten" />
```

**成员**：override System.Guid.GetHashCode()</br>
**签名**：_c864de9987127be4</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>The hash code for this instance.</returns>
```

**成员**：override System.Guid.Equals(object)</br>
**签名**：_4497d32e44ae0b33</br>
**注释**：

```xml
<summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
<param name="o">The object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="o" /> is a <see cref="T:System.Guid" /> that has the same value as this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Guid.Equals(System.Guid)</br>
**签名**：_c3978b03448c489b</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Guid" /> object represent the same value.</summary>
<param name="g">An object to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="g" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Guid.CompareTo(object)</br>
**签名**：_547ae1162f700351</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
<param name="value">An object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Guid" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Description</description></listheader><item><term> A negative integer</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> A positive integer</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：System.Guid.CompareTo(System.Guid)</br>
**签名**：_ddfd78fc2ae1771c</br>
**注释**：

```xml
<summary>Compares this instance to a specified <see cref="T:System.Guid" /> object and returns an indication of their relative values.</summary>
<param name="value">An object to compare to this instance.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Description</description></listheader><item><term> A negative integer</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> A positive integer</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>
```

**成员**：static System.Guid.operator ==(System.Guid, System.Guid)</br>
**签名**：_2534893cec8470b4</br>
**注释**：

```xml
<summary>Indicates whether the values of two specified <see cref="T:System.Guid" /> objects are equal.</summary>
<param name="a">The first object to compare.</param>
<param name="b">The second object to compare.</param>
<returns>  <see langword="true" /> if <paramref name="a" /> and <paramref name="b" /> are equal; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Guid.operator !=(System.Guid, System.Guid)</br>
**签名**：_ba70ccfcbbb763f9</br>
**注释**：

```xml
<summary>Indicates whether the values of two specified <see cref="T:System.Guid" /> objects are not equal.</summary>
<param name="a">The first object to compare.</param>
<param name="b">The second object to compare.</param>
<returns>  <see langword="true" /> if <paramref name="a" /> and <paramref name="b" /> are not equal; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.Guid.ToString()</br>
**签名**：_5a00279d52dc6274</br>
**注释**：

```xml
<summary>Returns a string representation of the value of this instance in registry format.</summary>
<returns>The value of this <see cref="T:System.Guid" />, formatted by using the "D" format specifier as follows: <c>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</c> where the value of the GUID is represented as a series of lowercase hexadecimal digits in groups of 8, 4, 4, 4, and 12 digits and separated by hyphens. An example of a return value is "382c74c3-721d-4f34-80e5-57657b6cbc27". To convert the hexadecimal digits from a through f to uppercase, call the <see cref="M:System.String.ToUpper" /> method on the returned string.</returns>
```

**成员**：System.Guid.ToString(string)</br>
**签名**：_2bd09e77e1959a19</br>
**注释**：

```xml
<summary>Returns a string representation of the value of this <see cref="T:System.Guid" /> instance, according to the provided format specifier.</summary>
<param name="format">A single format specifier that indicates how to format the value of this <see cref="T:System.Guid" />. The <paramref name="format" /> parameter can be "N", "D", "B", "P", or "X". If <paramref name="format" /> is <see langword="null" /> or an empty string (""), "D" is used.</param>
<exception cref="T:System.FormatException">The value of <paramref name="format" /> is not <see langword="null" />, an empty string (""), "N", "D", "B", "P", or "X".</exception>
<returns>The value of this <see cref="T:System.Guid" />, represented as a series of lowercase hexadecimal digits in the specified format.</returns>
```

**成员**：System.Guid.ToString(string, System.IFormatProvider)</br>
**签名**：_b16ba45c60840732</br>
**注释**：

```xml
<summary>Returns a string representation of the value of this instance of the <see cref="T:System.Guid" /> class, according to the provided format specifier and culture-specific format information.</summary>
<param name="format">A single format specifier that indicates how to format the value of this <see cref="T:System.Guid" />. The <paramref name="format" /> parameter can be "N", "D", "B", "P", or "X". If <paramref name="format" /> is <see langword="null" /> or an empty string (""), "D" is used.</param>
<param name="provider">(Reserved) An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.FormatException">The value of <paramref name="format" /> is not <see langword="null" />, an empty string (""), "N", "D", "B", "P", or "X".</exception>
<returns>The value of this <see cref="T:System.Guid" />, represented as a series of lowercase hexadecimal digits in the specified format.</returns>
```

**成员**：System.Guid.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>)</br>
**签名**：_56643cfa4f869728</br>
**注释**：

```xml
<summary>Tries to format the current GUID instance into the provided character span.</summary>
<param name="destination">The span in which to write the GUID as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters written into the span.</param>
<param name="format">A read-only span containing the character representing one of the following specifiers that indicates the exact format to use when interpreting the current GUID instance: "N", "D", "B", "P", or "X".</param>
<returns>  <see langword="true" /> if the formatting operation was successful; <see langword="false" /> otherwise.</returns>
```

**成员**：System.Guid.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>)</br>
**签名**：_4194b846a877d0fd</br>
**注释**：

```xml
<param name="utf8Destination" />
<param name="bytesWritten" />
<param name="format" />
```

**成员**：static System.Guid.operator <(System.Guid, System.Guid)</br>
**签名**：_2f298fd69a10b710</br>
**注释**：

```xml
<summary>Compares two values to determine which is less.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Guid.operator <=(System.Guid, System.Guid)</br>
**签名**：_bdfd51d992261768</br>
**注释**：

```xml
<summary>Compares two values to determine which is less or equal.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Guid.operator >(System.Guid, System.Guid)</br>
**签名**：_acf241ebd5e1bb03</br>
**注释**：

```xml
<summary>Compares two values to determine which is greater.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Guid.operator >=(System.Guid, System.Guid)</br>
**签名**：_9a4b1333be942866</br>
**注释**：

```xml
<summary>Compares two values to determine which is greater or equal.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Guid.Parse(string, System.IFormatProvider)</br>
**签名**：_73103a7f2f74d23e</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.Guid.TryParse(string, System.IFormatProvider, out System.Guid)</br>
**签名**：_d5215a585ccd93b4</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Guid.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_92b1a5da5b078669</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.Guid.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Guid)</br>
**签名**：_4de537b5e40b8c32</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Guid.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_4815906615c5c38f</br>

**成员**：static System.Guid.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out System.Guid)</br>
**签名**：_29eacf257e0ac74e</br>

**成员**：static System.Guid.NewGuid()</br>
**签名**：_846c34b827153d3c</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Guid" /> structure.</summary>
<returns>A new GUID object.</returns>
```

