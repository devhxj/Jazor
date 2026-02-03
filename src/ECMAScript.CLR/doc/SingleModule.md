# SingleModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：float.Single()</br>
**签名**：_a6b96ca392da4917</br>

**成员**：static float.IsFinite(float)</br>
**签名**：_00118f159d09918d</br>
**注释**：

```xml
<summary>Determines whether the specified value is finite (zero, subnormal or normal).</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if the specified value is finite (zero, subnormal or normal); otherwise, <see langword="false" />.</returns>
```

**成员**：static float.IsInfinity(float)</br>
**签名**：_47887f5e1e35e199</br>
**注释**：

```xml
<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to <see cref="F:System.Single.PositiveInfinity" /> or <see cref="F:System.Single.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.IsNaN(float)</br>
**签名**：_8c3d7a2e3b690c9a</br>
**注释**：

```xml
<summary>Returns a value that indicates whether the specified value is not a number (<see cref="F:System.Single.NaN" />).</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to not a number (<see cref="F:System.Single.NaN" />); otherwise, <see langword="false" />.</returns>
```

**成员**：static float.IsNegative(float)</br>
**签名**：_846e9450c3f550b6</br>
**注释**：

```xml
<summary>Determines whether the specified value is negative.</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if negative, <see langword="false" /> otherwise.</returns>
```

**成员**：static float.IsNegativeInfinity(float)</br>
**签名**：_8b4a47cad79ef70b</br>
**注释**：

```xml
<summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to <see cref="F:System.Single.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.IsNormal(float)</br>
**签名**：_cbc5abbbccc623b6</br>
**注释**：

```xml
<summary>Determines whether the specified value is normal.</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="f" /> is normal; <see langword="false" /> otherwise.</returns>
```

**成员**：static float.IsPositiveInfinity(float)</br>
**签名**：_b2b89b81c87952dc</br>
**注释**：

```xml
<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="f" /> evaluates to <see cref="F:System.Single.PositiveInfinity" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.IsSubnormal(float)</br>
**签名**：_8e1067f50ae732cb</br>
**注释**：

```xml
<summary>Determines whether the specified value is subnormal.</summary>
<param name="f">A single-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="f" /> is subnormal; <see langword="false" /> otherwise.</returns>
```

**成员**：float.CompareTo(object)</br>
**签名**：_0b80f2f2f1a3c1a6</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
<param name="value">An object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Single" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Single.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or this instance and value are both not a number (<see cref="F:System.Single.NaN" />), <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, OR this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Single.NaN" />), OR <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：float.CompareTo(float)</br>
**签名**：_f6880f77edc2efe5</br>
**注释**：

```xml
<summary>Compares this instance to a specified single-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified single-precision floating-point number.</summary>
<param name="value">A single-precision floating-point number to compare.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Single.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or both this instance and <paramref name="value" /> are not a number (<see cref="F:System.Single.NaN" />), <see cref="F:System.Single.PositiveInfinity" />, or <see cref="F:System.Single.NegativeInfinity" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Single.NaN" />).</description></item></list></returns>
```

**成员**：static float.operator ==(float, float)</br>
**签名**：_f3cd888d249dd728</br>
**注释**：

```xml
<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> values are equal.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.operator !=(float, float)</br>
**签名**：_5778f48a657c2a49</br>
**注释**：

```xml
<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> values are not equal.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.operator <(float, float)</br>
**签名**：_9b49d03b9cec1f12</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.operator >(float, float)</br>
**签名**：_f640e4a5ea01dafa</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.operator <=(float, float)</br>
**签名**：_a5c15d0a8486be37</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.operator >=(float, float)</br>
**签名**：_de450491712f7a22</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Single"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：override float.Equals(object)</br>
**签名**：_eb69b50c7032a809</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="obj">An object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Single" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：float.Equals(float)</br>
**签名**：_5c45db76bd764c38</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Single" /> object represent the same value.</summary>
<param name="obj">An object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override float.GetHashCode()</br>
**签名**：_96e065ea302b67da</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：override float.ToString()</br>
**签名**：_a036f8edeee45300</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
<returns>The string representation of the value of this instance.</returns>
```

