# UInt128Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.UInt128.UInt128()</br>
**签名**：_8c61bda013f8b908</br>

**成员**：System.UInt128.UInt128(ulong, ulong)</br>
**签名**：_460dd8437a181f67</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.UInt128" /> struct.</summary>
<param name="upper">The upper 64-bits of the 128-bit value.</param>
<param name="lower">The lower 64-bits of the 128-bit value.</param>
```

**成员**：System.UInt128.CompareTo(object)</br>
**签名**：_c1dc559553950096</br>
**注释**：

```xml
<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
<param name="value">An object to compare with this instance.</param>
<returns>  <p>A value that indicates the relative order of the objects being compared. The return value has these meanings:</p>  <table>    <thead>      <tr>        <th>Value</th>        <th>Meaning</th>      </tr>    </thead>    <tbody>      <tr>        <td>Less than zero</td>        <td>This instance precedes <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>      <tr>        <td>Zero</td>        <td>This instance occurs in the same position in the sort order as <code data-dev-comment-type="paramref">value</code>.</td>      </tr>      <tr>        <td>Greater than zero</td>        <td>This instance follows <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>    </tbody>  </table></returns>
```

**成员**：System.UInt128.CompareTo(System.UInt128)</br>
**签名**：_91bc1016db0da25b</br>
**注释**：

```xml
<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
<param name="value">An object to compare with this instance.</param>
<returns>  <p>A value that indicates the relative order of the objects being compared. The return value has these meanings:</p>  <table>    <thead>      <tr>        <th>Value</th>        <th>Meaning</th>      </tr>    </thead>    <tbody>      <tr>        <td>Less than zero</td>        <td>This instance precedes <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>      <tr>        <td>Zero</td>        <td>This instance occurs in the same position in the sort order as <code data-dev-comment-type="paramref">value</code>.</td>      </tr>      <tr>        <td>Greater than zero</td>        <td>This instance follows <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>    </tbody>  </table></returns>
```

**成员**：override System.UInt128.Equals(object)</br>
**签名**：_0d272eef1d8d95cb</br>
**注释**：

```xml
<summary>Determines whether the specified object is equal to the current object.</summary>
<param name="obj">The object to compare with the current object.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the specified object is equal to the current object; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：System.UInt128.Equals(System.UInt128)</br>
**签名**：_599bc5ece092c79f</br>
**注释**：

```xml
<summary>Indicates whether the current object is equal to another object of the same type.</summary>
<param name="other">An object to compare with this object.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the current object is equal to the <code data-dev-comment-type="paramref">other</code> parameter; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：override System.UInt128.GetHashCode()</br>
**签名**：_bd5a3a9523f573e7</br>
**注释**：

```xml
<summary>Serves as the default hash function.</summary>
<returns>A hash code for the current object.</returns>
```

**成员**：override System.UInt128.ToString()</br>
**签名**：_2ea689aef6636a36</br>
**注释**：

```xml
<summary>Returns a string that represents the current object.</summary>
<returns>A string that represents the current object.</returns>
```

**成员**：System.UInt128.ToString(System.IFormatProvider)</br>
**签名**：_0c1a603ac1899034</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：System.UInt128.ToString(string)</br>
**签名**：_44e0941d5883f6c8</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">The format to use, or a <see langword="null" /> reference to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：System.UInt128.ToString(string, System.IFormatProvider)</br>
**签名**：_bae671fcc030f76a</br>
**注释**：

```xml
<summary>Formats the value of the current instance using the specified format.</summary>
<param name="format">The format to use, or a <see langword="null" /> reference to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The value of the current instance in the specified format.</returns>
```

**成员**：System.UInt128.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_029205a5f1310ecf</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.UInt128.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_03bb4d378248cadd</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.Parse(string)</br>
**签名**：_30fed79ec71cc7e4</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">A string containing a number to parse.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.UInt128.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_0f1308db09adb315</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">A string containing a number to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.UInt128.Parse(string, System.IFormatProvider)</br>
**签名**：_6d4342f227a4fbad</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.UInt128.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_a58539dfaa0aa547</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.UInt128.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_0080af67cc571b72</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.UInt128.TryParse(string, out System.UInt128)</br>
**签名**：_8845ce18c94ffbb4</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.UInt128.TryParse(System.ReadOnlySpan<char>, out System.UInt128)</br>
**签名**：_4d3bd14dc2810a3c</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.UInt128.TryParse(System.ReadOnlySpan<byte>, out System.UInt128)</br>
**签名**：_6b11c1fbc39c3749</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 128-bit unsigned integer equivalent.</summary>
<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 128-bit unsigned integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.UInt128.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.UInt128)</br>
**签名**：_48fc1f3242ea3e1e</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.UInt128.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.UInt128)</br>
**签名**：_07f5c4340bb74419</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.UInt128.explicit operator byte(System.UInt128)</br>
**签名**：_ec72a9ccd5bd9a8d</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Byte" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Byte" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked byte(System.UInt128)</br>
**签名**：_64e60de5b1e03760</br>

**成员**：static System.UInt128.explicit operator char(System.UInt128)</br>
**签名**：_e15ea70aeec221be</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Char" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Char" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked char(System.UInt128)</br>
**签名**：_b68867a4bbf792ed</br>

**成员**：static System.UInt128.explicit operator decimal(System.UInt128)</br>
**签名**：_cfc7a729e04a71ab</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Decimal" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Decimal" />.</returns>
```

