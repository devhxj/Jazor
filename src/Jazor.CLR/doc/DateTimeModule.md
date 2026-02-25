# DateTimeModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：static readonly System.DateTime.MinValue</br>
**签名**：_fad0c74e1c9df5bb</br>
**注释**：

```xml
<summary>Represents the smallest possible value of <see cref="T:System.DateTime" />. This field is read-only.</summary>
```

**成员**：static readonly System.DateTime.MaxValue</br>
**签名**：_eb38dc04224730ea</br>
**注释**：

```xml
<summary>Represents the largest possible value of <see cref="T:System.DateTime" />. This field is read-only.</summary>
```

**成员**：static readonly System.DateTime.UnixEpoch</br>
**签名**：_878591efc9a51388</br>
**注释**：

```xml
<summary>The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970, in the Gregorian calendar. <see cref="F:System.DateTime.UnixEpoch" /> defines the point in time when Unix time is equal to 0.</summary>
```

**成员**：System.DateTime.DateTime()</br>
**签名**：_bfa8ee5dd46e2005</br>

**成员**：System.DateTime.DateTime(long)</br>
**签名**：_1ba9ed95dd0eab48</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks.</summary>
<param name="ticks">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="ticks" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
```

**成员**：System.DateTime.DateTime(long, System.DateTimeKind)</br>
**签名**：_eda1c8bf8e1e617b</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks and to Coordinated Universal Time (UTC) or local time.</summary>
<param name="ticks">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="ticks" /> specifies a local time, Coordinated Universal Time (UTC), or neither.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="ticks" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
```

**成员**：System.DateTime.DateTime(System.DateOnly, System.TimeOnly)</br>
**签名**：_4fef4795bcbef97f</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />. The new instance will have the <see cref="F:System.DateTimeKind.Unspecified" /> kind.</summary>
<param name="date">The date part.</param>
<param name="time">The time part.</param>
```

**成员**：System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)</br>
**签名**：_85602323793168a5</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" /> and respecting the specified <see cref="T:System.DateTimeKind" />.</summary>
<param name="date">The date part.</param>
<param name="time">The time part.</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="date" /> and <paramref name="time" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
```

**成员**：System.DateTime.DateTime(int, int, int)</br>
**签名**：_4cb33a818161a3e1</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)</br>
**签名**：_a515b8bb82ad96b7</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in <paramref name="calendar" />).</param>
<param name="month">The month (1 through the number of months in <paramref name="calendar" />).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />. -or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)</br>
**签名**：_bd2c430e6327a2cc</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in <paramref name="calendar" />).</param>
<param name="month">The month (1 through the number of months in <paramref name="calendar" />).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, and <paramref name="millisecond" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />. -or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="millisecond" /> is less than 0 or greater than 999.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int)</br>
**签名**：_4903723bbf8a0a2f</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)</br>
**签名**：_f83be88cfb3fbce0</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and Coordinated Universal Time (UTC) or local time.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" /> and <paramref name="second" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)</br>
**签名**：_29bb943b21806bd9</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in <paramref name="calendar" />).</param>
<param name="month">The month (1 through the number of months in <paramref name="calendar" />).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />. -or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23 -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int)</br>
**签名**：_5822b271bb635d64</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="millisecond" /> is less than 0 or greater than 999.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)</br>
**签名**：_c52eec5e681a0b8b</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, and <paramref name="millisecond" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999. -or- <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="millisecond" /> is less than 0 or greater than 999.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)</br>
**签名**：_8a4d2d51b716bb36</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in <paramref name="calendar" />).</param>
<param name="month">The month (1 through the number of months in <paramref name="calendar" />).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />. -or- <paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />. -or- <paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />. -or- <paramref name="hour" /> is less than 0 or greater than 23. -or- <paramref name="minute" /> is less than 0 or greater than 59. -or- <paramref name="second" /> is less than 0 or greater than 59. -or- <paramref name="millisecond" /> is less than 0 or greater than 999.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, int)</br>
**签名**：_9117d26d23769ad1</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="microsecond">The microseconds (0 through 999).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999.-or-<paramref name="month" /> is less than 1 or greater than 12.-or-<paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or-<paramref name="hour" /> is less than 0 or greater than 23.-or-<paramref name="minute" /> is less than 0 or greater than 59.-or-<paramref name="second" /> is less than 0 or greater than 59.-or-<paramref name="millisecond" /> is less than 0 or greater than 999.-or-<paramref name="microsecond" /> is less than 0 or greater than 999.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)</br>
**签名**：_e84671346e2b9972</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
<param name="year">The year (1 through 9999).</param>
<param name="month">The month (1 through 12).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="microsecond">The microseconds (0 through 999).</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, and <paramref name="millisecond" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999.-or-<paramref name="month" /> is less than 1 or greater than 12.-or-<paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or-<paramref name="hour" /> is less than 0 or greater than 23.-or-<paramref name="minute" /> is less than 0 or greater than 59.-or-<paramref name="second" /> is less than 0 or greater than 59.-or-<paramref name="millisecond" /> is less than 0 or greater than 999.-or-<paramref name="microsecond" /> is less than 0 or greater than 999.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)</br>
**签名**：_bd13792ce57e1964</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in <paramref name="calendar" />).</param>
<param name="month">The month (1 through the number of months in <paramref name="calendar" />).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="microsecond">The microseconds (0 through 999).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> is <see langword="null" /></exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or-<paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or-<paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or-<paramref name="hour" /> is less than 0 or greater than 23.-or-<paramref name="minute" /> is less than 0 or greater than 59.-or-<paramref name="second" /> is less than 0 or greater than 59.-or-<paramref name="millisecond" /> is less than 0 or greater than 999.-or-<paramref name="microsecond" /> is less than 0 or greater than 999.</exception>
```

**成员**：System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)</br>
**签名**：_cd0b8f2bce1e09ed</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
<param name="year">The year (1 through the number of years in <paramref name="calendar" />).</param>
<param name="month">The month (1 through the number of months in <paramref name="calendar" />).</param>
<param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
<param name="hour">The hours (0 through 23).</param>
<param name="minute">The minutes (0 through 59).</param>
<param name="second">The seconds (0 through 59).</param>
<param name="millisecond">The milliseconds (0 through 999).</param>
<param name="microsecond">The microseconds (0 through 999).</param>
<param name="calendar">The calendar that is used to interpret <paramref name="year" />, <paramref name="month" />, and <paramref name="day" />.</param>
<param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, and <paramref name="millisecond" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="calendar" /> is <see langword="null" /></exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is not in the range supported by <paramref name="calendar" />.-or-<paramref name="month" /> is less than 1 or greater than the number of months in <paramref name="calendar" />.-or-<paramref name="day" /> is less than 1 or greater than the number of days in <paramref name="month" />.-or-<paramref name="hour" /> is less than 0 or greater than 23.-or-<paramref name="minute" /> is less than 0 or greater than 59.-or-<paramref name="second" /> is less than 0 or greater than 59.-or-<paramref name="millisecond" /> is less than 0 or greater than 999.-or-<paramref name="microsecond" /> is less than 0 or greater than 999.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="kind" /> is not one of the <see cref="T:System.DateTimeKind" /> values.</exception>
```

