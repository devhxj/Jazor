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
		var parts = new Array<string>();
		for (var index = 0; index < characters.Length; index++)
			parts.Push(characters[index]);
		return parts.Join("");
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
			return Utf8Decoder.Decode(new Uint8Array(utf8Text));
		}
		catch
		{
			return null;
		}
	}

	// TextDecoder.decode() is stateless when stream mode is not requested. Reusing the
	// configured decoder avoids constructing one for every numeric UTF-8 parse.
	private static readonly TextDecoder Utf8Decoder = new(
		"utf-8",
		new TextDecoderOptions(Fatal: true, IgnoreBOM: true));

	internal static string DecodeUtf8OrThrowFormat(Uint8Array utf8Text)
	{
		var text = TryDecodeUtf8(utf8Text);
		if (text == null)
			throw new Error("FormatException: The UTF-8 input was not in a correct format.");
		return text;
	}

	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFunc(value) != value || value < Number.MIN_SAFE_INTEGER || value > Number.MAX_SAFE_INTEGER)
			throw new Error(message);
	}

	// Dictionary and HashSet share the CLR hash-table prime sequence. JavaScript Map/Set do
	// not expose backing capacity, so this algorithm is the sole source of observed capacity.
	private static readonly Array<Number> HashCapacityPrimes =
	[
		3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197,
		239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931,
		2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143,
		14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851,
		75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371,
		324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687,
		1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287,
		4999559, 5999471, 7199369
	];

	private static readonly Number MaxHashCapacity = 2146435069;

	internal static Number GetHashCollectionCapacity(Number minimum)
	{
		EnsureWholeNumber(minimum, "ArgumentOutOfRangeException: capacity must be a whole number.");
		if (minimum < 0)
			throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
		if (minimum == 0)
			return 0;

		for (var index = 0; index < HashCapacityPrimes.Length; index++)
		{
			if (HashCapacityPrimes[index] >= minimum)
				return HashCapacityPrimes[index];
		}

		var candidate = minimum % 2 == 0 ? minimum + 1 : minimum;
		while (candidate <= MaxHashCapacity)
		{
			if (IsHashCapacityPrime(candidate) && (candidate - 1) % 101 != 0)
				return candidate;
			candidate += 2;
		}

		throw new Error("OutOfMemoryException: requested collection capacity is too large.");
	}

	internal static Number ExpandHashCollectionCapacity(Number currentCapacity)
	{
		EnsureWholeNumber(currentCapacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
		if (currentCapacity < 0)
			throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
		if (currentCapacity == 0)
			return GetHashCollectionCapacity(1);
		if (currentCapacity >= MaxHashCapacity)
			return MaxHashCapacity;

		var minimum = currentCapacity * 2;
		if (minimum > MaxHashCapacity)
			return MaxHashCapacity;
		return GetHashCollectionCapacity(minimum);
	}

	private static bool IsHashCapacityPrime(Number candidate)
	{
		if (candidate == 2)
			return true;
		if (candidate < 2 || candidate % 2 == 0)
			return false;

		var limit = Math.FloorFunc(Math.Sqrt(candidate));
		for (var divisor = 3; divisor <= limit; divisor += 2)
		{
			if (candidate % divisor == 0)
				return false;
		}
		return true;
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

	private static bool ThrowReadOnlyArraySet<TItem>(
		Array<TItem> target,
		ECMAScript.JazorPropertyKey property,
		object? value,
		object receiver)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlyArrayDelete<TItem>(Array<TItem> target, ECMAScript.JazorPropertyKey property)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlyArrayDefine<TItem>(
		Array<TItem> target,
		ECMAScript.JazorPropertyKey property,
		ECMAScript.JazorPropertyDescriptor attributes)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static void ThrowReadOnlyArrayMutation<TItem>()
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static object? GetReadOnlyArrayProperty<TItem>(
		Array<TItem> target,
		ECMAScript.JazorPropertyKey property,
		object receiver)
	{
		// Array prototype mutators execute with the proxy as `this`; returning a throwing
		// function prevents push/pop/sort and the other in-place methods from bypassing Set.
		var propertyName = property.AsString;
		if (propertyName == "copyWithin"
			|| propertyName == "fill"
			|| propertyName == "pop"
			|| propertyName == "push"
			|| propertyName == "reverse"
			|| propertyName == "shift"
			|| propertyName == "sort"
			|| propertyName == "splice"
			|| propertyName == "unshift")
			return (Action)ThrowReadOnlyArrayMutation<TItem>;

		return BindReadOnlyCollectionProperty(target, property);
	}

	internal static Array<TItem> CreateReadOnlyArrayView<TItem>(Array<TItem>? source, string nullMessage)
	{
		if (source is null)
			throw new Error(nullMessage);

		// The proxy remains an Array at runtime, so indexed access, length, and iteration retain
		// their normal carrier shape. Source writes stay observable through the live view, while
		// writes through the view are rejected even after the CLR collection type is erased.
		var handler = new ECMAScript.ProxyMutationHandler<Array<TItem>>
		{
			Get = GetReadOnlyArrayProperty<TItem>,
			Set = ThrowReadOnlyArraySet<TItem>,
			DeleteProperty = ThrowReadOnlyArrayDelete<TItem>,
			DefineProperty = ThrowReadOnlyArrayDefine<TItem>
		};
		var view = (Array<TItem>)(object)new ECMAScript.Proxy<Array<TItem>>(source, handler);
		ReadOnlyCarriers.Add(view);
		return view;
	}

	private static readonly WeakSet ReadOnlyCarriers = new();
	private static readonly WeakSet MutableListCarriers = new();

	/// <summary>
	/// Marks an Array carrier whose CLR source contract is <see cref="System.Collections.Generic.List{T}"/>.
	/// </summary>
	/// <remarks>
	/// Both <c>T[]</c> and <c>List&lt;T&gt;</c> erase to JavaScript arrays. The marker is therefore
	/// necessary to preserve interface mutation rules without treating fixed-size arrays as lists.
	/// It is attached only by List-producing factories, never inferred from array shape.
	/// </remarks>
	internal static Array<T> MarkAsMutableListCarrier<T>(Array<T> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		MutableListCarriers.Add(instance);
		return instance;
	}

	internal static bool IsMutableListCarrier<T>(Array<T>? instance)
		=> instance != null && MutableListCarriers.Has(instance);

	internal static void RequireMutableListCarrier<T>(Array<T>? instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		if (MutableListCarriers.Has(instance))
			return;
		if (ReadOnlyCarriers.Has(instance))
			throw new Error(ReadOnlyCarrierMutationMessage);

		throw new Error("NotSupportedException: Collection has a fixed size.");
	}

	// CLR object hashes need identity stability for reference carriers, while JavaScript has no
	// intrinsic object hash. WeakMap keeps the association without extending object lifetime.
	private static readonly WeakMap<object, Number> ReferenceHashCodes = new();
	private static Number NextReferenceHashCode = 1;

	/// <summary>
	/// Computes the Java-style 32-bit string hash used by erased CLR carriers.
	/// </summary>
	/// <remarks>
	/// Callers supply the historical seed because object hashing uses 17 while decimal's
	/// normalized-text path uses 0. Sharing the recurrence keeps overflow behavior in one place
	/// without changing either existing hash contract.
	/// </remarks>
	public static Number GetStringHashCode(string text, Number seed)
	{
		var hash = seed;
		for (var index = 0; index < text.Length; index++)
			hash = ((hash * 31) + text[index]) | 0;
		return hash;
	}

	/// <summary>
	/// Returns the index of the highest set bit in a non-negative integer carrier.
	/// </summary>
	public static Number GetHighestSetBit(Number value)
	{
		var bit = -1;
		while (value > 0)
		{
			value = Math.FloorFunc(value / 2);
			bit++;
		}

		return bit;
	}

	private static Number GetReferenceHashCode(object value)
	{
		if (ReferenceHashCodes.Has(value))
			return ReferenceHashCodes.Get(value)!;

		var hash = NextReferenceHashCode;
		NextReferenceHashCode = (NextReferenceHashCode + 1) | 0;
		if (NextReferenceHashCode == 0)
			NextReferenceHashCode = 1;
		ReferenceHashCodes.Set(value, hash);
		return hash;
	}

	/// <summary>
	/// Produces the deterministic hash contract shared by erased CLR object values.
	/// </summary>
	/// <remarks>
	/// Primitive carriers retain the existing comparer hashes. Object/function carriers receive a
	/// stable identity hash; this is the only case where JavaScript needs runtime state to model
	/// <c>object.GetHashCode()</c>. Callers retain their own null-receiver behavior.
	/// </remarks>
	internal static Number GetObjectHashCode(object? value)
	{
		if (value == null)
			return 0;

		var type = TypeOf(value);
		if (type == "boolean")
			return (bool)value ? 1 : 0;

		if (type == "number")
		{
			var number = (Number)value;
			if (IsNaN(number) || number == 0)
				return 0;
			if (Math.FloorFunc(number) == number && number >= -2147483648 && number <= 2147483647)
				return number | 0;
			return GetStringHashCode(number.ToString(), 17);
		}

		if (type == "string")
			return GetStringHashCode((string)value, 17);
		if (type == "bigint")
			return GetStringHashCode(((BigInt)value).ToString(), 17);

		if (type == "object" || type == "function")
			return GetReferenceHashCode(value);

		return GetStringHashCode(value.ToString() ?? "", 17);
	}

	/// <summary>
	/// Materializes the CLR string representation used by object-based string APIs.
	/// </summary>
	/// <remarks>
	/// These overloads have already erased the static value type, so this is the one shared
	/// boundary that can preserve null-as-empty, the CLR Boolean spelling, and a compiled
	/// object's virtual <c>toString</c> dispatch without duplicating ad hoc conversions.
	/// </remarks>
	internal static string GetStringRepresentation(object? value)
	{
		if (value == null)
			return "";

		if (TypeOf(value) == "boolean")
			return (bool)value ? "True" : "False";

		return value.ToString() ?? "";
	}

	private static object? BindReadOnlyCollectionProperty<TTarget>(TTarget target, ECMAScript.JazorPropertyKey property)
		where TTarget : class
	{
		var value = ECMAScript.Reflect.Get(target, property, target);
		return TypeOf(value) == "function" ? value!.Bind(target) : value;
	}

	internal static bool IsReadOnlySetCarrier<T>(Set<T> instance)
		=> instance != null && ReadOnlyCarriers.Has(instance);

	private static Set<T> ThrowReadOnlySetAdd<T>(T item)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlySetDelete<T>(T item)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static void ThrowReadOnlySetClear<T>()
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static object? GetReadOnlySetProperty<T>(Set<T> target, ECMAScript.JazorPropertyKey property, object receiver)
	{
		var propertyName = property.AsString;
		if (propertyName == "add")
			return (Func<T, Set<T>>)ThrowReadOnlySetAdd<T>;
		if (propertyName == "delete")
			return (Func<T, bool>)ThrowReadOnlySetDelete<T>;
		if (propertyName == "clear")
			return (Action)ThrowReadOnlySetClear<T>;

		return BindReadOnlyCollectionProperty(target, property);
	}

	private static bool ThrowReadOnlySetPropertySet<T>(
		Set<T> target,
		ECMAScript.JazorPropertyKey property,
		object? value,
		object receiver)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlySetPropertyDelete<T>(Set<T> target, ECMAScript.JazorPropertyKey property)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlySetPropertyDefine<T>(
		Set<T> target,
		ECMAScript.JazorPropertyKey property,
		ECMAScript.JazorPropertyDescriptor attributes)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	internal static Set<T> MarkAsReadOnlySetCarrier<T>(Set<T> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		if (IsReadOnlySetCarrier(instance))
			return instance;

		var handler = new ECMAScript.ProxyMutationHandler<Set<T>>
		{
			Get = GetReadOnlySetProperty<T>,
			Set = ThrowReadOnlySetPropertySet<T>,
			DeleteProperty = ThrowReadOnlySetPropertyDelete<T>,
			DefineProperty = ThrowReadOnlySetPropertyDefine<T>
		};
		var view = (Set<T>)(object)new ECMAScript.Proxy<Set<T>>(instance, handler);
		ReadOnlyCarriers.Add(view);
		return view;
	}

	internal static bool IsReadOnlyDictionaryCarrier<TKey, TValue>(Map<TKey, TValue> instance)
		=> instance != null && ReadOnlyCarriers.Has(instance);

	private static Map<TKey, TValue> ThrowReadOnlyDictionarySet<TKey, TValue>(TKey key, TValue value)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlyDictionaryDelete<TKey, TValue>(TKey key)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static void ThrowReadOnlyDictionaryClear<TKey, TValue>()
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static object? GetReadOnlyDictionaryProperty<TKey, TValue>(
		Map<TKey, TValue> target,
		ECMAScript.JazorPropertyKey property,
		object receiver)
	{
		var propertyName = property.AsString;
		if (propertyName == "set")
			return (Func<TKey, TValue, Map<TKey, TValue>>)ThrowReadOnlyDictionarySet<TKey, TValue>;
		if (propertyName == "delete")
			return (Func<TKey, bool>)ThrowReadOnlyDictionaryDelete<TKey, TValue>;
		if (propertyName == "clear")
			return (Action)ThrowReadOnlyDictionaryClear<TKey, TValue>;

		return BindReadOnlyCollectionProperty(target, property);
	}

	private static bool ThrowReadOnlyDictionaryPropertySet<TKey, TValue>(
		Map<TKey, TValue> target,
		ECMAScript.JazorPropertyKey property,
		object? value,
		object receiver)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlyDictionaryPropertyDelete<TKey, TValue>(Map<TKey, TValue> target, ECMAScript.JazorPropertyKey property)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	private static bool ThrowReadOnlyDictionaryPropertyDefine<TKey, TValue>(
		Map<TKey, TValue> target,
		ECMAScript.JazorPropertyKey property,
		ECMAScript.JazorPropertyDescriptor attributes)
		=> throw new Error(ReadOnlyCarrierMutationMessage);

	internal static Map<TKey, TValue> MarkAsReadOnlyDictionaryCarrier<TKey, TValue>(Map<TKey, TValue> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		if (IsReadOnlyDictionaryCarrier(instance))
			return instance;

		var handler = new ECMAScript.ProxyMutationHandler<Map<TKey, TValue>>
		{
			Get = GetReadOnlyDictionaryProperty<TKey, TValue>,
			Set = ThrowReadOnlyDictionaryPropertySet<TKey, TValue>,
			DeleteProperty = ThrowReadOnlyDictionaryPropertyDelete<TKey, TValue>,
			DefineProperty = ThrowReadOnlyDictionaryPropertyDefine<TKey, TValue>
		};
		var view = (Map<TKey, TValue>)(object)new ECMAScript.Proxy<Map<TKey, TValue>>(instance, handler);
		ReadOnlyCarriers.Add(view);
		return view;
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
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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
				+ Pad7(BigIntValue(Date.GetMilliseconds()) * BigIntValue(10000) + SubMillisecondTicks);
		}

		[Description("@#valueOf")]
		public Number ValueOf()
		{
			// Utc DateTime stores civil UTC fields in a local Date carrier so the CLR
			// getters remain stable. Local/unspecified values, however, are represented
			// by an actual local Date instant and must retain its timezone offset.
			if (Kind != 2)
				return Date.UTC(
					Date.GetFullYear(),
					Date.GetMonth(),
					Date.GetDate(),
					Date.GetHours(),
					Date.GetMinutes(),
					Date.GetSeconds(),
					Date.GetMilliseconds());

			return Date.GetTime();
		}

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
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
		{
			var local = new Date(UtcDateTime.GetTime() + NumberValue(OffsetTicks) / 10000);

			var negative = OffsetTicks < BigInt.Zero;
			var absolute = negative ? -OffsetTicks : OffsetTicks;
			var totalMinutes = absolute / BigIntValue(600000000);
			var hours = NumberValue(totalMinutes / BigIntValue(60));
			var minutes = NumberValue(totalMinutes % BigIntValue(60));
			var offset = (negative ? "-" : "+") + Pad2(hours) + ":" + Pad2(minutes);

			return FormatDateOnlyText(local.GetUTCFullYear(), local.GetUTCMonth() + 1, local.GetUTCDate())
				+ "T"
				+ Pad2(local.GetUTCHours())
				+ ":"
				+ Pad2(local.GetUTCMinutes())
				+ ":"
				+ Pad2(local.GetUTCSeconds())
				+ "."
				+ Pad7(BigIntValue(local.GetUTCMilliseconds()) * BigIntValue(10000) + UtcSubMillisecondTicks)
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
			DayNumber = Math.FloorFunc((utcDate.GetTime() - start.GetTime()) / 86400000);
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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
		public Number Head { get; set; }

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
			var normalized = ticks % BigIntValue("864000000000");
			this.Ticks = normalized < BigInt.Zero ? normalized + BigIntValue("864000000000") : normalized;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
			{
				Value = (Func<string?, object>)ToPrimitive,
				Configurable = true
			});
		}

		[Description("@#toString")]
		public override string ToString()
		{
			var hour = NumberValue(Ticks / BigIntValue("36000000000"));
			var minute = NumberValue((Ticks / BigIntValue(600000000)) % BigIntValue(60));
			var second = NumberValue((Ticks / BigIntValue(10000000)) % BigIntValue(60));
			var fraction = Ticks % BigIntValue(10000000);

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
			if (ticks < BigIntValue("-9223372036854775808") || ticks > BigIntValue("9223372036854775807"))
				throw new Error("OverflowException: TimeSpan is too long or too short.");

			this.Ticks = ticks;
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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
			var days = absolute / BigIntValue("864000000000");
			var hours = NumberValue((absolute / BigIntValue("36000000000")) % BigIntValue(24));
			var minutes = NumberValue((absolute / BigIntValue(600000000)) % BigIntValue(60));
			var seconds = NumberValue((absolute / BigIntValue(10000000)) % BigIntValue(60));
			var fraction = absolute % BigIntValue(10000000);

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
			Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.JazorPropertyDescriptor
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

	/// <summary>
	/// Calendar 基类和 GregorianCalendar 目前共享 JGregorianCalendar carrier。构造函数必须
	/// 保持 CLR 对 null calendar 的 ArgumentNullException 优先级，再委托具体日期 helper。
	/// </summary>
	internal static JGregorianCalendar RequireGregorianCalendar(JGregorianCalendar? calendar)
	{
		if (calendar == null)
			throw new Error("ArgumentNullException: calendar is null.");

		return calendar;
	}

	/// <summary>
	/// CancellationTokenRegistration 的 runtime carrier，保存解除订阅所需的 signal 与 listener。
	/// </summary>
	/// <remarks>
	/// CLR 的 registration 是"如何撤下这个回调"的凭据；擦除到浏览器侧它就是 (signal, listener) 二元组，
	/// 因此 carrier 只保存这两项，不模拟 CLR 的 node/id 结构。
	/// <c>Handler</c> 为 null 表示已经没有可撤回调：注册时 token 就已取消（回调当场同步跑完，
	/// abort 事件不会再触发），或者已经 Unregister/Dispose 过一次。
	/// </remarks>
	public sealed class JCancellationTokenRegistration
	{
		[Description("@#signal")]
		public AbortSignal Signal { get; }

		[Description("@#handler")]
		public HandleEventCallback? Handler { get; set; }

		public JCancellationTokenRegistration(AbortSignal signal, HandleEventCallback? handler)
		{
			this.Signal = signal;
			this.Handler = handler;
		}
	}

	/// <summary>
	/// CancellationToken.Register / UnsafeRegister 的公共注册路径。
	/// </summary>
	/// <remarks>
	/// token 已取消时 CLR 会同步执行回调并返回一个撤不掉的 registration。AbortSignal 在这种情况下
	/// 也不会再派发 abort，所以必须在这里显式调用一次，只挂 listener 会永久丢掉回调。
	/// UnsafeRegister 与 Register 的差别仅是不捕获 ExecutionContext，浏览器没有这个概念，
	/// 因此两组重载共用同一实现。
	/// </remarks>
	internal static JCancellationTokenRegistration RegisterCancellationCallback(AbortSignal signal, Action callback)
	{
		if (signal.Aborted)
		{
			callback();
			return new JCancellationTokenRegistration(signal, null);
		}

		var handler = (HandleEventCallback)(_ => callback());
		signal.AddEventListener("abort", handler, false);
		return new JCancellationTokenRegistration(signal, handler);
	}

	/// <summary>
	/// CancellationTokenRegistration.Unregister / Dispose 的公共撤销路径。
	/// </summary>
	/// <remarks>
	/// CLR 只在真正撤下一个尚未执行的回调时返回 true；已执行或已撤销都返回 false。signal 已 abort
	/// 说明回调跑过了，即使 listener 仍挂着也不算撤销成功，因此要先看 aborted 再看 handler。
	/// Dispose 在 CLR 下还需要等待并发执行中的回调，JS 单线程不存在这个窗口，两者行为一致。
	/// </remarks>
	internal static bool UnregisterCancellationCallback(JCancellationTokenRegistration registration)
	{
		var handler = registration.Handler;
		if (handler == null)
			return false;

		registration.Signal.RemoveEventListener("abort", handler, false);
		registration.Handler = null;
		return !registration.Signal.Aborted;
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
		var low = (int)NumberValue(BigInt.AsIntN(32, value));
		var high = (int)NumberValue(BigInt.AsIntN(32, value >> BigIntValue(32)));
		return low ^ high;
	}

	public static Number GetInt128HashCode(BigInt value)
	{
		var low = BigInt.AsIntN(64, value);
		var high = BigInt.AsIntN(64, value >> BigIntValue(64));
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
		var missing = width - text.Length;
		if (missing <= 0)
			return text;

		var parts = new Array<string>();
		for (var index = 0; index < missing; index++)
			parts.Push("0");
		parts.Push(text);

		return parts.Join("");
	}
}
