# DateTimeOffsetModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：static readonly System.DateTimeOffset.MinValue</br>
**签名**：_77107f0c23675b69</br>
**注释**：

```xml
<summary>Represents the earliest possible <see cref="T:System.DateTimeOffset" /> value. This field is read-only.</summary>
```

**成员**：static readonly System.DateTimeOffset.MaxValue</br>
**签名**：_d45d439f0b97ae0e</br>
**注释**：

```xml
<summary>Represents the greatest possible value of <see cref="T:System.DateTimeOffset" />. This field is read-only.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">  <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see> is outside the range of the current or specified culture's default calendar.</exception>
```

**成员**：static readonly System.DateTimeOffset.UnixEpoch</br>
**签名**：_087cabaedc1b5cc2</br>
**注释**：

```xml
<summary>The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970, in the Gregorian calendar. <see cref="F:System.DateTimeOffset.UnixEpoch" /> defines the point in time when Unix time is equal to 0.</summary>
```

**成员**：System.DateTimeOffset.DateTimeOffset()</br>
**签名**：_12b4f3f1dc14bea9</br>

**成员**：System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)</br>
**签名**：_1e9c5d2a64e6d41d</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified number of ticks and offset.</summary>
<param name="ticks">A date and time expressed as the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight on January 1, 0001.</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="offset" /> is not specified in whole minutes.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The <see cref="P:System.DateTimeOffset.UtcDateTime" /> property is earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> or later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>. -or- <paramref name="ticks" /> is less than <see langword="DateTimeOffset.MinValue.Ticks" /> or greater than <see langword="DateTimeOffset.MaxValue.Ticks" />. -or- <paramref name="offset" /> is less than -14 hours or greater than 14 hours.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(System.DateTime)</br>
**签名**：_7adf69a53659433a</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value.</summary>
<param name="dateTime">A date and time.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The Coordinated Universal Time (UTC) date and time that results from applying the offset is earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The UTC date and time that results from applying the offset is later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(System.DateTime, System.TimeSpan)</br>
**签名**：_106dabc0cc502aa4</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value and offset.</summary>
<param name="dateTime">A date and time.</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="dateTime.Kind" /> equals <see cref="F:System.DateTimeKind.Utc" /> and <paramref name="offset" /> does not equal zero. -or- <paramref name="dateTime.Kind" /> equals <see cref="F:System.DateTimeKind.Local" /> and <paramref name="offset" /> does not equal the offset of the system's local time zone. -or- <paramref name="offset" /> is not specified in whole minutes.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="offset" /> is less than -14 hours or greater than 14 hours. -or- <see cref="P:System.DateTimeOffset.UtcDateTime" /> is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> or greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(System.DateOnly, System.TimeOnly, System.TimeSpan)</br>
**签名**：_8f1aab77eeb6f786</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="date" />, <paramref name="time" />, and <paramref name="offset" />.</summary>
<param name="date">The date part.</param>
<param name="time">The time part.</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
```

**成员**：System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)</br>
**签名**：_d90dce0e1d2f06e4</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, and offset.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="offset" /> does not represent whole minutes.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than one or greater than 9999. -or- <paramref name="month" /> is less than one or greater than 12. -or- <paramref name="day" /> is less than one or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than zero or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="offset" /> is less than -14 hours or greater than 14 hours. -or- The <see cref="P:System.DateTimeOffset.UtcDateTime" /> property is earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> or later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.TimeSpan)</br>
**签名**：_6abaa2b2082f575c</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="offset" /> does not represent whole minutes.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than one or greater than 9999. -or- <paramref name="month" /> is less than one or greater than 12. -or- <paramref name="day" /> is less than one or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than zero or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="millisecond" /> is less than 0 or greater than 999. -or- <paramref name="offset" /> is less than -14 or greater than 14. -or- The <see cref="P:System.DateTimeOffset.UtcDateTime" /> property is earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> or later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)</br>
**签名**：_61ea80919619bab9</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset of a specified calendar.</summary>
<param name="year">The year.</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="offset" /> does not represent whole minutes.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> cannot be <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than the <paramref name="calendar" /> parameter's <see langword="MinSupportedDateTime.Year" /> or greater than <see langword="MaxSupportedDateTime.Year" />. -or- <paramref name="month" /> is either less than or greater than the number of months in <paramref name="year" /> in the <paramref name="calendar" />. -or- <paramref name="day" /> is less than one or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than zero or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="millisecond" /> is less than 0 or greater than 999. -or- <paramref name="offset" /> is less than -14 hours or greater than 14 hours. -or- The <paramref name="year" />, <paramref name="month" />, and <paramref name="day" /> parameters cannot be represented as a date and time value. -or- The <see cref="P:System.DateTimeOffset.UtcDateTime" /> property is earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> or later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.TimeSpan)</br>
**签名**：_04123d597aa761a3</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="microsecond">The microseconds (0 through 999).</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="offset" /> does not represent whole minutes.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999.-or-<paramref name="month" /> is less than 1 or greater than 12.-or-<paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or-<paramref name="hour" /> is less than 0 or greater than 23.-or-<paramref name="minute" /> is less than 0 or greater than 59.-or-<paramref name="second" /> is less than 0 or greater than 59.-or-<paramref name="millisecond" /> is less than 0 or greater than 999.-or-<paramref name="microsecond" /> is less than 0 or greater than 999.</exception>
```

