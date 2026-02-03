# UInt16Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：ushort.UInt16()</br>
**签名**：_2b4f1af6b7fc0173</br>

**成员**：ushort.CompareTo(object)</br>
**签名**：_d8d8b9cba9bd3347</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
<param name="value">An object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.UInt16" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：ushort.CompareTo(ushort)</br>
**签名**：_2ca53dc375a8ff3d</br>
**注释**：

```xml
<summary>Compares this instance to a specified 16-bit unsigned integer and returns an indication of their relative values.</summary>
<param name="value">An unsigned integer to compare.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>
```

**成员**：override ushort.Equals(object)</br>
**签名**：_c13e06040702dab1</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="obj">An object to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.UInt16" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：ushort.Equals(ushort)</br>
**签名**：_0faff9447540bf0f</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.UInt16" /> value.</summary>
<param name="obj">A 16-bit unsigned integer to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> has the same value as this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override ushort.GetHashCode()</br>
**签名**：_1289c3b26567b431</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：override ushort.ToString()</br>
**签名**：_97b1f766a137a176</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
<returns>The string representation of the value of this instance, which consists of a sequence of digits ranging from 0 to 9, without a sign or leading zeros.</returns>
```

**成员**：ushort.ToString(System.IFormatProvider)</br>
**签名**：_54f6d55d2ab58603</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance, which consists of a sequence of digits ranging from 0 to 9, without a sign or leading zeros.</returns>
```

**成员**：ushort.ToString(string)</br>
**签名**：_6f22376b1343fe81</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format.</summary>
<param name="format">A numeric format string.</param>
<exception cref="T:System.FormatException">The <paramref name="format" /> parameter is invalid.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：ushort.ToString(string, System.IFormatProvider)</br>
**签名**：_a995cb7019a823da</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A numeric format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
<returns>The string representation of the value of this instance, as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：ushort.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_72607726c0ca8cb0</br>
**注释**：

```xml
<summary>Tries to format the value of the current unsigned short number instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：ushort.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_c8d9586ea188f250</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.Parse(string)</br>
**签名**：_bfae72f49db4f3c9</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its 16-bit unsigned integer equivalent.</summary>
<param name="s">A string that represents the number to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>.</exception>
<returns>A 16-bit unsigned integer equivalent to the number contained in <paramref name="s" />.</returns>
```

**成员**：static ushort.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_fa01aff4be2733da</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style to its 16-bit unsigned integer equivalent. This method is not CLS-compliant. The CLS-compliant alternative is <see cref="M:System.Int32.Parse(System.String,System.Globalization.NumberStyles)" />.</summary>
<param name="s">A string that represents the number to convert. The string is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
<param name="style">A bitwise combination of the enumeration values that specify the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
<returns>A 16-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static ushort.Parse(string, System.IFormatProvider)</br>
**签名**：_c90f18e22ef793ae</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified culture-specific format to its 16-bit unsigned integer equivalent.</summary>
<param name="s">A string that represents the number to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>.</exception>
<returns>A 16-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static ushort.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_2d47dd2f7572ac82</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent.</summary>
<param name="s">A string that represents the number to convert. The string is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number that is less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
<returns>A 16-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static ushort.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_e0537feda3434747</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent.</summary>
<param name="s">A span containing the characters that represent the number to convert. The span is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<returns>A 16-bit unsigned integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static ushort.TryParse(string, out ushort)</br>
**签名**：_2efd27d401f7def7</br>
**注释**：

```xml
<summary>Tries to convert the string representation of a number to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string that represents the number to convert.</param>
<param name="result">When this method returns, contains the 16-bit unsigned integer value that is equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in the correct format, or represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static ushort.TryParse(System.ReadOnlySpan<char>, out ushort)</br>
**签名**：_0103a8bec9e9dfd7</br>
**注释**：

```xml
<summary>Tries to convert the span representation of a number to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A span containing the characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 16-bit unsigned integer value that is equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in the correct format. , or represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static ushort.TryParse(System.ReadOnlySpan<byte>, out ushort)</br>
**签名**：_f90ee83a31a4d447</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 16-bit unsigned integer equivalent.</summary>
<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 16-bit unsigned integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static ushort.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)</br>
**签名**：_0427e1fa823cd14c</br>
**注释**：