**成员**：float.ToString(System.IFormatProvider)</br>
**签名**：_7343d8ada7c3d925</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：float.ToString(string)</br>
**签名**：_fe0300c4411a1f62</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">A numeric format string.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：float.ToString(string, System.IFormatProvider)</br>
**签名**：_d0d4042bef295e49</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A numeric format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：float.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_3f2b511e96922b72</br>
**注释**：

```xml
<summary>Tries to format the value of the current float number instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the charactes that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：float.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_bfce4d32c259361c</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.Parse(string)</br>
**签名**：_d0492a7790d81596</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its single-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
<returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static float.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_77fa7745f751ec69</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style to its single-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is not a number in a valid format.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>A single-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static float.Parse(string, System.IFormatProvider)</br>
**签名**：_2aab5ef8cfa9accc</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified culture-specific format to its single-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
<returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static float.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_cddcce796b50f037</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</exception>
<returns>A single-precision floating-point number equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static float.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_d9762c1528057110</br>
**注释**：

```xml
<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent.</summary>
<param name="s">A character span that contains the number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />.  A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value.-or-<paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>A single-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static float.TryParse(string, out float)</br>
**签名**：_ced8b209dbd75890</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string representing a number to convert.</param>
<param name="result">When this method returns, contains single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or is not a number in a valid format. It also fails on .NET Framework and .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.TryParse(System.ReadOnlySpan<char>, out float)</br>
**签名**：_8f337f9f610204bb</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a character span to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">&gt;A character span that contains the string representation of the number to convert.</param>
<param name="result">When this method returns, contains the single-precision floating-point number equivalent of the <paramref name="s" /> parameter, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or empty or is not a number in a valid format. If <paramref name="s" /> is a valid number less than <see cref="F:System.Single.MinValue">Single.MinValue</see>, <paramref name="result" /> is <see cref="F:System.Single.NegativeInfinity" />. If <paramref name="s" /> is a valid number greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>, <paramref name="result" /> is <see cref="F:System.Single.PositiveInfinity" />. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.TryParse(System.ReadOnlySpan<byte>, out float)</br>
**签名**：_35fa5333706d7ec4</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its single-precision floating-point number equivalent.</summary>
<param name="utf8Text">A read-only UTF-8 character span that contains the number to convert.</param>
<param name="result">When this method returns, contains a single-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="utf8Text" /> if the conversion succeeded or zero if the conversion failed. The conversion fails if the <paramref name="utf8Text" /> is <see cref="P:System.ReadOnlySpan`1.Empty" /> or is not in a valid format. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out float)</br>
**签名**：_6b58aaed45e38509</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string representing a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumeration constants. It also fails on .NET Framework or .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static float.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out float)</br>
**签名**：_3a7ff2c98489b96d</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its single-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A read-only character span that contains the number to convert. The span is interpreted using the style specified by <paramref name="style" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains the single-precision floating-point number equivalent to the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not in a format compliant with <paramref name="style" />, represents a number less than <see cref="F:System.Single.MinValue">Single.MinValue</see> or greater than <see cref="F:System.Single.MaxValue">Single.MaxValue</see>, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumerated constants. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：float.GetTypeCode()</br>
**签名**：_e38cf33130abe213</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Single" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.Single" />.</returns>
```

**成员**：static float.IsPow2(float)</br>
**签名**：_0dcf89ab5d6bd60c</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.Log2(float)</br>
**签名**：_79aeb4d9a5bd7f76</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static float.Exp(float)</br>
**签名**：_9feb625727b5f8b7</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">E</code> raised to the power of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.ExpM1(float)</br>
**签名**：_225c97db4c06d542</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">Ex - 1</code></returns>
```

**成员**：static float.Exp2(float)</br>
**签名**：_850a2368fd9ebd00</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">2x</code></returns>
```

**成员**：static float.Exp2M1(float)</br>
**签名**：_bea586f79da8325a</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">2x - 1</code></returns>
```

**成员**：static float.Exp10(float)</br>
**签名**：_c4a8e15339b99e72</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">10x</code></returns>
```

**成员**：static float.Exp10M1(float)</br>
**签名**：_0c886f93ae8f2c80</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">10x - 1</code></returns>
```

**成员**：static float.Ceiling(float)</br>
**签名**：_b6616ccde8acba3f</br>
**注释**：