**成员**：System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)</br>
**签名**：_d027561c1f6af451</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="microsecond">The microseconds (0 through 999).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<param name="offset">The time's offset from Coordinated Universal Time (UTC).</param>
<exception cref="T:System.ArgumentException">  <paramref name="offset" /> does not represent whole minutes.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or-<paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or-<paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or-<paramref name="hour" /> is less than 0 or greater than 23.-or-<paramref name="minute" /> is less than 0 or greater than 59.-or-<paramref name="second" /> is less than 0 or greater than 59.-or-<paramref name="millisecond" /> is less than 0 or greater than 999.-or-<paramref name="microsecond" /> is less than 0 or greater than 999.-or-<paramref name="offset" /> is less than -14 hours or greater than 14 hours.-or-The <paramref name="year" />, <paramref name="month" />, and <paramref name="day" /> parameters             cannot be represented as a date and time value.-or-The <see cref="P:System.DateTimeOffset.UtcDateTime" /> property is earlier than <see cref="F:System.DateTimeOffset.MinValue" /> or later than <see cref="F:System.DateTimeOffset.MaxValue" />.</exception>
```

**成员**：static System.DateTimeOffset.UtcNow.get</br>
**签名**：_7f444d9ce7391e15</br>

**成员**：System.DateTimeOffset.DateTime.get</br>
**签名**：_2b7dd675863ae961</br>

**成员**：System.DateTimeOffset.UtcDateTime.get</br>
**签名**：_703902cecd7f61dd</br>

**成员**：System.DateTimeOffset.LocalDateTime.get</br>
**签名**：_ffbfe7b660ff0527</br>

**成员**：System.DateTimeOffset.ToOffset(System.TimeSpan)</br>
**签名**：_d1996f02ed3fa243</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to the date and time specified by an offset value.</summary>
<param name="offset">The offset to convert the <see cref="T:System.DateTimeOffset" /> value to.</param>
<exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTimeOffset" /> object has a <see cref="P:System.DateTimeOffset.DateTime" /> value earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> object has a <see cref="P:System.DateTimeOffset.DateTime" /> value later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="offset" /> is less than -14 hours. -or- <paramref name="offset" /> is greater than 14 hours.</exception>
<returns>An object that is equal to the original <see cref="T:System.DateTimeOffset" /> object (that is, their <see cref="M:System.DateTimeOffset.ToUniversalTime" /> methods return identical points in time) but whose <see cref="P:System.DateTimeOffset.Offset" /> property is set to <paramref name="offset" />.</returns>
```

**成员**：System.DateTimeOffset.Date.get</br>
**签名**：_d7098a1eabebc945</br>

**成员**：System.DateTimeOffset.Day.get</br>
**签名**：_ba8df912681fe784</br>

**成员**：System.DateTimeOffset.DayOfWeek.get</br>
**签名**：_17d30a204818ce34</br>

**成员**：System.DateTimeOffset.DayOfYear.get</br>
**签名**：_b69ef2b7d0abde1a</br>

**成员**：System.DateTimeOffset.Hour.get</br>
**签名**：_b7fc65477ef4df45</br>

**成员**：System.DateTimeOffset.Millisecond.get</br>
**签名**：_0c1b2675cd7a2faa</br>

**成员**：System.DateTimeOffset.Microsecond.get</br>
**签名**：_ae3a48995f0953ed</br>

**成员**：System.DateTimeOffset.Nanosecond.get</br>
**签名**：_f9acef215c7d5168</br>

**成员**：System.DateTimeOffset.Minute.get</br>
**签名**：_0fe8054b55f9f1c7</br>

**成员**：System.DateTimeOffset.Month.get</br>
**签名**：_79eb4c93cea58d59</br>

**成员**：System.DateTimeOffset.Offset.get</br>
**签名**：_2400298964c553b6</br>

**成员**：System.DateTimeOffset.TotalOffsetMinutes.get</br>
**签名**：_cad0683315440ded</br>

