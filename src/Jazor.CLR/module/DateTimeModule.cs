namespace Jazor.CLR;

/// <summary>
/// System.DateTime 类型模块映射规则
///
/// C# DateTime 与 JavaScript Date 的对应关系：
/// - 都表示日期和时间
/// - C# DateTime 是值类型，JavaScript Date 是对象
/// - 大部分方法可以直接映射
///
/// Op 类型选择原则：
/// - Inline: 简单表达式（如 Now、Today）
/// - Alias: JS Date 原生方法（如 getFullYear、getMonth）
/// - Import: 需要完整实现的复杂逻辑
/// - Discard: 不支持或极少使用
/// </summary>
[ECMAScriptModule("System/DateTimeModule.js")]
[Jazor(Op.Alias, "System.DateTime","Object")]
public static class DateTimeModule
{
	private static BigInt UnixEpochTicks => BigInt_("621355968000000000");
	private static BigInt FileTimeUnixEpochTicks => BigInt_("116444736000000000");
	private static BigInt TicksPerMicrosecond => BigInt_("10");
	private static BigInt TicksPerMillisecond => BigInt_("10000");
	private static BigInt ZeroTicks => BigInt.Zero;
	private static BigInt BinaryKindShift => BigInt_("4611686018427387904");
	private static BigInt BinaryTicksMask => BigInt_("4611686018427387903");
	private static Number OADateUnixOffsetDays => 25569d;
	private static Number MillisecondsPerDay => 86400000d;
	private static Number DateTimeKindUnspecified => 0;
	private static Number DateTimeKindUtc => 1;
	private static Number DateTimeKindLocal => 2;

	private static Date CreateLocalDate(Number year, Number month, Number day)
	{
		var result = new Date(0);
		result.SetHours(0, 0, 0, 0);
		result.SetFullYear(year, month - 1, day);
		return result;
	}

	private static Date CreateLocalDateTime(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
	{
		var result = CreateLocalDate(year, month, day);
		result.SetHours(hour, minute, second, millisecond);
		return result;
	}

	private static RuntimeModule.JDateTime CreateFromTicks(BigInt ticks)
		=> CreateFromTicks(ticks, DateTimeKindUnspecified);

	private static RuntimeModule.JDateTime CreateFromTicks(BigInt ticks, Number kind)
	{
		var ticksSinceUnixEpoch = ticks - UnixEpochTicks;
		var milliseconds = ticksSinceUnixEpoch / TicksPerMillisecond;
		var subMillisecondTicks = ticksSinceUnixEpoch % TicksPerMillisecond;
		if (subMillisecondTicks < ZeroTicks)
		{
			milliseconds -= BigInt_(1);
			subMillisecondTicks += TicksPerMillisecond;
		}

		var utc = new Date(Number_(milliseconds));
		return new RuntimeModule.JDateTime(
			new Date(
				utc.GetUTCFullYear(),
				utc.GetUTCMonth(),
				utc.GetUTCDate(),
				utc.GetUTCHours(),
				utc.GetUTCMinutes(),
				utc.GetUTCSeconds(),
				utc.GetUTCMilliseconds()),
			kind,
			subMillisecondTicks);
	}

	private static RuntimeModule.JDateTime CreateDateTime(Date date, Number kind)
		=> new(date, kind);

	private static RuntimeModule.JDateTime CreateDateTime(Date date, Number kind, BigInt subMillisecondTicks)
		=> new(date, kind, subMillisecondTicks);

	private static RuntimeModule.JDateTime CreateFromInstantTicks(BigInt ticks, Number kind)
	{
		if (kind == DateTimeKindUtc)
			return CreateFromTicks(ticks, kind);

		var ticksSinceUnixEpoch = ticks - UnixEpochTicks;
		var milliseconds = ticksSinceUnixEpoch / TicksPerMillisecond;
		var subMillisecondTicks = ticksSinceUnixEpoch % TicksPerMillisecond;
		if (subMillisecondTicks < ZeroTicks)
		{
			milliseconds -= BigInt_(1);
			subMillisecondTicks += TicksPerMillisecond;
		}

		return new RuntimeModule.JDateTime(new Date(Number_(milliseconds)), kind, subMillisecondTicks);
	}

	private static Number GetKind(object kind)
	{
		var value = Number_(kind);
		if (value != DateTimeKindUnspecified && value != DateTimeKindUtc && value != DateTimeKindLocal)
			throw new Error("ArgumentException: Invalid DateTimeKind value.");

		return value;
	}

	private static BigInt GetTicks(RuntimeModule.JDateTime instance)
	{
		var date = instance.Date;
		var milliseconds = Date.UTC(
			date.GetFullYear(),
			date.GetMonth(),
			date.GetDate(),
			date.GetHours(),
			date.GetMinutes(),
			date.GetSeconds(),
			date.GetMilliseconds());
		return BigInt_(milliseconds) * TicksPerMillisecond + instance.SubMillisecondTicks + UnixEpochTicks;
	}

	private static BigInt GetTicks(Date date)
	{
		var milliseconds = Date.UTC(
			date.GetFullYear(),
			date.GetMonth(),
			date.GetDate(),
			date.GetHours(),
			date.GetMinutes(),
			date.GetSeconds(),
			date.GetMilliseconds());
		return BigInt_(milliseconds) * TicksPerMillisecond + UnixEpochTicks;
	}

	private static BigInt GetInstantTicks(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindUtc)
			return GetTicks(instance);

		return BigInt_(instance.Date.GetTime()) * TicksPerMillisecond + instance.SubMillisecondTicks + UnixEpochTicks;
	}

