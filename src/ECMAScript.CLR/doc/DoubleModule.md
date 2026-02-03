# DoubleModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：double.Double()</br>
**签名**：_f28ac141e9398355</br>

**成员**：static double.IsFinite(double)</br>
**签名**：_aed2927097617729</br>
**注释**：

```xml
<summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if the value is finite (zero, subnormal or normal); <see langword="false" /> otherwise.</returns>
```

**成员**：static double.IsInfinity(double)</br>
**签名**：_8dab2b2ebaef92eb</br>
**注释**：

```xml
<summary>Returns a value indicating whether the specified number evaluates to negative or positive infinity.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.PositiveInfinity" /> or <see cref="F:System.Double.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.IsNaN(double)</br>
**签名**：_24e14b276e0c7e30</br>
**注释**：

```xml
<summary>Returns a value that indicates whether the specified value is not a number (<see cref="F:System.Double.NaN" />).</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.NaN" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.IsNegative(double)</br>
**签名**：_2f6ba4398ec15d8d</br>
**注释**：

```xml
<summary>Determines whether the specified value is negative.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if the value is negative; <see langword="false" /> otherwise.</returns>
```

**成员**：static double.IsNegativeInfinity(double)</br>
**签名**：_f0fb1d1302b488d6</br>
**注释**：

```xml
<summary>Returns a value indicating whether the specified number evaluates to negative infinity.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.NegativeInfinity" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.IsNormal(double)</br>
**签名**：_9b3adc853b9cfe8f</br>
**注释**：

```xml
<summary>Determines whether the specified value is normal.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if the value is normal; <see langword="false" /> otherwise.</returns>
```

**成员**：static double.IsPositiveInfinity(double)</br>
**签名**：_d15ff5d4064e951a</br>
**注释**：

```xml
<summary>Returns a value indicating whether the specified number evaluates to positive infinity.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if <paramref name="d" /> evaluates to <see cref="F:System.Double.PositiveInfinity" />; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.IsSubnormal(double)</br>
**签名**：_a48f9d7298aa7e76</br>
**注释**：

```xml
<summary>Determines whether the specified value is subnormal.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>  <see langword="true" /> if the value is subnormal; <see langword="false" /> otherwise.</returns>
```

**成员**：double.CompareTo(object)</br>
**签名**：_b0d483b6deae2278</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.</summary>
<param name="value">An object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Double" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> A negative integer</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Double.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or this instance and <paramref name="value" /> are both <see langword="Double.NaN" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" /></description></item><item><term> A positive integer</term><description> This instance is greater than <paramref name="value" />, OR this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Double.NaN" />), OR <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：double.CompareTo(double)</br>
**签名**：_7b8150796366d2b1</br>
**注释**：

```xml
<summary>Compares this instance to a specified double-precision floating-point number and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified double-precision floating-point number.</summary>
<param name="value">A double-precision floating-point number to compare.</param>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />, or this instance is not a number (<see cref="F:System.Double.NaN" />) and <paramref name="value" /> is a number.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />, or both this instance and <paramref name="value" /> are not a number (<see cref="F:System.Double.NaN" />), <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NegativeInfinity" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or this instance is a number and <paramref name="value" /> is not a number (<see cref="F:System.Double.NaN" />).</description></item></list></returns>
```

**成员**：override double.Equals(object)</br>
**签名**：_b5f97a04bba189b0</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="obj">An object to compare with this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Double" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.operator ==(double, double)</br>
**签名**：_a4d750aa912f2bd7</br>
**注释**：

```xml
<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> values are equal.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.operator !=(double, double)</br>
**签名**：_d17fe84520a83d30</br>
**注释**：

```xml
<summary>Returns a value that indicates whether two specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> values are not equal.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.operator <(double, double)</br>
**签名**：_f33377c7d472de67</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.operator >(double, double)</br>
**签名**：_0ff0091b916b4a34</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.operator <=(double, double)</br>
**签名**：_cda1ab775e265c7b</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is less than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.operator >=(double, double)</br>
**签名**：_4f7605355b48150a</br>
**注释**：