```xml
<summary>Computes the ceiling of a value.</summary>
<param name="x">The value whose ceiling is to be computed.</param>
<returns>The ceiling of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.ConvertToInteger<TInteger>(float)</br>
**签名**：_b860c3e3eb3014d6</br>
**注释**：

```xml
<summary>Converts a value to a specified integer type using saturation on overflow</summary>
<param name="value">The value to be converted.</param>
<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static float.ConvertToIntegerNative<TInteger>(float)</br>
**签名**：_59f5214dc916fb61</br>
**注释**：

```xml
<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
<param name="value">The value to be converted.</param>
<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static float.Floor(float)</br>
**签名**：_32eec2aa95114e61</br>
**注释**：

```xml
<summary>Computes the floor of a value.</summary>
<param name="x">The value whose floor is to be computed.</param>
<returns>The floor of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Round(float)</br>
**签名**：_99c8e34b34aa762c</br>
**注释**：

```xml
<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
<param name="x">The value to round.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using the default rounding mode.</returns>
```

**成员**：static float.Round(float, int)</br>
**签名**：_a0ef44092a5b0a96</br>
**注释**：

```xml
<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
<param name="x">The value to round.</param>
<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using the default rounding mode.</returns>
```

**成员**：static float.Round(float, System.MidpointRounding)</br>
**签名**：_34bdf4b36464daa4</br>
**注释**：

```xml
<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
<param name="x">The value to round.</param>
<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using <code data-dev-comment-type="paramref">mode</code>.</returns>
```

**成员**：static float.Round(float, int, System.MidpointRounding)</br>
**签名**：_b0f1294dc766b202</br>
**注释**：

```xml
<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
<param name="x">The value to round.</param>
<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using <code data-dev-comment-type="paramref">mode</code>.</returns>
```

**成员**：static float.Truncate(float)</br>
**签名**：_60637f5113854841</br>
**注释**：

```xml
<summary>Truncates a value.</summary>
<param name="x">The value to truncate.</param>
<returns>The truncation of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Atan2(float, float)</br>
**签名**：_81fb32cf771b3b93</br>
**注释**：

```xml
<summary>Computes the arc-tangent of the quotient of two values.</summary>
<param name="y">The y-coordinate of a point.</param>
<param name="x">The x-coordinate of a point.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Atan2Pi(float, float)</br>
**签名**：_6af9ae0f6ba947de</br>
**注释**：

```xml
<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="y">The y-coordinate of a point.</param>
<param name="x">The x-coordinate of a point.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.BitDecrement(float)</br>
**签名**：_9840b2a560428b4a</br>
**注释**：

```xml
<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
<param name="x">The value to be bitwise decremented.</param>
<returns>The smallest value that compares less than <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.BitIncrement(float)</br>
**签名**：_eac91380a48fb7bd</br>
**注释**：

```xml
<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
<param name="x">The value to be bitwise incremented.</param>
<returns>The smallest value that compares greater than <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.FusedMultiplyAdd(float, float, float)</br>
**签名**：_aff67a0c1864d405</br>
**注释**：

```xml
<summary>Computes the fused multiply-add of three values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
<param name="addend">The value that is added to the product of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</param>
<returns>The result of <code data-dev-comment-type="paramref">left</code> times <code data-dev-comment-type="paramref">right</code> plus <code data-dev-comment-type="paramref">addend</code> computed as one ternary operation.</returns>
```

**成员**：static float.Ieee754Remainder(float, float)</br>
**签名**：_e54bb5d6b1fb386d</br>
**注释**：

```xml
<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code> as specified by IEEE 754.</returns>
```

**成员**：static float.ILogB(float)</br>
**签名**：_390f9dfb01584a29</br>
**注释**：

```xml
<summary>Computes the integer logarithm of a value.</summary>
<param name="x">The value whose integer logarithm is to be computed.</param>
<returns>The integer logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Lerp(float, float, float)</br>
**签名**：_9784f111f543c6ac</br>
**注释**：

```xml
<summary>Performs a linear interpolation between two values based on the given weight.</summary>
<param name="value1">The first value, which is intended to be the lower bound.</param>
<param name="value2">The second value, which is intended to be the upper bound.</param>
<param name="amount">A value, intended to be between 0 and 1, that indicates the weight of the interpolation.</param>
<returns>The interpolated value.</returns>
```

**成员**：static float.ReciprocalEstimate(float)</br>
**签名**：_9a007a301b9dabab</br>
**注释**：