**成员**：System.DateTime.Add(System.TimeSpan)</br>
**签名**：_34a77be7365c459f</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the value of the specified <see cref="T:System.TimeSpan" /> to the value of this instance.</summary>
<param name="value">A positive or negative time interval.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the time interval represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddDays(double)</br>
**签名**：_558a3f189d9149d7</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of days to the value of this instance.</summary>
<param name="value">A number of whole and fractional days. The <paramref name="value" /> parameter can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of days represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddHours(double)</br>
**签名**：_101af978213c19c5</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of hours to the value of this instance.</summary>
<param name="value">A number of whole and fractional hours. The <paramref name="value" /> parameter can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of hours represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddMilliseconds(double)</br>
**签名**：_2b29e4c11fa12daa</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of milliseconds to the value of this instance.</summary>
<param name="value">A number of whole and fractional milliseconds. The <paramref name="value" /> parameter can be negative or positive. Note that this value is rounded to the nearest integer.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of milliseconds represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddMicroseconds(double)</br>
**签名**：_2b47368c73a3e1f2</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of microseconds to the value of this instance.</summary>
<param name="value">A number of whole and fractional microseconds.             The <paramref name="value" /> parameter can be negative or positive.             Note that this value is rounded to the nearest integer.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of microseconds represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddMinutes(double)</br>
**签名**：_8bdc25943cf2d39b</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of minutes to the value of this instance.</summary>
<param name="value">A number of whole and fractional minutes. The <paramref name="value" /> parameter can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of minutes represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddMonths(int)</br>
**签名**：_aae197b95f9024a4</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of months to the value of this instance.</summary>
<param name="months">A number of months. The <paramref name="months" /> parameter can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>. -or- <paramref name="months" /> is less than -120,000 or greater than 120,000.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and <paramref name="months" />.</returns>
```

**成员**：System.DateTime.AddSeconds(double)</br>
**签名**：_57045f93edac1460</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of seconds to the value of this instance.</summary>
<param name="value">A number of whole and fractional seconds. The <paramref name="value" /> parameter can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of seconds represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddTicks(long)</br>
**签名**：_d2e74845b174a889</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of ticks to the value of this instance.</summary>
<param name="value">A number of 100-nanosecond ticks. The <paramref name="value" /> parameter can be positive or negative.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the time represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.AddYears(int)</br>
**签名**：_3353d31b02f2bed8</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of years to the value of this instance.</summary>
<param name="value">A number of years. The <paramref name="value" /> parameter can be negative or positive.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="value" /> or the resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the sum of the date and time represented by this instance and the number of years represented by <paramref name="value" />.</returns>
```

