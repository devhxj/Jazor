# Int32Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：int.Int32()</br>
**签名**：_d8bb920f83e7d97e</br>

**成员**：static int.BigMul(int, int)</br>
**签名**：_6f2c27167c45a727</br>
**注释**：

```xml
<summary>Produces the full product of two 32-bit numbers.</summary>
<param name="left">The first number to multiply.</param>
<param name="right">The second number to multiply.</param>
<returns>The number containing the product of the specified numbers.</returns>
```

**成员**：int.CompareTo(object)</br>
**签名**：_b03337a2a71c762d</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
<param name="value">An object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not an <see cref="T:System.Int32" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：int.CompareTo(int)</br>
**签名**：_741df6ab5c9e75bc</br>
**注释**：

```xml
<summary>Compares this instance to a specified 32-bit signed integer and returns an indication of their relative values.</summary>
<param name="value">An integer to compare.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>
```

**成员**：override int.Equals(object)</br>
**签名**：_3f3e17a78ac17712</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="obj">An object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Int32" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：int.Equals(int)</br>
**签名**：_5e7fb3a45e5a8f45</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int32" /> value.</summary>
<param name="obj">An <see cref="T:System.Int32" /> value to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> has the same value as this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override int.GetHashCode()</br>
**签名**：_74e858272ce4a15a</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：override int.ToString()</br>
**签名**：_0103494bc5e6253f</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
<returns>The string representation of the value of this instance, consisting of a negative sign if the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.</returns>
```

**成员**：int.ToString(string)</br>
**签名**：_2d79e025317a398b</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">A standard or custom numeric format string.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid or not supported.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：int.ToString(System.IFormatProvider)</br>
**签名**：_1c432a82e61a7193</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：int.ToString(string, System.IFormatProvider)</br>
**签名**：_f57247af306a3082</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A standard or custom numeric format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid or not supported.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：int.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_bf6eee9bbd850f13</br>
**注释**：

```xml
<summary>Tries to format the value of the current integer number instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：int.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_11b66442f91f5212</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.Parse(string)</br>
**签名**：_151ccc6045162f8f</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its 32-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<returns>A 32-bit signed integer equivalent to the number contained in <paramref name="s" />.</returns>
```

**成员**：static int.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_976d6e5278dfc58f</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style to its 32-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
<returns>A 32-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static int.Parse(string, System.IFormatProvider)</br>
**签名**：_bb24095a38bb9666</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified culture-specific format to its 32-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</exception>
<returns>A 32-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static int.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_377c7ab241784b5b</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its 32-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific information about the format of <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
<returns>A 32-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static int.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_8791c7bfd3662e63</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its 32-bit signed integer equivalent.</summary>
<param name="s">A span containing the characters representing the number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific information about the format of <paramref name="s" />.</param>
<returns>A 32-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static int.TryParse(string, out int)</br>
**签名**：_16e2a901535b765e</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="result">When this method returns, contains the 32-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static int.TryParse(System.ReadOnlySpan<char>, out int)</br>
**签名**：_f6a664534980b0f4</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a culture-specific format to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters that represent the number to convert.</param>
<param name="result">When this method returns, contains the 32-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static int.TryParse(System.ReadOnlySpan<byte>, out int)</br>
**签名**：_2acff5418dba43bd</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 32-bit signed integer equivalent.</summary>
<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 32-bit signed integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static int.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out int)</br>
**签名**：_69f925b0bfe7fa2a</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.</summary>
<param name="s">A string containing a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the 32-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static int.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out int)</br>
**签名**：_b745c572061e8b30</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters that represent the number to convert. The span is interpreted using the style specified by <paramref name="style" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the 32-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int32.MinValue">Int32.MinValue</see> or greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：int.GetTypeCode()</br>
**签名**：_5c5bca3bf690f9b1</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int32" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.Int32" />.</returns>
```

**成员**：static int.DivRem(int, int)</br>
**签名**：_d4cc9914e60e5643</br>
**注释**：

```xml
<summary>Computes the quotient and remainder of two values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static int.LeadingZeroCount(int)</br>
**签名**：_f4458d4939549cbc</br>
**注释**：

```xml
<summary>Computes the number of leading zeros in a value.</summary>
<param name="value">The value whose leading zeroes are to be counted.</param>
<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static int.PopCount(int)</br>
**签名**：_e04660fe6cb92bf1</br>
**注释**：

```xml
<summary>Computes the number of bits that are set in a value.</summary>
<param name="value">The value whose set bits are to be counted.</param>
<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static int.RotateLeft(int, int)</br>
**签名**：_f7913110e7d03a57</br>
**注释**：

```xml
<summary>Rotates a value left by a given amount.</summary>
<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static int.RotateRight(int, int)</br>
**签名**：_f090db0dba3c3b28</br>
**注释**：

```xml
<summary>Rotates a value right by a given amount.</summary>
<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static int.TrailingZeroCount(int)</br>
**签名**：_43a8a807a2b103c8</br>
**注释**：

```xml
<summary>Computes the number of trailing zeros in a value.</summary>
<param name="value">The value whose trailing zeroes are to be counted.</param>
<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static int.IsPow2(int)</br>
**签名**：_8157179708f5a6c3</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.Log2(int)</br>
**签名**：_3173781f909bc9fc</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static int.Clamp(int, int, int)</br>
**签名**：_351e597bc27e1afc</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static int.CopySign(int, int)</br>
**签名**：_95793b26c4495935</br>
**注释**：

```xml
<summary>Copies the sign of a value to the sign of another value.</summary>
<param name="value">The value whose magnitude is used in the result.</param>
<param name="sign">The value whose sign is used in the result.</param>
<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
```

**成员**：static int.Max(int, int)</br>
**签名**：_a98fdc6e84d091b3</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static int.Min(int, int)</br>
**签名**：_a0b140070c2e6328</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static int.Sign(int)</br>
**签名**：_ab2e55d493adcdd8</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static int.Abs(int)</br>
**签名**：_49bf8261f5cf3a4b</br>
**注释**：

```xml
<summary>Computes the absolute of a value.</summary>
<param name="value">The value for which to get its absolute.</param>
<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static int.CreateChecked<TOther>(TOther)</br>
**签名**：_275663af53fa5529</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static int.CreateSaturating<TOther>(TOther)</br>
**签名**：_570b24c0c63f26f9</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static int.CreateTruncating<TOther>(TOther)</br>
**签名**：_0315334a27eea649</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static int.IsEvenInteger(int)</br>
**签名**：_4cbed0ce3a7f9c5f</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.IsNegative(int)</br>
**签名**：_3d1db358d3f6d96f</br>
**注释**：

```xml
<summary>Determines if a value is negative.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.IsOddInteger(int)</br>
**签名**：_0f92a85f87224c94</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.IsPositive(int)</br>
**签名**：_280b1b013a39c514</br>
**注释**：

```xml
<summary>Determines if a value is positive.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.MaxMagnitude(int, int)</br>
**签名**：_a36b4a6dbd50fa77</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static int.MinMagnitude(int, int)</br>
**签名**：_d0c6a74fd11d24bf</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static int.TryParse(string, System.IFormatProvider, out int)</br>
**签名**：_a1335dcbd870906d</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_40d7b4fbe4ce5fc0</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static int.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out int)</br>
**签名**：_635895827c275362</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_a78d8d9d4b2f22f6</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static int.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out int)</br>
**签名**：_e40b4c4d3f2f631c</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static int.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_835ae2f52c59c7ec</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static int.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out int)</br>
**签名**：_b1fd33b593bc8df8</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

