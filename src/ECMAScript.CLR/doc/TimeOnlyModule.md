# TimeOnlyModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.TimeOnly.TimeOnly()</br>
**签名**：_9f78f92d0753f4cf</br>

**成员**：static System.TimeOnly.MinValue.get</br>
**签名**：_5a02197e2ef2252f</br>

**成员**：static System.TimeOnly.MaxValue.get</br>
**签名**：_b1d0e19d91dbb54a</br>

**成员**：System.TimeOnly.TimeOnly(int, int)</br>
**签名**：_62d395c56c4c299d</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour and the minute.</summary>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
```

**成员**：System.TimeOnly.TimeOnly(int, int, int)</br>
**签名**：_e9a3481b3456aad4</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, and second.</summary>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
```

**成员**：System.TimeOnly.TimeOnly(int, int, int, int)</br>
**签名**：_335167098e226ccf</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, second, and millisecond.</summary>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The millisecond (0 through 999).</param>
```

**成员**：System.TimeOnly.TimeOnly(int, int, int, int, int)</br>
**签名**：_28c8cb012fe0e547</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure to the specified hour, minute, second, millisecond, and microsecond.</summary>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The millisecond (0 through 999).</param>
<param name="microsecond">The microsecond (0 through 999).</param>
```

**成员**：System.TimeOnly.TimeOnly(long)</br>
**签名**：_b8b3b95e8b848f44</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.TimeOnly" /> structure using a specified number of ticks.</summary>
<param name="ticks">A time of day expressed in the number of 100-nanosecond units since 00:00:00.0000000.</param>
```

**成员**：System.TimeOnly.Hour.get</br>
**签名**：_201ef41481f4e3fb</br>

**成员**：System.TimeOnly.Minute.get</br>
**签名**：_009addd612610031</br>

**成员**：System.TimeOnly.Second.get</br>
**签名**：_b9481eedd6cbeb99</br>

**成员**：System.TimeOnly.Millisecond.get</br>
**签名**：_3c789a48d39d0010</br>

**成员**：System.TimeOnly.Microsecond.get</br>
**签名**：_a091b803b851e27e</br>

**成员**：System.TimeOnly.Nanosecond.get</br>
**签名**：_656df0ee12e92399</br>

**成员**：System.TimeOnly.Ticks.get</br>
**签名**：_2fd46050126234ac</br>

**成员**：System.TimeOnly.Add(System.TimeSpan)</br>
**签名**：_4c935b985e7b6e02</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the value of the specified time span to the value of this instance.</summary>
<param name="value">A positive or negative time interval.</param>
<returns>An object whose value is the sum of the time represented by this instance and the time interval represented by value.</returns>
```

**成员**：System.TimeOnly.Add(System.TimeSpan, out int)</br>
**签名**：_31bb07d031379025</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the value of the specified time span to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
<param name="value">A positive or negative time interval.</param>
<param name="wrappedDays">When this method returns, contains the number of excess days, if any, that resulted from wrapping during this addition operation.</param>
<returns>An object whose value is the sum of the time represented by this instance and the time interval represented by value.</returns>
```

**成员**：System.TimeOnly.AddHours(double)</br>
**签名**：_8e71fa0d2695e84f</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of hours to the value of this instance.</summary>
<param name="value">A number of whole and fractional hours. The value parameter can be negative or positive.</param>
<returns>An object whose value is the sum of the time represented by this instance and the number of hours represented by value.</returns>
```

**成员**：System.TimeOnly.AddHours(double, out int)</br>
**签名**：_ad6cad38823a5ef6</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of hours to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
<param name="value">A number of whole and fractional hours. The value parameter can be negative or positive.</param>
<param name="wrappedDays">When this method returns, contains the number of excess days, if any, that resulted from wrapping during this addition operation.</param>
<returns>An object whose value is the sum of the time represented by this instance and the number of hours represented by value.</returns>
```

**成员**：System.TimeOnly.AddMinutes(double)</br>
**签名**：_77bd7db30cbf3bc9</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of minutes to the value of this instance.</summary>
<param name="value">A number of whole and fractional minutes. The value parameter can be negative or positive.</param>
<returns>An object whose value is the sum of the time represented by this instance and the number of minutes represented by value.</returns>
```