**成员**：static System.DateTime.Compare(System.DateTime, System.DateTime)</br>
**签名**：_0edfd00dcc8d70d0</br>
**注释**：

```xml
<summary>Compares two instances of <see cref="T:System.DateTime" /> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.</summary>
<param name="t1">The first object to compare.</param>
<param name="t2">The second object to compare.</param>
<returns>A signed number indicating the relative values of <paramref name="t1" /> and <paramref name="t2" />. <list type="table"><listheader><term> Value Type</term><description> Condition</description></listheader><item><term> Less than zero</term><description><paramref name="t1" /> is earlier than <paramref name="t2" />.</description></item><item><term> Zero</term><description><paramref name="t1" /> is the same as <paramref name="t2" />.</description></item><item><term> Greater than zero</term><description><paramref name="t1" /> is later than <paramref name="t2" />.</description></item></list></returns>
```

**成员**：System.DateTime.CompareTo(object)</br>
**签名**：_f7b2337bfa9864d9</br>
**注释**：

```xml
<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateTime" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
<param name="value">A boxed object to compare, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.DateTime" />.</exception>
<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is earlier than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is the same as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is later than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：System.DateTime.CompareTo(System.DateTime)</br>
**签名**：_40c6426fdc505e97</br>
**注释**：

```xml
<summary>Compares the value of this instance to a specified <see cref="T:System.DateTime" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
<param name="value">The object to compare to the current instance.</param>
<returns>A signed number indicating the relative values of this instance and the <paramref name="value" /> parameter. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is earlier than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is the same as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is later than <paramref name="value" />.</description></item></list></returns>
```

**成员**：static System.DateTime.DaysInMonth(int, int)</br>
**签名**：_38ef7423971afb7f</br>
**注释**：

```xml
<summary>Returns the number of days in the specified month and year.</summary>
<param name="year">The year.</param>
<param name="month">The month (a number ranging from 1 to 12).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="month" /> is less than 1 or greater than 12. -or- <paramref name="year" /> is less than 1 or greater than 9999.</exception>
<returns>The number of days in <paramref name="month" /> for the specified <paramref name="year" />. For example, if <paramref name="month" /> equals 2 for February, the return value is 28 or 29 depending upon whether <paramref name="year" /> is a leap year.</returns>
```

**成员**：override System.DateTime.Equals(object)</br>
**签名**：_f6903c1af8944917</br>
**注释**：

```xml
<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
<param name="value">The object to compare to this instance.</param>
<returns>  <see langword="true" /> if <paramref name="value" /> is an instance of <see cref="T:System.DateTime" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateTime.Equals(System.DateTime)</br>
**签名**：_c29ca32a998c517c</br>
**注释**：

```xml
<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateTime" /> instance.</summary>
<param name="value">The object to compare to this instance.</param>
<returns>  <see langword="true" /> if the <paramref name="value" /> parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.Equals(System.DateTime, System.DateTime)</br>
**签名**：_4937ff8bec81ddea</br>
**注释**：

```xml
<summary>Returns a value indicating whether two <see cref="T:System.DateTime" /> instances  have the same date and time value.</summary>
<param name="t1">The first object to compare.</param>
<param name="t2">The second object to compare.</param>
<returns>  <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.FromBinary(long)</br>
**签名**：_f437fad61f0046c7</br>
**注释**：

```xml
<summary>Deserializes a 64-bit binary value and recreates an original serialized <see cref="T:System.DateTime" /> object.</summary>
<param name="dateData">A 64-bit signed integer that encodes the <see cref="P:System.DateTime.Kind" /> property in a 2-bit field and the <see cref="P:System.DateTime.Ticks" /> property in a 62-bit field.</param>
<exception cref="T:System.ArgumentException">  <paramref name="dateData" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object that is equivalent to the <see cref="T:System.DateTime" /> object that was serialized by the <see cref="M:System.DateTime.ToBinary" /> method.</returns>
```

**成员**：static System.DateTime.FromFileTime(long)</br>
**签名**：_df025c273bde0e50</br>
**注释**：

```xml
<summary>Converts the specified Windows file time to an equivalent local time.</summary>
<param name="fileTime">A Windows file time expressed in ticks.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="fileTime" /> is less than 0 or represents a time greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object that represents the local time equivalent of the date and time represented by the <paramref name="fileTime" /> parameter.</returns>
```

**成员**：static System.DateTime.FromFileTimeUtc(long)</br>
**签名**：_93886aebedb72920</br>
**注释**：