**成员**：System.DateTimeOffset.Second.get</br>
**签名**：_822de224fed5bb6b</br>

**成员**：System.DateTimeOffset.Ticks.get</br>
**签名**：_584068ab15dcf3c9</br>

**成员**：System.DateTimeOffset.UtcTicks.get</br>
**签名**：_056adc0ac251ebd3</br>

**成员**：System.DateTimeOffset.TimeOfDay.get</br>
**签名**：_90401f92f6a9141e</br>

**成员**：System.DateTimeOffset.Year.get</br>
**签名**：_127105b7a40a7665</br>

**成员**：System.DateTimeOffset.Add(System.TimeSpan)</br>
**签名**：_09a94b0e7945eda6</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified time interval to the value of this instance.</summary>
<param name="timeSpan">A <see cref="T:System.TimeSpan" /> object that represents a positive or a negative time interval.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the time interval represented by <paramref name="timeSpan" />.</returns>
```

**成员**：System.DateTimeOffset.AddDays(double)</br>
**签名**：_7fd735ce2102a3cc</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional days to the value of this instance.</summary>
<param name="days">A number of whole and fractional days. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of days represented by <paramref name="days" />.</returns>
```

**成员**：System.DateTimeOffset.AddHours(double)</br>
**签名**：_309c83b8a2fbc988</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional hours to the value of this instance.</summary>
<param name="hours">A number of whole and fractional hours. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of hours represented by <paramref name="hours" />.</returns>
```

**成员**：System.DateTimeOffset.AddMilliseconds(double)</br>
**签名**：_1528b452af6dd41d</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of milliseconds to the value of this instance.</summary>
<param name="milliseconds">A number of whole and fractional milliseconds. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of whole milliseconds represented by <paramref name="milliseconds" />.</returns>
```

**成员**：System.DateTimeOffset.AddMicroseconds(double)</br>
**签名**：_4775ccfee8ed671f</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of microseconds to the value of this instance.</summary>
<param name="microseconds">A number of whole and fractional microseconds. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>.-or-The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of whole microseconds represented by <paramref name="microseconds" />.</returns>
```

**成员**：System.DateTimeOffset.AddMinutes(double)</br>
**签名**：_97aff1e2f4740394</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional minutes to the value of this instance.</summary>
<param name="minutes">A number of whole and fractional minutes. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of minutes represented by <paramref name="minutes" />.</returns>
```

**成员**：System.DateTimeOffset.AddMonths(int)</br>
**签名**：_db8ffdb562d3ac68</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of months to the value of this instance.</summary>
<param name="months">A number of whole months. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of months represented by <paramref name="months" />.</returns>
```

**成员**：System.DateTimeOffset.AddSeconds(double)</br>
**签名**：_54a4d6d554458fdb</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional seconds to the value of this instance.</summary>
<param name="seconds">A number of whole and fractional seconds. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of seconds represented by <paramref name="seconds" />.</returns>
```

**成员**：System.DateTimeOffset.AddTicks(long)</br>
**签名**：_804f8bd2dc1e9443</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of ticks to the value of this instance.</summary>
<param name="ticks">A number of 100-nanosecond ticks. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of ticks represented by <paramref name="ticks" />.</returns>
```

**成员**：System.DateTimeOffset.AddYears(int)</br>
**签名**：_f4ea4e123d38eaa5</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of years to the value of this instance.</summary>
<param name="years">A number of years. The number can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object and the number of years represented by <paramref name="years" />.</returns>
```

