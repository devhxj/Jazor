using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.DateTime", WhiteListOp.Allowed, null,"System/DateTimeModule.js")]
public static class DateTimeModule
{
	///<summary>Represents the smallest possible value of <see cref="T:System.DateTime" />. This field is read-only.</summary>
	[WhiteList("static readonly System.DateTime.MinValue", WhiteListOp.Discard)]
	public extern static Date _fad0c74e1c9df5bb();

	///<summary>Represents the largest possible value of <see cref="T:System.DateTime" />. This field is read-only.</summary>
	[WhiteList("static readonly System.DateTime.MaxValue", WhiteListOp.Discard)]
	public extern static Date _eb38dc04224730ea();

	///<summary>The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970, in the Gregorian calendar. <see cref="F:System.DateTime.UnixEpoch" /> defines the point in time when Unix time is equal to 0.</summary>
	[WhiteList("static readonly System.DateTime.UnixEpoch", WhiteListOp.Discard)]
	public extern static Date _878591efc9a51388();

	[WhiteList("System.DateTime.DateTime()", WhiteListOp.Discard)]
	public extern static Date _bfa8ee5dd46e2005();

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks.</summary>
	[WhiteList("System.DateTime.DateTime(long)", WhiteListOp.Discard)]
	public extern static Date _1ba9ed95dd0eab48(BigInt ticks);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks and to Coordinated Universal Time (UTC) or local time.</summary>
	[WhiteList("System.DateTime.DateTime(long, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _eda1c8bf8e1e617b(BigInt ticks, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />. The new instance will have the <see cref="F:System.DateTimeKind.Unspecified" /> kind.</summary>
	[WhiteList("System.DateTime.DateTime(System.DateOnly, System.TimeOnly)", WhiteListOp.Discard)]
	public extern static Date _4fef4795bcbef97f(Date date, Number time);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" /> and respecting the specified <see cref="T:System.DateTimeKind" />.</summary>
	[WhiteList("System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _85602323793168a5(Date date, Number time, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int)", WhiteListOp.Discard)]
	public extern static Date _4cb33a818161a3e1(Number year, Number month, Number day);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)", WhiteListOp.Discard)]
	public extern static Date _a515b8bb82ad96b7(Number year, Number month, Number day, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _bd2c430e6327a2cc(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int)", WhiteListOp.Discard)]
	public extern static Date _4903723bbf8a0a2f(Number year, Number month, Number day, Number hour, Number minute, Number second);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and Coordinated Universal Time (UTC) or local time.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _f83be88cfb3fbce0(Number year, Number month, Number day, Number hour, Number minute, Number second, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)", WhiteListOp.Discard)]
	public extern static Date _29bb943b21806bd9(Number year, Number month, Number day, Number hour, Number minute, Number second, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int)", WhiteListOp.Discard)]
	public extern static Date _5822b271bb635d64(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _c52eec5e681a0b8b(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)", WhiteListOp.Discard)]
	public extern static Date _8a4d2d51b716bb36(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, int)", WhiteListOp.Discard)]
	public extern static Date _9117d26d23769ad1(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _e84671346e2b9972(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)", WhiteListOp.Discard)]
	public extern static Date _bd13792ce57e1964(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[WhiteList("System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _cd0b8f2bce1e09ed(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, object kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the value of the specified <see cref="T:System.TimeSpan" /> to the value of this instance.</summary>
	[WhiteList("System.DateTime.Add(System.TimeSpan)", WhiteListOp.Discard)]
	public extern static Date _34a77be7365c459f(Date instance, BigInt value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of days to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddDays(double)", WhiteListOp.Discard)]
	public extern static Date _558a3f189d9149d7(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of hours to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddHours(double)", WhiteListOp.Discard)]
	public extern static Date _101af978213c19c5(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of milliseconds to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddMilliseconds(double)", WhiteListOp.Discard)]
	public extern static Date _2b29e4c11fa12daa(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of microseconds to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddMicroseconds(double)", WhiteListOp.Discard)]
	public extern static Date _2b47368c73a3e1f2(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of minutes to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddMinutes(double)", WhiteListOp.Discard)]
	public extern static Date _8bdc25943cf2d39b(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of months to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddMonths(int)", WhiteListOp.Discard)]
	public extern static Date _aae197b95f9024a4(Date instance, Number months);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of seconds to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddSeconds(double)", WhiteListOp.Discard)]
	public extern static Date _57045f93edac1460(Date instance, Number value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of ticks to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddTicks(long)", WhiteListOp.Discard)]
	public extern static Date _d2e74845b174a889(Date instance, BigInt value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of years to the value of this instance.</summary>
	[WhiteList("System.DateTime.AddYears(int)", WhiteListOp.Discard)]
	public extern static Date _3353d31b02f2bed8(Date instance, Number value);

	///<summary>Compares two instances of <see cref="T:System.DateTime" /> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.</summary>
	[WhiteList("static System.DateTime.Compare(System.DateTime, System.DateTime)", WhiteListOp.Discard)]
	public extern static Number _0edfd00dcc8d70d0(Date t1, Date t2);

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateTime" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[WhiteList("System.DateTime.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _f7b2337bfa9864d9(Date instance, Object? value);

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateTime" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[WhiteList("System.DateTime.CompareTo(System.DateTime)", WhiteListOp.Discard)]
	public extern static Number _40c6426fdc505e97(Date instance, Date value);

	///<summary>Returns the number of days in the specified month and year.</summary>
	[WhiteList("static System.DateTime.DaysInMonth(int, int)", WhiteListOp.Discard)]
	public extern static Number _38ef7423971afb7f(Number year, Number month);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[WhiteList("override System.DateTime.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _f6903c1af8944917(Date instance, Object? value);

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateTime" /> instance.</summary>
	[WhiteList("System.DateTime.Equals(System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _c29ca32a998c517c(Date instance, Date value);

	///<summary>Returns a value indicating whether two <see cref="T:System.DateTime" /> instances  have the same date and time value.</summary>
	[WhiteList("static System.DateTime.Equals(System.DateTime, System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _4937ff8bec81ddea(Date t1, Date t2);

	///<summary>Deserializes a 64-bit binary value and recreates an original serialized <see cref="T:System.DateTime" /> object.</summary>
	[WhiteList("static System.DateTime.FromBinary(long)", WhiteListOp.Discard)]
	public extern static Date _f437fad61f0046c7(BigInt dateData);

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[WhiteList("static System.DateTime.FromFileTime(long)", WhiteListOp.Discard)]
	public extern static Date _df025c273bde0e50(BigInt fileTime);

	///<summary>Converts the specified Windows file time to an equivalent UTC time.</summary>
	[WhiteList("static System.DateTime.FromFileTimeUtc(long)", WhiteListOp.Discard)]
	public extern static Date _93886aebedb72920(BigInt fileTime);

	///<summary>Returns a <see cref="T:System.DateTime" /> equivalent to the specified OLE Automation Date.</summary>
	[WhiteList("static System.DateTime.FromOADate(double)", WhiteListOp.Discard)]
	public extern static Date _12520a637fb85a70(Number d);

	///<summary>Indicates whether this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the current time zone.</summary>
	[WhiteList("System.DateTime.IsDaylightSavingTime()", WhiteListOp.Discard)]
	public extern static bool _d3b1cc7e750c6bc3(Date instance);

	///<summary>Creates a new <see cref="T:System.DateTime" /> object that has the same number of ticks as the specified <see cref="T:System.DateTime" />, but is designated as either local time, Coordinated Universal Time (UTC), or neither, as indicated by the specified <see cref="T:System.DateTimeKind" /> value.</summary>
	[WhiteList("static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)", WhiteListOp.Discard)]
	public extern static Date _a99826a92073614e(Date value, object kind);

	///<summary>Serializes the current <see cref="T:System.DateTime" /> object to a 64-bit binary value that subsequently can be used to recreate the <see cref="T:System.DateTime" /> object.</summary>
	[WhiteList("System.DateTime.ToBinary()", WhiteListOp.Discard)]
	public extern static BigInt _9cea54115c704cf7(Date instance);

	[WhiteList("System.DateTime.Date.get", WhiteListOp.Discard)]
	public extern static Date _d77d20d9d04e2b6b(Date instance);

	[WhiteList("System.DateTime.Day.get", WhiteListOp.Discard)]
	public extern static Number _3b9ecf5fd3c301db(Date instance);

	[WhiteList("System.DateTime.DayOfWeek.get", WhiteListOp.Discard)]
	public extern static System.DayOfWeek _6070f1709c491634(Date instance);

	[WhiteList("System.DateTime.DayOfYear.get", WhiteListOp.Discard)]
	public extern static Number _4f6ca20bf1aaa2d3(Date instance);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override System.DateTime.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _d3529b55e30e2a12(Date instance);

	[WhiteList("System.DateTime.Hour.get", WhiteListOp.Discard)]
	public extern static Number _f263cff61e6628a9(Date instance);

	[WhiteList("System.DateTime.Kind.get", WhiteListOp.Discard)]
	public extern static System.DateTimeKind _551add245db0b701(Date instance);

	[WhiteList("System.DateTime.Millisecond.get", WhiteListOp.Discard)]
	public extern static Number _742a8bcf918b97e6(Date instance);

	[WhiteList("System.DateTime.Microsecond.get", WhiteListOp.Discard)]
	public extern static Number _34d05014c270366f(Date instance);

	[WhiteList("System.DateTime.Nanosecond.get", WhiteListOp.Discard)]
	public extern static Number _46e11fe2eb2ee869(Date instance);

	[WhiteList("System.DateTime.Minute.get", WhiteListOp.Discard)]
	public extern static Number _f4ca5de4f63aa097(Date instance);

	[WhiteList("System.DateTime.Month.get", WhiteListOp.Discard)]
	public extern static Number _a8a6b6e36a0ea736(Date instance);

	[WhiteList("static System.DateTime.Now.get", WhiteListOp.Discard)]
	public extern static Date _ee9dd166a34a2fa5(Date instance);

	[WhiteList("System.DateTime.Second.get", WhiteListOp.Discard)]
	public extern static Number _10a94eacb3b7fd2d(Date instance);

	[WhiteList("System.DateTime.Ticks.get", WhiteListOp.Discard)]
	public extern static BigInt _bcde32e170f49354(Date instance);

	[WhiteList("System.DateTime.TimeOfDay.get", WhiteListOp.Discard)]
	public extern static BigInt _2efdc237be2f31aa(Date instance);

	[WhiteList("static System.DateTime.Today.get", WhiteListOp.Discard)]
	public extern static Date _4b250155b7c688bb(Date instance);

	[WhiteList("System.DateTime.Year.get", WhiteListOp.Discard)]
	public extern static Number _9d56b09432f81c05(Date instance);

	///<summary>Returns an indication whether the specified year is a leap year.</summary>
	[WhiteList("static System.DateTime.IsLeapYear(int)", WhiteListOp.Discard)]
	public extern static bool _4a9da83e9cb28c1a(Number year);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using the conventions of the current culture.</summary>
	[WhiteList("static System.DateTime.Parse(string)", WhiteListOp.Discard)]
	public extern static Date _a8a015c2d2bff2f6(object s);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information.</summary>
	[WhiteList("static System.DateTime.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Date _e0128ef45cc8584e(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[WhiteList("static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _7372e5e0d8ba24a6(object s, Intl.NumberFormat? provider, object styles);

	///<summary>Converts a memory span that contains string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[WhiteList("static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _2c85f5b20ae7559e(Uint32Array s, Intl.NumberFormat? provider, object styles);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[WhiteList("static System.DateTime.ParseExact(string, string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Date _7f3dce20074d610f(object s, object format, Intl.NumberFormat? provider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateTime.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _75cd4a49bd890e13(object s, object format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateTime.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _da7c1ef7b418c87d(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateTime.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _f47f23f5482d6f56(object s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[WhiteList("static System.DateTime.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)", WhiteListOp.Discard)]
	public extern static Date _32afd1b56d3b1c77(Uint32Array s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> that subtracts the specified date and time from the value of this instance.</summary>
	[WhiteList("System.DateTime.Subtract(System.DateTime)", WhiteListOp.Discard)]
	public extern static BigInt _4f5d235cac779f38(Date instance, Date value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that subtracts the specified duration from the value of this instance.</summary>
	[WhiteList("System.DateTime.Subtract(System.TimeSpan)", WhiteListOp.Discard)]
	public extern static Date _20a406afebff2025(Date instance, BigInt value);

	///<summary>Converts the value of this instance to the equivalent OLE Automation date.</summary>
	[WhiteList("System.DateTime.ToOADate()", WhiteListOp.Discard)]
	public extern static Number _fb61bb2ccf4b10b6(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[WhiteList("System.DateTime.ToFileTime()", WhiteListOp.Discard)]
	public extern static BigInt _37ee48ca629793fa(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[WhiteList("System.DateTime.ToFileTimeUtc()", WhiteListOp.Discard)]
	public extern static BigInt _c02c49ea68661175(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to local time.</summary>
	[WhiteList("System.DateTime.ToLocalTime()", WhiteListOp.Discard)]
	public extern static Date _db842725d5fd1ca0(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long date string representation.</summary>
	[WhiteList("System.DateTime.ToLongDateString()", WhiteListOp.Discard)]
	public extern static string _6e78dc03eecdd423(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long time string representation.</summary>
	[WhiteList("System.DateTime.ToLongTimeString()", WhiteListOp.Discard)]
	public extern static string _ab161bb1563732af(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short date string representation.</summary>
	[WhiteList("System.DateTime.ToShortDateString()", WhiteListOp.Discard)]
	public extern static string _6a67d54f5c865e5e(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short time string representation.</summary>
	[WhiteList("System.DateTime.ToShortTimeString()", WhiteListOp.Discard)]
	public extern static string _af2d02ec0c0a300d(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the formatting conventions of the current culture.</summary>
	[WhiteList("override System.DateTime.ToString()", WhiteListOp.Discard)]
	public extern static string _6659b3b5d1f081dd(Date instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[WhiteList("System.DateTime.ToString(string)", WhiteListOp.Discard)]
	public extern static string _3ee3e9478fe9a1fb(Date instance, object format);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("System.DateTime.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _606066f0ee1488c6(Date instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[WhiteList("System.DateTime.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _85393faf5839b9ef(Date instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current datetime instance into the provided span of characters.</summary>
	[WhiteList("System.DateTime.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _b50913efa5ca8082(Date instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("System.DateTime.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _100d184b5413769d(Date instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to Coordinated Universal Time (UTC).</summary>
	[WhiteList("System.DateTime.ToUniversalTime()", WhiteListOp.Discard)]
	public extern static Date _b62871088df3ca8f(Date instance);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParse(string, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _fa25ca318f086bb6(object s, Box<Date> result);

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _8658c3be6edb9d2c(Uint32Array s, Box<Date> result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _34043b1eb3a8183a(object s, Intl.NumberFormat? provider, object styles, Box<Date> result);

	///<summary>Converts the span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _6e8546b461b48646(Uint32Array s, Intl.NumberFormat? provider, object styles, Box<Date> result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _79e29a1615b41471(object s, object format, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _d8720f2bb55cf0af(Uint32Array s, Uint32Array format, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _d1eb33e53764ee27(object s, object formats, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[WhiteList("static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _685c4ca5481f00e7(Uint32Array s, object formats, Intl.NumberFormat? provider, object style, Box<Date> result);

	///<summary>Adds a specified time interval to a specified date and time, yielding a new date and time.</summary>
	[WhiteList("static System.DateTime.operator +(System.DateTime, System.TimeSpan)", WhiteListOp.Allowed)]
	public extern static Date _d48b23d7c5f7c2aa(Date d, BigInt t);

	///<summary>Subtracts a specified time interval from a specified date and time and returns a new date and time.</summary>
	[WhiteList("static System.DateTime.operator -(System.DateTime, System.TimeSpan)", WhiteListOp.Allowed)]
	public extern static Date _8d9ea66839ce392a(Date d, BigInt t);

	///<summary>Subtracts a specified date and time from another specified date and time and returns a time interval.</summary>
	[WhiteList("static System.DateTime.operator -(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static BigInt _85b6d162b092ce0e(Date d1, Date d2);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are equal.</summary>
	[WhiteList("static System.DateTime.operator ==(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static bool _37d87f65292f7083(Date d1, Date d2);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are not equal.</summary>
	[WhiteList("static System.DateTime.operator !=(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static bool _89406f797d33e566(Date d1, Date d2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[WhiteList("static System.DateTime.operator <(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static bool _5a97e2aec50193b3(Date t1, Date t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[WhiteList("static System.DateTime.operator <=(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static bool _a8b15168323b118c(Date t1, Date t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[WhiteList("static System.DateTime.operator >(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static bool _e98b0598f4980bcc(Date t1, Date t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[WhiteList("static System.DateTime.operator >=(System.DateTime, System.DateTime)", WhiteListOp.Allowed)]
	public extern static bool _91697ebd6031bb97(Date t1, Date t2);

	///<summary>Deconstructs this <see cref="T:System.DateTime" /> instance by <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />.</summary>
	[WhiteList("System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)", WhiteListOp.Discard)]
	public extern static void _bcf4183bef96ea21(Date instance, Box<Date> date, Box<Number> time);

	///<summary>Deconstructs this <see cref="T:System.DateOnly" /> instance by <see cref="P:System.DateTime.Year" />, <see cref="P:System.DateTime.Month" />, and <see cref="P:System.DateTime.Day" />.</summary>
	[WhiteList("System.DateTime.Deconstruct(out int, out int, out int)", WhiteListOp.Discard)]
	public extern static void _5f721827cf6b8105(Date instance, Box<Number> year, Box<Number> month, Box<Number> day);

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers.</summary>
	[WhiteList("System.DateTime.GetDateTimeFormats()", WhiteListOp.Discard)]
	public extern static string[] _8022abe7c2a9b946(Date instance);

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers and the specified culture-specific formatting information.</summary>
	[WhiteList("System.DateTime.GetDateTimeFormats(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string[] _65e4d2ad2f14918c(Date instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier.</summary>
	[WhiteList("System.DateTime.GetDateTimeFormats(char)", WhiteListOp.Discard)]
	public extern static string[] _daa9858a8adf981d(Date instance, Number format);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier and culture-specific formatting information.</summary>
	[WhiteList("System.DateTime.GetDateTimeFormats(char, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string[] _10c081a451aa4b71(Date instance, Number format, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.DateTime" />.</summary>
	[WhiteList("System.DateTime.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _9164c7979da236d5(Date instance);

	///<summary>Tries to parse a string into a value.</summary>
	[WhiteList("static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _6c36c46db30aacc1(object s, Intl.NumberFormat? provider, Box<Date> result);

	///<summary>Parses a span of characters into a value.</summary>
	[WhiteList("static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Date _41dcf008ea7cf6d9(Uint32Array s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[WhiteList("static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)", WhiteListOp.Discard)]
	public extern static bool _63fd53f09ba16132(Uint32Array s, Intl.NumberFormat? provider, Box<Date> result);

	[WhiteList("static System.DateTime.UtcNow.get", WhiteListOp.Discard)]
	public extern static Date _d4c39bdf47f391cf(Date instance);
}