```xml
<summary>Returns a value that indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value.</summary>
<param name="left">The first value to compare.</param>
<param name="right">The second value to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">left</code> is greater than or equal to <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：double.Equals(double)</br>
**签名**：_6c01d37504f73181</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Double" /> object represent the same value.</summary>
<param name="obj">A <see cref="T:System.Double" /> object to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override double.GetHashCode()</br>
**签名**：_73dea7106d8085a6</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：override double.ToString()</br>
**签名**：_faf4dc1f54bddf75</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
<returns>The string representation of the value of this instance.</returns>
```

**成员**：double.ToString(string)</br>
**签名**：_3fdd3b28b5e148e9</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
<param name="format">A numeric format string.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.</exception>
<returns>The string representation of the value of this instance as specified by <paramref name="format" />.</returns>
```

**成员**：double.ToString(System.IFormatProvider)</br>
**签名**：_060e7930ebdb6c74</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：double.ToString(string, System.IFormatProvider)</br>
**签名**：_3ab59f70a1114579</br>
**注释**：

```xml
<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A numeric format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：double.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_10530f8449c5e278</br>
**注释**：

```xml
<summary>Tries to format the value of the current double instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：double.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_d57e531de43c78e1</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.Parse(string)</br>
**签名**：_5810f85a3710b88d</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its double-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static double.Parse(string, System.Globalization.NumberStyles)</br>
**签名**：_41091ebfff87c5a3</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style to its double-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />. A typical value to specify is a combination of <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static double.Parse(string, System.IFormatProvider)</br>
**签名**：_5b091c28760d19a0</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified culture-specific format to its double-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a number in a valid format.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static double.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_e23e5c173e845cc9</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent.</summary>
<param name="s">A string that contains a number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<exception cref="T:System.OverflowException">          .NET Framework and .NET Core 2.2 and earlier versions only: <paramref name="s" /> represents a number that is less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</exception>
<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static double.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_1566d690221e91c2</br>
**注释**：

```xml
<summary>Converts a character span that contains the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent.</summary>
<param name="s">A character span that contains the number to convert.</param>
<param name="style">A bitwise combination of enumeration values that indicate the style elements that can be present in <paramref name="s" />.  A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not represent a numeric value.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>A double-precision floating-point number that is equivalent to the numeric value or symbol specified in <paramref name="s" />.</returns>
```

**成员**：static double.TryParse(string, out double)</br>
**签名**：_a29d389185c5e37d</br>
**注释**：

```xml
<summary>Converts the string representation of a number to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="result">When this method returns, contains the double-precision floating-point number equivalent of the <paramref name="s" /> parameter, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or is not a number in a valid format. It also fails on .NET Framework and .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.Double.MinValue">Double.MinValue</see> or greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.TryParse(System.ReadOnlySpan<char>, out double)</br>
**签名**：_059799e0a3b763c1</br>
**注释**：

```xml
<summary>Converts the span representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A character span that contains the string representation of the number to convert.</param>
<param name="result">When this method returns, contains the double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="s" /> parameter, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or empty. If <paramref name="s" /> is a valid number less than <see cref="F:System.Double.MinValue">Double.MinValue</see>, <paramref name="result" /> is <see cref="F:System.Double.NegativeInfinity" />. If <paramref name="s" /> is a valid number greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>, <paramref name="result" /> is <see cref="F:System.Double.PositiveInfinity" />. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.TryParse(System.ReadOnlySpan<byte>, out double)</br>
**签名**：_ec88293b6cb03791</br>
**注释**：

```xml
<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its double-precision floating-point number equivalent.</summary>
<param name="utf8Text">A read-only UTF-8 character span that contains the number to convert.</param>
<param name="result">When this method returns, contains a double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="utf8Text" /> if the conversion succeeded or zero if the conversion failed. The conversion fails if the <paramref name="utf8Text" /> is <see cref="P:System.ReadOnlySpan`1.Empty" /> or is not in a valid format. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out double)</br>
**签名**：_ac0f50fde0490598</br>
**注释**：