**成员**：System.TimeOnly.AddMinutes(double, out int)</br>
**签名**：_e698cb9920401887</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeOnly" /> that adds the specified number of minutes to the value of this instance.            If the result wraps past the end of the day, this method returns the number of excess days as an out parameter.</summary>
<param name="value">A number of whole and fractional minutes. The value parameter can be negative or positive.</param>
<param name="wrappedDays">When this method returns, contains the number of excess days, if any, that resulted from wrapping during this addition operation.</param>
<returns>An object whose value is the sum of the time represented by this instance and the number of minutes represented by value.</returns>
```

**成员**：System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)</br>
**签名**：_da64e8d379a7e47c</br>
**注释**：

```xml
<summary>Determines if a time falls within the range provided.            Supports both "normal" ranges such as 10:00-12:00, and ranges that span midnight such as 23:00-01:00.</summary>
<param name="start">The starting time of day, inclusive.</param>
<param name="end">The ending time of day, exclusive.</param>
<returns>  <see langword="true" />, if the time falls within the range, <see langword="false" /> otherwise.</returns>
```

**成员**：static System.TimeOnly.operator ==(System.TimeOnly, System.TimeOnly)</br>
**签名**：_8e47d4212be3070c</br>
**注释**：

```xml
<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>are equal.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if left and right represent the same time; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.operator !=(System.TimeOnly, System.TimeOnly)</br>
**签名**：_b3b712e75fff0050</br>
**注释**：

```xml
<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> are not equal.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if left and right do not represent the same time; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.operator >(System.TimeOnly, System.TimeOnly)</br>
**签名**：_341a3f0fbcda5677</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if left is later than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.operator >=(System.TimeOnly, System.TimeOnly)</br>
**签名**：_0656cf79f08fd69b</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if left is the same as or later than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.operator <(System.TimeOnly, System.TimeOnly)</br>
**签名**：_9b001b8f9a72a57d</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if left is earlier than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.operator <=(System.TimeOnly, System.TimeOnly)</br>
**签名**：_cd098f438100d4cb</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if left is the same as or earlier than right; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.operator -(System.TimeOnly, System.TimeOnly)</br>
**签名**：_888a9b439de5e7c1</br>
**注释**：

```xml
<summary>Gives the elapsed time between two points on a circular clock, which will always be a positive value.</summary>
<param name="t1">The first <see cref="T:System.TimeOnly" /> instance.</param>
<param name="t2">The second <see cref="T:System.TimeOnly" /> instance..</param>
<returns>The elapsed time between <paramref name="t1" /> and <paramref name="t2" />.</returns>
```

**成员**：System.TimeOnly.Deconstruct(out int, out int)</br>
**签名**：_d6170153a1f10bc3</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" /> and <see cref="P:System.TimeOnly.Minute" />.</summary>
<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
```

**成员**：System.TimeOnly.Deconstruct(out int, out int, out int)</br>
**签名**：_d36793074735968e</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, and <see cref="P:System.TimeOnly.Second" />.</summary>
<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="second">When this method returns, contains the <see cref="P:System.TimeOnly.Second" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
```

**成员**：System.TimeOnly.Deconstruct(out int, out int, out int, out int)</br>
**签名**：_b349a5fd892d33be</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, and <see cref="P:System.TimeOnly.Millisecond" />.</summary>
<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="second">When this method returns, contains the <see cref="P:System.TimeOnly.Second" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="millisecond">When this method returns, contains the <see cref="P:System.TimeOnly.Millisecond" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
```

**成员**：System.TimeOnly.Deconstruct(out int, out int, out int, out int, out int)</br>
**签名**：_1f5bb15cea73f15b</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, <see cref="P:System.TimeOnly.Millisecond" />, and <see cref="P:System.TimeOnly.Microsecond" />.</summary>
<param name="hour">When this method returns, contains the <see cref="P:System.TimeOnly.Hour" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="minute">When this method returns, contains the <see cref="P:System.TimeOnly.Minute" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="second">When this method returns, contains the <see cref="P:System.TimeOnly.Second" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="millisecond">When this method returns, contains the <see cref="P:System.TimeOnly.Millisecond" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
<param name="microsecond">When this method returns, contains the <see cref="P:System.TimeOnly.Microsecond" /> value for this <see cref="T:System.TimeOnly" /> instance.</param>
```

**成员**：static System.TimeOnly.FromTimeSpan(System.TimeSpan)</br>
**签名**：_df2fe8c100ae98f0</br>
**注释**：

```xml
<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a time span representing the time elapsed since midnight.</summary>
<param name="timeSpan">The time interval measured since midnight. This value has to be positive and not exceeding the time of the day.</param>
<returns>A <see cref="T:System.TimeOnly" /> object representing the time elapsed since midnight using the specified time span value.</returns>
```

**成员**：static System.TimeOnly.FromDateTime(System.DateTime)</br>
**签名**：_a305982aa6859677</br>
**注释**：

```xml
<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a <see cref="T:System.DateTime" /> representing the time of the day in this <see cref="T:System.DateTime" /> object.</summary>
<param name="dateTime">The <see cref="T:System.DateTime" /> object to extract the time of the day from.</param>
<returns>A <see cref="T:System.TimeOnly" /> object representing time of the day specified in the <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：System.TimeOnly.ToTimeSpan()</br>
**签名**：_3ae6313d263b390f</br>
**注释**：