```xml
<summary>Computes an estimate of the reciprocal of a value.</summary>
<param name="x">The value whose estimate of the reciprocal is to be computed.</param>
<returns>An estimate of the reciprocal of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.ReciprocalSqrtEstimate(float)</br>
**签名**：_4ede4daffe897997</br>
**注释**：

```xml
<summary>Computes an estimate of the reciprocal square root of a value.</summary>
<param name="x">The value whose estimate of the reciprocal square root is to be computed.</param>
<returns>An estimate of the reciprocal square root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.ScaleB(float, int)</br>
**签名**：_9019f10f92f8729e</br>
**注释**：

```xml
<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
<param name="x">The value which base-radix raised to the power of <code data-dev-comment-type="paramref">n</code> multiplies.</param>
<param name="n">The value to which base-radix is raised before multipliying <code data-dev-comment-type="paramref">x</code>.</param>
<returns>The product of <code data-dev-comment-type="paramref">x</code> and base-radix raised to the power of <code data-dev-comment-type="paramref">n</code>.</returns>
```

**成员**：static float.Acosh(float)</br>
**签名**：_85424839a031a4b7</br>
**注释**：

```xml
<summary>Computes the hyperbolic arc-cosine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
<returns>The hyperbolic arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Asinh(float)</br>
**签名**：_e6b2592394f1870f</br>
**注释**：

```xml
<summary>Computes the hyperbolic arc-sine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
<returns>The hyperbolic arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Atanh(float)</br>
**签名**：_3d792e12600731b6</br>
**注释**：

```xml
<summary>Computes the hyperbolic arc-tangent of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
<returns>The hyperbolic arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Cosh(float)</br>
**签名**：_530f9f361ebd69d6</br>
**注释**：

```xml
<summary>Computes the hyperbolic cosine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
<returns>The hyperbolic cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Sinh(float)</br>
**签名**：_5ebfd243857a3667</br>
**注释**：

```xml
<summary>Computes the hyperbolic sine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
<returns>The hyperbolic sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Tanh(float)</br>
**签名**：_54702f47ad6c11df</br>
**注释**：

```xml
<summary>Computes the hyperbolic tangent of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
<returns>The hyperbolic tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Log(float)</br>
**签名**：_0311a212e027ef2d</br>
**注释**：

```xml
<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
<param name="x">The value whose natural logarithm is to be computed.</param>
<returns>The natural logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Log(float, float)</br>
**签名**：_2346aa8a14187816</br>
**注释**：

```xml
<summary>Computes the logarithm of a value in the specified base.</summary>
<param name="x">The value whose logarithm is to be computed.</param>
<param name="newBase">The base in which the logarithm is to be computed.</param>
<returns>The base-<code data-dev-comment-type="paramref">newBase</code> logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.LogP1(float)</br>
**签名**：_375f5e807e36cf8a</br>
**注释**：

```xml
<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
<param name="x">The value to which one is added before computing the natural logarithm.</param>
<returns>  <code data-dev-comment-type="c">log<sub>e</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>
```

**成员**：static float.Log10(float)</br>
**签名**：_13b3c426479d8061</br>
**注释**：

