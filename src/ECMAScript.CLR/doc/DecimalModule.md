# DecimalModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：decimal.Decimal()</br>
**签名**：_a7246904c5449b5f</br>

**成员**：decimal.Decimal(int)</br>
**签名**：_9c4dd6829012e347</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 32-bit signed integer.</summary>
<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
```

**成员**：decimal.Decimal(uint)</br>
**签名**：_73a058b17ed5de01</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 32-bit unsigned integer.</summary>
<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
```

**成员**：decimal.Decimal(long)</br>
**签名**：_188ee93a8a80b7f4</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 64-bit signed integer.</summary>
<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
```

**成员**：decimal.Decimal(ulong)</br>
**签名**：_9a3a0f6f89e1e594</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified 64-bit unsigned integer.</summary>
<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
```

**成员**：decimal.Decimal(float)</br>
**签名**：_2f7f0d9035a4bbf6</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified single-precision floating-point number.</summary>
<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see> or less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</exception>
```

**成员**：decimal.Decimal(double)</br>
**签名**：_cb7c7a937d3b8460</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to the value of the specified double-precision floating-point number.</summary>
<param name="value">The value to represent as a <see cref="T:System.Decimal" />.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see> or less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />.</exception>
```

**成员**：static decimal.FromOACurrency(long)</br>
**签名**：_6cd0f8dfbedd7209</br>
**注释**：

```xml
<summary>Converts the specified 64-bit signed integer, which contains an OLE Automation Currency value, to the equivalent <see cref="T:System.Decimal" /> value.</summary>
<param name="cy">An OLE Automation Currency value.</param>
<returns>A <see cref="T:System.Decimal" /> that contains the equivalent of <paramref name="cy" />.</returns>
```

**成员**：static decimal.ToOACurrency(decimal)</br>
**签名**：_5d257b5cc33cdaeb</br>
**注释**：

```xml
<summary>Converts the specified <see cref="T:System.Decimal" /> value to the equivalent OLE Automation Currency value, which is contained in a 64-bit signed integer.</summary>
<param name="value">The decimal number to convert.</param>
<returns>A 64-bit signed integer that contains the OLE Automation equivalent of <paramref name="value" />.</returns>
```

**成员**：decimal.Decimal(int[])</br>
**签名**：_1189e4d3b4884066</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in a specified array.</summary>
<param name="bits">An array of 32-bit signed integers containing a representation of a decimal value.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="bits" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">The length of the <paramref name="bits" /> is not 4. -or- The representation of the decimal value in <paramref name="bits" /> is not valid.</exception>
```

**成员**：decimal.Decimal(System.ReadOnlySpan<int>)</br>
**签名**：_e195522f8f6783c0</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in the specified span.</summary>
<param name="bits">A span of four <see cref="T:System.Int32" /> values that contains a binary representation of a decimal value.</param>
<exception cref="T:System.ArgumentException">The length of <paramref name="bits" /> is not 4, or the representation of the decimal value in <paramref name="bits" /> is not valid.</exception>
```

**成员**：decimal.Decimal(int, int, int, bool, byte)</br>
**签名**：_030063a806322293</br>
**注释**：

```xml
<summary>Initializes a new instance of <see cref="T:System.Decimal" /> from parameters specifying the instance's constituent parts.</summary>
<param name="lo">The low 32 bits of a 96-bit integer.</param>
<param name="mid">The middle 32 bits of a 96-bit integer.</param>
<param name="hi">The high 32 bits of a 96-bit integer.</param>
<param name="isNegative">  <see langword="true" /> to indicate a negative number; <see langword="false" /> to indicate a positive number.</param>
<param name="scale">A power of 10 ranging from 0 to 28.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="scale" /> is greater than 28.</exception>
```

**成员**：decimal.Scale.get</br>
**签名**：_db7e7c8def75fee8</br>

**成员**：static decimal.Add(decimal, decimal)</br>
**签名**：_f73258f14e05c790</br>
**注释**：