**成员**：static System.UInt128.explicit operator double(System.UInt128)</br>
**签名**：_cd6d53ea42e52f42</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Double" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Double" />.</returns>
```

**成员**：static System.UInt128.explicit operator System.Half(System.UInt128)</br>
**签名**：_ebc69a5a022fe3e9</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Half" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Half" />.</returns>
```

**成员**：static System.UInt128.explicit operator short(System.UInt128)</br>
**签名**：_00a7733415bd9a50</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int16" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int16" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked short(System.UInt128)</br>
**签名**：_5efef087d1235b8b</br>

**成员**：static System.UInt128.explicit operator int(System.UInt128)</br>
**签名**：_0ab9aeb11107ae84</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int32" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int32" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked int(System.UInt128)</br>
**签名**：_ab4813fe5941ad49</br>

**成员**：static System.UInt128.explicit operator long(System.UInt128)</br>
**签名**：_b230f48381ed749f</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int64" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int64" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked long(System.UInt128)</br>
**签名**：_191ebf43930db2a5</br>

**成员**：static System.UInt128.explicit operator System.Int128(System.UInt128)</br>
**签名**：_a8ded488b275f658</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Int128" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int128" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.Int128(System.UInt128)</br>
**签名**：_c572f7b29eaf324c</br>

**成员**：static System.UInt128.explicit operator nint(System.UInt128)</br>
**签名**：_b74d6c6f2fe3373f</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.IntPtr" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.IntPtr" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked nint(System.UInt128)</br>
**签名**：_b810b3011b0b57b0</br>

**成员**：static System.UInt128.explicit operator sbyte(System.UInt128)</br>
**签名**：_a5c6bf0c046035c1</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.SByte" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.SByte" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked sbyte(System.UInt128)</br>
**签名**：_95c576d9e4841566</br>

**成员**：static System.UInt128.explicit operator float(System.UInt128)</br>
**签名**：_2d1b34588d4f3a11</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.Single" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Single" />.</returns>
```

**成员**：static System.UInt128.explicit operator ushort(System.UInt128)</br>
**签名**：_7cb9a373a2b731ae</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UInt16" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt16" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked ushort(System.UInt128)</br>
**签名**：_b68ba902309cfb9a</br>

**成员**：static System.UInt128.explicit operator uint(System.UInt128)</br>
**签名**：_6a569faa11d6516c</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UInt32" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt32" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked uint(System.UInt128)</br>
**签名**：_4b86a17a8f47b33f</br>

**成员**：static System.UInt128.explicit operator ulong(System.UInt128)</br>
**签名**：_f9acee955d63d389</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UInt64" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt64" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked ulong(System.UInt128)</br>
**签名**：_b7d11ef0703deabf</br>

**成员**：static System.UInt128.explicit operator nuint(System.UInt128)</br>
**签名**：_4ed9a24ef89a2ec1</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit unsigned integer to a <see cref="T:System.UIntPtr" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UIntPtr" />.</returns>
```

**成员**：static System.UInt128.explicit operator checked nuint(System.UInt128)</br>
**签名**：_4f5d29c8feefce8e</br>

**成员**：static System.UInt128.explicit operator System.UInt128(decimal)</br>
**签名**：_7a73b169cb4a8694</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Decimal" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator System.UInt128(double)</br>
**签名**：_8a2ad347ec233b35</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Double" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(double)</br>
**签名**：_5d464c2acf139edb</br>

