# GregorianCalendarModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：override System.Globalization.GregorianCalendar.MinSupportedDateTime.get</br>
**签名**：_13ca7ecb3e3aade5</br>

**成员**：override System.Globalization.GregorianCalendar.MaxSupportedDateTime.get</br>
**签名**：_7ba83b2ccdd567b5</br>

**成员**：override System.Globalization.GregorianCalendar.AlgorithmType.get</br>
**签名**：_2c293866a460d9ea</br>

**成员**：System.Globalization.GregorianCalendar.GregorianCalendar()</br>
**签名**：_23b9e8d671b5210e</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the default <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
```

**成员**：System.Globalization.GregorianCalendar.GregorianCalendar(System.Globalization.GregorianCalendarTypes)</br>
**签名**：_c043a86ee7a70c81</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Globalization.GregorianCalendar" /> class using the specified <see cref="T:System.Globalization.GregorianCalendarTypes" /> value.</summary>
<param name="type">The <see cref="T:System.Globalization.GregorianCalendarTypes" /> value that denotes which language version of the calendar to create.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="type" /> is not a member of the <see cref="T:System.Globalization.GregorianCalendarTypes" /> enumeration.</exception>
```

**成员**：virtual System.Globalization.GregorianCalendar.CalendarType.get</br>
**签名**：_33a82cf70a73ecdd</br>

**成员**：virtual System.Globalization.GregorianCalendar.CalendarType.set</br>
**签名**：_ab29134350e86147</br>

**成员**：override System.Globalization.GregorianCalendar.AddMonths(System.DateTime, int)</br>
**签名**：_1c4bd410ce12db05</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of months away from the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to which to add months.</param>
<param name="months">The number of months to add.</param>
<exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="months" /> is less than -120000.     -or-     <paramref name="months" /> is greater than 120000.</exception>
<returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of months to the specified <see cref="T:System.DateTime" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.AddYears(System.DateTime, int)</br>
**签名**：_705c207141cada42</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateTime" /> that is the specified number of years away from the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to which to add years.</param>
<param name="years">The number of years to add.</param>
<exception cref="T:System.ArgumentException">The resulting <see cref="T:System.DateTime" /> is outside the supported range.</exception>
<returns>The <see cref="T:System.DateTime" /> that results from adding the specified number of years to the specified <see cref="T:System.DateTime" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetDayOfMonth(System.DateTime)</br>
**签名**：_5f5d0a874674bdea</br>
**注释**：

```xml
<summary>Returns the day of the month in the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
<returns>An integer from 1 to 31 that represents the day of the month in <paramref name="time" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetDayOfWeek(System.DateTime)</br>
**签名**：_6cdddcc68587ea95</br>
**注释**：

```xml
<summary>Returns the day of the week in the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
<returns>A <see cref="T:System.DayOfWeek" /> value that represents the day of the week in <paramref name="time" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetDayOfYear(System.DateTime)</br>
**签名**：_81e475ed63f62602</br>
**注释**：

```xml
<summary>Returns the day of the year in the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
<returns>An integer from 1 to 366 that represents the day of the year in <paramref name="time" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetDaysInMonth(int, int, int)</br>
**签名**：_ce58c7d4d1c36fe3</br>
**注释**：

```xml
<summary>Returns the number of days in the specified month in the specified year in the specified era.</summary>
<param name="year">An integer that represents the year.</param>
<param name="month">An integer from 1 to 12 that represents the month.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.</exception>
<returns>The number of days in the specified month in the specified year in the specified era.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetDaysInYear(int, int)</br>
**签名**：_7545c4d66f0f3604</br>
**注释**：

```xml
<summary>Returns the number of days in the specified year in the specified era.</summary>
<param name="year">An integer that represents the year.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.</exception>
<returns>The number of days in the specified year in the specified era.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetEra(System.DateTime)</br>
**签名**：_21a6ebc60ed3b388</br>
**注释**：

```xml
<summary>Returns the era in the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
<returns>An integer that represents the era in <paramref name="time" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.Eras.get</br>
**签名**：_c01c2927eaf2fefe</br>

**成员**：override System.Globalization.GregorianCalendar.GetMonth(System.DateTime)</br>
**签名**：_ce76f400b1aa26d3</br>
**注释**：

```xml
<summary>Returns the month in the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
<returns>An integer from 1 to 12 that represents the month in <paramref name="time" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetMonthsInYear(int, int)</br>
**签名**：_5df8d3230f9681b9</br>
**注释**：

```xml
<summary>Returns the number of months in the specified year in the specified era.</summary>
<param name="year">An integer that represents the year.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.</exception>
<returns>The number of months in the specified year in the specified era.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetYear(System.DateTime)</br>
**签名**：_fd5a2cde6fb4d6f5</br>
**注释**：

