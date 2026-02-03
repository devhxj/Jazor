# DateOnlyModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.DateOnly.DateOnly()</br>
**签名**：_5f8053a9657a0844</br>

**成员**：static System.DateOnly.MinValue.get</br>
**签名**：_4ab7a6677b34a52b</br>

**成员**：static System.DateOnly.MaxValue.get</br>
**签名**：_d3542025e0317ea5</br>

**成员**：System.DateOnly.DateOnly(int, int, int)</br>
**签名**：_8c5a25d777626c6c</br>
**注释**：

```xml
<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified year, month, and day.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
```

**成员**：System.DateOnly.DateOnly(int, int, int, System.Globalization.Calendar)</br>
**签名**：_c0568bfa1df0ef59</br>
**注释**：

```xml
<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified year, month, and day for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in calendar).</param>
<param name="month">The month (1 through the number of months in calendar).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="calendar">The calendar that is used to interpret year, month, and day.<paramref name="month" />.</param>
```

**成员**：static System.DateOnly.FromDayNumber(int)</br>
**签名**：_96a80b211a70154c</br>
**注释**：

```xml
<summary>Creates a new instance of the <see cref="T:System.DateOnly" /> structure to the specified number of days.</summary>
<param name="dayNumber">The number of days since January 1, 0001 in the Proleptic Gregorian calendar.</param>
<returns>A <see cref="T:System.DateOnly" /> structure instance to the specified number of days.</returns>
```

**成员**：System.DateOnly.Year.get</br>
**签名**：_eeb6f43b5386f459</br>

**成员**：System.DateOnly.Month.get</br>
**签名**：_c189199a72fa745c</br>

**成员**：System.DateOnly.Day.get</br>
**签名**：_fa637ab5d7ac92a4</br>

**成员**：System.DateOnly.DayOfWeek.get</br>
**签名**：_faf7aaba77d4de0c</br>

**成员**：System.DateOnly.DayOfYear.get</br>
**签名**：_6eb4f28206445ae2</br>

**成员**：System.DateOnly.DayNumber.get</br>
**签名**：_04663ba34bb3359d</br>

**成员**：System.DateOnly.AddDays(int)</br>
**签名**：_cb25738994c034e6</br>
**注释**：

```xml
<summary>Adds the specified number of days to the value of this instance.</summary>
<param name="value">The number of days to add. To subtract days, specify a negative number.</param>
<exception cref="T:System.ArgumentOutOfRangeException">          The resulting value is greater than <see cref="F:System.DateOnly.MaxValue">DateOnly.MaxValue</see>.</exception>
<returns>An instance whose value is the sum of the date represented by this instance and the number of days represented by value.</returns>
```

**成员**：System.DateOnly.AddMonths(int)</br>
**签名**：_48134214e63fd9f3</br>
**注释**：

```xml
<summary>Adds the specified number of months to the value of this instance.</summary>
<param name="value">A number of months. The months parameter can be negative or positive.</param>
<returns>An object whose value is the sum of the date represented by this instance and months.</returns>
```

**成员**：System.DateOnly.AddYears(int)</br>
**签名**：_267d01eded65ff1c</br>
**注释**：

```xml
<summary>Adds the specified number of years to the value of this instance.</summary>
<param name="value">A number of years. The value parameter can be negative or positive.</param>
<returns>An object whose value is the sum of the date represented by this instance and the number of years represented by value.</returns>
```

**成员**：static System.DateOnly.operator ==(System.DateOnly, System.DateOnly)</br>
**签名**：_82086262cc7cfc9f</br>
**注释**：

```xml
<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are equal.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if left and right represent the same date; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.operator !=(System.DateOnly, System.DateOnly)</br>
**签名**：_56cd63706d2066a6</br>
**注释**：

```xml
<summary>Determines whether two specified instances of <see cref="T:System.DateOnly" /> are not equal.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if left and right do not represent the same date; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.operator >(System.DateOnly, System.DateOnly)</br>
**签名**：_9b5d78026d232bd9</br>
**注释**：

```xml
<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is later than another specified DateTime.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if left is later than right; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.operator >=(System.DateOnly, System.DateOnly)</br>
**签名**：_0c9d48e09790b085</br>
**注释**：

```xml
<summary>Determines whether one specified DateOnly represents a date that is the same as or later than another specified <see cref="T:System.DateOnly" />.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if left is the same as or later than right; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.operator <(System.DateOnly, System.DateOnly)</br>
**签名**：_5384e5a8b5389bd2</br>
**注释**：

```xml
<summary>Determines whether one specified <see cref="T:System.DateOnly" /> is earlier than another specified <see cref="T:System.DateOnly" />.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if left is earlier than right; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.operator <=(System.DateOnly, System.DateOnly)</br>
**签名**：_ba9123a74024d518</br>
**注释**：

```xml
<summary>Determines whether one specified <see cref="T:System.DateOnly" /> represents a date that is the same as or earlier than another specified <see cref="T:System.DateOnly" />.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if left is the same as or earlier than right; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateOnly.Deconstruct(out int, out int, out int)</br>
**签名**：_87be25300884e7c8</br>
**注释**：

