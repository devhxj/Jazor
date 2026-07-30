# Int128Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Int128.Int128()</br>
**签名**：_ed2ce49c470c9c69</br>

**成员**：System.Int128.Int128(ulong, ulong)</br>
**签名**：_bd38a63415786b75</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Int128" /> struct.</summary>
<param name="upper">The upper 64-bits of the 128-bit value.</param>
<param name="lower">The lower 64-bits of the 128-bit value.</param>
```

**成员**：System.Int128.CompareTo(object)</br>
**签名**：_b7fcdacf2f88dea3</br>
**注释**：

```xml
<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
<param name="value">An object to compare with this instance.</param>
<returns>  <p>A value that indicates the relative order of the objects being compared. The return value has these meanings:</p>  <table>    <thead>      <tr>        <th>Value</th>        <th>Meaning</th>      </tr>    </thead>    <tbody>      <tr>        <td>Less than zero</td>        <td>This instance precedes <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>      <tr>        <td>Zero</td>        <td>This instance occurs in the same position in the sort order as <code data-dev-comment-type="paramref">value</code>.</td>      </tr>      <tr>        <td>Greater than zero</td>        <td>This instance follows <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>    </tbody>  </table></returns>
```

**成员**：System.Int128.CompareTo(System.Int128)</br>
**签名**：_b5794ebe23a72285</br>
**注释**：

```xml
<summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
<param name="value">An object to compare with this instance.</param>
<returns>  <p>A value that indicates the relative order of the objects being compared. The return value has these meanings:</p>  <table>    <thead>      <tr>        <th>Value</th>        <th>Meaning</th>      </tr>    </thead>    <tbody>      <tr>        <td>Less than zero</td>        <td>This instance precedes <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>      <tr>        <td>Zero</td>        <td>This instance occurs in the same position in the sort order as <code data-dev-comment-type="paramref">value</code>.</td>      </tr>      <tr>        <td>Greater than zero</td>        <td>This instance follows <code data-dev-comment-type="paramref">value</code> in the sort order.</td>      </tr>    </tbody>  </table></returns>
```

**成员**：override System.Int128.Equals(object)</br>
**签名**：_3bfa5dfd4837a79e</br>
**注释**：

```xml
<summary>Determines whether the specified object is equal to the current object.</summary>
<param name="obj">The object to compare with the current object.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the specified object is equal to the current object; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：System.Int128.Equals(System.Int128)</br>
**签名**：_4031b3e3e167888e</br>
**注释**：

```xml
<summary>Indicates whether the current object is equal to another object of the same type.</summary>
<param name="other">An object to compare with this object.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the current object is equal to the <code data-dev-comment-type="paramref">other</code> parameter; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：override System.Int128.GetHashCode()</br>
**签名**：_2de13ea6377940aa</br>
**注释**：

```xml
<summary>Serves as the default hash function.</summary>
<returns>A hash code for the current object.</returns>
```

**成员**：override System.Int128.ToString()</br>
**签名**：_0cd70012444338f6</br>
**注释**：

```xml
<summary>Returns a string that represents the current object.</summary>
<returns>A string that represents the current object.</returns>
```

**成员**：System.Int128.ToString(System.IFormatProvider)</br>
**签名**：_5ea3d4988a658ce9</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：System.Int128.ToString(string)</br>
**签名**：_d1745b5899c82324</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">The format to use, or a <see langword="null" /> reference to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：System.Int128.ToString(string, System.IFormatProvider)</br>
**签名**：_97d31060bf8b1daf</br>
**注释**：

```xml
<summary>Formats the value of the current instance using the specified format.</summary>
<param name="format">The format to use, or a <see langword="null" /> reference to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The value of the current instance in the specified format.</returns>
```

**成员**：System.Int128.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_e8941fdbfbed9434</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Int128.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_e9b19593523937bf</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.Parse(string)</br>
**签名**：_e6ba6fd0fe70ed44</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">A string containing a number to parse.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.Int128.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_936bf5a339c27f5b</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">A string containing a number to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.Int128.Parse(string, System.IFormatProvider)</br>
**签名**：_1a9c00a8ce01999f</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.Int128.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_d4e73c2c718e1112</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.Int128.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_7af8b2902ab50959</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<returns>The result of parsing <paramref name="s" />.</returns>
```