```xml
<summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The first value to add.</param>
<param name="d2">The second value to add.</param>
<exception cref="T:System.OverflowException">The sum of <paramref name="d1" /> and <paramref name="d2" /> is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The sum of <paramref name="d1" /> and <paramref name="d2" />.</returns>
```

**成员**：static decimal.Ceiling(decimal)</br>
**签名**：_84028a6e79626057</br>
**注释**：

```xml
<summary>Returns the smallest integral value that is greater than or equal to the specified decimal number.</summary>
<param name="d">A decimal number.</param>
<returns>The smallest integral value that is greater than or equal to the <paramref name="d" /> parameter. Note that this method returns a <see cref="T:System.Decimal" /> instead of an integral type.</returns>
```

**成员**：static decimal.Compare(decimal, decimal)</br>
**签名**：_c11e0aef6b5ccf1e</br>
**注释**：

```xml
<summary>Compares two specified <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>A signed number indicating the relative values of <paramref name="d1" /> and <paramref name="d2" />. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="d1" /> is less than <paramref name="d2" />.</description></item><item><term> Zero</term><description><paramref name="d1" /> and <paramref name="d2" /> are equal.</description></item><item><term> Greater than zero</term><description><paramref name="d1" /> is greater than <paramref name="d2" />.</description></item></list></returns>
```

**成员**：decimal.CompareTo(object)</br>
**签名**：_ff0e77ab6566e092</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns a comparison of their relative values.</summary>
<param name="value">The object to compare with this instance, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Decimal" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：decimal.CompareTo(decimal)</br>
**签名**：_ca8a78810233056c</br>
**注释**：

```xml
<summary>Compares this instance to a specified <see cref="T:System.Decimal" /> object and returns a comparison of their relative values.</summary>
<param name="value">The object to compare with this instance.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>
```

**成员**：static decimal.Divide(decimal, decimal)</br>
**签名**：_f5c1c0a2a040b000</br>
**注释**：

```xml
<summary>Divides two specified <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The dividend.</param>
<param name="d2">The divisor.</param>
<exception cref="T:System.DivideByZeroException">  <paramref name="d2" /> is zero.</exception>
<exception cref="T:System.OverflowException">The return value (that is, the quotient) is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The result of dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>
```

**成员**：override decimal.Equals(object)</br>
**签名**：_8abe47785e51f122</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Object" /> represent the same type and value.</summary>
<param name="value">The object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="value" /> is a <see cref="T:System.Decimal" /> and equal to this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：decimal.Equals(decimal)</br>
**签名**：_3dfd87d9d2f35e11</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Decimal" /> object represent the same value.</summary>
<param name="value">An object to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="value" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override decimal.GetHashCode()</br>
**签名**：_f58659c33299d2b1</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：static decimal.Equals(decimal, decimal)</br>
**签名**：_b25c4446c28ed255</br>
**注释**：

```xml
<summary>Returns a value indicating whether two specified instances of <see cref="T:System.Decimal" /> represent the same value.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <see langword="true" /> if <paramref name="d1" /> and <paramref name="d2" /> are equal; otherwise, <see langword="false" />.</returns>
```

**成员**：static decimal.Floor(decimal)</br>
**签名**：_518facaaeeb29ead</br>
**注释**：

```xml
<summary>Rounds a specified <see cref="T:System.Decimal" /> number to the closest integer toward negative infinity.</summary>
<param name="d">The value to round.</param>
<returns>If <paramref name="d" /> has a fractional part, the next whole <see cref="T:System.Decimal" /> number toward negative infinity that is less than <paramref name="d" />. -or- If <paramref name="d" /> doesn't have a fractional part, <paramref name="d" /> is returned unchanged. Note that the method returns an integral value of type <see cref="T:System.Decimal" />.</returns>
```

