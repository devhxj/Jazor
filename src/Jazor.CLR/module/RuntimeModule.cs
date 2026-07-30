namespace Jazor.CLR;

[ECMAScriptModule("System/RuntimeModule.js")]
public static class RuntimeModule
{
	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
			throw new Error(message);
	}

	private static void EnsureYearAndMonth(Number year, Number month)
	{
		EnsureWholeNumber(year, "ArgumentOutOfRangeException: Year must be a whole number between 1 and 9999.");
		EnsureWholeNumber(month, "ArgumentOutOfRangeException: Month must be a whole number between 1 and 12.");
		if (year < 1 || year > 9999)
			throw new Error("ArgumentOutOfRangeException: Year must be between 1 and 9999.");
		if (month < 1 || month > 12)
			throw new Error("ArgumentOutOfRangeException: Month must be between 1 and 12.");
	}

	private static Array<TItem> MaterializeArray<TItem>(IEnumerable<TItem>? collection, string nullMessage)
	{
		if (collection is null)
			throw new Error(nullMessage);

		var result = new Array<TItem>();
		foreach (var item in collection)
			result.Push(item);

		return result;
	}

	private const string ReadOnlyCarrierMutationMessage = "NotSupportedException: Collection is read-only.";
	private static readonly WeakSet ReadOnlyCarriers = new();

	internal static bool IsReadOnlySetCarrier<T>(Set<T> instance)
		=> instance != null && ReadOnlyCarriers.Has(instance);

	private static Set<T> ThrowReadOnlySetAdd<T>(T item)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlySetDelete<T>(T item)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static void ThrowReadOnlySetClear<T>()
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	internal static Set<T> MarkAsReadOnlySetCarrier<T>(Set<T> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		if (IsReadOnlySetCarrier(instance))
			return instance;

		ReadOnlyCarriers.Add(instance);

		// Set cannot be made read-only via Object.freeze; override mutators on the carrier.
		Object.DefineProperty(instance, "add", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<T, Set<T>>)ThrowReadOnlySetAdd<T>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "delete", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<T, bool>)ThrowReadOnlySetDelete<T>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "clear", new ECMAScript.PropertyDescriptor
		{
			Value = (Action)ThrowReadOnlySetClear<T>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		return instance;
	}

	internal static bool IsReadOnlyDictionaryCarrier<TKey, TValue>(Map<TKey, TValue> instance)
		=> instance != null && ReadOnlyCarriers.Has(instance);

	private static Map<TKey, TValue> ThrowReadOnlyDictionarySet<TKey, TValue>(TKey key, TValue value)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlyDictionaryDelete<TKey, TValue>(TKey key)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static void ThrowReadOnlyDictionaryClear<TKey, TValue>()
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	internal static Map<TKey, TValue> MarkAsReadOnlyDictionaryCarrier<TKey, TValue>(Map<TKey, TValue> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		if (IsReadOnlyDictionaryCarrier(instance))
			return instance;

		ReadOnlyCarriers.Add(instance);

		// Map cannot be made read-only via Object.freeze; override mutators on the carrier.
		Object.DefineProperty(instance, "set", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, TValue, Map<TKey, TValue>>)ThrowReadOnlyDictionarySet<TKey, TValue>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "delete", new ECMAScript.PropertyDescriptor
		{
			Value = (Func<TKey, bool>)ThrowReadOnlyDictionaryDelete<TKey, TValue>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		Object.DefineProperty(instance, "clear", new ECMAScript.PropertyDescriptor
		{
			Value = (Action)ThrowReadOnlyDictionaryClear<TKey, TValue>,
			Enumerable = false,
			Writable = false,
			Configurable = false
		});
		return instance;
	}

	public sealed class JDateTime
	{
		[Description("@#date")]
		public Date Date { get; }

		[Description("@#kind")]
		public Number Kind { get; }

		[Description("@#subMillisecondTicks")]
		public BigInt SubMillisecondTicks { get; }

		public JDateTime(Date date)
		{
			this.Date = new Date(date.GetTime());
			this.Kind = 0;
			this.SubMillisecondTicks = BigInt.Zero;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		public JDateTime(Date date, Number kind)
		{
			this.Date = new Date(date.GetTime());
			this.Kind = kind;
			this.SubMillisecondTicks = BigInt.Zero;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		public JDateTime(Date date, Number kind, BigInt subMillisecondTicks)
		{
			this.Date = new Date(date.GetTime());
			this.Kind = kind;
			this.SubMillisecondTicks = subMillisecondTicks;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
		{
			return FormatDateOnlyText(Date.GetFullYear(), Date.GetMonth() + 1, Date.GetDate())
				+ "T"
				+ Pad2(Date.GetHours())
				+ ":"
				+ Pad2(Date.GetMinutes())
				+ ":"
				+ Pad2(Date.GetSeconds())
				+ "."
				+ Pad7(BigIntFn(Date.GetMilliseconds()) * BigIntFn(10000) + SubMillisecondTicks);
		}

		[Description("@#valueOf")]
		public Number ValueOf()
			=> Date.UTC(
				Date.GetFullYear(),
				Date.GetMonth(),
				Date.GetDate(),
				Date.GetHours(),
				Date.GetMinutes(),
				Date.GetSeconds(),
				Date.GetMilliseconds());

		[Description("@#toPrimitive")]
		public object ToPrimitive(string? hint)
		{
			// default hint 也走字符串分支，避免 JS 的 `"" + value` 把日期包装对象错误压成数值。
			if (hint == "number")
				return ValueOf();

			return ToString();
		}
	}

	public sealed class JDateTimeOffset
	{
		[Description("@#utcDateTime")]
		public Date UtcDateTime { get; }

		[Description("@#offsetTicks")]
		public BigInt OffsetTicks { get; }

		[Description("@#utcSubMillisecondTicks")]
		public BigInt UtcSubMillisecondTicks { get; }

		public JDateTimeOffset(Date utcDateTime, BigInt offsetTicks)
		{
			this.UtcDateTime = new Date(utcDateTime.GetTime());
			this.OffsetTicks = offsetTicks;
			this.UtcSubMillisecondTicks = BigInt.Zero;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		public JDateTimeOffset(Date utcDateTime, BigInt offsetTicks, BigInt utcSubMillisecondTicks)
		{
			this.UtcDateTime = new Date(utcDateTime.GetTime());
			this.OffsetTicks = offsetTicks;
			this.UtcSubMillisecondTicks = utcSubMillisecondTicks;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
		{
			var local = new Date(UtcDateTime.GetTime() + NumberFn(OffsetTicks) / 10000);

			var negative = OffsetTicks < BigInt.Zero;
			var absolute = negative ? -OffsetTicks : OffsetTicks;
			var totalMinutes = absolute / BigIntFn(600000000);
			var hours = NumberFn(totalMinutes / BigIntFn(60));
			var minutes = NumberFn(totalMinutes % BigIntFn(60));
			var offset = (negative ? "-" : "+") + Pad2(hours) + ":" + Pad2(minutes);

			return FormatDateOnlyText(local.GetUTCFullYear(), local.GetUTCMonth() + 1, local.GetUTCDate())
				+ "T"
				+ Pad2(local.GetUTCHours())
				+ ":"
				+ Pad2(local.GetUTCMinutes())
				+ ":"
				+ Pad2(local.GetUTCSeconds())
				+ "."
				+ Pad7(BigIntFn(local.GetUTCMilliseconds()) * BigIntFn(10000) + UtcSubMillisecondTicks)
				+ offset;
		}

		[Description("@#valueOf")]
		public Number ValueOf() => UtcDateTime.GetTime();

		[Description("@#toPrimitive")]
		public object ToPrimitive(string? hint)
		{
			// default hint 也走字符串分支，避免 JS 的 `"" + value` 把日期包装对象错误压成数值。
			if (hint == "number")
				return ValueOf();

			return ToString();
		}
	}

	public sealed class JDateOnly
	{
		[Description("@#year")]
		public Number Year { get; }

		[Description("@#month")]
		public Number Month { get; }

		[Description("@#day")]
		public Number Day { get; }

		[Description("@#dayNumber")]
		public Number DayNumber { get; }

		public JDateOnly(Number year, Number month, Number day)
		{
			this.Year = year;
			this.Month = month;
			this.Day = day;
			var utcDate = CreateUtcDate(year, month, day);
			var start = CreateUtcDate(1, 1, 1);
			DayNumber = Math.FloorFn((utcDate.GetTime() - start.GetTime()) / 86400000);
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString() => FormatDateOnlyText(Year, Month, Day);

		[Description("@#valueOf")]
		public Number ValueOf() => DayNumber;

		[Description("@#toPrimitive")]
		public object ToPrimitive(string? hint)
		{
			if (hint == "number")
				return ValueOf();

			return ToString();
		}
	}

	public sealed class JQueue<T>
	{
		[Description("@#items")]
		public Array<T> Items { get; }

		[Description("@#head")]
		public Number Head { get; }

		public JQueue()
		{
			this.Items = new Array<T>();
			this.Head = 0;
		}

		public static JQueue<T> WithCapacity(Number capacity)
		{
			EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
			if (capacity < 0)
				throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");

			return new JQueue<T>();
		}

		public JQueue(IEnumerable<T> collection)
		{
			this.Items = MaterializeArray(collection, "ArgumentNullException: collection cannot be null.");
			this.Head = 0;
		}
	}

	public sealed class JStack<T>
	{
		[Description("@#items")]
		public Array<T> Items { get; }

		public JStack()
		{
			this.Items = [];
		}

		public static JStack<T> WithCapacity(Number capacity)
		{
			EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
			if (capacity < 0)
				throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");

			return new JStack<T>();
		}

		public JStack(IEnumerable<T> collection)
		{
			this.Items = MaterializeArray(collection, "ArgumentNullException: collection cannot be null.");
		}
	}

	public sealed class JTimeOnly
	{
		[Description("@#ticks")]
		public BigInt Ticks { get; }

		public JTimeOnly(BigInt ticks)
		{
			var normalized = ticks % BigIntFn("864000000000");
			this.Ticks = normalized < BigInt.Zero ? normalized + BigIntFn("864000000000") : normalized;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
		{
			var hour = NumberFn(Ticks / BigIntFn("36000000000"));
			var minute = NumberFn((Ticks / BigIntFn(600000000)) % BigIntFn(60));
			var second = NumberFn((Ticks / BigIntFn(10000000)) % BigIntFn(60));
			var fraction = Ticks % BigIntFn(10000000);

			return Pad2(hour)
				+ ":"
				+ Pad2(minute)
				+ ":"
				+ Pad2(second)
				+ "."
				+ Pad7(fraction);
		}

		[Description("@#valueOf")]
		public BigInt ValueOf() => Ticks;

		[Description("@#toPrimitive")]
		public object ToPrimitive(string? hint)
		{
			if (hint == "number")
				return ValueOf();

			return ToString();
		}
	}

	public sealed class JTimeSpan
	{
		[Description("@#ticks")]
		public BigInt Ticks { get; }

		public JTimeSpan(BigInt ticks)
		{
			if (ticks < BigIntFn("-9223372036854775808") || ticks > BigIntFn("9223372036854775807"))
				throw new Error("OverflowException: TimeSpan is too long or too short.");

			this.Ticks = ticks;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
		{
			var negative = Ticks < BigInt.Zero;
			var absolute = negative ? -Ticks : Ticks;
			var days = absolute / BigIntFn("864000000000");
			var hours = NumberFn((absolute / BigIntFn("36000000000")) % BigIntFn(24));
			var minutes = NumberFn((absolute / BigIntFn(600000000)) % BigIntFn(60));
			var seconds = NumberFn((absolute / BigIntFn(10000000)) % BigIntFn(60));
			var fraction = absolute % BigIntFn(10000000);

			var text = (negative ? "-" : "")
				+ (days > BigInt.Zero ? days.ToString() + "." : "")
				+ Pad2(hours)
				+ ":"
				+ Pad2(minutes)
				+ ":"
				+ Pad2(seconds);

			if (fraction != BigInt.Zero)
				text += "." + Pad7(fraction);

			return text;
		}

		[Description("@#valueOf")]
		public BigInt ValueOf() => Ticks;

		[Description("@#toPrimitive")]
		public object ToPrimitive(string? hint)
		{
			if (hint == "number")
				return ValueOf();

			return ToString();
		}
	}

	public sealed class JGregorianCalendar
	{
		[Description("@#calendarType")]
		public Number CalendarType { get; set; }

		[Description("@#twoDigitYearMax")]
		public Number TwoDigitYearMax { get; set; }

		public JGregorianCalendar(Number calendarType, Number twoDigitYearMax)
		{
			this.CalendarType = calendarType;
			this.TwoDigitYearMax = twoDigitYearMax;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
			=> "System.Globalization.GregorianCalendar";

		[Description("@#valueOf")]
		public string ValueOf()
			=> ToString();

		[Description("@#toPrimitive")]
		public object ToPrimitive(string? hint)
			=> ToString();
	}

	public static Number GetDaysInMonth(Number year, Number month)
	{
		EnsureYearAndMonth(year, month);
		var probe = new Date(0);
		probe.SetUTCHours(0, 0, 0, 0);
		probe.SetUTCFullYear(year, month, 0);
		return probe.GetUTCDate();
	}

	public static Number GetInt64HashCode(BigInt value)
	{
		var low = (int)NumberFn(BigInt.AsIntN(32, value));
		var high = (int)NumberFn(BigInt.AsIntN(32, value >> BigIntFn(32)));
		return low ^ high;
	}

	private static void EnsureValidDateParts(Number year, Number month, Number day)
	{
		EnsureYearAndMonth(year, month);
		EnsureWholeNumber(day, "ArgumentOutOfRangeException: Day must be a whole number.");
		if (day < 1 || day > GetDaysInMonth(year, month))
			throw new Error("ArgumentOutOfRangeException: The supplied year, month, or day is out of range.");
	}

	private static void EnsureValidDateTimeParts(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
	{
		EnsureValidDateParts(year, month, day);
		EnsureWholeNumber(hour, "ArgumentOutOfRangeException: Hour must be a whole number.");
		EnsureWholeNumber(minute, "ArgumentOutOfRangeException: Minute must be a whole number.");
		EnsureWholeNumber(second, "ArgumentOutOfRangeException: Second must be a whole number.");
		EnsureWholeNumber(millisecond, "ArgumentOutOfRangeException: Millisecond must be a whole number.");
		if (hour < 0 || hour > 23
			|| minute < 0 || minute > 59
			|| second < 0 || second > 59
			|| millisecond < 0 || millisecond > 999)
			throw new Error("ArgumentOutOfRangeException: The supplied date or time component is out of range.");
	}

	public static Date CreateUtcDate(Number year, Number month, Number day)
	{
		EnsureValidDateParts(year, month, day);
		var result = new Date(0);
		result.SetUTCHours(0, 0, 0, 0);
		result.SetUTCFullYear(year, month - 1, day);
		return result;
	}

	public static Date CreateLocalDate(Number year, Number month, Number day)
	{
		EnsureValidDateParts(year, month, day);
		var result = new Date(0);
		result.SetHours(0, 0, 0, 0);
		result.SetFullYear(year, month - 1, day);
		return result;
	}

	public static Date CreateLocalDateTime(Number year, Number month, Number day, Number hour, Number minute, Number second, Number millisecond)
	{
		EnsureValidDateTimeParts(year, month, day, hour, minute, second, millisecond);
		var result = CreateLocalDate(year, month, day);
		result.SetHours(hour, minute, second, millisecond);
		return result;
	}

	public static string FormatDateOnlyText(Number year, Number month, Number day)
	{
		return PadLeft(year.ToString()!, 4) + "-" + Pad2(month) + "-" + Pad2(day);
	}

	public static string Pad2(Number value) => PadLeft(value.ToString()!, 2);

	public static string Pad7(BigInt value) => PadLeft(value.ToString()!, 7);

	public static string PadLeft(string text, int width)
	{
		while (text.Length < width)
			text = "0" + text;

		return text;
	}
}