```xml
<summary>Converts the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string containing a number to convert.</param>
<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An <see cref="T:System.IFormatProvider" /> that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns, contains a double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" /> or is not in a format compliant with <paramref name="style" />, or if <paramref name="style" /> is not a valid combination of <see cref="T:System.Globalization.NumberStyles" /> enumeration constants. It also fails on .NET Framework or .NET Core 2.2 and earlier versions if <paramref name="s" /> represents a number less than <see cref="F:System.SByte.MinValue">SByte.MinValue</see> or greater than <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> includes the <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> value.</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static double.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out double)</br>
**签名**：_632e234f0359bd6f</br>
**注释**：

```xml
<summary>Converts a character span containing the string representation of a number in a specified style and culture-specific format to its double-precision floating-point number equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
<param name="s">A read-only character span that contains the number to convert.</param>
<param name="style">A bitwise combination of <see cref="T:System.Globalization.NumberStyles" /> values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Float" /> combined with <see cref="F:System.Globalization.NumberStyles.AllowThousands" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="result">When this method returns and if the conversion succeeded, contains a double-precision floating-point number equivalent of the numeric value or symbol contained in <paramref name="s" />. Contains zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, an empty character span, or not a number in a format compliant with <paramref name="style" />. If <paramref name="s" /> is a valid number less than <see cref="F:System.Double.MinValue">Double.MinValue</see>, <paramref name="result" /> is <see cref="F:System.Double.NegativeInfinity" />. If <paramref name="s" /> is a valid number greater than <see cref="F:System.Double.MaxValue">Double.MaxValue</see>, <paramref name="result" /> is <see cref="F:System.Double.PositiveInfinity" />. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：double.GetTypeCode()</br>
**签名**：_faf3eda13d4c24c6</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Double" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.Double" />.</returns>
```

**成员**：static double.IsPow2(double)</br>
**签名**：_0f9f49a802919a8f</br>
**注释**：

```xml
<summary>Determines if a value is a power of two.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.Log2(double)</br>
**签名**：_3ca26f53faecc630</br>
**注释**：

```xml
<summary>Computes the log2 of a value.</summary>
<param name="value">The value whose log2 is to be computed.</param>
<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static double.Exp(double)</br>
**签名**：_e94626bfb529f1e2</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">E</code> raised to the power of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.ExpM1(double)</br>
**签名**：_1a8fc1577d8842a1</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">E</code> raised to a given power and subtracts one.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">E</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">Ex - 1</code></returns>
```

**成员**：static double.Exp2(double)</br>
**签名**：_894bcd9f10fe195f</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">2x</code></returns>
```

**成员**：static double.Exp2M1(double)</br>
**签名**：_b2c7a69c53b5558f</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">2</code> raised to a given power and subtracts one.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">2</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">2x - 1</code></returns>
```

**成员**：static double.Exp10(double)</br>
**签名**：_433ea7f5bfe42847</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">10x</code></returns>
```

**成员**：static double.Exp10M1(double)</br>
**签名**：_aece0b0b794624da</br>
**注释**：

```xml
<summary>Computes <code data-dev-comment-type="c">10</code> raised to a given power and subtracts one.</summary>
<param name="x">The power to which <code data-dev-comment-type="c">10</code> is raised.</param>
<returns>  <code data-dev-comment-type="c">10x - 1</code></returns>
```

**成员**：static double.Ceiling(double)</br>
**签名**：_e435d9759ac9c07d</br>
**注释**：

```xml
<summary>Computes the ceiling of a value.</summary>
<param name="x">The value whose ceiling is to be computed.</param>
<returns>The ceiling of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.ConvertToInteger<TInteger>(double)</br>
**签名**：_cf8db91150253994</br>
**注释**：

```xml
<summary>Converts a value to a specified integer type using saturation on overflow</summary>
<param name="value">The value to be converted.</param>
<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static double.ConvertToIntegerNative<TInteger>(double)</br>
**签名**：_869e51717acd1e28</br>
**注释**：

```xml
<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
<param name="value">The value to be converted.</param>
<typeparam name="TInteger">The integer type to which <code data-dev-comment-type="paramref">value</code> is converted.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TInteger</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static double.Floor(double)</br>
**签名**：_52dffd07187dd0c2</br>
**注释**：