**成员**：override decimal.ToString()</br>
**签名**：_65a0e4fe8ccdd829</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
<returns>A string that represents the value of this instance.</returns>
```

**成员**：decimal.ToString(string)</br>
**签名**：_af32d07083f1da07</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">A standard or custom numeric format string.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：decimal.ToString(System.IFormatProvider)</br>
**签名**：_6234ba988b3e006d</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：decimal.ToString(string, System.IFormatProvider)</br>
**签名**：_b1e6a06111674f0c</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A numeric format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：decimal.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_919259e7087cfd17</br>
**注释**：

```xml
<summary>Tries to format the value of the current decimal instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：decimal.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_c5d11df37776e790</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.Parse(string)</br>
**签名**：_91a2436283a24315</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
<param name="s">The string representation of the number to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The equivalent to the number contained in <paramref name="s" />.</returns>
```

**成员**：static decimal.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_79a0e8ede29256cc</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
<param name="s">The string representation of the number to convert.</param>
<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see></exception>
<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" />.</returns>
```

**成员**：static decimal.Parse(string, System.IFormatProvider)</br>
**签名**：_01be2a34fe2cda4e</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
<param name="s">The string representation of the number to convert.</param>
<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific parsing information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="provider" />.</returns>
```

**成员**：static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_f525a420b2d600ec</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
<param name="s">The string representation of the number to convert.</param>
<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
<param name="provider">An <see cref="T:System.IFormatProvider" /> object that supplies culture-specific information about the format of <paramref name="s" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" /> and <paramref name="provider" />.</returns>
```

**成员**：static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_8e0c949ee2411c7f</br>
**注释**：

```xml
<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
<param name="s">The span containing the characters representing the number to convert.</param>
<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
<param name="provider">An <see cref="T:System.IFormatProvider" /> object that supplies culture-specific information about the format of <paramref name="s" />.</param>
<returns>The <see cref="T:System.Decimal" /> number equivalent to the number contained in <paramref name="s" /> as specified by <paramref name="style" /> and <paramref name="provider" />.</returns>
```

**成员**：static decimal.TryParse(string, out decimal)</br>
**签名**：_e96278809bb50e35</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">The string representation of the number to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not a number in a valid format, or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)</br>
**签名**：_5f6432cf52162431</br>
**注释**：

```xml
<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A span containing the characters representing the number to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)</br>
**签名**：_0111d7c27998205b</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its signed decimal equivalent.</summary>
<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
<param name="result">When this method returns, contains the signed decimal value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)</br>
**签名**：_b4ecd2424c9a371e</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">The string representation of the number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
<param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not a number in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)</br>
**签名**：_ed6b24306e2ef5cd</br>
**注释**：

```xml
<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A span containing the characters representing the number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Number" />.</param>
<param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.Decimal" /> number that is equivalent to the numeric value contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not a number in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>. This parameter is passed uininitialized; any value originally supplied in <paramref name="result" /> is overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static decimal.GetBits(decimal)</br>
**签名**：_e0536acf9668ef57</br>
**注释**：

```xml
<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
<param name="d">The value to convert.</param>
<returns>A 32-bit signed integer array with four elements that contain the binary representation of <paramref name="d" />.</returns>
```

**成员**：static decimal.GetBits(decimal, System.Span<int>)</br>
**签名**：_9d53437d519e15cb</br>
**注释**：

```xml
<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
<param name="d">The value to convert.</param>
<param name="destination">The span into which to store the four-integer binary representation.</param>
<exception cref="T:System.ArgumentException">The destination span was not long enough to store the binary representation.</exception>
<returns>  <see langword="4" />, which is the number of integers in the binary representation.</returns>
```

**成员**：static decimal.TryGetBits(decimal, System.Span<int>, out int)</br>
**签名**：_db7a1f9648d8e6eb</br>
**注释**：

```xml
<summary>Tries to convert the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
<param name="d">The value to convert.</param>
<param name="destination">The span into which to store the binary representation.</param>
<param name="valuesWritten">When this method returns, contains the number of integers written to the destination.</param>
<returns>  <see langword="true" /> if the decimal's binary representation was written to the destination; <see langword="false" /> if the destination wasn't long enough.</returns>
```

**成员**：static decimal.Remainder(decimal, decimal)</br>
**签名**：_700359e0de148ee3</br>
**注释**：

```xml
<summary>Computes the remainder after dividing two <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The dividend.</param>
<param name="d2">The divisor.</param>
<exception cref="T:System.DivideByZeroException">  <paramref name="d2" /> is zero.</exception>
<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The remainder after dividing <paramref name="d1" /> by <paramref name="d2" />.</returns>
```

