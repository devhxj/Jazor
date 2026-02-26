namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.DateTimeOffset","System/DateTimeOffsetModule.js")]
public static class DateTimeOffsetModule
{
	///<summary>Represents the earliest possible <see cref="T:System.DateTimeOffset" /> value. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly System.DateTimeOffset.MinValue")]
	public extern static Date _77107f0c23675b69();

	///<summary>Represents the greatest possible value of <see cref="T:System.DateTimeOffset" />. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly System.DateTimeOffset.MaxValue")]
	public extern static Date _d45d439f0b97ae0e();

	///<summary>The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970, in the Gregorian calendar. <see cref="F:System.DateTimeOffset.UnixEpoch" /> defines the point in time when Unix time is equal to 0.</summary>
	[Jazor(Op.Discard ,"static readonly System.DateTimeOffset.UnixEpoch")]
	public extern static Date _087cabaedc1b5cc2();

	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset()")]
	public extern static Date _12b4f3f1dc14bea9();

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified number of ticks and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(long, System.TimeSpan)")]
	public extern static Date _1e9c5d2a64e6d41d(BigInt ticks, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(System.DateTime)")]
	public extern static Date _7adf69a53659433a(Date dateTime);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <see cref="T:System.DateTime" /> value and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(System.DateTime, System.TimeSpan)")]
	public extern static Date _106dabc0cc502aa4(Date dateTime, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="date" />, <paramref name="time" />, and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(System.DateOnly, System.TimeOnly, System.TimeSpan)")]
	public extern static Date _8f1aab77eeb6f786(Date date, Number time, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, System.TimeSpan)")]
	public extern static Date _d90dce0e1d2f06e4(Number year, Number month, Number day, Number hour, Number minute, Number second, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.TimeSpan)")]
	public extern static Date _6abaa2b2082f575c(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified year, month, day, hour, minute, second, millisecond, and offset of a specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")]
	public extern static Date _61ea80919619bab9(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.TimeSpan)")]
	public extern static Date _04123d597aa761a3(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, BigInt offset);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTimeOffset" /> structure using the specified <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" />, <paramref name="second" />, <paramref name="millisecond" />, <paramref name="microsecond" /> and <paramref name="offset" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")]
	public extern static Date _d027561c1f6af451(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, BigInt offset);

	[Jazor(Op.Discard ,"static System.DateTimeOffset.UtcNow.get")]
	public extern static Date _7f444d9ce7391e15();

	[Jazor(Op.Discard ,"System.DateTimeOffset.DateTime.get")]
	public extern static Date _2b7dd675863ae961(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.UtcDateTime.get")]
	public extern static Date _703902cecd7f61dd(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.LocalDateTime.get")]
	public extern static Date _ffbfe7b660ff0527(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to the date and time specified by an offset value.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToOffset(System.TimeSpan)")]
	public extern static Date _d1996f02ed3fa243(Date instance, BigInt offset);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Date.get")]
	public extern static Date _d7098a1eabebc945(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Day.get")]
	public extern static Number _ba8df912681fe784(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.DayOfWeek.get")]
	public extern static System.DayOfWeek _17d30a204818ce34(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.DayOfYear.get")]
	public extern static Number _b69ef2b7d0abde1a(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Hour.get")]
	public extern static Number _b7fc65477ef4df45(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Millisecond.get")]
	public extern static Number _0c1b2675cd7a2faa(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Microsecond.get")]
	public extern static Number _ae3a48995f0953ed(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Nanosecond.get")]
	public extern static Number _f9acef215c7d5168(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Minute.get")]
	public extern static Number _0fe8054b55f9f1c7(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Month.get")]
	public extern static Number _79eb4c93cea58d59(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Offset.get")]
	public extern static BigInt _2400298964c553b6(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.TotalOffsetMinutes.get")]
	public extern static Number _cad0683315440ded(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Second.get")]
	public extern static Number _822de224fed5bb6b(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Ticks.get")]
	public extern static BigInt _584068ab15dcf3c9(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.UtcTicks.get")]
	public extern static BigInt _056adc0ac251ebd3(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.TimeOfDay.get")]
	public extern static BigInt _90401f92f6a9141e(Date instance);

	[Jazor(Op.Discard ,"System.DateTimeOffset.Year.get")]
	public extern static Number _127105b7a40a7665(Date instance);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified time interval to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Add(System.TimeSpan)")]
	public extern static Date _09a94b0e7945eda6(Date instance, BigInt timeSpan);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional days to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddDays(double)")]
	public extern static Date _7fd735ce2102a3cc(Date instance, Number days);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional hours to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddHours(double)")]
	public extern static Date _309c83b8a2fbc988(Date instance, Number hours);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of milliseconds to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddMilliseconds(double)")]
	public extern static Date _1528b452af6dd41d(Date instance, Number milliseconds);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of microseconds to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddMicroseconds(double)")]
	public extern static Date _4775ccfee8ed671f(Date instance, Number microseconds);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional minutes to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddMinutes(double)")]
	public extern static Date _97aff1e2f4740394(Date instance, Number minutes);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of months to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddMonths(int)")]
	public extern static Date _db8ffdb562d3ac68(Date instance, Number months);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of whole and fractional seconds to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddSeconds(double)")]
	public extern static Date _54a4d6d554458fdb(Date instance, Number seconds);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of ticks to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddTicks(long)")]
	public extern static Date _804f8bd2dc1e9443(Date instance, BigInt ticks);

	///<summary>Returns a new <see cref="T:System.DateTimeOffset" /> object that adds a specified number of years to the value of this instance.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.AddYears(int)")]
	public extern static Date _f4ea4e123d38eaa5(Date instance, Number years);

	///<summary>Compares two <see cref="T:System.DateTimeOffset" /> objects and indicates whether the first is earlier than the second, equal to the second, or later than the second.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Compare(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static Number _56ac26a94d0f9bca(Date first, Date second);

	///<summary>Compares the current <see cref="T:System.DateTimeOffset" /> object to a specified <see cref="T:System.DateTimeOffset" /> object and indicates whether the current object is earlier than, the same as, or later than the second <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.CompareTo(System.DateTimeOffset)")]
	public extern static Number _255c7bf4a2c3c663(Date instance, Date other);

	///<summary>Determines whether a <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified object.</summary>
	[Jazor(Op.Discard ,"override System.DateTimeOffset.Equals(object)")]
	public extern static bool _fbec90dd4b315acd(Date instance, object? obj);

	///<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same point in time as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Equals(System.DateTimeOffset)")]
	public extern static bool _5a55745cbe84c163(Date instance, Date other);

	///<summary>Determines whether the current <see cref="T:System.DateTimeOffset" /> object represents the same time and has the same offset as a specified <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.EqualsExact(System.DateTimeOffset)")]
	public extern static bool _d4a929178865b462(Date instance, Date other);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Equals(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _817d2f7b0e423bec(Date first, Date second);

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.FromFileTime(long)")]
	public extern static Date _1185de87a3489deb(BigInt fileTime);

	///<summary>Converts a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.FromUnixTimeSeconds(long)")]
	public extern static Date _fb7d72712794a2e4(BigInt seconds);

	///<summary>Converts a Unix time expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z to a <see cref="T:System.DateTimeOffset" /> value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.FromUnixTimeMilliseconds(long)")]
	public extern static Date _89071e7da78164f5(BigInt milliseconds);

	///<summary>Returns the hash code for the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"override System.DateTimeOffset.GetHashCode()")]
	public extern static Number _484d626eb36d071d(Date instance);

	///<summary>Converts the specified string representation of a date, time, and offset to its <see cref="T:System.DateTimeOffset" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(string)")]
	public extern static Date _25187a24d190d864(string input);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(string, System.IFormatProvider)")]
	public extern static Date _fbb732b1255fdd38(string input, Intl.NumberFormat? formatProvider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _277a1a2c7845bcdc(string input, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified culture-specific format information and formatting style.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _948a165174740d96(string input, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider)")]
	public extern static Date _ef9349ca95c1e050(string input, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _6da8f452a2644e91(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts a character span that represents a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _cec804cac90222fc(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _d8c615ebc8c99180(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts a character span that contains the string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static Date _9eaf2ad9372cd2ec(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Subtracts a <see cref="T:System.DateTimeOffset" /> value that represents a specific date and time from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Subtract(System.DateTimeOffset)")]
	public extern static BigInt _f1e08916de33ed2a(Date instance, Date value);

	///<summary>Subtracts a specified time interval from the current <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Subtract(System.TimeSpan)")]
	public extern static Date _2636ae85f21cd963(Date instance, BigInt value);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to a Windows file time.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToFileTime()")]
	public extern static BigInt _d638010bc91ffd47(Date instance);

	///<summary>Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToUnixTimeSeconds()")]
	public extern static BigInt _8bc213443653978d(Date instance);

	///<summary>Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToUnixTimeMilliseconds()")]
	public extern static BigInt _e63166ec11d88ce1(Date instance);

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> object that represents the local time.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToLocalTime()")]
	public extern static Date _c45ea6b7c8ed9501(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override System.DateTimeOffset.ToString()")]
	public extern static string _2aaccc10061a3bb0(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToString(string)")]
	public extern static string _9b46cc87f855c6ba(Date instance, string? format);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToString(System.IFormatProvider)")]
	public extern static string _f0d70d071309b539(Date instance, Intl.NumberFormat? formatProvider);

	///<summary>Converts the value of the current <see cref="T:System.DateTimeOffset" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToString(string, System.IFormatProvider)")]
	public extern static string _e856edbfd7db0646(Date instance, string? format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current datetime offset instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _f899d2eb7dcfcfe9(Date instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _ec001bad537a3ce9(Date instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the current <see cref="T:System.DateTimeOffset" /> object to a <see cref="T:System.DateTimeOffset" /> value that represents the Coordinated Universal Time (UTC).</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.ToUniversalTime()")]
	public extern static Date _cbe0bd9bc2e35d83(Date instance);

	///<summary>Tries to converts a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(string, out System.DateTimeOffset)")]
	public extern static Array<object?> _2fd90dc37b274014(string? input, Date result);

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, out System.DateTimeOffset)")]
	public extern static Array<object?> _c7957aa2e68f8218(string input, Date result);

	///<summary>Tries to convert a specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _62fe5aa144f2c9e1(string? input, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Tries to convert a specified span representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _9dd0fca0c6a9a4de(string input, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _a99669f1d632e166(string? input, string? format, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified format, culture-specific format information, and style. The format of the date and time representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _639a2d041804058b(string input, string format, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _39ec2cd456e46b13(string? input, object formats, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Converts the representation of a date and time in a character span to its <see cref="T:System.DateTimeOffset" /> equivalent using the specified formats, culture-specific format information, and style. The format of the date and time representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTimeOffset)")]
	public extern static Array<object?> _d09753e75937de75(string input, object formats, Intl.NumberFormat? formatProvider, object styles, Date result);

	///<summary>Defines an implicit conversion of a <see cref="T:System.DateTime" /> object to a <see cref="T:System.DateTimeOffset" /> object.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.implicit operator System.DateTimeOffset(System.DateTime)")]
	public extern static Date _31bbd12ed57f4f76();

	///<summary>Adds a specified time interval to a <see cref="T:System.DateTimeOffset" /> object that has a specified date and time, and yields a <see cref="T:System.DateTimeOffset" /> object that has new a date and time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator +(System.DateTimeOffset, System.TimeSpan)")]
	public extern static Date _b8dd85346f7718fe(Date dateTimeOffset, BigInt timeSpan);

	///<summary>Subtracts a specified time interval from a specified date and time, and yields a new date and time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator -(System.DateTimeOffset, System.TimeSpan)")]
	public extern static Date _267065e6d921c80f(Date dateTimeOffset, BigInt timeSpan);

	///<summary>Subtracts one <see cref="T:System.DateTimeOffset" /> object from another and yields a time interval.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator -(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static BigInt _d1af541d3a7181e8(Date left, Date right);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects represent the same point in time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator ==(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _553dcbd8f7ea1a16(Date left, Date right);

	///<summary>Determines whether two specified <see cref="T:System.DateTimeOffset" /> objects refer to different points in time.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator !=(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _9f6eec56175d9528(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator <(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _43aa45c9517f4d47(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is less than a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator <=(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _a6755e7fc2ead5b5(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than (or later than) a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator >(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _84d1b669e69cd9bf(Date left, Date right);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object is greater than or equal to a second specified <xref data-throw-if-not-resolved="true" uid="System.DateTimeOffset"></xref> object.</summary>
	[Jazor(Op.Allowed ,"static System.DateTimeOffset.operator >=(System.DateTimeOffset, System.DateTimeOffset)")]
	public extern static bool _1cb1a326e417bc9b(Date left, Date right);

	///<summary>Deconstructs this <see cref="T:System.DateTimeOffset" /> instance by <see cref="T:System.DateOnly" />, <see cref="T:System.TimeOnly" />, and <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Discard ,"System.DateTimeOffset.Deconstruct(out System.DateOnly, out System.TimeOnly, out System.TimeSpan)")]
	public extern static Array<object?> _6ec7dc3f674ff16c(Date instance, Date date, Number time, BigInt offset);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(string, System.IFormatProvider, out System.DateTimeOffset)")]
	public extern static Array<object?> _61ef673e0dd00ab0(string? s, Intl.NumberFormat? provider, Date result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Date _b0967252268296ed(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static System.DateTimeOffset.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTimeOffset)")]
	public extern static Array<object?> _c9e042e683205a8b(string s, Intl.NumberFormat? provider, Date result);

	[Jazor(Op.Discard ,"static System.DateTimeOffset.Now.get")]
	public extern static Date _e679a7abf50cf648();
}