```xml
<summary>Convert the current <see cref="T:System.TimeOnly" /> instance to a <see cref="T:System.TimeSpan" /> object.</summary>
<returns>A <see cref="T:System.TimeSpan" /> object spanning to the time specified in the current <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：System.TimeOnly.CompareTo(System.TimeOnly)</br>
**签名**：_b08fb6c2056f6cd2</br>
**注释**：

```xml
<summary>Compares the value of this instance to a specified <see cref="T:System.TimeOnly" /> value and indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
<param name="value">The object to compare to the current instance.</param>
<returns>A signed number indicating the relative values of this instance and the value parameter.- Less than zero if this instance is earlier than value.- Zero if this instance is the same as value.- Greater than zero if this instance is later than value.</returns>
```

**成员**：System.TimeOnly.CompareTo(object)</br>
**签名**：_fa5c092641b8d1d5</br>
**注释**：

```xml
<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.TimeOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
<param name="value">A boxed object to compare, or <see langword="null" />.</param>
<returns>A signed number indicating the relative values of this instance and the value parameter.            Less than zero if this instance is earlier than value.            Zero if this instance is the same as value.            Greater than zero if this instance is later than value.</returns>
```

**成员**：System.TimeOnly.Equals(System.TimeOnly)</br>
**签名**：_f6e2f8f76d2b030d</br>
**注释**：

```xml
<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.TimeOnly" /> instance.</summary>
<param name="value">The object to compare to this instance.</param>
<returns>  <see langword="true" /> if the value parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.TimeOnly.Equals(object)</br>
**签名**：_f70c423884fcb611</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="value">The object to compare to this instance.</param>
<returns>  <see langword="true" /> if value is an instance of <see cref="T:System.TimeOnly" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.TimeOnly.GetHashCode()</br>
**签名**：_ec44c7db9ffc5397</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_5c89b5211b528926</br>
**注释**：

```xml
<summary>Converts a memory span that contains string representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent by using culture-specific format information and a formatting style.</summary>
<param name="s">The memory span that contains the time to parse.</param>
<param name="provider">The culture-specific format information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <code data-dev-comment-type="paramref">s</code>. A typical value to specify is <xref data-throw-if-not-resolved="true" uid="System.Globalization.DateTimeStyles.None"></xref>.</param>
<returns>A <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_7c5c52c213c7d2e0</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A span containing the time to convert.</param>
<param name="format">The format specifier that defines the required format of <paramref name="s" />.</param>
<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[])</br>
**签名**：_fe05a1ffa3020076</br>
**注释**：

```xml
<summary>Converts the specified span to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_b22aa6d58a65860e</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.Parse(string)</br>
**签名**：_c2335ab7e556bf0b</br>
**注释**：

```xml
<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using the conventions of the current culture.</summary>
<param name="s">The string to parse.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_b10aeed232e37ce3</br>
**注释**：

```xml
<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
<param name="s">The string containing the time to parse.</param>
<param name="provider">The culture-specific format information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of the enumeration values that indicates the style elements that can be present in s for the parse operation to succeed, and that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A<see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(string, string)</br>
**签名**：_716638d6af9e1f50</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A string containing a time to convert.</param>
<param name="format">A format specifier that defines the required format of <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_464a80539f893705</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A string containing the time to convert.</param>
<param name="format">The format specifier that defines the required format of <paramref name="s" />.</param>
<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.TimeOnly" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(string, string[])</br>
**签名**：_732d047579691da6</br>
**注释**：

```xml
<summary>Converts the specified span to a <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_a753be3cfd781575</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A string containing the time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a time.</exception>
<returns>A <see cref="T:System.TimeOnly" /> instance.</returns>
```