**成员**：static decimal.Multiply(decimal, decimal)</br>
**签名**：_d5be5da3d4effe96</br>
**注释**：

```xml
<summary>Multiplies two specified <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The multiplicand.</param>
<param name="d2">The multiplier.</param>
<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The result of multiplying <paramref name="d1" /> and <paramref name="d2" />.</returns>
```

**成员**：static decimal.Negate(decimal)</br>
**签名**：_26945a698afa2a91</br>
**注释**：

```xml
<summary>Returns the result of multiplying the specified <see cref="T:System.Decimal" /> value by negative one.</summary>
<param name="d">The value to negate.</param>
<returns>A decimal number with the value of <paramref name="d" />, but the opposite sign. -or- Zero, if <paramref name="d" /> is zero.</returns>
```

**成员**：static decimal.Round(decimal)</br>
**签名**：_4a816369b59f1ca3</br>
**注释**：

```xml
<summary>Rounds a decimal value to the nearest integer.</summary>
<param name="d">A decimal number to round.</param>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> value.</exception>
<returns>The integer that is nearest to the <paramref name="d" /> parameter. If <paramref name="d" /> is halfway between two integers, one of which is even and the other odd, the even number is returned.</returns>
```

**成员**：static decimal.Round(decimal, int)</br>
**签名**：_bc3a974d51c694ab</br>
**注释**：

```xml
<summary>Rounds a <see cref="T:System.Decimal" /> value to a specified number of decimal places.</summary>
<param name="d">A decimal number to round.</param>
<param name="decimals">A value from 0 to 28 that specifies the number of decimal places to round to.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="decimals" /> is not a value from 0 to 28.</exception>
<returns>The decimal number equivalent to <paramref name="d" /> rounded to <paramref name="decimals" /> decimal places.</returns>
```

**成员**：static decimal.Round(decimal, System.MidpointRounding)</br>
**签名**：_a334f7e82122cfc2</br>
**注释**：

```xml
<summary>Rounds a decimal value to an integer using the specified rounding strategy.</summary>
<param name="d">A decimal number to round.</param>
<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a <see cref="T:System.MidpointRounding" /> value.</exception>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
<returns>The integer that <paramref name="d" /> is rounded to using the <paramref name="mode" /> rounding strategy.</returns>
```

**成员**：static decimal.Round(decimal, int, System.MidpointRounding)</br>
**签名**：_09ee3a4652dbe73c</br>
**注释**：

```xml
<summary>Rounds a decimal value to the specified precision using the specified rounding strategy.</summary>
<param name="d">A decimal number to round.</param>
<param name="decimals">The number of significant decimal places (precision) in the return value.</param>
<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="decimals" /> is less than 0 or greater than 28.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a <see cref="T:System.MidpointRounding" /> value.</exception>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" /> object.</exception>
<returns>The number that <paramref name="d" /> is rounded to using the <paramref name="mode" /> rounding strategy and with a precision of <paramref name="decimals" />. If the precision of <paramref name="d" /> is less than <paramref name="decimals" />, <paramref name="d" /> is returned unchanged.</returns>
```

**成员**：static decimal.Subtract(decimal, decimal)</br>
**签名**：_3e80f2d9cf753d05</br>
**注释**：

```xml
<summary>Subtracts a specified <see cref="T:System.Decimal" /> value from another.</summary>
<param name="d1">The minuend.</param>
<param name="d2">The subtrahend.</param>
<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The result of subtracting <paramref name="d2" /> from <paramref name="d1" />.</returns>
```

**成员**：static decimal.ToByte(decimal)</br>
**签名**：_d2aabede7e0207c1</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit unsigned integer.</summary>
<param name="value">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>.</exception>
<returns>An 8-bit unsigned integer equivalent to <paramref name="value" />.</returns>
```

**成员**：static decimal.ToSByte(decimal)</br>
**签名**：_175bf5ee849fcf8f</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit signed integer.</summary>
<param name="value">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue">SByte.MinValue</see> or greater than <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>.</exception>
<returns>An 8-bit signed integer equivalent to <paramref name="value" />.</returns>
```