```xml
<summary>Deconstructs <see cref="T:System.DateOnly" /> by <see cref="P:System.DateOnly.Year" />, <see cref="P:System.DateOnly.Month" />, and <see cref="P:System.DateOnly.Day" />.</summary>
<param name="year">When this method returns, represents the <see cref="P:System.DateOnly.Year" /> value of this <see cref="T:System.DateOnly" /> instance.</param>
<param name="month">When this method returns, represents the <see cref="P:System.DateOnly.Month" /> value of this <see cref="T:System.DateOnly" /> instance.</param>
<param name="day">When this method returns, represents the <see cref="P:System.DateOnly.Day" /> value of this <see cref="T:System.DateOnly" /> instance.</param>
```

**成员**：System.DateOnly.ToDateTime(System.TimeOnly)</br>
**签名**：_877770696b013f43</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateTime" /> that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
<param name="time">The time of the day.</param>
<returns>The <see cref="T:System.DateTime" /> instance composed of the date of the current <see cref="T:System.DateOnly" /> instance and the time specified by the input time.</returns>
```

**成员**：System.DateOnly.ToDateTime(System.TimeOnly, System.DateTimeKind)</br>
**签名**：_458cbe4dafb71f56</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateTime" /> instance with the specified input kind that is set to the date of this <see cref="T:System.DateOnly" /> instance and the time of specified input time.</summary>
<param name="time">The time of the day.</param>
<param name="kind">One of the enumeration values that indicates whether ticks specifies a local time, Coordinated Universal Time (UTC), or neither.</param>
<returns>The <see cref="T:System.DateTime" /> instance composed of the date of the current <see cref="T:System.DateOnly" /> instance and the time specified by the input time.</returns>
```

**成员**：static System.DateOnly.FromDateTime(System.DateTime)</br>
**签名**：_8aa4a7a01276329d</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateOnly" /> instance that is set to the date part of the specified <paramref name="dateTime" />.</summary>
<param name="dateTime">The <see cref="T:System.DateTime" /> instance.</param>
<returns>The <see cref="T:System.DateOnly" /> instance composed of the date part of the specified input time <paramref name="dateTime" /> instance.</returns>
```

**成员**：System.DateOnly.CompareTo(System.DateOnly)</br>
**签名**：_e80970d38580b553</br>
**注释**：

```xml
<summary>Compares the value of this instance to a specified <see cref="T:System.DateOnly" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
<param name="value">The object to compare to the current instance.</param>
<returns>Less than zero if this instance is earlier than value. Greater than zero if this instance is later than value. Zero if this instance is the same as value.</returns>
```

**成员**：System.DateOnly.CompareTo(object)</br>
**签名**：_519a37b30f165f47</br>
**注释**：

```xml
<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateOnly" /> value.</summary>
<param name="value">A boxed object to compare, or <see langword="null" />.</param>
<returns>Less than zero if this instance is earlier than value. Greater than zero if this instance is later than value. Zero if this instance is the same as value.</returns>
```

**成员**：System.DateOnly.Equals(System.DateOnly)</br>
**签名**：_3c738069b4f977d8</br>
**注释**：

```xml
<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateOnly" /> instance.</summary>
<param name="value">The object to compare to this instance.</param>
<returns>  <see langword="true" /> if the value parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.DateOnly.Equals(object)</br>
**签名**：_48e30250a65786cc</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="value">The object to compare to this instance.</param>
<returns>  <see langword="true" /> if value is an instance of DateOnly and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.DateOnly.GetHashCode()</br>
**签名**：_6ea6fdcc8ab0282e</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_ec2f441fb253f83c</br>
**注释**：

```xml
<summary>Converts a memory span that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
<param name="s">The memory span that contains the string to parse.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by provider and styles.</returns>
```

**成员**：static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_d26bf763250fffed</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A span containing the characters that represent a date to convert.</param>
<param name="format">A span containing the characters that represent a format specifier that defines the required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>
```

**成员**：static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[])</br>
**签名**：_87edc293654333fc</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the characters that represent a date to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>
```

**成员**：static System.DateOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_6a107ddeb5c38aec</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the characters that represent a date to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>
```

**成员**：static System.DateOnly.Parse(string)</br>
**签名**：_e2640560d207afce</br>
**注释**：

```xml
<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using the conventions of the current culture.</summary>
<param name="s">The string that contains the string to parse.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />.</returns>
```

**成员**：static System.DateOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_60b758dae2c14037</br>
**注释**：

```xml
<summary>Converts a string that contains string representation of a date to its <see cref="T:System.DateOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
<param name="s">The string that contains the string to parse.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" /> for the parse operation to succeed, and that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by provider and styles.</returns>
```

**成员**：static System.DateOnly.ParseExact(string, string)</br>
**签名**：_350d290351e50952</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A string containing the characters that represent a date to convert.</param>
<param name="format">A string that represent a format specifier that defines the required format of <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format.</returns>
```