```xml
<summary>Converts the specified Windows file time to an equivalent UTC time.</summary>
<param name="fileTime">A Windows file time expressed in ticks.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="fileTime" /> is less than 0 or represents a time greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object that represents the UTC time equivalent of the date and time represented by the <paramref name="fileTime" /> parameter.</returns>
```

**成员**：static System.DateTime.FromOADate(double)</br>
**签名**：_12520a637fb85a70</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateTime" /> equivalent to the specified OLE Automation Date.</summary>
<param name="d">An OLE Automation Date value.</param>
<exception cref="T:System.ArgumentException">The date is not a valid OLE Automation Date value.</exception>
<returns>An object that represents the same date and time as <paramref name="d" />.</returns>
```

**成员**：System.DateTime.IsDaylightSavingTime()</br>
**签名**：_d3b1cc7e750c6bc3</br>
**注释**：

```xml
<summary>Indicates whether this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the current time zone.</summary>
<returns>  <see langword="true" /> if the value of the <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Local" /> or <see cref="F:System.DateTimeKind.Unspecified" /> and the value of this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the local time zone; <see langword="false" /> if <see cref="P:System.DateTime.Kind" /> is <see cref="F:System.DateTimeKind.Utc" />.</returns>
```

**成员**：static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)</br>
**签名**：_a99826a92073614e</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.DateTime" /> object that has the same number of ticks as the specified <see cref="T:System.DateTime" />, but is designated as either local time, Coordinated Universal Time (UTC), or neither, as indicated by the specified <see cref="T:System.DateTimeKind" /> value.</summary>
<param name="value">A date and time.</param>
<param name="kind">One of the enumeration values that indicates whether the new object represents local time, UTC, or neither.</param>
<returns>A new object that has the same number of ticks as the object represented by the <paramref name="value" /> parameter and the <see cref="T:System.DateTimeKind" /> value specified by the <paramref name="kind" /> parameter.</returns>
```

**成员**：System.DateTime.ToBinary()</br>
**签名**：_9cea54115c704cf7</br>
**注释**：

```xml
<summary>Serializes the current <see cref="T:System.DateTime" /> object to a 64-bit binary value that subsequently can be used to recreate the <see cref="T:System.DateTime" /> object.</summary>
<returns>A 64-bit signed integer that encodes the <see cref="P:System.DateTime.Kind" /> and <see cref="P:System.DateTime.Ticks" /> properties.</returns>
```

**成员**：System.DateTime.Date.get</br>
**签名**：_d77d20d9d04e2b6b</br>

**成员**：System.DateTime.Day.get</br>
**签名**：_3b9ecf5fd3c301db</br>

**成员**：System.DateTime.DayOfWeek.get</br>
**签名**：_6070f1709c491634</br>

**成员**：System.DateTime.DayOfYear.get</br>
**签名**：_4f6ca20bf1aaa2d3</br>

**成员**：override System.DateTime.GetHashCode()</br>
**签名**：_d3529b55e30e2a12</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：System.DateTime.Hour.get</br>
**签名**：_f263cff61e6628a9</br>

**成员**：System.DateTime.Kind.get</br>
**签名**：_551add245db0b701</br>

**成员**：System.DateTime.Millisecond.get</br>
**签名**：_742a8bcf918b97e6</br>

**成员**：System.DateTime.Microsecond.get</br>
**签名**：_34d05014c270366f</br>

**成员**：System.DateTime.Nanosecond.get</br>
**签名**：_46e11fe2eb2ee869</br>

**成员**：System.DateTime.Minute.get</br>
**签名**：_f4ca5de4f63aa097</br>

**成员**：System.DateTime.Month.get</br>
**签名**：_a8a6b6e36a0ea736</br>

**成员**：static System.DateTime.Now.get</br>
**签名**：_ee9dd166a34a2fa5</br>

**成员**：System.DateTime.Second.get</br>
**签名**：_10a94eacb3b7fd2d</br>

**成员**：System.DateTime.Ticks.get</br>
**签名**：_bcde32e170f49354</br>

**成员**：System.DateTime.TimeOfDay.get</br>
**签名**：_2efdc237be2f31aa</br>

**成员**：static System.DateTime.Today.get</br>
**签名**：_4b250155b7c688bb</br>

**成员**：System.DateTime.Year.get</br>
**签名**：_9d56b09432f81c05</br>

**成员**：static System.DateTime.IsLeapYear(int)</br>
**签名**：_4a9da83e9cb28c1a</br>
**注释**：