```xml
<summary>Computes the floor of a value.</summary>
<param name="x">The value whose floor is to be computed.</param>
<returns>The floor of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Round(double)</br>
**签名**：_0bc6b7459346bc5f</br>
**注释**：

```xml
<summary>Rounds a value to the nearest integer using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
<param name="x">The value to round.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using the default rounding mode.</returns>
```

**成员**：static double.Round(double, int)</br>
**签名**：_b439595e3752c6a9</br>
**注释**：

```xml
<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
<param name="x">The value to round.</param>
<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using the default rounding mode.</returns>
```

**成员**：static double.Round(double, System.MidpointRounding)</br>
**签名**：_7aeacc68b27f02f7</br>
**注释**：

```xml
<summary>Rounds a value to the nearest integer using the specified rounding mode.</summary>
<param name="x">The value to round.</param>
<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to the nearest integer using <code data-dev-comment-type="paramref">mode</code>.</returns>
```

**成员**：static double.Round(double, int, System.MidpointRounding)</br>
**签名**：_6e429701c9779ef6</br>
**注释**：

```xml
<summary>Rounds a value to a specified number of fractional-digits using the default rounding mode (<xref data-throw-if-not-resolved="true" uid="System.MidpointRounding.ToEven"></xref>).</summary>
<param name="x">The value to round.</param>
<param name="digits">The number of fractional digits to which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<param name="mode">The mode under which <code data-dev-comment-type="paramref">x</code> should be rounded.</param>
<returns>The result of rounding <code data-dev-comment-type="paramref">x</code> to <code data-dev-comment-type="paramref">digits</code> fractional-digits using <code data-dev-comment-type="paramref">mode</code>.</returns>
```

**成员**：static double.Truncate(double)</br>
**签名**：_98f3d13b9b717048</br>
**注释**：

```xml
<summary>Truncates a value.</summary>
<param name="x">The value to truncate.</param>
<returns>The truncation of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Atan2(double, double)</br>
**签名**：_d606d02df668235c</br>
**注释**：

```xml
<summary>Computes the arc-tangent of the quotient of two values.</summary>
<param name="y">The y-coordinate of a point.</param>
<param name="x">The x-coordinate of a point.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Atan2Pi(double, double)</br>
**签名**：_f54e39103ea7d6b5</br>
**注释**：

```xml
<summary>Computes the arc-tangent for the quotient of two values and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="y">The y-coordinate of a point.</param>
<param name="x">The x-coordinate of a point.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">y</code> divided-by <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.BitDecrement(double)</br>
**签名**：_4ce9474a7b3b7534</br>
**注释**：

```xml
<summary>Decrements a value to the smallest value that compares less than a given value.</summary>
<param name="x">The value to be bitwise decremented.</param>
<returns>The smallest value that compares less than <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.BitIncrement(double)</br>
**签名**：_a83d47e386f63de0</br>
**注释**：

```xml
<summary>Increments a value to the smallest value that compares greater than a given value.</summary>
<param name="x">The value to be bitwise incremented.</param>
<returns>The smallest value that compares greater than <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.FusedMultiplyAdd(double, double, double)</br>
**签名**：_a7385e0d1e651c3f</br>
**注释**：

```xml
<summary>Computes the fused multiply-add of three values.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> multiplies.</param>
<param name="right">The value which multiplies <code data-dev-comment-type="paramref">left</code>.</param>
<param name="addend">The value that is added to the product of <code data-dev-comment-type="paramref">left</code> and <code data-dev-comment-type="paramref">right</code>.</param>
<returns>The result of <code data-dev-comment-type="paramref">left</code> times <code data-dev-comment-type="paramref">right</code> plus <code data-dev-comment-type="paramref">addend</code> computed as one ternary operation.</returns>
```

**成员**：static double.Ieee754Remainder(double, double)</br>
**签名**：_092bc2bc891d33a8</br>
**注释**：

```xml
<summary>Computes the remainder of two values as specified by IEEE 754.</summary>
<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
<returns>The remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code> as specified by IEEE 754.</returns>
```

**成员**：static double.ILogB(double)</br>
**签名**：_48628732b1dc8ac9</br>
**注释**：