**成员**：static System.UInt128.explicit operator System.UInt128(short)</br>
**签名**：_1260da042a15cd4d</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Int16" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(short)</br>
**签名**：_958e84ffc74ece86</br>

**成员**：static System.UInt128.explicit operator System.UInt128(int)</br>
**签名**：_3fc4a35a82073e71</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Int32" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(int)</br>
**签名**：_06d213d11ddf681c</br>

**成员**：static System.UInt128.explicit operator System.UInt128(long)</br>
**签名**：_326147fc1f07f877</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Int64" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(long)</br>
**签名**：_1ef649fc443738a2</br>

**成员**：static System.UInt128.explicit operator System.UInt128(nint)</br>
**签名**：_09f191a4670066de</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.IntPtr" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(nint)</br>
**签名**：_af6df204728f788a</br>

**成员**：static System.UInt128.explicit operator System.UInt128(sbyte)</br>
**签名**：_53303fb5506255e9</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.SByte" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(sbyte)</br>
**签名**：_8366585a071ba8b1</br>

**成员**：static System.UInt128.explicit operator System.UInt128(float)</br>
**签名**：_5ac67fecfe01fee0</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Single" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.explicit operator checked System.UInt128(float)</br>
**签名**：_dec2fe2225e51e70</br>

**成员**：static System.UInt128.implicit operator System.UInt128(byte)</br>
**签名**：_98daec1f69c50f9c</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Byte" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.implicit operator System.UInt128(char)</br>
**签名**：_5e848b2f01adace3</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Char" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.implicit operator System.UInt128(ushort)</br>
**签名**：_6fab8bffd4b7f89c</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UInt16" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.implicit operator System.UInt128(uint)</br>
**签名**：_fb1429c669cf366b</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UInt32" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.implicit operator System.UInt128(ulong)</br>
**签名**：_bff36faddf999794</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UInt64" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.implicit operator System.UInt128(nuint)</br>
**签名**：_c7000f2dfee0777c</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a 128-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
```

**成员**：static System.UInt128.operator +(System.UInt128, System.UInt128)</br>
**签名**：_fd527b44b0db5c70</br>
**注释**：

```xml
<summary>Adds two values together to compute their sum.</summary>
<param name="left">The value to which <code data-dev-comment-type="paramref">right</code> is added.</param>
<param name="right">The value which is added to <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The sum of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator checked +(System.UInt128, System.UInt128)</br>
**签名**：_c754a5da22221b5c</br>
**注释**：

```xml
<summary>Adds two values together to compute their sum.</summary>
<param name="left">The value to which <code data-dev-comment-type="paramref">right</code> is added.</param>
<param name="right">The value which is added to <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The sum of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.DivRem(System.UInt128, System.UInt128)</br>
**签名**：_8796a5402e48210c</br>
**注释**：

```xml
<summary>Computes the quotient and remainder of two values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.LeadingZeroCount(System.UInt128)</br>
**签名**：_76106db43126b9b5</br>
**注释**：

```xml
<summary>Computes the number of leading zeros in a value.</summary>
<param name="value">The value whose leading zeroes are to be counted.</param>
<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.Log10(System.UInt128)</br>
**签名**：_4ae42163ca5ab057</br>

**成员**：static System.UInt128.PopCount(System.UInt128)</br>
**签名**：_e60df5c8bf2adf5c</br>
**注释**：

```xml
<summary>Computes the number of bits that are set in a value.</summary>
<param name="value">The value whose set bits are to be counted.</param>
<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.RotateLeft(System.UInt128, int)</br>
**签名**：_d743d2ddded2abe5</br>
**注释**：

```xml
<summary>Rotates a value left by a given amount.</summary>
<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static System.UInt128.RotateRight(System.UInt128, int)</br>
**签名**：_a2bab5c9eaffb253</br>
**注释**：

```xml
<summary>Rotates a value right by a given amount.</summary>
<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static System.UInt128.TrailingZeroCount(System.UInt128)</br>
**签名**：_f5f31da639f5ea89</br>
**注释**：

```xml
<summary>Computes the number of trailing zeros in a value.</summary>
<param name="value">The value whose trailing zeroes are to be counted.</param>
<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.IsPow2(System.UInt128)</br>
**签名**：_841b21ea8d8d4958</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.Log2(System.UInt128)</br>
**签名**：_44031589e94ab825</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator &(System.UInt128, System.UInt128)</br>
**签名**：_96b8e5ae109a1ff0</br>
**注释**：

```xml
<summary>Computes the bitwise-and of two values.</summary>
<param name="left">The value to <code data-dev-comment-type="langword">and</code> with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to <code data-dev-comment-type="langword">and</code> with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The bitwise-and of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator |(System.UInt128, System.UInt128)</br>
**签名**：_d208584e5e031050</br>
**注释**：

