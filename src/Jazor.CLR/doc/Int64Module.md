# Int64Module.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：long.Int64()</br>
**签名**：_74cd360dde6bde69</br>

**成员**：static long.BigMul(long, long)</br>
**签名**：_62ebef6eaaff4810</br>
**注释**：

```xml
<summary>Produces the full product of two 64-bit numbers.</summary>
<param name="left">The first number to multiply.</param>
<param name="right">The second number to multiply.</param>
<returns>The number containing the product of the specified numbers.</returns>
```

**成员**：long.CompareTo(object)</br>
**签名**：_a108636b79b7c8d2</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
<param name="value">An object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not an <see cref="T:System.Int64" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：long.CompareTo(long)</br>
**签名**：_e862e3c68f06f9e2</br>
**注释**：

```xml
<summary>Compares this instance to a specified 64-bit signed integer and returns an indication of their relative values.</summary>
<param name="value">An integer to compare.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />.</description></item></list></returns>
```

**成员**：override long.Equals(object)</br>
**签名**：_3fc2378cd670be8a</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="obj">An object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of an <see cref="T:System.Int64" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：long.Equals(long)</br>
**签名**：_73c4e4cb572f07f1</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.Int64" /> value.</summary>
<param name="obj">An <see cref="T:System.Int64" /> value to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> has the same value as this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override long.GetHashCode()</br>
**签名**：_a6f06b90e3618c16</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：override long.ToString()</br>
**签名**：_56beebc0ed49cbc9</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
<returns>The string representation of the value of this instance, consisting of a minus sign if the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.</returns>
```

**成员**：long.ToString(System.IFormatProvider)</br>
**签名**：_c9ee1b2e169f61aa</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：long.ToString(string)</br>
**签名**：_db383e0ca3b051e6</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">A numeric format string.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid or not supported.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：long.ToString(string, System.IFormatProvider)</br>
**签名**：_88dfcf515abb66f6</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A numeric format string.</param>
<param name="provider">An object that supplies culture-specific formatting information about this instance.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid or not supported.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：long.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_de37ec26b22b5ff8</br>
**注释**：

```xml
<summary>Tries to format the value of the current long number instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：long.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_a7f530e78a14a037</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.Parse(string)</br>
**签名**：_4174bb5b72e448a6</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its 64-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</exception>
<returns>A 64-bit signed integer equivalent to the number contained in <paramref name="s" />.</returns>
```

**成员**：static long.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_481fbf6d32029fcb</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style to its 64-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>. -or- <paramref name="style" /> supports fractional digits but <paramref name="s" /> includes non-zero fractional digits.</exception>
<returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static long.Parse(string, System.IFormatProvider)</br>
**签名**：_cb7366fbf6242a6a</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified culture-specific format to its 64-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in the correct format.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</exception>
<returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static long.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_540038b3f55a1010</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not in a format compliant with <paramref name="style" />.</exception>
<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>. -or- <paramref name="style" /> supports fractional digits, but <paramref name="s" /> includes non-zero fractional digits.</exception>
<returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static long.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_78d6c19de30b5937</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent.</summary>
<param name="s">A span containing the characters representing the number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
<returns>A 64-bit signed integer equivalent to the number specified in <paramref name="s" />.</returns>
```

**成员**：static long.TryParse(string, out long)</br>
**签名**：_2cba636c245c1675</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="result">When this method returns, contains the 64-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static long.TryParse(System.ReadOnlySpan<char>, out long)</br>
**签名**：_f65dcae3cb8d9ffc</br>
**注释**：

```xml
<summary>Converts the span representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A span containing the characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 64-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static long.TryParse(System.ReadOnlySpan<byte>, out long)</br>
**签名**：_8bee07df79eb3a90</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 64-bit signed integer equivalent.</summary>
<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
<param name="result">When this method returns, contains the 64-bit signed integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static long.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out long)</br>
**签名**：_de4d5fc73e6f5f38</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string containing a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the 64-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static long.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out long)</br>
**签名**：_c1dce355b4dded70</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A span containing the characters representing the number to convert. The span is interpreted using the style specified by <paramref name="style" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the 64-bit signed integer value equivalent of the number contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or represents a number less than <see cref="F:System.Int64.MinValue">Int64.MinValue</see> or greater than <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：long.GetTypeCode()</br>
**签名**：_5efdcf3dff57ebdf</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Int64" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.Int64" />.</returns>
```