```xml
<summary>Computes the integer logarithm of a value.</summary>
<param name="x">The value whose integer logarithm is to be computed.</param>
<returns>The integer logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Lerp(double, double, double)</br>
**签名**：_a5426c98bc8a2df3</br>
**注释**：

```xml
<summary>Performs a linear interpolation between two values based on the given weight.</summary>
<param name="value1">The first value, which is intended to be the lower bound.</param>
<param name="value2">The second value, which is intended to be the upper bound.</param>
<param name="amount">A value, intended to be between 0 and 1, that indicates the weight of the interpolation.</param>
<returns>The interpolated value.</returns>
```

**成员**：static double.ReciprocalEstimate(double)</br>
**签名**：_a07d02f7af20108d</br>
**注释**：

```xml
<summary>Computes an estimate of the reciprocal of a value.</summary>
<param name="x">The value whose estimate of the reciprocal is to be computed.</param>
<returns>An estimate of the reciprocal of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.ReciprocalSqrtEstimate(double)</br>
**签名**：_093ed023d5ee163e</br>
**注释**：

```xml
<summary>Computes an estimate of the reciprocal square root of a value.</summary>
<param name="x">The value whose estimate of the reciprocal square root is to be computed.</param>
<returns>An estimate of the reciprocal square root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.ScaleB(double, int)</br>
**签名**：_efc90b780554b82f</br>
**注释**：

```xml
<summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
<param name="x">The value which base-radix raised to the power of <code data-dev-comment-type="paramref">n</code> multiplies.</param>
<param name="n">The value to which base-radix is raised before multipliying <code data-dev-comment-type="paramref">x</code>.</param>
<returns>The product of <code data-dev-comment-type="paramref">x</code> and base-radix raised to the power of <code data-dev-comment-type="paramref">n</code>.</returns>
```

**成员**：static double.Acosh(double)</br>
**签名**：_a0e391e3d9aa5827</br>
**注释**：

```xml
<summary>Computes the hyperbolic arc-cosine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
<returns>The hyperbolic arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Asinh(double)</br>
**签名**：_57778d867801a120</br>
**注释**：

```xml
<summary>Computes the hyperbolic arc-sine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
<returns>The hyperbolic arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Atanh(double)</br>
**签名**：_21375f189d937aa8</br>
**注释**：

```xml
<summary>Computes the hyperbolic arc-tangent of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
<returns>The hyperbolic arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Cosh(double)</br>
**签名**：_e4a259570c5acab6</br>
**注释**：

```xml
<summary>Computes the hyperbolic cosine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
<returns>The hyperbolic cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Sinh(double)</br>
**签名**：_dea96f28cdef92ad</br>
**注释**：

```xml
<summary>Computes the hyperbolic sine of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
<returns>The hyperbolic sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Tanh(double)</br>
**签名**：_5169c7d89ba27c38</br>
**注释**：

```xml
<summary>Computes the hyperbolic tangent of a value.</summary>
<param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
<returns>The hyperbolic tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Log(double)</br>
**签名**：_f89aa2d9ce52cc5e</br>
**注释**：

```xml
<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code> logarithm of a value.</summary>
<param name="x">The value whose natural logarithm is to be computed.</param>
<returns>The natural logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Log(double, double)</br>
**签名**：_2367dc158f1f7ec9</br>
**注释**：

```xml
<summary>Computes the logarithm of a value in the specified base.</summary>
<param name="x">The value whose logarithm is to be computed.</param>
<param name="newBase">The base in which the logarithm is to be computed.</param>
<returns>The base-<code data-dev-comment-type="paramref">newBase</code> logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.LogP1(double)</br>
**签名**：_379f80adec6e897b</br>
**注释**：

```xml
<summary>Computes the natural (<code data-dev-comment-type="c">base-E</code>) logarithm of a value plus one.</summary>
<param name="x">The value to which one is added before computing the natural logarithm.</param>
<returns>  <code data-dev-comment-type="c">log<sub>e</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>
```

**成员**：static double.Log2P1(double)</br>
**签名**：_0f38233678cfefdc</br>
**注释**：

```xml
<summary>Computes the base-2 logarithm of a value plus one.</summary>
<param name="x">The value to which one is added before computing the base-2 logarithm.</param>
<returns>  <code data-dev-comment-type="c">log<sub>2</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>
```

**成员**：static double.Log10(double)</br>
**签名**：_d057b30c2fca7de9</br>
**注释**：

```xml
<summary>Computes the base-10 logarithm of a value.</summary>
<param name="x">The value whose base-10 logarithm is to be computed.</param>
<returns>The base-10 logarithm of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Log10P1(double)</br>
**签名**：_f0b78003a9ab01fb</br>
**注释**：