```xml
<summary>Returns an indication whether the specified year is a leap year.</summary>
<param name="year">A 4-digit year.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than 1 or greater than 9999.</exception>
<returns>  <see langword="true" /> if <paramref name="year" /> is a leap year; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.Parse(string)</br>
**签名**：_a8a015c2d2bff2f6</br>
**注释**：

```xml
<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using the conventions of the current culture.</summary>
<param name="s">A string that contains a date and time to convert. See The string to parse for more information.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date and time.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />.</returns>
```

**成员**：static System.DateTime.Parse(string, System.IFormatProvider)</br>
**签名**：_e0128ef45cc8584e</br>
**注释**：

```xml
<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information.</summary>
<param name="s">A string that contains a date and time to convert. See The string to parse for more information.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.  See Parsing and cultural conventions</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date and time.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" /> as specified by <paramref name="provider" />.</returns>
```

**成员**：static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_7372e5e0d8ba24a6</br>
**注释**：

```xml
<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
<param name="s">A string that contains a date and time to convert. See The string to parse for more information.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.  See Parsing and cultural conventions</param>
<param name="styles">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" /> for the parse operation to succeed, and that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date and time.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="provider" /> and <paramref name="styles" />.</returns>
```

**成员**：static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_2c85f5b20ae7559e</br>
**注释**：

```xml
<summary>Converts a memory span that contains string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
<param name="s">The memory span that contains the string to parse. See The string to parse for more information.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.  See Parsing and cultural conventions</param>
<param name="styles">A bitwise combination of the enumeration values that indicates the style elements that can be present in <paramref name="s" /> for the parse operation to succeed, and that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.FormatException">  <paramref name="s" /> does not contain a valid string representation of a date and time.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="provider" /> and <paramref name="styles" />.</returns>
```

**成员**：static System.DateTime.ParseExact(string, string, System.IFormatProvider)</br>
**签名**：_7f3dce20074d610f</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
<param name="s">A string that contains a date and time to convert.</param>
<param name="format">A format specifier that defines the required format of <paramref name="s" />. For more information, see the Remarks section.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> or <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> or <paramref name="format" /> is an empty string. -or- <paramref name="s" /> does not contain a date and time that corresponds to the pattern specified in <paramref name="format" />. -or- The hour component and the AM/PM designator in <paramref name="s" /> do not agree.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：static System.DateTime.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_75cd4a49bd890e13</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A string containing a date and time to convert.</param>
<param name="format">A format specifier that defines the required format of <paramref name="s" />. For more information, see the Remarks section.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.DateTime" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> or <paramref name="format" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> or <paramref name="format" /> is an empty string. -or- <paramref name="s" /> does not contain a date and time that corresponds to the pattern specified in <paramref name="format" />. -or- The hour component and the AM/PM designator in <paramref name="s" /> do not agree.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="format" />, <paramref name="provider" />, and <paramref name="style" />.</returns>
```

**成员**：static System.DateTime.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_da7c1ef7b418c87d</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
<param name="s">A span containing the characters that represent a date and time to convert.</param>
<param name="format">A span containing the characters that represent a format specifier that defines the required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of the enumeration values that provides additional information about <paramref name="s" />, about style elements that may be present in <paramref name="s" />, or about the conversion from <paramref name="s" /> to a <see cref="T:System.DateTime" /> value. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="format" />, <paramref name="provider" />, and <paramref name="style" />.</returns>
```

**成员**：static System.DateTime.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_f47f23f5482d6f56</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A string that contains a date and time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />. For more information, see the Remarks section.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> or <paramref name="formats" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">  <paramref name="s" /> is an empty string. -or- an element of <paramref name="formats" /> is an empty string. -or- <paramref name="s" /> does not contain a date and time that corresponds to any element of <paramref name="formats" />. -or- The hour component and the AM/PM designator in <paramref name="s" /> do not agree.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values. For example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />.</exception>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="formats" />, <paramref name="provider" />, and <paramref name="style" />.</returns>
```

**成员**：static System.DateTime.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)</br>
**签名**：_32afd1b56d3b1c77</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
<param name="s">A span containing the characters that represent a date and time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<returns>An object that is equivalent to the date and time contained in <paramref name="s" />, as specified by <paramref name="formats" />, <paramref name="provider" />, and <paramref name="style" />.</returns>
```

**成员**：System.DateTime.Subtract(System.DateTime)</br>
**签名**：_4f5d235cac779f38</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.TimeSpan" /> that subtracts the specified date and time from the value of this instance.</summary>
<param name="value">The date and time value to subtract.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The result is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>A time interval that is equal to the date and time represented by this instance minus the date and time represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.Subtract(System.TimeSpan)</br>
**签名**：_20a406afebff2025</br>
**注释**：

```xml
<summary>Returns a new <see cref="T:System.DateTime" /> that subtracts the specified duration from the value of this instance.</summary>
<param name="value">The time interval to subtract.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The result is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object that is equal to the date and time represented by this instance minus the time interval represented by <paramref name="value" />.</returns>
```

**成员**：System.DateTime.ToOADate()</br>
**签名**：_fb61bb2ccf4b10b6</br>
**注释**：

```xml
<summary>Converts the value of this instance to the equivalent OLE Automation date.</summary>
<exception cref="T:System.OverflowException">The value of this instance cannot be represented as an OLE Automation Date.</exception>
<returns>A double-precision floating-point number that contains an OLE Automation date equivalent to the value of this instance.</returns>
```

**成员**：System.DateTime.ToFileTime()</br>
**签名**：_37ee48ca629793fa</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting file time would represent a date and time before 12:00 midnight January 1, 1601 C.E. UTC.</exception>
<returns>The value of the current <see cref="T:System.DateTime" /> object expressed as a Windows file time.</returns>
```