**成员**：static decimal.ToInt16(decimal)</br>
**签名**：_5df8c6a064c50c5f</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit signed integer.</summary>
<param name="value">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</exception>
<returns>A 16-bit signed integer equivalent to <paramref name="value" />.</returns>
```

**成员**：static decimal.ToDouble(decimal)</br>
**签名**：_cfbbd251b43c99f4</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent double-precision floating-point number.</summary>
<param name="d">The decimal number to convert.</param>
<returns>A double-precision floating-point number equivalent to <paramref name="d" />.</returns>
```

**成员**：static decimal.ToInt32(decimal)</br>
**签名**：_ad71e0d1a8679244</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit signed integer.</summary>
<param name="d">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="d" /> is less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<returns>A 32-bit signed integer equivalent to the value of <paramref name="d" />.</returns>
```

**成员**：static decimal.ToInt64(decimal)</br>
**签名**：_7a077e2e1baba462</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit signed integer.</summary>
<param name="d">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="d" /> is less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</exception>
<returns>A 64-bit signed integer equivalent to the value of <paramref name="d" />.</returns>
```

**成员**：static decimal.ToUInt16(decimal)</br>
**签名**：_21bc553743dd324b</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit unsigned integer.</summary>
<param name="value">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see> or less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see>.</exception>
<returns>A 16-bit unsigned integer equivalent to the value of <paramref name="value" />.</returns>
```

**成员**：static decimal.ToUInt32(decimal)</br>
**签名**：_c975b2e5b2f4c009</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit unsigned integer.</summary>
<param name="d">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="d" /> is negative or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>.</exception>
<returns>A 32-bit unsigned integer equivalent to the value of <paramref name="d" />.</returns>
```

**成员**：static decimal.ToUInt64(decimal)</br>
**签名**：_9b15def492d41a4a</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit unsigned integer.</summary>
<param name="d">The decimal number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="d" /> is negative or greater than <see cref="F:System.UInt64.MaxValue">UInt64.MaxValue</see>.</exception>
<returns>A 64-bit unsigned integer equivalent to the value of <paramref name="d" />.</returns>
```

**成员**：static decimal.ToSingle(decimal)</br>
**签名**：_1450e4ab34b1a945</br>
**注释**：

```xml
<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent single-precision floating-point number.</summary>
<param name="d">The decimal number to convert.</param>
<returns>A single-precision floating-point number equivalent to the value of <paramref name="d" />.</returns>
```

**成员**：static decimal.Truncate(decimal)</br>
**签名**：_be8b149ea0e1d76b</br>
**注释**：

```xml
<summary>Returns the integral digits of the specified <see cref="T:System.Decimal" />; any fractional digits are discarded.</summary>
<param name="d">The decimal number to truncate.</param>
<returns>The result of <paramref name="d" /> rounded toward zero, to the nearest whole number.</returns>
```

**成员**：static decimal.implicit operator decimal(byte)</br>
**签名**：_c605c67b2cd1973c</br>
**注释**：

```xml
<summary>Defines an implicit conversion of an 8-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The 8-bit unsigned integer to convert.</param>
<returns>The converted 8-bit unsigned integer.</returns>
```

**成员**：static decimal.implicit operator decimal(sbyte)</br>
**签名**：_e8d5240b7aa52784</br>
**注释**：

```xml
<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
<param name="value">The 8-bit signed integer to convert.</param>
<returns>The converted 8-bit signed integer.</returns>
```

**成员**：static decimal.implicit operator decimal(short)</br>
**签名**：_8635fe57a74e1249</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a 16-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The 16-bit signed integer to convert.</param>
<returns>The converted 16-bit signed integer.</returns>
```

**成员**：static decimal.implicit operator decimal(ushort)</br>
**签名**：_7c3cfa0de18bd43c</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
<param name="value">The 16-bit unsigned integer to convert.</param>
<returns>The converted 16-bit unsigned integer.</returns>
```

**成员**：static decimal.implicit operator decimal(char)</br>
**签名**：_d4af042bf014fd51</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a Unicode character to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The Unicode character to convert.</param>
<returns>The converted Unicode character.</returns>
```