```xml
<summary>Computes the base-10 logarithm of a value.</summary>
<param name="x">The value whose base-10 logarithm is to be computed.</param>
<returns>The base-10 logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Log2P1(float)</br>
**签名**：_320a7a02cb084671</br>
**注释**：

```xml
<summary>Computes the base-2 logarithm of a value plus one.</summary>
<param name="x">The value to which one is added before computing the base-2 logarithm.</param>
<returns>  <code data-dev-comment-type="c">log<sub>2</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>
```

**成员**：static float.Log10P1(float)</br>
**签名**：_9025daef4465a5f4</br>
**注释**：

```xml
<summary>Computes the base-10 logarithm of a value plus one.</summary>
<param name="x">The value to which one is added before computing the base-10 logarithm.</param>
<returns>  <code data-dev-comment-type="c">log<sub>10</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>
```

**成员**：static float.Clamp(float, float, float)</br>
**签名**：_fa04e6b14ed00f24</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static float.ClampNative(float, float, float)</br>
**签名**：_e50ccb4182ec0a52</br>

**成员**：static float.CopySign(float, float)</br>
**签名**：_959cd3c9f503af65</br>
**注释**：

```xml
<summary>Copies the sign of a value to the sign of another value.</summary>
<param name="x" />
<param name="y" />
<param name="value">The value whose magnitude is used in the result.</param>
<param name="sign">The value whose sign is used in the result.</param>
<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
```

**成员**：static float.Max(float, float)</br>
**签名**：_b4d95f21e04b4768</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.MaxNative(float, float)</br>
**签名**：_6f3b48cdfa90d3a2</br>

**成员**：static float.MaxNumber(float, float)</br>
**签名**：_3c8d94a02631a0b0</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.Min(float, float)</br>
**签名**：_f0e565231f96990c</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.MinNative(float, float)</br>
**签名**：_334fae190a459e2d</br>

**成员**：static float.MinNumber(float, float)</br>
**签名**：_6bf468999b5de10e</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.Sign(float)</br>
**签名**：_323a6b94e62b2729</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static float.Abs(float)</br>
**签名**：_a520369f28d7dc89</br>
**注释**：

```xml
<summary>Computes the absolute of a value.</summary>
<param name="value">The value for which to get its absolute.</param>
<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static float.CreateChecked<TOther>(TOther)</br>
**签名**：_687013ac9f43fbe4</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static float.CreateSaturating<TOther>(TOther)</br>
**签名**：_21f779ed6ef58263</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static float.CreateTruncating<TOther>(TOther)</br>
**签名**：_098c80c8c595a04e</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static float.IsEvenInteger(float)</br>
**签名**：_c74cdf25f3c81cf5</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.IsInteger(float)</br>
**签名**：_b330185da27a9f39</br>
**注释**：

```xml
<summary>Determines if a value represents an integral value.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.IsOddInteger(float)</br>
**签名**：_071c1156cfc9bd2f</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.IsPositive(float)</br>
**签名**：_aac0109c854f99d4</br>
**注释**：

```xml
<summary>Determines if a value is positive.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.IsRealNumber(float)</br>
**签名**：_9966e18806e99046</br>
**注释**：

```xml
<summary>Determines if a value represents a real number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a real number; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.MaxMagnitude(float, float)</br>
**签名**：_7c146ff0a50e958f</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.MaxMagnitudeNumber(float, float)</br>
**签名**：_b7b1d7781578b7e0</br>
**注释**：

```xml
<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.MinMagnitude(float, float)</br>
**签名**：_e5a7b14f707c69f7</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.MinMagnitudeNumber(float, float)</br>
**签名**：_4a2ec5d010e27cb1</br>
**注释**：

```xml
<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.MultiplyAddEstimate(float, float, float)</br>
**签名**：_0790dc6c4730eb68</br>
**注释**：

```xml
<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
<param name="left">The value to be multiplied with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to be multiplied with <code data-dev-comment-type="paramref">left</code>.</param>
<param name="addend">The value to be added to the result of <code data-dev-comment-type="paramref">left</code> multiplied by <code data-dev-comment-type="paramref">right</code>.</param>
<returns>An estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</returns>
```

**成员**：static float.TryParse(string, System.IFormatProvider, out float)</br>
**签名**：_c6cd666235929784</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.Pow(float, float)</br>
**签名**：_9dea84f9daad7225</br>
**注释**：

```xml
<summary>Computes a value raised to a given power.</summary>
<param name="x">The value which is raised to the power of <code data-dev-comment-type="paramref">x</code>.</param>
<param name="y">The power to which <code data-dev-comment-type="paramref">x</code> is raised.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> raised to the power of <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static float.Cbrt(float)</br>
**签名**：_51ff1f64e04042ff</br>
**注释**：

```xml
<summary>Computes the cube-root of a value.</summary>
<param name="x">The value whose cube-root is to be computed.</param>
<returns>The cube-root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Hypot(float, float)</br>
**签名**：_76c7c7ae956d3449</br>
**注释**：

```xml
<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
<param name="x">The value to square and add to <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to square and add to <code data-dev-comment-type="paramref">x</code>.</param>
<returns>The square root of <code data-dev-comment-type="paramref">x</code>-squared plus <code data-dev-comment-type="paramref">y</code>-squared.</returns>
```

**成员**：static float.RootN(float, int)</br>
**签名**：_9a3da74ee8bdf7c6</br>
**注释**：

```xml
<summary>Computes the n-th root of a value.</summary>
<param name="x">The value whose <code data-dev-comment-type="paramref">n</code>-th root is to be computed.</param>
<param name="n">The degree of the root to be computed.</param>
<returns>The <code data-dev-comment-type="paramref">n</code>-th root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Sqrt(float)</br>
**签名**：_daecc788f9d305e5</br>
**注释**：