**成员**：static System.Int128.TryParse(string, out System.Int128)</br>
**签名**：_14ac4f353ddae82c</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Int128.TryParse(System.ReadOnlySpan<char>, out System.Int128)</br>
**签名**：_b0e356aabfe72ec2</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Int128.TryParse(System.ReadOnlySpan<byte>, out System.Int128)</br>
**签名**：_b5211e33c4db2da9</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 128-bit signed integer equivalent.</summary>
<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 128-bit signed integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Int128.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out System.Int128)</br>
**签名**：_50e334c622e3b4c0</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Int128.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Int128)</br>
**签名**：_8dcf679cab70cfcc</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
<param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Int128.explicit operator byte(System.Int128)</br>
**签名**：_681cce7b9dc3e457</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Byte" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Byte" />.</returns>
```

**成员**：static System.Int128.explicit operator checked byte(System.Int128)</br>
**签名**：_75b77707d8797fe4</br>

**成员**：static System.Int128.explicit operator char(System.Int128)</br>
**签名**：_2fe34d368b81e0ae</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Char" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Char" />.</returns>
```

**成员**：static System.Int128.explicit operator checked char(System.Int128)</br>
**签名**：_f452363cdf448dd6</br>

**成员**：static System.Int128.explicit operator decimal(System.Int128)</br>
**签名**：_9e21259a765be818</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Decimal" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Decimal" />.</returns>
```

**成员**：static System.Int128.explicit operator double(System.Int128)</br>
**签名**：_05f30bc6677c8446</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Double" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Double" />.</returns>
```

**成员**：static System.Int128.explicit operator System.Half(System.Int128)</br>
**签名**：_53c418af5874ca57</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Half" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Half" />.</returns>
```

**成员**：static System.Int128.explicit operator short(System.Int128)</br>
**签名**：_f8ee91da89bfbc71</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Int16" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int16" />.</returns>
```

**成员**：static System.Int128.explicit operator checked short(System.Int128)</br>
**签名**：_2f789a7c53d14d8c</br>

**成员**：static System.Int128.explicit operator int(System.Int128)</br>
**签名**：_ce0386e19232c2f6</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Int32" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int32" />.</returns>
```

**成员**：static System.Int128.explicit operator checked int(System.Int128)</br>
**签名**：_93c11f1447efb175</br>

**成员**：static System.Int128.explicit operator long(System.Int128)</br>
**签名**：_25359af432a2c2e1</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Int64" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Int64" />.</returns>
```

**成员**：static System.Int128.explicit operator checked long(System.Int128)</br>
**签名**：_4d6353a3d3f19b88</br>

**成员**：static System.Int128.explicit operator nint(System.Int128)</br>
**签名**：_5c8c8b45c9b929e4</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.IntPtr" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.IntPtr" />.</returns>
```

**成员**：static System.Int128.explicit operator checked nint(System.Int128)</br>
**签名**：_1e364bd0c6e20318</br>

**成员**：static System.Int128.explicit operator sbyte(System.Int128)</br>
**签名**：_dd4a635494a253cd</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.SByte" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.SByte" />.</returns>
```

**成员**：static System.Int128.explicit operator checked sbyte(System.Int128)</br>
**签名**：_d08bfb41d3ab6ee2</br>

**成员**：static System.Int128.explicit operator float(System.Int128)</br>
**签名**：_68d0e51d50e84c44</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.Single" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.Single" />.</returns>
```

**成员**：static System.Int128.explicit operator ushort(System.Int128)</br>
**签名**：_ad0dd19a52ac3d36</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt16" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt16" />.</returns>
```

**成员**：static System.Int128.explicit operator checked ushort(System.Int128)</br>
**签名**：_304df15d6a44df74</br>

