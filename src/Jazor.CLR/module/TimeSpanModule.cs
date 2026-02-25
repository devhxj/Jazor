namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.TimeSpan","System/TimeSpanModule.js")]
public static class TimeSpanModule
{
	//System.TimeSpan.NanosecondsPerTick = 100;

	//System.TimeSpan.TicksPerMicrosecond = 10;

	//System.TimeSpan.TicksPerMillisecond = 10000;

	//System.TimeSpan.TicksPerSecond = 10000000;

	//System.TimeSpan.TicksPerMinute = 600000000;

	//System.TimeSpan.TicksPerHour = 36000000000;

	//System.TimeSpan.TicksPerDay = 864000000000;

	//System.TimeSpan.MicrosecondsPerMillisecond = 1000;

	//System.TimeSpan.MicrosecondsPerSecond = 1000000;

	//System.TimeSpan.MicrosecondsPerMinute = 60000000;

	//System.TimeSpan.MicrosecondsPerHour = 3600000000;

	//System.TimeSpan.MicrosecondsPerDay = 86400000000;

	//System.TimeSpan.MillisecondsPerSecond = 1000;

	//System.TimeSpan.MillisecondsPerMinute = 60000;

	//System.TimeSpan.MillisecondsPerHour = 3600000;

	//System.TimeSpan.MillisecondsPerDay = 86400000;

	//System.TimeSpan.SecondsPerMinute = 60;

	//System.TimeSpan.SecondsPerHour = 3600;

	//System.TimeSpan.SecondsPerDay = 86400;

	//System.TimeSpan.MinutesPerHour = 60;

	//System.TimeSpan.MinutesPerDay = 1440;

	//System.TimeSpan.HoursPerDay = 24;

	///<summary>Represents the zero <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly System.TimeSpan.Zero")]
	public extern static BigInt _e5548fcde33957a6();

	///<summary>Represents the maximum <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly System.TimeSpan.MaxValue")]
	public extern static BigInt _15e7c0dd01e25108();

	///<summary>Represents the minimum <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>
	[Jazor(Op.Discard ,"static readonly System.TimeSpan.MinValue")]
	public extern static BigInt _3205534506581110();