```xml
<summary>Computes the square-root of a value.</summary>
<param name="x">The value whose square-root is to be computed.</param>
<returns>The square-root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_347eb552b6176fde</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static float.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out float)</br>
**签名**：_c3b1663d39b1d889</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.Acos(float)</br>
**签名**：_fff14793e0685103</br>
**注释**：

```xml
<summary>Computes the arc-cosine of a value.</summary>
<param name="x">The value, in radians, whose arc-cosine is to be computed.</param>
<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.AcosPi(float)</br>
**签名**：_b3cd206da76e2588</br>
**注释**：

```xml
<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value whose arc-cosine is to be computed.</param>
<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.Asin(float)</br>
**签名**：_753afad06a77a6ce</br>
**注释**：

```xml
<summary>Computes the arc-sine of a value.</summary>
<param name="x">The value, in radians, whose arc-sine is to be computed.</param>
<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.AsinPi(float)</br>
**签名**：_5f4c7e35877dc08c</br>
**注释**：

```xml
<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value whose arc-sine is to be computed.</param>
<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.Atan(float)</br>
**签名**：_d91bd1cce9c18aa3</br>
**注释**：

```xml
<summary>Computes the arc-tangent of a value.</summary>
<param name="x">The value, in radians, whose arc-tangent is to be computed.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.AtanPi(float)</br>
**签名**：_4ba0e55e748cdc42</br>
**注释**：

```xml
<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
<param name="x">The value whose arc-tangent is to be computed.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.Cos(float)</br>
**签名**：_aef0ed870d0a4481</br>
**注释**：

```xml
<summary>Computes the cosine of a value.</summary>
<param name="x">The value, in radians, whose cosine is to be computed.</param>
<returns>The cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.CosPi(float)</br>
**签名**：_8901cace41b16205</br>
**注释**：

```xml
<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value, in half-revolutions, whose cosine is to be computed.</param>
<returns>The cosine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.DegreesToRadians(float)</br>
**签名**：_5973d9c23e108b1b</br>
**注释**：

```xml
<summary>Converts a given value from degrees to radians.</summary>
<param name="degrees">The value to convert to radians.</param>
<returns>The value of <code data-dev-comment-type="paramref">degrees</code> converted to radians.</returns>
```

**成员**：static float.RadiansToDegrees(float)</br>
**签名**：_b67d60ab600d4498</br>
**注释**：

```xml
<summary>Converts a given value from radians to degrees.</summary>
<param name="radians">The value to convert to degrees.</param>
<returns>The value of <code data-dev-comment-type="paramref">radians</code> converted to degrees.</returns>
```

**成员**：static float.Sin(float)</br>
**签名**：_28ff5aa7214bc112</br>
**注释**：

```xml
<summary>Computes the sine of a value.</summary>
<param name="x">The value, in radians, whose sine is to be computed.</param>
<returns>The sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.SinCos(float)</br>
**签名**：_9905e3952bca67bc</br>
**注释**：

```xml
<summary>Computes the sine and cosine of a value.</summary>
<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.SinCosPi(float)</br>
**签名**：_2c792a5d6ef88cd1</br>
**注释**：

```xml
<summary>Computes the sine and cosine of a value.</summary>
<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.SinPi(float)</br>
**签名**：_2d3a8b418dbab013</br>
**注释**：

```xml
<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its sine.</param>
<returns>The sine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.Tan(float)</br>
**签名**：_c379df7d9fb9a3bd</br>
**注释**：

```xml
<summary>Computes the tangent of a value.</summary>
<param name="x">The value, in radians, whose tangent is to be computed.</param>
<returns>The tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static float.TanPi(float)</br>
**签名**：_7775a2adde710e31</br>
**注释**：

```xml
<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its tangent.</param>
<returns>The tangent of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static float.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_5d3787482806eeab</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static float.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out float)</br>
**签名**：_b381be81bd5cd295</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static float.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_3d54467f93f0838e</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static float.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out float)</br>
**签名**：_e76b3bd6230a30ba</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