**成员**：static System.Int128.explicit operator uint(System.Int128)</br>
**签名**：_e51f817cdfd73059</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt32" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt32" />.</returns>
```

**成员**：static System.Int128.explicit operator checked uint(System.Int128)</br>
**签名**：_0ad5d1d4d4f5f677</br>

**成员**：static System.Int128.explicit operator ulong(System.Int128)</br>
**签名**：_4f4ad4e5fea9827f</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt64" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt64" />.</returns>
```

**成员**：static System.Int128.explicit operator checked ulong(System.Int128)</br>
**签名**：_0c7f2cd86870d034</br>

**成员**：static System.Int128.explicit operator System.UInt128(System.Int128)</br>
**签名**：_435090974b9cc147</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UInt128" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UInt128" />.</returns>
```

**成员**：static System.Int128.explicit operator checked System.UInt128(System.Int128)</br>
**签名**：_d9f967e451f57e1b</br>

**成员**：static System.Int128.explicit operator nuint(System.Int128)</br>
**签名**：_59cf51650b95aaab</br>
**注释**：

```xml
<summary>Explicitly converts a 128-bit signed integer to a <see cref="T:System.UIntPtr" /> value.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a <see cref="T:System.UIntPtr" />.</returns>
```

**成员**：static System.Int128.explicit operator checked nuint(System.Int128)</br>
**签名**：_72a141beb21e4813</br>

**成员**：static System.Int128.explicit operator System.Int128(decimal)</br>
**签名**：_ee13322cacfa030d</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Decimal" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.explicit operator System.Int128(double)</br>
**签名**：_fed29180182d65ba</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Double" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.explicit operator checked System.Int128(double)</br>
**签名**：_3d7c10f4becbee0b</br>

**成员**：static System.Int128.explicit operator System.Int128(float)</br>
**签名**：_f0c48afd1cde425d</br>
**注释**：

```xml
<summary>Explicitly converts a <see cref="T:System.Single" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.explicit operator checked System.Int128(float)</br>
**签名**：_1215d60b3aeb2477</br>

**成员**：static System.Int128.implicit operator System.Int128(byte)</br>
**签名**：_6c5b5cce56b6a31a</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Byte" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(char)</br>
**签名**：_84a75ee38ffb54f3</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Char" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(short)</br>
**签名**：_aa36c61698e86024</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Int16" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(int)</br>
**签名**：_2692bf3363e99c1b</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Int32" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(long)</br>
**签名**：_d0c6553702fcf78f</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.Int64" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(nint)</br>
**签名**：_3a03aa02661aebc0</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.IntPtr" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(sbyte)</br>
**签名**：_405d300a8a4894d7</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.SByte" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(ushort)</br>
**签名**：_992311e2df4638e5</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UInt16" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(uint)</br>
**签名**：_f6497b94c3678d10</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UInt32" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(ulong)</br>
**签名**：_fec01f2ce2f5e153</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UInt64" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.implicit operator System.Int128(nuint)</br>
**签名**：_3225d701adcc7f88</br>
**注释**：

```xml
<summary>Implicitly converts a <see cref="T:System.UIntPtr" /> value to a 128-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<returns>  <paramref name="value" /> converted to a 128-bit signed integer.</returns>
```

**成员**：static System.Int128.operator +(System.Int128, System.Int128)</br>
**签名**：_c67744f8c5d96c2b</br>
**注释**：

```xml
<summary>Adds two values together to compute their sum.</summary>
<param name="left">The value to which <code data-dev-comment-type="paramref">right</code> is added.</param>
<param name="right">The value which is added to <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The sum of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator checked +(System.Int128, System.Int128)</br>
**签名**：_5e6d45782cb5e4a5</br>
**注释**：

```xml
<summary>Adds two values together to compute their sum.</summary>
<param name="left">The value to which <code data-dev-comment-type="paramref">right</code> is added.</param>
<param name="right">The value which is added to <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The sum of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.DivRem(System.Int128, System.Int128)</br>
**签名**：_ca96ebfbc2a38481</br>
**注释**：

