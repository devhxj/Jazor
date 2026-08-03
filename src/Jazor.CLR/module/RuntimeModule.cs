namespace Jazor.CLR;

/// <summary>
/// Jazor.CLR 共用的 JavaScript runtime helper 模块。
/// </summary>
/// <remarks>
/// 这里承载多个 CLR 类型模块共同依赖的日期、时间、队列和基础校验逻辑，并以真实 C# helper
/// 的形式编译为 JavaScript。它不是完整 CLR runtime；只实现当前白名单 API 所需的最小语义闭环。
/// 修改内部类型或字段时要注意它们可能被多个模块通过 Import 间接引用。
/// </remarks>
[ECMAScriptModule("System/RuntimeModule.js")]
public static class RuntimeModule
{
	/// <summary>
	/// ReadOnlySpan&lt;char&gt; 的已支持来源保持原生 string/Array 表示；该 union 只提供
	/// adapter 侧的强类型闭合输入面，转译后不创建包装对象。
	/// </summary>
	[ECMAScript]
	public readonly union JReadOnlyCharSpan(string, Array<string>);

	internal static string MaterializeReadOnlyCharSpan(JReadOnlyCharSpan value)
	{
		var raw = value.Value;
		if (raw == null)
			return "";
		var text = raw as string;
		if (text != null)
			return text;

		var characters = (Array<string>)raw;
		var result = "";
		for (var index = 0; index < characters.Length; index++)
			result += characters[index];
		return result;
	}

	/// <summary>
	/// Strictly decodes a UTF-8 parsing input without erasing malformed bytes or a leading BOM.
	/// </summary>
	/// <remarks>
	/// Numeric UTF-8 parsers are failure-returning APIs. TextDecoder's replacement mode could turn
	/// malformed input into different text, while its default BOM handling could silently accept a
	/// prefix that CLR numeric parsers reject. Keep that boundary centralized for every numeric type.
	/// </remarks>
	internal static string? TryDecodeUtf8(Uint8Array utf8Text)
	{
		// TextDecoder consumes a UTF-8 BOM as transport metadata. CLR's UTF-8 numeric span
		// parsers receive raw characters instead, so the same bytes must remain invalid input.
		if (utf8Text.Length >= 3 && utf8Text[0] == 0xef && utf8Text[1] == 0xbb && utf8Text[2] == 0xbf)
			return null;

		try
		{
			var decoder = new TextDecoder(
				"utf-8",
				new TextDecoderOptions(Fatal: true, IgnoreBOM: true));
			return decoder.Decode(new Uint8Array(utf8Text));
		}
		catch
		{
			return null;
		}
	}

	internal static string DecodeUtf8OrThrowFormat(Uint8Array utf8Text)
	{
		var text = TryDecodeUtf8(utf8Text);
		if (text == null)
			throw new Error("FormatException: The UTF-8 input was not in a correct format.");
		return text;
	}

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

	/// <summary>
	/// DateTime 的 runtime carrier，以 tick 和 kind 保存可跨模块传递的日期值。
	/// </summary>
	/// <remarks>它不是原生 JavaScript Date；需要 Date 互操作时必须经过明确的转换 helper。</remarks>
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

	/// <summary>
	/// DateTimeOffset 的 runtime carrier，同时保存 UTC tick 和分钟级 offset。
	/// </summary>
	/// <remarks>保留 offset 是该类型区别于 DateTime/JavaScript Date 的关键，不能只保存时间戳。</remarks>
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

	/// <summary>
	/// DateOnly 的 runtime carrier，只保存日期 day number。
	/// </summary>
	/// <remarks>该 carrier 没有时间和时区字段，避免被 JavaScript Date 的时区解释污染。</remarks>
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

	/// <summary>
	/// System.Index 的 runtime carrier，保留从起点/终点计数的值语义。
	/// </summary>
	/// <remarks>它只承载已绑定的 Index API；实际 collection 边界仍由消费它的索引器负责。</remarks>
	public sealed class JIndex
	{
		[Description("@#value")]
		public Number Value { get; }

		[Description("@#fromEnd")]
		public bool IsFromEnd { get; }

		public JIndex(Number value, bool fromEnd)
		{
			EnsureWholeNumber(value, "ArgumentOutOfRangeException: Index value must be a non-negative whole number.");
			if (value < 0)
				throw new Error("ArgumentOutOfRangeException: Index value must be non-negative.");

			Value = value;
			IsFromEnd = fromEnd;
		}

		public Number GetOffset(Number length)
		{
			EnsureWholeNumber(length, "ArgumentOutOfRangeException: Length must be a non-negative whole number.");
			if (length < 0)
				throw new Error("ArgumentOutOfRangeException: Length must be non-negative.");

			return IsFromEnd ? length - Value : Value;
		}
	}

	/// <summary>
	/// System.Range 的 runtime carrier，以两个 JIndex 保留左闭右开的边界协议。
	/// </summary>
	public sealed class JRange
	{
		[Description("@#start")]
		public JIndex Start { get; }

		[Description("@#end")]
		public JIndex End { get; }

		public JRange(JIndex start, JIndex end)
		{
			Start = start;
			End = end;
		}

		public (Number Offset, Number Length) GetOffsetAndLength(Number length)
		{
			var start = Start.GetOffset(length);
			var end = End.GetOffset(length);
			if (start < 0 || end < start || end > length)
				throw new Error("ArgumentOutOfRangeException: Range is outside the bounds of the collection.");

			return (start, end - start);
		}
	}

	/// <summary>
	/// Queue&lt;T&gt; 的最小 runtime carrier，使用数组和游标模拟先进先出队列。
	/// </summary>
	/// <remarks>游标与数组增长策略属于内部实现，外部只能通过白名单成员访问。</remarks>
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

	/// <summary>
	/// Stack&lt;T&gt; 的最小 runtime carrier，使用数组保存后进先出状态。
	/// </summary>
	/// <remarks>它只承诺当前白名单所需的栈操作，不等同于完整 CLR Stack runtime identity。</remarks>
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

	/// <summary>
	/// TimeOnly 的 runtime carrier，以 tick 保存一天内的时间部分。
	/// </summary>
	/// <remarks>carrier 不携带日期和时区；与 DateTime 的组合必须通过显式 helper 完成。</remarks>
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

	/// <summary>
	/// TimeSpan 的 runtime carrier，以 BigInt tick 保存完整范围的时间间隔。
	/// </summary>
	/// <remarks>使用 BigInt 是为了避免 Number 对 Int64 tick 的精度截断。</remarks>
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

	/// <summary>
	/// GregorianCalendar 的 runtime carrier，保存 calendar type 和两位年份规则。
	/// </summary>
	/// <remarks>它只承载当前 GregorianCalendar helper 所需的状态，不模拟 CLR Calendar 类型层次。</remarks>
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

	public static Number GetInt128HashCode(BigInt value)
	{
		var low = BigInt.AsIntN(64, value);
		var high = BigInt.AsIntN(64, value >> BigIntFn(64));
		return GetInt64HashCode(low) ^ GetInt64HashCode(high);
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