**成员**：System.DateTime.ToFileTimeUtc()</br>
**签名**：_c02c49ea68661175</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting file time would represent a date and time before 12:00 midnight January 1, 1601 C.E. UTC.</exception>
<returns>The value of the current <see cref="T:System.DateTime" /> object expressed as a Windows file time.</returns>
```

**成员**：System.DateTime.ToLocalTime()</br>
**签名**：_db842725d5fd1ca0</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to local time.</summary>
<returns>An object whose <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Local" />, and whose value is the local time equivalent to the value of the current <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see> if the converted value is too large to be represented by a <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the converted value is too small to be represented as a <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：System.DateTime.ToLongDateString()</br>
**签名**：_6e78dc03eecdd423</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long date string representation.</summary>
<returns>A string that contains the long date string representation of the current <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：System.DateTime.ToLongTimeString()</br>
**签名**：_ab161bb1563732af</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long time string representation.</summary>
<returns>A string that contains the long time string representation of the current <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：System.DateTime.ToShortDateString()</br>
**签名**：_6a67d54f5c865e5e</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short date string representation.</summary>
<returns>A string that contains the short date string representation of the current <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：System.DateTime.ToShortTimeString()</br>
**签名**：_af2d02ec0c0a300d</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short time string representation.</summary>
<returns>A string that contains the short time string representation of the current <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：override System.DateTime.ToString()</br>
**签名**：_6659b3b5d1f081dd</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the formatting conventions of the current culture.</summary>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
<returns>A string representation of the value of the current <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：System.DateTime.ToString(string)</br>
**签名**：_3ee3e9478fe9a1fb</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
<param name="format">A standard or custom date and time format string.</param>
<exception cref="T:System.FormatException">The length of <paramref name="format" /> is 1, and it is not one of the format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo" />. -or- <paramref name="format" /> does not contain a valid custom format pattern.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by the current culture.</exception>
<returns>A string representation of value of the current <see cref="T:System.DateTime" /> object as specified by <paramref name="format" />.</returns>
```

**成员**：System.DateTime.ToString(System.IFormatProvider)</br>
**签名**：_606066f0ee1488c6</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by <paramref name="provider" />.</exception>
<returns>A string representation of value of the current <see cref="T:System.DateTime" /> object as specified by <paramref name="provider" />.</returns>
```

**成员**：System.DateTime.ToString(string, System.IFormatProvider)</br>
**签名**：_85393faf5839b9ef</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
<param name="format">A standard or custom date and time format string.</param>
<param name="provider">An object that supplies culture-specific formatting information.</param>
<exception cref="T:System.FormatException">The length of <paramref name="format" /> is 1, and it is not one of the format specifier characters defined for <see cref="T:System.Globalization.DateTimeFormatInfo" />. -or- <paramref name="format" /> does not contain a valid custom format pattern.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">The date and time is outside the range of dates supported by the calendar used by <paramref name="provider" />.</exception>
<returns>A string representation of value of the current <see cref="T:System.DateTime" /> object as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
```

**成员**：System.DateTime.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_b50913efa5ca8082</br>
**注释**：

```xml
<summary>Tries to format the value of the current datetime instance into the provided span of characters.</summary>
<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：System.DateTime.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_100d184b5413769d</br>
**注释**：

```xml
<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：System.DateTime.ToUniversalTime()</br>
**签名**：_b62871088df3ca8f</br>
**注释**：

```xml
<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to Coordinated Universal Time (UTC).</summary>
<returns>An object whose <see cref="P:System.DateTime.Kind" /> property is <see cref="F:System.DateTimeKind.Utc" />, and whose value is the UTC equivalent to the value of the current <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see> if the converted value is too large to be represented by a <see cref="T:System.DateTime" /> object, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the converted value is too small to be represented by a <see cref="T:System.DateTime" /> object.</returns>
```

**成员**：static System.DateTime.TryParse(string, out System.DateTime)</br>
**签名**：_fa25ca318f086bb6</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing a date and time to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, is an empty string (""), or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)</br>
**签名**：_8658c3be6edb9d2c</br>
**注释**：