**成员**：static System.DateTimeOffset.Compare(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_56ac26a94d0f9bca</br>
**注释**：

```xml
<summary>Compares two <see cref="T:System.DateTimeOffset" /> objects and indicates whether the first is earlier than the second, equal to the second, or later than the second.</summary>
<param name="first">The first object to compare.</param>
<param name="second">The second object to compare.</param>
<returns>A signed integer that indicates whether the value of the <paramref name="first" /> parameter is earlier than, later than, or the same time as the value of the <paramref name="second" /> parameter, as the following table shows. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="first" /> is earlier than <paramref name="second" />.</description></item><item><term> Zero</term><description><paramref name="first" /> is equal to <paramref name="second" />.</description></item><item><term> Greater than zero</term><description><paramref name="first" /> is later than <paramref name="second" />.</description></item></list></returns>
```

**成员**：System.DateTimeOffset.CompareTo(System.DateTimeOffset)</br>
**签名**：_255c7bf4a2c3c663</br>
**注释**：

```xml
<summary>Compares the current <see cref="T:System.DateTimeOffset" /> object to a specified <see cref="T:System.DateTimeOffset" /> object and indicates whether the current object is earlier than, the same as, or later than the second <see cref="T:System.DateTimeOffset" /> object.</summary>
<param name="other">An object to compare with the current <see cref="T:System.DateTimeOffset" /> object.</param>
<returns>A signed integer that indicates the relationship between the current <see cref="T:System.DateTimeOffset" /> object and <paramref name="other" />, as the following table shows. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> The current <see cref="T:System.DateTimeOffset" /> object is earlier than <paramref name="other" />.</description></item><item><term> Zero</term><description> The current <see cref="T:System.DateTimeOffset" /> object is the same as <paramref name="other" />.</description></item><item><term> Greater than zero.</term><description> The current <see cref="T:System.DateTimeOffset" /> object is later than <paramref name="other" />.</description></item></list></returns>
```

**成员**：override System.DateTimeOffset.Equals(object)</br>
**签名**：_fbec90dd4b315acd</br>
**注释**：

```xml
<summary>Determines whether a <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified object.</summary>
<param name="obj">The object to compare to the current <see cref="T:System.DateTimeOffset" /> object.</param>
<returns>  <see langword="true" /> if the <paramref name="obj" /> parameter is a <see cref="T:System.DateTimeOffset" /> object and represents the same point in time as the current <see cref="T:System.DateTimeOffset" /> object; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateTimeOffset.Equals(System.DateTimeOffset)</br>
**签名**：_5a55745cbe84c163</br>
**注释**：

```xml
<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
<param name="other">An object to compare to the current <see cref="T:System.DateTimeOffset" /> object.</param>
<returns>  <see langword="true" /> if both <see cref="T:System.DateTimeOffset" /> objects have the same <see cref="P:System.DateTimeOffset.UtcDateTime" /> value; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateTimeOffset.EqualsExact(System.DateTimeOffset)</br>
**签名**：_d4a929178865b462</br>
**注释**：

```xml
<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same time and has the same offset as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
<param name="other">The object to compare to the current <see cref="T:System.DateTimeOffset" /> object.</param>
<returns>  <see langword="true" /> if the current <see cref="T:System.DateTimeOffset" /> object and <paramref name="other" /> have the same date and time value and the same <see cref="P:System.DateTimeOffset.Offset" /> value; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.Equals(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_817d2f7b0e423bec</br>
**注释**：

```xml
<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
<param name="first">The first object to compare.</param>
<param name="second">The second object to compare.</param>
<returns>  <see langword="true" /> if the two <see cref="T:System.DateTimeOffset" /> objects have the same <see cref="P:System.DateTimeOffset.UtcDateTime" /> value; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.FromFileTime(long)</br>
**签名**：_1185de87a3489deb</br>
**注释**：

```xml
<summary>Converts the specified Windows file time to an equivalent local time.</summary>
<param name="fileTime">A Windows file time, expressed in ticks.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="fileTime" /> is less than zero. -or- <paramref name="fileTime" /> is greater than <see langword="DateTimeOffset.MaxValue.Ticks" />.</exception>
<returns>An object that represents the date and time of <paramref name="fileTime" /> with the offset set to the local time offset.</returns>
```

**成员**：static System.DateTimeOffset.FromUnixTimeSeconds(long)</br>
**签名**：_fb7d72712794a2e4</br>
**注释**：

```xml
<summary>Converts a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
<param name="seconds">A Unix time, expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). For Unix times before this date, its value is negative.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="seconds" /> is less than  -62,135,596,800. -or- <paramref name="seconds" /> is greater than 253,402,300,799.</exception>
<returns>A date and time value that represents the same moment in time as the Unix time.</returns>
```

**成员**：static System.DateTimeOffset.FromUnixTimeMilliseconds(long)</br>
**签名**：_89071e7da78164f5</br>
**注释**：

```xml
<summary>Converts a Unix time expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
<param name="milliseconds">A Unix time, expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). For Unix times before this date, its value is negative.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="milliseconds" /> is less than  -62,135,596,800,000. -or- <paramref name="milliseconds" /> is greater than 253,402,300,799,999.</exception>
<returns>A date and time value that represents the same moment in time as the Unix time.</returns>
```

**成员**：override System.DateTimeOffset.GetHashCode()</br>
**签名**：_484d626eb36d071d</br>
**注释**：

```xml
<summary>Returns the hash code for the current <see cref="T:System.DateTimeOffset" /> object.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：static System.DateTimeOffset.Parse(string)</br>
**签名**：_25187a24d190d864</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date, time, and offset to its <see cref="T:System.DateTimeOffset" /> equivalent.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> does not contain a valid string representation of a date and time. -or- <paramref name="input" /> contains the string representation of an offset value without a date or time.</exception>
<returns>An object that is equivalent to the date and time that is contained in <paramref name="input" />.</returns>
```

**成员**：static System.DateTimeOffset.Parse(string, System.IFormatProvider)</br>
**签名**：_fbb732b1255fdd38</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="formatProvider">An object that provides culture-specific format information about <paramref name="input" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> does not contain a valid string representation of a date and time. -or- <paramref name="input" /> contains the string representation of an offset value without a date or time.</exception>
<returns>An object that is equivalent to the date and time that is contained in <paramref name="input" />, as specified by <paramref name="formatProvider" />.</returns>
```

**成员**：static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_277a1a2c7845bcdc</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="formatProvider">An object that provides culture-specific format information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours. -or- <paramref name="styles" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <paramref name="styles" /> includes an unsupported <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <paramref name="styles" /> includes <see cref="T:System.Globalization.DateTimeStyles" /> values that cannot be used together.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> does not contain a valid string representation of a date and time. -or- <paramref name="input" /> contains the string representation of an offset value without a date or time.</exception>
<returns>An object that is equivalent to the date and time that is contained in <paramref name="input" /> as specified by <paramref name="formatProvider" /> and <paramref name="styles" />.</returns>
```

**成员**：static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_948a165174740d96</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
<param name="input">A span containing the characters that represent a date and time to convert.</param>
<param name="formatProvider">An object that provides culture-specific format information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<returns>An object that is equivalent to the date and time that is contained in <paramref name="input" /> as specified by <paramref name="formatProvider" /> and <paramref name="styles" />.</returns>
```

**成员**：static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider)</br>
**签名**：_ef9349ca95c1e050</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="format">A format specifier that defines the expected format of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />. -or- <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is an empty string (""). -or- <paramref name="input" /> does not contain a valid string representation of a date and time. -or- <paramref name="format" /> is an empty string. -or- The hour component and the AM/PM designator in <paramref name="input" /> do not agree.</exception>
<returns>An object that is equivalent to the date and time that is contained in <paramref name="input" /> as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>
```

**成员**：static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_6da8f452a2644e91</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="format">A format specifier that defines the expected format of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours. -or- The <paramref name="styles" /> parameter includes an unsupported value. -or- The <paramref name="styles" /> parameter contains <see cref="T:System.Globalization.DateTimeStyles" /> values that cannot be used together.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />. -or- <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is an empty string (""). -or- <paramref name="input" /> does not contain a valid string representation of a date and time. -or- <paramref name="format" /> is an empty string. -or- The hour component and the AM/PM designator in <paramref name="input" /> do not agree.</exception>
<returns>An object that is equivalent to the date and time that is contained in the <paramref name="input" /> parameter, as specified by the <paramref name="format" />, <paramref name="formatProvider" />, and <paramref name="styles" /> parameters.</returns>
```

**成员**：static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_cec804cac90222fc</br>
**注释**：

```xml
<summary>Converts a character span that represents a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
<param name="input">A character span that represents a date and time.</param>
<param name="format">A character span that contains a format specifier that defines the expected format of <paramref name="input" />.</param>
<param name="formatProvider">An object that provides culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours.-or-The <paramref name="styles" /> parameter includes an unsupported value.-or-The <paramref name="styles" /> parameter contains <see cref="T:System.Globalization.DateTimeStyles" /> values that cannot be used together.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is an empty character span. -or- <paramref name="input" /> does not contain a valid string representation of a date and time. -or- <paramref name="format" /> is an empty character span. -or- The hour component and the AM/PM designator in <paramref name="input" /> do not agree.</exception>
<returns>An object that is equivalent to the date and time that is contained in the <paramref name="input" /> parameter, as specified by the <paramref name="format" />, <paramref name="formatProvider" />, and <paramref name="styles" /> parameters.</returns>
```

**成员**：static System.DateTimeOffset.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_d8c615ebc8c99180</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="formats">An array of format specifiers that define the expected formats of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours. -or- <paramref name="styles" /> includes an unsupported value. -or- The <paramref name="styles" /> parameter contains <see cref="T:System.Globalization.DateTimeStyles" /> values that cannot be used together.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is an empty string (""). -or- <paramref name="input" /> does not contain a valid string representation of a date and time. -or- No element of <paramref name="formats" /> contains a valid format specifier. -or- The hour component and the AM/PM designator in <paramref name="input" /> do not agree.</exception>
<returns>An object that is equivalent to the date and time that is contained in the <paramref name="input" /> parameter, as specified by the <paramref name="formats" />, <paramref name="formatProvider" />, and <paramref name="styles" /> parameters.</returns>
```

**成员**：static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_9eaf2ad9372cd2ec</br>
**注释**：

```xml
<summary>Converts a character span that contains the string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
<param name="input">A character span that contains a date and time to convert.</param>
<param name="formats">An array of format specifiers that define the expected formats of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />.</param>
<exception cref="T:System.ArgumentException">The offset is greater than 14 hours or less than -14 hours.-or-<paramref name="styles" /> includes an unsupported value.-or-The <paramref name="styles" /> parameter contains <see cref="T:System.Globalization.DateTimeStyles" /> values that cannot be used together.</exception>
<exception cref="T:System.FormatException">  <paramref name="input" /> is an empty character span.-or-<paramref name="input" /> does not contain a valid string representation of a date and time.-or-No element of <paramref name="formats" /> contains a valid format specifier.-or-The hour component and the AM/PM designator in <paramref name="input" /> do not agree.</exception>
<returns>An object that is equivalent to the date and time that is contained in the <paramref name="input" /> parameter, as specified by the <paramref name="formats" />, <paramref name="formatProvider" />, and <paramref name="styles" /> parameters.</returns>
```

**成员**：System.DateTimeOffset.Subtract(System.DateTimeOffset)</br>
**签名**：_f1e08916de33ed2a</br>
**注释**：

```xml
<summary>Subtracts a <see cref="T:System.DateTimeOffset" /> value that represents a specific date and time from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
<param name="value">An object that represents the value to subtract.</param>
<returns>An object that specifies the interval between the two <see cref="T:System.DateTimeOffset" /> objects.</returns>
```

**成员**：System.DateTimeOffset.Subtract(System.TimeSpan)</br>
**签名**：_2636ae85f21cd963</br>
**注释**：

```xml
<summary>Subtracts a specified time interval from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
<param name="value">The time interval to subtract.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object that is equal to the date and time represented by the current <see cref="T:System.DateTimeOffset" /> object, minus the time interval represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTimeOffset.ToFileTime()</br>
**签名**：_d638010bc91ffd47</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to a Windows file time.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting file time would represent a date and time before midnight on January 1, 1601 C.E. Coordinated Universal Time (UTC).</exception>
<returns>The value of the current <see cref="T:System.DateTimeOffset" /> object, expressed as a Windows file time.</returns>
```

**成员**：System.DateTimeOffset.ToUnixTimeSeconds()</br>
**签名**：_8bc213443653978d</br>
**注释**：

```xml
<summary>Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.</summary>
<returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
```

**成员**：System.DateTimeOffset.ToUnixTimeMilliseconds()</br>
**签名**：_e63166ec11d88ce1</br>
**注释**：

```xml
<summary>Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
<returns>The number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</returns>
```

**成员**：System.DateTimeOffset.ToLocalTime()</br>
**签名**：_c45ea6b7c8ed9501</br>
**注释**：

```xml
<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> object that represents the local time.</summary>
<returns>An object that represents the date and time of the current <see cref="T:System.DateTimeOffset" /> object converted to local time.</returns>
```

**成员**：override System.DateTimeOffset.ToString()</br>
**签名**：_2aaccc10061a3bb0</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
<returns>A string representation of a <see cref="T:System.DateTimeOffset" /> object that includes the offset appended at the end of the string.</returns>
```

**成员**：System.DateTimeOffset.ToString(string)</br>
**签名**：_9b46cc87f855c6ba</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format.</summary>
<param name="format">A format string.</param>
<exception cref="T:System.FormatException">The length of <paramref name="format" /> is one, and it is not one of the standard format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo" />. -or- <paramref name="format" /> does not contain a valid custom format pattern.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
<returns>A string representation of the value of the current <see cref="T:System.DateTimeOffset" /> object, as specified by <paramref name="format" />.</returns>
```

**成员**：System.DateTimeOffset.ToString(System.IFormatProvider)</br>
**签名**：_f0d70d071309b539</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by <paramref name="formatProvider" />.</exception>
<returns>A string representation of the value of the current <see cref="T:System.DateTimeOffset" /> object, as specified by <paramref name="formatProvider" />.</returns>
```

**成员**：System.DateTimeOffset.ToString(string, System.IFormatProvider)</br>
**签名**：_e856edbfd7db0646</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A format string.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.FormatException">The length of <paramref name="format" /> is one, and it is not one of the standard format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo" />. -or- <paramref name="format" /> does not contain a valid custom format pattern.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by <paramref name="formatProvider" />.</exception>
<returns>A string representation of the value of the current <see cref="T:System.DateTimeOffset" /> object, as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>
```

**成员**：System.DateTimeOffset.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_f899d2eb7dcfcfe9</br>
**注释**：

```xml
<summary>Tries to format the value of the current datetime offset instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the charactes that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="formatProvider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateTimeOffset.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_ec001bad537a3ce9</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="formatProvider" />
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：System.DateTimeOffset.ToUniversalTime()</br>
**签名**：_cbe0bd9bc2e35d83</br>
**注释**：

```xml
<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> value that represents the Coordinated Universal Time (UTC).</summary>
<returns>An object that represents the date and time of the current <see cref="T:System.DateTimeOffset" /> object converted to Coordinated Universal Time (UTC).</returns>
```

**成员**：static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)</br>
**签名**：_2fd90dc37b274014</br>
**注释**：

```xml
<summary>Tries to converts a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>, if the conversion failed. The conversion fails if the <paramref name="input" /> parameter is <see langword="null" /> or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, out System.DateTimeOffset)</br>
**签名**：_c7957aa2e68f8218</br>
**注释**：