**成员**：static decimal.implicit operator decimal(int)</br>
**签名**：_f5a5d600ccd38777</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a 32-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The 32-bit signed integer to convert.</param>
<returns>The converted 32-bit signed integer.</returns>
```

**成员**：static decimal.implicit operator decimal(uint)</br>
**签名**：_d8b659cd861d2409</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
<param name="value">The 32-bit unsigned integer to convert.</param>
<returns>The converted 32-bit unsigned integer.</returns>
```

**成员**：static decimal.implicit operator decimal(long)</br>
**签名**：_23103e069358ca06</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a 64-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The 64-bit signed integer to convert.</param>
<returns>The converted 64-bit signed integer.</returns>
```

**成员**：static decimal.implicit operator decimal(ulong)</br>
**签名**：_7ab8c627f74cb718</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
<param name="value">The 64-bit unsigned integer to convert.</param>
<returns>The converted 64-bit unsigned integer.</returns>
```

**成员**：static decimal.explicit operator decimal(float)</br>
**签名**：_f456cac2ae523add</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a single-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The single-precision floating-point number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see> or less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</exception>
<returns>The converted single-precision floating point number.</returns>
```

**成员**：static decimal.explicit operator decimal(double)</br>
**签名**：_8f3a66f6dc828dff</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a double-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
<param name="value">The double-precision floating-point number to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see> or less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />.</exception>
<returns>The converted double-precision floating point number.</returns>
```

**成员**：static decimal.explicit operator byte(decimal)</br>
**签名**：_a8bfc1feb93c39cb</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit unsigned integer.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>.</exception>
<returns>An 8-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator sbyte(decimal)</br>
**签名**：_824c1dbd3e6691ba</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit signed integer. This API is not CLS-compliant.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.SByte.MinValue">SByte.MinValue</see> or greater than <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>.</exception>
<returns>An 8-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator char(decimal)</br>
**签名**：_e2c93b47df7960a8</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a Unicode character.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Char.MinValue">Char.MinValue</see> or greater than <see cref="F:System.Char.MaxValue">Char.MaxValue</see>.</exception>
<returns>A Unicode character that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator short(decimal)</br>
**签名**：_8f4ca64a21fb08cc</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int16.MinValue">Int16.MinValue</see> or greater than <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</exception>
<returns>A 16-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator ushort(decimal)</br>
**签名**：_3e209c4283c6e05e</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit unsigned integer. This API is not CLS-compliant.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.UInt16.MinValue">UInt16.MinValue</see> or greater than <see cref="F:System.UInt16.MaxValue">UInt16.MaxValue</see>.</exception>
<returns>A 16-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator int(decimal)</br>
**签名**：_bc03e302b86b6800</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<returns>A 32-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator uint(decimal)</br>
**签名**：_dea1c1c9c8f2b495</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit unsigned integer. This API is not CLS-compliant.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.UInt32.MinValue">UInt32.MinValue</see> or greater than <see cref="F:System.UInt32.MaxValue">UInt32.MaxValue</see>.</exception>
<returns>A 32-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator long(decimal)</br>
**签名**：_df6860f57d568704</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit signed integer.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</exception>
<returns>A 64-bit signed integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator ulong(decimal)</br>
**签名**：_047386be34a2d276</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit unsigned integer. This API is not CLS-compliant.</summary>
<param name="value">The value to convert.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> is negative or greater than <see cref="F:System.UInt64.MaxValue">UInt64.MaxValue</see>.</exception>
<returns>A 64-bit unsigned integer that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator float(decimal)</br>
**签名**：_2de5f5a183f9455b</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a single-precision floating-point number.</summary>
<param name="value">The value to convert.</param>
<returns>A single-precision floating-point number that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.explicit operator double(decimal)</br>
**签名**：_2db2eb304fe215ee</br>
**注释**：

```xml
<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a double-precision floating-point number.</summary>
<param name="value">The value to convert.</param>
<returns>A double-precision floating-point number that represents the converted <see cref="T:System.Decimal" />.</returns>
```