```xml
<summary>Computes the quotient and remainder of two values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.LeadingZeroCount(System.Int128)</br>
**签名**：_d295dfd29150ae75</br>
**注释**：

```xml
<summary>Computes the number of leading zeros in a value.</summary>
<param name="value">The value whose leading zeroes are to be counted.</param>
<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.Log10(System.Int128)</br>
**签名**：_f729da8a5282b658</br>

**成员**：static System.Int128.PopCount(System.Int128)</br>
**签名**：_9d72e9332fd24f23</br>
**注释**：

```xml
<summary>Computes the number of bits that are set in a value.</summary>
<param name="value">The value whose set bits are to be counted.</param>
<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.RotateLeft(System.Int128, int)</br>
**签名**：_d432cd8596dae24f</br>
**注释**：

```xml
<summary>Rotates a value left by a given amount.</summary>
<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static System.Int128.RotateRight(System.Int128, int)</br>
**签名**：_7adeb1315b95c346</br>
**注释**：

```xml
<summary>Rotates a value right by a given amount.</summary>
<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static System.Int128.TrailingZeroCount(System.Int128)</br>
**签名**：_7257dc92fb1e4c4c</br>
**注释**：

```xml
<summary>Computes the number of trailing zeros in a value.</summary>
<param name="value">The value whose trailing zeroes are to be counted.</param>
<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.IsPow2(System.Int128)</br>
**签名**：_d04628a14db21e34</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.Log2(System.Int128)</br>
**签名**：_f1a059f528650ba2</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator &(System.Int128, System.Int128)</br>
**签名**：_68ca38dcf867541d</br>
**注释**：

```xml
<summary>Computes the bitwise-and of two values.</summary>
<param name="left">The value to <code data-dev-comment-type="langword">and</code> with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to <code data-dev-comment-type="langword">and</code> with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The bitwise-and of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator |(System.Int128, System.Int128)</br>
**签名**：_a0d88d43c412365e</br>
**注释**：

```xml
<summary>Computes the bitwise-or of two values.</summary>
<param name="left">The value to <code data-dev-comment-type="langword">or</code> with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to <code data-dev-comment-type="langword">or</code> with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The bitwise-or of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator ^(System.Int128, System.Int128)</br>
**签名**：_46659df631c3627f</br>
**注释**：

```xml
<summary>Computes the exclusive-or of two values.</summary>
<param name="left">The value to xor with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to xor with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The exclusive-or of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator ~(System.Int128)</br>
**签名**：_406d8b09e6ec4129</br>
**注释**：

```xml
<summary>Computes the ones-complement representation of a given value.</summary>
<param name="value">The value for which to compute the ones-complement.</param>
<returns>The ones-complement of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator <(System.Int128, System.Int128)</br>
**签名**：_3631f568b169b219</br>
**注释**：

```xml
<summary>Compares two values to determine which is less.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator <=(System.Int128, System.Int128)</br>
**签名**：_7383f0483f670772</br>
**注释**：

```xml
<summary>Compares two values to determine which is less or equal.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator >(System.Int128, System.Int128)</br>
**签名**：_811c6d073ef6ca6e</br>
**注释**：

```xml
<summary>Compares two values to determine which is greater.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator >=(System.Int128, System.Int128)</br>
**签名**：_47979bbf00a44dc5</br>
**注释**：

```xml
<summary>Compares two values to determine which is greater or equal.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator --(System.Int128)</br>
**签名**：_76d6ddd943af6ff1</br>
**注释**：

```xml
<summary>Decrements a value.</summary>
<param name="value">The value to decrement.</param>
<returns>The result of decrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator checked --(System.Int128)</br>
**签名**：_1b31f1ebb654733d</br>
**注释**：

```xml
<summary>Decrements a value.</summary>
<param name="value">The value to decrement.</param>
<returns>The result of decrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator /(System.Int128, System.Int128)</br>
**签名**：_6357de67d5760485</br>
**注释**：