```xml
<summary>Computes the base-10 logarithm of a value plus one.</summary>
<param name="x">The value to which one is added before computing the base-10 logarithm.</param>
<returns>  <code data-dev-comment-type="c">log<sub>10</sub>(<code data-dev-comment-type="paramref">x</code> + 1)</code></returns>
```

**成员**：static double.Clamp(double, double, double)</br>
**签名**：_8a90b4c9a1beefd9</br>
**注释**：

```xml
<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
<param name="value">The value to clamp.</param>
<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
```

**成员**：static double.ClampNative(double, double, double)</br>
**签名**：_ead55aa3a172f045</br>

**成员**：static double.CopySign(double, double)</br>
**签名**：_7d753440d9da2ba5</br>
**注释**：

```xml
<summary>Copies the sign of a value to the sign of another value.</summary>
<param name="x" />
<param name="y" />
<param name="value">The value whose magnitude is used in the result.</param>
<param name="sign">The value whose sign is used in the result.</param>
<returns>A value with the magnitude of <code data-dev-comment-type="paramref">value</code> and the sign of <code data-dev-comment-type="paramref">sign</code>.</returns>
```

**成员**：static double.Max(double, double)</br>
**签名**：_4d275f0cc2087a70</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.MaxNative(double, double)</br>
**签名**：_a0dd8cfd308fc2ee</br>

**成员**：static double.MaxNumber(double, double)</br>
**签名**：_ca88bd0ea64fa29f</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.Min(double, double)</br>
**签名**：_8a25c3cdacb6ea23</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.MinNative(double, double)</br>
**签名**：_2aadcd7ef1e13714</br>

**成员**：static double.MinNumber(double, double)</br>
**签名**：_d19f0527d6ae110f</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.Sign(double)</br>
**签名**：_eee146c74a9bc322</br>
**注释**：

```xml
<summary>Computes the sign of a value.</summary>
<param name="value">The value whose sign is to be computed.</param>
<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
```

**成员**：static double.Abs(double)</br>
**签名**：_13256ae561a599a8</br>
**注释**：

```xml
<summary>Computes the absolute of a value.</summary>
<param name="value">The value for which to get its absolute.</param>
<returns>The absolute of <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static double.CreateChecked<TOther>(TOther)</br>
**签名**：_ddfc88bb430f2c3e</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
```

**成员**：static double.CreateSaturating<TOther>(TOther)</br>
**签名**：_5bb76ff1642d9cf8</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static double.CreateTruncating<TOther>(TOther)</br>
**签名**：_e3a12f862df0ccea</br>
**注释**：

```xml
<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
```

**成员**：static double.IsEvenInteger(double)</br>
**签名**：_e3c00c1b96ee23bd</br>
**注释**：

```xml
<summary>Determines if a value represents an even integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.IsInteger(double)</br>
**签名**：_f0cb8da3d3123834</br>
**注释**：

```xml
<summary>Determines if a value represents an integral value.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.IsOddInteger(double)</br>
**签名**：_0f52036842645ea9</br>
**注释**：

```xml
<summary>Determines if a value represents an odd integral number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.IsPositive(double)</br>
**签名**：_c1220c050b39d180</br>
**注释**：

```xml
<summary>Determines if a value is positive.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is positive; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.IsRealNumber(double)</br>
**签名**：_0e7439da8bbce1ab</br>
**注释**：

```xml
<summary>Determines if a value represents a real number.</summary>
<param name="value">The value to be checked.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a real number; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.MaxMagnitude(double, double)</br>
**签名**：_b6202851542d164c</br>
**注释**：

```xml
<summary>Compares two values to compute which is greater.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.MaxMagnitudeNumber(double, double)</br>
**签名**：_7f7b38b043f3f42f</br>
**注释**：