**成员**：static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, out System.TimeOnly)</br>
**签名**：_94c68599373e4134</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its TimeOnly equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing the time to convert.</param>
<param name="result">When this method returns, contains the TimeOnly value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or MinValue if the conversion failed. The conversion fails if <paramref name="s" /> is the empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)</br>
**签名**：_33c24989822cc33a</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the characters that represent a time to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <code data-dev-comment-type="paramref">s</code>. A typical value to specify is <xref data-throw-if-not-resolved="true" uid="System.Globalization.DateTimeStyles.None"></xref>.</param>
<param name="result">When this method returns, contains the <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> value equivalent to the time contained in <code data-dev-comment-type="paramref">s</code>, if the conversion succeeded, or System.TimeOnly.MinValue?text=TimeOnly.MinValue if the conversion failed. The conversion fails if <code data-dev-comment-type="paramref">s</code> is an empty string or does not contain a valid string representation of a date. This parameter is passed uninitialized.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if<code data-dev-comment-type="paramref">s</code> was converted successfully; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.TimeOnly)</br>
**签名**：_e2de5093ab6411a5</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the time to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in <paramref name="format" />. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)</br>
**签名**：_533e30052a71b943</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the time to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.TimeOnly)</br>
**签名**：_7949d623f32a801f</br>
**注释**：

```xml
<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The span containing the time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)</br>
**签名**：_c88c8d59055208af</br>
**注释**：

```xml
<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The span containing the time to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed time. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParse(string, out System.TimeOnly)</br>
**签名**：_ee7de3e005ab6751</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the time to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)</br>
**签名**：_c9d76d7d723eb7f2</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the time to convert.</param>
<param name="provider">The culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(string, string, out System.TimeOnly)</br>
**签名**：_635f76a219a898ce</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing the time to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)</br>
**签名**：_5d909e2eac7e90ea</br>
**注释**：

```xml
<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing a time to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a time that corresponds to the pattern specified in format. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(string, string[], out System.TimeOnly)</br>
**签名**：_c464924dd070f03b</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The string containing the time to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.TimeOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)</br>
**签名**：_a8c2964fb6e24ce0</br>
**注释**：

```xml
<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The string containing the time to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.TimeOnly" /> value equivalent to the time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.TimeOnly.MinValue">TimeOnly.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> is an empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：System.TimeOnly.ToLongTimeString()</br>
**签名**：_237d7e75836b3e58</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent long date string representation.</summary>
<returns>The long time string representation of the current instance.</returns>
```

**成员**：System.TimeOnly.ToShortTimeString()</br>
**签名**：_656ad6fcd28355ef</br>
**注释**：

```xml
<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation.</summary>
<returns>The short time string representation of the current instance.</returns>
```

**成员**：override System.TimeOnly.ToString()</br>
**签名**：_95a460669a453469</br>
**注释**：

```xml
<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation using the formatting conventions of the current culture.</summary>
<returns>The short time string representation of the current instance.</returns>
```

**成员**：System.TimeOnly.ToString(string)</br>
**签名**：_b95bf75d8e4cc6af</br>
**注释**：

```xml
<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
<param name="format">A standard or custom time format string.</param>
<returns>A string representation of the current instance with the specified format and the formatting conventions of the current culture.</returns>
```

**成员**：System.TimeOnly.ToString(System.IFormatProvider)</br>
**签名**：_c2fe4568a7f1bbeb</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">The culture-specific formatting information.</param>
<returns>A string representation of the current instance as specified by the provider.</returns>
```

**成员**：System.TimeOnly.ToString(string, System.IFormatProvider)</br>
**签名**：_dd80539f727e11c1</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="format">A standard or custom time format string.</param>
<param name="provider">The culture-specific formatting information.</param>
<returns>A string representation of value of the current instance.</returns>
```

**成员**：System.TimeOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_d3c7ece118e478fa</br>
**注释**：

```xml
<summary>Tries to format the value of the current TimeOnly instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.TimeOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_98dcae3d77df54e1</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.Parse(string, System.IFormatProvider)</br>
**签名**：_ef54bbdfdbe24915</br>
**注释**：

```xml
<summary>Parses a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.TimeOnly.TryParse(string, System.IFormatProvider, out System.TimeOnly)</br>
**签名**：_8fea7e8fcaae2f91</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">A string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> value equivalent to the time contained in <code data-dev-comment-type="paramref">s</code>, if the conversion succeeded, or <xref data-throw-if-not-resolved="true" uid="System.TimeOnly.MinValue"></xref> if the conversion failed. The conversion fails if <code data-dev-comment-type="paramref">s</code> is the empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was parsed successfully; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_ae9862bc80a4bba9</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">A span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeOnly)</br>
**签名**：_1c2553fed0fac496</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">A span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> value equivalent to the time contained in <code data-dev-comment-type="paramref">s</code>, if the conversion succeeded, or <xref data-throw-if-not-resolved="true" uid="System.TimeOnly.MinValue"></xref> if the conversion failed. The conversion fails if <code data-dev-comment-type="paramref">s</code> is the empty string or does not contain a valid string representation of a time. This parameter is passed uninitialized.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