```xml
<summary>Divides two values together to compute their quotient.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator checked /(System.Int128, System.Int128)</br>
**签名**：_830753b6d4a84cc4</br>
**注释**：

```xml
<summary>Divides two values together to compute their quotient.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator ==(System.Int128, System.Int128)</br>
**签名**：_371d707661ecc52c</br>
**注释**：

```xml
<summary>Compares two values to determine equality.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator !=(System.Int128, System.Int128)</br>
**签名**：_299ca1abf18c4811</br>
**注释**：

```xml
<summary>Compares two values to determine inequality.</summary>
<param name="left">The value to compare with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to compare with <code data-dev-comment-type="paramref">left</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is not equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator ++(System.Int128)</br>
**签名**：_8dab4bca565b4529</br>
**注释**：

```xml
<summary>Increments a value.</summary>
<param name="value">The value to increment.</param>
<returns>The result of incrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator checked ++(System.Int128)</br>
**签名**：_6dacb4c587ca3df1</br>
**注释**：

```xml
<summary>Increments a value.</summary>
<param name="value">The value to increment.</param>
<returns>The result of incrementing <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.MinValue.get</br>
**签名**：_9bb56306acf5a086</br>

**成员**：static System.Int128.MaxValue.get</br>
**签名**：_0f41854e8fe45c4a</br>

**成员**：static System.Int128.operator %(System.Int128, System.Int128)</br>
**签名**：_6521eedba51d7990</br>
**注释**：

```xml
<summary>Divides two values together to compute their modulus or remainder.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The modulus or remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator *(System.Int128, System.Int128)</br>
**签名**：_7823e0b640baf5e3</br>
**注释**：

```xml
<summary>Multiplies two values together to compute their product.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The product of <code data-dev-comment-type="paramref">left</code> multiplied-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.operator checked *(System.Int128, System.Int128)</br>
**签名**：_056e8fba577b7eeb</br>
**注释**：

```xml
<summary>Multiplies two values together to compute their product.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The product of <code data-dev-comment-type="paramref">left</code> multiplied-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static System.Int128.BigMul(System.Int128, System.Int128, out System.Int128)</br>
**签名**：_d32138c04ddcda2e</br>

**成员**：static System.Int128.Clamp(System.Int128, System.Int128, System.Int128)</br>
**签名**：_587401c79d5e216e</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static System.Int128.CopySign(System.Int128, System.Int128)</br>
**签名**：_2f2f3fb10237971f</br>
**注释**：

```xml
<summary>Copies the sign of a value to the sign of another value.</summary>
<param name="value">The value whose magnitude is used in the result.</param>
<param name="sign">The value whose sign is used in the result.</param>
<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
```

**成员**：static System.Int128.Max(System.Int128, System.Int128)</br>
**签名**：_bbbede4a8d6a94d0</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static System.Int128.Min(System.Int128, System.Int128)</br>
**签名**：_b3776eca350d4ad5</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static System.Int128.Sign(System.Int128)</br>
**签名**：_635630e4489249c0</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static System.Int128.One.get</br>
**签名**：_c1bcc15342fa30d0</br>

**成员**：static System.Int128.Zero.get</br>
**签名**：_69aaad155ef75bb3</br>

**成员**：static System.Int128.Abs(System.Int128)</br>
**签名**：_bc93f10cc4270d3d</br>
**注释**：

```xml
<summary>Computes the absolute of a value.</summary>
<param name="value">The value for which to get its absolute.</param>
<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.CreateChecked<TOther>(TOther)</br>
**签名**：_44ad6bcbe8d6480c</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.CreateSaturating<TOther>(TOther)</br>
**签名**：_81379c94dbf23e09</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value which is used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static System.Int128.CreateTruncating<TOther>(TOther)</br>
**签名**：_2fbfa53df417f6f1</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value which is used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static System.Int128.IsEvenInteger(System.Int128)</br>
**签名**：_6b8a91b15afb966d</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.IsNegative(System.Int128)</br>
**签名**：_9027f9d901e94b3a</br>
**注释**：