```xml
<summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.MinMagnitude(double, double)</br>
**签名**：_bb1daa880a2ad14e</br>
**注释**：

```xml
<summary>Compares two values to compute which is lesser.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.MinMagnitudeNumber(double, double)</br>
**签名**：_315c6cdfa11efcf2</br>
**注释**：

```xml
<summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <code data-dev-comment-type="c">NaN</code>.</summary>
<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.MultiplyAddEstimate(double, double, double)</br>
**签名**：_a3676143141ac38a</br>
**注释**：

```xml
<summary>Computes an estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</summary>
<param name="left">The value to be multiplied with <code data-dev-comment-type="paramref">right</code>.</param>
<param name="right">The value to be multiplied with <code data-dev-comment-type="paramref">left</code>.</param>
<param name="addend">The value to be added to the result of <code data-dev-comment-type="paramref">left</code> multiplied by <code data-dev-comment-type="paramref">right</code>.</param>
<returns>An estimate of (<code data-dev-comment-type="paramref">left</code> * <code data-dev-comment-type="paramref">right</code>) + <code data-dev-comment-type="paramref">addend</code>.</returns>
```

**成员**：static double.TryParse(string, System.IFormatProvider, out double)</br>
**签名**：_f1644d5121fae09c</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.Pow(double, double)</br>
**签名**：_a9ce690fc0374936</br>
**注释**：

```xml
<summary>Computes a value raised to a given power.</summary>
<param name="x">The value which is raised to the power of <code data-dev-comment-type="paramref">x</code>.</param>
<param name="y">The power to which <code data-dev-comment-type="paramref">x</code> is raised.</param>
<returns>  <code data-dev-comment-type="paramref">x</code> raised to the power of <code data-dev-comment-type="paramref">y</code>.</returns>
```

**成员**：static double.Cbrt(double)</br>
**签名**：_be2f8c6b23df2f9d</br>
**注释**：

```xml
<summary>Computes the cube-root of a value.</summary>
<param name="x">The value whose cube-root is to be computed.</param>
<returns>The cube-root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Hypot(double, double)</br>
**签名**：_7b8e31add532abe8</br>
**注释**：

```xml
<summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
<param name="x">The value to square and add to <code data-dev-comment-type="paramref">y</code>.</param>
<param name="y">The value to square and add to <code data-dev-comment-type="paramref">x</code>.</param>
<returns>The square root of <code data-dev-comment-type="paramref">x</code>-squared plus <code data-dev-comment-type="paramref">y</code>-squared.</returns>
```

**成员**：static double.RootN(double, int)</br>
**签名**：_83649fc6ded4d88e</br>
**注释**：

```xml
<summary>Computes the n-th root of a value.</summary>
<param name="x">The value whose <code data-dev-comment-type="paramref">n</code>-th root is to be computed.</param>
<param name="n">The degree of the root to be computed.</param>
<returns>The <code data-dev-comment-type="paramref">n</code>-th root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Sqrt(double)</br>
**签名**：_73df268429011d00</br>
**注释**：

```xml
<summary>Computes the square-root of a value.</summary>
<param name="x">The value whose square-root is to be computed.</param>
<returns>The square-root of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_ffac89005f82f8e5</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static double.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out double)</br>
**签名**：_55ffdd4c4ffdc9a8</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.Acos(double)</br>
**签名**：_1c32d7b441f1bec1</br>
**注释**：

```xml
<summary>Computes the arc-cosine of a value.</summary>
<param name="x">The value, in radians, whose arc-cosine is to be computed.</param>
<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.AcosPi(double)</br>
**签名**：_4a99593b807868d6</br>
**注释**：

```xml
<summary>Computes the arc-cosine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value whose arc-cosine is to be computed.</param>
<returns>The arc-cosine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.Asin(double)</br>
**签名**：_517eb387ef38a60b</br>
**注释**：