```xml
<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="input">A span containing the characters representing the date and time to convert.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>, if the conversion failed. The conversion fails if the <paramref name="input" /> parameter is <see langword="null" /> or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)</br>
**签名**：_62fe5aa144f2c9e1</br>
**注释**：

```xml
<summary>Tries to convert a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="formatProvider">An object that provides culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> value equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>, if the conversion failed. The conversion fails if the <paramref name="input" /> parameter is <see langword="null" /> or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> includes an undefined <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <see cref="F:System.Globalization.DateTimeStyles.NoCurrentDateDefault" /> is not supported. -or- <paramref name="styles" /> includes mutually exclusive <see cref="T:System.Globalization.DateTimeStyles" /> values.</exception>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)</br>
**签名**：_9dd0fca0c6a9a4de</br>
**注释**：

```xml
<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="input">A span containing the characters representing the date and time to convert.</param>
<param name="formatProvider">An object that provides culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> value equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>, if the conversion failed. The conversion fails if the <paramref name="input" /> parameter is <see langword="null" /> or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)</br>
**签名**：_a99669f1d632e166</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="format">A format specifier that defines the required format of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of input. A typical value to specify is <see langword="None" />.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>, if the conversion failed. The conversion fails if the <paramref name="input" /> parameter is <see langword="null" />, or does not contain a valid string representation of a date and time in the expected format defined by <paramref name="format" /> and <c>provider</c>. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> includes an undefined <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <see cref="F:System.Globalization.DateTimeStyles.NoCurrentDateDefault" /> is not supported. -or- <paramref name="styles" /> includes mutually exclusive <see cref="T:System.Globalization.DateTimeStyles" /> values.</exception>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)</br>
**签名**：_639a2d041804058b</br>
**注释**：

```xml
<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
<param name="input">A span containing the characters that represent a date and time to convert.</param>
<param name="format">A format specifier that defines the required format of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> if the conversion failed. The conversion fails if the</param>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> includes an undefined <see cref="T:System.Globalization.DateTimeStyles" /> value.-or-<see cref="F:System.Globalization.DateTimeStyles.NoCurrentDateDefault" /> is not supported.-or-<paramref name="styles" /> includes mutually exclusive <see cref="T:System.Globalization.DateTimeStyles" /> values.</exception>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)</br>
**签名**：_39ec2cd456e46b13</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
<param name="input">A string that contains a date and time to convert.</param>
<param name="formats">An array that defines the expected formats of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of input. A typical value to specify is <see langword="None" />.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>, if the conversion failed. The conversion fails if the <paramref name="input" /> does not contain a valid string representation of a date and time, or does not contain the date and time in the expected format defined by <paramref name="formats" />, or if <paramref name="formats" /> is <see langword="null" />. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> includes an undefined <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <see cref="F:System.Globalization.DateTimeStyles.NoCurrentDateDefault" /> is not supported. -or- <paramref name="styles" /> includes mutually exclusive <see cref="T:System.Globalization.DateTimeStyles" /> values.</exception>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)</br>
**签名**：_d09753e75937de75</br>
**注释**：