```xml
<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing a date and time to convert.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, is an empty string (""), or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)</br>
**签名**：_34043b1eb3a8183a</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing a date and time to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="styles">A bitwise combination of enumeration values that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, is an empty string (""), or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentException">  <paramref name="styles" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <paramref name="styles" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values (for example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />).</exception>
<exception cref="T:System.NotSupportedException">  <paramref name="provider" /> is a neutral culture and cannot be used in a parsing operation.</exception>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)</br>
**签名**：_6e8546b461b48646</br>
**注释**：

```xml
<summary>Converts the span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing the date and time to convert.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="styles">A bitwise combination of enumeration values that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, is an empty string (""), or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)</br>
**签名**：_79e29a1615b41471</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string containing a date and time to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if either the <paramref name="s" /> or <paramref name="format" /> parameter is <see langword="null" />, is an empty string, or does not contain a date and time that correspond to the pattern specified in <paramref name="format" />. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <paramref name="style" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values (for example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />).</exception>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)</br>
**签名**：_d8720f2bb55cf0af</br>
**注释**：

```xml
<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A span containing the characters representing a date and time to convert.</param>
<param name="format">The required format of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of one or more enumeration values that indicate the permitted format of <paramref name="s" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if either the <paramref name="s" /> or <paramref name="format" /> parameter is <see langword="null" />, is an empty string, or does not contain a date and time that correspond to the pattern specified in <paramref name="format" />. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)</br>
**签名**：_d1eb33e53764ee27</br>
**注释**：

```xml
<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">A string that contains a date and time to convert.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific format information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that indicates the permitted format of <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if <paramref name="s" /> or <paramref name="formats" /> is <see langword="null" />, <paramref name="s" /> or an element of <paramref name="formats" /> is an empty string, or the format of <paramref name="s" /> is not exactly as specified by at least one of the format patterns in <paramref name="formats" />. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a valid <see cref="T:System.Globalization.DateTimeStyles" /> value. -or- <paramref name="style" /> contains an invalid combination of <see cref="T:System.Globalization.DateTimeStyles" /> values (for example, both <see cref="F:System.Globalization.DateTimeStyles.AssumeLocal" /> and <see cref="F:System.Globalization.DateTimeStyles.AssumeUniversal" />).</exception>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)</br>
**签名**：_685c4ca5481f00e7</br>
**注释**：

```xml
<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
<param name="s">The span containing the string to parse.</param>
<param name="formats">An array of allowable formats of <paramref name="s" />.</param>
<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />.</param>
<param name="style">A bitwise combination of enumeration values that defines how to interpret the parsed date in relation to the current time zone or the current date. A typical value to specify is <see cref="F:System.Globalization.DateTimeStyles.None" />.</param>
<param name="result">When this method returns, contains the <see cref="T:System.DateTime" /> value equivalent to the date and time contained in <paramref name="s" />, if the conversion succeeded, or <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" />, is <see cref="F:System.String.Empty" />, or does not contain a valid string representation of a date and time. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.operator +(System.DateTime, System.TimeSpan)</br>
**签名**：_d48b23d7c5f7c2aa</br>
**注释**：

```xml
<summary>Adds a specified time interval to a specified date and time, yielding a new date and time.</summary>
<param name="d">The date and time value to add.</param>
<param name="t">The time interval to add.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object that is the sum of the values of <paramref name="d" /> and <paramref name="t" />.</returns>
```

**成员**：static System.DateTime.operator -(System.DateTime, System.TimeSpan)</br>
**签名**：_8d9ea66839ce392a</br>
**注释**：

```xml
<summary>Subtracts a specified time interval from a specified date and time and returns a new date and time.</summary>
<param name="d">The date and time value to subtract from.</param>
<param name="t">The time interval to subtract.</param>
<exception cref="T:System.ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see> or greater than <see cref="F:System.DateTime.MaxValue">DateTime.MaxValue</see>.</exception>
<returns>An object whose value is the value of <paramref name="d" /> minus the value of <paramref name="t" />.</returns>
```

**成员**：static System.DateTime.operator -(System.DateTime, System.DateTime)</br>
**签名**：_85b6d162b092ce0e</br>
**注释**：

```xml
<summary>Subtracts a specified date and time from another specified date and time and returns a time interval.</summary>
<param name="d1">The date and time value to subtract from (the minuend).</param>
<param name="d2">The date and time value to subtract (the subtrahend).</param>
<returns>The time interval between <paramref name="d1" /> and <paramref name="d2" />; that is, <paramref name="d1" /> minus <paramref name="d2" />.</returns>
```

**成员**：static System.DateTime.operator ==(System.DateTime, System.DateTime)</br>
**签名**：_37d87f65292f7083</br>
**注释**：

```xml
<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are equal.</summary>
<param name="d1">The first object to compare.</param>
<param name="d2">The second object to compare.</param>
<returns>  <see langword="true" /> if <paramref name="d1" /> and <paramref name="d2" /> represent the same date and time; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.operator !=(System.DateTime, System.DateTime)</br>
**签名**：_89406f797d33e566</br>
**注释**：