```xml
<summary>Computes the bitwise-or of two values.</summary>
<param name="left">The value to <code data-dev-comment-type="langword">or</code> with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to <code data-dev-comment-type="langword">or</code> with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The bitwise-or of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator ^(System.UInt128, System.UInt128)</br>
**签名**：_c1355590879666a7</br>
**注释**：

```xml
<summary>Computes the exclusive-or of two values.</summary>
<param name="left">The value to xor with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to xor with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The exclusive-or of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator ~(System.UInt128)</br>
**签名**：_f4f575ec9a0a472a</br>
**注释**：

```xml
<summary>Computes the ones-complement representation of a given value.</summary>
<param name="value">The value for which to compute the ones-complement.</param>
<returns>The ones-complement of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator <(System.UInt128, System.UInt128)</br>
**签名**：_b39d9b2d9c7479e3</br>
**注释**：

```xml
<summary>Compares two values to determine which is less.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator <=(System.UInt128, System.UInt128)</br>
**签名**：_5976a0a34fbfe19a</br>
**注释**：

```xml
<summary>Compares two values to determine which is less or equal.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator >(System.UInt128, System.UInt128)</br>
**签名**：_a5d136c7ac6d9d21</br>
**注释**：

```xml
<summary>Compares two values to determine which is greater.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator >=(System.UInt128, System.UInt128)</br>
**签名**：_8ae7181f4f5684f5</br>
**注释**：

```xml
<summary>Compares two values to determine which is greater or equal.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator --(System.UInt128)</br>
**签名**：_9576b4fa37800283</br>
**注释**：

```xml
<summary>Decrements a value.</summary>
<param name="value">The value to decrement.</param>
<returns>The result of decrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator checked --(System.UInt128)</br>
**签名**：_2570268944e834ba</br>
**注释**：

```xml
<summary>Decrements a value.</summary>
<param name="value">The value to decrement.</param>
<returns>The result of decrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator /(System.UInt128, System.UInt128)</br>
**签名**：_30e28339559d8888</br>
**注释**：

```xml
<summary>Divides two values together to compute their quotient.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator checked /(System.UInt128, System.UInt128)</br>
**签名**：_b0d1618f64eba0cd</br>
**注释**：

```xml
<summary>Divides two values together to compute their quotient.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator ==(System.UInt128, System.UInt128)</br>
**签名**：_e3fe1ff91364288e</br>
**注释**：

```xml
<summary>Compares two values to determine equality.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator !=(System.UInt128, System.UInt128)</br>
**签名**：_38d10160fd6e7017</br>
**注释**：

```xml
<summary>Compares two values to determine inequality.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is not equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator ++(System.UInt128)</br>
**签名**：_0121bfc5e52ac327</br>
**注释**：

```xml
<summary>Increments a value.</summary>
<param name="value">The value to increment.</param>
<returns>The result of incrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator checked ++(System.UInt128)</br>
**签名**：_cf08bccf56129f82</br>
**注释**：

```xml
<summary>Increments a value.</summary>
<param name="value">The value to increment.</param>
<returns>The result of incrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.MinValue.get</br>
**签名**：_0b7d00260a524531</br>

**成员**：static System.UInt128.MaxValue.get</br>
**签名**：_f0d23ddd466a780b</br>

**成员**：static System.UInt128.operator %(System.UInt128, System.UInt128)</br>
**签名**：_4541585272909795</br>
**注释**：

```xml
<summary>Divides two values together to compute their modulus or remainder.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The modulus or remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator *(System.UInt128, System.UInt128)</br>
**签名**：_c1612a3b4558628b</br>
**注释**：

```xml
<summary>Multiplies two values together to compute their product.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The product of <code data-dev-comment-type="paramref">left</code> multiplied-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.operator checked *(System.UInt128, System.UInt128)</br>
**签名**：_7b7dc120501d3144</br>
**注释**：

```xml
<summary>Multiplies two values together to compute their product.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The product of <code data-dev-comment-type="paramref">left</code> multiplied-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.UInt128.BigMul(System.UInt128, System.UInt128, out System.UInt128)</br>
**签名**：_08f69578289009db</br>