```xml
<summary>Determines if a value is negative.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.IsOddInteger(System.Int128)</br>
**签名**：_265a23c7352a4445</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.IsPositive(System.Int128)</br>
**签名**：_ab537fdef4fbd602</br>
**注释**：

```xml
<summary>Determines if a value is positive.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.MaxMagnitude(System.Int128, System.Int128)</br>
**签名**：_829ea04f38a9820e</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static System.Int128.MinMagnitude(System.Int128, System.Int128)</br>
**签名**：_ef5bdd18c3a981cf</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static System.Int128.TryParse(string, System.IFormatProvider, out System.Int128)</br>
**签名**：_c829bcba6a9b9105</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator <<(System.Int128, int)</br>
**签名**：_df6cfd9e1caeef21</br>
**注释**：

```xml
<summary>Shifts a value left by a given amount.</summary>
<param name="value">The value that is shifted left by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted left.</param>
<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
```

**成员**：static System.Int128.operator >>(System.Int128, int)</br>
**签名**：_aa3dd6025b84b3af</br>
**注释**：

```xml
<summary>Shifts a value right by a given amount.</summary>
<param name="value">The value that is shifted right by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted right.</param>
<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
```

**成员**：static System.Int128.operator >>>(System.Int128, int)</br>
**签名**：_9759894c554ab989</br>
**注释**：

```xml
<summary>Shifts a value right by a given amount.</summary>
<param name="value">The value that is shifted right by <code data-dev-comment-type="paramref">shiftAmount</code>.</param>
<param name="shiftAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is shifted right.</param>
<returns>The result of shifting <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">shiftAmount</code>.</returns>
```

**成员**：static System.Int128.NegativeOne.get</br>
**签名**：_b43cb7b43fce0d14</br>

**成员**：static System.Int128.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_4d90655f04c3cb26</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.Int128.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.Int128)</br>
**签名**：_18dfb394fe14fa70</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.operator -(System.Int128, System.Int128)</br>
**签名**：_88fc4b8cb4eaa1bb</br>
**注释**：

```xml
<summary>Subtracts two values to compute their difference.</summary>
<param name="left">The value from which <code data-dev-comment-type="paramref">right</code> is subtracted.</param>
<param name="right">The value which is subtracted from <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The difference of <code data-dev-comment-type="paramref">right</code> subtracted from <code data-dev-comment-type="paramref">left</code>.</returns>
```

**成员**：static System.Int128.operator checked -(System.Int128, System.Int128)</br>
**签名**：_bce2a2f696e0d716</br>
**注释**：

```xml
<summary>Subtracts two values to compute their difference.</summary>
<param name="left">The value from which <code data-dev-comment-type="paramref">right</code> is subtracted.</param>
<param name="right">The value which is subtracted from <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The difference of <code data-dev-comment-type="paramref">right</code> subtracted from <code data-dev-comment-type="paramref">left</code>.</returns>
```

**成员**：static System.Int128.operator -(System.Int128)</br>
**签名**：_7287b47decce69d8</br>
**注释**：

```xml
<summary>Computes the unary negation of a value.</summary>
<param name="value">The value for which to compute the unary negation.</param>
<returns>The unary negation of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator checked -(System.Int128)</br>
**签名**：_9f88084238b2cecc</br>
**注释**：

```xml
<summary>Computes the unary negation of a value.</summary>
<param name="value">The value for which to compute the unary negation.</param>
<returns>The unary negation of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.operator +(System.Int128)</br>
**签名**：_03c5cd4887db7285</br>
**注释**：

```xml
<summary>Computes the unary plus of a value.</summary>
<param name="value">The value for which to compute the unary plus.</param>
<returns>The unary plus of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static System.Int128.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_42de94cc986e1c0b</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static System.Int128.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out System.Int128)</br>
**签名**：_345775a0bab572a9</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.Int128.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_a68f252adc28b1db</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static System.Int128.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out System.Int128)</br>
**签名**：_35d67a7f4feee9b2</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```