**成员**：static long.DivRem(long, long)</br>
**签名**：_28273cd350760efe</br>
**注释**：

```xml
<summary>Computes the quotient and remainder of two values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
```

**成员**：static long.LeadingZeroCount(long)</br>
**签名**：_f67b17bf5c4120f2</br>
**注释**：

```xml
<summary>Computes the number of leading zeros in a value.</summary>
<param name="value">The value whose leading zeroes are to be counted.</param>
<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static long.PopCount(long)</br>
**签名**：_77fd605bbb6ce669</br>
**注释**：

```xml
<summary>Computes the number of bits that are set in a value.</summary>
<param name="value">The value whose set bits are to be counted.</param>
<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static long.RotateLeft(long, int)</br>
**签名**：_62ef461b6a515b85</br>
**注释**：

```xml
<summary>Rotates a value left by a given amount.</summary>
<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static long.RotateRight(long, int)</br>
**签名**：_6a70bc88f689ce73</br>
**注释**：

```xml
<summary>Rotates a value right by a given amount.</summary>
<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
```

**成员**：static long.TrailingZeroCount(long)</br>
**签名**：_df6d7288bc845b53</br>
**注释**：

```xml
<summary>Computes the number of trailing zeros in a value.</summary>
<param name="value">The value whose trailing zeroes are to be counted.</param>
<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static long.IsPow2(long)</br>
**签名**：_fd78c89cf0a7feff</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.Log2(long)</br>
**签名**：_e90fc1096a04c8f9</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static long.Clamp(long, long, long)</br>
**签名**：_8e63712ecf0da200</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static long.CopySign(long, long)</br>
**签名**：_dd2c6c8297bd4df3</br>
**注释**：

```xml
<summary>Copies the sign of a value to the sign of another value.</summary>
<param name="value">The value whose magnitude is used in the result.</param>
<param name="sign">The value whose sign is used in the result.</param>
<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
```

**成员**：static long.Max(long, long)</br>
**签名**：_2c60dae3f93fedef</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static long.Min(long, long)</br>
**签名**：_e9f5fe363044ceda</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static long.Sign(long)</br>
**签名**：_003e583f1faf343b</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static long.Abs(long)</br>
**签名**：_6ae5b36df368d1e5</br>
**注释**：

```xml
<summary>Computes the absolute of a value.</summary>
<param name="value">The value for which to get its absolute.</param>
<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static long.CreateChecked<TOther>(TOther)</br>
**签名**：_a7b7a24d0da5bf7e</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static long.CreateSaturating<TOther>(TOther)</br>
**签名**：_bef5211a1d823672</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static long.CreateTruncating<TOther>(TOther)</br>
**签名**：_363a470fd9444076</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static long.IsEvenInteger(long)</br>
**签名**：_203dfc08764b3516</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.IsNegative(long)</br>
**签名**：_cac37e2db2e55b1b</br>
**注释**：

```xml
<summary>Determines if a value is negative.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is negative; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.IsOddInteger(long)</br>
**签名**：_23594f30886ac699</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.IsPositive(long)</br>
**签名**：_3c8be08897a76569</br>
**注释**：

```xml
<summary>Determines if a value is positive.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.MaxMagnitude(long, long)</br>
**签名**：_9618dc0d855ee729</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static long.MinMagnitude(long, long)</br>
**签名**：_bfad1ee52075b36e</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static long.TryParse(string, System.IFormatProvider, out long)</br>
**签名**：_6f90bee529e2eb6c</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_22b931abaca743ae</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static long.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out long)</br>
**签名**：_1fa9b46a2b1345f4</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_37d384b6ca28fb02</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static long.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out long)</br>
**签名**：_0ea07687b9ce11f1</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static long.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_45277b7b17f7a046</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static long.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out long)</br>
**签名**：_232a05c0262521da</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