```xml
<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
<param name="input">A span containing the characters that represent a date and time to convert.</param>
<param name="formats">A array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
<param name="formatProvider">An object that supplies culture-specific formatting information about <paramref name="input" />.</param>
<param name="styles">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="input" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When the method returns, contains the <see cref="T:System.DateTimeOffset" /> equivalent to the date and time of <paramref name="input" />, if the conversion succeeded, or <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> if the conversion failed. The conversion fails if the</param>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> includes an undefined <see cref="T:System.Globalization.DateTimeStyles" /> value.-or-<see cref="F:System.Globalization.DateTimeStyles.NoCurrentDateDefault" /> is not supported.-or-<paramref name="styles" /> includes mutually exclusive <see cref="T:System.Globalization.DateTimeStyles" /> values.</exception>
<returns>  <see langword="true" /> if the <paramref name="input" /> parameter is successfully converted; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.implicit operator System.DateTimeOffset(System.DateTime)</br>
**签名**：_31bbd12ed57f4f76</br>
**注释**：

```xml
<summary>Defines an implicit conversion of a <see cref="T:System.DateTime" /> object to a <see cref="T:System.DateTimeOffset" /> object.</summary>
<param name="dateTime">The object to convert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The Coordinated Universal Time (UTC) date and time that results from applying the offset is earlier than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The UTC date and time that results from applying the offset is later than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>The converted object.</returns>
```

**成员**：static System.DateTimeOffset.operator +(System.DateTimeOffset, System.TimeSpan)</br>
**签名**：_b8dd85346f7718fe</br>
**注释**：

```xml
<summary>Adds a specified time interval to a <see cref="T:System.DateTimeOffset" /> object that has a specified date and time, and yields a <see cref="T:System.DateTimeOffset" /> object that has new a date and time.</summary>
<param name="dateTimeOffset">The object to add the time interval to.</param>
<param name="timeSpan">The time interval to add.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see>. -or- The resulting <see cref="T:System.DateTimeOffset" /> value is greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the values of <paramref name="dateTimeTz" /> and <paramref name="timeSpan" />.</returns>
```

**成员**：static System.DateTimeOffset.operator -(System.DateTimeOffset, System.TimeSpan)</br>
**签名**：_267065e6d921c80f</br>
**注释**：

```xml
<summary>Subtracts a specified time interval from a specified date and time, and yields a new date and time.</summary>
<param name="dateTimeOffset">The date and time object to subtract from.</param>
<param name="timeSpan">The time interval to subtract.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> value is less than <see cref="F:System.DateTimeOffset.MinValue">DateTimeOffset.MinValue</see> or greater than <see cref="F:System.DateTimeOffset.MaxValue">DateTimeOffset.MaxValue</see>.</exception>
<returns>An object that is equal to the value of <paramref name="dateTimeOffset" /> minus <paramref name="timeSpan" />.</returns>
```

**成员**：static System.DateTimeOffset.operator -(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_d1af541d3a7181e8</br>
**注释**：

```xml
<summary>Subtracts one <see cref="T:System.DateTimeOffset" /> object from another and yields a time interval.</summary>
<param name="left">The minuend.</param>
<param name="right">The subtrahend.</param>
<returns>An object that represents the difference between <paramref name="left" /> and <paramref name="right" />.</returns>
```

**成员**：static System.DateTimeOffset.operator ==(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_553dcbd8f7ea1a16</br>
**注释**：

```xml
<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if both <see cref="T:System.DateTimeOffset" /> objects have the same <see cref="P:System.DateTimeOffset.UtcDateTime" /> value; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.operator !=(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_9f6eec56175d9528</br>
**注释**：

```xml
<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects refer to different points in time.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> do not have the same <see cref="P:System.DateTimeOffset.UtcDateTime" /> value; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTimeOffset.operator <(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_43aa45c9517f4d47</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">left</code> is earlier than the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTimeOffset.operator <=(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_a6755e7fc2ead5b5</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">left</code> is earlier than the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTimeOffset.operator >(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_84d1b669e69cd9bf</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than (or later than) a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">left</code> is later than the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTimeOffset.operator >=(System.DateTimeOffset, System.DateTimeOffset)</br>
**签名**：_1cb1a326e417bc9b</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than or equal to a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
<param name="left">The first object to compare.</param>
<param name="right">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">left</code> is the same as or later than the <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset.UtcDateTime"></xref> value of <code data-dev-comment-type="paramref">right</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：System.DateTimeOffset.Deconstruct(out System.DateOnly, out System.TimeOnly, out System.TimeSpan)</br>
**签名**：_6ec7dc3f674ff16c</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.DateTimeOffset" /> instance by <see cref="T:System.DateOnly" />, <see cref="T:System.TimeOnly" />, and <see cref="T:System.TimeSpan" />.</summary>
<param name="date">When this method returns, represents the <see cref="P:System.DateOnly" /> value of this <see cref="T:System.DateTimeOffset" /> instance.</param>
<param name="time">When this method returns, represents the <see cref="P:System.TimeOnly" /> value of this <see cref="T:System.DateTimeOffset" /> instance.</param>
<param name="offset">When this method returns, represents the <see cref="P:System.DateTimeOffset.Offset" /> value of this <see cref="T:System.DateTimeOffset" /> instance.</param>
```

**成员**：static System.DateTimeOffset.TryParse(string, System.IFormatProvider, out System.DateTimeOffset)</br>
**签名**：_61ef673e0dd00ab0</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_b0967252268296ed</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTimeOffset)</br>
**签名**：_c9e042e683205a8b</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTimeOffset.Now.get</br>
**签名**：_e679a7abf50cf648</br>