**成员**：static decimal.operator +(decimal)</br>
**签名**：_53fb6447e19a3943</br>
**注释**：

```xml
<summary>Returns the value of the <see cref="T:System.Decimal" /> operand (the sign of the operand is unchanged).</summary>
<param name="d">The operand to return.</param>
<returns>The value of the operand, <paramref name="d" />.</returns>
```

**成员**：static decimal.operator -(decimal)</br>
**签名**：_ec128cb5140788f6</br>
**注释**：

```xml
<summary>Negates the value of the specified <see cref="T:System.Decimal" /> operand.</summary>
<param name="d">The value to negate.</param>
<returns>The result of <paramref name="d" /> multiplied by negative one (-1).</returns>
```

**成员**：static decimal.operator ++(decimal)</br>
**签名**：_20e1c565f1757f95</br>
**注释**：

```xml
<summary>Increments the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by 1.</summary>
<param name="d">The value to increment.</param>
<returns>The value of <code data-dev-comment-type="paramref">d</code> incremented by 1.</returns>
```

**成员**：static decimal.operator --(decimal)</br>
**签名**：_92103936e252998e</br>
**注释**：

```xml
<summary>Decrements the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by one.</summary>
<param name="d">The value to decrement.</param>
<returns>The value of <code data-dev-comment-type="paramref">d</code> decremented by 1.</returns>
```

**成员**：static decimal.operator +(decimal, decimal)</br>
**签名**：_6916013808c205d4</br>
**注释**：

```xml
<summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The first value to add.</param>
<param name="d2">The second value to add.</param>
<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The result of adding <paramref name="d1" /> and <paramref name="d2" />.</returns>
```

**成员**：static decimal.operator -(decimal, decimal)</br>
**签名**：_7b8c963ebbb0237b</br>
**注释**：

```xml
<summary>Subtracts two specified <see cref="T:System.Decimal" /> values.</summary>
<param name="d1">The minuend.</param>
<param name="d2">The subtrahend.</param>
<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see> or greater than <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</exception>
<returns>The result of subtracting <paramref name="d2" /> from <paramref name="d1" />.</returns>
```

**成员**：static decimal.operator *(decimal, decimal)</br>
**签名**：_5794746a3d1c5c7d</br>
**注释**：

```xml
<summary>Multiplies two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
<param name="d1">The first value to multiply.</param>
<param name="d2">The second value to multiply.</param>
<returns>The result of multiplying <code data-dev-comment-type="paramref">d1</code> by <code data-dev-comment-type="paramref">d2</code>.</returns>
```

**成员**：static decimal.operator /(decimal, decimal)</br>
**签名**：_18540fea4c4d81f3</br>
**注释**：

```xml
<summary>Divides two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
<param name="d1">The dividend.</param>
<param name="d2">The divisor.</param>
<returns>The result of dividing <code data-dev-comment-type="paramref">d1</code> by <code data-dev-comment-type="paramref">d2</code>.</returns>
```

**成员**：static decimal.operator %(decimal, decimal)</br>
**签名**：_cf5ffdcf799ce372</br>
**注释**：

```xml
<summary>Returns the remainder resulting from dividing two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
<param name="d1">The dividend.</param>
<param name="d2">The divisor.</param>
<returns>The remainder resulting from dividing <code data-dev-comment-type="paramref">d1</code> by <code data-dev-comment-type="paramref">d2</code>.</returns>
```

**成员**：static decimal.operator ==(decimal, decimal)</br>
**签名**：_9831be72bebc3a57</br>
**注释**：

```xml
<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values are equal.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> and <code data-dev-comment-type="paramref">d2</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.operator !=(decimal, decimal)</br>
**签名**：_6e351e0d21e0ccd9</br>
**注释**：

```xml
<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> objects have different values.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> and <code data-dev-comment-type="paramref">d2</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.operator <(decimal, decimal)</br>
**签名**：_9e3b1978bc32f62a</br>
**注释**：