**成员**：static System.UInt128.Clamp(System.UInt128, System.UInt128, System.UInt128)</br>
**签名**：_a545c5c1dd9b956a</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static System.UInt128.Max(System.UInt128, System.UInt128)</br>
**签名**：_fe718fcf9ea5e7c2</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static System.UInt128.Min(System.UInt128, System.UInt128)</br>
**签名**：_9b8aa52a420963fd</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static System.UInt128.Sign(System.UInt128)</br>
**签名**：_f9135bb711742dbc</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static System.UInt128.One.get</br>
**签名**：_8f31c1f8717c0095</br>

**成员**：static System.UInt128.Zero.get</br>
**签名**：_26fb05b39e23ffb6</br>

**成员**：static System.UInt128.CreateChecked<TOther>(TOther)</br>
**签名**：_6b99cde9ef76edf1</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.CreateSaturating<TOther>(TOther)</br>
**签名**：_bc9cc7899a1e35e1</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value which is used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static System.UInt128.CreateTruncating<TOther>(TOther)</br>
**签名**：_97c9e3166e089937</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value which is used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static System.UInt128.IsEvenInteger(System.UInt128)</br>
**签名**：_f413e72394669d0a</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.IsOddInteger(System.UInt128)</br>
**签名**：_db80c70118467db9</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.TryParse(string, System.IFormatProvider, out System.UInt128)</br>
**签名**：_201a443b1608c214</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator <<(System.UInt128, int)</br>
**签名**：_0f03623e7f627eca</br>
**注释**：

```xml
<summary>Shifts a value left by a given amount.</summary>
<param name="value">The value that is shifted left by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted left.</param>
<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
```

**成员**：static System.UInt128.operator >>(System.UInt128, int)</br>
**签名**：_85b70c1560acb52e</br>
**注释**：

```xml
<summary>Shifts a value right by a given amount.</summary>
<param name="value">The value that is shifted right by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted right.</param>
<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
```

**成员**：static System.UInt128.operator >>>(System.UInt128, int)</br>
**签名**：_e9352047e6007a39</br>
**注释**：

```xml
<summary>Shifts a value right by a given amount.</summary>
<param name="value">The value that is shifted right by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted right.</param>
<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
```

**成员**：static System.UInt128.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_c88639ae1d5401bd</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.UInt128.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.UInt128)</br>
**签名**：_76b9708fc50ff818</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.operator -(System.UInt128, System.UInt128)</br>
**签名**：_892ff8736bbd8e4e</br>
**注释**：

```xml
<summary>Subtracts two values to compute their difference.</summary>
<param name="left">The value from which <code data-dev-comment-type="paramref">right</code> is subtracted.</param>
<param name="right">The value which is subtracted from <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The difference of <code data-dev-comment-type="paramref">right</code> subtracted from <code data-dev-comment-type="paramref">left</code>.</returns>
```

**成员**：static System.UInt128.operator checked -(System.UInt128, System.UInt128)</br>
**签名**：_9b4d82822297f055</br>
**注释**：

```xml
<summary>Subtracts two values to compute their difference.</summary>
<param name="left">The value from which <code data-dev-comment-type="paramref">right</code> is subtracted.</param>
<param name="right">The value which is subtracted from <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The difference of <code data-dev-comment-type="paramref">right</code> subtracted from <code data-dev-comment-type="paramref">left</code>.</returns>
```

**成员**：static System.UInt128.operator -(System.UInt128)</br>
**签名**：_e29c8b28c70d54d4</br>
**注释**：

```xml
<summary>Computes the unary negation of a value.</summary>
<param name="value">The value for which to compute the unary negation.</param>
<returns>The unary negation of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator checked -(System.UInt128)</br>
**签名**：_86264fa0bd6d25be</br>
**注释**：

```xml
<summary>Computes the unary negation of a value.</summary>
<param name="value">The value for which to compute the unary negation.</param>
<returns>The unary negation of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.operator +(System.UInt128)</br>
**签名**：_01935e48d0078b16</br>
**注释**：

```xml
<summary>Computes the unary plus of a value.</summary>
<param name="value">The value for which to compute the unary plus.</param>
<returns>The unary plus of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.UInt128.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_3a273da9611bdfc5</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static System.UInt128.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out System.UInt128)</br>
**签名**：_40d6510086406c74</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.UInt128.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_8c6b3ee07c4c9ea5</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static System.UInt128.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out System.UInt128)</br>
**签名**：_4f6644b18a22d5e1</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```