```xml
<summary>Computes the arc-sine of a value.</summary>
<param name="x">The value, in radians, whose arc-sine is to be computed.</param>
<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.AsinPi(double)</br>
**签名**：_1a0239dc7bac42d0</br>
**注释**：

```xml
<summary>Computes the arc-sine of a value and divides the result by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value whose arc-sine is to be computed.</param>
<returns>The arc-sine of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.Atan(double)</br>
**签名**：_a6a8f60d8be1baab</br>
**注释**：

```xml
<summary>Computes the arc-tangent of a value.</summary>
<param name="x">The value, in radians, whose arc-tangent is to be computed.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.AtanPi(double)</br>
**签名**：_fa0c5717daf60a22</br>
**注释**：

```xml
<summary>Computes the arc-tangent of a value and divides the result by pi.</summary>
<param name="x">The value whose arc-tangent is to be computed.</param>
<returns>The arc-tangent of <code data-dev-comment-type="paramref">x</code>, divided by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.Cos(double)</br>
**签名**：_ab249d49b3cb5f87</br>
**注释**：

```xml
<summary>Computes the cosine of a value.</summary>
<param name="x">The value, in radians, whose cosine is to be computed.</param>
<returns>The cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.CosPi(double)</br>
**签名**：_68646d1a3f7e1c4e</br>
**注释**：

```xml
<summary>Computes the cosine of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value, in half-revolutions, whose cosine is to be computed.</param>
<returns>The cosine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.DegreesToRadians(double)</br>
**签名**：_b613a401ab60cfa7</br>
**注释**：

```xml
<summary>Converts a given value from degrees to radians.</summary>
<param name="degrees">The value to convert to radians.</param>
<returns>The value of <code data-dev-comment-type="paramref">degrees</code> converted to radians.</returns>
```

**成员**：static double.RadiansToDegrees(double)</br>
**签名**：_1ed0662536b0a079</br>
**注释**：

```xml
<summary>Converts a given value from radians to degrees.</summary>
<param name="radians">The value to convert to degrees.</param>
<returns>The value of <code data-dev-comment-type="paramref">radians</code> converted to degrees.</returns>
```

**成员**：static double.Sin(double)</br>
**签名**：_82a42c3870a8a263</br>
**注释**：

```xml
<summary>Computes the sine of a value.</summary>
<param name="x">The value, in radians, whose sine is to be computed.</param>
<returns>The sine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.SinCos(double)</br>
**签名**：_bc56189e3e1f8a22</br>
**注释**：

```xml
<summary>Computes the sine and cosine of a value.</summary>
<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.SinCosPi(double)</br>
**签名**：_0f4aeef5d225794d</br>
**注释**：

```xml
<summary>Computes the sine and cosine of a value.</summary>
<param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
<returns>The sine and cosine of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.SinPi(double)</br>
**签名**：_364c4226f027481d</br>
**注释**：

```xml
<summary>Computes the sine of a value that has been multiplied by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its sine.</param>
<returns>The sine of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.Tan(double)</br>
**签名**：_3f5c35650c642d58</br>
**注释**：

```xml
<summary>Computes the tangent of a value.</summary>
<param name="x">The value, in radians, whose tangent is to be computed.</param>
<returns>The tangent of <code data-dev-comment-type="paramref">x</code>.</returns>
```

**成员**：static double.TanPi(double)</br>
**签名**：_c193db8303daa585</br>
**注释**：

```xml
<summary>Computes the tangent of a value that has been multipled by <code data-dev-comment-type="c">pi</code>.</summary>
<param name="x">The value, in half-revolutions, that is multipled by <code data-dev-comment-type="c">pi</code> before computing its tangent.</param>
<returns>The tangent of <code data-dev-comment-type="paramref">x</code> multiplied-by <code data-dev-comment-type="c">pi</code>.</returns>
```

**成员**：static double.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)</br>
**签名**：_95cf4052dcf1d6d8</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static double.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out double)</br>
**签名**：_654e8bbd8869bbea</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static double.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)</br>
**签名**：_cd8bb3b9e099ef63</br>
**注释**：

```xml
<summary>Parses a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
```

**成员**：static double.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out double)</br>
**签名**：_75fcd554c7fa663e</br>
**注释**：

```xml
<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
<param name="utf8Text">The span of UTF-8 characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