```xml
<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is less than <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.operator <=(decimal, decimal)</br>
**签名**：_01544ed3b8bf9a49</br>
**注释**：

```xml
<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is less than or equal to <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.operator >(decimal, decimal)</br>
**签名**：_bb8c4bd3620de56b</br>
**注释**：

```xml
<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is greater than <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.operator >=(decimal, decimal)</br>
**签名**：_325daf3875076acb</br>
**注释**：

```xml
<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
<param name="d1">The first value to compare.</param>
<param name="d2">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">d1</code> is greater than or equal to <code data-dev-comment-type="paramref">d2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：decimal.GetTypeCode()</br>
**签名**：_323e061741a92593</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Decimal" />.</summary>
<returns>The enumerated constant <see cref="F:System.TypeCode.Decimal" />.</returns>
```

**成员**：static decimal.ConvertToInteger<TInteger>(decimal)</br>
**签名**：_3c8005c9c5a1e322</br>
**注释**：

```xml
<summary>Converts a value to a specified integer type using saturation on overflow</summary>
<param name="value">The value to be converted.</param>
<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static decimal.ConvertToIntegerNative<TInteger>(decimal)</br>
**签名**：_c3fce0dbb13c48ea</br>
**注释**：

```xml
<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
<param name="value">The value to be converted.</param>
<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static decimal.Clamp(decimal, decimal, decimal)</br>
**签名**：_e886400fbfdbdaaa</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static decimal.CopySign(decimal, decimal)</br>
**签名**：_30df447725c40575</br>
**注释**：

```xml
<summary>Copies the sign of a value to the sign of another value.</summary>
<param name="value">The value whose magnitude is used in the result.</param>
<param name="sign">The value whose sign is used in the result.</param>
<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
```

**成员**：static decimal.Max(decimal, decimal)</br>
**签名**：_872018e11335480a</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static decimal.Min(decimal, decimal)</br>
**签名**：_ceb21f954af742e7</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static decimal.Sign(decimal)</br>
**签名**：_ed803cf9c8c052f1</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="d">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">d</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">d</code> is zero, and a negative value if <code data-dev-comment-type="paramref">d</code> is negative.</returns>
```

**成员**：static decimal.Abs(decimal)</br>
**签名**：_e85678b4de2283e8</br>
**注释**：

```xml
<summary>Computes the absolute of a value.</summary>
<param name="value">The value for which to get its absolute.</param>
<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static decimal.CreateChecked<TOther>(TOther)</br>
**签名**：_1db5e716e3d6b295</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static decimal.CreateSaturating<TOther>(TOther)</br>
**签名**：_0263284f14d9d42b</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static decimal.CreateTruncating<TOther>(TOther)</br>
**签名**：_5c966a3c7ee1bf4c</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static decimal.IsCanonical(decimal)</br>
**签名**：_b80d517d733633a6</br>
**注释**：

```xml
<summary>Determines if a value is in its canonical representation.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is in its canonical representation; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.IsEvenInteger(decimal)</br>
**签名**：_9d28fa751d24ce2e</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.IsInteger(decimal)</br>
**签名**：_e79590278b446432</br>
**注释**：

```xml
<summary>Determines if a value represents an integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.IsNegative(decimal)</br>
**签名**：_1ad42f1c78dbe014</br>
**注释**：

```xml
<summary>Determines if a value is negative.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.IsOddInteger(decimal)</br>
**签名**：_38587400d9c44cb5</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.IsPositive(decimal)</br>
**签名**：_03c325899b0e33f0</br>
**注释**：

```xml
<summary>Determines if a value is positive.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.MaxMagnitude(decimal, decimal)</br>
**签名**：_becce0ac49342bb2</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static decimal.MinMagnitude(decimal, decimal)</br>
**签名**：_5df17b0a512de878</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static decimal.TryParse(string, System.IFormatProvider, out decimal)</br>
**签名**：_a3ffdb214a9c82a0</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_c644fa2b15360347</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)</br>
**签名**：_7ac8df441c1485cf</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_e81acb76373d457e</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)</br>
**签名**：_acbda6e104ca3de4</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_d3d821054d142668</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)</br>
**签名**：_8122c647766e18ff</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

