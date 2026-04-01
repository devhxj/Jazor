namespace Jazor.CLR;

[ECMAScriptModule("System/TimeSpanModule.js")]
[Jazor(Op.Alias, "System.TimeSpan", "Object")]
public static class TimeSpanModule
{
	private static BigInt TicksPerMicrosecond => BigInt_("10");
	private static BigInt TicksPerMillisecond => BigInt_("10000");
	private static BigInt TicksPerSecond => BigInt_("10000000");
	private static BigInt TicksPerMinute => BigInt_("600000000");
	private static BigInt TicksPerHour => BigInt_("36000000000");
	private static BigInt TicksPerDay => BigInt_("864000000000");

	private static RuntimeModule.JTimeSpan Create(Number days, Number hours, Number minutes, Number seconds, Number milliseconds, Number microseconds)
	{
		var ticks = BigInt_(days) * TicksPerDay
			+ BigInt_(hours) * TicksPerHour
			+ BigInt_(minutes) * TicksPerMinute
			+ BigInt_(seconds) * TicksPerSecond
			+ BigInt_(milliseconds) * TicksPerMillisecond
			+ BigInt_(microseconds) * TicksPerMicrosecond;
		return new RuntimeModule.JTimeSpan(ticks);
	}

	private static RuntimeModule.JTimeSpan ParseCore(string input)
	{
		var s = input.Trim();
		if (s.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid TimeSpan.");

		var negative = false;
		if (s[0] == '+' || s[0] == '-')
		{
			negative = s[0] == '-';
			s = s.Substring(1);
		}

		var firstColon = s.IndexOf(':');
		if (firstColon < 0)
			throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");

		var lastColon = s.LastIndexOf(':');
		var prefix = s.Substring(0, firstColon);
		var daySeparator = prefix.IndexOf('.');
		var hasDays = daySeparator >= 0;
		var dayText = "0";
		var hourText = prefix;
		if (hasDays)
		{
			dayText = prefix.Substring(0, daySeparator);
			hourText = prefix.Substring(daySeparator + 1);
		}

		var minuteText = lastColon == firstColon
			? s.Substring(firstColon + 1)
			: s.Substring(firstColon + 1, lastColon - firstColon - 1);

		var secondText = "0";
		var fractionText = "";
		if (lastColon != firstColon)
		{
			var tail = s.Substring(lastColon + 1);
			var fractionSeparator = tail.IndexOf('.');
			if (fractionSeparator < 0)
			{
				secondText = tail;
			}
			else
			{
				secondText = tail.Substring(0, fractionSeparator);
				fractionText = tail.Substring(fractionSeparator + 1);
			}
		}
		else
		{
			var fractionSeparator = minuteText.IndexOf('.');
			if (fractionSeparator >= 0)
				throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");
		}

		var days = Number_(dayText);
		var hours = Number_(hourText);
		var minutes = Number_(minuteText);
		var seconds = Number_(secondText);
		if (IsNaN(days) || IsNaN(hours) || IsNaN(minutes) || IsNaN(seconds))
			throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");
		if (days < 0 || hours < 0 || minutes < 0 || seconds < 0)
			throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");
		if (hasDays && hours > 23)
			throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");
		if (minutes > 59 || seconds > 59)
			throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");

		var fractionTicks = BigInt.Zero;
		if (fractionText.Length > 0)
		{
			if (fractionText.Length > 7)
				throw new Error($"FormatException: String '{input}' was not recognized as a valid TimeSpan.");

			while (fractionText.Length < 7)
				fractionText += "0";

			fractionTicks = BigInt_(fractionText);
		}

		var totalTicks = BigInt_(days) * TicksPerDay
			+ BigInt_(hours) * TicksPerHour
			+ BigInt_(minutes) * TicksPerMinute
			+ BigInt_(seconds) * TicksPerSecond
			+ fractionTicks;

		return new RuntimeModule.JTimeSpan(negative ? -totalTicks : totalTicks);
	}

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

	// 常量字段 - 使用 Op.Inline 内联 BigInt 字面量

	/// <summary>
	/// C#: TimeSpan.Zero
	/// JS: wrapper with zero ticks
	/// </summary>
	[Jazor(Op.Import, "static readonly System.TimeSpan.Zero")]
	public static RuntimeModule.JTimeSpan _e5548fcde33957a6() => new(BigInt.Zero);

	/// <summary>
	/// C#: TimeSpan.MaxValue
	/// JS: wrapper with max ticks
	/// </summary>
	[Jazor(Op.Import, "static readonly System.TimeSpan.MaxValue")]
	public static RuntimeModule.JTimeSpan _15e7c0dd01e25108() => new(BigInt_("9223372036854775807"));

	/// <summary>
	/// C#: TimeSpan.MinValue
	/// JS: wrapper with min ticks
	/// </summary>
	[Jazor(Op.Import, "static readonly System.TimeSpan.MinValue")]
	public static RuntimeModule.JTimeSpan _3205534506581110() => new(BigInt_("-9223372036854775808"));

	[Jazor(Op.Import ,"System.TimeSpan.TimeSpan()")]
	public static RuntimeModule.JTimeSpan _5af0f6ad850e6702() => new(BigInt.Zero);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to the specified number of ticks.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.TimeSpan(long)")]
	public static RuntimeModule.JTimeSpan _d4ecddf3bf0f01b8(BigInt ticks) => new(ticks);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, and seconds.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.TimeSpan(int, int, int)")]
	public static RuntimeModule.JTimeSpan _6f22e268aec62fe7(Number hours, Number minutes, Number seconds)
		=> Create(0, hours, minutes, seconds, 0, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, and seconds.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.TimeSpan(int, int, int, int)")]
	public static RuntimeModule.JTimeSpan _13098d82160f45dc(Number days, Number hours, Number minutes, Number seconds)
		=> Create(days, hours, minutes, seconds, 0, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, and milliseconds.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.TimeSpan(int, int, int, int, int)")]
	public static RuntimeModule.JTimeSpan _d5283dec9fea7d04(Number days, Number hours, Number minutes, Number seconds, Number milliseconds)
		=> Create(days, hours, minutes, seconds, milliseconds, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.TimeSpan(int, int, int, int, int, int)")]
	public static RuntimeModule.JTimeSpan _baceecc82b7d48ba(Number days, Number hours, Number minutes, Number seconds, Number milliseconds, Number microseconds)
		=> Create(days, hours, minutes, seconds, milliseconds, microseconds);

	/// <summary>
	/// C#: TimeSpan.Ticks
	/// JS: instance (TimeSpan在JS中用BigInt表示ticks)
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Ticks.get")]
	public static BigInt _72d4a471ef1a968f(RuntimeModule.JTimeSpan instance)
		=> instance.Ticks;

	/// <summary>
	/// C#: TimeSpan.Days
	/// JS: Number(instance / 864000000000n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Days.get")]
	public static Number _a980180cac17c195(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerDay = 864000000000
		return Number_(instance.Ticks / BigInt_("864000000000"));
	}

	/// <summary>
	/// C#: TimeSpan.Hours
	/// JS: (instance / 36000000000n) % 24n
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Hours.get")]
	public static Number _e1126ea3789ed210(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerHour = 36000000000, HoursPerDay = 24
		return Number_((instance.Ticks / BigInt_("36000000000")) % BigInt_("24"));
	}

	/// <summary>
	/// C#: TimeSpan.Milliseconds
	/// JS: (instance / 10000n) % 1000n
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Milliseconds.get")]
	public static Number _af6dae8b5cdc7078(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerMillisecond = 10000, MillisecondsPerSecond = 1000
		return Number_((instance.Ticks / BigInt_(10000)) % BigInt_(1000));
	}

	/// <summary>
	/// C#: TimeSpan.Microseconds
	/// JS: (instance / 10n) % 1000000n
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Microseconds.get")]
	public static Number _b5ff892bced87c7a(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerMicrosecond = 10, MicrosecondsPerSecond = 1000000
		return Number_((instance.Ticks / BigInt_(10)) % BigInt_(1000000));
	}

	/// <summary>
	/// C#: TimeSpan.Nanoseconds
	/// JS: (instance * 100n) % 1000000000n
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Nanoseconds.get")]
	public static Number _95472c42904823fa(RuntimeModule.JTimeSpan instance)
	{
		// NanosecondsPerTick = 100, NanosecondsPerSecond = 1000000000
		return Number_((instance.Ticks * BigInt_(100)) % BigInt_(1000000000));
	}

	/// <summary>
	/// C#: TimeSpan.Minutes
	/// JS: (instance / 600000000n) % 60n
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Minutes.get")]
	public static Number _f84ed3952defaf6d(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerMinute = 600000000, MinutesPerHour = 60
		return Number_((instance.Ticks / BigInt_(600000000)) % BigInt_(60));
	}

	/// <summary>
	/// C#: TimeSpan.Seconds
	/// JS: (instance / 10000000n) % 60n
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.Seconds.get")]
	public static Number _f3cdc3642c68ede1(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerSecond = 10000000, SecondsPerMinute = 60
		return Number_((instance.Ticks / BigInt_(10000000)) % BigInt_(60));
	}

	/// <summary>
	/// C#: TimeSpan.TotalDays
	/// JS: Number(instance) / 864000000000
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalDays.get")]
	public static Number _3709bd5d7e02854b(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerDay = 864000000000
		return Number_(instance.Ticks) / 864000000000d;
	}

	/// <summary>
	/// C#: TimeSpan.TotalHours
	/// JS: Number(instance) / 36000000000
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalHours.get")]
	public static Number _b4c8b94ce8b8d996(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerHour = 36000000000
		return Number_(instance.Ticks) / 36000000000d;
	}

	/// <summary>
	/// C#: TimeSpan.TotalMilliseconds
	/// JS: Number(instance) / 10000
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalMilliseconds.get")]
	public static Number _b73ebb6b17996726(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerMillisecond = 10000
		return Number_(instance.Ticks) / 10000;
	}

	/// <summary>
	/// C#: TimeSpan.TotalMicroseconds
	/// JS: Number(instance) / 10
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalMicroseconds.get")]
	public static Number _48066d805fb56409(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerMicrosecond = 10
		return Number_(instance.Ticks) / 10;
	}

	/// <summary>
	/// C#: TimeSpan.TotalNanoseconds
	/// JS: Number(instance) * 100
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalNanoseconds.get")]
	public static Number _c34f00910f115965(RuntimeModule.JTimeSpan instance)
	{
		// NanosecondsPerTick = 100
		return Number_(instance.Ticks) * 100;
	}

	/// <summary>
	/// C#: TimeSpan.TotalMinutes
	/// JS: Number(instance) / 600000000
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalMinutes.get")]
	public static Number _265f245f5ef9d2ed(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerMinute = 600000000
		return Number_(instance.Ticks) / 600000000;
	}

	/// <summary>
	/// C#: TimeSpan.TotalSeconds
	/// JS: Number(instance) / 10000000
	/// </summary>
	[Jazor(Op.Import, "System.TimeSpan.TotalSeconds.get")]
	public static Number _d3a0d6dab09b85a6(RuntimeModule.JTimeSpan instance)
	{
		// TicksPerSecond = 10000000
		return Number_(instance.Ticks) / 10000000;
	}

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the sum of the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Add(System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _0f42e55865af8fbf(RuntimeModule.JTimeSpan instance, RuntimeModule.JTimeSpan ts) => new(instance.Ticks + ts.Ticks);

	///<summary>Compares two <see cref="T:System.TimeSpan" /> values and returns an integer that indicates whether the first value is shorter than, equal to, or longer than the second value.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)")]
	public static Number _06719a9a062fc7ca(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
	{
		if (t1.Ticks < t2.Ticks)
			return -1;
		if (t1.Ticks > t2.Ticks)
			return 1;
		return 0;
	}

	///<summary>Compares this instance to a specified object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the specified object.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.CompareTo(object)")]
	public static Number _224114f954c0aa27(RuntimeModule.JTimeSpan instance, object? value)
	{
		var other = value as RuntimeModule.JTimeSpan;
		if (other == null)
			throw new Error("ArgumentException: Object must be of type TimeSpan.");

		return _810426c1d7c3f64f(instance, other);
	}

	///<summary>Compares this instance to a specified <see cref="T:System.TimeSpan" /> object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.CompareTo(System.TimeSpan)")]
	public static Number _810426c1d7c3f64f(RuntimeModule.JTimeSpan instance, RuntimeModule.JTimeSpan value)
	{
		if (instance.Ticks < value.Ticks)
			return -1;
		if (instance.Ticks > value.Ticks)
			return 1;
		return 0;
	}

	/// <summary>
	/// C#: TimeSpan.FromDays(double)
	/// JS: BigInt(Math.floor(value * 864000000000))
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromDays(double)")]
	public static RuntimeModule.JTimeSpan _174093cb4f47884f(Number value)
	{
		// TicksPerDay = 864000000000
		return new RuntimeModule.JTimeSpan(BigInt_(Math.Floor_(value * 864000000000d)));
	}

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the absolute value of the current <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Duration()")]
	public static RuntimeModule.JTimeSpan _eeb4ad83b79a892c(RuntimeModule.JTimeSpan instance) => new(instance.Ticks < BigInt.Zero ? -instance.Ticks : instance.Ticks);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Import ,"override System.TimeSpan.Equals(object)")]
	public static bool _c6b8a216cf6205b9(RuntimeModule.JTimeSpan instance, object? value)
	{
		var other = value as RuntimeModule.JTimeSpan;
		return other != null && _6b7d08559c6c9859(instance, other);
	}

	///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Equals(System.TimeSpan)")]
	public static bool _6b7d08559c6c9859(RuntimeModule.JTimeSpan instance, RuntimeModule.JTimeSpan obj)
		=> instance.Ticks == obj.Ticks;

	///<summary>Returns a value that indicates whether two specified instances of <see cref="T:System.TimeSpan" /> are equal.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.Equals(System.TimeSpan, System.TimeSpan)")]
	public static bool _77a10002dccedd59(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks == t2.Ticks;

	///<summary>Returns a hash code for this instance.</summary>
	[Jazor(Op.Import ,"override System.TimeSpan.GetHashCode()")]
	public static Number _650390adf244b5eb(RuntimeModule.JTimeSpan instance)
		=> Number_(instance.Ticks % BigInt_("2147483647"));

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromDays(int)")]
	public static RuntimeModule.JTimeSpan _1ef0cc8c95c82bc4(Number days)
		=> Create(days, 0, 0, 0, 0, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromDays(int, int, long, long, long, long)")]
	public static RuntimeModule.JTimeSpan _3e2fa32df3160e87(Number days, Number hours, BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds)
		=> Create(days, hours, Number_(minutes), Number_(seconds), Number_(milliseconds), Number_(microseconds));

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromHours(int)")]
	public static RuntimeModule.JTimeSpan _98fc150ce35e78d8(Number hours)
		=> Create(0, hours, 0, 0, 0, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromHours(int, long, long, long, long)")]
	public static RuntimeModule.JTimeSpan _f307370e05d16ca3(Number hours, BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds)
		=> Create(0, hours, Number_(minutes), Number_(seconds), Number_(milliseconds), Number_(microseconds));

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of minutes.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromMinutes(long)")]
	public static RuntimeModule.JTimeSpan _059d32e87cf36f24(BigInt minutes)
		=> Create(0, 0, Number_(minutes), 0, 0, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of minutes, seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromMinutes(long, long, long, long)")]
	public static RuntimeModule.JTimeSpan _f07d6f07ee70a1bd(BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds)
		=> Create(0, 0, Number_(minutes), Number_(seconds), Number_(milliseconds), Number_(microseconds));

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of seconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromSeconds(long)")]
	public static RuntimeModule.JTimeSpan _e0c33d45a9703e74(BigInt seconds)
		=> Create(0, 0, 0, Number_(seconds), 0, 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of seconds, milliseconds, and microseconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromSeconds(long, long, long)")]
	public static RuntimeModule.JTimeSpan _60df3ea4b8b2693c(BigInt seconds, BigInt milliseconds, BigInt microseconds)
		=> Create(0, 0, 0, Number_(seconds), Number_(milliseconds), Number_(microseconds));

	[Jazor(Op.Import ,"static System.TimeSpan.FromMilliseconds(long)")]
	public static RuntimeModule.JTimeSpan _9dc3c54535eb1333(BigInt milliseconds)
		=> Create(0, 0, 0, 0, Number_(milliseconds), 0);

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of milliseconds, and microseconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromMilliseconds(long, long)")]
	public static RuntimeModule.JTimeSpan _4bf16885c28b9c57(BigInt milliseconds, BigInt microseconds)
		=> Create(0, 0, 0, 0, Number_(milliseconds), Number_(microseconds));

	///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of microseconds.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.FromMicroseconds(long)")]
	public static RuntimeModule.JTimeSpan _5864e2e6b3820640(BigInt microseconds)
		=> Create(0, 0, 0, 0, 0, Number_(microseconds));

	/// <summary>
	/// C#: TimeSpan.FromHours(double)
	/// JS: BigInt(Math.floor(value * 36000000000))
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromHours(double)")]
	public static RuntimeModule.JTimeSpan _105dc0462f9876d6(Number value)
	{
		// TicksPerHour = 36000000000
		return new RuntimeModule.JTimeSpan(BigInt_(Math.Floor_(value * 36000000000d)));
	}

	/// <summary>
	/// C#: TimeSpan.FromMilliseconds(double)
	/// JS: BigInt(Math.floor(value * 10000))
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromMilliseconds(double)")]
	public static RuntimeModule.JTimeSpan _a6de3a3b561d553b(Number value)
	{
		// TicksPerMillisecond = 10000
		return new RuntimeModule.JTimeSpan(BigInt_(Math.Floor_(value * 10000d)));
	}

	/// <summary>
	/// C#: TimeSpan.FromMicroseconds(double)
	/// JS: BigInt(Math.floor(value * 10))
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromMicroseconds(double)")]
	public static RuntimeModule.JTimeSpan _e05c52466faba973(Number value)
	{
		// TicksPerMicrosecond = 10
		return new RuntimeModule.JTimeSpan(BigInt_(Math.Floor_(value * 10d)));
	}

	/// <summary>
	/// C#: TimeSpan.FromMinutes(double)
	/// JS: BigInt(Math.floor(value * 600000000))
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromMinutes(double)")]
	public static RuntimeModule.JTimeSpan _2af67432bdd77d15(Number value)
	{
		// TicksPerMinute = 600000000
		return new RuntimeModule.JTimeSpan(BigInt_(Math.Floor_(value * 600000000d)));
	}

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the negated value of this instance.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Negate()")]
	public static RuntimeModule.JTimeSpan _63a8d2e980965d93(RuntimeModule.JTimeSpan instance) => new(-instance.Ticks);

	/// <summary>
	/// C#: TimeSpan.FromSeconds(double)
	/// JS: BigInt(Math.floor(value * 10000000))
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromSeconds(double)")]
	public static RuntimeModule.JTimeSpan _77a04fa2e0b66990(Number value)
	{
		// TicksPerSecond = 10000000
		return new RuntimeModule.JTimeSpan(BigInt_(Math.Floor_(value * 10000000)));
	}

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the difference between the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Subtract(System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _3c5049382d7807a8(RuntimeModule.JTimeSpan instance, RuntimeModule.JTimeSpan ts) => new(instance.Ticks - ts.Ticks);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object which value is the result of multiplication of this instance and the specified <paramref name="factor" />.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Multiply(double)")]
	public static RuntimeModule.JTimeSpan _a1b4efac0485c39e(RuntimeModule.JTimeSpan instance, Number factor) => new(BigInt_(Math.Round_(Number_(instance.Ticks) * factor)));

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the result of dividing this instance by the specified <paramref name="divisor" />.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Divide(double)")]
	public static RuntimeModule.JTimeSpan _871609175f846ae9(RuntimeModule.JTimeSpan instance, Number divisor) => new(BigInt_(Math.Round_(Number_(instance.Ticks) / divisor)));

	///<summary>Returns a new <see cref="T:System.Double" /> value that's the result of dividing this instance by <paramref name="ts" />.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.Divide(System.TimeSpan)")]
	public static Number _ca7e20ad5bf4a61a(RuntimeModule.JTimeSpan instance, RuntimeModule.JTimeSpan ts) => Number_(instance.Ticks) / Number_(ts.Ticks);

	/// <summary>
	/// C#: TimeSpan.FromTicks(long)
	/// JS: value (TimeSpan在JS中直接用BigInt表示ticks)
	/// </summary>
	[Jazor(Op.Import, "static System.TimeSpan.FromTicks(long)")]
	public static RuntimeModule.JTimeSpan _a43571552d95203d(BigInt value) => new(value);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.Parse(string)")]
	public static RuntimeModule.JTimeSpan _7b8fc48a806ecb54(string s)
		=> ParseCore(s);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.Parse(string, System.IFormatProvider)")]
	public static RuntimeModule.JTimeSpan _55da737da6ee6a65(string input, Intl.NumberFormat? formatProvider)
		=> ParseCore(input);

	///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static RuntimeModule.JTimeSpan _f2cd45773b91a418(string input, Intl.NumberFormat? formatProvider)
		=> ParseCore(input);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string, System.IFormatProvider)")]
	public extern static RuntimeModule.JTimeSpan _42989b67e04b2f67(string input, string format, Intl.NumberFormat? formatProvider);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified array of format strings and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string[], System.IFormatProvider)")]
	public extern static RuntimeModule.JTimeSpan _e5cf9105cd12d522(string input, object formats, Intl.NumberFormat? formatProvider);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string, System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static RuntimeModule.JTimeSpan _8a71d95721e67fec(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the char span of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static RuntimeModule.JTimeSpan _67b8aeaab1d188d1(string input, string format, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(string, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static RuntimeModule.JTimeSpan _48c034f2c5ba751e(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
	public extern static RuntimeModule.JTimeSpan _bd0deac0342bb804(string input, object formats, Intl.NumberFormat? formatProvider, object styles);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.TryParse(string, out System.TimeSpan)")]
	public static Array<object?> _6fb85ef4d11b9143(string? s, RuntimeModule.JTimeSpan result)
	{
		if (s == null || s.Length == 0)
			return [false, new RuntimeModule.JTimeSpan(BigInt.Zero)];

		try
		{
			return [true, ParseCore(s)];
		}
		catch
		{
			return [false, new RuntimeModule.JTimeSpan(BigInt.Zero)];
		}
	}

	///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, out System.TimeSpan)")]
	public static Array<object?> _11fc2c166b0126e3(string s, RuntimeModule.JTimeSpan result)
		=> _6fb85ef4d11b9143(s, result);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.TryParse(string, System.IFormatProvider, out System.TimeSpan)")]
	public static Array<object?> _0d5a8bac05463d1f(string? input, Intl.NumberFormat? formatProvider, RuntimeModule.JTimeSpan result)
		=> _6fb85ef4d11b9143(input, result);

	///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)")]
	public static Array<object?> _5eae656c46346343(string input, Intl.NumberFormat? formatProvider, RuntimeModule.JTimeSpan result)
		=> _6fb85ef4d11b9143(input, result);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string, System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _2b2eb2e3db30b277(string? input, string? format, Intl.NumberFormat? formatProvider, RuntimeModule.JTimeSpan result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _864eccd29dc703e8(string input, string format, Intl.NumberFormat? formatProvider, RuntimeModule.JTimeSpan result);

	///<summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string[], System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _c7fd68b8fa43fc42(string? input, object formats, Intl.NumberFormat? formatProvider, RuntimeModule.JTimeSpan result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, out System.TimeSpan)")]
	public extern static Array<object?> _2dcb055dc5bc064e(string input, object formats, Intl.NumberFormat? formatProvider, RuntimeModule.JTimeSpan result);

	///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information and styles. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string, System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _e8b6d8dc1990db2c(string? input, string? format, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JTimeSpan result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _277b3d9d45b63643(string input, string format, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JTimeSpan result);

	///<summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _0a5d629d630a904a(string? input, object formats, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JTimeSpan result);

	///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information and styles. The format of the string representation must match one of the specified formats exactly.</summary>
	[Jazor(Op.Discard ,"static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
	public extern static Array<object?> _cd955c1ba8f6a113(string input, object formats, Intl.NumberFormat? formatProvider, object styles, RuntimeModule.JTimeSpan result);

	///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation.</summary>
	[Jazor(Op.Alias ,"override System.TimeSpan.ToString()", "toString")]
	public extern static string _e595ae184a61ca5a(RuntimeModule.JTimeSpan instance);

	///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.ToString(string)")]
	public static string _95c4c385ed7aa2da(RuntimeModule.JTimeSpan instance, string? format)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format and culture-specific formatting information.</summary>
	[Jazor(Op.Import ,"System.TimeSpan.ToString(string, System.IFormatProvider)")]
	public static string _49fbba4d75df94f7(RuntimeModule.JTimeSpan instance, string? format, Intl.NumberFormat? formatProvider)
		=> instance.ToString();

	///<summary>Tries to format the value of the current timespan number instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _9f800f3ed3ef2b88(RuntimeModule.JTimeSpan instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.TimeSpan.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _2d87ae3016019fc2(RuntimeModule.JTimeSpan instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? formatProvider);

	///<summary>Returns a <see cref="T:System.TimeSpan" /> whose value is the negated value of the specified instance.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator -(System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _e8e884a7b14ce4b4(RuntimeModule.JTimeSpan t) => new(-t.Ticks);

	///<summary>Subtracts a specified <see cref="T:System.TimeSpan" /> from another specified <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator -(System.TimeSpan, System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _0228a4c011d04780(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2) => new(t1.Ticks - t2.Ticks);

	///<summary>Returns the specified instance of <see cref="T:System.TimeSpan" />.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator +(System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _6c2fe85d341763c7(RuntimeModule.JTimeSpan t) => new(t.Ticks);

	///<summary>Adds two specified <see cref="T:System.TimeSpan" /> instances.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator +(System.TimeSpan, System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _24670e70abc0feb8(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2) => new(t1.Ticks + t2.Ticks);

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of multiplying the specified <code data-dev-comment-type="paramref">timeSpan</code> instance and the specified <code data-dev-comment-type="paramref">factor</code>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator *(System.TimeSpan, double)")]
	public static RuntimeModule.JTimeSpan _f2a4ea62d054d8a3(RuntimeModule.JTimeSpan timeSpan, Number factor) => new(BigInt_(Math.Round_(Number_(timeSpan.Ticks) * factor)));

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of multiplying the specified <code data-dev-comment-type="paramref">factor</code> and the specified <code data-dev-comment-type="paramref">timeSpan</code> instance.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator *(double, System.TimeSpan)")]
	public static RuntimeModule.JTimeSpan _90eaec13ec0f9fea(Number factor, RuntimeModule.JTimeSpan timeSpan) => new(BigInt_(Math.Round_(factor * Number_(timeSpan.Ticks))));

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of dividing the specified <code data-dev-comment-type="paramref">timeSpan</code> by the specified <code data-dev-comment-type="paramref">divisor</code>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator /(System.TimeSpan, double)")]
	public static RuntimeModule.JTimeSpan _eba9e2c9c23d7df9(RuntimeModule.JTimeSpan timeSpan, Number divisor) => new(BigInt_(Math.Round_(Number_(timeSpan.Ticks) / divisor)));

	///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value that's the result of dividing <code data-dev-comment-type="paramref">t1</code> by <code data-dev-comment-type="paramref">t2</code>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator /(System.TimeSpan, System.TimeSpan)")]
	public static Number _f857571e543b3b87(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> Number_(t1.Ticks) / Number_(t2.Ticks);

	///<summary>Indicates whether two <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> instances are equal.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator ==(System.TimeSpan, System.TimeSpan)")]
	public static bool _cb0f1b7f98578d6e(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks == t2.Ticks;

	///<summary>Indicates whether two <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> instances are not equal.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator !=(System.TimeSpan, System.TimeSpan)")]
	public static bool _20d19f6d7c8824a6(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks != t2.Ticks;

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator <(System.TimeSpan, System.TimeSpan)")]
	public static bool _7b0fd798871f70d1(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks < t2.Ticks;

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator <=(System.TimeSpan, System.TimeSpan)")]
	public static bool _8d936a645fdca63f(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks <= t2.Ticks;

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator >(System.TimeSpan, System.TimeSpan)")]
	public static bool _99f4b8243dbe421d(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks > t2.Ticks;

	///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeSpan.operator >=(System.TimeSpan, System.TimeSpan)")]
	public static bool _60fd1bb34b700faa(RuntimeModule.JTimeSpan t1, RuntimeModule.JTimeSpan t2)
		=> t1.Ticks >= t2.Ticks;
}