```xml
<summary>Returns the year in the specified <see cref="T:System.DateTime" />.</summary>
<param name="time">The <see cref="T:System.DateTime" /> to read.</param>
<returns>An integer that represents the year in <paramref name="time" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.IsLeapDay(int, int, int, int)</br>
**签名**：_10c29328b0ef4014</br>
**注释**：

```xml
<summary>Determines whether the specified date in the specified era is a leap day.</summary>
<param name="year">An integer that represents the year.</param>
<param name="month">An integer from 1 to 12 that represents the month.</param>
<param name="day">An integer from 1 to 31 that represents the day.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.     -or-     <paramref name="day" /> is outside the range supported by the calendar.</exception>
<returns>  <see langword="true" /> if the specified day is a leap day; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.GetLeapMonth(int, int)</br>
**签名**：_91a08597c1c93445</br>
**注释**：

```xml
<summary>Calculates the leap month for a specified year and era.</summary>
<param name="year">A year.</param>
<param name="era">An era. Specify either <see cref="F:System.Globalization.GregorianCalendar.ADEra" /> or <see langword="GregorianCalendar.Eras[Calendar.CurrentEra]" />.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is less than the Gregorian calendar year 1 or greater than the Gregorian calendar year 9999.     -or-     <paramref name="era" /> is not <see cref="F:System.Globalization.GregorianCalendar.ADEra" /> or <see langword="GregorianCalendar.Eras[Calendar.CurrentEra]" />.</exception>
<returns>Always 0 because the Gregorian calendar does not recognize leap months.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.IsLeapMonth(int, int, int)</br>
**签名**：_9917941c9da950b5</br>
**注释**：

```xml
<summary>Determines whether the specified month in the specified year in the specified era is a leap month.</summary>
<param name="year">An integer that represents the year.</param>
<param name="month">An integer from 1 to 12 that represents the month.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.</exception>
<returns>This method always returns <see langword="false" />, unless overridden by a derived class.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.IsLeapYear(int, int)</br>
**签名**：_4c3723e9b82aa507</br>
**注释**：

```xml
<summary>Determines whether the specified year in the specified era is a leap year.</summary>
<param name="year">An integer that represents the year.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.</exception>
<returns>  <see langword="true" /> if the specified year is a leap year; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.ToDateTime(int, int, int, int, int, int, int, int)</br>
**签名**：_29ccd13d5e5508f8</br>
**注释**：

```xml
<summary>Returns a <see cref="T:System.DateTime" /> that is set to the specified date and time in the specified era.</summary>
<param name="year">An integer that represents the year.</param>
<param name="month">An integer from 1 to 12 that represents the month.</param>
<param name="day">An integer from 1 to 31 that represents the day.</param>
<param name="hour">An integer from 0 to 23 that represents the hour.</param>
<param name="minute">An integer from 0 to 59 that represents the minute.</param>
<param name="second">An integer from 0 to 59 that represents the second.</param>
<param name="millisecond">An integer from 0 to 999 that represents the millisecond.</param>
<param name="era">An integer that represents the era.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="era" /> is outside the range supported by the calendar.     -or-     <paramref name="year" /> is outside the range supported by the calendar.     -or-     <paramref name="month" /> is outside the range supported by the calendar.     -or-     <paramref name="day" /> is outside the range supported by the calendar.     -or-     <paramref name="hour" /> is less than zero or greater than 23.     -or-     <paramref name="minute" /> is less than zero or greater than 59.     -or-     <paramref name="second" /> is less than zero or greater than 59.     -or-     <paramref name="millisecond" /> is less than zero or greater than 999.</exception>
<returns>The <see cref="T:System.DateTime" /> that is set to the specified date and time in the current era.</returns>
```

**成员**：override System.Globalization.GregorianCalendar.TwoDigitYearMax.get</br>
**签名**：_e32c11e11fbe2e3b</br>

**成员**：override System.Globalization.GregorianCalendar.TwoDigitYearMax.set</br>
**签名**：_9537b0490ec80689</br>

**成员**：override System.Globalization.GregorianCalendar.ToFourDigitYear(int)</br>
**签名**：_cca1b99b56b6a322</br>
**注释**：

```xml
<summary>Converts the specified year to a four-digit year by using the <see cref="P:System.Globalization.GregorianCalendar.TwoDigitYearMax" /> property to determine the appropriate century.</summary>
<param name="year">A two-digit or four-digit integer that represents the year to convert.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="year" /> is outside the range supported by the calendar.</exception>
<returns>An integer that contains the four-digit representation of <paramref name="year" />.</returns>
```