```xml
<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are not equal.</summary>
<param name="d1">The first object to compare.</param>
<param name="d2">The second object to compare.</param>
<returns>  <see langword="true" /> if <paramref name="d1" /> and <paramref name="d2" /> do not represent the same date and time; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.DateTime.operator <(System.DateTime, System.DateTime)</br>
**签名**：_5a97e2aec50193b3</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
<param name="t1">The first object to compare.</param>
<param name="t2">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">t1</code> is earlier than <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTime.operator <=(System.DateTime, System.DateTime)</br>
**签名**：_a8b15168323b118c</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
<param name="t1">The first object to compare.</param>
<param name="t2">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">t1</code> is the same as or earlier than <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTime.operator >(System.DateTime, System.DateTime)</br>
**签名**：_e98b0598f4980bcc</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
<param name="t1">The first object to compare.</param>
<param name="t2">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">t1</code> is later than <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTime.operator >=(System.DateTime, System.DateTime)</br>
**签名**：_91697ebd6031bb97</br>
**注释**：

```xml
<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
<param name="t1">The first object to compare.</param>
<param name="t2">The second object to compare.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">t1</code> is the same as or later than <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)</br>
**签名**：_bcf4183bef96ea21</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.DateTime" /> instance by <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />.</summary>
<param name="date">When this method returns, represents the <see cref="P:System.DateOnly" /> value of this <see cref="T:System.DateTime" /> instance.</param>
<param name="time">When this method returns, represents the <see cref="P:System.TimeOnly" /> value of this <see cref="T:System.DateTime" /> instance.</param>
```

**成员**：System.DateTime.Deconstruct(out int, out int, out int)</br>
**签名**：_5f721827cf6b8105</br>
**注释**：

```xml
<summary>Deconstructs this <see cref="T:System.DateOnly" /> instance by <see cref="P:System.DateTime.Year" />, <see cref="P:System.DateTime.Month" />, and <see cref="P:System.DateTime.Day" />.</summary>
<param name="year">When this method returns, represents the <see cref="P:System.DateTime.Year" /> value of this <see cref="T:System.DateTime" /> instance.</param>
<param name="month">When this method returns, represents the <see cref="P:System.DateTime.Month" /> value of this <see cref="T:System.DateTime" /> instance.</param>
<param name="day">When this method returns, represents the <see cref="P:System.DateTime.Day" /> value of this <see cref="T:System.DateTime" /> instance.</param>
```

**成员**：System.DateTime.GetDateTimeFormats()</br>
**签名**：_8022abe7c2a9b946</br>
**注释**：

```xml
<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers.</summary>
<returns>A string array where each element is the representation of the value of this instance formatted with one of the standard date and time format specifiers.</returns>
```

**成员**：System.DateTime.GetDateTimeFormats(System.IFormatProvider)</br>
**签名**：_65e4d2ad2f14918c</br>
**注释**：

```xml
<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers and the specified culture-specific formatting information.</summary>
<param name="provider">An object that supplies culture-specific formatting information about this instance.</param>
<returns>A string array where each element is the representation of the value of this instance formatted with one of the standard date and time format specifiers.</returns>
```

**成员**：System.DateTime.GetDateTimeFormats(char)</br>
**签名**：_daa9858a8adf981d</br>
**注释**：

```xml
<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier.</summary>
<param name="format">A standard date and time format string.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is not a valid standard date and time format specifier character.</exception>
<returns>A string array where each element is the representation of the value of this instance formatted with the <paramref name="format" /> standard date and time format specifier.</returns>
```

**成员**：System.DateTime.GetDateTimeFormats(char, System.IFormatProvider)</br>
**签名**：_10c081a451aa4b71</br>
**注释**：

```xml
<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier and culture-specific formatting information.</summary>
<param name="format">A date and time format string.</param>
<param name="provider">An object that supplies culture-specific formatting information about this instance.</param>
<exception cref="T:System.FormatException">  <paramref name="format" /> is not a valid standard date and time format specifier character.</exception>
<returns>A string array where each element is the representation of the value of this instance formatted with one of the standard date and time format specifiers.</returns>
```

**成员**：System.DateTime.GetTypeCode()</br>
**签名**：_9164c7979da236d5</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.DateTime" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.DateTime" />.</returns>
```

**成员**：static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)</br>
**签名**：_6c36c46db30aacc1</br>
**注释**：

```xml
<summary>Tries to parse a string into a value.</summary>
<param name="s">The string to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)</br>
**签名**：_41dcf008ea7cf6d9</br>
**注释**：

```xml
<summary>Parses a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
```

**成员**：static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)</br>
**签名**：_63fd53f09ba16132</br>
**注释**：

```xml
<summary>Tries to parse a span of characters into a value.</summary>
<param name="s">The span of characters to parse.</param>
<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
```

**成员**：static System.DateTime.UtcNow.get</br>
**签名**：_d4c39bdf47f391cf</br>