**成员**：static System.DateOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_f626c308f69f76e8</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A string containing the characters that represent a date to convert.</param>
<param name="format">A string containing the characters that represent a format specifier that defines the required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.DateOnly" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>
```

**成员**：static System.DateOnly.ParseExact(string, string[])</br>
**签名**：_cf94a659a6885bb2</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the characters that represent a date to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>
```

**成员**：static System.DateOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_930ff81377f0d857</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A string containing the characters that represent a date to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date.</exception>
<returns>An object that is equivalent to the date contained in <paramref name="s" />, as specified by format, provider, and style.</returns>
```

**成员**：static System.DateOnly.TryParse(System.ReadOnlySpan<char>, out System.DateOnly)</br>
**签名**：_589f2bd8e9539a93</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing the date to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)</br>
**签名**：_0df2e2de9cba3b73</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the characters that represent a date to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.DateOnly)</br>
**签名**：_73f1ae967191e31e</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing a date to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)</br>
**签名**：_c9bb733ce9acfea6</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" />equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing a date to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.DateOnly)</br>
**签名**：_8f1847f9d4121feb</br>
**注释**：

```xml
<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The span containing the string to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if<paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)</br>
**签名**：_de5feefce32f12d9</br>
**注释**：

```xml
<summary>Converts the specified char span of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The span containing the string to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParse(string, out System.DateOnly)</br>
**签名**：_b14e4d5a572477d0</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the characters representing the date to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)</br>
**签名**：_025d467c3006d36b</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified array of formats, culture-specific format information, and style. And returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the characters that represent a date to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(string, string, out System.DateOnly)</br>
**签名**：_7c0f60b3f5622bbb</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the characters representing a date to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)</br>
**签名**：_19011c99380ebcfa</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date to its <see cref="T:System.DateOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing a date to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a date that correspond to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(string, string[], out System.DateOnly)</br>
**签名**：_c86325a1740751c5</br>
**注释**：

```xml
<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The string containing date to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateOnly)</br>
**签名**：_5326a681dc11fed4</br>
**注释**：

```xml
<summary>Converts the specified string of a date to its <see cref="T:System.DateOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The string containing the date to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateOnly" /> value equivalent to the date contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateOnly.MinValue">DateOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string, or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateOnly.ToLongDateString()</br>
**签名**：_28b00aeb94d7ea8a</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent long date string representation.</summary>
<returns>A string that contains the long date string representation of the current <see cref="T:System.DateOnly" /> object.</returns>
```

**成员**：System.DateOnly.ToShortDateString()</br>
**签名**：_2853e304d94edbd5</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent short date string representation.</summary>
<returns>A string that contains the short date string representation of the current <see cref="T:System.DateOnly" /> object.</returns>
```

**成员**：override System.DateOnly.ToString()</br>
**签名**：_a44c07083341cf3a</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the formatting conventions of the current culture.            The <see cref="T:System.DateOnly" /> object will be formatted in short form.</summary>
<returns>A string that contains the short date string representation of the current <see cref="T:System.DateOnly" /> object.</returns>
```

**成员**：System.DateOnly.ToString(string)</br>
**签名**：_5dd96e58e55f801c</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
<param name="format">A standard or custom date format string.</param>
<returns>A string representation of value of the current <see cref="T:System.DateOnly" /> object as specified by format.</returns>
```

**成员**：System.DateOnly.ToString(System.IFormatProvider)</br>
**签名**：_4a8e04add813d3bc</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>A string representation of value of the current <see cref="T:System.DateOnly" /> object as specified by provider.</returns>
```

**成员**：System.DateOnly.ToString(string, System.IFormatProvider)</br>
**签名**：_6135867fb7290a07</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateOnly" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="format">A standard or custom date format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<returns>A string representation of value of the current <see cref="T:System.DateOnly" /> object as specified by format and provider.</returns>
```

**成员**：System.DateOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_7bef8f375eb344b2</br>
**注释**：

```xml
<summary>Tries to format the value of the current <see cref="T:System.DateOnly" /> instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_435ac9e098a3389c</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateOnly.Parse(string, System.IFormatProvider)</br>
**签名**：_90dcc7a43f944613</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.DateOnly.TryParse(string, System.IFormatProvider, out System.DateOnly)</br>
**签名**：_09af445002e82710</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_18323464e5af4054</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.DateOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateOnly)</br>
**签名**：_e876a9d582a79f6a</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