```xml
<summary>Tries to convert the string representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string that represents the number to convert. The string is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the 16-bit unsigned integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static ushort.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)</br>
**签名**：_e1ac1ed9e4df0694</br>
**注释**：

```xml
<summary>Tries to convert the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A span containing the characters that represent the number to convert. The span is interpreted by using the style specified by the <paramref name="style" /> parameter.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the 16-bit unsigned integer value equivalent to the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：ushort.GetTypeCode()</br>
**签名**：_496bf7ba2bb081f6</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.UInt16" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.UInt16" />.</returns>
```

**成员**：static ushort.DivRem(ushort, ushort)</br>
**签名**：_80e78c0aa0b98fef</br>
**注释**：

```xml
<summary>Computes the quotient and remainder of two values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static ushort.LeadingZeroCount(ushort)</br>
**签名**：_680a923d09b804b9</br>
**注释**：

```xml
<summary>Computes the number of leading zeros in a value.</summary>
<param name="value">The value whose leading zeroes are to be counted.</param>
<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static ushort.PopCount(ushort)</br>
**签名**：_2ea0cab4f3f489d9</br>
**注释**：

```xml
<summary>Computes the number of bits that are set in a value.</summary>
<param name="value">The value whose set bits are to be counted.</param>
<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static ushort.RotateLeft(ushort, int)</br>
**签名**：_81462814a6e17f8a</br>
**注释**：

```xml
<summary>Rotates a value left by a given amount.</summary>
<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static ushort.RotateRight(ushort, int)</br>
**签名**：_68cb080f188abe14</br>
**注释**：

```xml
<summary>Rotates a value right by a given amount.</summary>
<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static ushort.TrailingZeroCount(ushort)</br>
**签名**：_08ec622fc4aabafb</br>
**注释**：

```xml
<summary>Computes the number of trailing zeros in a value.</summary>
<param name="value">The value whose trailing zeroes are to be counted.</param>
<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static ushort.IsPow2(ushort)</br>
**签名**：_5e7a013434210fd3</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.Log2(ushort)</br>
**签名**：_3e54056b3d1e32ad</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static ushort.Clamp(ushort, ushort, ushort)</br>
**签名**：_cfa99d1fe078f42e</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static ushort.Max(ushort, ushort)</br>
**签名**：_baf95be10fbe1b99</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static ushort.Min(ushort, ushort)</br>
**签名**：_5bde9c15f7f8b2f9</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static ushort.Sign(ushort)</br>
**签名**：_40243528ed598d7c</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static ushort.CreateChecked<TOther>(TOther)</br>
**签名**：_5f125252b32ddf67</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static ushort.CreateSaturating<TOther>(TOther)</br>
**签名**：_d885c6bcbc91e10a</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static ushort.CreateTruncating<TOther>(TOther)</br>
**签名**：_e7b18638be92c02a</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static ushort.IsEvenInteger(ushort)</br>
**签名**：_9efbbf8cbd046a16</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.IsOddInteger(ushort)</br>
**签名**：_fc6357bc14bbd89b</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.TryParse(string, System.IFormatProvider, out ushort)</br>
**签名**：_815a123a217a57dc</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_37538c358921bcf3</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static ushort.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out ushort)</br>
**签名**：_57f6f9049f0201c4</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_e04a106a21529984</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static ushort.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out ushort)</br>
**签名**：_8b4f59ba7c1bec8d</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static ushort.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_b0cfeeee7dd4575a</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static ushort.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out ushort)</br>
**签名**：_9a6ea927f4cb63da</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