	private static RuntimeModule.JDateTime CreateUtcNow()
	{
		var now = new Date();
		return new RuntimeModule.JDateTime(
			new Date(
				now.GetUTCFullYear(),
				now.GetUTCMonth(),
				now.GetUTCDate(),
				now.GetUTCHours(),
				now.GetUTCMinutes(),
				now.GetUTCSeconds(),
				now.GetUTCMilliseconds()),
			DateTimeKindUtc);
	}

	private static bool HasUtcSuffix(string input)
		=> input.EndsWith("Z") || input.EndsWith("z");

	private static bool HasExplicitOffset(string input)
	{
		var timeIndex = input.LastIndexOf('T');
		var spaceIndex = input.LastIndexOf(' ');
		if (spaceIndex > timeIndex)
			timeIndex = spaceIndex;

		if (input.Length >= 6)
		{
			var signIndex = input.Length - 6;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && input[input.Length - 3] == ':' && signIndex > timeIndex)
				return true;
		}

		if (input.Length >= 5)
		{
			var signIndex = input.Length - 5;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && signIndex > timeIndex)
				return true;
		}

		if (input.Length >= 3)
		{
			var signIndex = input.Length - 3;
			var sign = input[signIndex];
			if ((sign == '+' || sign == '-') && signIndex > timeIndex)
				return true;
		}

		return false;
	}

	private static RuntimeModule.JDateTime ParseCore(string input)
	{
		var s = input.Trim();
		if (s.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid DateTime.");

		if (s.Length == 10 && s[4] == '-' && s[7] == '-')
		{
			var year = Number_(s.Substring(0, 4));
			var month = Number_(s.Substring(5, 2));
			var day = Number_(s.Substring(8, 2));
			var date = CreateLocalDate(year, month, day);
			if (date.GetFullYear() != year || date.GetMonth() + 1 != month || date.GetDate() != day)
				throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTime.");

			return new RuntimeModule.JDateTime(date, DateTimeKindUnspecified);
		}

		var parsed = new Date(s);
		if (IsNaN(parsed.GetTime()))
			throw new Error($"FormatException: String '{input}' was not recognized as a valid DateTime.");

		if (HasUtcSuffix(s))
			return CreateFromTicks(BigInt_(parsed.GetTime()) * TicksPerMillisecond + UnixEpochTicks, DateTimeKindUtc);

		if (HasExplicitOffset(s))
			return new RuntimeModule.JDateTime(new Date(parsed.GetTime()), DateTimeKindLocal);

		return new RuntimeModule.JDateTime(parsed, DateTimeKindUnspecified);
	}

	/// <summary>
	/// C#: DateTime.MinValue
	/// JS: wrapper for 0001-01-01 00:00:00
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTime.MinValue")]
	public static RuntimeModule.JDateTime _fad0c74e1c9df5bb() => new(CreateLocalDate(1, 1, 1), DateTimeKindUnspecified);

	/// <summary>
	/// C#: DateTime.MaxValue
	/// JS: wrapper for 9999-12-31 23:59:59.999
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTime.MaxValue")]
	public static RuntimeModule.JDateTime _eb38dc04224730ea() => CreateDateTime(CreateLocalDateTime(9999, 12, 31, 23, 59, 59, 999), DateTimeKindUnspecified, BigInt_("9999"));

	/// <summary>
	/// C#: DateTime.UnixEpoch
	/// JS: new Date(0)
	/// </summary>
	[Jazor(Op.Import, "static readonly System.DateTime.UnixEpoch")]
	public static RuntimeModule.JDateTime _878591efc9a51388() => CreateFromTicks(UnixEpochTicks, DateTimeKindUtc);

	[Jazor(Op.Import ,"System.DateTime.DateTime()")]
	public static RuntimeModule.JDateTime _bfa8ee5dd46e2005() => new(CreateLocalDate(1, 1, 1), DateTimeKindUnspecified);

	/// <summary>
	/// C#: new DateTime(ticks)
	/// JS: new Date(Number((ticks - 621355968000000000n) / 10000n))
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(long)")]
	public static RuntimeModule.JDateTime _1ba9ed95dd0eab48(BigInt ticks)
		=> CreateFromTicks(ticks, DateTimeKindUnspecified);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to a specified number of ticks and to Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(long, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _eda1c8bf8e1e617b(BigInt ticks, object kind)
		=> CreateFromTicks(ticks, GetKind(kind));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />. The new instance will have the <see cref="F:System.DateTimeKind.Unspecified" /> kind.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(System.DateOnly, System.TimeOnly)")]
	public static RuntimeModule.JDateTime _4fef4795bcbef97f(RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time)
	{
		var milliseconds = Number_(time.Ticks / TicksPerMillisecond);
		var subMillisecondTicks = time.Ticks % TicksPerMillisecond;
		var hour = Math.Floor_(milliseconds / 3600000);
		var minute = Math.Floor_(milliseconds / 60000) % 60;
		var second = Math.Floor_(milliseconds / 1000) % 60;
		var millisecond = milliseconds % 1000;
		return CreateDateTime(CreateLocalDateTime(date.Year, date.Month, date.Day, hour, minute, second, millisecond), DateTimeKindUnspecified, subMillisecondTicks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" /> and respecting the specified <see cref="T:System.DateTimeKind" />.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(System.DateOnly, System.TimeOnly, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _85602323793168a5(RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time, object kind)
	{
		var result = _4fef4795bcbef97f(date, time);
		return CreateDateTime(result.Date, GetKind(kind), result.SubMillisecondTicks);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day.</summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(int, int, int)")]
	public static RuntimeModule.JDateTime _4cb33a818161a3e1(Number year, Number month, Number day) => new(CreateLocalDate(year, month, day), DateTimeKindUnspecified);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, and day for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _a515b8bb82ad96b7(Number year, Number month, Number day, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)")]
	public extern static RuntimeModule.JDateTime _bd2c430e6327a2cc(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar, object kind);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second.</summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _4903723bbf8a0a2f(Number year, Number month, Number day, Number hour, Number minute, Number second)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, 0));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _f83be88cfb3fbce0(Number year, Number month, Number day, Number hour, Number minute, Number second, object kind)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, 0), GetKind(kind));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, and second for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _29bb943b21806bd9(Number year, Number month, Number day, Number hour, Number minute, Number second, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond.</summary>
	[Jazor(Op.Import, "System.DateTime.DateTime(int, int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _5822b271bb635d64(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), DateTimeKindUnspecified);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _c52eec5e681a0b8b(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, object kind)
		=> new(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), GetKind(kind));

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, and millisecond for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _8a4d2d51b716bb36(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int)")]
	public static RuntimeModule.JDateTime _9117d26d23769ad1(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond)
		=> CreateDateTime(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), DateTimeKindUnspecified, BigInt_(microsecond) * TicksPerMicrosecond);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Import ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _e84671346e2b9972(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, object kind)
		=> CreateDateTime(CreateLocalDateTime(year, month, day, hour, minute, second, millisecond), GetKind(kind), BigInt_(microsecond) * TicksPerMicrosecond);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)")]
	public extern static RuntimeModule.JDateTime _bd13792ce57e1964(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar);

	///<summary>Initializes a new instance of the <see cref="T:System.DateTime" /> structure to the specified year, month, day, hour, minute, second, millisecond, and Coordinated Universal Time (UTC) or local time for the specified calendar.</summary>
	[Jazor(Op.Discard ,"System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)")]
	public extern static RuntimeModule.JDateTime _cd0b8f2bce1e09ed(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond, Number microsecond, GregorianCalendar calendar, object kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the value of the specified <see cref="T:System.TimeSpan" /> to the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.Add(System.TimeSpan)")]
	public static RuntimeModule.JDateTime _34a77be7365c459f(RuntimeModule.JDateTime instance, RuntimeModule.JTimeSpan value)
		=> CreateFromTicks(GetTicks(instance) + value.Ticks, instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of days to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddDays(double)")]
	public static RuntimeModule.JDateTime _558a3f189d9149d7(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + BigInt_(Math.Round_(value * 864000000000d)), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of hours to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddHours(double)")]
	public static RuntimeModule.JDateTime _101af978213c19c5(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + BigInt_(Math.Round_(value * 36000000000d)), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of milliseconds to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMilliseconds(double)")]
	public static RuntimeModule.JDateTime _2b29e4c11fa12daa(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + BigInt_(Math.Round_(value * 10000d)), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of microseconds to the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.AddMicroseconds(double)")]
	public static RuntimeModule.JDateTime _2b47368c73a3e1f2(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + BigInt_(Math.Round_(value * 10d)), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of minutes to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMinutes(double)")]
	public static RuntimeModule.JDateTime _8bdc25943cf2d39b(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + BigInt_(Math.Round_(value * 600000000d)), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of months to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddMonths(int)")]
	public static RuntimeModule.JDateTime _aae197b95f9024a4(RuntimeModule.JDateTime instance, Number months)
	{
		var result = new Date(instance.Date.GetTime());
		result.SetMonth(result.GetMonth() + months);
		return CreateDateTime(result, instance.Kind, instance.SubMillisecondTicks);
	}

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of seconds to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddSeconds(double)")]
	public static RuntimeModule.JDateTime _57045f93edac1460(RuntimeModule.JDateTime instance, Number value)
		=> CreateFromTicks(GetTicks(instance) + BigInt_(Math.Round_(value * 10000000d)), instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of ticks to the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.AddTicks(long)")]
	public static RuntimeModule.JDateTime _d2e74845b174a889(RuntimeModule.JDateTime instance, BigInt value)
		=> CreateFromTicks(GetTicks(instance) + value, instance.Kind);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that adds the specified number of years to the value of this instance.</summary>
	[Jazor(Op.Import, "System.DateTime.AddYears(int)")]
	public static RuntimeModule.JDateTime _3353d31b02f2bed8(RuntimeModule.JDateTime instance, Number value)
	{
		var result = new Date(instance.Date.GetTime());
		result.SetFullYear(result.GetFullYear() + value);
		return CreateDateTime(result, instance.Kind, instance.SubMillisecondTicks);
	}

	///<summary>Compares two instances of <see cref="T:System.DateTime" /> and returns an integer that indicates whether the first instance is earlier than, the same as, or later than the second instance.</summary>
	[Jazor(Op.Import, "static System.DateTime.Compare(System.DateTime, System.DateTime)")]
	public static Number _0edfd00dcc8d70d0(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
	{
		var ticks1 = GetTicks(t1);
		var ticks2 = GetTicks(t2);
		if (ticks1 < ticks2) return -1;
		if (ticks1 > ticks2) return 1;
		return 0;
	}

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.DateTime" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Import ,"System.DateTime.CompareTo(object)")]
	public static Number _f7b2337bfa9864d9(RuntimeModule.JDateTime instance, object? value)
	{
		var other = value as RuntimeModule.JDateTime;
		if (other == null)
			throw new Error("ArgumentException: Object must be of type DateTime.");

		return _40c6426fdc505e97(instance, other);
	}

	///<summary>Compares the value of this instance to a specified <see cref="T:System.DateTime" /> value and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.DateTime" /> value.</summary>
	[Jazor(Op.Import, "System.DateTime.CompareTo(System.DateTime)")]
	public static Number _40c6426fdc505e97(RuntimeModule.JDateTime instance, RuntimeModule.JDateTime value)
	{
		var ticks = GetTicks(instance);
		var otherTicks = GetTicks(value);
		if (ticks < otherTicks) return -1;
		if (ticks > otherTicks) return 1;
		return 0;
	}

	///<summary>Returns the number of days in the specified month and year.</summary>
	[Jazor(Op.Import, "static System.DateTime.DaysInMonth(int, int)")]
	public static Number _38ef7423971afb7f(Number year, Number month)
	{
		return new Date(year, month, 0).GetDate();
	}

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Import, "override System.DateTime.Equals(object)")]
	public static bool _f6903c1af8944917(RuntimeModule.JDateTime instance, object? value)
	{
		var other = value as RuntimeModule.JDateTime;
		return other != null && GetTicks(instance) == GetTicks(other);
	}

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.DateTime" /> instance.</summary>
	[Jazor(Op.Import, "System.DateTime.Equals(System.DateTime)")]
	public static bool _c29ca32a998c517c(RuntimeModule.JDateTime instance, RuntimeModule.JDateTime value)
		=> GetTicks(instance) == GetTicks(value);

	///<summary>Returns a value indicating whether two <see cref="T:System.DateTime" /> instances  have the same date and time value.</summary>
	[Jazor(Op.Import, "static System.DateTime.Equals(System.DateTime, System.DateTime)")]
	public static bool _4937ff8bec81ddea(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) == GetTicks(t2);

	///<summary>Deserializes a 64-bit binary value and recreates an original serialized <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromBinary(long)")]
	public static RuntimeModule.JDateTime _f437fad61f0046c7(BigInt dateData)
	{
		var kind = Number_(dateData / BinaryKindShift);
		var ticks = dateData % BinaryKindShift;
		return CreateFromTicks(ticks & BinaryTicksMask, kind);
	}

	///<summary>Converts the specified Windows file time to an equivalent local time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromFileTime(long)")]
	public static RuntimeModule.JDateTime _df025c273bde0e50(BigInt fileTime)
		=> CreateFromInstantTicks(fileTime - FileTimeUnixEpochTicks + UnixEpochTicks, DateTimeKindLocal);

	///<summary>Converts the specified Windows file time to an equivalent UTC time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromFileTimeUtc(long)")]
	public static RuntimeModule.JDateTime _93886aebedb72920(BigInt fileTime)
		=> CreateFromTicks(fileTime - FileTimeUnixEpochTicks + UnixEpochTicks, DateTimeKindUtc);

	///<summary>Returns a <see cref="T:System.DateTime" /> equivalent to the specified OLE Automation Date.</summary>
	[Jazor(Op.Import ,"static System.DateTime.FromOADate(double)")]
	public static RuntimeModule.JDateTime _12520a637fb85a70(Number d)
		=> CreateFromTicks(BigInt_(Math.Round_((d - OADateUnixOffsetDays) * MillisecondsPerDay)) * TicksPerMillisecond + UnixEpochTicks, DateTimeKindUnspecified);

	///<summary>Indicates whether this instance of <see cref="T:System.DateTime" /> is within the daylight saving time range for the current time zone.</summary>
	[Jazor(Op.Import ,"System.DateTime.IsDaylightSavingTime()")]
	public static bool _d3b1cc7e750c6bc3(RuntimeModule.JDateTime instance)
	{
		var year = instance.Date.GetFullYear();
		var januaryOffset = new Date(year, 0, 1).GetTimezoneOffset();
		var julyOffset = new Date(year, 6, 1).GetTimezoneOffset();
		var standardOffset = Math.Max(januaryOffset, julyOffset);
		return instance.Date.GetTimezoneOffset() < standardOffset;
	}

	///<summary>Creates a new <see cref="T:System.DateTime" /> object that has the same number of ticks as the specified <see cref="T:System.DateTime" />, but is designated as either local time, Coordinated Universal Time (UTC), or neither, as indicated by the specified <see cref="T:System.DateTimeKind" /> value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.SpecifyKind(System.DateTime, System.DateTimeKind)")]
	public static RuntimeModule.JDateTime _a99826a92073614e(RuntimeModule.JDateTime value, object kind)
		=> CreateDateTime(value.Date, GetKind(kind), value.SubMillisecondTicks);

	///<summary>Serializes the current <see cref="T:System.DateTime" /> object to a 64-bit binary value that subsequently can be used to recreate the <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToBinary()")]
	public static BigInt _9cea54115c704cf7(RuntimeModule.JDateTime instance)
		=> GetTicks(instance) + BigInt_(instance.Kind) * BinaryKindShift;

	[Jazor(Op.Import, "System.DateTime.Date.get")]
	public static RuntimeModule.JDateTime _d77d20d9d04e2b6b(RuntimeModule.JDateTime instance)
		=> CreateDateTime(CreateLocalDate(instance.Date.GetFullYear(), instance.Date.GetMonth() + 1, instance.Date.GetDate()), instance.Kind);

	[Jazor(Op.Import, "System.DateTime.Day.get")]
	public static Number _3b9ecf5fd3c301db(RuntimeModule.JDateTime instance)
		=> instance.Date.GetDate();

	[Jazor(Op.Import, "System.DateTime.DayOfWeek.get")]
	public static System.DayOfWeek _6070f1709c491634(RuntimeModule.JDateTime instance)
		=> instance.Date.GetDay();

	/// <summary>
	/// C#: DateTime.DayOfYear
	/// JS: 计算一年中的第几天
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.DayOfYear.get")]
	public static Number _4f6ca20bf1aaa2d3(RuntimeModule.JDateTime instance)
	{
		var start = new Date(instance.Date.GetFullYear(), 0, 0);
		var diff = instance.Date.GetTime() - start.GetTime();
		var oneDay = 1000 * 60 * 60 * 24;
		return Math.Floor_(diff / oneDay);
	}

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import ,"override System.DateTime.GetHashCode()")]
	public static Number _d3529b55e30e2a12(RuntimeModule.JDateTime instance)
		=> Number_(GetTicks(instance) % BigInt_("2147483647"));

	[Jazor(Op.Import, "System.DateTime.Hour.get")]
	public static Number _f263cff61e6628a9(RuntimeModule.JDateTime instance)
		=> instance.Date.GetHours();

	[Jazor(Op.Import ,"System.DateTime.Kind.get")]
	public static System.DateTimeKind _551add245db0b701(RuntimeModule.JDateTime instance)
		=> instance.Kind;

	[Jazor(Op.Import, "System.DateTime.Millisecond.get")]
	public static Number _742a8bcf918b97e6(RuntimeModule.JDateTime instance)
		=> instance.Date.GetMilliseconds();

	[Jazor(Op.Import ,"System.DateTime.Microsecond.get")]
	public static Number _34d05014c270366f(RuntimeModule.JDateTime instance)
		=> Number_((instance.SubMillisecondTicks / TicksPerMicrosecond) % BigInt_(1000));

	[Jazor(Op.Import ,"System.DateTime.Nanosecond.get")]
	public static Number _46e11fe2eb2ee869(RuntimeModule.JDateTime instance)
		=> Number_((instance.SubMillisecondTicks % TicksPerMicrosecond) * BigInt_(100));

	[Jazor(Op.Import, "System.DateTime.Minute.get")]
	public static Number _f4ca5de4f63aa097(RuntimeModule.JDateTime instance)
		=> instance.Date.GetMinutes();

	[Jazor(Op.Import, "System.DateTime.Month.get")]
	public static Number _a8a6b6e36a0ea736(RuntimeModule.JDateTime instance)
		=> instance.Date.GetMonth() + 1;

	[Jazor(Op.Import, "static System.DateTime.Now.get")]
	public static RuntimeModule.JDateTime _ee9dd166a34a2fa5() => new(new Date(), DateTimeKindLocal);

	[Jazor(Op.Import, "System.DateTime.Second.get")]
	public static Number _10a94eacb3b7fd2d(RuntimeModule.JDateTime instance)
		=> instance.Date.GetSeconds();

	/// <summary>
	/// C#: DateTime.Ticks
	/// JS: instance.getTime() * 10000 + 621355968000000000 (从公元1年1月1日开始的ticks)
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.Ticks.get")]
	public static BigInt _bcde32e170f49354(RuntimeModule.JDateTime instance)
		=> GetTicks(instance);

	/// <summary>
	/// C#: DateTime.TimeOfDay
	/// JS: 返回自午夜以来的时间（ticks）
	/// </summary>
	[Jazor(Op.Import, "System.DateTime.TimeOfDay.get")]
	public static RuntimeModule.JTimeSpan _2efdc237be2f31aa(RuntimeModule.JDateTime instance)
	{
		var ms = (((instance.Date.GetHours() * 60 + instance.Date.GetMinutes()) * 60 + instance.Date.GetSeconds()) * 1000) + instance.Date.GetMilliseconds();
		return new RuntimeModule.JTimeSpan(BigInt_(ms) * TicksPerMillisecond + instance.SubMillisecondTicks);
	}

	[Jazor(Op.Import, "static System.DateTime.Today.get")]
	public static RuntimeModule.JDateTime _4b250155b7c688bb()
	{
		var now = new Date();
		return new RuntimeModule.JDateTime(CreateLocalDate(now.GetFullYear(), now.GetMonth() + 1, now.GetDate()), DateTimeKindLocal);
	}

	[Jazor(Op.Import, "System.DateTime.Year.get")]
	public static Number _9d56b09432f81c05(RuntimeModule.JDateTime instance)
		=> instance.Date.GetFullYear();

	///<summary>Returns an indication whether the specified year is a leap year.</summary>
	[Jazor(Op.Import, "static System.DateTime.IsLeapYear(int)")]
	public static bool _4a9da83e9cb28c1a(Number year)
	{
		return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
	}

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Import, "static System.DateTime.Parse(string)")]
	public static RuntimeModule.JDateTime _a8a015c2d2bff2f6(string s)
		=> ParseCore(s);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(string, System.IFormatProvider)")]
	public static RuntimeModule.JDateTime _e0128ef45cc8584e(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Converts the string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTime _7372e5e0d8ba24a6(string s, Intl.NumberFormat? provider, object styles)
		=> ParseCore(s);

	///<summary>Converts a memory span that contains string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JDateTime _2c85f5b20ae7559e(string s, Intl.NumberFormat? provider, object styles)
		=> ParseCore(s);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string, System.IFormatProvider)")]
	public extern static RuntimeModule.JDateTime _7f3dce20074d610f(string s, string format, Intl.NumberFormat? provider);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _75cd4a49bd890e13(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _da7c1ef7b418c87d(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _f47f23f5482d6f56(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JDateTime _32afd1b56d3b1c77(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Returns a new <see cref="T:System.TimeSpan" /> that subtracts the specified date and time from the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.Subtract(System.DateTime)")]
	public static RuntimeModule.JTimeSpan _4f5d235cac779f38(RuntimeModule.JDateTime instance, RuntimeModule.JDateTime value)
		=> _85b6d162b092ce0e(instance, value);

	///<summary>Returns a new <see cref="T:System.DateTime" /> that subtracts the specified duration from the value of this instance.</summary>
	[Jazor(Op.Import ,"System.DateTime.Subtract(System.TimeSpan)")]
	public static RuntimeModule.JDateTime _20a406afebff2025(RuntimeModule.JDateTime instance, RuntimeModule.JTimeSpan value)
		=> _8d9ea66839ce392a(instance, value);

	///<summary>Converts the value of this instance to the equivalent OLE Automation date.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToOADate()")]
	public static Number _fb61bb2ccf4b10b6(RuntimeModule.JDateTime instance)
		=> Number_(GetTicks(instance) - UnixEpochTicks) / 10000d / MillisecondsPerDay + OADateUnixOffsetDays;

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToFileTime()")]
	public static BigInt _37ee48ca629793fa(RuntimeModule.JDateTime instance)
		=> GetInstantTicks(instance) - UnixEpochTicks + FileTimeUnixEpochTicks;

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to a Windows file time.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToFileTimeUtc()")]
	public static BigInt _c02c49ea68661175(RuntimeModule.JDateTime instance)
		=> _37ee48ca629793fa(instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to local time.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToLocalTime()")]
	public static RuntimeModule.JDateTime _db842725d5fd1ca0(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindLocal)
			return CreateDateTime(instance.Date, DateTimeKindLocal, instance.SubMillisecondTicks);

		return CreateFromInstantTicks(GetInstantTicks(instance), DateTimeKindLocal);
	}

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long date string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToLongDateString()")]
	public static string _6e78dc03eecdd423(RuntimeModule.JDateTime instance)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent long time string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToLongTimeString()")]
	public static string _ab161bb1563732af(RuntimeModule.JDateTime instance)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short date string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToShortDateString()")]
	public static string _6a67d54f5c865e5e(RuntimeModule.JDateTime instance)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent short time string representation.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToShortTimeString()")]
	public static string _af2d02ec0c0a300d(RuntimeModule.JDateTime instance)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the formatting conventions of the current culture.</summary>
	[Jazor(Op.Alias, "override System.DateTime.ToString()", "toString")]
	public extern static string _6659b3b5d1f081dd(RuntimeModule.JDateTime instance);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToString(string)")]
	public static string _3ee3e9478fe9a1fb(RuntimeModule.JDateTime instance, string? format)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToString(System.IFormatProvider)")]
	public static string _606066f0ee1488c6(RuntimeModule.JDateTime instance, Intl.NumberFormat? provider)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.DateTime.ToString(string, System.IFormatProvider)")]
	public static string _85393faf5839b9ef(RuntimeModule.JDateTime instance, string? format, Intl.NumberFormat? provider)
		=> instance.ToString();

	///<summary>Tries to format the value of the current datetime instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.DateTime.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _b50913efa5ca8082(RuntimeModule.JDateTime instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.DateTime.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _100d184b5413769d(RuntimeModule.JDateTime instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Converts the value of the current <see cref="T:System.DateTime" /> object to Coordinated Universal Time (UTC).</summary>
	[Jazor(Op.Import ,"System.DateTime.ToUniversalTime()")]
	public static RuntimeModule.JDateTime _b62871088df3ca8f(RuntimeModule.JDateTime instance)
	{
		if (instance.Kind == DateTimeKindUtc)
			return CreateDateTime(instance.Date, DateTimeKindUtc, instance.SubMillisecondTicks);

		return CreateFromInstantTicks(GetInstantTicks(instance), DateTimeKindUtc);
	}

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import, "static System.DateTime.TryParse(string, out System.DateTime)")]
	public static Array<object?> _fa25ca318f086bb6(string? s, RuntimeModule.JDateTime result)
	{
		if (s == null || s.Length == 0)
			return [false, new RuntimeModule.JDateTime(new Date(0), DateTimeKindUnspecified)];
		try
		{
			return [true, ParseCore(s)];
		}
		catch
		{
			return [false, new RuntimeModule.JDateTime(new Date(0), DateTimeKindUnspecified)];
		}
	}

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, out System.DateTime)")]
	public static Array<object?> _8658c3be6edb9d2c(string s, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public static Array<object?> _34043b1eb3a8183a(string? s, Intl.NumberFormat? provider, object styles, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	///<summary>Converts the span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified culture-specific format information and formatting style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public static Array<object?> _6e8546b461b48646(string s, Intl.NumberFormat? provider, object styles, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _79e29a1615b41471(string? s, string? format, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Converts the specified span representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format, culture-specific format information, and style. The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _d8720f2bb55cf0af(string s, string format, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified array of formats, culture-specific format information, and style. The format of the string representation must match at least one of the specified formats exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _d1eb33e53764ee27(string? s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Converts the specified char span of a date and time to its <see cref="T:System.DateTime" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.DateTime.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.DateTime)")]
	public extern static Array<object?> _685c4ca5481f00e7(string s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JDateTime result);

	///<summary>Adds a specified time interval to a specified date and time, yielding a new date and time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator +(System.DateTime, System.TimeSpan)")]
	public static RuntimeModule.JDateTime _d48b23d7c5f7c2aa(RuntimeModule.JDateTime d, RuntimeModule.JTimeSpan t)
		=> CreateFromTicks(GetTicks(d) + t.Ticks, d.Kind);

	///<summary>Subtracts a specified time interval from a specified date and time and returns a new date and time.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator -(System.DateTime, System.TimeSpan)")]
	public static RuntimeModule.JDateTime _8d9ea66839ce392a(RuntimeModule.JDateTime d, RuntimeModule.JTimeSpan t)
		=> CreateFromTicks(GetTicks(d) - t.Ticks, d.Kind);

	///<summary>Subtracts a specified date and time from another specified date and time and returns a time interval.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator -(System.DateTime, System.DateTime)")]
	public static RuntimeModule.JTimeSpan _85b6d162b092ce0e(RuntimeModule.JDateTime d1, RuntimeModule.JDateTime d2)
		=> new(GetTicks(d1) - GetTicks(d2));

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are equal.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator ==(System.DateTime, System.DateTime)")]
	public static bool _37d87f65292f7083(RuntimeModule.JDateTime d1, RuntimeModule.JDateTime d2)
		=> GetTicks(d1) == GetTicks(d2);

	///<summary>Determines whether two specified instances of <see cref="T:System.DateTime" /> are not equal.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator !=(System.DateTime, System.DateTime)")]
	public static bool _89406f797d33e566(RuntimeModule.JDateTime d1, RuntimeModule.JDateTime d2)
		=> GetTicks(d1) != GetTicks(d2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator <(System.DateTime, System.DateTime)")]
	public static bool _5a97e2aec50193b3(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) < GetTicks(t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator <=(System.DateTime, System.DateTime)")]
	public static bool _a8b15168323b118c(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) <= GetTicks(t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator >(System.DateTime, System.DateTime)")]
	public static bool _e98b0598f4980bcc(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) > GetTicks(t2);

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref> represents a date and time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.DateTime"></xref>.</summary>
	[Jazor(Op.Import ,"static System.DateTime.operator >=(System.DateTime, System.DateTime)")]
	public static bool _91697ebd6031bb97(RuntimeModule.JDateTime t1, RuntimeModule.JDateTime t2)
		=> GetTicks(t1) >= GetTicks(t2);

	///<summary>Deconstructs this <see cref="T:System.DateTime" /> instance by <see cref="T:System.DateOnly" /> and <see cref="T:System.TimeOnly" />.</summary>
	[Jazor(Op.Import ,"System.DateTime.Deconstruct(out System.DateOnly, out System.TimeOnly)")]
	public static Array<object?> _bcf4183bef96ea21(RuntimeModule.JDateTime instance, RuntimeModule.JDateOnly date, RuntimeModule.JTimeOnly time)
		=> [new RuntimeModule.JDateOnly(instance.Date.GetFullYear(), instance.Date.GetMonth() + 1, instance.Date.GetDate()), TimeOnlyModule._a305982aa6859677(instance)];

	///<summary>Deconstructs this <see cref="T:System.DateOnly" /> instance by <see cref="P:System.DateTime.Year" />, <see cref="P:System.DateTime.Month" />, and <see cref="P:System.DateTime.Day" />.</summary>
	[Jazor(Op.Import ,"System.DateTime.Deconstruct(out int, out int, out int)")]
	public static Array<object?> _5f721827cf6b8105(RuntimeModule.JDateTime instance, Number year, Number month, Number day)
		=> [instance.Date.GetFullYear(), instance.Date.GetMonth() + 1, instance.Date.GetDate()];

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats()")]
	public extern static string[] _8022abe7c2a9b946(RuntimeModule.JDateTime instance);

	///<summary>Converts the value of this instance to all the string representations supported by the standard date and time format specifiers and the specified culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(System.IFormatProvider)")]
	public extern static string[] _65e4d2ad2f14918c(RuntimeModule.JDateTime instance, Intl.NumberFormat? provider);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(char)")]
	public extern static string[] _daa9858a8adf981d(RuntimeModule.JDateTime instance, Number format);

	///<summary>Converts the value of this instance to all the string representations supported by the specified standard date and time format specifier and culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetDateTimeFormats(char, System.IFormatProvider)")]
	public extern static string[] _10c081a451aa4b71(RuntimeModule.JDateTime instance, Number format, Intl.NumberFormat? provider);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.DateTime" />.</summary>
	[Jazor(Op.Discard ,"System.DateTime.GetTypeCode()")]
	public extern static System.TypeCode _9164c7979da236d5(RuntimeModule.JDateTime instance);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(string, System.IFormatProvider, out System.DateTime)")]
	public static Array<object?> _6c36c46db30aacc1(string? s, Intl.NumberFormat? provider, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static RuntimeModule.JDateTime _41dcf008ea7cf6d9(string s, Intl.NumberFormat? provider)
		=> _a8a015c2d2bff2f6(s);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.DateTime.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.DateTime)")]
	public static Array<object?> _63fd53f09ba16132(string s, Intl.NumberFormat? provider, RuntimeModule.JDateTime result)
		=> _fa25ca318f086bb6(s, result);

	[Jazor(Op.Import, "static System.DateTime.UtcNow.get")]
	public static RuntimeModule.JDateTime _d4c39bdf47f391cf() => CreateUtcNow();
}