	[Jazor(Op.Discard ,"System.TimeSpan.TimeSpan()")]
	public extern static BigInt _5af0f6ad850e6702();

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to the specified number of ticks.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TimeSpan(long)")]
	public extern static BigInt _d4ecddf3bf0f01b8(BigInt ticks);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, and seconds.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TimeSpan(int, int, int)")]
	public extern static BigInt _6f22e268aec62fe7(Number hours, Number minutes, Number seconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, and seconds.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TimeSpan(int, int, int, int)")]
	public extern static BigInt _13098d82160f45dc(Number days, Number hours, Number minutes, Number seconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, and milliseconds.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TimeSpan(int, int, int, int, int)")]
	public extern static BigInt _d5283dec9fea7d04(Number days, Number hours, Number minutes, Number seconds, Number milliseconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TimeSpan(int, int, int, int, int, int)")]
	public extern static BigInt _baceecc82b7d48ba(Number days, Number hours, Number minutes, Number seconds, Number milliseconds, Number microseconds);

	[Jazor(Op.Discard ,"System.TimeSpan.Ticks.get")]
	public extern static BigInt _72d4a471ef1a968f(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Days.get")]
	public extern static Number _a980180cac17c195(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Hours.get")]
	public extern static Number _e1126ea3789ed210(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Milliseconds.get")]
	public extern static Number _af6dae8b5cdc7078(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Microseconds.get")]
	public extern static Number _b5ff892bced87c7a(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Nanoseconds.get")]
	public extern static Number _95472c42904823fa(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Minutes.get")]
	public extern static Number _f84ed3952defaf6d(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.Seconds.get")]
	public extern static Number _f3cdc3642c68ede1(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalDays.get")]
	public extern static Number _3709bd5d7e02854b(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalHours.get")]
	public extern static Number _b4c8b94ce8b8d996(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalMilliseconds.get")]
	public extern static Number _b73ebb6b17996726(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalMicroseconds.get")]
	public extern static Number _48066d805fb56409(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalNanoseconds.get")]
	public extern static Number _c34f00910f115965(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalMinutes.get")]
	public extern static Number _265f245f5ef9d2ed(BigInt instance);

	[Jazor(Op.Discard ,"System.TimeSpan.TotalSeconds.get")]
	public extern static Number _d3a0d6dab09b85a6(BigInt instance);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the sum of the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Add(System.TimeSpan)")]
	public extern static BigInt _0f42e55865af8fbf(BigInt instance, BigInt ts);

	///<summary>Compares two <see cref="T:System.TimeSpan" /> values and returns an integer that indicates whether the first value is shorter than, equal to, or longer than the second value.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)")]
	public extern static Number _06719a9a062fc7ca(BigInt t1, BigInt t2);

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the specified object.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.CompareTo(object)")]
	public extern static Number _224114f954c0aa27(BigInt instance, object? value);

	///<summary>Compares this instance to a specified <see cref="T:System.TimeSpan" /> object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.CompareTo(System.TimeSpan)")]
	public extern static Number _810426c1d7c3f64f(BigInt instance, BigInt value);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of days, where the specification is accurate to the nearest millisecond.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromDays(double)")]
	public extern static BigInt _174093cb4f47884f(Number value);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the absolute value of the current <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Duration()")]
	public extern static BigInt _eeb4ad83b79a892c(BigInt instance);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"override System.TimeSpan.Equals(object)")]
	public extern static bool _c6b8a216cf6205b9(BigInt instance, object? value);

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Equals(System.TimeSpan)")]
	public extern static bool _6b7d08559c6c9859(BigInt instance, BigInt obj);

	///<summary>Returns a value that indicates whether two specified instances of <see cref="T:System.TimeSpan" /> are equal.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.Equals(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _77a10002dccedd59(BigInt t1, BigInt t2);

	///<summary>Returns a hash code for this instance.</summary>
	[Jazor(Op.Discard ,"override System.TimeSpan.GetHashCode()")]
	public extern static Number _650390adf244b5eb(BigInt instance);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromDays(int)")]
	public extern static BigInt _1ef0cc8c95c82bc4(Number days);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromDays(int, int, long, long, long, long)")]
	public extern static BigInt _3e2fa32df3160e87(Number days, Number hours, BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromHours(int)")]
	public extern static BigInt _98fc150ce35e78d8(Number hours);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromHours(int, long, long, long, long)")]
	public extern static BigInt _f307370e05d16ca3(Number hours, BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of minutes.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMinutes(long)")]
	public extern static BigInt _059d32e87cf36f24(BigInt minutes);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMinutes(long, long, long, long)")]
	public extern static BigInt _f07d6f07ee70a1bd(BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of seconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromSeconds(long)")]
	public extern static BigInt _e0c33d45a9703e74(BigInt seconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromSeconds(long, long, long)")]
	public extern static BigInt _60df3ea4b8b2693c(BigInt seconds, BigInt milliseconds, BigInt microseconds);

	[Jazor(Op.Discard ,"static System.TimeSpan.FromMilliseconds(long)")]
	public extern static BigInt _9dc3c54535eb1333(BigInt milliseconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of milliseconds, and microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMilliseconds(long, long)")]
	public extern static BigInt _4bf16885c28b9c57(BigInt milliseconds, BigInt microseconds);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMicroseconds(long)")]
	public extern static BigInt _5864e2e6b3820640(BigInt microseconds);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of hours, where the specification is accurate to the nearest millisecond.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromHours(double)")]
	public extern static BigInt _105dc0462f9876d6(Number value);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of milliseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMilliseconds(double)")]
	public extern static BigInt _a6de3a3b561d553b(Number value);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of microseconds.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMicroseconds(double)")]
	public extern static BigInt _e05c52466faba973(Number value);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of minutes, where the specification is accurate to the nearest millisecond.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromMinutes(double)")]
	public extern static BigInt _2af67432bdd77d15(Number value);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the negated value of this instance.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Negate()")]
	public extern static BigInt _63a8d2e980965d93(BigInt instance);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of seconds, where the specification is accurate to the nearest millisecond.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromSeconds(double)")]
	public extern static BigInt _77a04fa2e0b66990(Number value);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the difference between the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Subtract(System.TimeSpan)")]
	public extern static BigInt _3c5049382d7807a8(BigInt instance, BigInt ts);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object which value is the result of multiplication of this instance and the specified <paramref name="factor" />.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Multiply(double)")]
	public extern static BigInt _a1b4efac0485c39e(BigInt instance, Number factor);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the result of dividing this instance by the specified <paramref name="divisor" />.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Divide(double)")]
	public extern static BigInt _871609175f846ae9(BigInt instance, Number divisor);

	///<summary>Returns a new <see cref="T:System.Double" /> value that's the result of dividing this instance by <paramref name="ts" />.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.Divide(System.TimeSpan)")]
	public extern static Number _ca7e20ad5bf4a61a(BigInt instance, BigInt ts);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified time, where the specification is in units of ticks.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.FromTicks(long)")]
	public extern static BigInt _a43571552d95203d(BigInt value);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.Parse(string)")]
	public extern static BigInt _7b8fc48a806ecb54(string s);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.Parse(string, System.IFormatProvider)")]
	public extern static BigInt _55da737da6ee6a65(string input, Intl.NumberFormat? formatProvider);

	///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static BigInt _f2cd45773b91a418(Uint32Array input, Intl.NumberFormat? formatProvider);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string, System.IFormatProvider)")]
	public extern static BigInt _42989b67e04b2f67(string input, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified array of format strings and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string[], System.IFormatProvider)")]
	public extern static BigInt _e5cf9105cd12d522(string input, object formats, Intl.NumberFormat? formatProvider);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string, System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static BigInt _8a71d95721e67fec(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the char span of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static BigInt _67b8aeaab1d188d1(Uint32Array input, Uint32Array format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static BigInt _48c034f2c5ba751e(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static BigInt _bd0deac0342bb804(Uint32Array input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParse(string, out System.TimeSpan)")]
	public extern static Array<object?> _6fb85ef4d11b9143(string? s, BigInt result);

	///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, out System.TimeSpan)")]
	public extern static Array<object?> _11fc2c166b0126e3(Uint32Array s, BigInt result);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParse(string, System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _0d5a8bac05463d1f(string? input, Intl.NumberFormat? formatProvider, BigInt result);

	///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _5eae656c46346343(Uint32Array input, Intl.NumberFormat? formatProvider, BigInt result);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string, System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _2b2eb2e3db30b277(string? input, string? format, Intl.NumberFormat? formatProvider, BigInt result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _864eccd29dc703e8(Uint32Array input, Uint32Array format, Intl.NumberFormat? formatProvider, BigInt result);

	///<summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string[], System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _c7fd68b8fa43fc42(string? input, object formats, Intl.NumberFormat? formatProvider, BigInt result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _2dcb055dc5bc064e(Uint32Array input, object formats, Intl.NumberFormat? formatProvider, BigInt result);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information and styles. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string, System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _e8b6d8dc1990db2c(string? input, string? format, Intl.NumberFormat? formatProvider, object styles, BigInt result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _277b3d9d45b63643(Uint32Array input, Uint32Array format, Intl.NumberFormat? formatProvider, object styles, BigInt result);

	///<summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _0a5d629d630a904a(string? input, object formats, Intl.NumberFormat? formatProvider, object styles, BigInt result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _cd955c1ba8f6a113(Uint32Array input, object formats, Intl.NumberFormat? formatProvider, object styles, BigInt result);

	///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Discard ,"override System.TimeSpan.ToString()")]
	public extern static string _e595ae184a61ca5a(BigInt instance);

	///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.ToString(string)")]
	public extern static string _95c4c385ed7aa2da(BigInt instance, string? format);

	///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format and culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.ToString(string, System.IFormatProvider)")]
	public extern static string _49fbba4d75df94f7(BigInt instance, string? format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current timespan number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _9f800f3ed3ef2b88(BigInt instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _2d87ae3016019fc2(BigInt instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? formatProvider);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> whose value is the negated value of the specified instance.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator -(System.TimeSpan)")]
	public extern static BigInt _e8e884a7b14ce4b4(BigInt t);

	///<summary>Subtracts a specified <see cref="T:System.TimeSpan" /> from another specified <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator -(System.TimeSpan, System.TimeSpan)")]
	public extern static BigInt _0228a4c011d04780(BigInt t1, BigInt t2);

	///<summary>Returns the specified instance of <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator +(System.TimeSpan)")]
	public extern static BigInt _6c2fe85d341763c7(BigInt t);

	///<summary>Adds two specified <see cref="T:System.TimeSpan" /> instances.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator +(System.TimeSpan, System.TimeSpan)")]
	public extern static BigInt _24670e70abc0feb8(BigInt t1, BigInt t2);

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of multiplying the specified <code data-dev-comment-type="paramref">timeSpan</code> instance and the specified <code data-dev-comment-type="paramref">factor</code>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator *(System.TimeSpan, double)")]
	public extern static BigInt _f2a4ea62d054d8a3(BigInt timeSpan, Number factor);

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of multiplying the specified <code data-dev-comment-type="paramref">factor</code> and the specified <code data-dev-comment-type="paramref">timeSpan</code> instance.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator *(double, System.TimeSpan)")]
	public extern static BigInt _90eaec13ec0f9fea(Number factor, BigInt timeSpan);

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of dividing the specified <code data-dev-comment-type="paramref">timeSpan</code> by the specified <code data-dev-comment-type="paramref">divisor</code>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator /(System.TimeSpan, double)")]
	public extern static BigInt _eba9e2c9c23d7df9(BigInt timeSpan, Number divisor);

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value that's the result of dividing <code data-dev-comment-type="paramref">t1</code> by <code data-dev-comment-type="paramref">t2</code>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator /(System.TimeSpan, System.TimeSpan)")]
	public extern static Number _f857571e543b3b87(BigInt t1, BigInt t2);

	///<summary>Indicates whether two <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> instances are equal.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator ==(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _cb0f1b7f98578d6e(BigInt t1, BigInt t2);

	///<summary>Indicates whether two <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> instances are not equal.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator !=(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _20d19f6d7c8824a6(BigInt t1, BigInt t2);

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator <(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _7b0fd798871f70d1(BigInt t1, BigInt t2);

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator <=(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _8d936a645fdca63f(BigInt t1, BigInt t2);

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator >(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _99f4b8243dbe421d(BigInt t1, BigInt t2);

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Allowed ,"static System.TimeSpan.operator >=(System.TimeSpan, System.TimeSpan)")]
	public extern static bool _60fd1bb34b700faa(BigInt t1, BigInt t2);
}
